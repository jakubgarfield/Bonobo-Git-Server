using System.Threading.Tasks;
using System.Web;
using LibGit2Sharp;

namespace Bonobo.Git.Server.Application.Hooks
{
    public interface IAfterGitPushHandler
    {
        void OnBranchCreated(HttpContext httpContext, GitBranchPushData branchData);
        void OnBranchDeleted(HttpContext httpContext, GitBranchPushData branchData);

        /// <param name="httpContext">The current <see cref="HttpContext" />.</param>
        /// <param name="branchData">Data related to the updated branch and repository.</param>
        /// <param name="isFastForward"><c>true</c> if push was fast forward, <c>false</c> implies force push.</param>
        void OnBranchModified(HttpContext httpContext, GitBranchPushData branchData, bool isFastForward);

        void OnTagCreated(HttpContext httpContext, GitTagPushData tagData);
        void OnTagDeleted(HttpContext httpContext, GitTagPushData tagData);
    }
}