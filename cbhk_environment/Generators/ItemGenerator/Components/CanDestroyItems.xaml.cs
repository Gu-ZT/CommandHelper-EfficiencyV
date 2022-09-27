using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// CanDestroyItems.xaml 的交互逻辑
    /// </summary>
    public partial class CanDestroyItems : UserControl
    {
        public CanDestroyItems()
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
            CanDestroyItems canDestroyItems = button.FindParent<CanDestroyItems>();
            StackPanel parent = canDestroyItems.FindParent<StackPanel>();
            #region 删除对应数据
            int index = parent.Children.IndexOf(canDestroyItems);
            item_datacontext.CanDestroyBlocks.RemoveAt(index);
            #endregion

            //删除自己
            parent.Children.Remove(canDestroyItems);
        }

        bool ItemLoaded = false;

        /// <summary>
        /// 加载所有子级成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanDestroyItemLoaded(object sender, RoutedEventArgs e)
        {
            IconComboBoxs iconComboBoxs = sender as IconComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.ItemIdSource.ItemDataSource;

            iconComboBoxs.SelectedIndex = 0;
            TextBox box = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            ItemDataGroup first = iconComboBoxs.Items[0] as ItemDataGroup;
            box.Text = first.ItemText;
            Image image = iconComboBoxs.Template.FindName("PART_DisplayIcon", iconComboBoxs) as Image;
            image.Source = first.ItemImage;
            ItemLoaded = true;
            CanDestroyItemSelectionChanged(iconComboBoxs,null);
        }

        /// <summary>
        /// 更新已选中的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanDestroyItemSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ItemLoaded)
            {
                IconComboBoxs current_box = sender as IconComboBoxs;
                ItemDataGroup current_data = current_box.SelectedItem as ItemDataGroup;
                CanDestroyItems canDestroyItems = current_box.FindParent<CanDestroyItems>();
                StackPanel parent = canDestroyItems.FindParent<StackPanel>();
                int index = parent.Children.IndexOf(canDestroyItems);
                #region 删除上一条数据
                if (item_datacontext.CanDestroyBlocks.Count > 0 && (item_datacontext.CanDestroyBlocks.Count - 1) >= index)
                {
                    item_datacontext.CanDestroyBlocks.RemoveAt(index);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.CanDestroyBlocks.Count - 1) >= 0)
                    item_datacontext.CanDestroyBlocks.Insert(index, current_data.ItemText);
                else
                    item_datacontext.CanDestroyBlocks.Add(current_data.ItemText);
                #endregion
            }
        }
    }
}
