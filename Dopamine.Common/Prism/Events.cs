using Prism.Events;
using System;

namespace Dopamine.Common.Prism
{
    public class ScrollToPlayingTrack : PubSubEvent<object>
    {
    }

    public class ScrollToHighlightedLyricsLine : PubSubEvent<object>
    {
    }

    public class CoverPlayerPlaylistButtonClicked : PubSubEvent<bool>
    {
    }

    public class MicroPlayerPlaylistButtonClicked : PubSubEvent<bool>
    {
    }

    public class NanoPlayerPlaylistButtonClicked : PubSubEvent<bool>
    {
    }

    public class SelectedSpectrumStyleChanged : PubSubEvent<string>
    {
    }

    public class ToggledCoverPlayerAlignPlaylistVertically : PubSubEvent<bool>
    {
    }

    public class ShellMouseUp : PubSubEvent<string>
    {
    }

    public class PerformSemanticJump : PubSubEvent<Tuple<string, string>>
    {
    }

    public class LyricsScreenIsActiveChanged : PubSubEvent<bool>
    {
    }

    public class NowPlayingIsSelectedChanged : PubSubEvent<bool>
    {
    }
}
