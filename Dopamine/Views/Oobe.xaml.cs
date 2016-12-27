using Digimezzo.Utilities.Settings;
using Dopamine.Common.Controls;
using Dopamine.Common.Services.Appearance;
using Dopamine.Common.Services.Indexing;
using Dopamine.Core.Prism;
using Dopamine.OobeModule.Views;
using Prism.Events;
using Prism.Regions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Dopamine.Views
{
    public partial class Oobe : DopamineWindow
    {
        #region Variables
        private IEventAggregator eventAggregator;
        private IAppearanceService appearanceService;
        private Storyboard backgroundAnimation;
        private IIndexingService indexingService;
        private IRegionManager regionManager;
        #endregion

        #region Construction
        public Oobe(IEventAggregator eventAggregator, IAppearanceService appearanceService, IIndexingService indexingService, IRegionManager regionManager)
        {
            InitializeComponent();

            this.eventAggregator = eventAggregator;
            this.appearanceService = appearanceService;
            this.indexingService = indexingService;
            this.regionManager = regionManager;

            this.eventAggregator.GetEvent<CloseOobeEvent>().Subscribe((_) => { this.Close(); });

            this.appearanceService.ThemeChanged += this.ThemeChangedHandler;
        }
        #endregion

        #region Event handlers
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

            // We're closeing the OOBE screen, tell the IndexingService to start.
            this.indexingService.IndexCollectionAsync(SettingsClient.Get<bool>("Indexing", "IgnoreRemovedFiles"), false);
        }

        private void ThemeChangedHandler(object sender, EventArgs e)
        {
            if (this.backgroundAnimation != null)
            {
                this.backgroundAnimation.Begin();
            }
        }

        private void BorderlessWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.ShowWelcome();
        }
        #endregion

        #region Private

        private async void ShowWelcome()
        {
            // Make sure that the regions are initialized
            while (this.regionManager.Regions[RegionNames.OobeAppNameRegion] == null)
            {
                await Task.Delay(100);
            }

            await Task.Delay(500);
            this.regionManager.RequestNavigate(RegionNames.OobeAppNameRegion, typeof(OobeAppName).FullName);
            await Task.Delay(500);
            this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeWelcome).FullName);
            await Task.Delay(500);
            this.regionManager.RequestNavigate(RegionNames.OobeControlsRegion, typeof(OobeControls).FullName);
        }
        #endregion
    }
}
