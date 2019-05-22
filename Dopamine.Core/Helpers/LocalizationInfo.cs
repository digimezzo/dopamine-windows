using Digimezzo.Foundation.Core.Utils;

namespace Dopamine.Core.Helpers
{
    public class LocalizationInfo : ILocalizationInfo
    {
        public string NeteaseLyrics => ResourceUtils.GetString("Language_NeteaseLyrics");
        public string XiamiLyrics => ResourceUtils.GetString("Language_XiamiLyrics");
    }
}
