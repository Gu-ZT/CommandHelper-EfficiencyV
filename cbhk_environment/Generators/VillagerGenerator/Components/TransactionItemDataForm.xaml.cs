using cbhk_environment.CustomControls;
using System.Windows.Controls;

namespace cbhk_environment.Generators.VillagerGenerator.Components
{
    /// <summary>
    /// TransactionItemDataForm.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionItemDataForm
    {

        public TransactionItemDataForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 点击关闭后隐藏窗体而不是销毁实例
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        ///// <summary>
        ///// 布尔值更新
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void RewardExpSelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    TextComboBoxs textComboBoxs = sender as TextComboBoxs;
        //    TextBox box = textComboBoxs.Template.FindName("EditableTextBox", textComboBoxs) as TextBox;
        //    box.Text = (textComboBoxs.SelectedItem as ControlsDataContexts.TextSource).ItemText;
        //}

        /// <summary>
        /// 值绑定后初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueTextChanged(object sender, TextChangedEventArgs e)
        {
            ColorNumbericUpDowns colorNumbericUpDowns = sender as ColorNumbericUpDowns;
            TextBox box = colorNumbericUpDowns.Template.FindName("textbox", colorNumbericUpDowns) as TextBox;
            if(box != null)
            box.Text = colorNumbericUpDowns.Text;
        }
    }
}
