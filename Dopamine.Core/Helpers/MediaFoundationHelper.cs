using System;
using System.IO;
using Digimezzo.Foundation.Core.Logging;

namespace Dopamine.Core.Helpers
{
    public static class MediaFoundationHelper
    {
        public static bool HasMediaFoundationSupport(bool canLog = false)
        {
            try
            {
                if (File.Exists(Path.Combine(Environment.SystemDirectory, "mf.dll")))
                {
                    if (canLog)
                    {
                        LogClient.Error("Windows Media Foundation was found!");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                if (canLog)
                {
                    LogClient.Error($"An error occurred while trying to find Windows Media Foundation. Exception: {ex.Message}");
                }
            }

            if (canLog)
            {
                LogClient.Error("Windows Media Foundation could not be found!");
            }

            return false;
        }
    }
}
