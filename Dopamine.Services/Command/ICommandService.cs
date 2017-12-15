using System.ServiceModel;

namespace Dopamine.Services.Command
{
    [ServiceContract(Namespace = "http://Dopamine.CommandService")]
    public interface ICommandService
    {
        [OperationContract()]
        void ShowMainWindowCommand();
    }
}
