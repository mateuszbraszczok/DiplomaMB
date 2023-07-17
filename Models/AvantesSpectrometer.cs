using DiplomaMB.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaMB.Models
{
    public class AvantesSpectrometer
    {
        private int m_DeviceHandle;
        private ushort m_pixelNumber;
        private AvantesAPIWrapper.PixelArrayType m_Lambda;

        private string integration_time_unit_string;
        public string IntegrationTimeUnit
        {
            get { return integration_time_unit_string; }
            set { integration_time_unit_string = value; }
        }

        private int integration_time;
        public int IntegrationTime
        {
            get { return integration_time; }
            set { integration_time = value; }
        }

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

        private List<double> dark_scan;
        public List<double> DarkScan
        {
            get { return dark_scan; }
            set { dark_scan = value; }
        }

        private bool dark_scan_taken;
        public bool DarkScanTaken
        {
            get { return dark_scan_taken; }
            set { dark_scan_taken = value; }
        }

        public AvantesSpectrometer()
        {
            connected = false;
            dark_scan_taken = false;
            status = "Disconnected";

            IntegrationTimeUnit = string.Empty;
        }

        public void Connect()
        {
            Connected = false;
            Status = "Connection failed";
            int l_Port = AvantesAPIWrapper.AVS_Init(0);
            if (l_Port >= 1)
            {

                uint l_RequiredSize = 0;
                uint l_Size = ((uint)l_Port) * (uint)Marshal.SizeOf(typeof(AvantesAPIWrapper.AvsIdentityType));

                AvantesAPIWrapper.AvsIdentityType[] l_Id = new AvantesAPIWrapper.AvsIdentityType[l_Port];
                int l_NrDevices = AvantesAPIWrapper.AVS_GetList(l_Size, ref l_RequiredSize, l_Id);
                int l_hDevice = AvantesAPIWrapper.AVS_Activate(ref l_Id[0]);
                m_DeviceHandle = l_hDevice;
                if (AvantesAPIWrapper.AVS_UseHighResAdc(m_DeviceHandle, true) != AvantesAPIWrapper.ERR_SUCCESS)
                {
                    MessageBox.Show("High Res mode not supported", "Avantes");
                }

                AvantesAPIWrapper.AVS_GetNumPixels(m_DeviceHandle, ref m_pixelNumber);
                if (AvantesAPIWrapper.AVS_GetLambda(m_DeviceHandle, ref m_Lambda) == AvantesAPIWrapper.ERR_SUCCESS)
                {
                    //chart1.ChartAreas[0].AxisX.Minimum = m_Lambda.Value[0];
                    //chart1.ChartAreas[0].AxisX.Maximum = m_Lambda.Value[m_NrPixels - 1];
                    //chart1.ChartAreas[0].AxisX.LabelStyle.Format = "{0.0}";
                    MessageBox.Show("Port successfully opened");
                    //int beat = AvantesAPIWrapper.AVS_Heartbeat();
                    connected = true;
                    status = "Connected";
                }
                else
                {
                    MessageBox.Show("AVS_GetLambda failed", "Avantes");
                }
                    
            }
            else
            {
                MessageBox.Show("Error opening port!", "Avantes");
            }
                
        }

        public void Disconnect()
        {
            _ = AvantesAPIWrapper.AVS_Deactivate(m_DeviceHandle);
            _ = AvantesAPIWrapper.AVS_Done();
            connected = false;
            status = "Disconnected";
        }

        public void ResetDevice()
        {

        }
        public List<Spectrum> ReadData(int frames_to_acquire)
        {
            List<Spectrum> spectrum_list = new();
            return spectrum_list;
        }

        public Spectrum ReadDataSmart(SmartRead smart_read)
        {
            //ushort[] pArray = new ushort[pixel_number];
            //int ret = BwtekAPIWrapper.bwtekDSPDataReadUSB(smart_read.SpectrumsToAverage, Convert.ToInt32(smart_read.Smoothing), Convert.ToInt32(smart_read.DarkCompensation), 0, pArray, channel);

            //if (ret != pixel_number)
            //{
            //    Debug.WriteLine("ReadDataSmart: Received: " + ret + " pixels");
            //    throw new Exception("Not received data");
            //}
            //_ = BwtekAPIWrapper.bwtekStopIntegration(channel);

            DataArray = new List<double>();
            //for (int i = xaxis_min; i <= xaxis_max; i++)
            //{
            //    DataArray.Add((double)pArray[i]);
            //}
            //SubtractDarkScan();

            return new Spectrum(Wavelengths, DataArray);
        }

        public void GetDarkScan()
        {
            //ushort[] pArray = new ushort[pixel_number];
            //int ret = BwtekAPIWrapper.bwtekFrameDataReadUSB(1, 0, pArray, channel);
            //if (ret != pixel_number)
            //{
            //    Debug.WriteLine("GetDarkScan: Received: " + ret + " pixels");
            //    throw new Exception("Not received data");
            //}
            //_ = BwtekAPIWrapper.bwtekStopIntegration(channel);

            //DarkScan = new List<double>();
            //for (int i = xaxis_min; i <= xaxis_max; i++)
            //{
            //    DarkScan.Add((double)pArray[i]);
            //}

            DarkScanTaken = true;
        }

        public Spectrum Smoothing(Smoothing smoothing, Spectrum spectrum)
        {
            double[] pArray = spectrum.DataArray.ToArray();

            //int ret = BwtekAPIWrapper.bwtekSmoothingUSB(smoothing.Type, smoothing.Parameter, pArray, spectrum.DataArray.Count);
            //if (ret < 0)
            //{
            //    throw new Exception("Smoothing failed");
            //}
            Debug.WriteLine("Smoothing success");
            DataArray = pArray.ToList().ConvertAll(x => (double)x);

            return new Spectrum(Wavelengths, DataArray);
        }

        public void SetIntegrationTime(int integration_time)
        {
            //int new_integration_time = integration_time;
            //if (integration_time_unit == 1)
            //{
            //    new_integration_time *= 1000;
            //}
            //int ret = BwtekAPIWrapper.bwtekSetTimeUSB(new_integration_time, channel);

            //if (ret != new_integration_time)
            //{
            //    MessageBox.Show("Failed to set new integration time", "title", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}

            IntegrationTime = integration_time;
        }
    }

   
}
