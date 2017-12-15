using Prism.Commands;

namespace Dopamine.Core.Prism
{
    public sealed class ApplicationCommands
    {
        public static CompositeCommand ShowNowPlayingCommand = new CompositeCommand();
        public static CompositeCommand ShowFullPlayerCommand = new CompositeCommand();
        public static CompositeCommand TogglePlayerCommand = new CompositeCommand();
        public static CompositeCommand RestoreWindowCommand = new CompositeCommand();
        public static CompositeCommand MinimizeWindowCommand = new CompositeCommand();
        public static CompositeCommand MaximizeRestoreWindowCommand = new CompositeCommand();
        public static CompositeCommand CloseWindowCommand = new CompositeCommand();
        public static CompositeCommand ChangePlayerTypeCommand = new CompositeCommand();
        public static CompositeCommand ShowMainWindowCommand = new CompositeCommand();
        public static CompositeCommand CoverPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand MicroPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand NanoPlayerPlaylistButtonCommand = new CompositeCommand();
        public static CompositeCommand RefreshLyricsCommand = new CompositeCommand();
        public static CompositeCommand ToggleAlignPlaylistVerticallyCommand = new CompositeCommand();
        public static CompositeCommand ToggleMiniPlayerPositionLockedCommand = new CompositeCommand();
        public static CompositeCommand ToggleMiniPlayerAlwaysOnTopCommand = new CompositeCommand();
        public static CompositeCommand ToggleAlwaysShowPlaybackInfoCommand = new CompositeCommand();
    }
}
