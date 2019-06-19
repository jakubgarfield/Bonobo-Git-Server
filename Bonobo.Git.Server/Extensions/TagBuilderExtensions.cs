using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonobo.Git.Server.Extensions
{
    internal static class TagBuilderExtensions
    {
        public static string ToString(this TagBuilder @this, TagRenderMode renderMode)
        {
            @this.TagRenderMode = renderMode;
            var strWriter = new StringWriter();
            @this.WriteTo(strWriter, HtmlEncoder.Default);
            return strWriter.ToString();
        }
    }
}
