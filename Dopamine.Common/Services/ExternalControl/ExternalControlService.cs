using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Microsoft.Practices.Unity;

namespace Dopamine.Common.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        #region Variables
        private ServiceHost svcHost;
        private ExternalControlServer svcExternalControlInstance;
        private IFftDataServer svcFftDataInstance;
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
            if (this.svcExternalControlInstance == null)
            {
                this.svcExternalControlInstance = new ExternalControlServer(this.playbackService, this.cacheService);
            }
            this.svcExternalControlInstance.Open();

            svcHost = new ServiceHost(svcExternalControlInstance, new Uri($"net.pipe://localhost/{ProductInformationBase.ApplicationDisplayName}"));
            svcHost.AddServiceEndpoint(typeof(IExternalControlServer), new NetNamedPipeBinding()
            {
#if DEBUG
                SendTimeout = new TimeSpan(0, 0, 8),
#else
                SendTimeout = new TimeSpan(0, 0, 1),
#endif
            }, "/ExternalControlService");

            svcHost.AddServiceEndpoint(typeof(IFftDataServer), new NetNamedPipeBinding()
            {
#if DEBUG
                SendTimeout = new TimeSpan(0, 0, 8),
#else
                SendTimeout = new TimeSpan(0, 0, 1),
#endif
            }, "/ExternalControlService/FftDataServer");

            var smb = svcHost.Description.Behaviors.Find<ServiceMetadataBehavior>() ?? new ServiceMetadataBehavior();
            smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            svcHost.Description.Behaviors.Add(smb);
            svcHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName,
                MetadataExchangeBindings.CreateMexNamedPipeBinding(), "/ExternalControlService/mex");

            svcHost.Open();


        }

        public void Stop()
        {
            this.svcHost.Close();
            this.svcExternalControlInstance.Close();
        }

#endregion

#region Private



#endregion
    }
}