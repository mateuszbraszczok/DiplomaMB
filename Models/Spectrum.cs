using Caliburn.Micro;
using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaMB.Models
{
    public class Spectrum
    {
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
            MessageBox.Show(file_path);
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
            Id = _id;
            Name = Path.GetFileNameWithoutExtension(file_path);
            Enabled = true;
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
            System.Diagnostics.Debug.WriteLine(json);
            System.Diagnostics.Debug.WriteLine("after json generation");
            MessageBox.Show("aaa" + json);
            File.WriteAllText(filename, json);
        }

    }
}

