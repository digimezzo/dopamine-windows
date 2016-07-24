using System.ServiceModel;

namespace Dopamine.Common.Services.File
{
    [ServiceContract(Namespace = "http://Dopamine.FileService")]
    public interface IFileService
    {
        [OperationContract()]
        void ProcessArguments(string[] iArgs);
    }
}
