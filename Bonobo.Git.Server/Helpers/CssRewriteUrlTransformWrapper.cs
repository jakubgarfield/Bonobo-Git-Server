using System.Web;
using System.Web.Optimization;

namespace Bonobo.Git.Server.Helpers
{
    public class CssRewriteUrlTransformWrapper : IItemTransform
    {
        public string Process(string includedVirtualPath, string input)
        {
            return new CssRewriteUrlTransform().Process(
                "~" + VirtualPathUtility.ToAbsolute(includedVirtualPath),
                input);
        }
    }
}