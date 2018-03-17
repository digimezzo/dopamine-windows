using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls.Enums;
using Dopamine.ViewModels.Common.Base;
using Dopamine.Core.Enums;
using Dopamine.Core.Prism;
using Prism.Commands;
using Prism.Events;
using System;
using Prism.Ioc;

namespace Dopamine.ViewModels.Common
{
    public class NowPlayingPlaybackControlsViewModel : PlaybackControlsViewModelBase
    {
        private IEventAggregator eventAggregator;
        private NowPlayingSubPage previousSelectedNowPlayingSubPage;
        private NowPlayingSubPage selectedNowPlayingSubPage;

        public DelegateCommand LoadedCommand { get; set; }

        public NowPlayingSubPage SelectedNowPlayingSubPage
        {
            get { return this.selectedNowPlayingSubPage; }
            set
            {
                SetProperty<NowPlayingSubPage>(ref this.selectedNowPlayingSubPage, value);
                SettingsClient.Set<int>("FullPlayer", "SelectedNowPlayingSubPage", (int)value);
                SlideDirection direction = value <= this.previousSelectedNowPlayingSubPage ? SlideDirection.LeftToRight : SlideDirection.RightToLeft;
                this.eventAggregator.GetEvent<IsNowPlayingSubPageChanged>().Publish(new Tuple<SlideDirection, NowPlayingSubPage>(direction, value));
                this.previousSelectedNowPlayingSubPage = value;
            }
        }

        public bool HasPlaybackQueue
        {
            get { return this.PlaybackService.Queue.Count > 0; }
        }

        public NowPlayingPlaybackControlsViewModel(IContainerProvider container, IEventAggregator eventAggregator) : base(container)
        {
            this.eventAggregator = eventAggregator;

            this.PlaybackService.PlaybackSuccess += (_, __) => RaisePropertyChanged(nameof(this.HasPlaybackQueue));
            this.PlaybackService.PlaybackStopped += (_, __) => this.Reset();

            this.LoadedCommand = new DelegateCommand(() =>
            {
                if (SettingsClient.Get<bool>("Startup", "ShowLastSelectedPage"))
                {
                    this.SelectedNowPlayingSubPage = (NowPlayingSubPage)SettingsClient.Get<int>("FullPlayer", "SelectedNowPlayingSubPage");
                }
                else
                {
                    this.SelectedNowPlayingSubPage = NowPlayingSubPage.ShowCase;
                }
            });
        }
    }
}
