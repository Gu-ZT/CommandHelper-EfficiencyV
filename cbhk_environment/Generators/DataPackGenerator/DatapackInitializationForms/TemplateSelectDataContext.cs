using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.Generators.DataPackGenerator.Components;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms
{
    public class TemplateSelectDataContext: ObservableObject
    {
        /// <summary>
        /// 初始化模板选择和属性设置窗体数据上下文
        /// </summary>
        public TemplateSelectDataContext()
        {
            #region 链接命令
            TemplateLastStep = new RelayCommand<Page>(TemplateLastStepCommand);
            TemplateNextStep = new RelayCommand<Page>(TemplateNextStepCommand);
            ClearAllSelectParameters = new RelayCommand(ClearAllSelectParametersCommand);
            #endregion

            #region 载入版本
            if (Directory.Exists(TemplateDataFilePath))
            {
                string[] versionList = Directory.GetDirectories(TemplateDataFilePath);

                #region 版本、文件类型、功能类型的默认选中项
                if (VersionList.Count == 0)
                    VersionList.Add("所有版本");

                if (FileTypeList.Count == 0)
                    FileTypeList.Add("所有文件类型");

                if (FunctionTypeList.Count == 0)
                    FunctionTypeList.Add("所有功能类型");
                #endregion

                foreach (string version in versionList)
                {
                    string versionString = Path.GetFileName(version);
                    VersionList.Add(versionString);
                }

                DefaultVersion = VersionList.Last();
            }
            #endregion
        }

        #region 存储已选中的模板
        public static List<TemplateItems> SelectedTemplateItemList = new List<TemplateItems>() { };
        #endregion

        #region 模板存放路径
        public static string TemplateDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\presets";
        #endregion

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

        #region 模板图标文件路径
        public static string TemplateIconFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\images";
        #endregion

        #region 近期使用的模板存放路径
        public static string RecentTemplateDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\recent_templates";
        #endregion

        /// <summary>
        /// 模板元数据存放路径
        /// </summary>
        string TemplateMetaDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\introductions";

        /// <summary>
        /// 近期使用的模板成员
        /// </summary>
        public static ObservableCollection<RecentTemplateItems> RecentTemplateList { get; set; } = new ObservableCollection<RecentTemplateItems>();

        /// <summary>
        /// 模板成员集合
        /// </summary>
        public ObservableCollection<TemplateItems> TemplateList { get; set; } = new ObservableCollection<TemplateItems>();

        /// <summary>
        /// 模板状态锁，防止更新死循环
        /// </summary>
        public static bool TemplateCheckLock = false;

        /// <summary>
        /// 存放版本列表
        /// </summary>
        public ObservableCollection<string> VersionList { get; set; } = new ObservableCollection<string> { };

        /// <summary>
        /// 存放文件类型列表
        /// </summary>
        public ObservableCollection<string> FileTypeList { get; set; } = new ObservableCollection<string> { };

        /// <summary>
        /// 存放功能类型列表
        /// </summary>
        public ObservableCollection<string> FunctionTypeList { get; set; } = new ObservableCollection<string> { };

        /// <summary>
        /// 已选择的版本
        /// </summary>
        public string SelectedVersionString = "";
        /// <summary>
        /// 已选择的文件类型
        /// </summary>
        public string SelectedFileTypeString = "";

        #region 获取搜索文本框引用
        TextBox SearchBox = null;
        #endregion

        #region 存储已选择的版本
        private static string selectedVersion = null;
        public static string SelectedVersion
        {
            get { return selectedVersion; }
            set
            {
                selectedVersion = value;
            }
        }
        #endregion

        #region 存储已选择的文件类型
        private static string selectedFileType = null;
        public static string SelectedFileType
        {
            get { return selectedFileType; }
            set
            {
                selectedFileType = value;
            }
        }
        #endregion

        #region 存储默认版本
        private static string defaultVersion = null;
        public static string DefaultVersion
        {
            get { return defaultVersion; }
            set
            {
                defaultVersion = value;
            }
        }
        #endregion

        #region 存储默认文件类型
        private static string defaultFileType = null;
        public static string DefaultFileType
        {
            get { return defaultFileType; }
            set
            {
                defaultFileType = value;
            }
        }
        #endregion

        #region 模板窗体:上一步、下一步命令和清除所有筛选
        public RelayCommand<Page> TemplateLastStep { get; set; }
        public RelayCommand<Page> TemplateNextStep { get; set; }
        public RelayCommand ClearAllSelectParameters { get; set; }
        #endregion

        #region 存储版本、文件类型和功能类型选择框的引用
        ComboBox VersionSelector = null;
        ComboBox FileTypeSelector = null;
        ComboBox FunctionTypeSelector = null;
        #endregion

        #region 存储所有类型的模板标签
        public static List<string> SelectedTemplateTypeTagList = new List<string> { };
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

        #region 模板选择窗体逻辑处理

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
        /// 返回主页
        /// </summary>
        private void TemplateLastStepCommand(Page page)
        {
            NavigationService.GetNavigationService(page).GoBack();
        }

        /// <summary>
        /// 进入下一步
        /// </summary>
        private void TemplateNextStepCommand(Page page)
        {
            //获取已选择的版本和文件类型
            SelectedVersionString = SelectedVersion;
            SelectedFileTypeString = SelectedFileType.ToLower();

            if (SelectedVersionString == "所有版本")
                SelectedVersionString = DefaultVersion;
            if (SelectedFileTypeString == "所有文件类型")
                SelectedFileTypeString = DefaultFileType.ToLower();

            foreach (var selectedTemplateItemList in SelectedTemplateItemList)
            {
                string FileType = selectedTemplateItemList.FileType;
                string FunctionType = selectedTemplateItemList.FunctionType;
                SelectedTemplateTypeTagList.Clear();
                SelectedTemplateTypeTagList.Add(FileType);
                SelectedTemplateTypeTagList.Clear();
                SelectedTemplateTypeTagList.Add(FunctionType);
            }

            //载入数据包属性设置页
            datapack_datacontext datapackDatacontext = Window.GetWindow(page).DataContext as datapack_datacontext;
            DatapackGenerateSetupPage datapackGenerateSetupPage = datapackDatacontext.GetDatapackGenerateSetupPage();
            if (datapackGenerateSetupPage == null)
                datapackGenerateSetupPage = new DatapackGenerateSetupPage();
            if(datapackGenerateSetupPage.DataContext == null)
            datapackGenerateSetupPage.DataContext = new DatapackGenerateSetupDataContext();
            NavigationService.GetNavigationService(page).Navigate(datapackGenerateSetupPage);
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

            //设置清除筛选按钮可见
            ClearAllParametersVisibility = Visibility.Visible;

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
            VersionSelector = sender as ComboBox;
        }

        /// <summary>
        /// 获取文件类型选择框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FileTypeSelectorLoaded(object sender, RoutedEventArgs e)
        {
            FileTypeSelector = sender as ComboBox;
        }

        /// <summary>
        /// 获取功能类型选择框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FunctionSelectorLoaded(object sender, RoutedEventArgs e)
        {
            FunctionTypeSelector = sender as ComboBox;
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
                CurrentVersion = VersionSelector.SelectedItem.ToString();
            else
                IgnoreVersion = true;
            //当前选中文件类型
            string CurrentFileType = "";
            if (FileTypeSelector != null && FileTypeSelector.SelectedItem != null && FileTypeSelector.SelectedIndex != 0)
                CurrentFileType = FileTypeSelector.SelectedItem.ToString();
            else
                IgnoreFileType = true;

            //当前选中功能类型
            string CurrentFunctionType = "";
            if (FunctionTypeSelector != null && FunctionTypeSelector.SelectedItem != null && FunctionTypeSelector.SelectedIndex != 0 )
                CurrentFunctionType = FunctionTypeSelector.SelectedItem.ToString();
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
                    foreach (string extension in ExtensionList)
                    {
                        string extensionString = extension.ToLower();
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
            if (Directory.Exists(RecentTemplateDataFilePath) && File.Exists(js_file) && RecentTemplateList.Count == 0)
            {
                string[] recentTemplateDataList = Directory.GetFiles(RecentTemplateDataFilePath,"*.*",SearchOption.AllDirectories);
                JsonScript(js_file);
                foreach (string recentTemplateData in recentTemplateDataList)
                {
                    string recentTemplateString = File.ReadAllText(recentTemplateData);
                    JsonScript("var data =" + recentTemplateString);

                    string templateName = JsonScript("data.TemplateName").ToString();
                    string fileType = JsonScript("data.FileType").ToString();
                    string nameSpace = JsonScript("data.FileNameSpace").ToString();
                    string filePath = JsonScript("data.FilePath").ToString();
                    string iconPath = JsonScript("data.IconPath").ToString();

                    RecentTemplateItems recentTemplateItems = new RecentTemplateItems(filePath, fileType, templateName, iconPath, nameSpace)
                    {
                        initializationDatacontext = this
                    };
                    RecentTemplateList.Add(recentTemplateItems);
                }
            }
        }

        /// <summary>
        /// 获取模板容器引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateListViewerLoaded(object sender, RoutedEventArgs e)
        {
            SelectedTemplateTypeTagList.Clear();
            if (Directory.Exists(TemplateMetaDataFilePath) && File.Exists(js_file) && TemplateList.Count == 0)
            {
                string js_file = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\data_sources\\json_reader.js");
                List<string> templateList = Directory.GetFiles(TemplateMetaDataFilePath).ToList();
                JsonScript(js_file);

                foreach (string template in templateList)
                {
                    string templateData = File.ReadAllText(template);
                    JsonScript("var data =" + templateData);

                    string FileName = Path.GetFileNameWithoutExtension(template);
                    string Description = JsonScript("data.Description").ToString();
                    string TemplateName = JsonScript("data.Name").ToString();
                    string FileType = JsonScript("data.FileType").ToString();
                    string FunctionType = JsonScript("data.FunctionType").ToString();
                    string fileNameSpace = JsonScript("data.NameSpace").ToString();

                    #region 载入文件类型和功能类型
                    if (FileTypeList.Where(item=>item == FileType).Count() == 0)
                    {
                        FileTypeList.Add(FileType);
                    }
                    if (FunctionTypeList.Where(item => item == FunctionType).Count() == 0)
                    {
                        FunctionTypeList.Add(FunctionType);
                    }
                    #endregion

                    #region 实例化模板成员
                    TemplateItems templateItem = new TemplateItems
                    {
                        TemplateID = FileName,
                        FileType = FileType,
                        FunctionType = FunctionType,
                        FileNameSpace = fileNameSpace
                    };

                    if(TemplateName != null)
                    templateItem.TemplateName.Text = TemplateName;
                    if(Description != null)
                    templateItem.TemplateDescription.Text = Description;

                    //判断获取的文件名是否有对应的图标文件
                    if (FileName != null && File.Exists(TemplateIconFilePath + "\\" + FileName + ".png"))
                        templateItem.TemplateImage.Source = new BitmapImage(
                            new Uri(TemplateIconFilePath + "\\" + FileName + ".png", UriKind.Absolute));

                    //添加文件类型和功能类型标签
                    if(FileType != null)
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FileType));
                    if(FunctionType != null)
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FunctionType));
                    TemplateList.Add(templateItem);
                    #endregion
                }

                //设置默认的文件类型
                DefaultFileType = FileTypeList.Last();
            }
        }

        #endregion
    }
}
