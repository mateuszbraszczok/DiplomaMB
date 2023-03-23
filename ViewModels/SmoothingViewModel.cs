using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.ViewModels
{
    public class SmoothingViewModel
    {
        private int year;

        public int Year
        {
            get { return year; }
            set { year = value; }
        }

        private Smoothing smoothing;

        public Smoothing Smoothing
        {
            get { return smoothing; }
            set { smoothing = value; }
        }

        public SmoothingViewModel()
        {
            smoothing = new Smoothing();
            Year = 20;
        }
    }
}
