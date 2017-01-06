using Prism.Commands;

namespace Dopamine.Common.Prism
{
    public sealed class ApplicationCommands
    {
        public static CompositeCommand NavigateToMainScreenCommand = new CompositeCommand();
        public static CompositeCommand NavigateToNowPlayingScreenCommand = new CompositeCommand();
        public static CompositeCommand NavigateBetweenCollectionCommand = new CompositeCommand();
        public static CompositeCommand NavigateBetweenSettingsCommand = new CompositeCommand();
        public static CompositeCommand NavigateBetweenInformationCommand = new CompositeCommand();
        public static CompositeCommand NavigateBetweenMainCommand = new CompositeCommand();
        public static CompositeCommand OpenLinkCommand = new CompositeCommand();
        public static CompositeCommand OpenPathCommand = new CompositeCommand();
        public static CompositeCommand TaskbarItemInfoPlayCommand = new CompositeCommand();
        public static CompositeCommand RestoreWindowCommand = new CompositeCommand();
        public static CompositeCommand MinimizeWindowCommand = new CompositeCommand();
        public static CompositeCommand MaximizeRestoreWindowCommand = new CompositeCommand();
        public static CompositeCommand CloseWindowCommand = new CompositeCommand();
        public static CompositeCommand NowPlayingScreenPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand NowPlayingScreenShowcaseButtonCommand = new CompositeCommand();
        public static CompositeCommand NowPlayingScreenArtistInformationButtonCommand = new CompositeCommand();
        public static CompositeCommand NowPlayingScreenLyricsButtonCommand = new CompositeCommand();
        public static CompositeCommand CoverPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand MicroPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand NanoPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand ChangePlayerTypeCommand = new CompositeCommand();
        public static CompositeCommand ToggleMiniPlayerPositionLockedCommand = new CompositeCommand();
        public static CompositeCommand ToggleMiniPlayerAlwaysOnTopCommand = new CompositeCommand();
        public static CompositeCommand ToggleAlwaysShowPlaybackInfoCommand = new CompositeCommand();
        public static CompositeCommand ToggleAlignPlaylistVerticallyCommand = new CompositeCommand();
        public static CompositeCommand TogglePlayerCommand = new CompositeCommand();
        public static CompositeCommand SemanticJumpCommand = new CompositeCommand();
        public static CompositeCommand ShowMainWindowCommand = new CompositeCommand();
        public static CompositeCommand ShowEqualizerCommand = new CompositeCommand();
        public static CompositeCommand AddTracksToPlaylistCommand = new CompositeCommand();
        public static CompositeCommand AddAlbumsToPlaylistCommand = new CompositeCommand();
        public static CompositeCommand AddArtistsToPlaylistCommand = new CompositeCommand();
        public static CompositeCommand AddGenresToPlaylistCommand = new CompositeCommand();
        public static CompositeCommand RefreshLyricsCommand = new CompositeCommand();
    }
}
