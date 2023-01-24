using System;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Diagnostics;
using cbhk_signin.resources.tools.classes;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace cbhk_signin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// 是否放行
        /// </summary>
        private bool through;

        /// <summary>
        /// 保存接口信息中的用户名
        /// </summary>
        private string user_name_string;
        /// <summary>
        /// 保存用户ID
        /// </summary>
        private string user_id;
        /// <summary>
        /// 保存用户的在mc中的id
        /// </summary>
        private string mc_id_of_user;
        public MainWindow()
        {
            InitializeComponent();
            //隐藏托盘
            //Hardcodet.Wpf.TaskbarNotification.TaskbarIcon taskbar_icon = FindResource("cbhk_taskbar") as Hardcodet.Wpf.TaskbarNotification.TaskbarIcon;
            //taskbar_icon.Visibility = Visibility.Collapsed;
            ////登录功能
            //signin_btn.Click += sign_in_command;
            //save_user_pwd.Checked += save_user_pwd_Checked;
            //ClickToWebSite.Click += forgot_pwd_command;
            ////初始化用户设置
            //ReadUserSettings();

            #region 调试
            ShowInTaskbar = false;
            WindowState = WindowState.Minimized;
            Opacity = 0;
            cbhk_environment.MainWindow CBHK = new cbhk_environment.MainWindow(StatsUserInfomation());
            CBHK.Topmost = true;
            CBHK.Show();
            CBHK.Topmost = false;
            #endregion
        }

        #region 用户服务
        private void ReadUserSettings()
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs");
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs\\user_info.ini"))
            {
                string[] user_info = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs\\user_info.ini");
                if (user_info != null)
                {
                    user_name_box.Text = user_info.Length > 1 ? user_info[0] : "";
                    user_pwd_box.Password = user_info.Length == 2 ? user_info[1] : "";
                    save_user_name.IsChecked = user_name_box.Text != "";
                    save_user_pwd.IsChecked = user_pwd_box.Password != "";
                }
            }

        }

        /// <summary>
        /// 统计用户信息
        /// </summary>
        private Dictionary<string,string> StatsUserInfomation()
        {
            Dictionary<string, string> user_information = new Dictionary<string, string> { };
            user_information.Add("user_frame", user_frame.ImageSource.ToString());
            user_information.Add("user_name",user_name_string);
            user_information.Add("user_id", user_id);
            user_information.Add("mc_id", mc_id_of_user);
            return user_information;
        }

        /// <summary>
        /// 保存用户信息
        /// </summary>
        /// <returns></returns>
        private JObject SaveUserInfo(JObject result)
        {
            user_name_string = result["data"]["name"].ToString();
            user_id = result["data"]["id"].ToString();
            mc_id_of_user = result["data"]["mc_id"].ToString();
            return result;
        }

        /// <summary>
        /// 替换正则无法识别的特殊字符
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string uriencode(string url)
        {
            return url.Replace(" ", "%20").Replace("\"", "%22").Replace("#", "%23").Replace("%", "%25").Replace("&", "%26").Replace("(", "%28").Replace(")", "%29").Replace("+", "%2B").Replace(",", "%2C").Replace("/", "%2F").Replace(":", "%3A").Replace(";", "%3B").Replace("<", "%3C").Replace("=", "%3D").Replace(">", "%3E").Replace("?", "%3F").Replace("@", "%40").Replace("\\", "%5C").Replace("|", "%7C");
        }

        /// <summary>
        /// 登录
        /// </summary>
        private async void sign_in_command(object sender , RoutedEventArgs e)
        {
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs");
            if (user_name_box.Text.Trim() == "")
            {
                result_display.Text = "账号不存在";
                user_name_box.Text = "";
                return;
            }
            if (user_pwd_box.Password.Trim() == "")
            {
                result_display.Text = "密码不能为空";
                user_pwd_box.Password = "";
                return;
            }
            result_display.Text = "登录中...";
            //立刻处理Window消息
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

            #region 保存账号和密码
            if (save_user_name.IsChecked.Value || save_user_pwd.IsChecked.Value)
            {
                string user_info = "";
                if (save_user_name.IsChecked.Value)
                    user_info += user_name_box.Text + "\r\n";
                if (save_user_pwd.IsChecked.Value)
                    user_info += Regex.Escape(user_pwd_box.Password) + "\r\n";
                byte[] user_pwd_bytes = Encoding.UTF8.GetBytes(user_info);
                if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs\\user_info.ini"))
                    File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs\\user_info.ini").Close();

                using (FileStream name_pwd_stream = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "resources\\signin_configs\\user_info.ini", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    name_pwd_stream.Write(user_pwd_bytes, 0, user_pwd_bytes.Length);
                }
            }
            #endregion

            #region 进行登录
            JObject result = new JObject();
            try
            {
                result = JsonConvert.DeserializeObject(signin.GetDataByPost("https://mc.metamo.cn/api/user/OAuth/login", uriencode(Regex.Match(user_name_box.Text, "(.*)").ToString()), uriencode(Regex.Match(user_pwd_box.Password, "(.*)").ToString()))) as JObject;
            }
            catch
            {
            }
            if (result["code"].ToString() == "200")
            {
                if (bool.Parse(result["data"]["cbhk_buy"].ToString()))
                {
                    result_display.Text = "登录成功!,欢迎" + result["data"]["name"];
                    //立刻处理Window消息
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));

                    if (result["data"]["avatar"].ToString() != null && result["data"]["avatar"].ToString().Contains("?") && !File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\user_head.png"))
                        await Task.Run(() => { signin.DownLoadUserHead(result["data"]["avatar"].ToString(), AppDomain.CurrentDomain.BaseDirectory + "resources\\user_head.png"); });
                    if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\user_head.png"))
                    user_frame.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\user_head.png",UriKind.Absolute));
                    else
                    if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\command_block.png"))
                        user_frame.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\command_block.png", UriKind.Absolute));
                    //立刻处理Window消息
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    Thread.Sleep(1000);
                    ShowInTaskbar = false;
                    WindowState = WindowState.Minimized;
                    Opacity = 0;
                    Visibility = Visibility.Collapsed;
                    WindowState = WindowState.Minimized;

                    SaveUserInfo(result);
                    cbhk_environment.MainWindow CBHK = new cbhk_environment.MainWindow(StatsUserInfomation());

                    #region 显示管家主窗体
                    CBHK.ShowInTaskbar = true;
                    CBHK.Opacity = 1.0;
                    CBHK.WindowState = WindowState.Normal;
                    CBHK.Topmost = true;
                    CBHK.Show();
                    CBHK.Topmost = false;
                    #endregion
                }
                else
                {
                    result_display.Text = "还未购买命令管家...";
                    //立刻处理Window消息
                    Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
                    Thread.Sleep(1000);
                }
            }
            else
                result_display.Text = result["message"].ToString();
            #endregion
        }

        /// <summary>
        /// 自动登录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void save_user_pwd_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox.IsChecked.Value)
                save_user_name.IsChecked = true;
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        public void forgot_pwd_command(object sender,EventArgs e)
        {
            Process.Start("https://mc.metamo.cn/u/login/");
        }
        #endregion

        #region 窗体行为
        /// <summary>
        /// 最小化窗体
        /// </summary>
        private void MinFormSize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 最大化窗体
        /// </summary>
        private void MaxFormSize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        private void CloseForm(object sender, RoutedEventArgs e)
        {
            Close();
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
        /// 窗体尺寸更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_StateChanged(object sender, EventArgs e)
        {

            switch (WindowState)
            {
                case WindowState.Maximized:
                    //MaxWidth = SystemParameters.WorkArea.Width + 16;
                    //MaxHeight = SystemParameters.WorkArea.Height + 16;
                    //BorderThickness = new Thickness(5); //最大化后需要调整
                    Left = Top = 0;
                    MaxHeight = SystemParameters.WorkArea.Height;
                    MaxWidth = SystemParameters.WorkArea.Width;
                    break;
                case WindowState.Normal:
                    BorderThickness = new Thickness(0);
                    break;
            }
        }
        #endregion

        private void save_user_pwd_Loaded(object sender, RoutedEventArgs e)
        {
            //自动登录
            if (save_user_pwd.IsChecked.Value)
            {
                Thread.Sleep(1000);
                signin_btn.IsEnabled = false;
                sign_in_command(null, null);
            }
        }

        private void user_name_box_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            sign_in_command(null,null);
        }

        private void user_pwd_box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                sign_in_command(null, null);
        }
    }
}
