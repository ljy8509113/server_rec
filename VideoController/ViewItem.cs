using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace VideoController
{
    class ViewItem : Panel
    {
        public delegate void OpenMovie(int index, String name);
        public AxWMPLib.AxWindowsMediaPlayer player;
        //Button buttonOpen;
        Label labelTitle;
        //TextBox txtTitle;
        //public int y = 0;
        public int height = 357;
        int width = 418;
        private int index;
        List<string> arrayPath = new List<string>();

        public OpenMovie openDelegate;
        ListView listView;
        
        public void init(int x, int height, int index, List<string> arrayPath)
        {
            if(arrayPath != null && arrayPath.Count > 0)
                this.arrayPath = arrayPath;
            SetBounds(x, 0, width, height);
            this.index = index;
            //txtTitle = new TextBox();
            //txtTitle.SetBounds(0, 1, 493, 21);
            //Controls.Add(txtTitle);

            labelTitle = new Label();
            labelTitle.SetBounds(15, 15, width - 75 - 30, 21);
            Controls.Add(labelTitle);

            //buttonOpen = new Button();
            //buttonOpen.SetBounds(width - 75, 10, 75, 23);
            //buttonOpen.Text = "Open";
            //buttonOpen.Click += new EventHandler(button_Click);
            //Controls.Add(buttonOpen);
            
            player = new AxWMPLib.AxWindowsMediaPlayer();
            int y = labelTitle.Height + labelTitle.Bounds.Y;//buttonOpen.Height + 13;
            player.SetBounds(15, y, width-15, this.height-y);
            player.Enabled = true;
            player.Enter += new System.EventHandler(this.axWindowsMediaPlayer1_Enter);
            Controls.Add(player);

            listView = new ListView();
            y = player.Bounds.Y + player.Bounds.Height + 5;
            listView.SetBounds(15, y, width - 15, height - y);
            listView.View = View.Details;
            listView.GridLines = true;
            listView.DoubleClick += new System.EventHandler(this.lstAddress_MouseDoubleClick);
            listView.Columns.Add("동영상 리스트", listView.Bounds.Y - 5);
            Controls.Add(listView);

            //if (!path.Equals(""))
            //{
            //    openFile(this.path);
            //}

            if (this.arrayPath.Count > 0)
            {
                listView.BeginUpdate();
                foreach(string s in this.arrayPath)
                {
                    ListViewItem item = new ListViewItem(s);
                    item.Tag = s;
                    listView.Items.Add(item);
                }
                listView.EndUpdate();
                openFile(this.arrayPath[0]);
            }
                
            
        }

        void openFile(string path)
        {
            //this.path = path;
            String name = Path.GetFileName(Path.GetFileName(path));
            //this.txtTitle.Text = openFileDialog.FileName;
            labelTitle.Text = name;

            player.URL = path;
            player.settings.volume = 100;
            player.Ctlcontrols.stop();

            if(openDelegate != null)
                openDelegate(this.index, name);
        }

        private void button_Click(object sender, EventArgs e)
        {
            //open
            String path = "";
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (!Common.FTP_PATH.Equals(""))
                openFileDialog.InitialDirectory = Common.FTP_PATH;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                openFile(openFileDialog.FileName);
            }

            //player.URL = txtTitle.Text;
            
        }

        private void axWindowsMediaPlayer1_Enter(object sender, EventArgs e)
        {
            AxWMPLib.AxWindowsMediaPlayer p = (AxWMPLib.AxWindowsMediaPlayer)sender;
            if (!p.fullScreen)
                p.fullScreen = true;
        }

        public int getIndex()
        {
            return this.index;
        }

        private void lstAddress_MouseDoubleClick(object sender, EventArgs e)
        {
            if (listView.SelectedItems.Count == 1)
            {
                ListView.SelectedListViewItemCollection items = listView.SelectedItems;
                ListViewItem lvItem = items[0];
                openFile(items[0].Tag.ToString());
                player.Ctlcontrols.play();
                Console.WriteLine("in");
            }
        }

        public void remove(Panel p)
        {
            player.Ctlcontrols.stop();
            listView.DoubleClick -= new System.EventHandler(this.lstAddress_MouseDoubleClick);
            player.Enter -= new System.EventHandler(this.axWindowsMediaPlayer1_Enter);
            p.Controls.Remove(this);
            this.Dispose();
        }
        
    }
}
