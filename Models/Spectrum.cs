using DiplomaMB.Utils;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
            get => wavelengths;
            set => wavelengths = value;
        }

        private List<double> data_values;
        public List<double> DataValues
        {
            get => data_values;
            set => data_values = value;
        }

        private List<Peak> peaks;
        public List<Peak> Peaks
        {
            get => peaks;
            set => peaks = value;
        }

        public Spectrum()
        {
            
        }

        public Spectrum(List<double> _wavelengths, List<double> _dataValues, string _name = "", int _id = 0)
        {
            Wavelengths = _wavelengths;
            data_values = _dataValues;
            Name = _name;
            Id = _id;
            Enabled = true;
            Peaks = new List<Peak>();
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
            Peaks = new List<Peak>();
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
            foreach (double item in data_values)
            {
                lineSerie.Points.Add(new DataPoint(wavelengths[i], item));
                i++;
            }

            return lineSerie;
        }

        public OxyPlot.Series.ScatterSeries getPeaks()
        {
            var scatterSerie = new OxyPlot.Series.ScatterSeries
            {
                MarkerFill = OxyPlot.OxyColors.Black,
                MarkerSize = 5,
                MarkerType = MarkerType.Triangle,
            };

            foreach (Peak peak in peaks)
            {
                scatterSerie.Points.Add(new ScatterPoint(wavelengths[peak.PeakIndex], data_values[peak.PeakIndex]));
            }

            return scatterSerie;
        }

        public static Spectrum operator +(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for(int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] + spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator +(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] + doubleValue);
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator -(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] - spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }
        public static Spectrum operator -(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] - doubleValue);
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator *(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] * spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator *(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] * doubleValue);
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator /(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    if (spectrum2.DataValues[i] != 0.0)
                    {
                        dataValues.Add(spectrum1.DataValues[i] / spectrum2.DataValues[i]);
                    }
                    else
                    {
                        dataValues.Add(0.0);
                    }
                }
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum operator /(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] / doubleValue);
            }
            Spectrum result = new(spectrum1.wavelengths, dataValues);
            return result;
        }

        public Spectrum PerformBaselineCorrection(Spectrum spectrum, long lambda, uint itermax)
        {
            double[] inputArray = spectrum.DataValues.ToArray();
            double[] output = SpectrumUtils.BaselineRemoveAirPLS(inputArray, lambda, itermax);

            //double[] output = SpectrumUtils.BaselineRemoveALS(inputArray, lambda, 0.001, itermax);

            for (int i = 0; i < inputArray.Length; i++)
            {
                output[i] = inputArray[i] - output[i];
            }

            string name = $"{spectrum.Name}_baselineRemoved";
            List<double> wavelengths = spectrum.wavelengths;
            List<double> dataValues = output.ToList();

            Spectrum result = new(wavelengths, dataValues, name);
            return result;
        }

        public void DetectPeaks(int min_peak_height)
        {
            List<Peak> peaks = SpectrumUtils.DetectSpectrumPeaks(data_values, wavelengths, min_peak_height);

            foreach( Peak peak in peaks )
            {
                double peakWavelength = wavelengths[(int)peak.PeakIndex];
                double peakValue = data_values[(int)peak.PeakIndex];
                Debug.WriteLine($"Peak wavelength: {peakWavelength}, value: {peakValue}");
            }

            Peaks = peaks;
        }

       
        public void SaveToFile()
        {   
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "CSV file (*.csv)|*.csv| Json files (*.json)|*.json| All Files (*.*)|*.*",
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
            Debug.WriteLine("wavelengths: "+Wavelengths.Count + " dataArray: " + DataValues.Count);
            for (int i = 0; i < Wavelengths.Count; i++)
            {
                var first = Wavelengths[i].ToString(CultureInfo.InvariantCulture);
                var second = DataValues[i];
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
            DataValues = new List<double>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                Wavelengths.Add(double.Parse(values[0], CultureInfo.InvariantCulture));
                DataValues.Add(Convert.ToDouble(values[1]));
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
                DataValues = spectrum.DataValues;
                Id = spectrum.Id;
                Name = spectrum.Name;
                Enabled = spectrum.Enabled;
            }
        }
    }
}

