// ============================================================
// 文件：FormLogin.cs
// 模块：UIForm（界面层）
// 说明：登录与用户管理窗体。从 XOR 加密的 UserData.txt 读取用户列表，
//       完成登录校验、添加/删除用户与权限切换，并按权限控制系统设置页的可用范围。
// ============================================================
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SmartVisionSystem.VisionCore;
using Sunny.UI;
using System.IO;
using System.Security.Cryptography;

namespace SmartVisionSystem.UIForm.Pages
{
    /// <summary>
    /// 登录窗体：负责用户名/密码校验与用户管理（添加、删除用户）；
    /// 用户数据以 XOR 加密（密钥 123456）按行保存在 UserData.txt 中。
    /// </summary>
    public partial class FormLogin : Sunny.UI.UIForm
    {
        /// <summary>设计器所需的无参构造函数（未做任何初始化）。</summary>
        public FormLogin() { }
        // 从用户数据文件加载的全部用户列表（用户名/密码/权限等级）
        List<User> mUsers = new List<User>();
        // 用户数据文件路径：..\Configurations\UserData.txt（每行一名用户，XOR 加密存储）
        internal string UserDataPath = Path.GetFullPath("..") + "\\Configurations\\UserData.txt";
        // 主窗体引用，用于回写当前登录用户与刷新主界面显示
        private MainForm mMain;
        /// <summary>
        /// 构造函数：读取 UserData.txt 并逐行解密，把用户加载到内存列表与
        /// 用户名下拉框，最后重新加密写回文件（保证磁盘上始终为密文）。
        /// </summary>
        /// <param name="main">主窗体引用</param>
        public FormLogin(MainForm main)
        {
            InitializeComponent();
            // 登录模式下的窗体高度（用户管理模式会加高以显示权限下拉框）
            this.Height = 257;
            mMain = main;
            if (File.Exists(UserDataPath))
            {
                // 读取全部用户记录（每行一名用户，密文存储）
                string[] UserData = File.ReadAllLines(UserDataPath);
                // 逐行 XOR 解密（密钥 123456），还原为明文“用户名,密码,权限”格式
                for (int i = 0; i < UserData.Length; i++)
                {
                    UserData[i] = TextDecrypt(UserData[i].ToCharArray(), "123456");
                }
                // 解析明文构建用户对象，同时把用户名加入身份下拉框
                for (int i = 0; i < UserData.Length; i++)
                {
                    string[] user = UserData[i].Split(",");
                    mUsers.Add(new User(user[0], user[1], int.Parse(user[2])));
                    CbIdentitySelect.Items.Add(user[0]);
                }
                // 重新加密并整体写回文件，避免明文落盘
                for (int i = 0; i < UserData.Length; i++)
                {
                    UserData[i] = new string(TextEncrypt(UserData[i], "123456"));
                }
                WriteAllLinesSafe(UserDataPath, UserData);
            }
            else
            {
                UIMessageBox.ShowError("用户数据文件不存在或已损坏，请检查！");
            }
            // 身份下拉框默认选中第一个用户
            CbIdentitySelect.Text = CbIdentitySelect.Items[0].ToString();
        }
        /// <summary>
        /// 窗体关闭事件：置 e.Cancel = false，允许窗体正常关闭（不拦截退出）。
        /// </summary>
        private void FormLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = false;
        }
        /// <summary>
        /// “退出/删除”按钮点击事件（按钮文本随用户管理模式切换）：
        /// 登录模式下注销当前用户并关闭窗体；用户管理模式下删除下拉框所选用户并重写用户文件。
        /// </summary>
        private void BtnExitLogin_Click(object sender, EventArgs e)
        {
            // 按按钮当前文本区分“退出登录”与“删除用户”两种处理
            switch (BtnExitLogin.Text)
            {
                case "退出":
                    // 注销：把当前用户重置为默认技术员（权限 0）并关闭登录窗体
                    mMain.mUser.Authority = 0;
                    mMain.mUser.Name = "技术员";
                    this.Close();
                    break;
                case "删除":
                    // 未选择用户名时不允许删除，直接提示返回
                    if (CbIdentitySelect.Text == "")
                    {
                        ShowErrorDialog("删除失败", "请选择需要删除的用户名！", UIStyle.Red, true);
                        return;
                    }
                    else
                    {
                        // 读取并解密用户文件，移除所选用户后重建用户列表与下拉框，最后重新加密写回
                        if (File.Exists(UserDataPath))
                        {
                            List<string> UserData = new List<string>();
                            UserData.AddRange(File.ReadAllLines(UserDataPath));
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                UserData[i] = TextDecrypt(UserData[i].ToCharArray(), "123456");
                            }
                            UserData.Remove(UserData.Find(g => g.Split(",")[0] == CbIdentitySelect.Text));
                            CbIdentitySelect.Items.Clear();
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                string[] user = UserData[i].Split(",");
                                mUsers.Add(new User(user[0], user[1], int.Parse(user[2])));
                                CbIdentitySelect.Items.Add(user[0]);
                            }
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                UserData[i] = new string(TextEncrypt(UserData[i], "123456"));
                            }
                            WriteAllLinesSafe(UserDataPath, UserData);
                            CbIdentitySelect.Text = "";
                            TbLoginPassword.Text = "";
                            PrivilegeSelect.Text = "";
                        }
                        else
                        {
                            UIMessageBox.ShowError("用户数据文件不存在或已损坏，请检查！");
                        }
                    }
                    break;
            }
        }
        /// <summary>
        /// “登陆/添加”按钮点击事件（按钮文本随用户管理模式切换）：
        /// 登录模式下遍历用户列表校验用户名密码，成功后更新主窗体当前用户并关闭窗体；
        /// 用户管理模式下按所选权限等级把新用户加密追加写入用户文件。
        /// </summary>
        private void BtnLoginSystem_Click(object sender, EventArgs e)
        {
            // 按按钮当前文本区分“登陆”与“添加用户”两种处理
            switch (BtnLoginSystem.Text)
            {
                case "登陆":
                    // 遍历用户列表，校验下拉框所选用户名与输入密码是否匹配
                    bool contain = false;
                    foreach (User user in mUsers)
                    {
                        if (user.Equal(CbIdentitySelect.Text, TbLoginPassword.Text))
                        {
                            contain = true;
                            mMain.mUser = user;
                            mMain.uiLabel1.Text = $"当前为：{user.Name}";
                        }
                    }
                    // 未匹配到用户则提示失败；匹配成功则提示并关闭登录窗体
                    if (!contain)
                    {
                        ShowErrorDialog("登陆失败", "用户名或密码错误！", UIStyle.Red, true);
                    }
                    else
                    {
                        ShowSuccessDialog("登陆成功", CbIdentitySelect.Text + "登陆成功！", UIStyle.Green, true);
                        this.Close();
                    }
                    break;
                case "添加":
                    // 添加用户前校验用户名/密码/权限是否均已填写
                    if (CbIdentitySelect.Text == "" || TbLoginPassword.Text == "" || PrivilegeSelect.Text == "")
                    {
                        ShowErrorDialog("添加失败", "用户名为空或密码为空或权限为空！", UIStyle.Red, true);
                        return;
                    }
                    else
                    {
                        // 读取并解密用户文件，追加新用户后重建用户列表与下拉框，最后重新加密写回
                        if (File.Exists(UserDataPath))
                        {
                            List<string> UserData = new List<string>();
                            UserData.AddRange(File.ReadAllLines(UserDataPath));
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                UserData[i] = TextDecrypt(UserData[i].ToCharArray(), "123456");
                            }
                            // 按所选权限等级拼装“用户名,密码,权限值”记录（一级=0、二级=2、三级=5）
                            switch (PrivilegeSelect.Text)
                            {
                                case "一级":
                                    UserData.Add(CbIdentitySelect.Text + "," + TbLoginPassword.Text + ",0");
                                    break;
                                case "二级":
                                    UserData.Add(CbIdentitySelect.Text + "," + TbLoginPassword.Text + ",2");
                                    break;
                                case "三级":
                                    UserData.Add(CbIdentitySelect.Text + "," + TbLoginPassword.Text + ",5");
                                    break;
                                default:
                                    break;
                            }
                            CbIdentitySelect.Items.Clear();
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                string[] user = UserData[i].Split(",");
                                mUsers.Add(new User(user[0], user[1], int.Parse(user[2])));
                                CbIdentitySelect.Items.Add(user[0]);
                            }
                            for (int i = 0; i < UserData.Count; i++)
                            {
                                UserData[i] = new string(TextEncrypt(UserData[i], "123456"));
                            }
                            WriteAllLinesSafe(UserDataPath, UserData);
                            CbIdentitySelect.Text = "";
                            TbLoginPassword.Text = "";
                            PrivilegeSelect.Text = "";
                        }
                        else
                        {
                            UIMessageBox.ShowError("用户数据文件不存在或已损坏，请检查！");
                        }
                    }
                    break;
                default:
                    break;
            }
            // 登录/添加处理完成后：仅“超级管理员”账号放开系统设置页的参数分组，其他用户禁用
            if (mMain.mUser.Name == "超级管理员")
            {
                mMain.mFormSystemSet.GroupBoxStampingPLC.Enabled = true;
                mMain.mFormSystemSet.uiGroupBox2.Enabled = true;
                mMain.mFormSystemSet.uiGroupBox3.Enabled = true;
            }
            else
            {
                mMain.mFormSystemSet.GroupBoxStampingPLC.Enabled = false;
                mMain.mFormSystemSet.uiGroupBox2.Enabled = false;
                mMain.mFormSystemSet.uiGroupBox3.Enabled = false;
            }

        }
        /// <summary>
        /// 安全写文件：目标文件存在且为只读时先去除只读属性，再写入全部行，
        /// 避免写回加密用户数据时因文件只读而失败。
        /// </summary>
        /// <param name="path">目标文件路径</param>
        /// <param name="contents">要写入的全部行</param>
        private void WriteAllLinesSafe(string path, IEnumerable<string> contents)
        {
            if (File.Exists(path))
            {
                // 文件为只读时先去掉只读属性
                FileInfo fi = new FileInfo(path);
                if (fi.IsReadOnly)
                {
                    fi.IsReadOnly = false;
                }
            }
            File.WriteAllLines(path, contents);
        }
        /// <summary>
        /// XOR 加密：明文每个字符与密钥字符按位异或（密钥循环使用），返回密文字符数组。
        /// </summary>
        /// <param name="content">待加密明文</param>
        /// <param name="secretKey">异或密钥</param>
        /// <returns>加密后的字符数组</returns>
        private char[] TextEncrypt(string content, string secretKey)
        {
            char[] data = content.ToCharArray();
            char[] key = secretKey.ToCharArray();
            // 逐字符与密钥（循环取用）异或完成加密
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            return data;
        }
        /// <summary>
        /// XOR 解密：与加密同一套异或运算（异或自反），还原出明文字符串。
        /// </summary>
        /// <param name="data">密文字符数组</param>
        /// <param name="secretKey">异或密钥</param>
        /// <returns>解密后的明文</returns>
        private string TextDecrypt(char[] data, string secretKey)
        {
            char[] key = secretKey.ToCharArray();
            // 逐字符与密钥（循环取用）异或完成解密
            for (int i = 0; i < data.Length; i++)
            {
                data[i] ^= key[i % key.Length];
            }
            return new string(data);
        }
        /// <summary>
        /// “用户管理”按钮点击事件：权限等级大于 2（管理员）时可进入/退出用户管理模式，
        /// 模式切换时同步切换登陆/退出按钮文本并加高窗体以显示权限选择框。
        /// </summary>
        private void Btn用户管理_Click(object sender, EventArgs e)
        {
            // 权限校验：仅管理员（权限 5）可进行用户管理
            if (mMain.mUser.Authority > 2)
            {
                // 在“用户管理”与“退出用户管理模式”两种状态间切换
                switch (Btn用户管理.Text)
                {
                    case "用户管理":
                        BtnLoginSystem.Text = "添加";
                        BtnExitLogin.Text = "删除";
                        Btn用户管理.Text = "退出用户管理模式";
                        this.Height = 309;
                        break;
                    case "退出用户管理模式":
                        BtnLoginSystem.Text = "登陆";
                        BtnExitLogin.Text = "退出";
                        Btn用户管理.Text = "用户管理";
                        this.Height = 257;
                        break;
                }
            }
            else
            {
                UIMessageBox.ShowWarning("亲，你目前的权限不够哦，请使用管理员权限，谢谢！");
            }
        }
    }
}
