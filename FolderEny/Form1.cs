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
            }
            else
            {
                OpenFileDialog SelectFileDialog = new OpenFileDialog();
                SelectFileDialog.InitialDirectory = "";
                SelectFileDialog.Filter = "*.*";
                SelectFileDialog.RestoreDirectory = true;
                SelectFileDialog.Title = "请选择要加密的文件：";
                if (SelectFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = SelectFileDialog.FileName;
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
                string file = EnyHelper.EncryptFolder(textBox1.Text, pwd);
                listViewAdd(file, radioButton1.Checked);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool s = checkpassword();
            if (s)
            {
                EnyHelper.DecryptFolder(textBox1.Text.Trim());
            }
        }
        private void listViewAdd(string filename , bool isfolder)
        {

            if (isfolder)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(filename);
                ListViewItem dirItem = listView1.Items.Add(dirinfo.Name, 2);
                dirItem.Name = dirinfo.FullName;
                dirItem.SubItems.Add("");
                dirItem.SubItems.Add("文件夹");
                dirItem.SubItems.Add(dirinfo.LastWriteTimeUtc.ToString());
            }
            else
            {
                FileInfo file = new FileInfo(filename);
                ListViewItem fileItem = listView1.Items.Add(file.Name);
                if (file.Extension == ".exe" || file.Extension == "")   //程序文件或无扩展名
                {
                    Icon fileIcon = GetSystemIcon.GetIconByFileName(file.FullName);
                    imageList1.Images.Add(file.Name, fileIcon);
                    imageList1.Images.Add(file.Name, fileIcon);
                    fileItem.ImageKey = file.Name;
                }
                else    //其它文件
                {
                    if (!imageList1.Images.ContainsKey(file.Extension))  //ImageList中不存在此类图标
                    {
                        Icon fileIcon = GetSystemIcon.GetIconByFileName(file.FullName);
                        imageList1.Images.Add(file.Extension, fileIcon);
                        imageList1.Images.Add(file.Extension, fileIcon);
                    }
                    fileItem.ImageKey = file.Extension;
                }
                fileItem.Name = file.FullName;
                fileItem.SubItems.Add(file.Length.ToString() + "字节");
                fileItem.SubItems.Add(file.Extension);
                fileItem.SubItems.Add(file.LastWriteTimeUtc.ToString());
            }

        }
        //序列化写入文件
        public void SerializeListViewItems(ListView listView)
        {
            string stFilePath = Application.StartupPath.Trim() + "\\fileinfo.dat";
            FileStream fs = new FileStream(stFilePath, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            foreach (var item in listView.Items)
            {
                using (var ms = new MemoryStream())
                {
                    serializer.Serialize(ms, item);
                    sw.WriteLine(Convert.ToBase64String(ms.ToArray()));
                }
            }
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        //反序列化读入文件
        public IEnumerable<ListViewItem> DeserializeListViewItems()
        {
            string stFilePath = Application.StartupPath.Trim() + "\\fileinfo.dat";
            using (StreamReader sr = new StreamReader(stFilePath, Encoding.Default))
            {
                String line;
                var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                while ((line = sr.ReadLine()) != null)
                {
                    using (var ms = new MemoryStream(Convert.FromBase64String(line)))
                    {
                        yield return serializer.Deserialize(ms) as ListViewItem;
                    };
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string stFilePath = Application.StartupPath.Trim() + "\\fileinfo.dat";
            if (File.Exists(stFilePath))
            {
                foreach (var item in DeserializeListViewItems())
                {
                    if (item != null)
                        listView1.Items.Add(item);
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SerializeListViewItems(listView1);
        }
    }
}

