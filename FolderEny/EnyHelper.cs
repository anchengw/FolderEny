using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace FolderEny
{
    /// <summary>
    /// 文件 文件夹加密
    /// </summary>
    public class EnyHelper
    {

        static string enytype = ".{2559a1f2-21d7-11d4-bdaf-00c04f60b9f0}";//是windows安全文件的类标识符，这时文件夹的图标就会变成一把锁
        /// <summary>
        /// 密钥，这个密码可以随便指定
        /// </summary>
        public static string sSecretKey = "?\a??64(?";

        /// <summary>
        /// 调用该函数从内存中删除的Key后使用
        /// </summary>
        [DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);


        /// <summary>
        /// 生成一个64位的密钥
        /// </summary>
        /// <returns>string</returns>
        public static string GenerateKey()
        {
            //创建对称算法的一个实例。自动生成的密钥和IV。
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            // 使用自动生成的密钥进行加密。
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }

        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="sInputFilename">待加密的文件的完整路径</param>
        /// <param name="sOutputFilename">加密后的文件的完整路径</param>
        public static void EncryptFile(string sInputFilename, string sOutputFilename)
        {
            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);

            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sSecretKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sSecretKey);
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Flush();
            fsInput.Flush();
            fsEncrypted.Flush();
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="sInputFilename">待解密的文件的完整路径</param>
        /// <param name="sOutputFilename">解密后的文件的完整路径</param>
        public static void DecryptFile(string sInputFilename, string sOutputFilename)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sSecretKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sSecretKey);

            FileStream fsread = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            CryptoStream cryptostreamDecr = new CryptoStream(fsread, desdecrypt, CryptoStreamMode.Read);
            StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }
        /// <summary>
        /// 加密文件夹
        /// </summary>
        public static string EncryptFolder(string folderPath,string pass)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            string selectedpath = d.Parent.FullName + d.Name;
            string destFolder;
            if (folderPath.LastIndexOf(".{") == -1)
            {
                XmlDocument xmldoc = new XmlDocument();
                XmlElement xmlelem;
                XmlNode xmlnode;
                XmlText xmltext;
                xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                xmldoc.AppendChild(xmlnode);
                xmlelem = xmldoc.CreateElement("", "ROOT", "");
                xmltext = xmldoc.CreateTextNode(pass);
                xmlelem.AppendChild(xmltext);
                xmldoc.AppendChild(xmlelem);
                xmldoc.Save(folderPath + "\\p.xml");
            }
            if (!d.Root.Equals(d.Parent.FullName))
                destFolder = d.Parent.FullName + "\\" + d.Name + enytype;
            else
                destFolder = d.Parent.FullName + d.Name + enytype;
            d.MoveTo(destFolder);
            setFolder(destFolder, true);
            return destFolder;
        }
        /// <summary>
        /// 解密文件夹
        /// </summary>
        public static void DecryptFolder(string folderPath)
        {
            DirectoryInfo d = new DirectoryInfo(folderPath);
            File.Delete(folderPath + "\\p.xml");
            string destFolder = folderPath.Substring(0, folderPath.LastIndexOf("."));
            d.MoveTo(destFolder);
            setFolder(destFolder, false);
        }
        /// <summary>
        /// 设置文件夹属性
        /// </summary>
        /// <param name="dirpath"></param>
        /// <returns></returns>
        public static bool setFolder(string dirpath,bool hide)
        {
            if (!Directory.Exists(dirpath))
                return false;
            DirectoryInfo dir = new DirectoryInfo(dirpath);
            if (hide)
            {                
                //dir.Attributes = FileAttributes.System | FileAttributes.Hidden | FileAttributes.ReadOnly;
                dir.Attributes |= FileAttributes.System;
                dir.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                dir.Attributes &= ~FileAttributes.System;
                dir.Attributes &= ~FileAttributes.Hidden;
                dir.Attributes &= ~FileAttributes.ReadOnly;
            }
            return true;
        }
        /// <summary>
        /// 设置文件属性
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="hide"></param>
        /// <returns></returns>
        public static bool setFile(string filepath, bool hide)
        {
            if (!File.Exists(filepath))
                return false;
            FileInfo fileInfo = new FileInfo(filepath);
            if (hide)
            {
                //File.SetAttributes(filepath, FileAttributes.Hidden & FileAttributes.System);
                fileInfo.Attributes |= FileAttributes.System;
                fileInfo.Attributes |= FileAttributes.Hidden;
            }
            else
            {
                //File.SetAttributes(filepath, FileAttributes.Normal);
                fileInfo.Attributes &= ~FileAttributes.System;
                fileInfo.Attributes &= ~FileAttributes.Hidden;
            }
            return true;
        }
        /// <summary>
        ///设置目录权限
        /// </summary>
        /// <param name="path">目录的路径。</param>
        /// <param name="permission">在目录上设置的权限。</param>
        /// <returns>指示是否在目录上应用权限的值。</returns>
        public bool SetDirectoryPermission(string path, FileSystemRights permission)
        {
            try
            {
                if (!Directory.Exists(path))
                    return false;
                //获取文件夹信息
                DirectoryInfo dir = new DirectoryInfo(path);
                //获得该文件夹的所有访问权限
                System.Security.AccessControl.DirectorySecurity dirSecurity = dir.GetAccessControl(AccessControlSections.All);
                //设定文件ACL继承
                InheritanceFlags inherits = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
                //添加ereryone用户组的访问权限规则 完全控制权限
                FileSystemAccessRule everyoneFileSystemAccessRule = new FileSystemAccessRule("Everyone", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                //添加Users用户组的访问权限规则 完全控制权限
                FileSystemAccessRule usersFileSystemAccessRule = new FileSystemAccessRule("Users", FileSystemRights.FullControl, inherits, PropagationFlags.None, AccessControlType.Allow);
                bool isModified = false;
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, everyoneFileSystemAccessRule, out isModified);
                dirSecurity.ModifyAccessRule(AccessControlModification.Add, usersFileSystemAccessRule, out isModified);
                //设置访问权限
                dir.SetAccessControl(dirSecurity);
                return true;
            }
            catch (Exception e)
            {
                //throw new Exception(e.Message, e);
                return false;
            }
            //权限追加
            //fileAcl.AddAccessRule(everyoneRule);
            //权限删除
            //fileAcl.RemoveAccessRule(everyoneRule);
            //从当前文件或目录移除所有匹配的允许或拒绝访问控制列表 (ACL) 权限。
            //fileAcl.RemoveAccessRuleAll(everyoneRule);
            //从当前文件或目录移除指定用户的所有访问控制列表 (ACL) 权限。
            //fileAcl.RemoveAccessRuleSpecific(everyoneRule);
            //从当前文件或目录移除单个匹配的允许或拒绝访问控制列表 (ACL) 权限。
        }

    }
}
