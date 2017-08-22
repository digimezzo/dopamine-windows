using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceContract(Namespace = nameof(ExternalControl))]
    public interface IFftDataServer
    {
        [OperationContract]
        int GetFftDataSize();

        [OperationContract]
        Task GetFftData();
    }
}