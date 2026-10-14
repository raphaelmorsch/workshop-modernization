using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace PortalOrdens.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            CompositionRoot.Start();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
