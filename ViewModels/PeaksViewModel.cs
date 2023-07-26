using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaMB.ViewModels
{
    public class PeaksViewModel : Screen
    {
        private Spectrum spectrum;
        public Spectrum Spectrum { 
            get { return spectrum; }
            set { spectrum = value; }
        }

        private BindableCollection<PeakInfo> peaks;

        public BindableCollection<PeakInfo> Peaks
        {
            get { return peaks; }
            set 
            { 
                peaks = value;
                NotifyOfPropertyChange(() => Peaks);
            }
        }

        public PeaksViewModel(Spectrum _spectrum) 
        {
            spectrum = _spectrum;

            peaks = new BindableCollection<PeakInfo> { };
            int index = 1;
            foreach (var peak in spectrum.Peaks)
            {
                double peakValue = spectrum.DataValues[(int)peak.PeakIndex];
                double startWavelength = spectrum.Wavelengths[(int)peak.PeakBeginIndex];
                double endWavelength = spectrum.Wavelengths[(int)peak.PeakEndIndex];
                double peakWavelength = spectrum.Wavelengths[(int)peak.PeakIndex];

                PeakInfo peak_info = new(index++, peakValue, startWavelength, endWavelength, peakWavelength);
                peaks.Add(peak_info);
            }
        }

        public class PeakInfo
        {
            public int PeakIndex { get; set; }
            public double PeakValue { get; set; }
            public double StartWavelength { get; set; }
            public double EndWavelength { get; set; }
            public double PeakWavelength { get; set; }

            public PeakInfo(int index, double peak_value, double start_wavelength, double end_wavelength, double peak_wavelength)
            {
                PeakIndex = index;
                PeakValue = Math.Round(peak_value, 2);
                StartWavelength = Math.Round(start_wavelength, 2);
                EndWavelength = Math.Round(end_wavelength, 2);
                PeakWavelength = Math.Round(peak_wavelength, 2);
            }
        }
    }
}
