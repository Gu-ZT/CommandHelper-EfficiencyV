using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace cbhk_environment.CustomControls
{
    public class TextToggleButtons:ToggleButton
    {
        public Brush SelectedBackground
        {
            get { return (Brush)GetValue(SelectedBackgroundProperty); }
            set { SetValue(SelectedBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SelectedBackgroundProperty =
            DependencyProperty.Register("SelectedBackground", typeof(Brush), typeof(TextToggleButtons), new PropertyMetadata(default(Brush)));
    }
}
