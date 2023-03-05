using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace cbhk_signin.resources.Tools
{
    public class SignIn
    {
		//任务进度条引用
		static ProgressBar download_progressbar;
		static Label progress_display;
		static Label progress_speed;
		static Stopwatch download_speed_tester = new Stopwatch();

		/// <summary>  
		/// 获取字符中指定标签的值  
		/// </summary>  
		/// <param name="str">字符串</param>  
		/// <param name="title">标签</param>  
		/// <param name="attrib">属性名</param>  
		/// <returns>属性</returns>  
		public static string GetTitleContent(string str, string title, string attrib)
		{

			string tmpStr = string.Format("<{0}[^>]*?{1}=(['\"\"]?)(?<url>[^'\"\"\\s>]+)\\1[^>]*>", title, attrib); //获取<title>之间内容  

			Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);

			string result = TitleMatch.Groups["url"].Value;
			return result;
		}

		public static string GetHtmlStr(string url, string encoding)
		{
			string htmlStr = "";
			try
			{
				if (!string.IsNullOrEmpty(url))
				{
					WebRequest request = WebRequest.Create(url);            //实例化WebRequest对象  
					WebResponse response = request.GetResponse();           //创建WebResponse对象  
					Stream datastream = response.GetResponseStream();       //创建流对象  
					Encoding ec = Encoding.Default;
					if (encoding == "UTF8")
					{
						ec = Encoding.UTF8;
					}
					else if (encoding == "Default")
					{
						ec = Encoding.Default;
					}
					StreamReader reader = new StreamReader(datastream, ec);
					htmlStr = reader.ReadToEnd();                  //读取网页内容  
					reader.Close();
					datastream.Close();
					response.Close();
				}
			}
			catch { }
			return htmlStr;
		}

		public static string RemoveJsonFieldIsNull(string str)
		{
			if (string.IsNullOrEmpty(str)) return str;
			string reg = ":\"\"";
			str = ReplaceByRegex(reg, str, ":null");

			return str;

			//string reg = "(?<beginStr>[^\"]*?)(?<key>\"?We*?\"?:?)(?<value>\".*?\",*)(?<endStr>[^\"]*?)";
			//string strSrcReg = "(?<beginSrc>^.*?(?=images))(?<char>(images))(?<endSrc>(?<=images).*?$)";
			// string reg = "(?<beginStr>^.*?)(?=:\"\")(?<value>(:\"\"))(?<endStr>(?<=:\"\").*?$)";
			//string reg = "(?<beginStr>^.*?)(?<value>(:\"\"))(?<endStr>.*?$)";
			//return ReplaceByRegex(reg, str, match =>
			//{
			//    string beginStr = match.Groups["beginStr"].Value;
			//    string value = match.Groups["value"].Value;
			//    string endStr = match.Groups["endStr"].Value;
			//    return beginStr+ ":null"+ endStr;
			//});
		}
		public static string ReplaceByRegex(string strReg, string html, string target)
		{
			return ReplaceByRegex(strReg, html, m => target);
		}
		public static string ReplaceByRegex(string strReg, string html, MatchEvaluator function)
		{
			if (string.IsNullOrEmpty(html) || string.IsNullOrEmpty(strReg)) return null;

			Regex regex = new Regex(strReg, RegexOptions.Singleline | RegexOptions.IgnoreCase);

			html = regex.Replace(html, function);
			return html;
		}
		public static string GetDataByPost(string url, string account, string password)
		{
			//0e805eb9ea5b2d7a266f29af992704c9
			//d984e4ac-191a-4fe6-a2df-31ee226402f3
			//string content = "account=" + account + "&password=" + password + "&key=d984e4ac-191a-4fe6-a2df-31ee226402f3";
			string content = "https://mc.metamo.cn/api/market/open/buyerQuery?token=0e805eb9ea5b2d7a266f29af992704c9&email="+account;

			using (WebClient webClient = new WebClient())
			{
				webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded; charset=utf-8";
				string result = webClient.UploadString(url, content);
				Console.WriteLine(result);
				return result;
			}
		}

		public static string GetDataByGet(string url, string postDatastr)
		{
			string result = "";
			try
			{
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				ServicePointManager.DefaultConnectionLimit = 200;
				//ServicePointManager.ServerCertificateValidationCallback = (object < p0 >, X509Certificate<p1>, X509Chain<p2>, SslPolicyErrors<p3>) => true;
				HttpWebRequest request2 = WebRequest.Create(url + ((postDatastr == "") ? "" : "?") + postDatastr) as HttpWebRequest;
				request2.Method = "GET";
				request2.Timeout = 5000;
				request2.KeepAlive = false;
				request2.ContentType = "application/json; charset=utf-8";
				HttpWebResponse response2 = request2.GetResponse() as HttpWebResponse;
				Stream result_stream = response2.GetResponseStream();
				StreamReader result_reader = new StreamReader(result_stream, new UTF8Encoding(false));
				result = result_reader.ReadToEnd();
				result_stream.Close();
				result_reader.Close();
				response2.Close();
				response2 = null;
				request2.Abort();
				request2 = null;
			}
			catch (Exception e)
			{
				System.Windows.MessageBox.Show(e.Message);
			}
			return result;
		}

		/// <summary>
		/// 设置证书
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="certificate"></param>
		/// <param name="chain"></param>
		/// <param name="errors"></param>
		/// <returns></returns>
		public static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{   // 总是接受  
			return true;
		}

        public static bool DownLoadUserHead(string target_url, string target_file_path)
        {
            //验证服务器证书回调自动验证
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            HttpWebRequest Myrq = WebRequest.Create(target_url) as HttpWebRequest;
            Myrq.KeepAlive = false;//持续连接
            Myrq.Timeout = 30 * 1000;//30秒，*1000是因为基础单位为毫秒
            Myrq.Method = "GET";//请求方法
            Myrq.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3";//network里面找
            Myrq.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36";

            //接受返回
            try
            {
                HttpWebResponse Myrp = (HttpWebResponse)Myrq.GetResponse();

                if (Myrp.StatusCode != HttpStatusCode.OK)
                { return false; }

                FileStream fs = new FileStream(target_file_path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);//展开一个流
                Myrp.GetResponseStream().CopyTo(fs);//复制到当前文件夹
                Myrp.Close();
                fs.Close();
            }
            catch { }
            return true;
        }

        public static string GetJarFile(string url, string target_path)
		{
			//try
			//{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			//request.Timeout = 30000;
			//request.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			request.AllowAutoRedirect = true;
			WebResponse response = request.GetResponse();
			Stream reader = response.GetResponseStream();
			FileStream writer = new FileStream(target_path, FileMode.OpenOrCreate, FileAccess.Write);
			byte[] buff = new byte[response.ContentLength];
			int c2 = 0;
			while ((c2 = reader.Read(buff, 0, buff.Length)) > 0)
			{
				writer.Write(buff, 0, c2);
			}
			writer.Close();
			writer.Dispose();
			reader.Close();
			reader.Dispose();
			response.Close();
			//}
			//catch (Exception e)
			//{
			//	return e.Message;
			//}
			return "success";
		}
	}
}
