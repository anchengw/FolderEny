using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FolderEny
{
    public partial class inputPassword : Form
    {
        public string pass;
        public bool status;
        public inputPassword()
        {
            InitializeComponent();
            status = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals(pass))
            {
                status = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("密码错误，解密终止！！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                status = false;
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
