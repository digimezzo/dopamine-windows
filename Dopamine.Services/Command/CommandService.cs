using Dopamine.Core.Prism;
using Dopamine.Services.Contracts.Command;
using System.ServiceModel;

namespace Dopamine.Services.Command
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class CommandService : ICommandService
    {
        public void ShowMainWindowCommand()
        {
            ApplicationCommands.ShowMainWindowCommand.Execute(null);
        }
    }
}
