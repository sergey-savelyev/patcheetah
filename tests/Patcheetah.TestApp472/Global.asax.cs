using Patcheetah.JsonNET;
using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Patcheetah.TestApp472
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            PatchEngine.Init(cfg =>
            {
                cfg.EnableAttributes();
                cfg.SetPrePatchProcessingFunction(context =>
                {
                    if (context.NewValue is long lng)
                    {
                        return Convert.ToInt32(lng);
                    }

                    if (context.NewValue is string str && (context.OldValue?.GetType()?.IsEnum ?? false))
                    {
                        return Enum.Parse(context.OldValue.GetType(), str, true);
                    }

                    return context.NewValue;
                });
            });
        }
    }
}
