using cbhk_environment.CustomControls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace cbhk_environment.GenerateResultDisplayer
{
    /// <summary>
    /// Displayer.xaml 的交互逻辑
    /// </summary>
    public partial class Displayer
    {
        /// <summary>
        /// 逻辑锁
        /// </summary>
        //private static bool NeedLock = false;

        /// <summary>
        /// 进程锁
        /// </summary>
        private static object obj = new object();

        private Style TabItemStyle = null;

        private Style ScrollViewerStyle = null;

        /// <summary>
        /// 单例模式,用于显示生成结果
        /// </summary>
        private Displayer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 单例,避免指令重排
        /// </summary>
        private volatile static Displayer content_displayer;

        public static Displayer GetContentDisplayer()
        {
            if (content_displayer == null)
            {
                lock (obj)
                {
                    if (content_displayer == null)
                    {
                        content_displayer = new Displayer();
                        int tab_count = content_displayer.ResultTabControl.Items.Count;
                        IconTabItems iconTabItems = content_displayer.ResultTabControl.Items[tab_count - 1] as IconTabItems;
                        content_displayer.TabItemStyle = iconTabItems.Style;
                        content_displayer.ScrollViewerStyle = content_displayer.item_scrollviewer.Style;
                        content_displayer.ResultTabControl.Items.Clear();
                    }
                }
            }
            return content_displayer;
        }

        /// <summary>
        /// 生成结果
        /// </summary>
        /// <param name="Overlying">是否覆盖</param>
        /// <param name="spawn_result">数据集</param>
        /// <param name="header_text">数据头</param>
        /// <param name="head_image_pathes">所表示的生成器图标</param>
        public void GeneratorResult(bool Overlying,string[] spawn_result, string[] header_text, string[] head_image_pathes,Vector3D ImageSize)
        {
            for (int i = 0; i < spawn_result.Length; i++)
            {
                bool have_data = false;
                if (Overlying)
                    foreach (IconTabItems item in content_displayer.ResultTabControl.Items)
                    {
                        if (item.HeaderText == header_text[i])
                        {
                            have_data = true;
                            TextBox box = (item.Content as ScrollViewer).Content as TextBox;
                            box.Text = spawn_result[i];
                        }
                    }
                if (have_data)
                    continue;
                IconTabItems itt = new IconTabItems()
                {
                    ImageWidth = ImageSize.X,
                    ImageHeight = ImageSize.Y,
                    TextMargin = new Thickness(0, 30, 0, 0),
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    HeaderText = header_text[i],
                    Style = content_displayer.TabItemStyle,
                    HeaderImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(head_image_pathes[i], UriKind.RelativeOrAbsolute)),
                };
                TextBox result_box = new TextBox()
                {
                    Text = spawn_result[i],
                    Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                    Background = null,
                    FontSize = 15,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    CaretBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255))
                };

                ScrollViewer scrollViewer = new ScrollViewer();

                scrollViewer.Style = content_displayer.ScrollViewerStyle;
                scrollViewer.Content = result_box;
                itt.Content = scrollViewer;
                content_displayer.ResultTabControl.Items.Add(itt);
                content_displayer.ResultTabControl.SelectedItem = itt;
            }
        }

        #region 窗体行为
        /// <summary>
        /// 隐藏单例窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Hide();
        }

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
        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    WindowState = WindowState.Normal;
                    MaxWidth = SystemParameters.WorkArea.Width + 16;
                    MaxHeight = SystemParameters.WorkArea.Height + 16;
                    BorderThickness = new Thickness(5); //最大化后需要调整
                    Margin = new Thickness(0);
                    break;
                case WindowState.Normal:
                    WindowState = WindowState.Maximized;
                    BorderThickness = new Thickness(5);
                    Margin = new Thickness(10);
                    break;
            }

            //switch (WindowState)
            //{
            //    case WindowState.Maximized:
            //        //MaxWidth = SystemParameters.WorkArea.Width + 16;
            //        //MaxHeight = SystemParameters.WorkArea.Height + 16;
            //        //BorderThickness = new Thickness(5); //最大化后需要调整
            //        Left = Top = 0;
            //        MaxHeight = SystemParameters.WorkArea.Height;
            //        MaxWidth = SystemParameters.WorkArea.Width;
            //        break;
            //    case WindowState.Normal:
            //        BorderThickness = new Thickness(0);
            //        break;
            //}
        }

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
        #endregion
    }
}
