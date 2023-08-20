using Caliburn.Micro;
using DiplomaMB.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    public class ShellViewModel : Screen
    {
        private PlotModel plot_model;
        public PlotModel PlotModel
        {
            get => plot_model;
            set { plot_model = value; NotifyOfPropertyChange(() => PlotModel); }
        }

        private ISpectrometer spectrometer;
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set { spectrometer = value; NotifyOfPropertyChange(() => Spectrometer); }
        }

        private Spectrum? selected_spectrum;
        public Spectrum? SelectedSpectrum
        {
            get => selected_spectrum;
            set { selected_spectrum = value; NotifyOfPropertyChange(() => SelectedSpectrum); }
        }

        private BindableCollection<Spectrum> spectrums;
        public BindableCollection<Spectrum> Spectrums
        {
            get => spectrums;
            set
            {
                if (spectrums != value)
                {
                    spectrums = value;
                    NotifyOfPropertyChange(() => Spectrums);
                }
            }
        }

        private void Spectrums_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => CanSpectrumOperations);
            NotifyOfPropertyChange(() => CanSpectrumPeaks);
            NotifyOfPropertyChange(() => CanEditSmoothing);
        }

        private int frames_to_acquire;
        public int FramesToAcquire
        {
            get => frames_to_acquire;
            set { frames_to_acquire = value; NotifyOfPropertyChange(() => FramesToAcquire); }
        }

        private int integration_time;
        public int IntegrationTime
        {
            get => integration_time;
            set { integration_time = value; NotifyOfPropertyChange(() => IntegrationTime); }
        }
        private int half_point;
        public int HalfPoint
        {
            get => half_point;
            set { half_point = value; NotifyOfPropertyChange(() => HalfPoint); }
        }

        private SmartRead smart_read;
        public SmartRead SmartRead
        {
            get => smart_read;
            set { smart_read = value; NotifyOfPropertyChange(() => SmartRead); }
        }

        private bool acquire_continuously;
        public bool AcquireContinuously
        {
            get => (acquire_continuously && Spectrometer.Connected);
            set { acquire_continuously = value; NotifyOfPropertyChange(() => AcquireContinuously); NotifyOfPropertyChange(() => NotAcquireContinuously); }
        }
        public bool NotAcquireContinuously
        {
            get => (!acquire_continuously && Spectrometer.Connected);
        }

        public bool GuiLocked
        {
            get => (!lock_gui && Spectrometer.Connected);
        }

        private bool lock_gui = false;
        private int last_id = 1;
        private Thread continuously_acquiring_thread;

        public ShellViewModel()
        {
            plot_model = new PlotModel { Title = "Spectrums Raw Data", Background = OxyColors.LightGray };
            spectrometer = new BwtekSpectrometer();
            spectrums = new BindableCollection<Spectrum> { };
            Spectrums.CollectionChanged += Spectrums_CollectionChanged;
            smart_read = new SmartRead();

            frames_to_acquire = 1;
            InitializePlot();
        }

        public void OnClose(CancelEventArgs e)
        {
            Debug.WriteLine("Goodbye");
            Environment.Exit(Environment.ExitCode);
        }

        private void InitializePlot()
        {
            PlotModel.Legends.Add(new Legend()
            {
                LegendTitle = "Legend",
                LegendPosition = LegendPosition.RightTop,
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
            });

            UpdatePlot();
            PlotModel.InvalidatePlot(true);
        }

        public void UpdatePlot()
        {
            PlotModel.Series.Clear();
            double min_x_value = double.MaxValue;
            double max_x_value = double.MinValue;
            double min_y_value = double.MaxValue;
            double max_y_value = double.MinValue;
            int spectrums_enabled = 0;
            foreach (var spectrum in Spectrums)
            {
                if (spectrum.Enabled)
                {
                    spectrums_enabled++;
                    PlotModel.Series.Add(spectrum.getPlotSerie());
                    if (spectrum.Peaks.Count > 0)
                    {
                        PlotModel.Series.Add(spectrum.getPeaks());
                    }
                    double max_x = spectrum.Wavelengths.Max();
                    double min_x = spectrum.Wavelengths.Min();
                    if (max_x > max_x_value)
                    {
                        max_x_value = max_x;
                    }
                    if (min_x < min_x_value)
                    {
                        min_x_value = min_x;
                    }

                    double max_y = spectrum.DataValues.Max();
                    double min_y = spectrum.DataValues.Min();
                    if (max_y > max_y_value)
                    {
                        max_y_value = max_y;
                    }
                    if (min_y < min_y_value)
                    {
                        min_y_value = min_y;
                    }
                }
            }

            Debug.WriteLine($"max:{max_y_value}  min:{min_y_value}");

            if (spectrums.Count == 0 || spectrums_enabled == 0)
            {
                min_x_value = 100;
                max_x_value = 1000;
                min_y_value = 0;
                max_y_value = 10000;
            }

            PlotModel.Axes.Clear();
            var Xaxis = new LinearAxis
            {
                Title = "Wavelength [nm]",
                Position = AxisPosition.Bottom,
                Minimum = min_x_value,
                Maximum = max_x_value,
                MajorGridlineStyle = LineStyle.DashDot,
                IntervalLength = 25
            };
            PlotModel.Axes.Add(Xaxis);

            var Yaxis = new LinearAxis
            {
                Title = "Relative intensity",
                Minimum = min_y_value - 0.03 * max_y_value,
                Maximum = max_y_value + 0.03 * max_y_value,
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.DashDot,
                IntervalLength = 20
            };
            PlotModel.Axes.Add(Yaxis);

            PlotModel.InvalidatePlot(true);
        }

        public void ExitProgram()
        {
            Spectrometer.Disconnect();
            Application.Current.Shutdown();
        }

        public void ConnectSpectrometer()
        {
            Spectrometer.Connect();
            if (Spectrometer.Connected == true)
            {
                IntegrationTime = Spectrometer.IntegrationTime;
                NotifyOfPropertyChange(() => AcquireContinuously); NotifyOfPropertyChange(() => NotAcquireContinuously);
            }
            else
            {
                MessageBox.Show("Can't connect with spectrometer", "Can't connect with spectrometer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            NotifyOfPropertyChange(() => Spectrometer);
        }

        public bool CanResetSpectrometer()
        {
            return Spectrometer.Connected;
        }
        public void ResetSpectrometer()
        {
            Spectrometer.ResetDevice();
        }

        public void SetIntegrationTime()
        {
            if (Spectrometer.Connected)
            {
                Spectrometer.SetIntegrationTime(IntegrationTime);
            }
            if (IntegrationTime == spectrometer.IntegrationTime)
            {
                MessageBox.Show($"Succesfully set integration time to {IntegrationTime} ms");
            }
            else
            {
                MessageBox.Show("Failed to set integration time", "Integration time set failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            IntegrationTime = spectrometer.IntegrationTime;
        }

        public bool CanGetSpectrum()
        {
            return IsSpectrometerConnected();
        }
        public void GetSpectrum()
        {
            Debug.WriteLine("before read data");
            List<Spectrum> spectrum_list = Spectrometer.ReadData(FramesToAcquire);

            Debug.WriteLine("readed data");
            foreach (Spectrum spectrum in spectrum_list)
            {
                spectrum.Id = last_id;
                spectrum.Name = "Spectrum " + last_id.ToString();
                last_id += 1;
                Spectrums.Add(spectrum);
            }
            UpdatePlot();
        }

        public void StartAcquire()
        {
            lock_gui = false;
            AcquireContinuously = true;
            continuously_acquiring_thread = new Thread(AcquiringSpectrums);
            continuously_acquiring_thread.Start();
        }

        private void AcquiringSpectrums()
        {
            bool acquired_first_spectrum = false;
            int id = last_id;
            last_id++;
            while (AcquireContinuously)
            {
                Spectrum spectrum = Spectrometer.ReadData(1).First();
                //Spectrum spectrum = Spectrometer.GenerateDummySpectrum();
                spectrum.Id = id;
                spectrum.Name = "Spectrum " + id.ToString();
                if (acquired_first_spectrum)
                {
                    Spectrums.RemoveAt(Spectrums.Count - 1);
                }
                Spectrums.Add(spectrum);
                UpdatePlot();
                acquired_first_spectrum = true;

                Debug.WriteLine("iteration");
            }
            Debug.WriteLine("returned");
            return;
        }
        public void StopAcquire()
        {
            lock_gui = false;
            AcquireContinuously = false;
            Debug.WriteLine("Stopping acquiring");
            //while(continuously_acquiring_thread.IsAlive)
            //{
            //    Thread.Sleep(50);
            //}
            //continuously_acquiring_thread.Join();
            Debug.WriteLine("Stopped acquiring");
        }

        public bool CanGetSpectrumSmart()
        {
            return IsSpectrometerConnected() && lock_gui;
        }
        public void GetSpectrumSmart()
        {
            Spectrum spectrum = Spectrometer.ReadDataSmart(SmartRead);
            spectrum.Id = last_id;
            spectrum.Name = "Spectrum " + last_id.ToString();
            last_id += 1;

            if (spectrum.DataValues != null)
            {
                Spectrums.Add(spectrum);
                UpdatePlot();
            }
        }

        public bool CanGetDarkScan()
        {
            return IsSpectrometerConnected();
        }
        public void GetDarkScan()
        {
            Spectrometer.GetDarkScan();
            NotifyOfPropertyChange(() => Spectrometer);
        }

        public void LoadSpectrum()
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Title = "Open Spectrum File",
                Filter = "CSV file (*.csv)|*.csv| Json files (*.json)|*.json| All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                string file_path = dialog.FileName;
                Spectrum spectrum = new Spectrum(file_path, last_id);
                last_id++;
                Spectrums.Add(spectrum);
                UpdatePlot();
            }
        }

        public void LoadDarkScan()
        {
            Spectrometer.LoadDarkScan();
            NotifyOfPropertyChange(() => Spectrometer);
        }

        public void SaveDarkScan()
        {
            Spectrometer.SaveDarkScan();
            NotifyOfPropertyChange(() => Spectrometer);
        }

        public void DeleteSelectedSpectrum()
        {
            if (spectrums.Count > 0 && SelectedSpectrum != null)
            {
                Spectrums.Remove(SelectedSpectrum);
                SelectedSpectrum = null;
                UpdatePlot();
            }
        }

        public void DeleteAllSpectrums()
        {
            Spectrums.Clear();
            SelectedSpectrum = null;
            UpdatePlot();
        }

        public void SaveSelectedSpectrum()
        {
            if (spectrums.Count > 0 && SelectedSpectrum != null)
            {
                SelectedSpectrum.SaveToFile();
            }
            else
            {
                MessageBox.Show("No available spectrum to save", "Spectrum save error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool CanSpectrumPeaks
        {
            get { return Spectrums?.Count > 0; }
        }
        public void SpectrumPeaks()
        {
            if (spectrums.Count > 0 && SelectedSpectrum != null)
            {
                var windowManager = new WindowManager();
                var peaks_dialog = new PeaksViewModel(SelectedSpectrum, Spectrometer);
                windowManager.ShowDialogAsync(peaks_dialog);
                UpdatePlot();
            }
            else
            {
                MessageBox.Show("No available spectrum to detect peak", "Spectrum peak detection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public bool CanSpectrumOperations
        {
            get { return Spectrums?.Count > 0; }
        }
        public void SpectrumOperations()
        {
            var windowManager = new WindowManager();
            var editing_dialog = new EditingViewModel(Spectrums);
            windowManager.ShowDialogAsync(editing_dialog);

            if (editing_dialog.OperationDone)
            {
                Spectrum result = editing_dialog.ResultSpectrum;
                result.Id = last_id++;

                Spectrums.Add(result);
                UpdatePlot();
            }
        }


        public bool CanEditSmoothing
        {
            get { return Spectrums?.Count > 0; }
        }
        public void EditSmoothing()
        {
            if (selected_spectrum == null)
            {
                MessageBox.Show("No Spectrum selected");
                return;
            }
            var windowManager = new WindowManager();
            var smoothing_dialog = new SmoothingViewModel();
            windowManager.ShowDialogAsync(smoothing_dialog);

            Smoothing smoothing = smoothing_dialog.Smoothing;

            if (smoothing.PerformSmoothing)
            {
                Debug.WriteLine("Parameter: " + smoothing.Parameter.ToString() + ", Type: " + smoothing.Type.ToString());
                Spectrum smoothed_spectrum = Spectrometer.Smoothing(smoothing, selected_spectrum);

                if (smoothed_spectrum.DataValues != null)
                {
                    if (smoothing.CreateNewSpectrum)
                    {

                        Debug.WriteLine("create new spectrum");
                        smoothed_spectrum.Id = last_id;
                        smoothed_spectrum.Name = selected_spectrum.Name + "_smoothed";
                        last_id += 1;
                        Spectrums.Add(smoothed_spectrum);
                    }
                    else
                    {
                        Debug.WriteLine("edit existing spectrum");
                        selected_spectrum.DataValues = smoothed_spectrum.DataValues;
                    }
                    UpdatePlot();
                }
            }
        }

        public void Derivative()
        {
            Spectrum result = Spectrometer.CalculateDerivative(1, 3, SelectedSpectrum);

            result.Id = last_id++;

            Spectrums.Add(result);
            UpdatePlot();
        }

        private bool IsSpectrometerConnected()
        {
            if (!Spectrometer.Connected) { MessageBox.Show("Spectrometer is not connected"); }
            return Spectrometer.Connected;
        }

    }
}
