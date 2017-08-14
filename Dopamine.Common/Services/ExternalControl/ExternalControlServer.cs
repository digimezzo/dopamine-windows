using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CSCore.DSP;
using CSCore.Streams;
using Dopamine.Common.Audio;
using Dopamine.Common.Database;
using Dopamine.Common.Extensions;
using Dopamine.Common.Services.Cache;
using Dopamine.Common.Services.Playback;
using Microsoft.Practices.ObjectBuilder2;

namespace Dopamine.Common.Services.ExternalControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class ExternalControlServer : IExternalControlServer, IFftDataServer, IDisposable
    {
        private const int FftDataLength = 256 * 4;

        #region Variables
        private readonly FftProvider fftProvider = new FftProvider(2, FftSize.Fft256);
        private readonly DispatcherTimer fftProviderDataTimer;
        private CSCorePlayer player;
        private readonly float[] fftDataBuffer = new float[FftDataLength / 4];
        private readonly byte[] fftDataBufferBytes = new byte[FftDataLength];
        private readonly MemoryMappedFile fftDataMemoryMappedFile;
        private readonly MemoryMappedViewStream fftDataMemoryMappedFileStream;
        private readonly BinaryWriter fftDataMemoryMappedFileStreamWriter;
        private readonly Mutex fftDataMemoryMappedFileMutex;
        private readonly Dictionary<string, IExternalControlServerCallback> clients = new Dictionary<string, IExternalControlServerCallback>();
        private readonly Stack<string> deadClients = new Stack<string>();

        private readonly IPlaybackService playbackService;
        private readonly ICacheService cacheService;
        private bool haveAddedInputStream;
        #endregion

        #region Constructor
        public ExternalControlServer(IPlaybackService playbackService, ICacheService cacheService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;

            this.fftProviderDataTimer = new DispatcherTimer(){Interval = TimeSpan.FromSeconds(2)};
            this.fftProviderDataTimer.Tick += FftProviderDataTimerElapsed;

            var sec = new MemoryMappedFileSecurity();
            sec.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(WellKnownSidType.SelfSid, null), MemoryMappedFileRights.FullControl, AccessControlType.Allow));
            sec.AddAccessRule(new AccessRule<MemoryMappedFileRights>(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MemoryMappedFileRights.Read, AccessControlType.Allow));
            fftDataMemoryMappedFile = MemoryMappedFile.CreateNew("DopamineFftDataMemory", FftDataLength, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, sec, HandleInheritability.None);
            fftDataMemoryMappedFileStream = fftDataMemoryMappedFile.CreateViewStream(0, FftDataLength, MemoryMappedFileAccess.ReadWrite);
            fftDataMemoryMappedFileStreamWriter = new BinaryWriter(fftDataMemoryMappedFileStream);
            fftDataMemoryMappedFileMutex = new Mutex(true, "DopamineFftDataMemoryMutex");
            fftDataMemoryMappedFileMutex.ReleaseMutex();
        }
        #endregion

        #region IDisposable
        private bool m_disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (m_disposed)
            {
                if (!disposing)
                {
                    fftDataMemoryMappedFileMutex.Dispose();
                    fftDataMemoryMappedFileStreamWriter.Dispose();
                    fftDataMemoryMappedFileStream.Dispose();
                    fftDataMemoryMappedFile.Dispose();
                }
            }
        }

        ~ExternalControlServer()
        {
            Dispose(false);
        }
        #endregion

        #region IExternalControlServer

        [OperationBehavior(ReleaseInstanceMode = ReleaseInstanceMode.None)]
        public string RegisterClient()
        {
            var context = OperationContext.Current;
            var sessionId = context.SessionId;
            try
            {
                var callback = context.GetCallbackChannel<IExternalControlServerCallback>();

                clients.TryRemove(context.SessionId);
                clients.Add(sessionId, callback);

                return sessionId;
            }
            catch (Exception)
            {
                clients.TryRemove(sessionId);
                return string.Empty;
            }
        }

        [OperationBehavior]
        public void DeregisterClient(string sessionId)
        {
            clients.TryRemove(sessionId);
        }

        [OperationBehavior]
        public async Task PlayNextAsync() => await this.playbackService.PlayNextAsync();

        [OperationBehavior]
        public async Task PlayPreviousAsync() => await this.playbackService.PlayPreviousAsync();

        [OperationBehavior]
        public void SetMute(bool mute) => this.playbackService.SetMute(mute);

        [OperationBehavior]
        public Task PlayOrPauseAsync() => this.playbackService.PlayOrPauseAsync();

        [OperationBehavior]
        public bool GetIsStopped() => this.playbackService.IsStopped;

        [OperationBehavior]
        public bool GetIsPlaying() => this.playbackService.IsPlaying;

        [OperationBehavior]
        public double GetProgress() => this.playbackService.Progress;

        [OperationBehavior]
        public void SetProgress(double progress) => this.playbackService.Skip(progress);

        [OperationBehavior]
        public PlayableTrack GetCurrenTrack() => this.playbackService.CurrentTrack.Value;

        [OperationBehavior]
        public string GetCurrentTrackArtworkPath(string artworkId) => this.cacheService.GetCachedArtworkPath(artworkId);

        [OperationBehavior]
        public bool GetFftData()
        {
            this.fftProviderDataTimer.Stop();
            this.fftProviderDataTimer.Start();
            TryAddInputStreamHandler();

            this.fftProvider.GetFftData(fftDataBuffer);

            fftDataMemoryMappedFileMutex.WaitOne();
            Buffer.BlockCopy(fftDataBuffer, 0, fftDataBufferBytes, 0, fftDataBufferBytes.Length);
            fftDataMemoryMappedFileStreamWriter.Seek(0, SeekOrigin.Begin);
            fftDataMemoryMappedFileStreamWriter.Write(fftDataBufferBytes);
            fftDataMemoryMappedFileMutex.ReleaseMutex();

            return true;
        }
        #endregion

        #region Internal
        internal void Open()
        {
            this.playbackService.PlaybackSuccess += PlaybackSuccessCallback;
            this.playbackService.PlaybackStopped += PlaybackStoppedCallback;
            this.playbackService.PlaybackPaused += PlaybackPausedCallBack;
            this.playbackService.PlaybackResumed += PlaybackResumedCallBack;
            this.playbackService.PlaybackProgressChanged += PlaybackProgressChangedCallBack;
            this.playbackService.PlaybackVolumeChanged += PlaybackVolumeChangedCallBack;
            this.playbackService.PlaybackMuteChanged += PlaybackMuteCallBack;
            this.playbackService.PlayingTrackPlaybackInfoChanged += PlayingTrackPlaybackInfoChangedCallback;
            this.playbackService.PlayingTrackArtworkChanged += PlayingTrackArtworkChangedCallBack;
        }

        internal void Close()
        {
            this.playbackService.PlaybackSuccess -= PlaybackSuccessCallback;
            this.playbackService.PlaybackStopped -= PlaybackStoppedCallback;
            this.playbackService.PlaybackPaused -= PlaybackPausedCallBack;
            this.playbackService.PlaybackResumed -= PlaybackResumedCallBack;
            this.playbackService.PlaybackProgressChanged -= PlaybackProgressChangedCallBack;
            this.playbackService.PlaybackVolumeChanged -= PlaybackVolumeChangedCallBack;
            this.playbackService.PlaybackMuteChanged -= PlaybackMuteCallBack;
            this.playbackService.PlayingTrackPlaybackInfoChanged -= PlayingTrackPlaybackInfoChangedCallback;
            this.playbackService.PlayingTrackArtworkChanged -= PlayingTrackArtworkChangedCallBack;
        }
        #endregion

        #region Private
        private async void PlayingTrackArtworkChangedCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlayingTrackArtworkChangedAsync));
        }

        private async void PlayingTrackPlaybackInfoChangedCallback(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlayingTrackPlaybackInfoChangedAsync));
        }

        private async void PlaybackMuteCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackMuteChangedAsync));
        }

        private async void PlaybackVolumeChangedCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackVolumeChangedAsync));
        }

        private async void PlaybackProgressChangedCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackProgressChangedAsync));
        }

        private async void PlaybackResumedCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackResumedAsync));
        }

        private async void PlaybackPausedCallBack(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackPausedAsync));
        }

        private async void PlaybackStoppedCallback(object sender, EventArgs e)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackStoppedAsync));
        }

        private async void PlaybackSuccessCallback(bool _)
        {
            await ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackSuccessAsync));
        }

        private async Task ProxyMethod(string methodName)
        {
            var methodInfo = typeof(IExternalControlServerCallback).GetMethod(methodName);
            foreach (var client in clients)
            {
                try
                {
                    await (Task) methodInfo.Invoke(client.Value, null);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Remove client {client.Key} in ExternalControlServer, reason: {ex.Message}");
                    deadClients.Push(client.Key);
                }
            }

            deadClients.ForEach(c => clients.Remove(c));
            deadClients.Clear();
        }

        private void TryAddInputStreamHandler()
        {
            this.player = playbackService.Player as CSCorePlayer;
            if (this.player != null && !this.haveAddedInputStream)
            {
                this.player.notificationSource.SingleBlockRead += InputStream;
                this.haveAddedInputStream = true;
            }
        }

        private void TryRemoveInputStreamHandler()
        {
            this.player = playbackService.Player as CSCorePlayer;
            if (this.player != null && this.haveAddedInputStream)
            {
                this.player.notificationSource.SingleBlockRead -= InputStream;
                this.haveAddedInputStream = false;
            }
        }

        private void InputStream(object sender, SingleBlockReadEventArgs e)
        {
            try
            {
                this.fftProvider.Add(e.Left, e.Right);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void FftProviderDataTimerElapsed(object sender, EventArgs e)
        {
            this.fftProviderDataTimer.Stop();
            TryRemoveInputStreamHandler();
        }
        #endregion
    }
}