using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Nerd.Api;
using Nerd.Api.Models;
using System.Web.Http.Cors;
using System.Collections.Generic;
using Nerd.Api.Results;
using System.Security.Claims;
using Nerd.Api.Providers;
using Microsoft.Owin.Security.OAuth;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Configuration;
using System.Text;
using Nerd.Api.Repository;

namespace Nerd.Api.Controllers
{

    [Authorize]
    [RoutePrefix("api/Account")]
    //[RequireHttpsAttribute]
    [EnableCors(origins: "*", headers: "*", methods: "*")]

    public class AccountController : ApiController
    {
        private ApplicationUserManager _userManager;
        private ApplicationSignInManager _signInManager;
        private readonly UnitOfWork _unitOfWork;
        private const string LocalLoginProvider = "Local";

        public ApplicationUserManager UserManager
        {
            get
            {
                if (_userManager == null)
                {
                    _userManager = _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }
                return _userManager ?? (_userManager = ApplicationUserManager.Create(
                    new IdentityFactoryOptions<ApplicationUserManager>()
                    {
                        Provider = new IdentityFactoryProvider<ApplicationUserManager>()
                    }, Request.GetOwinContext()));
            }
            private set
            {
                _userManager = value;
            }
        }
        private IAuthenticationManager AuthenticationManager
        {
            get { return Request.GetOwinContext().Authentication; }
        }
        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? Request.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }
        //public AccountController(ApplicationUserManager userManager
        //   )
        //{
        //    UserManager = userManager;

        //}
        public AccountController() {
            _unitOfWork = new UnitOfWork();
        }


        // POST api/Account/Logout
        [Route("Logout")]
        public IHttpActionResult Logout()
        {
            AuthenticationManager.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return Ok();
        }



