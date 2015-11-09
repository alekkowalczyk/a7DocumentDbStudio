using a7DocumentDbStudio.Controls.GridColumns;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace a7DocumentDbStudio.Controls
{
    public class CollectionGrid : System.Windows.Controls.DataGrid, INotifyPropertyChanged
    {
        #region dependency properties

        public static DependencyProperty SelectedDocumentProperty = DependencyProperty.Register("SelectedDocument", typeof(DocumentModel), typeof(CollectionGrid));
        public DocumentModel SelectedDocument
        {
            get { return (DocumentModel)GetValue(SelectedDocumentProperty); }
            set { SetValue(SelectedDocumentProperty, value); }
        }

        #endregion


        #region properties

        private bool _isHeaderFilterVisible;
        public bool IsHeaderFilterVisible
        {
            get { return _isHeaderFilterVisible; }
            set { HideShowFilterOnHeaders(!value); _isHeaderFilterVisible = value; }
        }



        public CollectionVM Collection
        {
            get { return  (CollectionVM)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ItemList.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof(CollectionVM), typeof(CollectionGrid),
            new PropertyMetadata(new PropertyChangedCallback(
                (o, e) =>
                {
                    var dg = o as CollectionGrid;
                    if (dg.Collection != null)
                    {
                        dg.SetBinding(CollectionGrid.ItemsSourceProperty, new Binding("Documents") { Source = dg.Collection });
                        //TODO: can cause memory leaks?
                        dg.Collection.Columns.CollectionChanged += (sender, events)=>
                        {
                            dg.Init();
                        };
                        dg.Init();
                        //await dg.Collection.Refresh();
                    }
                    dg.ForceColumnFiltersRerender();
                }
                )));


        private bool _isFilterVisible;
        public bool IsFilterVisible
        {
            get { return IsHeaderFilterVisible; }
            set { IsHeaderFilterVisible = value; _isFilterVisible = value; }
        }

        #endregion

        private bool _columnFiltersPrepared = false;
        private bool _isRendered = false;
        public Style NoFilterHeaderStyle { get; set; }
        
        public CollectionGrid()
            : base()
        {
            this.Sorting += CollectionGrid_Sorting;
            this.CanUserSortColumns = true;
            _isFilterVisible = true;
            this.EnableColumnVirtualization = false;
            this.EnableRowVirtualization = false;
            this.Loaded += CollectionGrid_Loaded;
            this._isHeaderFilterVisible = true;
            this.IsReadOnly = true;
            this.Columns.Clear();
        }

        public async Task Refresh()
        {
            if (this.Collection != null)
                await this.Collection.Refresh();
        }


        void CollectionGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isRendered && _isHeaderFilterVisible)
            {
                //prepareColumnFilters();
            }
        }
        
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (!_columnFiltersPrepared)
                prepareColumnFilters();
            base.OnRender(drawingContext);
            _isRendered = true;
        }
        
        public void Init()
        {
            this.Columns.Clear();
            if (this.Collection != null)
            {
                foreach (var column in this.Collection.Columns)
                {
                    if (column.IsNotEmpty())
                    {
                        var col = new CollectionGridColumn(this, column);
                        if (column.Path == "id")
                            col.MinWidth = 80;
                        this.Columns.Add(col);
                        this.Collection.Filter.FieldsFilter.SetFieldFilterPropertyType(column.Path, column.Type);
                    }
                }
            }
            this.ForceColumnFiltersRerender();
        }


        void CollectionGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            var col = e.Column;
        }
        
        //map inner dg selecteditem to SelectedDocument and selectedItem
        void dg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //this.SelectedItem = null;
            if (e.AddedItems.Count > 0 && e.OriginalSource == sender)
            {
                var addedItem = e.AddedItems[0];
                this.SelectedDocument = addedItem as DocumentModel;
            }
            else
                e.Handled = true;
        }
        
        #region filter related methods

        public void ForceColumnFiltersRerender()
        {
            _columnFiltersPrepared = false;
            //InvalidateVisual();
        }

        private void prepareColumnFilters()
        {
            if (IsHeaderFilterVisible)
            {
                for (int i = 0; i < this.Columns.Count; i++)
                {
                    DataGridColumnHeader header = GetColumnHeaderFromColumn(this.Columns[i]);
                    CollectionGridColumn idgc = this.Columns[i] as CollectionGridColumn;
                    if (header != null && idgc != null)
                    {
                        ContentControl cc = header.FindVisualChildByName<ContentControl>("columnFilterHost");
                        if (cc != null && idgc.ColumnFilterControl != null)
                        {
                            var colFlt = idgc.ColumnFilterControl;
                            colFlt.SetBinding(FrameworkElement.VisibilityProperty, new Binding("IsSqlEditMode")
                            {
                                Source = this.Collection,
                                Converter = new Converters.BoolToVisibilityReverse()
                            });
                            cc.Content = colFlt;
                        }
                        Button closeButton = header.FindVisualChildByName<Button>("btnClose");
                        var column = this.Columns[i];
                        RoutedEventHandler closeButtonClick = async (s, e) => {
                            if (this.Columns.Count == 1)
                            {
                                MessageBox.Show("Can't remove last column");
                            }
                            else
                            {
                                this.Columns.Remove(column);
                                if (this.Collection != null)
                                {
                                    await this.Collection.RemoveColumn(idgc.PropertyDefinition.Path);
                                }
                            }
                        };
                        closeButton.Click -= closeButtonClick;
                        closeButton.Click += closeButtonClick;
                    }
                }
            }
            HideShowFilterOnHeaders(!this.IsHeaderFilterVisible);
            _columnFiltersPrepared = true;
        }


        private void HideShowFilterOnHeaders(bool hideFilter)
        {
            if (!_isFilterVisible)
                hideFilter = true;
            // dataGrid is the name of your DataGrid. In this case Name="dataGrid"
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(this);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                // columnHeader.Height = 28;
                TextBox tb = columnHeader.FindVisualChildByName<TextBox>("filterTextBox");
                Grid gr = columnHeader.FindVisualChildByName<Grid>("grid");
                if (gr != null)
                {
                    if (hideFilter)
                        gr.RowDefinitions[1].Height = new GridLength(0.0);
                    else
                        gr.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Auto);
                }
                if (tb != null)
                {
                    if (hideFilter)
                        tb.Visibility = Visibility.Collapsed;
                    else
                        tb.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        #endregion

        #region helper functions

        private DataGridColumnHeader GetColumnHeaderFromColumn(DataGridColumn column)
        {
            // dataGrid is the name of your DataGrid. In this case Name="dataGrid"
            List<DataGridColumnHeader> columnHeaders = GetVisualChildCollection<DataGridColumnHeader>(this);
            foreach (DataGridColumnHeader columnHeader in columnHeaders)
            {
                if (columnHeader.Column == column)
                {
                    return columnHeader;
                }
            }
            return null;
        }

        public List<T> GetVisualChildCollection<T>(object parent) where T : Visual
        {
            List<T> visualCollection = new List<T>();
            GetVisualChildCollection(parent as DependencyObject, visualCollection);
            return visualCollection;
        }

        private void GetVisualChildCollection<T>(DependencyObject parent, List<T> visualCollection) where T : Visual
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                {
                    visualCollection.Add(child as T);
                }
                else if (child != null)
                {
                    GetVisualChildCollection(child, visualCollection);
                }
            }
        }

        #endregion

        #region overrides

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == CollectionGrid.IsReadOnlyProperty && e.NewValue is bool)
            {
                setColumnsRO();
            }
        }

        private void setColumnsRO()
        {
            foreach (var col in Columns)
            {
                col.IsReadOnly = this.IsReadOnly;
            }
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            HorizontalGridLinesBrush = Brushes.LightGray;
            VerticalGridLinesBrush = Brushes.Transparent;
            SelectionUnit = DataGridSelectionUnit.FullRow;
            Background = Brushes.White;
            RowHeaderWidth = 0;
            //     RowHeight = 20;
            BorderThickness = new Thickness(0);
            if (ColumnHeaderStyle == null)
                ColumnHeaderStyle = ResourcesManager.Instance.GetResource<Style>("CollectionGridColumnHeaderStyle");
            if (RowStyle == null)
                RowStyle = ResourcesManager.Instance.GetResource<Style>("CollectionGridRowStyle");
            if (CellStyle == null)
                CellStyle = ResourcesManager.Instance.GetResource<Style>("CollectionGridCellStyle");
            if (Style == null)
                Style = ResourcesManager.Instance.GetResource<Style>("CollectionGridStyle");
            AutoGenerateColumns = false;
            this.SelectionChanged += new SelectionChangedEventHandler(dg_SelectionChanged);

            this.Background = Brushes.White; //todo: hardcode :D
        }


        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                base.OnKeyUp(e);
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key != System.Windows.Input.Key.Enter)
                base.OnKeyDown(e);
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
            if (_isRendered)
            {
                //_columnFiltersPrepared = false;
                InvalidateVisual();
            }
        }
        
        #endregion

    
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DependencyObject dgPopup = GetTemplateChild("dgPopup");
            if (dgPopup != null)
            {
                _dgPopup = dgPopup as Popup;
                _dgPopup.Closed += new EventHandler(_dgPopup_Closed);
                _dgPopup.Opened += new EventHandler(_dgPopup_Opened);
            }
            var maxItemsComboBox = GetTemplateChild("PART_maxItemsComboBox") as ComboBox;
            if(maxItemsComboBox!=null)
            {
                maxItemsComboBox.ItemsSource = new Dictionary<int, string>
                {
                    [20] = "20",
                    [50] = "50",
                    [100] = "100"
                };
                maxItemsComboBox.SelectionChanged += MaxItemsComboBox_SelectionChanged;
                maxItemsComboBox.SetBinding(ComboBox.SelectedValueProperty, new Binding("Collection.MaxItems") { Source = this });
            }
        }

        private async void MaxItemsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.Collection!=null)
                await this.Collection.Refresh();
        }


        #region popup handling
        private Popup _dgPopup;


        void _dgPopup_Opened(object sender, EventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged += new EventHandler(wnd_LocationChanged);
        }


        void _dgPopup_Closed(object sender, EventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged -= wnd_LocationChanged;
        }


        void wnd_LocationChanged(object sender, EventArgs e)
        {
            var offset = _dgPopup.HorizontalOffset;
            _dgPopup.HorizontalOffset = offset + 1;
            _dgPopup.HorizontalOffset = offset;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropChange(string prop)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #endregion


        public bool IsInPopup
        {
            get;
            set;
        }
    }
}
