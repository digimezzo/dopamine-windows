using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
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
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ExternalControlServer : IExternalControlServer, IDisposable
    {
        private const int FftDataLength = 256;

        #region Variables
        private readonly FftProvider fftProvider = new FftProvider(2, FftSize.Fft256);
        private CSCorePlayer player;
        private readonly float[] fftDataBuffer = new float[FftDataLength];
        private readonly byte[] fftDataBufferBytes = new byte[FftDataLength * 4];
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

            fftDataMemoryMappedFile = MemoryMappedFile.CreateNew("DopamineFftDataMemory", sizeof(float) * FftDataLength);
            fftDataMemoryMappedFileStream = fftDataMemoryMappedFile.CreateViewStream();
            fftDataMemoryMappedFileStreamWriter = new BinaryWriter(fftDataMemoryMappedFileStream);
            fftDataMemoryMappedFileMutex = new Mutex(true, "DopamineFftDataMemoryMutex");
            fftDataMemoryMappedFileMutex.ReleaseMutex();
        }
        #endregion

        #region IDisposable
        private bool m_disposed;

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

                AddInputStreamHandler();

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

            TryRemoveInputStreamHandler();
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
            this.fftProvider.GetFftData(fftDataBuffer);

            fftDataMemoryMappedFileMutex.WaitOne();
            Buffer.BlockCopy(fftDataBuffer, 0, fftDataBufferBytes, 0, fftDataBufferBytes.Length);
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
        private void PlayingTrackArtworkChangedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlayingTrackArtworkChanged));
        }

        private void PlayingTrackPlaybackInfoChangedCallback(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlayingTrackPlaybackInfoChanged));
        }

        private void PlaybackMuteCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackMuteChanged));
        }

        private void PlaybackVolumeChangedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackVolumeChanged));
        }

        private void PlaybackProgressChangedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackProgressChanged));
        }

        private void PlaybackResumedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackResumed));
        }

        private void PlaybackPausedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackPaused));
        }

        private void PlaybackStoppedCallback(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackStopped));
        }

        private void PlaybackSuccessCallback(bool _)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackSuccess));
        }

        private void ProxyMethod(string methodName)
        {
            VerifyClients();

            var methodInfo = typeof(IExternalControlServerCallback).GetMethod(methodName);
            foreach (var client in clients)
            {
                try
                {
                    methodInfo.Invoke(client.Value, null);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        private void VerifyClients()
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Value.SendHeartBeat();
                }
                catch (Exception ex)
                {
                    deadClients.Push(client.Key);
                }
            }

            deadClients.ForEach(c => clients.Remove(c));
            deadClients.Clear();

            TryRemoveInputStreamHandler();
        }

        private void AddInputStreamHandler()
        {
            if (clients.Count == 1)
            {
                this.player = playbackService.Player as CSCorePlayer;
                if (this.player != null)
                {
                    this.player.notificationSource.SingleBlockRead += InputStream;
                    this.haveAddedInputStream = true;
                }
            }
        }

        private void TryRemoveInputStreamHandler()
        {
            if (clients.Count == 0)
            {
                this.player = playbackService.Player as CSCorePlayer;
                if (this.player != null && this.haveAddedInputStream)
                {
                    this.player.notificationSource.SingleBlockRead -= InputStream;
                    this.haveAddedInputStream = false;
                }
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
        #endregion
    }
}