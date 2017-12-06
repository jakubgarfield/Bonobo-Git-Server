using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bonobo.Git.Server.Helpers
{
    public static class CustomHtmlHelpers
    {
        public static HtmlString AssemblyVersion(this IHtmlHelper helper)
        {
            return new HtmlString(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public static HtmlString MarkdownToHtml(this IHtmlHelper helper, string markdownText)
        {
            return new HtmlString(CommonMark.CommonMarkConverter.Convert(markdownText));
        }

        public static HtmlString DisplayEnum(this IHtmlHelper helper, Enum e)
        {
            string result = "[[" + e.ToString() + "]]";
            var memberInfo = e.GetType().GetMember(e.ToString()).FirstOrDefault();
            if (memberInfo != null)
            {
                var display = memberInfo.GetCustomAttributes(false)
                    .OfType<DisplayAttribute>()
                    .LastOrDefault();

                if (display != null)
                {
                    result = display.GetName();
                }
            }

            return new HtmlString(result);
        }
    }
}
