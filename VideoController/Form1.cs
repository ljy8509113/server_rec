using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;


namespace VideoController
{
    public partial class Form1 : Form
    {
        string port = "";
        string ip = "";
        List<ViewItem> viewerList = null;

        public Form1()
        {
            InitializeComponent();

            this.listView1.Columns.Add("디바이스 이름", 200);
            //this.listView1.Columns.Add("UUID", 200);
            this.listView1.Columns.Add("User", 120);
            this.listView1.Columns.Add("상태", 200);

            FileInfo protFile = new FileInfo(Common.SETTING_PATH);
            if (protFile.Exists)
            {
                string settingStr = File.ReadAllText(Common.SETTING_PATH);
                JObject obj = JObject.Parse(settingStr);

                Console.WriteLine("json : {0}", obj);

                JToken ftpToken = obj.GetValue(Common.KEY_FTP_PATH);
                JToken ipToken = obj.GetValue(Common.KEY_IP);
                JToken portToken = obj.GetValue(Common.KEY_PORT);

                bool isIP = false;
                bool isPort = false;
                bool isFTP = false;

                if(ipToken != null)
                {
                    this.ip = ipToken.ToString(); //obj[Common.KEY_IP].ToString();
                    textBox3.Text = this.ip;
                    textBox3.Enabled = false;
                    isIP = true;
                }
                else
                {
                    //MessageBox.Show("IP 설정이 필요합니다.");
                    //return;
                    isIP = false;
                }
                
                if(portToken != null)
                {
                    this.port = portToken.ToString();//obj[Common.KEY_PORT].ToString();
                    textBox2.Text = this.port;
                    textBox2.Enabled = false;
                    isPort = true;
                }
                else
                {
                    //MessageBox.Show("PORT 설정이 필요합니다.");
                    //return;
                    isPort = false;
                }

                if (ftpToken != null)
                {
                    Common.FTP_PATH = ftpToken.ToString(); //obj[Common.KEY_FTP_PATH].ToString();
                    label4.Text = Common.FTP_PATH;
                    button13.Enabled = false;
                    isFTP = true;
                }
                else
                {
                    isFTP = false;
                    //MessageBox.Show("FTP 파일 저장경로 설정이 필요합니다.");
                }

                if(isIP && isPort)
                {
                    if (!openSocket())
                    {
                        removeIPPORT();
                        textBox2.Text = "";
                        textBox2.Enabled = true;
                        textBox3.Text = "";
                        textBox3.Enabled = true;
                        button6.Enabled = true;
                    }
                    else
                    {
                        button6.Enabled = false;
                    }
                }
                                
                if(isIP && isPort && isFTP)
                {

                }
                else
                {
                    string ipStr = isIP == true ? "" : "IP ";
                    string portStr = isPort ? "" : "PORT ";
                    string ftpStr = isFTP ? "" : "FTP ";

                    MessageBox.Show( ipStr + portStr + ftpStr + "설정이 필요합니다.");
                }                                
            }
            else
            {
                MessageBox.Show("기본 설정이 필요합니다.");
            }

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);

            panel1.AutoScroll = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox1.Text = openFileDialog.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.URL = textBox1.Text;
            axWindowsMediaPlayer1.settings.volume = 100;
            //axWindowsMediaPlayer1.Ctlcontrols.play();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            axWindowsMediaPlayer1.Ctlcontrols.stop();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string ipAddr = textBox3.Text;
            string port = textBox2.Text;

            IPAddress ip;
            bool b = IPAddress.TryParse(ipAddr, out ip);

            int num = 0;

            if (!b)
            {
                MessageBox.Show("아이피 확인이 필요합니다.");
                return;
            }

            if (!int.TryParse(port, out num))
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                return;
            }

            //JObject json = new JObject();
            //json.Add("ip", ipAddr);
            //json.Add("port", port);

            //StreamWriter sw = new StreamWriter(settingPath, false);
            //sw.Write(json.ToString());
            //this.port = port;
            //this.ip = ipAddr;
            //sw.Close();

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic[Common.KEY_IP] = ipAddr;
            dic[Common.KEY_PORT] = port;

            saveSetting(dic);

            this.ip = ipAddr;
            this.port = port;

