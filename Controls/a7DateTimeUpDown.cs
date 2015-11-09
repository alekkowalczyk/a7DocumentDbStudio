using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit;
using System.Windows;

namespace a7DocumentDbStudio.Controls
{
    public class a7DateTimeUpDown : DateTimeUpDown
    {
        public TimeSpan? ValueTimeSpan
        {
            get { return (TimeSpan?)GetValue(ValueTimeSpanProperty); }
            set { SetValue(ValueTimeSpanProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValueTimeSpan.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueTimeSpanProperty =
            DependencyProperty.Register("ValueTimeSpan", typeof(TimeSpan?), typeof(a7DateTimeUpDown), new PropertyMetadata(null));


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if(e.Property == a7DateTimeUpDown.ValueTimeSpanProperty)
            {
                if (this.ValueTimeSpan == null)
                    this.Value = null;
                else
                {
                    DateTime dt = new DateTime(2013, 01, 01);
                    this.Value = dt + this.ValueTimeSpan;
                }
            }
            else if (e.Property == a7DateTimeUpDown.ValueProperty)
            {
                if (this.Value == null)
                    this.ValueTimeSpan = null;
                else
                {
                    DateTime dt = new DateTime(Value.Value.Year, Value.Value.Month, Value.Value.Day);
                    this.ValueTimeSpan = this.Value.Value - dt;
                }
            }
            base.OnPropertyChanged(e);
        }
    }
}
