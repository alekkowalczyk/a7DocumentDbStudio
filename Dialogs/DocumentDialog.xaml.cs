using a7DocumentDbStudio.Utils;
using ICSharpCode.AvalonEdit.Folding;
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
    public partial class DocumentDialog : Window
    {
        FoldingManager foldingManager;
        BraceFoldingStrategy foldingStrategy;

        public bool IsEditMode => true;
        public string Value
        {
            get; private set;
        }

        public DocumentDialog(string value)
        {
            InitializeComponent();
            this.jsonEditor.Text = value;
            this.jsonEditor.TextChanged += Te_TextChanged;
            foldingManager = FoldingManager.Install(this.jsonEditor.TextArea);
            foldingStrategy = new BraceFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }

        private void Te_TextChanged(object sender, EventArgs e)
        {
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void bOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Value = this.jsonEditor.Text;
            this.Close();

        }
    }
}
