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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace a7DocumentDbStudio.Views
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class Document : UserControl
    {
        FoldingManager foldingManager;
        BraceFoldingStrategy foldingStrategy;

        public Document()
        {
            InitializeComponent();
            this.Loaded += Document_Loaded;
        }

        private void Document_Loaded(object sender, RoutedEventArgs e)
        {
            //using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("a7DocumentDbStudio.Resources.avalonEditJson.xshd"))
            //{
            //    using (var reader = new System.Xml.XmlTextReader(stream))
            //    {
            //        this.jsonEditor.SyntaxHighlighting =
            //            ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
            //            ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            //    }
            //}
            this.jsonEditor.TextChanged += Te_TextChanged;
            foldingManager = FoldingManager.Install(this.jsonEditor.TextArea);
            foldingStrategy = new BraceFoldingStrategy();
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }

        private void Te_TextChanged(object sender, EventArgs e)
        {
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }
    }
}
