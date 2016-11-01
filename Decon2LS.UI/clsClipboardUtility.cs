using System;
using System.Data;
using System.Windows.Forms;

namespace Decon2LS
{
    /// <summary>
    /// Summary description for clsClipboardUtility.
    /// </summary>
    public class clsClipboardUtility
    {
        public clsClipboardUtility()
        {
                        
        }

        public static void CopyXYValuesToClipboard(double[] xvals, double[] yvals)
        {
            
            int maxLength=0;
            if (xvals.Length == 0 || yvals.Length==0)return;
            
            if (xvals.Length>=yvals.Length)	maxLength=yvals.Length;
            else maxLength=xvals.Length;

            System.Text.StringBuilder sb=new System.Text.StringBuilder();


            for (int i=0;i<maxLength;i++)
            {
                sb.Append(xvals[i]);
                sb.Append("\t");
                sb.Append(yvals[i]);
                sb.Append(Environment.NewLine);
            }

            if (sb.ToString()==null || sb.ToString().Length==0)return;

            System.Windows.Forms.IDataObject dataobject = new DataObject();
            dataobject.SetData(DataFormats.Text,true,sb.ToString());
            System.Windows.Forms.Clipboard.SetDataObject(dataobject,true);
        }



        public static void CopyXYValuesToClipboard(float[] xvals, float[] yvals)
        {
            if (xvals==null || yvals==null)return;
            if (xvals.Length==0 || yvals.Length==0)return;
            
            double[]convertedxvals=new double[xvals.Length];
            double[]convertedyvals=new double[yvals.Length];
            
            xvals.CopyTo(convertedxvals,0);
            yvals.CopyTo(convertedyvals,0);

            clsClipboardUtility.CopyXYValuesToClipboard(convertedxvals,convertedyvals);
        }

    }
}
