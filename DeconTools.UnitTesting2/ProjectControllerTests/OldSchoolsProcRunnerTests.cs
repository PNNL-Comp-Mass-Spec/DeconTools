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
            OldSchoolProcRunner oldSchool = new OldSchoolProcRunner(FileRefs.YAFMSStandardFile1, Globals.MSFileType.YAFMS, FileRefs.ParameterFiles.YAFMSParameterFileScans6000_6050);
            oldSchool.Execute();



        }


    }
}
