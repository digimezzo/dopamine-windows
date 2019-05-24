using Digimezzo.Foundation.WPF.Controls;
using System.Threading.Tasks;

namespace Dopamine.Views
{
    public partial class Initialize : Windows10BorderlessWindow
    {
        public Initialize()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.MigrateAsync();
        }

        private async void MigrateAsync()
        {
            var initializer = new Initializer();

            // Migrate
            await initializer.MigrateAsync();

            // Small delay
            await Task.Delay(1000);

            this.Close();
        }
    }
}
