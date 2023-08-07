using DiplomaMB.Models;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.Peaks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DiplomaMB.Utils
{

    public class SpectrumUtils
    {
        [DllImport("Utils\\libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void airPLS(int itermax, double[] y, int size, double lambda, double[] outResult, ref int outSize);

        [DllImport("Utils\\libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ALS(int itermax, double p, double[] y, int size, double lambda, double[] outResult, ref int outSize);

        public static double[] BaselineRemoveAirPLS(double[] y, double lambda, uint itermax)
        {
            int outSize = y.Length;
            double[] outResult = new double[outSize];

            airPLS((int)itermax, y, y.Length, lambda, outResult, ref outSize);

            return outResult;
        }


        public static double[] BaselineRemoveALS(double[] y, double lambda, double p, uint itermax)
        {
            int outSize = y.Length;
            double[] outResult = new double[outSize];

            ALS((int)itermax, p, y, y.Length, lambda, outResult, ref outSize);
            return outResult;
        }

        public static List<Peak> DetectSpectrumPeaks(List<double> data, List<double> wavelengths, int min_peak_height)
        {
            PeakAnalyze peakAnalyze = new();

            MWNumericArray dataArray = new(data.ToArray());
            MWNumericArray wavelengthsArray = new(wavelengths.ToArray());

            MWStructArray result = (MWStructArray)peakAnalyze.PeakDetector(dataArray, wavelengthsArray, min_peak_height);

            double[] peaks_index = (double[])((MWNumericArray)result.GetField("peaks_index")).ToVector(0);
            double[] peaks_begin_index = (double[])((MWNumericArray)result.GetField("peaks_begin_index")).ToVector(0);
            double[] peaks_end_index = (double[])((MWNumericArray)result.GetField("peaks_end_index")).ToVector(0);

            List<Peak> peaks = new();

            for (int i = 0; i < peaks_index.Length; i++)
            {
                Peak peak = new((int)peaks_index[i] - 1, (int)peaks_begin_index[i] - 1, (int)peaks_end_index[i] - 1);
                peaks.Add(peak);
            }

            return peaks;
        }
    }
}
