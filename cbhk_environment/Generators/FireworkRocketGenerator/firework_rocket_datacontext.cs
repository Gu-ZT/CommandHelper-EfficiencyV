using cbhk_environment.CustomControls;
using cbhk_environment.CustomControls.ColorPickers;
using cbhk_environment.GenerateResultDisplayer;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Windows.System.RemoteSystems;

namespace cbhk_environment.Generators.FireworkRocketGenerator
{
    public class firework_rocket_datacontext: ObservableObject
    {

        #region 返回和运行指令
        public RelayCommand<CommonWindow> ReturnCommand { get; set; }
        public RelayCommand RunCommand { get; set; }
        #endregion

        #region 覆盖生成
        private bool overLying;
        public bool OverLying
        {
            get { return overLying; }
            set
            {
                overLying = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 版本
        private string selectedVersion = "";
        public string SelectedVersion
        {
            get
            {
                return selectedVersion;
            }
            set
            {
                selectedVersion = value;
                OnPropertyChanged();
            }
        }

        //数据源
        private ObservableCollection<string> VersionSource = new ObservableCollection<string> { "1.12-","1.13+" };
        #endregion

        #region 生成行为
        bool behavior_lock = true;
        private bool summon = true;
        public bool Summon
        {
            get { return summon; }
            set
            {
                summon = value;
                if (behavior_lock)
                {
                    behavior_lock = false;
                    Give = !Summon;
                    behavior_lock = true;
                }
                OnPropertyChanged();
            }
        }
        private bool give = false;
        public bool Give
        {
            get { return give; }
            set
            {
                give = value;
                if (behavior_lock)
                {
                    behavior_lock = false;
                    Summon = !give;
                    behavior_lock = true;
                }
                OnPropertyChanged();
            }
        }
        #endregion

        #region 形状
        private List<bool> firework_shape = new List<bool> { };
        public List<bool> FireworkShape
        {
            get { return firework_shape; }
            set
            {
                firework_shape = value; 
                OnPropertyChanged();
            }
        }
        private string FireworkShapeString
        {
            get
            {
                string result;
                int shape_index = 0;
                bool have_shape = false;
                for (int i = 0; i < FireworkShape.Count; i++)
                {
                    if (FireworkShape[i])
                    {
                        have_shape = true;
                        shape_index = i;
                        break;
                    }
                }
                if (!have_shape)
                    shape_index = 0;
                result = "Type:"+shape_index+",";
                return result;
            }
        }
        #endregion

        #region 轨迹
        private List<bool> firework_trajectory = new List<bool> { };
        public List<bool> FireworkTrajectory
        {
            get { return firework_trajectory; }
            set
            {
                firework_trajectory = value;
                OnPropertyChanged();
            }
        }
        private string FireworkTrajectoryString
        {
            get
            {
                string result = "";
                for (int i = 0; i < Trajectories.Count; i++)
                {
                    if (FireworkTrajectory[i])
                    {
                        result += Trajectories.ElementAt(i).Value + ":1,";
                    }
                }
                return result;
            }
        }
        #endregion

        #region 时长
        //短
        private bool short_duration = true;
        public bool ShortDuration
        {
            get { return short_duration; }
            set
            {
                short_duration = value;
                if (ShortDuration)
                {
                    MediumDuration = false;
                    LongDuration = false;
                }
                OnPropertyChanged();
            }
        }
        //中
        private bool medium_duration = false;
        public bool MediumDuration
        {
            get { return medium_duration; }
            set
            {
                medium_duration = value;
                if(MediumDuration)
                {
                    ShortDuration = false;
                    LongDuration = false;
                }
                OnPropertyChanged();
            }
        }
        //长
        private bool long_duration = false;
        public bool LongDuration
        {
            get { return long_duration; }
            set
            {
                long_duration = value;
                if(LongDuration)
                {
                    ShortDuration = false;
                    MediumDuration = false;
                }
                OnPropertyChanged();
            }
        }

        private string FireworkDurationString
        {
            get
            {
                string result = "";
                int duration_type;
                duration_type = ShortDuration ?1:1;
                duration_type = MediumDuration ? 2 : 1;
                duration_type = LongDuration ? 3 : 1;
                result = "Flight:"+ duration_type + ",";
                return result;
            }
        }
        #endregion

        #region 主颜色
        private string MainColorsString
        {
            get
            {
                string result = "Colors:[I;";
                foreach (string item in MainColors)
                {
                    result += item + ",";
                }
                result = result.Trim() != "Colors:[I;" ? result.TrimEnd(',')+"]," :"";
                return result;
            }
        }
        #endregion

        #region 备选颜色
        private string FadeColorsString
        {
            get
            {
                string result = "FadeColors:[I;";
                foreach (string item in FadeColors)
                {
                    result += item + ",";
                }
                result = result.Trim() != "FadeColors:[I;" ? result.TrimEnd(',') + "]," : "";
                return result;
            }
        }
        #endregion

        #region 按角度飞出
        private string FlyAngleString
        {
            get
            {
                string result;
                result = FlyAngle ? "ShotAtAngle:true," :"";
                return result;
            }
        }
        #endregion

        #region 清空主颜色和备选颜色
        public RelayCommand ClearColors { get; set; }
        public RelayCommand ClearFadeColors { get; set; }
        #endregion

        #region 全选等指令
        public RelayCommand<TextToggleButtons> SelectAll { get; set; }
        public RelayCommand<TextToggleButtons> ReverseAll { get; set; }
        public RelayCommand AddToColors { get; set; }
        public RelayCommand AddToFadeColors { get; set; }
        #endregion

        //主颜色库
        private List<string> MainColors = new List<string> { };
        //备选颜色库
        private List<string> FadeColors = new List<string> { };
        //结构颜色选择面板引用
        public StackPanel StructColorsPanel = new StackPanel { };
        //主颜色面板引用
        public StackPanel ColorStackPanel = null;
        //备用颜色面板引用
        public StackPanel FadeColorStackPanel = null;
        //自定义颜色选择器引用
        public ColorPickers colorPickers = null;
        //获取画布引用
        public Canvas EffectCanvas = null;
        //按角度飞出
        public bool FlyAngle { get; set; }
        //保存成员样式
        ColorCheckBoxs color_box = null;

        // 颜色库
        Dictionary<string, string> StructColors = new Dictionary<string, string> { };
        //轨迹库
        Dictionary<string, string> Trajectories = new Dictionary<string, string> { };

        //本生成器的图标路径
        string icon_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\FireworkRocket\\images\\icon.png";

        public firework_rocket_datacontext()
        {
            #region 连接指令
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            RunCommand = new RelayCommand(run_command);
            SelectAll = new RelayCommand<TextToggleButtons>(SelectAllCommand);
            ReverseAll = new RelayCommand<TextToggleButtons>(ReverseAllCommand);
            AddToColors = new RelayCommand(AddToColorsCommand);
            AddToFadeColors = new RelayCommand(AddToFadeColorsCommand);
            ClearColors = new RelayCommand(ClearColorsCommand);
            ClearFadeColors = new RelayCommand(ClearFadeColorsCommand);
            #endregion
        }

        /// <summary>
        /// 选择所有颜色
        /// </summary>
        private void SelectAllCommand(TextToggleButtons buttons)
        {
            bool current_value = buttons.IsChecked.Value;
            foreach (ColorCheckBoxs box in StructColorsPanel.Children)
                box.IsChecked = current_value;
        }

        /// <summary>
        /// 反选所有颜色
        /// </summary>
        private void ReverseAllCommand(TextToggleButtons buttons)
        {
            bool current_value = buttons.IsChecked.Value;
            foreach (ColorCheckBoxs box in StructColorsPanel.Children)
                box.IsChecked = !current_value;
        }

        /// <summary>
        /// 添加到主颜色
        /// </summary>
        private void AddToColorsCommand()
        {
            int select_count = 0;
            foreach (ColorCheckBoxs box in StructColorsPanel.Children)
            {
                if (box.IsChecked.Value)
                {
                    ColorCheckBoxs colorCheckBoxs = new ColorCheckBoxs()
                    {
                        HeaderHeight = 20,
                        HeaderWidth = 20,
                        ContentColor = box.ContentColor,
                        Style = box.Style,
                        IsChecked = true
                    };
                    MainColors.Add((int)Convert.ToInt64(box.ContentColor.ToString().Remove(0, 1), 16)+"");
                    colorCheckBoxs.Click += AddedColorClick;
                    ColorStackPanel.Children.Add(colorCheckBoxs);
                    select_count++;
                }
            }
            if(select_count == 0)
            {
                ColorCheckBoxs colorCheckBoxs = new ColorCheckBoxs()
                {
                    HeaderHeight = 20,
                    HeaderWidth = 20,
                    ContentColor = colorPickers.SelectColor,
                    Style = color_box.Style,
                    IsChecked = true
                };
                MainColors.Add((int)Convert.ToInt64(colorPickers.SelectColor.ToString().Remove(0, 1), 16) + "");
                colorCheckBoxs.Click += AddedColorClick;
                ColorStackPanel.Children.Add(colorCheckBoxs);
            }
        }

        /// <summary>
        /// 添加到备选颜色
        /// </summary>
        private void AddToFadeColorsCommand()
        {
            int select_count = 0;
            foreach (ColorCheckBoxs box in StructColorsPanel.Children)
            {
                if (box.IsChecked.Value)
                {
                    ColorCheckBoxs colorCheckBoxs = new ColorCheckBoxs()
                    {
                        HeaderHeight = 20,
                        HeaderWidth = 20,
                        ContentColor = box.ContentColor,
                        Style = box.Style,
                        IsChecked = true
                    };
                    FadeColors.Add((int)Convert.ToInt64(box.ContentColor.ToString().Remove(0, 1), 16) + "");
                    colorCheckBoxs.Click += AddedColorClick;
                    FadeColorStackPanel.Children.Add(colorCheckBoxs);
                    select_count++;
                }
            }
            if (select_count == 0)
            {
                ColorCheckBoxs colorCheckBoxs = new ColorCheckBoxs()
                {
                    HeaderHeight = 20,
                    HeaderWidth = 20,
                    ContentColor = colorPickers.SelectColor,
                    Style = color_box.Style,
                    IsChecked = true
                };
                FadeColors.Add((int)Convert.ToInt64(colorPickers.SelectColor.ToString().Remove(0, 1), 16) + "");
                colorCheckBoxs.Click += AddedColorClick;
                FadeColorStackPanel.Children.Add(colorCheckBoxs);
            }
        }

        /// <summary>
        /// 清空主颜色
        /// </summary>
        private void ClearColorsCommand()
        {
            ColorStackPanel.Children.Clear();
        }

        /// <summary>
        /// 清空备选颜色
        /// </summary>
        private void ClearFadeColorsCommand()
        {
            FadeColorStackPanel.Children.Clear();
        }

        public void VersionLoaded(object sender,RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.ItemsSource = VersionSource;
        }

        /// <summary>
        /// 载入结构颜色数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StructColorsLoaded(object sender, RoutedEventArgs e)
        {
            StructColorsPanel = sender as StackPanel;
            color_box = StructColorsPanel.Children[0] as ColorCheckBoxs;
            StructColorsPanel.Children.Clear();
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+ "resources\\configs\\struct_colors.ini"))
            {
                string[] colors = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\struct_colors.ini");
                for (int i = 0; i < colors.Length; i++)
                {
                    string[] key_value = colors[i].Split('=');
                    if (!StructColors.ContainsKey(key_value[0]))
                        StructColors.Add(key_value[0], key_value[1]);
                    ColorCheckBoxs colorCheckBoxs = new ColorCheckBoxs()
                    {
                        HeaderHeight = 20,
                        HeaderWidth = 20,
                        Style = color_box.Style,
                        ContentColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(key_value[1]))
                    };
                    StructColorsPanel.Children.Add(colorCheckBoxs);
                }
            }
        }

