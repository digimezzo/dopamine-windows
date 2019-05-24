using Digimezzo.Foundation.Core.Utils;
using Digimezzo.Foundation.WPF.Controls;
using System.Windows;

namespace Dopamine.Services.Dialog
{
    public partial class InputDialog : Windows10BorderlessWindow
    {
        private string responseText;
      
        public string ResponseText
        {
            get { return this.responseText; }
            set { this.responseText = value; }
        }
      
        public InputDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText, string defaultResponse)
            : base()
        {
            InitializeComponent();

            this.DialogIcon.Text = char.ConvertFromUtf32(iconCharCode);
            this.DialogIcon.FontSize = iconSize;
            this.Title = title;
            this.TextBlockTitle.Text = title;
            this.TextBlockContent.Text = content;
            this.ButtonOK.Content = okText;
            this.ButtonCancel.Content = cancelText;
            this.TextBoxResponse.Text = defaultResponse;

            WindowUtils.CenterWindow(this);
        }
    
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.ResponseText = TextBoxResponse.Text;
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set focus to the TextBox and highlight the text. This makes user input easier.
            this.TextBoxResponse.Focus();
            if (!string.IsNullOrEmpty(this.TextBoxResponse.Text)) this.TextBoxResponse.Select(0, this.TextBoxResponse.Text.Length);
        }
    }
}
