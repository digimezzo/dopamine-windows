using Digimezzo.WPFControls.Enums;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Dopamine.Core.Prism
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

    public class RenameSelectedPlaylistWithKeyF2 : PubSubEvent<object>
    {
    }

    public class DeleteSelectedPlaylistsWithKeyDelete : PubSubEvent<object>
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

    public class ToggledCoverPlayerAlignPlaylistVertically : PubSubEvent<bool>
    {
    }

    public class ShellMouseUp : PubSubEvent<string>
    {
    }

    public class SettingUseStarRatingChanged : PubSubEvent<bool>
    {
    }

    public class SettingEnableRatingChanged : PubSubEvent<bool>
    {
    }
}
