using DiplomaMB.Utils;
using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Xml.Linq;

namespace DiplomaMB.Models
{
    public class Spectrum
    {
        private int id;
        public int Id
        {
            get => id;
            set => id = value;
        }

        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        private List<double> wavelengths;
        public List<double> Wavelengths
        {
            get { return wavelengths; }
            set { wavelengths = value; }
        }

        private List<double> data_array;
        public List<double> DataArray
        {
            get { return data_array; }
            set { data_array = value; }
        }

        public Spectrum()
        {
            
        }

        public Spectrum(List<double> _wavelengths, List<double> _dataArray, string _name = "", int _id = 0)
        {
            Wavelengths = new List<double>();
            DataArray = new List<double>();
            Wavelengths = _wavelengths;
            DataArray = _dataArray;
            Name = _name;
            Id = _id;
            Enabled = true;
        }

        public Spectrum(string file_path, int _id)
        {
            string extension = Path.GetExtension(file_path);
            if (extension == ".csv")
            {
                LoadCsvFile(file_path, _id);
            }
            else if (extension == ".json")
            {
                LoadJsonFile(file_path);
            }
        }

        public OxyPlot.Series.LineSeries getPlotSerie()
        {
            var lineSerie = new OxyPlot.Series.LineSeries
            {
                StrokeThickness = 2,
                MarkerSize = 3,
                Title = Name,
                //MarkerStroke = colors[data.Key],
                //MarkerType = markerTypes[data.Key],
                //CanTrackerInterpolatePoints = false,
                //Smooth = false,
            };
            int i = 0;
            foreach (double item in DataArray)
            {
                lineSerie.Points.Add(new DataPoint(Wavelengths[i], item));
                i++;
            }

            return lineSerie;
        }

        public static Spectrum operator +(Spectrum spectrum1, Spectrum spectrum2)
        {
            string name = $"{spectrum1.Name}+{spectrum2.Name}";
            List<double> wavelengths = spectrum1.wavelengths;
            List<double> dataArray = new List<double>();

            if (spectrum1.DataArray.Count == spectrum2.DataArray.Count)
            {
                for(int i = 0; i < spectrum1.DataArray.Count; i++)
                {
                    dataArray.Add(spectrum1.DataArray[i] + spectrum2.DataArray[i]);
                }
            }
            Spectrum result = new Spectrum(wavelengths, dataArray, name);
            return result;
        }

        public static Spectrum operator -(Spectrum spectrum1, Spectrum spectrum2)
        {
            string name = $"{spectrum1.Name}-{spectrum2.Name}";
            List<double> wavelengths = spectrum1.wavelengths;
            List<double> dataArray = new List<double>();

            if (spectrum1.DataArray.Count == spectrum2.DataArray.Count)
            {
                for (int i = 0; i < spectrum1.DataArray.Count; i++)
                {
                    dataArray.Add(spectrum1.DataArray[i] - spectrum2.DataArray[i]);
                }
            }
            Spectrum result = new Spectrum(wavelengths, dataArray, name);
            return result;
        }

        public double[] PerformBaselineCorrection(double[] y, double lambda, int itermax)
        {
            int L = y.Length;
            MBMatrix D = GetDMatrix(L);
            double[] w = new double[L];

            for (int i = 0; i < L; i++)
            {
                w[i] = 1.0;
            }

            double[] z = null;
            for (int iter = 1; iter <= itermax; iter++)
            {
                Debug.WriteLine($"iteration: {iter}");
                MBMatrix W = GetWMatrix(w);
                MBMatrix W_sqrt = W.GetSqrt();
                MBMatrix Z = CalculateZMatrix(W_sqrt, D, L, lambda);
                z = CalculateZSignal(Z, y);
                double[] residuals = CalculateResiduals(y, z);
                double sumNegResiduals = residuals.Where(r => r < 0).Sum();
                for (int i = 0; i < residuals.Length; i++)
                {
                    if (residuals[i] > 0)
                    {
                        w[i] = 0;
                    }
                    else
                    {
                        w[i] = Math.Exp(-iter * Math.Abs(residuals[i]) / Math.Abs(sumNegResiduals));
                    }
                }

                for (int i = 0; i < z.Length; i++)
                {
                    Debug.WriteLine(z[i]);
                }

                if (Math.Abs(sumNegResiduals) < 0.001 * y.Sum())
                {
                    break;
                }
            }

            for (int i = 0; i < y.Length; i++ )
            {
                z[i] = y[i] - z[i];
            }

            return z;
        }

        private MBMatrix GetDMatrix(int L)
        {
            double[,] values = new double[L-2, L];
            for (int i = 0; i < L - 2; i++)
            {
                for (int j = 0; j < L; j++)
                {
                    if (i == (j - 1))
                    {
                        values[i, j] = -2.0;
                    }
                    else if (i == j || i == j - 2)
                    {
                        values[i, j] = 1.0;
                    }
                    else
                    {
                        values[i, j] = 0.0;
                    }
                }
            }
            MBMatrix D = new MBMatrix(values);
            return D;
        }

        private MBMatrix GetWMatrix(double[] w)
        {
            int L = w.Length;
            double[,] values = new double[L, L];
            for (int i = 0; i < L; i++)
            {
                values[i, i] = w[i];
            }
            MBMatrix W = new MBMatrix(values);
            return W;
        }

