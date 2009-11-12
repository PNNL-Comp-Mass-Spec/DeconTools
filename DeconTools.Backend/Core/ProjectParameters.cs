using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    public class ProjectParameters
    {

        public ProjectParameters()
        {
            this.oldDecon2LSParameters = new OldDecon2LSParameters();
            this.ExporterType = Globals.ExporterType.TYPICAL;
            this.numFramesSummed = 1;
            this.numScansSummed = 1;
        }

        private OldDecon2LSParameters oldDecon2LSParameters;

        public OldDecon2LSParameters OldDecon2LSParameters
        {
            get { return oldDecon2LSParameters; }
            set { oldDecon2LSParameters = value; }
        }

        private int numScansSummed;

        public int NumScansSummed
        {
            get { return numScansSummed; }
            set { numScansSummed = value; }
        }
        private int numFramesSummed;

        public int NumFramesSummed
        {
            get { return numFramesSummed; }
            set { numFramesSummed = value; }
        }

        private Globals.ExporterType exporterType;

        public Globals.ExporterType ExporterType
        {
            get { return exporterType; }
            set { exporterType = value; }
        }


    }
}
