using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools;
using cbhk_environment.Generators.ItemGenerator;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSScriptControl;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Services.Maps;
using WK.Libraries.BetterFolderBrowserNS;

namespace cbhk_environment.Generators.TagGenerator
{
    public class tag_datacontext: ObservableObject
    {
        #region 生成与返回
        public RelayCommand RunCommand { get; set; }

        public RelayCommand<CommonWindow> ReturnCommand { get; set; }
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

        #region 替换
        private bool replace;
        public bool Replace
        {
            get { return replace; }
            set
            {
                replace = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 当前文件名
        private string currentFileName = "Tag";
        public string CurrentFileName
        {
            get { return currentFileName; }
            set
            {
                currentFileName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //需要被动态定位的RichCheckBox
        private RichCheckBoxs TargetBox;
        //存储复选框样式
        Style icon_box_style = null;
        //存储最终生成的列表
        List<string> BlocksAndItems = new List<string> { };
        //保存复选框所在父级栈表
        StackPanel checkbox_parent = null;
        //保存搜索框引用
        TextBox search_box = null;
        //保存所有RichCheckBox的键
        List<string> BlockAndItemKeys = new List<string> { };
        //保存所有可视化成员
        List<RichCheckBoxs> VisibleCheckBoxs = new List<RichCheckBoxs> { };
        //已加载类型过滤列表
        bool TypeLoaded = false;
        //异步执行数据读取逻辑
        BackgroundWorker DataSourceReader = new BackgroundWorker();
        //骨架屏所在的容器
        Grid SkeletionGrid = null;
        //模板样式载体
        RichCheckBoxs style_template = null;
        //物品加载标记
        bool ItemLoaded = false;
        //实体加载标记
        bool EntityLoaded = false;

        public tag_datacontext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            #endregion
        }

        /// <summary>
        /// 载入id列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TagStackPanelLoaded(object sender, RoutedEventArgs e)
        {
            checkbox_parent = sender as StackPanel;

            #region 保存复选框样式
            style_template = checkbox_parent.Children[1] as RichCheckBoxs;
            icon_box_style = style_template.Style;
            SkeletionGrid = checkbox_parent.Children[0] as Grid;
            #endregion

            checkbox_parent.Children.RemoveAt(1);
            //开始读取数据
            InitializeBackgroundWorker();
            DataSourceReader.RunWorkerAsync();
        }

        /// <summary>
        /// 初始化加载进程
        /// </summary>
        private void InitializeBackgroundWorker()
        {
            //bool类型，指示DataSourceReader是否可以报告进度更新。当该属性值为True时，将可以成功调用ReportProgress方法
            DataSourceReader.WorkerReportsProgress = true;
            //bool类型，指示DataSourceReader是否支持异步取消操作。当该属性值为True是，将可以成功调用CancelAsync方法
            DataSourceReader.WorkerSupportsCancellation = true;
            //执行RunWorkerAsync方法后触发DoWork，将异步执行backgroundWorker_DoWork方法中的代码
            DataSourceReader.DoWork += new DoWorkEventHandler(ReadDataSource);
            //执行ReportProgress方法后触发ProgressChanged，将执行ProgressChanged方法中的代码
            DataSourceReader.ProgressChanged += InitUIData;//object sender, ProgressChangedEventArgs e
            //异步操作完成或取消时执行的操作，当调用DoWork事件执行完成时触发
            DataSourceReader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DataSourceLoadCompleted);
        }

        /// <summary>
        /// 加载UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitUIData(object sender, ProgressChangedEventArgs e)
        {
            SolidColorBrush white_brush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            BitmapImage bitmapImage = null;
            string image_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + e.UserState.ToString().Split(' ')[0] + ".png";
            if (File.Exists(image_path))
                bitmapImage = new BitmapImage(new Uri(image_path, UriKind.Absolute));
            else
                bitmapImage = new BitmapImage();

            //Style = icon_box_style,
            RichCheckBoxs iconCheckBoxs = new RichCheckBoxs()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Cursor = Cursors.Hand,
                Height = 30,
                Margin = new Thickness(10,0,0,0),
                FontSize = 15,
                Foreground = white_brush,
                HeaderHeight = 20,
                HeaderWidth = 20,
                ImageWidth = 40,
                ImageHeight = 40,
                ContentImage = bitmapImage,
                HeaderText = e.UserState.ToString(),
                TextMargin = new Thickness(40, 0, 0, 0)
            };
            iconCheckBoxs.Click += ItemSelected;
            iconCheckBoxs.Checked += ItemChecked;
            iconCheckBoxs.Unchecked += ItemUnChecked;
            checkbox_parent.Children.Add(iconCheckBoxs);
            Panel.SetZIndex(iconCheckBoxs, 0);
        }

        /// <summary>
        /// 加载完毕后删除骨架屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataSourceLoadCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            checkbox_parent.Children.Remove(SkeletionGrid);
            (checkbox_parent.Parent as ScrollViewer).IsEnabled = true;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReadDataSource(object sender, DoWorkEventArgs e)
        {
            #region 加载物品
            foreach (var item in MainWindow.item_database)
            {
                string id_and_name = item.Key.Replace(".", " ");
                if (!BlockAndItemKeys.Contains(id_and_name))
                {
                    Thread.Sleep(5);
                    BlockAndItemKeys.Add(id_and_name);
                    DataSourceReader.ReportProgress(0, id_and_name);
                }
            }
            ItemLoaded = true;
            #endregion

            #region 加载实体
            foreach (var item in MainWindow.entity_database)
            {
                string id_and_name = item.Key.Replace(".", " ");
                if (!BlockAndItemKeys.Contains(id_and_name))
                {
                    BlockAndItemKeys.Add(id_and_name);
                    DataSourceReader.ReportProgress(0, id_and_name);
                }
            }
            EntityLoaded = true;
            #endregion
        }

        /// <summary>
        /// 载入类型过滤列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TypeFilterLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBoxs = sender as ComboBox;
            comboBoxs.ItemsSource = MainWindow.TypeItemSource;
            TypeLoaded = true;
            TypeSelectionChanged(comboBoxs, null);
        }

