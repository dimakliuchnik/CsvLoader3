using System.Web.Mvc;
using System.Web.Routing;

namespace CsvLoader3
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "GoToUpload",
            //    "Files/Upload/{fileName}",
            //    new { controller = "Files", action = "Upload", fileName = UrlParameter.Optional}
            //);


            routes.MapRoute(
                 name: "Default",
                 url: "{controller}/{action}/{id}",
                 defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
             );
        }
    }
}