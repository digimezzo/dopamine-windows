using Dopamine.Data;
using Dopamine.Data.Entities;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.ExternalControl
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
        /// Request to unregister this specified client.
        /// </summary>
        /// <param name="sessionId">The sessionId of each connection.</param>
        [OperationContract(IsOneWay = true)]
        void DeregisterClient(string sessionId);

        [OperationContract]
        void SendHeartbeat();

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
    }

    [ServiceContract(Namespace = nameof(ExternalControl))]
    public interface IExternalControlServerCallback
    {
        [OperationContract]
        Task RaiseEventPlaybackSuccessAsync();

        [OperationContract]
        Task RaiseEventPlaybackStoppedAsync();

        [OperationContract]
        Task RaiseEventPlaybackPausedAsync();

        [OperationContract]
        Task RaiseEventPlaybackResumedAsync();

        [OperationContract]
        Task RaiseEventPlaybackProgressChangedAsync();

        [OperationContract]
        Task RaiseEventPlaybackVolumeChangedAsync();

        [OperationContract]
        Task RaiseEventPlaybackMuteChangedAsync();

        [OperationContract]
        Task RaiseEventPlayingTrackPlaybackInfoChangedAsync();

        [OperationContract]
        Task RaiseEventPlayingTrackArtworkChangedAsync();
    }
}