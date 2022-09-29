using cbhk_environment.ControlsDataContexts;
using cbhk_environment.CustomControls;
using cbhk_environment.CustomControls.ColorPickers;
using cbhk_environment.Generators.WrittenBookGenerator.Components;
using cbhk_environment.WindowDictionaries;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.Services.Maps;

namespace cbhk_environment.Generators.WrittenBookGenerator
{
    public class written_book_datacontext: ObservableObject
    {
        #region 生成与返回
        public RelayCommand RunCommand { get; set; }

        public RelayCommand<CommonWindow> ReturnCommand { get; set; }
        #endregion

        #region 覆盖生成
        private bool overLying;
        public bool OverLying
        {
            get { return overLying; }
            set
            {
                overLying = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //表示正在更改文本对象类型
        bool TextTypeChanging = false;

        //成书编辑框引用
        RichTextBox written_box = null;

        //成书背景文件路径
        string backgroundFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\written_book_background.png";
        //左箭头背景文件路径
        string leftArrowFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\left_arrow.png";
        //右箭头背景文件路径
        string rightArrowFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\right_arrow.png";
        //左箭头高亮背景文件路径
        string leftArrowHightLightFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\enter_left_arrow.png";
        //右箭头高亮背景文件路径
        string rightArrowHightLightFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\enter_right_arrow.png";
        //点击事件数据源
        public static ObservableCollection<TextSource> clickEventSource = new ObservableCollection<TextSource> { };
        //悬浮事件数据源
        public static ObservableCollection<TextSource> hoverEventSource = new ObservableCollection<TextSource> { };
        //点击事件数据源文件路径
        string clickEventSourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\clickEventActions.ini";
        //悬浮事件数据源文件路径
        string hoverEventSourceFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\hoverEventActions.ini";
        //事件数据库
        public static Dictionary<string, string> EventDataBase = new Dictionary<string, string> { };
        //混淆文本配置文件路径
        string obfuscateFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\obfuscateChars.ini";
        //混淆文本迭代链表
        public static List<char> obfuscates = new List<char> { };

        ///// <summary>
        ///// 迭代序列起始下标
        ///// </summary>
        //public int ObfuscateStart = -1;
        ///// <summary>
        ///// 迭代序列末尾下标
        ///// </summary>
        //public int ObfuscateEnd = -1;

        //流文档链表,每个成员代表成书中的一页

        List<FlowDocument> WrittenBookPages = new List<FlowDocument> { };

        //一页总字符数
        int PageMaxCharCount = 377;

        // 总页数
        int MaxPage = 1;

        // 当前页码
        int CurrentPage = 1;

        //一个段落包含最多的字符数
        //int MaxLineCharCount = 450;

        //事件设置窗体
        public static Window EventForm = new Window()
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F2F2F")),
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        #region 设置选定文本样式指令
        /// <summary>
        /// 粗体
        /// </summary>
        public RelayCommand BoldTextCommand { get; set; }
        /// <summary>
        /// 斜体
        /// </summary>
        public RelayCommand ItalicTextCommand { get; set; }
        /// <summary>
        /// 下划线
        /// </summary>
        public RelayCommand UnderlinedTextCommand { get; set; }
        /// <summary>
        /// 删除线
        /// </summary>
        public RelayCommand StrikethroughTextCommand { get; set; }
        /// <summary>
        /// 混淆文本
        /// </summary>
        public RelayCommand ObfuscateTextCommand { get; set; }
        /// <summary>
        /// 重置文本
        /// </summary>
        public RelayCommand ResetTextCommand { get; set; }
        #endregion

        #region 开启点击事件
        private bool enableClickEvent = false;
        public bool EnableClickEvent
        {
            get { return enableClickEvent; }
            set
            {
                enableClickEvent = value;
            }
        }
        #endregion

        #region 开启悬浮事件
        private bool enableHoverEvent = false;
        public bool EnableHoverEvent
        {
            get { return enableHoverEvent; }
            set
            {
                enableHoverEvent = value;
            }
        }
        #endregion

        #region 开启插入
        private bool enableInsertion = false;
        public bool EnableInsertion
        {
            get { return enableInsertion; }
            set
            {
                enableInsertion = value;
            }
        }
        #endregion

        #region 拾色器
        ColorPickers LeftColorPicker = null;
        ColorPickers RightColorPicker = null;
        #endregion

        #region 被选择文本的字体颜色
        private SolidColorBrush selectionColor = new SolidColorBrush(Color.FromRgb(0,0,0));
        public SolidColorBrush SelectionColor
        {
            get { return selectionColor; }
            set
            {
                selectionColor = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 当前页码与总页数数据
        public string PageData
        {
            get { return "页面 ："+ CurrentPage.ToString() + "/" + MaxPage.ToString(); }
        }
        #endregion

        #region 是否显示左箭头
        private Visibility displayLeftArrow = Visibility.Collapsed;
        public Visibility DisplayLeftArrow
        {
            get { return displayLeftArrow; }
            set
            {
                displayLeftArrow = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public written_book_datacontext()
        {
            #region 链接指令
            RunCommand = new RelayCommand(run_command);
            ReturnCommand = new RelayCommand<CommonWindow>(return_command);
            BoldTextCommand = new RelayCommand(boldTextCommand);
            ItalicTextCommand = new RelayCommand(italicTextCommand);
            UnderlinedTextCommand = new RelayCommand(underlinedTextCommand);
            StrikethroughTextCommand = new RelayCommand(strikethroughTextCommand);
            ObfuscateTextCommand = new RelayCommand(obfuscateTextCommand);
            ResetTextCommand = new RelayCommand(resetTextCommand);
            #endregion

            #region 读取点击事件
            if (File.Exists(clickEventSourceFilePath))
            {
                string[] source = File.ReadAllLines(clickEventSourceFilePath);
                for (int i = 0; i < source.Length; i++)
                {
                    string[] data = source[i].Split('.');
                    clickEventSource.Add(new TextSource()
                    {
                        ItemText = data[1]
                    });
                    if (!EventDataBase.ContainsKey(data[0]))
                        EventDataBase.Add(data[0], data[1]);
                }
            }
            #endregion

            #region 读取悬浮事件
            if (File.Exists(hoverEventSourceFilePath))
            {
                string[] source = File.ReadAllLines(hoverEventSourceFilePath);
                for (int i = 0; i < source.Length; i++)
                {
                    string[] data = source[i].Split('.');
                    hoverEventSource.Add(new TextSource()
                    {
                        ItemText = data[1]
                    });
                    if (!EventDataBase.ContainsKey(data[0]))
                        EventDataBase.Add(data[0], data[1]);
                }
            }
            #endregion

            #region 读取混淆文本配置
            if(File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\obfuscateChars.ini"))
            {
                obfuscates = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\obfuscateChars.ini").ToCharArray().ToList();
            }
            #endregion
        }

        /// <summary>
        /// 重置选中文本的所有属性
        /// </summary>
        private void resetTextCommand()
        {

        }

        /// <summary>
        /// 选中文本切换混淆文字
        /// </summary>
        private void obfuscateTextCommand()
        {
            if (written_box.Selection == null) return;
            if (written_box.Selection.Text.Length == 0) return;

            //获取当前段落所在的文档对象
            EnabledFlowDocument enabledFlowDocument = written_box.Document as EnabledFlowDocument;
            //找到光标头所在Run对象
            RichRun start_run = written_box.Selection.Start.Parent as RichRun;
            //找到光标尾所在的Run对象
            RichRun end_run = written_box.Selection.End.Parent as RichRun;
            //找到光标头所在段落
            Paragraph start_paragraph = start_run.Parent as Paragraph;
            //找到光标尾所在段落
            Paragraph end_paragraph = end_run.Parent as Paragraph;
            //当前所操作的段落包含的文本对象
            List<Inline> current_inlines = start_paragraph.Inlines.ToList();
            //保存光标头所在Run对象起始部分的文本
            TextRange startRunStartPart;
            //保存光标头所在Run对象末尾部分的文本
            TextRange startRunEndPart;
            //保存光标尾所在Run对象起始部分的文本
            TextRange endRunStartPart;
            //保存光标尾所在Run对象末尾部分的文本
            TextRange endRunEndPart;

            //所选文本起始对象所在段落的下标
            int SelectionStartIndex = current_inlines.IndexOf(start_run);
            //所选文本末尾对象所在段落的下标
            int SelectionEndIndex = current_inlines.IndexOf(end_run);

            //当作插入时的后置对象
            RichRun InsertNextRun = SelectionEndIndex < (current_inlines.Count - 1) ? current_inlines.ElementAt(SelectionEndIndex + 1) as RichRun : current_inlines.ElementAt(current_inlines.Count - 1) as RichRun;
            //当作插入时得前置对象
            RichRun InsertPreviewRun = SelectionStartIndex > 0 ? current_inlines.ElementAt(SelectionStartIndex - 1) as RichRun : current_inlines.ElementAt(0) as RichRun;

            //需要全部混淆的RichRun对象列表
            List<Inline> NeedObfuscatedRichRuns = new List<Inline> { };
            //处理同一行的混淆
            if (SelectionEndIndex - SelectionStartIndex > 1 && start_paragraph == end_paragraph)
                NeedObfuscatedRichRuns = current_inlines.GetRange((SelectionStartIndex + 1) < (current_inlines.Count - 1) ? SelectionStartIndex + 1 : SelectionStartIndex, (SelectionEndIndex - 1) > 0 ? SelectionEndIndex - 1 : SelectionEndIndex);
            else
            if (start_paragraph != end_paragraph)
            {

            }

            #region 分割光标首尾所选部分的文本对象

            #region 首
            startRunStartPart = new TextRange(start_run.ElementStart, written_box.Selection.Start);
            startRunEndPart = new TextRange(start_run.ElementEnd, written_box.Selection.Start);

            RichRun newStartRunStartPart = new RichRun()
            {
                Foreground = start_run.Foreground,
                FontStyle = start_run.FontStyle,
                FontWeight = start_run.FontWeight,
                TextDecorations = start_run.TextDecorations,
                Tag = start_run.Tag
            };
            newStartRunStartPart.Text = startRunStartPart.Text;

            if (newStartRunStartPart.Text.Trim() != "" /*&& !SelectOnlyOne*/)
            //    start_paragraph.Inlines.InsertBefore(InsertPreviewRun, newStartRunStartPart);
            //else
                start_paragraph.Inlines.InsertBefore(start_run,newStartRunStartPart);

            if (start_run != end_run)
            {
                start_run.Text = startRunEndPart.Text;
                start_run.UID = startRunEndPart.Text;
                start_run.IsObfuscated = true;
                start_run.ObfuscateTimer.Enabled = true;
            }
            #endregion

            #region 尾
            endRunStartPart = new TextRange(end_run.ElementStart, written_box.Selection.End);
            endRunEndPart = new TextRange(end_run.ElementEnd, written_box.Selection.End);

            RichRun newEndRunEndPart = new RichRun()
            {
                FontStyle = end_run.FontStyle,
                FontWeight = end_run.FontWeight,
                TextDecorations = end_run.TextDecorations,
                Tag = end_run.Tag
            };
            newEndRunEndPart.Text = endRunEndPart.Text;

            if (newEndRunEndPart.Text.Trim() != "" /*&& !SelectOnlyOne*/)
            //    start_paragraph.Inlines.InsertAfter(InsertNextRun, newEndRunEndPart);
            //else
                start_paragraph.Inlines.InsertAfter(end_run,newEndRunEndPart);

            if (end_run != start_run)
            {
                end_run.Text = endRunStartPart.Text;
                end_run.UID = endRunStartPart.Text;
                end_run.IsObfuscated = true;
                end_run.ObfuscateTimer.Enabled = true;
            }
            #endregion

            #endregion
            if (end_run == start_run)//如果光标首尾在同一个文本对象内则直接赋值
            {
                start_run.Text = written_box.Selection.Text;
                start_run.UID = written_box.Selection.Text;
                start_run.IsObfuscated = true;
                start_run.ObfuscateTimer.Enabled = true;
            }

            for (int i = 0; i < NeedObfuscatedRichRuns.Count; i++)
            {
                RichRun richRun = NeedObfuscatedRichRuns[i] as RichRun;
                richRun.UID = richRun.Text;
                richRun.IsObfuscated = true;
                richRun.ObfuscateTimer.Enabled = true;
            }
        }

        /// <summary>
        /// 选中文本切换删除线
        /// </summary>
        private void strikethroughTextCommand()
        {
            if (written_box.Selection == null)
                return;
            if (written_box.Selection.Text.Length == 0)
                return;
            TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            TextDecorationCollection current_decorations = textRange.GetPropertyValue(TextBlock.TextDecorationsProperty) as TextDecorationCollection;

            if(current_decorations != null)
            {
                int underline_index = current_decorations.IndexOf(TextDecorations.Baseline.First());
                if (!current_decorations.Contains(TextDecorations.Strikethrough.First()))
                {
                    TextDecorationCollection textDecorations = new TextDecorationCollection();
                    if (underline_index != -1)
                        textDecorations.Add(TextDecorations.Baseline);
                    textDecorations.Add(TextDecorations.Strikethrough);
                    textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, textDecorations);
                }
                else
                {
                    if (underline_index != -1)
                        textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, TextDecorations.Baseline);
                    else
                        textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, new TextDecorationCollection());
                }
            }
            else
                textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, TextDecorations.Strikethrough);
        }

        /// <summary>
        /// 选中文本切换下划线
        /// </summary>
        private void underlinedTextCommand()
        {
            if (written_box.Selection == null)
                return;
            if (written_box.Selection.Text.Length == 0)
                return;
            TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            TextDecorationCollection current_decorations = textRange.GetPropertyValue(TextBlock.TextDecorationsProperty) as TextDecorationCollection;

            if (current_decorations != null)
            {
                int strikethrough_index = current_decorations.IndexOf(TextDecorations.Strikethrough.First());
                if (!current_decorations.Contains(TextDecorations.Baseline.First()))
                {
                    TextDecorationCollection textDecorations = new TextDecorationCollection();
                    if (strikethrough_index != -1)
                        textDecorations.Add(TextDecorations.Strikethrough);
                    textDecorations.Add(TextDecorations.Baseline);
                    textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, textDecorations);
                }
                else
                {
                    if (strikethrough_index != -1)
                        textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, TextDecorations.Strikethrough);
                    else
                        textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, new TextDecorationCollection());
                }
            }
            else
                textRange.ApplyPropertyValue(TextBlock.TextDecorationsProperty, TextDecorations.Baseline);
        }

