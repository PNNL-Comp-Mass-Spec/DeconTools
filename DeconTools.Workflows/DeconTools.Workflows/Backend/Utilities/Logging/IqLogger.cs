using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Repository;

namespace DeconTools.Workflows.Backend.Utilities.Logging
{
	public static class IqLogger
	{
		static IqLogger()
		{
			//Sets up log4net
			//Make sure app.config is setup as well
			log4net.Config.XmlConfigurator.Configure();
			Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		}

		public static readonly ILog Log;

		public static string LogDirectory { get; set; }

		public static void Initialize(string datasetName)
		{


			//get the current logging repository for this application
			ILoggerRepository repository = LogManager.GetRepository();

			//get all of the appenders for the repository
			IAppender[] appenders = repository.GetAppenders();

			//only change the file path on the 'FileAppenders'
			foreach (FileAppender fileAppender in appenders)
			{
				//set the path to your LogDirectory
				fileAppender.File = Path.Combine(LogDirectory, datasetName + "_Iqlog.txt");
				//make sure to call fileAppender.ActivateOptions() to notify the logging of changes
				fileAppender.ActivateOptions();
			}
		}
	}
}
