using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace 约车抢购
{
    public partial class Main : Form
    {
        HttpClient client = Static.client;
        private List<KeyValuePair<string,string>> loginPost_temp = new List<KeyValuePair<string, string>>();

        public Main()
        {
            InitializeComponent();
            comboBox1.SelectedItem = "科目二";
        }

        private void Main_Load(object sender, EventArgs e)
        {
            updateOutData();
            updateFrame();

        }

        /// <summary>
        /// 更新外部框架
        /// </summary>
        private void updateOutData()
        {
            label5.Text = "姓    名:" + Static.UserName;
            label6.Text = "教    练:" + Static.TeachName;
            label7.Text = "剩余学时:" + Static.LeftTime;
        }

        /// <summary>
        /// 刷新子框架
        /// </summary>
        private async void updateFrame()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/book1bycoach.aspx?traintype=" + typeKM.Trim() + "&coachname="+Static.TeachName);
            var res = await client.SendAsync(req);
            var res_str = await res.Content.ReadAsStringAsync();
            resolve(res_str);
        }

        string temp = "";

        bool continues = true;

        /// <summary>
        /// 解析子框架html
        /// </summary>
        /// <param name="res_str"></param>
        private void resolve(string res_str)
        {
            temp = res_str;
            loginPost_temp = new List<KeyValuePair<string, string>>();

            string __EVENTVALIDATION = new Regex("id=\"__EVENTVALIDATION\" value=\"(.*?)\"").Match(res_str).Groups[1].Value;
            string __VIEWSTATE = new Regex("id=\"__VIEWSTATE\" value=\"(.*?)\"").Match(res_str).Groups[1].Value;
            string __VIEWSTATEGENERATOR = new Regex("id = \"__VIEWSTATEGENERATOR\" value = \"(.*?)\"").Match(res_str).Groups[1].Value;


            loginPost_temp.Add(new KeyValuePair<string, string>("__VIEWSTATE", __VIEWSTATE));
            loginPost_temp.Add(new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", __VIEWSTATEGENERATOR));
            loginPost_temp.Add(new KeyValuePair<string, string>("__EVENTVALIDATION", __EVENTVALIDATION));
            var x1 = string.Format("<a id=\"{0}\" href=\"javascript:__doPostBack('{0}','')\">{1}</a>", KS, Static.UserName);
            if (!res_str.Contains("网上约车尚未开始，请在上午8至19间进行约车")&&QK)
            {
                var x = string.Format("<a id=\"{0}\" href=\"javascript:__doPostBack('{0}','')\">{1}</a>", KS, Static.UserName);
                var yc = string.Format("<a id=\"{0}\" href=\"javascript:__doPostBack('{0}','')\">{1}</a>", KS, "约车");
                if (!res_str.Contains(yc))
                {
                    KS++;
                    if (KS > 14)
                    {
                        MessageBox.Show("未抢到");
                    }
                }

                if (res_str.Contains(x))
                {
                    continues = false;
                    MessageBox.Show("抢课成功");
                }
            }
            res_str = res_str.Replace("\r", "").Replace("\n", "");
            res_str = Regex.Replace(res_str, ">[\\s]{1,}<", "><");
            DataTable dt = new DataTable();
            dt.Columns.Add("content");
            MatchCollection MatchCollections = new Regex(">([^<>/][\\S]{1,}?)<").Matches(res_str);
            foreach (Match m in MatchCollections)
            {
                dt.Rows.Add(m.Groups[1].Value);
            }
            inv(dt); 
            return ;
        }

        private void inv(DataTable dt)
        {
            if (dataGridView1.InvokeRequired)
            {
                Action<DataTable> action = new Action<DataTable>(inv);
                dataGridView1.Invoke(action, dt);
            }else
            {
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dt;
                Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            }
            
        }
        bool QK = false;

        private void button1_Click(object sender, EventArgs e)
        {
            typeKM = comboBox1.Text;
            QK = false;
            updateFrame();
            invokes();
        }

        bool gos = false;

        int all = 0;

        string typeKM = "";

        int real = 0;

        /// <summary>
        /// 自动抢课
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            typeKM = comboBox1.Text;
            QK = true;
            continues = true;
            gos = false;
            all = 0;
            real = 0;
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                do
                {
                    if (DateTime.Now.Hour == 8 && DateTime.Now.Minute >= 30)
                    {
                        gos = true;
                    }
                    if (DateTime.Now.Hour == 8 && DateTime.Now.Minute >= 33)
                    {
                        gos = false;
                    }
                    all++;
                    if (gos)
                    {
                        real++;                        
                        Thread.Sleep(20);
                    }else
                    {
                        Thread.Sleep(10000);
                    }
                    go();
                    invokes();
                } while (continues);
            });
        }

        private void invokes()
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new Action(invokes));
            }else
            {
                label1.Text = "gos = " + gos.ToString();
                label2.Text = "continues = " + continues.ToString();

                label3.Text = "all = " + all.ToString();
                label4.Text = "real = " + real.ToString();
            }
        }

        //课时
        int KS = 0; 

        /// <summary>
        /// 开始抢课
        /// </summary>
        private void go()
        {
            

            try
            {
                if (KS == 0)
                    KS = int.Parse(textBox1.Text);
                List<KeyValuePair<string, string>> parms = new List<KeyValuePair<string, string>>(loginPost_temp);
                parms.Add(new KeyValuePair<string, string>("__EVENTTARGET", KS + ""));
                var req = new HttpRequestMessage(HttpMethod.Post, "book1bycoach.aspx?traintype=" + typeKM.Trim() + "&coachname=" + Static.TeachName);
                req.Content = new FormUrlEncodedContent(parms);
                var res = Static.client.SendAsync(req);
                res.Wait();
                var res_str = res.Result.Content.ReadAsStringAsync();
                res_str.Wait();
                resolve(res_str.Result);
            }
            catch 
            {

              
            }
        }

        /// <summary>
        /// 手动抢课
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            typeKM = comboBox1.Text;
            go();
        }

        /// <summary>
        /// 停止抢课
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            continues = false;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            continues = false;
        }
    }
}
