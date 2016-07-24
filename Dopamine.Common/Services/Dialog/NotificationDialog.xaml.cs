using Dopamine.Common.Controls;
using Dopamine.Common.Presentation.Utils;
using Dopamine.Core.Base;
using Dopamine.Core.IO;
using Dopamine.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Dopamine.Common.Services.Dialog
{
    public partial class NotificationDialog : DopamineWindow
    {
        #region Construction
        public NotificationDialog(int iconCharCode, int iconSize, string title, string content, string okText, bool showViewLogs, string viewLogsText) : base()
        {
            InitializeComponent();

            this.TitleBarHeight = Constants.DefaultWindowButtonHeight + 10;
            this.Icon.Text = char.ConvertFromUtf32(iconCharCode);
            this.Icon.FontSize = iconSize;
            this.Title = title;
            //Me.TextBlockTitle.Text = iTitle.ToUpper
            this.TextBlockTitle.Text = title;
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
                Actions.TryViewInExplorer(LogClient.Instance.LogFile);
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("Could not view the log file {0} in explorer. Exception: {1}", LogClient.Instance.LogFile, ex.Message);
            }
        }
        #endregion
    }
}
