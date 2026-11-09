
namespace SmartVisionSystem.UIForm
{
    partial class SplashScreenForm
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
            this.LabelStatusInfor = new Sunny.UI.UILabel();
            this.uiProgressIndicator1 = new Sunny.UI.UIProgressIndicator();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.uiStyleManager1 = new Sunny.UI.UIStyleManager(this.components);
            this.SuspendLayout();
            // 
            // LabelStatusInfor
            // 
            this.LabelStatusInfor.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.LabelStatusInfor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(100)))));
            this.LabelStatusInfor.Location = new System.Drawing.Point(110, 780);
            this.LabelStatusInfor.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.LabelStatusInfor.Name = "LabelStatusInfor";
            this.LabelStatusInfor.Size = new System.Drawing.Size(1284, 46);
            this.LabelStatusInfor.Style = Sunny.UI.UIStyle.Custom;
            this.LabelStatusInfor.TabIndex = 0;
            this.LabelStatusInfor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiProgressIndicator1
            // 
            this.uiProgressIndicator1.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiProgressIndicator1.Location = new System.Drawing.Point(602, 366);
            this.uiProgressIndicator1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.uiProgressIndicator1.MinimumSize = new System.Drawing.Size(2, 2);
            this.uiProgressIndicator1.Name = "uiProgressIndicator1";
            this.uiProgressIndicator1.Size = new System.Drawing.Size(300, 300);
            this.uiProgressIndicator1.TabIndex = 1;
            this.uiProgressIndicator1.Text = "uiProgressIndicator1";
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uiLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.uiLabel1.Location = new System.Drawing.Point(180, 108);
            this.uiLabel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(1144, 240);
            this.uiLabel1.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel1.TabIndex = 2;
            this.uiLabel1.Text = "SmartVision";
            this.uiLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.uiLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(100)))));
            this.uiLabel2.Location = new System.Drawing.Point(464, 692);
            this.uiLabel2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(576, 46);
            this.uiLabel2.Style = Sunny.UI.UIStyle.Custom;
            this.uiLabel2.TabIndex = 3;
            this.uiLabel2.Text = "More Power More Easy, To Be More";
            this.uiLabel2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // uiStyleManager1
            // 
            this.uiStyleManager1.DPIScale = true;
            // 
            // SplashScreenForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            this.ClientSize = new System.Drawing.Size(1506, 900);
            this.ControlBox = false;
            this.Controls.Add(this.uiLabel2);
            this.Controls.Add(this.uiLabel1);
            this.Controls.Add(this.uiProgressIndicator1);
            this.Controls.Add(this.LabelStatusInfor);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.Name = "SplashScreenForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SplashScreenForm";
            this.ResumeLayout(false);

        }

        #endregion

        private Sunny.UI.UILabel LabelStatusInfor;
        private Sunny.UI.UIProgressIndicator uiProgressIndicator1;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UIStyleManager uiStyleManager1;
    }
}