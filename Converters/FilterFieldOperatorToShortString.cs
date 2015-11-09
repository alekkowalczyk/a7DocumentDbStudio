using a7DocumentDbStudio.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace a7DocumentDbStudio.Converters
{
    class FilterFieldOperatorToShortString : IValueConverter
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
                        return "*a*";
                    case FilterFieldOperator.Equal:
                        return "=a";
                    case FilterFieldOperator.StartsWith:
                        return "a*";
                    case FilterFieldOperator.EndsWith:
                        return "*a";
                    case FilterFieldOperator.GreaterThan:
                        return ">1";
                    case FilterFieldOperator.In:
                        return "(a,b..)";
                    case FilterFieldOperator.LessThan:
                        return "<1";
                    case FilterFieldOperator.Like:
                        return "a%c";
                    case FilterFieldOperator.NotEqual:
                        return "=/=a";
                    case FilterFieldOperator.IsNull:
                        return "a is 0";
                    case FilterFieldOperator.IsNotNull:
                        return "a =/= 0";
                    case FilterFieldOperator.Between:
                        return "<i<";
                }
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
