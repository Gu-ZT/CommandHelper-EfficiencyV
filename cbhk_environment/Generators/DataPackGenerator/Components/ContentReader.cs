using cbhk_environment.CustomControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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
        public static List<RichTreeViewItems> ReadTargetContent(string folderPath)
        {
            List<RichTreeViewItems> result = new List<RichTreeViewItems> { };

            //存储相对路径下标
            int RelativePathStringIndex = folderPath.Length + 1;

            //初始化配置文件
            if (File.Exists(TargetFolderNameListFilePath) && TargetFolderNameList.Count == 0)
                TargetFolderNameList = File.ReadAllLines(TargetFolderNameListFilePath).ToList();

            if (File.Exists(ReadableFileExtensionListFilePath) && ReadableFileExtensionList.Count == 0)
                ReadableFileExtensionList = File.ReadAllLines(ReadableFileExtensionListFilePath).ToList();

            //如果路径不为空且目标命名空间和读取配置列表均不为空则继续执行
            if (folderPath != null && TargetFolderNameList.Count > 0 && ReadableFileExtensionList.Count > 0)
            {
                if (Directory.Exists(folderPath))
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
                            List<string> matchDirectory = TargetFolderNameList.Where(target_item => Directory.Exists(item + "\\" + target_item)).ToList();

                            //没有任何子级内容的命名空间
                            List<string> nullDirectory = new List<string> { };

                            nullDirectory = TargetFolderNameList.Except(matchDirectory).ToList();

                            //已确认当前为该包主目录
                            if (matchDirectory.Count > 0)
                            {
                                //遍历该包
                                foreach (string matchNameSpace in matchDirectory)
                                {
                                    string currentNameSpace = item + "\\" + matchNameSpace;
                                    //获取所有子级文件夹和文件
                                    string[] ScannedDataPackEntries = Directory.GetDirectories(currentNameSpace,"*",SearchOption.AllDirectories);
                                    for (int i = 0; i < ScannedDataPackEntries.Length; i++)
                                    {
                                        string[] currentDirectoryFiles = Directory.GetFiles(ScannedDataPackEntries[i]);
                                        for (int j = 0; j < currentDirectoryFiles.Length; j++)
                                        {
                                            //扫描到
                                            if (ReadableFileExtensionList.Contains(Path.GetFileNameWithoutExtension(currentDirectoryFiles[j])))
                                            {

                                            }
                                        }
                                    }
                                }

                                //创建空命名空间
                                foreach (string nullNameSpace in nullDirectory)
                                {

                                }
                                break;
                            }
                        }
                    }
                }
            }

            //#region 读取函数标签文件夹，读取初始化和主函数
            //#endregion

            //#region 读取函数文件
            //#endregion

            //#region 读取进度文件
            //#endregion

            //#region 读取战利品表文件
            //#endregion

            //#region 读取配方文件
            //#endregion

            //#region 读取标签文件
            //#endregion

            //#region 读取维度文件
            //#endregion

            //#region 读取生物群系文件
            //#endregion

            return result;
        }
    }
}
