using Digimezzo.WPFControls.Enums;
using Dopamine.Core.Enums;
using Prism.Events;
using System;

namespace Dopamine.Core.Prism
{
    public class ScrollToPlayingTrack : PubSubEvent<object>
    {
    }

    public class PerformSemanticJump : PubSubEvent<Tuple<string, string>>
    {
    }

    public class ShellMouseUp : PubSubEvent<string>
    {
    }

    public class ScrollToHighlightedLyricsLine : PubSubEvent<object>
    {
    }

    public class ToggledCoverPlayerAlignPlaylistVertically : PubSubEvent<bool>
    {
    }

    public class IsNowPlayingPageActiveChanged : PubSubEvent<bool>
    {
    }

    public class IsNowPlayingSubPageChanged : PubSubEvent<Tuple<SlideDirection, NowPlayingSubPage>>
    {
    }

    public class IsCollectionPageChanged : PubSubEvent<Tuple<SlideDirection, CollectionPage>>
    {
    }

    public class IsPlaylistsPageChanged : PubSubEvent<Tuple<SlideDirection, PlaylistsPage>>
    {
    }

    public class IsSettingsPageChanged : PubSubEvent<Tuple<SlideDirection, SettingsPage>>
    {
    }

    public class IsInformationPageChanged : PubSubEvent<Tuple<SlideDirection, InformationPage>>
    {
    }

    public class ToggleArtistOrderCommand : PubSubEvent<object>
    {
    }

    public class FocusSearchBox : PubSubEvent<object>
    {
    }

    public class ActiveSubfolderChanged : PubSubEvent<object>
    {
    }
}