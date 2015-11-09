using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using a7DocumentDbStudio.Filter;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel;
using a7DocumentDbStudio.Model;

namespace a7DocumentDbStudio.Controls.FilterEditor
{
    /// <summary>
    /// Interaction logic for a7FilterGroupEditor.xaml
    /// </summary>
    public partial class FilterGroupEditor : UserControl, INotifyPropertyChanged
    {
        public event EventHandler AddedFirstElement;

        public static readonly DependencyProperty JoinTypeProperty =
            DependencyProperty.Register("JoinType", typeof(eAndOrJoin?), typeof(FilterGroupEditor), new PropertyMetadata(default(eAndOrJoin?)));
        public eAndOrJoin? JoinType
        {
            get { return (eAndOrJoin?)GetValue(JoinTypeProperty); }
            set { SetValue(JoinTypeProperty, value); }
        }

        public static readonly DependencyProperty FilterProperty =
    DependencyProperty.Register("Filter", typeof(FilterExpressionData), typeof(FilterGroupEditor), new PropertyMetadata(default(FltAtomExprData)));
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
            DependencyProperty.Register("IsPopupMode", typeof(bool), typeof(FilterGroupEditor), new PropertyMetadata(false));




        private Orientation _orientation;
        public Orientation Orientation { get { return _orientation; } set { _orientation = value; OnPropertyChanged("Orientation"); OnPropertyChanged("OrientationNegate"); } }
        public Orientation OrientationNegate { get { return (_orientation== Orientation.Vertical)?Orientation.Horizontal : Orientation.Vertical; } }

        private IEnumerable<PropertyDefinitionModel> _elements;
        public IEnumerable<PropertyDefinitionModel> Elements { get { return _elements; } set { _elements = value; OnPropertyChanged("Elements"); } }

        private FilterGroupEditor _parent;

