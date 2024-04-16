namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class CustomizeHeightSectionForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomizeHeightSectionForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.callOutComboBox = new System.Windows.Forms.ComboBox();
            this.heightTextBox = new System.Windows.Forms.TextBox();
            this.noKeepOutRadioButton = new System.Windows.Forms.RadioButton();
            this.keepOutRadioButton = new System.Windows.Forms.RadioButton();
            this.calloutButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Callout：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "自定义高度（mm）：";
            // 
            // callOutComboBox
            // 
            this.callOutComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.callOutComboBox.FormattingEnabled = true;
            this.callOutComboBox.Location = new System.Drawing.Point(125, 25);
            this.callOutComboBox.Name = "callOutComboBox";
            this.callOutComboBox.Size = new System.Drawing.Size(121, 20);
            this.callOutComboBox.TabIndex = 2;
            // 
            // heightTextBox
            // 
            this.heightTextBox.Location = new System.Drawing.Point(125, 59);
            this.heightTextBox.Name = "heightTextBox";
            this.heightTextBox.Size = new System.Drawing.Size(121, 21);
            this.heightTextBox.TabIndex = 3;
            this.heightTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.heightTextBox_KeyPress);
            // 
            // noKeepOutRadioButton
            // 
            this.noKeepOutRadioButton.AutoSize = true;
            this.noKeepOutRadioButton.Location = new System.Drawing.Point(187, 102);
            this.noKeepOutRadioButton.Name = "noKeepOutRadioButton";
            this.noKeepOutRadioButton.Size = new System.Drawing.Size(59, 16);
            this.noKeepOutRadioButton.TabIndex = 5;
            this.noKeepOutRadioButton.TabStop = true;
            this.noKeepOutRadioButton.Text = "不遮挡";
            this.noKeepOutRadioButton.UseVisualStyleBackColor = true;
            this.noKeepOutRadioButton.CheckedChanged += new System.EventHandler(this.noKeepOutRadioButton_CheckedChanged);
            // 
            // keepOutRadioButton
            // 
            this.keepOutRadioButton.AutoSize = true;
            this.keepOutRadioButton.Location = new System.Drawing.Point(14, 102);
            this.keepOutRadioButton.Name = "keepOutRadioButton";
            this.keepOutRadioButton.Size = new System.Drawing.Size(47, 16);
            this.keepOutRadioButton.TabIndex = 4;
            this.keepOutRadioButton.TabStop = true;
            this.keepOutRadioButton.Text = "遮挡";
            this.keepOutRadioButton.UseVisualStyleBackColor = true;
            this.keepOutRadioButton.CheckedChanged += new System.EventHandler(this.keepOutRadioButton_CheckedChanged);
            // 
            // calloutButton
            // 
            this.calloutButton.Location = new System.Drawing.Point(93, 132);
            this.calloutButton.Name = "calloutButton";
            this.calloutButton.Size = new System.Drawing.Size(75, 23);
            this.calloutButton.TabIndex = 6;
            this.calloutButton.Text = "callout";
            this.calloutButton.UseVisualStyleBackColor = true;
            this.calloutButton.Click += new System.EventHandler(this.calloutButton_Click);
            // 
            // CustomizeHeightSectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(260, 167);
            this.Controls.Add(this.calloutButton);
            this.Controls.Add(this.noKeepOutRadioButton);
            this.Controls.Add(this.keepOutRadioButton);
            this.Controls.Add(this.heightTextBox);
            this.Controls.Add(this.callOutComboBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CustomizeHeightSectionForm";
            this.Text = "自定义切面高度";
            this.Load += new System.EventHandler(this.CustomizeHeightSectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox callOutComboBox;
        private System.Windows.Forms.TextBox heightTextBox;
        private System.Windows.Forms.RadioButton noKeepOutRadioButton;
        private System.Windows.Forms.RadioButton keepOutRadioButton;
        private System.Windows.Forms.Button calloutButton;
    }
}