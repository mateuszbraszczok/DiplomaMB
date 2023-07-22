using DiplomaMB.Utils;
using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

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

        public double[] PerformBaselineCorrection(double[] y, double lambda, uint itermax)
        {
            double[] output = MBMatrix.BaselineRemoveAirPLS(y, lambda, itermax);

            for (int i = 0; i < y.Length; i++)
            {
                output[i] = y[i] - output[i];
            }

            return output;
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

