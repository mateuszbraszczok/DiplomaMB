using Caliburn.Micro;

namespace DiplomaMB.Models
{
    public enum SmoothingType
    {
        Fft,
        SavGolay,
        BoxCar
    }

    public class Smoothing : PropertyChangedBase
    {
        private bool perform_smoothing;
        public bool PerformSmoothing
        {
            get => perform_smoothing;
            set => perform_smoothing = value;
        }

        private bool create_new_spectrum;
        public bool CreateNewSpectrum
        {
            get => create_new_spectrum;
            set => create_new_spectrum = value;
        }

        private int box_car_window;
        public int BoxCarWindow
        {
            get => box_car_window;
            set { box_car_window = value; }
        }

        private int sav_golay_window;
        public int SavGolayWindow
        {
            get => sav_golay_window;
            set { sav_golay_window = value; }
        }

        private int fft_smoothing_degree;
        public int FftSmoothingDegree
        {
            get => fft_smoothing_degree;
            set { fft_smoothing_degree = value; }
        }

        private SmoothingType smoothing_type;
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

        public bool IsBoxCarEnabled
        {
            get => (SmoothingType == SmoothingType.BoxCar);
        }

        public bool IsFftEnabled
        {
            get => (SmoothingType == SmoothingType.Fft);
        }

        public bool IsSavGolayEnabled
        {
            get => (SmoothingType == SmoothingType.SavGolay);
        }


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
