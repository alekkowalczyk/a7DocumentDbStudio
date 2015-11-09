using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for a7Wait.xaml
    /// </summary>
    public partial class a7Wait : UserControl
    {
        //public Color Color
        //{
        //    set
        //    {
        //        Element1.Rectangle.Fill = new SolidColorBrush(value);
        //        Element2.Rectangle.Fill = new SolidColorBrush(value);
        //        Element3.Rectangle.Fill = new SolidColorBrush(value);
        //        Element4.Rectangle.Fill = new SolidColorBrush(value);
        //        Element5.Rectangle.Fill = new SolidColorBrush(value);
        //    }
        //}

        public a7Wait()
        {
            InitializeComponent();
            this.SizeChanged += (sender, args) =>
                                    {
                                        if (args.WidthChanged)
                                        {
                                            Element1.Resize(this.ActualWidth);
                                            Element2.Resize(this.ActualWidth);
                                            Element3.Resize(this.ActualWidth);
                                            Element4.Resize(this.ActualWidth);
                                            Element5.Resize(this.ActualWidth);
                                        }
                                    };
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == HeightProperty)
            {
                Element1.Height = this.Height;
                Element2.Height = this.Height;
                Element3.Height = this.Height;
                Element4.Height = this.Height;
                Element5.Height = this.Height;
            }
        }

    }
}
