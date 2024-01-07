using System.Web;
using System.Web.Optimization;

namespace PhotosManager
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/site.css",
                      "~/Content/photo.css",
                      "~/Content/popup.css",
                      "~/Content/tooltip.css",
                      "~/Content/user.css",
                      "~/Content/customScrollBar.css"
                      ));

            bundles.Add(new ScriptBundle("~/bundles/js").Include(
                       "~/Scripts/session.js",
                       "~/Scripts/validation.js",
                       "~/Scripts/imageControl.js",
                        "~/Scripts/bootbox.js",
                       "~/Scripts/jquery.maskedinput.js"));

        }
    }
}
