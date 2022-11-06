using cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    /// <summary>
    /// RecentTemplateItems.xaml 的交互逻辑
    /// </summary>
    public partial class RecentTemplateItems : UserControl
    {
        //当前模板的id
        public string TemplateID = "";
        //文件类型
        public string FileType = "";
        //功能类型
        public string FunctionType = "";
        //所属命名空间
        public string FileNameSpace = "";
        //文件路径
        public string FilePath = "";

        //选中后的背景色
        SolidColorBrush SelectedColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D3D3D"));
        //未选中的背景色
        SolidColorBrush UnSelectColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Transparent"));

        /// <summary>
        /// 实例化一个历史模板
        /// </summary>
        /// <param name="filePath">模板路径</param>
        /// <param name="typeName">类型名</param>
        /// <param name="fileImage">模板图标</param>
        /// <param name="templateType">模板类型</param>
        public RecentTemplateItems(string filePath,string typeName,string fileImage,string templateType,string nameSpace)
        {
            InitializeComponent();

            FilePath = filePath;
            FileNameSpace = nameSpace;
            TemplateName.Text = typeName;
            TemplateImage.Source = new BitmapImage(new Uri(fileImage, UriKind.Absolute));
            TemplateType.Children.Add(new TemplateTypeTag(templateType));
        }

        /// <summary>
        /// 选中后同步右侧模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateSelectorChecked(object sender, RoutedEventArgs e)
        {
            Background = SelectedColor;

            if (!initialization_datacontext.TemplateCheckLock)
            {
                //表示加入已选择模板列表
                initialization_datacontext.SelectedTemplateItemList.Add(this);

                //表示开始更新
                initialization_datacontext.TemplateCheckLock = true;

                foreach (TemplateItems templateItems in initialization_datacontext.TemplateList)
                {
                    if (templateItems.TemplateName.Text == TemplateName.Text)
                        templateItems.TemplateSelector.IsChecked = true;
                }

                //更新完毕
                initialization_datacontext.TemplateCheckLock = false;
            }
        }

        /// <summary>
        /// 取消后取消右侧模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateSelectorUnchecked(object sender, RoutedEventArgs e)
        {
            Background = UnSelectColor;

            if (!initialization_datacontext.TemplateCheckLock)
            {
                //表示退出已选择模板列表
                initialization_datacontext.SelectedTemplateItemList.Remove(this);

                //表示开始更新
                initialization_datacontext.TemplateCheckLock = true;

                foreach (TemplateItems templateItems in initialization_datacontext.TemplateList)
                {
                    if (templateItems.TemplateName.Text == TemplateName.Text)
                        templateItems.TemplateSelector.IsChecked = false;
                }

                //更新完毕
                initialization_datacontext.TemplateCheckLock = false;
            }
        }

        /// <summary>
        /// 切换选中状态
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControlMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TemplateSelector.IsChecked = !TemplateSelector.IsChecked;
        }
    }
}
