namespace Dopamine.UWP.IO
{
    public static class LegacyPaths
    {
        public static string LocalAppDataFolder()
        {
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
        }

        //public static string AppDataFolder()
        //{
        //    return Windows.Storage.ApplicationData.Current.RoamingFolder.Path;
        //}
    }
}
