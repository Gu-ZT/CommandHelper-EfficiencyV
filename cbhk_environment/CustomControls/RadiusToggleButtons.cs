using System.Windows;
using System.Windows.Controls.Primitives;

namespace cbhk_environment.CustomControls
{
    public class RadiusToggleButtons:ToggleButton
    {
        public string SelectedToggleText
        {
            get { return (string)GetValue(SelectedToggleTextProperty); }
            set { SetValue(SelectedToggleTextProperty, value); }
        }

        public static readonly DependencyProperty SelectedToggleTextProperty =
            DependencyProperty.Register("SelectedToggleText", typeof(string), typeof(RadiusToggleButtons), new PropertyMetadata(default(string)));

        public string UnSelectedToggleText
        {
            get { return (string)GetValue(UnSelectedToggleTextProperty); }
            set { SetValue(UnSelectedToggleTextProperty, value); }
        }

        public static readonly DependencyProperty UnSelectedToggleTextProperty =
            DependencyProperty.Register("UnSelectedToggleText", typeof(string), typeof(RadiusToggleButtons), new PropertyMetadata(default(string)));
    }
}
