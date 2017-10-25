using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Base;
using System.Windows;

namespace Dopamine.Common.Services.Dialog
{
    public partial class ConfirmationDialog : DopamineWindow
    {
        public ConfirmationDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText) : base()
{
            InitializeComponent();

            this.TitleBarHeight = Constants.DefaultWindowButtonHeight + 10;
            this.Icon.Text = char.ConvertFromUtf32(iconCharCode);
            this.Icon.FontSize = iconSize;
            this.Title = title;
            this.TextBlockTitle.Text = title.ToLower();
            this.TextBlockContent.Text = content;
            this.ButtonOK.Content = okText;
            this.ButtonCancel.Content = cancelText;

            WindowUtils.CenterWindow(this);
        }
  
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
