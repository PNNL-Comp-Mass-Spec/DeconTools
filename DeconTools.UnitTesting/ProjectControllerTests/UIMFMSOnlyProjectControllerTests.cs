using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Core;

using System.ComponentModel;
using DeconTools.Backend.ProjectControllers;

namespace DeconTools.UnitTesting.ProjectControllerTests
{
    public class UIMFMSOnlyProjectControllerTests
    {
        string uimfTestfile1 = "..\\..\\TestFiles\\35min_QC_Shew_Formic_4T_1.8_500_20_30ms_fr1950_0000.uimf";

        string parameterFile1 = "..\\..\\TestFiles\\uimfParams_forReading_200Frames_MSonly.xml";

        [Test]
        public void test1()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;

            DeconTools.Backend.ProjectControllers.UIMFMSOnlyProjectController projectController = new UIMFMSOnlyProjectController(uimfTestfile1, DeconTools.Backend.Globals.MSFileType.PNNL_UIMF, parameterFile1, bw);
            projectController.Execute();
        }


    }
}
