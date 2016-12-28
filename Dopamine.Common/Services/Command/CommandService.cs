using Dopamine.Common.Prism;

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
