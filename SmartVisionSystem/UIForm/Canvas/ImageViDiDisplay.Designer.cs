
namespace SmartVisionSystem.UIForm.Canvas
{
    partial class ImageViDiDisplay
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BtnLoadImage = new Sunny.UI.UIHeaderButton();
            this.HeadPanel = new Sunny.UI.UITableLayoutPanel();
            this.BtnRun = new Sunny.UI.UIHeaderButton();
            this.ComboBoxCarModel = new Sunny.UI.UIComboBox();
            this.uiLabel4 = new Sunny.UI.UILabel();
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.LabelResult = new Sunny.UI.UILabel();
            this.LabelCameraName = new Sunny.UI.UILabel();
            this.ElementHostViDi = new System.Windows.Forms.Integration.ElementHost();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.HeadPanel.SuspendLayout();
            this.uiTableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // BtnLoadImage
            // 
            this.BtnLoadImage.CircleColor = System.Drawing.Color.DarkOrange;
            this.BtnLoadImage.CircleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.BtnLoadImage.CircleSize = 26;
            this.BtnLoadImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnLoadImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnLoadImage.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnLoadImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.BtnLoadImage.Location = new System.Drawing.Point(1261, 0);
            this.BtnLoadImage.Margin = new System.Windows.Forms.Padding(2, 0, 0, 0);
            this.BtnLoadImage.MinimumSize = new System.Drawing.Size(2, 2);
            this.BtnLoadImage.Name = "BtnLoadImage";
            this.BtnLoadImage.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.BtnLoadImage.Radius = 0;
            this.BtnLoadImage.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.BtnLoadImage.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.BtnLoadImage.ShowCircleHoverColor = true;
            this.BtnLoadImage.Size = new System.Drawing.Size(58, 48);
            this.BtnLoadImage.Style = Sunny.UI.UIStyle.Custom;
            this.BtnLoadImage.StyleCustomMode = true;
            this.BtnLoadImage.Symbol = 57489;
            this.BtnLoadImage.SymbolOffset = new System.Drawing.Point(0, 2);
            this.BtnLoadImage.SymbolSize = 24;
            this.BtnLoadImage.TabIndex = 12;
            this.BtnLoadImage.Text = "图片加载";
            this.BtnLoadImage.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnLoadImage.Click += new System.EventHandler(this.Btn_Click);
            // 
            // HeadPanel
            // 
            this.HeadPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.HeadPanel.ColumnCount = 5;
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.HeadPanel.Controls.Add(this.BtnLoadImage, 4, 0);
            this.HeadPanel.Controls.Add(this.BtnRun, 3, 0);
            this.HeadPanel.Controls.Add(this.ComboBoxCarModel, 2, 0);
            this.HeadPanel.Controls.Add(this.uiLabel4, 1, 0);
            this.HeadPanel.Controls.Add(this.uiTableLayoutPanel1, 0, 0);
            this.HeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeadPanel.Location = new System.Drawing.Point(0, 0);
            this.HeadPanel.Margin = new System.Windows.Forms.Padding(0);
            this.HeadPanel.Name = "HeadPanel";
            this.HeadPanel.RowCount = 1;
            this.HeadPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeadPanel.Size = new System.Drawing.Size(1319, 48);
            this.HeadPanel.TabIndex = 31;
            this.HeadPanel.TagString = null;
            // 
            // BtnRun
            // 
            this.BtnRun.CircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.BtnRun.CircleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.BtnRun.CircleSize = 26;
            this.BtnRun.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnRun.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BtnRun.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRun.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.BtnRun.Location = new System.Drawing.Point(1201, 0);
            this.BtnRun.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.BtnRun.MinimumSize = new System.Drawing.Size(2, 2);
            this.BtnRun.Name = "BtnRun";
            this.BtnRun.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.BtnRun.Radius = 0;
            this.BtnRun.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.BtnRun.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.BtnRun.ShowCircleHoverColor = true;
            this.BtnRun.Size = new System.Drawing.Size(56, 48);
            this.BtnRun.Style = Sunny.UI.UIStyle.Custom;
            this.BtnRun.StyleCustomMode = true;
            this.BtnRun.Symbol = 61764;
            this.BtnRun.SymbolOffset = new System.Drawing.Point(0, 2);
            this.BtnRun.SymbolSize = 24;
            this.BtnRun.TabIndex = 15;
            this.BtnRun.Text = "采集图片";
            this.BtnRun.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRun.Click += new System.EventHandler(this.Btn_Click);
            // 
            // ComboBoxCarModel
            // 
            this.ComboBoxCarModel.DataSource = null;
            this.ComboBoxCarModel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ComboBoxCarModel.DropDownWidth = 72;
            this.ComboBoxCarModel.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.ComboBoxCarModel.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.ComboBoxCarModel.Items.AddRange(new object[] {
            "GS8",
            "M8",
            "M6",
            "AM8",
            "A09"});
            this.ComboBoxCarModel.Location = new System.Drawing.Point(1100, 1);
            this.ComboBoxCarModel.Margin = new System.Windows.Forms.Padding(1);
            this.ComboBoxCarModel.MinimumSize = new System.Drawing.Size(63, 0);
            this.ComboBoxCarModel.Name = "ComboBoxCarModel";
            this.ComboBoxCarModel.Padding = new System.Windows.Forms.Padding(0, 0, 30, 2);
            this.ComboBoxCarModel.Radius = 2;
            this.ComboBoxCarModel.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.ComboBoxCarModel.Size = new System.Drawing.Size(98, 46);
            this.ComboBoxCarModel.Style = Sunny.UI.UIStyle.Custom;
            this.ComboBoxCarModel.StyleCustomMode = true;
            this.ComboBoxCarModel.TabIndex = 17;
            this.ComboBoxCarModel.Text = "T58";
            this.ComboBoxCarModel.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.ComboBoxCarModel.Watermark = "";
            // 
            // uiLabel4
            // 
            this.uiLabel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiLabel4.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiLabel4.Location = new System.Drawing.Point(1074, 0);
            this.uiLabel4.Name = "uiLabel4";
            this.uiLabel4.Size = new System.Drawing.Size(22, 48);
            this.uiLabel4.TabIndex = 16;
            this.uiLabel4.Text = "车型";
            this.uiLabel4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 1;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel1.Controls.Add(this.LabelResult, 0, 1);
            this.uiTableLayoutPanel1.Controls.Add(this.LabelCameraName, 0, 0);
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.uiTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 2;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(1071, 48);
            this.uiTableLayoutPanel1.TabIndex = 18;
            this.uiTableLayoutPanel1.TagString = null;
            // 
            // LabelResult
            // 
            this.LabelResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelResult.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LabelResult.Location = new System.Drawing.Point(1, 25);
            this.LabelResult.Margin = new System.Windows.Forms.Padding(1);
            this.LabelResult.Name = "LabelResult";
            this.LabelResult.Size = new System.Drawing.Size(1069, 22);
            this.LabelResult.TabIndex = 19;
            this.LabelResult.Text = "NA";
            this.LabelResult.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // LabelCameraName
            // 
            this.LabelCameraName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCameraName.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LabelCameraName.Location = new System.Drawing.Point(1, 1);
            this.LabelCameraName.Margin = new System.Windows.Forms.Padding(1);
            this.LabelCameraName.Name = "LabelCameraName";
            this.LabelCameraName.Size = new System.Drawing.Size(1069, 22);
            this.LabelCameraName.TabIndex = 14;
            this.LabelCameraName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ElementHostViDi
            // 
            this.ElementHostViDi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ElementHostViDi.Location = new System.Drawing.Point(0, 48);
            this.ElementHostViDi.Name = "ElementHostViDi";
            this.ElementHostViDi.Size = new System.Drawing.Size(1319, 852);
            this.ElementHostViDi.TabIndex = 32;
            this.ElementHostViDi.Text = "elementHost1";
            this.ElementHostViDi.Child = null;
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // ImageViDiDisplay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ElementHostViDi);
            this.Controls.Add(this.HeadPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ImageViDiDisplay";
            this.Size = new System.Drawing.Size(1319, 900);
            this.HeadPanel.ResumeLayout(false);
            this.uiTableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Sunny.UI.UIHeaderButton BtnLoadImage;
        private Sunny.UI.UITableLayoutPanel HeadPanel;
        public Sunny.UI.UILabel LabelCameraName;
        private System.Windows.Forms.Integration.ElementHost ElementHostViDi;
        public Sunny.UI.UIHeaderButton BtnRun;
        private Sunny.UI.UITableLayoutPanel uiTableLayoutPanel1;
        internal Sunny.UI.UILabel LabelResult;
        internal Sunny.UI.UIComboBox ComboBoxCarModel;
        private Sunny.UI.UILabel uiLabel4;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}
