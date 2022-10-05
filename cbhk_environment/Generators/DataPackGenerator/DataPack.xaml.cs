using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
