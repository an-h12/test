using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TalkBackAutoTest
{
    public partial class PLMForm : Form
    {
        public PLMForm()
        {
            InitializeComponent();
        }

        private void PLMForm_Load(object sender, EventArgs e)
        {
            txt_title_plm.Text = MainForm.SetValueForTitle;
            txt_body.Text = MainForm.SetValueForBody;
        }
    }
}
