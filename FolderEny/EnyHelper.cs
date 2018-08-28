using Microsoft.Win32;
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
        /// 对字符串进行DES加密
        /// </summary>
        /// <param name="sourceString">待加密的字符串</param>
        /// <returns>加密后的BASE64编码的字符串</returns>
        public static string Encrypt(string sourceString)
        {
            byte[] btKey = Encoding.Default.GetBytes(sSecretKey);
            byte[] btIV = Encoding.Default.GetBytes(sSecretKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Encoding.Default.GetBytes(sourceString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 对DES加密后的字符串进行解密
        /// </summary>
        /// <param name="encryptedString">待解密的字符串</param>
        /// <returns>解密后的字符串</returns>
        public static string Decrypt(string encryptedString)
        {
            byte[] btKey = Encoding.Default.GetBytes(sSecretKey);
            byte[] btIV = Encoding.Default.GetBytes(sSecretKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] inData = Convert.FromBase64String(encryptedString);
                try
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(inData, 0, inData.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.Default.GetString(ms.ToArray());
                }
                catch
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="sourceFile">待加密的文件的完整路径</param>
        /// <param name="destFile">加密后的文件的完整路径</param>
        public static void EncryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            byte[] btKey = Encoding.Default.GetBytes(sSecretKey);
            byte[] btIV = Encoding.Default.GetBytes(sSecretKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateEncryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
        }

        /// <summary>
        /// 解密文件
        /// </summary>
        /// <param name="sourceFile">待解密的文件的完整路径</param>
        /// <param name="destFile">解密后的文件的完整路径</param>
        public static void DecryptFile(string sourceFile, string destFile)
        {
            if (!File.Exists(sourceFile)) throw new FileNotFoundException("指定的文件路径不存在！", sourceFile);
            byte[] btKey = Encoding.Default.GetBytes(sSecretKey);
            byte[] btIV = Encoding.Default.GetBytes(sSecretKey);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] btFile = File.ReadAllBytes(sourceFile);
            using (FileStream fs = new FileStream(destFile, FileMode.Create, FileAccess.Write))
            {
                try
                {
                    using (CryptoStream cs = new CryptoStream(fs, des.CreateDecryptor(btKey, btIV), CryptoStreamMode.Write))
                    {
                        cs.Write(btFile, 0, btFile.Length);
                        cs.FlushFinalBlock();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    fs.Close();
                }
            }
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
            //sethide();设置注册表隐藏
            return destFolder;
        }
        /// <summary>
        /// 判断路径字符是否非法
        /// </summary>
        /// <param name="pathStr">表示路径的字符串</param>
        /// <returns>true 为有效 false无效</returns>
        public static bool isPathStr(string pathStr)
        {
            //Path.GetInvalidPathChars
            //Path.GetInvalidFileNameChars
            int index = pathStr.IndexOfAny(System.IO.Path.GetInvalidPathChars());
            return index == -1;
        }
        /// <summary>
        /// 解密文件夹
        /// </summary>
        public static void DecryptFolder(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;
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
        /// <summary>
        /// 设置WINDOWS不能查看隐藏文件和系统文件
        /// </summary>
        public static void sethide()
        {
            //ClassesRoot -->注册表基项HKEY_CLASSES_ROOT --> 包含了所有应用程序运行时必需的信息
            //CurrentUser -->注册表基项 HKEY_CURRENT_USER -->管理系统当前的用户信息
            //Users -->注册表基项HKEY_USERS -->仅包含了缺省用户设置和登录用户的信息
            //LocalMachine -->注册表基项HKEY_LOCAL_MACHINE -->保存了注册表里的所有与这台计算机有关的配置信息
            //CurrentConfig -->注册表基项 HKEY_CURRENT_CONFIG -->允许软件和设备驱动程序员很方便的更新注册表，而不涉及到多个配置文件信息
            //1表示选中 2的意思是随着另一个的选中而变化 把NOHIDDEN的CheckedValue值改成2，把SHOWALL的CheckedValue 值改成0，这样不管以后在面板里怎么调，始终都不会显示隐藏的文件和文件夹了
            RegistryKey hklm = Microsoft.Win32.Registry.LocalMachine;
            RegistryKey rkvalue = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Folder\Hidden\SHOWALL", true);
            rkvalue.SetValue("CheckedValue", 0);
            rkvalue.SetValue("DefaultValue", 0);
            rkvalue = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Folder\Hidden\NOHIDDEN", true);
            rkvalue.SetValue("CheckedValue", 0);
            rkvalue.SetValue("DefaultValue", 0);
            rkvalue = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Folder\SuperHidden", true);
            rkvalue.SetValue("CheckedValue", 1);
            rkvalue.SetValue("DefaultValue", 1);
            RegistryKey hkcu = Microsoft.Win32.Registry.CurrentUser;
            RegistryKey rkv = hkcu.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", true);
            rkv.SetValue("Hidden", 0);
            rkv.SetValue("ShowSuperHidden", 1);

        }
    }
}
