using System.Windows;
using System.Windows.Controls;

namespace cbhk_environment.Generators.RecipeGenerator
{
    /// <summary>
    /// Recipe.xaml 的交互逻辑
    /// </summary>
    public partial class Recipe
    {
        //主窗体引用
        public static MainWindow cbhk = null;
        public Recipe(MainWindow win)
        {
            InitializeComponent();
            cbhk = win;
        }
    }
}
