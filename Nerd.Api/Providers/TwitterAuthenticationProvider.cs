using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.Twitter;

namespace Nerd.Api.Providers
{

    public class TwitterAuthenticationProvider : ITwitterAuthenticationProvider
    {
        public void ApplyRedirect(TwitterApplyRedirectContext context)
        {
            context.Response.Redirect(context.RedirectUri);
        }

        public Task Authenticated(TwitterAuthenticatedContext context)
        {
            context.Identity.AddClaim(new Claim("ExternalAccessToken", context.AccessToken));
            context.Identity.AddClaim(new Claim("ExternalAccessTokenSecret", context.AccessTokenSecret));

            return Task.FromResult<object>(null);
        }

        public Task ReturnEndpoint(TwitterReturnEndpointContext context)
        {
            return Task.FromResult<object>(null);
        }
    }
}
