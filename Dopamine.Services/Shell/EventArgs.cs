using Dopamine.Core.Enums;
using System;
using System.Windows;

namespace Dopamine.Services.Shell
{
    public class IsMovableChangeRequestedEventArgs : EventArgs
    {
        public IsMovableChangeRequestedEventArgs(bool isMovable)
        {
            this.IsMovable = isMovable;
        }

        public bool IsMovable { get; }
    }

    public class PlaylistVisibilityChangeRequestedEventArgs : EventArgs
    {
        public PlaylistVisibilityChangeRequestedEventArgs(bool isPlaylistVisible, MiniPlayerType miniPlayerType)
        {
            this.IsPlaylistVisible = isPlaylistVisible;
            this.MiniPlayerType = miniPlayerType;
        }

        public bool IsPlaylistVisible { get; }

        public MiniPlayerType MiniPlayerType { get; }
    }

    public class ResizeModeChangeRequestedEventArgs : EventArgs
    {
        public ResizeModeChangeRequestedEventArgs(ResizeMode resizeMode)
        {
            this.ResizeMode = resizeMode;
        }

        public ResizeMode ResizeMode { get; set; }
    }

    public class TopmostChangeRequestedEventArgs : EventArgs
    {
        public TopmostChangeRequestedEventArgs(bool isTopmost)
        {
            this.IsTopmost = isTopmost;
        }

        public bool IsTopmost { get; set; }
    }

    public class WindowStateChangeRequestedEventArgs : EventArgs
    {
        public WindowStateChangeRequestedEventArgs(WindowState windowState)
        {
            this.WindowState = windowState;
        }

        public WindowState WindowState { get; }
    }

    public class ShowWindowControlsChangedEventArgs : EventArgs
    {
        public bool ShowWindowControls { get; set; }
    }

    public class MinimumSizeChangeRequestedEventArgs : EventArgs
    {
        public MinimumSizeChangeRequestedEventArgs(Size minimumSize)
        {
            this.MinimumSize = minimumSize;
        }

        public Size MinimumSize { get; }
    }

    public class GeometryChangeRequestedEventArgs : EventArgs
    {
        public GeometryChangeRequestedEventArgs(double top, double left, Size size)
        {
            this.Top = top;
            this.Left = left;
            this.Size = size;
        }

        public double Top { get; set; }

        public double Left { get; set; }

        public Size Size { get; set; }
    }
}