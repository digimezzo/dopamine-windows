using System.Threading.Tasks;

namespace Dopamine.UWP.Services.Dialog
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationAsync(int iconCharCode, int iconSize, string title, string content, string primaryButtonText, string secondaryButtonText);
    }
}
