using System.Web;

namespace TSharp.Core.Mvc
{
    public interface ISmartCaptcha
    {
        bool Enable(HttpContextBase context);
    }
}