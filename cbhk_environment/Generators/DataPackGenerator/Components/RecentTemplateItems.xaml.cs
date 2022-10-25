using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    /// <summary>
    /// RecentTemplateItems.xaml 的交互逻辑
    /// </summary>
    public partial class RecentTemplateItems : UserControl
    {
        //模板路径
        public string FilePath = "";
        //模板版本
        public string Version = "";
        //文件类型
        public string FileType = "";
        //功能类型
        public string FunctionType = "";

        /// <summary>
        /// 实例化一个历史模板
        /// </summary>
        /// <param name="filePath">模板路径</param>
        /// <param name="typeName">类型名</param>
        /// <param name="fileImage">模板图标</param>
        /// <param name="templateType">模板类型</param>
        public RecentTemplateItems(string filePath,string typeName,string fileImage,string templateType)
        {
            InitializeComponent();

            FilePath = filePath;
            TemplateName.Text = typeName;
            TemplateImage.Source = new BitmapImage(new Uri(fileImage, UriKind.Absolute));
            TemplateType.Children.Add(new TemplateTypeTag(templateType));
        }
    }
}
