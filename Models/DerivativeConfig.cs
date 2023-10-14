/**
 * @file DerivativeConfig.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief DerivativeConfig class for configuring derivative calculations on a spectrum.
 */

using Caliburn.Micro;

namespace DiplomaMB.Models
{
    /// <summary>
    /// Enum for types of derivative methods.
    /// </summary>
    public enum DerivativeMethod
    {
        Point_Diff,
        Savitzky_Golay
    }

    /// <summary>
    /// Configuration model for calculating the derivative of a spectrum.
    /// </summary>
    public class DerivativeConfig : PropertyChangedBase
    {
        private bool perform_derivative;
        /// <summary>
        /// Gets or sets whether to perform derivative calculation.
        /// </summary>
        public bool PerformDerivative
        {
            get => perform_derivative;
            set => perform_derivative = value;
        }

        private int degree_of_polynomial;
        /// <summary>
        /// Gets or sets the degree of the polynomial used in the derivative calculation.
        /// </summary>
        public int DegreeOfPolynomial
        {
            get => degree_of_polynomial;
            set { degree_of_polynomial = value; }
        }

        private int derivative_order;
        /// <summary>
        /// Gets or sets the order of the derivative.
        /// </summary>
        public int DerivativeOrder
        {
            get => derivative_order;
            set { derivative_order = value; }
        }

        private int window_size;
        /// <summary>
        /// Gets or sets the window size for the derivative calculation.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the type of derivative method to use.
        /// </summary>
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

        /// <summary>
        /// Checks if Point Difference method is enabled.
        /// </summary>
        public bool IsPointDiffEnabled
        {
            get => (DerivativeMethod == DerivativeMethod.Point_Diff);
        }

        /// <summary>
        /// Checks if Savitzky-Golay method is enabled.
        /// </summary>
        public bool IsSavitzkyGolayEnabled
        {
            get => (DerivativeMethod == DerivativeMethod.Savitzky_Golay);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DerivativeConfig"/> class.
        /// </summary>
        public DerivativeConfig()
        {

        }

    }
}
