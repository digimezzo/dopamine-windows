using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Services.Contracts.Appearance;
using Dopamine.Services.Contracts.Indexing;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Oobe : BorderlessWindows10Window
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

            backgroundAnimation?.Begin();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Prevent the OOBE window from appearing the next time the application is started
            SettingsClient.Set<bool>("General", "ShowOobe", false);

            // We're closing the OOBE window, tell the IndexingService to start checking the collection.
            this.indexingService.RefreshCollectionImmediatelyAsync();
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
