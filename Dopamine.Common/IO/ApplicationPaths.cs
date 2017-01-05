using Dopamine.Common.Base;

namespace Dopamine.Common.IO
{
    public static class ApplicationPaths
    {
        public static string LogFolder = "Log";
        public static string ColorSchemesFolder = "ColorSchemes";
        public static string EqualizerFolder = "Equalizer";
        public static string CacheFolder = "Cache";
        public static string CoverArtCacheFolder = "CoverArt";
        public static string TemporaryCacheFolder = "Temporary";
        public static string IconsSubDirectory = "Icons";
        public static string BuiltinLanguagesFolder = "Languages";
        public static string CustomLanguagesFolder = "Languages";
        public static string UpdatesFolder = "Updates";
        public static string LogFile = ProductInformation.ApplicationAssemblyName + ".log";
        public static string LogArchiveFile = ProductInformation.ApplicationAssemblyName + ".{#}.log";
        public static string ExecutionFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
