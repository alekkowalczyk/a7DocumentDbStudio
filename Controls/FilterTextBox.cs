using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace a7DocumentDbStudio.Controls
{
    class FilterTextBox : ComboBox, INotifyPropertyChanged
    {
        public static DependencyProperty FilterTypeProperty =
            DependencyProperty.Register("FilterType", typeof(FilterFieldOperator), typeof(FilterTextBox)
            , new PropertyMetadata(FilterFieldOperator.Equal, new PropertyChangedCallback(changed)));
        public FilterFieldOperator FilterType
        {
            get { return (FilterFieldOperator)GetValue(FilterTypeProperty); }
            set { SetValue(FilterTypeProperty, value); }
        }

        public static DependencyProperty AvailableFilterTypesProperty =
            DependencyProperty.Register("AvailableFilterTypes", typeof(List<FilterFieldOperator>), typeof(FilterTextBox));
        public List<FilterFieldOperator> AvailableFilterTypes
        {
            get { return (List<FilterFieldOperator>)GetValue(AvailableFilterTypesProperty); }
            set { SetValue(AvailableFilterTypesProperty, value); }
        }



        public PropertyType PropertyType
        {
            get { return (PropertyType)GetValue(PropertyTypeProperty); }
            set { SetValue(PropertyTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PropertyType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyTypeProperty =
            DependencyProperty.Register("PropertyType", typeof(PropertyType), typeof(FilterTextBox), new PropertyMetadata(PropertyType.String));




        private ToggleButton _toggleButton;

        public FilterTextBox(PropertyDefinitionModel prop, bool isInlineMode) : base()
        {
            if(isInlineMode)
                this.Template = ResourcesManager.Instance.GetControlTemplate("FilterTextBoxInlineTemplate");
            else
                this.Template = ResourcesManager.Instance.GetControlTemplate("FilterTextBoxTemplate");
            this.IsEditable = true;
            if (prop.Type == PropertyType.String)
            {
                FilterType = FilterFieldOperator.Equal;
                AvailableFilterTypes = new List<FilterFieldOperator>()
                        {
                            FilterFieldOperator.Contains,
                            FilterFieldOperator.StartsWith,
                            FilterFieldOperator.EndsWith,
                            FilterFieldOperator.Equal
                        };
            }
            else
            {
                FilterType = FilterFieldOperator.Equal;
                AvailableFilterTypes = new List<FilterFieldOperator>()
                        {
                            FilterFieldOperator.Equal,
                            FilterFieldOperator.GreaterThan,
                            FilterFieldOperator.LessThan
                        };
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            e.Handled = true;
            //base.OnSelectionChanged(e);
        }

        public event EventHandler FilterTypeChanged;
        public static readonly RoutedEvent FilterTypeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
   "FilterTypeChangedRouted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(FilterTextBox));

        // Provide CLR accessors for the event 
        public event RoutedEventHandler FilterTypeChangedRouted
        {
            add { AddHandler(FilterTypeChangedRoutedEvent, value); }
            remove { RemoveHandler(FilterTypeChangedRoutedEvent, value); }
        }

        static void changed(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FilterTextBox cb = o as FilterTextBox;
            cb.IsDropDownOpen = false;
            if (cb.FilterTypeChanged != null)
                cb.FilterTypeChanged(cb, new EventArgs());
            cb.RaiseEvent(new RoutedEventArgs(FilterTypeChangedRoutedEvent));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
