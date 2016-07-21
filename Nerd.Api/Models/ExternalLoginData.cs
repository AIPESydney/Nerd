using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
namespace Nerd.Api.Models
{
    public class ExternalLoginData
    {
        public string LoginProvider { get; set; }
        public string ProviderKey { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ExternalAccessToken { get; set; }
        public string ExternalAccessTokenSecret { get; set; }

        public IList<Claim> GetClaims()
        {
            IList<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

            if (UserName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
            }
            if (Email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, Email, null, LoginProvider));
            }

            return claims;
        }

        public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
        {
            if (identity == null)
            {
                return null;
            }

            Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

            if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                || String.IsNullOrEmpty(providerKeyClaim.Value))
            {
                return null;
            }

            if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
            {
                return null;
            }

            return new ExternalLoginData
            {
                LoginProvider = providerKeyClaim.Issuer,
                ProviderKey = providerKeyClaim.Value,
                UserName = identity.FindFirstValue(ClaimTypes.Name),
                Email = identity.FindFirstValue(ClaimTypes.Email),
                ExternalAccessToken = identity.FindFirstValue("ExternalAccessToken"),
                ExternalAccessTokenSecret = identity.FindFirstValue("ExternalAccessTokenSecret")
            };
        }
    }
}