using Caliburn.Micro;
using MathWorks.Baseline;
using MathWorks.Peaks;
using MathWorks.MATLAB.NET.Arrays;
using System.Collections.Generic;
using DiplomaMB.Models;
using System.Security.Cryptography;

namespace DiplomaMB.Utils
{

    public class SpectrumUtils
    {
        public static double[] BaselineRemoveAirPLS(double[] data, double lambda, uint itermax)
        {
            BaselineRemoval baseline = new();
            MWNumericArray dataArray = new(data);

            MWArray baselineAray = baseline.airPLS(dataArray, lambda, itermax);

            double[] resultArray = (double[])((MWNumericArray)baselineAray).ToVector(0);

            baseline.Dispose();

            return resultArray;
        }


        public static double[] BaselineRemoveALS(double[] data, double lambda, double p, uint itermax)
        {
            BaselineRemoval baseline = new();
            MWNumericArray dataArray = new(data);

            MWArray baselineAray = baseline.ALS(dataArray, lambda, p, itermax);

            double[] resultArray = (double[])((MWNumericArray)baselineAray).ToVector(0);

            baseline.Dispose();

            return resultArray;
        }

        public static List<Peak> DetectSpectrumPeaks(List<double> data, List<double> wavelengths)
        {
            PeakAnalyze peakAnalyze = new();

            MWNumericArray dataArray = new(data.ToArray());
            MWNumericArray wavelengthsArray = new(wavelengths.ToArray());

            MWStructArray result = (MWStructArray)peakAnalyze.PeakDetector(dataArray, wavelengthsArray);

            double[] peaks_index = (double[])((MWNumericArray)result.GetField("peaks_index")).ToVector(0);
            double[] peaks_begin_index = (double[])((MWNumericArray)result.GetField("peaks_begin_index")).ToVector(0);
            double[] peaks_end_index = (double[])((MWNumericArray)result.GetField("peaks_end_index")).ToVector(0);

            List<Peak> peaks = new();

            for (int i = 0; i < peaks_index.Length; i++)
            {
                Peak peak = new((int)peaks_index[i], (int)peaks_begin_index[i], (int)peaks_end_index[i]);
                peaks.Add(peak);
            }

            return peaks;
        }
    }
}
