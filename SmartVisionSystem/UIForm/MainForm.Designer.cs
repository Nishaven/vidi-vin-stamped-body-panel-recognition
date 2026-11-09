namespace SmartVisionSystem.UIForm
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.PictureBoxLogo = new System.Windows.Forms.PictureBox();
            this.BtnLogin = new Sunny.UI.UIAvatar();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.Header.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // Aside
            // 
            this.Aside.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Aside.Indent = 5;
            this.Aside.LineColor = System.Drawing.Color.Black;
            this.Aside.Location = new System.Drawing.Point(0, 95);
            this.Aside.Size = new System.Drawing.Size(191, 757);
            this.Aside.TipsFont = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Aside.MenuItemClick += new Sunny.UI.UINavMenu.OnMenuItemClick(this.Aside_MenuItemClick);
            // 
            // Header
            // 
            this.Header.Controls.Add(this.uiLabel2);
            this.Header.Controls.Add(this.uiLabel1);
            this.Header.Controls.Add(this.BtnLogin);
            this.Header.Controls.Add(this.PictureBoxLogo);
            this.Header.Size = new System.Drawing.Size(1373, 60);
            // 
            // PictureBoxLogo
            // 
            this.PictureBoxLogo.Image = ((System.Drawing.Image)(resources.GetObject("PictureBoxLogo.Image")));
            this.PictureBoxLogo.InitialImage = ((System.Drawing.Image)(resources.GetObject("PictureBoxLogo.InitialImage")));
            this.PictureBoxLogo.Location = new System.Drawing.Point(0, -27);
            this.PictureBoxLogo.Name = "PictureBoxLogo";
            this.PictureBoxLogo.Size = new System.Drawing.Size(600, 115);
            this.PictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PictureBoxLogo.TabIndex = 8;
            this.PictureBoxLogo.TabStop = false;
            // 
            // BtnLogin
            // 
            this.BtnLogin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnLogin.AvatarSize = 50;
            this.BtnLogin.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnLogin.Location = new System.Drawing.Point(1295, 2);
            this.BtnLogin.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnLogin.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnLogin.Name = "BtnLogin";
            this.BtnLogin.Radius = 3;
            this.BtnLogin.Size = new System.Drawing.Size(55, 55);
            this.BtnLogin.TabIndex = 9;
            this.BtnLogin.Text = "uiAvatar1";
            this.BtnLogin.Click += new System.EventHandler(this.BtnLogin_Click);
            // 
            // uiLabel1
            // 
            this.uiLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel1.ForeColor = System.Drawing.Color.DodgerBlue;
            this.uiLabel1.Location = new System.Drawing.Point(983, 2);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(307, 55);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 10;
            this.uiLabel1.Text = "当前为:技术员";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel2.ForeColor = System.Drawing.Color.Lime;
            this.uiLabel2.Location = new System.Drawing.Point(710, 2);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(276, 55);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 11;
            this.uiLabel2.Text = "当前为:自动模式";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiLabel2.Click += new System.EventHandler(this.uiLabel2_Click);
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1373, 852);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.ShowTitleIcon = true;
            this.StyleCustomMode = true;
            this.Text = "整车制造一部焊装VIN码智能识别系统";
            this.TitleFont = new System.Drawing.Font("幼圆", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 800, 450);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Header.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBoxLogo)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBoxLogo;
        private Sunny.UI.UIAvatar BtnLogin;
        public Sunny.UI.UILabel uiLabel1;
        public Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}