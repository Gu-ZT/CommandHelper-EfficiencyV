using cbhk_environment.CustomControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace cbhk_environment.Generators.VillagerGenerator
{
    /// <summary>
    /// Villager.xaml 的交互逻辑
    /// </summary>
    public partial class Villager
    {
        /// <summary>
        /// 主窗体引用
        /// </summary>
        public static MainWindow cbhk = null;
        public Villager(MainWindow win)
        {
            InitializeComponent();
            cbhk = win;
        }
    }
}
