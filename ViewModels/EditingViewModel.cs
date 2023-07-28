using Caliburn.Micro;
using DiplomaMB.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace DiplomaMB.ViewModels
{

    public enum Operations
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        BaselineRemove
    }

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

        private double double_value;

        public double DoubleValue
        {
            get { return double_value; }
            set
            {
                double_value = value;
                NotifyOfPropertyChange(() => DoubleValue);
            }
        }


        private bool is_spectrums2_comboBox_enabled = true;

        public bool IsSpectrums2ComboBoxEnabled
        {
            get { return is_spectrums2_comboBox_enabled; }
            set
            {
                is_spectrums2_comboBox_enabled = value;
                NotifyOfPropertyChange(() => IsSpectrums2ComboBoxEnabled);
            }
        }

        private Operations selected_operation;

        public Operations SelectedOperation
        {
            get => selected_operation;
            set
            {
                if (selected_operation != value)
                {
                    selected_operation = value;
                    UpdateGui();
                    NotifyOfPropertyChange(() => SelectedOperation);
                }
            }
        }

        public EditingViewModel(BindableCollection<Spectrum> spectrums)
        {
            operation_done = false;
            spectrums1 = spectrums;
            spectrums2 = spectrums;
            result_spectrum = new Spectrum();

            double_value = 1.0;

            selected_spectrum1 = Spectrums1.FirstOrDefault();
            selected_spectrum2 = Spectrums2.FirstOrDefault();
            selected_operation = Operations.Add;
        }


        private void UpdateGui()
        {
            Debug.WriteLine("Hello");
            if (selected_operation == Operations.BaselineRemove)
            {
                Debug.WriteLine("Baseline");
                IsSpectrums2ComboBoxEnabled = false;
            }
            else
            {
                Debug.WriteLine("Other");
                IsSpectrums2ComboBoxEnabled = true;
            }
        }


        public void ShowSelectedValue()
        {
            // Show a message box with the selected value
            MessageBox.Show($"Selected value: {SelectedOperation}");

            switch(SelectedOperation)
            {
                case Operations.Add:
                    Debug.WriteLine("Add");
                    break;
                case Operations.Subtract:
                    Debug.WriteLine("Subtract");
                    break;
                case Operations.Multiply:
                    Debug.WriteLine("Multiply");
                    break;
            }
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
