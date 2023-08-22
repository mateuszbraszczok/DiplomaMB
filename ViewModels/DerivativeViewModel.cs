using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    public class DerivativeViewModel : Screen
    {
        private DerivativeConfig derivative_config;
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
        public Spectrum Spectrum
        {
            get => spectrum;
            set => spectrum = value;
        }

        private Spectrum result_spectrum;
        public Spectrum ResultSpectrum
        {
            get => result_spectrum;
            set => result_spectrum = value;
        }

        private ISpectrometer spectrometer;
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set => spectrometer = value;
        }

        private bool operation_done;
        public bool OperationDone
        {
            get => operation_done;
            set => operation_done = value;
        }

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

        public void CloseWindow()
        {
            if (derivative_config.WindowSize <= derivative_config.DegreeOfPolynomial)
            {
                MessageBox.Show("Window size should be bigger than degree of polynomial");
                return;
            }
            result_spectrum = spectrometer.CalculateDerivative(spectrum, derivative_config);
            result_spectrum.Name = spectrum.Name + (derivative_config.DerivativeOrder == 1 ? "1st_derivative" : "2nd_derivative");

            operation_done = true;
            TryCloseAsync();
        }

        public void CancelWindow()
        {
            TryCloseAsync();
        }
    }
}
