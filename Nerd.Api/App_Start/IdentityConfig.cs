using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Nerd.Api.Models;
using System.Threading.Tasks;
using Twilio;
using System;
using System.Diagnostics;

namespace Nerd.Api
{
    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity 
    // and is used by the application.
    public class EmailService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            throw new NotImplementedException();
        }
    }

    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            //Twilio Begin
            var twilio = new TwilioRestClient(
              System.Configuration.ConfigurationManager.AppSettings["SMSAccountIdentification"],
              System.Configuration.ConfigurationManager.AppSettings["SMSAccountPassword"]);

            var result = twilio.SendMessage(System.Configuration.ConfigurationManager.AppSettings["SMSAccountFrom"], message.Destination, message.Body);

            //Status is one of Queued, Sending, Sent, Failed or null if the number is not valid
            Trace.TraceInformation(result.Status);
            //Twilio doesn't currently have an async API, so return success.
            return Task.FromResult(0);
            //Twilio End

        }
    }
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {

            var appDbcontext = context.Get<ApplicationDbContext>();

            if (appDbcontext == null)
            {
                appDbcontext = new ApplicationDbContext();
            }
            var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(appDbcontext));
            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<ApplicationUser>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = false,
            };
            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = false,
            };
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }
    }
}
