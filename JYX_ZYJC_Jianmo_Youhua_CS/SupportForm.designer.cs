namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class SupportForm
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
            this.checkBox_setParam = new System.Windows.Forms.CheckBox();
            this.comboBox_profile = new System.Windows.Forms.ComboBox();
            this.label_profile = new System.Windows.Forms.Label();
            this.textBox_length = new System.Windows.Forms.TextBox();
            this.lebel_length = new System.Windows.Forms.Label();
            this.lebel_height = new System.Windows.Forms.Label();
            this.lebel_height1 = new System.Windows.Forms.Label();
            this.textBox_height = new System.Windows.Forms.TextBox();
            this.textBox_height1 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_setParam
            // 
            this.checkBox_setParam.AutoSize = true;
            this.checkBox_setParam.Location = new System.Drawing.Point(16, 25);
            this.checkBox_setParam.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_setParam.Name = "checkBox_setParam";
            this.checkBox_setParam.Size = new System.Drawing.Size(59, 19);
            this.checkBox_setParam.TabIndex = 0;
            this.checkBox_setParam.Text = "激活";
            this.checkBox_setParam.UseVisualStyleBackColor = true;
            this.checkBox_setParam.CheckedChanged += new System.EventHandler(this.checkBox_setParam_CheckedChanged);
            // 
            // comboBox_profile
            // 
            this.comboBox_profile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_profile.FormattingEnabled = true;
            this.comboBox_profile.Location = new System.Drawing.Point(181, 30);
            this.comboBox_profile.Margin = new System.Windows.Forms.Padding(4);
            this.comboBox_profile.Name = "comboBox_profile";
            this.comboBox_profile.Size = new System.Drawing.Size(249, 23);
            this.comboBox_profile.TabIndex = 1;
            this.comboBox_profile.SelectedIndexChanged += new System.EventHandler(this.comboBox_profile_SelectedIndexChanged);
            // 
            // label_profile
            // 
            this.label_profile.AutoSize = true;
            this.label_profile.Location = new System.Drawing.Point(43, 33);
            this.label_profile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_profile.Name = "label_profile";
            this.label_profile.Size = new System.Drawing.Size(82, 15);
            this.label_profile.TabIndex = 0;
            this.label_profile.Text = "选择截面：";
            // 
            // textBox_length
            // 
            this.textBox_length.Location = new System.Drawing.Point(166, 58);
            this.textBox_length.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_length.Name = "textBox_length";
            this.textBox_length.Size = new System.Drawing.Size(249, 25);
            this.textBox_length.TabIndex = 2;
            this.textBox_length.TextChanged += new System.EventHandler(this.textBox_length_TextChanged);
            // 
            // lebel_length
            // 
            this.lebel_length.AutoSize = true;
            this.lebel_length.Location = new System.Drawing.Point(80, 61);
            this.lebel_length.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lebel_length.Name = "lebel_length";
            this.lebel_length.Size = new System.Drawing.Size(30, 15);
            this.lebel_length.TabIndex = 1;
            this.lebel_length.Text = "L：";
            // 
            // lebel_height
            // 
            this.lebel_height.AutoSize = true;
            this.lebel_height.Location = new System.Drawing.Point(80, 95);
            this.lebel_height.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lebel_height.Name = "lebel_height";
            this.lebel_height.Size = new System.Drawing.Size(30, 15);
            this.lebel_height.TabIndex = 3;
            this.lebel_height.Text = "H：";
            // 
            // lebel_height1
            // 
            this.lebel_height1.AutoSize = true;
            this.lebel_height1.Location = new System.Drawing.Point(72, 129);
            this.lebel_height1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lebel_height1.Name = "lebel_height1";
            this.lebel_height1.Size = new System.Drawing.Size(38, 15);
            this.lebel_height1.TabIndex = 5;
            this.lebel_height1.Text = "H1：";
            // 
            // textBox_height
            // 
            this.textBox_height.Location = new System.Drawing.Point(166, 91);
            this.textBox_height.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_height.Name = "textBox_height";
            this.textBox_height.Size = new System.Drawing.Size(249, 25);
            this.textBox_height.TabIndex = 4;
            this.textBox_height.TextChanged += new System.EventHandler(this.textBox_height_TextChanged);
            // 
            // textBox_height1
            // 
            this.textBox_height1.Location = new System.Drawing.Point(166, 129);
            this.textBox_height1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_height1.Name = "textBox_height1";
            this.textBox_height1.Size = new System.Drawing.Size(249, 25);
            this.textBox_height1.TabIndex = 6;
            this.textBox_height1.TextChanged += new System.EventHandler(this.textBox_height1_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_setParam);
            this.groupBox1.Controls.Add(this.textBox_height1);
            this.groupBox1.Controls.Add(this.lebel_length);
            this.groupBox1.Controls.Add(this.textBox_height);
            this.groupBox1.Controls.Add(this.lebel_height);
            this.groupBox1.Controls.Add(this.textBox_length);
            this.groupBox1.Controls.Add(this.lebel_height1);
            this.groupBox1.Location = new System.Drawing.Point(4, 62);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(479, 173);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "设置参数";
            // 
            // SupportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label_profile);
            this.Controls.Add(this.comboBox_profile);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SupportForm";
            this.Size = new System.Drawing.Size(487, 239);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.CheckBox checkBox_setParam;
        public System.Windows.Forms.ComboBox comboBox_profile;
        public System.Windows.Forms.Label label_profile;
        public System.Windows.Forms.TextBox textBox_length;
        public System.Windows.Forms.Label lebel_length;
        public System.Windows.Forms.Label lebel_height;
        public System.Windows.Forms.Label lebel_height1;
        public System.Windows.Forms.TextBox textBox_height;
        public System.Windows.Forms.TextBox textBox_height1;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}