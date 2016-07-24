using System;
using System.Diagnostics;

namespace Dopamine.Core.Utils
{
    public static class EnvironmentUtils
    {
        public static bool IsWindows10()
        {
            // IMPORTANT: Windows 8.1. and Windows 10 will ONLY admit their real version if you program's manifest 
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
