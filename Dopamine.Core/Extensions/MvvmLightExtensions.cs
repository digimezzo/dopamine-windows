using GalaSoft.MvvmLight.Ioc;

namespace Dopamine.Core.Extensions
{
    public static class MvvmLightExtensions
    {
        /// <summary>
        /// Avoids multiple attempts at registering when ViewModelLocator is created by the designer
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TClass"></typeparam>
        public static void RegisterOnce<TInterface, TClass>(this SimpleIoc simpleIoc)
        where TClass : class
            where TInterface : class
        {
            if (!SimpleIoc.Default.IsRegistered<TInterface>())
            {
                SimpleIoc.Default.Register<TInterface, TClass>();
            }
        }
    }
}
