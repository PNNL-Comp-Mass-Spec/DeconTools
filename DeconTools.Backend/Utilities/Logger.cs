using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DeconTools.Backend.Utilities
{
    public class Logger
    {
        StreamWriter sw;

        public List<LogEntry> LogEntryBuffer { get; set; }     //this contains unwritten logEngties  (i.e. once entries are written to disk, they are flushed)

        private List<LogEntry> logEntries;    //this contains all the logEntries made during this logging session

        private static Logger instance;


        public static Logger Instance
        {
            get
            {
                if (instance == null) instance = new Logger();
                return instance;
            }
            set
            {
                instance = value;
            }

        }

        private string m_outputFilename;

        public string OutputFilename
        {
            get { return m_outputFilename; }
            set { m_outputFilename = value; }
        }


        private Logger()
        {
            LogEntryBuffer = new List<LogEntry>();
            logEntries = new List<LogEntry>();
        }

        public struct LogEntry
        {
            public DateTime LogTime;
            public string LogDescription;
        }

        public DateTime TimeOfLastUpdate { get; set; }


        public void AddEntry(string desc)
        {
            LogEntry entry = new LogEntry();
            entry.LogTime = DateTime.Now;
            entry.LogDescription = desc;
            this.LogEntryBuffer.Add(entry);
            this.logEntries.Add(entry);
            TimeOfLastUpdate = DateTime.Now;
            
        }

        public void AddEntry(string desc, string outputFilename)
        {
            this.AddEntry(desc);
            WriteToFile(outputFilename, false);
            this.LogEntryBuffer.Clear();         // since they were already written out to file. will clear them
        }

        public void Close()
        {
            if (sw == null) return;
            sw.Close();
            instance = null;
            sw = null;
        }



        public TimeSpan GetTimeDifference(string string1, string string2)
        {
            LogEntry logentry1 = logEntries.Find(delegate(LogEntry entry) { return entry.LogDescription == string1; });
            LogEntry logentry2 = logEntries.Find(delegate(LogEntry entry) { return entry.LogDescription == string2; });

            if (logentry1.LogDescription == null || logentry2.LogDescription == null)
            {
                return new TimeSpan(-1);
            }

            TimeSpan span = logentry2.LogTime.Subtract(logentry1.LogTime);
            return span;



        }

        public void Display()
        {
            foreach (LogEntry entry in LogEntryBuffer)
            {
                Console.WriteLine(entry.LogTime.ToString() + "\t" + entry.LogDescription);

            }
        }



        public void WriteToFile(string outputfilename, bool closeAfterWriting)
        {
            try
            {
                if (sw == null)
                {
                    sw = new StreamWriter(new System.IO.FileStream(outputfilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read));
                    sw.AutoFlush = true;
                }

                foreach (LogEntry entry in LogEntryBuffer)
                {
                    sw.WriteLine(entry.LogTime.ToString() + "\t" + entry.LogDescription);

                }
                if (closeAfterWriting) sw.Close();

            }
            catch (Exception)
            {


            }






        }




    }
}
