using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class BOMForm :
#if DEBUG 
        Form
#else 
        Adapter
#endif
    {

        List<string> jyx_type_name_list = new List<string>();
        public BOMForm()
        {
            InitializeComponent();
        }
        public BOMForm(List<string> jyx_type_name_list)
        {
            InitializeComponent();
            foreach (string item in jyx_type_name_list)
            {
                dataGridView_bom1.Rows.Add(item);
            }
            dataGridView_bom1.AllowUserToAddRows = false;
            dataGridView_bom2.AllowUserToAddRows = false;
        }
        
        private void button_ok_Click(object sender, EventArgs e)
        {
            DataGridViewRowCollection dgvrc = dataGridView_bom2.Rows;
            if (dgvrc.Count == 0) return;
            jyx_type_name_list.Clear();
            foreach (DataGridViewRow item in dgvrc)
            {
                jyx_type_name_list.Add(item.Cells[0].Value.ToString());
            }
            BOM.bom.excel(jyx_type_name_list);

        }

        private void dataGridView_bom1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //添加
            DataGridViewSelectedRowCollection dgvsrc = dataGridView_bom1.SelectedRows;
            if (dgvsrc.Count == 0) return;
            dataGridView_bom2.Rows.Add(dgvsrc[0].Cells[0].Value.ToString());
            //删除
            dataGridView_bom1.Rows.Remove(dgvsrc[0]);
        }

        private void dataGridView_bom2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //添加
            DataGridViewSelectedRowCollection dgvsrc = dataGridView_bom2.SelectedRows;
            if (dgvsrc.Count == 0) return;
            dataGridView_bom1.Rows.Add(dgvsrc[0].Cells[0].Value.ToString());
            //删除
            dataGridView_bom2.Rows.Remove(dgvsrc[0]);
        }
    }
}
