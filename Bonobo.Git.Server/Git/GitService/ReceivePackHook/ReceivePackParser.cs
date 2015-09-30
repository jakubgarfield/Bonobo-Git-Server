using Ionic.Zlib;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Bonobo.Git.Server.Git.GitService.ReceivePackHook
{
    public class ReceivePackParser : IGitService
    {
        private readonly IGitService gitService;        
        private readonly IHookReceivePack receivePackHandler;
        private readonly GitServiceResultParser resultParser;

        public ReceivePackParser(IGitService gitService, IHookReceivePack receivePackHandler, GitServiceResultParser resultParser)
        {
            this.gitService = gitService;
            this.receivePackHandler = receivePackHandler;
            this.resultParser = resultParser;
        }

        public void ExecuteServiceByName(string correlationId, string repositoryName, string serviceName, ExecutionOptions options, System.IO.Stream inStream, System.IO.Stream outStream)
        {
            ParsedReceivePack receivedPack = null;

            if (serviceName == "receive-pack" && inStream.Length > 0)
            {
                // PARSING RECEIVE-PACK THAT IS OF THE FOLLOWING FORMAT: 
                // (NEW LINES added for ease of reading)
                // (LLLL is length of the line (expressed in HEX) until next LLLL value)
                //
                // LLLL------ REF LINE -----------\0------- OHTER DATA -----------
                // LLLL------ REF LINE ----------------
                // ...
                // ...
                // 0000PACK------- REST OF PACKAGE --------
                //

                var pktLines = new List<ReceivePackPktLine>();

                var buff1 = new byte[1];
                var buff4 = new byte[4];
                var buff20 = new byte[20];
                var buff16K = new byte[1024 * 16]; 

                while (true)
                {
                    ReadStream(inStream, buff4);
                    var len = Convert.ToInt32(Encoding.UTF8.GetString(buff4), 16);
                    if (len == 0)
                    {
                        break;
                    }
                    len = len - buff4.Length;

                    var accum = new LinkedList<byte>();

                    while (len > 0)
                    {
                        len -= 1;
                        ReadStream(inStream, buff1);
                        if (buff1[0] == 0)
                        {
                            break;
                        }
                        accum.AddLast(buff1[0]);
                    }
                    if (len > 0)
                    {
                        inStream.Seek(len, SeekOrigin.Current);
                    }
                    var pktLine = Encoding.UTF8.GetString(accum.ToArray());
                    var pktLineItems = pktLine.Split(' ');

                    var fromCommit = pktLineItems[0];
                    var toCommit = pktLineItems[1];
                    var refName = pktLineItems[2];

                    pktLines.Add(new ReceivePackPktLine(fromCommit, toCommit, refName));
                }

                // parse PACK contents
                var packCommits = new List<ReceivePackCommit>();

                // PACK format
                // https://www.kernel.org/pub/software/scm/git/docs/technical/pack-format.html
                // http://schacon.github.io/gitbook/7_the_packfile.html

                if (inStream.Position < inStream.Length)
                {
                    ReadStream(inStream, buff4);
                    if (Encoding.UTF8.GetString(buff4) != "PACK")
                    {
                        throw new Exception("Unexpected receive-pack 'PACK' content.");
                    }
                    ReadStream(inStream, buff4);
                    Array.Reverse(buff4);
                    var versionNum = BitConverter.ToInt32(buff4, 0);

                    ReadStream(inStream, buff4);
                    Array.Reverse(buff4);
                    var numObjects = BitConverter.ToInt32(buff4, 0);

                    while (numObjects > 0)
                    {
                        numObjects -= 1;

                        ReadStream(inStream, buff1);
                        var type = (GIT_OBJ_TYPE)((buff1[0] >> 4) & 7);
                        long len = buff1[0] & 15;

                        var shiftAmount = 4;
                        while ((buff1[0] >> 7) == 1)
                        {
                            ReadStream(inStream, buff1);
                            len = len | ((long)(buff1[0] & 127) << shiftAmount);

                            shiftAmount += 7;
                        }

                        if (type == GIT_OBJ_TYPE.OBJ_REF_DELTA)
                        {
                            // read ref name
                            ReadStream(inStream, buff20);
                        }
                        if (type == GIT_OBJ_TYPE.OBJ_OFS_DELTA)
                        {
                            // read negative offset
                            ReadStream(inStream, buff1);
                            while ((buff1[0] >> 7) == 1)
                            {
                                ReadStream(inStream, buff1);
                            }
                        }

                        var origPosition = inStream.Position;
                        long offsetVal = 0;

                        using (var zlibStream = new ZlibStream(inStream, CompressionMode.Decompress, true))
                        {
                            // read compressed data max 16KB at a time
                            var readRemaining = len;
                            do
                            {
                                var bytesUncompressed = zlibStream.Read(buff16K, 0, buff16K.Length);
                                readRemaining -= bytesUncompressed;
                            } while (readRemaining > 0);

                            if (type == GIT_OBJ_TYPE.OBJ_COMMIT)
                            {
                                var parsedCommit = ParseCommitDetails(buff16K, len);
                                packCommits.Add(parsedCommit);
                            }
                            offsetVal = zlibStream.TotalIn;
                        }
                        // move back position a bit because ZLibStream reads more than needed for inflating
                        inStream.Seek(origPosition + offsetVal, SeekOrigin.Begin);
                    }
                }
                // -------------------

                var user = HttpContext.Current.User.Id();
                receivedPack = new ParsedReceivePack(correlationId, repositoryName, pktLines, user, DateTime.Now, packCommits);

                inStream.Seek(0, SeekOrigin.Begin);

                receivePackHandler.PrePackReceive(receivedPack);
            }

            GitExecutionResult execResult = null;
            using (var capturedOutputStream = new MemoryStream())
            {
                gitService.ExecuteServiceByName(correlationId, repositoryName, serviceName, options, inStream, new ReplicatingStream(outStream, capturedOutputStream));

                // parse captured output
                capturedOutputStream.Seek(0, SeekOrigin.Begin);
                execResult = resultParser.ParseResult(capturedOutputStream);
            }

            if(receivedPack != null)
            {
                receivePackHandler.PostPackReceive(receivedPack, execResult);
            }
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ReceivePackCommit ParseCommitDetails(byte[] buff, long commitMsgLengthLong)
        {
            if (commitMsgLengthLong > buff.Length)
            {
                // buff at the moment is 16KB, should be enough for commit messages
                // but break just in case this ever does happen so it could be addressed then
                throw new Exception("Encountered unexpectedly large commit message");
            }
            int commitMsgLength = (int)commitMsgLengthLong; // guaranteed no truncation because of above guard clause

            var commitMsg = Encoding.UTF8.GetString(buff, 0, commitMsgLength);
            string treeHash = null;
            var parentHashes = new List<string>();
            ReceivePackCommitSignature author = null;
            ReceivePackCommitSignature committer = null;

            var commitLines = commitMsg.Split('\n');

            var commitHeadersEndIndex = 0;
            foreach (var commitLine in commitLines)
            {
                commitHeadersEndIndex += 1;
                
                // Make sure we have safe default values in case the string is empty.
                var commitHeaderType = "";
                var commitHeaderData = "";

                // Find the index of the first space.
                var firstSpace = commitLine.IndexOf(' ');
                if (firstSpace < 0)
                {
                    // Ensure that we always have a valid length for the type.
                    firstSpace = commitLine.Length;
                }

                // Take everything up to the first space as the type.
                commitHeaderType = commitLine.Substring(0, firstSpace);

                // Data starts immediately following the space (if there is any).
                var dataStart = firstSpace + 1;
                if (dataStart < commitLine.Length)
                {
                    commitHeaderData = commitLine.Substring(dataStart);
                }

                if (commitHeaderType == "tree")
                {
                    treeHash = commitHeaderData;
                }
                else if (commitHeaderType == "parent")
                {
                    parentHashes.Add(commitHeaderData);
                }
                else if (commitHeaderType == "author")
                {
                    author = ParseSignature(commitHeaderData);
                }
                else if (commitHeaderType == "committer")
                {
                    committer = ParseSignature(commitHeaderData);
                }
                else if (commitHeaderType == "")
                {
                    // The first empty type indicates the end of the headers.
                    break;
                }
                else
                {
                    // unrecognized header encountered, skip over it
                }
            }

            var commitComment = string.Join("\n", commitLines.Skip(commitHeadersEndIndex).ToArray()).TrimEnd('\n');


            // Compute commit hash
            using (var sha1 = new SHA1CryptoServiceProvider())
            {
                var commitHashHeader = Encoding.UTF8.GetBytes(string.Format("commit {0}\0", commitMsgLength));

                sha1.TransformBlock(commitHashHeader, 0, commitHashHeader.Length, commitHashHeader, 0);
                sha1.TransformFinalBlock(buff, 0, commitMsgLength);

                var commitHashBytes = sha1.Hash;

                var sb = new StringBuilder();
                foreach (byte b in commitHashBytes)
                {
                    var hex = b.ToString("x2");
                    sb.Append(hex);
                }
                var commitHash = sb.ToString();

                return new ReceivePackCommit(commitHash, treeHash, parentHashes,
                    author, committer, commitComment);
            }
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public ReceivePackCommitSignature ParseSignature(string commitHeaderData)
        {
            // Find the start and end markers of the email address.
            var emailStart = commitHeaderData.IndexOf('<');
            var emailEnd = commitHeaderData.IndexOf('>');
            
            // Leave out the trailing space.
            var nameLength = emailStart - 1;

            // Leave out the starting bracket.
            var emailLength = emailEnd - emailStart - 1;
            
            // Parse the name and email values.
            var name = commitHeaderData.Substring(0, nameLength);
            var email = commitHeaderData.Substring(emailStart + 1, emailLength);

            // The rest of the string is the timestamp, it may include a timezone.
            var timestampString = commitHeaderData.Substring(emailEnd + 2);
            var timestampComponents = timestampString.Split(' ');

            // Start with epoch in UTC, add the timestamp seconds.
            var timestamp = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            timestamp = timestamp.AddSeconds(long.Parse(timestampComponents[0]));

            return new ReceivePackCommitSignature(name, email, timestamp);
        }

        [MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void ReadStream(Stream s, byte[] buff)
        {
            var readBytes = s.Read(buff, 0, buff.Length);
            if (readBytes != buff.Length)
            {
                throw new Exception(string.Format("Expected to read {0} bytes, got {1}", buff.Length, readBytes));
            }
        }
    }
}