using System.ServiceModel;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceContract(Namespace = nameof(ExternalControl))]
    public interface IFftDataServer
    {
        [OperationContract]
        bool GetFftData();
    }
}