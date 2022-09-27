using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace cbhk_environment.CustomControls
{
    public class IconComboBoxItems:ComboBoxItem
    {
        public string ContentData
        {
            get { return (string)GetValue(ContentDataProperty); }
            set { SetValue(ContentDataProperty, value); }
        }

        public static readonly DependencyProperty ContentDataProperty =
            DependencyProperty.Register("ContentData", typeof(string), typeof(IconComboBoxItems), new PropertyMetadata(default(string)));

        public Image IconData
        {
            get { return (Image)GetValue(IconDataProperty); }
            set { SetValue(IconDataProperty, value); }
        }

        public static readonly DependencyProperty IconDataProperty =
            DependencyProperty.Register("IconData", typeof(Image), typeof(IconComboBoxItems), new PropertyMetadata(default(Image)));
    }
}
