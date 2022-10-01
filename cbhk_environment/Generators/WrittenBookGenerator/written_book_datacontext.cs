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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.System.Profile;

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

        //成书编辑框引用
        RichTextBox written_box = null;

        //成书背景文件路径
        string backgroundFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\written_book_background.png";
        //成书背景控件引用
        Image written_book_background = null;
        //左箭头背景文件路径
        string leftArrowFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\left_arrow.png";
        //右箭头背景文件路径
        string rightArrowFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\right_arrow.png";
        //左箭头高亮背景文件路径
        string leftArrowHightLightFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\enter_left_arrow.png";
        //右箭头高亮背景文件路径
        string rightArrowHightLightFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\enter_right_arrow.png";
        //署名按钮背景文件路径
        string signatureFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\signature_button.png";
        //署名背景文件路径
        string signatureBackgroundFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\signature_page.png";
        //署名并关闭背景文件路径
        string signatureAndCloseFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\setting_signature.png";
        //署名完毕背景文件路径
        string sureToSignatureFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\sure_to_signature.png";
        //署名完毕按钮引用
        Image sureToSignatureButton = null;
        //取消署名背景文件路径
        string signatureCancelFilePath = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\cancel_signature.png";
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
        //混淆字体类型
        string obfuscatedFontFamily = "Bitstream Vera Sans Mono";
        //普通字体类型
        string commonFontFamily = "Minecraft AE Pixel";
        //混淆文本迭代链表
        public static List<char> obfuscates = new List<char> { };

        //流文档链表,每个成员代表成书中的一页

        List<EnabledFlowDocument> WrittenBookPages = new List<EnabledFlowDocument> { };

        /// <summary>
        /// 一页总字符数
        /// </summary>
        int PageMaxCharCount = 377;

        #region 总页数
        int maxPage = 1;
        int MaxPage
        {
            get { return maxPage; }
            set
            {
                maxPage = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 当前页码下标
        int currentPageIndex = 0;
        int CurrentPageIndex
        {
            get 
            { 
                MaxPage = WrittenBookPages.Count; 
                PageData = "页面 ：" + (currentPageIndex + 1).ToString() + "/" + MaxPage.ToString(); 
                return currentPageIndex;
            }
            set
            {
                currentPageIndex = value;
                PageData = "页面 ：" + (CurrentPageIndex + 1).ToString() + "/" + MaxPage.ToString();
                if (CurrentPageIndex > 0)
                    DisplayLeftArrow = Visibility.Visible;
                else
                    DisplayLeftArrow = Visibility.Collapsed;
            }
        }
        #endregion

        #region 当前光标所在的文本对象引用
        private RichRun currentRichRun = null;
        public RichRun CurrentRichRun
        {
            get { return currentRichRun; }
            set
            {
                currentRichRun = value;
                OnPropertyChanged();
            }
        }
        #endregion

        /// <summary>
        /// 事件设置窗体
        /// </summary>
        public static Window EventForm = new Window()
        {
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F2F2F")),
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        /// <summary>
        /// 事件设置控件
        /// </summary>
        TextEventsForm EventComponent = new TextEventsForm();

        //控制事件窗体的显示
        bool DisplayEventForm = false;

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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
                OnPropertyChanged();
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
        string pageData = "页面 ：1/1";
        public string PageData
        {
            get { return pageData; }
            set { pageData = value; OnPropertyChanged(); }
        }
        #endregion

        #region 保存作者
        //标记当前背景样式
        bool HaveAuthor = false;
        private string author = "";
        public string Author
        {
            get { return author; }
            set
            {
                author = value;
                OnPropertyChanged();
                if(author.Trim() != "" && File.Exists(sureToSignatureFilePath) && !HaveAuthor)
                {
                    sureToSignatureButton.Source = new BitmapImage(new Uri(sureToSignatureFilePath, UriKind.Absolute));
                    HaveAuthor = true;
                }
                else
                    if(author.Trim() == "" && File.Exists(signatureAndCloseFilePath) && HaveAuthor)
                {
                    sureToSignatureButton.Source = new BitmapImage(new Uri(signatureAndCloseFilePath, UriKind.Absolute));
                    HaveAuthor = false;
                }
            }
        }
        private string AuthorString
        {
            get
            {
                return "author:\"" + Author + "\",";
            }
        }
        #endregion

        #region 保存标题
        private string title = "";
        public string Title
        {
            get { return title; }
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        private string TitleString
        {
            get
            {
                return "title:\"" + Title + "\",";
            }
        }
        #endregion

        #region 控制作者显示
        private Visibility displayAuthor = Visibility.Collapsed;
        public Visibility DisplayAuthor
        {
            get { return displayAuthor; }
            set
            {
                displayAuthor = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制标题显示
        private Visibility displayTitle = Visibility.Collapsed;
        public Visibility DisplayTitle
        {
            get { return displayTitle; }
            set
            {
                displayTitle = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制页码显示
        private Visibility displayPageIndex = Visibility.Collapsed;
        public Visibility DisplayPageIndex
        {
            get { return displayPageIndex; }
            set
            {
                displayPageIndex = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制确定署名按钮显示
        private Visibility displaySignatureButton = Visibility.Collapsed;
        public Visibility DisplaySignatureButton
        {
            get { return displaySignatureButton; }
            set
            {
                displaySignatureButton = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制取消署名按钮显示
        private Visibility displayCancelSignatureButton = Visibility.Collapsed;
        public Visibility DisplayCancelSignatureButton
        {
            get { return displayCancelSignatureButton; }
            set
            {
                displayCancelSignatureButton = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制成书内容键入框显示
        private Visibility displayWrittenBox = Visibility.Visible;
        public Visibility DisplayWrittenBox
        {
            get { return displayWrittenBox; }
            set
            {
                displayWrittenBox = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制署名按钮显示
        private Visibility displaySignature = Visibility.Visible;
        public Visibility DisplaySignature
        {
            get { return displaySignature; }
            set
            {
                displaySignature = value;
                OnPropertyChanged();
            }
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

        #region 是否显示右箭头
        private Visibility displayRightArrow = Visibility.Visible;
        public Visibility DisplayRightArrow
        {
            get { return displayRightArrow; }
            set
            {
                displayRightArrow = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region 控制左右侧样式切换面板显示
        private Visibility displayStylePanel = Visibility.Visible;
        public Visibility DisplayStylePanel
        {
            get { return displayStylePanel; }
            set
            {
                displayStylePanel = value;
                OnPropertyChanged();
            }
        }
        #endregion

        //图标路径
        string icon_path = AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\images\\icon.png";

        //当前光标选中的文本对象链表
        List<RichRun> CurrentSelectedRichRunList = new List<RichRun> { };

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
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\obfuscateChars.ini"))
            {
                obfuscates = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "resources\\configs\\WrittenBook\\data\\obfuscateChars.ini").ToCharArray().ToList();
            }
            #endregion

            #region 初始化事件设置窗体
            EventForm.Content = EventComponent;
            EventForm.Closing += (o, e) => { e.Cancel = true; EventForm.Hide(); };
            EventForm.DataContext = this;
            EventComponent.DataContext = this;
            #endregion
        }

        /// <summary>
        /// 重置选中文本的所有属性
        /// </summary>
        private void resetTextCommand()
        {
            RichRun start_run = written_box.Selection.Start.Parent as RichRun;
            RichRun end_run = written_box.Selection.End.Parent as RichRun;
            if(start_run == end_run)
            {
                start_run.FontWeight = FontWeights.Normal;
                start_run.FontStyle = FontStyles.Normal;
                start_run.TextDecorations = new TextDecorationCollection();
                start_run.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                start_run.IsObfuscated = false;
                start_run.ObfuscateTimer.Enabled = false;
                if (start_run.UID.Trim() != "")
                    start_run.Text = start_run.UID;
            }
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
            //获取当前文档中所有的段落
            List<Paragraph> CurrentParagraphList = enabledFlowDocument.Blocks.ToList().ConvertAll(item=>item as Paragraph);
            //找到光标头所在Run对象
            RichRun start_run = written_box.Selection.Start.Parent as RichRun;
            //找到光标尾所在的Run对象
            RichRun end_run = written_box.Selection.End.Parent as RichRun;

            //找到光标头所在段落
            Paragraph start_paragraph = start_run.Parent as Paragraph;
            //找到光标尾所在段落
            Paragraph end_paragraph = end_run.Parent as Paragraph;
            //保存光标首尾包含的所有段落
            List<Paragraph> SelectedParagraphList = new List<Paragraph> { };
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

            #region 如果光标的首尾分别与文本对象的首尾对应,则关闭混淆效果,还原原文
            TextRange SelectedStart = new TextRange(start_run.ElementStart, written_box.Selection.Start);
            TextRange SelectedEnd = new TextRange(end_run.ElementEnd, written_box.Selection.End);
            //计算光标所包含的所有中间段落,不包括头和尾所在的段落
            int start_paragraph_index = CurrentParagraphList.IndexOf(start_paragraph);
            int end_paragraph_index = CurrentParagraphList.IndexOf(end_paragraph);
            if (SelectedStart.Text.Trim() == "" && SelectedEnd.Text.Trim() == "" && start_run.IsObfuscated && end_run.IsObfuscated)
            {
                List<Inline> ObfuscatedRuns = NeedObfuscatedRichRuns.Where(item =>
                {
                    RichRun CurrentRichRun = item as RichRun;
                    return CurrentRichRun.IsObfuscated;
                }).ToList();

                int differenceCount = NeedObfuscatedRichRuns.Count - ObfuscatedRuns.Count;
                if (differenceCount > (-2) || differenceCount < 2)
                {
                    #region 还原光标首尾处的文本对象
                    start_run.FontFamily = new FontFamily(commonFontFamily);
                    start_run.IsObfuscated = false;
                    start_run.ObfuscateTimer.Enabled = false;
                    start_run.Text = start_run.UID;

                    end_run.FontFamily = new FontFamily(commonFontFamily);
                    end_run.IsObfuscated = false;
                    end_run.ObfuscateTimer.Enabled = false;
                    end_run.Text = end_run.UID;
                    #endregion

                    //处理不同行的混淆还原
                    if (start_paragraph != end_paragraph)
                    {
                        if(end_paragraph_index - start_paragraph_index > 1)
                        {
                            SelectedParagraphList = CurrentParagraphList.GetRange(start_paragraph_index + 1, end_paragraph_index - 1 - start_paragraph_index);
                            int SelectedMiddleRichRunCount = 0;
                            foreach (Paragraph item in SelectedParagraphList)
                                SelectedMiddleRichRunCount += item.Inlines.Count;
                            int SelectedObfuscatedRichRunCount = SelectedParagraphList.Where(item =>
                            {
                                foreach (var run in item.Inlines)
                                {
                                    if (run is RichRun)
                                    {
                                        RichRun richRun = run as RichRun;
                                        if (richRun.IsObfuscated)
                                        return richRun.IsObfuscated;
                                    }
                                    else
                                        return false;
                                }
                                return false;
                            }).Count();
                            if(SelectedMiddleRichRunCount == SelectedObfuscatedRichRunCount)
                            {
                                SelectedParagraphList.All(item =>
                                {
                                    ObfuscatedRuns.AddRange(item.Inlines.ToList().ConvertAll(line=>line as RichRun));
                                    return true;
                                });
                            }
                        }
                    }
                    else//处理同一行的混淆还原
                    {
                        ObfuscatedRuns = NeedObfuscatedRichRuns;
                    }

                    foreach (var item in ObfuscatedRuns)
                    {
                        if (item is RichRun)
                        {
                            RichRun richRun = item as RichRun;
                            richRun.FontFamily = new FontFamily(commonFontFamily);
                            richRun.IsObfuscated = false;
                            richRun.ObfuscateTimer.Enabled = false;
                            richRun.Text = richRun.UID;
                        }
                    }
                    return;
                }
            }
            #endregion

            if (start_paragraph != end_paragraph)//如果选择了多行,那么计算分别起始段落右侧和末尾段落左侧的所有文本对象
            {
                //判断光标首末所在段落是否有剩余文本对象
                if (SelectionStartIndex <= (current_inlines.Count - 1))
                {
                    //保存光标起始段落文本右侧所有文本对象
                    List<Inline> right_side_runs = current_inlines.GetRange(SelectionStartIndex, current_inlines.Count - SelectionStartIndex);
                    List<Inline> end_run_inlines = end_paragraph.Inlines.ToList();
                    SelectionEndIndex = end_run_inlines.IndexOf(end_run);
                    //保存光标末尾段落文本左侧所有文本对象
                    List<Inline> left_side_runs = end_run_inlines.GetRange(0, SelectionEndIndex);

                    NeedObfuscatedRichRuns.AddRange(right_side_runs);
                    NeedObfuscatedRichRuns.AddRange(left_side_runs);
                }

                //判断光标首末是否包含其他段落
                if (end_paragraph_index - start_paragraph_index > 1)
                {
                    SelectedParagraphList = CurrentParagraphList.GetRange(start_paragraph_index + 1, end_paragraph_index - 1 - start_paragraph_index);
                    SelectedParagraphList.All(item => { NeedObfuscatedRichRuns.AddRange(item.Inlines.ToList().ConvertAll(line => line as RichRun)); return true; });
                }
            }

            #region 分割光标首尾所选部分的文本对象

            #region 首
            if(!start_run.IsObfuscated)
            {
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

                if (newStartRunStartPart.Text.Trim() != "")
                    start_paragraph.Inlines.InsertBefore(start_run, newStartRunStartPart);

                if (start_run != end_run)
                {
                    start_run.FontFamily = new FontFamily(obfuscatedFontFamily);
                    start_run.Text = startRunEndPart.Text;
                    start_run.UID = startRunEndPart.Text;
                    start_run.IsObfuscated = true;
                    start_run.ObfuscateTimer.Enabled = true;
                }
            }
            #endregion

            #region 尾
            if(!end_run.IsObfuscated)
            {
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

                if (newEndRunEndPart.Text.Trim() != "" && start_paragraph == end_paragraph)
                    start_paragraph.Inlines.InsertAfter(end_run, newEndRunEndPart);
                else
                if (newEndRunEndPart.Text.Trim() != "")
                    end_paragraph.Inlines.InsertAfter(end_run, newEndRunEndPart);

                if (end_run != start_run)
                {
                    end_run.Text = endRunStartPart.Text;
                    end_run.UID = endRunStartPart.Text;
                    end_run.FontFamily = new FontFamily(obfuscatedFontFamily);
                    end_run.IsObfuscated = true;
                    end_run.ObfuscateTimer.Enabled = true;
                }
            }
            #endregion

            #endregion
            if (end_run == start_run)//如果光标首尾在同一个文本对象内则直接赋值
            {
                start_run.Text = written_box.Selection.Text;
                start_run.UID = written_box.Selection.Text;
                start_run.FontFamily = new FontFamily(obfuscatedFontFamily);
                start_run.IsObfuscated = true;
                start_run.ObfuscateTimer.Enabled = true;
            }

            for (int i = 0; i < NeedObfuscatedRichRuns.Count; i++)
            {
                RichRun richRun = NeedObfuscatedRichRuns[i] as RichRun;
                richRun.UID = richRun.Text;
                richRun.FontFamily = new FontFamily(obfuscatedFontFamily);
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
            //最终结果
            string result = "/give @p written_book";
            //合并所有页数据
            string pages_string = "pages:[";
            //每一页的数据
            string page_string = "";
            //遍历所有文档
            foreach (EnabledFlowDocument page in WrittenBookPages)
            {
                List<Paragraph> page_content = page.Blocks.ToList().ConvertAll(item=>item as Paragraph);
                page_string += "'[\"\",";
                for (int i = 0; i < page_content.Count; i++)
                {
                    page_string += string.Join("", page_content[i].Inlines.ToList().ConvertAll(line => line as RichRun).Select(run =>
                    {
                        return run.Result;
                    }));
                }
                page_string = page_string.TrimEnd(',') + "]',";
            }
            pages_string += page_string.TrimEnd(',') + "]";
            pages_string = pages_string.Trim() == "pages:['[\"\"]']" || pages_string.Trim() == "pages:['[\"\",{\"text\":\"\"}]']" ? "":pages_string;
            string NBTData = "";
            NBTData += TitleString + AuthorString + pages_string;
            NBTData = "{" + NBTData.TrimEnd(',') + "}";
            result += NBTData;

            GenerateResultDisplayer.Displayer displayer = GenerateResultDisplayer.Displayer.GetContentDisplayer();
            displayer.GeneratorResult(OverLying, new string[] { result }, new string[] { "" }, new string[] { icon_path }, new System.Windows.Media.Media3D.Vector3D() { X = 30, Y = 30 });
            displayer.Show();
        }

        /// <summary>
        /// 弹出事件设置窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OpenEventForm(object sender, MouseButtonEventArgs e)
        {
            //CurrentSelectedRichRunList
            //EventComponent
            DisplayEventForm = !DisplayEventForm;
            if (!DisplayEventForm) return;
            RichRun start_run = written_box.Selection.Start.Parent as RichRun;
            RichRun end_run = written_box.Selection.End.Parent as RichRun;
            Paragraph start_paragraph = start_run.Parent as Paragraph;
            Paragraph end_paragraph = end_run.Parent as Paragraph;
            bool SameRun = Equals(start_run,end_run);
            if(SameRun)
            {
                #region 同步数据
                CurrentRichRun = start_run;
                #region 事件的开关
                Binding HaveClickEventBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.HasClickEvent"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Binding HaveHoverEventBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.HasHoverEvent"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Binding HaveInsertionBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.HasInsertion"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                #endregion

                #region 事件的类型和值
                Binding ClickEventActionBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.ClickEventActionItem"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Binding HoverEventActionBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.HoverEventActionItem"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };

                Binding ClickEventValueBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.ClickEventValue"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Binding HoverEventValueBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.HoverEventValue"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                Binding InsertionValueBinder = new Binding()
                {
                    Path = new PropertyPath("CurrentRichRun.InsertionValue"),
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                #endregion

                BindingOperations.SetBinding(EventComponent.EnableClickEvent, RadiusToggleButtons.IsCheckedProperty, HaveClickEventBinder);
                BindingOperations.SetBinding(EventComponent.EnableHoverEvent, RadiusToggleButtons.IsCheckedProperty, HaveHoverEventBinder);
                BindingOperations.SetBinding(EventComponent.EnableInsertion, RadiusToggleButtons.IsCheckedProperty, HaveInsertionBinder);

                EventComponent.ClickEventPanel.Visibility = CurrentRichRun.HasClickEvent ? Visibility.Visible : Visibility.Collapsed;
                EventComponent.HoverEventPanel.Visibility = CurrentRichRun.HasHoverEvent ? Visibility.Visible : Visibility.Collapsed;
                EventComponent.InsertionPanel.Visibility = CurrentRichRun.HasInsertion ? Visibility.Visible : Visibility.Collapsed;

                BindingOperations.SetBinding(EventComponent.ClickEventActionBox,TextComboBoxs.SelectedItemProperty, ClickEventActionBinder);
                BindingOperations.SetBinding(EventComponent.HoverEventActionBox, TextComboBoxs.SelectedItemProperty, HoverEventActionBinder);

                #region 在视觉上更新事件类型
                EventComponent.ClickEventActionBox.ApplyTemplate();
                EventComponent.HoverEventActionBox.ApplyTemplate();
                TextBox clickEventActionBox = EventComponent.ClickEventActionBox.Template.FindName("EditableTextBox", EventComponent.ClickEventActionBox) as TextBox;
                clickEventActionBox.Text = CurrentRichRun.ClickEventActionItem.ItemText;

                TextBox hoverEventActionBox = EventComponent.HoverEventActionBox.Template.FindName("EditableTextBox", EventComponent.HoverEventActionBox) as TextBox;
                hoverEventActionBox.Text = CurrentRichRun.HoverEventActionItem.ItemText;
                #endregion

                BindingOperations.SetBinding(EventComponent.ClickEventValueBox, TextBox.TextProperty, ClickEventValueBinder);
                BindingOperations.SetBinding(EventComponent.HoverEventValueBox, TextBox.TextProperty, HoverEventValueBinder);
                BindingOperations.SetBinding(EventComponent.InsertionValueBox, TextBox.TextProperty, InsertionValueBinder);
                #endregion
            }
            else
            {

            }
            EventForm.Show();
            EventForm.Topmost = true;
        }

        /// <summary>
        /// 检测按回车键换行和粘贴
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
        /// 处理空文档内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void WrittenBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            EnabledFlowDocument enabledFlowDocument = written_box.Document as EnabledFlowDocument;
            if (enabledFlowDocument.Blocks.Count == 0)
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(new RichRun());
                enabledFlowDocument.Blocks.Add(paragraph);
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
                //string text = (current_obj as Run).Text;
                current_obj = new RichRun();
                //Run run = current_obj as Run;
                //RichRun richRun = new RichRun();
                //richRun.Text = run.Text;
                //current_obj = richRun;
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
            //初始化文档链表
            WrittenBookPages.Add(written_box.Document as EnabledFlowDocument);
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
            written_book_background = sender as Image;
            if (File.Exists(backgroundFilePath))
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
        /// 载入署名并关闭背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureAndCloseLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(signatureAndCloseFilePath))
            {
                sureToSignatureButton = sender as Image;
                sureToSignatureButton.Source = new BitmapImage(new Uri(signatureAndCloseFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 取消署名背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureCancelLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(signatureCancelFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(signatureCancelFilePath, UriKind.Absolute));
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
        /// 载入署名背景
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureBackgroundLoaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(signatureFilePath))
            {
                Image image = sender as Image;
                image.Source = new BitmapImage(new Uri(signatureFilePath, UriKind.Absolute));
            }
        }

        /// <summary>
        /// 署名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(File.Exists(signatureBackgroundFilePath))
            written_book_background.Source = new BitmapImage(new Uri(signatureBackgroundFilePath,UriKind.Absolute));

            DisplayAuthor = DisplayTitle = DisplaySignatureButton = DisplayCancelSignatureButton = Visibility.Visible;
            DisplayPageIndex = DisplayWrittenBox = DisplaySignature = DisplayLeftArrow = DisplayRightArrow = DisplayStylePanel = Visibility.Collapsed;
        }

        /// <summary>
        /// 署名并关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureAndCloseMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            run_command();
        }

        /// <summary>
        /// 取消署名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SignatureCancelMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (File.Exists(backgroundFilePath))
                written_book_background.Source = new BitmapImage(new Uri(backgroundFilePath, UriKind.Absolute));

            DisplayAuthor = DisplayTitle = DisplaySignatureButton = DisplayCancelSignatureButton = Visibility.Collapsed;
            DisplayPageIndex = DisplayWrittenBox = DisplaySignature = DisplayLeftArrow = DisplayRightArrow = DisplayStylePanel = Visibility.Visible;
        }

        /// <summary>
        /// 向左翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LeftArrowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentPageIndex--;
            //获取当前文档对象
            EnabledFlowDocument currentFlowDocument = written_box.Document as EnabledFlowDocument;
            WrittenBookPages[CurrentPageIndex + 1] = currentFlowDocument;
            written_box.Document = WrittenBookPages[CurrentPageIndex];
        }

        /// <summary>
        /// 向右翻页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void RightArrowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CurrentPageIndex++;
            //获取当前文档对象
            EnabledFlowDocument currentFlowDocument = written_box.Document as EnabledFlowDocument;
            while (WrittenBookPages.Count < (CurrentPageIndex + 1))
                WrittenBookPages.Add(new EnabledFlowDocument() { FontFamily = new FontFamily(commonFontFamily),FontSize = 30, LineHeight = 10 });
            MaxPage = WrittenBookPages.Count;
            WrittenBookPages[CurrentPageIndex - 1] = currentFlowDocument;
            written_box.Document = WrittenBookPages[CurrentPageIndex];
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
}
