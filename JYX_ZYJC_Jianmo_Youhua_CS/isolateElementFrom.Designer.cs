﻿namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class isolateElementFrom
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(isolateElementFrom));
            this.dataGridView_isoElement = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.elementId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.elementType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.desc = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_isoElement)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView_isoElement
            // 
            this.dataGridView_isoElement.AllowUserToAddRows = false;
            this.dataGridView_isoElement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView_isoElement.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView_isoElement.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView_isoElement.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_isoElement.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.elementId,
            this.elementType,
            this.desc});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView_isoElement.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridView_isoElement.Location = new System.Drawing.Point(13, 14);
            this.dataGridView_isoElement.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_isoElement.Name = "dataGridView_isoElement";
            this.dataGridView_isoElement.RowHeadersVisible = false;
            this.dataGridView_isoElement.RowTemplate.Height = 23;
            this.dataGridView_isoElement.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_isoElement.Size = new System.Drawing.Size(643, 252);
            this.dataGridView_isoElement.TabIndex = 0;
            // 
            // id
            // 
            this.id.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.id.FillWeight = 81.18082F;
            this.id.HeaderText = "序号";
            this.id.Name = "id";
            // 
            // elementId
            // 
            this.elementId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.elementId.FillWeight = 92.20712F;
            this.elementId.HeaderText = "元素id";
            this.elementId.Name = "elementId";
            // 
            // elementType
            // 
            this.elementType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.elementType.FillWeight = 111.269F;
            this.elementType.HeaderText = "元素类型";
            this.elementType.Name = "elementType";
            // 
            // desc
            // 
            this.desc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.desc.FillWeight = 115.3431F;
            this.desc.HeaderText = "描述";
            this.desc.Name = "desc";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(285, 276);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 33);
            this.button1.TabIndex = 1;
            this.button1.Text = "定位";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // isolateElementFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(669, 326);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.dataGridView_isoElement);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "isolateElementFrom";
            this.Text = "定位元素";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.isolateElementFrom_FormClosed);
            this.Load += new System.EventHandler(this.isolateElementFrom_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_isoElement)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_isoElement;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn elementId;
        private System.Windows.Forms.DataGridViewTextBoxColumn elementType;
        private System.Windows.Forms.DataGridViewTextBoxColumn desc;
    }
}