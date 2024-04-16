namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class MovePipesToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovePipesToolForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton_absolute = new System.Windows.Forms.RadioButton();
            this.radioButton_offset = new System.Windows.Forms.RadioButton();
            this.checkBox_isWorking = new System.Windows.Forms.CheckBox();
            this.label_offsetz = new System.Windows.Forms.Label();
            this.label_offsety = new System.Windows.Forms.Label();
            this.label_offsetx = new System.Windows.Forms.Label();
            this.textBox_Z = new System.Windows.Forms.TextBox();
            this.textBox_Y = new System.Windows.Forms.TextBox();
            this.textBox_X = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioButton_absolute);
            this.groupBox1.Controls.Add(this.radioButton_offset);
            this.groupBox1.Controls.Add(this.checkBox_isWorking);
            this.groupBox1.Controls.Add(this.label_offsetz);
            this.groupBox1.Controls.Add(this.label_offsety);
            this.groupBox1.Controls.Add(this.label_offsetx);
            this.groupBox1.Controls.Add(this.textBox_Z);
            this.groupBox1.Controls.Add(this.textBox_Y);
            this.groupBox1.Controls.Add(this.textBox_X);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(272, 258);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "移动管道";
            // 
            // radioButton_absolute
            // 
            this.radioButton_absolute.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.radioButton_absolute.AutoSize = true;
            this.radioButton_absolute.Enabled = false;
            this.radioButton_absolute.Location = new System.Drawing.Point(162, 88);
            this.radioButton_absolute.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_absolute.Name = "radioButton_absolute";
            this.radioButton_absolute.Size = new System.Drawing.Size(89, 21);
            this.radioButton_absolute.TabIndex = 2;
            this.radioButton_absolute.Text = "绝对坐标";
            this.radioButton_absolute.UseVisualStyleBackColor = true;
            // 
            // radioButton_offset
            // 
            this.radioButton_offset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButton_offset.AutoSize = true;
            this.radioButton_offset.Checked = true;
            this.radioButton_offset.Enabled = false;
            this.radioButton_offset.Location = new System.Drawing.Point(24, 88);
            this.radioButton_offset.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.radioButton_offset.Name = "radioButton_offset";
            this.radioButton_offset.Size = new System.Drawing.Size(89, 21);
            this.radioButton_offset.TabIndex = 1;
            this.radioButton_offset.TabStop = true;
            this.radioButton_offset.Text = "相对坐标";
            this.radioButton_offset.UseVisualStyleBackColor = true;
            // 
            // checkBox_isWorking
            // 
            this.checkBox_isWorking.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.checkBox_isWorking.AutoSize = true;
            this.checkBox_isWorking.Location = new System.Drawing.Point(24, 45);
            this.checkBox_isWorking.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkBox_isWorking.Name = "checkBox_isWorking";
            this.checkBox_isWorking.Size = new System.Drawing.Size(60, 21);
            this.checkBox_isWorking.TabIndex = 0;
            this.checkBox_isWorking.Text = "启用";
            this.checkBox_isWorking.UseVisualStyleBackColor = true;
            this.checkBox_isWorking.CheckedChanged += new System.EventHandler(this.checkBox_isWorking_CheckedChanged);
            // 
            // label_offsetz
            // 
            this.label_offsetz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label_offsetz.AutoSize = true;
            this.label_offsetz.Location = new System.Drawing.Point(22, 207);
            this.label_offsetz.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_offsetz.Name = "label_offsetz";
            this.label_offsetz.Size = new System.Drawing.Size(33, 17);
            this.label_offsetz.TabIndex = 7;
            this.label_offsetz.Text = "Z：";
            // 
            // label_offsety
            // 
            this.label_offsety.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label_offsety.AutoSize = true;
            this.label_offsety.Location = new System.Drawing.Point(22, 173);
            this.label_offsety.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_offsety.Name = "label_offsety";
            this.label_offsety.Size = new System.Drawing.Size(33, 17);
            this.label_offsety.TabIndex = 5;
            this.label_offsety.Text = "Y：";
            // 
            // label_offsetx
            // 
            this.label_offsetx.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label_offsetx.AutoSize = true;
            this.label_offsetx.Location = new System.Drawing.Point(21, 134);
            this.label_offsetx.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_offsetx.Name = "label_offsetx";
            this.label_offsetx.Size = new System.Drawing.Size(34, 17);
            this.label_offsetx.TabIndex = 3;
            this.label_offsetx.Text = "X：";
            // 
            // textBox_Z
            // 
            this.textBox_Z.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Z.Location = new System.Drawing.Point(65, 210);
            this.textBox_Z.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_Z.Name = "textBox_Z";
            this.textBox_Z.Size = new System.Drawing.Size(186, 27);
            this.textBox_Z.TabIndex = 8;
            this.textBox_Z.Text = "0";
            // 
            // textBox_Y
            // 
            this.textBox_Y.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Y.Location = new System.Drawing.Point(65, 173);
            this.textBox_Y.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_Y.Name = "textBox_Y";
            this.textBox_Y.Size = new System.Drawing.Size(186, 27);
            this.textBox_Y.TabIndex = 6;
            this.textBox_Y.Text = "0";
            // 
            // textBox_X
            // 
            this.textBox_X.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_X.Location = new System.Drawing.Point(65, 131);
            this.textBox_X.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_X.Name = "textBox_X";
            this.textBox_X.Size = new System.Drawing.Size(186, 27);
            this.textBox_X.TabIndex = 4;
            this.textBox_X.Text = "0";
            // 
            // MovePipesToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(294, 282);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(312, 329);
            this.MinimumSize = new System.Drawing.Size(312, 329);
            this.Name = "MovePipesToolForm";
            this.Text = "移动管道";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MovePipesToolForm_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        public System.Windows.Forms.RadioButton radioButton_absolute;
        public System.Windows.Forms.RadioButton radioButton_offset;
        public System.Windows.Forms.CheckBox checkBox_isWorking;
        private System.Windows.Forms.Label label_offsetz;
        private System.Windows.Forms.Label label_offsety;
        private System.Windows.Forms.Label label_offsetx;
        public System.Windows.Forms.TextBox textBox_Z;
        public System.Windows.Forms.TextBox textBox_Y;
        public System.Windows.Forms.TextBox textBox_X;
    }
}