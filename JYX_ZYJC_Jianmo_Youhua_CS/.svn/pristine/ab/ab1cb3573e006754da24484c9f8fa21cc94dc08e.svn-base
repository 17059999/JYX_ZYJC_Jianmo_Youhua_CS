using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.DgnPlatformNET.Elements;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.MstnPlatformNET.WinForms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class isolateElementFrom : //Form
#if DEBUG
        Form
#else  
        Adapter
#endif
    {
        public List<Element> elementList = new List<Element>();
        public isolateElementFrom(List<Element> eleList)
        {
            InitializeComponent();
            elementList = eleList;
        }

        /// <summary>
        /// 加载窗体给dataGridView赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isolateElementFrom_Load(object sender, EventArgs e)
        {
            int i = 1;
            foreach(Element ele in elementList)
            {
                //BIM.Element element = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(ele);
                string elementTypeName = OPM_Public_Api.getBMECObjectTypeName(ele);
                dataGridView_isoElement.Rows.Add(i, ele.ElementId.ToString(), elementTypeName, ele.Description);
                i++;
            }
        }

        /// <summary>
        /// 定位元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridView_isoElement.SelectedRows;
            if(selectedRows.Count==1)
            {
                string elementIdStr = selectedRows[0].Cells[1].Value.ToString();
                quexiaoG();
                locateElement(Convert.ToInt64(elementIdStr));
            }
            else
            {
                MessageBox.Show("请选择单行！");
            }
        }

        /*其他功能*/
        /// <summary>
        ///定位到元素
        /// </summary>
        public static void locateElement(long elementId)
        {
            Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(elementId);
            Bentley.Interop.MicroStationDGN.View viewport = app.CommandState.LastView();

            int view = viewport.Index;
            JYX_ZYJC_CLR.PublicMethod.zoom_elem(ielement, view);
            ielement.IsHighlighted = true;
        }

        /// <summary>
        /// 取消高亮状态
        /// </summary>
        public void quexiaoG()
        {
            foreach (Element ele in elementList)
            {
                BIM.Element element = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(ele);
                element.IsHighlighted = false;
            }
        }

        private void isolateElementFrom_FormClosed(object sender, FormClosedEventArgs e)
        {
            quexiaoG();
        }
    }
}
