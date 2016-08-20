using Dopamine.Common.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Services.Dialog
{
    public class DialogService : IDialogService
    {
        #region Variables
        private List<DopamineWindow> openDialogs;
        #endregion


        #region Construction
        public DialogService()
        {
            this.openDialogs = new List<DopamineWindow>();
        }
        #endregion

        #region Private
        private void ShowDialog(DopamineWindow win)
        {
            foreach (DopamineWindow dlg in this.openDialogs)
            {
                dlg.IsOverlayVisible = true;
            }

            this.openDialogs.Add(win);
            this.DialogVisibleChanged(this.openDialogs.Count > 0);

            win.ShowDialog();
            this.openDialogs.Remove(win);
            this.DialogVisibleChanged(this.openDialogs.Count > 0);

            foreach (DopamineWindow dlg in this.openDialogs)
            {
                dlg.IsOverlayVisible = false;
            }
        }
        #endregion

        #region Public
        public bool ShowConfirmation(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText)
        {
            bool returnValue = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                ConfirmationDialog dialog = new ConfirmationDialog(iconCharCode: iconCharCode, iconSize: iconSize, title: title, content: content, okText: okText, cancelText: cancelText);
                this.ShowDialog(dialog);

                if (dialog.DialogResult.HasValue & dialog.DialogResult.Value)
                {
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            });

            return returnValue;
        }

        public bool ShowNotification(int iconCharCode, int iconSize, string title, string content, string okText, bool showViewLogs, string viewLogsText = "Log file")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotificationDialog dialog = new NotificationDialog(iconCharCode: iconCharCode, iconSize: iconSize, title: title, content: content, okText: okText, showViewLogs: showViewLogs, viewLogsText: viewLogsText);
                this.ShowDialog(dialog);
            });

            // Always return True when a Notification is shown
            return true;
        }

        public bool ShowCustomDialog(int iconCharCode, int iconSize, string title, UserControl content, int width, int height, bool canResize, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback)
        {
            bool returnValue = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomDialog dialog = new CustomDialog(iconCharCode: iconCharCode, iconSize: iconSize, title: title, content: content, width: width, height: height, canResize: canResize, showCancelButton: showCancelButton, okText: okText, cancelText: cancelText, callback: callback);
                this.ShowDialog(dialog);

                if (dialog.DialogResult.HasValue & dialog.DialogResult.Value)
                {
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            });

            return returnValue;
        }

        public bool ShowCustomDialog(UserControl icon, string title, UserControl content, int width, int height, bool canResize, bool showCancelButton, string okText, string cancelText, Func<Task<bool>> callback)
        {
            bool returnValue = false;

            Application.Current.Dispatcher.Invoke(() =>
            {
                CustomDialog dialog = new CustomDialog(icon: icon, title: title, content: content, width: width, height: height, canResize: canResize, showCancelButton: showCancelButton, okText: okText, cancelText: cancelText, callback: callback);
                this.ShowDialog(dialog);

                if (dialog.DialogResult.HasValue & dialog.DialogResult.Value)
                {
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            });

            return returnValue;
        }

        public bool ShowInputDialog(int iconCharCode, int iconSize, string title, string content, string okText, string cancelText, ref string responeText)
        {
            bool returnValue = false;

            string localResponseText = responeText;

            Application.Current.Dispatcher.Invoke(() =>
            {
                InputDialog dialog = new InputDialog(iconCharCode: iconCharCode, iconSize: iconSize, title: title, content: content, okText: okText, cancelText: cancelText, defaultResponse: localResponseText);
                this.ShowDialog(dialog);


                if (dialog.DialogResult.HasValue & dialog.DialogResult.Value)
                {
                    localResponseText = ((InputDialog)dialog).ResponseText;
                    returnValue = true;
                }
                else
                {
                    returnValue = false;
                }
            });

            responeText = localResponseText;

            return returnValue;
        }
        #endregion

        #region Events
        public event Action<bool> DialogVisibleChanged = delegate { };
        #endregion
    }
}
