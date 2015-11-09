using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using a7DocumentDbStudio.Filter;
using a7DocumentDbStudio.ViewModel;

namespace a7DocumentDbStudio.Controls.FilterEditor
{
    public class FilterEditorButton : Control
    {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.Register("Collection", typeof (CollectionVM), typeof (FilterEditorButton), new PropertyMetadata(null));

        public CollectionVM Collection
        {
            get { return (CollectionVM) GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsReadOnly.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(FilterEditorButton), new UIPropertyMetadata(false));


        public Brush ActiveBackground
        {
            get { return (Brush)GetValue(ActiveBackgroundProperty); }
            set { SetValue(ActiveBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveBackgroundProperty =
            DependencyProperty.Register("ActiveBackground", typeof(Brush), typeof(FilterEditorButton), new UIPropertyMetadata(Brushes.Transparent));

        

        public Action<FilterExpressionData> UpdateFilterFunction
        {
            get { return (Action<FilterExpressionData>)GetValue(UpdateFilterFunctionProperty); }
            set { SetValue(UpdateFilterFunctionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RefreshFunction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UpdateFilterFunctionProperty =
            DependencyProperty.Register("UpdateFilterFunction", typeof(Action<FilterExpressionData>), typeof(FilterEditorButton));


        public FilterExpressionData FilterExpr
        {
            get { return (FilterExpressionData)GetValue(FilterExprProperty); }
            set { SetValue(FilterExprProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FilterExpr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FilterExprProperty =
            DependencyProperty.Register("FilterExpr", typeof(FilterExpressionData), typeof(FilterEditorButton), new UIPropertyMetadata(null));



        public bool IsPopupMode
        {
            get { return (bool)GetValue(IsPopupModeProperty); }
            set { SetValue(IsPopupModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPopupMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPopupModeProperty =
            DependencyProperty.Register("IsPopupMode", typeof(bool), typeof(FilterEditorButton), new PropertyMetadata(false));



        public FilterEditorButton()
        {
            Template = a7DocumentDbStudio.Utils.ResourcesManager.Instance.GetControlTemplate("a7FilterEditorButtonTemplate");
        }

        private Popup _fePopup;
        private FilterEditor _fePopupControl;
        private CollectionVM _collection = null;
        private Window myWindow;
        public override void OnApplyTemplate()
        {
            DependencyObject fePopup = GetTemplateChild("fePopup");
            if (fePopup != null)
            {
                _fePopup = fePopup as Popup;
                _fePopup.Opened += new EventHandler(_fePopup_Opened);
                _fePopup.Closed += new EventHandler(_fePopup_Closed);
            }
            var fePopupControl = GetTemplateChild("fePopupControl");
            if (fePopupControl != null)
            {
                _fePopupControl = fePopupControl as FilterEditor;
                if(_collection != null)
                    _fePopupControl.SetCollection(_collection);
                if (FilterExpr != null)
                    _fePopupControl.SetFilter(_collection, FilterExpr);
            }
            this.myWindow = Window.GetWindow(this);
            if(myWindow!=null)
                this.myWindow.PreviewMouseDown += new MouseButtonEventHandler(myWindow_PreviewMouseDown);

            if (UpdateFilterFunction != null)
            {
                _fePopupControl.UpdateFilterFunction = UpdateFilter;
            }
            _fePopupControl.SetBinding(FilterEditor.IsReadOnlyProperty, new Binding("IsReadOnly") { Source = this });
        }

        void UpdateFilter(FilterExpressionData filter)
        {
            if (UpdateFilterFunction != null)
                UpdateFilterFunction(filter);
            if (filter != null && filter.HasActiveFilter)
            {
                this.ActiveBackground = Brushes.Red;
            }
            else
            {
                this.ActiveBackground = Brushes.Transparent;
            }
        }

        void myWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseOver)
                return;
            if (this._fePopupControl != null && this._fePopupControl.IsMouseOver)
                return;
            var isOverPopup = false;
            foreach (Popup pp in _fePopupControl.EntityFieldsPopups)
            {
                if (pp.IsMouseOver)
                    isOverPopup = true;
            }
            if(!isOverPopup)
                this._fePopup.IsOpen = false;
        }




        private void _fePopup_Closed(object sender, EventArgs eventArgs)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged -= wnd_LocationChanged; 

        }

        void _fePopup_Opened(object sender, EventArgs e)
        {
            Window wnd = Window.GetWindow(this);
            if (wnd != null)
                wnd.LocationChanged += new EventHandler(wnd_LocationChanged);
        }

        void wnd_LocationChanged(object sender, EventArgs e)
        {
            var offset = _fePopup.HorizontalOffset;
            _fePopup.HorizontalOffset = offset + 1;
            _fePopup.HorizontalOffset = offset;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property ==  CollectionProperty && _collection != e.NewValue as CollectionVM)
            {
                if (_fePopupControl != null)
                {
                    _fePopupControl.SetCollection(e.NewValue as CollectionVM);
                    _collection = e.NewValue as CollectionVM;
                    if(this.FilterExpr !=null)
                        _fePopupControl.SetFilter(_collection, FilterExpr);
                }
                else
                {
                    _collection = e.NewValue as CollectionVM;
                }
            }
            else if (e.Property == FilterExprProperty)
            {
                if (_fePopupControl != null && this._collection != null)
                {
                    _fePopupControl.SetFilter(this._collection, e.NewValue as FilterExpressionData);
                }
                else
                {
                    this.FilterExpr = e.NewValue as FilterExpressionData;
                }
                if (this.FilterExpr != null && FilterExpr.HasActiveFilter)
                {
                    this.ActiveBackground = Brushes.Red;
                }
                else
                {
                    this.ActiveBackground = Brushes.Transparent;
                }
            }
            else if (e.Property == UpdateFilterFunctionProperty && this._fePopupControl != null)
            {
                this._fePopupControl.UpdateFilterFunction = UpdateFilter;
            }
        }
    }


}
