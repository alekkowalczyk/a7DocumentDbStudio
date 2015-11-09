using System;
using System.Collections.Generic;
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
    /// Interaction logic for NameSelector.xaml
    /// </summary>
    public partial class NameSelector : Window
    {
        public string Value
        {
            get; private set;
        }

        public NameSelector(string title)
        {
            InitializeComponent();
            this.Title = title;
            this.tb.Text = title;
        }
        

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Value = this.tbValue.Text;
            this.Close();

        }
    }
}
