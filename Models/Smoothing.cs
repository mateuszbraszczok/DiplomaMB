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

		private int type;

		public int Type
		{
			get { return type; }
			set { type = value; }
		}

		public int Parameter
		{
			get { 
				if (Type == 0)
				{
					return BoxCarWindow;
				}
				else if (Type == 1) 
				{
					return SavGolayWindow;
				}
				else if (Type == 2)
				{
					return FftSmoothingDegree;
				}
				else
				{
					return 0;
				}
			}
		}

	}
}
