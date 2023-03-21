using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using OxyPlot.Wpf;
using DiplomaMB.Models;
using System.ComponentModel;
using Microsoft.Win32;
using System.Globalization;
using System.IO;

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

		private Spectrometer spectrometer;
		public Spectrometer Spectrometer
        {
			get { return spectrometer; }
			set { spectrometer = value; NotifyOfPropertyChange(() => Spectrometer); }
		}

		private Spectrum selected_spectrum;
		public Spectrum SelectedSpectrum
		{
			get { return selected_spectrum; }
			set { selected_spectrum = value; NotifyOfPropertyChange(() => SelectedSpectrum); }
        }

        private BindableCollection<Spectrum> spectrums;
        public BindableCollection<Spectrum> Spectrums
        {
            get { return spectrums; }
            set { spectrums = value; NotifyOfPropertyChange(() => Spectrums);}
        }

        private int frames_to_acquire;
        public int FramesToAcquire
        {
            get { return frames_to_acquire; }
            set { frames_to_acquire = value; NotifyOfPropertyChange(() => FramesToAcquire); }
        }

        private SmartRead smart_read;

        public SmartRead SmartRead
        {
            get { return smart_read; }
            set { smart_read = value; }
        }


        private int last_id = 0;
        public ShellViewModel()
        {
            PlotModel = new PlotModel { Title = "Spectrums Raw Data" };
            Spectrometer = new Spectrometer();
            Spectrums = new BindableCollection<Spectrum> { };

            Spectrums.CollectionChanged += (e, v) => UpdatePlot();

            InitializePlot();
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
            MessageBox.Show("Plot update");
            PlotModel.Series.Clear();
            double min_x_value = double.MaxValue;
            double max_x_value = double.MinValue;
            ushort min_y_value = ushort.MaxValue;
            ushort max_y_value = ushort.MinValue;
            foreach (var spectrum in Spectrums)
            {
                if (spectrum.Enabled == true)
                {
                    PlotModel.Series.Add(spectrum.getPlotSerie());
                    double max_x = spectrum.wavelengths.Max();
                    double min_x = spectrum.wavelengths.Min();
                    if (max_x > max_x_value)
                    {
                        max_x_value = max_x;
                    }
                    if (min_x < min_x_value)
                    {
                        min_x_value = min_x;
                    }

                    ushort max_y = spectrum.dataArray.Max();
                    ushort min_y = spectrum.dataArray.Min();
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
            if (spectrums.Count == 0)
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
                Minimum = min_y_value,
                Maximum = max_y_value,
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
            System.Windows.Application.Current.Shutdown();
        }

        public void ConnectSpectrometer()
        {
            NotifyOfPropertyChange(() => Spectrometer);
            Spectrometer.Connect();
            if (Spectrometer.Connected == true)
            {
                //IntegrationTime = Spectrometer.IntegrationTime.ToString();
            }
            else
            {
                MessageBox.Show("Can't connect with spectrometer", "Can't connect with spectrometer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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
                //Spectrometer.SetIntegrationTime(IntegrationTime);
            }
            //IntegrationTime = spectrometer.IntegrationTime.ToString();
        }

        public bool CanGetSpectrum()
        {
            return IsSpectrometerConnected();
        }
        public void GetSpectrum()
        {
            List<Spectrum> spectrum_list = Spectrometer.ReadData(FramesToAcquire);

            foreach (Spectrum spectrum in spectrum_list)
            {
                spectrum.Id = last_id;
                spectrum.Name = "Spectrum " + last_id.ToString();
                last_id += 1;
                spectrums.Add(spectrum);
            }
            UpdatePlot();
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

            if (spectrum.dataArray != null)
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
        }

        public void LoadSpectrum()
        {
            OpenFileDialog dialog = new()
            {
                Title = "Open CSV File",
                Filter = "CSV Files (*.csv)|*.csv"
            };
            string filename;
            if (dialog.ShowDialog() == true)
            {
                filename = dialog.FileName;
            }
            else
            {
                return;
            }

            using var reader = new StreamReader(filename);
            List<double> wavelengths = new();
            List<ushort> data = new();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                wavelengths.Add(double.Parse(values[0], CultureInfo.InvariantCulture));
                data.Add(Convert.ToUInt16(values[1]));
            }
            string name = Path.GetFileNameWithoutExtension(filename);
            Spectrum spectrum = new Spectrum(wavelengths, data, name)
            {
                Id = last_id
            };
            last_id++;
            spectrums.Add(spectrum);

            UpdatePlot();
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

        public void OnChecked()
        {
            MessageBox.Show("Clicked checkbox");
            UpdatePlot();
        }


        private bool IsSpectrometerConnected()
        {
            if (!Spectrometer.Connected) { MessageBox.Show("Spectrometer is not connected"); }
            return Spectrometer.Connected;
        }



    }
}
