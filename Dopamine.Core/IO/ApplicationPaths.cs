using Dopamine.Core.Base;

namespace Dopamine.Core.IO
{
    public static class ApplicationPaths
    {
        public static string LogSubDirectory = "Log";
        public static string ColorSchemesSubDirectory = "ColorSchemes";
        public static string CacheSubDirectory = "Cache";
        public static string CoverArtCacheSubDirectory = "CoverArt";
        public static string IconsSubDirectory = "Icons";
        public static string BuiltinLanguagesSubDirectory = "Languages";
        public static string CustomLanguagesSubDirectory = "Languages";
        public static string UpdatesSubDirectory = "Updates";
        public static string LogFile = ProductInformation.ApplicationAssemblyName + ".log";
        public static string LogArchiveFile = ProductInformation.ApplicationAssemblyName + ".{#}.log";
        public static string ExecutionFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }
}
