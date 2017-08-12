using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using Digimezzo.Utilities.Settings;
using Dopamine.Common.Base;
using Dopamine.Common.Extensions;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;

namespace Dopamine.Common.Services.ExternalControl
{
    public class ExternalControlService : IExternalControlService
    {
        #region Variables
        private ServiceHost host;
        private ExternalControlServer serverInstance;
        private readonly IPlaybackService playbackService;
        #endregion

        #region Construction
        public ExternalControlService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            if(SettingsClient.Get<bool>("Playback", "EnableExternalControl"))
                Start();
        }
        #endregion

        #region IExternalControlService

        public void Start()
        {
            if (this.serverInstance == null)
            {
                this.serverInstance = new ExternalControlServer(this.playbackService);
            }
            this.serverInstance.Open();

            host = new ServiceHost(serverInstance, new Uri($"net.pipe://localhost/{ProductInformationBase.ApplicationDisplayName}"));
            host.AddServiceEndpoint(typeof(IExternalControlServer), new NetNamedPipeBinding(), "ExternalControlService");
            
            host.Open();
        }

        public void Stop()
        {
            this.host.Close();
            this.serverInstance.Close();
        }

        #endregion

        #region Private



        #endregion
    }
}