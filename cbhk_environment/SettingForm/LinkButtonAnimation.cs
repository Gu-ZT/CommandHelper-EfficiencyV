using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace cbhk_environment.SettingForm
{
    public class LinkButtonAnimation
    {
        /// <summary>
        /// 播放位移动画
        /// </summary>
        private static DoubleAnimation ButtonLocationAnimation = new DoubleAnimation()
        {
            From = 0,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = new RepeatBehavior(1),
            FillBehavior = FillBehavior.Stop
        };

        /// <summary>
        /// 播放透明度动画
        /// </summary>
        private static DoubleAnimation ButtonOpacityAnimation = new DoubleAnimation()
        {
            To = 0,
            Duration = TimeSpan.FromSeconds(1),
            RepeatBehavior = new RepeatBehavior(1),
            FillBehavior = FillBehavior.Stop
        };

        public Storyboard storyboard = new Storyboard();

        //保存控制按钮面板
        public List<Button> switch_buttons = new List<Button>{ };

        //保存网站图片链表
        public List<Button> link_buttons = new List<Button> { };

        //保存两种按钮样式
        public List<Style> styles = new List<Style> { };

        //当前按钮
        public Button current_button = new Button();

        //"RenderTransform.X"
        //"(0).(1)", propertyChain
        private static object[] propertyChain = new object[] { UIElement.RenderTransformProperty,
            TranslateTransform.XProperty };

        /// <summary>
        /// 初始化动画参数
        /// </summary>
        /// <param name="buttons">需要位移的按钮链表</param>
        /// <param name="SwitchButtons">两个切换按钮</param>
        /// <param name="ButtonIndexes">切换按钮的索引</param>
        /// <param name="Styles">两个按钮的样式</param>
        public LinkButtonAnimation(List<Button> buttons)
        {
            foreach (Button current_button in buttons)
            {
                current_button.RenderTransform = new TranslateTransform();
            }
            //绑定回调事件
            storyboard.Completed += StoryCompletedHandler;
        }

        public void SwitchOpacityAndTranslate(Button current_button,Button next_button,List<Button> LinkButtons, List<Button> SwitchButtons, List<Style> Styles)
        {
            #region 初始化参数
            switch_buttons = SwitchButtons;
            styles = Styles;
            link_buttons = LinkButtons;
            this.current_button = current_button;
            #endregion

            #region 透明度动画
            ButtonOpacityAnimation.From = current_button.Opacity;
            #endregion

            #region 位移动画
            ButtonLocationAnimation.To = -current_button.Width - 40;
            #endregion

            Storyboard.SetTarget(ButtonLocationAnimation, this.current_button);
            Storyboard.SetTarget(ButtonOpacityAnimation, this.current_button);
            Storyboard.SetTargetProperty(ButtonLocationAnimation, new PropertyPath("(0).(1)", propertyChain));//依赖的属性
            Storyboard.SetTargetProperty(ButtonOpacityAnimation, new PropertyPath(UIElement.OpacityProperty));//依赖的属性
            storyboard.Children.Add(ButtonLocationAnimation);
            storyboard.Children.Add(ButtonOpacityAnimation);

            storyboard.Begin();
            MainWindow.CircularBannerState = true;
        }

        public void StoryCompletedHandler(object sender,EventArgs e)
        {
            MainWindow.CircularBannerState = false;
            Panel.SetZIndex(current_button, 0);
            current_button.Name = "index_";
            for (int i = 0; i < link_buttons.Count; i++)
            {
                if (!link_buttons[i].Name.Contains("_"))
                {
                    int index = int.Parse(System.Text.RegularExpressions.Regex.Match(link_buttons[i].Name, @"^\+?(:?(:?\d+\.\d+)|(:?\d+))|(-?\d+)(\.\d+)?$").ToString());
                    link_buttons[i].Name = "index" + (index + 1);
                    Panel.SetZIndex(link_buttons[i], index + 1);
                }
            }
            current_button.Name = "index0";
            switch_buttons[0].Style = styles[0]; switch_buttons[1].Style = styles[1]; MainWindow.LinkButtonAnimator.IsEnabled = true;
        }
    }
}
