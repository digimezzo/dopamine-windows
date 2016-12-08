using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace Dopamine.Core.Utils
{
    public static class EnvironmentUtils
    {
        /// <summary>
        /// Uses WMI to get the "friendly" Windows version
        /// </summary>
        /// <returns></returns>
        public static string GetFriendlyWindowsVersion()
        {
            var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                        select x.GetPropertyValue("Caption")).FirstOrDefault();
            return name != null ? name.ToString() : "Unknown";
        }

        /// <summary>
        /// Uses Environment.OSVersion to get the internal Windows version
        /// </summary>
        /// <returns></returns>
        public static string GetInternalWindowsVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        public static bool IsWindows10()
        {
            // IMPORTANT: Windows 8.1. and Windows 10 will ONLY admit their real version if your program's manifest 
            // claims to be compatible. Otherwise they claim to be Windows 8. See the first comment on:
            // https://msdn.microsoft.com/en-us/library/windows/desktop/ms724833%28v=vs.85%29.aspx

            bool returnValue = false;

            // Get Operating system information
            OperatingSystem os = Environment.OSVersion;

            // Get the Operating system version information
            Version vi = os.Version;

            // Pre-NT versions of Windows are PlatformID.Win32Windows. We're not interested in those.

            if (os.Platform == PlatformID.Win32NT)
            {
                if (vi.Major == 10)
                {
                    returnValue = true;
                }
            }

            return returnValue;
        }

        public static bool IsSingleInstance(string processName)
        {

            Process[] pName = Process.GetProcessesByName(processName);

            if ((pName.Length > 1 | pName.Length == 0))
            {
                return false;
            }
            else
            {
                return true;
            }

        }

    }
}
