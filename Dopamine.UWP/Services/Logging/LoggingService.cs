using Dopamine.UWP.IO;
using SQLite;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Dopamine.UWP.Services.Logging
{
    public class LoggingService : Core.Services.Logging.ILoggingService
    {
        #region Variables
        private SQLiteConnection connection;
        private ConcurrentQueue<LogEntry> logEntries;
        private DispatcherTimer saveTimer;
        private DispatcherTimer cleanupTimer;
        private bool isCleaning;
        private int keepMaxLogRecords = 50;
        #endregion

        #region Construction
        public LoggingService()
        {
            this.Initialize();
        }
        #endregion

        #region Private
        private void Initialize()
        {
            // Initializing the connection also creates the database file if it doesn't exist yet.
            this.connection = new SQLiteConnection(System.IO.Path.Combine(LegacyPaths.LocalAppDataFolder(), "Logging.db"));

            // Check if the table exists. If not, create it.
            var tableExistsQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name='LogEntry';";
            var result = connection.ExecuteScalar<string>(tableExistsQuery);

            if (result == null) this.connection.CreateTable<LogEntry>();

            // Instantiate the logEntries queue
            this.logEntries = new ConcurrentQueue<LogEntry>();

            // Initialize the timers
            this.saveTimer = new DispatcherTimer();
            this.saveTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); // Every 10 milliseconds
            this.saveTimer.Tick += SaveTimer_Tick;

            this.cleanupTimer = new DispatcherTimer();
            this.cleanupTimer.Interval = new TimeSpan(0, 5, 0); // Every 5 minutes
            this.cleanupTimer.Tick += CleanupTimer_Tick;
            this.cleanupTimer.Start();
        }

        private void SaveTimer_Tick(object sender, object e)
        {
            this.SaveLogEntries();
        }

        private void CleanupTimer_Tick(object sender, object e)
        {
            this.CleanupLog();
        }

        private async void SaveLogEntries()
        {
            if (this.isCleaning) return; // Don't save logEntries when the log is being cleaned up

            this.saveTimer.Stop();

            await Task.Run(() =>
            {
                try
                {
                    while (this.logEntries.Count > 0)
                    {
                        LogEntry entry;

                        if (this.logEntries.TryDequeue(out entry)) this.connection.Insert(entry);
                    }
                }
                catch (Exception)
                {
                    // A failure to should never crash the application
                }
            });

            // In case there are entries left
            if (this.logEntries.Count > 0) this.saveTimer.Start();
        }

        private async void CleanupLog()
        {
            this.cleanupTimer.Stop();
            this.isCleaning = true;

            await Task.Run(() =>
            {
                while (this.connection.Table<LogEntry>().Count() > this.keepMaxLogRecords)
                {
                    this.connection.Execute("DELETE FROM LogEntry WHERE TimeStamp IN(SELECT TimeStamp FROM LogEntry ORDER BY TimeStamp LIMIT 1);");
                }
            });

            this.isCleaning = false;
            this.cleanupTimer.Start();
        }

        private void AddLogEntry(LogLevel level, string message, string[] args, string callerFilePath, string callerMemberName)
        {
            try
            {
                if (args != null) message = string.Format(message, args);

                string levelDescription = string.Empty;

                switch (level)
                {
                    case LogLevel.Info:
                        levelDescription = "Info";
                        break;
                    case LogLevel.Warning:
                        levelDescription = "Warning";
                        break;
                    case LogLevel.Error:
                        levelDescription = "Error";
                        break;
                    default:
                        levelDescription = "Error";
                        break;
                }

                this.logEntries.Enqueue(new LogEntry { TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), Level = levelDescription, CallerFilePath = System.IO.Path.GetFileNameWithoutExtension(callerFilePath), CallerMemberName = callerMemberName, Message = message });
                this.saveTimer.Stop();
                this.saveTimer.Start();
            }
            catch (Exception)
            {
                // A failure to should never crash the application
            }
        }
        #endregion

        #region ILogService
        public void Info(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            this.AddLogEntry(LogLevel.Info, message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, sourceFilePath, memberName);
        }

        public void Warning(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            this.AddLogEntry(LogLevel.Warning, message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, sourceFilePath, memberName);
        }

        public void Error(string message, string arg1 = "", string arg2 = "", string arg3 = "", string arg4 = "", string arg5 = "", string arg6 = "", string arg7 = "", string arg8 = "", [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "")
        {
            this.AddLogEntry(LogLevel.Error, message, new string[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 }, sourceFilePath, memberName);
        }

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
        #endregion
    }
}