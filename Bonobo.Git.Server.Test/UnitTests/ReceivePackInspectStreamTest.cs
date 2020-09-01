using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonobo.Git.Server.Git;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Bonobo.Git.Server.Test.UnitTests
{
    [TestClass]
    public class ReceivePackInspectStreamTest
    {
        // These tests use request data as previously recorded from a Git for Windows client v. 2.15.1, stored as raw binary
        // data in file assets.
        private static readonly string GitAssetsDir = Path.GetFullPath(Path.Combine("..", "..", "assets", "git-receive-packs"));
        private static readonly Random Random = new Random();

        private ReceivePackInspectStream _sut;

        [TestMethod]
        public void WorksWithOversizedBuffer()
        {
            CreateSutWithRequestData("delete-b1.raw");

            MemoryStream readData = ReadData(bufferSize: 1024 * 1024);

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b1");
            Assert.AreEqual(readData.Length, 159);
        }

        [TestMethod]
        public void WorksWithOversizedBufferOnEmptyStream()
        {
            _sut = new ReceivePackInspectStream(new MemoryStream());

            MemoryStream readData = ReadData(bufferSize: 1024 * 1024);

            Assert.AreEqual(_sut.PeekedCommands.Count, 0);
            Assert.AreEqual(readData.Length, 0);
        }

        [TestMethod]
        public void WorksWithOneByteBuffer()
        {
            CreateSutWithRequestData("delete-b1.raw");

            // DONT use very small buffers in production, they'll be terrible slow, but still the stream 
            // should work fine with them.
            MemoryStream readData = ReadData(bufferSize: 1);

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b1");
            Assert.AreEqual(readData.Length, 159);
        }

        [TestMethod]
        public void WhateverGoesInComesBackOutAgain()
        {
            for (int i = 0; i < 5; i++)
            {
                MemoryStream gitRequest = GitRequestFromFileWithJunkAppended("push-to-master-one-commit-empty-file.raw", 1024 * 1024 * 128);
                _sut = new ReceivePackInspectStream(gitRequest);

                MemoryStream readData = ReadData();

                // note: this asssertion is much faster than CollectionAssert.AreEqual
                Assert.IsTrue(gitRequest.ToArray().SequenceEqual(readData.ToArray()));
            }
        }

        #region Command Peeking
        [TestMethod]
        public void PeeksProperlyOnPushVeryFirstCommit()
        {
            CreateSutWithRequestData("very-first-push-empty-initial-commit.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "master");
            Assert.AreEqual(_sut.PeekedCommands[0].FullRefName, "refs/heads/master");
        }

        [TestMethod]
        public void PeeksProperlyOnPushOneCommit()
        {
            CreateSutWithRequestData("push-to-master-one-commit-empty-file.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Modify);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "8d2f9c640ffce30ba098c38ac047b693e66ce62a");
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "master");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTwoCommits()
        {
            CreateSutWithRequestData("push-to-master-two-commits-empty-files.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Modify);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "68cf11994ac812ac08aaecbf55ed6e0b02860612");
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "master");
        }

        [TestMethod]
        public void PeeksProperlyOnPushOneCommitWithManyFiles()
        {
            CreateSutWithRequestData("push-to-master-one-commit-10-empty-files.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Modify);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "31a68756b81fd699efabfdd1f713d1ba22699a20");
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "13f6396914e8b2542234f7d24abd14ec34f79180");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "master");
        }

        [TestMethod]
        public void PeeksProperlyOnPushNewBranchWithNoCommits()
        {
            CreateSutWithRequestData("push-to-new-b1-with-no-own-commits.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b1");
        }

        [TestMethod]
        public void PeeksProperlyOnPushNewBranchWithSlashInName()
        {
            CreateSutWithRequestData("push-to-feature-branch-one-commit-empty-file.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Modify);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "8d2f9c640ffce30ba098c38ac047b693e66ce62a");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "feature/foo");
            Assert.AreEqual(_sut.PeekedCommands[0].FullRefName, "refs/heads/feature/foo");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTwoBranchesWithNoCommits()
        {
            CreateSutWithRequestData("push-to-new-b1-and-b2-with-no-commits.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 2);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "a");
            Assert.AreEqual(_sut.PeekedCommands[1].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[1].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[1].NewSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[1].RefName, "b");
        }

        [TestMethod]
        public void PeeksProperlyOnPushOneBranchWithCommit()
        {
            CreateSutWithRequestData("push-to-new-b2-with-one-commit.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "d2f686b3e1871028f23a5dcf4acd89fcac33f33e");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b2");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTwoBranchesWithCommits()
        {
            CreateSutWithRequestData("push-to-new-b1-and-b2-with-one-commit-each.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 2);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "a304e84d44b95e0d6efb55b521bb4e5cf4a54e43");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b3");
            Assert.AreEqual(_sut.PeekedCommands[1].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[1].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[1].NewSha1, "e689f03413b5759fd0a9b2d93871bfaa5a093358");
            Assert.AreEqual(_sut.PeekedCommands[1].RefName, "b4");
        }

        [TestMethod]
        public void PeeksProperlyOnForcePushAendedCommit()
        {
            CreateSutWithRequestData("push-to-new-b1-and-b2-with-one-commit-each.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 2);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "a304e84d44b95e0d6efb55b521bb4e5cf4a54e43");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b3");
            Assert.AreEqual(_sut.PeekedCommands[1].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[1].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[1].NewSha1, "e689f03413b5759fd0a9b2d93871bfaa5a093358");
            Assert.AreEqual(_sut.PeekedCommands[1].RefName, "b4");
        }

        [TestMethod]
        public void PeeksProperlyOnDeleteBranch()
        {
            CreateSutWithRequestData("delete-b1.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Branch);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "b1");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTag()
        {
            CreateSutWithRequestData("push-t1.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTwoTags()
        {
            CreateSutWithRequestData("push-t1-and-t2.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 2);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
            Assert.AreEqual(_sut.PeekedCommands[1].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[1].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[1].NewSha1, "07e9a07587d2c22b1d2bd0cb6b82468831a785c9");
            Assert.AreEqual(_sut.PeekedCommands[1].RefName, "t2");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTenTags()
        {
            CreateSutWithRequestData("push-t1-to-t10.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 10);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "13f6396914e8b2542234f7d24abd14ec34f79180");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
            Assert.AreEqual(_sut.PeekedCommands[5].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[5].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[5].NewSha1, "13f6396914e8b2542234f7d24abd14ec34f79180");
            Assert.AreEqual(_sut.PeekedCommands[5].RefName, "t5");
            Assert.AreEqual(_sut.PeekedCommands[9].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[9].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[9].NewSha1, "13f6396914e8b2542234f7d24abd14ec34f79180");
            Assert.AreEqual(_sut.PeekedCommands[9].RefName, "t9");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTagWithCommits()
        {
            CreateSutWithRequestData("push-t1-with-two-commits.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "9f2e9089c7f05a598cc5de372e6df8f30b887166");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
        }

        [TestMethod]
        public void PeeksProperlyOnPushAnnontatedTag()
        {
            CreateSutWithRequestData("push-at1-with-annontation.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "6d1736c1b7dab50ec77a5a7694797d64dcaa2f2f");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "at1");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTagWithLongName()
        {
            CreateSutWithRequestData("push-tag-with-211-chars-name.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "13f6396914e8b2542234f7d24abd14ec34f79180");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "Lorem_ipsum_dolor_sit_amet,_vocent_referrentur_eu_eos,_forensibus_conclusionemque_et_sea._Esse_deleniti_definitiones_vix_ei,_solum_invenire_in_mea._Legendos_adolescens_inciderint_et_quo,_qui_magna_adipiscing_id");
        }

        [TestMethod]
        public void PeeksProperlyOnPushTagWithLongAnnontation()
        {
            CreateSutWithRequestData("push-tag-with-6437-chars-annontation.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Create);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].NewSha1, "6b0570134e6c29a47d6efbe616185913c92babdb");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "tag-with-long-annontation");
        }

        [TestMethod]
        public void PeeksProperlyOnDeleteTag()
        {
            CreateSutWithRequestData("delete-t1.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "ec97ddee4a8dc3568727d5381afa594fb5eaec2d");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
        }

        [TestMethod]
        public void PeeksProperlyOnDeleteTagWithUnreferencedCommits()
        {
            CreateSutWithRequestData("delete-t1-with-commits-referenced-by-no-branch.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "9f2e9089c7f05a598cc5de372e6df8f30b887166");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "t1");
        }

        [TestMethod]
        public void PeeksProperlyOnDeleteAnnontatedTag()
        {
            CreateSutWithRequestData("delete-at1.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 1);
            Assert.AreEqual(_sut.PeekedCommands[0].CommandType, GitProtocolCommand.Delete);
            Assert.AreEqual(_sut.PeekedCommands[0].RefType, GitRefType.Tag);
            Assert.AreEqual(_sut.PeekedCommands[0].OldSha1, "6d1736c1b7dab50ec77a5a7694797d64dcaa2f2f");
            Assert.AreEqual(_sut.PeekedCommands[0].RefName, "at1");
        }

        [TestMethod]
        public void PeeksNothingOnFlushOnly()
        {
            CreateSutWithRequestData("flush-only.raw");

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 0);
        }

        [TestMethod]
        public void PeeksNothingOnEmptyStream()
        {
            _sut = new ReceivePackInspectStream(new MemoryStream());

            ReadData();

            Assert.AreEqual(_sut.PeekedCommands.Count, 0);
        }
        #endregion

        [TestCleanup]
        public void Cleanup()
        {
            _sut.Dispose();
        }

        private void CreateSutWithRequestData(string requestFileName)
        {
            _sut = new ReceivePackInspectStream(GitRequestFromFile(requestFileName));
        }

        /// <summary>
        ///     Used to extend the PACK portion of the request stream.
        /// </summary>
        private static MemoryStream GitRequestFromFileWithJunkAppended(string requestFileName, int maxBytesOfJunk)
        {
            MemoryStream requestData = GitRequestFromFile(requestFileName, keepPosition: true);

            byte[] junkData = RandomizedBuffer(maxBytesOfJunk);
            requestData.Write(junkData, 0, junkData.Length);

            requestData.Position = 0;
            return requestData;
        }

        private static MemoryStream GitRequestFromFile(string fileName, bool keepPosition = false)
        {
            MemoryStream requestData = new MemoryStream();

            string rawFilePath = Path.Combine(GitAssetsDir, fileName);
            using (FileStream fileContent = new FileStream(rawFilePath, FileMode.Open))
                fileContent.CopyTo(requestData);

            if (!keepPosition)
                requestData.Position = 0;

            return requestData;
        }

        private MemoryStream ReadData(int bufferSize = 1024 * 64)
        {
            MemoryStream result = new MemoryStream();
            var buffer = new byte[bufferSize];

            int bytesRead;
            while ((bytesRead = _sut.Read(buffer, 0, buffer.Length)) > 0)
                result.Write(buffer, 0, bytesRead);

            return result;
        }

        private static byte[] RandomizedBuffer(int size)
        {
            var buffer = new byte[Random.Next(size)];
            Random.NextBytes(buffer);

            return buffer;
        }
    }
}
