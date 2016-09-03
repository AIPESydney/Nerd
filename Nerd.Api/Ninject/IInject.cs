
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
