namespace DiplomaMB.Models
{
    public class SmartRead
    {
        private int spectrums_to_avarage;
        public int SpectrumsToAverage
        {
            get => spectrums_to_avarage;
            set => spectrums_to_avarage = value;
        }

        private bool smoothing;
        public bool Smoothing
        {
            get => smoothing;
            set => smoothing = value;
        }

        private bool dark_compensation;
        public bool DarkCompensation
        {
            get => dark_compensation;
            set => dark_compensation = value;
        }

        public SmartRead()
        {
            SpectrumsToAverage = 1;
            Smoothing = false;
            DarkCompensation = true;
        }
    }
}