        /// <summary>
        /// 选中文本切换斜体
        /// </summary>
        private void italicTextCommand()
        {
            if (written_box.Selection == null)
                return;
            if (written_box.Selection.Text.Length == 0)
                return;
            TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            if (Equals(textRange.GetPropertyValue(TextBlock.FontStyleProperty), FontStyles.Normal))
                textRange.ApplyPropertyValue(TextBlock.FontStyleProperty, FontStyles.Italic);
            else
                textRange.ApplyPropertyValue(TextBlock.FontStyleProperty, FontStyles.Normal);
        }

        /// <summary>
        /// 选中文本切换粗体
        /// </summary>
        private void boldTextCommand()
        {
            if (written_box.Selection == null)
                return;
            if (written_box.Selection.Text.Length == 0)
                return;
            TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            if(Equals(textRange.GetPropertyValue(TextBlock.FontWeightProperty),FontWeights.Normal))
            textRange.ApplyPropertyValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            else
                textRange.ApplyPropertyValue(TextBlock.FontWeightProperty, FontWeights.Normal);
        }

        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="win"></param>
        private void return_command(CommonWindow win)
        {
            WrittenBook.cbhk.Topmost = true;
            WrittenBook.cbhk.WindowState = WindowState.Normal;
            WrittenBook.cbhk.Show();
            WrittenBook.cbhk.Topmost = false;
            WrittenBook.cbhk.ShowInTaskbar = true;
            win.Close();
        }

