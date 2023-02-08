using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

        //存储最终生成的列表
        List<string> BlocksAndItems = new List<string> { };
        List<string> Entities = new List<string>();

        //保存搜索框的值引用
        private string searchText = "";
        public string SearchText
        {
            get
            {
                return searchText;
            }
            set
            {
                searchText = value;
                OnPropertyChanged();
            }
        }
        //保存所有RichCheckBox的键
        List<string> BlockAndItemKeys = new List<string> { };
        //保存所有可视化成员
        List<TagItemTemplate> VisibleCheckBoxs = new List<TagItemTemplate> { };

        #region 所有标签成员
        private ObservableCollection<TagItemTemplate> tagItems = new ObservableCollection<TagItemTemplate> { };
        public ObservableCollection<TagItemTemplate> TagItems
        {
            get
            {
                return tagItems;
            }
            set
            {
                tagItems = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //载入进程锁
        object tagItemsLock = new object();

        //滚动视图引用
        ScrollViewer scrollViewer = null;

        //标签容器
        ListBox TagZone = null;

        #region 当前选中成员
        private TagItemTemplate selectedItem = null;
        public TagItemTemplate SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
            }
        }
        #endregion

        public tag_datacontext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            #endregion

            #region 异步载入标签成员
            BindingOperations.EnableCollectionSynchronization(TagItems, tagItemsLock);
            Task.Run(() =>
            {
                lock(tagItemsLock)
                {
                    string uriDirectoryPath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\item_and_block_images\\";
                    string urlPath = "";
                    string id = "";
                    foreach (var item in MainWindow.ItemDataBase)
                    {
                        id = item.Key.Substring(0, item.Key.IndexOf(":"));
                        urlPath = uriDirectoryPath + id + ".png";
                        if (File.Exists(urlPath))
                        {
                            int matchCount = MainWindow.EntityDataBase.Where(block=> block.Key.Substring(0, block.Key.IndexOf(':')) == id).Count();
                            string uid = matchCount > 0 ?"Item":"Entity";
                            TagItems.Add(new TagItemTemplate()
                            {
                                ContentImage = new Uri(urlPath,UriKind.Absolute),
                                ContentString = item.Key.Replace(":", " "),
                                TagString = uid,
                                BeChecked = false
                            });
                        }
                    }
                }
            });
            #endregion
        }

        /// <summary>
        /// 鼠标移入时加载图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemMouseEnter(object sender, MouseEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;
            string imagePath = richCheckBoxs.Tag.ToString();
            if (File.Exists(imagePath) && richCheckBoxs.ContentImage == null)
            richCheckBoxs.ContentImage = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
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
        }

        /// <summary>
        /// 更新过滤类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        /// <summary>
        /// 成员已被取消选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemUnChecked(object sender, RoutedEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;
            if(richCheckBoxs.Uid == "Item")
            BlocksAndItems.Remove("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
            else
                if (richCheckBoxs.Uid == "Entity")
                Entities.Remove("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
        }

        /// <summary>
        /// 成员已被选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ItemChecked(object sender, RoutedEventArgs e)
        {
            RichCheckBoxs richCheckBoxs = sender as RichCheckBoxs;

            if (richCheckBoxs.Uid == "Item")
                BlocksAndItems.Add("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
            else
                if (richCheckBoxs.Uid == "Entity")
                Entities.Add("\"minecraft:" + Regex.Match(richCheckBoxs.HeaderText.Trim(), "[a-zA-z_]+").ToString() + "\",");
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
            string result = string.Join("\r\n",BlocksAndItems) + string.Join("\r\n",Entities).TrimEnd(',');
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
                foreach (var item in TagItems)
                {
                    SetItemValue(item,value);
                }
            }
            else
                for (int i = 0; i < VisibleCheckBoxs.Count; i++)
                {
                    SetItemValue(VisibleCheckBoxs[i],value);
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
                foreach (var item in TagItems)
                {
                    ReverseValue(item);
                }
            }
            else
                foreach (var item in VisibleCheckBoxs)
                {
                    ReverseValue(item);
                }
        }

        /// <summary>
        /// 搜索服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchBoxKeyUp(object sender, TextChangedEventArgs e)
        {
            #region 为空则恢复所有RichCheckBox的可见性
            string content = SearchText;
            if (content.Trim() == "")
            {
                foreach (var item in TagZone.Items)
                {
                    ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    listBoxItem.Visibility = Visibility.Visible;
                }
                VisibleCheckBoxs.Clear();
                return;
            }
            #endregion

            //暂时全部隐藏
            foreach (var item in TagZone.Items)
            {
                ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                listBoxItem.Visibility = Visibility.Collapsed;
            }
            List<string> result = BlockAndItemKeys.Where(item => item.StartsWith(SearchText)).ToList();
            //更新当前新可视化成员列表
            VisibleCheckBoxs.Clear();

            if(result.Count > 0)
            foreach (string item in result)
            {
                TagItemTemplate tagItemTemplate = TagZone.Items[BlockAndItemKeys.IndexOf(item)] as TagItemTemplate;
                ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(tagItemTemplate) as ListBoxItem;
                ContentPresenter contentPresenter = ChildrenHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                TextBlock textblock = contentPresenter.ContentTemplate.FindName("contentbox", contentPresenter) as TextBlock;

                if (textblock.Text.StartsWith(SearchText) || textblock.Text.Substring(textblock.Text.IndexOf(' ') + 1).StartsWith(SearchText))
                {
                    listBoxItem.Visibility = Visibility.Visible;
                    VisibleCheckBoxs.Add(tagItemTemplate);
                }
            }
            else
                foreach (var item in TagItems)
                {
                    ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                    ContentPresenter contentPresenter = ChildrenHelper.FindVisualChild<ContentPresenter>(listBoxItem);
                    TextBlock textblock = contentPresenter.ContentTemplate.FindName("contentbox", contentPresenter) as TextBlock;

                    if (textblock.Text.StartsWith(SearchText) || textblock.Text.Substring(textblock.Text.IndexOf(' ') + 1).StartsWith(SearchText))
                    {
                        listBoxItem.Visibility = Visibility.Visible;
                        VisibleCheckBoxs.Add(item);
                    }
                }
        }

        public void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = sender as ScrollViewer;
        }

        public void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            TagZone = sender as ListBox;
        }

        public void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = e.Source
            };
            ScrollViewer scv = (ScrollViewer)sender;
            scv.RaiseEvent(eventArg);
            e.Handled = true;
        }

        public void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer_PreviewMouseWheel(scrollViewer, e);
        }

        /// <summary>
        /// 设置目标成员的值
        /// </summary>
        /// <param name="CurrentItem"></param>
        /// <param name="ItemValue"></param>
        private void SetItemValue(object CurrentItem,bool ItemValue)
        {
            TagItemTemplate tagItemTemplate = CurrentItem as TagItemTemplate;
            ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(CurrentItem) as ListBoxItem;
            ContentPresenter contentPresenter = ChildrenHelper.FindVisualChild<ContentPresenter>(listBoxItem);
            IconCheckBoxs iconCheckBoxs = contentPresenter.ContentTemplate.FindName("checkbox", contentPresenter) as IconCheckBoxs;
            TextBlock textblock = contentPresenter.ContentTemplate.FindName("contentbox", contentPresenter) as TextBlock;
            string itemString = "";

            if(textblock.Text.Trim().Length > 0)
            {
                itemString = textblock.Text.Trim().Substring(0, textblock.Text.Trim().IndexOf(' '));
                if (Regex.IsMatch(textblock.Text.Trim(), "[a-zA-z_]+"))
                {
                    iconCheckBoxs.IsChecked = ItemValue;
                    if (iconCheckBoxs.IsChecked.Value)
                    {
                        if (tagItemTemplate.TagString == "Item")
                            BlocksAndItems.Add("\"minecraft:" + itemString + "\",");
                        else
                        if (tagItemTemplate.TagString == "Entity")
                            Entities.Add("\"minecraft:" + itemString + "\",");
                    }
                    else
                    {
                        if (tagItemTemplate.TagString == "Item")
                            BlocksAndItems.Remove("\"minecraft:" + itemString + "\",");
                        else
                        if (tagItemTemplate.TagString == "Entity")
                            Entities.Remove("\"minecraft:" + itemString + "\",");
                    }
                }
            }
        }

        /// <summary>
        /// 反转目标成员的值
        /// </summary>
        /// <param name="CurrentItem"></param>
        private void ReverseValue(object CurrentItem)
        {
            TagItemTemplate tagItemTemplate = CurrentItem as TagItemTemplate;
            ListBoxItem listBoxItem = TagZone.ItemContainerGenerator.ContainerFromItem(CurrentItem) as ListBoxItem;
            ContentPresenter contentPresenter = ChildrenHelper.FindVisualChild<ContentPresenter>(listBoxItem);
            IconCheckBoxs iconCheckBoxs = contentPresenter.ContentTemplate.FindName("checkbox", contentPresenter) as IconCheckBoxs;
            TextBlock textblock = contentPresenter.ContentTemplate.FindName("contentbox", contentPresenter) as TextBlock;

            string itemString = "";
            if(textblock.Text.Trim().Length > 0)
            {
                itemString = textblock.Text.Trim().Substring(0, textblock.Text.Trim().IndexOf(' '));
                if (Regex.IsMatch(textblock.Text.Trim(), "[a-zA-z_]+"))
                {
                    iconCheckBoxs.IsChecked = !iconCheckBoxs.IsChecked.Value;
                    if (iconCheckBoxs.IsChecked.Value)
                    {
                        if (tagItemTemplate.TagString == "Item")
                            BlocksAndItems.Add("\"minecraft:" + itemString + "\",");
                        else
                        if (tagItemTemplate.TagString == "Entity")
                            Entities.Add("\"minecraft:" + itemString + "\",");
                    }
                    else
                    {
                        if (tagItemTemplate.TagString == "Item")
                            BlocksAndItems.Remove("\"minecraft:" + itemString + "\",");
                        else
                        if (tagItemTemplate.TagString == "Entity")
                            Entities.Remove("\"minecraft:" + itemString + "\",");
                    }
                }
            }
        }

        public void ListBoxClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedItem != null)
            {
                ReverseValue(TagZone.SelectedItem);
            }
        }
    }

    /// <summary>
    /// 载入成员的属性模板
    /// </summary
    public class TagItemTemplate
    {
        public Uri ContentImage { get; set; }
        public string TagString { get; set; }
        public string ContentString { get; set; }
        public bool? BeChecked { get; set; }
    }
}
