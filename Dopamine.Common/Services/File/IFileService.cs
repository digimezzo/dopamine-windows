using Dopamine.Common.Database;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.File
{
    [ServiceContract(Namespace = "http://Dopamine.FileService")]
    public interface IFileService
    {
        [OperationContract()]
        void ProcessArguments(string[] iArgs);
    }
}
