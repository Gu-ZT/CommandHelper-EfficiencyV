using cbhk_environment.ControlsDataContexts;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;

namespace cbhk_environment.Generators.VillagerGenerator.Components
{
    /// <summary>
    /// GossipsItems.xaml 的交互逻辑
    /// </summary>
    public partial class GossipsItems : UserControl
    {

        #region 返回该言论的数据
        public string GossipData
        {
            get
            {
                string result;
                string item_data = Type.SelectedItem.ToString();
                string TypeData = item_data.Trim() != ""?"Type:"+ item_data+",":"";
                string ValueData = Value.Text.Trim() != "" ? "Value:" + (Value.Text.Contains(".") ? Value.Text.Split('.')[0] :Value.Text) +",":"";
                string TargetData = Target.Text.Trim() != "" ?"Target:"+Target.Text+",":"";
                result = TypeData != "" || ValueData != "" || TargetData != "" ?TypeData + ValueData + TargetData:"";
                result = "{" + result.TrimEnd(',') + "},";
                return result;
            }
        }
        #endregion

        //言论类型
        ObservableCollection<string> TypeList = new ObservableCollection<string> { };

        //言论配置文件路径
        string TypeFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\Villager\\data\\GossipTypes.ini";

        public GossipsItems()
        {
            InitializeComponent();
        }

        private void TypeLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            CustomControls.TextComboBoxs textComboBoxs = sender as CustomControls.TextComboBoxs;
            //读取言论类型
            if (File.Exists(TypeFilePath))
            {
                string[] types = File.ReadAllLines(TypeFilePath);
                for (int i = 0; i < types.Length; i++)
                {
                    TypeList.Add(types[i]);
                }
                textComboBoxs.ItemsSource = TypeList;
                textComboBoxs.SelectedItem = TypeList.First();
                TextBox box = textComboBoxs.Template.FindName("EditableTextBox", textComboBoxs) as TextBox;
                box.Text = TypeList.First();
            }
        }

        /// <summary>
        /// 右击后删除
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            villager_datacontext.gossipItems.Remove(this);
        }
    }
}
