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
    public partial class PropertySelectorPopup : UserControl
    {
        public event EventHandler SelectClicked;

        public ObservableCollection<PropertyDefinitionModel> Properties
        {
            get { return (ObservableCollection<PropertyDefinitionModel>)GetValue(PropertiesProperty); }
            set { SetValue(PropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Properties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertiesProperty =
            DependencyProperty.Register("Properties", typeof(ObservableCollection<PropertyDefinitionModel>), typeof(PropertySelectorPopup), new PropertyMetadata(null, (s, e) =>
            {
            }));



        public PropertyDefinitionModel SelectedProperty
        {
            get { return (PropertyDefinitionModel)GetValue(SelectedPropertyProperty); }
            set { SetValue(SelectedPropertyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedPropertyProperty =
            DependencyProperty.Register("SelectedProperty", typeof(PropertyDefinitionModel), typeof(PropertySelectorPopup), new PropertyMetadata(null));



        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(PropertySelectorPopup), new PropertyMetadata(null));



        public PropertySelectorPopup()
        {
            InitializeComponent();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            if (ClickCommand != null && this.SelectedProperty != null)
            {
                ClickCommand.Execute(this.SelectedProperty);
                this.propSelector.Clear();
            }
            if (SelectClicked != null && this.SelectedProperty != null)
            {
                SelectClicked(this, null);
                this.propSelector.Clear();
            }
        }
    }
}
