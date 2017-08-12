using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Dopamine.Common.Extensions;
using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ObjectBuilder2;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ExternalControlServer : IExternalControlServer
    {
        #region Variables
        private readonly Dictionary<string, IExternalControlClientCallback> clients;
        private readonly Stack<string> deadClients;
        private readonly IPlaybackService playbackService;
        #endregion

        #region MyRegion
        public ExternalControlServer(IPlaybackService playbackService)
        {
            this.clients = new Dictionary<string, IExternalControlClientCallback>();
            this.deadClients = new Stack<string>();
            this.playbackService = playbackService;
        }
        #endregion

        #region IExternalControlServer

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public bool RegisterClient(string clientName)
        {
            if (string.IsNullOrWhiteSpace(clientName) || clientName == string.Empty)
                return false;

            try
            {
                var callback = OperationContext.Current.GetCallbackChannel<IExternalControlClientCallback>();

                clients.TryRemove(clientName);
                clients.Add(clientName, callback);

                return true;
            }
            catch (Exception)
            {
                clients.TryRemove(clientName);
                return false;
            }
        }

        [OperationBehavior]
        public void DeregisterClient(string clientName) => clients.TryRemove(clientName);

        [OperationBehavior]
        public async Task PlayNextAsync() => await this.playbackService.PlayNextAsync();

        [OperationBehavior]
        public async Task PlayPreviousAsync() => await this.playbackService.PlayPreviousAsync();

        [OperationBehavior]
        public void SetMute(bool mute) => this.playbackService.SetMute(mute);

        [OperationBehavior]
        public Task PlayOrPauseAsync() => this.playbackService.PlayOrPauseAsync();

        [OperationBehavior]
        public bool IsStopped() => this.playbackService.IsStopped;

        [OperationBehavior]
        public bool IsPlaying() => this.playbackService.IsPlaying;

        [OperationBehavior]
        public double GetProgress() => this.playbackService.Progress;

        [OperationBehavior]
        public void SetProgress(double progress) => this.playbackService.Skip(progress);

        #endregion

        #region Internal
        internal void Open()
        {
            this.playbackService.PlaybackSuccess += PlaybackSuccessCallback;
        }

        internal void Close()
        {
            this.playbackService.PlaybackSuccess -= PlaybackSuccessCallback;
        }
        #endregion

        #region Private

        private void VerifyClients()
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Value.SendHeartBeat();
                }
                catch (Exception ex)
                {
                    deadClients.Push(client.Key);
                }
            }

            deadClients.ForEach(c=>clients.Remove(c));
            deadClients.Clear();
        }

        private void PlaybackSuccessCallback(bool _)
        {
            VerifyClients();
            foreach (var client in clients)
            {
                try
                {
                    client.Value.SendPlaybackSuccess();
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }


        #endregion
    }
}