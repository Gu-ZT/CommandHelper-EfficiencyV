using cbhk_environment.CustomControls;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
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
        //原版命名空间配置文件路径
        static string OriginalTargetFolderNameListFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\DataPack\\data\\minecraftNameSpaceList.ini";
        //存储目标目录类型列表
        static List<string> TargetFolderNameList = new List<string> { };
        //存储能够读取的文件类型列表
        static List<string> ReadableFileExtensionList = new List<string> { };
        //存储原版命名空间类型列表
        static List<string> OriginalTargetFolderNameList = new List<string> { };

        /// <summary>
        /// 传入目标文件夹路径，返回数据包节点对象，该对象订阅展开事件
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static RichTreeViewItems ReadTargetContent(string folderPath,ContentType type)
        {
            RichTreeViewItems result = null;

            #region 初始化配置文件
            if (File.Exists(TargetFolderNameListFilePath) && TargetFolderNameList.Count == 0)
                TargetFolderNameList = File.ReadAllLines(TargetFolderNameListFilePath).ToList();

            if (File.Exists(ReadableFileExtensionListFilePath) && ReadableFileExtensionList.Count == 0)
                ReadableFileExtensionList = File.ReadAllLines(ReadableFileExtensionListFilePath).ToList();

            if (File.Exists(OriginalTargetFolderNameListFilePath) && OriginalTargetFolderNameList.Count == 0)
                OriginalTargetFolderNameList = File.ReadAllLines(OriginalTargetFolderNameListFilePath).ToList();
            #endregion

                //如果路径不为空且目标命名空间和读取配置列表均不为空则继续执行
            if (folderPath != null && TargetFolderNameList.Count > 0 && ReadableFileExtensionList.Count > 0)
            {
                switch (type)
                {
                    case ContentType.DataPack:
                        if (Directory.Exists(folderPath))
                        {
                            //拥有pack.mcmeta文件和data文件夹,证实确实是数据包文件夹
                            if (Directory.Exists(folderPath + "\\data") && File.Exists(folderPath + "\\pack.mcmeta"))
                            {
                                ContentItems contentItems = new ContentItems(folderPath, type);
                                RichTreeViewItems CurrentNode = new RichTreeViewItems()
                                {
                                    Uid = folderPath,
                                    Tag = type,
                                    Header = contentItems
                                };

                                result = CurrentNode;

                                //如果数据包头的子级有目录则创建一个空子级
                                int SubDirectoryCount = Directory.GetDirectories(folderPath + "\\data").Count();
                                if (SubDirectoryCount > 0)
                                {
                                    CurrentNode.Expanded += GetCurrentSubContent;
                                    CurrentNode.Items.Add("");
                                }
                            }
                        }
                        break;
                    case ContentType.Folder:
                        if (Directory.Exists(folderPath))
                        {
                            string[] subContent = Directory.GetDirectories(folderPath);

                            //读取所有目录
                            foreach (string item in subContent)
                            {
                                //实例化一个文件夹节点
                                ContentItems contentItems = new ContentItems(item, type);
                                RichTreeViewItems CurrentNode = new RichTreeViewItems
                                {
                                    Uid = item,
                                    Tag = type,
                                    Header = contentItems
                                };

                                result = CurrentNode;

                                int SubDirectoryCount = Directory.GetDirectories(item).Count();
                                if (SubDirectoryCount > 0)
                                {
                                    CurrentNode.Expanded += GetCurrentSubContent;
                                    CurrentNode.Items.Add("");
                                }
                                break;
                            }
                        }
                        break;
                    case ContentType.File:
                        if (File.Exists(folderPath))
                        {
                            string[] subContent = Directory.GetFiles(folderPath);
                            foreach (string item in subContent)
                            {
                                //实例化一个文件节点
                                ContentItems contentItems = new ContentItems(item, type);
                                RichTreeViewItems CurrentNode = new RichTreeViewItems
                                {
                                    Uid = item,
                                    Tag = type,
                                    Header = contentItems,
                                };
                                result = CurrentNode;
                                break;
                            }
                        }
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// 分析指定内容属于哪个类型
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public static ContentType GetTargetContentType(string contentPath)
        {
            return ContentType.DataPack;
        }

        /// <summary>
        /// 获取当前节点的子级内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void GetCurrentSubContent(object sender, RoutedEventArgs e)
        {
            RichTreeViewItems CurrentNode = sender as RichTreeViewItems;
            ContentType type = (ContentType)CurrentNode.Tag;

            //若已处理则返回
            if (CurrentNode.Items.Count >= 1 && !(CurrentNode.Items[0] is string)) return;

            //清空初始化的子级
            CurrentNode.Items.Clear();
            string currentPath = CurrentNode.Uid;
            switch (type)
            {
                //当前内容节点类型为数据包
                case ContentType.DataPack:

                    //获取命名空间上级目录
                    string NameSpaceParentPath = CurrentNode.Uid + "\\data";
                    //遍历能够执行逻辑的命名空间
                    string[] suspectedTargetNameSpaceList = Directory.GetDirectories(NameSpaceParentPath);
                    foreach (string suspectedTargetNameSpace in suspectedTargetNameSpaceList)
                    {
                        string folderName = Path.GetFileNameWithoutExtension(suspectedTargetNameSpace);
                        List<string> matchDirectries = TargetFolderNameList.Where(TargetFolderName => Directory.Exists(suspectedTargetNameSpace + "\\" + TargetFolderName)).ToList();
                        //List<string> matchOriginalDirectries = OriginalTargetFolderNameList.Where(TargetFolderName => Directory.Exists(suspectedTargetNameSpace + "\\" + TargetFolderName)).ToList();

                        ////检测原版命名空间是否存在
                        //if (folderName.Trim() == "minecraft" && Directory.Exists(suspectedTargetNameSpace))
                        //{

                        //}

                        //检测到当前文件夹为命名空间
                        if(matchDirectries.Count > 0 /*&& folderName.Trim() != "minecraft"*/)
                        {
                            #region 新建内容节点
                            ContentType newType = ContentType.FolderOrFile;
                            ContentItems contentItems = new ContentItems(suspectedTargetNameSpace, newType);
                            RichTreeViewItems SubNode = new RichTreeViewItems()
                            {
                                Header = contentItems,
                                Tag = newType,
                                Uid = suspectedTargetNameSpace
                            };
                            CurrentNode.Items.Add(SubNode);

                            int SubDirectoriesCount = Directory.GetDirectories(suspectedTargetNameSpace).Count();
                            int SubFilesCount = Directory.GetFiles(suspectedTargetNameSpace).Count();

                            //如果拥有子级内容则添加一个空子级并订阅展开事件
                            if (SubDirectoriesCount + SubFilesCount > 0)
                            {
                                SubNode.Expanded += GetCurrentSubContent;
                                SubNode.Items.Add("");
                            }
                            #endregion
                        }
                    }
                    break;
                case ContentType.FolderOrFile:
                    #region 获取所有子级内容并新建内容节点
                    string[] SubDirectories = Directory.GetDirectories(currentPath);
                    string[] SubFiles = Directory.GetFiles(currentPath);

                    foreach (string SubDirectory in SubDirectories)
                    {
                        ContentItems contentItems = new ContentItems(SubDirectory,type);

                        RichTreeViewItems SubNode = new RichTreeViewItems()
                        {
                            Header = contentItems,
                            Tag = type,
                            Uid = SubDirectory
                        };

                        CurrentNode.Items.Add(SubNode);

                        int SubDirectoriesCount = Directory.GetDirectories(SubDirectory).Count();
                        int SubFilesCount = Directory.GetFiles(SubDirectory).Count();

                        //如果拥有子级内容则添加一个空子级并订阅展开事件
                        if (SubDirectoriesCount + SubFilesCount > 0)
                        {
                            SubNode.Expanded += GetCurrentSubContent;
                            SubNode.Items.Add("");
                        }
                    }

                    ContentType fileType = ContentType.File;
                    foreach (string SubFile in SubFiles)
                    {
                        ContentItems contentItems = new ContentItems(SubFile, fileType);
                        CurrentNode.Items.Add(contentItems);
                    }
                    #endregion
                    break;
            }
        }

        /// <summary>
        /// 用于区分需要新建内容的类型
        /// </summary>
        public enum ContentType
        {
            DataPack,
            FolderOrFile,
            Folder,
            File,
            UnKnown
        }
    }
}
