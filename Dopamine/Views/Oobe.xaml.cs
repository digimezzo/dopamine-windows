using Digimezzo.Utilities.Settings;
using Dopamine.Common.Controls;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Indexing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Oobe : DopamineWindow
    {
        private IAppearanceService appearanceService;
        private Storyboard backgroundAnimation;
        private IIndexingService indexingService;

        public Oobe(IAppearanceService appearanceService, IIndexingService indexingService)
        {
            InitializeComponent();

            this.appearanceService = appearanceService;
            this.indexingService = indexingService;

            this.appearanceService.ThemeChanged += this.ThemeChangedHandler;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Retrieve BackgroundAnimation storyboard
            this.backgroundAnimation = this.WindowBorder.Resources["BackgroundAnimation"] as Storyboard;

            if (this.backgroundAnimation != null)
            {
                this.backgroundAnimation.Begin();
            }
        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            // Prevent the Oobe window from appearing the next time the application is started
            SettingsClient.Set<bool>("General", "ShowOobe", false);

            // Closing the Oobe windows, must show the main window
            Application.Current.MainWindow.Show();

            // We're closing the OOBE screen, tell the IndexingService to start checking the collection.
            this.indexingService.QuickCheckCollectionAsync();
        }

        private void ThemeChangedHandler(bool useLightTheme)
        {
            this.backgroundAnimation?.Begin();
        }

        private void ButtonFinish_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
