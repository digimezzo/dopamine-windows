using Digimezzo.Utilities.Settings;
using Dopamine.Data;
using Dopamine.Services.Entities;
using Prism.Ioc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dopamine.ViewModels.Common.Base
{
    public class TracksViewModelBaseWithTrackArt : TracksViewModelBase
    {
        private bool showTrackArt;

        public bool ShowTrackArt
        {
            get { return this.showTrackArt; }
            set { SetProperty<bool>(ref this.showTrackArt, value); }
        }

        public TracksViewModelBaseWithTrackArt(IContainerProvider container) : base(container)
        {
            // Settings changed
            SettingsClient.SettingChanged += (_, e) =>
            {
                if (SettingsClient.IsSettingChanged(e, "Appearance", "ShowTrackArtOnPlaylists"))
                {
                    this.ShowTrackArt = (bool)e.SettingValue;
                    this.UpdateShowTrackArtAsync();
                }
            };

            // Load settings
            this.ShowTrackArt = SettingsClient.Get<bool>("Appearance", "ShowTrackArtOnPlaylists");
        }

        protected override async Task FillListsAsync()
        {
            // Not implemented here
        }

        protected override async Task GetTracksCommonAsync(IList<TrackViewModel> tracks, TrackOrder trackOrder)
        {
            await base.GetTracksCommonAsync(tracks, TrackOrder);
            this.UpdateShowTrackArtAsync();
        }

        private async void UpdateShowTrackArtAsync()
        {
            if (this.Tracks == null || this.Tracks.Count == 0)
            {
                return;
            }

            await Task.Run(() =>
            {
                foreach (TrackViewModel track in this.Tracks)
                {
                    if (track != null)
                    {
                        track.ShowTrackArt = this.ShowTrackArt;
                    }
                }
            });
        }
    }
}