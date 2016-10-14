using System.Web;
using System.Web.Mvc;

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
            var prevGuid = Request.ServerVariables["Query_String"];
            if (!string.IsNullOrEmpty(prevGuid))
                Session.Remove(prevGuid);
            var options = new MvcCaptchaOptions();
            var config = MvcCaptchaConfigSection.GetConfig();
            if (config != null)
            {
                options.TextChars = config.TextChars;
                options.TextLength = config.TextLength;
                options.FontWarp = config.FontWarp;
                options.BackgroundNoise = config.BackgroundNoise;
                options.LineNoise = config.LineNoise;
            }

            var image = new MvcCaptchaImage(options);
            Session.Add(
                image.UniqueId,
                image);
            Response.Cache.SetNoStore();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return Content(image.UniqueId);
        }
    }
}