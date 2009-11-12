using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using System.IO;

namespace DeconTools.Backend.ProcessingTasks.PeakListExporters
{
    public abstract class IPeakListExporter : Task
    {

        private StreamWriter outputStream;

        public StreamWriter OutputStream
        {
            get { return outputStream; }
            set { outputStream = value; }
        }

        public abstract void WriteToStream(ResultCollection resultList); 

        public override void Execute(ResultCollection resultList)
        {
            WriteToStream(resultList);
        }

        public override void Cleanup()
        {
            try
            {
                outputStream.Close();
            }
            catch (Exception)
            {
                
            }

            outputStream = null;
        }

    }
}
