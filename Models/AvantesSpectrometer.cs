using DiplomaMB.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaMB.Models
{
    public class AvantesSpectrometer
    {
        private int m_DeviceHandle;
        private ushort m_pixelNumber;
        private AvantesAPIWrapper.PixelArrayType m_Lambda;
        public AvantesSpectrometer()
        {

        }

        public void Connect()
        {
            int l_Port = AvantesAPIWrapper.AVS_Init(0);
            if (l_Port >= 1)
            {
                //uint nr_devices = (uint) AvantesAPIWrapper.AVS_UpdateUSBDevices();
                //uint required_size = nr_devices * sizeof(AvsI)


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
                }
                else
                    MessageBox.Show("AVS_GetLambda failed", "Avantes");
            }
            else
            {
                MessageBox.Show("Error opening port!", "Avantes");
            }
                
        }

        public void Disconnect()
        {
            int l_Res = AvantesAPIWrapper.AVS_Deactivate(m_DeviceHandle);
            _ = AvantesAPIWrapper.AVS_Done();
        }

        public void ResetDevice()
        {

        }
        public List<Spectrum> ReadData(int frames_to_acquire)
        {
            List<Spectrum> spectrum_list = new();
            return spectrum_list;
        }
    }

   
}
