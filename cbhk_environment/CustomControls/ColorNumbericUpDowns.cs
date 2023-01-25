using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace cbhk_environment.CustomControls
{
    public class ColorNumbericUpDowns:TextBox
    {
        public ImageBrush PressedBackground
        {
            get { return (ImageBrush)GetValue(PressedBackgroundProperty); }
            set { SetValue(PressedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty PressedBackgroundProperty =
            DependencyProperty.Register("PressedBackground", typeof(ImageBrush), typeof(RepeatButtonWithBackground), new PropertyMetadata(default(ImageBrush)));

        public Brush ArrowBackground
        {
            get { return (Brush)GetValue(ArrowBackgroundProperty); }
            set { SetValue(ArrowBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ArrowBackgroundProperty =
            DependencyProperty.Register("ArrowBackground", typeof(Brush), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(Brush)));

        public Brush ArrowForeground
        {
            get { return (Brush)GetValue(ArrowForegroundProperty); }
            set { SetValue(ArrowForegroundProperty, value); }
        }

        public static readonly DependencyProperty ArrowForegroundProperty =
            DependencyProperty.Register("ArrowForeground", typeof(Brush), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(Brush)));

        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(double)));

        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(double)));

        public double ArrowWidth
        {
            get { return (double)GetValue(ArrowWidthProperty); }
            set { SetValue(ArrowWidthProperty, value); }
        }

        public static readonly DependencyProperty ArrowWidthProperty =
            DependencyProperty.Register("ArrowWidth", typeof(double), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(double)));

        public double ArrowHeight
        {
            get { return (double)GetValue(ArrowHeightProperty); }
            set { SetValue(ArrowHeightProperty, value); }
        }

        public static readonly DependencyProperty ArrowHeightProperty =
            DependencyProperty.Register("ArrowHeight", typeof(double), typeof(ColorNumbericUpDowns), new PropertyMetadata(default(double)));
    }
}
