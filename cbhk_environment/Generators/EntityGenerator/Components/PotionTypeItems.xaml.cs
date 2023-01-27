using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.EntityGenerator.Components
{
    /// <summary>
    /// PotionTypeItems.xaml 的交互逻辑
    /// </summary>
    public partial class PotionTypeItems : UserControl
    {
        public PotionTypeItems()
        {
            InitializeComponent();
        }

        StackPanel parent = null;
        private bool DurationLoaded = false;
        private bool LevelLoaded = false;
        private bool IdLoaded = false;

        /// <summary>
        /// 删除当前控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconTextButtons_Click(object sender, RoutedEventArgs e)
        {
            IconTextButtons button = sender as IconTextButtons;
            PotionTypeItems potionTypeItems = button.FindParent<PotionTypeItems>();
            StackPanel parent = potionTypeItems.FindParent<StackPanel>();

            #region 删除对应数据
            int index = parent.Children.IndexOf(potionTypeItems);
            if ((entity_datacontext.MobEffectIDs.Count - 1) >= index)
                entity_datacontext.MobEffectIDs.RemoveAt(index);

            if ((entity_datacontext.MobEffectLevels.Count - 1) >= index)
                entity_datacontext.MobEffectLevels.RemoveAt(index);

            if ((entity_datacontext.MobEffectDurations.Count - 1) >= index)
                entity_datacontext.MobEffectDurations.RemoveAt(index);
            #endregion

            //删除自己
            parent.Children.Remove(potionTypeItems);
        }

        /// <summary>
        /// 载入药水类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectIdLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox iconComboBoxs = sender as ComboBox;
            iconComboBoxs.ItemsSource = MainWindow.MobEffectIdSource;
            iconComboBoxs.SelectedIndex = 0;
            IdLoaded = true;
            MobEffectIdSelectionChanged(iconComboBoxs, null);
        }

        /// <summary>
        /// 载入药水持续时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectDurationLoaded(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns colorNumbericUpDowns = sender as ColorNumbericUpDowns;
            colorNumbericUpDowns.Text = "0";
            DurationLoaded = true;
            MobEffectDurationSelectionChanged(colorNumbericUpDowns, null);
        }

        /// <summary>
        /// 载入药水等级
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectLevelLoaded(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns colorNumbericUpDowns = sender as ColorNumbericUpDowns;
            colorNumbericUpDowns.Text = "0";
            LevelLoaded = true;
            MobEffectLevelSelectionChanged(colorNumbericUpDowns, null);
        }

        /// <summary>
        /// 更新药水效果id列表成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectIdSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IdLoaded)
            {
                ComboBox current_box = sender as ComboBox;
                IconComboBoxItem current_item = current_box.SelectedItem as IconComboBoxItem;

                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                StackPanel parent = control_parent.FindParent<StackPanel>();
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (entity_datacontext.MobEffectIDs.Count > 0 && (entity_datacontext.MobEffectIDs.Count - 1) >= index)
                {
                    string remove_key = entity_datacontext.MobEffectDurations.ElementAt(index);
                    entity_datacontext.MobEffectIDs.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                string current_key = MainWindow.mob_effect_database.Where(item => Regex.Match(item.Value, @"[\u4E00-\u9FFF]+").ToString() == current_item.ComboBoxItemText).First().Value;
                current_key = Regex.Match(current_key, @"[\d]+").ToString();

                if ((entity_datacontext.MobEffectIDs.Count - 1) >= 0)
                    entity_datacontext.MobEffectIDs.Insert(index, current_key);
                else
                    entity_datacontext.MobEffectIDs.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 药水效果时长更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectDurationSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (DurationLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                string current_data = current_box.Text;
                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                parent = control_parent.FindParent<StackPanel>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;

                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (entity_datacontext.MobEffectDurations.Count > 0 && (entity_datacontext.MobEffectDurations.Count - 1) >= index)
                {
                    string remove_key = entity_datacontext.MobEffectDurations.ElementAt(index);
                    entity_datacontext.MobEffectDurations.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                if ((entity_datacontext.MobEffectDurations.Count - 1) >= 0)
                    entity_datacontext.MobEffectDurations.Insert(index, current_data);
                else
                    entity_datacontext.MobEffectDurations.Add(current_data);
                #endregion
            }
        }

        /// <summary>
        /// 药水效果等级更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectLevelSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (LevelLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                string current_data = current_box.Text;
                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                parent = control_parent.FindParent<StackPanel>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;

                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (entity_datacontext.MobEffectLevels.Count > 0 && (entity_datacontext.MobEffectLevels.Count - 1) >= index)
                {
                    string remove_key = entity_datacontext.MobEffectLevels.ElementAt(index);
                    entity_datacontext.MobEffectLevels.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                if ((entity_datacontext.MobEffectLevels.Count - 1) >= 0)
                    entity_datacontext.MobEffectLevels.Insert(index, current_data);
                else
                    entity_datacontext.MobEffectLevels.Add(current_data);
                #endregion
            }
        }
    }
}
