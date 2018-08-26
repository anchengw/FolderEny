using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace FolderEny
{
    public partial class Form1 : Form
    {
        OpenFileDialog SelectFileDialog;
        public Form1()
        {
            InitializeComponent();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                FolderBrowserDialog SelectFolderDialog = new FolderBrowserDialog();
                if (SelectFolderDialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = SelectFolderDialog.SelectedPath;
                    if (folder.Contains( "Documents "))
                    {
                        MessageBox.Show("不要对系统文件夹加密,可能会造成系统出问题", "提示信息",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    }
                    else if (SelectFolderDialog.SelectedPath.Length < 4)
                    {
                        MessageBox.Show("不能对盘符和根目录加密！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        textBox1.Text = SelectFolderDialog.SelectedPath;
                    }                    
                }
                else
                {
                    OpenFileDialog SelectFileDialog = new OpenFileDialog();
                    SelectFileDialog.InitialDirectory = "";
                    SelectFileDialog.Filter = "*.*";
                    SelectFileDialog.RestoreDirectory = true;
                    SelectFileDialog.Title = "请选择要加密的文件：";
                    if (SelectFolderDialog.ShowDialog() == DialogResult.OK)
                    {
                        textBox1.Text = SelectFileDialog.FileName;
                    }
                }
            }
        }
        private string setpassword(string path)
        {
            string password = "";
            EnyPassword p = new EnyPassword();
            p.path = path;
            p.ShowDialog();
            if (p.ShowDialog() == DialogResult.OK)
            {
                password = p.pass;
                p.Close();
            }
            return password;
        }
        private bool checkpassword()
        {
            XmlTextReader read = new XmlTextReader(textBox1.Text + "\\p.xml");
            if (read.ReadState == ReadState.Error)
                return true;
            else
            {
                try
                {
                    while (read.Read())
                        if (read.NodeType == XmlNodeType.Text)
                        {
                            inputPassword c = new inputPassword();
                            c.pass = read.Value;
                            if (c.ShowDialog() == DialogResult.OK)
                            {
                                read.Close();
                                return c.status;
                            }

                        }
                }
                catch { return true; }

            }
            read.Close();
            return false;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("请选择文件或文件夹路径！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string pwd =  setpassword(textBox1.Text);
            if (string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show("密码设置出错！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                EnyHelper.EncryptFolder(textBox1.Text, pwd);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string folder = textBox1.Text;
            bool s = checkpassword();
            if (s)
            {
                EnyHelper.DecryptFolder(folder);
            }
        }
    }
}

