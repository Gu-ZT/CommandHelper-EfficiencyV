using cbhk_environment.CustomControls;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace cbhk_environment.ControlsDataContexts
{
    public class ColorNumbericUpDownHander
    {
        public ColorNumbericUpDowns box = new ColorNumbericUpDowns();
        public TextBox source_box = new TextBox();
        private bool Handled = false;

        public void ColorNumbericTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            source_box = e.Source as TextBox;
            box = source_box.TemplatedParent as ColorNumbericUpDowns;
        }

        public void ColorNumbericUpDownsPreviewKeyDown(object sender, TextCompositionEventArgs e)
        {
            Handled = false;
            TextBox current_box = sender as TextBox;
            if ( !Regex.IsMatch(e.Text,@"^[0-9.-]*$") || (!Regex.IsMatch(e.Text, @"^[0-9.]*$") && !Regex.IsMatch(e.Text, @"-") && source_box.Text == "") || (current_box.Text.Trim()!= "" && e.Text == "-" && current_box.SelectionStart > 0))
            {
                e.Handled = true;
                Handled = true;
                return;
            }
            if (Regex.Matches(source_box.Text, @"[\.]").Count > 0 && Regex.Matches(source_box.Text, @"-").Count > 0 && !Regex.IsMatch(e.Text, @"^[0-9]*$"))
                e.Handled = true;
            else
            if (Regex.Matches(source_box.Text, @"[\.]").Count >= 1 && Regex.Matches(source_box.Text, @"-").Count == 0 && !Regex.IsMatch(e.Text, @"^[0-9-]*$"))
                e.Handled = true;
            else
            if (Regex.Matches(source_box.Text, @"-").Count >= 1 && Regex.Matches(source_box.Text, @"[\.]").Count == 0 && !Regex.IsMatch(e.Text, @"^[0-9\.]*$"))
                e.Handled = true;

            bool Isbool = double.TryParse(source_box.Text, out double current_value);

            if (Isbool && current_value > box.MaxValue)
                box.Text = double.MaxValue + "";
            if (Isbool && current_value < box.MinValue)
                box.Text = double.MinValue + "";
        }

        public string[] GetIntValue(string current_value)
        {
            if (!current_value.Contains(".")) return null;
            string[] text_list = current_value.Split('.');
                text_list[0] = int.Parse(text_list[0]) + 1+"";
            return text_list;
        }

        public void ColorNumberUpClick(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns color_box = (sender as RepeatButton).TemplatedParent as ColorNumbericUpDowns;
            TextBox current_box = color_box.Template.FindName("textbox", color_box) as TextBox;

            if (current_box.Text.Trim() == "" || current_box.Text.Trim() == "-")
            {
                current_box.Text = color_box.Text = "0";
                return;
            }

            string[] number_list = new string[2];
            number_list = GetIntValue(current_box.Text);
            if (number_list != null)
                color_box.Text = double.Parse(number_list[0]) + "." + (number_list[1].Trim() != "" ? number_list[1] : "");
            else
                color_box.Text = double.Parse(current_box.Text) + 1+"";

            current_box.Text = color_box.Text;

            if (double.Parse(current_box.Text) > color_box.MaxValue)
            {
                current_box.Text = color_box.MaxValue + "";
                color_box.Text = current_box.Text;
                return;
            }
        }

        public void ColorNumberDownClick(object sender, RoutedEventArgs e)
        {
            ColorNumbericUpDowns color_box = (sender as RepeatButton).TemplatedParent as ColorNumbericUpDowns;
            TextBox current_box = color_box.Template.FindName("textbox", color_box) as TextBox;

            if (current_box.Text.Trim() == "" || current_box.Text.Trim() == "-")
            {
                current_box.Text = color_box.Text = "0";
                return;
            }

            string[] number_list = new string[2];
            number_list = GetIntValue(current_box.Text);
            if (number_list != null)
                color_box.Text = double.Parse(number_list[0]) - 2.0 + "." + (number_list[1].Trim() != "" ? number_list[1] : "");
            else
                color_box.Text = double.Parse(current_box.Text) - 1.0 + "";

            current_box.Text = color_box.Text;

            if (double.Parse(current_box.Text) < color_box.MinValue)
            {
                current_box.Text = color_box.MinValue + "";
                color_box.Text = current_box.Text;
                return;
            }
        }

        public void TextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (Handled)
                return;
            TextBox current_box = sender as TextBox;
            ColorNumbericUpDowns color_box = current_box.TemplatedParent as ColorNumbericUpDowns;
            if (current_box.Text.Trim() == "-")
                return;
            if(current_box.Text.Replace(" ","") == "-.")
            {
                current_box.Text = "-0.";
                current_box.SelectionStart = 3;
            }
            if (current_box.Text.Trim() == "" || current_box.Text.Trim() == ".")
            {
                current_box.Text = color_box.Text = "0";
                return;
            }

            if (double.Parse(current_box.Text) > color_box.MaxValue)
            {
                current_box.Text = color_box.Text = color_box.MaxValue + "";
            }
            else
                if(double.Parse(current_box.Text) < color_box.MinValue)
                current_box.Text = color_box.Text = color_box.MinValue+"";
            else
                color_box.Text = current_box.Text;
        }
    }
}
