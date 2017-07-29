using System;
using System.Runtime.CompilerServices;

namespace Dopamine.Core.Services.Logging
{
    public interface ILoggingService
    {
        void Info(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");
        void Warning(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");
        void Error(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");
        string GetAllExceptions(Exception ex);
    }
}
