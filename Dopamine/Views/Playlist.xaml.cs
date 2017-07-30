using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.Common.Base;
using Dopamine.Common.Prism;
using Prism.Events;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Dopamine.Views
{
    public partial class Playlist : DopamineWindow
    {
        #region Variables
        private DopamineWindow parent;
        private IPlaybackService playbackService;
        private IRegionManager regionManager;
        private IEventAggregator eventAggregator;
        private MiniPlayerType miniPlayerType;
        private double separationSize = 5;
        private bool alignCoverPlayerPlaylistVertically;
        private IntPtr windowHandle;
        #endregion

        #region Construction
        public Playlist(DopamineWindow parent, IPlaybackService playbackService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.parent = parent;
            this.playbackService = playbackService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<ToggledCoverPlayerAlignPlaylistVertically>().Subscribe(async alignPlaylistVertically =>
            {
                this.alignCoverPlayerPlaylistVertically = alignPlaylistVertically;
                if (this.IsVisible)
                    await this.SetGeometry();
            });
        }
        #endregion

        #region Parent Handlers
        private async void Parent_LocationChanged(object sender, EventArgs e)
        {
            await this.SetGeometry();
        }
        #endregion

        #region Handlers
        protected override async void OnActivated(EventArgs e)
        {
            await this.SetGeometry();
        }
        #endregion

        #region Public
        public async Task Show(MiniPlayerType miniPlayerType)
        {
            this.miniPlayerType = miniPlayerType;

            // The order in the owner chain is very important.
            // It makes sure the overlapping of the windows is correct:
            // From Top to Bottom: Me -> mParent -> mShadow
            this.Owner = this.parent;

            this.parent.LocationChanged += Parent_LocationChanged;

            this.alignCoverPlayerPlaylistVertically = SettingsClient.Get<bool>("Behaviour", "CoverPlayerAlignPlaylistVertically");
            this.SetWindowBorder(SettingsClient.Get<bool>("Appearance", "ShowWindowBorder"));

            // Makes sure the playlist doesn't appear briefly at the topleft 
            // of the screen just before it is positioned by Me.SetGeometry()
            this.Top = this.parent.Top;
            this.Left = this.parent.Left;

            base.Show();

            this.windowHandle = new WindowInteropHelper(this).Handle;

            this.SetTransparency();
            await this.SetGeometry();
        }

        public new void Hide()
        {
            this.parent.LocationChanged -= Parent_LocationChanged;

            base.Hide();
        }
        #endregion

        #region Private
        private void SetTransparency()
        {
            if (EnvironmentUtils.IsWindows10() && SettingsClient.Get<bool>("Appearance", "EnableTransparency"))
            {
                this.PlaylistBackground.Opacity = Constants.OpacityWhenBlurred;
                WindowUtils.EnableBlur(this);
            }
            else
            {
                this.PlaylistBackground.Opacity = 1.0;
            }
        }

        private async Task SetGeometry()
        {
            switch (this.miniPlayerType)
            {
                case MiniPlayerType.CoverPlayer:

                    if (this.alignCoverPlayerPlaylistVertically)
                    {
                        this.Width = this.parent.ActualWidth;
                        this.Height = Constants.CoverPlayerVerticalPlaylistHeight;
                    }
                    else
                    {
                        this.Width = Constants.CoverPlayerHorizontalPlaylistWidth;
                        this.Height = this.parent.ActualHeight;
                    }
                    break;
                case MiniPlayerType.MicroPlayer:
                    this.Width = this.parent.ActualWidth;
                    this.Height = Constants.MicroPlayerPlaylistHeight;
                    break;
                case MiniPlayerType.NanoPlayer:
                    this.Width = this.parent.ActualWidth;
                    this.Height = Constants.NanoPlayerPlaylistHeight;
                    break;
                default:
                    break;
                // Doesn't happen
            }

            var tal = await this.GetTopAndLeft(this.Width, this.Height, this.parent.Top, this.parent.Left,
                this.parent.ActualWidth, this.parent.ActualHeight);
            var top = tal.Item1;
            var left = tal.Item2;

            this.Left = left;
            this.Top = top;
        }

        private async Task<Tuple<double, double>> GetTopAndLeft(double width, double height, double parentTop,
            double parentLeft, double parentActualWidth, double parentActualheight)
        {
            double top = 0, left = 0;
            await Task.Run(() =>
            {
                // We're using the Windows Forms Screen class to get correct screen information.
                // WPF doesn't provide such detailed information about the screens.
                var screen = System.Windows.Forms.Screen.FromHandle(windowHandle);

                if (this.miniPlayerType == MiniPlayerType.CoverPlayer & !this.alignCoverPlayerPlaylistVertically)
                {
                    top = parentTop;

                    if (parentLeft + parentActualWidth + width <= screen.WorkingArea.Width)
                    {
                        // Position at the right of the main window
                        left = parentLeft + parentActualWidth + this.separationSize;
                    }
                    else
                    {
                        // Position at the left of the main window
                        left = parentLeft - Width - this.separationSize;
                    }
                }
                else
                {
                    left = parentLeft;

                    if (parentTop + parentActualheight + height <= screen.WorkingArea.Height)
                    {
                        // Position at the bottom of the main window
                        top = parentTop + parentActualheight + this.separationSize;
                    }
                    else
                    {
                        // Position at the top of the main window
                        top = parentTop - height - this.separationSize;
                    }
                }
            });

            return new Tuple<double, double>(top, left);
        }

        private async void PlaylistWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Duration is set after 1/2 second
            await Task.Delay(500);
            this.MiniPlayerPlaylistRegion.SlideDuration = Constants.SlideTimeoutSeconds;
            this.MiniPlayerPlaylistRegion.FadeOutDuration = Constants.FadeOutTimeoutSeconds;
            this.MiniPlayerPlaylistRegion.FadeInDuration = Constants.FadeInTimeoutSeconds;
        }
        #endregion
    }
}
