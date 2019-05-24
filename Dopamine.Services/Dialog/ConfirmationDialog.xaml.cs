using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.WPF.Controls;
using System.Windows;

namespace Dopamine.Services.Dialog
{
    public partial class ConfirmationDialog : Windows10BorderlessWindow
    {
        public ConfirmationDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText) : base()
{
            InitializeComponent();

            this.DialogIcon.Text = char.ConvertFromUtf32(iconCharCode);
            this.DialogIcon.FontSize = iconSize;
            this.Title = title;
            this.TextBlockTitle.Text = title;
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
