using Caliburn.Micro;
using DiplomaMB.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private bool should_acquire = false;
        private bool gui_locked = false;
        private int last_id = 1;
        private Thread? continuously_acquiring_thread = null;

        public ShellViewModel()
        {
            plot_model = new PlotModel { Title = "Spectrums Raw Data", Background = OxyColors.LightGray };
            spectrometer = new BwtekSpectrometer();
            spectrums = new BindableCollection<Spectrum> { };
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

        private void UpdateGui()
        {
            NotifyOfPropertyChange(() => CanConnectSpectrometer);
            NotifyOfPropertyChange(() => CanResetSpectrometer);
            NotifyOfPropertyChange(() => CanSetIntegrationTime);
            NotifyOfPropertyChange(() => CanGetSpectrum);
            NotifyOfPropertyChange(() => CanGetSpectrumSmart);
            NotifyOfPropertyChange(() => CanGetDarkScan);
            NotifyOfPropertyChange(() => CanStartAcquire);
            NotifyOfPropertyChange(() => CanStopAcquire);
            NotifyOfPropertyChange(() => CanLoadSpectrum);
            NotifyOfPropertyChange(() => CanLoadDarkScan);
            NotifyOfPropertyChange(() => CanSaveDarkScan);
            NotifyOfPropertyChange(() => CanDeleteSelectedSpectrum);
            NotifyOfPropertyChange(() => CanDeleteAllSpectrums);
            NotifyOfPropertyChange(() => CanSaveSelectedSpectrum);
            NotifyOfPropertyChange(() => CanSpectrumPeaks);
            NotifyOfPropertyChange(() => CanSpectrumOperations);
            NotifyOfPropertyChange(() => CanEditSmoothing);
            NotifyOfPropertyChange(() => CanDerivative);
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

        public bool CanConnectSpectrometer
        {
            get { return !IsSpectrometerConnected() && !gui_locked; }
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
            UpdateGui();
        }

        public bool CanResetSpectrometer
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        public void ResetSpectrometer()
        {
            Spectrometer.ResetDevice();
        }

        public bool CanSetIntegrationTime
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
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
            UpdateGui();
        }

        public bool CanGetSpectrum
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        public async void GetSpectrum()
        {
            gui_locked = true;
            UpdateGui();
            Debug.WriteLine("before read data");
            List<Spectrum> spectrum_list = await Task.Run(() => Spectrometer.ReadData(FramesToAcquire));

            Debug.WriteLine("readed data");
            foreach (Spectrum spectrum in spectrum_list)
            {
                spectrum.Id = last_id;
                spectrum.Name = "Spectrum " + last_id.ToString();
                last_id += 1;
                Spectrums.Add(spectrum);
            }
            UpdatePlot();
            gui_locked = false;
            UpdateGui();
        }

        public bool CanGetDarkScan
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        public async void GetDarkScan()
        {
            gui_locked = true;
            UpdateGui();
            await Task.Run(() => Spectrometer.GetDarkScan());

            NotifyOfPropertyChange(() => Spectrometer);
            gui_locked = false;
            UpdateGui();
        }

        public bool CanStartAcquire
        {
            get { return IsSpectrometerConnected() && !gui_locked && !should_acquire; }
        }
        public void StartAcquire()
        {
            if (continuously_acquiring_thread == null || !continuously_acquiring_thread.IsAlive)
            {
                gui_locked = true;
                should_acquire = true;
                UpdateGui();

                continuously_acquiring_thread = new Thread(AcquiringSpectrums);
                continuously_acquiring_thread.Start();
            }
        }

        public bool CanStopAcquire
        {
            get { return IsSpectrometerConnected() && should_acquire; }
        }
        public void StopAcquire()
        {
            should_acquire = false;
            while (continuously_acquiring_thread != null && continuously_acquiring_thread.IsAlive)
            {
                Thread.Sleep(10);
                break;
            }
            gui_locked = false;
            UpdateGui();
        }

        private void AcquiringSpectrums()
        {
            bool acquiredFirstSpectrum = false;
            int id = last_id;
            last_id++;

            while (should_acquire)
            {
                Spectrum spectrum = Spectrometer.ReadData(1).First();
                //Spectrum spectrum = Spectrometer.GenerateDummySpectrum();
                spectrum.Id = id;
                spectrum.Name = "Spectrum " + id.ToString();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (acquiredFirstSpectrum)
                    {
                        Spectrums.RemoveAt(Spectrums.Count - 1);
                    }
                    Spectrums.Add(spectrum);
                    UpdatePlot();
                });

                acquiredFirstSpectrum = true;

                Debug.WriteLine("iteration");
            }
        }

        public bool CanGetSpectrumSmart
        {
            get { return IsSpectrometerConnected() && !gui_locked && spectrometer is BwtekSpectrometer; }
        }
        public async void GetSpectrumSmart()
        {
            gui_locked = true;
            UpdateGui();
            Spectrum spectrum = await Task.Run(() => Spectrometer.ReadDataSmart(SmartRead));
            spectrum.Id = last_id;
            spectrum.Name = "Spectrum " + last_id.ToString();
            last_id += 1;

            if (spectrum.DataValues != null)
            {
                Spectrums.Add(spectrum);
                UpdatePlot();
            }

            gui_locked = false;
            UpdateGui();
        }

        public bool CanLoadSpectrum
        {
            get { return !gui_locked; }
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
            UpdateGui();
        }

        public bool CanLoadDarkScan
        {
            get { return !gui_locked; }
        }
        public void LoadDarkScan()
        {
            Spectrometer.LoadDarkScan();
            NotifyOfPropertyChange(() => Spectrometer);
            UpdateGui();
        }

        public bool CanSaveDarkScan
        {
            get { return !gui_locked; }
        }
        public void SaveDarkScan()
        {
            Spectrometer.SaveDarkScan();
            NotifyOfPropertyChange(() => Spectrometer);
            UpdateGui();
        }

        public bool CanDeleteSelectedSpectrum
        {
            get { return !gui_locked && spectrums.Count > 0; }
        }
        public void DeleteSelectedSpectrum()
        {
            if (spectrums.Count > 0 && SelectedSpectrum != null)
            {
                Spectrums.Remove(SelectedSpectrum);
                SelectedSpectrum = null;
                UpdatePlot();
            }
            UpdateGui();
        }

        public bool CanDeleteAllSpectrums
        {
            get { return !gui_locked; }
        }
        public void DeleteAllSpectrums()
        {
            Spectrums.Clear();
            SelectedSpectrum = null;
            UpdatePlot();
            UpdateGui();
        }

        public bool CanSaveSelectedSpectrum
        {
            get { return !gui_locked; }
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
            UpdateGui();
        }

        public bool CanSpectrumPeaks
        {
            get { return Spectrums?.Count > 0 && !gui_locked; }
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
            UpdateGui();
        }


        public bool CanSpectrumOperations
        {
            get { return Spectrums?.Count > 0 && !gui_locked; }
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
            UpdateGui();
        }


        public bool CanEditSmoothing
        {
            get { return (Spectrums?.Count > 0 && !gui_locked); }
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
            UpdateGui();
        }

        public bool CanDerivative
        {
            get { return (Spectrums?.Count > 0 && !gui_locked); }
        }
        public void Derivative()
        {
            if (selected_spectrum == null)
            {
                MessageBox.Show("No Spectrum selected");
                return;
            }
            var windowManager = new WindowManager();
            var derivative_dialog = new DerivativeViewModel(SelectedSpectrum, Spectrometer);
            windowManager.ShowDialogAsync(derivative_dialog);

            if (derivative_dialog.OperationDone)
            {
                Spectrum result = derivative_dialog.ResultSpectrum;
                result.Id = last_id++;

                Spectrums.Add(result);
                UpdatePlot();
            }
            UpdateGui();
        }

        private bool IsSpectrometerConnected()
        {
            return Spectrometer.Connected;
        }

    }
}
