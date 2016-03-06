using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Reflection;
using System.Text;
using System.Web.Routing;
using System.Linq.Expressions;
using Bonobo.Git.Server.Models;
using MarkdownDeep;
using System.ComponentModel.DataAnnotations;

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

        public static MvcHtmlString DisplayEnum(this HtmlHelper helper, Enum e)
        {
            string result = "[[" + e.ToString() + "]]";

            var display = e.GetType()
                       .GetMember(e.ToString()).First()
                       .GetCustomAttributes(false)
                       .OfType<DisplayAttribute>()
                       .LastOrDefault();

            if (display != null)
            {
                result = display.GetName();
            }

            return MvcHtmlString.Create(result);
        }
    }
}
