using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;


namespace DeconTools.UnitTesting.ProjectControllerTests
{
    [TestFixture]
    public class RunMergingProjectControllerTests
    {
        string c2_blankfilePath = "..\\..\\TestFiles\\MergeTestFiles\\Blank C2 MeOH 50-1000 pos FTMS.raw";
        string c2_6FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-6 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_7FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-7 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_8FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-8 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_9FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-9 Stock MeOH 50-1000 pos FTMS.raw";
        string c2_10FilePath = "..\\..\\TestFiles\\MergeTestFiles\\C2-10 Stock MeOH 50-1000 pos FTMS.raw";

        public string xcaliburParameterFile = "..\\..\\TestFiles\\xcaliburParameterFile2.xml";


        [Test]
        public void test1()
        {
            List<string>inputfileNames=new List<string>();
            inputfileNames.Add(c2_blankfilePath);
            inputfileNames.Add(c2_6FilePath);
            inputfileNames.Add(c2_7FilePath);
            inputfileNames.Add(c2_8FilePath);
            inputfileNames.Add(c2_9FilePath);
            inputfileNames.Add(c2_10FilePath);

            ProjectController runner = new RunMergingProjectController(inputfileNames, DeconTools.Backend.Globals.MSFileType.Finnigan, xcaliburParameterFile);
            runner.Execute();
        }


    }
}
