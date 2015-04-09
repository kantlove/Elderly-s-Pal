﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace NiceDreamers.Windows.Converters
{
    /// <summary>
    ///     Multiplies a double value by another double value specified in converter parameter.
    /// </summary>
    public class MultiplicationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value, culture) * System.Convert.ToDouble(parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}