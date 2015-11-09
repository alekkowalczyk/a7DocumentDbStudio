using a7DocumentDbStudio.ViewModel;
using Newtonsoft.Json.Linq;
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

namespace a7DocumentDbStudio.Controls
{
    /// <summary>
    /// Interaction logic for JsonTree.xaml
    /// </summary>
    public partial class JsonTree : UserControl
    {
        public DocumentVM Document
        {
            get { return (DocumentVM)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(DocumentVM), typeof(JsonTree), new PropertyMetadata(null));



        public JToken JToken
        {
            get { return (JToken)GetValue(JTokenProperty); }
            set { SetValue(JTokenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Document.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JTokenProperty =
            DependencyProperty.Register("JToken", typeof(JToken), typeof(JsonTree), new PropertyMetadata(null));



        public JsonTree()
        {
            InitializeComponent();
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if(e.Property == JTokenProperty)
            {
                if(e.NewValue is JToken)
                {
                    this.treeView1.ItemsSource = null;
                    this.treeView1.Items.Clear();
                    this.treeView1.ItemsSource = new List<JToken> {( e.NewValue as JToken)};
                }
            }
        }
    }
}
