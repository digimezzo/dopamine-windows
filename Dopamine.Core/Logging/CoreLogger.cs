using Microsoft.Practices.ServiceLocation;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dopamine.Core.Logging
{
    public abstract class CoreLogger : ICoreLogger
    {
        public static ICoreLogger Current
        {
            get
            {
                ICoreLogger coreLogger;

                try
                {
                    coreLogger = ServiceLocator.Current.GetInstance<ICoreLogger>();
                }
                catch (Exception)
                {
                    // Failure to resolve an implementation of ICoreLogger should not break code which require logging.
                    // This is especially useful in unit tests, where logging is not the center of focus.
                    coreLogger = new MockCoreLogger();
                }

                return coreLogger;
            }
        }

        public abstract string Logfile();

        public abstract void Info(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");
        public abstract void Warning(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");
        public abstract void Error(string message, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, object arg6 = null, object arg7 = null, object arg8 = null, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "");

        public string GetAllExceptions(Exception ex)
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