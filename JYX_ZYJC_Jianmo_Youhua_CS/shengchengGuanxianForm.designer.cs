namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class shengchengGuanxianForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(shengchengGuanxianForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_kancedian_liulan = new System.Windows.Forms.Button();
            this.button_guanxian_liulan = new System.Windows.Forms.Button();
            this.textBox_kancedian_data_lujing = new System.Windows.Forms.TextBox();
            this.textBox_guanxian_wenjian_lujing = new System.Windows.Forms.TextBox();
            this.comboBox_guanxian_leixing = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_shengcheng_guanxian = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_kancedian_liulan);
            this.groupBox1.Controls.Add(this.button_guanxian_liulan);
            this.groupBox1.Controls.Add(this.textBox_kancedian_data_lujing);
            this.groupBox1.Controls.Add(this.textBox_guanxian_wenjian_lujing);
            this.groupBox1.Controls.Add(this.comboBox_guanxian_leixing);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(11, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(746, 155);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "选择勘察资料";
            // 
            // button_kancedian_liulan
            // 
            this.button_kancedian_liulan.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_kancedian_liulan.Location = new System.Drawing.Point(619, 101);
            this.button_kancedian_liulan.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_kancedian_liulan.Name = "button_kancedian_liulan";
            this.button_kancedian_liulan.Size = new System.Drawing.Size(112, 33);
            this.button_kancedian_liulan.TabIndex = 7;
            this.button_kancedian_liulan.Text = "浏览";
            this.button_kancedian_liulan.UseVisualStyleBackColor = true;
            this.button_kancedian_liulan.Click += new System.EventHandler(this.button_kancedian_liulan_Click);
            // 
            // button_guanxian_liulan
            // 
            this.button_guanxian_liulan.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_guanxian_liulan.Location = new System.Drawing.Point(619, 61);
            this.button_guanxian_liulan.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_guanxian_liulan.Name = "button_guanxian_liulan";
            this.button_guanxian_liulan.Size = new System.Drawing.Size(112, 33);
            this.button_guanxian_liulan.TabIndex = 4;
            this.button_guanxian_liulan.Text = "浏览";
            this.button_guanxian_liulan.UseVisualStyleBackColor = true;
            this.button_guanxian_liulan.Click += new System.EventHandler(this.button_guanxian_liulan_Click);
            // 
            // textBox_kancedian_data_lujing
            // 
            this.textBox_kancedian_data_lujing.Location = new System.Drawing.Point(162, 102);
            this.textBox_kancedian_data_lujing.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_kancedian_data_lujing.Name = "textBox_kancedian_data_lujing";
            this.textBox_kancedian_data_lujing.Size = new System.Drawing.Size(448, 27);
            this.textBox_kancedian_data_lujing.TabIndex = 6;
            // 
            // textBox_guanxian_wenjian_lujing
            // 
            this.textBox_guanxian_wenjian_lujing.Location = new System.Drawing.Point(162, 63);
            this.textBox_guanxian_wenjian_lujing.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_guanxian_wenjian_lujing.Name = "textBox_guanxian_wenjian_lujing";
            this.textBox_guanxian_wenjian_lujing.Size = new System.Drawing.Size(448, 27);
            this.textBox_guanxian_wenjian_lujing.TabIndex = 3;
            // 
            // comboBox_guanxian_leixing
            // 
            this.comboBox_guanxian_leixing.FormattingEnabled = true;
            this.comboBox_guanxian_leixing.Items.AddRange(new object[] {
            "给水",
            "污水",
            "雨水",
            "燃气",
            "电力",
            "电信",
            "路灯"});
            this.comboBox_guanxian_leixing.Location = new System.Drawing.Point(162, 25);
            this.comboBox_guanxian_leixing.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_guanxian_leixing.Name = "comboBox_guanxian_leixing";
            this.comboBox_guanxian_leixing.Size = new System.Drawing.Size(448, 25);
            this.comboBox_guanxian_leixing.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("华文中宋", 9F);
            this.label3.Location = new System.Drawing.Point(26, 105);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(128, 17);
            this.label3.TabIndex = 5;
            this.label3.Text = "勘测点数据路径：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("华文中宋", 9F);
            this.label2.Location = new System.Drawing.Point(43, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "管线数据路径：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("华文中宋", 9F);
            this.label1.Location = new System.Drawing.Point(76, 29);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "管线类型：";
            // 
            // button_shengcheng_guanxian
            // 
            this.button_shengcheng_guanxian.Font = new System.Drawing.Font("华文中宋", 9F);
            this.button_shengcheng_guanxian.Location = new System.Drawing.Point(172, 162);
            this.button_shengcheng_guanxian.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_shengcheng_guanxian.Name = "button_shengcheng_guanxian";
            this.button_shengcheng_guanxian.Size = new System.Drawing.Size(202, 35);
            this.button_shengcheng_guanxian.TabIndex = 1;
            this.button_shengcheng_guanxian.Text = "一键生成综合管线";
            this.button_shengcheng_guanxian.UseVisualStyleBackColor = true;
            this.button_shengcheng_guanxian.Click += new System.EventHandler(this.button_shengcheng_guanxian_Click);
            // 
            // shengchengGuanxianForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(767, 205);
            this.Controls.Add(this.button_shengcheng_guanxian);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(785, 252);
            this.MinimumSize = new System.Drawing.Size(785, 252);
            this.Name = "shengchengGuanxianForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "一键生成综合管线";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }



        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_shengcheng_guanxian;
        private System.Windows.Forms.Button button_kancedian_liulan;
        private System.Windows.Forms.Button button_guanxian_liulan;
        private System.Windows.Forms.TextBox textBox_kancedian_data_lujing;
        private System.Windows.Forms.TextBox textBox_guanxian_wenjian_lujing;
        private System.Windows.Forms.ComboBox comboBox_guanxian_leixing;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
    }
}