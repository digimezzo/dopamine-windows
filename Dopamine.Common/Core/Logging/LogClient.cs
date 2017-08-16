using System.Runtime.CompilerServices;

namespace Dopamine.Core.Logging
{
    public partial class LogClient
    {
        public static string Logfile()
        {
            return Digimezzo.Utilities.Log.LogClient.Logfile();
        }

        public void LogInfo(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            Digimezzo.Utilities.Log.LogClient.Info(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }

        public void LogWarning(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            Digimezzo.Utilities.Log.LogClient.Warning(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }
        public void LogError(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            Digimezzo.Utilities.Log.LogClient.Error(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }
    }
}
