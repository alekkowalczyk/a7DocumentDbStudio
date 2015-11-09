using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace a7DocumentDbStudio.Converters.Json
{
    class JPropertyToChildren : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is JProperty)
            {
                var jp = value as JProperty;
                if (jp.Value.Type == JTokenType.Object || jp.Value.Type == JTokenType.Array)
                    return jp.Value.Children();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
