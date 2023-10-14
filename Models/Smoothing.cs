/**
 * @file Smoothing.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief Represents the parameters and options for performing data smoothing.
 */

using Caliburn.Micro;

namespace DiplomaMB.Models
{
    /// <summary>
    /// Defines the types of smoothing algorithms that can be used.
    /// </summary>
    public enum SmoothingType
    {
        Fft,
        SavGolay,
        BoxCar
    }

    /// <summary>
    /// Represents the parameters and options for performing data smoothing.
    /// </summary>
    public class Smoothing : PropertyChangedBase
    {
        private bool perform_smoothing;
        /// <summary>
        /// Gets or sets a value indicating whether smoothing should be performed.
        /// </summary>
        public bool PerformSmoothing
        {
            get => perform_smoothing;
            set => perform_smoothing = value;
        }

        private bool create_new_spectrum;
        /// <summary>
        /// Gets or sets a value indicating whether a new spectrum should be created after smoothing.
        /// </summary>
        public bool CreateNewSpectrum
        {
            get => create_new_spectrum;
            set => create_new_spectrum = value;
        }

        private int box_car_window;
        /// <summary>
        /// Gets or sets the window size for the BoxCar smoothing algorithm.
        /// </summary>
        public int BoxCarWindow
        {
            get => box_car_window;
            set { box_car_window = value; }
        }

        private int sav_golay_window;
        /// <summary>
        /// Gets or sets the window size for the Savitzky-Golay smoothing algorithm.
        /// </summary>
        public int SavGolayWindow
        {
            get => sav_golay_window;
            set { sav_golay_window = value; }
        }

        private int fft_smoothing_degree;
        /// <summary>
        /// Gets or sets the degree for FFT (Fast Fourier Transform) smoothing.
        /// </summary>
        public int FftSmoothingDegree
        {
            get => fft_smoothing_degree;
            set { fft_smoothing_degree = value; }
        }

        private SmoothingType smoothing_type;
        /// <summary>
        /// Gets or sets the type of smoothing algorithm to be used.
        /// </summary>
        public SmoothingType SmoothingType
        {
            get => smoothing_type;
            set
            {
                smoothing_type = value;
                NotifyOfPropertyChange(() => SmoothingType);
                NotifyOfPropertyChange(() => IsBoxCarEnabled);
                NotifyOfPropertyChange(() => IsFftEnabled);
                NotifyOfPropertyChange(() => IsSavGolayEnabled);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the BoxCar algorithm is enabled.
        /// </summary>
        public bool IsBoxCarEnabled
        {
            get => (SmoothingType == SmoothingType.BoxCar);
        }

        /// <summary>
        /// Gets a value indicating whether the FFT algorithm is enabled.
        /// </summary>
        public bool IsFftEnabled
        {
            get => (SmoothingType == SmoothingType.Fft);
        }

        /// <summary>
        /// Gets a value indicating whether the Savitzky-Golay algorithm is enabled.
        /// </summary>
        public bool IsSavGolayEnabled
        {
            get => (SmoothingType == SmoothingType.SavGolay);
        }

        /// <summary>
        /// Gets the parameter value for the selected smoothing algorithm.
        /// </summary>
        public int Parameter
        {
            get
            {
                if (IsBoxCarEnabled)
                {
                    return BoxCarWindow;
                }
                else if (IsSavGolayEnabled)
                {
                    return SavGolayWindow;
                }
                else if (IsFftEnabled)
                {
                    return FftSmoothingDegree;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets the type index for the selected smoothing algorithm.
        /// </summary>
        public int Type
        {
            get
            {
                if (IsBoxCarEnabled)
                {
                    return 0;
                }
                else if (IsFftEnabled)
                {
                    return 1;
                }
                else if (IsSavGolayEnabled)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

    }
}
