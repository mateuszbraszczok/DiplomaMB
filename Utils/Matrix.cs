using MathWorks.Baseline;
using MathWorks.MATLAB.NET.Arrays;

namespace DiplomaMB.Utils
{

    public class MBMatrix
    {

        public static double[] BaselineRemoveAirPLS(double[] data, double lambda, uint itermax)
        {
            Baseline baseline = new();
            MWNumericArray dataArray = new(data);

            MWNumericArray lambdaValue = new(lambda);
            MWNumericArray itermaxValue = new(itermax);

            MWArray array = baseline.airPLS(dataArray, lambdaValue, itermaxValue);

            object objData = array.ToArray();

            double[,] doubleMatrix = (double[,])objData;

            int rows = doubleMatrix.GetLength(0);
            int columns = doubleMatrix.GetLength(1);

            // Calculate the total number of elements in the 2D array
            int totalElements = rows * columns;

            // Create a new 1D array to store the flattened elements
            double[] doubleArray = new double[totalElements];

            // Flatten the 2D array into the 1D array
            int index = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    doubleArray[index] = doubleMatrix[i, j];
                    index++;
                }
            }

            return doubleArray;
        }
    }
}
