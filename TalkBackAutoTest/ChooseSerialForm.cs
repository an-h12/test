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
    public partial class ChooseSerialForm : Form
    {
        public string serial { get; set; } 

        public ChooseSerialForm()
        {
            InitializeComponent();
        }

        private void ChooseSerialForm_Load(object sender, EventArgs e)
        {
            foreach (string s in MainForm.listSerial)
            {
                comboBox1.Items.Add(s);
                comboBox1.Text = s;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.serial = comboBox1.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
