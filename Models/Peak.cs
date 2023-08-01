namespace DiplomaMB.Models
{
    public class Peak
    {
        public int PeakIndex { get; set; }

        public int PeakBeginIndex { get; set; }

        public int PeakEndIndex { get; set; }

        public Peak(int peak_index, int peak_begin_index, int peak_end_index)
        {
            PeakIndex = peak_index;
            PeakBeginIndex = peak_begin_index;
            PeakEndIndex = peak_end_index;
        }
    }
}
