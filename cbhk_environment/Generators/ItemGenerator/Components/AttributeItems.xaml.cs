using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools.ScrollViewerHelper;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// AttributeItems.xaml 的交互逻辑
    /// </summary>
    public partial class AttributeItems : UserControl
    {
        public AttributeItems()
        {
            InitializeComponent();
        }

        StackPanel parent = null;

        //属性id加载完毕
        bool IdLoaded = false;
        //生效槽位加载完毕
        bool SlotLoaded = false;
        //属性值类型加载完毕
        bool ValueTypeLoaded = false;
        //属性值加载完毕
        bool ValueLoaded = false;
        //属性名称加载完毕
        bool NameLoaded = false;

        /// <summary>
        /// 载入属性ID列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeIdsLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs textComboBoxs = sender as TextComboBoxs;
            textComboBoxs.ItemsSource = MainWindow.AttributeSource.ItemDataSource;

            IdLoaded = true;
            textComboBoxs.SelectedIndex = 0;
            AttributeIdSelectionChanged(textComboBoxs, null);
            TextBox box = textComboBoxs.Template.FindName("EditableTextBox", textComboBoxs) as TextBox;
            box.Text = (textComboBoxs.Items[0] as TextSource).ItemText;

        }

        /// <summary>
        /// 属性名称载入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeNameLoaded(object sender, RoutedEventArgs e)
        {
            TextBox current_box = sender as TextBox;
            current_box.Text = "null";
            NameLoaded = true;
            AttributeNameSelectionChanged(current_box,null);
        }

        /// <summary>
        /// 载入属性生效槽位ID列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeSlotsLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs textComboBoxs = sender as TextComboBoxs;
            textComboBoxs.ItemsSource = MainWindow.AttributeSlotSource.ItemDataSource;

            SlotLoaded = true;
            textComboBoxs.SelectedIndex = 0;
            AttributeSlotSelectionChanged(textComboBoxs, null);
            TextBox box = textComboBoxs.Template.FindName("EditableTextBox", textComboBoxs) as TextBox;
            box.Text = (textComboBoxs.Items[0] as TextSource).ItemText;
        }

        private void AttributeValueLoaded(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns colorNumbericUpDowns = sender as ColorNumbericUpDowns;
            colorNumbericUpDowns.Text = "0";
            ValueLoaded = true;
            AttributeValueSelectionChanged(colorNumbericUpDowns,null);
        }

        /// <summary>
        /// 载入属性值类型ID列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeValueTypesLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs textComboBoxs = sender as TextComboBoxs;
            textComboBoxs.ItemsSource = MainWindow.AttributeValueTypeSource.ItemDataSource;

            SlotLoaded = true;
            textComboBoxs.SelectedIndex = 0;
            AttributeValueTypeSelectionChanged(textComboBoxs, null);
            TextBox box = textComboBoxs.Template.FindName("EditableTextBox", textComboBoxs) as TextBox;
            box.Text = (textComboBoxs.Items[0] as TextSource).ItemText;
        }

        /// <summary>
        /// 属性ID列表成员更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeIdSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(IdLoaded)
            {
                TextComboBoxs current_box = sender as TextComboBoxs;
                TextSource current_data = current_box.SelectedItem as TextSource;

                AttributeItems control_parent = current_box.FindParent<AttributeItems>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.AttributeIDs.Count > 0 && (item_datacontext.AttributeIDs.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.AttributeIDs.ElementAt(index);
                    item_datacontext.AttributeIDs.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                string current_key = MainWindow.attribute_database.Where(item => item.Value == current_data.ItemText).Select(item => item.Key).First();

                if ((item_datacontext.AttributeIDs.Count - 1) >= 0)
                    item_datacontext.AttributeIDs.Insert(index, current_key);
                else
                    item_datacontext.AttributeIDs.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 属性生效槽位列表成员更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeSlotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SlotLoaded)
            {
                TextComboBoxs current_box = sender as TextComboBoxs;
                TextSource current_data = current_box.SelectedItem as TextSource;

                AttributeItems control_parent = current_box.FindParent<AttributeItems>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.AttributeSlots.Count > 0 && (item_datacontext.AttributeSlots.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.AttributeSlots.ElementAt(index);
                    item_datacontext.AttributeSlots.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员

                string current_key = MainWindow.attribute_database.Where(item => item.Value == current_data.ItemText).Select(item => item.Key).First();

                if ((item_datacontext.AttributeSlots.Count - 1) >= 0)
                    item_datacontext.AttributeSlots.Insert(index, current_key);
                else
                    item_datacontext.AttributeSlots.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 属性值列表成员更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeValueSelectionChanged(object sender, RoutedEventArgs e)
        {
            if(ValueLoaded)
            {
                ColorNumbericUpDowns current_box = sender as ColorNumbericUpDowns;
                string current_data = current_box.Text;
                AttributeItems control_parent = current_box.FindParent<AttributeItems>();
                StackPanel parent = control_parent.FindParent<StackPanel>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;

                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.AttributeValues.Count > 0 && (item_datacontext.AttributeValues.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.AttributeValues.ElementAt(index);
                    item_datacontext.AttributeValues.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                if ((item_datacontext.AttributeValues.Count - 1) >= 0)
                    item_datacontext.AttributeValues.Insert(index, current_data);
                else
                    item_datacontext.AttributeValues.Add(current_data);
                #endregion
            }
        }

        /// <summary>
        /// 属性值类型列表成员更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeValueTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ValueTypeLoaded)
            {
                TextComboBoxs current_box = sender as TextComboBoxs;
                TextSource current_data = current_box.SelectedItem as TextSource;

                AttributeItems control_parent = current_box.FindParent<AttributeItems>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.AttributeValueTypes.Count > 0 && (item_datacontext.AttributeValueTypes.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.AttributeValueTypes.ElementAt(index);
                    item_datacontext.AttributeValueTypes.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员

                string current_key = MainWindow.attribute_database.Where(item => item.Value == current_data.ItemText).Select(item => item.Key).First();

                if ((item_datacontext.AttributeValueTypes.Count - 1) >= 0)
                    item_datacontext.AttributeValueTypes.Insert(index, current_key);
                else
                    item_datacontext.AttributeValueTypes.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 属性名称列表成员更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttributeNameSelectionChanged(object sender, RoutedEventArgs e)
        {
            if(NameLoaded)
            {
                TextBox current_box = sender as TextBox;
                string current_data = current_box.Text;

                AttributeItems control_parent = current_box.FindParent<AttributeItems>();
                if (parent == null)
                    parent = control_parent.Parent as StackPanel;
                int index = parent.Children.IndexOf(control_parent);

                #region 删除上一条数据
                if (item_datacontext.AttributeNames.Count > 0 && (item_datacontext.AttributeNames.Count - 1) >= index)
                {
                    string remove_key = item_datacontext.AttributeNames.ElementAt(index);
                    item_datacontext.AttributeNames.Remove(remove_key);
                }
                #endregion

                #region 添加当前选中的成员
                string current_key = "";
                if (item_datacontext.AttributeNames.Count > 0)
                {
                    current_key = item_datacontext.AttributeNames.Where(item => item == current_data).First();
                    if ((item_datacontext.AttributeNames.Count - 1) >= 0)
                        item_datacontext.AttributeNames.Insert(index, current_key);
                }
                else
                    item_datacontext.AttributeNames.Add(current_key);
                #endregion
            }
        }

        /// <summary>
        /// 删除当前属性成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconTextButtons_Click(object sender, RoutedEventArgs e)
        {
            IconTextButtons button = sender as IconTextButtons;
            AttributeItems attributeItems = button.FindParent<AttributeItems>();
            StackPanel parent = attributeItems.FindParent<StackPanel>();
            #region 删除对应数据
            int index = parent.Children.IndexOf(attributeItems);
            if((item_datacontext.AttributeIDs.Count-1) >= index)
            item_datacontext.AttributeIDs.RemoveAt(index);

            if ((item_datacontext.AttributeNames.Count - 1) >= index)
                item_datacontext.AttributeNames.RemoveAt(index);

            if ((item_datacontext.AttributeSlots.Count - 1) >= index)
                item_datacontext.AttributeSlots.RemoveAt(index);

            if ((item_datacontext.AttributeValues.Count - 1) >= index)
                item_datacontext.AttributeValues.RemoveAt(index);

            if ((item_datacontext.AttributeValueTypes.Count - 1) >= index)
                item_datacontext.AttributeValueTypes.RemoveAt(index);
            #endregion

            //删除自己
            parent.Children.Remove(attributeItems);
        }
    }
}
