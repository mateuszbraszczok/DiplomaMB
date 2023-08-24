using Caliburn.Micro;

namespace DiplomaMB.Models
{
    public enum DerivativeMethod
    {
        Point_Diff,
        Savitzky_Golay
    }

    public class DerivativeConfig : PropertyChangedBase
    {
        private bool perform_derivative;
        public bool PerformDerivative
        {
            get => perform_derivative;
            set => perform_derivative = value;
        }

        private int degree_of_polynomial;
        public int DegreeOfPolynomial
        {
            get => degree_of_polynomial;
            set { degree_of_polynomial = value; }
        }

        private int derivative_order;
        public int DerivativeOrder
        {
            get => derivative_order;
            set { derivative_order = value; }
        }

        private int window_size;
        public int WindowSize
        {
            get => window_size;
            set
            {
                if (value % 2 == 0)
                {
                    window_size = value + 1;
                }
                else
                {
                    window_size = value;
                }
                NotifyOfPropertyChange(() => WindowSize);
            }
        }

        private DerivativeMethod derivative_method;
        public DerivativeMethod DerivativeMethod
        {
            get => derivative_method;
            set
            {
                derivative_method = value;
                NotifyOfPropertyChange(() => DerivativeMethod);
                NotifyOfPropertyChange(() => IsPointDiffEnabled);
                NotifyOfPropertyChange(() => IsSavitzkyGolayEnabled);
            }
        }

        public bool IsPointDiffEnabled
        {
            get => (DerivativeMethod == DerivativeMethod.Point_Diff);
        }

        public bool IsSavitzkyGolayEnabled
        {
            get => (DerivativeMethod == DerivativeMethod.Savitzky_Golay);
        }

        public DerivativeConfig()
        {

        }

    }
}
