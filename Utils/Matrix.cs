using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Windows.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using System.IO;
using System.Globalization;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics;
using System.Runtime.InteropServices;

using MathWorks.Training.MLNE;
using MathWorks.MATLAB.NET.Arrays;
using MathWorks.Baseline;

namespace DiplomaMB.Utils
{


    public class MBMatrix
    {
        public int rows_number;

        public int cols_number;

        public Matrix<double> data;

        GDPTools gdpmod = null;



        public static double[] BaselineRemoveAirPLS(double[] data, double lambda, uint itermax)
        {
            Baseline baseline = new Baseline();
            MWNumericArray dataArray = new MWNumericArray(data);

            MWNumericArray lambdaValue = new MWNumericArray(lambda);
            MWNumericArray itermaxValue = new MWNumericArray(itermax);
            


            MWArray array = baseline.airPLS(dataArray, lambdaValue, itermaxValue);

            MWNumericArray numericArray = (MWNumericArray)array;


            //object objData = array.ToArray();

            //double[] returnValue = (double[])objData;
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

        public MBMatrix(double[,] matrix) 
        { 
            rows_number = matrix.GetLength(0);
            cols_number = matrix.GetLength(1);
            data = DenseMatrix.OfArray(matrix);

            gdpmod = new GDPTools();
        }

        public MBMatrix(Matrix<double> matrix)
        {
            rows_number = matrix.RowCount;
            cols_number = matrix.ColumnCount;
            data = matrix;

            gdpmod = new GDPTools();

        }


        public static MBMatrix operator +(MBMatrix matrix1, MBMatrix matrix2)
        {
            if (matrix1.cols_number != matrix2.cols_number || matrix1.rows_number != matrix2.rows_number)
            {
                throw new ArgumentException("can't add matrices");
            }

            Matrix<double> temp = matrix1.data + matrix2.data;
            MBMatrix result = new MBMatrix(temp);

            return result;
        }

        public static MBMatrix operator -(MBMatrix matrix1, MBMatrix matrix2)
        {
            if (matrix1.cols_number != matrix2.cols_number || matrix1.rows_number != matrix2.rows_number)
            {
                throw new ArgumentException("can't subtract matrices");
            }

            Matrix<double> temp = matrix1.data - matrix2.data;
            MBMatrix result = new MBMatrix(temp);

            return result;
        }

        public static MBMatrix operator *(MBMatrix matrix1, MBMatrix matrix2)
        {
            if (matrix1.cols_number != matrix2.rows_number)
            {
                throw new ArgumentException("can't multiply matrices");
            }

            Matrix<double> temp = matrix1.data * matrix2.data;
            MBMatrix result = new MBMatrix(temp);

            return result;
        }

        public MBMatrix MultiplyBy(double value)
        {
            double[,] temp = new double[data.RowCount, data.ColumnCount];
            for (int row = 0; row < data.RowCount; row++)
            {
                for (int col = 0; col < data.ColumnCount; col++)
                {
                    temp[row, col] = data[row,col] * value;
                }
            }
            MBMatrix result = new MBMatrix(temp);

            return result;
        }

        public MBMatrix AddValue(double value)
        {
            double[,] temp = new double[data.RowCount, data.ColumnCount];
            for (int row = 0; row < data.RowCount; row++)
            {
                for (int col = 0; col < data.ColumnCount; col++)
                {
                    temp[row, col] = data[row, col] + value;
                }
            }
            MBMatrix result = new MBMatrix(temp);
            return result;
        }

        public MBMatrix GetSqrt()
        {
            Matrix<double> sqrtOfMatrix = data.PointwiseSqrt();
            MBMatrix sqrted = new MBMatrix(sqrtOfMatrix);
            return sqrted;
        }

        public MBMatrix Transpose()
        {
            Matrix<double> transposedMatrix = data.Transpose();
            MBMatrix transposed = new MBMatrix(transposedMatrix);
            return transposed;
        }

        public MBMatrix Inverse()
        {
            //int maxDegreeOfParallelism = 8; // Set to the number of threads you want to use

            // Set the maximum degree of parallelism for MathNet.Numerics
            //Control.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            //Control.UseManaged();

            //Debug.WriteLine(Control.NativeProviderPath);

            double[,] doubleMatrix = data.ToArray();

            var watch = new System.Diagnostics.Stopwatch();

            watch.Start();

            MWNumericArray numericArray = new MWNumericArray(doubleMatrix);
            MWArray array = gdpmod.invert_matrix(numericArray);

            object objData = array.ToArray();

            double[,] doubleMatrix2 = (double[,])objData;



            //if (CalculateDeterminant() == 0)
            //{
            //    throw new ArgumentException("can't inverse matrix");
            //}
            ////Matrix<double> inverseMatrix = data.Inverse();

            //QR<double> qr = data.QR();

            //// Get the R factor from the QR decomposition
            //Matrix<double> R = qr.R;

            //// Calculate the inverse using QR decomposition
            //Matrix<double> inverseA_QR = qr.Solve(DenseMatrix.CreateIdentity(data.RowCount));

            MBMatrix inversed = new MBMatrix(doubleMatrix2);

            watch.Stop();

            Debug.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            return inversed;
        }

        public double CalculateDeterminant()
        {
            double determinant = data.Determinant();
            return determinant;
        }

        public void Print()
        {
            Debug.WriteLine($"Matrix rows={rows_number}, columns={cols_number}");
            for (int i = 0; i < rows_number; i++)
            {
                for (int j = 0; j < cols_number; j++)
                {
                    double value = data[i, j];
                    Debug.WriteLine($"[{i},{j}]={value}");
                }
            }
        }

        public void SaveToFile(string filename)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            using (StreamWriter writer = new StreamWriter(filename))
            {
                for (int row = 0; row < data.RowCount; row++)
                {
                    for (int col = 0; col < data.ColumnCount; col++)
                    {
                        string formattedValue = data[row, col].ToString(culture);
                        writer.Write(formattedValue);

                        if (col < data.ColumnCount - 1)
                        {
                            writer.Write(",");
                        }
                    }

                    writer.WriteLine();
                }
            }
        }
    }
}
