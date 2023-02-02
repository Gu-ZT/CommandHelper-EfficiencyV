using cbhk_environment.ControlsDataContexts;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.ItemGenerator.Components
{
    /// <summary>
    /// PotionTypeItems.xaml 的交互逻辑
    /// </summary>
    public partial class PotionTypeItems : UserControl
    {
        private IconComboBoxItem potionType;
        public IconComboBoxItem PotionType
        {
            get
            {
                return potionType;
            }
            set
            {
                potionType = value;
            }
        }

        private string potionDuration;
        public string PotionDuration
        {
            get
            {
                return potionDuration;
            }
            set
            {
                potionDuration = value;
            }
        }

        private string potionLevel;
        public string PotionLevel
        {
            get
            {
                return potionLevel;
            }
            set
            {
                potionLevel = value;
            }
        }

        public string Result
        {
            get
            {
                string result;
                string id = MainWindow.MobEffectDataBase.Where(item => item.Value.Contains(PotionType.ComboBoxItemText)).Select(item=>item.Key).First();
                result = "{Id:"+id+"b,Duration:"+PotionDuration+",Amplifier:"+PotionLevel+"b,Ambient:0b,ShowParticles:0b},";
                return result;
            }
        }

        public PotionTypeItems()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 删除当前药水效果
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
        /// 载入药水类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MobEffectIdLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBoxs = sender as ComboBox;
            comboBoxs.ItemsSource = MainWindow.MobEffectIdSource;
        }
    }
}
