using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Services.Contracts.ExternalControl
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