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

        // Lyrics extensions
        public static string LRC = ".lrc";

        // Playlist extensions
        public static string M3U = ".m3u";
        public static string WPL = ".wpl";
        public static string ZPL = ".zpl";

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
                                                            FileFormats.WAV
                                                          };
        public static string[] SupportedPlaylistExtensions = {
                                                                FileFormats.M3U,
                                                                FileFormats.ZPL
                                                             };

        public static bool IsSupportedAudioFile(string path)
        {
            return SupportedMediaExtensions.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }

        public static bool IsSupportedPlaylistFile(string path)
        {
            return SupportedPlaylistExtensions.Contains(System.IO.Path.GetExtension(path), StringComparer.OrdinalIgnoreCase);
        }
    }
}