        /// <summary>
        /// 执行生成
        /// </summary>
        private void run_command()
        {
            #region 添加一段文本
            FlowDocument flowDocument = written_box.Document;
            List<Block> blocks = flowDocument.Blocks.ToList();
            //Paragraph last_paragraph = blocks[blocks.Count - 1] as Paragraph;
            //last_paragraph.Inlines.Add(new Run("Test") { Foreground = new SolidColorBrush(Color.FromRgb(255,0,0)) });
            #endregion

            #region 搜索光标所在文本
            ////被搜索文本长度
            //int selection_length = written_box.Selection.Text.Length;
            ////被搜索文本起始位置
            //TextPointer selection_start = written_box.Selection.Start;
            ////被搜索文本所在段落起始位置
            //TextPointer line_start = written_box.CaretPosition.GetLineStartPosition(0);
            #endregion

            //written_box.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Color.FromRgb(255,0,0)));r
            //TextPointer textPointer = written_box.CaretPosition.GetPositionAtOffset(1);
            //TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            //textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Color.FromRgb(255, 0, 0)));
            ////获取当前光标所在的父级Run对象
            //MessageBox.Show(written_box.Selection.Start.Parent.GetType().ToString());

            //Paragraph paragraph = blocks[0] as Paragraph;
            foreach (Paragraph item in blocks)
            {
                MessageBox.Show(item.Inlines.Count+"");
            }
            //MessageBox.Show(paragraph.Inlines.Count+"");
        }

