using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.DataPackGenerator
{
    public class EditDataContext: ObservableObject
    {
        //获取文本编辑区的引用
        public static TabControl FileModifyZone = null;

        //获取内容视图引用
        public static TreeView ContentView = null;

        public EditDataContext(/*RichTreeViewItems result*/)
        {
            //initItems = result;
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
    }
}
