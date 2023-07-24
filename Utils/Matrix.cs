using Caliburn.Micro;
using MathWorks.Baseline;
using MathWorks.MATLAB.NET.Arrays;

namespace DiplomaMB.Utils
{

    public class MBMatrix
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
    }
}
