/**
 * @file Converters.cs
 * @author Mateusz Braszczok
 * @date 2023-08-25
 * @brief Provides utilities for converting between types in WPF.
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DiplomaMB.Utils
{
    public class EnumBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// Converts an enum value to a boolean.
        /// </summary>
        /// <param name="value">The enum value to be converted.</param>
        /// <param name="targetType">The target type for the conversion (unused).</param>
        /// <param name="parameter">An optional string representing the enum value to consider as 'true'.</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>Returns boolean value after conversion.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            if (Enum.IsDefined(value.GetType(), value) == false)
                return DependencyProperty.UnsetValue;

            object parameterValue = Enum.Parse(value.GetType(), parameterString);

            return parameterValue.Equals(value);
        }

        /// <summary>
        /// Converts a boolean value back to an enum value.
        /// </summary>
        /// <param name="value">The boolean value to convert back.</param>
        /// <param name="targetType">The target type of the enum.</param>
        /// <param name="parameter">An optional string representing the enum value to consider as 'true'.</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>Returns enum value after back-conversion.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string parameterString = parameter as string;
            if (parameterString == null)
                return DependencyProperty.UnsetValue;

            return Enum.Parse(targetType, parameterString);
        }
        #endregion
    }

    /// <summary>
    /// Converts boolean values to Visibility enum and vice versa.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility enum value.
        /// </summary>
        /// <param name="value">The boolean value to be converted.</param>
        /// <param name="targetType">The target type for the conversion (unused).</param>
        /// <param name="parameter">A string "invert" to invert the logic.</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>Returns Visibility value after conversion.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value as bool?;
            var invert = (parameter as string)?.Equals("invert", StringComparison.OrdinalIgnoreCase) ?? false;

            if (!boolValue.HasValue)
                return Visibility.Collapsed;

            if (invert)
                return boolValue.Value ? Visibility.Collapsed : Visibility.Visible;

            return boolValue.Value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Not implemented for this converter.
        /// </summary>
        /// <param name="value">The Visibility value to convert back.</param>
        /// <param name="targetType">The target type for the back-conversion (unused).</param>
        /// <param name="parameter">A string "invert" to invert the logic (unused).</param>
        /// <param name="culture">Culture info (unused).</param>
        /// <returns>Throws NotImplementedException.</returns>
        /// <exception cref="NotImplementedException">Throws this exception as the method is not implemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
