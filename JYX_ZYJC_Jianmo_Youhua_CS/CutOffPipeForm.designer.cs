namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 
    /// </summary>
    partial class CutOffPipeForm
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
            //this.cutOffPipeTool.clearTool();//当窗口关闭时清理tool中保留的窗口对象
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CutOffPipeForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.textBox_length_pipe = new System.Windows.Forms.TextBox();
            this.label_length_pipe = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_ok);
            this.groupBox1.Controls.Add(this.textBox_length_pipe);
            this.groupBox1.Controls.Add(this.label_length_pipe);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(469, 96);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "断开管道";
            // 
            // button_ok
            // 
            this.button_ok.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.button_ok.Location = new System.Drawing.Point(341, 35);
            this.button_ok.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(112, 33);
            this.button_ok.TabIndex = 2;
            this.button_ok.Text = "断开管道";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.button_ok_Click);
            // 
            // textBox_length_pipe
            // 
            this.textBox_length_pipe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_length_pipe.Location = new System.Drawing.Point(134, 39);
            this.textBox_length_pipe.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox_length_pipe.Name = "textBox_length_pipe";
            this.textBox_length_pipe.Size = new System.Drawing.Size(178, 27);
            this.textBox_length_pipe.TabIndex = 1;
            // 
            // label_length_pipe
            // 
            this.label_length_pipe.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label_length_pipe.Location = new System.Drawing.Point(7, 42);
            this.label_length_pipe.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_length_pipe.Name = "label_length_pipe";
            this.label_length_pipe.Size = new System.Drawing.Size(119, 17);
            this.label_length_pipe.TabIndex = 0;
            this.label_length_pipe.Text = "预制长度(mm)：";
            // 
            // CutOffPipeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(493, 120);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(511, 167);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(511, 167);
            this.Name = "CutOffPipeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "断开管道";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CutOffPipeForm_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.TextBox textBox_length_pipe;
        private System.Windows.Forms.Label label_length_pipe;
    }
}