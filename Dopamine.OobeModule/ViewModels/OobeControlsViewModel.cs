using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Prism;
using Dopamine.OobeModule.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Events;
using Prism.Regions;
using Microsoft.Practices.Unity;

namespace Dopamine.OobeModule.ViewModels
{
    public class OobeControlsViewModel : BindableBase
    {
        #region Variables
        private readonly IRegionManager regionManager;
        private IUnityContainer container;
        private IEventAggregator eventAggregator;
        private bool showPreviousButton;
        private string activeViewModelName;
        private bool isDone;
        #endregion

        #region Commands
        public DelegateCommand PreviousCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        #endregion

        #region Properties
        public bool ShowPreviousButton
        {
            get { return this.showPreviousButton; }
            set { SetProperty<bool>(ref this.showPreviousButton, value); }
        }

        public bool IsDone
        {
            get { return this.isDone; }
            set { SetProperty<bool>(ref this.isDone, value); }
        }
        #endregion

        #region Construction
        public OobeControlsViewModel(IUnityContainer container, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            this.regionManager = regionManager;
            this.container = container;
            this.eventAggregator = eventAggregator;

            this.IsDone = false;
            this.ShowPreviousButton = false;

            this.PreviousCommand = new DelegateCommand(() =>
            {
                this.eventAggregator.GetEvent<ChangeOobeSlideDirectionEvent>().Publish(SlideDirection.LeftToRight);

                if (this.activeViewModelName == typeof(OobeFinishViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeDonate).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeDonateViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeCollection).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeCollectionViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeAppearance).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeAppearanceViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeLanguage).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeLanguageViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeWelcome).FullName);
                }
            });

            this.NextCommand = new DelegateCommand(() =>
            {
                this.eventAggregator.GetEvent<ChangeOobeSlideDirectionEvent>().Publish(SlideDirection.RightToLeft);

                if (this.activeViewModelName == typeof(OobeLanguageViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeAppearance).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeAppearanceViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeCollection).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeCollectionViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeDonate).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeDonateViewModel).FullName)
                {
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeFinish).FullName);
                }
                else if (this.activeViewModelName == typeof(OobeFinishViewModel).FullName)
                {
                    // Close the OOBE window
                    this.eventAggregator.GetEvent<CloseOobeEvent>().Publish(null);
                }else
                {
                    // OobeWelcomeViewModel is not navigated to when the OOBE window is shown. So this is handled here.
                    this.regionManager.RequestNavigate(RegionNames.OobeContentRegion, typeof(OobeLanguage).FullName);
                }
            });

            this.eventAggregator.GetEvent<OobeNavigatedToEvent>().Subscribe(activeViewModelName =>
            {
                this.activeViewModelName = activeViewModelName;

                if (activeViewModelName == typeof(OobeWelcomeViewModel).FullName | activeViewModelName == typeof(OobeLanguageViewModel).FullName)
                {
                    this.ShowPreviousButton = false;
                }
                else
                {
                    this.ShowPreviousButton = true;
                }

                if (activeViewModelName == typeof(OobeFinishViewModel).FullName)
                {
                    this.IsDone = true;
                }
                else
                {
                    this.IsDone = false;
                }
            });
        }
        #endregion
    }
}
