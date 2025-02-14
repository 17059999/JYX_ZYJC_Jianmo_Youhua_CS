﻿using Bentley.OpenPlant.Modeler.Api;
using Bentley.DgnPlatformNET.Elements;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.MstnPlatformNET.WinForms;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.Building.Mechanical.Components;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ShowPipingComponentInfosForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        List<BMECObject> m_BMECObjects;
        public ShowPipingComponentInfosForm(List<BMECObject> BMECObjects)
        {
            InitializeComponent();
            m_BMECObjects = BMECObjects;
        }

        private void dataGridView_unconnected_piping_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex!=-1)
            {
                Bentley.ECObjects.Instance.IECInstance instance = (Bentley.ECObjects.Instance.IECInstance)this.dataGridView_unconnected_piping.Rows[e.RowIndex].Tag;

                //将高亮元素恢复
                BMECApi.Instance.ViewComponent(null, false, false, false);
                BMECApi.Instance.ViewComponent(instance, true, true, false);

            }
        }

        public void setElementDataToDataGridView(List<BMECObject> BMECObjects)
        {
            
            //清空 dataGridView 行
            this.dataGridView_unconnected_piping.Rows.Clear();
            if (BMECObjects == null || BMECObjects.Count == 0) return;
            int rowCount = 0;
            int colCount = 0;
            //填充行
            for (int i = 0; i < BMECObjects.Count; i++)
            {
                //Element elem = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(BMECObjects[i]);
                List<string> rowData = new List<string>();
                rowData.Add((i + 1).ToString());
                

                rowData.Add(BMECObjects[i].Instance["LINENUMBER"].StringValue);
                rowData.Add(BMECObjects[i].Instance["NAME"].StringValue);
                rowCount = this.dataGridView_unconnected_piping.Rows.Add();
                foreach (var coldata in rowData)
                {
                    this.dataGridView_unconnected_piping.Rows[rowCount].Cells[colCount++].Value = coldata;
                    this.dataGridView_unconnected_piping.Rows[rowCount].Tag = BMECObjects[i].Instance;
                }
                colCount = 0;
            }
        }

        private void ShowPipingComponentInfosForm_Load(object sender, EventArgs e)
        {
            setElementDataToDataGridView(m_BMECObjects);
        }

        private void ShowPipingComponentInfosForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //将高亮元素恢复
            BMECApi.Instance.ViewComponent(null, false, false, false);
        }
    }
}
