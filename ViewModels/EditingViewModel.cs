using Caliburn.Micro;
using DiplomaMB.Models;
using System.Linq;
using System.Windows;

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
            get => spectrums1;
            set => spectrums1 = value;
        }
        private BindableCollection<Spectrum> spectrums2;
        public BindableCollection<Spectrum> Spectrums2
        {
            get => spectrums2;
            set => spectrums2 = value;
        }

        private Spectrum? selected_spectrum1;
        public Spectrum? SelectedSpectrum1
        {
            get => selected_spectrum1;
            set { selected_spectrum1 = value; NotifyOfPropertyChange(() => SelectedSpectrum1); }
        }

        private Spectrum? selected_spectrum2;
        public Spectrum? SelectedSpectrum2
        {
            get => selected_spectrum2;
            set { selected_spectrum2 = value; NotifyOfPropertyChange(() => SelectedSpectrum2); }
        }

        private Spectrum result_spectrum;
        public Spectrum ResultSpectrum
        {
            get => result_spectrum;
            set => result_spectrum = value;
        }

        private Operations selected_operation;
        public Operations SelectedOperation
        {
            get => selected_operation;
            set
            {
                selected_operation = value;
                if (value == Operations.BaselineRemove)
                {
                    IsSpectrums2ComboBoxEnabled = false;
                }
                else
                {
                    IsSpectrums2ComboBoxEnabled = true;
                }
                NotifyOfPropertyChange(() => SelectedOperation);
            }
        }

        private bool operation_done;
        public bool OperationDone
        {
            get => operation_done;
            set => operation_done = value;
        }

        private double double_value;
        public double DoubleValue
        {
            get => double_value;
            set { double_value = value; NotifyOfPropertyChange(() => DoubleValue); }
        }

        private string new_spectrum_name;
        public string NewSpectrumName
        {
            get => new_spectrum_name;
            set => new_spectrum_name = value;
        }

        private bool is_spectrums2_comboBox_enabled;
        public bool IsSpectrums2ComboBoxEnabled
        {
            get => is_spectrums2_comboBox_enabled;
            set { is_spectrums2_comboBox_enabled = value; NotifyOfPropertyChange(() => IsSpectrums2ComboBoxEnabled); }
        }

        private long baseline_removal_lambda = 10000000L;
        public long BaselineRemovalLambda
        {
            get => baseline_removal_lambda;
            set { baseline_removal_lambda = value; NotifyOfPropertyChange(() => BaselineRemovalLambda); }
        }

        private bool is_panel_1_enabled;
        public bool IsPanel1Enabled
        {
            get => is_panel_1_enabled;
            set
            {
                is_panel_1_enabled = value;
                NotifyOfPropertyChange(() => IsPanel1Enabled);
                if (value)
                {
                    IsPanel2Enabled = !value;
                }
            }
        }

        private bool is_panel_2_enabled;
        public bool IsPanel2Enabled
        {
            get => is_panel_2_enabled;
            set
            {
                is_panel_2_enabled = value;
                NotifyOfPropertyChange(() => IsPanel2Enabled);
                if (value)
                {
                    IsPanel1Enabled = !value;
                }
            }
        }


        public EditingViewModel(BindableCollection<Spectrum> spectrums)
        {
            operation_done = false;
            spectrums1 = spectrums;
            spectrums2 = spectrums;
            result_spectrum = new Spectrum();

            DoubleValue = 1.0;
            is_spectrums2_comboBox_enabled = true;
            is_panel_1_enabled = true;
            new_spectrum_name = "new_spectrum";

            BaselineRemovalLambda = 10000000L;

            selected_spectrum1 = Spectrums1.FirstOrDefault();
            selected_spectrum2 = Spectrums2.FirstOrDefault();
            selected_operation = Operations.Add;
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
            ResultSpectrum.Name = NewSpectrumName;
            TryCloseAsync();
        }

        private void AddSelectedSpectrums()
        {
            if (IsPanel1Enabled)
            {
                if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 + SelectedSpectrum2;
                    MessageBox.Show("Added two spectrums");
                    OperationDone = true;
                }
            }
            else
            {
                if (SelectedSpectrum1 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 + DoubleValue;
                    MessageBox.Show("Added value to spectrum");
                    OperationDone = true;
                }
            }
        }

        private void SubtractSelectedSpectrums()
        {
            if (IsPanel1Enabled)
            {
                if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 - SelectedSpectrum2;
                    MessageBox.Show("Subtracted two spectrums");
                    OperationDone = true;
                }
            }
            else
            {
                if (SelectedSpectrum1 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 - DoubleValue;
                    MessageBox.Show("Subtracted value from spectrum");
                    OperationDone = true;
                }
            }
        }

        private void MultiplySelectedSpectrums()
        {
            if (IsPanel1Enabled)
            {
                if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 * SelectedSpectrum2;
                    MessageBox.Show("Multiplied two spectrums");
                    OperationDone = true;
                }
            }
            else
            {
                if (SelectedSpectrum1 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 * DoubleValue;
                    MessageBox.Show("Multiplied spectrum by value");
                    OperationDone = true;
                }
            }
        }

        private void DivideSelectedSpectrums()
        {
            if (IsPanel1Enabled)
            {
                if (SelectedSpectrum1 != null && SelectedSpectrum2 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 / SelectedSpectrum2;
                    MessageBox.Show("Divided two spectrums");
                    OperationDone = true;
                }
            }
            else
            {
                if (SelectedSpectrum1 != null)
                {
                    ResultSpectrum = SelectedSpectrum1 / DoubleValue;
                    MessageBox.Show("Divided spectrum by value");
                    OperationDone = true;
                }
            }
        }

        private void BaselineRemoveSelectedSpectrum()
        {
            if (SelectedSpectrum1 != null)
            {
                MessageBox.Show($"baseline lambda: {BaselineRemovalLambda}");
                ResultSpectrum = SelectedSpectrum1.PerformBaselineCorrection(SelectedSpectrum1, BaselineRemovalLambda, 20);
                MessageBox.Show("Removed baseline");
                OperationDone = true;
            }
        }
    }

}
