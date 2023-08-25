/**
 * @file Peak.cs
 * @author Mateusz Braszczok
 * @date 2023-08-26
 * @brief Represents a single peak in a spectrum.
 */


namespace DiplomaMB.Models
{
    /// <summary>
    /// Represents a single peak in a spectrum.
    /// </summary>
    public class Peak
    {
        /// <summary>
        /// Gets or sets the index position of the peak.
        /// </summary>
        public int PeakIndex { get; set; }

        /// <summary>
        /// Gets or sets the index position where the peak begins.
        /// </summary>
        public int PeakBeginIndex { get; set; }

        /// <summary>
        /// Gets or sets the index position where the peak ends.
        /// </summary>
        public int PeakEndIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Peak"/> class.
        /// </summary>
        /// <param name="peak_index">The index position of the peak.</param>
        /// <param name="peak_begin_index">The index position where the peak begins.</param>
        /// <param name="peak_end_index">The index position where the peak ends.</param>
        public Peak(int peak_index, int peak_begin_index, int peak_end_index)
        {
            PeakIndex = peak_index;
            PeakBeginIndex = peak_begin_index;
            PeakEndIndex = peak_end_index;
        }
    }
}
