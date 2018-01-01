using LibGit2Sharp;

namespace Bonobo.Git.Server.Application.Hooks
{
    public struct GitTagPushData
    {
        public string RepositoryName { get; set; }
        public Repository Repository { get; set; }

        /// <summary>
        ///     Full name of the ref, e.g. "refs/tags/v.1.0"
        /// </summary>
        public string RefName { get; set; }

        /// <summary>
        ///     Tag name as it was extracted from <see cref="RefName" />
        ///     e.g. "v.1.0" or "foo-bar"
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        ///     The commit referenced by the tag.
        /// </summary>
        /// <returns>
        ///     The 40 characters long SHA1 hash in hex.
        /// </returns>
        public string ReferenceCommitSha { get; set; }
    }
}