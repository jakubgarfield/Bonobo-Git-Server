using System.Web;
using System.Web.Mvc;
using TSharp.Core.Web;

namespace TSharp.Core.Mvc
{
    public class _MvcCaptchaController : Controller
    {
        public ActionResult MvcCaptchaImage()
        {
            return new MvcCaptchaImageResult();
        }

        public ActionResult MvcCaptchaLoader()
        {
            string prevGuid = Request.ServerVariables["Query_String"];
            if (!string.IsNullOrEmpty(prevGuid))
                HttpContext.Session.Remove(prevGuid);
            var options = new MvcCaptchaOptions();
            MvcCaptchaConfigSection config = MvcCaptchaConfigSection.GetConfig();
            if (config != null)
            {
                options.TextChars = config.TextChars;
                options.TextLength = config.TextLength;
                options.FontWarp = config.FontWarp;
                options.BackgroundNoise = config.BackgroundNoise;
                options.LineNoise = config.LineNoise;
            }

            var image = new MvcCaptchaImage(options);
            HttpContext.Session.Add(
                image.UniqueId,
                image);
            HttpContext.Response.Cache.SetNoStore();
            HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(image.UniqueId);
        }
    }
}