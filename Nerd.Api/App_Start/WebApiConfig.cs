using JsonPatch.Formatting;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Nerd.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //new line
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));


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
                         "http://localhost:59031",
                         "http://192.168.34.70:8100"
                    };
            var cors = new EnableCorsAttribute(string.Join(",", origins.ToArray()), "*", "GET, POST, OPTIONS, PUT, DELETE");
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "AccountApi",
                routeTemplate: "api/account/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null
                );
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional },
                constraints: null
                //handler: HttpClientFactory.CreatePipeline(
                //          new HttpControllerDispatcher(config),
                //          new DelegatingHandler[] { new ApiKeyHandler("1qaz2wsx3edc4rfv5tgb6yhn") })  // per-route message handler
                );

            //Cachecow caching
            var cacheCow= new CacheCow.Server.CachingHandler(config, "");
            config.MessageHandlers.Add(cacheCow);
            
            // removes contract serializable attributes
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = true };
        }
    }
}
