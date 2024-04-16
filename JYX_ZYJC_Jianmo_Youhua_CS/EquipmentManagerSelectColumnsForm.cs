using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class EquipmentManagerSelectColumnsForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        public DataGridView DataGridView_equipment { get; set; }

        public DataTable DataTable { get; set; } = new DataTable();

        public EquipmentManagerSelectColumnsForm()
        {
            InitializeComponent();
        }

        public EquipmentManagerSelectColumnsForm(DataGridView dataGridView_equipment) : this()
        {
            this.DataGridView_equipment = dataGridView_equipment;
            LoadData();
            SetFormStyle();
        }

        private void LoadData() {
            DataTable.Columns.Add("ColumnName");
            DataTable.Columns.Add("ColumnHeaderText");
            DataTable.Columns.Add("Visible");

            DataGridViewColumn[] columnsSortArray = new DataGridViewColumn[this.DataGridView_equipment.Columns.Count];
            foreach (DataGridViewColumn item in this.DataGridView_equipment.Columns)
            {
                columnsSortArray[item.DisplayIndex] = item;
            }

            for (int i = 0; i < columnsSortArray.Length; i++)
            {
                if (columnsSortArray[i].Name == "CheckBoxColumn" || columnsSortArray[i].Name.Contains("InstanceId"))
                    continue;
                DataTable.Rows.Add(columnsSortArray[i].Name, columnsSortArray[i].HeaderText, columnsSortArray[i].Visible);
            }
            //foreach (DataGridViewColumn item in this.DataGridView_equipment.Columns)
            //{
            //    if (item.Name == "CheckBoxColumn" || item.Name.Contains("InstanceId"))
            //        continue;
            //    DataTable.Rows.Add(item.Name, item.HeaderText, item.Visible);
            //}
            RefreshCheckListBox(DataTable);
        }

        private void RefreshCheckListBox(DataTable table)
        {
            this.checkedListBox_selectColumns.DataSource = table;
            this.checkedListBox_selectColumns.ValueMember = "ColumnName";
            this.checkedListBox_selectColumns.DisplayMember = "ColumnHeaderText";
            for (int i = 0; i < table.Rows.Count; i++)
            {
                this.checkedListBox_selectColumns.SetItemChecked(i, Convert.ToBoolean(table.Rows[i]["Visible"]));
            }
        }

        private void SetFormStyle() {
            int offsetDis = 6;
            this.checkedListBox_selectColumns.Width = 200;
            this.button_moveUp.Enabled = false;
            this.button_moveDown.Enabled = false;
            this.button_moveUp.Location = new Point(this.checkedListBox_selectColumns.Location.X + this.checkedListBox_selectColumns.Width + offsetDis, this.button_moveUp.Location.Y);
            this.button_moveDown.Location = new Point(this.checkedListBox_selectColumns.Location.X + this.checkedListBox_selectColumns.Width + offsetDis, this.button_moveDown.Location.Y );
            this.button_ok.Location = new Point(this.checkedListBox_selectColumns.Location.X + this.checkedListBox_selectColumns.Width + offsetDis, this.button_ok.Location.Y);
            this.button_cancel.Location = new Point(this.checkedListBox_selectColumns.Location.X + this.checkedListBox_selectColumns.Width + offsetDis, this.button_cancel.Location.Y);
            this.ClientSize = new Size(this.button_moveUp.Location.X + this.button_moveUp.Width + offsetDis, this.ClientSize.Height);
        }

        private void button_moveUp_Click(object sender, EventArgs e)
        {
            int index = this.checkedListBox_selectColumns.SelectedIndex;
            if (index > 0)
            {
                object[] itemArrary = this.DataTable.Rows[index].ItemArray;
                this.DataTable.Rows[index].ItemArray = this.DataTable.Rows[index-1].ItemArray;
                this.DataTable.Rows[index - 1].ItemArray = itemArrary;
                RefreshCheckListBox(DataTable);
                this.checkedListBox_selectColumns.SelectedIndex = index - 1;
            }
        }

        private void button_moveDown_Click(object sender, EventArgs e)
        {
            int index = this.checkedListBox_selectColumns.SelectedIndex;
            if (index < this.checkedListBox_selectColumns.Items.Count - 1)
            {
                object[] itemArrary = this.DataTable.Rows[index].ItemArray;
                this.DataTable.Rows[index].ItemArray = this.DataTable.Rows[index + 1].ItemArray;
                this.DataTable.Rows[index + 1].ItemArray = itemArrary;
                RefreshCheckListBox(DataTable);
                this.checkedListBox_selectColumns.SelectedIndex = index + 1;
            }
        }

        private void checkedListBox_selectColumns_MouseClick(object sender, MouseEventArgs e)
        {
            if (this.checkedListBox_selectColumns.SelectedItems.Count > 0)
            {
                this.button_moveUp.Enabled = true;
                this.button_moveDown.Enabled = true;
            }
            else
            {
                this.button_moveUp.Enabled = false;
                this.button_moveDown.Enabled = false;
            }
        }

        private void checkedListBox_selectColumns_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.DataTable.Rows[e.Index]["Visible"] = e.NewValue == CheckState.Checked ? "True" : "False";
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
