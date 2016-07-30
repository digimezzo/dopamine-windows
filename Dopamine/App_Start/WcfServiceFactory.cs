using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Activation;
using System.Text;
using System.Threading.Tasks;
using Unity.Wcf;

namespace Dopamine {
    public class WcfServiceFactory : UnityServiceHostFactory {

        protected override void ConfigureContainer(IUnityContainer container) {
            container.LoadConfiguration();
            //UnityConfig.RegisterTypes(container);
        }
    }
}
