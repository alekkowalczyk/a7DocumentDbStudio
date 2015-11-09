using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace a7DocumentDbStudio.Controls
{
    /// <summary>
    /// Interaction logic for PropertySelector.xaml
    /// </summary>
    public partial class PropertySelector : UserControl
    {
        public event EventHandler SelectedPropertyChanged;

        public ObservableCollection<PropertyDefinitionModel> Properties
        {
            get { return (ObservableCollection<PropertyDefinitionModel>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Properties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(ObservableCollection<PropertyDefinitionModel>), typeof(PropertySelector), new PropertyMetadata(null, (s, e) =>
            {
            }));

        public PropertyDefinitionModel SelectedProperty
        {
            get { return (PropertyDefinitionModel)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register("SelectedProperty", typeof(PropertyDefinitionModel), typeof(PropertySelector), new PropertyMetadata(null));

        public PropertySelector()
        {
            InitializeComponent();
            this.cbType.SelectedValue = PropertyType.String;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if(e.Property == SelectedPropertyProperty)
            {
                this.cbType.SelectedValue = this.SelectedProperty.Type;
                this.acbProperty.Text = this.SelectedProperty.Path;
            }
        }

        public void Clear()
        {
            this.cbType.SelectedValue = PropertyType.String;
            this.acbProperty.Text = "";
            this.acbProperty.SelectedItem = null;
        }

        private void selectedPropertychanged()
        {
            if (this.SelectedPropertyChanged != null)
                this.SelectedPropertyChanged(this, null);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectedProperty != null && e.AddedItems.Count > 0 && e.AddedItems[0] is PropertyType)
            {
                this.SelectedProperty.Type = (PropertyType)e.AddedItems[0];
                selectedPropertychanged();
            }
        }

        private void AutoCompleteBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is PropertyDefinitionModel)
            {
                this.SelectedProperty = e.AddedItems[0] as PropertyDefinitionModel;
                this.cbType.SelectedValue = this.SelectedProperty.Type;
                selectedPropertychanged();
            }
        }

        private void AutoCompleteBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var type = PropertyType.String;
            if (cbType.SelectedValue is PropertyType)
                type = (PropertyType)cbType.SelectedValue;
            else
                cbType.SelectedValue = type;
            this.SelectedProperty = new PropertyDefinitionModel
            {
                Name = acbProperty.Text,
                Path = acbProperty.Text,
                Type = type
            };
            selectedPropertychanged();
        }
    }
}
