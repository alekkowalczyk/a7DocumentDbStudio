using a7DocumentDbStudio.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace a7DocumentDbStudio.Controls
{
    public class a7OptionButton : Button
    {
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(a7OptionButton), new PropertyMetadata(default(ImageSource)));

        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        public a7OptionButton() : base()
        {
            this.Template = ResourcesManager.Instance.GetControlTemplate("OptionButtonTemplate");
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("pack://application:,,,/a7DocumentDbStudio;component/Images/green_right.png");
            bitmapImage.EndInit();
            this.Image = bitmapImage;
        }
    }
}