            if (SocketManager.getInstance()._isOn == false)
            {
                button6.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;

                if (!openSocket())
                {
                    removeIPPORT();
                }
            }
            else
            {
                SocketManager.getInstance().onEnd();
                Application.Restart();
            }
        }
        
        public bool openSocket()
        {
            SocketManager.getInstance().f = this;
            return SocketManager.getInstance().init(this.ip.ToString(), Int32.Parse(this.port));
            
        }

        public void removeIPPORT()
        {
            if (DialogResult.OK == MessageBox.Show("IP, PORT 확인이 필요합니다."))
            {
                FileInfo settingFile = new FileInfo(Common.SETTING_PATH);
                JObject json = null;

                if (settingFile.Exists)
                {
                    string settingStr = File.ReadAllText(Common.SETTING_PATH);
                    json = JObject.Parse(settingStr);

                    json.Remove(Common.KEY_IP);
                    json.Remove(Common.KEY_PORT);

                    StreamWriter sw = new StreamWriter(Common.SETTING_PATH, false);
                    sw.Write(json.ToString());
                    sw.Close();
                }
            }
        }

        private BackgroundWorker backgroundWorker1;

        public void addConnectionInfo()
        {
            this.backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                bool isUsed = false;
                foreach (ListViewItem i in this.listView1.Items)
                {
                    if (i.Tag.Equals(info.uuid))
                    {
                        isUsed = true;
                        if (info.isDownloading)
                        {
                            i.SubItems[2].Text = info.getDownMsg();
                        }
                        else
                        {
                            i.Tag = info.uuid;
                            i.SubItems[0].Text = info.name;
                            i.SubItems[1].Text = (info.isTeacher == true ? "선생님" : "학생");
                            i.SubItems[2].Text = info.status;//(info.socket.Connected == true ? "연결" : "해제");
                        }
                        break;
                    }
                }

                if (!isUsed)
                {
                    ListViewItem item = new ListViewItem(info.name);
                    item.Tag = info.uuid;
                    item.SubItems.Add((info.isTeacher == true ? "선생님" : "학생"));
                    item.SubItems.Add(info.status);//item.SubItems.Add((info.socket.Connected == true ? "연결" : "해제"));
                    AddItem(item, this.listView1);

                }
            }
            this.listView1.EndUpdate();
        }

        delegate void AddListCallback(ListViewItem item, ListView v);

        private void AddItem(ListViewItem item, ListView v)
        {
            if (v.InvokeRequired)
            {
                AddListCallback d = new AddListCallback(AddItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                v.Items.Add(item);
            }
        }

        public void updateProgress(string uuid, int progress, int current, int max, string fileName)
        {
            //foreach (ConnectionInfo info in _listInfo)
            //{
            //    if (uuid.Equals(info.uuid))
            //    {
            //        if (!info.isDownloading)
            //        {
            //            info.sendFile(true, fileName);
            //        }
            //        else
            //        {
            //            if (progress == 100 && current == max)
            //            {
            //                info.endDownLoad();
            //            }
            //            else
            //            {
            //                info.progressData(current, progress, max);
            //            }
            //        }
            //    }
            //}

            //this.backgroundWorker1.RunWorkerAsync();
        }

        public void updateMovieCount(int current, int max, string uuid)
        {
            foreach(ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                if (uuid.Equals(info.uuid))
                {
                    if(current != max && max != 0)
                        info.progressData(current, max);
                    else
                        info.endDownLoad();
                }
            }
            this.backgroundWorker1.RunWorkerAsync();
        }

        //public void removeItem(string uuid)
        //{
        //    for (int i = 0; i < SocketManager.getInstance().listSocket.Count; i++)
        //    {
        //        _listInfo.RemoveAt(i);
        //        break;
        //    }

        //    for (int i = 0; i < this.listView1.Items.Count; i++)
        //    {
        //        if (this.listView1.Items[i].SubItems[1].Text.Equals(uuid))
        //        {
        //            this.listView1.Items.RemoveAt(i);
        //            break;
        //        }
        //    }
        //    this.listView1.EndUpdate();

        //}

        bool isDownloading()
        {
            foreach (ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                if (info.isDownloading)
                {
                    MessageBox.Show("동영상을 가져오는 중입니다.");
                    return true;
                }
            }

            return false;
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            //녹화
            if (!isDownloading())
            {
                JObject json = new JObject();
                json.Add("id", "recode");
                SocketManager.getInstance().sendMessage(json.ToString(), null);
            }            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //중지
            if (!isDownloading())
            {
                JObject json = new JObject();
                json.Add("id", "stop");
                SocketManager.getInstance().sendMessage(json.ToString(), null);
            }            
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //영상 가져오기 
            JObject json = new JObject();
            json.Add("id", "file");
           
            foreach (ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                if (!info.isDownloading)
                {
                    info.sendFile(true);
                    SocketManager.getInstance().sendMessage(json.ToString(), null);
                }                
            }

            this.backgroundWorker1.RunWorkerAsync();            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //대표(선생용)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!Common.FTP_PATH.Equals(""))
                openFileDialog.InitialDirectory = Common.FTP_PATH;
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox4.Text = openFileDialog.FileName;
                axWindowsMediaPlayer1.URL = openFileDialog.FileName;
                axWindowsMediaPlayer1.settings.volume = 100;
                axWindowsMediaPlayer1.Ctlcontrols.stop();
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            //플레이어 추가 
            if (viewerList != null)
            {
                if (viewerList.Count >= 10)
                {
                    MessageBox.Show("최대 10개까지 추가 가능합니다.");
                }
                else
                {
                    addViewer(viewerList.Count * viewerList[0].height);
                }
            }
            else
            {
                viewerList = new List<ViewItem>();
                addViewer(0);
            }
        }

        void addViewer(int y)
        {
            ViewItem v = new ViewItem();
            panel1.Controls.Add(v);
            v.init(y);
            v.Tag = viewerList.Count + "h";
            viewerList.Add(v);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //플레이어 제거
            if (viewerList != null && viewerList.Count > 0)
            {
                for (int i = 0; i < panel1.Controls.Count; i++)
                {
                    Control ctr = panel1.Controls[i];

                    if (ctr.Tag.Equals(viewerList.Count - 1 + "h"))
                    {
                        panel1.Controls.Remove(ctr);
                        viewerList.RemoveAt(i);
                        break;
                    }
                }

            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //전체 재생
            axWindowsMediaPlayer1.Ctlcontrols.play();
            if(viewerList != null)
            {
                foreach (ViewItem item in viewerList)
                {
                    if (item.player.playState != WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        item.player.Ctlcontrols.play();
                    }
                }
            }            
        }

        private void button10_Click(object sender, EventArgs e)
        {
            //전체 정지
            axWindowsMediaPlayer1.Ctlcontrols.stop();
            if(viewerList != null)
            {
                foreach (ViewItem item in viewerList)
                {
                    if (item.player.playState != WMPLib.WMPPlayState.wmppsStopped)
                    {
                        item.player.Ctlcontrols.stop();
                    }
                }
            }            
        }

        private void button13_Click(object sender, EventArgs e)
        {
            //ftp 경로저장
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.label4.Text = dialog.SelectedPath;
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic[Common.KEY_FTP_PATH] = dialog.SelectedPath;
                Common.FTP_PATH = dialog.SelectedPath;
                saveSetting(dic);
            }
        }

        void saveSetting(Dictionary<string, string> dic)
        {
            FileInfo settingFile = new FileInfo(Common.SETTING_PATH);
            JObject json = null;

            if (settingFile.Exists)
            {
                string settingStr = File.ReadAllText(Common.SETTING_PATH);
                json = JObject.Parse(settingStr);
            }
            else
            {
                json = new JObject();                
            }

            foreach(string key in dic.Keys)
            {
                json.Add(key, dic[key]);
            }

            StreamWriter sw = new StreamWriter(Common.SETTING_PATH, false);
            sw.Write(json.ToString());
            sw.Close();
        }

        public void changeStatus(string status)
        {
            foreach (ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                info.status = status;
            }

            this.backgroundWorker1.RunWorkerAsync();
        }

        public void changeStatus(string status, string uuid)
        {
            foreach (ConnectionInfo info in SocketManager.getInstance().listSocket)
            {
                if(uuid.Equals(info.uuid))
                    info.status = status;
            }

            this.backgroundWorker1.RunWorkerAsync();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            //일시 정지 
            axWindowsMediaPlayer1.Ctlcontrols.pause();
            if (viewerList != null)
            {
                foreach (ViewItem item in viewerList)
                {
                    if (item.player.playState == WMPLib.WMPPlayState.wmppsPlaying)
                    {
                        item.player.Ctlcontrols.pause();
                    }
                }
            }            
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            AxWMPLib.AxWindowsMediaPlayer player = (AxWMPLib.AxWindowsMediaPlayer)sender;
            player.fullScreen = !player.fullScreen;            
        }
    }

}