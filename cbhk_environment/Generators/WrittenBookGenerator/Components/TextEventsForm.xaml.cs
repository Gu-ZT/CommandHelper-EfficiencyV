using cbhk_environment.CustomControls;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.WrittenBookGenerator.Components
{
    /// <summary>
    /// TextEventsForm.xaml 的交互逻辑
    /// </summary>
    public partial class TextEventsForm : UserControl
    {
        #region 允许编辑点击事件
        public Visibility EnableEditClickEvent
        {
            get
            {
                return EnableClickEvent.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region 允许编辑悬浮事件
        public Visibility EnableEditHoverEvent
        {
            get
            {
                return EnableHoverEvent.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region 允许插入文本
        public Visibility EnableEditInsertion
        {
            get
            {
                return EnableInsertion.IsChecked.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        public TextEventsForm()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 载入点击事件的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickEventsLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs textComboBoxs = sender as TextComboBoxs;
            textComboBoxs.ItemsSource = written_book_datacontext.clickEventSource;
        }

        /// <summary>
        /// 载入悬浮事件的成员
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HoverEventsLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs textComboBoxs = sender as TextComboBoxs;
            textComboBoxs.ItemsSource = written_book_datacontext.hoverEventSource;
        }
    }
}
