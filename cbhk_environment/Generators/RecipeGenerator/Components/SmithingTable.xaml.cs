using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static cbhk_environment.Generators.RecipeGenerator.recipe_datacontext;

namespace cbhk_environment.Generators.RecipeGenerator.Components
{
    /// <summary>
    /// SmithingTable.xaml 的交互逻辑
    /// </summary>
    public partial class SmithingTable : UserControl
    {
        //获取基础物品的引用
        Image BasedItem = null;
        //获取材料的引用
        Image AdditionItem = null;
        //空内容图像
        ImageSource empty_image = null;
        //显示物品信息的窗体
        public ItemsDisplayer ItemInfomationWindow = new ItemsDisplayer();
        //背景图文件路径
        string background_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Recipe\\images\\smithing_table.png";

        //是否显示当前槽位物品
        public static bool DisplayCurrentItem = false;

        //获取经验的引用
        Slider RecipeCount = null;
        //获取文件名引用
        public TextBox RecipeFileName = null;

        #region 当前槽位被点击子级物品,用于绑定单个物品的标签
        private Image currentSubItem = null;
        public Image CurrentSubItem
        {
            get { return currentSubItem; }
            set { currentSubItem = value; }
        }
        #endregion

        #region 当前选中的物品
        private Image currentItemImage = null;
        public Image CurrentItemImage
        {
            get { return currentItemImage; }
            set { currentItemImage = value; }
        }
        #endregion

        #region 生成结果
        private string recipe_result = "";
        public string RecipeResult
        {
            get
            {
                string result;
                string CountData = ",\"count\":";

                RecipeCount.Value = int.Parse(RecipeCount.Value.ToString().Contains(".") ? RecipeCount.Value.ToString().Split('.')[0].Replace("-", "") : RecipeCount.Value.ToString());
                CountData += RecipeCount.Value + ",";
                result = "\"result\":\"minecraft:" + recipe_result + "\"" + CountData;
                return result;
            }
            set { recipe_result = value; }
        }
        #endregion

        #region 最终数据
        public string RecipeData
        {
            get
            {
                string result = "{\"type\":\"minecraft:smithing\"," + (GroupId.Text.Trim() != "" ? "\"group\":\"" + GroupId.Text + "\"," : "");
                string BaseData = "";
                string AdditionData = "";
                if(BasedItem.Tag != null)
                {
                    BaseData = string.Join("", "\"item\":\"minecraft:" + BasedItem.Tag.ToString() + "\""+ (BasedItem.Name.Trim() != "" ? ",\"tag\":\"" + BasedItem.Name + "\"" : "") + "");
                    BaseData = "\"base\":{" + BaseData + "},";
                }
                if(AdditionItem.Tag != null)
                {
                    AdditionData = string.Join("", "\"item\":\"minecraft:" + AdditionItem.Tag.ToString() + "\""+ (AdditionItem.Name.Trim() != "" ? ",\"tag\":\"" + AdditionItem.Name + "\"" : "") + "");
                    AdditionData = "\"addition\":{" + AdditionData + "},";
                }
                result += BaseData + AdditionData + RecipeResult + "}";
                return result;
            }
        }
        #endregion

        public SmithingTable()
        {
            InitializeComponent();

            ItemInfomationWindow.DataContext = this;
        }

