using System;
using System.Collections.Generic;
using DeconTools.Backend.Core;

namespace DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders
{
    public class N14N15ResultObject
    {

        public string  DatasetName { get; set; }

        public PeptideTarget MassTag { get; set; }

        public List<XYData> ChromListUnlabeled { get; set; }
        public List<XYData> ChromListLabeled { get; set; }

        public List<Peak> ChromPeakSelectedUnlabeled { get; set; }
        public List<Peak> ChromPeakSelectedLabeled { get; set; }


        public N14N15ResultObject(string datasetName, PeptideTarget mt)
        {
            DatasetName = datasetName;
            MassTag = mt;

            ChromListLabeled = new List<XYData>();
            ChromListUnlabeled = new List<XYData>();
            ChromPeakSelectedLabeled = new List<Peak>();
            ChromPeakSelectedUnlabeled = new List<Peak>();

        }


        internal void DisplaySelectedChromPeaks(List<Peak> chromPeakList)
        {

            foreach (var item in chromPeakList)
            {
                if (item != null)
                {
                    Console.WriteLine(item.XValue.ToString("0.000") + "\t" + item.Height.ToString("0") + "\t" + item.Width.ToString("0.0000"));
                }
            }
        }
    }
}
