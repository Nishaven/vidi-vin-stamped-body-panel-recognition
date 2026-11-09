
using Sunny.UI;

namespace SmartVisionSystem.UIForm.Pages
{
    partial class FormCamera
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle23 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle24 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle31 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle32 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle33 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle25 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle26 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle27 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle28 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle29 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle30 = new System.Windows.Forms.DataGridViewCellStyle();
            this.uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            this.ResultDataGridView = new Sunny.UI.UIDataGridView();
            this.TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CameraNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.CarModelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FXColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VINCodeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ResultColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.uiTableLayoutPanel1 = new Sunny.UI.UITableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.uiTitlePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ResultDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // uiTitlePanel1
            // 
            this.uiTitlePanel1.Controls.Add(this.ResultDataGridView);
            this.uiTitlePanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTitlePanel1.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTitlePanel1.Location = new System.Drawing.Point(0, 0);
            this.uiTitlePanel1.Margin = new System.Windows.Forms.Padding(1);
            this.uiTitlePanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiTitlePanel1.Name = "uiTitlePanel1";
            this.uiTitlePanel1.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.uiTitlePanel1.ShowText = false;
            this.uiTitlePanel1.Size = new System.Drawing.Size(504, 615);
            this.uiTitlePanel1.TabIndex = 1;
            this.uiTitlePanel1.Text = "结果显示";
            this.uiTitlePanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ResultDataGridView
            // 
            this.ResultDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle23.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.ResultDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle23;
            this.ResultDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.ResultDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.ResultDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ResultDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.ResultDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle24.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle24.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle24.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle24.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle24.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle24.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle24.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ResultDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle24;
            this.ResultDataGridView.ColumnHeadersHeight = 34;
            this.ResultDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.ResultDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TimeColumn,
            this.CameraNameColumn,
            this.CarModelColumn,
            this.FXColumn,
            this.VINCodeColumn,
            this.ResultColumn});
            dataGridViewCellStyle31.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle31.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle31.Font = new System.Drawing.Font("微软雅黑", 9F);
            dataGridViewCellStyle31.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle31.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle31.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle31.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.ResultDataGridView.DefaultCellStyle = dataGridViewCellStyle31;
            this.ResultDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResultDataGridView.EnableHeadersVisualStyles = false;
            this.ResultDataGridView.Font = new System.Drawing.Font("微软雅黑", 9F);
            this.ResultDataGridView.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(173)))), ((int)(((byte)(255)))));
            this.ResultDataGridView.Location = new System.Drawing.Point(0, 35);
            this.ResultDataGridView.Margin = new System.Windows.Forms.Padding(1);
            this.ResultDataGridView.Name = "ResultDataGridView";
            this.ResultDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle32.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle32.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle32.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle32.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle32.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle32.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle32.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ResultDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle32;
            this.ResultDataGridView.RowHeadersVisible = false;
            this.ResultDataGridView.RowHeadersWidth = 62;
            this.ResultDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle33.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle33.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle33.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle33.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle33.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.ResultDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle33;
            this.ResultDataGridView.RowTemplate.Height = 23;
            this.ResultDataGridView.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.ResultDataGridView.SelectedIndex = -1;
            this.ResultDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.ResultDataGridView.Size = new System.Drawing.Size(504, 580);
            this.ResultDataGridView.Style = Sunny.UI.UIStyle.Custom;
            this.ResultDataGridView.StyleCustomMode = true;
            this.ResultDataGridView.TabIndex = 8;
            // 
            // TimeColumn
            // 
            this.TimeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.TimeColumn.DataPropertyName = "RecordTime";
            dataGridViewCellStyle25.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.TimeColumn.DefaultCellStyle = dataGridViewCellStyle25;
            this.TimeColumn.Frozen = true;
            this.TimeColumn.HeaderText = "记录时间";
            this.TimeColumn.MinimumWidth = 8;
            this.TimeColumn.Name = "TimeColumn";
            this.TimeColumn.ReadOnly = true;
            this.TimeColumn.Width = 80;
            // 
            // CameraNameColumn
            // 
            this.CameraNameColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.CameraNameColumn.DataPropertyName = "CameraName";
            dataGridViewCellStyle26.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.CameraNameColumn.DefaultCellStyle = dataGridViewCellStyle26;
            this.CameraNameColumn.HeaderText = "拍照相机";
            this.CameraNameColumn.MinimumWidth = 8;
            this.CameraNameColumn.Name = "CameraNameColumn";
            this.CameraNameColumn.ReadOnly = true;
            this.CameraNameColumn.Width = 80;
            // 
            // CarModelColumn
            // 
            this.CarModelColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.CarModelColumn.DataPropertyName = "CarModel";
            dataGridViewCellStyle27.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.CarModelColumn.DefaultCellStyle = dataGridViewCellStyle27;
            this.CarModelColumn.HeaderText = "车型";
            this.CarModelColumn.MinimumWidth = 8;
            this.CarModelColumn.Name = "CarModelColumn";
            this.CarModelColumn.ReadOnly = true;
            this.CarModelColumn.Width = 56;
            // 
            // FXColumn
            // 
            this.FXColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.FXColumn.DataPropertyName = "FX";
            dataGridViewCellStyle28.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.FXColumn.DefaultCellStyle = dataGridViewCellStyle28;
            this.FXColumn.HeaderText = "返修结果";
            this.FXColumn.MinimumWidth = 8;
            this.FXColumn.Name = "FXColumn";
            this.FXColumn.ReadOnly = true;
            this.FXColumn.Width = 80;
            // 
            // VINCodeColumn
            // 
            this.VINCodeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.VINCodeColumn.DataPropertyName = "VINCode";
            dataGridViewCellStyle29.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.VINCodeColumn.DefaultCellStyle = dataGridViewCellStyle29;
            this.VINCodeColumn.HeaderText = "VIN码";
            this.VINCodeColumn.MinimumWidth = 8;
            this.VINCodeColumn.Name = "VINCodeColumn";
            this.VINCodeColumn.ReadOnly = true;
            // 
            // ResultColumn
            // 
            this.ResultColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ResultColumn.DataPropertyName = "Result";
            dataGridViewCellStyle30.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.ResultColumn.DefaultCellStyle = dataGridViewCellStyle30;
            this.ResultColumn.HeaderText = "结果";
            this.ResultColumn.MinimumWidth = 8;
            this.ResultColumn.Name = "ResultColumn";
            this.ResultColumn.ReadOnly = true;
            this.ResultColumn.Width = 56;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.uiTitlePanel1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(1114, 35);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(506, 617);
            this.panel2.TabIndex = 4;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(1104, 35);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(10, 617);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // uiTableLayoutPanel1
            // 
            this.uiTableLayoutPanel1.ColumnCount = 3;
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.uiTableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.uiTableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.uiTableLayoutPanel1.Name = "uiTableLayoutPanel1";
            this.uiTableLayoutPanel1.RowCount = 3;
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.uiTableLayoutPanel1.Size = new System.Drawing.Size(1104, 617);
            this.uiTableLayoutPanel1.TabIndex = 0;
            this.uiTableLayoutPanel1.TagString = null;
            this.uiTableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.uiTableLayoutPanel1_Paint);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.uiTableLayoutPanel1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 35);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1104, 617);
            this.panel1.TabIndex = 6;
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // FormCamera
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1620, 652);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel2);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormCamera";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.PageIndex = 999;
            this.ShowTitle = true;
            this.StyleCustomMode = true;
            this.Symbol = 62056;
            this.SymbolSize = 28;
            this.Text = "主界面";
            this.TitleFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.TitleFont = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.uiTitlePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ResultDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private UITitlePanel uiTitlePanel1;
        public UIDataGridView ResultDataGridView;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Splitter splitter1;
        private UITableLayoutPanel uiTableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private UIStyleManager uiStyleManager1;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CameraNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn CarModelColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn FXColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn VINCodeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ResultColumn;
    }
}