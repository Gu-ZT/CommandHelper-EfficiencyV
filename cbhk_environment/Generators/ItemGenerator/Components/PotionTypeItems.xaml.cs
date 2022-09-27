using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using cbhk_environment.ControlsDataContexts;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace cbhk_environment.Generators.ItemGenerator.Components
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
        /// 删除当前药水效果
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
            if ((item_datacontext.MobEffectIDs.Count - 1) >= index)
                item_datacontext.MobEffectIDs.RemoveAt(index);

            if ((item_datacontext.MobEffectLevels.Count - 1) >= index)
                item_datacontext.MobEffectLevels.RemoveAt(index);

            if ((item_datacontext.MobEffectDurations.Count - 1) >= index)
                item_datacontext.MobEffectDurations.RemoveAt(index);
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
            IconComboBoxs iconComboBoxs = sender as IconComboBoxs;
            iconComboBoxs.ItemsSource = MainWindow.MobEffectIdSource.ItemDataSource;

            iconComboBoxs.SelectedIndex = 0;
            TextBox box = iconComboBoxs.Template.FindName("EditableTextBox", iconComboBoxs) as TextBox;
            ItemDataGroup first = iconComboBoxs.Items[0] as ItemDataGroup;
            box.Text = first.ItemText;
            Image image = iconComboBoxs.Template.FindName("PART_DisplayIcon", iconComboBoxs) as Image;
            image.Source = first.ItemImage;
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
            if(IdLoaded)
            {
                IconComboBoxs current_box = sender as IconComboBoxs;
                ItemDataGroup current_item = current_box.SelectedItem as ItemDataGroup;

                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                StackPanel parent = control_parent.FindParent<StackPanel>();
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.MobEffectIDs.Count > 0 && (item_datacontext.MobEffectIDs.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.MobEffectDurations.ElementAt(index);
                    item_datacontext.MobEffectIDs.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                string current_key = MainWindow.mob_effect_database.Where(item => Regex.Match(item.Value, @"[\u4E00-\u9FFF]+").ToString() == current_item.ItemText).First().Value;
                current_key = Regex.Match(current_key,@"[\d]+").ToString();

                if ((item_datacontext.MobEffectIDs.Count - 1) >= 0)
                    item_datacontext.MobEffectIDs.Insert(index, current_key);
                else
                    item_datacontext.MobEffectIDs.Add(current_key);
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
            if(DurationLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                string current_data = current_box.Text;
                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                parent = control_parent.FindParent<StackPanel>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;

                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.MobEffectDurations.Count > 0 && (item_datacontext.MobEffectDurations.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.MobEffectDurations.ElementAt(index);
                    item_datacontext.MobEffectDurations.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.MobEffectDurations.Count - 1) >= 0)
                    item_datacontext.MobEffectDurations.Insert(index, current_data);
                else
                    item_datacontext.MobEffectDurations.Add(current_data);
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
            if(LevelLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                string current_data = current_box.Text;
                PotionTypeItems control_parent = current_box.FindParent<PotionTypeItems>();
                parent = control_parent.FindParent<StackPanel>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;

                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.MobEffectLevels.Count > 0 && (item_datacontext.MobEffectLevels.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.MobEffectLevels.ElementAt(index);
                    item_datacontext.MobEffectLevels.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.MobEffectLevels.Count - 1) >= 0)
                    item_datacontext.MobEffectLevels.Insert(index, current_data);
                else
                    item_datacontext.MobEffectLevels.Add(current_data);
                #endregion
            }
        }
    }
}
