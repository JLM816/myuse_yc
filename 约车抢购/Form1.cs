using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 约车抢购
{
    public partial class Form1 : Form
    {  
        List<KeyValuePair<string, string>> loginPost_temp;

        public static bool Success = false;

        public static string loginName = "";
        public static string loginPwd = "";
        public static string userName = "";

        public Form1()
        {
            InitializeComponent();
            Static.handler= new HttpClientHandler();
            Static.handler.UseCookies = true;
            Static.handler.UseProxy = true;
            Static.cookie = Static.handler.CookieContainer;
            Static.client = new HttpClient(Static.handler);
            Static.client.BaseAddress = new Uri("http://rl.yueche.net");
            Static.client.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/36.0.1985.143 Safari/537.36");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            getLoginPage();
        }

        /// <summary>
        /// 获取登录页面(获取到cookie同时去获取图片)
        /// </summary>
        async void getLoginPage()
        {
            loginPost_temp = new List<KeyValuePair<string, string>>();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "/login.aspx");
            //请求登陆页面获取cookie
            var loginResponse = await Static.client.SendAsync(request);
            getLoginImage();
            var res_str = await loginResponse.Content.ReadAsStringAsync();
            string __EVENTVALIDATION = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*?)\"").Match(res_str).Groups[1].Value;
            string __VIEWSTATE = new Regex("id=\"__VIEWSTATE\" value=\"(.*?)\"").Match(res_str).Groups[1].Value;
            string __VIEWSTATEGENERATOR = new Regex("id = \"__VIEWSTATEGENERATOR\" value = \"(.*?)\"").Match(res_str).Groups[1].Value;
            loginPost_temp.Add(new KeyValuePair<string, string>("__VIEWSTATE", __VIEWSTATE));
            loginPost_temp.Add(new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR));
            loginPost_temp.Add(new KeyValuePair<string, string>("__EVENTVALIDATION", __EVENTVALIDATION));
            
        }

        /// <summary>
        /// 获取登陆验证码图片
        /// </summary>
        async void getLoginImage()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/ValidateCode.aspx");
            HttpResponseMessage response = await Static.client.SendAsync(request);
            var stream = await response.Content.ReadAsStreamAsync();
            pictureBox1.Image = Image.FromStream(stream);
        }

        /// <summary>
        /// 点击图片刷新验证码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            getLoginImage();
        }

        /// <summary>
        /// 点击登陆按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
             
            Login();
        }

        async void Login()
        {  /*
            if(textBox1.Text == ""|| textBox2.Text == "")
            {
                MessageBox.Show("请填写流水号和密码");
                return;
            }*/
            List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>(loginPost_temp);
            parms.Add(new KeyValuePair<string, string>("ImageButton1.x", new Random().Next(10, 40).ToString()));
            parms.Add(new KeyValuePair<string, string>("ImageButton1.y", new Random().Next(0, 15).ToString()));
 
            parms.Add(new KeyValuePair<string, string>("stuid", textBox1.Text==""? "17060001';--" : textBox1.Text));
            parms.Add(new KeyValuePair<string, string>("psw", textBox2.Text == "" ? "" : textBox2.Text));

            parms.Add(new KeyValuePair<string, string>("code", textBox3.Text));

            var req = new HttpRequestMessage(HttpMethod.Post, "/login.aspx");
            req.Content = new FormUrlEncodedContent(parms);
            var res = await Static.client.SendAsync(req);
            var res_str = await res.Content.ReadAsStringAsync();
            res_str = Public.myTrim(res_str); 
            string checkStr = "流水号:" + (textBox1.Text == "" ? "" : textBox1.Text);
            if (res_str.Contains(checkStr))
            {
                Static.UserName = new Regex("姓 名:(.*?)<br />").Match(res_str).Groups[1].Value;
                Static.TeachName = new Regex("分配教练:(.*?)</td>").Match(res_str).Groups[1].Value.Trim();
                Static.UseTime = new Regex("已练学时:(.*?)<br />").Match(res_str).Groups[1].Value.Trim();
                Static.LeftTime = new Regex("剩余学时:(.*?)<br />").Match(res_str).Groups[1].Value.Trim();
                Success = true;
                this.Close();
            }
            else
            {
                if (res_str.Contains("验证码错误!"))
                {
                    MessageBox.Show("验证码错误,请重新输入");
                }
                Success = false;
            }
        }

        private void textBox3_Enter(object sender, EventArgs e)
        {
            Login();
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Login();
            }
        }
    }
}
