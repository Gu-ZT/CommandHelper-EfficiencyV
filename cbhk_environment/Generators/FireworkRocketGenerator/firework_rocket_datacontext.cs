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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

        #region 已选择的形状
        private string selectedShape = "";
        public string SelectedShape
        {
            get
            {
                return selectedShape;
            }
            set
            {
                selectedShape = value;
                OnPropertyChanged();
            }
        }
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
        //原版颜色库路径
        string colorStoragePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\FireworkRocket\\images";

        public firework_rocket_datacontext()
        {
            #region 连接指令
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            RunCommand = new RelayCommand(run_command);
            //SelectAll = new RelayCommand<TextToggleButtons>(SelectAllCommand);
            //ReverseAll = new RelayCommand<TextToggleButtons>(ReverseAllCommand);
            //AddToColors = new RelayCommand(AddToColorsCommand);
            //AddToFadeColors = new RelayCommand(AddToFadeColorsCommand);
            //ClearColors = new RelayCommand(ClearColorsCommand);
            //ClearFadeColors = new RelayCommand(ClearFadeColorsCommand);
            #endregion
        }

        /// <summary>
        /// 载入原版颜色库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StructureColorList_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            UniformGrid uniformGrid = scrollViewer.Content as UniformGrid;

            string[] colorArray = Directory.GetFiles(colorStoragePath);
            foreach (var item in colorArray)
            {
                if(item.Contains("dye"))
                {
                    string colorName = item.Substring(0, item.LastIndexOf('_'));
                    System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(item,UriKind.Absolute));
                    IconCheckBoxs iconCheckBoxs = new IconCheckBoxs
                    {
                        ContentImage = bitmapImage,
                        HeaderHeight = 25,
                        HeaderWidth = 25,
                        SnapsToDevicePixels = true,
                        UseLayoutRounding = true,
                        ToolTip = Path.GetFileNameWithoutExtension(colorName).Replace("_dye", ""),
                        Tag = colorName,
                        Style = Application.Current.Resources["IconCheckBox"] as Style
                    };
                    iconCheckBoxs.Checked += StructureColorChecked;
                    iconCheckBoxs.Unchecked += StructureColorUnChecked;
                    RenderOptions.SetBitmapScalingMode(iconCheckBoxs,BitmapScalingMode.NearestNeighbor);
                    RenderOptions.SetClearTypeHint(iconCheckBoxs,ClearTypeHint.Enabled);
                    ToolTipService.SetShowDuration(iconCheckBoxs,1000);
                    ToolTipService.SetInitialShowDelay(iconCheckBoxs,0);
                    uniformGrid.Children.Add(iconCheckBoxs);
                }
            }
        }

        /// <summary>
        /// 已取消选择结构色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StructureColorUnChecked(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 已选择结构色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StructureColorChecked(object sender, RoutedEventArgs e)
        {

        }

        public void VersionLoaded(object sender,RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.ItemsSource = VersionSource;
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
