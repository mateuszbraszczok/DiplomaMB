/**
 * @file SpectrumUtils.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief Provides utilities for spectrum baseline removal.
 */

using System.Runtime.InteropServices;

namespace DiplomaMB.Utils
{

    /// <summary>
    /// Provides utilities for spectrum baseline removal.
    /// </summary>
    public class SpectrumUtils
    {
        [DllImport("Utils\\Libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void airPLS(int itermax, double[] y, int size, double lambda, double[] outResult, ref int outSize);

        [DllImport("Utils\\Libraries\\baselineRemove.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ALS(int itermax, double p, double[] y, int size, double lambda, double[] outResult, ref int outSize);

        // <summary>
        /// Removes the baseline from a spectrum using the airPLS algorithm.
        /// </summary>
        /// <param name="y">The spectrum data points.</param>
        /// <param name="lambda">The smoothing parameter for airPLS.</param>
        /// <param name="itermax">The maximum number of iterations for airPLS.</param>
        /// <returns>Returns an array of baseline-removed data points.</returns>
        public static double[] BaselineRemoveAirPLS(double[] y, double lambda, uint itermax)
        {
            int outSize = y.Length;
            double[] outResult = new double[outSize];
            airPLS((int)itermax, y, y.Length, lambda, outResult, ref outSize);
            return outResult;
        }

        /// <summary>
        /// Removes the baseline from a spectrum using the ALS algorithm.
        /// </summary>
        /// <param name="y">The spectrum data points.</param>
        /// <param name="lambda">The smoothing parameter for ALS.</param>
        /// <param name="p">The asymmetry parameter for ALS.</param>
        /// <param name="itermax">The maximum number of iterations for ALS.</param>
        /// <returns>Returns an array of baseline-removed data points.</returns>
        public static double[] BaselineRemoveALS(double[] y, double lambda, double p, uint itermax)
        {
            int outSize = y.Length;
            double[] outResult = new double[outSize];
            ALS((int)itermax, p, y, y.Length, lambda, outResult, ref outSize);
            return outResult;
        }

    }
}
