using System;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Twitter;
using Owin;
using Nerd.Api.Providers;
using Nerd.Api.Models;
namespace Nerd.Api
{
    public partial class Startup
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static readonly string OAuthFacebookAppId = ConfigurationManager.AppSettings["FacebookAppId"];
        public static readonly string OAuthFacebookSecret = ConfigurationManager.AppSettings["FacebookSecret"];
        public static readonly string OAuthTwitterConsumerKey = ConfigurationManager.AppSettings["Twitter_ConsumerKey"];
        public static readonly string OAuthTwitterConsumerSecret = ConfigurationManager.AppSettings["Twitter_ConsumerSecret"];
        public static readonly string OAuthGoogleClientId = ConfigurationManager.AppSettings["Google_ClientId"];
        public static string PublicClientId { get; private set; }


        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);



            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseCookieAuthentication(new CookieAuthenticationOptions());
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Configure the application for OAuth based flow
            PublicClientId = "clientid3ed54ggm5f4l";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),
                AllowInsecureHttp = true,
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");
            var twitterAuthenticationOptions = new TwitterAuthenticationOptions()
            {
                ConsumerKey = OAuthTwitterConsumerKey,
                ConsumerSecret = OAuthTwitterConsumerSecret,
                Provider = new Nerd.Api.Providers.TwitterAuthenticationProvider(),
                BackchannelCertificateValidator = new Microsoft.Owin.Security.CertificateSubjectKeyIdentifierValidator(new[]
            {
                "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
                "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
                "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
                "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
                "4eb6d578499b1ccf5f581ead56be3d9b6744a5e5", // VeriSign Class 3 Primary CA - G5
                "5168FF90AF0207753CCCD9656462A212B859723B", // DigiCert SHA2 High Assurance Server C‎A 
                "B13EC36903F8BF4701D498261A0802EF63642BC3" // DigiCert High Assurance EV Root CA
            })
            };

            app.UseTwitterAuthentication(twitterAuthenticationOptions);



            var facebookAuthenticationOptions = new FacebookAuthenticationOptions()
            {
                AppId = OAuthFacebookAppId,
                AppSecret = OAuthFacebookSecret,
                Provider = new Nerd.Api.Providers.FacebookAuthenticationProvider(),
            };
            facebookAuthenticationOptions.Scope.Add("email");
            app.UseFacebookAuthentication(facebookAuthenticationOptions);


        }
    }
}
