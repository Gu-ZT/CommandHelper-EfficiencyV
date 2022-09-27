namespace cbhk_environment.SettingForm
{
    /// <summary>
    /// StartupItemForm.xaml 的交互逻辑
    /// </summary>
    public partial class StartupItemForm
    {
        public StartupItemForm()
        {
            InitializeComponent();
        }

        private void CommonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = true;
        }
    }
}
