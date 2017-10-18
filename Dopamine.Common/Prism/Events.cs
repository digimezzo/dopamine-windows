using Prism.Events;
using System;

namespace Dopamine.Common.Prism
{
    // Used after refactoring
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

    // To be verified
    //public class CoverPlayerPlaylistButtonClicked : PubSubEvent<bool>
    //{
    //}

    //public class MicroPlayerPlaylistButtonClicked : PubSubEvent<bool>
    //{
    //}

    //public class NanoPlayerPlaylistButtonClicked : PubSubEvent<bool>
    //{
    //}

    public class ToggledCoverPlayerAlignPlaylistVertically : PubSubEvent<bool>
    {
    }
}