        /// <summary>
        /// 更新过滤类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TypeLoaded)
            {

            }
        }

        /// <summary>
        /// 成员已被取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemUnChecked(object sender, RoutedEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;
            BlocksAndItems.Remove("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
        }

        /// <summary>
        /// 成员已被选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemChecked(object sender, RoutedEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;
            BlocksAndItems.Add("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
        }

        /// <summary>
        /// 成员选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemSelected(object sender, RoutedEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;
            if(richCheckBoxs.IsChecked.Value)
            BlocksAndItems.Add("\"minecraft:"+Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString()+"\",");
            else
                BlocksAndItems.Remove("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString()+"\",");
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="win"></param>
        private void return_command(CommonWindow win)
        {
            Tag.cbhk.Topmost = true;
            Tag.cbhk.WindowState = WindowState.Normal;
            Tag.cbhk.Show();
            Tag.cbhk.Topmost = false;
            Tag.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        private void run_command()
        {
            string result = string.Join("\r\n",BlocksAndItems).TrimEnd(',');
            result = "{\r\n  \"replace\": " + Replace.ToString().ToLower() + ",\r\n\"values\": [\r\n" + result + "  \r\n]\r\n}";
            BetterFolderBrowser folderBrowser = new BetterFolderBrowser()
            {
                Multiselect = true,
                Title = "请选择标签文件生成路径",
                RootFolder = Environment.SpecialFolder.MyComputer.ToString(),
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> WaitToOpen = new List<string> { };
                foreach (string path in folderBrowser.SelectedPaths)
                {
                    if (Directory.Exists(path))
                    {
                        File.WriteAllText(path + "\\" + (CurrentFileName.Trim() == "" ? "Tag" : CurrentFileName) + ".json", result);
                        WaitToOpen.Add(path+"\\"+ (CurrentFileName.Trim() == "" ? "Tag" : CurrentFileName)+".json");
                    }
                }
                //去重
                WaitToOpen = WaitToOpen.Distinct().ToList();
                foreach (string path in WaitToOpen)
                {
                    OpenFolderThenSelectFiles.ExplorerFile(path);
                }
            }
        }

        /// <summary>
        /// 全选当前已显示的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectAllVisibleItems(object sender, RoutedEventArgs e)
        {
            IconCheckBoxs current = sender as IconCheckBoxs;
            bool value = current.IsChecked.Value;

            if (VisibleCheckBoxs.Count == 0)
            {
                for (int i = 0; i < checkbox_parent.Children.Count; i++)
                {
                    (checkbox_parent.Children[i] as RichCheckBoxs).IsChecked = value;
                }
            }
            else
                for (int i = 0; i < VisibleCheckBoxs.Count; i++)
                {
                    VisibleCheckBoxs[i].IsChecked = value;
                }
        }

        /// <summary>
        /// 反选当前已显示的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReverseAllVisibleItems(object sender, RoutedEventArgs e)
        {
            if (VisibleCheckBoxs.Count == 0)
            {
                foreach (RichCheckBoxs item in checkbox_parent.Children)
                {
                    item.IsChecked = !item.IsChecked;
                }
            }
            else
                foreach (RichCheckBoxs item in VisibleCheckBoxs)
                {
                    item.IsChecked = !item.IsChecked;
                }
        }

        public void SearchBoxLoaded(object sender, RoutedEventArgs e)
        {
            search_box = sender as TextBox;
        }

        public void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            #region 为空则恢复所有RichCheckBox的可见性
            if (search_box.Text.Trim() == "")
            {
                foreach (RichCheckBoxs item in checkbox_parent.Children)
                {
                    item.Visibility = Visibility.Visible;
                }
                VisibleCheckBoxs.Clear();
                return;
            }
            #endregion

            //暂时全部隐藏
            foreach (RichCheckBoxs item in checkbox_parent.Children)
            {
                item.Visibility = Visibility.Collapsed;
            }

            List<string> result = BlockAndItemKeys.Where(item => item.Contains(search_box.Text) || search_box.Text.Contains(item)).Distinct().ToList();
            //更当前新可视化成员列表
            VisibleCheckBoxs.Clear();

            foreach (string item in result)
            {
                RichCheckBoxs box = checkbox_parent.Children[BlockAndItemKeys.IndexOf(item)] as RichCheckBoxs;
                if(box.HeaderText.Contains(item) || item.Contains(box.HeaderText))
                {
                    box.Visibility = Visibility.Visible;
                    VisibleCheckBoxs.Add(box);
                }
            }
        }
    }
}
