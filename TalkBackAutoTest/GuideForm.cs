using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TalkBackAutoTest
{
    public partial class GuideForm : Form
    {
        public GuideForm()
        {
            InitializeComponent();
        }

        string IMAGEPATH = Path.GetDirectoryName(Application.ExecutablePath).ToString();

        string[] imageLists = { "anh0.jpg","anh1.jpg", "anh2.jpg", "anh3.jpg" };
        string[] imageSubs = { "1. First time, need manualy skip popups to turn on TB before using tool","2. Settings app > Accessibility > TalkBack > Settings > Verbosity\r\nChoose a preset: Low or Custom\r\nTurn off all verbosity details\r\nOnly toggle ON: Speak element type", "3. Set None/Nothing for keyboard option","4. TalkBack > Settings > Advanced settings > Developer settings\r\nDisplay speech output: ON\r\nLog output level : Verbose" };

        int idx = 0;

        //PC
        string[] imageLists2 = { "anh0.jpg", "anh1.jpg", "anh2.jpg", "anh3.jpg" };
        string[] imageSubs2 = { "1. First time, need manualy skip popups to turn on TB before using tool", "2. Settings app > Accessibility > TalkBack > Settings > Verbosity\r\nChoose a preset: Low or Custom\r\nTurn off all verbosity details\r\nOnly toggle ON: Speak element type", "3. Set None/Nothing for keyboard option", "4. TalkBack > Settings > Advanced settings > Developer settings\r\nDisplay speech output: ON\r\nLog output level : Verbose" };

        int idx2 = 0;

        private void GuideForm_Load(object sender, EventArgs e)
        {
            setContent(0);
            setContent2(0);
            
        }

        private void setContent(int idx)
        {
            try
            {
                Image image = Image.FromFile(@IMAGEPATH + "/" + imageLists[idx]);
                this.pbImage.Image = image;
                this.textBox1.Text = imageSubs[idx];
            }
            catch
            {

            }
            if (idx <= 0)
            {
                btnPrevious.Enabled = false;
                btnNext.Enabled = true;
            }
            else if (idx >= imageLists.Length - 1)
            {
                btnPrevious.Enabled = true;
                btnNext.Enabled = false;
            }
            else
            {
                btnPrevious.Enabled = true;
                btnNext.Enabled = true;
            }
        }

        private void setContent2(int idx2)
        {
            try
            {
                Image image2 = Image.FromFile(@IMAGEPATH + "/" + imageLists2[idx2]);
                this.pbImage2.Image = image2;
                this.textBox2.Text = imageSubs2[idx2];
            }
            catch
            {

            }
            if (idx2 <= 0)
            {
                btnPrevious2.Enabled = false;
                btnNext2.Enabled = true;
            }
            else if (idx2 >= imageLists2.Length - 1)
            {
                btnPrevious2.Enabled = true;
                btnNext2.Enabled = false;
            }
            else
            {
                btnPrevious2.Enabled = true;
                btnNext2.Enabled = true;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            idx++;
            if (idx == imageLists.Length)
            {
                idx = 0;
            }
            else if (idx < 0)
            {
                idx = imageLists.Length - 1;
            }

            setContent(idx);
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            idx--;
            if (idx == imageLists.Length)
            {
                idx = 0;
            }
            else if (idx < 0)
            {
                idx = imageLists.Length - 1;
            }

            setContent(idx);
        }


        private void btnNext2_Click(object sender, EventArgs e)
        {
            idx2++;
            if (idx2 == imageLists2.Length)
            {
                idx2 = 0;
            }
            else if (idx2 < 0)
            {
                idx2 = imageLists2.Length - 1;
            }

            setContent2(idx2);
        }

        private void btnPrevious2_Click(object sender, EventArgs e)
        {
            idx2--;
            if (idx2 == imageLists2.Length)
            {
                idx2 = 0;
            }
            else if (idx2 < 0)
            {
                idx2 = imageLists2.Length - 1;
            }

            setContent2(idx2);
        }
    }
}
