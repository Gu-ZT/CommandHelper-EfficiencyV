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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace cbhk_environment.Generators.DataPackGenerator.DatapackInitializationForms
{
    public class initialization_datacontext: ObservableObject
    {
        //模板图标文件存放路径
        string TemplateIconFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\images";

        //模板元数据存放路径
        string TemplateMetaDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\introductions";

        //模板存放路径
        string TemplateDataFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\templates\\presets";

        //存储已选中的模板
        List<TemplateItems> SelectedTemplateItemList = new List<TemplateItems>() { };

        //近期使用的模板成员
        public ObservableCollection<string> RecentTemplateList { get; set; } = new ObservableCollection<string>();

        //模板成员
        public ObservableCollection<TemplateItems> TemplateList { get; set; } = new ObservableCollection<TemplateItems>();

        //存放版本列表
        public ObservableCollection<TextSource> VersionList { get; set; } = new ObservableCollection<TextSource> { };

        //存放文件类型列表
        public ObservableCollection<TextSource> FileTypeList { get; set; } = new ObservableCollection<TextSource> { };

        //存放功能类型列表
        public ObservableCollection<TextSource> FunctionTypeList { get; set; } = new ObservableCollection<TextSource> { };

        #region 上一步和下一步命令
        public RelayCommand<Window> LastStep { get; set; }
        public RelayCommand NextStep { get; set; }
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

        /// <summary>
        /// 初始化模板选择窗体
        /// </summary>
        public initialization_datacontext()
        {
            #region 链接命令
            LastStep = new RelayCommand<Window>(LastStepCommand);
            NextStep = new RelayCommand(NextStepCommand);
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
            }
            #endregion
        }

        /// <summary>
        /// 返回上一步
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void LastStepCommand(Window target)
        {
            target.Close();
        }

        /// <summary>
        /// 进入下一步
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void NextStepCommand()
        {

        }

        /// <summary>
        /// 模板的版本切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateVersionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 模板的文件类型切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateFileTypeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 模板的功能类型切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateFunctionTypeChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// 载入近期使用的模板列表元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RecentTemplateListLoaded(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 获取模板容器引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TemplateListLoaded(object sender, RoutedEventArgs e)
        {
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
                    TemplateItems templateItem = new TemplateItems();
                    templateItem.TemplateName.Text = name;
                    templateItem.TemplateDescription.Text = description;

                    //判断获取的文件名是否有对应的图标文件
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\images\\" + FileName + ".png"))
                        templateItem.TemplateImage.Source = new BitmapImage(
                            new Uri(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\images\\" + FileName + ".png", UriKind.Absolute));

                    //添加文件类型和功能类型标签
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FileType));
                    templateItem.TemplateTypeTagPanel.Children.Add(new TemplateTypeTag(FunctionType));

                    TemplateList.Add(templateItem);
                    #endregion
                }
            }
        }
    }
}
