using System.Runtime.InteropServices;

namespace DiplomaMB.Utils
{

    public class SpectrumUtils
    {
        [DllImport("Utils\\Libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void airPLS(int itermax, double[] y, int size, double lambda, double[] outResult, ref int outSize);

        [DllImport("Utils\\Libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
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

    }
}
