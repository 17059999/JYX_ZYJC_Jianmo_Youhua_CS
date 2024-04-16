namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class pipeBiaozhuForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_pipenei = new System.Windows.Forms.RadioButton();
            this.radioButton_cent = new System.Windows.Forms.RadioButton();
            this.radioButton_betton = new System.Windows.Forms.RadioButton();
            this.radioButton_top = new System.Windows.Forms.RadioButton();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_colse = new System.Windows.Forms.Button();
            this.radioButton_none = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton_none);
            this.groupBox1.Controls.Add(this.radioButton_pipenei);
            this.groupBox1.Controls.Add(this.radioButton_cent);
            this.groupBox1.Controls.Add(this.radioButton_betton);
            this.groupBox1.Controls.Add(this.radioButton_top);
            this.groupBox1.Location = new System.Drawing.Point(8, 9);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(250, 134);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "管道标注显示信息设置";
            // 
            // radioButton_pipenei
            // 
            this.radioButton_pipenei.AutoSize = true;
            this.radioButton_pipenei.Location = new System.Drawing.Point(7, 110);
            this.radioButton_pipenei.Name = "radioButton_pipenei";
            this.radioButton_pipenei.Size = new System.Drawing.Size(149, 16);
            this.radioButton_pipenei.TabIndex = 3;
            this.radioButton_pipenei.Text = "管道编号+管道底内标高";
            this.radioButton_pipenei.UseVisualStyleBackColor = true;
            // 
            // radioButton_cent
            // 
            this.radioButton_cent.AutoSize = true;
            this.radioButton_cent.Location = new System.Drawing.Point(7, 88);
            this.radioButton_cent.Name = "radioButton_cent";
            this.radioButton_cent.Size = new System.Drawing.Size(149, 16);
            this.radioButton_cent.TabIndex = 2;
            this.radioButton_cent.Text = "管道编号+管道中心标高";
            this.radioButton_cent.UseVisualStyleBackColor = true;
            // 
            // radioButton_betton
            // 
            this.radioButton_betton.AutoSize = true;
            this.radioButton_betton.Location = new System.Drawing.Point(7, 66);
            this.radioButton_betton.Name = "radioButton_betton";
            this.radioButton_betton.Size = new System.Drawing.Size(149, 16);
            this.radioButton_betton.TabIndex = 1;
            this.radioButton_betton.Text = "管道编号+管道管底标高";
            this.radioButton_betton.UseVisualStyleBackColor = true;
            // 
            // radioButton_top
            // 
            this.radioButton_top.AutoSize = true;
            this.radioButton_top.Location = new System.Drawing.Point(7, 44);
            this.radioButton_top.Name = "radioButton_top";
            this.radioButton_top.Size = new System.Drawing.Size(149, 16);
            this.radioButton_top.TabIndex = 0;
            this.radioButton_top.Text = "管道编号+管道管顶标高";
            this.radioButton_top.UseVisualStyleBackColor = true;
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(8, 149);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "确定";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // button_colse
            // 
            this.button_colse.Location = new System.Drawing.Point(183, 149);
            this.button_colse.Name = "button_colse";
            this.button_colse.Size = new System.Drawing.Size(75, 23);
            this.button_colse.TabIndex = 3;
            this.button_colse.Text = "取消";
            this.button_colse.UseVisualStyleBackColor = true;
            this.button_colse.Click += new System.EventHandler(this.button_colse_Click);
            // 
            // radioButton_none
            // 
            this.radioButton_none.AutoSize = true;
            this.radioButton_none.Checked = true;
            this.radioButton_none.Location = new System.Drawing.Point(7, 22);
            this.radioButton_none.Name = "radioButton_none";
            this.radioButton_none.Size = new System.Drawing.Size(71, 16);
            this.radioButton_none.TabIndex = 4;
            this.radioButton_none.TabStop = true;
            this.radioButton_none.Text = "管道编号";
            this.radioButton_none.UseVisualStyleBackColor = true;
            // 
            // pipeBiaozhuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(264, 177);
            this.Controls.Add(this.button_colse);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.groupBox1);
            this.Name = "pipeBiaozhuForm";
            this.Text = "管道标注";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_top;
        private System.Windows.Forms.RadioButton radioButton_pipenei;
        private System.Windows.Forms.RadioButton radioButton_cent;
        private System.Windows.Forms.RadioButton radioButton_betton;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_colse;
        private System.Windows.Forms.RadioButton radioButton_none;
    }
}