using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using System.Windows;
using System.Windows.Controls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// CanPlaceOnItems.xaml 的交互逻辑
    /// </summary>
    public partial class CanPlaceOnItems : UserControl
    {

        public CanPlaceOnItems()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 删除该控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconTextButtons_Click(object sender, RoutedEventArgs e)
        {
            IconTextButtons button = sender as IconTextButtons;
            CanPlaceOnItems canPlaceOnItems = button.FindParent<CanPlaceOnItems>();
            StackPanel parent = canPlaceOnItems.FindParent<StackPanel>();
            #region 删除对应数据
            int index = parent.Children.IndexOf(canPlaceOnItems);
            item_datacontext.CanPlaceOnBlocks.RemoveAt(index);
            #endregion

            //删除自己
            parent.Children.Remove(canPlaceOnItems);
        }

        private bool ItemLoaded = false;

        /// <summary>
        /// 加载所有子级成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanPlaceOnItemLoaded(object sender, RoutedEventArgs e)
        {
            IconComboBoxs iconComboBoxs = sender as IconComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.ItemIdSource;
            ItemLoaded = true;
            CanPlaceOnItemSelectionChanged(iconComboBoxs, null);
        }

        /// <summary>
        /// 更新已选中的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanPlaceOnItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ItemLoaded)
            {
                IconComboBoxs current_box = sender as IconComboBoxs;
                IconComboBoxItem current_data = current_box.SelectedItem as IconComboBoxItem;
                CanPlaceOnItems canPlaceOnItems = current_box.FindParent<CanPlaceOnItems>();
                StackPanel parent = canPlaceOnItems.FindParent<StackPanel>();
                int index = parent.Children.IndexOf(canPlaceOnItems);
                #region 删除上一条数据
                if (item_datacontext.CanPlaceOnBlocks.Count > 0 && (item_datacontext.CanPlaceOnBlocks.Count - 1) >= index)
                {
                    item_datacontext.CanPlaceOnBlocks.RemoveAt(index);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.CanPlaceOnBlocks.Count - 1) >= 0)
                    item_datacontext.CanPlaceOnBlocks.Insert(index, current_data.ComboBoxItemText);
                else
                    item_datacontext.CanPlaceOnBlocks.Add(current_data.ComboBoxItemText);
                #endregion
            }
        }
    }
}
