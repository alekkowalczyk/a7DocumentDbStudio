using a7DocumentDbStudio.Controls.FilterEditor;
using a7DocumentDbStudio.ViewModel;
using a7DocumentDbStudio.ViewModel.Workspace;
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

namespace a7DocumentDbStudio.Views.Workspaces
{
    /// <summary>
    /// Interaction logic for Collection.xaml
    /// </summary>
    public partial class Collection : UserControl
    {
        public Collection()
        {
            InitializeComponent();
            this.Loaded += Collection_Loaded;
        }

        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
            using (var stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("a7DocumentDbStudio.Resources.avalonEditSql.xshd"))
            {
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    this.sqlEditor.SyntaxHighlighting =
                        ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader,
                        ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.Property == DataContextProperty)
            {
                if(e.NewValue is CollectionWrkspcVM)
                {
                    var cwvm = e.NewValue as CollectionWrkspcVM;
                    cwvm.Collection.PropertyChanged -= Cwvm_PropertyChanged;
                    cwvm.Collection.PropertyChanged += Cwvm_PropertyChanged;
                }
                else
                {
                    if(this.DataContext is CollectionWrkspcVM)
                    {
                        (e.NewValue as CollectionWrkspcVM).Collection.PropertyChanged -= Cwvm_PropertyChanged;
                    }
                }
            }
            base.OnPropertyChanged(e);
        }

        private void Cwvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.DataContext is CollectionWrkspcVM && e.PropertyName == "Documents") //TODO: magic string - ugly
            {
                var cwvm = (this.DataContext as CollectionWrkspcVM).Collection;
                this.feb.Collection = cwvm;
                this.feb.SetBinding(FilterEditorButton.FilterExprProperty, new Binding("Filter.AdvancedFilter") { Source = cwvm });
                this.feb.UpdateFilterFunction = async (flt) =>
                {
                    cwvm.Filter.AdvancedFilter = flt;
                    await cwvm.Refresh();
                };
            }
        }
    }
}
