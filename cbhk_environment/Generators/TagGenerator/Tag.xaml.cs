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
using System.Windows.Threading;
using Windows.UI.Xaml.Controls;

namespace cbhk_environment.Generators.TagGenerator
{
    /// <summary>
    /// Tag.xaml 的交互逻辑
    /// </summary>
    public partial class Tag
    {
        //主页引用
        public static MainWindow cbhk = null;
        public Tag(MainWindow win)
        {
            InitializeComponent();
            cbhk = win;
        }
    }
}
