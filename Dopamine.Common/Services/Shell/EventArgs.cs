using Dopamine.Common.Enums;
using System;
using System.Windows;

namespace Dopamine.Common.Services.Shell
{
    public class IsMovableChangedEventArgs : EventArgs
    {
        public bool IsMovable { get; set; }
    }

    public class PlaylistVisibilityChangedEventArgs : EventArgs
    {
        public bool IsPlaylistVisible { get; set; }
        public MiniPlayerType MiniPlayerType { get; set; }
    }

    public class ResizeModeChangedEventArgs : EventArgs
    {
        public ResizeMode ResizeMode { get; set; }
    }

    public class TopmostChangedEventArgs : EventArgs
    {
        public bool IsTopmost { get; set; }
    }

    public class WindowStateChangedEventArgs : EventArgs
    {
        public WindowState WindowState { get; set; }
    }

    public class ShowWindowControlsChangedEventArgs : EventArgs
    {
        public bool ShowWindowControls { get; set; }
    }

    public class MinimumSizeChangedEventArgs : EventArgs
    {
        public Size MinimumSize { get; set; }
    }

    public class GeometryChangedEventArgs : EventArgs
    {
        public double Top { get; set; }
        public double Left { get; set; }
        public Size Size { get; set; }
    }
}