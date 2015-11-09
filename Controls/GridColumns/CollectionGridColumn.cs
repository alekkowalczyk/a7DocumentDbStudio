using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace a7DocumentDbStudio.Controls.GridColumns
{
    class CollectionGridColumn: DataGridTemplateColumn
    {
        public Binding TextBinding { get; private set; }
        #region a7IDataGridColumn Members


        private FrameworkElement _columnFilterControl;
        public FrameworkElement ColumnFilterControl
        {
            get
            {
                getColumnFilterControl();
                return _columnFilterControl;
            }
        }

        public string FieldName
        {
            get;
            private set;
        }

        #endregion

        private CollectionGrid _dg;
        private string _fieldName;
        public PropertyDefinitionModel PropertyDefinition { get; private set; }

        public CollectionGridColumn(CollectionGrid dg, PropertyDefinitionModel prop)
        {
            _dg = dg;
            PropertyDefinition = prop;
            _fieldName = PropertyDefinition.Path;
            this.FieldName = PropertyDefinition.Path;
            TextBinding = new Binding($"[{FieldName}]")
            {
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay,
                ValidatesOnDataErrors = true,
                NotifyOnValidationError = true
            }; 
            FrameworkElementFactory template = GetTemplate(TextBinding,  this);
            this.CellTemplate = new DataTemplate() { VisualTree = template };
            this.Header = PropertyDefinition.Path;
        }

        public static FrameworkElementFactory GetTemplate(Binding textBinding, DataGridColumn column)
        {
            FrameworkElementFactory template;
            template = new FrameworkElementFactory(typeof(TextBox));
            template.SetBinding(TextBox.TextProperty, textBinding);
            template.SetValue(TextBox.IsReadOnlyCaretVisibleProperty, false);
            template.SetValue(TextBox.IsReadOnlyProperty, true);
            template.SetValue(TextBox.BackgroundProperty, Brushes.Transparent);
            template.SetValue(TextBox.PaddingProperty, new Thickness(1));
            template.SetValue(TextBox.MarginProperty, new Thickness(0));
            template.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            template.SetValue(TextBox.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            template.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            template.SetValue(TextBox.TemplateProperty, ResourcesManager.Instance.GetControlTemplate("TextBoxInColumnTemplate"));
            return template;
        }

        private void getColumnFilterControl()
        {
            if (true)//PropertyDefinition.Type != Enums.PropertyType.DateTime)//DateTime isn't so straightforward in DocumentDb
                getTextColumnFilterControl();
            else
                getDateTimeColumnFilterControl();
        }

        #region text filter
        private void getTextColumnFilterControl()
        {
            FilterTextBox ftb = new FilterTextBox(this.PropertyDefinition, false);

            ftb.KeyUp += new System.Windows.Input.KeyEventHandler(ftb_KeyUp);
            ftb.KeyDown += new System.Windows.Input.KeyEventHandler(ftb_KeyDown);
            ftb.Padding = new Thickness(0.0);
            ftb.Margin = new Thickness(0.0);
            ftb.HorizontalAlignment = HorizontalAlignment.Stretch;
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");

            if (ftb.IsEnabled)
            {
                Binding tbBinding = new Binding("Collection.Filter.FieldsFilter[" + this.FieldName + "]")
                {
                    Source = _dg,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                ftb.SetBinding(FilterTextBox.TextProperty, tbBinding);
            }
            ftb.Height = 30;
            ftb.FontSize = 12;

            Binding cbBinding = new Binding("Collection.Filter.FieldsFilter.FieldFilters[" + this.FieldName + "].Operator")
            {
                Source = _dg,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterTextBox.FilterTypeProperty, cbBinding);
            Binding typeBinding = new Binding("Collection.Filter.FieldsFilter.FieldFilters[" + this.FieldName + "].PropertyType")
            {
                Source = _dg,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterTextBox.PropertyTypeProperty, typeBinding);
            ftb.FilterTypeChanged += new EventHandler(ftb_FilterTypeChanged);
            ftb.PropertyChanged += Ftb_PropertyChanged;
            _columnFilterControl = ftb;

        }

        private void Ftb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == FilterTextBox.FilterTypeProperty.Name)
            {

            }
        }

        async void ftb_FilterTypeChanged(object sender, EventArgs e)
        {
            FilterTextBox ftb = sender as FilterTextBox;
            if (ftb != null)
            {
                if (ftb.Text.IsEmpty())
                    return;
            }
            await _dg.Refresh();
        }

        async void ftb_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                await _dg.Refresh();
                e.Handled = true;
            }
        }

        void ftb_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
        #endregion

        #region datetime filter

        private void getDateTimeColumnFilterControl()
        {
            FilterDatePicker ftb = new FilterDatePicker();
            ftb.HasTime = false;

            ftb.FilterDateChanged += new EventHandler(ftb_FilterDateChanged);
            ftb.Padding = new Thickness(0.0);
            ftb.Margin = new Thickness(0.0);
            ftb.HorizontalAlignment = HorizontalAlignment.Stretch;
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            if (ftb.IsEnabled)
            {

                Binding tbBinding = new Binding("Collection.Filter.FieldsFilter[" + this.FieldName + "]")
                {
                    Source = _dg,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                //ftb.SetBinding(a7FilterDatePicker.FilterDateProperty, tbBinding);
                ftb.SetBinding(FilterDatePicker.TextProperty, tbBinding);
                if (_dg.Collection.Filter.FieldsFilter.FieldFilters[this.FieldName].Operator == FilterFieldOperator.Contains)
                    _dg.Collection.Filter.FieldsFilter.FieldFilters[this.FieldName].Operator = FilterFieldOperator.Between;
            }
            ftb.Height = 30;
            ftb.FontSize = 12;
            ftb.Background = Brushes.White;
            Binding cbBinding = new Binding("Collection.Filter.FieldsFilter.FieldFilters[" + this.FieldName + "].Operator")
            {
                Source = _dg,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterDatePicker.FilterTypeProperty, cbBinding);
            Binding typeBinding = new Binding("Collection.Filter.FieldsFilter.FieldFilters[" + this.FieldName + "].PropertyType")
            {
                Source = _dg,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterDatePicker.PropertyTypeProperty, typeBinding);
            ftb.FilterTypeChanged += new EventHandler(ftb_DateFilterTypeChanged);
            _columnFilterControl = ftb;
        }

        async void ftb_FilterDateChanged(object sender, EventArgs e)
        {
            await _dg.Refresh();
        }

        async void ftb_DateFilterTypeChanged(object sender, EventArgs e)
        {
            FilterDatePicker fdp = sender as FilterDatePicker;
            if (fdp != null)
            {
                if (fdp.Text.IsEmpty())
                    return;
            }
            await _dg.Refresh();
        }
        #endregion
    }
}
