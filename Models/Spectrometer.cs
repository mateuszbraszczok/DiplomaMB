using OxyPlot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using DiplomaMB.Utils;

namespace DiplomaMB.Models
{
    public class Spectrometer
    {
        public class ConfigProperty
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public ConfigProperty(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }
        private List<ConfigProperty> config_properties;

        public List<ConfigProperty> ConfigProperties
        {
            get { return config_properties; }
            set { config_properties = value; }
        }

        private int usb_type;
        private int channel;

        private string c_code;
        private string model;

        private int pixel_number;

        private double a0_coefficient;
        private double a1_coefficient;
        private double a2_coefficient;
        private double a3_coefficient;

        private int integration_time_unit;
        private string integration_time_unit_string;
        public string IntegrationTimeUnit
        {
            get { return integration_time_unit_string; }
            set { integration_time_unit_string = value; }
        }
        private int integration_time_min;
        private int timing_mode;
        private int input_mode;
        private int integration_time;
        public int IntegrationTime
        {
            get { return integration_time; }
            set { integration_time = value; }
        }


        private string eeprom_filename = Assembly.GetEntryAssembly().Location + "\\..\\para.ini";

        private bool connected;
        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        private List<double> wavelengths = new List<double>();
        private List<ushort> dataArray = new List<ushort>();
        private List<ushort> darkScan = new List<ushort>();

        private bool dark_scan_taken;
        public bool DarkScanTaken
        {
            get { return dark_scan_taken; }
            set { dark_scan_taken = value; }
        }


        public Spectrometer()
        {
            c_code = string.Empty;
            model = string.Empty;
            connected = false;
            dark_scan_taken = false;
            status = "Disconnected";

            IntegrationTimeUnit = string.Empty;
            config_properties = new List<ConfigProperty>();
        }

        public void Connect()
        {
            connected = false;
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

                MessageBox.Show("Test Usb success");

                int new_integration_time = integration_time_min;
                if (integration_time_unit == 1)
                {
                    new_integration_time *= 1000;
                }
                ret = BwtekAPIWrapper.bwtekSetTimeUSB(new_integration_time, channel);
                if (ret != new_integration_time) { break; }

                int lTriggerExit = 0;
                ret = BwtekAPIWrapper.bwtekSetTimingsUSB(lTriggerExit, 1, channel);
                if (ret != lTriggerExit) { break; }
                connected = true;
                status = "Connected";
            }
            while (false);
        }

        public void Disconnect()
        {
            _ = BwtekAPIWrapper.bwtekCloseUSB(channel);
            _ = BwtekAPIWrapper.CloseDevices();
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
            if (ret == pixel_number)
            {
                throw new Exception("Not received data");
            }
            ret = BwtekAPIWrapper.bwtekStopIntegration(channel);

            ushort[] data_array = new ushort[pixel_number];

            List<Spectrum> spectrum_list = new();
            int i = 1;
            foreach (ushort value in pArray)
            {
                if (i == pixel_number)
                {
                    i = 1;
                    dataArray = data_array.ToList();
                    SubtractDarkScan();
                    spectrum_list.Add(new Spectrum(wavelengths, dataArray));
                    data_array = new ushort[pixel_number];
                }

                data_array[i] = value;

                i++;
            }
            return spectrum_list;
        }



        public Spectrum ReadDataSmart(int spectrums_to_average, bool smoothing, bool dark_compensation)
        {
            ushort[] pArray = new ushort[pixel_number];
            int ret = BwtekAPIWrapper.bwtekDSPDataReadUSB(spectrums_to_average, Convert.ToInt32(smoothing), Convert.ToInt32(dark_compensation), 0, pArray, channel);

            if (ret == pixel_number)
            {
                throw new Exception("Not received data");
            }
            ret = BwtekAPIWrapper.bwtekStopIntegration(channel);

            dataArray = pArray.ToList();
            SubtractDarkScan();

            return new Spectrum(wavelengths, dataArray);
        }

        private void SubtractDarkScan()
        {
            if (dark_scan_taken)
            {
                for (int i = 0; i < dataArray.Count; i++)
                {
                    dataArray[i] -= darkScan[i];
                }
            }

        }

        public void GetDarkScan()
        {
            ushort[] pArray = new ushort[pixel_number];
            int ret = BwtekAPIWrapper.bwtekFrameDataReadUSB(1, 0, pArray, channel);
            if (ret == pixel_number)
            {
                throw new Exception("Not received data");
            }
            ret = BwtekAPIWrapper.bwtekStopIntegration(channel);

            darkScan = pArray.ToList();

            dark_scan_taken = true;
        }

        public void SetIntegrationTime(string integration_time)
        {
            int new_integration_time = Int32.Parse(integration_time);
            if (integration_time_unit == 1)
            {
                new_integration_time *= 1000;
            }
            int ret = BwtekAPIWrapper.bwtekSetTimeUSB(new_integration_time, channel);

            if (ret != new_integration_time)
            {
                MessageBox.Show("Failed to set new integration time", "title", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IntegrationTime = Int32.Parse(integration_time);
        }

        private void ReadEeprom()
        {
            int ret = BwtekAPIWrapper.bwtekReadEEPROMUSB(eeprom_filename, channel);

            var iniFile = new IniFile(eeprom_filename);

            model = iniFile.Read("model", "COMMON");
            config_properties.Add(new ConfigProperty("model", model));
            c_code = iniFile.Read("c_code", "COMMON");
            config_properties.Add(new ConfigProperty("c_code", c_code));

            pixel_number = Convert.ToInt32(iniFile.Read("pixelnumber", "BRC115"));
            config_properties.Add(new ConfigProperty("pixelNumber", pixel_number.ToString()));

            integration_time_min = Convert.ToInt32(iniFile.Read("inttime_min", model));
            config_properties.Add(new ConfigProperty("inttime_min", integration_time_min.ToString()));

            IntegrationTime = integration_time_min;

            integration_time_unit = Convert.ToInt32(iniFile.Read("inttime_unit", model));
            config_properties.Add(new ConfigProperty("inttime_unit", integration_time_unit.ToString()));

            if (integration_time_unit == 0)
            {
                IntegrationTimeUnit = "\xE6s";
            }
            else if (integration_time_unit == 1)
            {
                IntegrationTimeUnit = "ms";
            }

            timing_mode = Convert.ToInt32(iniFile.Read("timing_mode", model));
            config_properties.Add(new ConfigProperty("timing_mode", timing_mode.ToString()));

            input_mode = Convert.ToInt32(iniFile.Read("input_mode", model));
            config_properties.Add(new ConfigProperty("input_mode", input_mode.ToString()));


            a0_coefficient = double.Parse(iniFile.Read("coefs_a0", "COMMON"), CultureInfo.InvariantCulture);
            a1_coefficient = double.Parse(iniFile.Read("coefs_a1", "COMMON"), CultureInfo.InvariantCulture);
            a2_coefficient = double.Parse(iniFile.Read("coefs_a2", "COMMON"), CultureInfo.InvariantCulture);
            a3_coefficient = double.Parse(iniFile.Read("coefs_a3", "COMMON"), CultureInfo.InvariantCulture);

            for (int i = 0; i < pixel_number; i++)
            {
                wavelengths.Add(a0_coefficient + a1_coefficient * i + a2_coefficient * Math.Pow(i, 2) + a3_coefficient * Math.Pow(i, 3));
            }
        }

    }
}
