//using CSCore.DSP;
//using CSCore.Streams;
using Dopamine.Core.Audio;
using Dopamine.Core.Extensions;
using Dopamine.Services.Cache;
using Dopamine.Services.Playback;
using System;
using System.Collections.Generic;
//using System.IO;
//using System.IO.MemoryMappedFiles;
using System.Reflection;
using System.ServiceModel;
//using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Threading;

namespace Dopamine.Services.ExternalControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    internal class ExternalControlServer : IExternalControlServer, IDisposable // , IFftDataServer
    {
        private const int FftDataLength = 256 * 4;

        private CSCorePlayer player;
        //private readonly FftProvider fftProvider = new FftProvider(2, FftSize.Fft256);
        //private readonly DispatcherTimer fftProviderDataTimer;
        //private bool haveAddedInputStream;
        //private readonly float[] fftDataBuffer = new float[FftDataLength / 4];
        //private readonly byte[] fftDataBufferBytes = new byte[FftDataLength];
        //private readonly MemoryMappedFile fftDataMemoryMappedFile;
        //private readonly MemoryMappedViewStream fftDataMemoryMappedFileStream;
        //private readonly BinaryWriter fftDataMemoryMappedFileStreamWriter;
        //private readonly Mutex fftDataMemoryMappedFileMutex;

        private readonly Dictionary<string, IExternalControlServerCallback> clients = new Dictionary<string, IExternalControlServerCallback>();
        private readonly Stack<string> deadClients = new Stack<string>();
        private readonly object clientsLock = new object();

        private readonly IPlaybackService playbackService;
        private readonly ICacheService cacheService;
   
        public ExternalControlServer(IPlaybackService playbackService, ICacheService cacheService)
        {
            this.playbackService = playbackService;
            this.cacheService = cacheService;

            //this.fftProviderDataTimer = new DispatcherTimer(){Interval = TimeSpan.FromSeconds(2)};
            //this.fftProviderDataTimer.Tick += FftProviderDataTimerElapsed;

            //fftDataMemoryMappedFile = MemoryMappedFile.CreateOrOpen("DopamineFftDataMemory", FftDataLength, MemoryMappedFileAccess.ReadWrite, MemoryMappedFileOptions.DelayAllocatePages, null, HandleInheritability.None);
            //fftDataMemoryMappedFileStream = fftDataMemoryMappedFile.CreateViewStream(0, FftDataLength, MemoryMappedFileAccess.ReadWrite);
            //fftDataMemoryMappedFileStreamWriter = new BinaryWriter(fftDataMemoryMappedFileStream);
            //fftDataMemoryMappedFileMutex = new Mutex(true, "DopamineFftDataMemoryMutex");
            //fftDataMemoryMappedFileMutex.ReleaseMutex();
        }
    
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
                    //fftDataMemoryMappedFileMutex?.Dispose();
                    //fftDataMemoryMappedFileStreamWriter?.Dispose();
                    //fftDataMemoryMappedFileStream?.Dispose();
                    //fftDataMemoryMappedFile?.Dispose();
                }
            }
        }

        ~ExternalControlServer()
        {
            Dispose(false);
        }
       
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
        public void SendHeartbeat() { }

        [OperationBehavior]
        public async Task PlayNext() => await this.playbackService.PlayNextAsync();

        [OperationBehavior]
        public async Task PlayPrevious() => await this.playbackService.PlayPreviousAsync();

        [OperationBehavior]
        public void SetMute(bool mute) => this.playbackService.SetMute(mute);

        [OperationBehavior]
        public Task PlayOrPause() => this.playbackService.PlayOrPauseAsync();

        [OperationBehavior]
        public bool GetIsStopped() => this.playbackService.IsStopped;

        [OperationBehavior]
        public bool GetIsPlaying() => this.playbackService.IsPlaying;

        [OperationBehavior]
        public double GetProgress() => this.playbackService.Progress;

        [OperationBehavior]
        public void SetProgress(double progress) => this.playbackService.SkipProgress(progress);

        [OperationBehavior]
        public ExternalTrack GetCurrenTrack() => new ExternalTrack(this.playbackService.CurrentTrack);

        [OperationBehavior]
        public string GetCurrentTrackArtworkPath(string artworkId) => this.cacheService.GetCachedArtworkPath(artworkId);
     
        [OperationBehavior]
        public int GetFftDataSize() => FftDataLength;

        //[OperationBehavior]
        //public async Task GetFftData()
        //{
        //    this.fftProviderDataTimer.Stop();
        //    this.fftProviderDataTimer.Start();
        //    TryAddInputStreamHandler();

        //    await Task.Run(() =>
        //    {
        //        this.fftProvider.GetFftData(fftDataBuffer);

        //        fftDataMemoryMappedFileMutex.WaitOne();
        //        Buffer.BlockCopy(fftDataBuffer, 0, fftDataBufferBytes, 0, fftDataBufferBytes.Length);
        //        fftDataMemoryMappedFileStreamWriter.Seek(0, SeekOrigin.Begin);
        //        fftDataMemoryMappedFileStreamWriter.Write(fftDataBufferBytes);
        //        fftDataMemoryMappedFileMutex.ReleaseMutex();
        //    });
        //}
       
        internal void Open()
        {
            this.playbackService.PlaybackSuccess += PlaybackSuccessCallback;
            this.playbackService.PlaybackStopped += PlaybackStoppedCallback;
            this.playbackService.PlaybackPaused += PlaybackPausedCallBack;
            this.playbackService.PlaybackResumed += PlaybackResumedCallBack;
            this.playbackService.PlaybackProgressChanged += PlaybackProgressChangedCallBack;
            this.playbackService.PlaybackVolumeChanged += PlaybackVolumeChangedCallBack;
            this.playbackService.PlaybackMuteChanged += PlaybackMuteCallBack;
            this.playbackService.PlayingTrackChanged += PlayingTrackChangedCallback;
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
            this.playbackService.PlayingTrackChanged -= PlayingTrackChangedCallback;
        }

        private void PlayingTrackChangedCallback(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlayingTrackChangedAsync));
        }

        private void PlaybackMuteCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackMuteChangedAsync));
        }

        private void PlaybackVolumeChangedCallBack(object sender, PlaybackVolumeChangedEventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackVolumeChangedAsync));
        }

        private void PlaybackProgressChangedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackProgressChangedAsync));
        }

        private void PlaybackResumedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackResumedAsync));
        }

        private void PlaybackPausedCallBack(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackPausedAsync));
        }

        private void PlaybackStoppedCallback(object sender, EventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackStoppedAsync));
        }

        private void PlaybackSuccessCallback(object sender, PlaybackSuccessEventArgs e)
        {
            ProxyMethod(nameof(IExternalControlServerCallback.RaiseEventPlaybackSuccessAsync));
        }

        private void ProxyMethod(string methodName)
        {
            MethodInfo methodInfo = typeof(IExternalControlServerCallback).GetMethod(methodName);

            lock (clientsLock)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        methodInfo.Invoke(client.Value, null);
                    }
                    catch (Exception ex)
                    {
                        deadClients.Push(client.Key);
                    }
                }

                foreach (var c in deadClients)
                {
                    clients.Remove(c);
                }
                deadClients.Clear();
            }
        }

        //private void TryAddInputStreamHandler()
        //{
        //    this.player = playbackService.Player as CSCorePlayer;

        //    if (this.player != null && !this.haveAddedInputStream)
        //    {
        //        this.player.NotificationSource.SingleBlockRead += InputStream;
        //        this.haveAddedInputStream = true;
        //    }
        //}

        //private void TryRemoveInputStreamHandler()
        //{
        //    this.player = playbackService.Player as CSCorePlayer;

        //    if (this.player != null && this.haveAddedInputStream)
        //    {
        //        this.player.NotificationSource.SingleBlockRead -= InputStream;
        //        this.haveAddedInputStream = false;
        //    }
        //}

        //private void InputStream(object sender, SingleBlockReadEventArgs e)
        //{
        //    try
        //    {
        //        this.fftProvider.Add(e.Left, e.Right);
        //    }
        //    catch (Exception)
        //    {
        //        // Intended suppression 
        //    }
        //}

        //private void FftProviderDataTimerElapsed(object sender, EventArgs e)
        //{
        //    this.fftProviderDataTimer.Stop();
        //    this.TryRemoveInputStreamHandler();
        //}
    }
}