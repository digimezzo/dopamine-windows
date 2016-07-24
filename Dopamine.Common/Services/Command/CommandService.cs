using Dopamine.Core.Prism;

namespace Dopamine.Common.Services.Command
{
    public class CommandService : ICommandService
    {
        public void ShowMainWindowCommand()
        {
            ApplicationCommands.ShowMainWindowCommand.Execute(null);
        }
    }
}