        // POST api/Account/ChangePassword
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }



        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterBindingModel model)
        {
            if (!ModelState.IsValid || model == null || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(ModelState);
            }
            
            var user = new ApplicationUser()
            {
                UserName = model.UserName,
                Email = model.Email,
            };

            IdentityResult result = await UserManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            await UserManager.AddToRoleAsync(user.Id, "Users");

            var userdata = new Nerd.Data.User
            {
                AspNetUserId = user.Id,
                CreatedDate = DateTime.Now
            };

            await _unitOfWork.UserRepository.InsertAsync(userdata);
           
            return Ok();
        }



        // GET api/Account/ExternalLogin
        [OverrideAuthentication]
        [HostAuthentication(DefaultAuthenticationTypes.ExternalCookie)]
        [AllowAnonymous]
        [Route("ExternalLogin", Name = "ExternalLogin")]
        public async Task<IHttpActionResult> GetExternalLogin(string provider, string error = null)
        {
            string redirectUri = string.Empty;
            if (error != null)
            {
                return Redirect(Url.Content("~/") + "#error=" + Uri.EscapeDataString(error));
            }

            if (!User.Identity.IsAuthenticated)
            {
                return new ChallengeResult(provider, this);
            }
            var redirectUriValidationResult = ValidateClientAndRedirectUri(this.Request, ref redirectUri);
            if (!string.IsNullOrWhiteSpace(redirectUriValidationResult))
            {
                return BadRequest(redirectUriValidationResult);
            }

            ExternalLoginData externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);

            if (externalLogin == null)
            {
                return InternalServerError();
            }

            if (externalLogin.LoginProvider != provider)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
                return new ChallengeResult(provider, this);
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(externalLogin.LoginProvider,
                externalLogin.ProviderKey));

            bool hasRegistered = user != null;

            if (hasRegistered)
            {
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);

                ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                   OAuthDefaults.AuthenticationType);
                ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                    CookieAuthenticationDefaults.AuthenticationType);

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                AuthenticationManager.SignIn(properties, oAuthIdentity, cookieIdentity);

                externalLogin.UserName = user.UserName;
            }
            else
            {
                IEnumerable<Claim> claims = externalLogin.GetClaims();
                ClaimsIdentity identity = new ClaimsIdentity(claims, OAuthDefaults.AuthenticationType);
                AuthenticationManager.SignIn(identity);
            }

            if (string.IsNullOrEmpty(externalLogin.ExternalAccessTokenSecret))
            {
                externalLogin.ExternalAccessTokenSecret = "qaz12ws";
            }



            redirectUri = string.Format("{0}#external_access_token={1}&provider={2}&haslocalaccount={3}&external_user_name={4}&externalEmail={5}&externalAccessTokenSecret={6}",
                                redirectUri,
                                externalLogin.ExternalAccessToken,
                                externalLogin.LoginProvider,
                                hasRegistered.ToString(),
                                externalLogin.UserName,
                                externalLogin.Email,
                                externalLogin.ExternalAccessTokenSecret
                                );


            return Redirect(redirectUri);
            //return Ok();
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true
        [AllowAnonymous]
        [Route("ExternalLogins")]
        public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        {
            IEnumerable<AuthenticationDescription> descriptions = AuthenticationManager.GetExternalAuthenticationTypes();
            List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

            string state;

            if (generateState)
            {
                const int strengthInBits = 256;
                state = RandomOAuthStateGenerator.Generate(strengthInBits);
            }
            else
            {
                state = null;
            }

            foreach (AuthenticationDescription description in descriptions)
            {
                ExternalLoginViewModel login = new ExternalLoginViewModel
                {
                    Name = description.Caption,
                    Url = Url.Route("ExternalLogin", new
                    {
                        provider = description.AuthenticationType,
                        response_type = "token",
                        client_id = Startup.PublicClientId,
                        redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
                        state = state
                    }),
                    State = state
                };
                logins.Add(login);
            }

            return logins;
        }

        // POST api/Account/RegisterExternal
        [AllowAnonymous]
        [Route("RegisterExternal")]
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        public async Task<IHttpActionResult> RegisterExternal(RegisterExternalBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // External Bearer cookie was used to register with external providers.we changed this.
            //var info = await Authentication.GetExternalLoginInfoAsync();  

            //blog:http://bitoftech.net/2014/08/11/asp-net-web-api-2-external-logins-social-logins-facebook-google-angularjs-app/


            var verifiedAccessToken = await VerifyExternalAccessToken(model.Provider, model.ExternalAccessToken, model.ExternalAccessTokenSecret);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            var applicationUsername = "";
            var firstname = "";
            var lastname = "";
            try
            {
                if (string.IsNullOrEmpty(model.Email))
                {
                    model.Email = string.Format("{0}@nerding.com.au", Guid.NewGuid().ToString());
                }
                switch (model.Provider)
                {
                    case "Facebook":
                        applicationUsername = model.Email;
                        firstname = model.UserName.Split(' ').FirstOrDefault();
                        lastname = model.UserName.Split(' ').Skip(1).FirstOrDefault();
                        break;
                    case "Twitter":
                        applicationUsername = verifiedAccessToken.RetrievedUserName;
                        firstname = verifiedAccessToken.RetrievedName.Split(' ').FirstOrDefault();
                        lastname = verifiedAccessToken.RetrievedName.Split(' ').Skip(1).FirstOrDefault();
                        break;
                }
            }
            finally
            {

            }
            var user = new ApplicationUser()
            {
                UserName = applicationUsername,
                Email = model.Email,
            };


            IdentityResult result = await UserManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            await UserManager.AddToRoleAsync(user.Id, "Users");

            result = await UserManager.AddLoginAsync(user.Id, new UserLoginInfo(model.Provider, verifiedAccessToken.UserId));
            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }
            var accessTokenResponse = await GenerateLocalAccessTokenResponse(model.Provider, applicationUsername, model.Email, user);

            return Ok(new
            {
                UserId = user.Id,
                AccessToken = accessTokenResponse
            });
        }



        [AllowAnonymous]
        [HttpGet]
        [Route("ObtainLocalAccessToken")]
        public async Task<IHttpActionResult> ObtainLocalAccessToken(string provider, string externalAccessToken, string externalAccessTokenSecret)
        {

            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(externalAccessToken))
            {
                return BadRequest("Provider or external access token is not sent");
            }

            var verifiedAccessToken = await VerifyExternalAccessToken(provider, externalAccessToken, externalAccessTokenSecret);
            if (verifiedAccessToken == null)
            {
                return BadRequest("Invalid Provider or External Access Token");
            }

            ApplicationUser user = await UserManager.FindAsync(new UserLoginInfo(provider, verifiedAccessToken.UserId));

            bool hasRegistered = user != null;

            if (!hasRegistered)
            {
                return BadRequest("External user is not registered");
            }
            //generate access token response
            var accessTokenResponse = await GenerateLocalAccessTokenResponse(provider, user.UserName, user.Email, user);
            return Ok(accessTokenResponse);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                if (_userManager != null)
                {
                    _userManager.Dispose();
                }

            }

            base.Dispose(disposing);
        }



        #region TwoFactorAuthentication SMS
        [HttpPost, Authorize]
        public async Task<IHttpActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                return Ok();
            }
            return NotFound();
        }
        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost, Authorize]
        public async Task<IHttpActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                return Ok();
            }
            return NotFound();
        }

        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return Ok();
        }

        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager,
                       OAuthDefaults.AuthenticationType);
                    ClaimsIdentity cookieIdentity = await user.GenerateUserIdentityAsync(UserManager,
                        CookieAuthenticationDefaults.AuthenticationType);

                    AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.UserName);
                    AuthenticationManager.SignIn(properties, oAuthIdentity, cookieIdentity);
                }
                return Ok();
            }
            return BadRequest("Failed to verify phone");
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return BadRequest("Error");
            }
            return Ok();
        }
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return Ok();
                case SignInStatus.LockedOut:
                    return BadRequest("Lockout");
                case SignInStatus.Failure:
                default:
                    return BadRequest("Invalid code.");
            }
        }
        #endregion

        #region Helpers

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                return HttpServerUtility.UrlTokenEncode(data);
            }
        }
        #endregion

        #region Helper Functions For External Authentication
        private string ValidateClientAndRedirectUri(HttpRequestMessage request, ref string redirectUriOutput)
        {

            Uri redirectUri;

            var redirectUriString = GetQueryString(Request, "redirect_uri");

            if (string.IsNullOrWhiteSpace(redirectUriString))
            {
                return "redirect_uri is required";
            }

            bool validUri = Uri.TryCreate(redirectUriString, UriKind.Absolute, out redirectUri);

            if (!validUri)
            {
                return "redirect_uri is invalid";
            }

            var clientId = GetQueryString(Request, "client_id");

            if (string.IsNullOrWhiteSpace(clientId))
            {
                return "client_Id is required";
            }
            redirectUriOutput = redirectUri.AbsoluteUri;

            return string.Empty;

        }
        private static string GetQueryString(HttpRequestMessage request, string key)
        {
            var queryStrings = request.GetQueryNameValuePairs();

            if (queryStrings == null) return null;

            var match = queryStrings.FirstOrDefault(keyValue => System.String.Compare(keyValue.Key, key, System.StringComparison.OrdinalIgnoreCase) == 0);

            return string.IsNullOrEmpty(match.Value) ? null : match.Value;
        }

        private async Task<ParsedExternalAccessToken> VerifyExternalAccessToken(string provider, string accessToken, string accessTokenSecret)
        {
            ParsedExternalAccessToken parsedToken = null;

            string verifyTokenEndPoint = "", authHeader = "";
            verifyTokenEndPoint = PrepareVerificationLink(provider, accessToken, accessTokenSecret, out authHeader);

            var client = new HttpClient();
            var uri = new Uri(verifyTokenEndPoint);

            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dynamic jObj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                parsedToken = new ParsedExternalAccessToken();

                if (provider == "Facebook")
                {
                    parsedToken.UserId = jObj["data"]["user_id"];
                    parsedToken.AppId = jObj["data"]["app_id"];
                    parsedToken.RetrievedName = jObj["data"]["firstname"];

                    if (!string.Equals(Startup.OAuthFacebookAppId, parsedToken.AppId, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
                else if (provider == "Twitter")
                {
                    parsedToken.UserId = jObj["id"];
                    parsedToken.AppId = ConfigurationManager.AppSettings.Get("Twitter_OwnerId");
                    parsedToken.RetrievedUserName = jObj["screen_name"];
                    parsedToken.RetrievedName = jObj["name"];

                }
                else if (provider == "Google")
                {
                    parsedToken.UserId = jObj["user_id"];
                    parsedToken.AppId = jObj["audience"];

                    if (!string.Equals(Startup.OAuthGoogleClientId, parsedToken.AppId, StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }

                }

            }

            return parsedToken;









        }

        private static string PrepareVerificationLink(string provider, string accessToken, string accessTokenSecret, out string authHeader)
        {
            string baseFormat = "", url = "", verifyTokenEndPoint = ""; authHeader = "";
            switch (provider)
            {
                case "Twitter":
                    // oauth implementation details
                    const string oauthVersion = "1.0";
                    const string oauthSignatureMethod = "HMAC-SHA1";

                    // configuration parameters
                    url = ConfigurationManager.AppSettings.Get("Twitter_VerificationUrl");
                    var oauthToken = accessToken; //ConfigurationManager.AppSettings.Get("Twitter_App_AccessToken");
                    var oauthTokenSecret = accessTokenSecret; //ConfigurationManager.AppSettings.Get("Twitter_App_AccessTokenSecret");
                    var oauthConsumerKey = ConfigurationManager.AppSettings.Get("Twitter_ConsumerKey");
                    var oauthConsumerSecret = ConfigurationManager.AppSettings.Get("Twitter_ConsumerSecret");

                    // unique request details
                    var oauthNonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture)));
                    var timeSpan = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                    var oauthTimestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);

                    var basestringParameters = new SortedDictionary<string, string>();
                    basestringParameters.Add("oauth_version", oauthVersion);
                    basestringParameters.Add("oauth_consumer_key", oauthConsumerKey);
                    basestringParameters.Add("oauth_nonce", oauthNonce);
                    basestringParameters.Add("oauth_signature_method", oauthSignatureMethod);
                    basestringParameters.Add("oauth_timestamp", oauthTimestamp);
                    basestringParameters.Add("oauth_token", oauthToken);

                    //build the signature string
                    var baseString = new StringBuilder();
                    baseString.Append("GET" + "&");
                    baseString.Append(EncodeCharacters(Uri.EscapeDataString(url) + "&"));
                    foreach (KeyValuePair<string, string> entry in basestringParameters)
                    {
                        baseString.Append(EncodeCharacters(Uri.EscapeDataString(entry.Key + "=" + entry.Value + "&")));
                    }

                    //baseString is urlEncoded , remove the last 3 chars - %26
                    string finalBaseString = baseString.ToString().Substring(0, baseString.Length - 3);

                    //build the authentication header signing key
                    string compositeKey = EncodeCharacters(Uri.EscapeDataString(oauthConsumerSecret)) + "&" +
                    EncodeCharacters(Uri.EscapeDataString(oauthTokenSecret));

                    //Sign the request
                    var hasher = new HMACSHA1(new ASCIIEncoding().GetBytes(compositeKey));
                    string oauthSignature = Convert.ToBase64String(hasher.ComputeHash(new ASCIIEncoding().GetBytes(finalBaseString)));


                    // create the request header
                    var authorizationHeaderParams = new StringBuilder();
                    authorizationHeaderParams.Append("OAuth ");
                    authorizationHeaderParams.Append("oauth_nonce=" + "\"" + Uri.EscapeDataString(oauthNonce) + "\",");
                    authorizationHeaderParams.Append("oauth_signature_method=" + "\"" + Uri.EscapeDataString(oauthSignatureMethod) + "\",");
                    authorizationHeaderParams.Append("oauth_timestamp=" + "\"" + Uri.EscapeDataString(oauthTimestamp) + "\",");
                    authorizationHeaderParams.Append("oauth_consumer_key=" + "\"" + Uri.EscapeDataString(oauthConsumerKey) + "\",");
                    authorizationHeaderParams.Append("oauth_token=" + "\"" + Uri.EscapeDataString(oauthToken) + "\",");
                    authorizationHeaderParams.Append("oauth_signature=" + "\"" + Uri.EscapeDataString(oauthSignature) + "\",");
                    authorizationHeaderParams.Append("oauth_version=" + "\"" + Uri.EscapeDataString(oauthVersion) + "\"");

                    authHeader = authorizationHeaderParams.ToString();



                    verifyTokenEndPoint = url;
                    return verifyTokenEndPoint;
                case "Facebook":

                    // configuration parameters
                    url = ConfigurationManager.AppSettings.Get("Facebook_VerificationUrl");
                    var appToken = ConfigurationManager.AppSettings.Get("Facebook_appToken");
                    baseFormat = "{0}?input_token={1}&access_token={2}";

                    verifyTokenEndPoint = string.Format(baseFormat, url, accessToken, appToken);
                    return verifyTokenEndPoint;
                case "Google":
                    // configuration parameters
                    url = ConfigurationManager.AppSettings.Get("Google_VerificationUrl");
                    baseFormat = "{0}?access_token={1}";

                    verifyTokenEndPoint = string.Format(baseFormat, url, accessToken);
                    return verifyTokenEndPoint;
                default:
                    return "";
            }
        }
        private async Task<JObject> GenerateLocalAccessTokenResponse(string provider, string userName, string email, ApplicationUser user)
        {

            var tokenExpiration = TimeSpan.FromDays(365);
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            ClaimsIdentity oAuthIdentity = await user.GenerateUserIdentityAsync(UserManager, OAuthDefaults.AuthenticationType);
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, userName));
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

            oAuthIdentity.AddClaim(new Claim("role", "Users"));
            IDictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "userName", userName }
                    };
            var props = new AuthenticationProperties(data)
            {
                IssuedUtc = DateTime.UtcNow,
                ExpiresUtc = DateTime.UtcNow.Add(tokenExpiration),
            };

            var ticket = new AuthenticationTicket(oAuthIdentity, props);
            var accessToken = Startup.OAuthOptions.AccessTokenFormat.Protect(ticket);
            var tokenResponse = new JObject(
                                        new JProperty("userName", userName),
                                        new JProperty("access_token", (string)accessToken),
                                        new JProperty("token_type", "bearer"),
                                        new JProperty("expires_in", tokenExpiration.TotalSeconds.ToString(CultureInfo.InvariantCulture)),
                                        new JProperty(".issued", FormatTokenDate(ticket.Properties.IssuedUtc)),
                                        new JProperty(".expires", FormatTokenDate(ticket.Properties.ExpiresUtc)));
            return tokenResponse;
        }

        private static string FormatTokenDate(DateTimeOffset? date)
        {
            return date.HasValue ? date.Value.ToUniversalTime().ToString("r") : "";
        }
        private static string EncodeCharacters(string data)
        {
            //as per OAuth Core 1.0 Characters in the unreserved character set MUST NOT be encoded
            //unreserved = ALPHA, DIGIT, '-', '.', '_', '~'
            if (data.Contains("!"))
                data = data.Replace("!", "%21");
            if (data.Contains("'"))
                data = data.Replace("'", "%27");
            if (data.Contains("("))
                data = data.Replace("(", "%28");
            if (data.Contains(")"))
                data = data.Replace(")", "%29");
            if (data.Contains("*"))
                data = data.Replace("*", "%2A");
            if (data.Contains(","))
                data = data.Replace(",", "%2C");

            return data;
        }
        #endregion

    }
}
