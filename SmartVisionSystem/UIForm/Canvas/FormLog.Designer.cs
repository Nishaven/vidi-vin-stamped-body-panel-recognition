
namespace SmartVisionSystem.UIForm.Canvas
{
    partial class FormLog
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.LogDataGridView = new Sunny.UI.UIDataGridView();
            this.TimeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LevelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColorColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.LogDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // LogDataGridView
            // 
            this.LogDataGridView.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.LogDataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.LogDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.LogDataGridView.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.LogDataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            this.LogDataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogDataGridView.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.LogDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.LogDataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.LogDataGridView.ColumnHeadersHeight = 28;
            this.LogDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.LogDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.TimeColumn,
            this.LevelColumn,
            this.MessageColumn,
            this.ColorColumn});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("微软雅黑", 12F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.LogDataGridView.DefaultCellStyle = dataGridViewCellStyle3;
            this.LogDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogDataGridView.EnableHeadersVisualStyles = false;
            this.LogDataGridView.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.LogDataGridView.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(173)))), ((int)(((byte)(255)))));
            this.LogDataGridView.Location = new System.Drawing.Point(0, 35);
            this.LogDataGridView.Name = "LogDataGridView";
            this.LogDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(249)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.LogDataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.LogDataGridView.RowHeadersVisible = false;
            this.LogDataGridView.RowHeadersWidth = 62;
            this.LogDataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            dataGridViewCellStyle5.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(236)))), ((int)(((byte)(255)))));
            dataGridViewCellStyle5.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.LogDataGridView.RowsDefaultCellStyle = dataGridViewCellStyle5;
            this.LogDataGridView.RowTemplate.Height = 23;
            this.LogDataGridView.ScrollBarRectColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(160)))), ((int)(((byte)(255)))));
            this.LogDataGridView.SelectedIndex = -1;
            this.LogDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.LogDataGridView.Size = new System.Drawing.Size(479, 466);
            this.LogDataGridView.Style = Sunny.UI.UIStyle.Custom;
            this.LogDataGridView.StyleCustomMode = true;
            this.LogDataGridView.TabIndex = 0;
            // 
            // TimeColumn
            // 
            this.TimeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.TimeColumn.DataPropertyName = "DateTime";
            this.TimeColumn.HeaderText = "日期时间";
            this.TimeColumn.MinimumWidth = 8;
            this.TimeColumn.Name = "TimeColumn";
            this.TimeColumn.ReadOnly = true;
            this.TimeColumn.Width = 190;
            // 
            // LevelColumn
            // 
            this.LevelColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.LevelColumn.DataPropertyName = "LogLevel";
            this.LevelColumn.HeaderText = "等级";
            this.LevelColumn.MinimumWidth = 8;
            this.LevelColumn.Name = "LevelColumn";
            this.LevelColumn.ReadOnly = true;
            this.LevelColumn.Width = 126;
            // 
            // MessageColumn
            // 
            this.MessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.MessageColumn.DataPropertyName = "LogText";
            this.MessageColumn.HeaderText = "信息";
            this.MessageColumn.MinimumWidth = 8;
            this.MessageColumn.Name = "MessageColumn";
            this.MessageColumn.ReadOnly = true;
            // 
            // ColorColumn
            // 
            this.ColorColumn.DataPropertyName = "TextColor";
            this.ColorColumn.HeaderText = "颜色";
            this.ColorColumn.MinimumWidth = 8;
            this.ColorColumn.Name = "ColorColumn";
            this.ColorColumn.ReadOnly = true;
            this.ColorColumn.Visible = false;
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // FormLog
            // 
            this.AllowShowTitle = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(479, 501);
            this.Controls.Add(this.LogDataGridView);
            this.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "FormLog";
            this.Padding = new System.Windows.Forms.Padding(0, 35, 0, 0);
            this.ShowTitle = true;
            this.StyleCustomMode = true;
            this.Symbol = 61474;
            this.SymbolSize = 28;
            this.Text = "日志信息";
            this.TitleFont = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            ((System.ComponentModel.ISupportInitialize)(this.LogDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        public Sunny.UI.UIDataGridView LogDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn TimeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn LevelColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn MessageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColorColumn;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}