using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Utilities
{
    [TestFixture]
    public class DllTests
    {
        [Test]
        public void checkDeconEngineDllsInAllProjects()
        {
            string baseFolder = @"..\\..\\..\\..\\..\\DeconTools";

            string deconEngineVersion = "DeconEngineV2, Version=1.0.4724.25548, Culture=neutral, processorArchitecture=x86";
            string hintPath = @"..\Library\DeconEngineV2.dll";
            string specificVersion = "False";

            string outputFolder = @"d:\temp\DeconToolsDllTesting";
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            DirectoryInfo directoryInfo = new DirectoryInfo(baseFolder);
            if (directoryInfo.Exists)
            {
                var fileList=  GetFileList("*.csproj", baseFolder);

                foreach (var file in fileList)
                {
                    //Console.WriteLine(file);

                    XDocument xDocument = new XDocument();
                    XDocument xml = XDocument.Load(File.OpenRead(file));

                    //if (!file.Contains("Backend.csproj")) continue;

                    XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
                    


                    var itemGroup =xml.Element(msbuild + "Project")
                        .Elements(msbuild + "ItemGroup").FirstOrDefault(x => x.Descendants().Any(y => y.Name == msbuild + "Reference"));

                    var referenceItems = itemGroup.Descendants().Where(y => y.Name == msbuild + "Reference");

                    Console.WriteLine(Path.GetFileName(file));
                    foreach (XElement referenceItem in referenceItems)
                    {
                        var xAttribute = referenceItem.Attribute("Include");
                        if (xAttribute != null && xAttribute.Value.Contains("DeconEngineV2"))
                        {

                            Console.WriteLine("\t\t\t"+ xAttribute.Value);
                            xAttribute.Value = deconEngineVersion;

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


                        //string outputProjectFilename = Path.Combine(outputFolder, Path.GetFileName(file));
                        //StringBuilder sb=new StringBuilder();

                        //XmlWriterSettings xws = new XmlWriterSettings();
                        
                        //xws.Indent = true;

                        //using (XmlWriter xw= XmlWriter.Create(outputProjectFilename,xws))
                        //{

                        //    xml.WriteTo(xw);
                        //}


                       

                    }



               

                }



                //foreach (var file in fileList)
                //{
                //    string newProjectFile = Path.Combine(outputFolder, Path.GetFileName(file));

                //    File.Copy(newProjectFile, file, true);


                //}

            }

        }





        public static IEnumerable<string> GetFileList(string fileSearchPattern, string rootFolderPath)
        {
            Queue<string> pending = new Queue<string>();
            pending.Enqueue(rootFolderPath);
            string[] tmp;
            while (pending.Count > 0)
            {
                rootFolderPath = pending.Dequeue();
                tmp = Directory.GetFiles(rootFolderPath, fileSearchPattern);
                for (int i = 0; i < tmp.Length; i++)
                {
                    yield return tmp[i];
                }
                tmp = Directory.GetDirectories(rootFolderPath);
                for (int i = 0; i < tmp.Length; i++)
                {
                    pending.Enqueue(tmp[i]);
                }
            }
        }

    }
}
