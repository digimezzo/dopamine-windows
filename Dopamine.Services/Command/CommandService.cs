using Dopamine.Core.Prism;
using Dopamine.Services.Contracts.Command;

namespace Dopamine.Services.Command
{
    public class CommandService : ICommandService
    {
        public void ShowMainWindowCommand()
        {
            ApplicationCommands.ShowMainWindowCommand.Execute(null);
        }
    }
}
