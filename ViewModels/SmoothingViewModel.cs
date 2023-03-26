using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    public class SmoothingViewModel : Screen
    {

        private Smoothing smoothing;

        public Smoothing Smoothing
        {
            get { return smoothing; }
            set { smoothing = value; }
        }

        private bool perform_smoothing;

        public bool PerformSmoothing
        {
            get { return perform_smoothing; }
            set { perform_smoothing = value; }
        }

        private bool create_new_spectrum;

        public bool CreateNewSpectrum
        {
            get { return create_new_spectrum; }
            set { create_new_spectrum = value; }
        }


        public SmoothingViewModel()
        {
            smoothing = new Smoothing();
            smoothing.BoxcarSmoothing = true;
        }

        public void CloseWindow()
        {
            if (Smoothing.BoxcarSmoothing && (Smoothing.BoxCarWindow < 1 || Smoothing.BoxCarWindow > 1023))
            {
                MessageBox.Show("Incorrect BoxCar window, please enter correct window in range (1-1023)");
            }
            else if (Smoothing.FftSmoothing && (Smoothing.FftSmoothingDegree < 1 || Smoothing.FftSmoothingDegree > 99))
            {
                MessageBox.Show("Incorrect FFT smoothing degree, please enter correct degree in range (1-99%)");
            }
            else if (Smoothing.SavGolaySmoothing && (Smoothing.SavGolayWindow < 2 || Smoothing.SavGolayWindow > 5))
            {
                MessageBox.Show("Incorrect Savitzky-Golay window, please enter correct degree in range (2-5)");
            }
            else
            {
                PerformSmoothing = true;
                TryCloseAsync();
            }

        }
        public void CancelWindow()
        {
            PerformSmoothing = false;
            TryCloseAsync();
        }
    }
}
