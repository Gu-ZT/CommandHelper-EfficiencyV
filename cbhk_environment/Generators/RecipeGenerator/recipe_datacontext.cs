using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools;
using cbhk_environment.Generators.RecipeGenerator.Components;
using cbhk_environment.GenerateResultDisplayer;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSScriptControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WK.Libraries.BetterFolderBrowserNS;

namespace cbhk_environment.Generators.RecipeGenerator
{
    public class recipe_datacontext : ObservableObject
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

        #region 保存物品ID
        private ItemDataGroup select_item_id_source;
        public ItemDataGroup SelectItemIdSource
        {
            get { return select_item_id_source; }
            set
            {
                select_item_id_source = value;
                OnPropertyChanged();
            }
        }
        private string ItemId
        {
            get
            {
                string key = MainWindow.item_database.Where(item => Regex.Match(item.Key, @"[\u4e00-\u9fa5]+").ToString() == SelectItemIdSource.ItemText).First().Key;
                string result = key != "" ? Regex.Match(key, @"[a-zA-Z_]+").ToString() : "";
                return result;
            }
        }
        #endregion

        #region 所有类型的配方
        static CraftingTable craftingTable = null;
        static Furnace furnace = null;
        static BlastFurnace blastFurnace = null;
        static SmithingTable smithingTable = null;
        static Smoker smoker = null;
        static Stonecutter stonecutter = null;
        static CampFire campFire = null;
        static List<UserControl> RecipeTypes = null;
        #endregion

        //配方窗体可视化区域引用
        Grid RecipeZone = null;
        //被抓取的物品
        public static Image GrabedImage = new Image();

        //是否选择物品
        public static bool IsGrabingItem = false;

        //本生成器的图标路径
        string icon_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Recipe\\images\\icon.png";
        //右侧缓存区引用
        private UniformGrid CacheZone = null;
        //拖拽源
        public static Image drag_source = null;

        /// <summary>
        /// 配方操作类型
        /// </summary>
        public enum RecipeModifyTypes
        {
            Add,
            Delete
        }

        public recipe_datacontext()
        {
            #region 链接命令
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            RunCommand = new RelayCommand(run_command);
            #endregion
        }

        private void return_command(CommonWindow win)
        {
            Recipe.cbhk.Topmost = true;
            Recipe.cbhk.WindowState = WindowState.Normal;
            Recipe.cbhk.Show();
            Recipe.cbhk.Topmost = false;
            Recipe.cbhk.ShowInTaskbar = true;
            win.Close();
        }
        private void run_command()
        {
            string result = "";
            string file_name = "";

            #region 工作台
            result = craftingTable.Visibility == Visibility.Visible ?craftingTable.RecipeData: result;
            file_name = craftingTable.Visibility == Visibility.Visible ? craftingTable.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 高炉
            result = blastFurnace.Visibility == Visibility.Visible ?blastFurnace.RecipeData: result;
            file_name = blastFurnace.Visibility == Visibility.Visible ? blastFurnace.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 营火
            result = campFire.Visibility == Visibility.Visible ? campFire.RecipeData : result;
            file_name = campFire.Visibility == Visibility.Visible ? campFire.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 熔炉
            result = furnace.Visibility == Visibility.Visible ?furnace.RecipeData: result;
            file_name = furnace.Visibility == Visibility.Visible ? furnace.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 烟熏炉
            result = smoker.Visibility == Visibility.Visible ?smoker.RecipeData: result;
            file_name = smoker.Visibility == Visibility.Visible ? smoker.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 切石机
            result = stonecutter.Visibility == Visibility.Visible ?stonecutter.RecipeData: result;
            file_name = stonecutter.Visibility == Visibility.Visible ? stonecutter.RecipeFileName.Text.Trim() : file_name;
            #endregion

            #region 锻造台
            result = smithingTable.Visibility == Visibility.Visible ?smithingTable.RecipeData:result;
            file_name = smithingTable.Visibility == Visibility.Visible ? smithingTable.RecipeFileName.Text.Trim() : file_name;
            #endregion

            BetterFolderBrowser folderBrowser = new BetterFolderBrowser()
            {
                Multiselect = true,
                Title = "请选择配方文件生成路径",
                RootFolder = Environment.SpecialFolder.MyComputer.ToString(),
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<string> WaitToOpen = new List<string> { };
                foreach (string path in folderBrowser.SelectedPaths)
                {
                    File.WriteAllText(path + "\\"+file_name+".json", result);
                    WaitToOpen.Add(path + "\\"+ file_name + ".json");
                }
                //去重
                WaitToOpen = WaitToOpen.Distinct().ToList();
                foreach (string path in WaitToOpen)
                {
                    OpenFolderThenSelectFiles.ExplorerFile(path);
                }
            }

            //Displayer displayer = Displayer.GetContentDisplayer();
            //displayer.GeneratorResult(OverLying, new string[] { result }, new string[] { "" }, new string[] { icon_path }, new System.Windows.Media.Media3D.Vector3D() { X = 30, Y = 30 });
            //displayer.Show();
        }

