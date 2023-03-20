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
			set { spectrometer = value; }
		}

		private int selected_spectrum;
		public int SelectedPectrum
		{
			get { return selected_spectrum; }
			set { selected_spectrum = value; }
		}
        private BindableCollection<Spectrum> spectrums;

        public BindableCollection<Spectrum> Spectrums
        {
            get { return spectrums; }
            set { spectrums = value; }
        }


        public ShellViewModel()
        {
            PlotModel = new PlotModel { Title = "Spectrums Raw Data" };
            Spectrometer = new Spectrometer();
            Spectrums = new BindableCollection<Spectrum> { };

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



    }
}
