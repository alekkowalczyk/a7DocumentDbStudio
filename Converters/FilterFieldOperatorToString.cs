using a7DocumentDbStudio.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace a7DocumentDbStudio.Converters
{
    class FilterFieldOperatorToString : IValueConverter
    {

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FilterFieldOperator)
            {
                FilterFieldOperator ft = (FilterFieldOperator)value;
                switch(ft)
                {
                    case FilterFieldOperator.Contains:
                        return "Contains";
                    case FilterFieldOperator.Equal:
                        return "Equals";
                    case FilterFieldOperator.StartsWith:
                        return "StartsWith";
                    case FilterFieldOperator.EndsWith:
                        return "EndsWith";
                    case FilterFieldOperator.GreaterThan:
                        return "GreaterThan";
                    case FilterFieldOperator.In:
                        return "In";
                    case FilterFieldOperator.LessThan:
                        return "LessThan";
                    case FilterFieldOperator.Like:
                        return "Like";
                    case FilterFieldOperator.NotEqual:
                        return "NotEqual";
                    case FilterFieldOperator.IsNull:
                        return "IsNull";
                    case FilterFieldOperator.IsNotNull:
                        return "IsNotNull";
                    case FilterFieldOperator.Between:
                        return "Between";
                }
                return ft.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
