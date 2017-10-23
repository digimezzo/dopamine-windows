using Prism.Events;
using System;

namespace Dopamine.Common.Prism
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

    public class IsNowPlayingLyricsPageActiveChanged : PubSubEvent<bool>
    {
    }
}