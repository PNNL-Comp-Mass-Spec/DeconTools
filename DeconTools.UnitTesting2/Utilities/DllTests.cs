using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class DllTests
    {
        [Test]
        [Ignore("Obsolete")]
        public void checkDeconEngineDllsInAllProjects()
        {
            var baseFolder = @"..\..\..\..";

            var hintPath = @"..\Library\DeconEngineV2.dll";
            var specificVersion = "False";

            var outputDirectoryPath = @"C:\temp\DeconToolsDllTesting";
            if (!Directory.Exists(outputDirectoryPath))
                Directory.CreateDirectory(outputDirectoryPath);

            var directoryInfo = new DirectoryInfo(baseFolder);
            if (directoryInfo.Exists)
            {
                var fileList = GetFileList("*.csproj", baseFolder);

                foreach (var file in fileList)
                {
                    Console.WriteLine();
                    Console.WriteLine("Opening file " + file.FullName);

                    var xml = XDocument.Load(File.OpenRead(file.FullName));

                    //if (!file.Name.Contains("Backend.csproj")) continue;

                    XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

                    var buildNode = xml.Element(msbuild + "Project");

                    Assert.NotNull(buildNode, "Could not get node '" + msbuild + "Project' in the XML");

                    var itemGroup = buildNode.Elements(msbuild + "ItemGroup").
                        FirstOrDefault(x => x.Descendants().Any(y => y.Name == msbuild + "Reference"));

                    Assert.NotNull(itemGroup, "Could not find a match to '" + msbuild + "Reference'");

                    var referenceItems = itemGroup.Descendants().Where(y => y.Name == msbuild + "Reference");

                    foreach (var referenceItem in referenceItems)
                    {
                        var xAttribute = referenceItem.Attribute("Include");
                        if (xAttribute?.Value.Contains("DeconEngineV2") == true)
                        {
                            Console.WriteLine("\t\t\t" + xAttribute.Value);

                            var hintPathElement = referenceItem.Descendants().FirstOrDefault(y => y.Name == msbuild + "HintPath");
                            var specificVersionElement = referenceItem.Descendants().FirstOrDefault(y => y.Name == msbuild + "SpecificVersion");

                            if (specificVersionElement != null)
                            {
                                Console.WriteLine("\t\t\tSpecific version= \t" + specificVersionElement.Value);
                                specificVersionElement.Value = specificVersion;
                            }

                            if (hintPathElement != null)
                            {
                                Console.WriteLine("\t\t\tHintPath = \t" + hintPathElement.Value);
                                hintPathElement.Value = hintPath;
                            }
                        }
                    }
                }

                //foreach (var file in fileList)
                //{
                //    string newProjectFile = Path.Combine(outputDirectoryPath, Path.GetFileName(file));

                //    File.Copy(newProjectFile, file, true);

                //}

            }
        }

        public static List<FileInfo> GetFileList(string fileSearchPattern, string rootDirectoryPath, bool includeSubdirectories = true)
        {
            var directory = new DirectoryInfo(rootDirectoryPath);
            if (!directory.Exists)
                return new List<FileInfo>();

            if (includeSubdirectories)
                return directory.GetFiles(fileSearchPattern, SearchOption.AllDirectories).ToList();

            return directory.GetFiles(fileSearchPattern, SearchOption.TopDirectoryOnly).ToList();
        }
    }
}
