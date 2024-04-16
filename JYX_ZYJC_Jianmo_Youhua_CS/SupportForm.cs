using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class SupportForm : UserControl//Adapter//Form//
    {
        public double cWidth;
        public double cHeight;
        public double cThickness;

        public double jyx_weight_dry;
        public string jyx_material;
        public string jyx_catalog;

        List<string> k_whz = new List<string>();
        DataRow v_whz;
        public Dictionary<string, DataRow> kv_whz = new Dictionary<string, DataRow>(); 
        public SupportForm()
        {
            InitializeComponent();
            GetProfile();
            textBox_length.ReadOnly = true;
            textBox_height.ReadOnly = true;
            textBox_height1.ReadOnly = true;
        }
        //选择截面下拉框
        private void comboBox_profile_SelectedIndexChanged(object sender, EventArgs e)
        {
            string k = (sender as ComboBox).Text;
            v_whz =  kv_whz[k];
            cWidth = (double)v_whz["B"];
            cHeight = (double)v_whz["H"];
            cThickness = (double)v_whz["t1"];

        }

        //是否启用设置参数
        private void checkBox_setParam_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_setParam.Checked)
            {
                textBox_length.ReadOnly = false;
                textBox_height.ReadOnly = false;
                textBox_height1.ReadOnly = false;
            }
            else
            {
                //Enable,readonly
                textBox_length.ReadOnly = true;
                textBox_height.ReadOnly = true;
                textBox_height1.ReadOnly = true;
            }
        }

        private void textBox_length_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void textBox_height_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox_height1_TextChanged(object sender, EventArgs e)
        {

        }

        private void GetProfile()
        {
            #region 第一版

            //Bentley.Interop.MicroStationDGN.Application app = Utilities.ComApp;
            //string path1 = BMECInstanceManager.FindConfigVariableName("OPENPLANT_WORKSET_STANDARDS") + @"Specs\SUPPORT.mdb";
            //string path = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source="+ path1;
            //OleDbConnection _oleDbConn = new OleDbConnection(path);
            //_oleDbConn.Open();

            //OleDbDataAdapter _oleDbAda = new OleDbDataAdapter("select * from SUPPORTS where EC_CLASS_NAME='C'", _oleDbConn); 
            // DataTable dt = new DataTable();

            //_oleDbAda.Fill(dt);

            //foreach (DataRow item in dt.Rows)
            //{
            //    string k = item["B"] + "X" + item["H"] + "X" + item["t1"];
            //    k_whz.Add(k);
            //    kv_whz.Add(k, item);
            //}
            //comboBox_profile.DataSource = k_whz;

            //_oleDbConn.Close();
            #endregion



            DataTable dt = Utility.seleteMdb();
            foreach (DataRow item in dt.Rows)
            {
                string k = item["B"] + "X" + item["H"] + "X" + item["t1"];

                jyx_weight_dry = Convert.ToDouble(item["WEIGHT_DRY"]);
                jyx_material = item["MATERIAL"].ToString(); 
                jyx_catalog = item["CATALOG"].ToString();

                k_whz.Add(k);
                kv_whz.Add(k, item);
            }

            comboBox_profile.DataSource = k_whz;
            
        }
        
    }
}
