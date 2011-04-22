using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using UIMFLibrary;

namespace DeconTools.Testing.ProblemCases
{
    public class UIMFRunTester
    {
        public void DisplayBasicRunInfo(Backend.Runs.UIMFRun run)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("field\tvalue\n");
            sb.Append("filename\t" + run.Filename + "\n");
            sb.Append("filetype\t" + run.MSFileType + "\n");
            sb.Append("minFrame\t" + run.MinFrame + "\n");
            sb.Append("maxFrame\t" + run.MaxFrame + "\n");
            sb.Append("minScan\t" + run.MinScan + "\n");
            sb.Append("maxScan\t" + run.MaxScan + "\n");
            Console.WriteLine(sb.ToString());

        }



        public void GenerateMassSpectraForRanges(Backend.Runs.UIMFRun run, int startFrame, int stopFrame, int startScan, int stopScan)
        {
            StringBuilder sb = new StringBuilder();

            FrameSetCollectionCreator fscc = new FrameSetCollectionCreator(run, startFrame, stopFrame, 1, 1);
            fscc.Create();

            ScanSetCollectionCreator sscc = new ScanSetCollectionCreator(run, startScan, stopScan, 1, 1);
            sscc.Create();

            UIMF_MSGenerator msgen = new UIMF_MSGenerator();


            foreach (var frame in run.FrameSetCollection.FrameSetList)
            {

                run.CurrentFrameSet = frame;

                foreach (var scan in run.ScanSetCollection.ScanSetList)
                {
                    run.CurrentScanSet = scan;

                    sb.Append(frame.PrimaryFrame);
                    sb.Append("\t");

                    sb.Append(scan.PrimaryScanNumber);
                    sb.Append("\t");
                    try
                    {
                        msgen.Execute(run.ResultCollection);

                        sb.Append("Passed");

                    }
                    catch (Exception ex)
                    {
                        sb.Append("FAILED. \t" + ex.Message);
                    }

                    sb.Append(Environment.NewLine);



                }



            }
            Console.WriteLine(sb.ToString());







        }

        public void DisplayFrameParameters(UIMFRun run, int startFrame, int stopFrame)
        {
            StringBuilder sb = new StringBuilder();

            UIMFLibrary.DataReader dr = new DataReader();
            dr.OpenUIMF(run.Filename);


            FrameParameters fp;

            sb.Append("frame\tnumScans\tpressureFront\tpressureBack\tTOFLength\tCalIntercept\tCalSlope\tFrameNum\tFrameType\n");

            for (int frame = startFrame; frame <= stopFrame; frame++)
            {
                sb.Append(frame);
                sb.Append("\t");


                fp = dr.GetFrameParameters(frame);

                sb.Append(fp.Scans);
                sb.Append("\t");

                sb.Append(fp.PressureFront);
                sb.Append("\t");
                sb.Append(fp.PressureBack);
                sb.Append("\t");

                sb.Append(fp.AverageTOFLength);
                sb.Append("\t");
                sb.Append(fp.CalibrationIntercept);
                sb.Append("\t");
                sb.Append(fp.CalibrationSlope);
                sb.Append("\t");

                sb.Append(fp.FrameNum);
                sb.Append("\t");
                sb.Append(fp.FrameType);
                sb.Append(Environment.NewLine);




            }

            Console.WriteLine(sb.ToString());





        }
    }
}
