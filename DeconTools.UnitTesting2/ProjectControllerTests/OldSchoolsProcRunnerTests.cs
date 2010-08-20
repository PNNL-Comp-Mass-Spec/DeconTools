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


    }
}
