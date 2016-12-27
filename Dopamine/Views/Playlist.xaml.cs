using Digimezzo.Utilities.Settings;
using Digimezzo.Utilities.Utils;
using Dopamine.Common.Controls;
using Dopamine.Common.Enums;
using Dopamine.Common.Services.Playback;
using Dopamine.Core.Base;
using Dopamine.Core.Prism;
using Prism.Events;
using Prism.Regions;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        #endregion

        #region Construction
        public Playlist(DopamineWindow parent, IPlaybackService playbackService, IRegionManager regionManager, IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this.parent = parent;
            this.playbackService = playbackService;
            this.regionManager = regionManager;
            this.eventAggregator = eventAggregator;

            this.eventAggregator.GetEvent<ToggledCoverPlayerAlignPlaylistVertically>().Subscribe(alignPlaylistVertically =>
            {
                this.alignCoverPlayerPlaylistVertically = alignPlaylistVertically;
                if (this.IsVisible)
                    this.SetGeometry();
            });
        }
        #endregion

        #region Parent Handlers
        private void Parent_LocationChanged(object sender, EventArgs e)
        {
            this.SetGeometry();
        }
        #endregion

        #region Handlers
        protected override void OnActivated(EventArgs e)
        {
            this.SetGeometry();
        }
        #endregion

        #region Public
        public void Show(MiniPlayerType miniPlayerType)
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

            this.SetTransparency();
            this.SetGeometry();
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

        private void SetGeometry()
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

            this.Top = this.GetTop();
            this.Left = this.GetLeft();
        }

        private double GetTop()
        {
            if (this.miniPlayerType == MiniPlayerType.CoverPlayer & !this.alignCoverPlayerPlaylistVertically)
            {
                return this.parent.Top;
            }
            else
            {
                // We're using the Windows Forms Screen class to get correct screen information.
                // WPF doesn't provide such detailed information about the screens.
                var screen = System.Windows.Forms.Screen.FromRectangle(new System.Drawing.Rectangle(Convert.ToInt32(this.Left), Convert.ToInt32(this.Top), Convert.ToInt32(this.Width), Convert.ToInt32(this.Height)));


                if (this.parent.Top + this.parent.ActualHeight + this.Height <= screen.WorkingArea.Bottom)
                {
                    // Position at the bottom of the main window
                    return this.parent.Top + this.parent.ActualHeight + this.separationSize;
                }
                else
                {
                    // Position at the top of the main window
                    return this.parent.Top - this.Height - this.separationSize;
                }
            }
        }

        private double GetLeft()
        {
            if (this.miniPlayerType == MiniPlayerType.CoverPlayer & !this.alignCoverPlayerPlaylistVertically)
            {
                // We're using the Windows Forms Screen class to get correct screen information.
                // WPF doesn't provide such detailed information about the screens.
                var screen = System.Windows.Forms.Screen.FromRectangle(new System.Drawing.Rectangle(Convert.ToInt32(this.Left), Convert.ToInt32(this.Top), Convert.ToInt32(this.Width), Convert.ToInt32(this.Height)));


                if (this.parent.Left + this.parent.ActualWidth + this.Width <= screen.WorkingArea.Right)
                {
                    // Position at the right of the main window
                    return this.parent.Left + this.parent.ActualWidth + this.separationSize;
                }
                else
                {
                    // Position at the left of the main window
                    return this.parent.Left - this.Width - this.separationSize;
                }
            }
            else
            {
                return this.parent.Left;
            }
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
