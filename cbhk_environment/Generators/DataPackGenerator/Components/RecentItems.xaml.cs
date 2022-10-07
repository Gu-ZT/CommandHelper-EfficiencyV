﻿using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace cbhk_environment.Generators.DataPackGenerator.Components
{
    /// <summary>
    /// RecentItems.xaml 的交互逻辑
    /// </summary>
    public partial class RecentItems : UserControl
    {
        //日期前景色
        SolidColorBrush DateTimeForeground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

        DateTime CurrentTime = new DateTime();

        public RecentItems(string fileIconPath,string filePath)
        {
            InitializeComponent();

            #region 显示图像
            FileIcon.Source = new BitmapImage(new Uri(fileIconPath,UriKind.Absolute));
            #endregion

            #region 获取文件路径，文件名，文件最后修改日期
            if(File.Exists(filePath))
            {
                #region 获取该文件最后一次编辑时间戳
                FileInfo fileInfo = new FileInfo(filePath);
                DateTime dateTime = fileInfo.LastWriteTime;
                CurrentTime = dateTime;
                string fileModifyTime = dateTime.ToString("g");
                FileModifyDateTime.Text = fileModifyTime;
                FileModifyDateTime.Foreground = DateTimeForeground;
                #endregion

                //设置该文件路径
                FilePath.Tag = filePath;
                if (filePath.Length > 50)
                FilePath.Text = filePath.Substring(0,47)+"...";
                else
                    FilePath.Text = filePath;

                //设置该文件名
                FileName.Text = Path.GetFileNameWithoutExtension(filePath);
                FileName.FontWeight = FontWeights.Bold;

                Cursor = Cursors.Hand;
                ToolTip = "打开本地数据包\r\n" + filePath;
                ToolTipService.SetInitialShowDelay(this, 0);
                ToolTipService.SetShowDuration(this, 1000);
            }
            #endregion
        }

        /// <summary>
        /// 计算时间间隔
        /// </summary>
        /// <returns></returns>
        public string CalculationDateInterval()
        {
            int year_data = DateTime.Now.Year;
            int month_data = DateTime.Now.Month;
            int day_data = DateTime.Now.Day;

            int day_interval = day_data - CurrentTime.Day;
            int month_interval = CurrentTime.Month - month_data;
            int year_interval = CurrentTime.Year - year_data;

            //去年
            if (year_interval == 1)
                return "LastYear";
            //上月
            if (month_interval == 1 && year_interval == 0)
                return "LastMonth";
            //昨天
            if (day_interval == 1 && month_interval == 0 && year_interval == 0)
                return "Yesterday";
            //今年
            if (year_interval == 0 && month_interval > 1)
                return "ThisYear";
            //本月
            if (month_interval == 0 && day_interval > 7)
                return "ThisMonth";
            //本周
            if (day_interval > 1 && day_interval < 7)
                return "ThisWeek";
            //上周
            if (day_interval > 7 && month_interval == 0)
                return "LastWeek";
            //今天
            if (day_interval == 0 && month_interval == 0 && year_interval == 0)
                return "ToDay";

            return "";
        }
    }
}
