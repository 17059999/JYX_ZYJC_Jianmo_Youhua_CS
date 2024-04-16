namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class updateToleranceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(updateToleranceForm));
            this.TolerancetextBox = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TolerancetextBox
            // 
            this.TolerancetextBox.Location = new System.Drawing.Point(23, 15);
            this.TolerancetextBox.Name = "TolerancetextBox";
            this.TolerancetextBox.Size = new System.Drawing.Size(110, 21);
            this.TolerancetextBox.TabIndex = 0;
            this.TolerancetextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TolerancetextBox_KeyPress);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(158, 15);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // updateToleranceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(254, 53);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.TolerancetextBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "updateToleranceForm";
            this.Text = "修改容差值";
            this.Load += new System.EventHandler(this.updateToleranceForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TolerancetextBox;
        private System.Windows.Forms.Button button1;
    }
}