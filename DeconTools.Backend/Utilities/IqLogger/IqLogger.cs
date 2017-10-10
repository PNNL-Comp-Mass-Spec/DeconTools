using System;
using System.IO;
using System.Linq;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Core;


namespace DeconTools.Backend.Utilities.IqLogger
{
    public static class IqLogger
    {
        static IqLogger()
        {
            //Sets up log4net
            //Make sure app.config is setup as well
            SetupLoggingLevels();
            log4net.Config.XmlConfigurator.Configure();
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            //get the current logging repository for this application
            var repository = LogManager.GetRepository();

            //get all of the appenders for the repository
            var appenders = repository.GetAppenders().ToList();

            //only change the file path on the 'FileAppenders'
            //sets default path to the current working directory

            //only change the file path on the 'FileAppenders'
            foreach (var appender in appenders)
            {
                var isFileAppender = appender is FileAppender;

                if (!isFileAppender) continue;

                var fileAppender = (FileAppender)appender;
                //set the path to your LogDirectory
                fileAppender.File = Path.Combine(Environment.CurrentDirectory, fileAppender.Name + ".txt");
                //make sure to call fileAppender.ActivateOptions() to notify the logging of changes
                fileAppender.ActivateOptions();
            }



        }

        public static readonly ILog Log;

        public static string LogDirectory { get; set; }

        /// <summary>
        /// Serves as a globally accessible counter (for debugging purposes only!)
        /// </summary>
        public static long Counter1 { get; set; }

        private static readonly Level SamPayne = new Level(0, "SamPayne");

        private static readonly Level DebugChannel1 = new Level(29999, "DebugChannel1");

        private static readonly Level DebugChannel2 = new Level(29998, "DebugChannel2");

        private static readonly Level DebugChannel3 = new Level(29997, "DebugChannel3");


        /// <summary>
        /// Initializes the standard IqLog file.
        /// </summary>
        /// <param name="datasetName"></param>
        public static void InitializeIqLog (string datasetName)
        {
            //Sets up the default IqLog.txt file
            ChangeLogLocation("IqLog", LogDirectory, datasetName + "_IqLog.txt");
        }

        /// <summary>
        /// Changes the log location / log file name of the desired appender
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        public static void ChangeLogLocation(string name, string path, string filename)
        {
            //get the current logging repository for this application
            var repository = LogManager.GetRepository();

            //get all of the appenders for the repository
            var appenders = repository.GetAppenders();

            //only change the file path on the 'FileAppenders'
            foreach (var appender in appenders)
            {
                var isFileAppender = appender is FileAppender;

                if (!isFileAppender) continue;

                var fileAppender = (FileAppender)appender;
                //set the path to your LogDirectory
                fileAppender.File = Path.Combine(path, filename);
                //make sure to call fileAppender.ActivateOptions() to notify the logging of changes
                fileAppender.ActivateOptions();
            }


        }

        /// <summary>
        /// Log level between Finest and All
        /// Extremely Verbose!
        /// </summary>
        /// <param name="message"></param>
        public static void SamPayneLog(string message)
        {
            Log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, SamPayne, message, null);
        }

        /// <summary>
        /// DebugChannel1 is between Info and Debug
        /// </summary>
        /// <param name="message"></param>
        public static void DebugChannel1Log(string message)
        {
            Log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DebugChannel1, message, null);
        }


        /// <summary>
        /// DebugChannel2 is between Info and Debug
        /// </summary>
        /// <param name="message"></param>
        public static void DebugChannel2Log(string message)
        {
            Log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DebugChannel2, message, null);
        }


        /// <summary>
        /// DebugChannel3 is between Info and Debug
        /// </summary>
        /// <param name="message"></param>
        public static void DebugChannel3Log(string message)
        {
            Log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DebugChannel3, message, null);
        }


        private static void SetupLoggingLevels()
        {
            LogManager.GetRepository().LevelMap.Add(SamPayne);
            LogManager.GetRepository().LevelMap.Add(DebugChannel1);
            LogManager.GetRepository().LevelMap.Add(DebugChannel2);
            LogManager.GetRepository().LevelMap.Add(DebugChannel3);
        }
    }
}
