
namespace Bonobo.Git.Server.Data.Update
{
    /// <summary>
    /// When running an update script, the system will first test the Precondition (if it's present)
    /// Assuming the precondition doesn't stop execution, the Command will then be run (if it's present)
    /// Finally, the CodeAction will be called to perform steps which cannot be done by a plain SQL command
    /// </summary>
    public interface IUpdateScript
    {
        /// <summary>
        /// Optional SQL command to execute (pass null to ignore)
        /// </summary>
        string Command { get; }
        /// <summary>
        /// Null always execute this item, or SQL command returning non-zero to execute
        /// </summary>
        string Precondition { get; }
        /// <summary>
        /// A code-based action, if SQL Command is inadequate
        /// </summary>
        void CodeAction(BonoboGitServerContext context);
    }
}