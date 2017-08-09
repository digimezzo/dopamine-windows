using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System;

namespace Dopamine.UWP.Services.Dialog
{
    public class DialogService : IDialogService
    {
        public async Task<bool> ShowConfirmationAsync(int iconCharCode, int iconSize, string title, string content, string primaryButtonText, string secondaryButtonText)
        {
            var dialog = new ConfirmationDialog(iconCharCode, iconSize, title, content, primaryButtonText, secondaryButtonText);
            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }
    }
}
