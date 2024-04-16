using Bentley.ECObjects.Instance;
using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class EquipmentManagerModifyForm : /*Form*/
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        private EquipmentManagerForm equipmentManagerForm = null;

        public EquipmentManagerModifyForm()
        {
            InitializeComponent();
            LoadData();
        }
        public EquipmentManagerModifyForm(EquipmentManagerForm equipmentManagerForm) : this()
        {
            this.equipmentManagerForm = equipmentManagerForm;
        }

        //加载数据
        private void LoadData() {
            Dictionary<string, IECInstance> allUnit = EquipmentManager.GetAllEquipmentUnit();
            Dictionary<string, IECInstance> allService = EquipmentManager.GetAllEquipmentService();
            var unit_strs = allUnit.Keys;
            List<string> unit_list = new List<string>();
            foreach (var unit_str in unit_strs)
            {
                unit_list.Add(unit_str);
            }
            var service_strs = allService.Keys;
            List<string> service_list = new List<string>();
            foreach (var service_str in service_strs)
            {
                service_list.Add(service_str);
            }
            this.comboBox_equipmentunit.DataSource = unit_list;
            this.comboBox_equipmentservice.DataSource = service_list;
        }
        //确认修改
        private void button_OK_Click(object sender, EventArgs e)
        {
            if (equipmentManagerForm != null)
            {
                equipmentManagerForm.SetEquipmentNumber(this.textBox_equipmentNumber.Text);
                equipmentManagerForm.SetEquipmentUnit(this.comboBox_equipmentunit.Text);
                equipmentManagerForm.SetEquipmentService(this.comboBox_equipmentservice.Text);
            }
        }
    }
}
