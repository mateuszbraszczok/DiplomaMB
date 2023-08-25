/**
 * @file BwtekSpectrometer.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief BwtekSpectrometer class for handling spectrometer functionalities.
 */

using Caliburn.Micro;
using DiplomaMB.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace DiplomaMB.Models
{
    /// <summary>
    /// Represents a Bwtek spectrometer.
    /// Implements the <see cref="ISpectrometer"/> interface.
    /// </summary>
    public class BwtekSpectrometer : ISpectrometer
    {
        private BindableCollection<ConfigProperty> config_properties;
        /// <summary>
        /// Gets or sets the configuration properties for the spectrometer.
        /// </summary>
        public BindableCollection<ConfigProperty> ConfigProperties
        {
            get => config_properties;
            set => config_properties = value;
        }

        /// <summary>
        /// Gets or sets the connection status of the spectrometer.
        /// </summary>
        public bool Connected { get; set; }

        private int integration_time;
        /// <summary>
        /// Gets or sets the integration time for the spectrometer.
        /// </summary>
        public int IntegrationTime
        {
            get => integration_time;
            set => integration_time = value;
        }

        private int integration_time_min;
        /// <summary>
        /// Gets or sets the minimum allowable integration time for the spectrometer.
        /// </summary>
        public int IntegrationTimeMin
        {
            get => integration_time_min;
            set => integration_time_min = value;
        }

        public IntegrationTimeUnit IntegrationTimeUnit { get; set; }
        /// <summary>
        /// Gets the unit for integration time as a string.
        /// </summary>
        public string IntegrationTimeUnitStr
        {
            get => IntegrationTimeUnit == IntegrationTimeUnit.Microseconds ? "\xE6s" : "ms";
        }

        private string status;
        /// <summary>
        /// Gets the current status of the spectrometer.
        /// </summary>
        public string Status
        {
            get => status;
        }

        private bool dark_scan_taken;
        /// <summary>
        /// Gets whether a dark scan has been taken.
        /// </summary>
        public bool DarkScanTaken
        {
            get => dark_scan_taken;
        }

        /// <summary>
        /// Stores the USB type.
        /// </summary>
        private int usb_type;
        /// <summary>
        /// Stores the channel.
        /// </summary>
        private int channel;

        /// <summary>
        /// Stores the number of pixels.
        /// </summary>
        private int pixel_number;
        /// <summary>
        /// Stores the timing mode.
        /// </summary>
        private int timing_mode;
        /// <summary>
        /// Stores the input mode.
        /// </summary>
        private int input_mode;

        /// <summary>
        /// Stores the minimum wavelength value on the x-axis.
        /// </summary>
        private int xaxis_min;
        /// <summary>
        /// Stores the maximum wavelength value on the x-axis.
        /// </summary>
        private int xaxis_max;

        /// <summary>
        /// Filename for EEPROM parameters.
        /// </summary>
        private readonly string eeprom_filename = $"{Assembly.GetEntryAssembly().Location}\\..\\para.ini";

        /// <summary>
        /// Stores the wavelengths.
        /// </summary>
        private List<double> wavelengths;

        /// <summary>
        /// Stores the dark scan.
        /// </summary>
        private List<double> dark_scan;

        /// <summary>
        /// Initializes a new instance of the <see cref="BwtekSpectrometer"/> class.
        /// </summary>
        public BwtekSpectrometer()
        {
            Connected = false;
            dark_scan_taken = false;
            status = "Disconnected";
            integration_time_min = 1;

            IntegrationTimeUnit = IntegrationTimeUnit.Miliseconds;
            config_properties = new BindableCollection<ConfigProperty>();
            wavelengths = new List<double>();
            dark_scan = new List<double>();
        }

        /// <summary>
        /// Attempts to connect the spectrometer.
        /// </summary>
        public void Connect()
        {
            Connected = false;
            status = "Connection failed";
            do
            {
                bool retCode = BwtekAPIWrapper.InitDevices();
                if (retCode == false) { break; }

                byte[] tmp_channel = new byte[32];
                int ret = BwtekAPIWrapper.bwtekSetupChannel(-1, tmp_channel);
                if (ret <= 0) { break; }

                int device_count = BwtekAPIWrapper.GetDeviceCount();
                if (device_count != 1) { break; }
                channel = 0;
                ret = BwtekAPIWrapper.GetUSBType(ref usb_type, channel);
                if (ret != 1) { break; }

                ReadEeprom();

                ret = BwtekAPIWrapper.bwtekTestUSB(timing_mode, pixel_number, input_mode, channel, 0);

                if (ret < 0) { break; }

                Debug.WriteLine("Test Usb success");

                int new_integration_time = integration_time_min;
                if (IntegrationTimeUnit == IntegrationTimeUnit.Miliseconds)
                {
                    new_integration_time *= 1000;
                }
                ret = BwtekAPIWrapper.bwtekSetTimeUSB(new_integration_time, channel);
                if (ret != new_integration_time) { break; }

                int lTriggerExit = 0;
                ret = BwtekAPIWrapper.bwtekSetTimingsUSB(lTriggerExit, 1, channel);
                if (ret != lTriggerExit) { break; }
                Connected = true;
                status = "Connected";
            }
            while (false);
        }

        /// <summary>
        /// Disconnects the spectrometer.
        /// </summary>
        public void Disconnect()
        {
            _ = BwtekAPIWrapper.bwtekCloseUSB(channel);
            _ = BwtekAPIWrapper.CloseDevices();
            Connected = false;
            status = "Disconnected";
        }

        /// <summary>
        /// Resets the device.
        /// </summary>
        public void ResetDevice()
        {
            int ret = BwtekAPIWrapper.bwtekSoftReset_CEnP(channel);
            if (ret < 0)
            {
                MessageBox.Show("Failed to reset device", "Reset fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Reads spectrum data from the spectrometer.
        /// </summary>
        /// <param name="frames_to_acquire">Number of frames to acquire.</param>
        /// <returns>A list of spectrum data.</returns>
        public List<Spectrum> ReadData(int frames_to_acquire)
        {
            ushort[] pArray = new ushort[frames_to_acquire * pixel_number];
            int ret = BwtekAPIWrapper.bwtekFrameDataReadUSB(frames_to_acquire, 0, pArray, channel);
            if (ret != (frames_to_acquire * pixel_number))
            {
                throw new Exception("Not received data");
            }
            _ = BwtekAPIWrapper.bwtekStopIntegration(channel);

            Debug.WriteLine("ReadData: received data");

            List<Spectrum> spectrum_list = new List<Spectrum>();
            List<double> data_list = new List<double>();
            List<double> data_array = new List<double>();
            int i = 1;
            foreach (ushort value in pArray)
            {
                if (i == pixel_number)
                {
                    i = 1;
                    data_array = data_list;
                    SubtractDarkScan(data_array);
                    spectrum_list.Add(new Spectrum(wavelengths, data_array));
                    data_list = new List<double>();
                }

                if (i > xaxis_min && i <= xaxis_max + 1)
                {
                    data_list.Add((double)value);
                }
                i++;
            }
            return spectrum_list;
        }

        /// <summary>
        /// Reads spectrum data from the spectrometer with smart settings.
        /// </summary>
        /// <param name="smart_read">The smart read configuration.</param>
        /// <returns>A spectrum object containing the read data.</returns>
        public Spectrum ReadDataSmart(SmartRead smart_read)
        {
            ushort[] pArray = new ushort[pixel_number];
            int ret = BwtekAPIWrapper.bwtekDSPDataReadUSB(smart_read.SpectrumsToAverage, Convert.ToInt32(smart_read.Smoothing), Convert.ToInt32(smart_read.DarkCompensation), 0, pArray, channel);

            if (ret != pixel_number)
            {
                Debug.WriteLine($"ReadDataSmart: Received: {ret} pixels");
                throw new Exception("Not received data");
            }
            _ = BwtekAPIWrapper.bwtekStopIntegration(channel);

            List<double> data_array = new List<double>();
            for (int i = xaxis_min; i <= xaxis_max; i++)
            {
                data_array.Add((double)pArray[i]);
            }
            SubtractDarkScan(data_array);

            return new Spectrum(wavelengths, data_array);
        }

        /// <summary>
        /// Subtracts the dark scan from the data array.
        /// </summary>
        /// <param name="data_array">The data array to subtract from.</param>
        private void SubtractDarkScan(List<double> data_array)
        {
            if (dark_scan_taken)
            {
                Debug.WriteLine($"SubtractDarkScan: Data array count: {data_array.Count} Dark count: {dark_scan.Count}");

                bool[] bad_pixels = new bool[data_array.Count];
                data_array[0] = (dark_scan[0] >= data_array[0]) ? 0 : data_array[0] -= dark_scan[0];
                data_array[data_array.Count - 1] = (dark_scan[data_array.Count - 1] >= data_array[data_array.Count - 1]) ? 0 : data_array[data_array.Count - 1] -= dark_scan[data_array.Count - 1];
                for (int i = 1; i < data_array.Count - 1; i++)
                {
                    double mean = 0;
                    int validNeighborCount = 0;
                    if (i >= 2) { mean += dark_scan[i - 2]; validNeighborCount++; }
                    if (i >= 1) { mean += dark_scan[i - 1]; validNeighborCount++; }
                    if (i < data_array.Count - 1) { mean += dark_scan[i + 1]; validNeighborCount++; }
                    if (i < data_array.Count - 2) { mean += dark_scan[i + 2]; validNeighborCount++; }
                    mean /= validNeighborCount;

                    if (dark_scan[i] >= data_array[i])
                    {
                        bad_pixels[i] = true;
                    }
                    else if ((data_array[i] == 65535) && dark_scan[i] > 1.4 * mean)
                    {
                        bad_pixels[i] = true;
                    }
                    else
                    {
                        data_array[i] -= dark_scan[i];
                    }
                }

                for (int i = 0; i < bad_pixels.Length; i++)
                {
                    if (bad_pixels[i])
                    {
                        int start_i = i;
                        while (i + 1 < bad_pixels.Length && bad_pixels[i + 1])
                        {
                            i++;
                        }

                        if (i + 1 < data_array.Count && start_i > 0)
                        {
                            double factor = (data_array[i + 1] - data_array[start_i - 1]) / (i - start_i + 2);
                            for (int j = start_i; j <= i; j++)
                            {
                                data_array[j] = (j == start_i) ? data_array[start_i - 1] + factor : data_array[j - 1] + factor;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Acquires a dark scan from the spectrometer.
        /// </summary>
        public void GetDarkScan()
        {
            ushort[] pArray = new ushort[pixel_number];
            int ret = BwtekAPIWrapper.bwtekFrameDataReadUSB(1, 0, pArray, channel);
            if (ret != pixel_number)
            {
                Debug.WriteLine("GetDarkScan: Received: " + ret + " pixels");
                throw new Exception("Not received data");
            }
            _ = BwtekAPIWrapper.bwtekStopIntegration(channel);

            dark_scan = new List<double>();
            for (int i = xaxis_min; i <= xaxis_max; i++)
            {
                dark_scan.Add((double)pArray[i]);
            }

            dark_scan_taken = true;
            Debug.WriteLine("Received dark scan");
        }

        /// <summary>
        /// Loads a dark scan from a CSV file.
        /// </summary>
        public void LoadDarkScanFromFile()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Open CSV File",
                Filter = "CSV file (*.csv)|*.csv| All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                string file_path = dialog.FileName;
                dark_scan = new List<double>();
                using var reader = new StreamReader(file_path);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    dark_scan.Add(Convert.ToUInt16(line));
                }
            }
            dark_scan_taken = true;
            Debug.WriteLine("Read dark scan from file");
        }

        /// <summary>
        /// Saves the current dark scan to a CSV file.
        /// </summary>
        public void SaveDarkScanToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog()
            {
                Filter = "CSV file (*.csv)|*.csv",
                FilterIndex = 1,
                RestoreDirectory = true,
                DefaultExt = ".csv",
                FileName = "dark"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var csv = new StringBuilder();
                Debug.WriteLine("dark_scan: " + dark_scan.Count);
                for (int i = 0; i < dark_scan.Count; i++)
                {
                    var first = dark_scan[i].ToString(CultureInfo.InvariantCulture);
                    var newLine = $"{first}";
                    csv.AppendLine(newLine);
                }
                File.WriteAllText(saveFileDialog.FileName, csv.ToString());
            }
        }

        /// <summary>
        /// Applies smoothing to a given spectrum.
        /// </summary>
        /// <param name="smoothing">Smoothing configuration.</param>
        /// <param name="spectrum">Spectrum to be smoothed.</param>
        /// <returns>Smoothed spectrum.</returns>
        public Spectrum Smoothing(Smoothing smoothing, Spectrum spectrum)
        {
            double[] pArray = spectrum.DataValues.ToArray();

            Debug.WriteLine("Before Smoothing");

            int ret = BwtekAPIWrapper.bwtekSmoothingUSB(smoothing.Type, smoothing.Parameter, pArray, spectrum.DataValues.Count);
            if (ret < 0)
            {
                throw new Exception("Smoothing failed");
            }
            Debug.WriteLine("Smoothing success");
            List<double> data_array = pArray.ToList().ConvertAll(x => (double)x);

            return new Spectrum(wavelengths, data_array);
        }

        /// <summary>
        /// Sets the integration time of the spectrometer.
        /// </summary>
        /// <param name="integration_time">Integration time to set.</param>
        public void SetIntegrationTime(int integration_time)
        {
            int new_integration_time = integration_time;
            if (IntegrationTimeUnit == IntegrationTimeUnit.Miliseconds)
            {
                new_integration_time *= 1000;
            }
            int ret = BwtekAPIWrapper.bwtekSetTimeUSB(new_integration_time, channel);

            if (ret != new_integration_time)
            {
                MessageBox.Show("Failed to set new integration time", "title", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IntegrationTime = integration_time;
        }

        /// <summary>
        /// Calculates the derivative of a given spectrum.
        /// </summary>
        /// <param name="spectrum">The spectrum for which to calculate the derivative.</param>
        /// <param name="derivative_config">Derivative configuration parameters.</param>
        /// <returns>The spectrum after derivative calculation.</returns>
        public Spectrum CalculateDerivative(Spectrum spectrum, DerivativeConfig derivative_config)
        {
            double[] pArray = spectrum.DataValues.ToArray();
            double[] result_array = new double[pArray.Length];
            int ret = BwtekAPIWrapper.bwtekConvertDerivativeDouble((int)derivative_config.DerivativeMethod, (derivative_config.WindowSize - 1) / 2, derivative_config.DegreeOfPolynomial, derivative_config.DerivativeOrder, pArray, result_array, spectrum.DataValues.Count);

            Spectrum retVal = new Spectrum(spectrum.Wavelengths, result_array.ToList());

            return retVal;
        }

        /// <summary>
        /// Reads configuration and calibration information from the EEPROM of the device.
        /// Populates various configuration properties based on the information read.
        /// </summary>
        private void ReadEeprom()
        {
            _ = BwtekAPIWrapper.bwtekReadEEPROMUSB(eeprom_filename, channel);

            var iniFile = new IniFile(eeprom_filename);

            string model = iniFile.Read("model", "COMMON");
            config_properties.Add(new ConfigProperty("model", model));
            config_properties.Add(new ConfigProperty("c_code", iniFile.Read("c_code", "COMMON")));
            config_properties.Add(new ConfigProperty("Vid&Pid", iniFile.Read("VID and PID", "EEPROM")));
            config_properties.Add(new ConfigProperty("Manufacture date", iniFile.Read("Manufacture date", "EEPROM")));
            config_properties.Add(new ConfigProperty("AD Frequency", iniFile.Read("AD Frequency", "EEPROM")));
            config_properties.Add(new ConfigProperty("Slit size", iniFile.Read("Slit size", "EEPROM")));
            config_properties.Add(new ConfigProperty("DLL Vversion", iniFile.Read("DLL Version", "EEPROM")));
            config_properties.Add(new ConfigProperty("USB type", usb_type.ToString()));

            pixel_number = Convert.ToInt32(iniFile.Read("pixelnumber", "BRC115"));
            config_properties.Add(new ConfigProperty("Pixel Number", pixel_number.ToString()));


            int integration_time_unit = Convert.ToInt32(iniFile.Read("inttime_unit", model));
            IntegrationTimeUnit = integration_time_unit switch
            {
                0 => IntegrationTimeUnit.Microseconds,
                1 => IntegrationTimeUnit.Miliseconds,
                _ => IntegrationTimeUnit // default value or throw exception if needed
            };
            integration_time_min = Convert.ToInt32(iniFile.Read("inttime_min", model));
            IntegrationTime = integration_time_min;
            config_properties.Add(new ConfigProperty("inttime_min", integration_time_min.ToString()));

            config_properties.Add(new ConfigProperty("Integration Time Unit", IntegrationTimeUnitStr));

            timing_mode = Convert.ToInt32(iniFile.Read("timing_mode", model));
            config_properties.Add(new ConfigProperty("timing_mode", timing_mode.ToString()));

            input_mode = Convert.ToInt32(iniFile.Read("input_mode", model));
            config_properties.Add(new ConfigProperty("input_mode", input_mode.ToString()));

            xaxis_max = Convert.ToInt32(iniFile.Read("xaxis_max", "COMMON"));
            xaxis_min = Convert.ToInt32(iniFile.Read("xaxis_min", "COMMON"));

            double a0_coefficient = double.Parse(iniFile.Read("coefs_a0", "COMMON"), CultureInfo.InvariantCulture);
            double a1_coefficient = double.Parse(iniFile.Read("coefs_a1", "COMMON"), CultureInfo.InvariantCulture);
            double a2_coefficient = double.Parse(iniFile.Read("coefs_a2", "COMMON"), CultureInfo.InvariantCulture);
            double a3_coefficient = double.Parse(iniFile.Read("coefs_a3", "COMMON"), CultureInfo.InvariantCulture);

            for (int i = xaxis_min; i <= xaxis_max; i++)
            {
                wavelengths.Add(a0_coefficient + a1_coefficient * i + a2_coefficient * Math.Pow(i, 2) + a3_coefficient * Math.Pow(i, 3));
            }
        }

        /// <summary>
        /// Generates a dummy spectrum for testing purposes.
        /// </summary>
        /// <returns>A dummy spectrum with random data.</returns>
        public Spectrum GenerateDummySpectrum()
        {
            List<double> dummy_wavelengths = new List<double>();
            List<double> dummy_data_array = new List<double>();
            Random r = new Random();
            for (int i = 1; i < 200; i++)
            {
                dummy_wavelengths.Add((double)i);
                dummy_data_array.Add((double)r.NextDouble());
            }
            return new Spectrum(dummy_wavelengths, dummy_data_array);
        }

    }
}
