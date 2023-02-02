using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
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
        //保存复选框所在父级栈表
        ItemsControl checkbox_parent = null;
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
        List<RichCheckBoxs> VisibleCheckBoxs = new List<RichCheckBoxs> { };

        private ObservableCollection<RichCheckBoxs> tagItems = new ObservableCollection<RichCheckBoxs> { };
        public ObservableCollection<RichCheckBoxs> TagItems
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
            checkbox_parent = sender as ItemsControl;
            foreach (var item in MainWindow.TagSpawnerItemCheckBoxList)
            {
                item.Checked += ItemChecked;
                item.Unchecked += ItemUnChecked;
                item.MouseEnter += ItemMouseEnter;
                TagItems.Add(item);
                BlockAndItemKeys.Add(item.HeaderText);
            }
            foreach (var item in MainWindow.EntityCheckBoxList)
            {
                item.Checked += ItemChecked;
                item.Unchecked += ItemUnChecked;
                item.MouseEnter += ItemMouseEnter;
                TagItems.Add(item);
                BlockAndItemKeys.Add(item.HeaderText);
            }
            checkbox_parent.ItemsSource = TagItems;
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
        private void ItemUnChecked(object sender, RoutedEventArgs e)
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
        private void ItemChecked(object sender, RoutedEventArgs e)
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
            MessageBox.Show(SearchText);
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
                foreach (RichCheckBoxs item in checkbox_parent.Items)
                {
                    item.IsChecked = true;
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
                foreach (RichCheckBoxs item in checkbox_parent.Items)
                {
                    item.IsChecked = !item.IsChecked.Value;
                }
            }
            else
                foreach (RichCheckBoxs item in VisibleCheckBoxs)
                {
                    item.IsChecked = !item.IsChecked;
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
            if (SearchText.Trim() == "")
            {
                foreach (RichCheckBoxs item in checkbox_parent.Items)
                {
                    item.Visibility = Visibility.Visible;
                }
                VisibleCheckBoxs.Clear();
                return;
            }
            #endregion

            //暂时全部隐藏
            foreach (RichCheckBoxs item in checkbox_parent.Items)
            {
                item.Visibility = Visibility.Collapsed;
            }
            List<string> result = BlockAndItemKeys.Where(item => item.StartsWith(SearchText)).Distinct().ToList();
            //更新当前新可视化成员列表
            VisibleCheckBoxs.Clear();

            foreach (string item in result)
            {
                RichCheckBoxs box = checkbox_parent.Items[BlockAndItemKeys.IndexOf(item)] as RichCheckBoxs;
                if (box.HeaderText.Contains(item) || item.Contains(box.HeaderText))
                {
                    box.Visibility = Visibility.Visible;
                    VisibleCheckBoxs.Add(box);
                }
            }
        }
    }
}
