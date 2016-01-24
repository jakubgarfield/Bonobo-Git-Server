using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Text;
using System.Web.Routing;
using System.Linq.Expressions;
using Bonobo.Git.Server.Models;
using MarkdownDeep;

namespace Bonobo.Git.Server.Helpers
{
    public static class CustomHtmlHelpers
    {
        public static IHtmlString AssemblyVersion(this HtmlHelper helper)
        {
            return MvcHtmlString.Create(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public static IHtmlString MarkdownToHtml(this HtmlHelper helper, string markdownText)
        {
            Markdown markdown = new Markdown() { ExtraMode = true, SafeMode = true };
            return MvcHtmlString.Create(markdown.Transform(markdownText));
        }

    }
}
