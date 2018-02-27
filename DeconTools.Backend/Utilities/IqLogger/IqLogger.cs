using System;
using System.IO;
using PRISM.Logging;
using PRISM;

namespace DeconTools.Backend.Utilities.IqLogger
{
    public static class IqLogger
    {
        static IqLogger()
        {

            Log = new FileLogger {
                LogLevel = BaseLogger.LogLevels.INFO
            };

            var baseName = Path.Combine(Environment.CurrentDirectory, "IqLog");
            FileLogger.ChangeLogFileBaseName(baseName, false);
        }

        private static readonly FileLogger Log;

        public static string LogDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Set to True to enable verbose logging
        /// </summary>
        public static bool VerboseLogging { get; set; } = false;

        /// <summary>
        /// Immediately write out any queued messages
        /// </summary>
        public static void FlushPendingMessages()
        {
            FileLogger.FlushPendingMessages();
        }

        /// <summary>
        /// Initializes the standard IqLog file for the given dataset
        /// </summary>
        /// <param name="datasetName"></param>
        /// <param name="logDirectory"></param>
        public static void InitializeIqLog (string datasetName, string logDirectory)
        {
            if (string.IsNullOrWhiteSpace(logDirectory))
                LogDirectory = string.Empty;
            else
                LogDirectory = logDirectory;

            ChangeLogLocation(LogDirectory, datasetName, "_IqLog.txt");
        }

        /// <summary>
        /// Changes the log location / log file name of the desired appender
        /// </summary>
        /// <param name="logDirectoryPath"></param>
        /// <param name="logFileNamePrefix"></param>
        /// <param name="logFileNameSuffix"></param>
        private static void ChangeLogLocation(string logDirectoryPath, string logFileNamePrefix, string logFileNameSuffix)
        {
            var baseName = Path.Combine(logDirectoryPath, logFileNamePrefix + logFileNameSuffix);
            FileLogger.ChangeLogFileBaseName(baseName, false);
        }

        /// <summary>
        /// Verbose logging; not shown at the console
        /// </summary>
        /// <param name="message"></param>
        public static void LogTrace(string message)
        {
            if (!VerboseLogging)
                return;

            if (Log.LogLevel < BaseLogger.LogLevels.DEBUG)
                Log.LogLevel = BaseLogger.LogLevels.DEBUG;
            Log.Debug(message);
        }

        /// <summary>
        /// Log a debug message, level Info
        /// </summary>
        /// <param name="message"></param>
        public static void LogDebug(string message)
        {
            Log.Debug(message);
            ConsoleMsgUtils.ShowDebug(message);
        }

        /// <summary>
        /// Log an errr message
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ex"></param>
        public static void LogError(string message, Exception ex = null)
        {
            Log.Error(message, ex);
            ConsoleMsgUtils.ShowError(message, ex);
        }

        /// <summary>
        /// Log a message, level Info
        /// </summary>
        /// <param name="message"></param>
        public static void LogMessage(string message)
        {
            Log.Info(message);
            Console.WriteLine(message);
        }


        /// <summary>
        /// Log a message, level warn
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            Log.Warn(message);
            ConsoleMsgUtils.ShowWarning(message);
        }
    }
}
