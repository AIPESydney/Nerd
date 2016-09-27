using JsonPatch.Formatting;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
using System.Net.Http.Extensions.Compression.Core.Compressors;

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
                         "http://localhost:59031"
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


        private const int webApiGzipResponseThresholdBytes = 860;
        public static void CompressResponses(HttpConfiguration config)
        {
            var handler = webApiGzipResponseThresholdBytes>0
                ? new ServerCompressionHandler(webApiGzipResponseThresholdBytes, new GZipCompressor(), new DeflateCompressor())
                : new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor());
            GlobalConfiguration.Configuration.MessageHandlers.Insert(0, handler);
        }


    }
}
