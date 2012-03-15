using System.IO;

namespace DeconTools.Workflows.Backend.Utilities
{
    public class ResultUtilities
    {
        public static void MergeResultFiles(string resultFolder, string outputResultFilename)
        {
            DirectoryInfo resultDirinfo = new DirectoryInfo(resultFolder);

            FileInfo[] resultFiles = resultDirinfo.GetFiles("*_results.txt");

            using (StreamWriter writer = new StreamWriter(outputResultFilename))
            {
                bool headerWasWritten = false;

                foreach (var file in resultFiles)
                {
                    using (StreamReader reader = new StreamReader(file.FullName))
                    {
                        string header = reader.ReadLine(); //header

                        if (!headerWasWritten)
                        {
                            writer.WriteLine(header);
                            headerWasWritten = true;
                        }


                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();
                            writer.WriteLine(line);
                        }


                    }


                }

            }

        }

    }
}
