using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace a7DocumentDbStudio.Converters
{
    // combine the width of the TreeView control and the number of parent items to compute available width
    public class TreeViewItemWidthSumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double totalWidth = (double)values[0];
            double parentCount = (int)values[1];
            return totalWidth - parentCount * 20.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // count the number of TreeViewItems before reaching ScrollContentPresenter
    public class TreeViewItemWidthParentCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int parentCount = 1;
            DependencyObject o = VisualTreeHelper.GetParent(value as DependencyObject);
            while (o != null && o.GetType().FullName != "System.Windows.Controls.ScrollContentPresenter")
            {
                if (o.GetType().FullName == "System.Windows.Controls.TreeViewItem")
                    parentCount += 1;
                o = VisualTreeHelper.GetParent(o);
            }
            return parentCount;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
