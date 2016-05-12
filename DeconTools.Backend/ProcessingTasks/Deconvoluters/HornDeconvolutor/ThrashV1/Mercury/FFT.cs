using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.Mercury
{
    internal static class FFT
    {
        private const double Pi = 3.14159265358979323846;
        private const double TwoPi = 2 * 3.14159265358979323846;

        public static void Four1(int nn, ref List<double> Data, int isign)
        {
            /* Perform bit reversal of Data[] */
            var n = nn << 1;
            var j = 1;
            for (var i = 1; i < n; i += 2)
            {
                if (j > i)
                {
                    var wtemp = Data[i];
                    Data[i] = Data[j];
                    Data[j] = wtemp;
                    wtemp = Data[i + 1];
                    Data[i + 1] = Data[j + 1];
                    Data[j + 1] = wtemp;
                }
                var m = n >> 1;
                while (m >= 2 && j > m)
                {
                    j -= m;
                    m >>= 1;
                }
                j += m;
            }

            /* Perform Danielson-Lanczos section of FFT */
            n = nn << 1;
            var mmax = 2;
            while (n > mmax) /* Loop executed log(2)nn times */
            {
                var istep = mmax << 1;
                var theta = isign * (TwoPi / mmax); /* Initialize the trigonimetric recurrance */
                var wtemp = Math.Sin(0.5 * theta);
                var wpr = -2.0 * wtemp * wtemp;
                var wpi = Math.Sin(theta);
                var wr = 1.0;
                var wi = 0.0;
                for (var m = 1; m < mmax; m += 2)
                {
                    for (var i = m; i <= n; i += istep)
                    {
                        j = i + mmax; /* The Danielson-Lanczos formula */
                        var tempr = wr * Data[j] - wi * Data[j + 1];
                        var tempi = wr * Data[j + 1] + wi * Data[j];
                        Data[j] = Data[i] - tempr;
                        Data[j + 1] = Data[i + 1] - tempi;
                        Data[i] += tempr;
                        Data[i + 1] += tempi;
                    }
                    wr = (wtemp = wr) * wpr - wi * wpi + wr;
                    wi = wi * wpr + wtemp * wpi + wi;
                }
                mmax = istep;
            }
            /* Normalize if FT */
            if (isign == 1)
            {
                for (var i = 1; i <= nn; i++)
                {
                    Data[2 * i - 1] /= nn;
                    Data[2 * i] /= nn;
                }
            }
        } /* End of Four1() */
    }
}