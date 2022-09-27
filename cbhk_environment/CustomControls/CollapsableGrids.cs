using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.CustomControls
{
    public class CollapsableGrids:Expander
    {
        public Thickness TitleMargin
        {
            get { return (Thickness)GetValue(TitleMarginProperty); }
            set { SetValue(TitleMarginProperty, value); }
        }

        public static readonly DependencyProperty TitleMarginProperty =
            DependencyProperty.Register("TitleMargin", typeof(Thickness), typeof(CollapsableGrids), new PropertyMetadata(default(Thickness)));
    }
}
