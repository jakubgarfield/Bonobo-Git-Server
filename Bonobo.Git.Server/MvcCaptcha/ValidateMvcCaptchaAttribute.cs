using System;
using System.Web.Mvc;

namespace TSharp.Core.Mvc
{
    /// <summary>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
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


        /// <summary>
        ///     Called when [action executed].
        /// </summary>
        /// <param name="filterContext">The filter filterContext.</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            // get the guid from the post back 
            string guid = filterContext.HttpContext.Request.Form["_MvcCaptchaGuid"];

            // get values 
            var image = MvcCaptchaImage.GetCachedCaptcha(guid);
            string actualValue = filterContext.HttpContext.Request.Form[Field];
            string expectedValue = image == null ? String.Empty : image.Text;

            // removes the captch from Session so it cannot be used again 
            filterContext.HttpContext.Session.Remove(guid);

            bool isValid = !String.IsNullOrEmpty(actualValue)
                           && !String.IsNullOrEmpty(expectedValue)
                           && String.Equals(actualValue, expectedValue, StringComparison.OrdinalIgnoreCase);
            if (!isValid)
                ((Controller) filterContext.Controller).ModelState.AddModelError(Field, Captcha.Captcha_Incorrect);
            //(string)filterContext.HttpContext.GetGlobalResourceObject("LangPack","ValidationCode_Not_Match"));
        }
    }
}