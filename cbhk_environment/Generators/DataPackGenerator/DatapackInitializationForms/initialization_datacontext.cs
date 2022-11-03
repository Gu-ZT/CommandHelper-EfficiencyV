using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.Generators.DataPackGenerator.Components;
using cbhk_environment.Generators.WrittenBookGenerator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MSScriptControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Windows.Devices.PointOfService;
using WK.Libraries.BetterFolderBrowserNS;

namespace cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms
{
    public class initialization_datacontext: ObservableObject
    {
        /// <summary>
        /// 初始化模板选择和属性设置窗体数据上下文
        /// </summary>
        public initialization_datacontext()
        {
            #region 链接命令
            TemplateLastStep = new RelayCommand<Window>(TemplateLastStepCommand);
            TemplateNextStep = new RelayCommand<Window>(TemplateNextStepCommand);
            ClearAllSelectParameters = new RelayCommand(ClearAllSelectParametersCommand);

            AttributeLastStep = new RelayCommand<Window>(AttributeLastStepCommand);
            AttributeNextStep = new RelayCommand<Window>(AttributeNextStepCommand);
            SetDatapackPath = new RelayCommand(SetDatapackPathCommand);
            SetDatapackDescription = new RelayCommand(SetDatapackDescriptionCommand);
            AddDatapackFilter = new RelayCommand(AddDatapackFilterCommand);
            ClearDatapackFilter = new RelayCommand(ClearDatapackFilterCommand);

            CopyDatapackName = new RelayCommand(CopyDatapackNameCommand);
            CopyDatapackPath = new RelayCommand(CopyDatapackPathCommand);
            CopyDatapackDescription = new RelayCommand(CopyDatapackDescriptionCommand);
            CopyDatapackMainNameSpace = new RelayCommand(CopyDatapackMainNameSpaceCommand);

            SynchronizePackAndNameSpaceName = new RelayCommand<TextCheckBoxs>(SynchronizePackAndNameSpaceNameCommand);
            #endregion

            #region 载入版本
            if (Directory.Exists(TemplateDataFilePath))
            {
                string[] versionList = Directory.GetDirectories(TemplateDataFilePath);

                #region 版本、文件类型、功能类型的默认选中项
                if (VersionList.Count == 0)
                    VersionList.Add(new TextSource() { ItemText = "所有版本" });

                if (FileTypeList.Count == 0)
                    FileTypeList.Add(new TextSource() { ItemText = "所有文件类型" });

                if (FunctionTypeList.Count == 0)
                    FunctionTypeList.Add(new TextSource() { ItemText = "所有功能类型" });
                #endregion

                foreach (string version in versionList)
                {
                    string versionString = Path.GetFileName(version);
                    VersionList.Add(new TextSource() { ItemText = versionString });
                }

                DefaultVersion = VersionList.Last();
            }
            #endregion
        }

        #region js脚本执行者
        string js_file = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js";
        static string language = "javascript";
        public static ScriptControlClass json_parser = new ScriptControlClass()
        {
            Language = language
        };
        public static object JsonScript(string JScript)
        {
            object Result = null;
            try
            {
                Result = json_parser.Eval(JScript);
            }
            catch (Exception ex)
            {
                return ex.Source + "\n" + ex.Message;
            }
            return Result;
        }
        #endregion

        #region 模板选择窗体逻辑处理

        //模板图标文件存放路径
        string TemplateIconFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\images";

        //模板元数据存放路径
        string TemplateMetaDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\introductions";

        //模板存放路径
        public string TemplateDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\presets";

        //近期使用的模板存放路径
        public string RecentTemplateDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\recent_templates";

        //存储已选中的模板
        public static List<object> SelectedTemplateItemList = new List<object>() { };

        //近期使用的模板成员
        public static ObservableCollection<RecentTemplateItems> RecentTemplateList { get; set; } = new ObservableCollection<RecentTemplateItems>();

        //模板成员集合
        public static ObservableCollection<TemplateItems> TemplateList { get; set; } = new ObservableCollection<TemplateItems>();

        //模板状态锁，防止更新死循环
        public static bool TemplateCheckLock = false;

        //存放版本列表
        public ObservableCollection<TextSource> VersionList { get; set; } = new ObservableCollection<TextSource> { };

        //存放文件类型列表
        public ObservableCollection<TextSource> FileTypeList { get; set; } = new ObservableCollection<TextSource> { };

