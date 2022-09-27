

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace cbhk_environment.SettingForm
{
    public class InitCircularBanner:ObservableObject
    {
        public RelayCommand<string> LinkCommand { get; set; }

        public InitCircularBanner()
        {
            LinkCommand = new RelayCommand<string>(link_command);
        }

        /// <summary>
        /// 打开目标网址
        /// </summary>
        /// <param name="url">网址数据</param>
        private void link_command(string url)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(url, @"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$"))
            System.Diagnostics.Process.Start(url);
        }
    }
}
