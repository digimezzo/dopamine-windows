using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Services.ExternalControl
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
        Task PlayNext();

        [OperationContract]
        Task PlayPrevious();

        [OperationContract]
        void SetMute(bool mute);

        [OperationContract]
        Task PlayOrPause();

        [OperationContract]
        bool GetIsStopped();

        [OperationContract]
        bool GetIsPlaying();

        [OperationContract]
        double GetProgress();

        [OperationContract]
        void SetProgress(double progress);

        [OperationContract]
        ExternalTrack GetCurrenTrack();

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
        Task RaiseEventPlayingTrackChangedAsync();
    }
}