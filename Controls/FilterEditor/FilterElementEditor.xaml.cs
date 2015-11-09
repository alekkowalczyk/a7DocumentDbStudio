using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Filter;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace a7DocumentDbStudio.Controls.FilterEditor
{
    /// <summary>
    /// Interaction logic for a7FilterAtomEditor.xaml
    /// </summary>
    public partial class FilterElementEditor : UserControl
    {
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FilterElementEditor), new UIPropertyMetadata(false));


        public static readonly DependencyProperty FilterProperty =
            DependencyProperty.Register("Filter", typeof(FilterExpressionData), typeof(FilterElementEditor), new PropertyMetadata(default(FltAtomExprData)));

        public FilterExpressionData Filter
        {
            get { return (FilterExpressionData)GetValue(FilterProperty); }
            set { SetValue(FilterProperty, value); }
        }



        public bool IsPopupMode
        {
            get { return (bool)GetValue(IsPopupModeProperty); }
            set { SetValue(IsPopupModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPopupMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPopupModeProperty =
            DependencyProperty.Register("IsPopupMode", typeof(bool), typeof(FilterElementEditor), new PropertyMetadata(false));


        public FilterEditor EditorContext { get; set; }
        public bool IsWithcollectionIdFilter { get; private set; }
        private FrameworkElement _frameworkElement;

        public FilterElementEditor(CollectionVM collection, PropertyDefinitionModel elementDef, bool isPopupMode)
        {
            this.IsPopupMode = isPopupMode;
            InitializeComponent();
            this.propSelector.SelectedPropertyChanged += PropSelector_SelectedPropertyChanged;
            IsWithcollectionIdFilter = false;
            var field = elementDef;
            this.propSelector.Properties = collection.AvailableProperties;
            this.propSelector.SelectedProperty = elementDef;
            Filter = new FltAtomExprData()
            {
                Operator = FilterFieldOperator.Equal,
                PropertyType = field.Type,
                Field = field.Path
            };
            setField(field);
        }

        public FilterElementEditor(CollectionVM collection, FltAtomExprData filter, bool isPopupMode)
        {
            this.IsPopupMode = isPopupMode;
            InitializeComponent();
            this.propSelector.SelectedPropertyChanged += PropSelector_SelectedPropertyChanged;
            var field = collection.AvailableProperties.FirstOrDefault(ap => ap.Path == filter.Field);
            Filter = filter;
            this.propSelector.Properties = collection.AvailableProperties;
            this.propSelector.SelectedProperty = new PropertyDefinitionModel
            {
                Path = filter.Field,
                Name = filter.Field,
                Type = filter.PropertyType
            };
            IsWithcollectionIdFilter = false;
            setField(field);
        }

        private void PropSelector_SelectedPropertyChanged(object sender, EventArgs e)
        {
            if(this.Filter!=null && this.Filter is FltAtomExprData && this.propSelector.SelectedProperty != null)
            {
                var fa = this.Filter as FltAtomExprData;
                fa.Field = this.propSelector.SelectedProperty.Path;
                fa.PropertyType = this.propSelector.SelectedProperty.Type;
            }
        }

        public void FocusControl()
        {
            if (this.filterContainer.Content != null)
            {
                var _popupTimer = new DispatcherTimer(DispatcherPriority.Normal);
                var ctrl = this.filterContainer.Content as FrameworkElement;
                _popupTimer.Interval = TimeSpan.FromMilliseconds(100);
                _popupTimer.Tick += (obj, e) =>
                {
                    ctrl.Dispatcher.Invoke(new Action(() => ctrl.Focus()));
                    _popupTimer.Stop();
                };
                _popupTimer.Start();
            }
        }

        private void setField(PropertyDefinitionModel field)
        {
            if (this.IsPopupMode)
            {
                this.lCaption.Content = field.Path;
                this.firstColumn.Width = new GridLength(0);
            }
            else
            {
                this.lCaption.Content = "Field value";
                this.firstColumn.Width = new GridLength(50, GridUnitType.Star);
            }
            FrameworkElement fe;
            if (field.Type == PropertyType.Bool)
                fe = getBoolFilter(field);
            else if (field.Type == PropertyType.DateTime)
                fe = getDatePicker(field);
            else
                fe = getTextBox(field);
            fe.Margin = new Thickness(0);
            this.filterContainer.Content = fe;
            _frameworkElement = fe;
        }

        private FilterTextBox getTextBox(PropertyDefinitionModel field)
        {
            var ftb = new FilterTextBox(field, !this.IsPopupMode) { Padding = new Thickness(0), Width = 120 };
            if (this.IsPopupMode)
                ftb.Height = 26;
            else
                ftb.Height = 21;
            ftb.SetBinding(FilterTextBox.TextProperty, getFilterValueBinding());
            var faExpr = this.Filter as FltAtomExprData;
            if (faExpr != null && faExpr.Value.IsEmpty())
            {
                if (field.Type == PropertyType.String)
                    (this.Filter as FltAtomExprData).Operator = FilterFieldOperator.Equal;
                else
                    (this.Filter as FltAtomExprData).Operator = FilterFieldOperator.Equal;
            }
            var operatorBinding = new Binding("Operator")
            {
                Source = this.Filter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterTextBox.FilterTypeProperty, operatorBinding);
            ftb.SetBinding(FilterTextBox.PropertyTypeProperty, new Binding("PropertyType")
            {
                Source = this.Filter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            });
            ftb.SetBinding(FilterTextBox.IsEnabledProperty, this.getIsEnabledBinding());
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            ftb.KeyUp += (s, e) =>
            {
                if (e.Key == Key.Enter)
                    activateFilter();
            };
            return ftb;
        }


        private FilterDatePicker getDatePicker(PropertyDefinitionModel field)
        {
            FilterDatePicker ftb = new FilterDatePicker();
            ftb.HasTime = false;
            ftb.Padding = new Thickness(0.0);
            ftb.Margin = new Thickness(0.0);
            ftb.Width = 120;
            ftb.HorizontalAlignment = HorizontalAlignment.Stretch;
            ftb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            var fa = this.Filter as FltAtomExprData;
            fa.Operator = FilterFieldOperator.Between;
            //if(fa!=null)
            //    fa.Operator = FilterFieldOperator.Equal;
            ftb.SetBinding(FilterDatePicker.TextProperty, getFilterValueBinding());

            ftb.Height = 18;
            ftb.FontSize = 12;
            var operatorBinding = new Binding("Operator")
            {
                Source = this.Filter,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
            ftb.SetBinding(FilterDatePicker.FilterTypeProperty, operatorBinding);
            ftb.SetBinding(FilterDatePicker.IsEnabledProperty, getIsEnabledBinding());
            ftb.KeyUp += (s, e) =>
                {
                    if (e.Key == Key.Enter)
                        activateFilter();
                };
            return ftb;
        }

        class comboItem
        {
            public string Value { get; private set; }
            public string Name { get; private set; }
            public comboItem(string value, string name)
            {
                Value = value;
                Name = name;
            }
        }

        private a7ComboBox getBoolFilter(PropertyDefinitionModel field)
        {
            a7ComboBox cb = new a7ComboBox();
            cb.SelectedValuePath = "Value";
            cb.DisplayMemberPath = "Name";
            cb.Width = 120;
            var items = new ObservableCollection<comboItem>();
            items.Add(new comboItem("1", "True"));
            items.Add(new comboItem("0", "False"));
            cb.ItemsSource = items;
            cb.Background = Brushes.White;
            cb.Template = ResourcesManager.Instance.GetControlTemplate("CustomComboBox");
            cb.Padding = new Thickness(0.0);
            cb.Margin = new Thickness(0.0);
            cb.HorizontalAlignment = HorizontalAlignment.Stretch;
            cb.BorderBrush = ResourcesManager.Instance.GetBrush("IsReadOnlyBorderBrush");
            cb.SetBinding(a7ComboBox.SelectedValueProperty, getFilterValueBinding());
            cb.SetBinding(a7ComboBox.IsEnabledProperty, getIsEnabledBinding());
            cb.Height = 18;
            cb.FontSize = 12;
            cb.SelectionChanged += (s, e) => activateFilter();
            return cb;
        }

       
        private Binding getFilterValueBinding()
        {
            return new Binding("Value")
                {
                    Source = this.Filter, 
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, 
                    Mode = BindingMode.TwoWay 
                };
        }

        private Binding getIsEnabledBinding()
        {
            return new Binding("IsEnabled")
            {
                Source = this,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.TwoWay
            };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsReadOnlyProperty)
            {
                if (_frameworkElement != null)
                {
                    if (_frameworkElement.PropertyExists("IsReadOnly"))
                        _frameworkElement.SetPropertyValue("IsReadOnly", e.NewValue);
                    else
                        _frameworkElement.IsEnabled = !(bool)(e.NewValue);
                }
            }
        }

        private void activateFilter()
        {
            if (EditorContext != null && EditorContext.UpdateFilterFunction != null && !EditorContext.IsReadOnly)
                EditorContext.UpdateFilterFunction(EditorContext.FilterExpr);
        }
    }
}
