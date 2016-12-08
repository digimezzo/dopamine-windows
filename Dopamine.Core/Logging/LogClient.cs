using System;
using NLog;
using NLog.Config;
using NLog.Targets;
using Dopamine.Core.Settings;
using Dopamine.Core.IO;
using System.Text;

namespace Dopamine.Core.Logging
{
    public class LogClient : ILogClient
    {
        #region Variables
        private static LogClient instance;
        private string logFile;
        #endregion

        #region Properties
        public Logger Logger { get; set; }

        public string LogFile
        {
            get { return this.logFile; }
            set { this.logFile = value; }
        }
        #endregion

        #region Construction
        private LogClient()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties
            this.logFile = System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.LogFolder, ApplicationPaths.LogFile);

            fileTarget.FileName = this.logFile;
            fileTarget.ArchiveFileName = System.IO.Path.Combine(XmlSettingsClient.Instance.ApplicationFolder, ApplicationPaths.LogFolder, ApplicationPaths.LogArchiveFile);
            fileTarget.Layout = "${longdate}|${level}|${callsite}|${message}";
            fileTarget.ArchiveAboveSize = 5242880;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Rolling;
            fileTarget.MaxArchiveFiles = 3;

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            NLog.LogManager.Configuration = config;

            // Create the logger
            this.Logger = NLog.LogManager.GetCurrentClassLogger();
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
        #endregion

        #region Public
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
        #endregion
    }
}
