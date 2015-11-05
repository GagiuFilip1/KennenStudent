using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Nemiro.OAuth;
using Nemiro.OAuth.LoginForms;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using System.Web;
using System.Xml.Serialization;
using System.Xml;
using System.Timers;
using System.Text.RegularExpressions;

namespace KennenStudent
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
            
        string CurrentPath = "/";
        public static string string1;
        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 500;
            timer1.Enabled = true;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Start();
            
            if (String.IsNullOrEmpty(Properties.Settings.Default.AccessToken))
            {
                this.GetAccessToken();
            }
            else
            {
                this.GetFiles();
            }
        }
        private void GetAccessToken()
        {

            var login = new DropboxLogin("yxk0rl5wlff50vc", "rqcvwt7pmmqubpp");
            login.Owner = this;
            login.ShowDialog();
            if (login.IsSuccessfully)
            {
                Properties.Settings.Default.AccessToken = login.AccessToken.Value;
                Properties.Settings.Default.Save();
            }
            else
            {
                MessageBox.Show("error...");
                this.OnFormClosed();

            }
        }

        private void OnFormClosed()
        {
            this.Close();
        }
        private void GetFiles()
        {
            OAuthUtility.GetAsync
            (
              "https://api.dropbox.com/1/metadata/auto/",
                new HttpParameterCollection
                {
                    {"path",this.CurrentPath },
                    {"access_token",Properties.Settings.Default.AccessToken }
                },
                callback: GetFiles_Result
             );
        }
        private void GetFiles_Result(RequestResult result) { }
        private void GetShareLink_Result(RequestResult result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<RequestResult>(GetShareLink_Result), result);
                string appPath = Path.GetDirectoryName(Application.ExecutablePath);
                var rezultat = result.ToString();
                Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(rezultat);
                try
                {
                    string url = values["url"];
                    url = url.Remove(url.Length - 1, 1) + "1";
                    TextBox.CheckForIllegalCrossThreadCalls = false;
                    textBox1.Text = url;
                    string1 = url;
                }
                catch (Exception ex) { MessageBox.Show("You are not connected to Dropbox"); }
                XDocument doc = new XDocument(new XElement("body",
                                           new XElement("line", string1)
                                                    )
                                     );
                doc.Save(appPath + "\\" + "Data.xml");
                FileStream upstream = new FileStream((appPath + "\\" + "Data.xml").ToString(), FileMode.Open);
               OAuthUtility.PutAsync
                             (
                             "https://api-content.dropbox.com/1/files_put/auto/",
                             new HttpParameterCollection
                {
                    {"access_token",Properties.Settings.Default.AccessToken},
                    {"path",Path.Combine(Path.GetFileName("Data.xml")).Replace("\\","/")},
                    {"overwrite","false"},
                    {"autorename","false"},
                    {upstream}
                },
                             callback: Upload_Result
                             );
                return;
            }
        }
        private void Upload_Result(RequestResult result)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
                try
                {
                    string appPath = Path.GetDirectoryName(Application.ExecutablePath);
                    XmlDocument xDoc = new XmlDocument();
                    XmlDocument xmlDoc = new XmlDocument();
                    var web = new WebClient();
                    web.DownloadFile(new Uri(string.Format("https://api-content.dropbox.com/1/files/auto/{0}?access_token={1}", "Data.xml", Properties.Settings.Default.AccessToken)), appPath + "\\" + "Data.xml");
                    xDoc.Load(appPath + "\\" + "Data.xml");
                    XmlRootAttribute xRoot = new XmlRootAttribute();
                    xRoot.ElementName = "user";
                    xRoot.IsNullable = true;
                    XmlSerializer sr = new XmlSerializer(typeof(Information));
                    FileStream read = new FileStream("Data.xml", FileMode.Open, FileAccess.Read, FileShare.Read);
                    Information Info = (Information)sr.Deserialize(read);
                    textBox1.Text = Info.Url;
                    textBox1.Text.Trim().Replace(" ", "%20");
                    textBox1.Text.Trim().Replace("watch?", "");
                    textBox1.Text.Trim().Replace("=", "/");
                    
                    if (String.IsNullOrEmpty(textBox1.ToString()) || textBox1.ToString().Trim().Length == 0)
                    {
                        if (File.Exists("Data.xml"))
                        {
                        }
                        else
                        {
                            MessageBox.Show("Nu exista Fisierul");
                        }
                    }
                    timer1.Stop();

                }
                catch (WebException ex)
                {
                    if (((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                    {

                    }
                }
            
        }
        private void button2_Click(object sender, EventArgs e)
        {
            Vlc.Movie = textBox1.Text.ToString();
            Vlc.Movie.en
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string load = textBox1.Text.ToString();
            string load2 = Regex.Replace(load, "watch" , "" );
            string load3 = load2.ToString();
            string load4 = Regex.Replace(load3, "[?]", "");
            string load5 = load4.ToString();
            string load6 = Regex.Replace(load5, "[=]", "/");
            textBox1.Clear();
            textBox1.Text = load6.ToString();
        }
        }
    }
