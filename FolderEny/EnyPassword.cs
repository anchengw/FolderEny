using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace FolderEny
{
    public partial class EnyPassword : Form
    {
        public string path;
        public string pass;
        public EnyPassword()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Trim()) && textBox1.Text.Equals(textBox2.Text))
            {
                pass = textBox1.Text.Trim();
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("两次输入的密码不一致,请重新输入！", "错误");
                textBox1.Clear();
                textBox2.Clear();
                textBox1.Focus();
            }
        }
    }
    }

