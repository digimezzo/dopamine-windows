using Digimezzo.Utilities.IO;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Base;
using Digimezzo.Utilities.Log;
using System;
using System.Windows;

namespace Dopamine.Common.Services.Dialog
{
    public partial class NotificationDialog : DopamineWindow
    {
        #region Construction
        public NotificationDialog(int iconCharCode, int iconSize, string title, string content, string okText, bool showViewLogs, string viewLogsText) : base()
        {
            InitializeComponent();

            this.TitleBarHeight = 39;
            this.Icon.Text = char.ConvertFromUtf32(iconCharCode);
            this.Icon.FontSize = iconSize;
            this.Title = title;
            //Me.TextBlockTitle.Text = iTitle.ToUpper
            this.TextBlockTitle.Text = title.ToLower();
            this.TextBlockContent.Text = content;
            this.ButtonOK.Content = okText;
            this.ButtonViewLogs.Content = viewLogsText;

            if (showViewLogs)
            {
                this.ButtonViewLogs.Visibility = Visibility.Visible;
            }
            else
            {
                this.ButtonViewLogs.Visibility = Visibility.Collapsed;
            }

            WindowUtils.CenterWindow(this);
        }
        #endregion

        #region Event handlers
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonViewLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Actions.TryViewInExplorer(LogClient.Logfile());
            }
            catch (Exception ex)
            {
                LogClient.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Logfile(), ex.Message);
            }
        }
        #endregion
    }
}
