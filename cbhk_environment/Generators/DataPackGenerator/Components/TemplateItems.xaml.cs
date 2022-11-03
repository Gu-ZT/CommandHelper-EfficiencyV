using cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms;
using System.Windows;
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
        //当前模板的id
        public string TemplateID = "";
        //文件类型
        public string FileType = "";
        //功能类型
        public string FunctionType = "";
        //所属命名空间
        public string FileNameSpace = "";

        //选中后的背景色
        SolidColorBrush SelectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D3D3D"));
        //未选中的背景色
        SolidColorBrush UnSelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));

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

        /// <summary>
        /// 鼠标进入后更新样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateItemMouseEnter(object sender, MouseEventArgs e)
        {
            Background = SelectedColor;

            foreach (TemplateTypeTag item in TemplateTypeTagPanel.Children)
                item.border.BorderThickness = new System.Windows.Thickness(1);
        }

        /// <summary>
        /// 鼠标离开后更新样式
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateItemMouseLeave(object sender, MouseEventArgs e)
        {
            if(!TemplateSelector.IsChecked.Value)
            Background = UnSelectColor;

            foreach (TemplateTypeTag item in TemplateTypeTagPanel.Children)
                item.border.BorderThickness = new System.Windows.Thickness(0);
        }

        /// <summary>
        /// 选中后同步左侧历史模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateSelectorChecked(object sender, RoutedEventArgs e)
        {
            Background = SelectedColor;

            if(!initialization_datacontext.TemplateCheckLock)
            {
                //表示加入已选择模板列表
                initialization_datacontext.SelectedTemplateItemList.Add(this);

                //表示开始更新
                initialization_datacontext.TemplateCheckLock = true;

                foreach (RecentTemplateItems recentTemplateItems in initialization_datacontext.RecentTemplateList)
                {
                    if (recentTemplateItems.TemplateName.Text == TemplateName.Text)
                        recentTemplateItems.TemplateSelector.IsChecked = true;
                }

                //更新完毕
                initialization_datacontext.TemplateCheckLock = false;
            }
        }

        /// <summary>
        /// 取消后取消左侧历史模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateSelectorUnchecked(object sender, RoutedEventArgs e)
        {
            Background = UnSelectColor;

            if(!initialization_datacontext.TemplateCheckLock)
            {
                //表示退出已选择模板列表
                initialization_datacontext.SelectedTemplateItemList.Remove(this);

                //表示开始更新
                initialization_datacontext.TemplateCheckLock = true;

                foreach (RecentTemplateItems recentTemplateItems in initialization_datacontext.RecentTemplateList)
                {
                    if (recentTemplateItems.TemplateName.Text == TemplateName.Text)
                        recentTemplateItems.TemplateSelector.IsChecked = false;
                }

                //更新完毕
                initialization_datacontext.TemplateCheckLock = false;
            }
        }
    }
}
