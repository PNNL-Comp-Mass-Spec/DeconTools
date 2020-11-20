using System.IO;

namespace DeconTools.Workflows.Backend.Utilities
{
    public class ResultUtilities
    {
        public static void MergeResultFiles(string resultFolder, string outputResultFilename)
        {
            var resultDirinfo = new DirectoryInfo(resultFolder);

            var resultFiles = resultDirinfo.GetFiles("*_results.txt");

            using (var writer = new StreamWriter(outputResultFilename))
            {
                var headerWasWritten = false;

                foreach (var file in resultFiles)
                {
                    using (var reader = new StreamReader(file.FullName))
                    {
                        var header = reader.ReadLine(); //header

                        if (!headerWasWritten)
                        {
                            writer.WriteLine(header);
                            headerWasWritten = true;
                        }

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            writer.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
