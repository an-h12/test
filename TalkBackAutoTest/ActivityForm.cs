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
    public partial class ActivityForm : Form
    {
        private MainForm mainform;

        bool GLOBAL_STATE = true;

        List<AObject> listAObjects = new List<AObject>();

        public ActivityForm(MainForm main)
        {
            InitializeComponent();
            mainform = main;
        }
       

        private void ActivityForm_Load(object sender, EventArgs e)
        {
            
            string path = mainform.WORKSPACE + "\\list_activity.txt";
            //MessageBox.Show(path);
            string[] lines = File.ReadAllLines(@path);
            int dem = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                listAObjects.Add(new AObject(++dem,true,true,lines[i]));
            }

            refreshLV();

            this.ActiveControl = label1;
            updateInfor();
        }

        private void refreshLV()
        {
            lvActivity.Items.Clear();

            for (int i = 0; i < listAObjects.Count; i++)
            {
                try
                {
                    
                    AObject x = listAObjects[i];
                    if (x.isShow == true)
                    {
                        ListViewItem item1 = new ListViewItem();
                        item1.Checked = x.isChecked;
                        item1.SubItems.Add(x.no.ToString());
                        item1.SubItems.Add(x.activityName);
                        lvActivity.Items.AddRange(new ListViewItem[] { item1 });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("readXmlInforSeach:" + ex.Message);
                }
            }
        }

        private void updateInfor()
        {
            int numberOfActivites = 0;
            int numberOfCheckedActivites = 0;
            foreach (AObject x in listAObjects)
            {
                if (x.isChecked == true)
                {
                    numberOfCheckedActivites++;
                }
                numberOfActivites++;
            }
            //string  numberOfActivites = lvActivity.Items.Count.ToString();
            //string numberOfCheckedActivites = lvActivity.CheckedItems.Count.ToString();
            lbInfo.Text = numberOfCheckedActivites + "/" + numberOfActivites;
        }

        private void ActivityForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            saveFile();
        }

        private void saveFile()
        {

            string content = "";
            foreach (AObject x in listAObjects)
            {
                if (x.isChecked == true)
                {
                    content += x.activityName + "\r\n";
                }
            }

            string path = mainform.WORKSPACE + "\\list_activity.txt";
            using (StreamWriter writer1 =
                        new StreamWriter(path))
            {
                writer1.Write(content);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GLOBAL_STATE = !GLOBAL_STATE;
            foreach (ListViewItem item in lvActivity.Items)
            {
                item.Checked = GLOBAL_STATE;

                int idx = Int16.Parse(item.SubItems[1].Text.ToString());
                listAObjects[idx - 1].isChecked = GLOBAL_STATE;
            }
            updateInfor();
        }

        private void lvActivity_MouseClick(object sender, MouseEventArgs e)
        {
            ListViewItem lvi = lvActivity.GetItemAt(e.X, e.Y);
            if (e.X > 20)
            {
                bool status = !lvi.Checked;
                lvi.Checked = status;
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

            string keyword = textBox1.Text;
            if (keyword == "")
            {
                for (int i = 0; i < listAObjects.Count; i++)
                {
                    listAObjects[i].isShow = true;
                }
                refreshLV();
            }
            else
            {
                for (int i = 0; i < listAObjects.Count; i++)
                {
                    if (listAObjects[i].activityName.ToLower().Contains(keyword))
                    {
                        listAObjects[i].isShow = true;
                    }
                    else
                    {
                        listAObjects[i].isShow = false;
                    }
                    
                }
                refreshLV();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //saveFile();
            this.Close();
        }

        private void lvActivity_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int a = 5;
            //ListViewItem lvi = e.Item;

            //bool status = !lvi.Checked;

            //lvi.Checked = status;

            //int idx = Int16.Parse(lvi.SubItems[1].Text.ToString());
            //listAObjects[idx - 1].isChecked = status;

            //updateInfor();
        }

        private void lvActivity_ItemChecked_1(object sender, ItemCheckedEventArgs e)
        {
            int idx = Int16.Parse(e.Item.SubItems[1].Text.ToString());
            listAObjects[idx - 1].isChecked = e.Item.Checked;

            updateInfor();
        }
    }
}
