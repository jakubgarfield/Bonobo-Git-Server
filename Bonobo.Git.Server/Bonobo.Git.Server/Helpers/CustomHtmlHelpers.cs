using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Reflection;
using System.Text;

namespace Bonobo.Git.Server.Helpers
{
    public static class CustomHtmlHelpers
    {
        public static MvcHtmlString CheckboxList(this HtmlHelper html, string name, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            Dictionary<string, string> htmlAttrDict = new Dictionary<string, string>();
            if (htmlAttributes != null)
            {
                foreach (var prop in htmlAttributes.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    htmlAttrDict.Add(prop.Name, prop.GetValue(htmlAttributes, null).ToString());
                }
            }

            StringBuilder sb = new StringBuilder();

            TagBuilder ul = new TagBuilder("ul");

            foreach (var attr in htmlAttrDict)
            {
                ul.Attributes.Add(new KeyValuePair<string, string>(attr.Key, attr.Value));
            }

            if (selectList != null)
            {
                foreach (SelectListItem listItem in selectList)
                {
                    TagBuilder li = new TagBuilder("li");
                    TagBuilder input = new TagBuilder("input");
                    TagBuilder label = new TagBuilder("label");

                    input.Attributes.Add(new KeyValuePair<string, string>("type", "checkbox"));
                    input.Attributes.Add(new KeyValuePair<string, string>("name", name));
                    input.Attributes.Add(new KeyValuePair<string, string>("id", name + "_" + listItem.Value));

                    if (listItem.Selected)
                    {
                        input.Attributes.Add(new KeyValuePair<string, string>("checked", "checked"));
                    }

                    label.Attributes.Add(new KeyValuePair<string, string>("for", name + "_" + listItem.Value));
                    label.InnerHtml = listItem.Text;

                    li.InnerHtml = input.ToString() + label.ToString();

                    ul.InnerHtml += li.ToString();
                }

            }
            sb.Append(ul.ToString());

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString CheckboxList(this HtmlHelper html, string name, IEnumerable<SelectListItem> selectList)
        {
            return CheckboxList(html, name, selectList, null);
        }

    }
}