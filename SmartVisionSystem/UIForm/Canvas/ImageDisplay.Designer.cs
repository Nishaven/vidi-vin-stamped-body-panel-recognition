
namespace SmartVisionSystem.UIForm.Canvas
{
    partial class ImageDisplay
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageDisplay));
            this.Btn_Run = new Sunny.UI.UIHeaderButton();
            this.Btn_ToolSetting = new Sunny.UI.UIHeaderButton();
            this.Btn_LiveImage = new Sunny.UI.UIHeaderButton();
            this.HeadPanel = new Sunny.UI.UITableLayoutPanel();
            this.Btn_StopLive = new Sunny.UI.UIHeaderButton();
            this.LabelCameraName = new Sunny.UI.UILabel();
            this.RecordDisplay = new Cognex.VisionPro.CogRecordDisplay();
            this.HeadPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecordDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // Btn_Run
            // 
            this.Btn_Run.CircleColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.Btn_Run.CircleHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
            this.Btn_Run.CircleSize = 36;
            this.Btn_Run.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_Run.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Btn_Run.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_Run.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.Btn_Run.Location = new System.Drawing.Point(378, 0);
            this.Btn_Run.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Btn_Run.MinimumSize = new System.Drawing.Size(2, 2);
            this.Btn_Run.Name = "Btn_Run";
            this.Btn_Run.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Btn_Run.Radius = 0;
            this.Btn_Run.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.Btn_Run.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.Btn_Run.ShowCircleHoverColor = true;
            this.Btn_Run.Size = new System.Drawing.Size(64, 56);
            this.Btn_Run.Style = Sunny.UI.UIStyle.Custom;
            this.Btn_Run.StyleCustomMode = true;
            this.Btn_Run.Symbol = 61764;
            this.Btn_Run.SymbolOffset = new System.Drawing.Point(0, 2);
            this.Btn_Run.SymbolSize = 32;
            this.Btn_Run.TabIndex = 12;
            this.Btn_Run.Text = "单次运行";
            this.Btn_Run.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_Run.Click += new System.EventHandler(this.Btn_Click);
            // 
            // Btn_ToolSetting
            // 
            this.Btn_ToolSetting.CircleColor = System.Drawing.Color.Olive;
            this.Btn_ToolSetting.CircleSize = 36;
            this.Btn_ToolSetting.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_ToolSetting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Btn_ToolSetting.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_ToolSetting.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.Btn_ToolSetting.Location = new System.Drawing.Point(582, 0);
            this.Btn_ToolSetting.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Btn_ToolSetting.MinimumSize = new System.Drawing.Size(2, 2);
            this.Btn_ToolSetting.Name = "Btn_ToolSetting";
            this.Btn_ToolSetting.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Btn_ToolSetting.Radius = 0;
            this.Btn_ToolSetting.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.Btn_ToolSetting.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.Btn_ToolSetting.Size = new System.Drawing.Size(64, 56);
            this.Btn_ToolSetting.Style = Sunny.UI.UIStyle.Custom;
            this.Btn_ToolSetting.StyleCustomMode = true;
            this.Btn_ToolSetting.Symbol = 61459;
            this.Btn_ToolSetting.SymbolOffset = new System.Drawing.Point(1, 2);
            this.Btn_ToolSetting.SymbolSize = 32;
            this.Btn_ToolSetting.TabIndex = 11;
            this.Btn_ToolSetting.Text = "工具设置";
            this.Btn_ToolSetting.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_ToolSetting.Click += new System.EventHandler(this.Btn_Click);
            // 
            // Btn_LiveImage
            // 
            this.Btn_LiveImage.CircleColor = System.Drawing.Color.DarkOrange;
            this.Btn_LiveImage.CircleSize = 36;
            this.Btn_LiveImage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_LiveImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Btn_LiveImage.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_LiveImage.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.Btn_LiveImage.Location = new System.Drawing.Point(446, 0);
            this.Btn_LiveImage.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Btn_LiveImage.MinimumSize = new System.Drawing.Size(2, 2);
            this.Btn_LiveImage.Name = "Btn_LiveImage";
            this.Btn_LiveImage.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Btn_LiveImage.Radius = 0;
            this.Btn_LiveImage.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.Btn_LiveImage.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.Btn_LiveImage.Size = new System.Drawing.Size(64, 56);
            this.Btn_LiveImage.Style = Sunny.UI.UIStyle.Custom;
            this.Btn_LiveImage.StyleCustomMode = true;
            this.Btn_LiveImage.Symbol = 61501;
            this.Btn_LiveImage.SymbolOffset = new System.Drawing.Point(0, 1);
            this.Btn_LiveImage.SymbolSize = 32;
            this.Btn_LiveImage.TabIndex = 8;
            this.Btn_LiveImage.Text = "实时图像";
            this.Btn_LiveImage.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_LiveImage.Click += new System.EventHandler(this.Btn_Click);
            // 
            // HeadPanel
            // 
            this.HeadPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.HeadPanel.ColumnCount = 5;
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            this.HeadPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.HeadPanel.Controls.Add(this.Btn_Run, 1, 0);
            this.HeadPanel.Controls.Add(this.Btn_LiveImage, 2, 0);
            this.HeadPanel.Controls.Add(this.Btn_StopLive, 3, 0);
            this.HeadPanel.Controls.Add(this.Btn_ToolSetting, 4, 0);
            this.HeadPanel.Controls.Add(this.LabelCameraName, 0, 0);
            this.HeadPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.HeadPanel.Location = new System.Drawing.Point(0, 0);
            this.HeadPanel.Margin = new System.Windows.Forms.Padding(0);
            this.HeadPanel.Name = "HeadPanel";
            this.HeadPanel.RowCount = 1;
            this.HeadPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.HeadPanel.Size = new System.Drawing.Size(648, 56);
            this.HeadPanel.TabIndex = 31;
            this.HeadPanel.TagString = null;
            // 
            // Btn_StopLive
            // 
            this.Btn_StopLive.CircleColor = System.Drawing.Color.Teal;
            this.Btn_StopLive.CircleSize = 36;
            this.Btn_StopLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Btn_StopLive.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Btn_StopLive.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_StopLive.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.Btn_StopLive.Location = new System.Drawing.Point(514, 0);
            this.Btn_StopLive.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Btn_StopLive.MinimumSize = new System.Drawing.Size(2, 2);
            this.Btn_StopLive.Name = "Btn_StopLive";
            this.Btn_StopLive.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.Btn_StopLive.Radius = 0;
            this.Btn_StopLive.RadiusSides = Sunny.UI.UICornerRadiusSides.None;
            this.Btn_StopLive.RectSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.None;
            this.Btn_StopLive.Size = new System.Drawing.Size(64, 56);
            this.Btn_StopLive.Style = Sunny.UI.UIStyle.Custom;
            this.Btn_StopLive.StyleCustomMode = true;
            this.Btn_StopLive.Symbol = 61517;
            this.Btn_StopLive.SymbolOffset = new System.Drawing.Point(1, 2);
            this.Btn_StopLive.SymbolSize = 32;
            this.Btn_StopLive.TabIndex = 9;
            this.Btn_StopLive.Text = "停止实时";
            this.Btn_StopLive.TipsFont = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Btn_StopLive.Click += new System.EventHandler(this.Btn_Click);
            // 
            // LabelCameraName
            // 
            this.LabelCameraName.AutoSize = true;
            this.LabelCameraName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LabelCameraName.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LabelCameraName.Location = new System.Drawing.Point(0, 0);
            this.LabelCameraName.Margin = new System.Windows.Forms.Padding(0);
            this.LabelCameraName.Name = "LabelCameraName";
            this.LabelCameraName.Size = new System.Drawing.Size(376, 56);
            this.LabelCameraName.TabIndex = 14;
            this.LabelCameraName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // RecordDisplay
            // 
            this.RecordDisplay.ColorMapLowerClipColor = System.Drawing.Color.Black;
            this.RecordDisplay.ColorMapLowerRoiLimit = 0D;
            this.RecordDisplay.ColorMapPredefined = Cognex.VisionPro.Display.CogDisplayColorMapPredefinedConstants.None;
            this.RecordDisplay.ColorMapUpperClipColor = System.Drawing.Color.Black;
            this.RecordDisplay.ColorMapUpperRoiLimit = 1D;
            this.RecordDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RecordDisplay.DoubleTapZoomCycleLength = 2;
            this.RecordDisplay.DoubleTapZoomSensitivity = 2.5D;
            this.RecordDisplay.Location = new System.Drawing.Point(0, 56);
            this.RecordDisplay.Margin = new System.Windows.Forms.Padding(0);
            this.RecordDisplay.MouseWheelMode = Cognex.VisionPro.Display.CogDisplayMouseWheelModeConstants.Zoom1;
            this.RecordDisplay.MouseWheelSensitivity = 1D;
            this.RecordDisplay.Name = "RecordDisplay";
            this.RecordDisplay.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("RecordDisplay.OcxState")));
            this.RecordDisplay.Size = new System.Drawing.Size(648, 360);
            this.RecordDisplay.TabIndex = 32;
            // 
            // ImageDisplay
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.RecordDisplay);
            this.Controls.Add(this.HeadPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ImageDisplay";
            this.Size = new System.Drawing.Size(648, 416);
            this.HeadPanel.ResumeLayout(false);
            this.HeadPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RecordDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private Sunny.UI.UIHeaderButton Btn_ToolSetting;
        private Sunny.UI.UIHeaderButton Btn_LiveImage;
        private Sunny.UI.UIHeaderButton Btn_Run;
        private Sunny.UI.UITableLayoutPanel HeadPanel;
        internal Cognex.VisionPro.CogRecordDisplay RecordDisplay;
        public Sunny.UI.UILabel LabelCameraName;
        private Sunny.UI.UIHeaderButton Btn_StopLive;
    }
}
