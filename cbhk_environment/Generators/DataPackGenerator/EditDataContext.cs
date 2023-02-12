using cbhk_environment.CustomControls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace cbhk_environment.Generators.DataPackGenerator
{
    public class EditDataContext: ObservableObject
    {
        //获取文本编辑区的引用
        public static TabControl FileModifyZone = null;

        //获取内容视图引用
        public static TreeView ContentView = null;

        #region 生成与返回
        public RelayCommand RunCommand { get; set; }

        public RelayCommand<FrameworkElement> ReturnCommand { get; set; }
        #endregion

        public EditDataContext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<FrameworkElement>(return_command);
            #endregion
        }

        /// <summary>
        /// 获取文本编辑区的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FileModifyZoneLoaded(object sender, RoutedEventArgs e)
        {
            FileModifyZone = sender as TabControl;
        }

        /// <summary>
        /// 获取内容树视图引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ContentViewLoaded(object sender, RoutedEventArgs e)
        {
            ContentView = sender as TreeView;
            if (datapack_datacontext.newTreeViewItems != null)
                foreach (var item in datapack_datacontext.newTreeViewItems)
                {
                    ContentView.Items.Add(item);
                }
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="win"></param>
        private void return_command(FrameworkElement obj)
        {
            Window win = Window.GetWindow(obj);
            DataPack.cbhk.Topmost = true;
            DataPack.cbhk.WindowState = WindowState.Normal;
            DataPack.cbhk.Show();
            DataPack.cbhk.Topmost = false;
            DataPack.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        private void run_command()
        {
            RichTabItems CurrentItem = FileModifyZone.SelectedItem as RichTabItems;
            RichTextBox CurrentTextBox = CurrentItem.Content as RichTextBox;
            TextRange CurrentContent = new TextRange(CurrentTextBox.Document.ContentStart, CurrentTextBox.Document.ContentEnd);
            File.WriteAllText(CurrentItem.Uid, CurrentContent.Text);
        }
    }
}
