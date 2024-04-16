using Bentley.OpenPlant.Modeler.Api;
using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ShowPipingGroupComponentInfosForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        List<List<BMECObject>> m_BMECObject_list_list;
        public ShowPipingGroupComponentInfosForm(List<List<BMECObject>> result_bmec_object_list_list)
        {
            InitializeComponent();
            m_BMECObject_list_list = result_bmec_object_list_list;
        }

        public void setElementDataToDataGridView(List<List<BMECObject>> m_BMECObject_list_list)
        {

            //清空 dataGridView 行
            this.dataGridView_unconnected_piping.Rows.Clear();
            if (m_BMECObject_list_list == null || m_BMECObject_list_list.Count == 0) return;
            int rowCount = 0;
            //填充行
            for (int i = 0; i < m_BMECObject_list_list.Count; i++)
            {
                rowCount = this.dataGridView_unconnected_piping.Rows.Add();

                this.dataGridView_unconnected_piping.Rows[rowCount].Cells[0].Value = false;
                this.dataGridView_unconnected_piping.Rows[rowCount].Cells[1].Value = i + 1;
                this.dataGridView_unconnected_piping.Rows[rowCount].Tag = m_BMECObject_list_list[i];
            }
        }

        private void ShowPipingComponentInfosForm_Load(object sender, EventArgs e)
        {
            setElementDataToDataGridView(m_BMECObject_list_list);
        }

        private void ShowPipingComponentInfosForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //将高亮元素恢复
            BMECApi.Instance.ViewComponent(null, false, false, false);
        }

        private void dataGridView_unconnected_piping_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView_unconnected_piping.IsCurrentCellDirty)
            {
                dataGridView_unconnected_piping.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView_unconnected_piping_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (e.ColumnIndex == 0)
                {
                    //将高亮元素恢复
                    BMECApi.Instance.ViewComponent(null, false, false, false);
                    foreach (DataGridViewRow datarow in dataGridView_unconnected_piping.Rows)
                    {
                        if ((bool)datarow.Cells[0].Value == true)
                        {
                            List<BMECObject> bmec_object_list = (List<BMECObject>)datarow.Tag;
                            if (bmec_object_list != null)
                            {
                                foreach (BMECObject bmec_object in bmec_object_list)
                                {
                                    BMECApi.Instance.ViewComponent(bmec_object.Instance, true, true, false);
                                }
                            }
                        }
                    }
                }

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //将高亮元素恢复
            BMECApi.Instance.ViewComponent(null, false, false, false);
            foreach (DataGridViewRow datarow in dataGridView_unconnected_piping.Rows)
            {
                if ((bool)datarow.Cells[0].Value == true)
                {
                    List<BMECObject> bmec_object_list = (List<BMECObject>)datarow.Tag;
                    if (bmec_object_list != null)
                    {
                        foreach (BMECObject bmec_object in bmec_object_list)
                        {
                            BMECApi.Instance.SelectComponent(bmec_object.Instance, true);
                        }
                    }
                }
            }
            this.Close();
        }
    }
}