        private MBMatrix CalculateZMatrix(MBMatrix W_sqrt, MBMatrix D, int L, double lambda)
        {
            //D.SaveToFile("D.csv");
            //Debug.WriteLine("D matrix colums: " + D.rows_number + "colums: " + D.cols_number);
            //MBMatrix DT = D.Transpose();
            //Debug.WriteLine("DT matrix colums: " + DT.rows_number + "colums: " + DT.cols_number);
            //DT.SaveToFile("DT.csv");
            //double[,] dt= DT.data.AsArray();
            MBMatrix DTD = D.Transpose() * D;
            //DTD.SaveToFile("DTD.csv");

            //W_sqrt.SaveToFile("W_sqrt.csv");
            double factor = lambda / (double)L;
            //MBMatrix beforeSum = DTD.MultiplyBy(factor);
            //beforeSum.SaveToFile("beforeSum.csv");
            //MBMatrix beforeInvert = W_sqrt + beforeSum;
            //beforeInvert.SaveToFile("beforeInvert.csv");
            //MBMatrix afterInvert = beforeInvert.Inverse();
            //afterInvert.SaveToFile("afterInvert.csv");

            MBMatrix Z = (W_sqrt + DTD.MultiplyBy(factor)).Inverse() * W_sqrt;
            //Z.SaveToFile("Z.csv");
            return Z;
        }

        //private double[,] TransposeMatrix(double[,] matrix)
        //{
        //    int rows = matrix.GetLength(0);
        //    int cols = matrix.GetLength(1);
        //    double[,] transposedMatrix = new double[cols, rows];
        //    for (int i = 0; i < cols; i++)
        //    {
        //        for (int j = 0; j < rows; j++)
        //        {
        //            transposedMatrix[i, j] = matrix[j, i];
        //        }
        //    }
        //    return transposedMatrix;
        //}

        //private double[,] MultiplyMatrices(double[,] matrixA, double[,] matrixB)
        //{
        //    int rowsA = matrixA.GetLength(0);
        //    int colsA = matrixA.GetLength(1);
        //    int colsB = matrixB.GetLength(1);
        //    double[,] result = new double[rowsA, colsB];
        //    for (int i = 0; i < rowsA; i++)
        //    {
        //        for (int j = 0; j < colsB; j++)
        //        {
        //            for (int k = 0; k < colsA; k++)
        //            {
        //                result[i, j] += matrixA[i, k] * matrixB[k, j];
        //            }
        //        }
        //    }
        //    return result;
        //}

        //private double[,] InvertMatrix(double[,] matrix, double factor)
        //{
        //    int rows = matrix.GetLength(0);
        //    int cols = matrix.GetLength(1);
        //    double[,] invertedMatrix = new double[rows, cols];
        //    for (int i = 0; i < rows; i++)
        //    {
        //        for (int j = 0; j < cols; j++)
        //        {
        //            invertedMatrix[i, j] = matrix[i, j] / (matrix[i, i] + factor);
        //        }
        //    }
        //    return invertedMatrix;
        //}

        private double[] CalculateZSignal(MBMatrix Z, double[] y)
        {
            int L = y.Length;
            double[] z = new double[L];
            for (int i = 0; i < L; i++)
            {
                for (int j = 0; j < L; j++)
                {
                    z[i] += Z.data[i, j] * y[j];
                }
            }
            return z;
        }

        private double[] CalculateResiduals(double[] y, double[] z)
        {
            int L = y.Length;
            double[] residuals = new double[L];
            for (int i = 0; i < L; i++)
            {
                residuals[i] = y[i] - z[i];
            }
            return residuals;
        }


        public void SaveToFile()
        {   
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Json files (*.json)|*.json|CSV file (*.csv)|*.csv| All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true,
                DefaultExt = ".csv",
                FileName = Name
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string extension = Path.GetExtension(saveFileDialog.FileName);
                if (extension == ".csv")
                {
                    SaveAsCsvFile(saveFileDialog.FileName);
                }
                else if (extension == ".json")
                {
                    SaveAsJsonFile(saveFileDialog.FileName);
                }
            }
        }

        private void SaveAsCsvFile(string filename)
        {
            var csv = new StringBuilder();
            Debug.WriteLine("wavelengths: "+Wavelengths.Count + " dataarray: " + DataArray.Count);
            for (int i = 0; i < Wavelengths.Count; i++)
            {
                var first = Wavelengths[i].ToString(CultureInfo.InvariantCulture);
                var second = DataArray[i];
                var newLine = $"{first}, {second}";
                csv.AppendLine(newLine);
            }
            File.WriteAllText(filename, csv.ToString());
        }

        private void SaveAsJsonFile(string filename)
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            Debug.WriteLine(json);
            Debug.WriteLine("after json generation");
            File.WriteAllText(filename, json);
            Debug.WriteLine("after file saving");
        }

        private void LoadCsvFile(string file_path, int id)
        {
            using var reader = new StreamReader(file_path);
            Wavelengths = new List<double>();
            DataArray = new List<double>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                Wavelengths.Add(double.Parse(values[0], CultureInfo.InvariantCulture));
                DataArray.Add(Convert.ToUInt16(values[1]));
            }
            Id = id;
            Name = Path.GetFileNameWithoutExtension(file_path);
            Enabled = true;
        }

        private void LoadJsonFile(string file_path)
        {
            string json_content = File.ReadAllText(file_path);
            Debug.WriteLine(json_content);

            Spectrum spectrum = JsonSerializer.Deserialize<Spectrum?>(json_content);
            if (spectrum != null)
            {
                Wavelengths = spectrum.Wavelengths;
                DataArray = spectrum.DataArray;
                Id = spectrum.Id;
                Name = spectrum.Name;
                Enabled = spectrum.Enabled;
            }
        }
    }
}

