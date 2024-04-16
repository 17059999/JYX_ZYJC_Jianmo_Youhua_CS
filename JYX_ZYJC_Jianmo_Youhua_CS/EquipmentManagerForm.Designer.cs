﻿namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    partial class EquipmentManagerForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EquipmentManagerForm));
            this.dataGridView_equipment = new System.Windows.Forms.DataGridView();
            this.contextMenuStrip_equipment = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_equipment_unsort = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_equipment_selectColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.button_isolate = new System.Windows.Forms.Button();
            this.button_hilite = new System.Windows.Forms.Button();
            this.button_showComponents = new System.Windows.Forms.Button();
            this.button_modify = new System.Windows.Forms.Button();
            this.dataGridView_nozzleInfo = new System.Windows.Forms.DataGridView();
            this.button_hilite_nozzle = new System.Windows.Forms.Button();
            this.button_isolate_nozzle = new System.Windows.Forms.Button();
            this.panel_equipment = new System.Windows.Forms.Panel();
            this.button_save = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.panel_nozzle = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_equipment)).BeginInit();
            this.contextMenuStrip_equipment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_nozzleInfo)).BeginInit();
            this.panel_equipment.SuspendLayout();
            this.panel_nozzle.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView_equipment
            // 
            this.dataGridView_equipment.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView_equipment.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_equipment.GridColor = System.Drawing.SystemColors.Control;
            this.dataGridView_equipment.Location = new System.Drawing.Point(17, 50);
            this.dataGridView_equipment.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_equipment.Name = "dataGridView_equipment";
            this.dataGridView_equipment.RowTemplate.Height = 23;
            this.dataGridView_equipment.Size = new System.Drawing.Size(957, 272);
            this.dataGridView_equipment.TabIndex = 4;
            this.dataGridView_equipment.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_equipment_CellBeginEdit);
            this.dataGridView_equipment.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_equipment_CellEndEdit);
            this.dataGridView_equipment.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_equipment_CellValueChanged);
            this.dataGridView_equipment.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_equipment_ColumnHeaderMouseClick);
            this.dataGridView_equipment.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridView_equipment_CurrentCellDirtyStateChanged);
            // 
            // contextMenuStrip_equipment
            // 
            this.contextMenuStrip_equipment.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip_equipment.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_equipment_unsort,
            this.toolStripSeparator1,
            this.toolStripMenuItem_equipment_selectColumns});
            this.contextMenuStrip_equipment.Name = "contextMenuStrip_equipment";
            this.contextMenuStrip_equipment.Size = new System.Drawing.Size(125, 54);
            // 
            // toolStripMenuItem_equipment_unsort
            // 
            this.toolStripMenuItem_equipment_unsort.Enabled = false;
            this.toolStripMenuItem_equipment_unsort.Name = "toolStripMenuItem_equipment_unsort";
            this.toolStripMenuItem_equipment_unsort.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_equipment_unsort.Text = "取消排序";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(121, 6);
            // 
            // toolStripMenuItem_equipment_selectColumns
            // 
            this.toolStripMenuItem_equipment_selectColumns.Name = "toolStripMenuItem_equipment_selectColumns";
            this.toolStripMenuItem_equipment_selectColumns.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_equipment_selectColumns.Text = "选择列";
            // 
            // button_isolate
            // 
            this.button_isolate.Location = new System.Drawing.Point(17, 9);
            this.button_isolate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_isolate.Name = "button_isolate";
            this.button_isolate.Size = new System.Drawing.Size(130, 33);
            this.button_isolate.TabIndex = 0;
            this.button_isolate.Text = "开启单独显示";
            this.button_isolate.UseVisualStyleBackColor = true;
            this.button_isolate.Click += new System.EventHandler(this.button_isolate_Click);
            // 
            // button_hilite
            // 
            this.button_hilite.Location = new System.Drawing.Point(154, 9);
            this.button_hilite.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_hilite.Name = "button_hilite";
            this.button_hilite.Size = new System.Drawing.Size(130, 33);
            this.button_hilite.TabIndex = 1;
            this.button_hilite.Text = "开启高亮显示";
            this.button_hilite.UseVisualStyleBackColor = true;
            this.button_hilite.Click += new System.EventHandler(this.button_hilite_Click);
            // 
            // button_showComponents
            // 
            this.button_showComponents.Location = new System.Drawing.Point(294, 9);
            this.button_showComponents.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_showComponents.Name = "button_showComponents";
            this.button_showComponents.Size = new System.Drawing.Size(174, 33);
            this.button_showComponents.TabIndex = 2;
            this.button_showComponents.Text = "Show Components";
            this.button_showComponents.UseVisualStyleBackColor = true;
            this.button_showComponents.Click += new System.EventHandler(this.button_showComponents_Click);
            // 
            // button_modify
            // 
            this.button_modify.Location = new System.Drawing.Point(658, 9);
            this.button_modify.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_modify.Name = "button_modify";
            this.button_modify.Size = new System.Drawing.Size(191, 33);
            this.button_modify.TabIndex = 3;
            this.button_modify.Text = "同时修改Unit和Service";
            this.button_modify.UseVisualStyleBackColor = true;
            this.button_modify.Visible = false;
            this.button_modify.Click += new System.EventHandler(this.button_modify_Click);
            // 
            // dataGridView_nozzleInfo
            // 
            this.dataGridView_nozzleInfo.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView_nozzleInfo.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView_nozzleInfo.GridColor = System.Drawing.SystemColors.Control;
            this.dataGridView_nozzleInfo.Location = new System.Drawing.Point(17, 50);
            this.dataGridView_nozzleInfo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dataGridView_nozzleInfo.Name = "dataGridView_nozzleInfo";
            this.dataGridView_nozzleInfo.RowTemplate.Height = 23;
            this.dataGridView_nozzleInfo.Size = new System.Drawing.Size(957, 271);
            this.dataGridView_nozzleInfo.TabIndex = 2;
            this.dataGridView_nozzleInfo.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView_nozzleInfo_CellBeginEdit);
            this.dataGridView_nozzleInfo.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_nozzleInfo_CellEndEdit);
            this.dataGridView_nozzleInfo.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView_nozzleInfo_CellValueChanged);
            this.dataGridView_nozzleInfo.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dataGridView_equipment_ColumnHeaderMouseClick);
            this.dataGridView_nozzleInfo.CurrentCellDirtyStateChanged += new System.EventHandler(this.dataGridView_equipment_CurrentCellDirtyStateChanged);
            // 
            // button_hilite_nozzle
            // 
            this.button_hilite_nozzle.Location = new System.Drawing.Point(154, 9);
            this.button_hilite_nozzle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_hilite_nozzle.Name = "button_hilite_nozzle";
            this.button_hilite_nozzle.Size = new System.Drawing.Size(130, 33);
            this.button_hilite_nozzle.TabIndex = 1;
            this.button_hilite_nozzle.Text = "开启高亮显示";
            this.button_hilite_nozzle.UseVisualStyleBackColor = true;
            this.button_hilite_nozzle.Click += new System.EventHandler(this.button_nozzlehilite_Click);
            // 
            // button_isolate_nozzle
            // 
            this.button_isolate_nozzle.Location = new System.Drawing.Point(17, 9);
            this.button_isolate_nozzle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_isolate_nozzle.Name = "button_isolate_nozzle";
            this.button_isolate_nozzle.Size = new System.Drawing.Size(130, 33);
            this.button_isolate_nozzle.TabIndex = 0;
            this.button_isolate_nozzle.Text = "开启单独显示";
            this.button_isolate_nozzle.UseVisualStyleBackColor = true;
            this.button_isolate_nozzle.Click += new System.EventHandler(this.button_nozzleisolate_Click);
            // 
            // panel_equipment
            // 
            this.panel_equipment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_equipment.Controls.Add(this.button_save);
            this.panel_equipment.Controls.Add(this.button1);
            this.panel_equipment.Controls.Add(this.button_isolate);
            this.panel_equipment.Controls.Add(this.dataGridView_equipment);
            this.panel_equipment.Controls.Add(this.button_hilite);
            this.panel_equipment.Controls.Add(this.button_showComponents);
            this.panel_equipment.Controls.Add(this.button_modify);
            this.panel_equipment.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_equipment.Location = new System.Drawing.Point(0, 0);
            this.panel_equipment.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel_equipment.Name = "panel_equipment";
            this.panel_equipment.Size = new System.Drawing.Size(990, 330);
            this.panel_equipment.TabIndex = 0;
            this.panel_equipment.ClientSizeChanged += new System.EventHandler(this.panel_equipment_ClientSizeChanged);
            this.panel_equipment.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel_equipment_MouseDown);
            this.panel_equipment.MouseLeave += new System.EventHandler(this.panel_equipment_MouseLeave);
            this.panel_equipment.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel_equipment_MouseMove);
            this.panel_equipment.MouseUp += new System.Windows.Forms.MouseEventHandler(this.panel_equipment_MouseUp);
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(856, 9);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(118, 33);
            this.button_save.TabIndex = 6;
            this.button_save.Text = "保存";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(476, 9);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(174, 33);
            this.button1.TabIndex = 5;
            this.button1.Text = "更新所在图层";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panel_nozzle
            // 
            this.panel_nozzle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_nozzle.Controls.Add(this.button_isolate_nozzle);
            this.panel_nozzle.Controls.Add(this.dataGridView_nozzleInfo);
            this.panel_nozzle.Controls.Add(this.button_hilite_nozzle);
            this.panel_nozzle.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel_nozzle.Location = new System.Drawing.Point(0, 330);
            this.panel_nozzle.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel_nozzle.Name = "panel_nozzle";
            this.panel_nozzle.Size = new System.Drawing.Size(990, 326);
            this.panel_nozzle.TabIndex = 1;
            this.panel_nozzle.ClientSizeChanged += new System.EventHandler(this.panel_nozzle_ClientSizeChanged);
            // 
            // EquipmentManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(990, 666);
            this.Controls.Add(this.panel_nozzle);
            this.Controls.Add(this.panel_equipment);
            this.Font = new System.Drawing.Font("华文中宋", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "EquipmentManagerForm";
            this.Text = "设备管理器";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_equipment)).EndInit();
            this.contextMenuStrip_equipment.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView_nozzleInfo)).EndInit();
            this.panel_equipment.ResumeLayout(false);
            this.panel_nozzle.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private void ToolStripMenuItem_equipment_selectColumns_Click1(object sender, System.EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView_equipment;
        private System.Windows.Forms.Button button_isolate;
        private System.Windows.Forms.Button button_hilite;
        private System.Windows.Forms.Button button_showComponents;
        private System.Windows.Forms.Button button_modify;
        private System.Windows.Forms.DataGridView dataGridView_nozzleInfo;
        private System.Windows.Forms.Button button_hilite_nozzle;
        private System.Windows.Forms.Button button_isolate_nozzle;
        private System.Windows.Forms.Panel panel_equipment;
        private System.Windows.Forms.Panel panel_nozzle;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_equipment;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_equipment_unsort;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_equipment_selectColumns;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button_save;
    }
}