using System;
using System.Collections.Generic;
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
    /// Interaction logic for ChangeValue.xaml
    /// </summary>
    public partial class ChangeValue : Window
    {
        private Type _type;
        public object Value { get; private set; }

        public ChangeValue(object value)
        {
            InitializeComponent();
            this._type = value.GetType();
            this.tbValue.Text = value.ToString();
            if (value is string && (value as string).Contains("\n"))
                setMultiLine();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            var converter = TypeDescriptor.GetConverter(_type);
            try
            {
                this.Value = converter.ConvertFromInvariantString(this.tbValue.Text);
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Couldn't convert value - {Environment.NewLine}{ex.Message}");
            }
            
        }

        private void setMultiLine()
        {
            this.Height = 250;
            tbValue.Height = 135;
            tbValue.AcceptsReturn = true;
            tbValue.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            tbValue.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }
    }
}
