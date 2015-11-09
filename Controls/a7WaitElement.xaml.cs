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
    public partial class a7WaitElement : UserControl
    {
        private double _offsetSeconds;
        /// <summary>
        /// The number of seconds from the control being loaded that the animation should 
        /// be offset. This defaults to zero, meaning the animation will start as soon as 
        /// the control is loaded.
        /// </summary> 
        public double OffsetSeconds
        {
            get
            {
                return this._offsetSeconds;
            }
            set
            {
                this._offsetSeconds = value;
                this.OffsetKeyFrameKeyTimes();
            }
        }

        /// <summary>
        /// Offsets the four keyframes of the animation with the set offset value. This 
        /// allows rectangles to be visually staggered if more than one are being used together.
        /// </summary>
        private void OffsetKeyFrameKeyTimes()
        {
            TimeSpan offset = TimeSpan.FromSeconds(OffsetSeconds);
            KeyFrame1.KeyTime = KeyFrame1.KeyTime.TimeSpan.Add(offset);
            KeyFrame2.KeyTime = KeyFrame2.KeyTime.TimeSpan.Add(offset);
            KeyFrame3.KeyTime = KeyFrame3.KeyTime.TimeSpan.Add(offset);
            KeyFrame4.KeyTime = KeyFrame4.KeyTime.TimeSpan.Add(offset);
        }

        public void Resize(double width)
        {
            KeyFrame2.Value = width / 3;
            KeyFrame3.Value = KeyFrame2.Value * 2;
            KeyFrame4.Value = width + 10;
            KeyFrame5.Value = width + 10;
        }

        public a7WaitElement()
        {
            InitializeComponent();
            sb.Stop();
        }

        public void Start()
        {
            sb.Resume();
        }

        public void Stop()
        {
            sb.Stop();
        }
    }
}
