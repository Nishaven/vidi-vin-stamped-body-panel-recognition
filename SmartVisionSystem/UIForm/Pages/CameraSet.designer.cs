
namespace SmartVisionSystem.UIForm.Pages
{
    partial class CameraSet
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
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.uiTableLayoutPanel2 = new Sunny.UI.UITableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.TextBoxGain = new Sunny.UI.UITextBox();
            this.BtnStartUpLiveImage = new Sunny.UI.UIButton();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.TextBoxExposures = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.BtnSaveTools = new Sunny.UI.UIButton();
            this.button1 = new System.Windows.Forms.Button();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.uiTableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 1;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.Controls.Add(this.uiTableLayoutPanel2, 0, 0);
            this.uiTableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(1, 35);
            this.uiTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 2;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(748, 464);
            this.uiTableLayoutPanel1.TabIndex = 5;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // uiTableLayoutPanel2
            // 
            this.uiTableLayoutPanel2.ColumnCount = 2;
            this.uiTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.uiTableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.uiTableLayoutPanel2.Name = "uiTableLayoutPanel2";
            this.uiTableLayoutPanel2.RowCount = 2;
            this.uiTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.uiTableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.uiTableLayoutPanel2.Size = new System.Drawing.Size(748, 414);
            this.uiTableLayoutPanel2.TabIndex = 40;
            this.uiTableLayoutPanel2.TagString = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.uiLabel3);
            this.panel1.Controls.Add(this.TextBoxGain);
            this.panel1.Controls.Add(this.BtnStartUpLiveImage);
            this.panel1.Controls.Add(this.uiLabel2);
            this.panel1.Controls.Add(this.TextBoxExposures);
            this.panel1.Controls.Add(this.uiLabel1);
            this.panel1.Controls.Add(this.BtnSaveTools);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 414);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(748, 50);
            this.panel1.TabIndex = 4;
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel3.Location = new System.Drawing.Point(252, 14);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(79, 23);
            this.uiLabel3.TabIndex = 7;
            this.uiLabel3.Text = "增益：";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxGain
            // 
            this.TextBoxGain.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.TextBoxGain.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextBoxGain.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.TextBoxGain.Location = new System.Drawing.Point(338, 11);
            this.TextBoxGain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxGain.MinimumSize = new System.Drawing.Size(1, 1);
            this.TextBoxGain.Name = "TextBoxGain";
            this.TextBoxGain.Padding = new System.Windows.Forms.Padding(5);
            this.TextBoxGain.ShowText = false;
            this.TextBoxGain.Size = new System.Drawing.Size(90, 29);
            this.TextBoxGain.TabIndex = 6;
            this.TextBoxGain.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.TextBoxGain.Watermark = "";
            this.TextBoxGain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxGain_KeyDown);
            // 
            // BtnStartUpLiveImage
            // 
            this.BtnStartUpLiveImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnStartUpLiveImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnStartUpLiveImage.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnStartUpLiveImage.Location = new System.Drawing.Point(453, 9);
            this.BtnStartUpLiveImage.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnStartUpLiveImage.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnStartUpLiveImage.Name = "BtnStartUpLiveImage";
            this.BtnStartUpLiveImage.Size = new System.Drawing.Size(25, 33);
            this.BtnStartUpLiveImage.TabIndex = 4;
            this.BtnStartUpLiveImage.Text = "打开上光源";
            this.BtnStartUpLiveImage.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnStartUpLiveImage.Visible = false;
            this.BtnStartUpLiveImage.Click += new System.EventHandler(this.BtnStartUpLiveImage_Click);
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel2.Location = new System.Drawing.Point(213, 14);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(33, 23);
            this.uiLabel2.TabIndex = 3;
            this.uiLabel2.Text = "ms";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxExposures
            // 
            this.TextBoxExposures.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.TextBoxExposures.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextBoxExposures.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.TextBoxExposures.Location = new System.Drawing.Point(123, 11);
            this.TextBoxExposures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxExposures.MinimumSize = new System.Drawing.Size(1, 1);
            this.TextBoxExposures.Name = "TextBoxExposures";
            this.TextBoxExposures.Padding = new System.Windows.Forms.Padding(5);
            this.TextBoxExposures.ShowText = false;
            this.TextBoxExposures.Size = new System.Drawing.Size(90, 29);
            this.TextBoxExposures.TabIndex = 2;
            this.TextBoxExposures.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.TextBoxExposures.Watermark = "";
            this.TextBoxExposures.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxExposures_KeyDown);
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel1.Location = new System.Drawing.Point(41, 14);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(90, 23);
            this.uiLabel1.TabIndex = 1;
            this.uiLabel1.Text = "曝光时间：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnSaveTools
            // 
            this.BtnSaveTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSaveTools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnSaveTools.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnSaveTools.Location = new System.Drawing.Point(641, 9);
            this.BtnSaveTools.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnSaveTools.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnSaveTools.Name = "BtnSaveTools";
            this.BtnSaveTools.Size = new System.Drawing.Size(80, 33);
            this.BtnSaveTools.TabIndex = 0;
            this.BtnSaveTools.Text = "配置保存";
            this.BtnSaveTools.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnSaveTools.Click += new System.EventHandler(this.BtnSaveTools_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(98, 45);
            this.button1.TabIndex = 36;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // CameraSet
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(750, 500);
            this.Controls.Add(this.uiTableLayoutPanel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CameraSet";
            this.Padding = new System.Windows.Forms.Padding(1, 35, 1, 1);
            this.Text = "相机设置";
            this.ZoomScaleRect = new System.Drawing.Rectangle(22, 22, 900, 496);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraSet_FormClosing);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private Sunny.UI.UIButton BtnSaveTools;
        private System.Windows.Forms.Button button1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UITextBox TextBoxExposures;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UIButton BtnStartUpLiveImage;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UITextBox TextBoxGain;
        public Sunny.UI.UITableLayoutPanel uiTableLayoutPanel2;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}