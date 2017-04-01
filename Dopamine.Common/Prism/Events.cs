using Digimezzo.WPFControls.Enums;
using Dopamine.Common.Enums;
using Prism.Events;

namespace Dopamine.Common.Prism
{
    public class OobeNavigatedToEvent : PubSubEvent<string>
    {
    }

    public class CloseOobeEvent : PubSubEvent<object>
    {
    }

    public class ChangeOobeSlideDirectionEvent : PubSubEvent<SlideDirection>
    {
    }

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

    public class SettingShowWindowBorderChanged : PubSubEvent<bool>
    {
    }

    public class SettingShowTrayIconChanged : PubSubEvent<bool>
    {
    }

    public class SettingShowRemoveFromDiskChanged : PubSubEvent<bool>
    {
    }

    public class SettingSpectrumStyleChanged : PubSubEvent<SpectrumStyle>
    {
    }

    public class ToggledCoverPlayerAlignPlaylistVertically : PubSubEvent<bool>
    {
    }

    public class ShellMouseUp : PubSubEvent<string>
    {
    }

    public class SettingEnableRatingChanged : PubSubEvent<bool>
    {
    }

    public class SettingEnableLoveChanged : PubSubEvent<bool>
    {
    }

    public class SettingDownloadLyricsChanged : PubSubEvent<bool>
    {
    }

    public class LyricsScreenIsActiveChanged : PubSubEvent<bool>
    {
    }
}
