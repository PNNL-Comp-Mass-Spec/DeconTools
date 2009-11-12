using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DeconTools.Backend.Core;
using System.Runtime.Serialization.Formatters.Binary;

namespace DeconTools.Backend.Data
{
    public class IsosResultSerializer
    {
        private FileMode filemode;

        public IsosResultSerializer(string outputFilename, FileMode mode, bool deletePrevious)
        {
            
            this.outputFilename = outputFilename;
            this.filemode = mode;

            try
            {
                if (deletePrevious)
                {
                    if (File.Exists(outputFilename))
                    {
                        File.Delete(outputFilename);
                    }
                } 
                stream = File.Open(outputFilename, filemode);

            }
            catch (Exception ex)
            {
                throw new System.IO.IOException("Could not create temporary binary file for storing results. Details: " + ex.Message);
            }
        }

        private Stream stream;
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        private string outputFilename;
        public string OutputFilename
        {
            get { return outputFilename; }
            set { outputFilename = value; }
        }

        public void Serialize(ResultCollection resultCollection)
        {
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(stream, resultCollection);
        }

        public void Close()
        {
            stream.Close();
        }

       
    }
}
