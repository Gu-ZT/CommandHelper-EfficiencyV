﻿using cbhk_environment.ControlsDataContexts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// CanDestroyItems.xaml 的交互逻辑
    /// </summary>
    public partial class CanDestroyItems : UserControl
    {
        private IconComboBoxItem block;
        public IconComboBoxItem Block
        {
            get
            {
                return block;
            }
            set
            {
                block = value;
            }
        }

        public string Result
        {
            get
            {
                string result = "\"" + MainWindow.ItemDataBase.Where(item => item.Key.Split(':')[1] == Block.ComboBoxItemText).Select(item=>item.Key).First().Split(':')[0]+"\",";
                return result;
            }
        }
        public CanDestroyItems()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 删除该控件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IconTextButtons_Click(object sender, RoutedEventArgs e)
        {
            StackPanel parent = Parent as StackPanel;
            //删除自己
            parent.Children.Remove(this);
        }

        /// <summary>
        /// 加载所有子级成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CanDestroyItemLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBoxs = sender as ComboBox;
            comboBoxs.ItemsSource = MainWindow.ItemIdSource;
        }
    }
}
