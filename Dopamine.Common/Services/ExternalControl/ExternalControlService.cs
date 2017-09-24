using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Settings;
using Dopamine.Core.Base;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Dopamine.Common.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        #region Variables
        private ServiceHost svcHost;
        private ExternalControlServer svcExternalControlInstance;
        private IMergedSettings settings;
        private readonly IPlaybackService playbackService;
        private readonly ICacheService cacheService;
        #endregion

        #region Construction
        public ExternalControlService(IPlaybackService playbackService, ICacheService cacheService, IMergedSettings settings)
        {
            this.settings = settings;
            this.playbackService = playbackService;
            this.cacheService = cacheService;

            if(this.settings.EnableExternalControl)
            {
                this.Start();
            }   
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

            svcHost = new ServiceHost(svcExternalControlInstance, new Uri($"net.pipe://localhost/{ProductInformation.ApplicationName}"));
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