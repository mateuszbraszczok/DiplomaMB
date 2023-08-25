/**
 * @file   EditingViewModel.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief  This file contains the EditingViewModel class, responsible for handling the editing operations 
 *         related to spectrums like Add, Subtract, Multiply, Divide, and BaselineRemove.
 */

using Caliburn.Micro;
using DiplomaMB.Models;
using System.Linq;
using System.Windows;

namespace DiplomaMB.ViewModels
{
    /// <summary>
    /// Enumeration to represent the types of operations that can be performed on spectra.
    /// </summary>
    public enum Operations
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        BaselineRemove
    }

    /// <summary>
    /// ViewModel class for editing spectra.
    /// </summary>
    public class EditingViewModel : Screen
    {
        private BindableCollection<Spectrum> spectrums1;
        /// <summary>
        /// Gets or sets the first set of spectra.
        /// </summary>
        public BindableCollection<Spectrum> Spectrums1
        {
            get => spectrums1;
            set => spectrums1 = value;
        }

        private BindableCollection<Spectrum> spectrums2;
        /// <summary>
        /// Gets or sets the second set of spectra.
        /// </summary>
        public BindableCollection<Spectrum> Spectrums2
        {
            get => spectrums2;
            set => spectrums2 = value;
        }

        private Spectrum? selected_spectrum1;
        /// <summary>
        /// Gets or sets the selected spectrum from the first set of spectra.
        /// </summary>
        public Spectrum? SelectedSpectrum1
        {
            get => selected_spectrum1;
            set { selected_spectrum1 = value; NotifyOfPropertyChange(() => SelectedSpectrum1); }
        }

        private Spectrum? selected_spectrum2;
        /// <summary>
        /// Gets or sets the selected spectrum from the second set of spectra.
        /// </summary>
        public Spectrum? SelectedSpectrum2
        {
            get => selected_spectrum2;
            set { selected_spectrum2 = value; NotifyOfPropertyChange(() => SelectedSpectrum2); }
        }

        private Spectrum result_spectrum;
        /// <summary>
        /// Gets or sets the resulting spectrum after an operation is performed.
        /// </summary>
        public Spectrum ResultSpectrum
        {
            get => result_spectrum;
            set => result_spectrum = value;
        }

        private Operations selected_operation;
        // <summary>
        /// Gets or sets the selected operation to be performed.
        /// </summary>
        public Operations SelectedOperation
        {
            get => selected_operation;
            set
            {
                selected_operation = value;
                IsSpectrums2ComboBoxEnabled = value != Operations.BaselineRemove;
                NotifyOfPropertyChange(() => SelectedOperation);
            }
        }

        private bool operation_done;
        /// <summary>
        /// Gets or sets a value indicating whether the operation has been done.
        /// </summary>
        public bool OperationDone
        {
            get => operation_done;
            set => operation_done = value;
        }

        private double double_value;
        /// <summary>
        /// Gets or sets a double value used for certain operations.
        /// </summary>
        public double DoubleValue
        {
            get => double_value;
            set { double_value = value; NotifyOfPropertyChange(() => DoubleValue); }
        }

        private string new_spectrum_name;
        /// <summary>
        /// Gets or sets the name for the new spectrum after an operation.
        /// </summary>
        public string NewSpectrumName
        {
            get => new_spectrum_name;
            set => new_spectrum_name = value;
        }

        private bool is_spectrums2_comboBox_enabled;
        /// <summary>
        /// Gets or sets a value indicating whether the second spectrum ComboBox is enabled.
        /// </summary>
        public bool IsSpectrums2ComboBoxEnabled
        {
            get => is_spectrums2_comboBox_enabled;
            set { is_spectrums2_comboBox_enabled = value; NotifyOfPropertyChange(() => IsSpectrums2ComboBoxEnabled); }
        }

        private long baseline_removal_lambda = 10000000L;
        /// <summary>
        /// Gets or sets the lambda value for baseline removal.
        /// </summary>
        public long BaselineRemovalLambda
        {
            get => baseline_removal_lambda;
            set { baseline_removal_lambda = value; NotifyOfPropertyChange(() => BaselineRemovalLambda); }
        }

        private bool is_panel_1_enabled;
        /// <summary>
        /// Gets or sets a value indicating whether the first panel is enabled.
        /// </summary>
        public bool IsPanel1Enabled
        {
            get => is_panel_1_enabled;
            set
            {
                is_panel_1_enabled = value;
                is_panel_2_enabled = !value;
                NotifyOfPropertyChange(() => IsPanel1Enabled);
                NotifyOfPropertyChange(() => IsPanel2Enabled);
            }
        }

        private bool is_panel_2_enabled;
        /// <summary>
        /// Gets or sets a value indicating whether the second panel is enabled.
        /// </summary>
        public bool IsPanel2Enabled
        {
            get => is_panel_2_enabled;
            set
            {
                is_panel_1_enabled = !value;
                is_panel_2_enabled = value;
                NotifyOfPropertyChange(() => IsPanel1Enabled);
                NotifyOfPropertyChange(() => IsPanel2Enabled);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditingViewModel"/> class.
        /// </summary>
        /// <param name="spectrums">The set of spectra to be edited.</param>
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

        /// <summary>
        /// Executes the selected operation and closes the window.
        /// </summary>
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

        /// <summary>
        /// Adds the selected spectrums based on user input. 
        /// If both spectrums are selected from the list, it adds the two together. 
        /// If only one is selected, it adds a user-specified double value to it.
        /// </summary>
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

        /// <summary>
        /// Subtracts the selected spectrums based on user input. 
        /// If both spectrums are selected from the list, it subtracts the second from the first. 
        /// If only one is selected, it subtracts a user-specified double value from it.
        /// </summary>
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

        /// <summary>
        /// Multiplies the selected spectrums based on user input. 
        /// If both spectrums are selected from the list, it multiplies the two together. 
        /// If only one is selected, it multiplies it by a user-specified double value.
        /// </summary>
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

        /// <summary>
        /// Divides the selected spectrums based on user input. 
        /// If both spectrums are selected from the list, it divides the first by the second. 
        /// If only one is selected, it divides it by a user-specified double value.
        /// </summary>
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

        /// <summary>
        /// Removes the baseline of the selected spectrum using the given lambda value.
        /// </summary>
        private void BaselineRemoveSelectedSpectrum()
        {
            if (SelectedSpectrum1 != null)
            {
                ResultSpectrum = SelectedSpectrum1.PerformBaselineCorrection(SelectedSpectrum1, BaselineRemovalLambda, 20);
                MessageBox.Show("Removed baseline");
                OperationDone = true;
            }
        }
    }

}
