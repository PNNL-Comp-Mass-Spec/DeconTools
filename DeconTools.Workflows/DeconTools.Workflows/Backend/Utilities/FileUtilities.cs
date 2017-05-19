
using System.Collections.Generic;
using System.IO;
namespace DeconTools.Workflows.Backend.Utilities
{
    public class FileUtilities
    {

        public static List<string> LoadStringsFromFile(string filename, bool containsHeader=false)
        {
            var list = new List<string>();

            using (var reader = new StreamReader(filename))
            {
                var counter = 0;
                while (reader.Peek() != -1)
                {
                    if (containsHeader)
                    {
                        //skip the header
                        reader.ReadLine();
                    }

                    counter++;
                    var line = reader.ReadLine();
                    list.Add(line);
                }

                reader.Close();
            }

            return list;

        }



        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (var fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


        public static void CopyAll(FileInfo fileToBeCopied, DirectoryInfo targetFolder)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(targetFolder.FullName) == false)
            {
                Directory.CreateDirectory(targetFolder.FullName);
            }

            fileToBeCopied.CopyTo(Path.Combine(targetFolder.FullName, fileToBeCopied.Name), true);

        }
    }
}
