using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace cbhk_environment.CustomControls
{
    public class RichRun:Run
    {
        /// <summary>
        /// 迭代内容序列
        /// </summary>
        List<char> Obfuscates = new List<char> { };
        //开启混淆
        public bool IsObfuscated = false;
        /// <summary>
        /// 迭代结果
        /// </summary>
        StringBuilder ObfuscatesResult = new StringBuilder() { };
        /// <summary>
        /// 迭代序列长度
        /// </summary>
        int ObfuscatedLength = 0;
        /// <summary>
        /// 迭代器
        /// </summary>
        Random random = new Random();

        #region UID
        private string uid = "";
        public string UID
        {
            get { return uid; }
            set
            {
                uid = value;
            }
        }
        #endregion

        string ObfuscatedContent;

        /// <summary>
        /// 当前混淆最长长度
        /// </summary>
        double MaxContentLength = 0;

        public FontStyle fontStyles = FontStyles.Normal;
        public FontWeight fontWeights = FontWeights.Normal;

        public System.Windows.Forms.Timer ObfuscateTimer = new System.Windows.Forms.Timer()
        {
            Interval = 10,
            Enabled = false
        };

        /// <summary>
        /// 混淆文字控件
        /// </summary>
        /// <param name="current_run">当前光标所在的对象</param>
        /// <param name="chars">迭代序列</param>
        /// <param name="Length">迭代长度</param>
        /// <param name="foreground">迭代字体颜色</param>
        /// <param name="run">迭代文本对象,用于传输删除线和下划线</param>
        public RichRun(List<char> chars,string content)
        {
            Obfuscates = chars;
            UID = content;
            Text = content;
            ObfuscateTimer.Tick += ObfuscateTick;
            MouseEnter += ObfuscateTextMouseEnter;
            MouseLeave += ObfuscateTextMouseLeave;
            MouseLeftButtonDown += ObfuscateTextMouseLeftButtonDown;
            MouseLeftButtonUp += ObfuscateTextMouseLeftButtonUp;
        }

        /// <summary>
        /// 混淆文字控件
        /// </summary>
        public RichRun()
        {
        }

        /// <summary>
        /// 鼠标抬起时启用混淆效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObfuscateTextMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if(IsObfuscated)
            ObfuscateTimer.Enabled = true;
        }

        /// <summary>
        /// 鼠标按下时关闭混淆效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObfuscateTextMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsObfuscated)
            {
                ObfuscateTimer.Enabled = false;
                Text = UID;
            }
        }

        /// <summary>
        /// 鼠标移出时启用混淆效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObfuscateTextMouseLeave(object sender, MouseEventArgs e)
        {
            if (IsObfuscated)
                ObfuscateTimer.Enabled = true;
        }

        /// <summary>
        /// 鼠标移入时关闭混淆效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ObfuscateTextMouseEnter(object sender, MouseEventArgs e)
        {
            if (IsObfuscated)
            {
                ObfuscateTimer.Enabled = false;
                Text = UID;
            }
        }

        /// <summary>
        /// 混淆效果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ObfuscateTick(object sender, EventArgs e)
        {
            MaxContentLength = GeneralTools.GetTextWidth.Get(new Run(UID));
            ObfuscatesResult.Clear();
            for (int i = 0; i < UID.Length; i++)
                ObfuscatesResult.Append(Obfuscates[random.Next(0, Obfuscates.Count - 1)]);
            while (GeneralTools.GetTextWidth.Get(new Run(ObfuscatesResult.ToString())) > MaxContentLength)
            {
                ObfuscatesResult.Remove(ObfuscatesResult.Length-1,1);
            }
            Text = ObfuscatesResult.ToString();
        }
    }
}
