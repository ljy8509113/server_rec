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
        string port = "0";
        string ip = "";
        string settingPath = "./setting.txt";

        public Form1()
        {
            InitializeComponent();

            this.listView1.Columns.Add("디바이스 이름", 200);
            this.listView1.Columns.Add("UUID", 200);
            this.listView1.Columns.Add("User", 50);
            this.listView1.Columns.Add("상태", 200);

            FileInfo protFile = new FileInfo(settingPath);
            if (protFile.Exists)
            {
                string settingStr = File.ReadAllText(settingPath);
                JObject obj = JObject.Parse(settingStr);

                Console.WriteLine("json : {0}", obj);
                this.ip = obj["ip"].ToString();
                this.port = obj["port"].ToString();

                textBox3.Text = this.ip;
                textBox2.Text = this.port;

                button6.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;

                openSocket();
            }
            else
            {
                MessageBox.Show("포트 설정이 필요합니다.");
            }

            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);

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
            axWindowsMediaPlayer1.Ctlcontrols.play();

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

            JObject json = new JObject();
            json.Add("ip", ipAddr);
            json.Add("port", port);

            StreamWriter sw = new StreamWriter(settingPath, false);
            sw.Write(json.ToString());
            this.port = port;
            this.ip = ipAddr;
            sw.Close();

            if (SocketManager.getInstance()._isOn == false)
            {
                button6.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;

                openSocket();
            }
            else
            {
                SocketManager.getInstance().onEnd();
                Application.Restart();
            }
        }

        void openSocket()
        {
            SocketManager.getInstance().init(this.ip.ToString(), Int32.Parse(this.port));
            SocketManager.getInstance().f = this;
            //this.label4.Text = SocketManager.getInstance().GetLocalIP();
        }

        private BackgroundWorker backgroundWorker1;
        List<ConnectionInfo> _listInfo = new List<ConnectionInfo>();

        public void addConnectionInfo(List<ConnectionInfo> list)
        {
            _listInfo = list;
            this.backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach (ConnectionInfo info in _listInfo)
            {
                bool isUsed = false;
                foreach (ListViewItem i in this.listView1.Items)
                {
                    if (i.SubItems[1].Text.Equals(info.uuid))
                    {
                        isUsed = true;
                        if (info.isDownloading)
                        {
                            i.SubItems[3].Text = info.getDownMsg();
                        }
                        else
                        {
                            i.SubItems[0].Text = info.name;
                            i.SubItems[1].Text = info.uuid;
                            i.SubItems[2].Text = (info.isTeacher == true ? "선생님" : "학생");
                            i.SubItems[3].Text = "";//(info.socket.Connected == true ? "연결" : "해제");
                        }
                        break;
                    }
                }

                if (!isUsed)
                {
                    ListViewItem item = new ListViewItem(info.name);
                    item.SubItems.Add(info.uuid);
                    item.SubItems.Add((info.isTeacher == true ? "선생님" : "학생"));
                    item.SubItems.Add("");//item.SubItems.Add((info.socket.Connected == true ? "연결" : "해제"));
                    AddItem(item);

                }
            }
            this.listView1.EndUpdate();
        }

        delegate void AddListCallback(ListViewItem item);

        private void AddItem(ListViewItem item)
        {
            if (this.listView1.InvokeRequired)
            {
                AddListCallback d = new AddListCallback(AddItem);
                this.Invoke(d, new object[] { item });
            }
            else
            {
                this.listView1.Items.Add(item);
            }
        }

        public void updateData(string name, string uuid, bool isTeacher, Socket s)
        {
            foreach (ConnectionInfo info in _listInfo)
            {
                if (uuid.Equals(info.uuid))
                {
                    info.setData(name, uuid, isTeacher);
                    //info.socket = s;
                }
            }
            this.backgroundWorker1.RunWorkerAsync();

        }
        
        public void updateProgress(string uuid, int progress, int current, int max, string fileName)
        {
            foreach(ConnectionInfo info in _listInfo)
            {
                if (uuid.Equals(info.uuid))
                {
                    if (!info.isDownloading)
                    {
                        info.sendFile(true, fileName);
                    }
                    else
                    {
                        if (progress == 100 && current == max)
                        {
                            info.endDownLoad();
                        }
                        else
                        {
                            info.progressData(current, progress, max);
                        }
                    }
                }
            }
            
            this.backgroundWorker1.RunWorkerAsync();
        }
        
        public void removeItem(string uuid)
        {
            for(int i=0; i<_listInfo.Count; i++)
            {
                _listInfo.RemoveAt(i);
                break;
            }

            for (int i=0; i< this.listView1.Items.Count; i++)
            {
                if (this.listView1.Items[i].SubItems[1].Text.Equals(uuid))
                {
                    this.listView1.Items.RemoveAt(i);
                    break;
                }
            }
            this.listView1.EndUpdate();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //녹화
            JObject json = new JObject();
            json.Add("id", "recode");
            SocketManager.getInstance().sendMessage(json.ToString(), null, false);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //중지
            JObject json = new JObject();
            json.Add("id", "stop");
            SocketManager.getInstance().sendMessage(json.ToString(), null, false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //영상 가져오기 
            JObject json = new JObject();
            json.Add("id", "file");
            SocketManager.getInstance().sendMessage(json.ToString(), null, true);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //대표(선생용)
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox4.Text = openFileDialog.FileName;
            }
        }
    }
    
}
