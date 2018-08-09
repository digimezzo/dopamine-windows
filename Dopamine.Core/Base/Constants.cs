using System;
using System.Windows;
using Digimezzo.Utilities.Utils;

namespace Dopamine.Core.Base
{
    public static class Constants
    {
        // Tags
        public static string TagDelimiter = ";";
        public static string DoubleTagDelimiter = $"{TagDelimiter}{TagDelimiter}";

        // Environment
        public static bool IsWindows10 = EnvironmentUtils.IsWindows10();

        // Links
        public static string HomeLink = "https://www.digimezzo.com";
        public static string LastFmJoinLink = "https://www.last.fm/join";

        // Default Window button sizes
        //public static double DefaultWindowButtonHeight = 29;
        //public static double DefaultWindowButtonWidth = 45;

        // Fonts
        public static double GlobalFontSize = 12;
        public static string GlobalFontFamily = "Segoe UI";
        public static double OobeAppNameFontSize = 44;
        public static double OobeTitleFontSize = 36;
        public static double OobeTextFontSize = 18;
        public static double NowPlayingHeaderFontSize = 36;
        public static int AboutAppNameFontSize = 44;

        // Covers
        public static double CoverSmallSize = 120;
        public static double CoverMediumSize = 150;
        public static double CoverLargeSize = 180;
        public static double TrackCoverSize = 40;

        // We're forcing PathToImageConverter to create thumbnails that are 2.5 larger than what will
        // be displayed at Windows Desktop scaling of 100%. That way, when the Windows Desktop scaling
        // is set to 250%, images won't need to be upscaled by Windows and they'll remain sharp.
        public static readonly double CoverUpscaleFactor = 2.5;
        public static readonly int CoverQualityPercent = 80;

        // Headphone icon
        public static readonly double HeadPhoneSmallSize = 45;
        public static readonly double HeadPhoneLargeSize = 65;

        // Shell
        public static readonly double DefaultShellTop = 50;
        public static readonly double DefaultShellLeft = 50;
        public static readonly double MinShellWidth = 815;
        public static readonly double MinShellHeight = 480;

        // Cover Player
        public static readonly double CoverPlayerWidth = 350;
        public static readonly double CoverPlayerHeight = 424;

        // Micro Player
        public static readonly double MicroPlayerWidth = 459;
        public static readonly double MicroPlayerHeight = 139;

        // Nano Player
        public static readonly double NanoPlayerWidth = 460;
        public static readonly double NanoPlayerHeight = 48;

        // Cover Player playlist
        public static readonly double CoverPlayerHorizontalPlaylistWidth = 460;
        public static readonly double CoverPlayerVerticalPlaylistHeight = 444;

        // Micro Player playlist
        public static readonly double MicroPlayerPlaylistHeight = 444;

        // Nano Player playlist
        public static readonly double NanoPlayerPlaylistHeight = 444;

        // Intervals
        public static readonly int UpdateCheckIntervalSeconds = 900; // 900 seconds = 15 minutes
        public static readonly double SearchTimeoutSeconds = 0.4;
        public static readonly double ScrollToPlayingTrackTimeoutSeconds = 0.4;

        // Delays
        public static readonly int ClosingFadeOutDelay = 500;
        public static readonly int ArtworkLoadDelay = 150;
        public static readonly int CommonListLoadDelay = 250;
        public static readonly int NowPlayingListLoadDelay = 400;
        public static readonly int MiniPlayerListLoadDelay = 250;
        public static readonly int CloudLoadDelay = 150;
        public static readonly int DelaySelectedAlbumsDelay = 250;

        // Transparency
        public static readonly double OpacityWhenBlurred = 0.85;

        // File sizes in Bytes
        public static readonly int GigaByteInBytes = 1073741824;
        public static readonly int MegaByteInBytes = 1048576;
        public static readonly int KiloByteInBytes = 1024;

        // Semantic zoom
        public static readonly int SemanticZoomHeaderHeight = 45;

        // Tray controls
        public static readonly double TrayControlsWidth = 300;
        public static readonly double TrayControlsHeight = 255;

        // ListBoxAlbums
        public static readonly double AlbumTileAlbumInfoHeight = 44;
        public static readonly Thickness AlbumTilePadding = new Thickness(4);
        public static readonly Thickness AlbumTileMargin = new Thickness(6);

        // Animation
        public static readonly Duration MouseEnterDuration = new Duration(TimeSpan.FromMilliseconds(50));
        public static readonly Duration MouseLeaveDuration = new Duration(TimeSpan.FromMilliseconds(200));
        public static readonly Duration SourceChangedImageChangedDuration = new Duration(TimeSpan.FromMilliseconds(500));
        public static readonly int SlideDistance = 20;
    }
}
