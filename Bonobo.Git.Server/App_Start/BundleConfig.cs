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

            bundles.Add(new ScriptBundle("~/bundles/syntaxhighlighter").Include(
                        "~/Content/syntaxhighlighter/scripts/shCore.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushJScript.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushBash.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushCpp.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushCsharp.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushCss.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushDelphi.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushDiff.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushErlang.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushJava.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushPerl.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushPhp.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushPowerShell.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushPython.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushRuby.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushScala.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushSql.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushVb.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushXml.js",
                        "~/Content/syntaxhighlighter/scripts/shBrushPlain.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/components/pure/pure-min.css",
                        "~/Content/fonts.css",
                        "~/Content/site.css",
                        "~/Content/uni/css/uni-form.css",
                        "~/Content/uni/css/default.uni-form.css",
                        "~/Content/syntaxhighlighter/styles/shCoreDefault.css"));
        }
    }
}