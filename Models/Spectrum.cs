﻿/**
 * @file Spectrum.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief This file contains the Spectrum class which represents a spectrum with wavelengths and data values.
 */

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
    /// <summary>
    /// Represents a spectrum with wavelengths and data values.
    /// </summary>
    public class Spectrum
    {
        /// <summary>
        /// Gets or sets the identifier for this instance.
        /// </summary>
        /// <value>
        /// An integer representing the unique identifier.
        /// </value>
        private int id;
        public int Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Gets or sets the name associated with this instance.
        /// </summary>
        /// <value>
        /// A string representing the name.
        /// </value>
        private string name;
        public string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }

        /// <summary>
        /// Gets or sets the list of wavelengths.
        /// </summary>
        /// <value>
        /// The list of wavelengths.
        /// </value>
        private List<double> wavelengths;
        public List<double> Wavelengths
        {
            get => wavelengths;
            set => wavelengths = value;
        }

        /// <summary>
        /// Gets or sets the list of data values.
        /// </summary>
        /// <value>
        /// The list of data values.
        /// </value>
        private List<double> data_values;
        public List<double> DataValues
        {
            get => data_values;
            set => data_values = value;
        }

        private List<Peak> peaks;
        /// <summary>
        /// Gets or sets the list of peaks in the spectrum.
        /// </summary>
        /// <value>
        /// A list of <see cref="Peak"/> objects representing the peaks in the spectrum.
        /// </value>
        /// <remarks>
        /// Use this property to manipulate the peaks associated with the spectrum.
        /// </remarks>
        public List<Peak> Peaks
        {
            get => peaks;
            set => peaks = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class with default values.
        /// </summary>
        /// <remarks>
        /// This constructor initializes all properties to their default values.
        /// </remarks>
        public Spectrum()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class with the provided wavelengths and data values.
        /// </summary>
        /// <param name="_wavelengths">A list of wavelengths to be assigned to this instance.</param>
        /// <param name="_dataValues">A list of data values to be assigned to this instance.</param>
        /// <param name="_name">A string representing the name of the Spectrum. Default is an empty string.</param>
        /// <param name="_id">An integer ID to be assigned to this Spectrum instance. Default is 0.</param>
        /// <remarks>
        /// The constructor initializes the Wavelengths, DataValues, Name, and ID properties with the provided values.
        /// It also sets the Enabled property to true and initializes an empty list for storing peaks.
        /// </remarks>
        public Spectrum(List<double> _wavelengths, List<double> _dataValues, string _name = "", int _id = 0)
        {
            Wavelengths = _wavelengths;
            data_values = _dataValues;
            Name = _name;
            Id = _id;
            Enabled = true;
            Peaks = new List<Peak>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class by loading data from a file.
        /// </summary>
        /// <param name="file_path">The path to the file to be loaded. The file can be either a CSV or a JSON file.</param>
        /// <param name="_id">An integer ID that will be assigned to the Spectrum instance.</param>
        /// <remarks>
        /// The constructor will determine the file type based on the file extension (.csv or .json) 
        /// and then call the appropriate method (<see cref="LoadCsvFile"/> or <see cref="LoadJsonFile"/>) to load the data.
        /// After loading the data, it initializes an empty list for storing peaks.
        /// 
        /// Note: The given file should conform to the expected CSV or JSON format. Otherwise, the behavior is undefined.
        /// </remarks>
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

        /// <summary>
        /// Creates and returns a plot series for displaying on an OxyPlot chart.
        /// </summary>
        /// <returns>An instance of <see cref="OxyPlot.Series.LineSeries"/> containing the data for the plot.</returns>
        /// <remarks>
        /// This method initializes a new LineSeries object with various properties such as StrokeThickness and MarkerSize.
        /// It populates the plot series with data points based on the 'data_values' and 'wavelengths' fields of the current instance.
        /// 
        /// Note: This method assumes that 'data_values' and 'wavelengths' are synchronized, meaning they have the same count of items.
        /// </remarks>
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

        /// <summary>
        /// Creates and returns a scatter series representing the peaks for displaying on an OxyPlot chart.
        /// </summary>
        /// <returns>
        /// An instance of <see cref="OxyPlot.Series.ScatterSeries"/> containing the data for the scatter plot.
        /// </returns>
        /// <remarks>
        /// This method initializes a new ScatterSeries object with various properties such as MarkerFill, MarkerSize, and MarkerType.
        /// It populates the scatter series with data points based on the 'peaks' field and associates each point with its corresponding wavelength and data value.
        /// 
        /// Note: This method assumes that the 'peaks' field contains valid Peak objects that reference correct indices in 'wavelengths' and 'data_values'.
        /// </remarks>
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

        /// <summary>
        /// Adds each data value in one Spectrum to the corresponding data value in another Spectrum.
        /// </summary>
        /// <param name="spectrum1">The first Spectrum object whose data values are to be added.</param>
        /// <param name="spectrum2">The second Spectrum object whose data values are to be added to the first Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the addition of corresponding data values in the input Spectra.</returns>
        /// <remarks>
        /// This operator enables pointwise addition of each data value in one Spectrum object to the corresponding data value in another Spectrum object.
        /// It creates a new Spectrum object that contains the result of each addition.
        /// 
        /// The operation is only performed if the number of data values in both Spectrum objects are equal.
        /// 
        /// Note: This operation does not alter the original Spectrum objects.
        /// </remarks>
        public static Spectrum operator +(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new List<double>();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] + spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Adds a specified double value to each data value in the Spectrum.
        /// </summary>
        /// <param name="spectrum1">The Spectrum object whose data values are to be added to.</param>
        /// <param name="doubleValue">The double value to be added to each data value in the Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the addition.</returns>
        /// <remarks>
        /// This operator allows for the easy addition of a double value to each data value in a Spectrum object.
        /// It creates a new Spectrum object containing the results of the addition.
        /// 
        /// Note: This operation does not alter the original Spectrum object.
        /// </remarks>
        public static Spectrum operator +(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new List<double>();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] + doubleValue);
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Subtracts each data value in one Spectrum from the corresponding data value in another Spectrum.
        /// </summary>
        /// <param name="spectrum1">The Spectrum object from which data values are to be subtracted.</param>
        /// <param name="spectrum2">The Spectrum object whose data values are to be subtracted from the first Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the subtraction of corresponding data values in the input Spectra.</returns>
        /// <remarks>
        /// This operator enables pointwise subtraction of each data value in one Spectrum object from the corresponding data value in another Spectrum object.
        /// It creates a new Spectrum object that contains the result of each subtraction.
        /// 
        /// The operation is only performed if the number of data values in both Spectrum objects are equal.
        /// 
        /// Note: This operation does not alter the original Spectrum objects.
        /// </remarks>
        public static Spectrum operator -(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new List<double>();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] - spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Subtracts a specified double value from each data value in the Spectrum.
        /// </summary>
        /// <param name="spectrum1">The Spectrum object whose data values are to be subtracted from.</param>
        /// <param name="doubleValue">The double value to be subtracted from each data value in the Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the subtraction.</returns>
        /// <remarks>
        /// This operator allows for the easy subtraction of a double value from each data value in a Spectrum object.
        /// It creates a new Spectrum object containing the results of the subtraction.
        /// 
        /// Note: This operation does not alter the original Spectrum object.
        /// </remarks>
        public static Spectrum operator -(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new List<double>();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] - doubleValue);
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Multiplies each data value in one Spectrum by the corresponding data value in another Spectrum.
        /// </summary>
        /// <param name="spectrum1">The first Spectrum object whose data values are to be multiplied.</param>
        /// <param name="spectrum2">The second Spectrum object by which to multiply each data value in the first Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the multiplication of corresponding data values in the input Spectra.</returns>
        /// <remarks>
        /// This operator allows for pointwise multiplication of each data value in one Spectrum object by the corresponding data value in another Spectrum object.
        /// It creates a new Spectrum object that contains the result of each multiplication.
        /// 
        /// The operation is performed only if the number of data values in both Spectrum objects are equal.
        /// 
        /// Note: This operation does not alter the original Spectrum objects.
        /// </remarks>
        public static Spectrum operator *(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new List<double>();

            if (spectrum1.DataValues.Count == spectrum2.DataValues.Count)
            {
                for (int i = 0; i < spectrum1.DataValues.Count; i++)
                {
                    dataValues.Add(spectrum1.DataValues[i] * spectrum2.DataValues[i]);
                }
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Multiplies each data value in the Spectrum by a specified double value.
        /// </summary>
        /// <param name="spectrum1">The Spectrum object whose data values are to be multiplied.</param>
        /// <param name="doubleValue">The double value by which to multiply each data value in the Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the multiplication.</returns>
        /// <remarks>
        /// This operator allows for the easy multiplication of each data value in a Spectrum object by a double value.
        /// It creates a new Spectrum object that contains the result of the multiplication.
        /// 
        /// Note: This operation does not alter the original Spectrum object.
        /// </remarks>
        public static Spectrum operator *(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new List<double>();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] * doubleValue);
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Divides each data value in one Spectrum by the corresponding data value in another Spectrum.
        /// </summary>
        /// <param name="spectrum1">The numerator Spectrum object whose data values are to be divided.</param>
        /// <param name="spectrum2">The denominator Spectrum object by which to divide each data value in the first Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the division of corresponding data values in the input Spectra.</returns>
        /// <remarks>
        /// This operator allows for pointwise division of each data value in one Spectrum object by the corresponding data value in another Spectrum object.
        /// It creates a new Spectrum object that contains the result of each division.
        /// 
        /// The operation is performed only if the number of data values in both Spectrum objects are equal. 
        /// 
        /// If any data value in the denominator Spectrum is zero, the corresponding result will be zero to avoid division by zero errors.
        ///
        /// Note: This operation does not alter the original Spectrum objects.
        /// </remarks>
        public static Spectrum operator /(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new List<double>();

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
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        /// <summary>
        /// Divides each data value in the Spectrum by a specified double value.
        /// </summary>
        /// <param name="spectrum1">The Spectrum object whose data values are to be divided.</param>
        /// <param name="doubleValue">The double value by which to divide each data value in the Spectrum.</param>
        /// <returns>A new Spectrum object where each data value is the result of the division.</returns>
        /// <remarks>
        /// This operator allows for easy division of each data value in a Spectrum object by a double value.
        /// It creates a new Spectrum object that contains the result of the division.
        /// 
        /// Note: This operation does not alter the original Spectrum object.
        /// </remarks>
        public static Spectrum operator /(Spectrum spectrum1, double doubleValue)
        {
            List<double> dataValues = new List<double>();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add(spectrum1.DataValues[i] / doubleValue);
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }


        /// <summary>
        /// Performs baseline correction on a given Spectrum object using the AirPLS algorithm.
        /// </summary>
        /// <param name="spectrum">The Spectrum object containing the original data.</param>
        /// <param name="lambda">The regularization parameter for the AirPLS algorithm.</param>
        /// <param name="itermax">The maximum number of iterations for the AirPLS algorithm.</param>
        /// <returns>Returns a new Spectrum object with the baseline corrected data.</returns>
        /// <remarks>
        /// This method performs baseline correction on the input Spectrum object using the AirPLS algorithm. 
        /// It subtracts the estimated baseline from the original data and returns a new Spectrum object containing the corrected data.
        /// 
        /// The 'lambda' and 'itermax' parameters control the behavior of the AirPLS algorithm. 
        /// 'lambda' is the regularization parameter, and 'itermax' is the maximum number of iterations allowed.
        /// 
        /// The method can also be modified to use different baseline correction algorithms like ALS (currently commented out).
        /// </remarks>
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

            Spectrum result = new Spectrum(wavelengths, dataValues, name);
            return result;
        }

        /// <summary>
        /// Opens a SaveFileDialog to let the user save the data to a file in either CSV or JSON format.
        /// </summary>
        /// <remarks>
        /// This method uses a SaveFileDialog to allow the user to specify the format and location where the data should be saved.
        /// 
        /// The supported file formats are:
        /// - CSV: Save the data in comma-separated values format.
        /// - JSON: Save the data in JSON format.
        /// 
        /// Depending on the selected file extension (.csv or .json), the appropriate SaveAsCsvFile or SaveAsJsonFile method is called to perform the save operation.
        /// </remarks>
        public void SaveToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
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

        /// <summary>
        /// Saves the Wavelengths and DataValues to a CSV file.
        /// </summary>
        /// <remarks>
        /// This method assumes that Wavelengths and DataValues lists have the same size.
        /// The CSV format is: "Wavelength, DataValue".
        /// 
        /// Parameter:
        /// filename - The name of the file to save the data to.
        /// </remarks>
        private void SaveAsCsvFile(string filename)
        {
            var csv = new StringBuilder();
            for (int i = 0; i < Wavelengths.Count; i++)
            {
                var first = Wavelengths[i].ToString(CultureInfo.InvariantCulture);
                var second = DataValues[i];
                var newLine = $"{first}, {second}";
                csv.AppendLine(newLine);
            }
            File.WriteAllText(filename, csv.ToString());
        }

        /// <summary>
        /// Saves the current object's state to a JSON file.
        /// </summary>
        /// <remarks>
        /// This method serializes the current object to a JSON-formatted string and saves it to a file with the specified filename.
        /// 
        /// Parameter:
        /// filename - The name (or path) of the file where the JSON data will be saved.
        /// 
        /// The JSON output will be indented for better readability.
        /// </remarks>
        private void SaveAsJsonFile(string filename)
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filename, json);
        }

        /// <summary>
        /// Loads data from a CSV file into Wavelengths and DataValues lists.
        /// </summary>
        /// <remarks>
        /// This method reads a CSV file from the given file path and populates the Wavelengths and DataValues lists.
        /// 
        /// Parameters:
        /// file_path - The path to the CSV file to read.
        /// id - The identifier to assign to the loaded data.
        /// 
        /// The CSV file should have two columns separated by a comma. The first column is interpreted as wavelengths (in double format), and the second column is interpreted as data values (also in double format).
        /// 
        /// The method also sets the Id and Name properties based on the input parameters and the file name.
        /// </remarks>
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

        /// <summary>
        /// Loads data from a JSON file into the current object.
        /// </summary>
        /// <remarks>
        /// This method reads a JSON file from the given file path and populates the current object's properties.
        /// 
        /// Parameter:
        /// file_path - The path to the JSON file to read.
        /// 
        /// The JSON file should be formatted to match the Spectrum class structure.
        /// 
        /// If the JSON content is valid, this method sets the Wavelengths, DataValues, Id, Name, and Enabled properties based on the JSON data.
        /// </remarks>
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

