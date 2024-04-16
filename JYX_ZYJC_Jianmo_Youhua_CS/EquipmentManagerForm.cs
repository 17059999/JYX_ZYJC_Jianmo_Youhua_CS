﻿using Bentley.DgnPlatformNET;
using Bentley.ECObjects.Instance;
using Bentley.Internal.MstnPlatformNET;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.OpenPlant.Modeler.Api;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class EquipmentManagerForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        //该窗体的实例
        private static EquipmentManagerForm instance = null;
        //列名
        private List<string> columnsName = new List<string>() { "CheckBoxColumn", "CERI_Equipment_Name", "EquipmentName", "EquipmentUnit", "EquipmentService", "EquipmentType", "CERI_Model", "CERI_Technical", "CERI_Quantity", "CERI_Remark", "CERI_Equipment_Code", "EquipmentInstanceId" };
        private List<string> columnsInstancePro = new List<string>() { "CheckBoxColumn", "CERI_Equipment_Name", "NAME", "UNIT", "SERVICE", "设备类型", "CERI_Model", "CERI_Technical", "CERI_Quantity", "CERI_Remark", "CERI_Equipment_Code", "InstanceId" };
        private List<string> columnsDisplayName = new List<string>() { "CheckBoxColumn", "设备名称", "设备编号", "Unit", "Service", "设备类型", "型号", "技术参数", "数量", "备注", "物料编码", "InstanceId" };
        private List<string> selectedInstanceId = new List<string>();
        //是否显示管嘴信息
        private bool isViewNozzleInfo = false;
        //管嘴信息列名及显示的列名
        private List<string> nozzleColumnsName = new List<string>() { "CheckBoxColumn", "Name", "EquipmentTag", "Number", "State", "Type", "Datum", "A", "B", "C", "D", "E", "R", "T", "Service", "LineNumber", "Specification", "NominalDiameter", "InsulationThickness", "InstanceId" };
        private List<string> nozzlePropertiesName = new List<string>() { "CheckBoxColumn", "NAME", "EQUIPMENT_TAG", "NUMBER", "STATE", "TYPE_FOR_DATUM", "DATUM", "PARAM_A", "PARAM_B", "PARAM_C", "PARAM_D", "PARAM_E", "PARAM_R", "PARAM_T", "SERVICE", "LINENUMBER", "SPECIFICATION", "NOMINAL_DIAMETER", "INSULATION_THICKNESS", "InstanceId" };
        //查询 Model 中的设备，添加到数据表中
        private List<IECInstance> allEquipmentInstance = new List<IECInstance>();
        private List<IECInstance> allNozzleInstance = new List<IECInstance>();
        //批量修改元素的容器
        private List<IECInstance> modifyInstances = new List<IECInstance>();
        //批量修改元素的容器
        private List<IECInstance> modifyNozzleInstances = new List<IECInstance>();
        //高亮
        private bool hiLite = false;
        //聚焦
        private bool zoom = false;
        //单独显示
        private bool isolate = false;
        //高亮
        private bool nozzlehiLite = false;
        //聚焦
        private bool nozzlezoom = false;
        //单独显示
        private bool nozzleisolate = false;
        //修改属性窗体
        private EquipmentManagerModifyForm modifyForm = null;
        //修改的属性
        private string equipmentNumber = "";
        private string equipmentUnit = "";
        private string equipmentService = "";
        //窗体属性
        private int controlsOffset = 6;
        private int formDefaultHeightHide = 239;
        private int formDefaultHeightShow = 470;
        //当前需要设置列名的 DataGridView
        private DataGridView selectColumnsDataGridView;
        private bool isResizePanel = false;
        private int minDataGridViewSize = 50;
        private int cursorSizeDistance = 5;
        private object cellValueBeginEdit = "";
        //存储修改过的实例信息
        private List<ChangeInfo> infoList = new List<ChangeInfo>();
        private List<ChangeInfo> nozzleInfoList = new List<ChangeInfo>();
        private bool isSave = true;


        private struct ChangeInfo
        {
            public ChangeInfo(IECInstance _currentInstance, string _columnsInstanceProName, object _currentValue,int _index)
            {
                currentInstance = _currentInstance;
                columnsInstanceProName = _columnsInstanceProName;
                currentValue = _currentValue;
                index = _index;
            }
            public IECInstance currentInstance;
            public string columnsInstanceProName;
            public object currentValue;
            public int index;
        }

        //获取该窗体的实例
        public static EquipmentManagerForm getInstance()
        {
            if (instance == null)
            {
                instance = new EquipmentManagerForm();
            }
            return instance;
        }

        //构造
        private EquipmentManagerForm()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();

            Type dataGridViewEquipType = this.dataGridView_equipment.GetType();
            PropertyInfo pie = dataGridViewEquipType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pie.SetValue(this.dataGridView_equipment, true, null);

            Type dataGridViewNozzleType = this.dataGridView_nozzleInfo.GetType();
            PropertyInfo pin = dataGridViewNozzleType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pin.SetValue(this.dataGridView_nozzleInfo, true, null);
        }

        //窗体加载
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.LoadData();
            this.SetFormStyle();
        }

        //加载数据
        private void LoadData()
        {
            this.dataGridView_equipment.Columns.Clear();
            if (columnsName.Count != columnsDisplayName.Count)
            {
                System.Windows.Forms.MessageBox.Show("列名数量不匹配，请确认是否遗漏或存在非法列名");
                this.Close();
                return;
            }
            for (int i = 0; i < columnsName.Count; i++)
            {
                if (columnsName[i].Equals("CheckBoxColumn"))
                {
                    DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                    this.dataGridView_equipment.Columns.Add(chk);
                    this.dataGridView_equipment.Columns[i].MinimumWidth = 30;
                    this.dataGridView_equipment.Columns[i].Name = columnsName[i];
                    this.dataGridView_equipment.Columns[i].HeaderText = "";
                }
                else if (columnsName[i].Equals("EquipmentInstanceId"))
                {
                    this.dataGridView_equipment.Columns.Add(columnsName[i], columnsDisplayName[i]);
                    this.dataGridView_equipment.Columns[i].Visible = false;
                }
                else if (columnsName[i].Equals("EquipmentUnit") || columnsName[i].Equals("EquipmentService"))
                {
                    //((DataGridViewComboBoxColumn)this.dataGridView_equipment.Columns[i]).Items.AddRange(EquipmentManager.GetAllEquipmentUnit());
                    DataGridViewComboBoxColumn cbc = new DataGridViewComboBoxColumn();
                    
                    if (columnsName[i].Equals("EquipmentUnit"))
                    {
                        cbc.Items.Add("UNIT");
                        var unit_strs = EquipmentManager.GetAllEquipmentUnit().Keys;
                        List<string> unit_list = new List<string>();
                        foreach (var unit_str in unit_strs)
                        {
                            unit_list.Add(unit_str);
                        }
                        cbc.Items.AddRange(unit_list.ToArray());//TODO
                    }
                    else
                    {
                        cbc.Items.Add("SERVICE");
                        var service_strs = EquipmentManager.GetAllEquipmentService().Keys;
                        List<string> service_list = new List<string>();
                        foreach (var service_str in service_strs)
                        {
                            service_list.Add(service_str);
                        }
                        cbc.Items.AddRange(service_list.ToArray());//TODO
                    }

                    this.dataGridView_equipment.Columns.Add(cbc);
                    this.dataGridView_equipment.Columns[i].MinimumWidth = 30;
                    this.dataGridView_equipment.Columns[i].Name = columnsName[i];
                    this.dataGridView_equipment.Columns[i].HeaderText = columnsDisplayName[i];
                    //var a = this.dataGridView_equipment.Columns[i].CellTemplate.Value;
                    //var b = this.dataGridView_equipment.Columns[i].CellTemplate;
                }
                else
                {
                    this.dataGridView_equipment.Columns.Add(columnsName[i], columnsDisplayName[i]);
                    this.dataGridView_equipment.Columns[i].ReadOnly = false;
                    this.dataGridView_equipment.Columns[i].MinimumWidth = 50;
                }
            }
            this.dataGridView_equipment.Rows.Clear();
            this.dataGridView_equipment.Controls.Clear();
            //TODO
            allEquipmentInstance = EquipmentManager.GetAllEquipment();

            int RowsCount = 0;
            foreach (IECInstance instance in allEquipmentInstance)
            {
                
                RowsCount = this.dataGridView_equipment.Rows.Add();
                for (int i = 0; i < this.dataGridView_equipment.Columns.Count; i++)
                {
                    try
                    {
                        string currentColumnsName = this.dataGridView_equipment.Columns[i].Name;
                        if (currentColumnsName != "CheckBoxColumn")
                        {
                            if (currentColumnsName == "EquipmentInstanceId")
                            {
                                this.dataGridView_equipment.Rows[RowsCount].Cells[i].Value = instance.InstanceId;
                            }
                            else if (currentColumnsName == "EquipmentType")
                            {
                                this.dataGridView_equipment.Rows[RowsCount].Cells[i].ReadOnly = true;
                                this.dataGridView_equipment.Rows[RowsCount].Cells[i].Style.ForeColor = Color.Gray;
                                this.dataGridView_equipment.Rows[RowsCount].Cells[i].Value = instance.ClassDefinition.Name;
                            }
                            else if (currentColumnsName == "EquipmentUnit")
                            {
                                //DataGridViewComboBoxCell dataGridViewComboBoxCell = new DataGridViewComboBoxCell();
                                //dataGridViewComboBoxCell.DataSource = EquipmentManager.GetAllEquipmentUnit();
                                int index = columnsName.FindIndex(arg => arg == currentColumnsName);
                                //dataGridViewComboBoxCell.Value = instance[columnsInstancePro[index]].StringValue;
                                string tempvalue = instance[columnsInstancePro[index]].StringValue;
                                if (tempvalue != null)
                                {
                                    if (((DataGridViewComboBoxColumn)this.dataGridView_equipment.Columns[i]).Items.Contains(tempvalue))
                                    {
                                        ((DataGridViewComboBoxCell)this.dataGridView_equipment.Rows[RowsCount].Cells[i]).Value = tempvalue;
                                    }
                                    else
                                    {
                                        ((DataGridViewComboBoxCell)this.dataGridView_equipment.Rows[RowsCount].Cells[i]).Value = "UNIT";
                                    }
                                }

                            }
                            else if (currentColumnsName == "EquipmentService")
                            {
                                //DataGridViewComboBoxCell dataGridViewComboBoxCell = new DataGridViewComboBoxCell();
                                //dataGridViewComboBoxCell.DataSource = EquipmentManager.GetAllEquipmentService();
                                int index = columnsName.FindIndex(arg => arg == currentColumnsName);
                                //dataGridViewComboBoxCell.Value = instance[columnsInstancePro[index]].StringValue;
                                //this.dataGridView_equipment.Rows[RowsCount].Cells[i] = dataGridViewComboBoxCell;
                                string tempvalue = instance[columnsInstancePro[index]].StringValue;
                                if (tempvalue != null)
                                {
                                    if (((DataGridViewComboBoxColumn)this.dataGridView_equipment.Columns[i]).Items.Contains(tempvalue))
                                    {
                                        ((DataGridViewComboBoxCell)this.dataGridView_equipment.Rows[RowsCount].Cells[i]).Value = tempvalue;
                                    }
                                    else
                                    {
                                        ((DataGridViewComboBoxCell)this.dataGridView_equipment.Rows[RowsCount].Cells[i]).Value = "SERVICE";
                                    }
                                }
                            }
                            else
                            {
                                int index = columnsName.FindIndex(arg => arg == currentColumnsName);
                                if (!instance[columnsInstancePro[index]].IsNull)
                                {
                                    string ff = instance[columnsInstancePro[index]].StringValue;
                                    this.dataGridView_equipment.Rows[RowsCount].Cells[i].Value = ff;
                                }
                                
                                
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString());
                    }
                }
            }
        }

        //TODO 按照列命填数据
        private void SetDataForColumnsName()
        {
            throw new NotImplementedException();
        }

        //刷新数据
        private void RefreshData(bool isUpdateInstance)
        {
            //EquipmentManager.ViewComponent(null, false, false, false);
            //if (selectedRowsIndex != -1)
            //{
            //    isolateInstance = allEquipmentInstance[selectedRowsIndex];
            //    hiliteInstance = allEquipmentInstance[selectedRowsIndex];
            //}
            //EquipmentManager.ViewComponent(isolateInstance, hiLite, zoom, isolate);
            //EquipmentManager.ViewComponent(hiliteInstance, hiLite, zoom, isolate);
            EquipmentManager.ViewComponent(null, false, false, false);
            for (int i = 0; i < modifyNozzleInstances.Count; i++)
            {
                //EquipmentManager.ViewComponent(modifyNozzleInstances[i], nozzlehiLite, nozzlezoom, nozzleisolate);
                if (nozzlehiLite || nozzleisolate)
                {
                    Bentley.OpenPlant.Modeler.Api.BMECApi.Instance.ViewComponent(modifyNozzleInstances[i], nozzlehiLite, nozzlezoom, nozzleisolate);
                }
            }
            for (int i = 0; i < modifyInstances.Count; i++)
            {
                //EquipmentManager.ViewComponent(modifyInstances[i], hiLite, zoom, isolate);
                if (hiLite || isolate)
                {
                    Bentley.OpenPlant.Modeler.Api.BMECApi.Instance.ViewComponent(modifyInstances[i], hiLite, zoom, isolate);
                }
            }
            if (isUpdateInstance)
            {
                LoadData();
                SetFormStyle();
                modifyInstances.Clear();
                //modifyBMECObject.Clear();
            }
        }

        //设置窗体样式
        private void SetFormStyle()
        {
            this.Text = "设备管理器";
            //this.MinimizeBox = false;
            //this.MaximizeBox = false;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            //equipment information datagridview
            this.dataGridView_equipment.AllowUserToAddRows = false;
            this.dataGridView_equipment.AllowUserToDeleteRows = false;
            this.dataGridView_equipment.RowHeadersVisible = false;
            this.dataGridView_equipment.AllowUserToResizeColumns = true;
            this.dataGridView_equipment.AllowUserToResizeRows = false;
            this.dataGridView_equipment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_equipment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView_equipment.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //nozzle information datagridview
            this.dataGridView_nozzleInfo.AllowUserToAddRows = false;
            this.dataGridView_nozzleInfo.AllowUserToDeleteRows = false;
            this.dataGridView_nozzleInfo.RowHeadersVisible = false;
            this.dataGridView_nozzleInfo.AllowUserToResizeColumns = true;
            this.dataGridView_nozzleInfo.AllowUserToResizeRows = false;
            this.dataGridView_nozzleInfo.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView_nozzleInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dataGridView_nozzleInfo.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            bool isVisiblenozzleInfo = false;

            button_modify.Visible = false;//不要这个批量修改了

            if (this.button_showComponents.Text.Contains("Hide"))
            {
                isVisiblenozzleInfo = true;
                //this.Height = 284 + 191 + 50;
                //this.ClientSize = new Size(this.ClientSize.Width, this.dataGridView_nozzleInfo.Location.Y + this.dataGridView_nozzleInfo.Height + controlsOffset);
                this.ClientSize = new Size(this.ClientSize.Width, formDefaultHeightShow);
            }
            else
            {
                isVisiblenozzleInfo = false;
                //this.Height = 284;
                this.ClientSize = new Size(this.ClientSize.Width, formDefaultHeightHide);
            }
            this.button_isolate_nozzle.Visible = isVisiblenozzleInfo;
            this.button_hilite_nozzle.Visible = isVisiblenozzleInfo;
            this.dataGridView_nozzleInfo.Visible = isVisiblenozzleInfo;
            //datagridviewOffsetY = this.dataGridView_nozzleInfo.Location.Y - this.dataGridView_equipment.Location.Y - this.dataGridView_equipment.Height;
        }

        //单独显示按钮
        private void button_isolate_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "关闭单独显示")
            {
                ((Button)sender).Text = "开启单独显示";
                isolate = false;
            }
            else
            {
                ((Button)sender).Text = "关闭单独显示";
                isolate = true;
            }
            RefreshData(false);
        }

        //高亮显示按钮
        private void button_hilite_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "关闭高亮显示")
            {
                ((Button)sender).Text = "开启高亮显示";
                hiLite = false;
            }
            else
            {
                ((Button)sender).Text = "关闭高亮显示";
                hiLite = true;
            }
            RefreshData(false);
        }

        //单独显示按钮
        private void button_nozzleisolate_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "关闭单独显示")
            {
                ((Button)sender).Text = "开启单独显示";
                nozzleisolate = false;
            }
            else
            {
                ((Button)sender).Text = "关闭单独显示";
                nozzleisolate = true;
            }
            RefreshData(false);
        }

        //高亮显示按钮
        private void button_nozzlehilite_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "关闭高亮显示")
            {
                ((Button)sender).Text = "开启高亮显示";
                nozzlehiLite = false;
            }
            else
            {
                ((Button)sender).Text = "关闭高亮显示";
                nozzlehiLite = true;
            }
            RefreshData(false);
        }

        //窗体关闭
        protected override void OnClosed(EventArgs e)
        {
            if (!isSave && System.Windows.Forms.MessageBox.Show("确认更改属性的值吗？", "设备管理器", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                foreach (var item in infoList)
                {                 
                    IECInstance currentInstance = item.currentInstance;
                    string columnsInstanceProName = item.columnsInstanceProName;
                    object currentValue = item.currentValue;
                    int index = item.index;

                    EquipmentManager.ModifyBMECObject(ref currentInstance, columnsInstanceProName, Convert.ToString(currentValue));
                    EquipmentManager.ModifyBMECObjectUnitService(ref currentInstance, equipmentUnit, equipmentService);
                    UpdateEquipmentLevel(currentInstance);

                    allEquipmentInstance[index] = currentInstance;

                    //button_modify_Click(_sender, _e);
                }

                //foreach (IECInstance item2 in modifyInstances)
                //{
                //    IECInstance itemRef = item2;
                //    if (equipmentNumber != "")
                //    {
                //        EquipmentManager.ModifyBMECObject(ref itemRef, "NUMBER", equipmentNumber);
                //    }
                //    EquipmentManager.ModifyBMECObjectUnitService(ref itemRef, equipmentUnit, equipmentService);
                //}
                RefreshData(true);

                LoadNozzleData();
            }

            base.OnClosed(e);
            ClearData();
            instance = null;
        }

        //清理方法
        private void ClearData()
        {
            EquipmentManager.ViewComponent(null, false, false, false);
        }

        public void SetEquipmentNumber(string value)
        {
            if (value != null)
            {
                this.equipmentNumber = value;
            }
        }

        public void SetEquipmentUnit(string value)
        {
            if (value != null)
            {
                this.equipmentUnit = value;
            }
        }

        public void SetEquipmentService(string value)
        {
            if (value != null)
            {
                this.equipmentService = value;
            }
        }

        //批量修改
        private void button_modify_Click(object sender, EventArgs e)
        {
            modifyForm = new EquipmentManagerModifyForm(this);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EquipmentManagerModifyForm));
            modifyForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            modifyForm.ShowDialog();

            if (modifyForm.DialogResult == DialogResult.OK)
            {
                foreach (IECInstance item in modifyInstances)
                {
                    IECInstance itemRef = item;
                    if (equipmentNumber != "")
                    {
                        EquipmentManager.ModifyBMECObject(ref itemRef, "NUMBER", equipmentNumber);
                    }
                    EquipmentManager.ModifyBMECObjectUnitService(ref itemRef, equipmentUnit, equipmentService);
                }
                RefreshData(true);
                LoadNozzleData();
            }
        }

        //显示设备管嘴信息
        private void button_showComponents_Click(object sender, EventArgs e)
        {
            isViewNozzleInfo = !isViewNozzleInfo;
            if (isViewNozzleInfo)
            {
                this.button_showComponents.Text = "Hide Component";
                this.ClientSize = new Size(this.ClientSize.Width, this.panel_nozzle.Location.Y + this.panel_nozzle.Height + controlsOffset);
                this.dataGridView_nozzleInfo.Visible = true;
                this.button_hilite_nozzle.Visible = true;
                this.button_isolate_nozzle.Visible = true;
            }
            else
            {
                this.button_showComponents.Text = "Show Component";
                this.ClientSize = new Size(this.ClientSize.Width, this.panel_equipment.Height + this.panel_equipment.Location.Y + controlsOffset);
                this.dataGridView_nozzleInfo.Visible = false;
                this.button_hilite_nozzle.Visible = false;
                this.button_isolate_nozzle.Visible = false;
            }
            LoadNozzleData();
        }

        //加载管嘴数据
        private void LoadNozzleData()
        {
            this.dataGridView_nozzleInfo.Columns.Clear();
            if (columnsName.Count != columnsDisplayName.Count)
            {
                System.Windows.Forms.MessageBox.Show("列名数量不匹配，请确认是否遗漏或存在非法列名");
                this.Close();
                return;
            }
            for (int i = 0; i < nozzleColumnsName.Count; i++)
            {
                if (nozzleColumnsName[i].Equals("CheckBoxColumn"))
                {
                    DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                    this.dataGridView_nozzleInfo.Columns.Add(chk);
                    this.dataGridView_nozzleInfo.Columns[i].MinimumWidth = 30;
                    this.dataGridView_nozzleInfo.Columns[i].Name = nozzleColumnsName[i];
                    this.dataGridView_nozzleInfo.Columns[i].HeaderText = "";
                }
                else if (nozzleColumnsName[i].Equals("InstanceId"))
                {
                    this.dataGridView_nozzleInfo.Columns.Add(nozzleColumnsName[i], nozzleColumnsName[i]);
                    this.dataGridView_nozzleInfo.Columns[i].Visible = false;
                }
                else
                {
                    this.dataGridView_nozzleInfo.Columns.Add(nozzleColumnsName[i], nozzleColumnsName[i]);
                }
            }
            this.dataGridView_nozzleInfo.Rows.Clear();
            List<IECInstance> selectedEquipmentInstance = new List<IECInstance>();
            selectedEquipmentInstance = modifyInstances;
            int RowsCount = 0;
            foreach (IECInstance equipmentInstance in selectedEquipmentInstance)
            {
                List<IECInstance> nozzles = EquipmentManager.GetAllNozzleInstance(equipmentInstance);
                foreach (IECInstance nozzle in nozzles)
                {
                    RowsCount = this.dataGridView_nozzleInfo.Rows.Add();
                    for (int i = 0; i < this.dataGridView_nozzleInfo.Columns.Count; i++)
                    {
                        try
                        {
                            string currentColumnsName = this.dataGridView_nozzleInfo.Columns[i].Name;
                            if (currentColumnsName != "CheckBoxColumn")
                            {
                                if (currentColumnsName == "InstanceId")
                                {
                                    this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].Value = nozzle.InstanceId;
                                }
                                else
                                {
                                    int index = nozzleColumnsName.FindIndex(arg => arg == currentColumnsName);
                                    this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].Value = nozzle[nozzlePropertiesName[index]].StringValue;
                                    if (nozzleColumnsName[i].Equals("Name") || nozzleColumnsName[i].Equals("EquipmentTag"))
                                    {
                                        this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].ReadOnly = true;//
                                        this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].Style.ForeColor = Color.Gray;
                                    }
                                }
                                if (currentColumnsName != "Number")
                                {
                                    this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].ReadOnly = true;
                                    this.dataGridView_nozzleInfo.Rows[RowsCount].Cells[i].Style.ForeColor = Color.Gray;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }
                }
            }
        }

        //提交临时修改的数据
        private void dataGridView_equipment_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (((DataGridView)sender).IsCurrentCellDirty)
            {
                ((DataGridView)sender).CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        //单元格数值改变时
        private void dataGridView_equipment_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (((DataGridView)sender).Columns[e.ColumnIndex].Name != "CheckBoxColumn") return;
            if (e.RowIndex < 0) return;
            object value = (((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex]).FormattedValue;

            object tempInstanceId = ((DataGridView)sender).Rows[e.RowIndex].Cells["EquipmentInstanceId"].Value;
            //IECInstance tempInstance = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstanceByInstanceId(tempInstanceId.ToString());
            IECInstance tempInstance = null;
            int instanceindex;
            FindInstanceById(allEquipmentInstance, tempInstanceId.ToString(), ref tempInstance, out instanceindex);
            if ((bool)((DataGridView)sender).Rows[e.RowIndex].Cells[0].EditedFormattedValue)
            {
                bool flag = false;
                foreach (IECInstance instance in modifyInstances)
                {
                    if (instance.InstanceId == tempInstanceId.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag) return;
                modifyInstances.Add(tempInstance);
            }
            else
            {
                int index = -1;
                for (int i = 0; i < modifyInstances.Count; i++)
                {
                    if (modifyInstances[i].InstanceId == tempInstanceId.ToString())
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    modifyInstances.RemoveAt(index);
                }
            }
            RefreshData(false);
            LoadNozzleData();
        }

        //TODO
        public static List<string> GetAllPropertiesOfInstance(IECInstance instance)
        {
            throw new NotImplementedException();
        }

        private void dataGridView_equipment_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string currentColumnsName = ((DataGridView)sender).Columns[e.ColumnIndex].Name;
            if (currentColumnsName == "CheckBoxColumn")
            {
                return;
            }
            object currentValue = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            // if (currentValue != null && cellValueBeginEdit != null)
            {
                //当前更改的值与以前的值不同时
                if (Convert.ToString(currentValue) != Convert.ToString(cellValueBeginEdit))
                {
                    isSave = false;
                    int columnsInstanceProIndex = columnsName.FindIndex(a => a == currentColumnsName);
                    string columnsInstanceProName = columnsInstancePro[columnsInstanceProIndex];
                    object currentInstanceId = ((DataGridView)sender).Rows[e.RowIndex].Cells["EquipmentInstanceId"].Value;
                    //IECInstance currentInstance = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstanceByInstanceId(currentInstanceId.ToString());
                    IECInstance currentInstance = null;
                    int index;
                    FindInstanceById(allEquipmentInstance, currentInstanceId.ToString(), ref currentInstance, out index);

                    try
                    {
                        //收集修改的信息
                        infoList.Add(new ChangeInfo(currentInstance, columnsInstanceProName, currentValue, index));
                    }
                    catch (Exception ex1)
                    {
                        System.Windows.Forms.MessageBox.Show(ex1.ToString());
                    }

                    //object b = ((DataGridView)sender).Rows[e.RowIndex].Cells["EquipmentService"].Value;
                }
            }
        }

        private void dataGridView_equipment_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (((DataGridView)sender).Columns[e.ColumnIndex].Name == "CheckBoxColumn")
            {
                return;
            }
            cellValueBeginEdit = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dataGridView_nozzleInfo_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (((DataGridView)sender).Columns[e.ColumnIndex].Name != "CheckBoxColumn") return;
            if (e.RowIndex < 0) return;
            object value = (((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex]).FormattedValue;

            object tempInstanceId = ((DataGridView)sender).Rows[e.RowIndex].Cells["InstanceId"].Value;
            IECInstance tempInstance = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstanceByInstanceId(tempInstanceId.ToString());
            if ((bool)((DataGridView)sender).Rows[e.RowIndex].Cells[0].EditedFormattedValue)
            {
                bool flag = false;
                foreach (IECInstance instance in modifyNozzleInstances)
                {
                    if (instance.InstanceId == tempInstanceId.ToString())
                    {
                        flag = true;
                    }
                }
                if (flag) return;
                modifyNozzleInstances.Add(tempInstance);
            }
            else
            {
                int index = -1;
                for (int i = 0; i < modifyNozzleInstances.Count; i++)
                {
                    if (modifyNozzleInstances[i].InstanceId == tempInstanceId.ToString())
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    modifyNozzleInstances.RemoveAt(index);
                }
            }
            RefreshData(false);
        }

        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);
            Cursor currentCursor = this.Cursor;
            this.panel_equipment.Width = this.ClientSize.Width;
            this.dataGridView_equipment.Location = new Point(controlsOffset, this.button_hilite.Height + 2 * controlsOffset);
            this.dataGridView_equipment.Width = this.ClientSize.Width - (controlsOffset << 2);
            this.panel_nozzle.Width = this.ClientSize.Width;
            this.dataGridView_nozzleInfo.Location = new Point(controlsOffset, this.button_hilite.Height + 2 * controlsOffset);
            this.dataGridView_nozzleInfo.Width = this.ClientSize.Width - (controlsOffset << 2);

            if (this.button_showComponents.Text.Contains("Hide"))
            {
                int offsetHeight = this.ClientSize.Height - this.panel_equipment.Height - this.panel_nozzle.Height;
                this.panel_equipment.Height += offsetHeight / 2;
                this.panel_nozzle.Height += offsetHeight / 2;
            }
            else
            {
                this.panel_equipment.Height = this.ClientSize.Height;
            }
        }

        private void dataGridView_nozzleInfo_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            cellValueBeginEdit = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        private void dataGridView_nozzleInfo_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string currentColumnsName = ((DataGridView)sender).Columns[e.ColumnIndex].Name;
            //if (currentColumnsName != "Number")
            //{
            //    return;
            //}
            object currentValue = ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (Convert.ToString(currentValue) != Convert.ToString(cellValueBeginEdit))
            {
                //if (currentValue.ToString() != cellValueBeginEdit.ToString())
                //{
                isSave = false;
                int columnsInstanceProIndex = nozzleColumnsName.FindIndex(a => a == currentColumnsName);
                string columnsInstanceProName = nozzlePropertiesName[columnsInstanceProIndex];
                object currentInstanceId = ((DataGridView)sender).Rows[e.RowIndex].Cells["InstanceId"].Value;
                IECInstance currentInstance = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstanceByInstanceId(currentInstanceId.ToString());
                int index;
                FindInstanceById(allNozzleInstance, currentInstanceId.ToString(), ref currentInstance, out index);
                try
                {
                    nozzleInfoList.Add(new ChangeInfo(currentInstance, columnsInstanceProName, currentValue, index));


                    //EquipmentManager.ModifyBMECObject(ref currentInstance, columnsInstanceProName, Convert.ToString(currentValue));

                    //if (currentInstance[columnsInstanceProName].Type.Name == "string")
                    //{
                    //    currentInstance.SetString(columnsInstanceProName, currentValue.ToString());
                    //}
                    //else if (currentInstance[columnsInstanceProName].Type.Name == "double")
                    //{
                    //    currentInstance.SetDouble(columnsInstanceProName, Convert.ToDouble(currentValue));
                    //}
                    //else
                    //{
                    //    throw new NotImplementedException();
                    //}
                }
                catch (Exception)
                {
                    System.Windows.Forms.MessageBox.Show("请不要输入非法的值!");
                    ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cellValueBeginEdit;
                    return;
                }
                        //RefreshDataDridView(currentInstance, e.RowIndex, nozzleColumnsName, nozzlePropertiesName, (DataGridView)sender);
                //else
                //    {
                //        ((DataGridView)sender).Rows[e.RowIndex].Cells[e.ColumnIndex].Value = cellValueBeginEdit;
                //    }
                //}
            }
        }

        private void RefreshDataDridView(IECInstance instance, int rowIndex, List<string> columnsNames, List<string> properties, DataGridView dataGridView)
        {
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                if (dataGridView.Columns[i].Name == "CheckBoxColumn" || dataGridView.Columns[i].Name.Contains("InstanceId"))
                {
                    continue;
                }
                else
                {
                    if (dataGridView.Columns[i].Name == "EquipmentType")
                    {
                        dataGridView.Rows[rowIndex].Cells[i].Value = instance.ClassDefinition.Name;
                    }
                    else
                    {
                        int index = columnsNames.FindIndex(a => a == dataGridView.Columns[i].Name);
                        string proName = properties[index];
                        if (!instance[proName].IsNull)
                        {
                            string currentValue = instance[proName].StringValue;
                            dataGridView.Rows[rowIndex].Cells[i].Value = currentValue;
                        }
                        
                    }
                }
            }
        }

        private void panel_equipment_MouseDown(object sender, MouseEventArgs e)
        {
            Point ep = e.Location;
            if (ep.Y >= ((Panel)sender).Height - 5)
            {
                if (e.Button == MouseButtons.Left)
                {
                    isResizePanel = true;
                }
            }
        }

        private void panel_equipment_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.button_showComponents.Text.Contains("Hide"))
            {
                Point ep = e.Location;
                if (isResizePanel)
                {
                    this.panel_equipment.Cursor = Cursors.SizeNS;
                    if (e.Button == MouseButtons.Left)
                    {
                        int currentPanelHeight = ep.Y - this.panel_equipment.Location.Y;
                        if (currentPanelHeight < this.button_isolate.Height + 3 * controlsOffset + minDataGridViewSize)
                        {
                            currentPanelHeight = this.button_isolate.Height + 3 * controlsOffset + minDataGridViewSize;
                        }
                        else if (this.ClientSize.Height - currentPanelHeight < this.button_isolate.Height + 3 * controlsOffset + minDataGridViewSize)
                        {
                            currentPanelHeight = this.ClientSize.Height - (this.button_isolate.Height + 3 * controlsOffset + minDataGridViewSize);
                        }
                        this.panel_equipment.Height = currentPanelHeight;
                        this.panel_nozzle.Height = this.ClientSize.Height - currentPanelHeight;
                    }
                }
                else
                {
                    if (ep.Y >= this.panel_equipment.Height - cursorSizeDistance)
                    {
                        this.panel_equipment.Cursor = Cursors.SizeNS;
                    }
                    else
                    {
                        this.panel_equipment.Cursor = Cursors.Default;
                    }
                }
            }
        }

        private void panel_equipment_MouseUp(object sender, MouseEventArgs e)
        {
            isResizePanel = false;
        }

        private void panel_equipment_ClientSizeChanged(object sender, EventArgs e)
        {
            this.dataGridView_equipment.Height = this.panel_equipment.Height - (this.button_isolate.Height + 3 * controlsOffset);
        }

        private void panel_nozzle_ClientSizeChanged(object sender, EventArgs e)
        {
            this.dataGridView_nozzleInfo.Height = this.panel_nozzle.Height - (this.button_isolate_nozzle.Height + 3 * controlsOffset);
        }

        private void panel_equipment_MouseLeave(object sender, EventArgs e)
        {
            if (!(Control.MouseButtons == MouseButtons.Left))
            {
                this.panel_equipment.Cursor = Cursors.Default;
            }
        }

        private void dataGridView_equipment_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectColumnsDataGridView = (DataGridView)sender;
                this.contextMenuStrip_equipment.Show(MousePosition.X, MousePosition.Y);
            }
        }

        private void ToolStripMenuItem_equipment_selectColumns_Click(object sender, System.EventArgs e)
        {
            EquipmentManagerSelectColumnsForm selectColumnsForm = new EquipmentManagerSelectColumnsForm(selectColumnsDataGridView);
            //selectColumnsForm.Show();
            if (selectColumnsForm.ShowDialog() == DialogResult.OK)
            {
                DataTable table = selectColumnsForm.DataTable;
                int visibleCount = 1;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    if (Convert.ToBoolean(table.Rows[i]["Visible"]))
                    {
                        selectColumnsDataGridView.Columns[table.Rows[i]["ColumnName"].ToString()].DisplayIndex = visibleCount;
                        selectColumnsDataGridView.Columns[table.Rows[i]["ColumnName"].ToString()].Visible = true;
                        visibleCount++;
                    }
                    else
                    {
                        selectColumnsDataGridView.Columns[table.Rows[i]["ColumnName"].ToString()].DisplayIndex = selectColumnsDataGridView.ColumnCount - visibleCount - 1;
                        selectColumnsDataGridView.Columns[table.Rows[i]["ColumnName"].ToString()].Visible = false;
                    }
                }
                selectColumnsForm.Close();
            }
        }

        private bool FindInstanceById(List<IECInstance> source, string instanceId, ref IECInstance instance, out int index)
        {
            index = -1;
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].InstanceId == instanceId)
                {
                    instance = source[i];
                    index = i;
                    return true;
                }
            }
            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach (IECInstance item in modifyInstances)
            {
                UpdateEquipmentLevel(item);
            }
        }

        private void UpdateEquipmentLevel(IECInstance currentInstance)
        {
            ulong id = JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(currentInstance);
            Bentley.DgnPlatformNET.ElementId elem_id = new ElementId(ref id);
            Bentley.DgnPlatformNET.Elements.Element elem = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().FindElementById(elem_id);
            

            ElementPropertiesSetter elem_pro_setter = new ElementPropertiesSetter();
            

            OpenPlantSymbolProvider.AddOpenPlantSymbolProvider(); //需要加这个 不加这个返回text null
            //NamedExpressionManager
            NamedExpression expressionByName = NamedExpressionManager.GetExpressionByName(SymbologyTypeLookup.GetModelSymbology(BMSymbolBasisType.EquipmentSymbology));
            BMECObject ecobject = new BMECObject(currentInstance);
            string text = (string)NamedExpressionManager.EvaluateExpression(expressionByName.Expression, ecobject, expressionByName.RequiredSymbolSets);
            string level_name = text.Split(':')[1];
            LevelHandle levelhandle =Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetLevelCache().GetLevelByName(level_name);

            int color, weight, style;
            color = weight = style = -1;
            //Marshal
            JYX_ZYJC_CLR.PublicMethod.getSymbologyFromString(ecobject, text,ref color,ref weight,ref style);
            
            elem_pro_setter.SetLevel(levelhandle.LevelId);
            elem_pro_setter.SetColor((uint)color);
            elem_pro_setter.SetWeight((uint)weight);
            elem_pro_setter.SetLinestyle(style, levelhandle.GetByLevelLineStyle().GetStyleParams());
            elem_pro_setter.Apply(elem);
            elem.ReplaceInModel(elem);
        }

        //点击保存按钮
        private void button_save_Click(object sender, EventArgs e)
        {
            if (isSave)
            {
                System.Windows.Forms.MessageBox.Show("数据已保存!");
                return;
            }
            if (System.Windows.Forms.MessageBox.Show("确认更改属性的值吗？", "设备管理器", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                //EquipmentManager.ModifyBMECObjects(ref instanceList, proNameList, valueList);

                foreach (var item in infoList)
                {
                    IECInstance currentInstance = item.currentInstance;
                    string columnsInstanceProName = item.columnsInstanceProName;
                    object currentValue = item.currentValue;
                    int index = item.index;

                    if (columnsInstanceProName.Equals("UNIT"))
                        equipmentUnit = currentValue.ToString();
                    if (columnsInstanceProName.Equals("SERVICE"))
                        equipmentService = currentValue.ToString();
                    EquipmentManager.ModifyBMECObject(ref currentInstance, columnsInstanceProName, currentValue.ToString());
                    //EquipmentManager.ModifyBMECObjectUnitService(ref currentInstance, equipmentUnit, equipmentService);

                    UpdateEquipmentLevel(currentInstance);

                    if (index >= 0)
                    {
                        allEquipmentInstance[index] = currentInstance;

                    }
                }

                foreach (var item in nozzleInfoList)
                {
                    IECInstance currentInstance = item.currentInstance;
                    string columnsInstanceProName = item.columnsInstanceProName;
                    object currentValue = item.currentValue;
                    int index = item.index;

                    if (columnsInstanceProName.Equals("UNIT"))
                        equipmentUnit = currentValue.ToString();
                    if (columnsInstanceProName.Equals("SERVICE"))
                        equipmentService = currentValue.ToString();
                    EquipmentManager.ModifyBMECObject(ref currentInstance, columnsInstanceProName, currentValue.ToString());
                    //EquipmentManager.ModifyBMECObjectUnitService(ref currentInstance, equipmentUnit, equipmentService);

                    UpdateEquipmentLevel(currentInstance);

                    if (index >= 0)
                    {
                        allNozzleInstance[index] = currentInstance;

                    }
                }
             
                infoList.Clear();
                RefreshData(true);

                LoadNozzleData();

                isSave = true;
            }

        }
    }
}