        /// <summary>
        /// 获取主颜色面板引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ColorsPanelLoaded(object sender, RoutedEventArgs e)
        {
            ColorStackPanel = sender as StackPanel;
        }

        /// <summary>
        /// 获取备用颜色面板引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FadeColorsPanelLoaded(object sender, RoutedEventArgs e)
        {
            FadeColorStackPanel = sender as StackPanel;
        }

        /// <summary>
        /// 获取自定义颜色选择器引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CustomColorSelectorLoaded(object sender, RoutedEventArgs e)
        {
            colorPickers = sender as ColorPickers;
        }

        /// <summary>
        /// 获取效果显示画布引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EffectCanvasLoaded(object sender, RoutedEventArgs e)
        {
            EffectCanvas = sender as Canvas;
        }

        /// <summary>
        /// 获取所有形状所在栈的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ShapeStackPanelLoaded(object sender, RoutedEventArgs e)
        {
            StackPanel shapes = sender as StackPanel;
            TextToggleButtons shape_item = shapes.Children[0] as TextToggleButtons;
            shapes.Children.Clear();
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+ "resources\\configs\\FireworkRocket\\data\\shapes.ini"))
            {
                List<bool> shape_value = new List<bool> { };
                string[] shape_configs = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\FireworkRocket\\data\\shapes.ini");
                for (int i = 0; i < shape_configs.Length; i++)
                {
                    if (i == 0)
                        shape_value.Add(true);
                    else
                        shape_value.Add(false);
                    Binding binding = new Binding()
                    {
                        Path = new PropertyPath("FireworkShape[" + i + "]"),
                        Mode = BindingMode.TwoWay
                    };
                    TextToggleButtons textToggleButtons = new TextToggleButtons()
                    {
                        Content = shape_configs[i],
                        Cursor = Cursors.Hand,
                        Style = shape_item.Style,
                        Foreground = shape_item.Foreground,
                        Background = shape_item.Background,
                        VerticalContentAlignment = shape_item.VerticalContentAlignment,
                        SelectedBackground = shape_item.SelectedBackground
                    };
                    textToggleButtons.Click += ShapeItemClick;
                    shapes.Children.Add(textToggleButtons);
                    BindingOperations.SetBinding(textToggleButtons, TextToggleButtons.IsCheckedProperty, binding);
                }
                FireworkShape = shape_value;
            }
        }

        /// <summary>
        /// 选取形状点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShapeItemClick(object sender, RoutedEventArgs e)
        {
            TextToggleButtons textToggleButtons = sender as TextToggleButtons;
            StackPanel parent = textToggleButtons.Parent as StackPanel;
            int current_index = 0;
            if(textToggleButtons.IsChecked.Value)
            {
                current_index = parent.Children.IndexOf(textToggleButtons);
                for (int i = 0; i < parent.Children.Count; i++)
                {
                    if(i != current_index)
                    {
                        (parent.Children[i] as TextToggleButtons).IsChecked = false;
                    }
                }
            }
        }

        /// <summary>
        /// 获取轨迹所在栈的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TrajectoriesStackPanelLoaded(object sender, RoutedEventArgs e)
        {
            StackPanel trajectories = sender as StackPanel;
            TextToggleButtons shape_trajectory = trajectories.Children[0] as TextToggleButtons;
            trajectories.Children.Clear();
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\FireworkRocket\\data\\trajectories.ini"))
            {
                List<bool> trajectory = new List<bool> { };
                string[] shape_configs = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\FireworkRocket\\data\\trajectories.ini");
                for (int i = 0; i < shape_configs.Length; i++)
                {
                    trajectory.Add(false);
                    string[] trajectory_info = shape_configs[i].Split('=');
                    Trajectories.Add(trajectory_info[0], trajectory_info[1]);
                    Binding binding = new Binding()
                    {
                        Path = new PropertyPath("FireworkTrajectory[" + i + "]"),
                        Mode = BindingMode.TwoWay
                    };
                    TextToggleButtons textToggleButtons = new TextToggleButtons()
                    {
                        Content = trajectory_info[0],
                        Cursor = Cursors.Hand,
                        Style = shape_trajectory.Style,
                        Foreground = shape_trajectory.Foreground,
                        Background = shape_trajectory.Background,
                        VerticalContentAlignment = shape_trajectory.VerticalContentAlignment,
                        SelectedBackground = shape_trajectory.SelectedBackground
                    };
                    trajectories.Children.Add(textToggleButtons);
                    BindingOperations.SetBinding(textToggleButtons, TextToggleButtons.IsCheckedProperty, binding);
                }
                FireworkTrajectory = trajectory;
            }
        }

        /// <summary>
        /// 被添加的颜色点击事件
        /// </summary>
        private void AddedColorClick(object sender, RoutedEventArgs e)
        {
            ColorCheckBoxs box = sender as ColorCheckBoxs;
            if(box.Parent == ColorStackPanel)
            {
                int current_index = MainColors.IndexOf((int)Convert.ToInt64(box.ContentColor.ToString().Remove(0, 1), 16) + "");
                MainColors.RemoveAt(current_index);
            }
            else
            if (box.Parent == FadeColorStackPanel)
            {
                int current_index = FadeColors.IndexOf((int)Convert.ToInt64(box.ContentColor.ToString().Remove(0, 1), 16) + "");
                FadeColors.RemoveAt(current_index);
            }
            StackPanel parent = box.Parent as StackPanel;
            parent.Children.Remove(box);
        }

        /// <summary>
        /// 颜色成员点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void SelectColorClick(object sender, RoutedEventArgs e)
        //{
        //    ColorCheckBoxs box = sender as ColorCheckBoxs;
        //    if (box.IsChecked.Value)
        //        Colors.Add(box.ContentColor.ToString());
        //    else
        //    {
        //        if(Equals(box.Parent, ColorStackPanel) || Equals(box.Parent,FadeColorStackPanel))
        //        {
        //            StackPanel parent = box.Parent as StackPanel;
        //            parent.Children.Remove(box);
        //        }
        //        Colors.Remove(box.ContentColor.ToString());
        //    }
        //}

        private void return_command(CommonWindow win)
        {
            FireworkRocket.cbhk.Topmost = true;
            FireworkRocket.cbhk.WindowState = WindowState.Normal;
            FireworkRocket.cbhk.Show();
            FireworkRocket.cbhk.Topmost = false;
            FireworkRocket.cbhk.ShowInTaskbar = true;
            win.Close();
        }
        private void run_command()
        {
            string result = "";
            if (Give)
            {
                result = "{Explosions:[{";
                result += FireworkShapeString + FireworkTrajectoryString + MainColorsString + FadeColorsString;
                result = result.TrimEnd(',') + "}],";
                result += FireworkDurationString.TrimEnd(',') + "}";
                result = "give @p firework_rocket{Fireworks:" + result + "}";
            }
            else
            if(Summon)
            {
                result = FireworkShapeString + FireworkTrajectoryString + MainColorsString + FadeColorsString;
                result = "FireworksItem:{id:firework_rocket,Count:1b,tag:{Fireworks:{Explosions:[{" + result.TrimEnd(',') + "}],";
                result = "summon firework_rocket ~ ~ ~ {LifeTime:20," + FlyAngleString + result + FireworkDurationString.TrimEnd(',') + "}}}}";
            }

            Displayer displayer = Displayer.GetContentDisplayer();
            displayer.GeneratorResult(OverLying, new string[] { result }, new string[] { "" }, new string[] { icon_path }, new System.Windows.Media.Media3D.Vector3D() { X = 30, Y = 30 });
            displayer.Show();
        }

        /// <summary>
        /// 处理图片
        /// </summary>
        //private void HandleImage(int effectWidth)
        //{
        //    if (map == null) return;
        //    this.mainCanvas.Children.Clear();
        //    List<Particle> particleList = MosaicHelper.AdjustTobMosaic(map, effectWidth);
        //    foreach (var pd in particleList)
        //    {
        //        var ep = new Ellipse
        //        {
        //            Width = pd.Size,
        //            Height = pd.Size,
        //            Fill = new SolidColorBrush(Colors.Black),
        //        };
        //        Canvas.SetTop(ep, pd.Position.Y);
        //        Canvas.SetLeft(ep, pd.Position.X);
        //        this.mainCanvas.Children.Add(ep);
        //    }
        //}
    }
}
