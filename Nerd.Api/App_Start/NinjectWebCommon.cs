using System;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using IFilterProvider = System.Web.Http.Filters.IFilterProvider;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Nerd.Api.Models;
using Nerd.Api.Ninject;
using Nerd.Api.Repository;
using Nerd.Api.NinjectInterfaces;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Nerd.Api.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Nerd.Api.App_Start.NinjectWebCommon), "Stop")]
namespace Nerd.Api.App_Start
{


    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        public static IKernel Kernel { get; private set; }
        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);

        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);

                var resolver = new NinjectDependencyResolver(kernel);
                GlobalConfiguration.Configuration.DependencyResolver = resolver; //register  Ninject for WebApi


                // add our ninject filter provider so we get injection in filters
                var providers = GlobalConfiguration.Configuration.Services.GetFilterProviders().ToList();
                GlobalConfiguration.Configuration.Services.Add(typeof(IFilterProvider), new NinjectFilterProvider(kernel));
                // remove the old filter provider because we don't like it.
                var defaultprovider = providers.First(i => i is ActionDescriptorFilterProvider);
                GlobalConfiguration.Configuration.Services.Remove(typeof(IFilterProvider), defaultprovider);

                return Kernel=kernel;



            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {

            kernel.Bind<UnitOfWork>().To<UnitOfWork>().InRequestScope();
            kernel.Bind<IUserStore<ApplicationUser>>().To<UserStore<ApplicationUser>>().InRequestScope();
            kernel.Bind<ApplicationUserManager>().To<ApplicationUserManager>().InRequestScope();

            kernel.Load(AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.StartsWith("Nerd")));


        }
    }
}
