namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 自动清理多余图形
    /// </summary>
    partial class ElementClearForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElementClearForm));
            this.dataGridView_separateElement = new System.Windows.Forms.DataGridView();
            this.textBox_distance = new System.Windows.Forms.TextBox();
            this.label_distance = new System.Windows.Forms.Label();
            this.button_search = new System.Windows.Forms.Button();
            this.button_delete = new System.Windows.Forms.Button();
            this.button_locate = new System.Windows.Forms.Button();
            this.comboBox_tiaojian = new System.Windows.Forms.ComboBox();
            this.button_update_connection = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_separateElement)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView_separateElement
            // 
            this.dataGridView_separateElement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_separateElement.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView_separateElement.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView_separateElement.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView_separateElement.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_separateElement.Location = new System.Drawing.Point(14, 71);
            this.dataGridView_separateElement.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_separateElement.Name = "dataGridView_separateElement";
            this.dataGridView_separateElement.RowTemplate.Height = 23;
            this.dataGridView_separateElement.Size = new System.Drawing.Size(837, 282);
            this.dataGridView_separateElement.TabIndex = 5;
            // 
            // textBox_distance
            // 
            this.textBox_distance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_distance.Location = new System.Drawing.Point(513, 32);
            this.textBox_distance.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_distance.Name = "textBox_distance";
            this.textBox_distance.Size = new System.Drawing.Size(134, 27);
            this.textBox_distance.TabIndex = 3;
            // 
            // label_distance
            // 
            this.label_distance.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_distance.AutoSize = true;
            this.label_distance.Location = new System.Drawing.Point(324, 36);
            this.label_distance.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_distance.Name = "label_distance";
            this.label_distance.Size = new System.Drawing.Size(179, 17);
            this.label_distance.TabIndex = 2;
            this.label_distance.Text = "孤立元素判定间隔(mm)：";
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Location = new System.Drawing.Point(739, 28);
            this.button_search.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(112, 33);
            this.button_search.TabIndex = 4;
            this.button_search.Text = "筛选";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // button_delete
            // 
            this.button_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_delete.Location = new System.Drawing.Point(135, 28);
            this.button_delete.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(112, 33);
            this.button_delete.TabIndex = 1;
            this.button_delete.Text = "删除";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_delete_Click);
            // 
            // button_locate
            // 
            this.button_locate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_locate.Location = new System.Drawing.Point(14, 28);
            this.button_locate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_locate.Name = "button_locate";
            this.button_locate.Size = new System.Drawing.Size(112, 33);
            this.button_locate.TabIndex = 0;
            this.button_locate.Text = "定位";
            this.button_locate.UseVisualStyleBackColor = true;
            this.button_locate.Click += new System.EventHandler(this.button_locate_Click);
            // 
            // comboBox_tiaojian
            // 
            this.comboBox_tiaojian.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_tiaojian.FormattingEnabled = true;
            this.comboBox_tiaojian.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.comboBox_tiaojian.Items.AddRange(new object[] {
            "全部",
            "连接性",
            "距离"});
            this.comboBox_tiaojian.Location = new System.Drawing.Point(96, 33);
            this.comboBox_tiaojian.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.comboBox_tiaojian.Name = "comboBox_tiaojian";
            this.comboBox_tiaojian.Size = new System.Drawing.Size(157, 25);
            this.comboBox_tiaojian.TabIndex = 1;
            this.comboBox_tiaojian.SelectedIndexChanged += new System.EventHandler(this.comboBox_tiaojian_SelectedIndexChanged);
            // 
            // button_update_connection
            // 
            this.button_update_connection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_update_connection.Location = new System.Drawing.Point(256, 28);
            this.button_update_connection.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_update_connection.Name = "button_update_connection";
            this.button_update_connection.Size = new System.Drawing.Size(112, 33);
            this.button_update_connection.TabIndex = 2;
            this.button_update_connection.Text = "更新连接性";
            this.button_update_connection.UseVisualStyleBackColor = true;
            this.button_update_connection.Click += new System.EventHandler(this.button_update_connection_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.button_delete);
            this.groupBox1.Controls.Add(this.button_update_connection);
            this.groupBox1.Controls.Add(this.button_locate);
            this.groupBox1.Location = new System.Drawing.Point(13, 387);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox1.Size = new System.Drawing.Size(868, 80);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "其他功能";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(376, 28);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 33);
            this.button1.TabIndex = 3;
            this.button1.Text = "修复管嘴焊点";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.textBox_distance);
            this.groupBox2.Controls.Add(this.label_distance);
            this.groupBox2.Controls.Add(this.dataGridView_separateElement);
            this.groupBox2.Controls.Add(this.comboBox_tiaojian);
            this.groupBox2.Controls.Add(this.button_search);
            this.groupBox2.Location = new System.Drawing.Point(13, 14);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox2.Size = new System.Drawing.Size(868, 363);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "筛选元素";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 36);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "筛选条件：";
            // 
            // ElementClearForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(889, 476);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ElementClearForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "元素清除";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ElementClearForm_FormClosed);
            this.Load += new System.EventHandler(this.ElementClearForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_separateElement)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_separateElement;
        private System.Windows.Forms.TextBox textBox_distance;
        private System.Windows.Forms.Label label_distance;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.Button button_delete;
        private System.Windows.Forms.Button button_locate;
        private System.Windows.Forms.ComboBox comboBox_tiaojian;
        private System.Windows.Forms.Button button_update_connection;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
    }
}