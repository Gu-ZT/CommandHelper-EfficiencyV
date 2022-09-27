using System.Windows;

namespace cbhk_environment.SettingForm
{
    /// <summary>
    /// IndividualizationForm.xaml 的交互逻辑
    /// </summary>
    public partial class IndividualizationForm
    {
        public IndividualizationForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 引用设置好的轮播图字典
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetLinks(object sender, RoutedEventArgs e)
        {
            LinkForm lf = new LinkForm();
            if (lf.ShowDialog() == true){ }
        }

        private void SettingForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = true;
        }
    }
}