        /// <summary>
        /// 弹出事件设置窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenEventForm(object sender, MouseButtonEventArgs e)
        {
            TextEventsForm EventComponent = new TextEventsForm();
            //设置事件设置窗体内容
            EventForm.Content = EventComponent;
            EventForm.Show();
        }

        /// <summary>
        /// 检测按回车键换行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WrittenBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            //处理粘贴的数据,合并为RichRun以适配混淆效果
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                RichRun richRun = written_box.CaretPosition.Parent as RichRun;
                TextPointer start = richRun.ElementStart;
                TextPointer select = written_box.CaretPosition;
                int index = start.GetOffsetToPosition(select);
                richRun.Text = richRun.Text.Insert(index - 1, Clipboard.GetText());
                e.Handled = true;
            }

            TextRange textRange = new TextRange(written_box.Document.ContentStart, written_box.Document.ContentEnd);

            if (textRange.Text.Length > PageMaxCharCount)
            {
                textRange.Text = textRange.Text.Substring(0, PageMaxCharCount);
            }
        }

        /// <summary>
        /// 判断当前编辑的对象类型是否为Run
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WrittenBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            object current_obj = written_box.Selection.Start.Parent;
            if (current_obj != null && (current_obj is Run))
            {
                Run run = current_obj as Run;
                run = new RichRun();
            }
        }

        /// <summary>
        /// 设置被选择文本的字体颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SetSelectionColor(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ColorPickers colorPickers = sender as ColorPickers;
            if (colorPickers.Uid == "Left")
                RightColorPicker.Background = colorPickers.SelectColor;
            if (colorPickers.Uid == "Right")
                LeftColorPicker.Background = colorPickers.SelectColor;
            TextRange textRange = new TextRange(written_box.Selection.Start, written_box.Selection.End);
            textRange.ApplyPropertyValue(TextBlock.ForegroundProperty, colorPickers.SelectColor);
        }

        /// <summary>
        /// 获取成书编辑框的引用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WrittenBoxLoaded(object sender, RoutedEventArgs e)
        {
            written_box = sender as RichTextBox;
        }

        /// <summary>
        /// 载入拾色器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ColorPickerLoaded(object sender, RoutedEventArgs e)
        {
            ColorPickers colorPickers = sender as ColorPickers;
            if (LeftColorPicker == null && colorPickers.Uid == "Left")
                LeftColorPicker = colorPickers;
            if (RightColorPicker == null && colorPickers.Uid == "Right")
                RightColorPicker = colorPickers;
        }

        /// <summary>
        /// 载入成书背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BackgroundLoaded(object sender, RoutedEventArgs e)
        {
            if(File.Exists(backgroundFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(backgroundFilePath,UriKind.Absolute));
            }
        }

        /// <summary>
        /// 载入左箭头背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeftArrowLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(leftArrowFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(leftArrowFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 载入右箭头背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RightArrowLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(rightArrowFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(rightArrowFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 鼠标移到左箭头处显示高亮图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeftArrowMouseEnter(object sender, MouseEventArgs e)
        {
            if (File.Exists(leftArrowHightLightFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(leftArrowHightLightFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 鼠标移到右箭头处显示高亮图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RightArrowMouseEnter(object sender, MouseEventArgs e)
        {
            if (File.Exists(rightArrowHightLightFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(rightArrowHightLightFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 鼠标移出左箭头处显示普通图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeftArrowMouseLeave(object sender, MouseEventArgs e)
        {
            if (File.Exists(leftArrowFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(leftArrowFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 鼠标移出右箭头处显示普通图像
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RightArrowMouseLeave(object sender, MouseEventArgs e)
        {
            if (File.Exists(rightArrowFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(rightArrowFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 向左翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeftArrowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// 向右翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RightArrowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// 检测光标所在的段落与内容载体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WrittenBoxMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }

    public class TextEventProperties:ObservableObject
    {
        private bool hasClickEvent = false;

        public bool HasClickEvent
        {
            get { return hasClickEvent; }
            set
            {
                hasClickEvent = value;
            }
        }

        private bool hasHoverEvent = false;

        public bool HasHoverEvent
        {
            get { return hasHoverEvent; }
            set
            {
                hasHoverEvent = value;
            }
        }

        private bool hasInsertion = false;

        public bool HasInsertion
        {
            get { return hasInsertion; }
            set
            {
                hasInsertion = value;
            }
        }

        private string clickEvent = "";
        public string ClickEvent { get { return clickEvent; } set { clickEvent = value;OnPropertyChanged(); } }

        private string hoverEvent = "";
        public string HoverEvent { get { return hoverEvent; } set { hoverEvent = value; OnPropertyChanged(); } }

        private string insertion = "";
        public string Insertion { get { return insertion; } set { insertion = value; OnPropertyChanged(); } }
    }
}
