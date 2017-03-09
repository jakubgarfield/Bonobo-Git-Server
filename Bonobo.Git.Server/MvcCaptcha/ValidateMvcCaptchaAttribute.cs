using System;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace TSharp.Core.Mvc
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ValidateMvcCaptchaAttribute : ActionFilterAttribute
    {
        /// <summary>
        ///     Initializes a new instance of the CaptchaValidationAttribute class.
        /// </summary>
        public ValidateMvcCaptchaAttribute()
            : this("_mvcCaptchaText")
        {
        }

        //private static readonly string SESSION_AUTH_CODE = "SESSION_AUTH_CODE";
        /// <summary>
        ///     Initializes a new instance of the CaptchaValidationAttribute class.
        /// </summary>
        /// <param name="field">The field.</param>
        public ValidateMvcCaptchaAttribute(string field)
        {
            Field = field;
        }

        /// <summary>
        ///     Gets or sets the field.
        /// </summary>
        /// <value>The field.</value>
        public string Field { get; private set; }

        [OptionalDependency]
        public ISmartCaptcha Judger { get; set; }

        /// <summary>
        ///     Called when [action executed].
        /// </summary>
        /// <param name="filterContext">The filter filterContext.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // var judger = DependencyResolver.Current.GetService<ISmartCaptcha>();
            if ((Judger != null) && !Judger.Enable(filterContext.HttpContext))
                return;

            // get the guid from the post back 
            var guid = filterContext.HttpContext.Request.Form["_MvcCaptchaGuid"];

            // get values 
            var image = MvcCaptchaImage.GetCachedCaptcha(guid);
            var actualValue = filterContext.HttpContext.Request.Form[Field];
            var expectedValue = image == null ? string.Empty : image.Text;

            // removes the captch from Session so it cannot be used again 
            filterContext.HttpContext.Session.Remove(guid);

            var isValid = !string.IsNullOrEmpty(actualValue)
                          && !string.IsNullOrEmpty(expectedValue)
                          && string.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
            if (!isValid)
                ((Controller)filterContext.Controller).ModelState.AddModelError(Field,
                    CaptchaResource.Captcha_Incorrect);
            //(string)filterContext.HttpContext.GetGlobalResourceObject("LangPack","ValidationCode_Not_Match"));
        }
    }
}