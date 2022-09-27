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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MSScriptControl;

namespace WpfApp1
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	/// 

	public partial class MainWindow : Window
	{
		public static ScriptControlClass javascript;
		
		public MainWindow()
		{
			javascript = new ScriptControlClass();
			javascript.Language = "javascript";
			InitializeComponent();
			
		}
		static object EvalJScript(string JScript)
		{
			object Result = null;
			try
			{
				Result = javascript.Eval(JScript);
			}
			catch (Exception ex)
			{
				return ex.Source + "\n" + ex.Message;
			}
			return Result;
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			String line;
			String Text = "";
			TextBlock1.Text = "";
			try
			{
				//Pass the file path and file name to the StreamReader constructor
				Text = System.IO.File.ReadAllText(".\\json.js");
				//close the file
				// Console.ReadLine();
				if(Text == null)
				{
					TextBlock1.Text += ("Exception: " + "读取文件 .\\js.json 失败\n");

				}
				//TextBox1.Text = Text;
				try
				{
					// Console.WriteLine(Text);
					TextBlock1.Text += "Eval Result1: " + (String)EvalJScript(Text) + "\n";

				}
				catch (Exception err)
				{
					TextBlock1.Text += ("Exception: " + err.Message + "\n");

				}
			}
			catch (Exception err)
			{
				TextBlock1.Text += ("Exception: " + err.Message + "\n");
			}
			try
			{
				//Pass the file path and file name to the StreamReader constructor
				StreamReader sr = new StreamReader(".\\items.json");
				//Read the first line of text
				line = sr.ReadLine();
				Text = "";
				//Continue to read until you reach end of file
				while (line != null)
				{
					//write the line to console window
					//Console.WriteLine(line);
					Text += line + "\n";
					//Read the next line
					line = sr.ReadLine();
				}
				//close the file
				sr.Close();
				// Console.ReadLine();
				TextBox1.Text = Text;
				try
				{
					// Console.WriteLine(Text);
					TextBlock1.Text += "Eval Result2: " + ((Boolean)EvalJScript("parseJSON(" + Text + ");") ? "true" : "false") + "\n";
					TextBlock1.Text += "Eval Result3: " + ((String)EvalJScript("getJSON('[0].name');")) + "\n";
				}
				catch (Exception err)
				{
					TextBlock1.Text += ("Exception: " + err.Message + "\n");

				}
			}
			catch (Exception err)
			{
				TextBlock1.Text += ("Exception: " + err.Message + "\n");
			}
			finally
			{
				// 解析JSON

				// Console.WriteLine("Executing finally block.");
			}
		}
	}
}