        /// <summary>
        /// 载入左侧切换栏图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void IconViewerLoaded(object sender, RoutedEventArgs e)
        {
            StackPanel parent = sender as StackPanel;
            Style btn_style = (parent.Children[0] as TextButtons).Style;
            //获取所有配方图标文件
            string[] icon_files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Recipe\\images");
            for (int i = 0; i < icon_files.Length; i++)
            {
                string current_file_name = Path.GetFileName(icon_files[i]);
                if (current_file_name.Contains("icon.png") && current_file_name.Length > 8)
                {
                    TextButtons iconTextButtons = new TextButtons
                    {
                        ThicknessBackground = new BitmapImage(new Uri(icon_files[i], UriKind.Absolute)),
                        Tag = icon_files[i],
                        Height = 50,
                        Width = 50,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0)),
                        BorderThickness = new Thickness(1),
                        Style = btn_style,
                        ClickMode = ClickMode.Press
                    };
                    iconTextButtons.Click += RecipeTyleSwitcher;
                    parent.Children.Add(iconTextButtons);
                }
            }
        }

        /// <summary>
        /// 切换配方类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecipeTyleSwitcher(object sender, RoutedEventArgs e)
        {
            TextButtons textButtons = sender as TextButtons;
            foreach (var item in RecipeTypes)
            {
                if (Path.GetFileNameWithoutExtension(textButtons.Tag.ToString()).Replace("_icon","") == item.Tag.ToString())
                {
                    Panel.SetZIndex(item,1);
                    item.Opacity = 1;
                    item.Visibility = Visibility.Visible;
                }
                else
                {
                    Panel.SetZIndex(item, 0);
                    item.Opacity = 0;
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// 载入物品id列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemIdsLoaded(object sender, RoutedEventArgs e)
        {
            IconComboBoxs iconComboBoxs = sender as IconComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.ItemIdSource.ItemDataSource;

            iconComboBoxs.SelectedIndex = 0;
            TextBox box = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            ItemDataGroup first = iconComboBoxs.Items[0] as ItemDataGroup;
            box.Text = first.ItemText;
            Image image = iconComboBoxs.Template.FindName("PART_DisplayIcon", iconComboBoxs) as Image;
            image.Source = first.ItemImage;
            SelectItemIdSource = first;
        }

        /// <summary>
        /// 载入编辑区控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ModifyGridLoaded(object sender, RoutedEventArgs e)
        {
            Grid zone = sender as Grid;

            #region 初始化所有配方类型
            craftingTable = new CraftingTable();
            furnace = new Furnace();
            blastFurnace = new BlastFurnace();
            smithingTable = new SmithingTable();
            smoker = new Smoker();
            stonecutter = new Stonecutter();
            campFire = new CampFire();
            #endregion

            #region 连接所有类型的配方引用
            RecipeTypes = new List<UserControl> { };
            RecipeTypes.Add(craftingTable);
            RecipeTypes.Add(furnace);
            RecipeTypes.Add(blastFurnace);
            RecipeTypes.Add(smithingTable);
            RecipeTypes.Add(smoker);
            RecipeTypes.Add(stonecutter);
            RecipeTypes.Add(campFire);
            #endregion

            #region 添加所有类型的配方
            zone.Children.Add(furnace);
            zone.Children.Add(blastFurnace);
            zone.Children.Add(smithingTable);
            zone.Children.Add(smoker);
            zone.Children.Add(stonecutter);
            zone.Children.Add(craftingTable);
            zone.Children.Add(campFire);
            furnace.Opacity = blastFurnace.Opacity = smithingTable.Opacity = smoker.Opacity = stonecutter.Opacity = campFire.Opacity = 0;
            furnace.Visibility = blastFurnace.Visibility = smithingTable.Visibility = smoker.Visibility = stonecutter.Visibility = campFire.Visibility = Visibility.Collapsed;
            #endregion
        }

        /// <summary>
        /// 获取右侧ID缓存区引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CacheZoneLoaded(object sender, RoutedEventArgs e)
        {
            CacheZone = sender as UniformGrid;
        }

        /// <summary>
        /// 获取窗体可视化区域引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecipeZoneLoaded(object sender, RoutedEventArgs e)
        {
            RecipeZone = sender as Grid;
            RecipeZone.MouseMove += SelectItemMove;
            RecipeZone.MouseLeftButtonUp += RecipeZoneMouseLeftButtonUp;
        }

        /// <summary>
        /// 添加ID缓存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddItemIdKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                IconComboBoxs box = sender as IconComboBoxs;
                ItemDataGroup dataGroup = box.SelectedItem as ItemDataGroup;
                KeyValuePair<string, BitmapImage> a_item = MainWindow.item_database.Where(item => item.Key.Contains(dataGroup.ItemText)).First();
                string image_name = Regex.Match(a_item.Key, @"[a-zA-Z_]+").ToString();
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + image_name + ".png");
                bitmap = ChangeBitmapSize.Magnifier(bitmap, 10);
                BitmapImage bitmapImage = BitmapImageConverter.ToBitmapImage(bitmap);
                Image image = new Image
                {
                    Height = 50,
                    Width = 50,
                    Cursor = Cursors.Hand,
                    Source = bitmapImage,
                    Tag = image_name,
                    ToolTip = Regex.Match(a_item.Key, @"[\u4e00-\u9fa5]+").ToString()
                };
                image.MouseLeftButtonDown += SelectItemClickDown;
                image.MouseRightButtonUp += DeleteItemClick;
                ToolTipService.SetInitialShowDelay(image, 0);
                ToolTipService.SetShowDuration(image, 1000);
                CacheZone.Children.Add(image);
            }
        }

        /// <summary>
        /// 右击删除该物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteItemClick(object sender, MouseButtonEventArgs e)
        {
            UniformGrid parent = (sender as Image).Parent as UniformGrid;
            parent.Children.Remove(sender as Image);
        }

        /// <summary>
        /// 处理开始拖拽物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectItemClickDown(object sender, MouseButtonEventArgs e)
        {
            IsGrabingItem = !IsGrabingItem;
            Image image = sender as Image;
            drag_source = image;
            GrabedImage.Source = image.Source;
            GrabedImage.Tag = image.Tag;
            GrabedImage.Height = GrabedImage.Width = 50;
            GrabedImage.Cursor = Cursors.Hand;
        }

        /// <summary>
        /// 松开后停止拖拽
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void RecipeZoneMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            IsGrabingItem = false;
        }

        /// <summary>
        /// 处理拖拽物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectItemMove(object sender, MouseEventArgs e)
        {
            if (IsGrabingItem && drag_source != null && GrabedImage != null)
            {
                DataObject dataObject = new DataObject(typeof(Image), GrabedImage);
                if(dataObject != null)
                DragDrop.DoDragDrop(drag_source, dataObject, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// 检测预览视图是否有内容重叠
        /// </summary>
        private static int CheckOverLap(Image image)
        {
            int index = -1;
            int count = craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children.Count;
            Image current_image;
            for (int i = 0; i < count; i++)
            {
                current_image = craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children[i] as Image;
                if(current_image.Tag == image.Tag)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        /// <summary>
        /// 更新右侧多选预览视图
        /// </summary>
        /// <param name="image">图像控件</param>
        /// <param name="delete">是否删除</param>
        /// <param name="cover">是否覆盖</param>
        public static void UpdateMultipleItemView(Image image,RecipeModifyTypes modifyTypes,List<Image> images)
        {
            int index = CheckOverLap(image);
            switch (modifyTypes)
            {
                case RecipeModifyTypes.Add:
                    if (index == -1)
                        craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children.Add(image);
                    else
                        craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children[index].Visibility = Visibility.Visible;
                    break;
                case RecipeModifyTypes.Delete:
                    images.Remove(image);
                    int result = 0;
                    result += CraftingTable.FirstItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.SecondItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.ThirdItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.FourthItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.FifthItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.SixthItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.SeventhItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.EighthItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    result += CraftingTable.NinthItem.Find(item => item.Tag == image.Tag) != null ? 1 : 0;
                    if (result >= 2)
                        craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children[index].Visibility = Visibility.Collapsed;
                    else
                        craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children.RemoveAt(index);
                    if (craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children.Count == 1)
                        craftingTable.ItemInfomationWindow.CurrentItemInfomation.Children[0].Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
