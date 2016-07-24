using System.ServiceModel;

namespace Dopamine.Common.Services.Command
{
    [ServiceContract(Namespace = "http://Dopamine.CommandService")]
    public interface ICommandService
    {
        [OperationContract()]
        void ShowMainWindowCommand();
    }
}
