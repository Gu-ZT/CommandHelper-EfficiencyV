using System.Collections.ObjectModel;
using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Collections.Generic;
using cbhk_environment.CustomControls;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using cbhk_environment.Generators.ItemGenerator;

namespace cbhk_environment.ControlsDataContexts
{
    //ObservableObject
    public class ComboBoxItemSource: ObservableObject
    {
        public ObservableCollection<ItemDataGroup> ItemDataSource { get; set; }

        public Popup pop = new Popup();

        IconComboBoxs current_box = new IconComboBoxs();

        public ComboBoxItemSource()
        {
        }

        public void UpdateSelectionItem(object sender,KeyEventArgs e)
        {
            TextBox box = sender as TextBox;
            current_box = box.TemplatedParent as IconComboBoxs;

            #region 打开下拉框
            ObservableCollection<ItemDataGroup> dataGroup = current_box.ItemsSource as ObservableCollection<ItemDataGroup>;
            var target_data_groups = dataGroup.Where(item => item.item_text.Contains(box.Text.Trim()));
            if (target_data_groups.Count() > 1 && box.Text.Trim() == "")
            {
                pop = CreatePop(pop, target_data_groups, current_box, current_box.ItemTemplate);
                pop.IsOpen = true;
            }
            #endregion

            #region 搜索目标成员
            IEnumerable<ItemDataGroup> item_source = current_box.ItemsSource as IEnumerable<ItemDataGroup>;
            IEnumerable<ItemDataGroup> select_item = item_source.Where(item => item.ItemText == box.Text);
            if (select_item.Count() == 1)
                current_box.SelectedItem = select_item.First();
            #endregion
        }

        public Popup CreatePop(Popup pop, IEnumerable<ItemDataGroup> listSource, UIElement element,DataTemplate display_template)
        {
            Border border = new Border
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0)
            };

            StackPanel panel1 = new StackPanel();

            panel1.Children.Clear();

            panel1.Background = new SolidColorBrush(Colors.Black);

            ScrollViewer viewer = new ScrollViewer()
            {
                Style = item_datacontext.scrollbarStyle,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            ListBox listbox = new ListBox
            {
                Background = null,
                Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
                MinWidth = 100,
                Height = 120,
                ItemsSource = listSource,
                ItemTemplate = display_template
            };

            ScrollViewer.SetVerticalScrollBarVisibility(listbox, ScrollBarVisibility.Disabled);
            ScrollViewer.SetHorizontalScrollBarVisibility(listbox, ScrollBarVisibility.Disabled);

            viewer.Content = listbox;

            listbox.MouseDoubleClick += Listbox_MouseDoubleClick;

            panel1.Children.Add(viewer);

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
            if (box.SelectedItem == null) return;
            ItemDataGroup selected_item = box.SelectedItem as ItemDataGroup;
            current_box.SelectedItem = selected_item;
            TextBox text_box = current_box.Template.FindName("EditableTextBox", current_box) as TextBox;
            Image image = current_box.Template.FindName("PART_DisplayIcon", current_box) as Image;
            image.Source = new BitmapImage(new Uri(selected_item.ItemImagePath, UriKind.Absolute));
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
            #region 更新显示的文本和图像
            IconComboBoxs cb = (sender as Popup).TemplatedParent as IconComboBoxs;
            if (cb.SelectedItem == null) return;
            TextBox tb = cb.Template.FindName("EditableTextBox", cb) as TextBox;
            ItemDataGroup item_textblock = cb.SelectedItem as ItemDataGroup;
            string new_text = item_textblock.ItemText;
            BitmapImage image_obj = (cb.SelectedItem as ItemDataGroup).ItemImage;
            Image image = cb.Template.FindName("PART_DisplayIcon", cb) as Image;
            image.Source = new BitmapImage(new Uri(item_textblock.ItemImagePath, UriKind.Absolute));
            tb.Text = new_text;
            #endregion
        }
    }

    public class ItemDataGroup:ObservableObject
    {
        public string item_text;
        public string ItemText
        {
            get { return item_text; }
            set
            {
                item_text = value;
                OnPropertyChanged();
            }
        }

        public string item_image_path;
        public string ItemImagePath
        {
            get { return item_image_path; }
            set
            {
                item_image_path = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage item_image;
        public BitmapImage ItemImage
        {
            get { return item_image; }
            set
            {
                item_image = value;
                OnPropertyChanged();
            }
        }

        public int item_index;
        public int ItemIndex
        {
            get { return item_index; }
            set
            {
                item_index = value;
                OnPropertyChanged();
            }
        }
    }
}
