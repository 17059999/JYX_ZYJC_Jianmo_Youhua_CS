using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.OpenPlant.Modeler.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 通过距离筛选孤立元素
    /// 通过连结性筛选孤立元素
    /// 通过距离和连结性筛选孤立元素
    /// 选择其中一行数据定位至该元素
    /// 选中多行元素将其从当前 Model 中移除
    /// 通过选择列头显示对应数据
    /// </summary>
    public partial class ElementClearForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        
        public static ElementClearForm instance = null;
        private double default_distance = 300;//默认孤立距离
        /// <summary>
        /// 列头名称
        /// </summary>
        public List<string> columnsName = new List<string>();
        /// <summary>
        /// 构造
        /// </summary>
        private ElementClearForm()
        {
            InitializeComponent();
            this.init();
            this.initColumsName();
        }
        public static ElementClearForm getInstance() {
            if (instance == null)
            {
                instance = new ElementClearForm();
            }
            return instance;
        }
        /// <summary>
        /// 组件初始化
        /// </summary>
        protected void init() {
            this.textBox_distance.Text = this.default_distance.ToString();//设置默认的孤立距离
            this.dataGridView_separateElement.ReadOnly = true;//设置数据表只读
            this.dataGridView_separateElement.AllowUserToAddRows = false;//不显示最下面的新行
            this.dataGridView_separateElement.AllowUserToDeleteRows = false;//不允许用户删除行
            this.dataGridView_separateElement.RowHeadersVisible = false;//设置行标题不可见
            this.dataGridView_separateElement.SelectionMode = DataGridViewSelectionMode.FullRowSelect;//单击选中整行
            this.dataGridView_separateElement.AllowUserToResizeColumns = true;//允许用户调整列宽
            this.dataGridView_separateElement.AllowUserToResizeRows = false;//禁止用户调整行高
            this.comboBox_tiaojian.Text = "全部";
        }
        /// <summary>
        /// 初始化列头名称
        /// TODO 可以选择列头数据源？
        /// </summary>
        private void initColumsName() {
            this.columnsName.Add("序号");
            this.columnsName.Add("元素ID");
            this.columnsName.Add("元素类型");
            this.columnsName.Add("描述");

            //创建列头
            int colNum = this.columnsName.Count;
            for (int i = 0; i < colNum; i++)
            {
                this.dataGridView_separateElement.Columns.Add(this.columnsName[i], this.columnsName[i]);
                this.dataGridView_separateElement.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;//不允许用户列排序
            }
            int colcount = 0;
            this.dataGridView_separateElement.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;//自动填充到容器宽
            this.dataGridView_separateElement.Columns[colcount++].FillWeight = 10;
            this.dataGridView_separateElement.Columns[colcount++].FillWeight = 15;
            this.dataGridView_separateElement.Columns[colcount++].FillWeight = 20;
            this.dataGridView_separateElement.Columns[colcount++].FillWeight = 55;

        }
        /// <summary>
        /// 是否点了筛选按钮
        /// </summary>
        private bool flag = false;
        /// <summary>
        /// 获取数据
        /// 将获取到的数据传给 datagridview 让其显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_search_Click(object sender, EventArgs e)
        {
            string tiaojian = this.comboBox_tiaojian.Text;
            flag = true;
            Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog();
            waitDialog.SetTitleString("删除多余图形");
            waitDialog.SetInformationSting("获取数据");
            waitDialog.Show();
            this.setDataByTiaojian(tiaojian);
            flag = false;//用完置初始
            waitDialog.Close();
        }
        /// <summary>
        /// 根据筛选条件查找数据
        /// </summary>
        /// <param name="tiaojian"></param>
        private void setDataByTiaojian(string tiaojian) {


            //将高亮元素恢复
            try
            {
                if (hightlightedElement != null && hightlightedElement != "")
                {
                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(Convert.ToInt64(hightlightedElement));
                    if (ielement != null)
                    {
                        ielement.IsHighlighted = false;
                        hightlightedElement = "";
                    }
                }

                ElementClear elementClear = new ElementClear();
                List<IECInstance> elements = new List<IECInstance>();
                string distance = this.textBox_distance.Text;
                if (CutOffPipeForm.isPositiveIntegerUnlimited(distance))
                {
                    if (distance == string.Empty)
                    {
                        this.default_distance = 0;
                    }
                    else
                    {
                        this.default_distance = Convert.ToDouble(distance);
                    }
                    switch (tiaojian)
                    {
                        case "全部":
                            elements = ElementClear.addElementListNorepeat(elementClear.filterInstanceByConnection(), elementClear.filterInstanceByDistance(this.default_distance));
                            break;
                        case "连接性":
                            elements = elementClear.filterInstanceByConnection();
                            break;
                        case "距离":
                            elements = elementClear.filterInstanceByDistance(this.default_distance);
                            break;
                        default:
                            break;
                    }
                    if (elements.Count == 0 && flag)
                    {
                        MessageBox.Show("未找到符合条件的元素");
                    }

                    List<Element> elementlist = new List<Element>();
                    foreach (var instance in elements)
                    {
                        Element ele = JYX_ZYJC_CLR.PublicMethod.get_element_by_instance(instance);
                        elementlist.Add(ele);
                    }

                    this.setElementDataToDataGridView(elementlist);
                }
                else
                {
                    MessageBox.Show("请输入合法的字符串");
                }
            }
            catch (Exception)
            {

            }
            
        }
        /// <summary>
        /// 将获取的数据传给 datagridview
        /// </summary>
        public void setElementDataToDataGridView(List<Element> elements) {
            //清楚重复元素
            elements = NoRepeatElement(elements);
            //清空 dataGridView 行
            this.dataGridView_separateElement.Rows.Clear();
            if (elements == null || elements.Count == 0) return;
            int rowCount = 0;
            int colCount = 0;
            //填充行
            for (int i = 0; i < elements.Count; i++)
            {
                List<string> rowData = new List<string>();
                rowData.Add((i + 1).ToString());
                rowData.Add(elements[i].ElementId.ToString());
                string elementTypeName = OPM_Public_Api.getBMECObjectTypeName(elements[i]);
                rowData.Add(elementTypeName);
                rowData.Add(elements[i].Description);
                rowCount = this.dataGridView_separateElement.Rows.Add();
                foreach (var coldata in rowData)
                {
                    this.dataGridView_separateElement.Rows[rowCount].Cells[colCount++].Value = coldata;
                }
                colCount = 0;
            }
        }
        public static string hightlightedElement = "";
        /// <summary>
        /// 定位元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_locate_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridView_separateElement.SelectedRows;
            if (selectedRows.Count == 1)
            {
                //将高亮元素恢复
                if (hightlightedElement != null && hightlightedElement != "")
                {
                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(Convert.ToInt64(hightlightedElement));
                    if (ielement != null)
                    {
                        ielement.IsHighlighted = false;
                        hightlightedElement = "";
                    }
                }
                string elementIdStr = selectedRows[0].Cells[1].Value.ToString();//TODO 这个不能这么写死
                ElementClear.locateElement(Convert.ToInt64(elementIdStr));
                hightlightedElement = elementIdStr;
            }
            else
            {
                MessageBox.Show("请选择单行！");
            }


        }
        /// <summary>
        /// 从当前 Model 中删除选中的行对应的元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_delete_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridView_separateElement.SelectedRows;
            List<long> elementIds = new List<long>();//被选中行的元素的 ElementId
            foreach (var row in selectedRows)
            {
                string elementIdStr = ((DataGridViewRow)row).Cells[1].Value.ToString();//TODO 不能写死
                if (elementIdStr == hightlightedElement)
                {
                    hightlightedElement = "";
                }
                elementIds.Add(Convert.ToInt64(elementIdStr));
            }
            ElementClear.deleteElement(elementIds);
            //this.refreshDataGridView();
            //更新数据表中的数据
            for (int i = 0; i < this.dataGridView_separateElement.Rows.Count; i++)
            {
                this.dataGridView_separateElement.Rows[i].Cells[0].Value = i + 1;
                if (elementIds.Contains(Convert.ToInt64(this.dataGridView_separateElement.Rows[i].Cells[1].Value.ToString())))
                {
                    this.dataGridView_separateElement.Rows.Remove(this.dataGridView_separateElement.Rows[i]);
                    i--;
                }
            }
        }
        /// <summary>
        /// 刷新 DataGridView 中的数据，不应该这么刷新，这相当于重查了一遍，很浪费时间。
        /// 在筛选时还是这个方法，但在删除时应该直接从表中删除该数据
        /// </summary>
        private void refreshDataGridView() {
            string tiaojian = this.comboBox_tiaojian.Text;
            this.setDataByTiaojian(tiaojian);
            //将高亮元素恢复
            if (hightlightedElement != null && hightlightedElement != "")
            {
                try
                {

                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(Convert.ToInt64(hightlightedElement));
                    if (ielement != null)
                    {
                        ielement.IsHighlighted = false;
                        hightlightedElement = "";
                    }
                }
                catch (Exception)
                {

                }
            }
            flag = false;
        }
        
        /// <summary>
        /// 更新连接性
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_update_connection_Click(object sender, EventArgs e)
        {
            DataGridViewSelectedRowCollection selectedRows = this.dataGridView_separateElement.SelectedRows;
            List<long> elementIds = new List<long>();
            
            SelectionSetManager.EmptyAll();

            foreach (var row in selectedRows)
            {
                string elementIdStr = ((DataGridViewRow)row).Cells[1].Value.ToString();//TODO 不能写死
                elementIds.Add(Convert.ToInt64(elementIdStr));
            }
            foreach (var elementId in elementIds)
            {
                BMECObject ecObject = null;
                ecObject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(Convert.ToUInt64(elementId));
                if (ecObject != null && ecObject.Instance != null)
                {
                    //ecObjectList.Add(ecObject);
                    BMECApi api = BMECApi.Instance;
                    string err = "";
                    if (api.InstanceDefinedAsClass(ecObject.Instance, "PIPING_COMPONENT", true))
                    {
                        RepairConnectTool.updateConnectTool(ecObject, out err);
                        try
                        {
                            ecObject.DiscoverConnectionsEx();
                            ecObject.UpdateConnections();
                            ecObject.Create();
                            System.Windows.Forms.MessageBox.Show(err);
                        }
                        catch
                        {
                            MessageBox.Show("更新失败！");
                        }                        
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("该元素不具备连接性更新功能，请选择管道");
                    }
                }
            }
            //this.refreshDataGridView();
            //更新数据表中的数据
            for (int i = 0; i < this.dataGridView_separateElement.Rows.Count; i++)
            {
                this.dataGridView_separateElement.Rows[i].Cells[0].Value = i + 1;
                if (elementIds.Contains(Convert.ToInt64(this.dataGridView_separateElement.Rows[i].Cells[1].Value.ToString())))
                {
                    this.dataGridView_separateElement.Rows.Remove(this.dataGridView_separateElement.Rows[i]);
                    i--;
                }
            }
        }

        private void comboBox_tiaojian_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox temp = (ComboBox)sender;
            if (temp.Text == "连接性")
            {
                this.textBox_distance.Enabled = false;
            }
            else
            {
                this.textBox_distance.Enabled = true;
            }
        }

        private void ElementClearForm_Load(object sender, EventArgs e)
        {
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void ElementClearForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //将高亮元素恢复
            if (hightlightedElement != null && hightlightedElement != "")
            {
                try
                {
                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(Convert.ToInt64(hightlightedElement));
                    if (ielement != null)
                    {
                        ielement.IsHighlighted = false;
                        hightlightedElement = "";
                    }
                }
                catch (Exception exce)
                {
                    exce.ToString();
                }
            }
            ElementClearForm.instance = null;
        }

        public static List<Element> NoRepeatElement(List<Element> elements) {
            List<Element> result = new List<Element>();
            List<string> index = new List<string>();
            foreach (Element item in elements)
            {
                string elementId = item.ElementId.ToString();
                if (!index.Contains(elementId))
                {
                    index.Add(elementId);
                    result.Add(item);
                }
            }
            return result;
        }

        //修复焊点
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateConnectTool updateconnecttool = new UpdateConnectTool();
            updateconnecttool.InstallTool();
        }

        public void CheckedElement(Element element)
        {
            //获得传入Element的ID
            string s_element = element.ElementId.ToString();

            //对比搜索列表内的Element，一致则标记为选中状态
            DataGridViewRowCollection Rows = this.dataGridView_separateElement.Rows;
            for (int i = 0; i < Rows.Count; i++)
            {
                string elementIdStr = Rows[i].Cells[1].Value.ToString();//TODO 不能写死
                if (s_element == elementIdStr) Rows[i].Selected = true; //标记为选中状态
            }
        }
        public void CheckedElement()
        {
            //获取高亮元素
            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            List<string> elementIds = new List<string>();
            for (uint i = 0; i < element_agenda.GetCount(); i++)
            {
                elementIds.Add(element_agenda.GetEntry(i).ElementId.ToString());
            }
            //对比搜索列表内的Element，一致则标记为选中状态
            DataGridViewRowCollection Rows = this.dataGridView_separateElement.Rows;
            for (int i = 0; i < Rows.Count; i++)
            {
                string elementIdStr = Rows[i].Cells[1].Value.ToString();//TODO 不能写死
                if (elementIds.Contains(elementIdStr))
                {
                    Rows[i].Selected = true; //标记为选中状态
                }
                else
                {
                    Rows[i].Selected = false;
                }
                    
            }
        }
    }
}
