using Digimezzo.Foundation.WPF.Controls;
using Dopamine.Core.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Dopamine.Views.FullPlayer.Settings;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;

namespace Dopamine.ViewModels.FullPlayer.Settings
{
    public class SettingsViewModel : BindableBase
    {
        private int slideInFrom;
        private IRegionManager regionManager;

        public int SlideInFrom
        {
            get { return this.slideInFrom; }
            set { SetProperty<int>(ref this.slideInFrom, value); }
        }

        public SettingsViewModel(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.regionManager = regionManager;

            eventAggregator.GetEvent<IsSettingsPageChanged>().Subscribe(tuple =>
            {
                this.NagivateToPage(tuple.Item1, tuple.Item2);
            });
        }

        private void NagivateToPage(SlideDirection direction, SettingsPage page)
        {
            this.SlideInFrom = direction == SlideDirection.RightToLeft ? Constants.SlideDistance : -Constants.SlideDistance;

            switch (page)
            {
                case SettingsPage.Appearance:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsAppearance).FullName);
                    break;
                case SettingsPage.Behaviour:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsBehaviour).FullName);
                    break;
                case SettingsPage.Online:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsOnline).FullName);
                    break;
                case SettingsPage.Playback:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsPlayback).FullName);
                    break;
                case SettingsPage.Startup:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsStartup).FullName);
                    break;
                case SettingsPage.Blacklist:
                    this.regionManager.RequestNavigate(RegionNames.SettingsRegion, typeof(SettingsBlacklist).FullName);
                    break;
                default:
                    break;
            }
        }
    }
}
