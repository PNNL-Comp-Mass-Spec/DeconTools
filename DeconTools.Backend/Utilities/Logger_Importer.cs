using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DeconTools.Backend.Utilities
{
    public class Logger_Importer
    {



        public static void displayLogFile(string logtestfile)
        {
            if (!File.Exists(logtestfile))
            {
                Console.WriteLine("Log file couldn't be loaded. Check path etc.");
                throw new FileNotFoundException("Log file couldn't be loaded. Check path");

            }
            var sr = new StreamReader(logtestfile);

            var sb = new StringBuilder();

            while (sr.Peek()!=-1)
            {
                var currentLine = sr.ReadLine();

                var match= Regex.Match(currentLine, @"(?<date>.+)\tProcessed scan/frame\s+(?<scan>\d+),\s+(?<percentcomplete>[0-9.]+)%\s+complete,\s+(?<features>\d+)");
                if (match.Success)
                {
                    sb.Append(match.Groups["date"].Value);
                    sb.Append("\t");
                    sb.Append(match.Groups["scan"].Value);
                    sb.Append("\t");
                    sb.Append(match.Groups["percentcomplete"].Value);
                    sb.Append("\t");
                    sb.Append(match.Groups["features"].Value);
                    sb.Append(Environment.NewLine);
                }

            }

            Console.Write(sb.ToString());

            sr.Close();
            
        }
    }
}
