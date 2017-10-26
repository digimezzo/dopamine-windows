using System.Windows;

namespace Dopamine.Common.Services.Shell
{
    public delegate void WindowStateChangedEventHandler(object sender, WindowStateChangedEventArgs e);
    public delegate void PlaylistVisibilityChangedEventHandler(object sender, PlaylistVisibilityChangedEventArgs e);
    public delegate void IsMovableChangedEventHandler(object sender, IsMovableChangedEventArgs e);
    public delegate void ResizeModeChangedEventHandler(object sender, ResizeModeChangedEventArgs e);
    public delegate void TopmostChangedEventHandler(object sender, TopmostChangedEventArgs e);
    public delegate void ShowWindowControlsChangedEventHandler(object sender, ShowWindowControlsChangedEventArgs e);
    public delegate void MinimumSizeChangedEventHandler(object sender, MinimumSizeChangedEventArgs e);
    public delegate void GeometryChangedEventHandler(object sender, GeometryChangedEventArgs e);

    public interface IShellService
    {
        void CheckIfTabletMode(bool isInitializing);
        void ForceFullPlayer();
        void SaveWindowLocation(double top, double left, WindowState state);
        void SaveWindowState(WindowState state);
        void SaveWindowSize(WindowState state, Size size);
        event WindowStateChangedEventHandler WindowStateChanged;
        event PlaylistVisibilityChangedEventHandler PlaylistVisibilityChanged;
        event IsMovableChangedEventHandler IsMovableChanged;
        event ResizeModeChangedEventHandler ResizeModeChanged;
        event TopmostChangedEventHandler TopmostChanged;
        event ShowWindowControlsChangedEventHandler ShowWindowControlsChanged;
        event MinimumSizeChangedEventHandler MinimumSizeChanged;
        event GeometryChangedEventHandler GeometryChanged;
    }
}
