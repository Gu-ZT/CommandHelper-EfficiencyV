using cbhk_environment.CustomControls;
using cbhk_environment.GeneralTools;
using cbhk_environment.Generators.DataPackGenerator.Components;
using cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace cbhk_environment.Generators.DataPackGenerator
{
    public class EditDataContext : ObservableObject
    {
        //获取文本编辑区的引用
        public static TabControl FileModifyZone = null;

        //获取内容视图引用
        public static TreeView ContentView = null;

        #region 数据包搜索文本框内容
        private string datapackSeacherValue = "";
        public string DatapackSeacherValue
        {
            get
            {
                return datapackSeacherValue;
            }
            set
            {
                datapackSeacherValue = value;
            }
        }
        #endregion

        #region 当前数据包版本
        public ObservableCollection<string> AddFileSearchVersionSource { get; set; } = new ObservableCollection<string> { };
        #endregion

        /// <summary>
        /// 文件模板成员列表
        /// </summary>
        public ObservableCollection<TemplateItems> TemplateItemList { get; set; } = new ObservableCollection<TemplateItems> { };

        /// <summary>
        /// 已被选中的模板成员
        /// </summary>
        private ObservableCollection<TemplateItems> SelectedTemplateItemList { get; set; } = new ObservableCollection<TemplateItems> { };

        #region 已选择的文件版本
        private string selectedFileVersion = "";
        public string SelectedFileVersion
        {
            get
            {
                return selectedFileVersion;
            }
            set
            {
                selectedFileVersion = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 生成与返回
        public RelayCommand RunCommand { get; set; }

        public RelayCommand<FrameworkElement> ReturnCommand { get; set; }
        #endregion

        #region 数据包管理器右键菜单
        public RelayCommand AddFile { get; set; }
        public RelayCommand AddFolder { get; set; }
        public RelayCommand Cut { get; set; }
        public RelayCommand Copy { get; set; }
        public RelayCommand Paste { get; set; }
        public RelayCommand CopyFullPath { get; set; }
        public RelayCommand OpenWithResourceManagement { get; set; }
        public RelayCommand ExcludeFromProject { get; set; }
        public RelayCommand OpenWithTerminal { get; set; }
        public RelayCommand Delete { get; set; }
        public RelayCommand Attribute { get; set; }
        #endregion

        #region 文件添加窗体.文件名称
        private string newFileName = "";
        public string NewFileName
        {
            get
            {
                return newFileName;
            }
            set
            {
                newFileName = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 确定/取消添加文件
        private bool addFileFormResult = false;
        public bool AddFileFormResult
        {
            get
            {
                return addFileFormResult;
            }
            set
            {
                addFileFormResult = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<Window> SureToAddFile { get; set; }
        public RelayCommand<Window> CancelToAddFile { get; set; }
        #endregion

        public EditDataContext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<FrameworkElement>(return_command);

            AddFile = new RelayCommand(AddFileCommand);
            AddFolder = new RelayCommand(AddFolderCommand);
            Cut = new RelayCommand(CutCommand);
            Copy = new RelayCommand(CopyCommand);
            Paste = new RelayCommand(PasteCommand);
            CopyFullPath = new RelayCommand(CopyFullPathCommand);
            OpenWithResourceManagement = new RelayCommand(OpenWithResourceManagementCommand);
            ExcludeFromProject = new RelayCommand(ExcludeFromProjectCommand);
            OpenWithTerminal = new RelayCommand(OpenWithTerminalCommand);
            Delete = new RelayCommand(DeleteCommand);
            Attribute = new RelayCommand(AttributeCommand);

            SureToAddFile = new RelayCommand<Window>(SureToAddFileCommand);
            CancelToAddFile = new RelayCommand<Window>(CancelToAddFileCommand);
            #endregion
        }

        #region 添加文件窗体逻辑
        /// <summary>
        /// 模板成员视图载入
        /// </summary>
        public void TemplateItemViewLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var item in TemplateSelectDataContext.TemplateList)
            {
                TemplateItemList.Add(item);
            }
        }

        /// <summary>
        /// 添加文件窗体版本载入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddFileFormVersionLoaded(object sender,RoutedEventArgs e)
        {
            foreach (var item in DatapackGenerateSetupDataContext.DatapackVersionDatabase)
            {
                AddFileSearchVersionSource.Add(item.Key);
            }
        }

        /// <summary>
        /// 确定添加文件
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void SureToAddFileCommand(Window win)
        {
            win.DialogResult = true;
        }

        /// <summary>
        /// 取消添加文件
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void CancelToAddFileCommand(Window win)
        {
            win.DialogResult = false;
        }

        /// <summary>
        /// 关闭添加文件窗体
        /// </summary>
        /// <param name="win"></param>
        public void ClosingAddFileForm(Window win)
        {
            win.DialogResult = false;
        }
        #endregion

        /// <summary>
        /// 添加文件
        /// </summary>
        private void AddFileCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            ContentItems contentItems = richTreeViewItems.Header as ContentItems;
            if(Directory.Exists(contentItems.Uid))
            {
                AddFileForm addFileForm = new AddFileForm();
                if(addFileForm.ShowDialog() == true)
                {
                    //根据版本、类型和名称添加文件
                    foreach (var item in SelectedTemplateItemList)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        private void AddFolderCommand()
        {

        }

        /// <summary>
        /// 剪切
        /// </summary>
        private void CutCommand()
        {

        }

        /// <summary>
        /// 复制对象
        /// </summary>
        private void CopyCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            ContentItems templateItems = richTreeViewItems.Header as ContentItems;
            Clipboard.SetText(templateItems.Uid);
        }

        /// <summary>
        /// 粘贴对象
        /// </summary>
        private void PasteCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            ContentItems contentItems = richTreeViewItems.Header as ContentItems;
            string templateItemsPath = Clipboard.GetText();
            if (File.Exists(templateItemsPath) && contentItems.FileType == ContentReader.ContentType.File)
            {
                File.Copy(templateItemsPath, contentItems.Uid + "\\" + Path.GetFileName(templateItemsPath));
            }
            else
            if (Directory.Exists(templateItemsPath))
            {
                Directory.Move(templateItemsPath, contentItems.Uid);
            }
        }

        /// <summary>
        /// 复制完整路径
        /// </summary>
        private void CopyFullPathCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            TemplateItems templateItems = richTreeViewItems.Header as TemplateItems;
            Clipboard.SetText(templateItems.Uid);
        }

        /// <summary>
        /// 从资源管理器打开
        /// </summary>
        private void OpenWithResourceManagementCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            TemplateItems templateItems = richTreeViewItems.Header as TemplateItems;
            OpenFolderThenSelectFiles.ExplorerFile(templateItems.Uid);
        }

        /// <summary>
        /// 从项目中排除
        /// </summary>
        private void ExcludeFromProjectCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            if(richTreeViewItems.Parent != null)
            {
                RichTreeViewItems parent = richTreeViewItems.Parent as RichTreeViewItems;
                parent.Items.Remove(richTreeViewItems);
            }
            else
            {
                ContentView.Items.Remove(ContentView.SelectedItem);
            }
        }

        /// <summary>
        /// 在终端打开
        /// </summary>
        private void OpenWithTerminalCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            TemplateItems templateItems = richTreeViewItems.Header as TemplateItems;
            if(Directory.Exists(templateItems.Uid))
            Process.Start(@"explorer.exe", "cd " + templateItems.Uid);
        }

        /// <summary>
        /// 删除对象
        /// </summary>
        private void DeleteCommand()
        {
            RichTreeViewItems richTreeViewItems = ContentView.SelectedItem as RichTreeViewItems;
            TemplateItems templateItems = richTreeViewItems.Header as TemplateItems;
            if (Directory.Exists(templateItems.Uid))
                Directory.Delete(templateItems.Uid, true);
            else
                if(File.Exists(templateItems.Uid))
                File.Delete(templateItems.Uid);
            ExcludeFromProjectCommand();
        }

        /// <summary>
        /// 查看属性
        /// </summary>
        private void AttributeCommand()
        {

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