        internal FilterGroupEditor Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;
                UnSetAsRoot();

            }
        }

        internal FilterEditor EditorContext { get; private set; }
        internal List<FilterGroupEditor> SubGroups { get; private set; }
        internal Label JoinLabelOnParent { get; set; }
        internal FilterElementEditor AtomFilter { get; private set; }

        private bool _isReadOnly;
        public bool IsReadOnly {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                if (AtomFilter != null)
                    AtomFilter.IsReadOnly = value;
                else
                {
                    foreach (var gr in this.SubGroups)
                    {
                        gr.IsReadOnly = value;
                    }
                };
                if (value)
                {
                    contextMenu.Visibility = Visibility.Collapsed;
                    grButtonsAndPopup.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    contextMenu.Visibility = Visibility.Visible;
                    grButtonsAndPopup.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private CollectionVM _collection;
        private bool _vertical;
        private Brush _currentBrush;
        private Brush _currentMouseOverBrush;

        public FilterGroupEditor(CollectionVM collection, bool vertical, bool isReadOnly, FilterEditor editorContext, FilterExpressionData filter)
            : this(collection, vertical, isReadOnly, editorContext)
        {
            SetFilter(filter);
        }

        public FilterGroupEditor(CollectionVM collection, bool vertical, bool isReadOnly, FilterEditor editorContext)
        {
            InitializeComponent();
            this.IsPopupMode = editorContext.IsPopupMode;
            EditorContext = editorContext;
            _collection = collection;
            if (collection != null)
                Elements = collection.AvailableProperties;
            else
                Elements = new List<PropertyDefinitionModel>();
            SubGroups = new List<FilterGroupEditor>();
            _vertical = vertical;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            if (!_vertical)
                Orientation = Orientation.Horizontal;
            else
                Orientation = Orientation.Vertical;
            IsReadOnly = false;
            popupFieldSelect.Opened += (sender, args) =>
                                           {
                                               if (EditorContext != null)
                                                   EditorContext.EntityFieldsPopups.Add(popupFieldSelect);
                                           };
            this.IsReadOnly = isReadOnly;
        }

        public void SetFilter(FilterExpressionData filter)
        {
            Reset();
            this.Filter = filter;
            if (filter != null)
            {
                if (filter is FltAtomExprData)
                {
                    if (filter.HasActiveFilter)
                    {
                        this.Filter = filter;
                        if(_collection != null && _collection.AvailableProperties.Any(ap => ap.Path ==(filter as FltAtomExprData).Field))
                            this.SetAtomFilter(new FilterElementEditor(_collection, filter as FltAtomExprData, this.IsPopupMode) { 
                                IsReadOnly = this.IsReadOnly,
                                EditorContext = this.EditorContext
                            });
                    }
                }
                else if (filter is FltGroupExprData)
                {
                    var fge = filter as FltGroupExprData;
                    this.Filter = new FltGroupExprData(fge.AndOr);
                    this.JoinType = fge.AndOr;
                    foreach (var f in fge.FilterExpressions)
                    {
                        if (f.HasActiveFilter)
                        {
                            var subGroup = new FilterGroupEditor(_collection, !_vertical, this.IsReadOnly, EditorContext, f);
                            this.AddGroupSubFilter(subGroup);
                        }
                    }
                }
                else if (filter is FltFlatGroupExprData)
                {
                    var fge = filter as FltFlatGroupExprData;
                    this.Filter = new FltFlatGroupExprData(fge.AndOr);
                    this.JoinType = fge.AndOr;
                    foreach (var f in fge.FieldFilters.Values)
                    {
                        if (f.HasActiveFilter)
                        {
                            var subGroup = new FilterGroupEditor(_collection, !_vertical, this.IsReadOnly, EditorContext, f);
                            this.AddGroupSubFilter(subGroup);
                        }
                    }
                }
                this.Negate(filter.Negate);
            }

            if (this.AtomFilter == null && this.SubGroups.Count == 0)
                Reset();
        }

        public void Reset()
        {
            this.Filter = null;
            this.ccAtom.Content = null;
            this.AtomFilter = null;
            this.spSubGroups.Children.Clear();
            this.SubGroups.Clear();
        }

        public void RemoveSubGroup(FilterGroupEditor fe)
        {
            var ix = this.spSubGroups.Children.IndexOf(fe);
            if (ix == 0 && this.SubGroups.Count>1) //remove the join label from second element if first is removed
            {
                this.spSubGroups.Children.Remove(this.SubGroups[1].JoinLabelOnParent);
                this.SubGroups[1].JoinLabelOnParent = null;
            }
            this.spSubGroups.Children.Remove(fe);
            this.spSubGroups.Children.Remove(fe.JoinLabelOnParent);
            this.SubGroups.Remove(fe);
            var fgeExpr = this.Filter as FltGroupExprData;
            if (fgeExpr != null)
                fgeExpr.FilterExpressions.Remove(fe.Filter);
            if (this.SubGroups.Count == 1 )
            {
                if (Parent != null)
                {
                    var or = Parent.Orientation;
                    this.SubGroups[0].Orientation = (or == Orientation.Horizontal)
                                                        ? Orientation.Vertical
                                                        : Orientation.Horizontal;
                    this.SubGroups[0]._vertical = !Parent._vertical;
                    this.spSubGroups.Children.Remove(this.SubGroups[0]);
                    Parent.addGroupSubFilter(this.SubGroups[0], true);
                    Parent.RemoveSubGroup(this);
                }
                else
                {
                    if (this.SubGroups[0].SubGroups.Count < 2)
                    {
                        var or = this.SubGroups[0].Orientation;
                        this.SubGroups[0].Orientation = Orientation.Horizontal;
                        this.SubGroups[0]._vertical = false;
                        this.spSubGroups.Children.Remove(this.SubGroups[0]);
                        EditorContext.SetRootGroup(this.SubGroups[0]);
                    }
                    bAdd.Visibility = Visibility.Collapsed;
                    bAnd.Visibility = Visibility.Visible;
                    bOr.Visibility = Visibility.Visible;
                }
            }
            else if (this.SubGroups.Count == 0)
            {
                if (Parent != null)
                {
                    Parent.RemoveSubGroup(this);
                }
                else
                {
                    EditorContext.Reset();
                }
            }
        }

        public void SetAtomFilter(FilterElementEditor fa)
        {
            this.Background = Brushes.White;
            _currentBrush = Brushes.White;
            this.border.BorderBrush = ResourcesManager.Instance.GetBrush("ShadowBorderBrush");
            if (this.Parent != null)
                this.Parent.SetBackground(FilterEditor.BackgroundIndexStep);
            bAdd.Visibility = Visibility.Collapsed;
            bAnd.Visibility = Visibility.Visible;
            bOr.Visibility = Visibility.Visible;
            miChangeToAnd.Visibility = Visibility.Collapsed;
            miChangeToOr.Visibility = Visibility.Collapsed;
            this.Filter = fa.Filter;
            AtomFilter = fa;
            ccAtom.Content = fa;
            fa.FocusControl();
        }

        public void SetAsRoot()
        {
            this.Height = 80;
            this.spAndOr.VerticalAlignment = VerticalAlignment.Top;
        }

        public void UnSetAsRoot()
        {
            this.Height = Double.NaN;
            this.spAndOr.VerticalAlignment = VerticalAlignment.Center;
        }

        public void AddGroupSubFilter(FilterGroupEditor fge)
        {
            addGroupSubFilter(fge, false);
        }

        private void addGroupSubFilter(FilterGroupEditor fge, bool fromRemove)
        {
            fge.Parent = this;
            FltGroupExprData fgeExpr = null;
            if (this.Filter is FltGroupExprData)
                fgeExpr = this.Filter as FltGroupExprData;
            else
            {
                if (JoinType.HasValue)
                {
                    fgeExpr = new FltGroupExprData(JoinType.Value);
                }
                else
                {
                    fgeExpr = new FltGroupExprData();
                }
                if (Parent != null && Parent.Filter != null)
                {
                    var parentGroup = Parent.Filter as FltGroupExprData;
                    parentGroup.FilterExpressions.Remove(this.Filter);
                    parentGroup.FilterExpressions.Add(fgeExpr);
                }
                this.Filter = fgeExpr;
            }
            fgeExpr.FilterExpressions.Add(fge.Filter);              

            

            if (this.SubGroups.Count > 0 || this.AtomFilter != null )
            {
                bAdd.Visibility = Visibility.Visible;
                bAnd.Visibility = Visibility.Collapsed;
                bOr.Visibility = Visibility.Collapsed;
            }
            else 
            {
                SetBackground(fge.MyBackgroundIndex + FilterEditor.BackgroundIndexStep);
                bAdd.Visibility = Visibility.Collapsed;
                bAnd.Visibility = Visibility.Visible;
                bOr.Visibility = Visibility.Visible;
            }

            if (JoinType.HasValue)
            {
                if (JoinType.Value == eAndOrJoin.And)
                {
                    miChangeToOr.Visibility = System.Windows.Visibility.Visible;
                    miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
                }
                else
                {
                    miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
                    miChangeToAnd.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
                miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (!fromRemove && ( AddedFirstElement != null && this.Parent == null))
            {
                this.AddedFirstElement(this, null);
            }

            if (AtomFilter!=null) //replacing existing atomfilter with groupfilter containing the atomfilter
            {
                var newFge = new FilterGroupEditor(_collection, !_vertical, this.IsReadOnly, EditorContext, AtomFilter.Filter);
                fgeExpr.FilterExpressions.Add(AtomFilter.Filter);
                newFge.Parent = this;
                ccAtom.Content = null;
                newFge.SetAtomFilter(AtomFilter);
                AtomFilter = null;
                this.Negate(false);
                spSubGroups.Children.Add(newFge);
                SubGroups.Add(newFge);
            }

            if (SubGroups.Count>0 && JoinType.HasValue)
            {
                var andOrLabel = new Label()
                                     {
                                         Content = (JoinType.Value == eAndOrJoin.And) ? "And": "Or",
                                         VerticalAlignment = VerticalAlignment.Center,
                                         HorizontalAlignment = HorizontalAlignment.Center
                                     };
                spSubGroups.Children.Add(andOrLabel);
                fge.JoinLabelOnParent = andOrLabel;
                fgeExpr.AndOr = JoinType.Value;
            }
            SubGroups.Add(fge);
            spSubGroups.Children.Add(fge);
            if (fge.AtomFilter != null)
                fge.AtomFilter.FocusControl();
        }

        public int MyBackgroundIndex = 0;
        public void SetBackground( int backgroundIndex)
        {
            if (backgroundIndex > MyBackgroundIndex)
            {
                if (backgroundIndex*2.5 > 255)
                    backgroundIndex = FilterEditor.BackgroundIndexStep;
                byte r = (byte) (255 - (backgroundIndex*2.5));
                byte g = (byte) (255 - (backgroundIndex));
                _currentBrush = new SolidColorBrush(new Color() {A = 255, R = r, G = g, B = 255});
                this.Background = _currentBrush;
                _currentMouseOverBrush = new SolidColorBrush(new Color() { A = 255, R = (byte)( r-5), G = (byte)(g-10), B = 250 });
                    this.border.BorderBrush = new SolidColorBrush(new Color() { A = 255, R = r, G = g, B = 240 });
                MyBackgroundIndex = backgroundIndex;
            }
            if (this.Parent != null)
                Parent.SetBackground(backgroundIndex + FilterEditor.BackgroundIndexStep);
        }

        private void bAdd_Click(object sender, RoutedEventArgs e)
        {
            if (this.IsPopupMode)
            {
                popupFieldSelect.IsOpen = true;
            }
            else
            {
                this.setProperty(PropertyDefinitionModel.GetEmpty());
            }
        }

        private void bOr_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.Or;
            if (this.IsPopupMode)
            {
                popupFieldSelect.IsOpen = true;
            }
            else
            {
                this.setProperty(PropertyDefinitionModel.GetEmpty());
            }
        }

        private void bAnd_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.And;
            if (this.IsPopupMode)
            {
                popupFieldSelect.IsOpen = true;
            }
            else
            {
                this.setProperty(PropertyDefinitionModel.GetEmpty());
            }
        }

        //private void lbFields_MouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    var selectedField = lbFields.SelectedItem as FilterElementDefinition;
        //    if (selectedField != null)
        //    {
        //        var fae = new FilterElementEditor(selectedField) {Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly};
        //        fae.EditorContext = this.EditorContext;
        //        var fge = new FilterGroupEditor(_collection, !_vertical, IsReadOnly, EditorContext)
        //        {Background = Brushes.White};
        //        fge.SetAtomFilter(fae);
        //        AddGroupSubFilter(fge);
        //        popupFieldSelect.IsOpen = false;
        //    }
        //}

        private void propertySelector_SelectClicked(object sender, EventArgs e)
        {
            var selectedField = propertySelector.SelectedProperty;
            if (selectedField != null)
            {
                this.setProperty(selectedField);
            }
        }

        private void setProperty(PropertyDefinitionModel selectedField)
        {
            var fae = new FilterElementEditor(_collection, selectedField, this.IsPopupMode) { Margin = new Thickness(0, 0, 0, 0), IsReadOnly = this.IsReadOnly };
            fae.EditorContext = this.EditorContext;
            var fge = new FilterGroupEditor(_collection, !_vertical, IsReadOnly, EditorContext)
            { Background = Brushes.White };
            fge.SetAtomFilter(fae);
            AddGroupSubFilter(fge);
            popupFieldSelect.IsOpen = false;
        }

        private void miRemove_Click(object sender, RoutedEventArgs e)
        {
            if(Parent!=null)
                Parent.RemoveSubGroup(this);
            else
                EditorContext.Reset(true);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        private void miNegate_Click(object sender, RoutedEventArgs e)
        {
            Negate(!this.Filter.Negate);
        }

        private void Negate(bool negate)
        {
            this.Filter.Negate = negate;
            if (this.Filter.Negate)
            {
                this.borderNegate.Visibility = System.Windows.Visibility.Visible;
                this.bNegate.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                this.borderNegate.Visibility = System.Windows.Visibility.Collapsed;
                this.bNegate.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void miChangeToOr_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.Or;
            (this.Filter as FltGroupExprData).AndOr = eAndOrJoin.Or;
            this.miChangeToOr.Visibility = System.Windows.Visibility.Collapsed;
            this.miChangeToAnd.Visibility = System.Windows.Visibility.Visible;
            foreach (var subgr in this.SubGroups)
            {
                if (subgr.JoinLabelOnParent != null)
                    subgr.JoinLabelOnParent.Content = "Or";
            }
        }

        private void miChangeToAnd_Click(object sender, RoutedEventArgs e)
        {
            this.JoinType = eAndOrJoin.And;
            (this.Filter as FltGroupExprData).AndOr = eAndOrJoin.And;
            this.miChangeToOr.Visibility = System.Windows.Visibility.Visible;
            this.miChangeToAnd.Visibility = System.Windows.Visibility.Collapsed;
            foreach (var subgr in this.SubGroups)
            {
                if (subgr.JoinLabelOnParent!=null)
                    subgr.JoinLabelOnParent.Content = "And";
            }
        }
    }
}
