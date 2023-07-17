using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DiplomaMB.ViewModels
{
    public class EditingViewModel : Screen
    {
        private BindableCollection<Spectrum> spectrums1;
        public BindableCollection<Spectrum> Spectrums1
        {
            get { return spectrums1; }
            set { spectrums1 = value; }
        }
        private BindableCollection<Spectrum> spectrums2;
        public BindableCollection<Spectrum> Spectrums2
        {
            get { return spectrums2; }
            set { spectrums2 = value; }
        }

        private Spectrum? selected_spectrum1;
        public Spectrum? SelectedSpectrum1
        {
            get { return selected_spectrum1; }
            set { selected_spectrum1 = value; NotifyOfPropertyChange(() => SelectedSpectrum1); }
        }

        private Spectrum? selected_spectrum2;
        public Spectrum? SelectedSpectrum2
        {
            get { return selected_spectrum2; }
            set { selected_spectrum2 = value; NotifyOfPropertyChange(() => SelectedSpectrum2); }
        }

        private Spectrum result_spectrum;
        public Spectrum ResultSpectrum
        {
            get { return result_spectrum; }
            set { result_spectrum = value; }
        }

        public enum Operations
        {
            Add,
            Subtract,
            Divide
        }

        public string[] OperationsValues { get; } = Enum.GetNames(typeof(Operations));
        public Operations SelectedOperation { get; set; }

        public EditingViewModel(BindableCollection<Spectrum> spectrums)
        {
            Spectrums1 = spectrums;
            Spectrums2 = spectrums;
            ResultSpectrum = new Spectrum();

            SelectedSpectrum1 = Spectrums1.FirstOrDefault();
            SelectedSpectrum2 = Spectrums2.FirstOrDefault();
            SelectedOperation = Operations.Add; // Enum.GetNames(typeof(Operations)).Cast<Operations>().FirstOrDefault();
        }

        public void CloseWindow()
        {
            switch (SelectedOperation)
            {      
                case Operations.Add:
                    ResultSpectrum = SelectedSpectrum1 + SelectedSpectrum2;
                    MessageBox.Show("Add");
                    break;
                case Operations.Subtract:
                    ResultSpectrum = SelectedSpectrum1 - SelectedSpectrum2;
                    MessageBox.Show("Subtract");
                    break;
                case Operations.Divide:
                    MessageBox.Show("Divide");
                    break;
                default:
                    break;
            }
            TryCloseAsync();
        }
    }
}
