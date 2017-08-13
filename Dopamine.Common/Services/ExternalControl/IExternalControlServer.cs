using System.ServiceModel;
using System.Threading.Tasks;
using Dopamine.Common.Database;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceContract(CallbackContract = typeof(IExternalControlServerCallback), Namespace = nameof(ExternalControl))]
    public interface IExternalControlServer
    {
        /// <summary>
        /// Register to Dopamine WCF server.
        /// </summary>
        /// <returns>
        /// SessionId if register operation is successful; otherwise, string.Empty.
        /// </returns>
        [OperationContract]
        string RegisterClient();

        /// <summary>
        /// Request to deregister this specified client.
        /// </summary>
        /// <param name="sessionId">The sessionId of each connection.</param>
        [OperationContract(IsOneWay = true)]
        void DeregisterClient(string sessionId);

        [OperationContract]
        Task PlayNextAsync();

        [OperationContract]
        Task PlayPreviousAsync();

        [OperationContract]
        void SetMute(bool mute);

        [OperationContract]
        Task PlayOrPauseAsync();

        [OperationContract]
        bool GetIsStopped();

        [OperationContract]
        bool GetIsPlaying();

        [OperationContract]
        double GetProgress();

        [OperationContract]
        void SetProgress(double progress);

        [OperationContract]
        PlayableTrack GetCurrenTrack();

        [OperationContract]
        string GetCurrentTrackArtworkPath(string artworkId);

        [OperationContract]
        bool GetFftData();
    }

    [ServiceContract(Namespace = nameof(ExternalControl))]
    public interface IExternalControlServerCallback
    {
        [OperationContract]
        void SendHeartBeat();

        [OperationContract]
        void RaiseEventPlaybackSuccess();

        [OperationContract]
        void RaiseEventPlaybackStopped();

        [OperationContract]
        void RaiseEventPlaybackPaused();

        [OperationContract]
        void RaiseEventPlaybackResumed();

        [OperationContract]
        void RaiseEventPlaybackProgressChanged();

        [OperationContract]
        void RaiseEventPlaybackVolumeChanged();

        [OperationContract]
        void RaiseEventPlaybackMuteChanged();

        [OperationContract]
        void RaiseEventPlayingTrackPlaybackInfoChanged();

        [OperationContract]
        void RaiseEventPlayingTrackArtworkChanged();
    }
}