using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace a7DocumentDbStudio.Converters
{
    public class AddDoubleConverter : ValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) + System.Convert.ToDouble(parameter);
        }
    }
}
