
namespace SmartVisionSystem.UIForm.Pages
{
    partial class VisionToolSet
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
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.panel1 = new Sunny.UI.UIPanel();
            this.ComboBoxCarModel = new Sunny.UI.UIComboBox();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.TextBoxGain = new Sunny.UI.UITextBox();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.TextBoxExposures = new Sunny.UI.UITextBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.BtnRunAcqImage = new Sunny.UI.UIButton();
            this.BtnOfflineRunTools = new Sunny.UI.UIButton();
            this.BtnSaveTools = new Sunny.UI.UIButton();
            this.uiTableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 1;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(1, 33);
            this.uiTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 2;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 59F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(1198, 696);
            this.uiTableLayoutPanel1.TabIndex = 4;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.ComboBoxCarModel);
            this.panel1.Controls.Add(this.uiLabel4);
            this.panel1.Controls.Add(this.TextBoxGain);
            this.panel1.Controls.Add(this.uiLabel3);
            this.panel1.Controls.Add(this.uiLabel2);
            this.panel1.Controls.Add(this.TextBoxExposures);
            this.panel1.Controls.Add(this.uiLabel1);
            this.panel1.Controls.Add(this.BtnRunAcqImage);
            this.panel1.Controls.Add(this.BtnOfflineRunTools);
            this.panel1.Controls.Add(this.BtnSaveTools);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.panel1.Location = new System.Drawing.Point(1, 638);
            this.panel1.Margin = new System.Windows.Forms.Padding(1);
            this.panel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1196, 57);
            this.panel1.TabIndex = 4;
            this.panel1.Text = null;
            this.panel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ComboBoxCarModel
            // 
            this.ComboBoxCarModel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ComboBoxCarModel.DataSource = null;
            this.ComboBoxCarModel.DropDownWidth = 120;
            this.ComboBoxCarModel.FillColor = System.Drawing.Color.White;
            this.ComboBoxCarModel.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ComboBoxCarModel.Items.AddRange(new object[] {
            "GS8",
            "M8",
            "M6",
            "AM8",
            "A09"});
            this.ComboBoxCarModel.Location = new System.Drawing.Point(708, 14);
            this.ComboBoxCarModel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ComboBoxCarModel.MinimumSize = new System.Drawing.Size(63, 0);
            this.ComboBoxCarModel.Name = "ComboBoxCarModel";
            this.ComboBoxCarModel.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.ComboBoxCarModel.Size = new System.Drawing.Size(120, 29);
            this.ComboBoxCarModel.TabIndex = 15;
            this.ComboBoxCarModel.Text = "GS8";
            this.ComboBoxCarModel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.ComboBoxCarModel.Visible = false;
            this.ComboBoxCarModel.Watermark = "";
            // 
            // uiLabel4
            // 
            this.uiLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel4.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel4.Location = new System.Drawing.Point(655, 17);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(63, 23);
            this.uiLabel4.TabIndex = 14;
            this.uiLabel4.Text = "车型:";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.uiLabel4.Visible = false;
            // 
            // TextBoxGain
            // 
            this.TextBoxGain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxGain.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.TextBoxGain.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextBoxGain.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.TextBoxGain.Location = new System.Drawing.Point(552, 14);
            this.TextBoxGain.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxGain.MinimumSize = new System.Drawing.Size(1, 1);
            this.TextBoxGain.Name = "TextBoxGain";
            this.TextBoxGain.Padding = new System.Windows.Forms.Padding(5);
            this.TextBoxGain.ShowText = false;
            this.TextBoxGain.Size = new System.Drawing.Size(90, 29);
            this.TextBoxGain.TabIndex = 11;
            this.TextBoxGain.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.TextBoxGain.Watermark = "";
            this.TextBoxGain.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxGain_KeyDown);
            // 
            // uiLabel3
            // 
            this.uiLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel3.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel3.Location = new System.Drawing.Point(496, 17);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(63, 23);
            this.uiLabel3.TabIndex = 12;
            this.uiLabel3.Text = "增益：";
            this.uiLabel3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel2.Location = new System.Drawing.Point(459, 17);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(33, 23);
            this.uiLabel2.TabIndex = 10;
            this.uiLabel2.Text = "ms";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TextBoxExposures
            // 
            this.TextBoxExposures.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TextBoxExposures.ButtonSymbolOffset = new System.Drawing.Point(0, 0);
            this.TextBoxExposures.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.TextBoxExposures.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.TextBoxExposures.Location = new System.Drawing.Point(369, 14);
            this.TextBoxExposures.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.TextBoxExposures.MinimumSize = new System.Drawing.Size(1, 1);
            this.TextBoxExposures.Name = "TextBoxExposures";
            this.TextBoxExposures.Padding = new System.Windows.Forms.Padding(5);
            this.TextBoxExposures.ShowText = false;
            this.TextBoxExposures.Size = new System.Drawing.Size(90, 29);
            this.TextBoxExposures.TabIndex = 9;
            this.TextBoxExposures.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.TextBoxExposures.Watermark = "";
            this.TextBoxExposures.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TextBoxExposures_KeyDown);
            // 
            // uiLabel1
            // 
            this.uiLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLabel1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel1.Location = new System.Drawing.Point(287, 17);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(90, 23);
            this.uiLabel1.TabIndex = 8;
            this.uiLabel1.Text = "曝光时间：";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnRunAcqImage
            // 
            this.BtnRunAcqImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnRunAcqImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnRunAcqImage.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRunAcqImage.Location = new System.Drawing.Point(845, 12);
            this.BtnRunAcqImage.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnRunAcqImage.Name = "BtnRunAcqImage";
            this.BtnRunAcqImage.Size = new System.Drawing.Size(99, 33);
            this.BtnRunAcqImage.TabIndex = 3;
            this.BtnRunAcqImage.Text = "触发拍照";
            this.BtnRunAcqImage.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRunAcqImage.Click += new System.EventHandler(this.BtnRunAcqImage_Click);
            // 
            // BtnOfflineRunTools
            // 
            this.BtnOfflineRunTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOfflineRunTools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnOfflineRunTools.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnOfflineRunTools.Location = new System.Drawing.Point(961, 12);
            this.BtnOfflineRunTools.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnOfflineRunTools.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnOfflineRunTools.Name = "BtnOfflineRunTools";
            this.BtnOfflineRunTools.Size = new System.Drawing.Size(99, 33);
            this.BtnOfflineRunTools.TabIndex = 1;
            this.BtnOfflineRunTools.Text = "离线模拟";
            this.BtnOfflineRunTools.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnOfflineRunTools.Click += new System.EventHandler(this.BtnOfflineRunTools_Click);
            // 
            // BtnSaveTools
            // 
            this.BtnSaveTools.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnSaveTools.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnSaveTools.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.BtnSaveTools.Location = new System.Drawing.Point(1077, 12);
            this.BtnSaveTools.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.BtnSaveTools.MinimumSize = new System.Drawing.Size(1, 1);
            this.BtnSaveTools.Name = "BtnSaveTools";
            this.BtnSaveTools.Size = new System.Drawing.Size(99, 33);
            this.BtnSaveTools.TabIndex = 0;
            this.BtnSaveTools.Text = "配置保存";
            this.BtnSaveTools.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnSaveTools.Click += new System.EventHandler(this.BtnSaveTools_Click);
            // 
            // VisionToolSet
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1200, 730);
            this.Controls.Add(this.uiTableLayoutPanel1);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximumSize = new System.Drawing.Size(1536, 990);
            this.Name = "VisionToolSet";
            this.Padding = new System.Windows.Forms.Padding(1, 33, 1, 1);
            this.Text = "工具设置";
            this.ZoomScaleRect = new System.Drawing.Rectangle(15, 15, 1024, 750);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.VisionToolSet_FormClosed);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        private Sunny.UI.UIPanel panel1;
        private Sunny.UI.UIButton BtnSaveTools;
        private Sunny.UI.UIButton BtnOfflineRunTools;
        private Sunny.UI.UIButton BtnRunAcqImage;
        private Sunny.UI.UITextBox TextBoxGain;
        private Sunny.UI.UILabel uiLabel3;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UITextBox TextBoxExposures;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UIComboBox ComboBoxCarModel;
    }
}