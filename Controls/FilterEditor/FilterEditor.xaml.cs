using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using a7DocumentDbStudio.Filter;
using a7DocumentDbStudio.ViewModel;
using a7DocumentDbStudio.Model;

namespace a7DocumentDbStudio.Controls.FilterEditor
{
    /// <summary>
    /// Interaction logic for a7FilterEditor.xaml
    /// </summary>
    public partial class FilterEditor : UserControl, INotifyPropertyChanged
    {
        public static int BackgroundIndexStep = 18;

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FilterEditor), new UIPropertyMetadata(false));      

        public FilterExpressionData FilterExpr
        {
            get { return (FilterExpressionData)GetValue(FilterExprProperty); }
            set { SetValue(FilterExprProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterExpr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterExprProperty =
            DependencyProperty.Register("FilterExpr", typeof(FilterExpressionData), typeof(FilterEditor), new UIPropertyMetadata(null));       

        public Action<FilterExpressionData> UpdateFilterFunction
        {
            get { return (Action<FilterExpressionData>)GetValue(UpdateFilterFunctionProperty); }
            set { SetValue(UpdateFilterFunctionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for RefreshFunction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateFilterFunctionProperty =
            DependencyProperty.Register("UpdateFilterFunction", typeof(Action<FilterExpressionData>), typeof(FilterEditor));



        public bool IsPopupMode
        {
            get { return (bool)GetValue(IsPopupModeProperty); }
            set { SetValue(IsPopupModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPopupMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPopupModeProperty =
            DependencyProperty.Register("IsPopupMode", typeof(bool), typeof(FilterEditor), new PropertyMetadata(false));



        private IEnumerable<PropertyDefinitionModel> _elements;
        public IEnumerable<PropertyDefinitionModel> Elements { get { return _elements; } set { _elements = value; OnPropertyChanged("Elements"); } }
        
        private CollectionVM _collection;
        private int _backgroundIndex;
        private FilterGroupEditor _rootGroup;
        public List<Popup> EntityFieldsPopups { get; private set; }

        public FilterEditor()
        {
            EntityFieldsPopups = new List<Popup>();
            InitializeComponent();
        }

        public void SetCollection(CollectionVM collection)
        {
            if (collection != null)
            {
                _collection = collection;
                Elements = collection.AvailableProperties;
            }
            Reset();
        }

        public void SetFilter(CollectionVM collection, FilterExpressionData filter)
        {
            _collection = collection;
            Elements = collection.AvailableProperties;
            if (filter != null)
            {
                var fge = new FilterGroupEditor(collection, false, this.IsReadOnly, this);
                fge.SetFilter(filter);
                this.FilterExpr = filter;
                SetRootGroup(fge);
                if (filter != null)
                {
                    gStartPanel.Visibility = Visibility.Collapsed;
                    MyBorder.Visibility = System.Windows.Visibility.Visible;
                    if (!IsReadOnly)
                        spButtons.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                this.setProperty(PropertyDefinitionModel.GetEmpty());
            }
        }



        public void Reset(bool withRefresh = false)
        {
            _backgroundIndex = 0;
            mainGrid.Children.Clear();
            this.FilterExpr = null;
            if (!this.IsReadOnly)
            {
                if (IsPopupMode)
                {
                    gStartPanel.Visibility = Visibility.Visible;
                    MyBorder.Visibility = Visibility.Collapsed;
                    spButtons.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.setProperty(PropertyDefinitionModel.GetEmpty());
                }
            }
         //   spButtons.Visibility = Visibility.Collapsed;
            if (withRefresh && UpdateFilterFunction!=null)
                UpdateFilterFunction(null);
        }

        private void fgeOnAddedFirstElement(object sender, EventArgs eventArgs)
        {
            if (!this.IsReadOnly)
            {
                _backgroundIndex += BackgroundIndexStep;
                var newFge = new FilterGroupEditor(_collection, true, this.IsReadOnly, this);
                mainGrid.Children.Remove(_rootGroup);
                newFge.AddGroupSubFilter(_rootGroup);
                newFge.SetBackground(_rootGroup.MyBackgroundIndex + BackgroundIndexStep);
                SetRootGroup(newFge);
            }
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.UpdateFilterFunction != null)
                UpdateFilterFunction(FilterExpr);
        }

        //private void lbFields_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //     var selectedField = lbFields.SelectedItem as FilterElementDefinition;
        //     if (selectedField != null)
        //     {
        //         gStartPanel.Visibility = Visibility.Collapsed;
        //         MyBorder.Visibility = System.Windows.Visibility.Visible;
        //         spButtons.Visibility = System.Windows.Visibility.Visible;
        //         var fge = new FilterGroupEditor(_collection, false, this.IsReadOnly, this);
        //         var fae = new FilterElementEditor(selectedField) { Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly };
        //         fae.EditorContext = this;
        //         fge.SetAtomFilter(fae);
        //         this.FilterExpr = fge.Filter;
        //         SetRootGroup(fge);
        //     }
        //}

        public void SetRootGroup(FilterGroupEditor fge)
        {
            if (_rootGroup != null)
            {
                var flt = _rootGroup.Filter; //setvalue, clear binding
                BindingOperations.ClearBinding(_rootGroup, FilterGroupEditor.FilterProperty);
                _rootGroup.Filter = flt;
                _rootGroup.AddedFirstElement -= fgeOnAddedFirstElement;
                _rootGroup.Parent = fge;
                mainGrid.Children.Remove(_rootGroup);
            }
            _rootGroup = fge;
            this.SetBinding(FilterEditor.FilterExprProperty, new Binding("Filter") { Source = fge, Mode = BindingMode.TwoWay });
      //      fge.Background = Brushes.White;
            fge.AddedFirstElement += fgeOnAddedFirstElement;
            fge.Parent = null;
            mainGrid.Children.Add(fge);
            if(fge.AtomFilter!=null)
                fge.SetAsRoot();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsReadOnlyProperty)
            {
                if (_rootGroup != null)
                    _rootGroup.IsReadOnly = ((bool)e.NewValue);
                if ((bool)e.NewValue)
                {
                    gStartPanel.Visibility = Visibility.Collapsed;
                    MyBorder.Visibility = System.Windows.Visibility.Visible;
                    spButtons.Visibility = Visibility.Visible;
                }
                else
                {
                    if (this._rootGroup != null)
                    {
                        gStartPanel.Visibility = Visibility.Collapsed;
                        MyBorder.Visibility = System.Windows.Visibility.Visible;
                        spButtons.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        gStartPanel.Visibility = Visibility.Visible;
                        MyBorder.Visibility = System.Windows.Visibility.Collapsed;
                        spButtons.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
            else if(e.Property == IsPopupModeProperty)
            {
                this.Reset();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }


        private void propertySelector_SelectClicked(object sender, EventArgs e)
        {
            var selectedProperty = propertySelector.SelectedProperty;
            if (selectedProperty != null)
            {
                setProperty(selectedProperty);
            }
        }

        private void setProperty(PropertyDefinitionModel selectedProperty)
        {
            gStartPanel.Visibility = Visibility.Collapsed;
            MyBorder.Visibility = System.Windows.Visibility.Visible;
            spButtons.Visibility = System.Windows.Visibility.Visible;
            var fge = new FilterGroupEditor(_collection, false, this.IsReadOnly, this);
            var fae = new FilterElementEditor(_collection, selectedProperty, this.IsPopupMode) { Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly};
            fae.EditorContext = this;
            fge.SetAtomFilter(fae);
            this.FilterExpr = fge.Filter;
            SetRootGroup(fge);
        }
    }
}
