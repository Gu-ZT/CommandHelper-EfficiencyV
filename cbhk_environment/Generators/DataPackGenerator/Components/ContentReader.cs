using cbhk_environment.CustomControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    public class ContentReader
    {
        //目标类型目录配置文件路径
        static string TargetFolderNameListFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\targetFolderNameList.ini";
        //能够读取的文件类型配置文件路径
        static string ReadableFileExtensionListFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\ReadableFileExtensionList.ini";
        //存储目标目录类型列表
        static List<string> TargetFolderNameList = new List<string> { };
        //存储能够读取的文件类型列表
        static List<string> ReadableFileExtensionList = new List<string> { };

        /// <summary>
        /// 传入目标文件夹路径，返回数据包节点对象，该对象包含目标的所有数据
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static List<RichTreeViewItems> ReadTargetContent(string folderPath,bool IsNameSpace = true)
        {
            List<RichTreeViewItems> result = new List<RichTreeViewItems> { };

            //初始化配置文件
            if (File.Exists(TargetFolderNameListFilePath) && TargetFolderNameList.Count == 0)
                TargetFolderNameList = File.ReadAllLines(TargetFolderNameListFilePath).ToList();

            if (File.Exists(ReadableFileExtensionListFilePath) && ReadableFileExtensionList.Count == 0)
                ReadableFileExtensionList = File.ReadAllLines(ReadableFileExtensionListFilePath).ToList();

            //如果路径不为空且目标命名空间和读取配置列表均不为空则继续执行
            if (folderPath != null && TargetFolderNameList.Count > 0 && ReadableFileExtensionList.Count > 0)
            {
                if (Directory.Exists(folderPath) && IsNameSpace)
                {
                    string[] subContent = Directory.GetDirectories(folderPath);
                    string folderName = "";

                    //读取除minecraft以外的所有目录
                    foreach (string item in subContent)
                    {
                        folderName = Path.GetFileNameWithoutExtension(item).Trim();
                        if (folderName != "minecraft")
                        {
                            //只要当前迭代目录名称与目标目录列表中的元素匹配一个就确认是数据包的主目录
                            foreach (string target_item in TargetFolderNameList)
                            {
                                //遍历该包
                                string currentNameSpace = item + "\\" + target_item;
                                //实例化一个文件夹节点
                                ContentItems contentItems = new ContentItems(currentNameSpace,false,true);
                                RichTreeViewItems CurrentNode = new RichTreeViewItems
                                {
                                    Tag = currentNameSpace,
                                    Header = contentItems
                                };
                                CurrentNode.Expanded += datapack_datacontext.OpenSubContentClick;
                                CurrentNode.Items.Add("");
                                result.Add(CurrentNode);
                            }
                            break;
                        }
                    }
                }
                else
                    if(!IsNameSpace)
                {
                    string[] subContent = Directory.GetFiles(folderPath);

                    foreach (string item in subContent)
                    {
                        //实例化一个文件节点
                        ContentItems contentItems = new ContentItems(item, false, false);
                        RichTreeViewItems CurrentNode = new RichTreeViewItems
                        {
                            Tag = item,
                            Header = contentItems
                        };
                        CurrentNode.Expanded += datapack_datacontext.OpenSubContentClick;
                        CurrentNode.Items.Add("");
                        result.Add(CurrentNode);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
