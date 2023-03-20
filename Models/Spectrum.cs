using Microsoft.Win32;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.Models
{
    public class Spectrum
    {
        public List<double> wavelengths = new List<double>();
        public List<ushort> dataArray = new List<ushort>();

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


        public Spectrum(List<double> _wavelengths, List<ushort> _dataArray, string _name = "", int _id = 0)
        {
            wavelengths = _wavelengths;
            dataArray = _dataArray;
            Name = _name;
            Id = _id;
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
            foreach (ushort item in dataArray)
            {
                lineSerie.Points.Add(new DataPoint(wavelengths[i], Convert.ToDouble(item)));
                i++;
            }

            return lineSerie;
        }


        public void SaveToFile()
        {
            var csv = new StringBuilder();
            for (int i = 0; i < wavelengths.Count; i++)
            {
                var first = wavelengths[i].ToString(CultureInfo.InvariantCulture);
                var second = dataArray[i];
                var newLine = $"{first}, {second}";
                csv.AppendLine(newLine);
            }
            string fileText = string.Join(" ", dataArray);
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.FileName = Name;

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
            }
        }

    }
}

