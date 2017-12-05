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

namespace VideoController
{
    public partial class Form1 : Form
    {
        string port = "";
        string settingPath = "./setting.txt";
        bool isOpenSocket = false;

        public Form1()
        {
            InitializeComponent();

            this.listView1.Columns.Add("디바이스 이름", 200);
            this.listView1.Columns.Add("상태", 100);

            FileInfo protFile = new FileInfo(settingPath);
            if (protFile.Exists)
            {
                this.port = File.ReadAllText(settingPath);
                textBox2.Text = this.port;
                openSocket();
            }
            else
            {
                MessageBox.Show("포트 설정이 필요합니다.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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
            string port = textBox2.Text;
            int num = 0;

            if(!int.TryParse(port, out num))
            {
                MessageBox.Show("숫자만 입력 가능합니다.");
                return;
            }

            StreamWriter sw = new StreamWriter(settingPath, false);
            sw.Write(port);
            this.port = port;
            sw.Close();

            if(this.isOpenSocket == false)
            {
                openSocket();
            }
            
        }

        void openSocket()
        {
            SocketManager.getInstance().init(Int32.Parse(this.port));
            this.label4.Text = SocketManager.getInstance().GetLocalIP();
        }
    }
}
