using System.Threading.Tasks;

namespace Dopamine.UWP.Services.Dialog
{
    public interface IDialogService
    {
        Task<bool> ShowContentDialogAsync(string title, object content, string primaryButtonText, string secondaryButtonText);
    }
}
