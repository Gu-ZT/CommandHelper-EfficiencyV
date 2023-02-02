using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using cbhk_environment.Generators.VillagerGenerator.Components;
using cbhk_environment.CustomControls;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Data;
using cbhk_environment.ControlsDataContexts;
using System.Windows.Controls.Primitives;

namespace cbhk_environment.Generators.VillagerGenerator
{
    public class villager_datacontext:ObservableObject
    {
        #region 处理拖拽
        public static bool IsGrabingItem = false;
        Image drag_source = null;
        Image GrabedImage = new Image();
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
            }
        }
        #endregion

        //图标路径
        string icon_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\images\\icon.png";

        //导入的背包的列数
        int MaxColumnCount = 9;

        //拖拽响应范围
        Grid VillagerGridZone = null;
        //左侧交易项数据源
        public static ObservableCollection<TransactionItems> transactionItems { get; set; } = new ObservableCollection<TransactionItems> { };
        //言论数据源
        public static ObservableCollection<GossipsItems> gossipItems { get; set; } = new ObservableCollection<GossipsItems> { };

        //是否显示物品数据页
        bool IsDisplayItemInfoWindow = false;
        //物品数据页
        public static TransactionItemDataForm transactionItemDataForm = new TransactionItemDataForm();

        //当前选中的物品
        private TransactionItems CurrentItem = null;

        //物品搜索框引用
        TextBox ItemSearcher = null;

        #region 言论面板收放
        private Visibility isEditGossips = Visibility.Collapsed;
        public Visibility IsEditGossips
        {
            get { return isEditGossips; }
            set
            {
                isEditGossips = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以编辑言论
        private bool canEditGossips = false;
        public bool CanEditGossips
        {
            get { return canEditGossips; }
            set
            {
                canEditGossips = value;
                OnPropertyChanged();
                IsEditGossips = CanEditGossips ? Visibility.Visible:Visibility.Collapsed;
                //恢复所有交易项的价格
                if (!CanEditGossips)
                    transactionItems.All(item => { item.HideDiscountData();return true; });
                else
                    transactionItems.All(item => { item.HideDiscountData(false); return true; });
                OnlyEditItem = !CanEditBrain && !CanEditGossips ? Visibility.Collapsed : Visibility.Visible;
            }
        }
        #endregion

        #region 记忆面板收放
        private Visibility isEditBrain = Visibility.Collapsed;
        public Visibility IsEditBrain
        {
            get { return isEditBrain; }
            set
            {
                isEditBrain = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 是否可以编辑记忆
        private bool canEditBrain = false;
        public bool CanEditBrain
        {
            get { return canEditBrain; }
            set
            {
                canEditBrain = value;
                OnPropertyChanged();
                IsEditBrain = CanEditBrain ? Visibility.Visible : Visibility.Collapsed;
                OnlyEditItem = !CanEditBrain && !CanEditGossips ?Visibility.Collapsed:Visibility.Visible;
            }
        }
        #endregion

        #region 言论与记忆面板收放
        private Visibility onlyEditItem = Visibility.Visible;
        public Visibility OnlyEditItem
        {
            get { return onlyEditItem; }
            set
            {
                onlyEditItem = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //言论搜索目标引用
        TextBox GossipSearchTarget = null;
        //言论搜索类型引用
        ComboBox GossipSearchType = null;
        //言论数据源所在视图引用
        ScrollViewer GossipViewer = null;

        //背包物品数据源
        public List<UniformGrid> BagItems { get; set; } = new List<UniformGrid> { new UniformGrid() { Width = 1080 } };
        //背包物品引用
        ItemsControl ItemList = null;
        //交易列表
        public static List<string> Recipes = new List<string> { };
        //言论搜索类型数据源
        ObservableCollection<string> gossipSearchType = new ObservableCollection<string> { };
        //言论搜索类型配置文件路径
        string gossipSearchTypeFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\GossipSearchTypes.ini";
        //维度数据源
        ObservableCollection<string> DimensionTypeSource = new ObservableCollection<string> { };
        //维度数据源配置文件路径
        string dimensionTypeFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\DimensionTypes.ini";
        //维度类型数据库
        Dictionary<string, string> DimensionDataBase = new Dictionary<string, string> { };

        #region Offers
        private string Offers
        {
            get
            {
                if (transactionItems.Count == 0) return "";
                string result = "Offers:{Recipes:[";
                string transactionItemData = string.Join("", transactionItems.Select(item => item.TransactionItemData + ","));
                result += transactionItemData.TrimEnd(',') + "]},";
                return result;
            }
        }
        #endregion

        #region Gossips
        private string Gossips
        {
            get
            {
                if (!CanEditGossips || OnlyEditItem == Visibility.Collapsed || gossipItems.Count == 0) return "";
                string result = "Gossips:[";
                result += string.Join("", gossipItems.Select(item => "{" + item.GossipData + "},"));
                result = result.TrimEnd(',') + "],";
                return result;
            }
        }
        #endregion

        #region Brain

        #region 聚集点
        private string meeting_pointX = "";
        public string MeetingPointX
        {
            set { meeting_pointX = value; }
            get
            {
                return meeting_pointX;
            }
        }
        private string meeting_pointY = "";
        public string MeetingPointY
        {
            set { meeting_pointY = value; }
            get
            {
                return meeting_pointY;
            }
        }
        private string meeting_pointZ = "";
        public string MeetingPointZ
        {
            set { meeting_pointZ = value; }
            get
            {
                return meeting_pointZ;
            }
        }
        private string meetingPointDimension = null;
        public string MeetingPointDimension
        {
            get { return meetingPointDimension; }
            set
            {
                meetingPointDimension = value;
            }
        }
        private string MeetingPointDimensionString
        {
            get
            {
                string DimensionId = DimensionDataBase.Where(item => item.Value == MeetingPointDimension).First().Key;
                return MeetingPointDimension.Trim() != "" ? "dimension:\"minecraft:" + DimensionId + "\"" : "";
            }
        }
        private string MeetingPoint
        {
            get
            {
                string result = "meeting_point:{";
                string pos = MeetingPointX.Trim() != "" && MeetingPointY.Trim() != "" && MeetingPointZ.Trim() != "" ? "pos:[" + MeetingPointX + "," + MeetingPointY + "," + MeetingPointZ + "]," : "";
                if(MeetingPointDimensionString != "" && pos != "")
                result += pos + MeetingPointDimensionString;
                if (result.Trim() == "meeting_point:{") return "";
                return result.TrimEnd(',') + "},";
            }
        }
        #endregion

        #region 床位置
        private string homeX = "";
        public string HomeX
        {
            set { homeX = value; }
            get
            {
                return homeX;
            }
        }
        private string homeY = "";
        public string HomeY
        {
            set { homeY = value; }
            get
            {
                return homeY;
            }
        }
        private string homeZ = "";
        public string HomeZ
        {
            set { homeZ = value; }
            get
            {
                return homeZ;
            }
        }
        private string homeDimension = null;
        public string HomeDimension
        {
            get { return homeDimension; }
            set
            {
                homeDimension = value;
            }
        }
        private string HomeDimensionString
        {
            get
            {
                string DimensionId = DimensionDataBase.Where(item=>item.Value == HomeDimension).First().Key;
                return HomeDimension.Trim() != "" ? "dimension:\"minecraft:" + DimensionId + "\"" : "";
            }
        }
        private string Home
        {
            get
            {
                string result = "home:{";
                string pos = HomeX.Trim() != "" && HomeY.Trim() != "" && HomeZ.Trim() != "" ? "pos:[" + HomeX + "," + HomeY + "," + HomeZ + "]," : "";
                if(pos != "" && HomeDimensionString != "")
                result += pos + HomeDimensionString;
                if (result.Trim() == "home:{") return "";
                return result.TrimEnd(',') + "},";
            }
        }
        #endregion

        #region 工作站点
        private string job_siteX = "";
        public string JobSiteX
        {
            set { job_siteX = value; }
            get
            {
                return job_siteX;
            }
        }
        private string job_siteY = "";
        public string JobSiteY
        {
            set { job_siteY = value; }
            get
            {
                return job_siteY;
            }
        }
        private string job_siteZ = "";
        public string JobSiteZ
        {
            set { job_siteZ = value; }
            get
            {
                return job_siteZ;
            }
        }
        private string jobSiteDimension = null;
        public string JobSiteDimension
        {
            get { return jobSiteDimension; }
            set
            {
                jobSiteDimension = value;
            }
        }
        private string JobSiteDimensionString
        {
            get
            {
                string DimensionId = DimensionDataBase.Where(item => item.Value == jobSiteDimension).First().Key;
                return JobSiteDimension.Trim() !=""? "dimension:\"minecraft:" + DimensionId + "\",":"";
            }
        }
        private string JobSite
        {
            get
            {
                string result = "job_site:{";
                string pos = JobSiteX.Trim() != "" && JobSiteY.Trim() !="" && JobSiteZ.Trim() != ""? "pos:[" + JobSiteX + "," + JobSiteY + "," + JobSiteZ + "],":"";
                if(pos != "" && JobSiteDimensionString != "")
                result += pos + JobSiteDimensionString;
                if (result.Trim() == "job_site:{") return "";
                return result.TrimEnd(',')+"},";
            }
        }
        #endregion

        #region 记忆
        private string Brain
        {
            get
            {
                if (!CanEditBrain || OnlyEditItem == Visibility.Collapsed) return "";
                string memoriesContent = MeetingPoint + Home + JobSite;
                string result = "Brain:{memories:{" + memoriesContent.TrimEnd(',') + "}},";
                if (result.Trim() == "Brain:{memories:{}},") return "";
                return result;
            }
        }
        #endregion

        #endregion

        #region 村民属性

        #region 数据源
        string VillagerTypeSourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\VillagerTypes.ini";
        string VillagerProfessionsSourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\VillagerProfessionTypes.ini";
        string VillagerLevelSourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\VillagerLevels.ini";

        Dictionary<string, string> VillagerTypeDataBase = new Dictionary<string, string> { };
        Dictionary<string, string> VillagerProfessionTypeDataBase = new Dictionary<string, string> { };

        ObservableCollection<string> VillagerTypeSource = new ObservableCollection<string> { };
        ObservableCollection<string> VillagerProfessionTypeSource = new ObservableCollection<string> { };
        ObservableCollection<string> VillagerLevelSource = new ObservableCollection<string> { };
        #endregion

        #region 村民种类
        string villagerType = null;
        public string VillagerType
        {
            get
            {
                return villagerType;
            }
            set
            {
                villagerType = value;
            }
        }
        private string VillagerTypeString
        {
            get
            {
                string result = "type:\"minecraft:";
                result = result + VillagerTypeDataBase.Where(item => item.Value == VillagerType).First().Key + "\",";
                return result;
            }
        }
        #endregion

        #region 村民职业
        string villagerProfessionType = null;
        public string VillagerProfessionType
        {
            get { return villagerProfessionType; }
            set
            {
                villagerProfessionType = value;
            }
        }
        private string VillagerProfessionTypeString
        {
            get
            {
                string result = "profession:\"minecraft:";
                result = result + VillagerProfessionTypeDataBase.Where(item => item.Value == villagerProfessionType).First().Key + "\",";
                return result;
            }
        }
        #endregion

        #region 村民交易等级
        private string villagerLevel = null;
        public string VillagerLevel
        {
            get { return villagerLevel; }
            set
            {
                villagerLevel = value;
            }
        }
        private string VillagerLevelString
        {
            get
            {
                string result = "level:";
                result = result + VillagerLevel + ",";
                return result;
            }
        }

        #endregion

        #region 村民数据
        private string VillagerData
        {
            get
            {
                string result = "VillagerData:{";
                result += VillagerTypeString + VillagerProfessionTypeString + VillagerLevelString;
                result = result.TrimEnd(',') + "},";
                return result;
            }
        }
        #endregion

        #endregion

        #region 是否愿意交配
        private bool willing = false;
        public bool Willing
        {
            get { return willing; }
            set
            {
                willing = value;
            }
        }
        private string WillingString
        {
            get
            {
                return "Willing:" + (Willing ?1:0) + "b,";
            }
        }
        #endregion

        #region 此村民最后一次前往工作站点重新供应交易的刻。
        private string lastRestock = "0";
        public string LastRestock
        {
            get { return lastRestock; }
            set { lastRestock = value; }
        }
        private string LastRestockString
        {
            get
            {
                return LastRestock.Trim() != ""? "LastRestock:" +LastRestock+",":"";
            }
        }
        #endregion

        #region 此村民当前的经验值。
        private string xp = "1";
        public string Xp
        {
            get { return xp; }
            set { xp = value; }
        }
        private string XpString
        {
            get
            {
                return Xp.Trim() != "" ? "Xp:" + Xp + "," : "";
            }
        }
        #endregion

        public villager_datacontext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            #endregion

            #region 初始化网格的列
            BagItems[0].Columns = MaxColumnCount;
            #endregion
        }

        /// <summary>
        /// 载入高清背景并订阅鼠标移动事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TransactionBackgroundLoaded(object sender, RoutedEventArgs e)
        {
            VillagerGridZone = sender as Grid;
            VillagerGridZone.MouseMove += SelectItemMove;
        }

        /// <summary>
        /// 载入村民种类数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VillagerTypeLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;

            if(File.Exists(VillagerTypeSourceFilePath))
            {
                string[] data = File.ReadAllLines(VillagerTypeSourceFilePath);
                for (int i = 0; i < data.Length; i++)
                {
                    string[] item = data[i].Split('.');
                    string id = item[0];
                    string name = item[1];

                    if(!VillagerTypeDataBase.ContainsKey(id))
                    VillagerTypeDataBase.Add(id,name);

                    VillagerTypeSource.Add(name);
                }
                box.ItemsSource = VillagerTypeSource;
                box.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 载入村民职业种类数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VillagerProfessionTypeLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;

            if (File.Exists(VillagerProfessionsSourceFilePath))
            {
                string[] data = File.ReadAllLines(VillagerProfessionsSourceFilePath);
                for (int i = 0; i < data.Length; i++)
                {
                    string[] item = data[i].Split('.');
                    string id = item[0];
                    string name = item[1];

                    if(!VillagerProfessionTypeDataBase.ContainsKey(id))
                    VillagerProfessionTypeDataBase.Add(id, name);

                    VillagerProfessionTypeSource.Add(name);
                }
                box.ItemsSource = VillagerProfessionTypeSource;
                box.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 载入村民等级数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VillagerLevelLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;

            if (File.Exists(VillagerLevelSourceFilePath))
            {
                int level = int.Parse(File.ReadAllText(VillagerLevelSourceFilePath));
                for (int i = 1; i <= level; i++)
                {
                    VillagerLevelSource.Add(i.ToString());
                }
                box.ItemsSource = VillagerLevelSource;
                box.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 载入维度种类数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DimensionTypeLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox box = sender as ComboBox;

            if (File.Exists(dimensionTypeFilePath))
            {
                string[] data = File.ReadAllLines(dimensionTypeFilePath);
                for (int i = 0; i < data.Length; i++)
                {
                    string[] item = data[i].Split('.');
                    string id = item[0];
                    string name = item[1];

                    if(!DimensionDataBase.ContainsKey(id))
                    DimensionDataBase.Add(id, name);

                    DimensionTypeSource.Add(name);
                }
                box.ItemsSource = DimensionTypeSource;
                box.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 获取言论所在的视图引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GossipViewerLoaded(object sender, RoutedEventArgs e)
        {
            GossipViewer = sender as ScrollViewer;
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="win"></param>
        private void return_command(CommonWindow win)
        {
            Villager.cbhk.Topmost = true;
            Villager.cbhk.WindowState = WindowState.Normal;
            Villager.cbhk.Show();
            Villager.cbhk.Topmost = false;
            Villager.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        private void run_command()
        {
            string result = "";

            result += WillingString + VillagerData + Offers + Gossips + Brain + LastRestockString + XpString;

            result = "/summon villager ~ ~1 ~ {" + result.TrimEnd(',') + "}";

            GenerateResultDisplayer.Displayer displayer = GenerateResultDisplayer.Displayer.GetContentDisplayer();
            displayer.GeneratorResult(OverLying, new string[] { result }, new string[] { "" }, new string[] { icon_path }, new System.Windows.Media.Media3D.Vector3D() { X = 30, Y = 30 });
            displayer.Show();
        }

        /// <summary>
        /// 获取背包引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemZoneLoaded(object sender, RoutedEventArgs e)
        {
            ItemList = sender as ItemsControl;
            foreach (KeyValuePair<string, BitmapImage> item in MainWindow.ItemDataBase)
            {
                string[] image_name = item.Key.Split(':');
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + image_name[0] + ".png"))
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + image_name[0] + ".png", UriKind.Absolute));
                    Image a_item = new Image()
                    {
                        Tag = image_name[0],
                        ToolTip = image_name[0] + " " + image_name[1],
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Source = bitmapImage
                    };
                    a_item.MouseLeftButtonDown += SelectItemClickDown;
                    ToolTipService.SetShowDuration(a_item, 1500);
                    ToolTipService.SetInitialShowDelay(a_item, 0);
                    BagItems[0].Children.Add(a_item);
                    ToolTipService.GetToolTip(a_item);
                }
            }
            ItemList.ItemsSource = BagItems;
            BagItems[0].Children[0].Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 同步物品数据值
        /// </summary>
        private void ValueBinder()
        {
            #region 属性绑定器
            Binding rewardExpBinder = new Binding()
            {
                Path = new PropertyPath("RewardExp"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = false
            };
            Binding maxUsesBinder = new Binding()
            {
                Path = new PropertyPath("MaxUses"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            Binding usesBinder = new Binding()
            {
                Path = new PropertyPath("Uses"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            Binding xpBinder = new Binding()
            {
                Path = new PropertyPath("Xp"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            Binding demandBinder = new Binding()
            {
                Path = new PropertyPath("Demand"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            Binding specialPriceBinder = new Binding()
            {
                Path = new PropertyPath("SpecialPrice"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            Binding priceMultiplierBinder = new Binding()
            {
                Path = new PropertyPath("PriceMultiplier"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            #endregion

            BindingOperations.SetBinding(transactionItemDataForm.rewardExp, ToggleButton.IsCheckedProperty, rewardExpBinder);
            BindingOperations.SetBinding(transactionItemDataForm.maxUses, TextBox.TextProperty, maxUsesBinder);
            BindingOperations.SetBinding(transactionItemDataForm.uses, TextBox.TextProperty, usesBinder);
            BindingOperations.SetBinding(transactionItemDataForm.xp, TextBox.TextProperty, xpBinder);
            BindingOperations.SetBinding(transactionItemDataForm.demand, TextBox.TextProperty, demandBinder);
            BindingOperations.SetBinding(transactionItemDataForm.specialPrice, TextBox.TextProperty, specialPriceBinder);
            BindingOperations.SetBinding(transactionItemDataForm.priceMultiplier, TextBox.TextProperty, priceMultiplierBinder);
        }

        /// <summary>
        /// 添加一个交易项控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddTransactionItemClick(object sender, RoutedEventArgs e)
        {
            TransactionItems transaction = new TransactionItems()
            {
                Height = 125,
                Width = 575
            };
            transaction.MouseLeftButtonDown += BuyItemDataUpdater;
            transactionItems.Add(transaction);
        }

        /// <summary>
        /// 弹出物品数据更新窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BuyItemDataUpdater(object sender, MouseButtonEventArgs e)
        {
            IsDisplayItemInfoWindow = !IsDisplayItemInfoWindow;
            TransactionItems item = sender as TransactionItems;
            if (CurrentItem != item && CurrentItem != null)
            {
                IsDisplayItemInfoWindow = true;
            }
            CurrentItem = item;
            ValueBinder();
            transactionItemDataForm.DataContext = item;
            if (IsDisplayItemInfoWindow)
            {
                item.SetInfomationWindowPosition();
                transactionItemDataForm.Show();
            }
            else
                transactionItemDataForm.Hide();
        }

        /// <summary>
        /// 添加一个言论控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddGossipItemClick(object sender, RoutedEventArgs e)
        {
            GossipsItems gossipsItem = new GossipsItems()
            {
                Height = 125,
                Width = 400,
                Margin = new Thickness(0, 0, 5, 10),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            gossipsItem.MouseLeftButtonDown += GossipsItemMouseLeftButtonDown;
            gossipItems.Add(gossipsItem);
        }

        /// <summary>
        /// 获取言论搜索目标的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GossipSearchTargetLoaded(object sender, RoutedEventArgs e)
        {
            GossipSearchTarget = sender as TextBox;
        }

        /// <summary>
        /// 获取物品搜索框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemSearcherLoaded(object sender, RoutedEventArgs e)
        {
            ItemSearcher = sender as TextBox;
        }

        /// <summary>
        /// 查找物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchItemTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ItemSearcher.Text.Trim() == "")
            {
                foreach (Image item in BagItems[0].Children)
                {
                    item.Visibility = Visibility.Visible;
                }
                return;
            }

            foreach (Image item in BagItems[0].Children)
            {

                if (item.ToolTip.ToString() == ItemSearcher.Text.Trim() || item.ToolTip.ToString().Contains(ItemSearcher.Text.Trim()))
                {
                    item.Visibility = Visibility.Visible;
                }
                else
                    item.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 获取言论搜索类型的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void GossipSearchTypeLoaded(object sender, RoutedEventArgs e)
        {
            GossipSearchType = sender as ComboBox;
            if(File.Exists(gossipSearchTypeFilePath))
            {
                string[] types = File.ReadAllLines(gossipSearchTypeFilePath);
                for (int i = 0; i < types.Length; i++)
                {
                    gossipSearchType.Add(types[i]);
                }
            }
            GossipSearchType.ItemsSource = gossipSearchType;
            GossipSearchType.SelectedIndex = 0;
        }

        /// <summary>
        /// 查找目标言论并更新价格数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchGossipsTextChanged(object sender, TextChangedEventArgs e)
        {
            if(CanEditGossips)
            {
                string current_type = GossipSearchType.SelectedItem as string;
                List<GossipsItems> target_gossip = gossipItems.Where(gossip =>
                {
                    string type = gossip.Type.SelectedItem as string;
                    if (gossip.Target.Text == GossipSearchTarget.Text.Trim() && type == current_type)
                        return true;
                    else
                        return false;
                }).ToList();

                if (target_gossip.Count > 0)
                    GeneralTools.ScrollToSomeWhere.Scroll(target_gossip[0], GossipViewer);
            }
        }

        /// <summary>
        /// 左击言论成员后计算左侧所有物品的价格
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GossipsItemMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GossipsItems current_item = sender as GossipsItems;

            string current_type = current_item.Type.SelectedItem.ToString();

            if (current_type != "minor_negative" && current_type != "trading")
                return;

            string search_type = current_type == "minor_negative" ? "trading" : current_type;

            string compare_type = null;

            List<GossipsItems> another_item = gossipItems.Where(item =>
            {
                compare_type = item.Type.SelectedItem as string;
                if (item.Target.Text.Trim() == current_item.Target.Text.Trim() && search_type == compare_type)
                    return true;
                else
                    return false;
            }).ToList();

            if(another_item.Count > 0)
            if(another_item.First().Value.Text.Trim() != "" && current_item.Value.Text.Trim() != "")
            {
                int minor_negative = current_type == "minor_negative" ? int.Parse(another_item.First().Value.Text) : int.Parse(current_item.Value.Text);
                int trading = current_type != "trading" ? int.Parse(another_item.First().Value.Text) : int.Parse(current_item.Value.Text);
                transactionItems.All(item => { item.UpdateDiscountData(minor_negative, trading); return true; });
            }
        }

        /// <summary>
        /// 处理开始拖拽物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectItemClickDown(object sender, MouseButtonEventArgs e)
        {
            IsGrabingItem = true;
            Image image = sender as Image;
            drag_source = image;
            GrabedImage = new Image() { Tag = image.ToolTip.ToString() };
        }

        /// <summary>
        /// 处理拖拽物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectItemMove(object sender, MouseEventArgs e)
        {
            if (IsGrabingItem && drag_source != null && GrabedImage != null)
            {
                DataObject dataObject = new DataObject(typeof(Image), GrabedImage);
                if (dataObject != null)
                    DragDrop.DoDragDrop(drag_source, dataObject, DragDropEffects.Move);
                IsGrabingItem = false;
            }
        }
    }
}
