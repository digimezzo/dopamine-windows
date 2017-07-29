using Digimezzo.Utilities.Log;
using System;
using System.Runtime.CompilerServices;

namespace Dopamine.Common.Services.Logging
{
    public class LoggingService : Core.Services.Logging.ILoggingService
    {
        #region Private
        private string GetCallsite(string sourceFilePath = "", string memberName = "")
        {
            string callsite = string.Empty;

            try
            {
                callsite = string.Format("{0}.{1}", System.IO.Path.GetFileNameWithoutExtension(sourceFilePath), memberName);
            }
            catch (Exception)
            {
                // Swallow
            }

            return callsite;
        }
        #endregion

        #region ILogService
        public void Info(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Info(message, this.GetCallsite(sourceFilePath, memberName), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public void Warning(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Warning(message, this.GetCallsite(sourceFilePath, memberName), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        public void Error(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Error(message, this.GetCallsite(sourceFilePath, memberName), arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        public string GetAllExceptions(Exception ex)
        {
            return LogClient.GetAllExceptions(ex);
        }
        #endregion
    }
}
