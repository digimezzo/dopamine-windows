using Dopamine.Common.Services.Scrobbling;
using System.Windows.Controls;

namespace Dopamine.SettingsModule.Views
{
    public partial class SettingsOnline : UserControl
    {
        #region Private
        private IScrobblingService scrobblingService;
        #endregion

        public SettingsOnline(IScrobblingService scrobblingService)
        {
            InitializeComponent();

            this.scrobblingService = scrobblingService;

            this.scrobblingService.SignInStateChanged += (_) => this.PasswordBox.Password = scrobblingService.Password;

            this.PasswordBox.Password = scrobblingService.Password;
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            scrobblingService.Password = this.PasswordBox.Password;
        }
    }
}
