using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace VideoController
{
    class LoadingView : Panel
    {
        private static LoadingView instance = null;

        Panel backPanel;
        ProgressBar progress;
        
        public static LoadingView Instance(Form f)
        {
            if(instance == null)
            {
                instance = new LoadingView();
                instance.init(f);
            }

            return instance;
        }

        void init(Form f)
        {
            this.SetBounds(0,0, f.Width, f.Height);
            backPanel = new Panel();
            progress = new ProgressBar();
            progress.Location = new Point(10, 10);
            progress.Size = new Size(100, 30);
            progress.MarqueeAnimationSpeed = 30;
            progress.Style = ProgressBarStyle.Marquee;
            
            backPanel.SetBounds(this.Bounds.X, this.Bounds.Y, this.Bounds.Width, this.Bounds.Height);
            this.Controls.Add(backPanel);
            this.Controls.Add(progress);
            f.Controls.Add(this);


        }

        public void show()
        {
            this.Visible = true;
        }

        public void hide()
        {
            this.Visible = false;
        }
    }
}
