using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using System.Windows.Media.Imaging;
using System.IO;
using System.Linq;

namespace cbhk_environment.SettingForm
{
    /// <summary>
    /// LinkForm.xaml 的交互逻辑
    /// </summary>
    public partial class LinkForm : Window
    { 
        /// <summary>
        /// 图像控件的容器
        /// </summary>
        Grid grid = new Grid();

        /// <summary>
        /// 需要显示的图像所在的窗体
        /// </summary>
        Window display_image = new Window()
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            SizeToContent = SizeToContent.WidthAndHeight,
            Topmost = true,
            WindowStyle = WindowStyle.None
        };

        /// <summary>
        /// 图像显示载体
        /// </summary>
        Image image = new Image()
        {
            MaxHeight = 250,
            MaxWidth = 250,
            Width = 250,
            Height = 250
        };

        /// <summary>
        /// 创建图像载体
        /// </summary>
        BitmapImage bitmapImage;

        /// <summary>
        /// 选中的图像是否更新
        /// </summary>
        bool IsSelectedImageUpdated = false;

        public LinkForm()
        {
            InitializeComponent();
            InitLinkData();
            grid.Children.Add(image);
            display_image.Content = grid;
        }

        /// <summary>
        /// 初始化轮播图视图数据
        /// </summary>
        private void InitLinkData()
        {
            foreach (var a_link in MainWindow.CircularBanner.Keys)
            {
                Button link_btn = new Button()
                {
                    Style = Resources["LinkItem"] as Style
                };
                LinkStack.Children.Add(link_btn);
                link_btn.ApplyTemplate();

                //载入数据
                if (File.Exists(a_link) || File.Exists(MainWindow.CircularBanner[a_link]))
                {
                    (link_btn.Template.FindName("LinkPanel", link_btn) as DockPanel).Tag = a_link.ToString() + ";" + MainWindow.CircularBanner[a_link];

                    Button image_btn = (link_btn.Template.FindName("SelectImage", link_btn) as Button);
                    image_btn.Tag = "*"+ a_link.ToString();
                    image_btn.Content = a_link.ToString().Substring(0, 17) + "...";

                    Button url_btn = (link_btn.Template.FindName("SelectUrl", link_btn) as Button);
                    url_btn.Tag = "*" + MainWindow.CircularBanner[a_link];
                    url_btn.Content = MainWindow.CircularBanner[a_link].ToString().Substring(0, 17) + "...";
                    url_btn.ToolTip = MainWindow.CircularBanner[a_link].ToString();
                    ToolTipService.SetBetweenShowDelay(url_btn,0);

                    //通知预览显示事件更新图像源
                    IsSelectedImageUpdated = true;
                }
            }
        }

