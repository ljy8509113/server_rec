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
        Button buttonOpen;
        Label labelTitle;
        //TextBox txtTitle;
        //public int y = 0;
        public int height = 357;
        int width = 418;
        private int index;

        public OpenMovie openDelegate;
        
        
        public void init(int x, int index)
        {
            SetBounds(x, 0, width, height);
            this.index = index;
            //txtTitle = new TextBox();
            //txtTitle.SetBounds(0, 1, 493, 21);
            //Controls.Add(txtTitle);

            labelTitle = new Label();
            labelTitle.SetBounds(15, 10, width - 75 - 30, 21);
            Controls.Add(labelTitle);

            buttonOpen = new Button();
            buttonOpen.SetBounds(width - 75, 10, 75, 23);
            buttonOpen.Text = "Open";
            buttonOpen.Click += new EventHandler(button_Click);
            Controls.Add(buttonOpen);
            
            player = new AxWMPLib.AxWindowsMediaPlayer();
            int y = buttonOpen.Height + 13;
            player.SetBounds(15, y, width-15, height-y);
            player.Enabled = true;
            player.Enter += new System.EventHandler(this.axWindowsMediaPlayer1_Enter);
            Controls.Add(player);
            
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
                path = openFileDialog.FileName;
                String name = Path.GetFileName(openFileDialog.FileName);
                //this.txtTitle.Text = openFileDialog.FileName;
                labelTitle.Text = name;

                player.URL = path;
                player.settings.volume = 100;
                player.Ctlcontrols.stop();

                openDelegate(this.index, name);
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


    }
}
