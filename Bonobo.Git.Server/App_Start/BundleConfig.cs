using System.Web.Optimization;

namespace Bonobo.Git.Server.App_Start
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
           
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                         "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Content/uni/js/uni-form-validation.jquery.js",
                        "~/Scripts/MicrosoftAjax.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/components/pure/pure-min.css",
                        "~/Content/components/font-awesome/css/font-awesome.min.css",
                        "~/Content/fonts.css",
                        "~/Content/site.css"));
        }
    }
}