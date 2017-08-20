using System;
using System.Runtime.CompilerServices;

namespace Dopamine.Core.Logging
{
    public class MockLogClient : ILogClient
    {
        public string Logfile()
        {
            return string.Empty;
        }

        public void Info(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
        }

        public void Warning(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
        }

        public void Error(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
        }

        public string GetAllExceptions(Exception ex)
        {
            return string.Empty;
        }
    }
}
