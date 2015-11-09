using a7DocumentDbStudio.Enums;
using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace a7DocumentDbStudio.Dialogs
{
    /// <summary>
    /// Interaction logic for SelectPropertyDlg.xaml
    /// </summary>
    public partial class SelectPropertyDlg : Window, INotifyPropertyChanged
    {
        public ObservableCollection<PropertyDefinitionModel> Properties { get; set; }

        private PropertyDefinitionModel _selectedProperty;
        public PropertyDefinitionModel SelectedProperty
        {
            get
            {
                return _selectedProperty;
            }
            set
            {
                _selectedProperty = value;
                if(_selectedProperty!=null)
                    this.SelectedPropertyType = _selectedProperty.Type;
            }
        }

        private PropertyType _selectedPropertyType;

        public event PropertyChangedEventHandler PropertyChanged;

        public PropertyType SelectedPropertyType
        {
            get { return _selectedPropertyType; }
            set {
                _selectedPropertyType = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("SelectedPropertyType"));
            }
        }


        public string FieldNameText { get; set; }

        public SelectPropertyDlg(ObservableCollection<PropertyDefinitionModel> properties)
        {
            Properties = properties;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
