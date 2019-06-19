using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using Bonobo.Git.Server.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

namespace Bonobo.Git.Server.Helpers
{
    public static class CustomHtmlHelpers
    {
        private static readonly string NoDataMessage = "No Records...";

        private static readonly string EmptyModelMessage =
            "View Model cannot be null! Please make sure your View Model is created and passed to this View";

        private static readonly string EmptyNameMessage = "Name of the CheckBoxList cannot be null or empty";

        public static IHtmlContent AssemblyVersion(this IHtmlHelper helper)
        {
            return new HtmlString(Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        public static IHtmlContent MarkdownToHtml(this IHtmlHelper helper, string markdownText)
        {
            return new HtmlString(CommonMark.CommonMarkConverter.Convert(markdownText));
        }

        public static HtmlString DisplayEnum(this IHtmlHelper helper, Enum e)
        {
            string result = "[[" + e + "]]";
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

        // Originally from https://github.com/mikhail-tsennykh/MvcCheckBoxList
        public static IHtmlContent CheckBoxListFor<TModel, TProperty, TItem, TValue, TKey>(
            this IHtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, TProperty>> listNameExpr,
            Expression<Func<TModel, IEnumerable<TItem>>> sourceDataExpr,
            Expression<Func<TItem, TValue>> valueExpr,
            Expression<Func<TItem, TKey>> textToDisplayExpr,
            Expression<Func<TModel, IEnumerable<TItem>>> selectedValuesExpr)
        {
            var listName = ExpressionHelper.GetExpressionText(listNameExpr);

            if (sourceDataExpr == null || sourceDataExpr.Body.ToString() == "null")
                return new HtmlString(NoDataMessage);
            if (htmlHelper.ViewData.Model == null) throw new NoNullAllowedException(EmptyModelMessage);
            if (string.IsNullOrEmpty(listName)) throw new ArgumentException(EmptyNameMessage, nameof(listNameExpr));

            var model = htmlHelper.ViewData.Model;
            var sourceData = sourceDataExpr.Compile()(model).ToList();
            var valueFunc = valueExpr.Compile();
            var textToDisplayFunc = textToDisplayExpr.Compile();
            var selectedItems = new List<TItem>();
            var selectedItemsTemp = selectedValuesExpr?.Compile()(model);
            if (selectedItemsTemp != null) selectedItems = selectedItemsTemp.ToList();
            var selectedValues = selectedItems.Select(s => valueFunc(s).ToString()).ToList();

            if (!sourceData.Any()) return new HtmlString(NoDataMessage);

            var sb = new StringBuilder();
            var linkedLabelCounter = 0;

            foreach (var item in sourceData)
            {
                var itemValue = valueFunc(item).ToString();
                var itemText = textToDisplayFunc(item).ToString();

                linkedLabelCounter = GenerateCheckBoxListElement(sb, linkedLabelCounter, htmlHelper,
                    selectedValues, listName, itemValue,
                    itemText);
            }

            return new HtmlString(sb.ToString());
        }

        private static int GenerateCheckBoxListElement(StringBuilder sb,
            int linkedLabelCounter, IHtmlHelper htmlHelper,
            List<string> selectedValues, string name, string itemValue, string itemText)
        {
            var fullName = htmlHelper.ViewData.TemplateInfo.GetFullHtmlFieldName(name);

            var checkboxBuilder = new TagBuilder("input");

            if (selectedValues.Any(value => value == itemValue)) checkboxBuilder.MergeAttribute("checked", "checked");

            checkboxBuilder.MergeAttribute("type", "checkbox");
            checkboxBuilder.MergeAttribute("value", itemValue);
            checkboxBuilder.MergeAttribute("name", fullName);

            var linkId = htmlHelper.GenerateIdFromName(htmlHelper.ViewData.TemplateInfo.GetFullHtmlFieldName(name)) +
                         linkedLabelCounter++;
            checkboxBuilder.GenerateId(linkId, "?");
            var linkedLabelBuilder = new TagBuilder("label");
            linkedLabelBuilder.MergeAttribute("for", linkId);
            linkedLabelBuilder.InnerHtml.Clear();
            linkedLabelBuilder.InnerHtml.Append(itemText);

            // if there are any errors for a named field, we add the css attribute
            if (htmlHelper.ViewData.ModelState.TryGetValue(fullName, out var modelStateEntry))
            {
                if (modelStateEntry.Errors.Count > 0)
                {
                    checkboxBuilder.AddCssClass(HtmlHelper.ValidationInputCssClassName);
                }
            }

            var modelExplorer =
                ExpressionMetadataProvider.FromStringExpression(name, htmlHelper.ViewData, htmlHelper.MetadataProvider);
            var validationAttributeProvider =
                htmlHelper.ViewContext.HttpContext.RequestServices.GetService(typeof(ValidationHtmlAttributeProvider))
                    as ValidationHtmlAttributeProvider;
            validationAttributeProvider?.AddAndTrackValidationAttributes(htmlHelper.ViewContext, modelExplorer, name,
                checkboxBuilder.Attributes);

            sb.Append(checkboxBuilder.ToString(TagRenderMode.SelfClosing));
            sb.Append(linkedLabelBuilder.ToString(TagRenderMode.Normal));
            sb.Append("<br/>");

            return linkedLabelCounter;
        }
    }
}