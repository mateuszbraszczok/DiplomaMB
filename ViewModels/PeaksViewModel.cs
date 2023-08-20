using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiplomaMB.ViewModels
{
    public class PeaksViewModel : Screen
    {
        private Spectrum spectrum;
        public Spectrum Spectrum
        {
            get => spectrum;
            set => spectrum = value;
        }

        private ISpectrometer spectrometer;
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set => spectrometer = value;
        }

        private BindableCollection<PeakInfo> peaks;
        public BindableCollection<PeakInfo> Peaks
        {
            get => peaks;
            set { peaks = value; NotifyOfPropertyChange(() => Peaks); }
        }

        private int min_peak_height;
        public int MinPeakHeight
        {
            get => min_peak_height;
            set { min_peak_height = value; NotifyOfPropertyChange(() => MinPeakHeight); }
        }

        public PeaksViewModel(Spectrum _spectrum, ISpectrometer _spectrometer)
        {
            spectrum = _spectrum;
            spectrometer = _spectrometer;
            min_peak_height = 2000;

            peaks = new BindableCollection<PeakInfo> { };
            int index = 1;

            foreach (var peak in spectrum.Peaks)
            {
                double peakValue = spectrum.DataValues[(int)peak.PeakIndex];
                double startWavelength = spectrum.Wavelengths[(int)peak.PeakBeginIndex];
                double endWavelength = spectrum.Wavelengths[(int)peak.PeakEndIndex];
                double peakWavelength = spectrum.Wavelengths[(int)peak.PeakIndex];

                PeakInfo peak_info = new PeakInfo(index++, peakValue, startWavelength, endWavelength, peakWavelength);
                peaks.Add(peak_info);
            }
            NotifyOfPropertyChange(() => Peaks);
        }

        public void DetectPeaks()
        {

            Spectrum secondDerivative = Spectrometer.CalculateDerivative(2, 3, Spectrum);
            bool inMinimum = false;
            int x1 = 0;
            int x2;
            int peakCount = 0;

            List<double> fwhmList = new List<double>();
            List<int> peaksIndex = new List<int>();
            List<int> peaksBeginIndex = new List<int>();
            List<int> peaksEndIndex = new List<int>();

            for (int i = 0; i < secondDerivative.DataValues.Count; i++)
            {
                if (secondDerivative.DataValues[i] < 0 && !inMinimum)
                {
                    x1 = i;
                    inMinimum = true;
                }
                else if (secondDerivative.DataValues[i] > 0 && inMinimum)
                {
                    inMinimum = false;
                    x2 = i;
                    double distance = secondDerivative.Wavelengths[x2] - secondDerivative.Wavelengths[x1];
                    double A = -Trapz(secondDerivative.Wavelengths.GetRange(x1, x2 - x1 + 1), secondDerivative.DataValues.GetRange(x1, x2 - x1 + 1));
                    double h = (Math.Exp(1 / 2.0) / 4) * A * distance;

                    if (h > 200)
                    {
                        double maxValue = 0;
                        int indexx = 0;
                        for (int j = x1; j <= x2; j++)
                        {
                            if (spectrum.DataValues[j] > maxValue)
                            {
                                maxValue = spectrum.DataValues[j];
                                indexx = j;
                            }
                        }

                        if (spectrum.DataValues[indexx] > min_peak_height)
                        {
                            if (indexx - 4 > 0 && indexx + 4 < spectrum.DataValues.Count && spectrum.DataValues[indexx] >= spectrum.DataValues.GetRange(indexx - 4, 9).Max())
                            {
                                peakCount++;
                                double fwhm = Math.Sqrt(2 * Math.Log(2)) * distance;
                                fwhmList.Add(fwhm);
                                peaksIndex.Add(indexx);
                                peaksBeginIndex.Add(x1);
                                peaksEndIndex.Add(x2);
                            }
                        }
                    }
                }
            }


            List<Peak> peaks = new List<Peak>();
            Peaks = new BindableCollection<PeakInfo> { };

            for (int i = 0; i < peakCount; i++)
            {
                Peak peak = new Peak(peaksIndex[i], peaksBeginIndex[i], peaksEndIndex[i]);
                peaks.Add(peak);

                double peakValue = spectrum.DataValues[(int)peak.PeakIndex];
                double startWavelength = spectrum.Wavelengths[(int)peak.PeakBeginIndex];
                double endWavelength = spectrum.Wavelengths[(int)peak.PeakEndIndex];
                double peakWavelength = spectrum.Wavelengths[(int)peak.PeakIndex];
                PeakInfo peak_info = new PeakInfo(i+1, peakValue, startWavelength, endWavelength, peakWavelength);
                Peaks.Add(peak_info);
            }
            Spectrum.Peaks = peaks;
            
        }

        private double Trapz(List<double> x, List<double> y)
        {
            double result = 0;
            for (int i = 0; i < x.Count - 1; i++)
            {
                result += (y[i] + y[i + 1]) * (x[i + 1] - x[i]) / 2;
            }
            return result;
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