        /// <summary>
        /// 更新右侧多选预览视图
        /// </summary>
        /// <param name="image">图像控件</param>
        /// <param name="delete">是否删除</param>
        /// <param name="cover">是否覆盖</param>
        public void UpdateMultipleItemView(Image new_image)
        {
            int index = -1;
            for (int i = 0; i < ItemInfomationWindow.CurrentItemInfomation.Children.Count; i++)
            {
                Image item = ItemInfomationWindow.CurrentItemInfomation.Children[i] as Image;
                if (item.Uid == new_image.Uid)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
                ItemInfomationWindow.CurrentItemInfomation.Children[index] = new_image;
            else
                ItemInfomationWindow.CurrentItemInfomation.Children.Add(new_image);
        }

        /// <summary>
        /// 绑定单个物品的tag数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemTagBinder(object sender, MouseButtonEventArgs e)
        {
            #region 动态绑定每个槽位的标签信息
            CurrentSubItem = sender as Image;
            switch (CurrentSubItem.Uid)
            {
                case "0":
                    CurrentSubItem = BasedItem;
                    break;
                case "1":
                    CurrentSubItem = AdditionItem;
                    break;
            }
            Binding tag_binder = new Binding()
            {
                Path = new PropertyPath("CurrentSubItem.Name"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                FallbackValue = ""
            };
            BindingOperations.SetBinding(ItemInfomationWindow.TagBox, TextBox.TextProperty, tag_binder);
            #endregion
        }

        /// <summary>
        /// 更新物品
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemUpdate(object sender, DragEventArgs e)
        {
            IsGrabingItem = !IsGrabingItem;

            #region 放大当前图像
            Image current_item = sender as Image;
            string item_id = GrabedImage.Tag.ToString();
            string image_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\" + item_id + ".png";
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image_path);
            bitmap = GeneralTools.ChangeBitmapSize.Magnifier(bitmap, 10);
            BitmapImage bitmapImage = GeneralTools.BitmapImageConverter.ToBitmapImage(bitmap);
            #endregion

            #region 获取空图像引用
            if (empty_image == null)
                empty_image = current_item.Source;
            #endregion

            #region 右侧预览新增图像
            Image cache_image = new Image()
            {
                Height = current_item.Height,
                Cursor = Cursors.Hand,
                Width = current_item.Width,
                Tag = item_id,
                ToolTip = item_id + " 右击删除",
                Uid = current_item.Uid,
            };
            ToolTipService.SetInitialShowDelay(cache_image, 0);
            ToolTipService.SetShowDuration(cache_image, 1000);
            cache_image.MouseLeftButtonDown += ItemTagBinder;
            #endregion

            ToolTipService.SetInitialShowDelay(current_item, 0);
            ToolTipService.SetShowDuration(current_item, 1000);

            current_item.Tag = item_id;
            switch (current_item.Uid)
            {
                case "0":
                    current_item.Source = bitmapImage;
                    cache_image.Source = current_item.Source;
                    BasedItem = current_item;
                    UpdateMultipleItemView(cache_image);
                    break;
                case "1":
                    current_item.Source = bitmapImage;
                    cache_image.Source = current_item.Source;
                    AdditionItem = current_item;
                    UpdateMultipleItemView(cache_image);
                    break;
                case "2":
                    current_item.Source = bitmapImage;
                    RecipeResult = current_item.Tag.ToString();
                    break;
            }
        }

        /// <summary>
        /// 移除Tag中的非法字符
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RemoveIllegalCharacter(object sender, KeyEventArgs e)
        {
            TextBox current_box = sender as TextBox;
            current_box.Text = Regex.Replace(current_box.Text, @"[\\/:*?" + "\"" + "<>|]", "").ToString();
            //current_box.Text = Regex.Replace(current_box.Text, @"^\d+", "").ToString();
        }

        /// <summary>
        /// 载入背景图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundLoaded(object sender, RoutedEventArgs e)
        {
            Image current_image = sender as Image;
            current_image.Source = new BitmapImage(new Uri(background_path, UriKind.Absolute));
        }

        /// <summary>
        /// 获取文件名引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecipeFileNameLoaded(object sender, RoutedEventArgs e)
        {
            RecipeFileName = sender as TextBox;
        }

        /// <summary>
        /// 获取经验引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecipeCountLoaded(object sender, RoutedEventArgs e)
        {
            RecipeCount = sender as Slider;
        }

        /// <summary>
        /// 获取基础物品引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasedItemLoaded(object sender, RoutedEventArgs e)
        {
            BasedItem = sender as Image;
        }

        /// <summary>
        /// 获取材料物品引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AdditionItemLoaded(object sender, RoutedEventArgs e)
        {
            AdditionItem = sender as Image;
        }

        /// <summary>
        /// 选中物品,更新数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectItemClick(object sender, MouseButtonEventArgs e)
        {
            DisplayCurrentItem = !DisplayCurrentItem;
            Image image = sender as Image;
            Border border = image.Parent as Border;
            if (image != CurrentItemImage && CurrentItemImage != null)
            {
                Border last_parent = CurrentItemImage.Parent as Border;
                last_parent.BorderThickness = new Thickness(0);
                DisplayCurrentItem = true;
            }
            border.BorderThickness = DisplayCurrentItem ? new Thickness(5) : new Thickness(0);
            CurrentItemImage = image;

            #region 动态"绑定"每个槽位的物品信息
            switch (image.Uid)
            {
                case "0":
                    if (BasedItem.Tag != null)
                        foreach (Image item in ItemInfomationWindow.CurrentItemInfomation.Children)
                        {
                            if (BasedItem.Tag == item.Tag && BasedItem.Uid == item.Uid)
                                item.Visibility = Visibility.Visible;
                            else
                                item.Visibility = Visibility.Collapsed;
                        }
                    else
                        foreach (Image item in ItemInfomationWindow.CurrentItemInfomation.Children)
                            item.Visibility = Visibility.Collapsed;
                    break;
                case "1":
                    if (AdditionItem.Tag != null)
                        foreach (Image item in ItemInfomationWindow.CurrentItemInfomation.Children)
                        {
                            if (AdditionItem.Tag == item.Tag && AdditionItem.Uid == item.Uid)
                                item.Visibility = Visibility.Visible;
                            else
                                item.Visibility = Visibility.Collapsed;
                        }
                    else
                        foreach (Image item in ItemInfomationWindow.CurrentItemInfomation.Children)
                            item.Visibility = Visibility.Collapsed;
                    break;
            }
            #endregion

            if (DisplayCurrentItem)
            {
                Window window = Window.GetWindow(image);
                Point point = image.TransformToAncestor(window).Transform(new Point(0, 0));
                point = PointToScreen(point);
                ItemInfomationWindow.Left = point.X - 180;
                ItemInfomationWindow.Top = point.Y - 400;
                ItemInfomationWindow.Show();
            }
            else
                ItemInfomationWindow.Hide();
        }
    }
}
