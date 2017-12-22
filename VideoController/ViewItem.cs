using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoController
{
    class ViewItem : Panel
    {
        public AxWMPLib.AxWindowsMediaPlayer player;
        Button buttonOpen;
        TextBox txtTitle;
        public int y = 0;
        public int height = 457;
        int width = 583;
        
        public void init(int y)
        {
            SetBounds(0, y, width, height);

            txtTitle = new TextBox();
            txtTitle.SetBounds(0, 1, 493, 21);
            Controls.Add(txtTitle);

            buttonOpen = new Button();
            buttonOpen.SetBounds(width - 75, 0, 75, 23);
            buttonOpen.Text = "Open";
            buttonOpen.Click += new EventHandler(button_Click);
            Controls.Add(buttonOpen);
            
            player = new AxWMPLib.AxWindowsMediaPlayer();
            player.SetBounds(0, buttonOpen.Height + 13, width, 412);
            player.Enabled = true;
            Controls.Add(player);


        }

        private void button_Click(object sender, EventArgs e)
        {
            //open
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(!Common.FTP_PATH.Equals(""))
                openFileDialog.InitialDirectory = Common.FTP_PATH;

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtTitle.Text = openFileDialog.FileName;
            }

            player.URL = txtTitle.Text;
            player.settings.volume = 100;
            player.Ctlcontrols.stop();
        }
    }
}
