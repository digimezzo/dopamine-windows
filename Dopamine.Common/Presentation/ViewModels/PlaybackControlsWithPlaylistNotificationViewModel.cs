using Digimezzo.Utilities.Utils;
using Dopamine.Common.Services.Collection;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Services.Playlist;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Timers;

namespace Dopamine.Common.Presentation.ViewModels
{
    public class PlaybackControlsWithPlaylistNotificationViewModel : BindableBase
    {
        #region Private
        private ICollectionService collectionService;
        private IPlaybackService playbackService;
        private IPlaylistService playlistService;
        private string addedTracksToPlaylistText;
        private bool showAddedTracksToPlaylistText;
        private Timer showAddedTracksToPlaylistTextTimer;
        private int showAddedTracksToPlaylistTextTimeout = 2; // seconds
        #endregion

        #region Commands
        public DelegateCommand PlaylistNotificationMouseEnterCommand { get; set; }
        #endregion

        #region Public
        public string AddedTracksToPlaylistText
        {
            get { return this.addedTracksToPlaylistText; }
            set { SetProperty<string>(ref this.addedTracksToPlaylistText, value); }
        }

        public bool ShowAddedTracksToPlaylistText
        {
            get { return this.showAddedTracksToPlaylistText; }
            set
            {
                SetProperty<bool>(ref this.showAddedTracksToPlaylistText, value);

                if (value)
                {
                    this.showAddedTracksToPlaylistTextTimer.Stop();
                    this.showAddedTracksToPlaylistTextTimer.Start();
                }
            }
        }
        #endregion

        #region Construction
        public PlaybackControlsWithPlaylistNotificationViewModel(ICollectionService collectionService, IPlaybackService playbackService,IPlaylistService playlistService)
        {
            this.collectionService = collectionService;
            this.playbackService = playbackService;
            this.playlistService = playlistService;

            this.PlaylistNotificationMouseEnterCommand = new DelegateCommand(() => this.HideText());

            this.playlistService.AddedTracksToPlaylist += (iNumberOfTracks, iPlaylistName) =>
            {
                string text = ResourceUtils.GetStringResource("Language_Added_Track_To_Playlist");

                if (iNumberOfTracks > 1)
                {
                    text = ResourceUtils.GetStringResource("Language_Added_Tracks_To_Playlist");
                }

                this.AddedTracksToPlaylistText = text.Replace("%numberoftracks%", iNumberOfTracks.ToString()).Replace("%playlistname%", iPlaylistName);

                this.ShowAddedTracksToPlaylistText = true;
            };

            this.playbackService.AddedTracksToQueue += iNumberOfTracks =>
            {
                string text = ResourceUtils.GetStringResource("Language_Added_Track_To_Now_Playing");

                if (iNumberOfTracks > 1)
                {
                    text = ResourceUtils.GetStringResource("Language_Added_Tracks_To_Now_Playing");
                }

                this.AddedTracksToPlaylistText = text.Replace("%numberoftracks%", iNumberOfTracks.ToString());

                this.ShowAddedTracksToPlaylistText = true;
            };

            this.showAddedTracksToPlaylistTextTimer = new Timer();
            this.showAddedTracksToPlaylistTextTimer.Interval = TimeSpan.FromSeconds(this.showAddedTracksToPlaylistTextTimeout).TotalMilliseconds;
            this.showAddedTracksToPlaylistTextTimer.Elapsed += ShowAddedTracksToPlaylistTextTimerElapsedHandler;
        }
        #endregion

        #region Private
        private void ShowAddedTracksToPlaylistTextTimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            this.HideText();
        }

        private void HideText()
        {
            this.showAddedTracksToPlaylistTextTimer.Stop();
            this.ShowAddedTracksToPlaylistText = false;
        }
        #endregion
    }
}