        /// <summary>
        /// 打开本地目录,选择图像文件
        /// </summary>
        private void ClickToSelectImages(object sender, RoutedEventArgs e)
        {
            Button this_btn = e.Source as Button;
            if (this_btn.Tag == null)
                this_btn.Content = "点击选择图像";
            OpenFileDialog image_selector = new OpenFileDialog()
            {
                AddExtension = false,
                DefaultExt = ".png",
                Filter = "(*.png)|*.png|(*.jpg)|*.jpg|(*.jpeg)|*.jpeg",
                RestoreDirectory = true,
                Multiselect = false,
                Title = "请选择一个图像文件",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };
            if(image_selector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    System.Drawing.Image.FromFile(image_selector.FileName);
                }
                catch {
                    System.Windows.MessageBox.Show("请选择图像文件=-=");
                    return;
                }
                //是否更新了图像
                if (this_btn.Tag == null || image_selector.FileName != this_btn.Tag.ToString())
                    IsSelectedImageUpdated = true;
                else
                    IsSelectedImageUpdated = false;
                //获取选中的所有图像
                string image = image_selector.FileName;
                this_btn.Content = image.Length > 20 ? image.Substring(0, 20) + "..." : image;
                this_btn.Tag = image;
                UpLoadImagePath(this_btn);
            }
        }

        /// <summary>
        /// 显示所选图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplaySelectedImage(object sender, MouseEventArgs e)
        {
            Button this_btn = e.Source as Button;
            if (this_btn.Tag == null) return;
            //包含"@"则为载入的数据而非新建的，给予显示
            if (IsSelectedImageUpdated || this_btn.Tag.ToString().Contains("*"))
            {
                if (!File.Exists(this_btn.Tag.ToString()) && !File.Exists(this_btn.Tag.ToString().Substring(1, this_btn.Tag.ToString().Length - 1)))
                    return;
                //为窗体填充图像
                if (this_btn.Tag.ToString().Contains("*"))
                    bitmapImage = new BitmapImage(new Uri(this_btn.Tag.ToString().Substring(1, this_btn.Tag.ToString().Length - 1), UriKind.Absolute));
                else
                    bitmapImage = new BitmapImage(new Uri(this_btn.Tag.ToString(), UriKind.Absolute));
                image.Source = bitmapImage;
            }
            //获取鼠标当前相对于屏幕的位置
            Point form_p = Mouse.GetPosition(e.Source as FrameworkElement);
            Point screen_point = (e.Source as FrameworkElement).PointToScreen(form_p);
            //设置窗体坐标
            display_image.Left = screen_point.X + 20;
            display_image.Top = screen_point.Y;
            display_image.Show();
        }

        private void HideSelectedImage(object sender, MouseEventArgs e)
        {
            display_image.Hide();
        }

        /// <summary>
        /// 从按钮上传路径数据
        /// </summary>
        /// <param name="data"></param>
        private void UpLoadImagePath(Button data)
        {
            DockPanel parent = data.Parent as DockPanel;
            if (parent.Tag != null)
            {
                string first_part = parent.Tag.ToString().Substring(0, parent.Tag.ToString().IndexOf(";")).Trim();
                if (first_part != "")
                    parent.Tag.ToString().Replace(first_part, data.Tag.ToString());
                else
                    parent.Tag = data.Tag.ToString() + parent.Tag;
            }
            else
                parent.Tag = data.Tag.ToString() + ";";
        }

        /// <summary>
        /// 从按钮上传网址链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpLoadWebUrl(Button data)
        {
            DockPanel parent = data.Parent as DockPanel;
            if (parent.Tag != null)
                parent.Tag += data.Tag.ToString();
            else
                parent.Tag += ";" + data.Tag.ToString();
        }

        /// <summary>
        /// 添加链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLinkButton(object sender, RoutedEventArgs e)
        {
            Button link_btn = new Button()
            {
                Style = Resources["LinkItem"] as Style
            };
            LinkStack.Children.Add(link_btn);
        }

        /// <summary>
        /// 判断目标文件是否为文本文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsTextFile(string path)
        {
            // only check 32 char here, can increase to achieve higher accuracy
            var buf = new char[32];

            try
            {
                using (var reader = new StreamReader(path))
                {
                    int readint = reader.ReadBlock(buf, 0, buf.Length);

                    for (int i = 0; i < readint; i++)
                    {
                        if (buf[i] == '\0')
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
            catch{}
            return false;
        }

        /// <summary>
        /// 设置图像的网址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetWebUrl(object sender, RoutedEventArgs e)
        {
            Button this_btn = e.Source as Button;
            if (this_btn.Tag == null)
                this_btn.Content = "设置网站路径";
            OpenFileDialog url_selector = new OpenFileDialog()
            {
                AddExtension = false,
                DefaultExt = ".png",
                Filter = "(*.txt)|*.txt",
                RestoreDirectory = true,
                Multiselect = true,
                Title = "请选择一个文本文件(含网址链接)",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer)
            };

            if(url_selector.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(!IsTextFile(url_selector.FileName))
                {
                    System.Windows.MessageBox.Show("请选择一个文本文件=-=");
                    return;
                }
                ToolTipService.SetInitialShowDelay(this_btn,0);
                string current_url = File.ReadAllText(url_selector.FileName);
                this_btn.ToolTip = current_url;
                this_btn.Tag = url_selector.FileName;
                UpLoadWebUrl(this_btn);
            }
        }

        /// <summary>
        /// 移除当前链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLink(object sender, RoutedEventArgs e)
        {
            Button this_btn = e.Source as Button;
            LinkStack.Children.Remove(this_btn.TemplatedParent as Button);
        }

        /// <summary>
        /// 清空链接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLinks(object sender, RoutedEventArgs e)
        {
            LinkStack.Children.Clear();
        }

        /// <summary>
        /// 生成链接数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GeneratorLinksData(object sender, RoutedEventArgs e)
        {
            //清空原字典中的所有数据
            MainWindow.CircularBanner.Clear();
            //设置轮播图播放延迟
            MainWindowProperties.LinkAnimationDelay = int.Parse(CircularBannerDelay.Text);
            //遍历栈中成员
            if (LinkStack.Children.Count > 0)
            {
                foreach (Button button in LinkStack.Children)
                {
                    //遍历按钮样版中的按钮，取出所需数据
                    DockPanel parent_panel = button.Template.FindName("LinkPanel", button) as DockPanel;
                    if (parent_panel.Tag == null) continue;
                    string[] OneLinkData = parent_panel.Tag.ToString().Split(';');
                    //添加到词典
                    MainWindow.CircularBanner.Add(OneLinkData[0], OneLinkData[1]);
                }
            }
            DialogResult = true;
        }

        #region 窗体行为
        /// <summary>
        /// 由于不是主窗体，所以关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Close();
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

        /// <summary>
        /// 双击最大化或最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }
        #endregion

        /// <summary>
        /// 移动显示的图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveSelectedImage(object sender, MouseEventArgs e)
        {
            //获取鼠标当前相对于屏幕的位置
            Point form_p = Mouse.GetPosition(e.Source as FrameworkElement);
            Point screen_point = (e.Source as FrameworkElement).PointToScreen(form_p);
            //设置窗体坐标
            display_image.Left = screen_point.X + 20;
            display_image.Top = screen_point.Y;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void CircularBannerDelay_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = System.Text.RegularExpressions.Regex.IsMatch(e.Text,@"[^0-9]+");
        }
    }
}
