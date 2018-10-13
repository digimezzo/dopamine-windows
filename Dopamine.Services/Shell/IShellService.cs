using System.Windows;

namespace Dopamine.Services.Shell
{
    public delegate void WindowStateChangedEventHandler(object sender, WindowStateChangedEventArgs e);
    public delegate void WindowStateChangeRequestedEventHandler(object sender, WindowStateChangeRequestedEventArgs e);
    public delegate void PlaylistVisibilityChangeRequestedEventHandler(object sender, PlaylistVisibilityChangeRequestedEventArgs e);
    public delegate void IsMovableChangeRequestedEventHandler(object sender, IsMovableChangeRequestedEventArgs e);
    public delegate void ResizeModeChangeRequestedEventHandler(object sender, ResizeModeChangeRequestedEventArgs e);
    public delegate void TopmostChangeRequestedEventHandler(object sender, TopmostChangeRequestedEventArgs e);
    public delegate void ShowWindowControlsChangedEventHandler(object sender, ShowWindowControlsChangedEventArgs e);
    public delegate void MinimumSizeChangeRequestedEventHandler(object sender, MinimumSizeChangeRequestedEventArgs e);
    public delegate void GeometryChangeRequestedEventHandler(object sender, GeometryChangeRequestedEventArgs e);

    public interface IShellService
    {
        WindowState WindowState { get; set; }

        void CheckIfTabletMode(bool isInitializing);

        void SaveWindowLocation(double top, double left, WindowState state);

        void SaveWindowState(WindowState state);

        void SaveWindowSize(WindowState state, Size size);

        event WindowStateChangedEventHandler WindowStateChanged;
        event WindowStateChangeRequestedEventHandler WindowStateChangeRequested;
        event PlaylistVisibilityChangeRequestedEventHandler PlaylistVisibilityChangeRequested;
        event IsMovableChangeRequestedEventHandler IsMovableChangeRequested;
        event ResizeModeChangeRequestedEventHandler ResizeModeChangeRequested;
        event TopmostChangeRequestedEventHandler TopmostChangeRequested;
        event MinimumSizeChangeRequestedEventHandler MinimumSizeChangeRequested;
        event GeometryChangeRequestedEventHandler GeometryChangeRequested;
    }
}
