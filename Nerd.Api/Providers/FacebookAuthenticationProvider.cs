using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Facebook;

namespace Nerd.Api.Providers
{
    public class FacebookAuthenticationProvider : Microsoft.Owin.Security.Facebook.FacebookAuthenticationProvider
    {
        public override Task Authenticated(FacebookAuthenticatedContext context)
        {
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));

            if (!string.IsNullOrEmpty(context.Email))
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.Email, context.Email));
            }

            if (!string.IsNullOrEmpty(context.Name))
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.Name, context.Name));
            }



            return Task.FromResult<object>(null);
        }
    }
}