    public class WcfServiceFactory : UnityServiceHostFactory {

        protected override void ConfigureContainer(IUnityContainer container) {
            container.LoadConfiguration();
            //UnityConfig.RegisterTypes(container);
        }
    }