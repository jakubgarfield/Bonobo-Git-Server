using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Bonobo.Git.Server.Git {
    /// <summary>
    ///     Reads data from the wrapped stream analyzing / parsing the initial parts of the communicated Git http protocol 
    ///     and aggregates important metadata about what commands are performed.
    ///     This stream does not modify or transform the data.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     A recive pack request can be gigabytes in size and is made of a command list and a PACK file.
    ///     </para>
    ///
    ///     <para>
    ///     This stream will peek on the (usually much smaller) command list portion of the request. This portion
    ///     contains refs (i.e. tags and branch heads) along with their old and new SHA1 identifiers so that Git can update them.
    ///     This information is sufficient for us to tell whether a branch / tag will be updated, created or deleted etc.
    ///     We may aswell tell which commits Git is going to add, if we check all refs between an old head pointer and
    ///     the new one after Git itself has received and parsed the PACK contents that far.
    ///     </para>
    ///
    ///     <para>
    ///     The PACK contents could be parsed aswell, but this is going to cause a significant overhead because the 
    ///     anatomy of the PACK format forces us to deflate all compressed objects in it. We can not just skip over certain
    ///     objects, because we don't know their inflated (current) size in the stream.
    ///     </para>
    /// </remarks>
    /// <seealso href="https://github.com/git/git/blob/master/Documentation/technical/http-protocol.txt"/>
    /// <seealso href="https://github.com/git/git/blob/master/Documentation/technical/pack-format.txt"/>
    /// <seealso href="https://github.com/git/git/blob/master/Documentation/technical/protocol-common.txt"/>
    /// <seealso href="https://github.com/git/git/blob/master/Documentation/technical/protocol-capabilities.txt"/>
    public class ReceivePackInspectStream: Stream {
        // version, number of objects. (the 4 byte PACK signature is not included.)
        private const int PackHeaderSize = 4 + 4;
        private const int PktLineLengthSize = 4;

        private readonly Stream _wrappedStream;

        /// <summary>
        ///     While we follow the protocol, this will be the "phase" where we're currently in
        /// </summary>
        private ProtocolState _state;

        /// <summary>
        ///     The remaining amount of bytes we have to process until we're done with what we're currently reading.
        /// </summary>
        private int _bytesNeeded;

        // It's rather unlikely that we make use of our buffer at all, given that the initial Read buffer
        // is of a decent size and the command list part of the request isn't unusually large.
        private readonly Lazy<MemoryStream> _dataFromPreviousRead = new Lazy<MemoryStream>();

        private readonly List<GitReceiveCommand> _caughtOperations;

        private readonly Action<ReceivePackInspectStream> _commandListReceived;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;

        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        ///     The Git commands which have been peeked from the wrapped stream so far.
        /// </summary>
        public ReadOnlyCollection<GitReceiveCommand> PeekedCommands { get; }

        /// <summary>
        ///     The version number of the PACK.
        /// </summary>
        public int PackVersion { get; private set; }

        /// <summary>
        ///     The amount of objects in the Git pack file.
        /// </summary>
        public int PackObjectCount { get; private set; }

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <remarks>
        ///     Use <paramref name="commandListReceived" /> to preprocess the Git commands right after they were
        ///     received. You may reject the whole pack then by throwing an <see cref="IOException" />.
        /// </remarks>
        /// <param name="wrappedStream">The origin stream to read from.</param>
        /// <param name="commandListReceived">Called once the command list has been completely retrieved.</param>
        public ReceivePackInspectStream(Stream wrappedStream, Action<ReceivePackInspectStream> commandListReceived = null) {
            if (wrappedStream == null) throw new ArgumentNullException(nameof(wrappedStream));

            _wrappedStream = wrappedStream;
            _caughtOperations = new List<GitReceiveCommand>();
            _commandListReceived = commandListReceived;
            PeekedCommands = new ReadOnlyCollection<GitReceiveCommand>(_caughtOperations);

            SetPktLineLengthState();
        }
        
        public override int Read(byte[] buffer, int offset, int count) {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > buffer.Length) throw new ArgumentException(nameof(buffer));

            int bytesRead = _wrappedStream.Read(buffer, offset, count);

            byte[] bufferToProcess = BufferWithPreviousDataPrepended(buffer, offset, ref bytesRead);
            Continue(bytesRead, bufferToProcess, offset);

            return bytesRead;
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _dataFromPreviousRead.IsValueCreated)
                _dataFromPreviousRead.Value.Dispose();
        }

        private byte[] BufferWithPreviousDataPrepended(byte[] buffer, int offset, ref int bytesRead)
        {
            Debug.Assert(buffer != null);
            Debug.Assert(offset >= 0);

            byte[] bufferToWorkWith;
            if (_dataFromPreviousRead.IsValueCreated && _dataFromPreviousRead.Value.Length > 0)
            {
                var dataFromPreviousReadStream = _dataFromPreviousRead.Value;

                dataFromPreviousReadStream.Write(buffer, offset, bytesRead);
                bufferToWorkWith = dataFromPreviousReadStream.ToArray();
                bytesRead = Convert.ToInt32(dataFromPreviousReadStream.Length);

                dataFromPreviousReadStream.SetLength(0);
            }
            else
                bufferToWorkWith = buffer;

            return bufferToWorkWith;
        }

        /// <summary>
        ///     Processes the data according to the current state of the protocol, if enough data is available.
        ///     Will recurse until either the whole command list has been processed or the buffer has run out of data.
        /// </summary>
        private void Continue(int bytesAvailable, byte[] buffer, int offset) {
            Debug.Assert(bytesAvailable >= 0);
            Debug.Assert(buffer != null);
            Debug.Assert(offset >= 0);

            if (_bytesNeeded == 0)
                return;
            if (bytesAvailable < _bytesNeeded) {
                _dataFromPreviousRead.Value.Write(buffer, offset, bytesAvailable);
                return;
            }

            switch (_state) {
                case ProtocolState.PktLineLengthOrPackString: {
                    int peekByte = buffer[offset];
                    if (peekByte != 'P') {
                        int pktLineLength = FourHexCharsToInt(buffer, offset);

                        bool isFlushPkt = pktLineLength == 0;
                        if (isFlushPkt)
                            SetPackHeaderState(); // all commands have been processed
                        else if (pktLineLength == PktLineLengthSize) // empty line
                            SetPktLineLengthState();
                        else
                            SetPktLinePayloadState(pktLineLength - PktLineLengthSize);
                    } else {
                        SetPackHeaderState(); // it was a pack string
                    }

                    Continue(bytesAvailable - PktLineLengthSize, buffer, offset + PktLineLengthSize);
                    return;
                }
                case ProtocolState.PktLinePayload: {
                    int payloadLength = _bytesNeeded;
                    string pktLinePayload = Encoding.UTF8.GetString(buffer, offset, payloadLength);
                    ProcessCommandPktLine(pktLinePayload);

                    SetPktLineLengthState();
                    Continue(bytesAvailable - payloadLength, buffer, offset + payloadLength);
                    return;
                }
                case ProtocolState.PackHeader: {
                    PackVersion = FourNetByteToInt(buffer, offset);
                    PackObjectCount = FourNetByteToInt(buffer, offset + 4);

                    _commandListReceived?.Invoke(this);

                    SetPackObjectsState();
                    Continue(bytesAvailable - PackHeaderSize, buffer, offset + PackHeaderSize);
                    return;
                }
                case ProtocolState.PackObjects: // won't parse this, skip
                    return;
            }
        }

        // command-pkt line format: <FromSHA1> <ToSHA1> <RefPath>\n?
        // first command-pkt also appends a \0 followed by a <list of capabilities>
        private void ProcessCommandPktLine(string payload) {
            Debug.Assert(payload != null);

            try {
                var oldSha1 = payload.Substring(0, 40);
                var newSha1 = payload.Substring(41, 40);

                int refPathPos = 82;
                int newlineOrTerminatorPos = payload.IndexOfAny(new[] {'\n', '\0'}, refPathPos);

                string refPath;
                if (newlineOrTerminatorPos != -1)
                    refPath = payload.Substring(refPathPos, newlineOrTerminatorPos - refPathPos);
                else
                    refPath = payload;

                _caughtOperations.Add(new GitReceiveCommand(refPath, oldSha1, newSha1));
            } catch (ArgumentException) {
                throw new IOException("Unexpected pkt-line payload: " + payload);
            }
        }

        private void SetPktLineLengthState() {
            _state = ProtocolState.PktLineLengthOrPackString;
            _bytesNeeded = PktLineLengthSize;
        }

        private void SetPktLinePayloadState(int payloadLength) {
            Debug.Assert(payloadLength > PktLineLengthSize);

            _state = ProtocolState.PktLinePayload;
            _bytesNeeded = payloadLength;
        }

        private void SetPackHeaderState() {
            _state = ProtocolState.PackHeader;
            _bytesNeeded = PackHeaderSize;
        }

        private void SetPackObjectsState() {
            _state = ProtocolState.PackObjects;
            _bytesNeeded = 0;
        }

        // "0032" => 50; "00a0" => 160 etc.
        private static int FourHexCharsToInt(byte[] buffer, int offset) {
            Debug.Assert(buffer != null);
            Debug.Assert(offset >= 0);

            int result = HexCharToInt(buffer[offset])  * 16 * 16 * 16;
            result += HexCharToInt(buffer[offset + 1]) * 16 * 16;
            result += HexCharToInt(buffer[offset + 2]) * 16;
            result += HexCharToInt(buffer[offset + 3]);
            return result;
        }

        private static int HexCharToInt(int chr) {
            Debug.Assert((chr >= '0' && chr <= '9') || (chr >= 'a' && chr <= 'f'));

            if (chr < 'a')
                return chr - '0';
            else
                return chr - 'a' + 10;
        }

        /// <summary>
        ///     Converts a network byte-order int32 to a normal integer.
        /// </summary>
        private static int FourNetByteToInt(byte[] buffer, int offset) {
            Debug.Assert(buffer != null);
            Debug.Assert(offset >= 0);

            return (buffer[offset]     << 24) |
                   (buffer[offset + 1] << 16) |
                   (buffer[offset + 2] <<  8) |
                   (buffer[offset + 3] <<  0);
        }

        private enum ProtocolState {
            PktLineLengthOrPackString,
            PktLinePayload,
            PackHeader,
            PackObjects,
        }
    }
}