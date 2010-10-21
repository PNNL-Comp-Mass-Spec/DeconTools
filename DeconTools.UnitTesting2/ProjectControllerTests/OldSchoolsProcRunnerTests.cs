using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NUnit.Framework;
using DeconTools.Backend;

namespace DeconTools.UnitTesting2.ProjectControllerTests
{
    [TestFixture]
    public class OldSchoolsProcRunnerTests
    {
        [Test]
        public void processYAFMSFile1()
        {
            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(FileRefs.RawDataMSFiles.YAFMSStandardFile2, Globals.MSFileType.YAFMS, FileRefs.ParameterFiles.YAFMSParameterFileScans4000_4050);
            oldSchool.Execute();
        }


        [Test]
        public void processBruker12TSolarixFile1()
        {
            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(FileRefs.RawDataMSFiles.BrukerSolarix12TFile1, Globals.MSFileType.Bruker_V2, FileRefs.ParameterFiles.Bruker12TSolarixScans4_8ParamFile);
            oldSchool.Execute();
        }



     

    }
}
