using System.ServiceModel;

namespace Dopamine.Common.Services.ExternalControl
{
    public interface IExternalControlClient
    {
        
    }

    public interface IExternalControlClientCallback
    {
        [OperationContract]
        void SendHeartBeat();

        [OperationContract]
        void SendPlaybackSuccess();
    }
}