using Caliburn.Micro;
using DiplomaMB.Models;
using System.Diagnostics;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    public class SmoothingViewModel : Screen
    {
        private Smoothing smoothing;
        public Smoothing Smoothing
        {
            get => smoothing;
            set 
            {
                smoothing = value;
                NotifyOfPropertyChange(() => Smoothing);
            }      
        }

        public SmoothingViewModel()
        {
            smoothing = new Smoothing
            {
                SmoothingType = SmoothingType.BoxCar,
                BoxCarWindow = 1,
                FftSmoothingDegree = 1,
                SavGolayWindow = 2,
                PerformSmoothing = false
            };
        }

        public void CloseWindow()
        {
            smoothing.PerformSmoothing = true;
            TryCloseAsync();
        }

        public void CancelWindow()
        {
            TryCloseAsync();
        }
    }
}
