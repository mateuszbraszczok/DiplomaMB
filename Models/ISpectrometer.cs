/**
 * @file ISpectrometer.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief Provides the interface for interacting with different types of spectrometers.
 */

using Caliburn.Micro;
using System.Collections.Generic;

namespace DiplomaMB.Models
{
    /// <summary>
    /// Enum for specifying the unit of integration time.
    /// </summary>
    public enum IntegrationTimeUnit
    {
        Miliseconds,
        Microseconds
    }

    /// <summary>
    /// Represents a configuration property for a spectrometer.
    /// </summary>
    public class ConfigProperty
    {
        /// <summary>
        /// Gets or sets the name of the configuration property.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the configuration property.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigProperty"/> class.
        /// </summary>
        /// <param name="name">Name of the configuration property.</param>
        /// <param name="value">Value of the configuration property.</param>
        public ConfigProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// Interface representing a spectrometer device.
    /// </summary>
    public interface ISpectrometer
    {
        /// <summary>
        /// Gets or sets a value indicating whether the spectrometer is connected.
        /// </summary>
        public bool Connected { get; set; }

        /// <summary>
        /// Gets or sets the integration time for the spectrometer.
        /// </summary>
        public int IntegrationTime { get; set; }

        /// <summary>
        /// Gets the minimum allowable integration time for the spectrometer.
        /// </summary>
        public int IntegrationTimeMin { get; set; }

        /// <summary>
        /// Gets the current status of the spectrometer.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// Gets a value indicating whether a dark scan has been taken.
        /// </summary>
        public bool DarkScanTaken { get; }

        /// <summary>
        /// Gets or sets the configuration properties of the spectrometer.
        /// </summary>
        public BindableCollection<ConfigProperty> ConfigProperties { get; set; }

        /// <summary>
        /// Gets or sets the unit of measurement for integration time.
        /// </summary>
        public IntegrationTimeUnit IntegrationTimeUnit { get; set; }

        /// <summary>
        /// Gets the string representation of the unit of measurement for integration time.
        /// </summary>
        public string IntegrationTimeUnitStr { get; }

        /// <summary>
        /// Connects to the spectrometer.
        /// </summary>
        public void Connect();

        /// <summary>
        /// Disconnects from the spectrometer.
        /// </summary>
        public void Disconnect();

        /// <summary>
        /// Resets the spectrometer device.
        /// </summary>
        public void ResetDevice();

        /// <summary>
        /// Reads data from the spectrometer.
        /// </summary>
        /// <param name="frames_to_acquire">Number of frames to acquire.</param>
        /// <param name="new_id">A boolean indicating whether to generate a new ID for each acquired spectrum. Default is true.</param>
        /// <returns>A list of spectra.</returns>
        public List<Spectrum> ReadData(int frames_to_acquire, bool new_id = true);

        /// <summary>
        /// Reads data from the spectrometer using smart reading techniques.
        /// </summary>
        /// <param name="smart_read">Settings for smart reading.</param>
        /// <returns>A single spectrum.</returns>
        public Spectrum ReadDataSmart(SmartRead smart_read);

        /// <summary>
        /// Acquires a dark scan.
        /// </summary>
        public void GetDarkScan();

        /// <summary>
        /// Loads a previously acquired dark scan from a file.
        /// </summary>
        public void LoadDarkScanFromFile();

        /// <summary>
        /// Saves the current dark scan to a file.
        /// </summary>
        public void SaveDarkScanToFile();

        /// <summary>
        /// Performs smoothing on a given spectrum.
        /// </summary>
        /// <param name="smoothing">Smoothing parameters.</param>
        /// <param name="spectrum">The spectrum to smooth.</param>
        /// <returns>A smoothed spectrum.</returns>
        public Spectrum Smoothing(Smoothing smoothing, Spectrum spectrum);

        /// <summary>
        /// Sets the integration time of the spectrometer.
        /// </summary>
        /// <param name="integration_time">The new integration time.</param>
        public void SetIntegrationTime(int integration_time);

        /// <summary>
        /// Calculates the derivative of a given spectrum.
        /// </summary>
        /// <param name="spectrum">The spectrum to use.</param>
        /// <param name="derivative_config">Configuration for the derivative calculation.</param>
        /// <returns>A spectrum representing the derivative.</returns>
        public Spectrum CalculateDerivative(Spectrum spectrum, DerivativeConfig derivative_config);
    }
}
