using Digimezzo.Utilities.Settings;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Api.Lastfm;
using Dopamine.Common.Base;
using Dopamine.Common.Database;
using Digimezzo.Utilities.Log;
using System;
using System.Threading.Tasks;

namespace Dopamine.Common.Services.Scrobbling
{
    public class LastFmScrobblingService : IScrobblingService
    {
        private SignInState signInState;
        private string username;
        private string password;
        private string sessionKey;
        private IPlaybackService playbackService;
        private DateTime trackStartTime;
        private bool canScrobble;
      
        public event Action<SignInState> SignInStateChanged = delegate { };
    
        public SignInState SignInState
        {
            get
            {
                return this.signInState;
            }

            set
            {
                this.signInState = value;
            }
        }

        public string Username
        {
            get
            {
                return this.username;
            }

            set
            {
                this.username = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }

            set
            {
                this.password = value;
            }
        }
    
        public LastFmScrobblingService(IPlaybackService playbackService)
        {
            this.playbackService = playbackService;

            this.playbackService.PlaybackSuccess += PlaybackService_PlaybackSuccess;
            this.playbackService.PlaybackProgressChanged += PlaybackService_PlaybackProgressChanged;
            this.playbackService.PlaybackSkipped += PlaybackService_PlaybackSkipped;

            this.username = SettingsClient.Get<string>("Lastfm", "Username");
            this.password = SettingsClient.Get<string>("Lastfm", "Password");
            this.sessionKey = SettingsClient.Get<string>("Lastfm", "Key");

            if (!string.IsNullOrEmpty(this.username) && !string.IsNullOrEmpty(this.password) && !string.IsNullOrEmpty(this.sessionKey))
            {
                this.signInState = SignInState.SignedIn;
            }
            else
            {
                this.signInState = SignInState.SignedOut;
            }
        }

        private void PlaybackService_PlaybackSkipped(object sender, EventArgs e)
        {
            // When the user skips, we don't allow scrobbling.
            this.canScrobble = false;
        }
      
        private async void PlaybackService_PlaybackSuccess(object sender, PlaybackSuccessEventArgs e)
        {
            if (this.SignInState == SignInState.SignedIn)
            {
                // As soon as a track starts playing, send a Now Playing request.
                this.trackStartTime = DateTime.Now;
                this.canScrobble = true;
                string artist = this.playbackService.CurrentTrack.Value.ArtistName != Defaults.UnknownArtistText ? this.playbackService.CurrentTrack.Value.ArtistName : string.Empty;
                string trackTitle = this.playbackService.CurrentTrack.Value.TrackTitle;
                string albumTitle = this.playbackService.CurrentTrack.Value.AlbumTitle != Defaults.UnknownAlbumText ? this.playbackService.CurrentTrack.Value.AlbumTitle : string.Empty;

                if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(trackTitle))
                {
                    try
                    {
                        bool isSuccess = await LastfmApi.TrackUpdateNowPlaying(this.sessionKey, artist, trackTitle, albumTitle);

                        if (isSuccess)
                        {
                            LogClient.Info("Successfully updated Now Playing for track '{0} - {1}'", artist, trackTitle);
                        }
                        else
                        {
                            LogClient.Error("Could not update Now Playing for track '{0} - {1}'", artist, trackTitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not update Now Playing for track '{0} - {1}'. Exception: {2}", artist, trackTitle, ex.Message);
                    }
                    
                }
            }
        }

        private async void PlaybackService_PlaybackProgressChanged(object sender, EventArgs e)
        {
            if (this.SignInState == SignInState.SignedIn)
            {
                // When is a scrobble a scrobble?
                // - The track must be longer than 30 seconds
                // - And the track has been played for at least half its duration, or for 4 minutes (whichever occurs earlier)
                string artist = this.playbackService.CurrentTrack.Value.ArtistName != Defaults.UnknownArtistText ? this.playbackService.CurrentTrack.Value.ArtistName : string.Empty;
                string trackTitle = this.playbackService.CurrentTrack.Value.TrackTitle;
                string albumTitle = this.playbackService.CurrentTrack.Value.AlbumTitle != Defaults.UnknownAlbumText ? this.playbackService.CurrentTrack.Value.AlbumTitle : string.Empty;

                if (this.canScrobble && !string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(trackTitle))
                {
                    TimeSpan currentTime = this.playbackService.GetCurrentTime;
                    TimeSpan totalTime = this.playbackService.GetTotalTime;

                    if (totalTime.TotalSeconds > 30)
                    {
                        if (currentTime.TotalSeconds >= totalTime.TotalSeconds / 2 | currentTime.TotalMinutes > 4)
                        {
                            this.canScrobble = false;

                            try
                            {
                                bool isSuccess = await LastfmApi.TrackScrobble(this.sessionKey, artist, trackTitle, albumTitle, this.trackStartTime);

                                if (isSuccess)
                                {
                                    LogClient.Info("Successfully Scrobbled track '{0} - {1}'", artist, trackTitle);
                                }
                                else
                                {
                                    LogClient.Error("Could not Scrobble track '{0} - {1}'", artist, trackTitle);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Error("Could not Scrobble track '{0} - {1}'. Exception: {2}", artist, trackTitle, ex.Message);
                            }
                        }
                    }
                }
            }
        }
     
        public async Task SignIn()
        {
            try
            {
                this.sessionKey = await LastfmApi.GetMobileSession(this.username, this.password);

                if (!string.IsNullOrEmpty(this.sessionKey))
                {
                    SettingsClient.Set<string>("Lastfm", "Username", this.username);
                    SettingsClient.Set<string>("Lastfm", "Password", this.password);
                    SettingsClient.Set<string>("Lastfm", "Key", this.sessionKey);
                    LogClient.Info("User '{0}' successfully signed in to Last.fm.", this.username);
                    this.SignInState = SignInState.SignedIn;
                }
                else
                {
                    LogClient.Error("User '{0}' could not sign in to Last.fm.", this.username);
                    this.SignInState = SignInState.Error;
                }
            }
            catch (Exception ex)
            {
                LogClient.Error("User '{0}' could not sign in to Last.fm. Exception: {1}", this.username, ex.Message);
                this.SignInState = SignInState.Error;
            }

            this.SignInStateChanged(this.SignInState);
        }

        public void SignOut()
        {
            this.sessionKey = string.Empty;
            SettingsClient.Set<string>("Lastfm", "Key", string.Empty);

            LogClient.Info("User '{0}' signed out of Last.fm.", this.username);
            this.SignInState = SignInState.SignedOut;

            this.SignInStateChanged(this.SignInState);
        }

        public async Task<bool> SendTrackLoveAsync(PlayableTrack track, bool love)
        {
            bool isSuccess = false;

            // We can't send track love for an unknown track
            if (track.ArtistName == Defaults.UnknownArtistText | string.IsNullOrEmpty(track.TrackTitle)) return false;

            if (this.SignInState == SignInState.SignedIn)
            {
                if (love)
                {
                    try
                    {
                        isSuccess = await LastfmApi.TrackLove(this.sessionKey, track.ArtistName, track.TrackTitle);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not send track.love to Last.fm. Exception: {0}", ex.Message);
                    }
                    
                }else
                {
                    try
                    {
                        isSuccess = await LastfmApi.TrackUnlove(this.sessionKey, track.ArtistName, track.TrackTitle);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Error("Could not send track.unlove to Last.fm. Exception: {0}", ex.Message);
                    }
                }
            }

            return isSuccess;
        }
    }
}
