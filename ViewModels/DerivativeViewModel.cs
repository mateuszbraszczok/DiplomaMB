/**
 * @file DerivativeViewModel.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief ViewModel for derivative configuration.
 */

using Caliburn.Micro;
using DiplomaMB.Models;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    /// <summary>
    /// ViewModel for managing derivative configuration.
    /// </summary>
    public class DerivativeViewModel : Screen
    {
        private DerivativeConfig derivative_config;
        /// <summary>
        /// Gets or sets the DerivativeConfig settings.
        /// </summary>
        public DerivativeConfig DerivativeConfig
        {
            get => derivative_config;
            set
            {
                derivative_config = value;
                NotifyOfPropertyChange(() => DerivativeConfig);
            }
        }

        private Spectrum spectrum;
        /// <summary>
        /// Gets or sets the spectrum.
        /// </summary>
        public Spectrum Spectrum
        {
            get => spectrum;
            set => spectrum = value;
        }

        private Spectrum result_spectrum;
        /// <summary>
        /// Gets or sets the result spectrum.
        /// </summary>
        public Spectrum ResultSpectrum
        {
            get => result_spectrum;
            set => result_spectrum = value;
        }

        private ISpectrometer spectrometer;
        /// <summary>
        /// Gets or sets the spectrometer.
        /// </summary>
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set => spectrometer = value;
        }

        private bool operation_done;
        /// <summary>
        /// Gets or sets the operation_done flag.
        /// </summary>
        public bool OperationDone
        {
            get => operation_done;
            set => operation_done = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativeViewModel"/> class.
        /// </summary>
        /// <param name="_spectrum">The spectrum.</param>
        /// <param name="_spectrometer">The spectrometer.</param>
        public DerivativeViewModel(Spectrum _spectrum, ISpectrometer _spectrometer)
        {
            derivative_config = new DerivativeConfig
            {
                DegreeOfPolynomial = 3,
                DerivativeOrder = 1,
                WindowSize = 7,
            };
            spectrum = _spectrum;
            spectrometer = _spectrometer;

            operation_done = false;
        }

        /// <summary>
        /// Closes the derivative configuration window and performs the derivative operation.
        /// </summary>
        public void CloseWindow()
        {
            if (derivative_config.WindowSize <= derivative_config.DegreeOfPolynomial)
            {
                MessageBox.Show("Window size should be bigger than degree of polynomial");
                return;
            }
            result_spectrum = spectrometer.CalculateDerivative(spectrum, derivative_config);
            result_spectrum.Name = spectrum.Name + "_" + (derivative_config.DerivativeOrder == 1 ? "1st" : "2nd") + "_derivative";

            operation_done = true;
            TryCloseAsync();
        }

        /// <summary>
        /// Closes the derivative configuration window without performing any operations.
        /// </summary>
        public void CancelWindow()
        {
            TryCloseAsync();
        }
    }
}
