/**
 * @file SmoothingViewModel.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief ViewModel for the Smoothing settings.
 */

using Caliburn.Micro;
using DiplomaMB.Models;

namespace DiplomaMB.ViewModels
{
    /// <summary>
    /// ViewModel for managing the smoothing settings.
    /// </summary>
    public class SmoothingViewModel : Screen
    {
        private Smoothing smoothing;
        /// <summary>
        /// Gets or sets the Smoothing settings.
        /// </summary>
        public Smoothing Smoothing
        {
            get => smoothing;
            set
            {
                smoothing = value;
                NotifyOfPropertyChange(() => Smoothing);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmoothingViewModel"/> class.
        /// </summary>
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

        /// <summary>
        /// Closes the smoothing settings window and saves the changes.
        /// </summary>
        public void CloseWindow()
        {
            smoothing.PerformSmoothing = true;
            TryCloseAsync();
        }

        /// <summary>
        /// Closes the smoothing settings window without saving the changes.
        /// </summary>
        public void CancelWindow()
        {
            TryCloseAsync();
        }
    }
}