        //存放功能类型列表
        public ObservableCollection<TextSource> FunctionTypeList { get; set; } = new ObservableCollection<TextSource> { };

        //存储所有类型的模板标签
        public List<string> SelectedTemplateTypeTagList = new List<string> { };

        /// <summary>
        /// 已选择的版本
        /// </summary>
        public string SelectedVersionString = "";
        /// <summary>
        /// 已选择的文件类型
        /// </summary>
        public string SelectedFileTypeString = "";

        //获取搜索文本框引用
        TextBox SearchBox = null;

        #region 存储已选择的版本
        private static TextSource selectedVersion = null;
        public static TextSource SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                selectedVersion = value;
            }
        }
        #endregion

        #region 存储已选择的文件类型
        private static TextSource selectedFileType = null;
        public static TextSource SelectedFileType
        {
            get { return selectedFileType; }
            set
            {
                selectedFileType = value;
            }
        }
        #endregion

        #region 存储默认版本
        private static TextSource defaultVersion = null;
        public static TextSource DefaultVersion
        {
            get { return defaultVersion; }
            set
            {
                defaultVersion = value;
            }
        }
        #endregion

        #region 存储默认文件类型
        private static TextSource defaultFileType = null;
        public static TextSource DefaultFileType
        {
            get { return defaultFileType; }
            set
            {
                defaultFileType = value;
            }
        }
        #endregion

        #region 模板窗体:上一步、下一步命令和清除所有筛选
        public RelayCommand<Window> TemplateLastStep { get; set; }
        public RelayCommand<Window> TemplateNextStep { get; set; }
        public RelayCommand ClearAllSelectParameters { get; set; }
        #endregion

        #region 存储版本、文件类型和功能类型选择框的引用
        TextComboBoxs VersionSelector = null;
        TextComboBoxs FileTypeSelector = null;
        TextComboBoxs FunctionTypeSelector = null;
        #endregion

        #region 清空筛选参数按钮可见性
        private Visibility clearAllParametersVisibility = Visibility.Hidden;
        public Visibility ClearAllParametersVisibility
        {
            get { return clearAllParametersVisibility; }
            set
            {
                clearAllParametersVisibility = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 清除所有筛选参数
        /// </summary>
        private void ClearAllSelectParametersCommand()
        {
            ClearAllParametersVisibility = Visibility.Hidden;
            VersionSelector.SelectedIndex = FileTypeSelector.SelectedIndex = FunctionTypeSelector.SelectedIndex = 0;

            if (SearchBox != null)
                SearchBox.Text = "";
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void TemplateLastStepCommand(Window target)
        {
            target.DialogResult = false;
        }

        /// <summary>
        /// 进入下一步
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void TemplateNextStepCommand(Window target)
        {
            //获取已选择的版本和文件类型
            SelectedVersionString = SelectedVersion.ItemText;
            SelectedFileTypeString = SelectedFileType.ItemText.ToLower();

            if (SelectedVersionString == "所有版本")
                SelectedVersionString = DefaultVersion.ItemText;
            if (SelectedFileTypeString == "所有文件类型")
                SelectedFileTypeString = DefaultFileType.ItemText.ToLower();

            foreach (var selectedTemplateItemList in SelectedTemplateItemList)
            {
                if (selectedTemplateItemList is RecentTemplateItems)
                {
                    RecentTemplateItems recentTemplateItem = selectedTemplateItemList as RecentTemplateItems;
                    string FileType = recentTemplateItem.FileType;
                    string FunctionType = recentTemplateItem.FunctionType;
                    if (!SelectedTemplateTypeTagList.Contains(FileType))
                        SelectedTemplateTypeTagList.Add(FileType);
                    if (!SelectedTemplateTypeTagList.Contains(FunctionType))
                        SelectedTemplateTypeTagList.Add(FunctionType);
                }

                if (selectedTemplateItemList is TemplateItems)
                {
                    TemplateItems templateItem = selectedTemplateItemList as TemplateItems;
                    string FileType = templateItem.FileType;
                    string FunctionType = templateItem.FunctionType;
                    if (!SelectedTemplateTypeTagList.Contains(FileType))
                        SelectedTemplateTypeTagList.Add(FileType);
                    if (!SelectedTemplateTypeTagList.Contains(FunctionType))
                        SelectedTemplateTypeTagList.Add(FunctionType);
                }
            }

            //打开数据包设置窗体
            DatapackGenerateSetup datapackGenerateSetup = new DatapackGenerateSetup();
            if (datapackGenerateSetup.ShowDialog() == true)
            {
                //TemplateGenerator.Generator(SelectedTemplateItemList,RecentTemplateDataFilePath,TemplateDataFilePath,SelectedVersionString,SelectedFileTypeString, SelectedDatapackPathString);
                //处理完毕后清空已选择模板列表成员
                SelectedTemplateItemList.Clear();
                //当前窗体已完成任务
                target.DialogResult = true;
            }
        }

        /// <summary>
        /// 搜索符合某个特征的模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if(SearchBox == null)
            SearchBox = sender as TextBox;

            string CurrentSearchText = SearchBox.Text;

            if(CurrentSearchText.Trim() == "" && VersionSelector.SelectedIndex == 0 && FileTypeSelector.SelectedIndex == 0 && FunctionTypeSelector.SelectedIndex == 0)
            {
                TemplateList.All(item => { item.Visibility = Visibility.Visible;return true; });
                return;
            }

            foreach (TemplateItems Template in TemplateList)
            {
                //默认为关闭
                Template.Visibility = Visibility.Collapsed;
                string TemplateName = Template.TemplateName.Text;
                string TemplateDescription = Template.TemplateDescription.Text;

                #region 对比一系列数据
                if (CurrentSearchText == TemplateName || CurrentSearchText.Contains(TemplateName) || TemplateName.Contains(CurrentSearchText))
                {
                    Template.Visibility = Visibility.Visible;
                    continue;
                }
                if(CurrentSearchText == TemplateDescription || CurrentSearchText.Contains(TemplateDescription) || TemplateDescription.Contains(CurrentSearchText))
                {
                    Template.Visibility = Visibility.Visible;
                    continue;
                }

                DockPanel TypeTagPanel = Template.TemplateTypeTagPanel;
                CurrentSearchText = CurrentSearchText.ToUpper();
                foreach (TemplateTypeTag tag in TypeTagPanel.Children)
                {
                    if(tag.Text.Text == CurrentSearchText || tag.Text.Text.Contains(CurrentSearchText) || CurrentSearchText.Contains(tag.Text.Text))
                    {
                        Template.Visibility = Visibility.Visible;
                        break;
                    }
                }
                #endregion
            }

            //搜索完毕后更新版本，文件类型和功能类型筛选
            if(VersionSelector.SelectedIndex != 0 || FileTypeSelector.SelectedIndex != 0 || FunctionTypeSelector.SelectedIndex != 0)
            UpdateTemplateListVisibility(true);
        }

        /// <summary>
        /// 全选模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectAllTemplatesClick(object sender, RoutedEventArgs e)
        {
            TextCheckBoxs box = sender as TextCheckBoxs;
            TemplateList.All(item=> { if(item.Visibility == Visibility.Visible) item.TemplateSelector.IsChecked = box.IsChecked; return true; });
        }

        /// <summary>
        /// 反选模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReverseSelectAllTemplatesClick(object sender, RoutedEventArgs e)
        {
            TextCheckBoxs box = sender as TextCheckBoxs;
            TemplateList.All(item => { if (item.Visibility == Visibility.Visible) item.TemplateSelector.IsChecked = !item.TemplateSelector.IsChecked; return true; });
        }

        /// <summary>
        /// 全选历史模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SelectAllRecentTemplatesClick(object sender, RoutedEventArgs e)
        {
            TextCheckBoxs box = sender as TextCheckBoxs;
            RecentTemplateList.All(item => { if (item.Visibility == Visibility.Visible) item.TemplateSelector.IsChecked = box.IsChecked; return true; });
        }

        /// <summary>
        /// 反选历史模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ReverseSelectAllRecentTemplatesClick(object sender, RoutedEventArgs e)
        {
            TextCheckBoxs box = sender as TextCheckBoxs;
            RecentTemplateList.All(item => { if (item.Visibility == Visibility.Visible) item.TemplateSelector.IsChecked = !item.TemplateSelector.IsChecked; return true; });
        }

        /// <summary>
        /// 获取版本选择框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void VersionSelectorLoaded(object sender, RoutedEventArgs e)
        {
            VersionSelector = sender as TextComboBoxs;
        }

        /// <summary>
        /// 获取文件类型选择框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FileTypeSelectorLoaded(object sender, RoutedEventArgs e)
        {
            FileTypeSelector = sender as TextComboBoxs;
        }

        /// <summary>
        /// 获取功能类型选择框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FunctionSelectorLoaded(object sender, RoutedEventArgs e)
        {
            FunctionTypeSelector = sender as TextComboBoxs;
        }

        /// <summary>
        /// 模板的版本切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateVersionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (VersionSelector.SelectedIndex != 0)
                ClearAllParametersVisibility = Visibility.Visible;

            UpdateTemplateListVisibility();
            IsResetSelectParameter();
        }

        /// <summary>
        /// 模板的文件类型切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateFileTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FileTypeSelector.SelectedIndex != 0)
                ClearAllParametersVisibility = Visibility.Visible;

            UpdateTemplateListVisibility();
            IsResetSelectParameter();
        }

        /// <summary>
        /// 模板的功能类型切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateFunctionTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FunctionTypeSelector.SelectedIndex != 0)
                ClearAllParametersVisibility = Visibility.Visible;

            UpdateTemplateListVisibility();
            IsResetSelectParameter();
        }

        /// <summary>
        /// 根据模板成员属性来更新它们的可见性
        /// </summary>
        private void UpdateTemplateListVisibility(bool Searched = false)
        {
            //忽略版本
            bool IgnoreVersion = false;
            //忽略文件类型
            bool IgnoreFileType = false;
            //忽略功能类型
            bool IgnoreFunctionType = false;

            //当前选中的版本
            string CurrentVersion = "";
            if (VersionSelector != null && VersionSelector.SelectedItem != null && VersionSelector.SelectedIndex != 0)
                CurrentVersion = (VersionSelector.SelectedItem as TextSource).ItemText;
            else
                IgnoreVersion = true;
            //当前选中文件类型
            string CurrentFileType = "";
            if (FileTypeSelector != null && FileTypeSelector.SelectedItem != null && FileTypeSelector.SelectedIndex != 0)
                CurrentFileType = (FileTypeSelector.SelectedItem as TextSource).ItemText;
            else
                IgnoreFileType = true;

            //当前选中功能类型
            string CurrentFunctionType = "";
            if (FunctionTypeSelector != null && FunctionTypeSelector.SelectedItem != null && FunctionTypeSelector.SelectedIndex != 0 )
                CurrentFunctionType = (FunctionTypeSelector.SelectedItem as TextSource).ItemText;
            else
                IgnoreFunctionType = true;

            //若全部忽略则显示逻辑由搜索框负责
            if(IgnoreVersion && IgnoreFileType && IgnoreFunctionType)
            {
                TemplateList.All(item => { item.Visibility = Visibility.Visible; return true; });
                return;
            }

            //获取版本集合
            string[] VersionList = Directory.GetDirectories(TemplateDataFilePath);

            foreach (TemplateItems templateItem in TemplateList)
            {
                DockPanel container = templateItem.TemplateTypeTagPanel;

                //比较版本、文件类型和功能类型是否相同
                bool SameVersion = false;
                bool SameFileType = false;
                bool SameFunctionType = false;

                #region 筛选版本和文件类型
                if (IgnoreFileType && FileTypeSelector.SelectedIndex == 0)
                {
                    string CurrentFileNameWithOutExtension = TemplateDataFilePath + "\\" + CurrentVersion + "\\" + templateItem.TemplateID+".";

                    ItemCollection ExtensionList = FileTypeSelector.Items;
                    foreach (TextSource extension in ExtensionList)
                    {
                        string extensionString = extension.ItemText.ToLower();
                        if(File.Exists(CurrentFileNameWithOutExtension + extensionString))
                        {
                            SameVersion = true;
                            break;
                        }
                    }
                }
                else
                if (File.Exists(TemplateDataFilePath + "\\" + CurrentVersion + "\\" + templateItem.TemplateID + "." + CurrentFileType.ToLower()) || IgnoreVersion)
                    SameVersion = true;

                if (IgnoreVersion)
                    SameVersion = true;
                #endregion

                foreach (TemplateTypeTag item in container.Children)
                {
                    if (item.Text.Text == CurrentFileType || IgnoreFileType)
                        SameFileType = true;
                    if (item.Text.Text == CurrentFunctionType || IgnoreFunctionType)
                        SameFunctionType = true;

                    if (SameFileType && SameFunctionType)
                        break;
                }

                if(Searched)
                {
                    if ((!SameVersion || !SameFileType || !SameFunctionType) && templateItem.Visibility == Visibility.Visible)
                        templateItem.Visibility = Visibility.Collapsed;
                }
                else
                if(SameVersion && SameFileType && SameFunctionType)
                    templateItem.Visibility = Visibility.Visible;
                else
                templateItem.Visibility = Visibility.Collapsed;

                //搜索指定特征的模板
                if (!Searched && SearchBox != null && SearchBox.Text.Trim() != "")
                SearchBoxTextChanged(null,null);
            }
        }

        /// <summary>
        /// 判断是否隐藏清除筛选按钮
        /// </summary>
        private void IsResetSelectParameter()
        {
            if (VersionSelector != null && FileTypeSelector != null && FunctionTypeSelector != null)
                if (VersionSelector.SelectedIndex == 0 && FileTypeSelector.SelectedIndex == 0 && FunctionTypeSelector.SelectedIndex == 0)
                ClearAllParametersVisibility = Visibility.Hidden;
        }

        /// <summary>
        /// 载入近期使用的模板列表元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecentTemplateListLoaded(object sender, RoutedEventArgs e)
        {
            if(Directory.Exists(RecentTemplateDataFilePath) && File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js"))
            {
                string[] recentTemplateDataList = Directory.GetFiles(RecentTemplateDataFilePath);
                JsonScript(js_file);

                foreach (string recentTemplateData in recentTemplateDataList)
                {
                    string recentTemplateString = File.ReadAllText(recentTemplateData);
                    JsonScript("parseJSON(" + recentTemplateString + ");");

                    string filePath = JsonScript("getJSON('.filePath');").ToString();
                    string typeName = JsonScript("getJSON('.typeName');").ToString();
                    string fileImage = JsonScript("getJSON('.fileImage');").ToString();
                    string templateType = JsonScript("getJSON('.templateType');").ToString();

                    RecentTemplateItems recentTemplateItems = new RecentTemplateItems(filePath,typeName,fileImage,templateType);

                    recentTemplateItems.MouseLeftButtonUp += RecentTemplateItemsMouseLeftButtonUp;

                    RecentTemplateList.Add(recentTemplateItems);
                }
            }
        }

        /// <summary>
        /// 已选择某个历史模板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecentTemplateItemsMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// 获取模板容器引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateListViewerLoaded(object sender, RoutedEventArgs e)
        {
            SelectedTemplateTypeTagList.Clear();
            if (Directory.Exists(TemplateMetaDataFilePath) && File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js"))
            {
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");
                List<string> templateList = Directory.GetFiles(TemplateMetaDataFilePath).ToList();
                JsonScript(js_file);

                foreach (string template in templateList)
                {
                    string FileName = Path.GetFileNameWithoutExtension(template);//获取文件名

                    string templateData = File.ReadAllText(template);
                    JsonScript("parseJSON(" + templateData + ");");
                    string description = JsonScript("getJSON('.description');").ToString();
                    string name = JsonScript("getJSON('.name');").ToString();
                    string FileType = JsonScript("getJSON('.FileType');").ToString();
                    string FunctionType = JsonScript("getJSON('.FunctionType');").ToString();

                    #region 载入文件类型和功能类型
                    if (FileTypeList.Where(item=>item.ItemText == FileType).Count() == 0)
                    {
                        FileTypeList.Add(new TextSource() { ItemText = FileType });
                    }
                    if (FunctionTypeList.Where(item => item.ItemText == FunctionType).Count() == 0)
                    {
                        FunctionTypeList.Add(new TextSource() { ItemText = FunctionType });
                    }
                    #endregion

                    #region 实例化模板成员
                    TemplateItems templateItem = new TemplateItems
                    {
                        TemplateID = FileName,
                        FileType = FileType,
                        FunctionType = FunctionType
                    };
                    templateItem.TemplateName.Text = name;
                    templateItem.TemplateDescription.Text = description;
                    templateItem.MouseLeftButtonUp += TemplateItemMouseLeftButtonUp;

                    //判断获取的文件名是否有对应的图标文件
                    if (File.Exists(TemplateIconFilePath + "\\" + FileName + ".png"))
                        templateItem.TemplateImage.Source = new BitmapImage(
                            new Uri(TemplateIconFilePath + "\\" + FileName + ".png", UriKind.Absolute));

                    //添加文件类型和功能类型标签
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FileType));
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FunctionType));

                    TemplateList.Add(templateItem);
                    #endregion
                }

                //设置默认的文件类型
                DefaultFileType = FileTypeList.Last();
            }
        }

        /// <summary>
        /// 加入或退出已选择模板列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemplateItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TemplateItems current = sender as TemplateItems;
            if (current.TemplateSelector.IsChecked.Value)
                SelectedTemplateItemList.Add(current);
            else
                SelectedTemplateItemList.Remove(current);
        }

        #endregion

        #region 属性设置窗体逻辑处理

        #region 属性设置窗体:上一步、下一步和设置路径等指令
        public RelayCommand<Window> AttributeLastStep { get; set; }
        public RelayCommand<Window> AttributeNextStep { get; set; }
        public RelayCommand SetDatapackPath { get; set; }
        public RelayCommand SetDatapackDescription { get; set; }
        public RelayCommand AddDatapackFilter { get; set; }
        public RelayCommand ClearDatapackFilter { get; set; }
        public RelayCommand CopyDatapackName { get; set; }
        public RelayCommand CopyDatapackPath { get; set; }
        public RelayCommand CopyDatapackDescription { get; set; }
        public RelayCommand CopyDatapackMainNameSpace { get; set; }
        public RelayCommand<TextCheckBoxs> SynchronizePackAndNameSpaceName { get; set; }
        #endregion

        #region 存储数据包的名称
        private string datapackName = "Datapack";
        public string DatapackName
        {
            get { return datapackName; }
            set
            {
                datapackName = value;

                if (datapackName.Trim() == "")
                    DatapackNameIsNull = Visibility.Visible;
                else
                    DatapackNameIsNull = Visibility.Hidden;

                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包的主命名空间名称
        private string datapackMainNameSpace = "Datapack";
        public string DatapackMainNameSpace
        {
            get { return datapackMainNameSpace; }
            set
            {
                datapackMainNameSpace = value;

                if (datapackMainNameSpace.Trim() == "")
                    DatapackNameIsNull = Visibility.Visible;
                else
                    DatapackNameIsNull = Visibility.Hidden;

                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包的保存路径
        private static TextSource selectedDatapackPath = new TextSource() { ItemText = "" };
        public TextSource SelectedDatapackPath
        {
            get { return selectedDatapackPath; }
            set
            {
                selectedDatapackPath = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包对应的游戏版本
        private TextSource selectedDatapackVersion = new TextSource() { ItemText = "" };
        public TextSource SelectedDatapackVersion
        {
            get { return selectedDatapackVersion; }
            set
            {
                selectedDatapackVersion = value;
            }
        }
        #endregion

        #region 数据包名称为空时的提示可见性
        private Visibility datapackNameIsNull = Visibility.Hidden;
        public Visibility DatapackNameIsNull
        {
            get { return datapackNameIsNull; }
            set
            {
                datapackNameIsNull = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 数据包命名空间为空时的提示可见性
        private Visibility datapackMainNameSpaceIsNull = Visibility.Hidden;
        public Visibility DatapackMainNameSpaceIsNull
        {
            get { return datapackMainNameSpaceIsNull; }
            set
            {
                datapackMainNameSpaceIsNull = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 命名空间可操作性
        private bool canModifyNameSpace = true;
        public bool CanModifyNameSpace
        {
            get { return canModifyNameSpace; }
            set
            {
                canModifyNameSpace = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包的描述类型
        private TextSource selectedDatapackDescriptionType = new TextSource() { ItemText = "" };
        public TextSource SelectedDatapackDescriptionType
        {
            get { return selectedDatapackDescriptionType; }
            set
            {
                selectedDatapackDescriptionType = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包的描述
        private string selectedDatapackDescription = "This is a Datapack";
        public string SelectedDatapackDescription
        {
            get { return selectedDatapackDescription; }
            set
            {
                selectedDatapackDescription = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包JSON组件类型的描述数据
        private string jsonObjectDescription = "";
        private string JsonObjectDescription
        {
            get { return jsonObjectDescription; }
            set
            {
                jsonObjectDescription = value;
            }
        }
        private string jsonArrayDescription = "";
        private string JsonArrayDescription
        {
            get { return jsonArrayDescription; }
            set
            {
                jsonArrayDescription = value;
            }
        }
        #endregion

        #region 存储数据包的过滤器
        public static ObservableCollection<FilterItems> DatapackFilterSource { get; set; } = new ObservableCollection<FilterItems> { };
        private string datapackFilter = "";
        public string DatapackFilter
        {
            get { return datapackFilter; }
            set
            {
                datapackFilter = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 存储数据包简介类型键入控件父级容器、布尔类型、切换控件的引用
        TextComboBoxs DescriptionBoolBox = null;
        DockPanel DescriptionContainer = null;
        TextComboBoxs DescriptionTypeSwitcher = null;
        #endregion

        //数据包历史路径列表
        public ObservableCollection<TextSource> HistoryDatapackGeneratorPathList { get; set; } = new ObservableCollection<TextSource> { };

        //存储数据包历史路径配置文件
        string HistoryDatapackGeneratorPathFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\historyDatapackGeneratorPath.ini";

        //数据包所对应游戏版本配置文件路径
        string DatapackVersionFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\datapackVersion.json";

        //数据包简介数据类型配置文件路径
        string DatapackDescriptionTypeFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\descriptionTypeList.ini";

        //数据包所对应游戏版本数据库
        Dictionary<string, string> DatapackVersionDatabase = new Dictionary<string, string> { };

        //用于显示数据包简介的文档对象
        List<EnabledFlowDocument> DescriptionDisplayDocument = null;

        /// <summary>
        /// 初始化已选中的所有模板的标签类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TypeTagListLoaded(object sender,RoutedEventArgs e)
        {
            DockPanel container = sender as DockPanel;
            bool IsFirst = true;
            foreach (string SelectedTemplateTypeTag in SelectedTemplateTypeTagList)
            {
                TemplateTypeTag templateTypeTag = new TemplateTypeTag(SelectedTemplateTypeTag);
                if (IsFirst)
                    templateTypeTag.Margin = new Thickness(0);
                IsFirst = false;
                container.Children.Add(templateTypeTag);
            }
        }

        /// <summary>
        /// 初始化数据包简介数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DatapackDescriptionTypeLoaded(object sender, RoutedEventArgs e)
        {
            DescriptionTypeSwitcher = sender as TextComboBoxs;

            if (File.Exists(DatapackDescriptionTypeFilePath))
            {
                ObservableCollection<TextSource> descriptionList = new ObservableCollection<TextSource> { };
                DescriptionTypeSwitcher.ItemsSource = descriptionList;

                #region 解析简介类型配置文件
                string[] DescriptionList = File.ReadAllLines(DatapackDescriptionTypeFilePath);

                foreach (string descriptionItem in DescriptionList)
                {
                    descriptionList.Add(new TextSource() { ItemText = descriptionItem });
                }
                #endregion
            }
        }

        /// <summary>
        /// 初始化数据包所对应游戏版本数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DatapackVersionLoaded(object sender, RoutedEventArgs e)
        {
            TextComboBoxs VersionBox = sender as TextComboBoxs;

            if (File.Exists(DatapackVersionFilePath))
            {
                ObservableCollection<TextSource> versionList = new ObservableCollection<TextSource> { };
                VersionBox.ItemsSource = versionList;

                #region 解析版本配置文件
                string VersionFile = File.ReadAllText(DatapackVersionFilePath);
                string[] versionStringList = VersionFile.Replace("{", "").Replace("}", "").Replace("\"", "").Split(',');
                foreach (string versionItem in versionStringList)
                {
                    string[] itemData = versionItem.Split(':');
                    string display = itemData[0].Trim();
                    string value = itemData[1].Trim();
                    versionList.Add(new TextSource() { ItemText = display });
                    DatapackVersionDatabase.Add(display, value);
                }
                #endregion

                if(versionList.Count > 0)
                SelectedDatapackVersion = versionList[0];
            }
        }

        /// <summary>
        /// 初始化数据包简介父级容器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DatapackDescriptionContainerLoaded(object sender, RoutedEventArgs e)
        {
            DescriptionContainer = sender as DockPanel;
        }

        /// <summary>
        /// 初始化数据包简介布尔类数据源
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DatapackDescriptionBoolTypeLoaded(object sender, RoutedEventArgs e)
        {
            //获取引用
            DescriptionBoolBox = sender as TextComboBoxs;
            DescriptionBoolBox.Items.Add(new TextSource() { ItemText = "true" });
            DescriptionBoolBox.Items.Add(new TextSource() { ItemText = "false" });
        }

        /// <summary>
        /// 同步设置不同数据类型的控件可见性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DescriptionTypeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextSource CurrentItem = DescriptionTypeSwitcher.SelectedItem as TextSource;
            string CurrentValue = CurrentItem.ItemText;
            foreach (FrameworkElement item in DescriptionContainer.Children)
            {
                if (item.Uid == CurrentValue || item.Uid.Contains(CurrentValue))
                    item.Visibility = Visibility.Visible;
                else
                    if(item.Uid != "")
                    item.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 添加一个数据包过滤器成员
        /// </summary>
        private void AddDatapackFilterCommand()
        {
            FilterItems filterItems = new FilterItems();
            DatapackFilterSource.Add(filterItems);
        }

        /// <summary>
        /// 清空数据包的过滤器
        /// </summary>
        private void ClearDatapackFilterCommand()
        {
            DatapackFilterSource.Clear();
        }

        /// <summary>
        /// 设置数据包的描述
        /// </summary>
        private void SetDatapackDescriptionCommand()
        {
            //实例化一个成书生成器作为JSON文本编辑工具
            WrittenBook writtenBook = new WrittenBook(DataPack.cbhk,true);
            written_book_datacontext datacontext = writtenBook.DataContext as written_book_datacontext;

            if (DescriptionDisplayDocument != null)
            {
                datacontext.HistroyFlowDocumentList = DescriptionDisplayDocument;
            }

            if (writtenBook.ShowDialog() == true)
            {
                DescriptionDisplayDocument = datacontext.HistroyFlowDocumentList;
                JsonArrayDescription = datacontext.result;
                JsonObjectDescription = datacontext.object_result;
            }
        }

        /// <summary>
        /// 设置数据包的路径
        /// </summary>
        private void SetDatapackPathCommand()
        {
            BetterFolderBrowser folderBrowser = new BetterFolderBrowser()
            {
                Multiselect = false,
                Title = "请选择当前数据包生成路径",
                RootFolder = Environment.SpecialFolder.MyComputer.ToString(),
            };

            if(folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(Directory.Exists(folderBrowser.SelectedFolder))
                {
                    string selectedPath = folderBrowser.SelectedFolder;
                    if (HistoryDatapackGeneratorPathList.Where(item=>item.ItemText == selectedPath).Count() == 0)
                    HistoryDatapackGeneratorPathList.Add(new TextSource() { ItemText = selectedPath });

                    if(SelectedDatapackPath.ItemText == "")
                       SelectedDatapackPath = HistoryDatapackGeneratorPathList[0];
                }
            }
        }

        /// <summary>
        /// 属性设置窗体进入下一步
        /// </summary>
        private void AttributeNextStepCommand(Window target)
        {
            #region 整理数据包的各种属性
            foreach (FilterItems filterItem in DatapackFilterSource)
            {
            }
            #endregion
            //关闭属性设置窗体
            target.DialogResult = true;
        }

        /// <summary>
        /// 属性设置窗体进入上一步
        /// </summary>
        private void AttributeLastStepCommand(Window target)
        {
            //关闭属性设置窗体
            target.DialogResult = false;
        }

        /// <summary>
        /// 复制数据包的简介
        /// </summary>
        private void CopyDatapackDescriptionCommand()
        {
            Clipboard.SetText(SelectedDatapackDescription);
        }

        /// <summary>
        /// 复制数据包的路径
        /// </summary>
        private void CopyDatapackPathCommand()
        {
            Clipboard.SetText(SelectedDatapackPath.ItemText);
        }

        /// <summary>
        /// 复制数据包的名称
        /// </summary>
        private void CopyDatapackNameCommand()
        {
            Clipboard.SetText(DatapackName);
        }

        /// <summary>
        /// 复制数据包的主命名空间名称
        /// </summary>
        private void CopyDatapackMainNameSpaceCommand()
        {
            Clipboard.SetText(DatapackMainNameSpace);
        }

        /// <summary>
        /// 同步主命名空间和数据包的名称
        /// </summary>
        private void SynchronizePackAndNameSpaceNameCommand(TextCheckBoxs box)
        {
            CanModifyNameSpace = !box.IsChecked.Value;
            DatapackMainNameSpace = DatapackName;
        }
        #endregion
    }
}
