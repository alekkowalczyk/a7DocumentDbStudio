using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace a7DocumentDbStudio.Controls
{
    public class AddColumnButton : Control
    {
        public ObservableCollection<PropertyDefinitionModel> Properties
        {
            get { return (ObservableCollection<PropertyDefinitionModel>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Properties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(ObservableCollection<PropertyDefinitionModel>), typeof(AddColumnButton), new PropertyMetadata(null, (s, e) =>
            {
            }));



        public PropertyDefinitionModel SelectedProperty
        {
            get { return (PropertyDefinitionModel)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register("SelectedProperty", typeof(PropertyDefinitionModel), typeof(AddColumnButton), new PropertyMetadata(null));

        public ICommand SelectCommand
        {
            get { return (ICommand)GetValue(SelectCommandProperty); }
            set { SetValue(SelectCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectCommandProperty =
            DependencyProperty.Register("SelectCommand", typeof(ICommand), typeof(AddColumnButton), new PropertyMetadata(null));


        public AddColumnButton()
        {
            Template = a7DocumentDbStudio.Utils.ResourcesManager.Instance.GetControlTemplate("a7AddColumnButtonTemplate");
        }

        private Popup _fePopup;
        private PropertySelectorPopup _fePopupControl;
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
                _fePopupControl = fePopupControl as PropertySelectorPopup;
                _fePopupControl.bOk.Click += (s, e) =>
                {
                    _fePopup.IsOpen = false;
                };
            }
            this.myWindow = Window.GetWindow(this);
            if (myWindow != null)
                this.myWindow.PreviewMouseDown += new MouseButtonEventHandler(myWindow_PreviewMouseDown);
        }

        void myWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (this.IsMouseOver)
                return;
            if (this._fePopup != null && this._fePopup.IsMouseOver)
                return;
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

        
    }
}
