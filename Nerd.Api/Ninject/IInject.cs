using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nerd.Api.NinjectInterfaces
{
    public interface IInject
    {
    }

    public interface ISingletonScope : IInject
    {
    }

    public interface ITransientScope : IInject
    {
    }

    public interface IThreadScope : IInject
    {

    }

    public interface IRequestScope : IInject
    {
    }
}
