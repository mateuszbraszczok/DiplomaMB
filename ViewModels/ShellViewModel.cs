/**
 * @file   ShellViewModel.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief  ShellViewModel class responsible for handling the main application logic including spectrometer
 *         connectivity and data acquisition.
 */

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
    /// <summary>
    /// ShellViewModel class provides properties and methods for the main application logic.
    /// </summary>
    public class ShellViewModel : Screen
    {
        private PlotModel plot_model;
        /// <summary>
        /// Gets or sets the PlotModel.
        /// </summary>
        public PlotModel PlotModel
        {
            get => plot_model;
            set { plot_model = value; NotifyOfPropertyChange(() => PlotModel); }
        }

        private ISpectrometer spectrometer;
        /// <summary>
        /// Gets or sets the spectrometer object.
        /// </summary>
        public ISpectrometer Spectrometer
        {
            get => spectrometer;
            set { spectrometer = value; NotifyOfPropertyChange(() => Spectrometer); }
        }

        private Spectrum? selected_spectrum;
        /// <summary>
        /// Gets or sets the selected spectrum.
        /// </summary>
        public Spectrum? SelectedSpectrum
        {
            get => selected_spectrum;
            set { selected_spectrum = value; NotifyOfPropertyChange(() => SelectedSpectrum); }
        }

        private BindableCollection<Spectrum> spectrums;
        /// <summary>
        /// Gets or sets the list of spectrums.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the number of frames to acquire.
        /// </summary>
        public int FramesToAcquire
        {
            get => frames_to_acquire;
            set { frames_to_acquire = value; NotifyOfPropertyChange(() => FramesToAcquire); }
        }

        private int integration_time;
        /// <summary>
        /// Gets or sets the integration time for the spectrometer.
        /// </summary>
        public int IntegrationTime
        {
            get => integration_time;
            set { integration_time = value; NotifyOfPropertyChange(() => IntegrationTime); }
        }

        private SmartRead smart_read;
        /// <summary>
        /// Gets or sets the SmartRead object for the spectrometer.
        /// </summary>
        public SmartRead SmartRead
        {
            get => smart_read;
            set { smart_read = value; NotifyOfPropertyChange(() => SmartRead); }
        }

        private bool should_acquire = false;
        private bool gui_locked = false;
        private Thread? continuously_acquiring_thread = null;

        /// <summary>
        /// Initializes a new instance of the ShellViewModel class.
        /// </summary>
        public ShellViewModel()
        {
            plot_model = new PlotModel { Title = "Spectrums Raw Data", Background = OxyColors.LightGray };
            spectrometer = new BwtekSpectrometer();
            spectrums = new BindableCollection<Spectrum> { };
            smart_read = new SmartRead();

            frames_to_acquire = 1;
            InitializePlot();
        }

        /// <summary>
        /// Event handler for closing the application.
        /// </summary>
        public void OnClose(CancelEventArgs e)
        {
            Debug.WriteLine("Goodbye");
            Environment.Exit(Environment.ExitCode);
        }

        /// <summary>
        /// Initializes the PlotModel.
        /// </summary>
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

        /// <summary>
        /// Updates the GUI based on the state of various properties.
        /// </summary>
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

        /// <summary>
        /// Updates the PlotModel with the enabled spectrums.
        /// </summary>
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

        /// <summary>
        /// Safely exits the program by disconnecting the spectrometer and shutting down the application.
        /// </summary>
        public void ExitProgram()
        {
            Spectrometer.Disconnect();
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Checks if the spectrometer can be connected.
        /// </summary>
        /// <returns>True if the spectrometer can be connected, otherwise false.</returns>
        public bool CanConnectSpectrometer
        {
            get { return !IsSpectrometerConnected() && !gui_locked; }
        }
        /// <summary>
        /// Connects to the spectrometer and updates the integration time if the connection is successful.
        /// </summary>
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

        /// <summary>
        /// Checks if the spectrometer can be reset.
        /// </summary>
        /// <returns>True if the spectrometer can be reset, otherwise false.</returns>
        public bool CanResetSpectrometer
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        /// <summary>
        /// Resets the spectrometer device.
        /// </summary>
        public void ResetSpectrometer()
        {
            Spectrometer.ResetDevice();
        }

        /// <summary>
        /// Checks if the integration time can be set.
        /// </summary>
        /// <returns>True if the integration time can be set, otherwise false.</returns>
        public bool CanSetIntegrationTime
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        /// <summary>
        /// Sets the integration time for the spectrometer and shows a message box indicating the result.
        /// </summary>
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

        /// <summary>
        /// Checks if a spectrum can be acquired.
        /// </summary>
        /// <returns>True if a spectrum can be acquired, otherwise false.</returns>
        public bool CanGetSpectrum
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        /// <summary>
        /// Acquires a spectrum and adds it to the Spectrums collection.
        /// </summary>
        public async void GetSpectrum()
        {
            gui_locked = true;
            UpdateGui();
            Debug.WriteLine("before read data");
            List<Spectrum> spectrum_list = await Task.Run(() => Spectrometer.ReadData(FramesToAcquire));

            Debug.WriteLine("readed data");
            foreach (Spectrum spectrum in spectrum_list)
            {
                //spectrum.Id = last_id;
                //spectrum.Name = "Spectrum " + last_id.ToString();
                //last_id += 1;
                Spectrums.Add(spectrum);
            }
            UpdatePlot();
            gui_locked = false;
            UpdateGui();
        }

        /// <summary>
        /// Checks if a dark scan can be acquired.
        /// </summary>
        /// <returns>True if a dark scan can be acquired, otherwise false.</returns>
        public bool CanGetDarkScan
        {
            get { return IsSpectrometerConnected() && !gui_locked; }
        }
        /// <summary>
        /// Acquires a dark scan from the spectrometer.
        /// </summary>
        public async void GetDarkScan()
        {
            gui_locked = true;
            UpdateGui();
            await Task.Run(() => Spectrometer.GetDarkScan());

            NotifyOfPropertyChange(() => Spectrometer);
            gui_locked = false;
            UpdateGui();
        }

        /// <summary>
        /// Checks if the acquiring process can be started.
        /// </summary>
        public bool CanStartAcquire
        {
            get { return IsSpectrometerConnected() && !gui_locked && !should_acquire; }
        }
        /// <summary>
        /// Starts acquiring spectra continuously using a separate thread.
        /// </summary>
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

        /// <summary>
        /// Checks if the acquiring process can be stopped.
        /// </summary>
        public bool CanStopAcquire
        {
            get { return IsSpectrometerConnected() && should_acquire; }
        }
        /// <summary>
        /// Stops the acquiring process and unlocks the GUI.
        /// </summary>
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

        /// <summary>
        /// Private method that continuously acquires spectra while 'should_acquire' is true.
        /// </summary>
        private void AcquiringSpectrums()
        {
            bool acquiredFirstSpectrum = false;

            while (should_acquire)
            {
                Spectrum spectrum = Spectrometer.ReadData(1).First();
                //Spectrum spectrum = Spectrometer.GenerateDummySpectrum();
                spectrum.Name = "Spectrum " + spectrum.Id.ToString();

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

        /// <summary>
        /// Checks if a "smart" spectrum can be acquired.
        /// </summary>
        public bool CanGetSpectrumSmart
        {
            get { return IsSpectrometerConnected() && !gui_locked && spectrometer is BwtekSpectrometer; }
        }
        /// <summary>
        /// Acquires a "smart" spectrum asynchronously.
        /// </summary>
        public async void GetSpectrumSmart()
        {
            gui_locked = true;
            UpdateGui();
            Spectrum spectrum = await Task.Run(() => Spectrometer.ReadDataSmart(SmartRead));
            spectrum.Name = "Spectrum " + spectrum.Id.ToString();

            if (spectrum.DataValues != null)
            {
                Spectrums.Add(spectrum);
                UpdatePlot();
            }

            gui_locked = false;
            UpdateGui();
        }

        /// <summary>
        /// Checks if a spectrum can be loaded from a file.
        /// </summary>
        public bool CanLoadSpectrum
        {
            get { return !gui_locked; }
        }
        /// <summary>
        /// Opens a dialog to load a spectrum from a file.
        /// </summary>
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
                Spectrum spectrum = new Spectrum(file_path);
                Spectrums.Add(spectrum);
                UpdatePlot();
            }
            UpdateGui();
        }

        /// <summary>
        /// Checks if a dark scan can be loaded from a file.
        /// </summary>
        public bool CanLoadDarkScan
        {
            get { return !gui_locked && IsSpectrometerConnected(); }
        }
        /// <summary>
        /// Loads a dark scan from a file.
        /// </summary>
        public void LoadDarkScan()
        {
            Spectrometer.LoadDarkScanFromFile();
            NotifyOfPropertyChange(() => Spectrometer);
            UpdateGui();
        }

        /// <summary>
        /// Checks if a dark scan can be saved to a file.
        /// </summary>
        public bool CanSaveDarkScan
        {
            get { return !gui_locked && IsSpectrometerConnected() && Spectrometer.DarkScanTaken; }
        }
        /// <summary>
        /// Saves the current dark scan to a file.
        /// </summary>
        public void SaveDarkScan()
        {
            Spectrometer.SaveDarkScanToFile();
            NotifyOfPropertyChange(() => Spectrometer);
            UpdateGui();
        }

        /// <summary>
        /// Checks if the selected spectrum can be deleted.
        /// </summary>
        public bool CanDeleteSelectedSpectrum
        {
            get { return !gui_locked && spectrums.Count > 0; }
        }
        /// <summary>
        /// Deletes the selected spectrum and updates the plot.
        /// </summary>
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

        /// <summary>
        /// Checks if all spectra can be deleted.
        /// </summary>
        public bool CanDeleteAllSpectrums
        {
            get { return !gui_locked && spectrums.Count > 0; }
        }
        /// <summary>
        /// Deletes all spectra and updates the plot.
        /// </summary>
        public void DeleteAllSpectrums()
        {
            Spectrums.Clear();
            SelectedSpectrum = null;
            UpdatePlot();
            UpdateGui();
        }

        /// <summary>
        /// Checks if the selected spectrum can be saved.
        /// </summary>
        public bool CanSaveSelectedSpectrum
        {
            get { return !gui_locked && spectrums.Count > 0; }
        }
        /// <summary>
        /// Saves the selected spectrum to a file. If no spectrum is selected, shows an error message.
        /// </summary>
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

        /// <summary>
        /// Checks if peak detection can be performed on the spectra.
        /// </summary>
        public bool CanSpectrumPeaks
        {
            get { return Spectrums?.Count > 0 && !gui_locked; }
        }
        /// <summary>
        /// Opens a dialog for peak detection in the selected spectrum.
        /// </summary>
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

        /// <summary>
        /// Checks if additional operations can be performed on the spectra.
        /// </summary>
        public bool CanSpectrumOperations
        {
            get { return Spectrums?.Count > 0 && !gui_locked; }
        }
        /// <summary>
        /// Opens a dialog for editing and applying various operations on the spectra.
        /// </summary>
        public void SpectrumOperations()
        {
            var windowManager = new WindowManager();
            var editing_dialog = new EditingViewModel(Spectrums);
            windowManager.ShowDialogAsync(editing_dialog);

            if (editing_dialog.OperationDone)
            {
                Spectrum result = editing_dialog.ResultSpectrum;

                Spectrums.Add(result);
                UpdatePlot();
            }
            UpdateGui();
        }

        /// <summary>
        /// Checks if smoothing can be applied to the spectra.
        /// </summary>
        public bool CanEditSmoothing
        {
            get { return (Spectrums?.Count > 0 && !gui_locked); }
        }
        /// <summary>
        /// Opens a dialog for smoothing the selected spectrum.
        /// </summary>
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
                        smoothed_spectrum.Name = selected_spectrum.Name + "_smoothed";
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

        /// <summary>
        /// Checks if derivative calculations can be performed on the spectra.
        /// </summary>
        public bool CanDerivative
        {
            get { return (Spectrums?.Count > 0 && !gui_locked); }
        }
        /// <summary>
        /// Opens a dialog for derivative calculations on the selected spectrum.
        /// </summary>
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

                Spectrums.Add(result);
                UpdatePlot();
            }
            UpdateGui();
        }

        /// <summary>
        /// Checks if the spectrometer is connected.
        /// </summary>
        private bool IsSpectrometerConnected()
        {
            return Spectrometer.Connected;
        }

    }
}
