using JsonPatch.Formatting;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Nerd.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            //Patch support
            config.Formatters.Add(new JsonPatchFormatter());

            //Media Type Support
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.Add(new BsonMediaTypeFormatter());
            config.Formatters.Add(new FormUrlEncodedMediaTypeFormatter());




            //Cors Support
            var origins = new List<string>()
                    {    "http://localhost",
                         "http://localhost:59667"
                    };
            var cors = new EnableCorsAttribute(string.Join(",", origins.ToArray()), "*", "GET, POST, OPTIONS, PUT, DELETE");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //Cachecow caching
            var cacheCow= new CacheCow.Server.CachingHandler(config, "");
            config.MessageHandlers.Add(cacheCow);
        }
    }
}
