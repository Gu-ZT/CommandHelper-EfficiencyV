using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace cbhk_environment.Generators.ArmorStandGenerator
{
    /// <summary>
    /// ArmorStand.xaml 的交互逻辑
    /// </summary>
    public partial class ArmorStand
    {
        //主页引用
        public static MainWindow cbhk = null;
        public ArmorStand(MainWindow win)
        {
            InitializeComponent();
            cbhk = win;
        }

        //private void Window_Closed(object sender, EventArgs e)
        //{
        //    Application.Current.Resources.MergedDictionaries.Add(HandyTheme);
        //    Application.Current.Resources.MergedDictionaries.Add(HandySkinDefault);
        //    Close();
        //}

        /// <summary>
        /// 最小化窗体
        /// </summary>
        private void MinFormSize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 鼠标拖拽窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            Point title_range = e.GetPosition(TitleStack);
            if (title_range.X >= 0 && title_range.X < TitleStack.ActualWidth && title_range.Y >= 0 && title_range.Y < TitleStack.ActualHeight && e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// 最大化窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void Window_StateChanged(object sender, EventArgs e)
        //{
        //    switch (WindowState)
        //    {
        //        case WindowState.Maximized:
        //            WindowState = WindowState.Normal;
        //            MaxWidth = SystemParameters.WorkArea.Width + 16;
        //            MaxHeight = SystemParameters.WorkArea.Height + 16;
        //            BorderThickness = new Thickness(5); //最大化后需要调整
        //            Margin = new Thickness(0);
        //            break;
        //        case WindowState.Normal:
        //            WindowState = WindowState.Maximized;
        //            BorderThickness = new Thickness(5);
        //            Margin = new Thickness(10);
        //            break;
        //    }
        //}

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            StackPanel this_Panel = null;
            if (Equals(typeof(StackPanel), e.Source.GetType()))
                this_Panel = e.Source as StackPanel;
            else
                return;
            if (e.ClickCount == 2 && this_Panel.Name == "TitleStack")
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }
    }
}
