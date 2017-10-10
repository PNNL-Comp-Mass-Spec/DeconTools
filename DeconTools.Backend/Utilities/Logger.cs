using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DeconTools.Backend.Utilities
{
    public class Logger
    {
        public List<LogEntry> LogEntryBuffer { get; set; }     //this contains unwritten logEntries  (i.e. once entries are written to disk, they are flushed)

        private readonly List<LogEntry> LogEntries;    //this contains all the logEntries made during this logging session

        private static Logger instance;

        public static Logger Instance => instance ?? (instance = new Logger());

        public string OutputFilename { get; set; }


        private Logger()
        {
            LogEntryBuffer = new List<LogEntry>();
            LogEntries = new List<LogEntry>();
        }

        public struct LogEntry
        {
            public DateTime LogTime;
            public string LogDescription;

            public override string ToString()
            {
                return LogDescription;
            }
        }

        public DateTime TimeOfLastUpdate { get; set; }

        /// <summary>
        /// Add a message to log, optionally flushing cached messages to the log ifle
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="writeCachedEntriesToDisk"></param>
        public void AddEntry(string desc, bool writeCachedEntriesToDisk = false)
        {
            var entry = new LogEntry
            {
                LogTime = DateTime.Now,
                LogDescription = desc
            };

            LogEntryBuffer.Add(entry);
            LogEntries.Add(entry);
            TimeOfLastUpdate = DateTime.Now;

            if (!writeCachedEntriesToDisk)
                return;

            FlushLogEntries(Instance.OutputFilename);

        }

        /// <summary>
        /// Flush the log to the specified file
        /// </summary>
        /// <param name="desc"></param>
        /// <param name="outputFilename"></param>
        public void AddEntry(string desc, string outputFilename)
        {
            AddEntry(desc);

            // Flush entries in LogEntryBuffer to disk
            FlushLogEntries(outputFilename);
        }

        public void Close()
        {
            instance = null;
        }

        /// <summary>
        /// Flush entries in LogEntryBuffer to console or disk
        /// </summary>
        /// <param name="outputFile"></param>
        private void FlushLogEntries(string outputFile)
        {
            if (string.IsNullOrWhiteSpace(outputFile))
            {
                foreach (var item in LogEntryBuffer)
                    Console.WriteLine(item);
            }
            else
            {
                WriteToFile(outputFile);
            }

            LogEntryBuffer.Clear();
        }

        public TimeSpan GetTimeDifference(string string1, string string2)
        {
            var logentry1 = LogEntries.Find(entry => entry.LogDescription == string1);
            var logentry2 = LogEntries.Find(entry => entry.LogDescription == string2);

            if (logentry1.LogDescription == null || logentry2.LogDescription == null)
            {
                return new TimeSpan(-1);
            }

            var span = logentry2.LogTime.Subtract(logentry1.LogTime);
            return span;



        }

        public void Display()
        {
            foreach (var entry in LogEntryBuffer)
            {
                Console.WriteLine(entry.LogTime.ToString(CultureInfo.InvariantCulture) + "\t" + entry.LogDescription);
            }
        }

        public void WriteToFile(string outputfilename)
        {
            using (var sw = new StreamWriter(new FileStream(outputfilename, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                foreach (var entry in LogEntryBuffer)
                {
                    sw.WriteLine(entry.LogTime.ToString(CultureInfo.InvariantCulture) + "\t" + entry.LogDescription);
                }
            }

        }

    }
}
