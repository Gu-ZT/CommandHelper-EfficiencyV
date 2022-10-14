using cbhk_environment.CustomControls;
using cbhk_environment.Generators.DataPackGenerator.Components;
using HandyControl.Controls;
using System;
using System.Windows.Media;
using Windows.Services.Maps;

namespace cbhk_environment.Generators.DataPackGenerator
{
    /// <summary>
    /// DataPack.xaml 的交互逻辑
    /// </summary>
    public partial class DataPack
    {
        /// <summary>
        /// 主窗体
        /// </summary>
        public static MainWindow cbhk = null;

        public DataPack(MainWindow win)
        {
            InitializeComponent();
            cbhk = win;
        }
    }
}
