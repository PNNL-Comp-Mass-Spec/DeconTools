using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DeconTools.Backend.Utilities;

namespace DeconTools.UnitTesting.UtilitiesTests
{
    public class UIMFCalibrationConstantsUpdaterTests
    {

        double calSlope;
        double calIntercept;
        double a2, b2, c2, d2, e2, f2;

        float tofCorrectionTime;

        string testUIMFFile1 = @"F:\Gord\Data\UIMF\FPGA\2010_05_13\Calibration_Adjusted\agilent_3ppp_700_100_fr5_th0.db";



        [Test]
        public void updateTOFCorrectionTimeTest1()
        {
            tofCorrectionTime = 29.6F;

            UIMFCalibrationConstantUpdater.UpdateUIMFFileWithTOFCorrectionTime(testUIMFFile1, tofCorrectionTime);


        }



        [Test]
        public void applyCalibratedSlopeAndInterceptOnly()
        {

            calSlope = 0.000573144354932388 * 1000;
            calIntercept = 1028.59383405109 / 1000;

            a2 = 0;
            b2 = 0;
            c2 = 0;
            d2 = 0;
            e2 = 0;
            f2 = 0;

            //a2 = 1.53872744761647E-07;
            //b2 = -2.16087414874342E-16;
            //c2 = 8.6147727194931E-26;
            //d2 = -1.22495401570348E-35;
            //e2 = 5.14744461619862E-46;
            //f2 = 0;


            UIMFCalibrationConstantUpdater.UpdateUIMFFileWithCalibrationConstants(testUIMFFile1, calSlope, calIntercept, a2, b2, c2, d2, e2, f2);
        }


        [Test]
        public void applyCalibratedSlopeInterceptAndPolyNomials()
        {

            calSlope = 0.000573144354932388 * 1000;
            calIntercept = 1028.59383405109 / 1000;

            a2 = 1.538727000E-04;
            b2 = -2.160874000E-07;
            c2 = 8.614773000E-11;
            d2 = -1.224954000E-14;
            e2 = 5.147445000E-19;
            f2 = 0;


            //a2 = 1.53872744761647E-07;
            //b2 = -2.16087414874342E-16;
            //c2 = 8.6147727194931E-26;
            //d2 = -1.22495401570348E-35;
            //e2 = 5.14744461619862E-46;
            //f2 = 0;


            UIMFCalibrationConstantUpdater.UpdateUIMFFileWithCalibrationConstants(testUIMFFile1, calSlope, calIntercept, a2, b2, c2, d2, e2, f2);
        }



        [Test]
        public void applyOriginalConstants()
        {

            calSlope = 0.573063492774963;
            calIntercept = 1.02438902854919;

            a2 = 0;
            b2 = 0;
            c2 = 0;
            d2 = 0;
            e2 = 0;
            f2 = 0;

    

            UIMFCalibrationConstantUpdater.UpdateUIMFFileWithCalibrationConstants(testUIMFFile1, calSlope, calIntercept, a2, b2, c2, d2, e2, f2);
        }

        private void getCalibrationConstants()
        {

            calSlope = 0.000573144354932388 * 1000;
            calIntercept = 1028.59383405109 / 1000;
            a2 = 1.53872744761647E-07;
            b2 = -2.16087414874342E-16;
            c2 = 8.6147727194931E-26;
            d2 = -1.22495401570348E-35;
            e2 = 5.14744461619862E-46;
            f2 = 0;


        }



    }
}
