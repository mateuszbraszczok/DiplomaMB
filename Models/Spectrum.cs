/**
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

        private static int last_spectrum_id = 1;

        private static List<OxyColor> colors = new List<OxyColor>
        {
            OxyColor.FromRgb((byte)(0 * 255), (byte)(0.4470 * 255), (byte)(0.7410 * 255)),  // Blue
            OxyColor.FromRgb((byte)(0.8500 * 255), (byte)(0.3250 * 255), (byte)(0.0980 * 255)),  // Orange
            OxyColor.FromRgb((byte)(0.9290 * 255), (byte)(0.6940 * 255), (byte)(0.1250 * 255)),  // Yellow
            OxyColor.FromRgb((byte)(0.4940 * 255), (byte)(0.1840 * 255), (byte)(0.5560 * 255)),  // Purple
            OxyColor.FromRgb((byte)(0.4660 * 255), (byte)(0.6740 * 255), (byte)(0.1880 * 255)),  // Green
            OxyColor.FromRgb((byte)(0.3010 * 255), (byte)(0.7450 * 255), (byte)(0.9330 * 255)),  // Light Blue
            OxyColor.FromRgb((byte)(0.6350 * 255), (byte)(0.0780 * 255), (byte)(0.1840 * 255)),  // Red

            OxyColor.FromRgb(255, 140, 0),  // Dark Orange
            OxyColor.FromRgb(0, 255, 255),  // Cyan
            OxyColor.FromRgb(148, 0, 211),  // Dark Violet
            OxyColor.FromRgb(0, 255, 127),  // Spring Green
            OxyColor.FromRgb(220, 20, 60),  // Crimson
            OxyColor.FromRgb(210, 105, 30), // Chocolate
            OxyColor.FromRgb(255, 20, 147), // Deep Pink
            OxyColor.FromRgb(128, 128, 0),  // Olive
            OxyColor.FromRgb(106, 90, 205), // Slate Blue
            OxyColor.FromRgb(46, 139, 87)   // Sea Green
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="Spectrum"/> class with the provided wavelengths and data values.
        /// </summary>
        /// <param name="_wavelengths">A list of wavelengths to be assigned to this instance.</param>
        /// <param name="_dataValues">A list of data values to be assigned to this instance.</param>
        /// <param name="_name">A string representing the name of the Spectrum. Default is an empty string.</param>
        /// <remarks>
        /// The constructor initializes the Wavelengths, DataValues, Name properties with the provided values.
        /// It also sets the Enabled property to true and initializes an empty list for storing peaks.
        /// </remarks>
        public Spectrum(List<double> _wavelengths, List<double> _dataValues, string _name = "")
        {
            wavelengths = _wavelengths;
            data_values = _dataValues;
            name = _name;
            id = last_spectrum_id++;
            enabled = true;
            peaks = new List<Peak>();
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
        public Spectrum(string file_path)
        {
            data_values = new List<double>();
            wavelengths = new List<double>();
            name = "";

            string extension = Path.GetExtension(file_path);
            if (extension == ".csv")
            {
                LoadCsvFile(file_path);
            }
            else if (extension == ".json")
            {
                LoadJsonFile(file_path);
            }
            peaks = new List<Peak>();
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
                Color = colors[(Id - 1) % colors.Count],
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
        /// Averages the data values of two Spectrum objects element-wise.
        /// </summary>
        /// <param name="spectrum1">The first Spectrum object whose data values are to be averaged.</param>
        /// <param name="spectrum2">The second Spectrum object whose data values are to be averaged.</param>
        /// <returns>A new Spectrum object where each data value is the average of the corresponding data values in the input Spectrums.</returns>
        /// <remarks>
        /// This function allows for easy averaging of data values from two Spectrum objects.
        /// It creates a new Spectrum object that contains the averaged data values.
        /// 
        /// Note: This operation does not alter the original Spectrum objects.
        /// 
        /// Caution: The function assumes that both Spectrum objects have the same length of data values.
        /// If they do not, this will result in an index out-of-range exception.
        /// </remarks>
        public static Spectrum AverageSpectrums(Spectrum spectrum1, Spectrum spectrum2)
        {
            List<double> dataValues = new List<double>();

            for (int i = 0; i < spectrum1.DataValues.Count; i++)
            {
                dataValues.Add((spectrum1.DataValues[i] + spectrum2.DataValues[i]) / 2);
            }
            Spectrum result = new Spectrum(spectrum1.wavelengths, dataValues);
            return result;
        }

        public static Spectrum MergeSpectrums(Spectrum spectrum1, Spectrum spectrum2, int threshold, int spectrum_max_value)
        {
            bool[] in_merged_region = spectrum2.GetMergingRegions(threshold);
            // Initialize new signal array
            List<double> newsignal = new List<double>();

            // Loop through all data and write true when in a merged region
            bool in_region = false;
            double[] arith_seq = null;
            double OldMax = 0;
            double OldMin = 0;
            int next_false_idx = 0;

            for (int i = 0; i < spectrum2.DataValues.Count; i++)
            {
                if (in_merged_region[i])
                {
                    if (!in_region)
                    {
                        next_false_idx = Array.FindIndex(in_merged_region, i, x => !x);
                        double ratio_begin = spectrum2.DataValues[i] / spectrum1.DataValues[i];
                        double ratio_end = spectrum2.DataValues[next_false_idx] / spectrum1.DataValues[next_false_idx];

                        arith_seq = Enumerable.Range(0, next_false_idx - i)
                                              .Select(n => ratio_begin + n * (ratio_end - ratio_begin) / (next_false_idx - i - 1))
                                              .ToArray();

                        var data_series = spectrum1.DataValues.GetRange(i, next_false_idx - i)
                                             .Select((val, idx) => val * arith_seq[idx])
                                             .ToList();

                        OldMax = data_series.Max();
                        OldMin = data_series.Min();

                        in_region = true;
                    }
                    double normalized_data = ((spectrum1.DataValues[i] * arith_seq[next_false_idx - i - 1] - OldMin) / (OldMax - OldMin)) * (spectrum_max_value - OldMin) + OldMin;
                    newsignal.Add(normalized_data);
                }
                else
                {
                    in_region = false;
                    newsignal.Add(spectrum2.DataValues[i]);
                }
            }

            Spectrum result = new Spectrum(spectrum1.wavelengths, newsignal);
            return result;
        }

        private bool[] GetMergingRegions(int threshold)
        {
            List<Tuple<int, int>> above_threshold_regions = new List<Tuple<int, int>>();
            int? start_idx = null;

            // Loop through the data series
            for (int i = 0; i < DataValues.Count; i++)
            {
                double value = DataValues[i];

                if (value > threshold)
                {
                    if (start_idx == null)
                    {
                        start_idx = i;
                    }
                }
                else
                {
                    if (start_idx != null)
                    {
                        above_threshold_regions.Add(new Tuple<int, int>((int)start_idx, i - 1));
                        start_idx = null;
                    }
                }
            }

            // Capture trailing regions
            if (start_idx != null)
            {
                above_threshold_regions.Add(new Tuple<int, int>((int)start_idx, DataValues.Count - 1));
            }

            // Merging regions
            List<Tuple<int, int>> merged_regions = new List<Tuple<int, int>>();
            int current_start = above_threshold_regions[0].Item1;
            int current_end = above_threshold_regions[0].Item2;

            for (int i = 1; i < above_threshold_regions.Count; i++)
            {
                if (above_threshold_regions[i].Item1 - current_end <= 5)
                {
                    // Merge the region with the previous one
                    current_end = above_threshold_regions[i].Item2;
                }
                else
                {
                    // Add the current merged region to the list and reset
                    merged_regions.Add(new Tuple<int, int>(current_start, current_end));
                    current_start = above_threshold_regions[i].Item1;
                    current_end = above_threshold_regions[i].Item2;
                }
            }

            merged_regions.Add(new Tuple<int, int>(current_start, current_end));

            bool[] in_merged_region = new bool[DataValues.Count];

            // Loop through each merged region and set corresponding indices to true
            foreach (var region in merged_regions)
            {
                for (int i = region.Item1; i <= region.Item2; i++)
                {
                    in_merged_region[i] = true;
                }
            }

            return in_merged_region;
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
        /// 
        /// The CSV file should have two columns separated by a comma. The first column is interpreted as wavelengths (in double format), and the second column is interpreted as data values (also in double format).
        /// 
        /// The method also sets the Name property based on the input parameters and the file name.
        /// </remarks>
        private void LoadCsvFile(string file_path)
        {
            using var reader = new StreamReader(file_path);
            Wavelengths = new List<double>();
            DataValues = new List<double>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = (line ?? string.Empty).Split(',');

                Wavelengths.Add(double.Parse(values[0], CultureInfo.InvariantCulture));
                DataValues.Add(Convert.ToDouble(values[1]));
            }
            Name = Path.GetFileNameWithoutExtension(file_path);
            Id = last_spectrum_id++;
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

            JsonElement json_root = JsonDocument.Parse(json_content).RootElement;
            Wavelengths = json_root.GetProperty(nameof(Wavelengths)).EnumerateArray().Select(e => e.GetDouble()).ToList();
            DataValues = json_root.GetProperty(nameof(DataValues)).EnumerateArray().Select(e => e.GetDouble()).ToList();
            Name = json_root.GetProperty(nameof(Name)).GetString() ?? "";
            Enabled = json_root.GetProperty(nameof(Enabled)).GetBoolean();
            Id = last_spectrum_id++;
        }
    }
}

