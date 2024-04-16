namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class biaogaoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(biaogaoForm));
            this.biaogao_label = new System.Windows.Forms.Label();
            this.biaogao_text = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // biaogao_label
            // 
            this.biaogao_label.AutoSize = true;
            this.biaogao_label.Location = new System.Drawing.Point(12, 18);
            this.biaogao_label.Name = "biaogao_label";
            this.biaogao_label.Size = new System.Drawing.Size(101, 12);
            this.biaogao_label.TabIndex = 0;
            this.biaogao_label.Text = "标高保留小数位：";
            // 
            // biaogao_text
            // 
            this.biaogao_text.Location = new System.Drawing.Point(119, 14);
            this.biaogao_text.Name = "biaogao_text";
            this.biaogao_text.Size = new System.Drawing.Size(100, 21);
            this.biaogao_text.TabIndex = 1;
            // 
            // biaogaoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(232, 50);
            this.Controls.Add(this.biaogao_text);
            this.Controls.Add(this.biaogao_label);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "biaogaoForm";
            this.Text = "剖面标高";
            this.Load += new System.EventHandler(this.biaogaoForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label biaogao_label;
        public System.Windows.Forms.TextBox biaogao_text;
    }
}