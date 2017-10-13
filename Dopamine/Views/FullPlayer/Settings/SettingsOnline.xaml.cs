using Dopamine.Common.Services.Scrobbling;
using Microsoft.Practices.ServiceLocation;
using System.Windows.Controls;

namespace Dopamine.Views.FullPlayer.Settings
{
    public partial class SettingsOnline : UserControl
    {
        private IScrobblingService scrobblingService;

        public SettingsOnline()
        {
            InitializeComponent();

            this.scrobblingService = ServiceLocator.Current.GetInstance<IScrobblingService>();
            this.scrobblingService.SignInStateChanged += (_) => this.PasswordBox.Password = scrobblingService.Password;
            this.PasswordBox.Password = scrobblingService.Password;
        }

        private void PasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            scrobblingService.Password = this.PasswordBox.Password;
        }
    }
}
