
using Bentley.ECObjects.Instance;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.OpenPlantModeler.SDK.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.OpenPlant.Modeler.Api;
namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class MaterialsProManagerForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        /// <summary>
        /// 控制只能打开一个窗口
        /// </summary>
        public static MaterialsProManagerForm materForm = null;

        /// <summary>
        /// 图纸所有元件的集合
        /// </summary>
        public Dictionary<int, List<IECInstance>> dicInstance = new Dictionary<int, List<IECInstance>>();

        /// <summary>
        /// 是否高亮显示
        /// </summary>
        public bool hiLite = false;

        /// <summary>
        /// 是否放大
        /// </summary>
        public bool zoom = false;

        /// <summary>
        /// 是否单独显示
        /// </summary>
        public bool isolate = false;

        public bool ishiden = false;
        public MaterialsProManagerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 控制只能打开一个窗口
        /// </summary>
        /// <returns></returns>
        public static MaterialsProManagerForm instance()
        {
            if (materForm == null)
            {
                materForm = new MaterialsProManagerForm();
            }
            else
            {
                materForm.Close();
                materForm = new MaterialsProManagerForm();
            }
            return materForm;
        }

        /// <summary>
        /// 窗体加载时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MaterialsProManagerForm_Load(object sender, EventArgs e)
        {
            //AddCheckBoxToDataGridView.dgvm = dataGridView1;
            //AddCheckBoxToDataGridView.AddFullSelectm();
            if (!ishiden)
            {
                ishiden = true;
                AddCheckBoxToDataGridView.dgvm1 = dataGridView2;
                AddCheckBoxToDataGridView.AddFullSelectm1();
                ECInstanceList ecList = DgnUtilities.GetAllInstancesFromDgn();
                dicInstance = getData(ecList);
                if (dicInstance.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in dicInstance)
                    {
                        dataGridView1.Rows.Add(false, kv.Value[0]["LINENUMBER"].StringValue, kv.Value[0]["SPECIFICATION"].StringValue);
                        //foreach(IECInstance ec1 in kv.Value)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod
                        //    BMECObject bemc = new BMECObject(ec1);
                        //    ulong id = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bemc);
                        //    dataGridView2.Rows.Add(true, id.ToString(), ec1.ClassDefinition.Name, ec1["NOMINAL_DIAMETER"].DoubleValue.ToString(), ec1["LINENUMBER"].StringValue);
                        //}
                    }
                }
            }
        }

        /// <summary>
        /// 单元格值数据更改并提交后触发，为dataGridView2赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0)
                return;
            if (e.RowIndex == -1)
                return;
            dataGridView2.Rows.Clear();
            Dictionary<int, List<IECInstance>> dicIecinstanceList = new Dictionary<int, List<IECInstance>>();
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    bool b = Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value);
                    if (b == true)
                    {
                        dicIecinstanceList.Add(i, dicInstance[i]);
                    }
                }
            }
            if (dicIecinstanceList.Count > 0)
            {
                dataGridView2.Rows.Clear();
                foreach (KeyValuePair<int, List<IECInstance>> kv in dicIecinstanceList)
                {
                    foreach (IECInstance iec in kv.Value)
                    {
                        BMECObject bemc = new BMECObject(iec);
                        ulong id = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bemc);
                        dataGridView2.Rows.Add(true, id.ToString(), iec.ClassDefinition.Name, iec["NOMINAL_DIAMETER"].DoubleValue.ToString(), iec["LINENUMBER"].StringValue);
                    }
                }
            }


        }

        /// <summary>
        /// 获取符合条件的数据
        /// </summary>
        /// <param name="ecList"></param>
        /// <returns></returns>
        public Dictionary<int, List<IECInstance>> getData(ECInstanceList ecList)
        {
            Dictionary<int, List<IECInstance>> dicIecList = new Dictionary<int, List<IECInstance>>();
            int key = 0;
            foreach (IECInstance ec in ecList)
            {
                bool b = BMECApi.Instance.InstanceDefinedAsClass(ec, "PIPING_COMPONENT", true);
                if (b)
                {
                    string lineNumber = ec["LINENUMBER"].StringValue;
                    if (dicIecList.Count == 0)
                    {
                        List<IECInstance> iecList = new List<IECInstance>();
                        iecList.Add(ec);
                        dicIecList.Add(key, iecList);
                    }
                    else
                    {
                        string lineNumber1 = ec["LINENUMBER"].StringValue;
                        bool isCz = false;
                        int key1 = key;
                        foreach (KeyValuePair<int, List<IECInstance>> kv in dicIecList)
                        {
                            string lineNumber2 = kv.Value[0]["LINENUMBER"].StringValue;
                            if (lineNumber1.Equals(lineNumber2))
                            {
                                isCz = true;
                                key1 = kv.Key;
                                break;
                            }
                        }
                        if (isCz)
                        {
                            dicIecList[key1].Add(ec);
                        }
                        else
                        {
                            key++;
                            List<IECInstance> iecList2 = new List<IECInstance>();
                            iecList2.Add(ec);
                            dicIecList.Add(key, iecList2);
                        }
                    }
                }
            }
            return dicIecList;
        }

        /// <summary>
        /// 提交当前单元格更改的数据（用来触发更改事件）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isolate = !isolate;
            if (isolate)
            {
                button1.Text = "取消单独显示";
            }
            else
            {
                button1.Text = "开启单独显示";
            }
            xianshi();
            #region
            //if(button1.Text.Equals("开启单独显示"))
            //{
            //    button1.Text = "取消单独显示";
            //    if(dataGridView2.Rows.Count>0)
            //    {
            //        for(int i=0;i<dataGridView2.Rows.Count;i++)
            //        {
            //            bool b = Convert.ToBoolean(dataGridView2.Rows[i].Cells[0].Value);
            //            if(b)
            //            {
            //                ulong id = Convert.ToUInt64(dataGridView2.Rows[i].Cells["ID"].Value);
            //            }
            //        }
            //    }
            //}
            #endregion
        }

        private void button2_Click(object sender, EventArgs e)
        {
            hiLite = !hiLite;
            if (hiLite)
            {
                button2.Text = "取消高亮显示";
            }
            else
            {
                button2.Text = "开启高亮显示";
            }
            xianshi();
        }

        /// <summary>
        /// 控制显示  高亮 放大  独立
        /// </summary>
        public void xianshi()
        {
            EquipmentManager.ViewComponent(null, false, false, false);
            if (dataGridView2.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    bool b = Convert.ToBoolean(dataGridView2.Rows[i].Cells[0].Value);
                    if (b)
                    {
                        ulong id = Convert.ToUInt64(dataGridView2.Rows[i].Cells["ID"].Value);
                        BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
                        EquipmentManager.ViewComponent(bmec.Instance, hiLite, zoom, isolate);
                    }
                }
            }
        }

        private void dataGridView2_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridView2.IsCurrentCellDirty)
            {
                dataGridView2.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0)
                return;
            if (e.RowIndex == -1)
                return;
            if (isolate || hiLite)
            {
                xianshi();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<BMECObject> bmecList = new List<BMECObject>();
            if (dataGridView2.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    bool b = Convert.ToBoolean(dataGridView2.Rows[i].Cells[0].Value);
                    if (b)
                    {
                        ulong id = Convert.ToUInt64(dataGridView2.Rows[i].Cells["ID"].Value);
                        BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
                        if (bmec.Instance != null)
                        {
                            bmecList.Add(bmec);
                        }
                    }
                }
            }
            UpdateProForm upForm = UpdateProForm.instance(bmecList, this);
#if DEBUG
#else
            upForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateProForm));
            upForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            upForm.Show();
        }

        /// <summary>
        /// 将选中行变为勾选 其它行取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                if (dataGridView1.Rows.Count > 0)
                {
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        dataGridView1.Rows[i].Cells[0].Value = false;
                    }
                    if (dataGridView1.SelectedRows.Count > 0)
                    {
                        dataGridView1.SelectedRows[0].Cells[0].Value = true;
                    }
                }
            }
        }
    }
}
