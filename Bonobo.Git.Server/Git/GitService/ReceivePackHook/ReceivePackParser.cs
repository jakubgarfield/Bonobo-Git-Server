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

                var refChanges = new List<ReceivePackRefChange>();

                var buff1 = new byte[1];
                var buff4 = new byte[4];
                var buff20 = new byte[20];

                while (true)
                {
                    ReadStream(inStream, buff4);
                    var len = Convert.ToInt32(Encoding.ASCII.GetString(buff4), 16);
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
                    var refLine = Encoding.ASCII.GetString(accum.ToArray());
                    var refLineItems = refLine.Split(' ');

                    var fromCommit = refLineItems[0];
                    var toCommit = refLineItems[1];
                    var refName = refLineItems[2];

                    refChanges.Add(new ReceivePackRefChange(fromCommit, toCommit, refName));
                }

                // parse PACK contents

                var packCommits = new List<ReceivePackCommit>();

                // PACK format
                // https://www.kernel.org/pub/software/scm/git/docs/technical/pack-format.html
                // http://schacon.github.io/gitbook/7_the_packfile.html

                ReadStream(inStream, buff4);
                if(Encoding.ASCII.GetString(buff4) != "PACK")
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
                    var len = buff1[0] & 15;

                    var shiftAmount = 4;
                    while ((buff1[0] >> 7) == 1)
                    {
                        ReadStream(inStream, buff1);
                        len = len | ((buff1[0] & 127) << shiftAmount);

                        shiftAmount += 7;
                    }

                    if (type == GIT_OBJ_TYPE.OBJ_REF_DELTA)
                    {
                        // read name
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
                        var uncompressed = new byte[len];
                        var bytesRead = zlibStream.Read(uncompressed, 0, len);

                        var uncompressedStr = Encoding.UTF8.GetString(uncompressed);
                        
                        if(type == GIT_OBJ_TYPE.OBJ_COMMIT)
                        {
                            // Compute commit hash
                            using(var sha1 = new SHA1CryptoServiceProvider())
                            {
                                var commitMessage = string.Format("commit {0}\0{1}", uncompressedStr.Length, uncompressedStr);
                                var commitHashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(commitMessage));
                                
                                var sb = new StringBuilder();
                                foreach (byte b in commitHashBytes)
                                {
                                    var hex = b.ToString("x2");
                                    sb.Append(hex);
                                }
                                var commitHash = sb.ToString();

                                packCommits.Add(new ReceivePackCommit(commitHash));
                            }
                        }
                        offsetVal = zlibStream.TotalIn;
                    }
                    // move back position a bit because ZLibStream reads more than needed for inflating
                    inStream.Seek(origPosition + offsetVal, SeekOrigin.Begin);
                }
                
                // -------------------

                var user = HttpContext.Current.User.Identity.Name;
                receivedPack = new ParsedReceivePack(correlationId, repositoryName, refChanges, user, DateTime.Now, packCommits);

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
        public void ReadStream(Stream s, byte[] buff)
        {
            if (s.Read(buff, 0, buff.Length) != buff.Length)
            {
                throw new Exception("Expected to read 1 byte, got 0.");
            }
        }
    }
}