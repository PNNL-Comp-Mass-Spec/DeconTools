using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.QCTesting
{
    public class TimeResult
    {


        public double SecondsElapsed { get; set; }

        public DateTime TimeOfEntry { get; set; }

        public string Description { get; set; }

        public TimeResult()
        {

        }

        public TimeResult(DateTime timeOfEntry,  double secondsElapsed, string description)
        {
            this.SecondsElapsed = secondsElapsed;
            this.TimeOfEntry = timeOfEntry;
            this.Description = description;
        }

        public override string ToString()
        {
            string outstring = TimeOfEntry.ToLongTimeString() + "\t" + Description + "\t" + SecondsElapsed.ToString("0.0");
            return outstring;
        }
  
    }
}
