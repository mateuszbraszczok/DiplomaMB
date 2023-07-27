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

        private bool operation_done;
        public bool OperationDone
        {
            get { return operation_done; }
            set { operation_done = value; }
        }

        public enum Operations
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            BaselineRemove
        }

        public string[] OperationsValues { get; } = Enum.GetNames(typeof(Operations));
        public Operations SelectedOperation { get; set; }

        public EditingViewModel(BindableCollection<Spectrum> spectrums)
        {
            OperationDone = false;
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
                    AddSelectedSpectrums();
                    break;
                case Operations.Subtract:
                    SubtractSelectedSpectrums();
                    break;
                case Operations.Multiply:
                    MultiplySelectedSpectrums();
                    break;
                case Operations.Divide:
                    DivideSelectedSpectrums();
                    break;

                case Operations.BaselineRemove:
                    BaselineRemoveSelectedSpectrum();
                    break;
                default:
                    break;
            }
            TryCloseAsync();
        }

        private void AddSelectedSpectrums()
        {
            if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
            {
                ResultSpectrum = SelectedSpectrum1 + SelectedSpectrum2;
                MessageBox.Show("Added two spectrums");
                OperationDone = true;
            }
        }

        private void SubtractSelectedSpectrums()
        {
            if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
            {
                ResultSpectrum = SelectedSpectrum1 - SelectedSpectrum2;
                MessageBox.Show("Subtracted two spectrums");
                OperationDone = true;
            }
        }

        private void MultiplySelectedSpectrums()
        {
            if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
            {
                ResultSpectrum = SelectedSpectrum1 * SelectedSpectrum2;
                MessageBox.Show("Multplied two spectrums");
                OperationDone = true;
            }
        }

        private void DivideSelectedSpectrums()
        {
            if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
            {
                ResultSpectrum = SelectedSpectrum1 / SelectedSpectrum2;
                MessageBox.Show("Divided two spectrums");
                OperationDone = true;
            }
        }

        private void BaselineRemoveSelectedSpectrum()
        {
            if (SelectedSpectrum1 != null)
            {
                ResultSpectrum = SelectedSpectrum1.PerformBaselineCorrection(SelectedSpectrum1, 10000000, 5);
                MessageBox.Show("Removed baseline");
                OperationDone = true;
            }
        }
    }

}
