
namespace SmartVisionSystem.UIForm.Pages
{
    partial class FormLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.CbIdentitySelect = new Sunny.UI.UIComboBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiAvatar = new Sunny.UI.UIAvatar();
            this.TbLoginPassword = new Sunny.UI.UITextBox();
            this.BtnLoginSystem = new Sunny.UI.UIButton();
            this.BtnExitLogin = new Sunny.UI.UIButton();
            this.Btn用户管理 = new Sunny.UI.UIButton();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.PrivilegeSelect = new Sunny.UI.UIComboBox();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.SuspendLayout();
            // 
            // CbIdentitySelect
            // 
            this.CbIdentitySelect.DataSource = null;
            this.CbIdentitySelect.FillColor = System.Drawing.Color.White;
            this.CbIdentitySelect.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.CbIdentitySelect.Location = new System.Drawing.Point(85, 66);
            this.CbIdentitySelect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.CbIdentitySelect.MinimumSize = new System.Drawing.Size(63, 0);
            this.CbIdentitySelect.Name = "CbIdentitySelect";
            this.CbIdentitySelect.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.CbIdentitySelect.Size = new System.Drawing.Size(173, 40);
            this.CbIdentitySelect.TabIndex = 0;
            this.CbIdentitySelect.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.CbIdentitySelect.Watermark = "";
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel1.Location = new System.Drawing.Point(25, 128);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(87, 39);
            this.uiLabel1.TabIndex = 1;
            this.uiLabel1.Text = "密码：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiAvatar
            // 
            this.uiAvatar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiAvatar.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiAvatar.Location = new System.Drawing.Point(13, 47);
            this.uiAvatar.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.uiAvatar.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiAvatar.Name = "uiAvatar";
            this.uiAvatar.Size = new System.Drawing.Size(66, 70);
            this.uiAvatar.TabIndex = 6;
            this.uiAvatar.Text = "uiAvatar1";
            // 
            // TbLoginPassword
            // 
            this.TbLoginPassword.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.TbLoginPassword.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TbLoginPassword.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.TbLoginPassword.Location = new System.Drawing.Point(85, 128);
            this.TbLoginPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TbLoginPassword.MinimumSize = new System.Drawing.Size(1, 1);
            this.TbLoginPassword.Name = "TbLoginPassword";
            this.TbLoginPassword.Padding = new System.Windows.Forms.Padding(5);
            this.TbLoginPassword.PasswordChar = '*';
            this.TbLoginPassword.ShowText = false;
            this.TbLoginPassword.Size = new System.Drawing.Size(173, 39);
            this.TbLoginPassword.TabIndex = 7;
            this.TbLoginPassword.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.TbLoginPassword.Watermark = "";
            // 
            // BtnLoginSystem
            // 
            this.BtnLoginSystem.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnLoginSystem.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnLoginSystem.Location = new System.Drawing.Point(29, 175);
            this.BtnLoginSystem.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnLoginSystem.Name = "BtnLoginSystem";
            this.BtnLoginSystem.Size = new System.Drawing.Size(103, 35);
            this.BtnLoginSystem.TabIndex = 8;
            this.BtnLoginSystem.Text = "登陆";
            this.BtnLoginSystem.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnLoginSystem.Click += new System.EventHandler(this.BtnLoginSystem_Click);
            // 
            // BtnExitLogin
            // 
            this.BtnExitLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnExitLogin.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnExitLogin.Location = new System.Drawing.Point(155, 175);
            this.BtnExitLogin.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnExitLogin.Name = "BtnExitLogin";
            this.BtnExitLogin.Size = new System.Drawing.Size(103, 35);
            this.BtnExitLogin.TabIndex = 9;
            this.BtnExitLogin.Text = "退出";
            this.BtnExitLogin.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnExitLogin.Click += new System.EventHandler(this.BtnExitLogin_Click);
            // 
            // Btn用户管理
            // 
            this.Btn用户管理.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn用户管理.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.Btn用户管理.Location = new System.Drawing.Point(29, 216);
            this.Btn用户管理.MinimumSize = new System.Drawing.Size(1, 1);
            this.Btn用户管理.Name = "Btn用户管理";
            this.Btn用户管理.Size = new System.Drawing.Size(229, 35);
            this.Btn用户管理.TabIndex = 10;
            this.Btn用户管理.Text = "用户管理";
            this.Btn用户管理.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn用户管理.Click += new System.EventHandler(this.Btn用户管理_Click);
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel3.Location = new System.Drawing.Point(25, 259);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(61, 39);
            this.uiLabel3.TabIndex = 17;
            this.uiLabel3.Text = "权限：";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // PrivilegeSelect
            // 
            this.PrivilegeSelect.DataSource = null;
            this.PrivilegeSelect.FillColor = System.Drawing.Color.White;
            this.PrivilegeSelect.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.PrivilegeSelect.Items.AddRange(new object[] {
            "一级",
            "二级",
            "三级"});
            this.PrivilegeSelect.Location = new System.Drawing.Point(86, 259);
            this.PrivilegeSelect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.PrivilegeSelect.MinimumSize = new System.Drawing.Size(63, 0);
            this.PrivilegeSelect.Name = "PrivilegeSelect";
            this.PrivilegeSelect.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.PrivilegeSelect.Size = new System.Drawing.Size(173, 40);
            this.PrivilegeSelect.TabIndex = 16;
            this.PrivilegeSelect.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.PrivilegeSelect.Watermark = "";
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // FormLogin
            // 
            this.AcceptButton = this.BtnLoginSystem;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(276, 309);
            this.Controls.Add(this.uiLabel3);
            this.Controls.Add(this.PrivilegeSelect);
            this.Controls.Add(this.Btn用户管理);
            this.Controls.Add(this.BtnExitLogin);
            this.Controls.Add(this.BtnLoginSystem);
            this.Controls.Add(this.TbLoginPassword);
            this.Controls.Add(this.uiAvatar);
            this.Controls.Add(this.uiLabel1);
            this.Controls.Add(this.CbIdentitySelect);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLogin";
            this.Text = "用户登陆";
            this.ZoomScaleRect = new System.Drawing.Rectangle(30, 30, 276, 309);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormLogin_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UIComboBox CbIdentitySelect;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIAvatar uiAvatar;
        private Sunny.UI.UITextBox TbLoginPassword;
        private Sunny.UI.UIButton BtnLoginSystem;
        private Sunny.UI.UIButton BtnExitLogin;
        private Sunny.UI.UIButton Btn用户管理;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UIComboBox PrivilegeSelect;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}