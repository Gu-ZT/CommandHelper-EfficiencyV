using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// EnchantmentItems.xaml 的交互逻辑
    /// </summary>
    public partial class EnchantmentItems : UserControl
    {
        public EnchantmentItems()
        {
            InitializeComponent();
        }

        private bool IdLoaded = false;
        private bool LevelLoaded = false;

        /// <summary>
        /// 载入附魔列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnchantmentIdLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs iconComboBoxs = sender as TextComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.EnchantmentIdSource;

            iconComboBoxs.SelectedIndex = 0;
            TextBox box = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            string first = iconComboBoxs.Items[0] as string;
            box.Text = first;
            IdLoaded = true;
            EnchantmentIdSelectionChanged(iconComboBoxs, null);
        }

        /// <summary>
        /// 加载附魔等级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnchantmentLevelLoaded(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns colorNumbericUpDowns = sender as ColorNumbericUpDowns;
            colorNumbericUpDowns.Text = "0";
            LevelLoaded = true;
            EnchantmentLevelSelectionChanged(colorNumbericUpDowns, null);
        }

        /// <summary>
        /// 删除该控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconTextButtons_Click(object sender, RoutedEventArgs e)
        {
            IconTextButtons button = sender as IconTextButtons;
            EnchantmentItems enchantmentItems = button.FindParent<EnchantmentItems>();
            StackPanel parent = enchantmentItems.FindParent<StackPanel>();
            #region 删除对应数据
            int index = parent.Children.IndexOf(enchantmentItems);
            item_datacontext.EnchantmentIDs.RemoveAt(index);
            if((item_datacontext.EnchantmentLevels.Count - 1) >= index)
            item_datacontext.EnchantmentLevels.RemoveAt(index);
            #endregion

            //删除自己
            parent.Children.Remove(enchantmentItems);
        }

        /// <summary>
        /// 更新附魔id列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnchantmentIdSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IdLoaded)
            {
                TextComboBoxs current_box = sender as TextComboBoxs;
                string current_data = current_box.SelectedItem as string;

                EnchantmentItems control_parent = current_box.FindParent<EnchantmentItems>();
                StackPanel parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.EnchantmentIDs.Count > 0 && (item_datacontext.EnchantmentIDs.Count - 1) >= index)
                {
                    item_datacontext.EnchantmentIDs.RemoveAt(index);
                }
                #endregion

                #region 添加当前选中的成员

                string current_key = MainWindow.enchantment_databse.Where(item => Regex.Match(item.Value, @"[\u4E00-\u9FFF]+").ToString() == current_data).Select(item => item.Key).First();

                if ((item_datacontext.EnchantmentIDs.Count - 1) >= 0)
                    item_datacontext.EnchantmentIDs.Insert(index, current_key);
                else
                    item_datacontext.EnchantmentIDs.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 更新附魔等级列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnchantmentLevelSelectionChanged(object sender, RoutedEventArgs e)
        {
            if(LevelLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                EnchantmentItems control_parent = current_box.FindParent<EnchantmentItems>();
                StackPanel parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.EnchantmentLevels.Count > 0 && (item_datacontext.EnchantmentLevels.Count - 1) >= index)
                {
                    item_datacontext.EnchantmentLevels.RemoveAt(index);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.EnchantmentLevels.Count - 1) >= 0)
                    item_datacontext.EnchantmentLevels.Insert(index, current_box.Text);
                else
                    item_datacontext.EnchantmentLevels.Add(current_box.Text);
                #endregion
            }
        }
    }
}
