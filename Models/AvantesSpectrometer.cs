using Caliburn.Micro;
using System.Collections.Generic;

namespace DiplomaMB.Models
{
    public class AvantesSpectrometer : ISpectrometer
    {
        public bool Connected { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int IntegrationTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public int IntegrationTimeMin { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string Status => throw new System.NotImplementedException();

        public bool DarkScanTaken => throw new System.NotImplementedException();

        public BindableCollection<ConfigProperty> ConfigProperties { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
        public IntegrationTimeUnit IntegrationTimeUnit { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public string IntegrationTimeUnitStr => throw new System.NotImplementedException();

        public Spectrum CalculateDerivative(Spectrum spectrum, DerivativeConfig derivative_config)
        {
            throw new System.NotImplementedException();
        }

        public void Connect()
        {
            throw new System.NotImplementedException();
        }

        public void Disconnect()
        {
            throw new System.NotImplementedException();
        }

        public void GetDarkScan()
        {
            throw new System.NotImplementedException();
        }

        public void LoadDarkScanFromFile()
        {
            throw new System.NotImplementedException();
        }

        public List<Spectrum> ReadData(int frames_to_acquire, bool new_id = true)
        {
            throw new System.NotImplementedException();
        }

        public Spectrum ReadDataSmart(SmartRead smart_read)
        {
            throw new System.NotImplementedException();
        }

        public void ResetDevice()
        {
            throw new System.NotImplementedException();
        }

        public void SaveDarkScanToFile()
        {
            throw new System.NotImplementedException();
        }

        public void SetIntegrationTime(int integration_time)
        {
            throw new System.NotImplementedException();
        }

        public Spectrum Smoothing(Smoothing smoothing, Spectrum spectrum)
        {
            throw new System.NotImplementedException();
        }
    }
}
