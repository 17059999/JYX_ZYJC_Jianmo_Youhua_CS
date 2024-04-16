namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class KeepOutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeepOutForm));
            this.keepOutRadioButton = new System.Windows.Forms.RadioButton();
            this.noKeepOutRadioButton = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // keepOutRadioButton
            // 
            this.keepOutRadioButton.AutoSize = true;
            this.keepOutRadioButton.Location = new System.Drawing.Point(6, 20);
            this.keepOutRadioButton.Name = "keepOutRadioButton";
            this.keepOutRadioButton.Size = new System.Drawing.Size(47, 16);
            this.keepOutRadioButton.TabIndex = 0;
            this.keepOutRadioButton.TabStop = true;
            this.keepOutRadioButton.Text = "遮挡";
            this.keepOutRadioButton.UseVisualStyleBackColor = true;
            this.keepOutRadioButton.CheckedChanged += new System.EventHandler(this.keepOutRadioButton_CheckedChanged);
            // 
            // noKeepOutRadioButton
            // 
            this.noKeepOutRadioButton.AutoSize = true;
            this.noKeepOutRadioButton.Location = new System.Drawing.Point(151, 20);
            this.noKeepOutRadioButton.Name = "noKeepOutRadioButton";
            this.noKeepOutRadioButton.Size = new System.Drawing.Size(59, 16);
            this.noKeepOutRadioButton.TabIndex = 1;
            this.noKeepOutRadioButton.TabStop = true;
            this.noKeepOutRadioButton.Text = "不遮挡";
            this.noKeepOutRadioButton.UseVisualStyleBackColor = true;
            this.noKeepOutRadioButton.CheckedChanged += new System.EventHandler(this.noKeepOutRadioButton_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.noKeepOutRadioButton);
            this.groupBox1.Controls.Add(this.keepOutRadioButton);
            this.groupBox1.Location = new System.Drawing.Point(13, 10);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 54);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "遮挡关系";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButton2);
            this.groupBox2.Controls.Add(this.radioButton1);
            this.groupBox2.Location = new System.Drawing.Point(13, 73);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(227, 57);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "中心线";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(137, 20);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(83, 16);
            this.radioButton2.TabIndex = 3;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "隐藏中心线";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(6, 20);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(83, 16);
            this.radioButton1.TabIndex = 2;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "显示中心线";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // KeepOutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(254, 144);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "KeepOutForm";
            this.Text = "出图控制";
            this.Load += new System.EventHandler(this.KeepOutForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton keepOutRadioButton;
        private System.Windows.Forms.RadioButton noKeepOutRadioButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
    }
}