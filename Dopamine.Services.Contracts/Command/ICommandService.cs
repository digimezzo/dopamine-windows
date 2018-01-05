using System.ServiceModel;

namespace Dopamine.Services.Contracts.Command
{
    [ServiceContract(Namespace = "http://Dopamine.CommandService")]
    public interface ICommandService
    {
        [OperationContract()]
        void ShowMainWindowCommand();
    }
}
