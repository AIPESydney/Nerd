using System;
using System.Linq;
using Ninject.Modules;
using Ninject.Extensions.Conventions;
using Nerd.Api.NinjectInterfaces;

namespace Nerd.Api.NinjectInterfaces
{
    public class AppModule : NinjectModule
    {
        public override void Load()
        {
            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName.StartsWith("Nerd"))
                .ToArray();
            Kernel.Bind(x => x.From(assemblies)
                .SelectAllClasses()
                .InheritedFrom<ITransientScope>()
                .BindDefaultInterface()
                .Configure(conf => conf.InTransientScope()));
            Kernel.Bind(x => x.From(assemblies)
                .SelectAllClasses()
                .InheritedFrom<ISingletonScope>()
                .BindDefaultInterface()
                .Configure(conf => conf.InSingletonScope()));
            Kernel.Bind(x => x.From(assemblies)
                .SelectAllClasses()
                .InheritedFrom<IThreadScope>()
                .BindDefaultInterface()
                .Configure(conf => conf.InThreadScope()));
        }
    }

}

