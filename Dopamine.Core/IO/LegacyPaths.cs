using System;

namespace Dopamine.Core.IO
{
    public sealed class LegacyPaths
    {
        /// <summary>
        /// Gets the user's AppData\Local directory.
        /// </summary>
        /// <returns>User's AppData\Local directory.</returns>
        public static string LocalAppData()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }

        /// <summary>
        /// Gets the user's AppData\Roaming directory.
        /// </summary>
        /// <returns>User's AppData\Roaming directory.</returns>
        public static string AppData()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
    }
}
