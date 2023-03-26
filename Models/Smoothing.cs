using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.Models
{
    public class Smoothing
    {
		private int box_car_window;

		public int BoxCarWindow
		{
			get { return box_car_window; }
			set { box_car_window = value; }
		}

		private int sav_golay_window;

		public int SavGolayWindow
		{
			get { return sav_golay_window; }
			set { sav_golay_window = value; }
		}

		private int fft_smoothing_degree;

		public int FftSmoothingDegree
		{
			get { return fft_smoothing_degree; }
			set { fft_smoothing_degree = value; }
		}


		private bool fft_smoothing;

		public bool FftSmoothing
		{
			get { return fft_smoothing; }
			set { fft_smoothing = value; }
		}

		private bool sav_golay_smoothing;

		public bool SavGolaySmoothing
		{
			get { return sav_golay_smoothing; }
			set { sav_golay_smoothing = value; }
		}

		private bool boxcar_smoothing;

		public bool BoxcarSmoothing
		{
			get { return boxcar_smoothing; }
			set { boxcar_smoothing = value; }
		}

		public int Parameter
		{
			get { 
				if (BoxcarSmoothing)
				{
					return BoxCarWindow;
				}
				else if (SavGolaySmoothing) 
				{
					return SavGolayWindow;
				}
				else if (FftSmoothing)
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
                if (BoxcarSmoothing)
                {
                    return 0;
                }
                else if (FftSmoothing)
                {
                    return 1;
                }
                else if (SavGolaySmoothing)
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
