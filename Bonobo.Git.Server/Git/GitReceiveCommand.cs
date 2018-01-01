namespace Bonobo.Git.Server.Git {
    public struct GitReceiveCommand {
        public static GitReceiveCommand Invalid = default(GitReceiveCommand);

        /// <summary>
        ///     Full name of the ref, e.g. "refs/heads/master"
        /// </summary>
        public string FullRefName { get; set; }

        public GitRefType RefType { get; set; }

        /// <summary>
        ///     Branch name as it was extracted from <see cref="RefName" />
        ///     e.g. "master" or "feature/foo"
        /// </summary>
        public string RefName { get; set; }

        public GitProtocolCommand CommandType { get; set; }

        /// <summary>
        ///     SHA1 identifier of the old object as hex string.
        /// </summary>
        /// <remarks>
        ///     40 characters long SHA1 hash in hex.
        ///     Will be zero if <c>CommandType</c> is <c>CommandType.Create</c>.
        /// </remarks>
        public string OldSha1 { get; set; }

        /// <summary>
        ///     SHA1 identifier of the new object as hex string.
        /// </summary>
        /// <remarks>
        ///     40 characters long SHA1 hash in hex.
        ///     Will be zero if <c>CommandType</c> is <c>CommandType.Delete</c>.
        /// </remarks>
        public string NewSha1 { get; set; }

        public GitReceiveCommand(string fullRefName, string oldSha1, string newSha1) {
            this.OldSha1 = oldSha1;
            this.NewSha1 = newSha1;

            const string zeroId = "0000000000000000000000000000000000000000";
            if (oldSha1 == zeroId)
                this.CommandType = GitProtocolCommand.Create;
            else if (newSha1 == zeroId)
                this.CommandType = GitProtocolCommand.Delete;
            else
                this.CommandType = GitProtocolCommand.Modify;

            this.FullRefName = fullRefName;
            int firstSlashPos = fullRefName.IndexOf('/');
            int secondSlashPos = fullRefName.IndexOf('/', firstSlashPos + 1);
            var refTypeRaw = fullRefName.Substring(firstSlashPos + 1, secondSlashPos - firstSlashPos - 1);
            this.RefName = fullRefName.Substring(secondSlashPos + 1);
            
            if (refTypeRaw == "heads")
                this.RefType = GitRefType.Branch;
            else if (refTypeRaw == "tags")
                this.RefType = GitRefType.Tag;
            else
                this.RefType = GitRefType.Unknown;
        }
    }
}