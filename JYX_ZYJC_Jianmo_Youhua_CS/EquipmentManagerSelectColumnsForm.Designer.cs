namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class EquipmentManagerSelectColumnsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EquipmentManagerSelectColumnsForm));
            this.button_moveUp = new System.Windows.Forms.Button();
            this.button_moveDown = new System.Windows.Forms.Button();
            this.button_ok = new System.Windows.Forms.Button();
            this.button_cancel = new System.Windows.Forms.Button();
            this.checkedListBox_selectColumns = new System.Windows.Forms.CheckedListBox();
            this.SuspendLayout();
            // 
            // button_moveUp
            // 
            this.button_moveUp.Location = new System.Drawing.Point(305, 17);
            this.button_moveUp.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_moveUp.Name = "button_moveUp";
            this.button_moveUp.Size = new System.Drawing.Size(112, 33);
            this.button_moveUp.TabIndex = 1;
            this.button_moveUp.Text = "上移";
            this.button_moveUp.UseVisualStyleBackColor = true;
            this.button_moveUp.Click += new System.EventHandler(this.button_moveUp_Click);
            // 
            // button_moveDown
            // 
            this.button_moveDown.Location = new System.Drawing.Point(305, 58);
            this.button_moveDown.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_moveDown.Name = "button_moveDown";
            this.button_moveDown.Size = new System.Drawing.Size(112, 33);
            this.button_moveDown.TabIndex = 2;
            this.button_moveDown.Text = "下移";
            this.button_moveDown.UseVisualStyleBackColor = true;
            this.button_moveDown.Click += new System.EventHandler(this.button_moveDown_Click);
            // 
            // button_ok
            // 
            this.button_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button_ok.Location = new System.Drawing.Point(305, 101);
            this.button_ok.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(112, 33);
            this.button_ok.TabIndex = 3;
            this.button_ok.Text = "完成";
            this.button_ok.UseVisualStyleBackColor = true;
            // 
            // button_cancel
            // 
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(305, 142);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(112, 33);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // checkedListBox_selectColumns
            // 
            this.checkedListBox_selectColumns.AllowDrop = true;
            this.checkedListBox_selectColumns.FormattingEnabled = true;
            this.checkedListBox_selectColumns.Location = new System.Drawing.Point(13, 17);
            this.checkedListBox_selectColumns.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.checkedListBox_selectColumns.Name = "checkedListBox_selectColumns";
            this.checkedListBox_selectColumns.Size = new System.Drawing.Size(280, 400);
            this.checkedListBox_selectColumns.TabIndex = 0;
            this.checkedListBox_selectColumns.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListBox_selectColumns_ItemCheck);
            this.checkedListBox_selectColumns.MouseClick += new System.Windows.Forms.MouseEventHandler(this.checkedListBox_selectColumns_MouseClick);
            // 
            // EquipmentManagerSelectColumnsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(426, 431);
            this.Controls.Add(this.checkedListBox_selectColumns);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.button_moveDown);
            this.Controls.Add(this.button_moveUp);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximumSize = new System.Drawing.Size(444, 478);
            this.MinimumSize = new System.Drawing.Size(444, 478);
            this.Name = "EquipmentManagerSelectColumnsForm";
            this.Text = "选择列";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_moveUp;
        private System.Windows.Forms.Button button_moveDown;
        private System.Windows.Forms.Button button_ok;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.CheckedListBox checkedListBox_selectColumns;
    }
}