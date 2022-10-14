using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Input;

namespace cbhk_environment.CustomControls
{
    public class RichTabItems:TabItem
    {
        #region DependencyProperties
        public BitmapImage HeaderImage
        {
            get { return (BitmapImage)GetValue(HeaderImageProperty); }
            set { SetValue(HeaderImageProperty, value); }
        }

        public static readonly DependencyProperty HeaderImageProperty =
            DependencyProperty.Register("HeaderImage", typeof(BitmapImage), typeof(RichTabItems), new PropertyMetadata(default(BitmapImage)));

        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(RichTabItems), new PropertyMetadata(default(string)));

        public Brush CloseButtonBackground
        {
            get { return (Brush)GetValue(CloseButtonBackgroundProperty); }
            set { SetValue(CloseButtonBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonBackgroundProperty =
            DependencyProperty.Register("CloseButtonBackground", typeof(Brush), typeof(RichTabItems), new PropertyMetadata(default(Brush)));

        public Brush CloseButtonBorderBrush
        {
            get { return (Brush)GetValue(CloseButtonBorderBrushProperty); }
            set { SetValue(CloseButtonBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonBorderBrushProperty =
            DependencyProperty.Register("CloseButtonBorderBrush", typeof(Brush), typeof(RichTabItems), new PropertyMetadata(default(Brush)));

        public Thickness CloseButtonBorderThickness
        {
            get { return (Thickness)GetValue(CloseButtonBorderThicknessProperty); }
            set { SetValue(CloseButtonBorderThicknessProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonBorderThicknessProperty =
            DependencyProperty.Register("CloseButtonBorderThickness", typeof(Thickness), typeof(RichTabItems), new PropertyMetadata(default(Thickness)));

        public Brush CloseButtonForeground
        {
            get { return (Brush)GetValue(CloseButtonForegroundProperty); }
            set { SetValue(CloseButtonForegroundProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonForegroundProperty =
            DependencyProperty.Register("CloseButtonForeground", typeof(Brush), typeof(RichTabItems), new PropertyMetadata(default(Brush)));

        public Visibility CloseButtonVisibility
        {
            get { return (Visibility)GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(RichTabItems), new PropertyMetadata(default(Visibility)));


        #endregion

        static int current_index = -1;
        static int select_index = -1;
        static RichTabItems current_item = null;
        static RichTabItems select_item = null;
        static bool Draging = false;

        public RichTabItems()
        {
            PreviewMouseLeftButtonDown += TabItem_PreviewMouseLeftButtonDown;
            MouseLeftButtonUp += TabItem_MouseLeftButtonUp;
            MouseEnter += TabItem_MouseEnter;
        }

        public void RichTabItems_Click(object sender, RoutedEventArgs e)
        {
            RichTabItems item = (sender as Button).TemplatedParent as RichTabItems;
            TabControl parent = item.Parent as TabControl;
            parent.Items.RemoveAt(parent.SelectedIndex);
        }

        #region 处理拖拽互换位置
        private void TabItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Draging = true;
            current_item = sender as RichTabItems;
            TabControl current_parent = current_item.Parent as TabControl;
            current_index = current_parent.Items.IndexOf(current_item);
        }

        private void TabItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (select_index != current_index && select_index != -1 && current_index != -1)
            {
                TabControl current_parent = (sender as RichTabItems).Parent as TabControl;
                if (select_item != null && current_item != null && current_index != -1 && select_index != -1)
                {
                    RichTabItems new_select_item = new RichTabItems()
                    {
                        Header = select_item.Header,
                        Margin = new Thickness(0),
                        Foreground = new SolidColorBrush(Color.FromRgb(255,255,255)),
                        Content = select_item.Content,
                        Style = this.Style
                    };
                    RichTabItems new_current_item = new RichTabItems()
                    {
                        Header = current_item.Header,
                        Margin = new Thickness(0),
                        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                        Content = current_item.Content,
                        Style = this.Style
                    };
                    new_select_item.MouseLeftButtonUp += TabItem_MouseLeftButtonUp;
                    new_current_item.MouseLeftButtonUp += TabItem_MouseLeftButtonUp;
                    new_select_item.PreviewMouseLeftButtonDown += TabItem_PreviewMouseLeftButtonDown;
                    new_current_item.PreviewMouseLeftButtonDown += TabItem_PreviewMouseLeftButtonDown;
                    new_select_item.MouseEnter += TabItem_MouseEnter;
                    new_current_item.MouseEnter += TabItem_MouseEnter;

                    current_parent.Items.RemoveAt(select_index);
                    current_parent.Items.Insert(select_index, new_current_item);
                    current_parent.Items.RemoveAt(current_index);
                    current_parent.Items.Insert(current_index, new_select_item);
                    current_parent.SelectedIndex = select_index;
                }
            }
            Draging = false;
            current_index = -1;
            select_index = -1;
            current_item = null;
            select_item = null;
        }

        private void TabItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Draging)
            {
                RichTabItems current_item = sender as RichTabItems;
                select_item = current_item;
                TabControl current_parent = current_item.Parent as TabControl;
                select_index = current_parent.Items.IndexOf(current_item);
            }
        }
        #endregion
    }
}
