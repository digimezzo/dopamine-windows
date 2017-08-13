using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Description;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Extensions;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.Common.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        private static string baseAddress = $"net.pipe://localhost/{ProductInformationBase.ApplicationDisplayName}";

        #region Variables
        private ServiceHost svcHost;
        private ExternalControlServer svcInstance;
        private readonly IUnityContainer container;
        private readonly IPlaybackService playbackService;
        private readonly ICacheService cacheService;
        #endregion

        #region Construction
        public ExternalControlService(IUnityContainer container)
        {
            this.container = container;
            this.playbackService = this.container.Resolve<IPlaybackService>();
            this.cacheService = this.container.Resolve<ICacheService>();

            if(SettingsClient.Get<bool>("Playback", "EnableExternalControl"))
                Start();
        }
        #endregion

        #region IExternalControlService

        public void Start()
        {
            if (this.svcInstance == null)
            {
                this.svcInstance = new ExternalControlServer(this.playbackService, this.cacheService);
            }
            this.svcInstance.Open();

            svcHost = new ServiceHost(svcInstance);
            svcHost.AddServiceEndpoint(typeof(IExternalControlServer), new NetNamedPipeBinding()
            {
                SendTimeout = new TimeSpan(0, 0, 2),
            }, new Uri($"{baseAddress}/ExternalControlService"));

            var smb = svcHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            svcHost.Description.Behaviors.Add(smb);
            svcHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), new Uri($"{baseAddress}/ExternalControlService/mex"));

            svcHost.Open();
        }

        public void Stop()
        {
            this.svcHost.Close();
            this.svcInstance.Close();
        }

        #endregion

        #region Private



        #endregion
    }
}