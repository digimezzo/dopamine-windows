using Digimezzo.Utilities.Settings;
using Dopamine.Core.Base;
using Dopamine.Services.Cache;
using Dopamine.Services.Playback;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Dopamine.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        private ServiceHost svcHost;
        private ExternalControlServer svcExternalControlInstance;
        private readonly IPlaybackService playbackService;
        private readonly ICacheService cacheService;
     
        public ExternalControlService(IPlaybackService playbackService, ICacheService cacheService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;

            if(SettingsClient.Get<bool>("Playback", "EnableExternalControl"))
            {
                this.Start();
            }   
        }
       
        public void Start()
        {
            if (this.svcExternalControlInstance == null)
            {
                this.svcExternalControlInstance = new ExternalControlServer(this.playbackService, this.cacheService);
            }
            this.svcExternalControlInstance.Open();

            svcHost = new ServiceHost(svcExternalControlInstance, new Uri($"net.pipe://localhost/{ProductInformation.ApplicationName}"));
            svcHost.AddServiceEndpoint(typeof(IExternalControlServer), new NetNamedPipeBinding()
            {
#if DEBUG
                SendTimeout = new TimeSpan(0, 0, 8),
#else
                SendTimeout = new TimeSpan(0, 0, 1),
#endif
            }, "/ExternalControlService");

//            svcHost.AddServiceEndpoint(typeof(IFftDataServer), new NetNamedPipeBinding()
//            {
//#if DEBUG
//                SendTimeout = new TimeSpan(0, 0, 8),
//#else
//                SendTimeout = new TimeSpan(0, 0, 1),
//#endif
//            }, "/ExternalControlService/FftDataServer");

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
    }
}