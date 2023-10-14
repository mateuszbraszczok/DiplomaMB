/**
 * @file SmartRead.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief Represents settings and parameters for performing smart reading of spectrums.
 */

namespace DiplomaMB.Models
{
    /// <summary>
    /// Represents settings and parameters for performing smart reading of spectrums.
    /// </summary>
    public class SmartRead
    {
        private int spectrums_to_avarage;
        /// <summary>
        /// Gets or sets the number of spectrums to average during a smart read operation.
        /// </summary>
        public int SpectrumsToAverage
        {
            get => spectrums_to_avarage;
            set => spectrums_to_avarage = value;
        }

        private bool smoothing;
        /// <summary>
        /// Gets or sets a value indicating whether smoothing should be applied during a smart read operation.
        /// </summary>
        public bool Smoothing
        {
            get => smoothing;
            set => smoothing = value;
        }

        private bool dark_compensation;
        /// <summary>
        /// Gets or sets a value indicating whether dark compensation should be applied during a smart read operation.
        /// </summary>
        public bool DarkCompensation
        {
            get => dark_compensation;
            set => dark_compensation = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartRead"/> class with default settings.
        /// </summary>
        public SmartRead()
        {
            SpectrumsToAverage = 1;
            Smoothing = false;
            DarkCompensation = true;
        }
    }
}
