using Unity;
using Unity.Lifetime;

namespace Dopamine.Core.Extensions
{
    public static class PrismExtensions
    {
        public static void RegisterSingletonType<TFrom, TTo>(this IUnityContainer container) where TTo : TFrom
        {
            container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
        }
    }
}
