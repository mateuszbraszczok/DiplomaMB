/**
 * @file PeaksViewModel.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief ViewModel for peak detection.
 */

using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DiplomaMB.ViewModels
{
    /// <summary>
    /// ViewModel responsible for peak detection and display in a spectrum.
    /// </summary>
    public class PeaksViewModel : Screen
    {
        private Spectrum spectrum;
        /// <summary>
        /// Gets or sets the current spectrum.
        /// </summary>
        public Spectrum Spectrum
        {
            get => spectrum;
            set => spectrum = value;
        }

        private ISpectrometer spectrometer;
        /// <summary>
        /// Gets or sets the spectrometer instance.
        /// </summary>
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set => spectrometer = value;
        }

        private BindableCollection<PeakInfo> peaks;
        /// <summary>
        /// Gets or sets the collection of peaks.
        /// </summary>
        public BindableCollection<PeakInfo> Peaks
        {
            get => peaks;
            set { peaks = value; NotifyOfPropertyChange(() => Peaks); }
        }

        private int min_peak_height;
        /// <summary>
        /// Gets or sets the minimum peak height for peak detection.
        /// </summary>
        public int MinPeakHeight
        {
            get => min_peak_height;
            set { min_peak_height = value; NotifyOfPropertyChange(() => MinPeakHeight); }
        }

        /// <summary>
        /// Initializes a new instance of the PeaksViewModel class.
        /// </summary>
        /// <param name="_spectrum">The spectrum to be used.</param>
        /// <param name="_spectrometer">The spectrometer to be used.</param>
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

        /// <summary>
        /// Detects the peaks within the spectrum based on various parameters.
        /// </summary>
        public void DetectPeaks()
        {
            
            DerivativeConfig derivative_config = new DerivativeConfig
            {
                DerivativeOrder = 2,
                DerivativeMethod = DerivativeMethod.Savitzky_Golay,
                DegreeOfPolynomial = 3,
                WindowSize = 7,
            };
            Spectrum secondDerivative = Spectrometer.CalculateDerivative(Spectrum, derivative_config);
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

                    if (h > 50)
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
                            if (indexx - 2 > 0 && indexx + 2 < spectrum.DataValues.Count && spectrum.DataValues[indexx] >= spectrum.DataValues.GetRange(indexx - 2, 5).Max())
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
                PeakInfo peak_info = new PeakInfo(i + 1, peakValue, startWavelength, endWavelength, peakWavelength);
                Peaks.Add(peak_info);
            }
            Spectrum.Peaks = peaks;

        }

        /// <summary>
        /// Implements the Trapezoidal Rule to calculate the area under a curve.
        /// </summary>
        /// <param name="x">The list of x-values.</param>
        /// <param name="y">The list of y-values.</param>
        /// <returns>The area under the curve.</returns>
        private double Trapz(List<double> x, List<double> y)
        {
            double result = 0;
            for (int i = 0; i < x.Count - 1; i++)
            {
                result += (y[i] + y[i + 1]) * (x[i + 1] - x[i]) / 2;
            }
            return result;
        }

        /// <summary>
        /// Represents a peak with its details.
        /// </summary>
        public class PeakInfo
        {
            public int PeakIndex { get; set; }
            public double PeakValue { get; set; }
            public double StartWavelength { get; set; }
            public double EndWavelength { get; set; }
            public double PeakWavelength { get; set; }

            // <summary>
            /// Initializes a new instance of the PeakInfo class.
            /// </summary>
            /// <param name="index">The index of the peak.</param>
            /// <param name="peak_value">The value of the peak.</param>
            /// <param name="start_wavelength">The starting wavelength of the peak.</param>
            /// <param name="end_wavelength">The ending wavelength of the peak.</param>
            /// <param name="peak_wavelength">The peak wavelength.</param>
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
