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

namespace DiplomaMB.Utils
{
    public class MBMatrix
    {
        public int rows_number;

        public int cols_number;

        public Matrix<double> data;

        public MBMatrix(double[,] matrix) 
        { 
            rows_number = matrix.GetLength(0);
            cols_number = matrix.GetLength(1);
            data = DenseMatrix.OfArray(matrix);
        }

        public MBMatrix(Matrix<double> matrix)
        {
            rows_number = matrix.RowCount;
            cols_number = matrix.ColumnCount;
            data = matrix;
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
            Matrix<double> multipliedMatrix = data.Multiply(value);
            MBMatrix result = new MBMatrix(multipliedMatrix);

            return result;
        }

        public MBMatrix AddValue(MBMatrix matrix, double value)
        {
            Matrix<double> multipliedMatrix = data.Add(value);
            MBMatrix result = new MBMatrix(multipliedMatrix);

            return result;
        }

        public MBMatrix Transpose()
        {
            if (CalculateDeterminant() == 0)
            {
                throw new ArgumentException("can't transpose matrix");
            }

            Matrix<double> transposedMatrix = data.Transpose();

            MBMatrix transposed = new MBMatrix(transposedMatrix);

            return transposed;
        }

        public double CalculateDeterminant()
        {
            double determinant = data.Determinant();
            return determinant;
        }

        public MBMatrix Inverse()
        {
            Matrix<double> inverseMatrix = data.Inverse();
            MBMatrix inversed = new MBMatrix(inverseMatrix);
            return inversed;
        }


        public void print()
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
    }
}
