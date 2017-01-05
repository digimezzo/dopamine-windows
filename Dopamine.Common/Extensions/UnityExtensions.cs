using Microsoft.Practices.Unity;

namespace Dopamine.Common.Extensions
{
    public static class UnityExtensions
    {
        public static void RegisterSingletonType<TFrom, TTo>(this IUnityContainer container) where TTo : TFrom
        {
            container.RegisterType<TFrom, TTo>(new ContainerControlledLifetimeManager());
        }
    }
}
