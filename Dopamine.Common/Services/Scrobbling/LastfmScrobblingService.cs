using System;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Settings;
using System.Threading.Tasks;
using Dopamine.Core.Api.Lastfm;
using Dopamine.Core.Logging;
using Dopamine.Core.Base;
using Dopamine.Core.Database;
using Dopamine.Core.Database.Repositories.Interfaces;

namespace Dopamine.Common.Services.Scrobbling
{
    public class LastFmScrobblingService : IScrobblingService
    {
        #region Private
        private SignInState signInState;
        private string username;
        private string password;
        private string sessionKey;
        private IPlaybackService playbackService;
        private DateTime trackStartTime;
        private bool canScrobble;
        private ITrackRepository trackRepository;
        #endregion

        #region Events
        public event Action<SignInState> SignInStateChanged = delegate { };
        #endregion

        #region Properties
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
        #endregion

        #region Construction
        public LastFmScrobblingService(IPlaybackService playbackService, ITrackRepository trackRepository)
        {
            this.playbackService = playbackService;
            this.trackRepository = trackRepository;

            this.playbackService.PlaybackSuccess += PlaybackService_PlaybackSuccess;
            this.playbackService.PlaybackProgressChanged += PlaybackService_PlaybackProgressChanged;

            this.username = XmlSettingsClient.Instance.Get<string>("Lastfm", "Username");
            this.password = XmlSettingsClient.Instance.Get<string>("Lastfm", "Password");
            this.sessionKey = XmlSettingsClient.Instance.Get<string>("Lastfm", "Key");

            if (!string.IsNullOrEmpty(this.username) && !string.IsNullOrEmpty(this.password) && !string.IsNullOrEmpty(this.sessionKey))
            {
                this.signInState = SignInState.SignedIn;
            }
            else
            {
                this.signInState = SignInState.SignedOut;
            }
        }
        #endregion

        #region Private
        private async void PlaybackService_PlaybackSuccess(bool isPlayingPreviousTrack)
        {
            if (this.SignInState == SignInState.SignedIn)
            {
                // As soon as a track starts playing, send a Now Playing request.
                this.trackStartTime = DateTime.Now;
                this.canScrobble = true;

                MergedTrack mergedTrack = await trackRepository.GetMergedTrackAsync(this.playbackService.PlayingPath);

                if(mergedTrack == null)
                {
                    LogClient.Instance.Logger.Error("Track not found in the database for path: {0}", this.playbackService.PlayingPath);
                    return;
                }

                string artist = mergedTrack.ArtistName != Defaults.UnknownArtistString ? mergedTrack.ArtistName : string.Empty;
                string trackTitle = mergedTrack.TrackTitle;
                string albumTitle = mergedTrack.AlbumTitle != Defaults.UnknownAlbumString ? mergedTrack.AlbumTitle : string.Empty;

                if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(trackTitle))
                {
                    try
                    {
                        bool isSuccess = await LastfmApi.TrackUpdateNowPlaying(this.sessionKey, artist, trackTitle, albumTitle);

                        if (isSuccess)
                        {
                            LogClient.Instance.Logger.Info("Successfully updated Now Playing for track '{0} - {1}'", artist, trackTitle);
                        }
                        else
                        {
                            LogClient.Instance.Logger.Error("Could not update Now Playing for track '{0} - {1}'", artist, trackTitle);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not update Now Playing for track '{0} - {1}'. Exception: {2}", artist, trackTitle, ex.Message);
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
                MergedTrack mergedTrack = await trackRepository.GetMergedTrackAsync(this.playbackService.PlayingPath);

                if (mergedTrack == null)
                {
                    LogClient.Instance.Logger.Error("Track not found in the database for path: {0}", this.playbackService.PlayingPath);
                    return;
                }

                string artist = mergedTrack.ArtistName != Defaults.UnknownArtistString ? mergedTrack.ArtistName : string.Empty;
                string trackTitle = mergedTrack.TrackTitle;
                string albumTitle = mergedTrack.AlbumTitle != Defaults.UnknownAlbumString ? mergedTrack.AlbumTitle : string.Empty;

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
                                    LogClient.Instance.Logger.Info("Successfully Scrobbled track '{0} - {1}'", artist, trackTitle);
                                }
                                else
                                {
                                    LogClient.Instance.Logger.Error("Could not Scrobble track '{0} - {1}'", artist, trackTitle);
                                }
                            }
                            catch (Exception ex)
                            {
                                LogClient.Instance.Logger.Error("Could not Scrobble track '{0} - {1}'. Exception: {2}", artist, trackTitle, ex.Message);
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Public
        public async Task SignIn()
        {
            try
            {
                this.sessionKey = await LastfmApi.GetMobileSession(this.username, this.password);

                if (!string.IsNullOrEmpty(this.sessionKey))
                {
                    XmlSettingsClient.Instance.Set<string>("Lastfm", "Username", this.username);
                    XmlSettingsClient.Instance.Set<string>("Lastfm", "Password", this.password);
                    XmlSettingsClient.Instance.Set<string>("Lastfm", "Key", this.sessionKey);
                    LogClient.Instance.Logger.Info("User '{0}' successfully signed in to Last.fm.", this.username);
                    this.SignInState = SignInState.SignedIn;
                }
                else
                {
                    LogClient.Instance.Logger.Error("User '{0}' could not sign in to Last.fm.", this.username);
                    this.SignInState = SignInState.Error;
                }
            }
            catch (Exception ex)
            {
                LogClient.Instance.Logger.Error("User '{0}' could not sign in to Last.fm. Exception: {1}", this.username, ex.Message);
                this.SignInState = SignInState.Error;
            }

            this.SignInStateChanged(this.SignInState);
        }

        public void SignOut()
        {
            this.sessionKey = string.Empty;
            XmlSettingsClient.Instance.Set<string>("Lastfm", "Key", string.Empty);

            LogClient.Instance.Logger.Info("User '{0}' signed out of Last.fm.", this.username);
            this.SignInState = SignInState.SignedOut;

            this.SignInStateChanged(this.SignInState);
        }

        public async Task<bool> SendTrackLoveAsync(MergedTrack mergedTrack, bool love)
        {
            bool isSuccess = false;

            // We can't send track love for an unknown track
            if (mergedTrack.ArtistName == Defaults.UnknownArtistString | string.IsNullOrEmpty(mergedTrack.TrackTitle)) return false;

            if (this.SignInState == SignInState.SignedIn)
            {
                if (love)
                {
                    try
                    {
                        isSuccess = await LastfmApi.TrackLove(this.sessionKey, mergedTrack.ArtistName, mergedTrack.TrackTitle);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not send track.love to Last.fm. Exception: {0}", ex.Message);
                    }
                    
                }else
                {
                    try
                    {
                        isSuccess = await LastfmApi.TrackUnlove(this.sessionKey, mergedTrack.ArtistName, mergedTrack.TrackTitle);
                    }
                    catch (Exception ex)
                    {
                        LogClient.Instance.Logger.Error("Could not send track.unlove to Last.fm. Exception: {0}", ex.Message);
                    }
                }
            }

            return isSuccess;
        }
        #endregion
    }
}
