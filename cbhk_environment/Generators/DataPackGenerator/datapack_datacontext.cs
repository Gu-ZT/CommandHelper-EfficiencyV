using cbhk_environment.CustomControls;
using cbhk_environment.Generators.DataPackGenerator.Components;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSScriptControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace cbhk_environment.Generators.DataPackGenerator
{
    public class datapack_datacontext:ObservableObject
    {
        //存储所有类型的变量(比如记分板类型，记分板变量等等),用于补全
        Dictionary<string, ObservableCollection<string>> variable_source = new Dictionary<string, ObservableCollection<string>> { };
        //选择器类型文件路径
        string targetSelectorFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\target_selector.json";
        //选择器参数文件路径
        string targetSelectorParameterFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\target_selector_parameters.json";
        //bossbar样式文件路径
        string bossbarStyleFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\bossbarStyles.ini";
        //bossbar颜色文件路径
        string bossbarColorFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\bossbarColors.ini";
        //原版成就文件路径
        string advancementsFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\vanilla_advancement_list.ini";
        //原版生物属性列表
        string mobAttributeFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\mob_attribute_list.ini";

        //存储语法树数据
        Dictionary<string, ObservableCollection<string>> grammaticalTree = new Dictionary<string, ObservableCollection<string>> { };
        //存储所有指令的部首
        Dictionary<string,int> grammaticalRadical = new Dictionary<string, int> { };
        //语法树文件路径
        string grammaticalFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\grammatical_structure.json";

        //近期内容文件列表路径
        public static string recentContentsFolderPath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\recent_contents";

        //获取近期内容视图引用
        TreeView recentContentView = null;

        //获取内容视图引用
        TreeView ContentView = null;

        //近期内容前景色
        SolidColorBrush RecentContentForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

        //树视图样式引用
        Style RichTreeViewItemStyle = null;

        #region 初始化页面右侧按钮的指令列表
        public RelayCommand OpenLocalProject { get; set; }
        public RelayCommand OpenLocalFolder { get; set; }
        public RelayCommand CreateLocalDataPack { get; set; }
        #endregion

        #region 近期内容父级日期节点
        List<RichTreeViewItems> recentContentList = new List<RichTreeViewItems>()
                {
                    new RichTreeViewItems() { Header = "今天",Tag = "ToDay",IsExpanded = true},
                    new RichTreeViewItems() { Header = "昨天",Tag = "Yesterday",IsExpanded = true },
                    new RichTreeViewItems() { Header = "本周",Tag = "ThisWeek",IsExpanded = true },
                    new RichTreeViewItems() { Header = "上周",Tag = "LastWeek",IsExpanded = true },
                    new RichTreeViewItems() { Header = "本月",Tag = "ThisMonth",IsExpanded = true },
                    new RichTreeViewItems() { Header = "上月",Tag = "LastMonth",IsExpanded = true },
                    new RichTreeViewItems() { Header = "今年",Tag = "ThisYear",IsExpanded = true },
                    new RichTreeViewItems() { Header = "去年",Tag = "LastYear",IsExpanded = true }
                };
        #endregion

        #region 函数文本框可见性
        Visibility functionEditorZoneVisibility = Visibility.Collapsed;
        public Visibility FunctionEditorZoneVisibility
        {
            get { return functionEditorZoneVisibility; }
            set
            {
                functionEditorZoneVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 初始化页面可见性
        Visibility initPageVisibility = Visibility.Visible;
        public Visibility InitPageVisibility
        {
            get { return initPageVisibility; }
            set
            {
                initPageVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region js脚本执行者
        string js_file = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js";
        static string language = "javascript";
        static ScriptControlClass json_parser = new ScriptControlClass()
        {
            Language = language
        };
        public static object JsonScript(string JScript)
        {
            object Result = null;
            try
            {
                Result = json_parser.Eval(JScript);
            }
            catch (Exception ex)
            {
                return ex.Source + "\n" + ex.Message;
            }
            return Result;
        }
        #endregion

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

        public datapack_datacontext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            OpenLocalProject = new RelayCommand(OpenLocalProjectCommand);
            OpenLocalFolder = new RelayCommand(OpenLocalFolderCommand);
            CreateLocalDataPack = new RelayCommand(CreateLocalDataPackCommand);
            #endregion

            #region 读取语法树所需数据并解析每个指令的数据
            if (File.Exists(grammaticalFilePath))
            {
                string grammaticalJson = File.ReadAllText(grammaticalFilePath);

                //开始存储部首
                if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && 
                    grammaticalRadical.Count == 0)
                {
                    string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");
                    JsonScript(js_file);

                    JsonScript("parseJSON(" + grammaticalJson + ");");
                    int item_count = int.Parse(JsonScript("getLength();").ToString());
                    for (int i = 0; i < item_count; i++)
                    {
                        string radicalString = JsonScript("getJSON('[" + i + "].radical');").ToString();
                        //存储指令部首
                        grammaticalRadical.Add(radicalString,i);
                        //获取第一个子级的原串数据
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// 打开本地数据包
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CreateLocalDataPackCommand()
        {

        }

        /// <summary>
        /// 打开本地文件夹
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void OpenLocalFolderCommand()
        {

        }

        /// <summary>
        /// 打开本地项目
        /// </summary>
        private void OpenLocalProjectCommand()
        {

        }

        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="win"></param>
        private void return_command(CommonWindow win)
        {
            DataPack.cbhk.Topmost = true;
            DataPack.cbhk.WindowState = WindowState.Normal;
            DataPack.cbhk.Show();
            DataPack.cbhk.Topmost = false;
            DataPack.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        private void run_command()
        {
        }

        /// <summary>
        /// 获取树视图样式引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RichTreeViewItemStyleLoaded(object sender, RoutedEventArgs e)
        {
            RichTreeViewItemStyle = (sender as RichTreeViewItems).Style;
        }

        /// <summary>
        /// 获取内容树视图引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentViewLoaded(object sender, RoutedEventArgs e)
        {
            ContentView = sender as TreeView;
        }

        /// <summary>
        /// 获取近期内容视图引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecentFilesViewLoaded(object sender, RoutedEventArgs e)
        {
            recentContentView = sender as TreeView;

            #region 读取近期使用的内容
            if (Directory.Exists(recentContentsFolderPath))
            {
                string[] recent_contents = Directory.GetFiles(recentContentsFolderPath);

                //判断日期并添加成员
                foreach (string a_file in recent_contents)
                {
                    RecentItems recentItems = new RecentItems(AppDomain.CurrentDomain.BaseDirectory+ "resources\\configs\\DataPack\\images\\icon.png", a_file);
                    recentItems.MouseLeftButtonUp += OpenContentClick;
                    string dateData = recentItems.CalculationDateInterval();
                    RichTreeViewItems currentDateNode = recentContentList.Where(item=> item.Tag.ToString() == dateData).First();
                    currentDateNode.Foreground = RecentContentForeground;
                    currentDateNode.Items.Add(recentItems);
                }

                //添加日期
                foreach (RichTreeViewItems item in recentContentList)
                {
                    if (item.Items.Count > 0)
                        recentContentView.Items.Add(item);
                }
            }
            #endregion
        }

        /// <summary>
        /// 打开指定内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenContentClick(object sender, MouseButtonEventArgs e)
        {
            RecentItems item = sender as RecentItems;
            string CurrentContentFilePath = item.FilePath.Tag.ToString();

            if (File.Exists(CurrentContentFilePath) && File.Exists(js_file))
            {
                //解析内容文件
                string js_content = File.ReadAllText(js_file);
                JsonScript(js_content);

                string content = File.ReadAllText(CurrentContentFilePath);
                JsonScript("parseJSON("+content+");");
                string FilePath = JsonScript("getJSON('.FilePath');").ToString();

                if (Directory.Exists(FilePath))
                {
                    #region 判断目标为普通文件夹还是数据包
                    string[] datapackFiles = Directory.GetDirectories(FilePath);
                    //拥有data文件夹
                    bool HadDataFolder = false;
                    //拥有pack.mcmeta文件
                    bool HadMcmetaFile = false;
                    foreach (string file in datapackFiles)
                    {
                        if (Directory.Exists(file) && Path.GetDirectoryName(file) == "data")
                        {
                            HadDataFolder = true;
                            continue;
                        }

                        if(File.Exists(file) && Path.GetFileName(file) == "pack.mcmeta")
                        {
                            HadMcmetaFile = true;
                            continue;
                        }

                        if (HadDataFolder && HadMcmetaFile)
                            break;
                    }

                    //证实确实是数据包文件夹
                    if(HadDataFolder && HadMcmetaFile)
                    {
                        RichTreeViewItems contentItem = ContentReader.ReadTargetContent(FilePath+"\\data");
                        ContentView.Items.Add(contentItem);
                    }
                    #endregion

                    InitPageVisibility = Visibility.Collapsed;
                    FunctionEditorZoneVisibility = Visibility.Visible;
                    return;
                }
                else
                if (File.Exists(FilePath))
                {
                    RichTreeViewItems contentItem = new RichTreeViewItems() 
                    { 
                        Style = RichTreeViewItemStyle,
                        HorizontalAlignment = HorizontalAlignment.Left 
                    };

                    contentItem.ApplyTemplate();
                    Grid controlsContainer = contentItem.Template.FindName("controlsContainer", contentItem) as Grid;
                    FileItems fileItems = new FileItems() { Uid = FilePath };
                    fileItems.FileName.Text = Path.GetFileNameWithoutExtension(FilePath);
                    controlsContainer.Children.Add(fileItems);
                    ContentView.Items.Add(contentItem);

                    InitPageVisibility = Visibility.Collapsed;
                    FunctionEditorZoneVisibility = Visibility.Visible;
                }
                else
                {
                    if(MessageBox.Show("当前所选内容不存在,是否删除引用?","警告",MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        RichTreeViewItems dateItem = item.Parent as RichTreeViewItems;
                        dateItem.Items.Remove(item);
                        if (dateItem.Items.Count == 0)
                            recentContentView.Items.Remove(dateItem);
                        File.Delete(CurrentContentFilePath);
                    }
                }
            }
        }

        public void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
