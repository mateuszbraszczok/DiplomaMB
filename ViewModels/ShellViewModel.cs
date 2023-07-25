using Caliburn.Micro;
using DiplomaMB.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Xml.Linq;

namespace DiplomaMB.ViewModels
{
    public class ShellViewModel : Screen
    {
		private PlotModel plot_model;
		public PlotModel PlotModel
        {
			get { return plot_model; }
			set { plot_model = value; NotifyOfPropertyChange(() => PlotModel); }
		}

		private BwtekSpectrometer spectrometer;
		public BwtekSpectrometer Spectrometer
        {
			get { return spectrometer; }
			set { spectrometer = value; NotifyOfPropertyChange(() => Spectrometer); }
		}

		private Spectrum? selected_spectrum;
		public Spectrum? SelectedSpectrum
		{
			get { return selected_spectrum; }
			set { selected_spectrum = value; NotifyOfPropertyChange(() => SelectedSpectrum); }
        }

        private BindableCollection<Spectrum> spectrums;
        public BindableCollection<Spectrum> Spectrums
        {
            get { return spectrums; }
            set { spectrums = value; }
        }

        private int frames_to_acquire;
        public int FramesToAcquire
        {
            get { return frames_to_acquire; }
            set { frames_to_acquire = value; NotifyOfPropertyChange(() => FramesToAcquire); }
        }

        private int integrationTime;
        public int IntegrationTime
        {
            get { return integrationTime; }
            set { integrationTime = value; NotifyOfPropertyChange(() => IntegrationTime); }
        }

        private SmartRead smart_read;
        public SmartRead SmartRead
        {
            get { return smart_read; }
            set { smart_read = value; NotifyOfPropertyChange(() => SmartRead); }
        }

        private bool acquire_continuously;
        public bool AcquireContinuously
        {
            get { return acquire_continuously; }
            set { acquire_continuously = value; NotifyOfPropertyChange(() => AcquireContinuously); NotifyOfPropertyChange(() => NotAcquireContinuously); }
        }
        public bool NotAcquireContinuously
        {
            get { return !acquire_continuously; }
        }

        private int last_id = 1;
        private Thread continuously_acquiring_thread;
        public ShellViewModel()
        {
            PlotModel = new PlotModel { Title = "Spectrums Raw Data" };
            Spectrometer = new BwtekSpectrometer();
            Spectrums = new BindableCollection<Spectrum> { };
            SmartRead = new SmartRead();

            FramesToAcquire = 1;
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

        private void UpdatePlot()
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
                MessageBox.Show("Succesfully set integration time");
            }
            else
            {
                MessageBox.Show("Failed to set integration time","Integration time set failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
                spectrums.Add(spectrum);
            }
            UpdatePlot();
        }

        public void StartAcquire()
        {
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
            return IsSpectrometerConnected();
        }
        public void GetSpectrumSmart()
        {
            Spectrum spectrum = Spectrometer.ReadDataSmart(SmartRead);
            spectrum.Id = last_id;
            spectrum.Name = "Spectrum " + last_id.ToString();
            last_id += 1;

            if (spectrum.DataValues != null)
            {
                spectrums.Add(spectrum);
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
            OpenFileDialog dialog = new()
            {
                Title = "Open Spectrum File",
                Filter = "CSV file (*.csv)|*.csv| Json files (*.json)|*.json| All Files (*.*)|*.*"
            };
            if (dialog.ShowDialog() == true)
            {
                string file_path = dialog.FileName;
                Spectrum spectrum = new(file_path, last_id);
                last_id++;
                spectrums.Add(spectrum);
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

        public void SpectrumPeaks()
        {
            if (spectrums.Count > 0 && SelectedSpectrum != null)
            {
                SelectedSpectrum.DetectPeaks();
            }
            else
            {
                MessageBox.Show("No available spectrum to detect peak", "Spectrum peak detection error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (smoothing_dialog.PerformSmoothing)
            {
                Smoothing smoothing = smoothing_dialog.Smoothing;
                Debug.WriteLine("Parameter: " + smoothing.Parameter.ToString() + "Type: " + smoothing.Type.ToString());

                Spectrum spectrum = Spectrometer.Smoothing(smoothing, selected_spectrum);

                if (spectrum.DataValues != null)
                {
                    if (smoothing_dialog.CreateNewSpectrum)
                    {

                        Debug.WriteLine("create new spectrum");
                        spectrum.Id = last_id;
                        spectrum.Name = selected_spectrum.Name + "_smoothed";
                        last_id += 1;
                        spectrums.Add(spectrum);
                    }
                    else
                    {
                        Spectrum edited_spectrum = selected_spectrum;
                        edited_spectrum.DataValues = spectrum.DataValues;
                        Debug.WriteLine("edit existing spectrum");
                        spectrums.Remove(selected_spectrum);
                        spectrums.Add(edited_spectrum);
                    }
                    UpdatePlot();
                }
            }
        }

        public void ChangeVisibility()
        {
            Debug.WriteLine("In ChangeVisibility function");
            UpdatePlot();
        }

        private bool IsSpectrometerConnected()
        {
            if (!Spectrometer.Connected) { MessageBox.Show("Spectrometer is not connected"); }
            return Spectrometer.Connected;
        }

    }
}
