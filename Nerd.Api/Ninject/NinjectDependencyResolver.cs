using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Syntax;

namespace Nerd.Api.Ninject
{
    public class NinjectDependencyResolver : NinjectDependencyScope, IDependencyResolver, System.Web.Mvc.IDependencyResolver
    {
        private readonly IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
            : base(kernel)
        {
            _kernel = kernel;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(_kernel.BeginBlock());
        }
    }

    /// <summary>
    /// Dependency Scope implementation for ninject
    /// </summary>
    public class NinjectDependencyScope : IDependencyScope
    {
        private IResolutionRoot _resolver;

        internal NinjectDependencyScope(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }

        public void Dispose()
        {
            var disposable = _resolver as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            _resolver = null;
        }

        public object GetService(Type serviceType)
        {
            if (_resolver == null)
            {
                throw new ObjectDisposedException("this", "This scope has already been disposed");
            }

            return _resolver.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (_resolver == null)
            {
                throw new ObjectDisposedException("this", "This scope has already been disposed");
            }

            return _resolver.GetAll(serviceType);
        }
    }
    /// <summary>
    /// Filter provider for for the Web API so that we can resolve dependencies with ninject. It wasn't exactly easy to work out how to do this.
    /// http://lozanotek.com/blog/archive/2010/10/12/dependency_injection_for_filters_in_mvc3.aspx
    /// </summary>
    public class NinjectFilterProvider : ActionDescriptorFilterProvider, IFilterProvider
    {
        private readonly IKernel _kernel;

        public NinjectFilterProvider(IKernel kernel)
        {
            this._kernel = kernel;
        }

        // ReSharper disable once CSharpWarnings::CS0108
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, System.Web.Http.Controllers.HttpActionDescriptor actionDescriptor)
        {
            // get our filters
            var filters = base.GetFilters(configuration, actionDescriptor).ToList();
            filters.ForEach(f => this._kernel.Inject(f.Instance));
            return filters;
        }
    }
}