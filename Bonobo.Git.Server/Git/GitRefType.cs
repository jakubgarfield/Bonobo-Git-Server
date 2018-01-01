namespace Bonobo.Git.Server.Git {
    /// <summary>
    ///    Interesting ref types found in refnames of command_pkt.
    /// </summary>
    public enum GitRefType {
        Unknown,
        Tag,
        Branch,
    }
}