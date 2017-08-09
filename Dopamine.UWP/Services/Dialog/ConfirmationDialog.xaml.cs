using Windows.UI.Xaml.Controls;

namespace Dopamine.UWP.Services.Dialog
{
    public sealed partial class ConfirmationDialog : ContentDialog
    {
        #region Construction
        public ConfirmationDialog(int iconCharCode, int iconSize, string title, string content, string primaryButtonText, string secondaryButtonText)
        {
            this.InitializeComponent();

            this.Icon.Glyph = char.ConvertFromUtf32(iconCharCode);
            this.Icon.FontSize = iconSize;
            this.TextBlockTitle.Text = title;
            this.TextBlockContent.Text = content;
            this.PrimaryButtonText = primaryButtonText;
            this.SecondaryButtonText = secondaryButtonText;
        }
        #endregion
    }
}
