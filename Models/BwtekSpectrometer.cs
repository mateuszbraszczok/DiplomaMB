using Caliburn.Micro;
using DiplomaMB.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Windows;

namespace DiplomaMB.Models
{
    public class BwtekSpectrometer : ISpectrometer
    {
        private BindableCollection<ConfigProperty> config_properties;
        public BindableCollection<ConfigProperty> ConfigProperties
        {
            get => config_properties;
            set => config_properties = value;
        }

        public bool Connected { get; set; }

        private int integration_time;
        public int IntegrationTime
        {
            get => integration_time;
            set => integration_time = value;
        }

        private int integration_time_min;
        public int IntegrationTimeMin
        {
            get => integration_time_min;
            set => integration_time_min = value;
        }

        public IntegrationTimeUnit IntegrationTimeUnit { get; set; }
        public string IntegrationTimeUnitStr
        {
            get => IntegrationTimeUnit == IntegrationTimeUnit.Microseconds ? "\xE6s" : "ms";
        }

        private string status;
        public string Status
        {
            get => status;
        }

        private bool dark_scan_taken;
        public bool DarkScanTaken
        {
            get => dark_scan_taken;
        }

        private int usb_type;
        private int channel;

        private int pixel_number;
        private int timing_mode;
        private int input_mode;

        private int xaxis_min;
        private int xaxis_max;

        private readonly string eeprom_filename = $"{Assembly.GetEntryAssembly().Location}\\..\\para.ini";

        private List<double> wavelengths;

        private List<double> dark_scan;


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

        public void Disconnect()
        {
            _ = BwtekAPIWrapper.bwtekCloseUSB(channel);
            _ = BwtekAPIWrapper.CloseDevices();
            Connected = false;
            status = "Disconnected";
        }

        public void ResetDevice()
        {
            int ret = BwtekAPIWrapper.bwtekSoftReset_CEnP(channel);
            if (ret < 0)
            {
                MessageBox.Show("Failed to reset device", "Reset fail", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                if (i > xaxis_min && i <= xaxis_max+1)
                {
                    data_list.Add((double)value);
                }
                i++;
            }
            return spectrum_list;
        }

        public Spectrum ReadDataSmart(SmartRead smart_read)
        {
            ushort[] pArray = new ushort[pixel_number];
            int ret = BwtekAPIWrapper.bwtekDSPDataReadUSB(smart_read.SpectrumsToAverage, Convert.ToInt32(smart_read.Smoothing), Convert.ToInt32(smart_read.DarkCompensation), 0, pArray, channel);

            if (ret != pixel_number)
            {
                Debug.WriteLine($"ReadDataSmart: Received: { ret} pixels");
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

        private void SubtractDarkScan(List<double> data_array)
        {
            if (dark_scan_taken)
            {
                Debug.WriteLine($"SubtractDarkScan: Data array count: {data_array.Count} Dark count: {dark_scan.Count}");

                bool[] bad_pixels = new bool[data_array.Count];
                for (int i = 0; i < data_array.Count; i++)
                {
                    double mean = (dark_scan[i - 2] + dark_scan[i - 1] + dark_scan[i + 1] + dark_scan[i + 2]) / 4;
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
                        while (bad_pixels[i+1])
                        {
                            i++;
                        }
                        double factor = (data_array[i+1] - data_array[start_i - 1]) / (i - start_i + 1);
                        for (int j = start_i; j <= i ; j++)
                        {
                            data_array[j] = data_array[j - 1] + factor;   
                        }
                    }
                }

            }
        }

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

        public void LoadDarkScan()
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

        public void SaveDarkScan()
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

        public Spectrum CalculateDerivative(int order, int half_point, Spectrum spectrum)
        {
            double[] pArray = spectrum.DataValues.ToArray();
            double[] result_array = new double[pArray.Length];
            int ret = BwtekAPIWrapper.bwtekConvertDerivativeDouble(1, half_point, 3, order, pArray, result_array, spectrum.DataValues.Count);

            Spectrum retVal = new Spectrum(spectrum.Wavelengths, result_array.ToList());

            return retVal;

        }


        private void ReadEeprom()
        {
            int ret = BwtekAPIWrapper.bwtekReadEEPROMUSB(eeprom_filename, channel);

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
