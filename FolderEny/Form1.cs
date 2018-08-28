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
                    if (folder.Contains("Documents "))
                    {
                        MessageBox.Show("不要对系统文件夹加密,可能会造成系统出问题", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (SelectFolderDialog.SelectedPath.Length < 4)
                    {
                        MessageBox.Show("不能对盘符和根目录加密！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        textBox1.Text = SelectFolderDialog.SelectedPath;
                        button1.Enabled = true;
                    }
                }
            }
            else
            {
                OpenFileDialog SelectFileDialog = new OpenFileDialog();
                SelectFileDialog.InitialDirectory = "";
                SelectFileDialog.Filter = "所有文件(*.*)|*.*"; 
                SelectFileDialog.RestoreDirectory = true;
                SelectFileDialog.Title = "请选择要加密的文件：";
                if (SelectFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = SelectFileDialog.FileName;
                    button1.Enabled = true;
                }
            }
        }
        private string setpassword(string path)
        {
            string password = "";
            EnyPassword p = new EnyPassword();
            p.path = path;
            if (p.ShowDialog() == DialogResult.OK)
            {
                password = p.pass;
                p.Close();
            }
            return password;
        }
        private bool checkpassword(string strPath)
        {
            XmlTextReader read = new XmlTextReader(strPath + "\\p.xml");
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
                            if (c.ShowDialog()==DialogResult.OK)
                            {
                                read.Close();
                                return c.status;
                            }
                        }
                }
                catch { return true; }
            }
            read.Close();
            return true;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("请选择文件或文件夹路径！", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (radioButton1.Checked)//文件夹加密
            {
                string pwd = setpassword(textBox1.Text);
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
            else //文件加密
            {
                string outPath = Application.StartupPath + "\\encryFile\\";
                if (!Directory.Exists(outPath))
                    Directory.CreateDirectory(outPath);
                string outfile = outPath + Path.GetFileName(textBox1.Text);
                EnyHelper.EncryptFile(textBox1.Text, outfile);
                listViewAdd(outfile, radioButton1.Checked);
            }
            SerializeListViewItems(listView1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count <= 0)
                return;
            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                ListViewItem item = listView1.SelectedItems[i];
                if (item.SubItems[1].Text.Equals("文件夹"))
                {
                    bool s = checkpassword(item.Name);
                    if (s)
                    {
                        EnyHelper.DecryptFolder(item.Name);
                        listView1.Items.Remove(item);
                    }
                }
                else
                {
                    SaveFileDialog saveFile = new SaveFileDialog();
                    saveFile.Filter = "所有文件(*.*)|*.*";
                    saveFile.DefaultExt = Path.GetExtension(item.Name);//设置默认格式
                    saveFile.AddExtension = true;//设置自动在文件名中添加扩展名
                    if (saveFile.ShowDialog() == DialogResult.OK)
                    {
                        EnyHelper.DecryptFile(item.Name,saveFile.FileName);
                        listView1.Items.Remove(item);
                    }
                }
            }
            SerializeListViewItems(listView1);
        }
        private void listViewAdd(string filename, bool isfolder)
        {

            if (isfolder)
            {
                DirectoryInfo dirinfo = new DirectoryInfo(filename);
                ListViewItem dirItem = listView1.Items.Add(dirinfo.Name.Split('.')[0], 2);
                dirItem.Name = dirinfo.FullName;
                dirItem.SubItems.Add("文件夹");
                dirItem.SubItems.Add(dirinfo.FullName.Split('.')[0]);
                dirItem.SubItems.Add("");
                dirItem.SubItems.Add(dirinfo.LastWriteTimeUtc.ToString());
                dirItem.SubItems.Add("");
            }
            else
            {
                FileInfo file = new FileInfo(filename);
                ListViewItem fileItem = listView1.Items.Add(file.Name);
                if (file.Extension == ".exe" || file.Extension == "")   //程序文件或无扩展名
                {
                    Icon fileIcon = GetSystemIcon.GetIconByFileName(file.FullName);
                    imageList1.Images.Add(file.Name, fileIcon);
                    fileItem.ImageKey = file.Name;
                }
                else    //其它文件
                {
                    if (!imageList1.Images.ContainsKey(file.Extension))  //ImageList中不存在此类图标
                    {
                        Icon fileIcon = GetSystemIcon.GetIconByFileName(file.FullName);
                        imageList1.Images.Add(file.Extension, fileIcon);
                    }
                    fileItem.ImageKey = file.Extension;
                }
                fileItem.Name = file.FullName;
                fileItem.SubItems.Add("文件");
                fileItem.SubItems.Add(file.FullName);
                fileItem.SubItems.Add(file.Length.ToString() + "字节");              
                fileItem.SubItems.Add(file.LastWriteTimeUtc.ToString());
                fileItem.SubItems.Add(file.Extension);
            }

        }
        //序列化写入文件 Tag 和Name属性不能保存
        public void SerializeListViewItems(ListView listView)
        {
            string stFilePath = Application.StartupPath.Trim() + "\\fileinfo.dat";
            FileStream fs = new FileStream(stFilePath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.Default);
            var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            foreach (ListViewItem item in listView.Items)
            {
                item.SubItems[2].Text = item.Name;
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
        //反序列化读入文件 Tag 和Name属性丢失
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
                        ListViewItem item = serializer.Deserialize(ms) as ListViewItem;
                        item.Name = item.SubItems[2].Text;
                        if (item.SubItems[1].Text.Equals("文件夹"))
                            item.SubItems[2].Text = item.Name.Split('.')[0];                      
                        yield return item;
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
            button1.Enabled = false;
            button3.Enabled = false;
            initListview();
        }
        private void initListview()
        {
            this.listView1.View = View.Details;
            this.listView1.SmallImageList = this.imageList1;  //将listView的图标集与imageList1绑定
            ColumnHeader ch = new ColumnHeader();
            ch.Text = "名称";   //设置列标题
            ch.Width = 120;    //设置列宽度
            ch.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch);    //将列头添加到ListView控件。
            ColumnHeader ch1 = new ColumnHeader();
            ch1.Text = "类型";   //设置列标题
            ch1.Width = 50;    //设置列宽度
            ch1.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch1);    //将列头添加到ListView控件。
            ColumnHeader ch2 = new ColumnHeader();
            ch2.Text = "路径";   //设置列标题
            ch2.Width = 250;    //设置列宽度
            ch2.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch2);    //将列头添加到ListView控件。
            ColumnHeader ch3 = new ColumnHeader();
            ch3.Text = "大小";   //设置列标题
            ch3.Width = 120;    //设置列宽度
            ch3.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch3);    //将列头添加到ListView控件。
            ColumnHeader ch4 = new ColumnHeader();
            ch4.Text = "创建时间";   //设置列标题
            ch4.Width = 120;    //设置列宽度
            ch4.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch4);    //将列头添加到ListView控件。
            ColumnHeader ch5 = new ColumnHeader();
            ch5.Text = "备注";   //设置列标题
            ch5.Width = 120;    //设置列宽度
            ch5.TextAlign = HorizontalAlignment.Center;   //设置列的对齐方式
            this.listView1.Columns.Add(ch5);    //将列头添加到ListView控件。
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            SerializeListViewItems(listView1);
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                button3.Enabled = true;
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    listView1.SelectedItems[i].Checked = true;
                }
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            //获取选中数目要用：m = listView1.CheckedItems.Count;而不是listView1.SelectedItems.Count; 
            e.Item.Selected = e.Item.Checked;//选中复选框，也就选中项
            button3.Enabled = true;
        }
    }
}

