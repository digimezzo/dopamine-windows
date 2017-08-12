using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceContract(CallbackContract = typeof(IExternalControlClientCallback))]
    public interface IExternalControlServer
    {
        [OperationContract]
        bool RegisterClient(string clientName);

        [OperationContract(IsOneWay = true)]
        void DeregisterClient(string clientName);

        [OperationContract]
        Task PlayNextAsync();

        [OperationContract]
        Task PlayPreviousAsync();

        [OperationContract]
        void SetMute(bool mute);

        [OperationContract]
        Task PlayOrPauseAsync();

        [OperationContract]
        bool IsStopped();

        [OperationContract]
        bool IsPlaying();

        [OperationContract]
        double GetProgress();

        [OperationContract]
        void SetProgress(double progress);
    }


}