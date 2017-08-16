using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dopamine.Core.Logging
{
    public partial class LogClient
    {
        private static LogClient instance;

        private LogClient()
        {
        }

        public static LogClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogClient();
                }
                return instance;
            }
        }

        public static void Info(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Instance.LogInfo(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }

        public static void Warning(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Instance.LogWarning(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }

        public static void Error(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Instance.LogError(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }

        public static string GetAllExceptions(Exception ex)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Exception:");
            sb.AppendLine(ex.ToString());
            sb.AppendLine("");
            sb.AppendLine("Stack trace:");
            sb.AppendLine(ex.StackTrace);

            int innerExceptionCounter = 0;

            while (ex.InnerException != null)
            {
                innerExceptionCounter += 1;
                sb.AppendLine("Inner Exception " + innerExceptionCounter + ":");
                sb.AppendLine(ex.InnerException.ToString());
                ex = ex.InnerException;
            }

            return sb.ToString();
        }
    }
}
