namespace Bonobo.Git.Server.Git {
    /// <summary>
    ///    Commands which can be represented by git pkt-lines.
    /// </summary>
    public enum GitProtocolCommand {
        Create,
        Delete,
        Modify
    }
}