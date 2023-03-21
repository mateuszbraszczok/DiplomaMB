using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.Models
{
    public class SmartRead
    {
		private int spectrums_to_avarage;
        private bool smoothing;
        private bool dark_compensation;

        public int SpectrumsToAverage
        {
			get { return spectrums_to_avarage; }
			set { spectrums_to_avarage = value; }
		}

		public bool Smoothing
		{
			get { return smoothing; }
			set { smoothing = value; }
		}

		public bool DarkCompensation
		{
			get { return dark_compensation; }
			set { dark_compensation = value; }
		}
	}
}
