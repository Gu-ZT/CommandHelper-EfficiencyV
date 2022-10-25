using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    /// <summary>
    /// TemplateItems.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateItems : UserControl
    {
        //选中后的背景色
        SolidColorBrush SelectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D3D3D"));
        //未选中的背景色
        SolidColorBrush UnSelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));
        //当前模板的id
        public string TemplateID = "";
        //文件类型
        public string FileType = "";
        //功能类型
        public string FunctionType = "";

        public TemplateItems()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选中该项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TemplateSelector.IsChecked = !TemplateSelector.IsChecked;

            if (TemplateSelector.IsChecked.Value)
                Background = SelectedColor;
            else
                Background = UnSelectColor;
        }

        private void TemplateItemMouseEnter(object sender, MouseEventArgs e)
        {
            Background = SelectedColor;
        }

        private void TemplateItemMouseLeave(object sender, MouseEventArgs e)
        {
            if(!TemplateSelector.IsChecked.Value)
            Background = UnSelectColor;
        }

        private void TemplateSelectorChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Background = SelectedColor;
        }

        private void TemplateSelectorUnchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            Background = UnSelectColor;
        }
    }
}
