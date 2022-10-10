using cbhk_environment.CustomControls;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    /// <summary>
    /// FileItems.xaml 的交互逻辑
    /// </summary>
    public partial class FileItems : UserControl
    {
        public FileItems()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 保存文件原来的名字
        /// </summary>
        string originalName = "";

        /// <summary>
        /// 编辑改文件名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyFileNameClick(object sender, RoutedEventArgs e)
        {
            originalName = FileName.Text;
            FileName.IsReadOnly = false;
            FileName.Focus();
            FileName.SelectAll();
        }

        /// <summary>
        /// 文件名修改完毕,根据UID中的路径修改对应的实体文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModifyCompletedKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                string extensionName = Path.GetExtension(Uid);
                string pathName = Path.GetDirectoryName(Uid);
                if (File.Exists(pathName + "\\" + originalName + extensionName))
                    File.Move(pathName + "\\" + originalName + extensionName, pathName + "\\" + FileName.Text + extensionName);
                FileName.IsReadOnly = true;
            }
        }

        /// <summary>
        /// 删除该文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            RichTreeViewItems parent = Parent as RichTreeViewItems;
            if(parent.Parent is RichTreeViewItems)
            {
                RichTreeViewItems grand_parent = parent.Parent as RichTreeViewItems;
                grand_parent.Items.Remove(parent);
            }
            else
                if(parent.Parent is TreeView)
            {
                TreeView grand_parent = parent.Parent as TreeView;
                grand_parent.Items.Remove(parent);
            }
        }
    }
}
