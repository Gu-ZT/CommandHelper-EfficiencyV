using cbhk_environment.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace cbhk_environment.ControlsDataContexts
{
    public class TextComboBoxItemSource: ObservableObject
    {
        public ObservableCollection<TextSource> ItemDataSource { get; set; }

        public Popup pop = new Popup();

        TextComboBoxs current_box = new TextComboBoxs();

        public TextComboBoxItemSource()
        {
        }

        public void display_selected_itemText(TextComboBoxs cb)
        {
            #region 更新显示的文本
            if (cb.SelectedItem == null) return;
            TextBox tb = cb.Template.FindName("EditableTextBox", cb) as TextBox;
            TextSource item_textblock = cb.SelectedItem as TextSource;
            string new_text = item_textblock.ItemText;
            tb.Text = new_text;
            #endregion
        }

        /// <summary>
        /// 选择第一个成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            TextComboBoxs combobox = box.TemplatedParent as TextComboBoxs;

            //订阅选择成员更新事件
            combobox.SelectionChanged += ComboboxSelectionChanged;

            if (combobox.Items.Count > 0)
            {
                combobox.SelectedIndex = 0;
                TextSource textSource = combobox.Items[0] as TextSource;
                box.Text = textSource.ItemText;
            }
        }

        /// <summary>
        /// 实时更新所选文本显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboboxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextComboBoxs current_box = sender as TextComboBoxs;

            TextSource CurrentItem = current_box.SelectedItem as TextSource;

            TextBox text_box = current_box.Template.FindName("EditableTextBox", current_box) as TextBox;

            text_box.Text = CurrentItem.ItemText;
        }

        public void UpdateSelectionItem(object sender, KeyEventArgs e)
        {
            TextBox box = sender as TextBox;
            current_box = box.TemplatedParent as TextComboBoxs;

            #region 打开下拉框
            ObservableCollection<TextSource> dataGroup = current_box.ItemsSource as ObservableCollection<TextSource>;
            var target_data_groups = dataGroup.Where(item => item.item_text.Contains(box.Text.Trim()));
            if (box.Text.Trim() == "")
                pop.IsOpen = false;
            if (target_data_groups.Count() > 1)
            {
                pop = CreatePop(pop, target_data_groups, current_box, current_box.ItemTemplate);
                pop.IsOpen = true;
            }
            #endregion

            #region 搜索目标成员
            IEnumerable<TextSource> item_source = current_box.ItemsSource as IEnumerable<TextSource>;
            IEnumerable<TextSource> select_item = item_source.Where(item => item.ItemText.Contains(box.Text));
            if (select_item.Count() >= 1)
                current_box.SelectedItem = select_item.First();
            #endregion
        }

        public Popup CreatePop(Popup pop, IEnumerable<TextSource> listSource, UIElement element, DataTemplate display_template)
        {
            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0)
            };

            StackPanel panel1 = new StackPanel();

            panel1.Children.Clear();

            panel1.Background = new SolidColorBrush(Colors.Black);

            ListBox listbox = new ListBox
            {
                Background = null,
                MinWidth = 100,
                Height = 120,
                ItemsSource = listSource,
                ItemTemplate = display_template
            };
            listbox.MouseDoubleClick += Listbox_MouseDoubleClick;

            panel1.Children.Add(listbox);

            border.Child = panel1;

            pop.Child = border;

            pop.Placement = PlacementMode.Bottom;

            pop.PlacementTarget = element;

            return pop;
        }

        /// <summary>
        /// 更新已选中成员并更新显示文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Listbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            ListBox box = sender as ListBox;
            TextSource selected_item = box.SelectedItem as TextSource;
            current_box.SelectedItem = selected_item;
            TextBox text_box = current_box.Template.FindName("EditableTextBox", current_box) as TextBox;
            text_box.Text = selected_item.ItemText;
            pop.IsOpen = false;
        }

        /// <summary>
        /// 点击成员后更新选中的文本和图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PopupClosed(object sender, EventArgs e)
        {
            #region 更新显示的文本
            TextComboBoxs cb = (sender as Popup).TemplatedParent as TextComboBoxs;
            if (cb.SelectedItem == null) return;
            TextBox tb = cb.Template.FindName("EditableTextBox", cb) as TextBox;
            TextSource item_textblock = cb.SelectedItem as TextSource;
            string new_text = item_textblock.ItemText;
            tb.Text = new_text;
            #endregion
        }

    }
    public class TextSource:ObservableObject
    {
        public string item_text { get; set; }

        public string ItemText
        {
            get { return item_text; }
            set
            {
                item_text = value;
                OnPropertyChanged();
            }
        }
    }
}
