using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.Models
{
    public enum IntegrationTimeUnit
    {
        Miliseconds,
        Microseconds
    }

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

    public interface ISpectrometer
    {
        public bool Connected { get; set; }
        public int IntegrationTime { get; set; }
        public int IntegrationTimeMin { get; set; }
        public string Status { get; }
        public bool DarkScanTaken { get; }

        public BindableCollection<ConfigProperty> ConfigProperties { get; set; }

        public IntegrationTimeUnit IntegrationTimeUnit{ get; set; }

        public string IntegrationTimeUnitStr { get; }

        public void Connect();

        public void Disconnect();

        public void ResetDevice();

        public List<Spectrum> ReadData(int frames_to_acquire);

        public Spectrum ReadDataSmart(SmartRead smart_read);

        public void GetDarkScan();

        public void LoadDarkScan();

        public void SaveDarkScan();

        public Spectrum Smoothing(Smoothing smoothing, Spectrum spectrum);

        public void SetIntegrationTime(int integration_time);

        public Spectrum CalculateDerivative(Spectrum spectrum, DerivativeConfig derivative_config);
    }
}
