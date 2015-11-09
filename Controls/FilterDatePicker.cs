using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Utils;

namespace a7DocumentDbStudio.Controls
{
    class FilterDatePicker : ComboBox, INotifyPropertyChanged
    {
        public static DependencyProperty FilterTypeProperty = 
            DependencyProperty.Register("FilterType", typeof(FilterFieldOperator), typeof(FilterDatePicker)
            , new PropertyMetadata(FilterFieldOperator.Equal, new PropertyChangedCallback(changed)) );
        public FilterFieldOperator FilterType
        {
            get { return (FilterFieldOperator)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public static DependencyProperty AvailableFilterTypesProperty =
            DependencyProperty.Register("AvailableFilterTypes", typeof(List<FilterFieldOperator>), typeof(FilterDatePicker));
        public List<FilterFieldOperator> AvailableFilterTypes
        {
            get { return (List<FilterFieldOperator>)GetValue(AvailableFilterTypesProperty); }
            set { SetValue(AvailableFilterTypesProperty, value); }
        }

        public static readonly DependencyProperty HasTimeProperty =
            DependencyProperty.Register("HasTime", typeof (bool), typeof (FilterDatePicker), new PropertyMetadata(true));

        public bool HasTime
        {
            get { return (bool) GetValue(HasTimeProperty); }
            set { SetValue(HasTimeProperty, value); }
        }


        public bool TwoDatesSelectable
        {
            get { return (bool)GetValue(TwoDatesSelectableProperty); }
            set { SetValue(TwoDatesSelectableProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TwoDatesSelectable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TwoDatesSelectableProperty =
            DependencyProperty.Register("TwoDatesSelectable", typeof(bool), typeof(FilterDatePicker), new PropertyMetadata(false));

        public PropertyType PropertyType
        {
            get { return (PropertyType)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(PropertyType), typeof(FilterDatePicker), new PropertyMetadata(PropertyType.DateTime));

        public FilterDatePicker() : base()
        {
            this.Template = ResourcesManager.Instance.GetControlTemplate("a7FilterDatePickerTemplate"); 
            FilterType = FilterFieldOperator.Contains;
            this.IsEditable = true;
            AvailableFilterTypes = new List<FilterFieldOperator>()
            {
                FilterFieldOperator.Between,
                FilterFieldOperator.GreaterThan,
                FilterFieldOperator.LessThan,
                FilterFieldOperator.Equal
            };
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.Handled = true;
            //base.OnSelectionChanged(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var dp = GetTemplateChild("innerDatePicker") as a7DateTimePicker;
            if (dp != null)
            {
                dp.SelectedDateChanged += dp_SelectedDateChanged;
            }
        }

        void dp_SelectedDateChanged(object sender, EventArgs e)
        {
            if (FilterDateChanged != null)
                FilterDateChanged(this, new EventArgs());
        }


        public event EventHandler FilterTypeChanged;
        public event EventHandler FilterDateChanged;

        static void changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FilterDatePicker cb = o as FilterDatePicker;
            cb.IsDropDownOpen = false;
            if (cb.FilterType == FilterFieldOperator.Between)
                cb.TwoDatesSelectable = true;
            else
            {
                cb.TwoDatesSelectable = false;
            }
            if (cb.FilterTypeChanged != null)
                cb.FilterTypeChanged(cb, new EventArgs());
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
