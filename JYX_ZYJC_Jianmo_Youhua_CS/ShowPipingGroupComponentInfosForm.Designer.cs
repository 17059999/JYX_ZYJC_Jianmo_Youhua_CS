namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class ShowPipingGroupComponentInfosForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowPipingGroupComponentInfosForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridView_unconnected_piping = new System.Windows.Forms.DataGridView();
            this.elem_selected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.xuhao = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_unconnected_piping)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dataGridView_unconnected_piping);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(335, 335);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "首尾选中管道";
            // 
            // dataGridView_unconnected_piping
            // 
            this.dataGridView_unconnected_piping.AllowUserToAddRows = false;
            this.dataGridView_unconnected_piping.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_unconnected_piping.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView_unconnected_piping.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView_unconnected_piping.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_unconnected_piping.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_unconnected_piping.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_unconnected_piping.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.elem_selected,
            this.xuhao});
            this.dataGridView_unconnected_piping.Location = new System.Drawing.Point(7, 28);
            this.dataGridView_unconnected_piping.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_unconnected_piping.MultiSelect = false;
            this.dataGridView_unconnected_piping.Name = "dataGridView_unconnected_piping";
            this.dataGridView_unconnected_piping.RowHeadersVisible = false;
            this.dataGridView_unconnected_piping.RowTemplate.Height = 23;
            this.dataGridView_unconnected_piping.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridView_unconnected_piping.Size = new System.Drawing.Size(321, 261);
            this.dataGridView_unconnected_piping.TabIndex = 0;
            this.dataGridView_unconnected_piping.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_unconnected_piping_CellValueChanged);
            this.dataGridView_unconnected_piping.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridView_unconnected_piping_CurrentCellDirtyStateChanged);
            // 
            // elem_selected
            // 
            this.elem_selected.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.elem_selected.FillWeight = 40F;
            this.elem_selected.HeaderText = "是否选中";
            this.elem_selected.Name = "elem_selected";
            this.elem_selected.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.elem_selected.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // xuhao
            // 
            this.xuhao.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.xuhao.DefaultCellStyle = dataGridViewCellStyle2;
            this.xuhao.FillWeight = 50F;
            this.xuhao.HeaderText = "序号";
            this.xuhao.Name = "xuhao";
            this.xuhao.ReadOnly = true;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(124, 297);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(84, 26);
            this.button1.TabIndex = 1;
            this.button1.Text = "确认";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ShowPipingGroupComponentInfosForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(359, 359);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ShowPipingGroupComponentInfosForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "首尾选中管道";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ShowPipingComponentInfosForm_FormClosed);
            this.Load += new System.EventHandler(this.ShowPipingComponentInfosForm_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_unconnected_piping)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridView dataGridView_unconnected_piping;
        private System.Windows.Forms.DataGridViewCheckBoxColumn elem_selected;
        private System.Windows.Forms.DataGridViewTextBoxColumn xuhao;
    }
}