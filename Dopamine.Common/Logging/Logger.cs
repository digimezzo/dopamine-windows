using Digimezzo.Utilities.Log;
using System.Runtime.CompilerServices;

namespace Dopamine.Common.Logging
{
    public partial class Logger
    {
        public void Info(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Info(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }

        public void Warning(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Warning(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }
        public void Error(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            LogClient.Error(message, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, sourceFilePath, memberName);
        }
    }
}
