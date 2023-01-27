using cbhk_environment.Distributor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static cbhk_environment.FilePathComparator;
using cbhk_environment.SettingForm;
using Point = System.Windows.Point;
using System.Linq;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using cbhk_environment.ControlsDataContexts;
using System.Collections.ObjectModel;
using MSScriptControl;
using System.Drawing;
using Image = System.Windows.Controls.Image;
using System.Text.RegularExpressions;

namespace cbhk_environment
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow:Window
    {
        /// <summary>
        /// 主页可见性
        /// </summary>
        public static MainWindowProperties.Visibility cbhk_visibility = MainWindowProperties.Visibility.MinState;

        /// <summary>
        /// 生成器背景图路径
        /// </summary>
        private string spawner_image_path =  AppDomain.CurrentDomain.BaseDirectory+ "resources\\spawner_button_images";

        /// <summary>
        /// 生成器视图列数
        /// </summary>
        private int spawner_button_column = 3;

        /// <summary>
        /// 生成器视图行列数
        /// </summary>
        private int spawner_button_row= 5;

        /// <summary>
        /// 生成器背景图索引
        /// </summary>
        private int spawner_backround_index = 0;
        
        /// <summary>
        /// 用户头像
        /// </summary>
        private string user_frame_source = "";
        /// <summary>
        /// 装载网址按钮
        /// </summary>
        private List<Button> LinkButtons = new List<Button> { };

        /// <summary>
        /// 切换按钮预览图像源
        /// </summary>
        private BitmapImage preview_image_source;

        /// <summary>
        /// 切换按钮预览图像渲染器
        /// </summary>
        private Image preview_image = new Image();

        /// <summary>
        /// 切换按钮预览内容
        /// </summary>
        private Grid preview_grid = new Grid();

        /// <summary>
        /// 切换按钮预览视图
        /// </summary>
        private Window preview_image_window = new Window 
        {
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowStyle = WindowStyle.None,
            WindowState = WindowState.Normal,
            BorderBrush = null,
            Width = 100,
            Height = 100,
            Topmost = true,
            BorderThickness = new Thickness(0)
        };

        /// <summary>
        /// 轮播图动画
        /// </summary>
        private LinkButtonAnimation lba;

        /// <summary>
        /// 主页轮播图公共数据库
        /// </summary>
        public static Dictionary<string, string> CircularBanner = new Dictionary<string, string> { };

        /// <summary>
        /// 保存当前图片的索引
        /// </summary>
        public static int current_image_index = 1;

        /// <summary>
        /// 保存上一个图片的索引
        /// </summary>
        public static int last_image_index = 0;

        /// <summary>
        /// 轮播图计时器
        /// </summary>
        public static DispatcherTimer LinkButtonAnimator = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(MainWindowProperties.LinkAnimationDelay),
            IsEnabled = false
        };

        //骨架屏计时器
        System.Windows.Forms.Timer SkeletonTimer = new System.Windows.Forms.Timer()
        {
            Interval = 1000,
            Enabled = false
        };

        /// <summary>
        /// 表示轮播图状态
        /// </summary>
        public static bool CircularBannerState = false;

        public TaskbarIcon taskbar_icon;

        #region js脚本执行者
        private static string language = "javascript";
        public static ScriptControlClass json_parser = new ScriptControlClass()
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

        #region 所有数据源对象
        //属性数据源
        public static ObservableCollection<string> AttributeSource = new ObservableCollection<string> { };
        //属性生效槽位数据源
        public static ObservableCollection<string> AttributeSlotSource = new ObservableCollection<string> { };
        //属性值类型数据源
        public static ObservableCollection<string> AttributeValueTypeSource = new ObservableCollection<string> { };
        //物品id数据源
        public static ObservableCollection<IconComboBoxItem> ItemIdSource = new ObservableCollection<IconComboBoxItem> { };
        //附魔id数据源
        public static ObservableCollection<string> EnchantmentIdSource = new ObservableCollection<string> { };
        //保存id与name
        public static Dictionary<string, BitmapImage> entity_database = new Dictionary<string, BitmapImage> { };
        //物品id数据源
        public static ObservableCollection<IconComboBoxItem> EntityIdSource = new ObservableCollection<IconComboBoxItem> { };
        //保存药水id与name
        public static Dictionary<string, string> mob_effect_database = new Dictionary<string, string> { };
        //药水id数据源
        public static ObservableCollection<IconComboBoxItem> MobEffectIdSource = new ObservableCollection<IconComboBoxItem> { };
        //保存id与name
        public static Dictionary<string, BitmapImage> item_database = new Dictionary<string, BitmapImage> { };
        //保存附魔id与name
        public static Dictionary<string, string> enchantment_databse = new Dictionary<string, string> { };
        //保存属性id与name
        public static Dictionary<string, string> attribute_database = new Dictionary<string, string> { };
        //保存隐藏信息id与name
        public static Dictionary<string, string> hide_infomation_database = new Dictionary<string, string> { };
        //信息隐藏标记
        public static ObservableCollection<string> hide_flags_source = new ObservableCollection<string> { };
        public static ObservableCollection<string> hide_flags_collection = new ObservableCollection<string>();

        //标签生成器的过滤类型数据源
        public static ObservableCollection<string> TypeItemSource = new ObservableCollection<string> { };
        public static ObservableCollection<string> TagTypeCollection = new ObservableCollection<string>();

        //粒子列表数据源
        public static ObservableCollection<string> particle_database = new ObservableCollection<string> { };

        //音效列表数据源
        public static ObservableCollection<string> sound_database = new ObservableCollection<string> { };

        //音效列表id名称字典
        public static Dictionary<string,string> soundIdNameSource = new Dictionary<string,string> { };

        //记分板判据类型数据源
        public static ObservableCollection<string> scoreboardType_database = new ObservableCollection<string> { };

        //队伍颜色列表
        public static ObservableCollection<string> teamColor_database = new ObservableCollection<string> { };
        #endregion

        //异步执行数据读取逻辑
        //BackgroundWorker DataSourceReader = new BackgroundWorker();
        //用户数据
        Dictionary<string, string> UserData = new Dictionary<string, string> { };

        public MainWindow(Dictionary<string,string> user_info)
        {
            InitializeComponent();
            UserData = user_info;
            //InitializeBackgroundWorker();
            //执行数据源加载进程
            //DataSourceReader.RunWorkerAsync();
            ReadDataSource();
            InitUIData();
            SkeletonTimer.Tick += SkeletonScreenShowDuration;
        }

        /// <summary>
        /// 骨架屏持续时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SkeletonScreenShowDuration(object sender, EventArgs e)
        {
            SkeletonGrid.Visibility = Visibility.Collapsed;
            GeneratorTable.Visibility = Visibility.Visible;
            SkeletonTimer.Enabled = false;
        }

        /// <summary>
        /// 初始化加载进程
        /// </summary>
        //private void InitializeBackgroundWorker()
        //{
        //    #region
        //    if (DataSourceReader == null)
        //        DataSourceReader = new BackgroundWorker();
        //    //bool类型，指示DataSourceReader是否可以报告进度更新。当该属性值为True时，将可以成功调用ReportProgress方法
        //    DataSourceReader.WorkerReportsProgress = true;
        //    //bool类型，指示DataSourceReader是否支持异步取消操作。当该属性值为True是，将可以成功调用CancelAsync方法
        //    DataSourceReader.WorkerSupportsCancellation = true;
        //    //执行RunWorkerAsync方法后触发DoWork，将异步执行backgroundWorker_DoWork方法中的代码
        //    DataSourceReader.DoWork += new DoWorkEventHandler(ReadDataSource);
        //    //执行ReportProgress方法后触发ProgressChanged，将执行ProgressChanged方法中的代码
        //    //DataSourceReader.ProgressChanged += InitUIData;//object sender, ProgressChangedEventArgs e
        //    //异步操作完成或取消时执行的操作，当调用DoWork事件执行完成时触发
        //    DataSourceReader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DataSourceLoadCompleted);
        //    #endregion
        //}

        /// <summary>
        /// 读取所有数据源,提供给所有生成器使用
        /// </summary>
        private void ReadDataSource()
        {
            #region 获取所有物品的id和对应的中文
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\items.json") &&
               File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && item_database.Count == 0)
            {
                string items_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\items.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);
                string item_id;
                string item_name;

                JsonScript("parseJSON(" + items_json + ");");
                int item_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < item_count; i++)
                {
                    item_id = JsonScript("getJSON('[" + i + "].id');").ToString();
                    item_name = JsonScript("getJSON('[" + i + "].name');").ToString();
                    BitmapImage image = null;
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + item_id + ".png"))
                    {
                        image = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + item_id + ".png", UriKind.Relative));
                        ItemIdSource.Add(new IconComboBoxItem() { ComboBoxItemText = item_name, ComboBoxItemIcon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + item_id + ".png", UriKind.Absolute)) });
                    }
                    if (!item_database.ContainsKey(item_id + "." + item_name))
                        item_database.Add(item_id + "." + item_name, image);
                }
            }
            #endregion

            #region 获取所有附魔id和描述
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\enchantments.json") &&
                File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && enchantment_databse.Count == 0)
            {
                string enchantments_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\enchantments.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);

                ObservableCollection<string> itemDataGroups = new ObservableCollection<string>();
                string enchantment_id = "";
                string enchantment_name = "";
                string enchantment_num = "";

                JsonScript("parseJSON(" + enchantments_json + ");");

                int enchantment_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < enchantment_count; i++)
                {
                    enchantment_id = JsonScript("getJSON('[" + i + "].id');").ToString();
                    enchantment_name = JsonScript("getJSON('[" + i + "].name');").ToString();
                    enchantment_num = JsonScript("getJSON('[" + i + "].num');").ToString();
                    enchantment_databse.Add(enchantment_id, enchantment_name + enchantment_num);
                    itemDataGroups.Add(enchantment_name);
                }
                EnchantmentIdSource = itemDataGroups;
            }
            #endregion

            #region 获取所有药水效果和描述
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\mob_effects.json") &&
                File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && mob_effect_database.Count == 0)
            {
                string potion_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\mob_effects.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);

                string potion_id = "";
                string potion_name = "";
                string potion_num = "";

                JsonScript("parseJSON(" + potion_json + ");");

                int effect_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < effect_count; i++)
                {
                    potion_id = JsonScript("getJSON('[" + i + "].id');").ToString();
                    potion_name = JsonScript("getJSON('[" + i + "].name');").ToString();
                    potion_num = JsonScript("getJSON('[" + i + "].num');").ToString();

                    mob_effect_database.Add(potion_id, potion_name + potion_num);
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\mob_effects_images\\" + potion_id + ".png"))
                    {
                        MobEffectIdSource.Add(new IconComboBoxItem() { ComboBoxItemText = potion_name, ComboBoxItemIcon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\mob_effects_images\\" + potion_id + ".png", UriKind.Absolute)) });
                    }
                }
            }
            #endregion

            #region 获取属性列表
            if (AttributeSource.Count == 0)
            {
                ObservableCollection<string> attribbuteSource = new ObservableCollection<string>();

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\AttributeIds.ini"))
                {
                    string[] attribute = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\AttributeIds.ini");
                    for (int i = 0; i < attribute.Length; i++)
                    {
                        string[] attribute_info = attribute[i].Split(':');
                        string attribue_id = attribute_info[0];
                        string attribute_name = attribute_info[1];
                        if (!attribute_database.ContainsKey(attribue_id))
                            attribute_database.Add(attribue_id, attribute_name);
                        attribbuteSource.Add(attribute_name);
                    }
                    AttributeSource = attribbuteSource;
                }
            }
            #endregion

            #region 获取属性生效槽位列表
            if (AttributeSlotSource.Count == 0)
            {
                ObservableCollection<string> attributeSlotSource = new ObservableCollection<string>();

                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\Slots.ini"))
                {
                    string[] attribute = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\Slots.ini");
                    for (int i = 0; i < attribute.Length; i++)
                    {
                        string[] attribute_info = attribute[i].Split(':');
                        string attribue_id = attribute_info[0];
                        string attribute_name = attribute_info[1];
                        if (!attribute_database.ContainsKey(attribue_id))
                            attribute_database.Add(attribue_id, attribute_name);
                        attributeSlotSource.Add(attribute_name);
                    }
                    AttributeSlotSource = attributeSlotSource;
                }
            }
            #endregion

            #region 获取属性类型列表
            if (AttributeValueTypeSource.Count == 0)
            {
                ObservableCollection<string> attributeValueTypeSource = new ObservableCollection<string>();
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\ValueTypes.ini"))
                {
                    string[] attribute = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\ValueTypes.ini");
                    for (int i = 0; i < attribute.Length; i++)
                    {
                        string[] attribute_info = attribute[i].Split(':');
                        string attribue_id = attribute_info[0];
                        string attribute_name = attribute_info[1];
                        if (!attribute_database.ContainsKey(attribue_id))
                            attribute_database.Add(attribue_id, attribute_name);
                        attributeValueTypeSource.Add(attribute_name);
                    }
                    AttributeValueTypeSource = attributeValueTypeSource;
                }
            }
            #endregion

            #region 获取信息隐藏标记
            hide_flags_source = hide_flags_collection;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\HideInfomationOptions.ini"))
            {
                string[] hide_flag = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Item\\data\\HideInfomationOptions.ini");
                foreach (string item in hide_flag)
                {
                    string[] hide_info = item.Split(':');
                    hide_infomation_database.Add(hide_info[0], hide_info[1]);
                    hide_flags_collection.Add(hide_info[1]);
                }
            }
            #endregion

            #region 获取所有实体的id和对应的中文
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\entities.json") &&
               File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && entity_database.Count == 0)
            {
                string entities_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\entities.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);
                string entity_id = "";
                string entity_name = "";
                BitmapImage image = new BitmapImage();

                JsonScript("parseJSON(" + entities_json + ");");
                int item_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < item_count; i++)
                {
                    entity_id = JsonScript("getJSON('[" + i + "].id');").ToString();
                    entity_name = JsonScript("getJSON('[" + i + "].name');").ToString();
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + "_spawn_egg.png"))
                    {
                        Bitmap bitmap = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + "_spawn_egg.png");
                        bitmap = GeneralTools.ChangeBitmapSize.Magnifier(bitmap, 2);
                        image = GeneralTools.BitmapImageConverter.ToBitmapImage(bitmap);
                        EntityIdSource.Add(new IconComboBoxItem() { ComboBoxItemText = entity_name, ComboBoxItemIcon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + "_spawn_egg.png", UriKind.Absolute)) });
                    }
                    else
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + ".png"))
                    {
                        Bitmap bitmap = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + ".png");
                        bitmap = GeneralTools.ChangeBitmapSize.Magnifier(bitmap, 2);
                        image = GeneralTools.BitmapImageConverter.ToBitmapImage(bitmap);
                        EntityIdSource.Add(new IconComboBoxItem() { ComboBoxItemText = entity_name, ComboBoxItemIcon = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + entity_id + ".png", UriKind.Absolute)) });
                    }
                    if (!entity_database.ContainsKey(entity_id + "." + entity_name))
                        entity_database.Add(entity_id + "." + entity_name, image);
                }
            }
            #endregion

            #region 获取所有粒子id
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory+ "resources\\data_sources\\particles.json") && File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && particle_database.Count == 0)
            {
                string particle_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\particles.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);
                string particle_id = "";
                JsonScript("parseJSON(" + particle_json + ");");
                int item_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < item_count; i++)
                {
                    particle_id = JsonScript("getJSON('[" + i + "]');").ToString();
                    particle_database.Add(particle_id);
                }
            }
            #endregion

            #region 获取所有音效id
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\sounds.json") && File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && sound_database.Count == 0)
            {
                string sound_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\sounds.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);
                string sound_id = "";
                string sound_name = "";
                JsonScript("parseJSON(" + sound_json + ");");
                int item_count = int.Parse(JsonScript("getLength();").ToString());
                for (int i = 0; i < item_count; i++)
                {
                    sound_id = JsonScript("getJSON('[" + i + "].id');").ToString();
                    sound_name = JsonScript("getJSON('[" + i + "].name');").ToString();

                    soundIdNameSource.Add(sound_id, sound_name);
                    sound_database.Add(sound_id);
                }
            }
            #endregion

            #region 获取所有队伍颜色
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\teamColor.ini"))
            {
                string[] team_colors = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\teamColor.ini");
                for (int i = 0; i < team_colors.Length; i++)
                {
                    teamColor_database.Add(team_colors[i]);
                }
            }
            #endregion

            #region 获取所有记分板判据类型
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\scoreboardType.json") && File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js") && scoreboardType_database.Count == 0)
            {
                string scoreboardType_json = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\scoreboardType.json");
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");

                JsonScript(js_file);
                string scoreboard_type;
                JsonScript("parseJSON(" + scoreboardType_json + ");");
                int item_count = int.Parse(JsonScript("getLength();").ToString());
                //按正则提取记分板的类型分支成员
                Regex GetTypeItems = new Regex(@"(?<=\{)[^}]*(?=\})");
                for (int i = 0; i < item_count; i++)
                {
                    scoreboard_type = JsonScript("getJSON('[" + i + "]');").ToString();
                    if(scoreboard_type.Contains("{"))
                    {
                        string item = GetTypeItems.Match(scoreboard_type).ToString();
                        string type_head = GetTypeItems.Replace(scoreboard_type, "").Replace("{", "").Replace("}", "");
                        switch (item)
                        {
                            case "teamColor":
                                foreach (var color in teamColor_database)
                                {
                                    scoreboardType_database.Add(type_head + color);
                                }
                                break;
                            case "itemName":
                                foreach (var an_item in item_database)
                                {
                                    string item_key = an_item.Key.Split('.')[0];
                                    scoreboardType_database.Add(type_head + item_key);
                                }
                                break;
                            case "entityName":
                                foreach (var an_entity in entity_database)
                                {
                                    string item_key = an_entity.Key.Split('.')[0];
                                    scoreboardType_database.Add(type_head + item_key);
                                }
                                break;
                        }
                    }
                    else
                    scoreboardType_database.Add(scoreboard_type);
                }
            }
            #endregion

            #region 加载过滤类型
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Tag\\TypeFilter.ini"))
            {
                string[] Types = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Tag\\TypeFilter.ini");
                for (int i = 0; i < Types.Length; i++)
                {
                    TagTypeCollection.Add(Types[i]);
                }
                TypeItemSource = TagTypeCollection;
            }
            #endregion
        }

        /// <summary>
        /// 初始化界面数据
        /// </summary>
        private void InitUIData()
        {
            #region 初始化托盘
            taskbar_icon = FindResource("cbhk_taskbar") as TaskbarIcon;
            //显示
            taskbar_icon.Visibility = Visibility.Visible;
            taskbar_icon.DataContext = new resources.MainFormDataContext.NotifyIconViewModel(this);
            #endregion

            #region 加载启动器配置
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\cbhk.ini"))
            {
                string[] configs = File.ReadAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\cbhk.ini");
                for (int i = 0; i < configs.Length; i++)
                {
                    string[] data = configs[i].Split(':');
                    switch (data[0])
                    {
                        case "CBHKVisibility":
                            {
                                switch (data[1])
                                {
                                    case "KeepState":
                                        {
                                            cbhk_visibility = MainWindowProperties.Visibility.KeepState;
                                            break;
                                        }
                                    case "MinState":
                                        {
                                            cbhk_visibility = MainWindowProperties.Visibility.MinState;
                                            break;
                                        }
                                    case "Close":
                                        {
                                            cbhk_visibility = MainWindowProperties.Visibility.Close;
                                            break;
                                        }
                                }
                                break;
                            }
                        case "CloseToTray":
                            {
                                MainWindowProperties.CloseToTray = bool.Parse(data[1]);
                                break;
                            }
                        //case "AutoStart":
                        //    {
                        //        MainWindowProperties.AutoStart = bool.Parse(data[1]);
                        //        if (MainWindowProperties.AutoStart)
                        //            GeneralTools.AutoStart.CreateShortcut("C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup", "命令管家", AppDomain.CurrentDomain.BaseDirectory + "cbhk.exe", "命令管家1.19", AppDomain.CurrentDomain.BaseDirectory + "cb.ico");
                        //        else
                        //            File.Delete("C:\\Users\\"+Environment.UserName+"\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\命令管家");
                        //        break;
                        //    }
                        case "LinkAnimationDelay":
                            {
                                MainWindowProperties.LinkAnimationDelay = int.Parse(data[1]);
                                break;
                            }
                    }
                }
            }
            #endregion

            #region 加载用户数据
            UserData.TryGetValue("user_frame", out user_frame_source);
            //没有头像就加载默认头像
            if (user_frame_source.Trim() != "")
            {
                user_frame.ImageSource = new BitmapImage(new Uri(user_frame_source, UriKind.Absolute));
            }
            else
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\command_block.png"))
            {
                user_frame.ImageSource = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\command_block.png", UriKind.RelativeOrAbsolute));
            }
            #endregion

            #region 加载轮播图数据
            //初始化切换按钮预览图
            preview_image_window.Content = preview_grid;
            preview_grid.Children.Add(preview_image);
            lba = new LinkButtonAnimation(LinkButtons);

            //读取本地现有轮播图数据
            string current_value = "";
            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\link_data"))
            {
                foreach (string data in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "resources\\link_data"))
                {
                    if (Path.GetExtension(data) == ".png")
                    {
                        current_value = Path.GetFileNameWithoutExtension(data);
                        current_value = Path.GetDirectoryName(data) + "\\" + current_value + ".txt";
                        current_value = File.Exists(current_value) ? current_value : "";
                        CircularBanner.Add(data, current_value);
                    }
                }
                CircularBannerLoader(false);
                LinkButtonAnimator.Tick += AnimatorBehavior;
            }
            #endregion

            #region 初始化生成器按钮

            #region 生成器背景图列表
            List<BitmapImage> spawner_background = new List<BitmapImage> { };
            //获取生成器图片列表
            string[] spawn_background_list = null;
            
            if(Directory.Exists(spawner_image_path))
            spawn_background_list = Directory.GetFiles(spawner_image_path);
            List<FileNameString> spawnerBgPathSorter = new List<FileNameString> { };
            //分配值给比较器
            if(spawn_background_list != null && spawn_background_list.Length > 0)
            for (int i = 0; i < spawn_background_list.Length; i++)
            {
                string current_path = Path.GetFileNameWithoutExtension(spawn_background_list[i]);
                spawnerBgPathSorter.Add(new FileNameString() { FileName = current_path, FilePath = spawn_background_list[i], FileIndex = i });
            }
            FileNameComparer fileNameComparer = new FileNameComparer { };
            //比较器开始排序
            spawnerBgPathSorter.Sort(fileNameComparer);
            //遍历本地生成器图像目录
            foreach (FileNameString item in spawnerBgPathSorter)
            {
                //添加进背景图链表
                if (File.Exists(item.FilePath))
                    spawner_background.Add(new BitmapImage(new Uri(item.FilePath, UriKind.Absolute)));
                else
                    spawner_background.Add(null);
            }
            #endregion

            #region 生成布局,分配方法
            //实例化方法分配器,链接数据上下文
            GeneratorFunction sf = new GeneratorFunction(this);
            //遍历方法分配器中的启动器属性
            int current_column = 0;
            int current_row = 0;
            int spawner_background_length = spawner_background.Count;

            for (int i = 0; i < spawner_background_length; i++)
            {
                //检查是否需要重置行和列
                current_row = current_column > spawner_button_column ? ++current_row : current_row;
                current_column = current_column > spawner_button_column ? 0 : current_column;

                #region 实例化生成器按钮
                Button spawner_btn = new Button
                {
                    DataContext = sf,
                    Width = 188,
                    Height = 70,
                    BorderBrush = null,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Style = Resources["GeneratorButtonStyle"] as Style
                };

                if (spawner_background[i] != null)
                {
                    int function_index = int.Parse(Path.GetFileNameWithoutExtension(spawner_background[i].UriSource.ToString()));
                    spawner_btn.Background = new ImageBrush(spawner_background[i]);
                    spawner_btn.Command = sf.spawner_functions[function_index];
                    spawner_btn.CommandParameter = this;
                }
                //在页面上生成对应的行或列
                ColumnDefinition cd = new ColumnDefinition()
                {
                    Width = new GridLength(1, GridUnitType.Auto)
                };
                RowDefinition rd = new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Auto)
                };
                #endregion
                GeneratorTable.ColumnDefinitions.Add(cd);
                GeneratorTable.RowDefinitions.Add(rd);
                GeneratorTable.Children.Add(spawner_btn);
                //设置按钮的行列的同时迭代1个单位
                Grid.SetRow(spawner_btn, current_row);
                Grid.SetColumn(spawner_btn, current_column++);
            }
            #endregion

            #endregion
        }

        private void CircularBannerStartEvent(object sender, SelectionChangedEventArgs e)
        {
            TabControl this_tabcontrol = e.Source as TabControl;
            LinkButtonAnimator.IsEnabled = this_tabcontrol.SelectedIndex == 1;
        }

        /// <summary>
        /// 个性化设置
        /// </summary>
        private void IndividualizationForm(object sender, EventArgs e)
        {
            IndividualizationForm indivi_form = new IndividualizationForm();

            LinkButtonAnimator.IsEnabled = false;
            if(indivi_form.ShowDialog() == true)
            {
                if (CircularBanner == null)
                {
                    #region 清空当前网址页的所有功能性成员
                    LinkSwitchPanel.Children.Clear();
                    LinkButtons.Clear();

                    for (int i = 0; i < LinkGrid.Children.Count; i++)
                    {
                        if (LinkGrid.Children[i] is Button)
                            LinkGrid.Children.Remove(LinkGrid.Children[i]);
                    }
                    #endregion
                    return;
                }

                //更新轮播图播放延迟
                LinkButtonAnimator.Interval = TimeSpan.FromSeconds(MainWindowProperties.LinkAnimationDelay);
                // 处理轮播图
                CircularBannerLoader(true);
            }
            //设置网址按钮的层级
            for (int i = 0; i < LinkButtons.Count; i++)
            {
                Panel.SetZIndex(LinkButtons[i],LinkButtons.Count - (i+1));
            }
        }

        /// <summary>
        /// 轮播图加载器
        /// </summary>
        /// <param name="IsInit">是否为初始化</param>
        private void CircularBannerLoader(bool IsIndividualizationForm)
        {
            #region 清空当前网址页的所有功能性成员
            LinkSwitchPanel.Children.Clear();
            LinkButtons.Clear();

            for (int i = 0; i < LinkGrid.Children.Count; i++)
            {
                if (LinkGrid.Children[i] is Button)
                    LinkGrid.Children.Remove(LinkGrid.Children[i]);
            }
            last_image_index = 0;
            current_image_index = 1;
            #endregion

            //选中第一张图片
            bool SelectFirst = true;
            //标记切换按钮索引
            int switch_button_index = 0;
            //标记图片按钮索引
            int image_button_index = CircularBanner.Count-1;
            //搜索字典中的最后一个键
            string last_key = CircularBanner.ElementAt(CircularBanner.Keys.Count - 1).Key;
            if (CircularBanner != null && CircularBanner.Count > 0)
            {
                #region 载入图片
                foreach (var a_link in CircularBanner.Keys)
                {
                    ImageBrush LinkBrush = new ImageBrush(new BitmapImage(new Uri(a_link, UriKind.Absolute)))
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    //读取目标路径的网址
                    string target_link = File.Exists(CircularBanner[a_link])? File.ReadAllText(CircularBanner[a_link]):"";
                    //复制一份到管家目录
                    if (IsIndividualizationForm && File.Exists(a_link) && (Path.GetDirectoryName(a_link) != AppDomain.CurrentDomain.BaseDirectory + "resources\\link_data"))//文件依然存在且不同源
                    {
                        File.Copy(a_link, AppDomain.CurrentDomain.BaseDirectory + "resources\\link_data\\" + Path.GetFileName(a_link), true);
                        File.Copy(CircularBanner[a_link], AppDomain.CurrentDomain.BaseDirectory + "resources\\link_data\\" + Path.GetFileNameWithoutExtension(a_link) + ".txt", true);
                    }
                    //生成网址跳转按钮
                    Button DisplayLinkButton = new Button()
                    {
                        Background = LinkBrush,
                        Style = Resources["LinkImageButtonStyle"] as Style,
                        Width = ActualWidth + 30,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        DataContext = new InitCircularBanner(),
                        Name = "index"+ image_button_index--,
                        Margin = new Thickness(0),
                        CommandParameter = target_link,
                        Tag = target_link,
                        ToolTip = target_link
                    };
                    LinkGrid.Children.Add(DisplayLinkButton);
                    //装载网址按钮链表
                    LinkButtons.Add(DisplayLinkButton);

                    #region 生成切换管理器
                    //生成切换按钮
                    Button switch_button = new Button()
                    {
                        Style = SelectFirst ? (Resources["SelectedSwitchButtonStyle"] as Style) : (Resources["SelectSwitchButtonStyle"] as Style),
                        CommandParameter = switch_button_index++
                    };
                    switch_button.MouseEnter += Switch_button_MouseEnter;
                    switch_button.MouseMove += SwitchButtonMouseMove;
                    switch_button.MouseLeave += Switch_button_MouseLeave;
                    #endregion

                    #region 更新UI属性和细节
                    //绑定切换按钮的点击事件
                    switch_button.Click += SwitchImageCommand;
                    //如果是最后一个元素,那么取消按钮的右边距
                    switch_button.Margin = last_key == a_link ? new Thickness(0, 0, 0, 0) : switch_button.Margin;
                    SelectFirst = false;
                    LinkSwitchPanel.Children.Add(switch_button);
                    //将宽拉长指定量，否则最后一个按钮一部分会被遮挡
                    LinkSwitchPanel.Width = last_key == a_link ? LinkSwitchPanel.Children.Count * switch_button.Width + (switch_button.Width/2+5) : 0;
                    #endregion
                }

                #region 重排列网址链接按钮的层级
                image_button_index = CircularBanner.Count - 1;
                for (int i = 0; i < LinkButtons.Count; i++)
                {
                    Panel.SetZIndex(LinkButtons[i], image_button_index--);
                }
                #endregion
                #endregion

                //初始化坐标属性(否则无法移动)
                lba = new LinkButtonAnimation(LinkButtons);
                //设置控制按钮管理器为最高层
                Panel.SetZIndex(LinkSwitchPanel, LinkGrid.Children.Count + 1);
                //若当前在网址页则开启计时器
                LinkButtonAnimator.IsEnabled = TaskTabControl.SelectedIndex == 1;
                //更新层级和索引
                SwitchImageCommand(LinkSwitchPanel.Children[0], null);
            }
        }

        /// <summary>
        /// 加载预览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Switch_button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button this_btn = e.Source as Button;
            preview_image_source = new BitmapImage(new Uri(CircularBanner.Keys.ElementAt(int.Parse(this_btn.CommandParameter.ToString())), UriKind.Absolute));
            preview_image.Source = preview_image_source;
            preview_image_window.Show();
        }

        /// <summary>
        /// 隐藏预览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Switch_button_MouseLeave(object sender, MouseEventArgs e)
        {
            preview_image_window.Hide();
        }

        /// <summary>
        /// 显示预览图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchButtonMouseMove(object sender, MouseEventArgs e)
        {
            //获取鼠标当前相对于屏幕的位置
            Point form_p = Mouse.GetPosition(e.Source as FrameworkElement);
            Point screen_point = (e.Source as FrameworkElement).PointToScreen(form_p);
            //设置窗体坐标
            preview_image_window.Left = screen_point.X - 50;
            preview_image_window.Top = screen_point.Y-100;
        }

        /// <summary>
        /// 切换按钮指令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchImageCommand(object sender, EventArgs e)
        {
            Button this_btn = sender as Button;
            current_image_index = int.Parse(this_btn.CommandParameter.ToString());

            //如果触发本事件时动画正在播放则历史索引向前推进一个单位
            if (CircularBannerState)
                last_image_index--;
            if (last_image_index > (LinkSwitchPanel.Children.Count - 1))
                last_image_index = LinkSwitchPanel.Children.Count - 1;
            if (last_image_index == current_image_index)
                last_image_index--;
            if(last_image_index < 0)
                last_image_index = LinkSwitchPanel.Children.Count - 1;

            lba.storyboard.Stop();
            LinkButtonAnimator.IsEnabled = false;
            int current_index = Panel.GetZIndex(LinkButtons[int.Parse(this_btn.CommandParameter.ToString())]);
            Panel.SetZIndex(LinkButtons[int.Parse(this_btn.CommandParameter.ToString())], -1);

            #region 处理当前层的上层所有按钮层级
            for (int i = 0; i < LinkButtons.Count; i++)
            {
                int compare_index = Panel.GetZIndex(LinkButtons[i]);
                if (compare_index > current_index /*&& compare_index >=0*/)
                {
                    LinkButtons[i].Name = "index" + (compare_index - 1);
                    Panel.SetZIndex(LinkButtons[i], compare_index - 1);
                }
            }
            Panel.SetZIndex(LinkButtons[int.Parse(this_btn.CommandParameter.ToString())], LinkButtons.Count-1);
            #endregion

            //处理上一组
            last_image_index = last_image_index > LinkButtons.Count - 1 ? LinkButtons.Count - 1 :last_image_index;
            try
            {
                (LinkSwitchPanel.Children[last_image_index] as Button).Style = Resources["SelectSwitchButtonStyle"] as Style;
            }
            catch { /*MessageBox.Show(last_image_index+"");*/ }

            //处理当前组
            (LinkSwitchPanel.Children[current_image_index] as Button).Style = Resources["SelectedSwitchButtonStyle"] as Style;

            #region 索引同步
            last_image_index = current_image_index = int.Parse(this_btn.CommandParameter.ToString());
            current_image_index+=1;
            current_image_index = current_image_index > (LinkButtons.Count - 1) ? 0 : current_image_index;
            #endregion

            //延迟三秒开启轮播
            System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(MainWindowProperties.LinkAnimationDelay)).ContinueWith(_ => { LinkButtonAnimator.IsEnabled = true; });
        }

        /// <summary>
        /// 轮播图管理器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimatorBehavior(object sender, EventArgs e)
        {
            DispatcherTimer this_timer = sender as DispatcherTimer;

            #region 判断两组按钮是否超出数据索引范围
            current_image_index = current_image_index > (LinkButtons.Count - 1) ? 0 : current_image_index;
            last_image_index = last_image_index > (LinkButtons.Count - 1) ? 0 : last_image_index;
            #endregion

            this_timer.IsEnabled = false;
            lba.SwitchOpacityAndTranslate(LinkButtons[last_image_index], LinkButtons[current_image_index],LinkButtons,new List<Button> { LinkSwitchPanel.Children[last_image_index] as Button, LinkSwitchPanel.Children[current_image_index] as Button },new List<Style> { Resources["SelectSwitchButtonStyle"] as Style, Resources["SelectedSwitchButtonStyle"] as Style });

            //两组按钮索引迭代一个单位
            current_image_index++; last_image_index++;
        }

        /// <summary>
        /// 启动项设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatupItemClick(object sender, RoutedEventArgs e)
        {
            StartupItemForm sif = new StartupItemForm();

            #region 把当前的启动数据传递给启动项设置窗体
            //sif.AutoStartup.IsChecked = MainWindowProperties.AutoStart;
            sif.CloseToTray.IsChecked = sif.CloseForm.IsChecked = MainWindowProperties.CloseToTray;
            sif.CloseForm.IsChecked = !sif.CloseToTray.IsChecked;
            sif.KeepState.IsChecked = cbhk_visibility == MainWindowProperties.Visibility.KeepState;
            sif.MinSize.IsChecked = cbhk_visibility == MainWindowProperties.Visibility.MinState;
            #endregion

            if (sif.ShowDialog() == true)
            {
                //主页可见性
                cbhk_visibility = sif.KeepState.IsChecked.Value? MainWindowProperties.Visibility.KeepState:(sif.MinSize.IsChecked.Value?MainWindowProperties.Visibility.MinState:MainWindowProperties.Visibility.Close);
                //是否开机自启
                //MainWindowProperties.AutoStart = sif.AutoStartup.IsChecked.Value;
                //是否关闭后缩小到托盘
                MainWindowProperties.CloseToTray = sif.CloseToTray.IsChecked.Value;
                //轮播图播放速度
            }
        }

        #region 窗体行为
        /// <summary>
        /// 由于不是主窗体，所以退出应用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            SaveConfigs();
            taskbar_icon.Visibility = MainWindowProperties.CloseToTray ? Visibility.Visible : Visibility.Collapsed;
            ShowInTaskbar = cbhk_visibility != MainWindowProperties.Visibility.MinState;
            WindowState = WindowState.Minimized;

            if (!MainWindowProperties.CloseToTray)
            {
                taskbar_icon.Visibility = Visibility.Collapsed;
                Environment.Exit(0);
            }
        }

        /// <summary>
        /// 保存启动器配置
        /// </summary>
        private void SaveConfigs()
        {
            //保存的配置
            //if (MainWindowProperties.AutoStart)
            //    GeneralTools.AutoStart.CreateShortcut("C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup", "命令管家", AppDomain.CurrentDomain.BaseDirectory + "cbhk.exe", "命令管家1.19", AppDomain.CurrentDomain.BaseDirectory + "cb.ico");
            //else
            //    File.Delete("C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Programs\\Startup\\命令管家.lnk");

            if(Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs"))
            {
                List<string> data = new List<string> { };
                data.Add("CBHKVisibility:" + cbhk_visibility.ToString());
                //data.Add("AutoStart:"+MainWindowProperties.AutoStart);
                data.Add("CloseToTray:" + MainWindowProperties.CloseToTray);
                data.Add("LinkAnimationDelay:" + MainWindowProperties.LinkAnimationDelay);
                File.WriteAllLines(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\cbhk.ini", data.ToArray());
            }
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
                    MaxWidth = SystemParameters.WorkArea.Width+16;
                    MaxHeight = SystemParameters.WorkArea.Height+16;
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
            if(e.ClickCount == 2 && this_Panel.Name == "TitleStack")
            {
                WindowState = WindowState == WindowState.Maximized ?WindowState.Normal:WindowState.Maximized;
            }
        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //设置所有按钮的宽为实际长度+30
            foreach (Button btn in LinkButtons)
                btn.Width = ActualWidth + 30;
            //设置网址管理器宽为实际长度+100
            LinkSwitchPanel.Width += 50;
            ////绑定轮播图开启事件
            TaskTabControl.SelectionChanged += CircularBannerStartEvent;

            SkeletonTimer.Enabled = true;
        }
    }

    /// <summary>
    /// 管家的启动属性
    /// </summary>
    public static class MainWindowProperties
    {
        /// <summary>
        /// 开机自启
        /// </summary>
        //public static bool AutoStart { get; set; } = false;

        /// <summary>
        /// 关闭后缩小到托盘
        /// </summary>
        public static bool CloseToTray { get; set; } = true;

        /// <summary>
        /// 轮播图播放延迟
        /// </summary>
        public static int LinkAnimationDelay { get; set; } = 3;

        /// <summary>
        /// 主页可见性
        /// </summary>
        public enum Visibility
        {
            KeepState,
            MinState,
            Close
        }
    }
}
