using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Common.Base;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Services.Dialog
{
    public partial class CustomDialog : DopamineWindow
    {
        #region Variables
        private Func<Task<bool>> callback;
        #endregion

        #region Construction
        public CustomDialog(string title, UserControl content, int width, int height, bool canResize,bool autoSize, bool showTitle, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback) : base()
        {
            InitializeComponent();

            this.TitleBarHeight = Constants.DefaultWindowButtonHeight + 10;
            this.Title = title;
            this.TextBlockTitle.Text = title.ToLower();
            this.Width = width;
            this.MinWidth = width;
            this.CustomContent.Content = content;

            if (canResize)
            {
                this.ResizeMode = ResizeMode.CanResize;
                this.Height = height;
                this.MinHeight = height;
                this.SizeToContent = SizeToContent.Manual;
            }
            else
            {
                this.ResizeMode = ResizeMode.NoResize;

                if (autoSize)
                {
                    this.SizeToContent = SizeToContent.Height;
                }
                else
                {
                    this.Height = height;
                    this.MinHeight = height;
                    this.SizeToContent = SizeToContent.Manual;
                }
            }

            if (showCancelButton)
            {
                this.ButtonCancel.Visibility = Visibility.Visible;
            }
            else
            {
                this.ButtonCancel.Visibility = Visibility.Collapsed;
            }

            if (showTitle)
            {
                this.TitlePanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.TitlePanel.Visibility = Visibility.Collapsed;
            }

            this.ButtonOK.Content = okText;
            this.ButtonCancel.Content = cancelText;

            this.callback = callback;

            WindowUtils.CenterWindow(this);
        }

        public CustomDialog(int iconCharCode, int iconSize, string title, UserControl content, int width, int height, bool canResize, bool autoSize, bool showTitle, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback)
            : this(title, content, width, height, canResize, autoSize,showTitle, showCancelButton, okText, cancelText, callback)
        {
            this.Icon.Text = char.ConvertFromUtf32(iconCharCode);
            this.Icon.FontSize = iconSize;
        }

        public CustomDialog(UserControl icon, string title, UserControl content, int width, int height, bool canResize, bool autoSize, bool showTitle, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback)
            : this(title, content, width, height, canResize, autoSize, showTitle, showCancelButton, okText, cancelText, callback)
        {
            this.IconContentControl.Content = icon.Content;
        }
        #endregion

        #region Event Handlers
        private async void ButtonOK_Click(Object sender, RoutedEventArgs e)
        {
            if (this.callback != null)
            {
                // Prevents clicking the buttons when the callback is already executing, and this prevents this Exception:
                // System.InvalidOperationException: DialogResult can be set only after Window is created and shown as dialog.
                this.ButtonOK.IsEnabled = false;
                this.ButtonOK.IsDefault = false;
                this.ButtonCancel.IsEnabled = false;
                this.ButtonCancel.IsCancel = false;

                // Execute some function in the caller of this dialog.
                // If the result is False, DialogResult is not set.
                // That keeps the dialog open.
                if (await this.callback.Invoke())
                {
                    DialogResult = true;
                }
                else
                {
                    this.ButtonOK.IsEnabled = true;
                    this.ButtonOK.IsDefault = true;
                    this.ButtonCancel.IsEnabled = true;
                    this.ButtonCancel.IsCancel = true;
                }
            }
            else
            {
                DialogResult = true;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
        #endregion
    }
}
