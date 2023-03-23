using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.ViewModels
{
    public class SmoothingViewModel : Screen
    {

        private Smoothing smoothing;

        public Smoothing Smoothing
        {
            get { return smoothing; }
            set { smoothing = value; }
        }

        public SmoothingViewModel()
        {
            smoothing = new Smoothing();
            smoothing.BoxcarSmoothing = true;
        }

        public void CloseWindow()
        {
            TryCloseAsync();
        }
    }
}
