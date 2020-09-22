using System;
using System.Linq;

namespace Dopamine.Core.Base
{
    public static class FileFormats
    {
        // Audio extensions
        public static string MP3 = ".mp3";
        public static string OGG = ".ogg";
        public static string WMA = ".wma";
        public static string FLAC = ".flac";
        public static string M4A = ".m4a";
        public static string AAC = ".aac";
        public static string WAV = ".wav";
        public static string OPUS = ".opus";
        public static string AIF = ".aif";
        public static string AIFF = ".aiff";
        public static string APE = ".ape";

        // Lyrics extensions
        public static string LRC = ".lrc";

        // Playlist extensions
        public static string M3U = ".m3u";
        public static string M3U8 = ".m3u8";
        public static string WPL = ".wpl";
        public static string ZPL = ".zpl";

        // Smart playlist extensions
        public static string DSPL = ".dspl";

        // Image extensions
        public static string PNG = ".png";
        public static string JPG = ".jpg";
        public static string JPEG = ".jpeg";
        public static string BMP = ".bmp";

        // Equalizer preset extension
        public static string DEQ = ".deq";

        // Supported extensions
        public static string[] SupportedMediaExtensions = {
            FileFormats.MP3,
            FileFormats.OGG,
            FileFormats.WMA,
            FileFormats.FLAC,
            FileFormats.M4A,
            FileFormats.AAC,
            FileFormats.WAV,
            FileFormats.OPUS,
            FileFormats.AIF,
            FileFormats.AIFF,
            FileFormats.APE
        };

        public static string[] SupportedStaticPlaylistExtensions = {
            FileFormats.M3U,
             FileFormats.M3U8,
            FileFormats.ZPL,
            FileFormats.WPL
        };

        public static string[] SupportedSmartPlaylistExtensions = {
            FileFormats.DSPL
        };

        public static bool IsSupportedAudioFile(string path)
        {
            return SupportedMediaExtensions.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsSupportedStaticPlaylistFile(string path)
        {
            return SupportedStaticPlaylistExtensions.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsSupportedSmartPlaylistFile(string path)
        {
            return SupportedSmartPlaylistExtensions.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }
    }
}
