using Dopamine.UWP.Settings;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Services.Dialog
{
    public class DialogService : IDialogService
    {
        #region IDialogService
        public async Task<bool> ShowContentDialogAsync(string title, object content, string primaryButtonText, string secondaryButtonText)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                SecondaryButtonText = secondaryButtonText,
                RequestedTheme = CoreSettings.Current.UseLightTheme ? Windows.UI.Xaml.ElementTheme.Light : Windows.UI.Xaml.ElementTheme.Dark
            };

            var result = await dialog.ShowAsync();

            return result == ContentDialogResult.Primary;
        }
        #endregion
    }
}
