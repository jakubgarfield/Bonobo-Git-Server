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

        public static MvcHtmlString CheckboxListFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<SelectListItem> selectList, object htmlAttributes)
        {
            StringBuilder sb = new StringBuilder();
            TagBuilder ul = new TagBuilder("ul");

            ul.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            ModelMetadata metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
            string propertyName = ExpressionHelper.GetExpressionText(expression);            
            TModel model = (TModel)helper.ViewContext.ViewData.ModelMetadata.Model;        
            IEnumerable<TValue> collection = expression.Compile().Invoke(model);

            if (selectList != null)
            {
                foreach (SelectListItem listItem in selectList)
                {
                    TagBuilder li = new TagBuilder("li");
                    TagBuilder input = new TagBuilder("input");
                    TagBuilder label = new TagBuilder("label");

                    input.Attributes.Add(new KeyValuePair<string, string>("type", "checkbox"));
                    input.Attributes.Add(new KeyValuePair<string, string>("name", propertyName));
                    input.Attributes.Add(new KeyValuePair<string, string>("value", listItem.Value));
                    input.Attributes.Add(new KeyValuePair<string, string>("id", propertyName + "_" + listItem.Value));

                    bool selected = listItem.Selected;
                    if (!selected && collection != null) { selected = (from v in collection where v.Equals(listItem.Value) select v).Any(); } 

                    if (selected)
                    {
                        input.Attributes.Add(new KeyValuePair<string, string>("checked", "checked"));
                    }

                    label.Attributes.Add(new KeyValuePair<string, string>("for", propertyName + "_" + listItem.Value));
                    label.InnerHtml = listItem.Text;

                    li.InnerHtml = input.ToString() + label.ToString();

                    ul.InnerHtml += li.ToString();
                }

            }
            sb.Append(ul.ToString());

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString CheckboxListFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<SelectListItem> selectList)
        {
            return CheckboxListFor<TModel, TValue>(helper, expression, selectList, null);
        }
    }
}
