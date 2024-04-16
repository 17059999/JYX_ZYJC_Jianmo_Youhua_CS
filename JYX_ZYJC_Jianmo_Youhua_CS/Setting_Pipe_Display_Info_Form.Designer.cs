namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class Setting_Pipe_Display_Info_Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Setting_Pipe_Display_Info_Form));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_inside_bottom_level = new System.Windows.Forms.CheckBox();
            this.checkBox_top_level = new System.Windows.Forms.CheckBox();
            this.checkBox_bottom_level = new System.Windows.Forms.CheckBox();
            this.checkBox_center_level = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.checkBox_inside_bottom_level);
            this.groupBox1.Controls.Add(this.checkBox_top_level);
            this.groupBox1.Controls.Add(this.checkBox_bottom_level);
            this.groupBox1.Controls.Add(this.checkBox_center_level);
            this.groupBox1.Location = new System.Drawing.Point(13, 14);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(282, 204);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "管道显示信息设置";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(25, 49);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "管道显示内容：";
            // 
            // checkBox_inside_bottom_level
            // 
            this.checkBox_inside_bottom_level.AutoSize = true;
            this.checkBox_inside_bottom_level.Location = new System.Drawing.Point(161, 145);
            this.checkBox_inside_bottom_level.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox_inside_bottom_level.Name = "checkBox_inside_bottom_level";
            this.checkBox_inside_bottom_level.Size = new System.Drawing.Size(105, 21);
            this.checkBox_inside_bottom_level.TabIndex = 4;
            this.checkBox_inside_bottom_level.Text = "管内底标高";
            this.checkBox_inside_bottom_level.UseVisualStyleBackColor = true;
            // 
            // checkBox_top_level
            // 
            this.checkBox_top_level.AutoSize = true;
            this.checkBox_top_level.Location = new System.Drawing.Point(28, 95);
            this.checkBox_top_level.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox_top_level.Name = "checkBox_top_level";
            this.checkBox_top_level.Size = new System.Drawing.Size(75, 21);
            this.checkBox_top_level.TabIndex = 1;
            this.checkBox_top_level.Text = "顶标高";
            this.checkBox_top_level.UseVisualStyleBackColor = true;
            // 
            // checkBox_bottom_level
            // 
            this.checkBox_bottom_level.AutoSize = true;
            this.checkBox_bottom_level.Location = new System.Drawing.Point(161, 95);
            this.checkBox_bottom_level.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox_bottom_level.Name = "checkBox_bottom_level";
            this.checkBox_bottom_level.Size = new System.Drawing.Size(75, 21);
            this.checkBox_bottom_level.TabIndex = 2;
            this.checkBox_bottom_level.Text = "底标高";
            this.checkBox_bottom_level.UseVisualStyleBackColor = true;
            // 
            // checkBox_center_level
            // 
            this.checkBox_center_level.AutoSize = true;
            this.checkBox_center_level.Location = new System.Drawing.Point(28, 145);
            this.checkBox_center_level.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox_center_level.Name = "checkBox_center_level";
            this.checkBox_center_level.Size = new System.Drawing.Size(90, 21);
            this.checkBox_center_level.TabIndex = 3;
            this.checkBox_center_level.Text = "中心标高";
            this.checkBox_center_level.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(13, 228);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 33);
            this.button1.TabIndex = 1;
            this.button1.Text = "保存";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(183, 228);
            this.button2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 33);
            this.button2.TabIndex = 2;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Setting_Pipe_Display_Info_Form
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(311, 273);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(329, 320);
            this.MinimumSize = new System.Drawing.Size(329, 320);
            this.Name = "Setting_Pipe_Display_Info_Form";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "管道显示信息设置";
            this.Load += new System.EventHandler(this.Setting_Pipe_Display_Info_Form_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_inside_bottom_level;
        private System.Windows.Forms.CheckBox checkBox_top_level;
        private System.Windows.Forms.CheckBox checkBox_bottom_level;
        private System.Windows.Forms.CheckBox checkBox_center_level;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}