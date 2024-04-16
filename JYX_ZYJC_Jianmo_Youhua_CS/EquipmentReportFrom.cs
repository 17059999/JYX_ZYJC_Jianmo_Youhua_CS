using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.ECObjects.Instance;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.OpenPlant.Modeler.Api;
using MOIE = Microsoft.Office.Interop.Excel;
using System.IO;
using Bentley.MstnPlatformNET.InteropServices;
using System.Diagnostics;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class EquipmentReportFrom : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        private static Bentley.Interop.MicroStationDGN.Application app1 = Utilities.ComApp;
        private static string path0 = BMECInstanceManager.FindConfigVariableName("MSDIR"); //相对路径
        //C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMetric\Standards\OpenPlant
        //bool isSele = false;
        public static EquipmentReportFrom reportFrom = null;
        public List<string> MsName = new List<string>();
        public Dictionary<string, List<string>> msJtName = new Dictionary<string, List<string>>();
        public ECInstanceList ecList = new ECInstanceList();
        //public Dictionary<int, Dictionary<int, List<IECInstance>>> eqDicList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
        public Dictionary<int, List<IECInstance>> eqDicList = new Dictionary<int, List<IECInstance>>();
        public Dictionary<int, Dictionary<int, List<IECInstance>>> piDicList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
        MOIE.Application app = null;
        public string openPath = "";
        public EquipmentReportFrom()
        {
            InitializeComponent();
            initialization();
        }

        public static EquipmentReportFrom instence()
        {
            if (reportFrom == null)
            {
                reportFrom = new EquipmentReportFrom();
            }
            else
            {
                reportFrom.Close();
                reportFrom = new EquipmentReportFrom();
            }
            return reportFrom;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (zlcomboBox1.Text.Equals("设备表"))
                {
                    fuzhi(ecList);
                    exportEqment();
                }
                else if (zlcomboBox1.Text.Equals("汇总设备表"))
                {
                    if (wlbmGroupcheckBox1.Checked)
                    {
                        hzfuzhi(ecList);
                        hzExport();
                    }
                    else
                    {
                        fuzhi(ecList);
                        hzExport();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                if(app!=null)
                {
                    app.Quit();
                    KillP.Kill(app);
                }               
            }
            finally
            {
                if (openPath != "")
                {
                    Process.Start(openPath);
                }
            }
            //ExportMaterials exClass = new ExportMaterials();
            //if (radioButton1.Checked == true)
            //{
            //    isSele = false;
            //}
            //if (radioButton2.Checked == true)
            //{
            //    isSele = true;
            //}
            //exClass.selectEquipmentExcel(isSele);
        }

        private void EquipmentReportFrom_Load(object sender, EventArgs e)
        {
            zlcomboBox1.DataSource = MsName;
            zlcomboBox1.SelectedIndex = 0;
            wlbmGroupcheckBox1.Checked = true;
            radioButton1.Checked = true;
        }

        public void initialization()
        {
            MsName.Add("设备表");
            MsName.Add("汇总设备表");
            List<string> grclbList = new List<string>();
            grclbList.Add("CQ-B3004-1909-1A通用设备表（国内）");
            grclbList.Add("CQ-B3004-1909-1B通用设备表（海外）");
            grclbList.Add("CQ-B3004-1909-4A管道设备表（国内）");
            grclbList.Add("CQ-B3004-1909-4B管道设备表（海外）");
            grclbList.Add("CQ-B3004-1909-11A压力管道设备表（国内）");
            msJtName.Add("设备表", grclbList);
            List<string> gdclbList = new List<string>();
            gdclbList.Add("CQ-B3004-1909-8A压力管道设备汇总表（国内）");
            msJtName.Add("汇总设备表", gdclbList);
        }

        private void zlcomboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lxcomboBox1.DataSource = msJtName[zlcomboBox1.Text];
            if (zlcomboBox1.Text.Equals("汇总设备表"))
            {
                wlbmGroupcheckBox1.Enabled = true;
            }
            else
            {
                wlbmGroupcheckBox1.Enabled = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                ecList = DgnUtilities.GetAllInstancesFromDgn();
                ecList = ReportFrom.attDisplayFil(ecList);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                ecList = DgnUtilities.GetSelectedInstances();
                if (ecList.Count == 0)
                {
                    MessageBox.Show("请先选择元素！");
                    radioButton1.Checked = true;
                }
            }
        }

        public void fuzhi(ECInstanceList iecList)
        {
            //Dictionary<int, Dictionary<int, List<IECInstance>>> eqList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            Dictionary<int, List<IECInstance>> eqList = new Dictionary<int, List<IECInstance>>();
            Dictionary<int, Dictionary<int, List<IECInstance>>> pipeList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (iecList.Count > 0)
            {
                int keyNumber = 0, keyNumber1 = 0;
                foreach (IECInstance iec in iecList)
                {
                    bool sheb = BMECApi.Instance.InstanceDefinedAsClass(iec, "EQUIPMENT", true);
                    if (sheb)
                    {
                        string sbNum = "";
                        if (iec.GetPropertyValue("CERI_Equipment_Num") != null)
                        {
                            sbNum = iec["CERI_Equipment_Num"].StringValue;
                        }
                        bool isSb = false;

                        foreach (KeyValuePair<int, List<IECInstance>> kv in eqList)
                        {
                            bool isrz = pdXt(iec, kv.Value[0], true);
                            if (isrz)
                            {
                                isSb = true;
                                eqList[kv.Key].Add(iec);
                            }
                            #region
                            //string sbNum1 = "";
                            //if (kv.Value[0][0].GetPropertyValue("CERI_Equipment_Num") != null)
                            //{
                            //    sbNum1 = kv.Value[0][0]["CERI_Equipment_Num"].StringValue;
                            //    if (sbNum.Equals(sbNum1))
                            //    {
                            //        isSb = true;
                            //        bool isRz = false;
                            //        foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            //        {
                            //            bool isrz1 = pdXt(iec, kvs.Value[0], true);
                            //            if (isrz1)
                            //            {
                            //                isRz = true;
                            //                eqList[kv.Key][kvs.Key].Add(iec);
                            //            }
                            //        }
                            //        if (!isRz)
                            //        {
                            //            List<IECInstance> rzList = new List<IECInstance>();
                            //            rzList.Add(iec);
                            //            eqList[kv.Key].Add(eqList[kv.Key].Count, rzList);
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                        if (!isSb)
                        {
                            List<IECInstance> zecList = new List<IECInstance>();
                            zecList.Add(iec);
                            //Dictionary<int, List<IECInstance>> zdicList = new Dictionary<int, List<IECInstance>>();
                            //zdicList.Add(0, zecList);
                            eqList.Add(keyNumber, zecList);
                            keyNumber++;
                        }
                        //eqDicList = eqList;
                    }
                    //bool isPsb = false;
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPING_COMPONENT", true); //查找ec的父类是否含有PIPING_COMPONENT
                    if (b)
                    {
                        string fenlei = iec["CERI_Classify"].StringValue;
                        //if (fenlei == "shebei")
                        if (fenlei == "shebei")
                        {
                            string pipeNum = "";
                            pipeNum = iec["NAME"].StringValue;
                            bool isP = false;
                            //int keyNumber = 0;
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in pipeList)
                            {
                                string pipenumber1 = kv.Value[0][0]["NAME"].StringValue;
                                if (pipeNum.Equals(pipenumber1))
                                {
                                    isP = true;
                                    bool ispz = false;
                                    foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                                    {
                                        bool iscz = pdXt(iec, kvs.Value[0], false);
                                        if (iscz)
                                        {
                                            ispz = true;
                                            pipeList[kv.Key][kvs.Key].Add(iec);
                                        }
                                    }
                                    if (!ispz)
                                    {
                                        List<IECInstance> iecpList = new List<IECInstance>();
                                        iecpList.Add(iec);
                                        pipeList[kv.Key].Add(pipeList[kv.Key].Count, iecpList);
                                    }
                                }
                            }
                            if (!isP)
                            {
                                List<IECInstance> iecPlist = new List<IECInstance>();
                                iecPlist.Add(iec);
                                Dictionary<int, List<IECInstance>> dicPlist = new Dictionary<int, List<IECInstance>>();
                                dicPlist.Add(0, iecPlist);
                                pipeList.Add(keyNumber1, dicPlist);
                                keyNumber1++;
                            }
                        }
                    }

                }
                //zlcomboBox1.Text.Equals("汇总设备表")
                if (zlcomboBox1.Text.Equals("汇总设备表"))
                    eqList = tjpaixu(eqList);
                else
                    eqList = paixu(eqList);
                eqDicList = eqList;
                piDicList = pipeList;
            }
        }

        public bool pdXt(IECInstance iec, IECInstance iec1, bool isEq)
        {
            string wlbm = "", sbmc = "", xh = "", gg = "", jscs = "", dw = "", bz = "";
            double dz = 0, dn = 0;
            if (isEq)
            {
                if (iec.GetPropertyValue("CERI_Equipment_Code") != null)
                {
                    wlbm = iec["CERI_Equipment_Code"].StringValue;
                }
                if (iec.GetPropertyValue("CERI_Equipment_Name") != null)
                {
                    sbmc = iec["CERI_Equipment_Name"].StringValue;
                }
                if (iec.GetPropertyValue("CERI_Model") != null)
                {
                    xh = iec["CERI_Model"].StringValue;
                }
                if (iec.GetPropertyValue("NOMINAL_DIAMETER") != null)
                {
                    dn = iec["NOMINAL_DIAMETER"].DoubleValue;
                }
                if (iec.GetPropertyValue("CERI_Technical") != null)
                {
                    jscs = iec["CERI_Technical"].StringValue;
                }
                dw = "个";
                if (iec.GetPropertyValue("CERI_Remark") != null)
                {
                    bz = iec["CERI_Remark"].StringValue;
                }
                if (iec.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                {
                    dz = iec["CERI_DRY_WEIGHT"].DoubleValue;
                }
            }
            else
            {
                if (iec.GetPropertyValue("CERI_MAT_GRADE") != null)
                {
                    wlbm = iec["CERI_MAT_GRADE"].StringValue;
                }
                else
                {
                    if (iec.GetPropertyValue("GRADE") != null)
                    {
                        wlbm = iec["GRADE"].StringValue;
                    }
                }
                if (iec.GetPropertyValue("CERI_SHORT_DESC") != null)
                {
                    sbmc = iec["CERI_SHORT_DESC"].StringValue;
                }
                else
                {
                    if (iec.GetPropertyValue("SHORT_DESCRIPTION") != null)
                    {
                        sbmc = iec["SHORT_DESCRIPTION"].StringValue;
                    }
                }
                if (iec.GetPropertyValue("CERI_PIECE_MARK") != null)
                {
                    xh = iec["CERI_PIECE_MARK"].StringValue;
                }
                else
                {
                    if (iec.GetPropertyValue("PIECE_MARK") != null)
                    {
                        xh = iec["PIECE_MARK"].StringValue;
                    }
                }
                if (iec.GetPropertyValue("CERI_MAIN_SIZE") != null)
                {
                    gg = iec["CERI_MAIN_SIZE"].StringValue;
                }
                if (iec.GetPropertyValue("NOTES") != null)
                {
                    bz = iec["NOTES"].StringValue;
                }
                bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                double dry_weight = 0;
                if (b)
                {
                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                    {
                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue * 1000;
                        dz = dry_weight;
                    }
                    else
                    {
                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                        {
                            dry_weight = iec["DRY_WEIGHT"].DoubleValue * 1000;
                            dz = dry_weight;
                        }
                    }
                    dw = "米";
                }
                else
                {
                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                    {
                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue;
                        dz = dry_weight;
                    }
                    else
                    {
                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                        {
                            dry_weight = iec["DRY_WEIGHT"].DoubleValue;
                            dz = dry_weight;
                        }
                    }
                    dw = "个";
                }
            }

            string wlbm1 = "", sbmc1 = "", xh1 = "", gg1 = "", jscs1 = "", dw1 = "", bz1 = "";
            double dz1 = 0, dn1 = 0;

            if (isEq)
            {
                if (iec1.GetPropertyValue("CERI_Equipment_Code") != null)
                {
                    wlbm1 = iec1["CERI_Equipment_Code"].StringValue;
                }
                if (iec1.GetPropertyValue("CERI_Equipment_Name") != null)
                {
                    sbmc1 = iec1["CERI_Equipment_Name"].StringValue;
                }
                if (iec1.GetPropertyValue("CERI_Model") != null)
                {
                    xh1 = iec1["CERI_Model"].StringValue;
                }
                if (iec1.GetPropertyValue("NOMINAL_DIAMETER") != null)
                {
                    dn1 = iec1["NOMINAL_DIAMETER"].DoubleValue;
                }
                if (iec1.GetPropertyValue("CERI_Technical") != null)
                {
                    jscs1 = iec1["CERI_Technical"].StringValue;
                }
                dw1 = "个";
                if (iec1.GetPropertyValue("CERI_Remark") != null)
                {
                    bz1 = iec1["CERI_Remark"].StringValue;
                }
                if (iec1.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                {
                    dz1 = iec1["CERI_DRY_WEIGHT"].DoubleValue;
                }
            }
            else
            {
                if (iec1.GetPropertyValue("CERI_MAT_GRADE") != null)
                {
                    wlbm1 = iec1["CERI_MAT_GRADE"].StringValue;
                }
                else
                {
                    if (iec1.GetPropertyValue("GRADE") != null)
                    {
                        wlbm1 = iec1["GRADE"].StringValue;
                    }
                }
                if (iec1.GetPropertyValue("CERI_SHORT_DESC") != null)
                {
                    sbmc1 = iec1["CERI_SHORT_DESC"].StringValue;
                }
                else
                {
                    if (iec1.GetPropertyValue("SHORT_DESCRIPTION") != null)
                    {
                        sbmc1 = iec1["SHORT_DESCRIPTION"].StringValue;
                    }
                }
                if (iec1.GetPropertyValue("CERI_PIECE_MARK") != null)
                {
                    xh1 = iec1["CERI_PIECE_MARK"].StringValue;
                }
                else
                {
                    if (iec1.GetPropertyValue("PIECE_MARK") != null)
                    {
                        xh1 = iec1["PIECE_MARK"].StringValue;
                    }
                }
                if (iec1.GetPropertyValue("CERI_MAIN_SIZE") != null)
                {
                    gg1 = iec1["CERI_MAIN_SIZE"].StringValue;
                }
                if (iec1.GetPropertyValue("NOTES") != null)
                {
                    bz1 = iec1["NOTES"].StringValue;
                }
                bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec1, "PIPE", true);
                double dry_weight1 = 0;
                if (b1)
                {
                    if (iec1.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                    {
                        dry_weight1 = iec1["CERI_WEIGHT_DRY"].DoubleValue * 1000;
                        dz1 = dry_weight1;
                    }
                    else
                    {
                        if (iec1.GetPropertyValue("DRY_WEIGHT") != null)
                        {
                            dry_weight1 = iec1["DRY_WEIGHT"].DoubleValue * 1000;
                            dz1 = dry_weight1;
                        }
                    }
                    dw1 = "米";
                }
                else
                {
                    if (iec1.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                    {
                        dry_weight1 = iec1["CERI_WEIGHT_DRY"].DoubleValue;
                        dz1 = dry_weight1;
                    }
                    else
                    {
                        if (iec1.GetPropertyValue("DRY_WEIGHT") != null)
                        {
                            dry_weight1 = iec1["DRY_WEIGHT"].DoubleValue;
                            dz1 = dry_weight1;
                        }
                    }
                    dw1 = "个";
                }
            }

            bool isP = false;
            if (wlbm == wlbm1 && sbmc == sbmc1 && xh == xh1 && gg == gg1 && jscs == jscs1 && dw == dw1 && bz == bz1 && dz == dz1 && dn1 == dn)
            {
                isP = true;
            }
            return isP;
        }

        public void exportEqment()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = path0; //设置初始目录
            sfd.Filter = "Excel Files(*.xlsx)|*.xlsx";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string excelPath = sfd.FileName.ToString();
                string excelName = excelPath.Substring(excelPath.LastIndexOf("\\") + 1);
                int i = excelName.LastIndexOf(".");
                string houzhui = excelName.Substring(i + 1);
                if (houzhui != "xlsx")
                {
                    MessageBox.Show("输入文件名格式不正确");
                    return;
                }
                if (File.Exists(excelPath)) //判断是否有同名的文件
                {
                    string excelname = excelName.Substring(0, excelName.LastIndexOf("."));
                    string datetime = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    //string.Format("{0:yyyyMMddHHmmssffff}",dt);
                    //string datetime = string.Format("{ 0:yyyyMMddHHmmssffff}", dt.ToString());
                    //excelname += "1.xlsx";
                    excelname = excelname + datetime + ".xlsx";
                    //string name = excelname.Substring(excelPath1.LastIndexOf("\\") + 1);
                    MessageBox.Show("该路径下存在同名的文件。修改后的文件名为：" + excelname);
                    string path = excelPath.Substring(0, excelPath.LastIndexOf("\\") + 1);
                    excelPath = path + excelname;
                }
                bool ishz = false;
                if (zlcomboBox1.Text.Equals("汇总设备表"))
                {
                    ishz = true;
                }
                string yName = lxcomboBox1.Text + ".xlsx";
                try
                {
                    if (ishz)
                    {
                        File.Copy(path0 + "\\JYXConfig\\汇总设备表\\" + yName, excelPath);
                    }
                    else
                    {
                        File.Copy(path0 + "\\JYXConfig\\设备表\\" + yName, excelPath);
                    }

                }
                catch
                {
                    if (ishz)
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\汇总设备表：下没有找到" + yName + "文件");
                    }
                    else
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\设备表：下没有找到" + yName + "文件");
                    }
                    return;
                }
                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();

                app = new MOIE.Application();
                MOIE.Workbooks wkb = app.Workbooks;
                MOIE.Workbook wb = wkb.Open(excelPath);
                //app.Visible = true;
                //wb=wkb.Open(excelPath1);
                MOIE.Worksheet s;
                if (ishz)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["压力管道设备汇总表-国内"];
                }
                else
                {
                    if (lxcomboBox1.SelectedIndex == 0)
                    {
                        s = (MOIE.Worksheet)wb.Worksheets["设备表-国内"];
                    }
                    else if (lxcomboBox1.SelectedIndex == 1)
                    {
                        s = (MOIE.Worksheet)wb.Worksheets["通用设备表-海外"];
                    }
                    else if (lxcomboBox1.SelectedIndex == 2)
                    {
                        s = (MOIE.Worksheet)wb.Worksheets["管道设备表-国内"];
                    }
                    else if (lxcomboBox1.SelectedIndex == 3)
                    {
                        s = (MOIE.Worksheet)wb.Worksheets["管道设备表-海外"];
                    }
                    else
                    {
                        s = (MOIE.Worksheet)wb.Worksheets["压力管道设备表-国内"];
                    }
                }

                bool isgd = false;
                if (lxcomboBox1.SelectedIndex == 2 || lxcomboBox1.SelectedIndex == 3 || lxcomboBox1.SelectedIndex == 4)
                {
                    isgd = true;
                }
                #region 导入设备
                int row = 7, index = 0;
                if (eqDicList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in eqDicList)
                    {

                        foreach (IECInstance iec1 in kv.Value)
                        {
                            index++;
                            s.Cells[row, 1] = index;
                            string sbbm = "";
                            if (iec1.GetPropertyValue("CERI_Equipment_Num") != null)
                            {
                                sbbm = iec1["CERI_Equipment_Num"].StringValue;
                            }
                            if (ishz)
                            {
                                s.Cells[row, 2] = sbbm;
                            }
                            else
                            {
                                if (isgd)
                                {
                                    s.Cells[row, 3] = sbbm;
                                }
                                else
                                {
                                    s.Cells[row, 2] = sbbm;
                                }
                            }
                            string wlbm = "", sbmc = "", xh = "", jscs = "", dw = "", bz = "", dn = string.Empty;
                            double dz = 0, zz = 0;
                            int sl = 0;
                            if (iec1.GetPropertyValue("CERI_Equipment_Code") != null)
                            {
                                wlbm = iec1["CERI_Equipment_Code"].StringValue;
                            }
                            if (iec1.GetPropertyValue("CERI_Equipment_Name") != null)
                            {
                                sbmc = iec1["CERI_Equipment_Name"].StringValue;
                            }
                            if (iec1.GetPropertyValue("CERI_Model") != null)
                            {
                                xh = iec1["CERI_Model"].StringValue;
                            }
                            if (iec1.GetPropertyValue("NOMINAL_DIAMETER") != null)
                            {
                                dn = iec1["NOMINAL_DIAMETER"].DoubleValue.ToString();
                                if (dn == "0")
                                {
                                    dn = string.Empty;
                                }
                                dn = "DN" + dn;
                            }
                            if (iec1.GetPropertyValue("CERI_Technical") != null)
                            {
                                jscs = iec1["CERI_Technical"].StringValue;
                            }
                            dw = "个";
                            if (iec1.GetPropertyValue("CERI_Remark") != null)
                            {
                                bz = iec1["CERI_Remark"].StringValue;
                            }
                            if (iec1.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                            {
                                dz = iec1["CERI_DRY_WEIGHT"].DoubleValue;
                            }
                            if (iec1.GetPropertyValue("CERI_Quantity") != null)
                            {
                                sl = iec1["CERI_Quantity"].IntValue;
                            }
                            //foreach (IECInstance iec1 in kvs.Value)
                            //{
                            //    if (iec1.GetPropertyValue("CERI_Quantity") != null)
                            //    {
                            //        sl += iec1["CERI_Quantity"].IntValue;
                            //    }
                            //}
                            if (dz < 0.005)
                                dz = 0;
                            zz = dz * sl;
                            if (isgd)
                            {
                                s.Cells[row, 4] = wlbm;
                                s.Cells[row, 5] = sbmc;
                                s.Cells[row, 7] = xh;
                                s.Cells[row, 9] = dn;
                                s.Cells[row, 10] = jscs;
                                s.Cells[row, 13] = dw;
                                s.Cells[row, 14] = sl;
                                s.Cells[row, 15] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 16] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 17] = bz;
                            }
                            else
                            {
                                s.Cells[row, 3] = wlbm;
                                s.Cells[row, 4] = sbmc;
                                s.Cells[row, 6] = xh;
                                s.Cells[row, 7] = dn;
                                s.Cells[row, 8] = jscs;
                                s.Cells[row, 11] = dw;
                                s.Cells[row, 12] = sl;
                                s.Cells[row, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 15] = bz;
                            }
                            row++;
                        }

                    }
                }
                #endregion
                #region 管道设备表和汇总表
                if (isgd || ishz)
                {
                    if (piDicList.Count > 0)
                    {
                        row--;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in piDicList)
                        {
                            //row++;
                            //index++;
                            //s.Cells[row, 1] = index;
                            //string pipeLineName = "";
                            //pipeLineName = kv.Value[0][0]["NAME"].StringValue;
                            ////string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLineName);
                            //s.Cells[row, 2] = pipeLineName;
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                row++;
                                index++;
                                s.Cells[row, 1] = index;
                                string pipeLineName = "";
                                pipeLineName = kvs.Value[0]["LINENUMBER"].StringValue;
                                string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLineName);                                                                
                                string eqName = kvs.Value[0]["NAME"].StringValue;
                                
                                if (ishz)
                                {
                                    s.Cells[row, 2] = eqName;
                                }
                                else
                                {
                                    s.Cells[row, 2] = pipeNumber;
                                    s.Cells[row, 3] = eqName;
                                }

                                string wlbm = "", sbmc = "", xh = "", gg = "",/* jscs = "",*/ dw = "", bz = "";
                                double dz = 0, nums = 0, zz = 0;
                                if (kvs.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = kvs.Value[0]["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = kvs.Value[0]["GRADE"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    sbmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        sbmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                                {
                                    xh = kvs.Value[0]["CERI_PIECE_MARK"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("PIECE_MARK") != null)
                                    {
                                        xh = kvs.Value[0]["PIECE_MARK"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                {
                                    gg = kvs.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                {
                                    bz = kvs.Value[0]["CERI_NOTE"].StringValue;
                                }
                                else
                                {                                  
                                    if (kvs.Value[0].GetPropertyValue("NOTES") != null)
                                    {
                                        bz = kvs.Value[0]["NOTES"].StringValue;
                                    }
                                }
                                bool b = BMECApi.Instance.InstanceDefinedAsClass(kvs.Value[0], "PIPE", true);
                                double dry_weight = 0;
                                if (b)
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "米";
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "个";
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                }
                                if (ishz)
                                {
                                    s.Cells[row, 3] = wlbm;
                                    s.Cells[row, 4] = sbmc;
                                    s.Cells[row, 6] = xh;
                                    s.Cells[row, 7] = gg;
                                    //s.Cells[row, 8] = jscs;
                                    s.Cells[row, 11] = dw;
                                    s.Cells[row, 12] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 15] = bz;
                                }
                                else
                                {
                                    s.Cells[row, 4] = wlbm;
                                    s.Cells[row, 5] = sbmc;
                                    s.Cells[row, 7] = xh;
                                    s.Cells[row, 9] = gg;
                                    s.Cells[row, 13] = dw;
                                    s.Cells[row, 14] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 15] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 16] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 17] = bz;
                                }
                                //row++;
                            }
                        }
                    }
                }
                #endregion
                else  //通用设备表  输出管道(不需要按管线编号分组)
                {
                    if (piDicList.Count > 0)
                    {
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in piDicList)
                        {
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                index++;
                                s.Cells[row, 1] = index;
                                string wlbm = "", sbmc = "", xh = "", gg = "",/* jscs = "",*/ dw = "", bz = "", pName = "";
                                double dz = 0, nums = 0, zz = 0;
                                pName = kvs.Value[0]["NAME"].StringValue;
                                s.Cells[row, 2] = pName;
                                if (kvs.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = kvs.Value[0]["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = kvs.Value[0]["GRADE"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    sbmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        sbmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                                {
                                    xh = kvs.Value[0]["CERI_PIECE_MARK"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("PIECE_MARK") != null)
                                    {
                                        xh = kvs.Value[0]["PIECE_MARK"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                {
                                    gg = kvs.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                {
                                    bz = kvs.Value[0]["CERI_NOTE"].StringValue;
                                }
                                else
                                {                                    
                                    if (kvs.Value[0].GetPropertyValue("NOTES") != null)
                                    {
                                        bz = kvs.Value[0]["NOTES"].StringValue;
                                    }
                                }
                                bool b = BMECApi.Instance.InstanceDefinedAsClass(kvs.Value[0], "PIPE", true);
                                double dry_weight = 0;
                                if (b)
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "米";
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "个";
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                }
                                s.Cells[row, 3] = wlbm;
                                s.Cells[row, 4] = sbmc;
                                s.Cells[row, 6] = xh;
                                s.Cells[row, 7] = gg;
                                s.Cells[row, 11] = dw;
                                s.Cells[row, 12] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 15] = bz;
                                row++;
                            }
                        }
                    }
                }
                eqDicList.Clear();
                piDicList.Clear();
                wb.Save();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app);

                openPath = excelPath;
                //wb = null;
                //wkb = null;
                //app = null;
                //GC.Collect();
                //MOIE.Application app1 = new MOIE.Application();
                //MOIE.Workbooks wbs = app1.Workbooks;
                //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                //app1.Visible = true;
                waitDialog.Close();
            }
        }

        public void hzfuzhi(ECInstanceList iecList)
        {
            Dictionary<int, List<IECInstance>> eqList = new Dictionary<int, List<IECInstance>>();
            Dictionary<int, Dictionary<int, List<IECInstance>>> pipeList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (iecList.Count > 0)
            {
                int keyNumber = 0, keyNumber1 = 0;
                foreach (IECInstance iec in iecList)
                {
                    bool sheb = BMECApi.Instance.InstanceDefinedAsClass(iec, "EQUIPMENT", true);
                    if (sheb)
                    {
                        string sbNum = "";
                        if (iec.GetPropertyValue("CERI_Equipment_Num") != null)
                        {
                            sbNum = iec["CERI_Equipment_Num"].StringValue;
                        }
                        string wlbm = "";
                        if (iec.GetPropertyValue("CERI_Equipment_Code") != null)
                        {
                            wlbm = iec["CERI_Equipment_Code"].StringValue;
                        }
                        bool isK = false;
                        if (wlbm == "" || wlbm == " ")
                        {
                            isK = true;
                        }
                        bool isSb = false;

                        foreach (KeyValuePair<int, List<IECInstance>> kv in eqList)
                        {
                            if (isK)
                            {
                                bool noWlbm = pdXt(iec, kv.Value[0], true);
                                if (noWlbm)
                                {
                                    isSb = true;
                                    eqList[kv.Key].Add(iec);
                                }
                            }
                            else
                            {
                                string wlbm1 = "";
                                if (kv.Value[0].GetPropertyValue("CERI_Equipment_Code") != null)
                                {
                                    wlbm1 = kv.Value[0]["CERI_Equipment_Code"].StringValue;
                                }
                                if (wlbm.Equals(wlbm1))
                                {
                                    isSb = true;
                                    eqList[kv.Key].Add(iec);
                                }
                            }
                            #region
                            //string sbNum1 = "";
                            //if (kv.Value[0][0].GetPropertyValue("CERI_Equipment_Num") != null)
                            //{

                            //    sbNum1 = kv.Value[0][0]["CERI_Equipment_Num"].StringValue;
                            //    if (sbNum.Equals(sbNum1))
                            //    {
                            //        isSb = true;
                            //        bool isRz = false;
                            //        foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            //        {
                            //            bool isrz1 = false;
                            //            if (isK)
                            //            {
                            //                isrz1 = pdXt(iec, kvs.Value[0], true);
                            //            }
                            //            else
                            //            {
                            //                string wlbm1 = "";
                            //                if (kvs.Value[0].GetPropertyValue("CERI_Equipment_Code") != null)
                            //                {
                            //                    wlbm1 = kvs.Value[0]["CERI_Equipment_Code"].StringValue;
                            //                }
                            //                if (wlbm.Equals(wlbm1))
                            //                {
                            //                    isrz1 = true;
                            //                }
                            //            }
                            //            if (isrz1)
                            //            {
                            //                isRz = true;
                            //                eqList[kv.Key][kvs.Key].Add(iec);
                            //            }
                            //        }
                            //        if (!isRz)
                            //        {
                            //            List<IECInstance> rzList = new List<IECInstance>();
                            //            rzList.Add(iec);
                            //            eqList[kv.Key].Add(eqList[kv.Key].Count, rzList);
                            //        }
                            //    }
                            //}
                            #endregion
                        }
                        if (!isSb)
                        {
                            List<IECInstance> zecList = new List<IECInstance>();
                            zecList.Add(iec);
                            //Dictionary<int, List<IECInstance>> zdicList = new Dictionary<int, List<IECInstance>>();
                            //zdicList.Add(0, zecList);
                            eqList.Add(keyNumber, zecList);
                            keyNumber++;
                        }
                        //eqDicList = eqList;
                    }
                    //bool isPsb = false;
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPING_COMPONENT", true); //查找ec的父类是否含有PIPING_COMPONENT
                    if (b)
                    {
                        string fenlei = iec["CERI_Classify"].StringValue;
                        //if (fenlei == "shebei")
                        if (fenlei == "shebei")
                        {
                            string pipeNum = "";
                            pipeNum = iec["NAME"].StringValue;
                            string wlbm = "";
                            if (iec.GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm = iec["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (iec.GetPropertyValue("GRADE") != null)
                                {
                                    wlbm = iec["GRADE"].StringValue;
                                }
                            }
                            bool isK = false;
                            if (wlbm == "" || wlbm == " ")
                            {
                                isK = true;
                            }
                            bool isP = false;
                            //int keyNumber = 0;
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in pipeList)
                            {
                                string pipenumber1 = kv.Value[0][0]["NAME"].StringValue;
                                if (pipeNum.Equals(pipenumber1))
                                {
                                    isP = true;
                                    bool ispz = false;
                                    foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                                    {
                                        bool iscz = false;
                                        if (isK)
                                        {
                                            iscz = pdXt(iec, kvs.Value[0], false);
                                        }
                                        else
                                        {
                                            string wlbm1 = "";
                                            if (kvs.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                            {
                                                wlbm1 = kvs.Value[0]["CERI_MAT_GRADE"].StringValue;
                                            }
                                            else
                                            {
                                                if (kvs.Value[0].GetPropertyValue("GRADE") != null)
                                                {
                                                    wlbm1 = kvs.Value[0]["GRADE"].StringValue;
                                                }
                                            }
                                            if (wlbm.Equals(wlbm1))
                                            {
                                                iscz = true;
                                            }
                                        }
                                        if (iscz)
                                        {
                                            ispz = true;
                                            pipeList[kv.Key][kvs.Key].Add(iec);
                                        }
                                    }
                                    if (!ispz)
                                    {
                                        List<IECInstance> iecpList = new List<IECInstance>();
                                        iecpList.Add(iec);
                                        pipeList[kv.Key].Add(pipeList[kv.Key].Count, iecpList);
                                    }
                                }
                            }
                            if (!isP)
                            {
                                List<IECInstance> iecPlist = new List<IECInstance>();
                                iecPlist.Add(iec);
                                Dictionary<int, List<IECInstance>> dicPlist = new Dictionary<int, List<IECInstance>>();
                                dicPlist.Add(0, iecPlist);
                                pipeList.Add(keyNumber1, dicPlist);
                                keyNumber1++;
                            }
                        }
                    }
                }
                eqList = tjpaixu(eqList);
                eqDicList = eqList;
                //eqDicList = tjpaixu(eqDicList);
                piDicList = pipeList;
            }
        }

        public void hzExport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.InitialDirectory = path0; //设置初始目录
            sfd.Filter = "Excel Files(*.xlsx)|*.xlsx";
            sfd.RestoreDirectory = true;

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string excelPath = sfd.FileName.ToString();
                string excelName = excelPath.Substring(excelPath.LastIndexOf("\\") + 1);
                int i = excelName.LastIndexOf(".");
                string houzhui = excelName.Substring(i + 1);
                if (houzhui != "xlsx")
                {
                    MessageBox.Show("输入文件名格式不正确");
                    return;
                }
                if (File.Exists(excelPath)) //判断是否有同名的文件
                {
                    string excelname = excelName.Substring(0, excelName.LastIndexOf("."));
                    string datetime = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                    //string.Format("{0:yyyyMMddHHmmssffff}",dt);
                    //string datetime = string.Format("{ 0:yyyyMMddHHmmssffff}", dt.ToString());
                    //excelname += "1.xlsx";
                    excelname = excelname + datetime + ".xlsx";
                    //string name = excelname.Substring(excelPath1.LastIndexOf("\\") + 1);
                    MessageBox.Show("该路径下存在同名的文件。修改后的文件名为：" + excelname);
                    string path = excelPath.Substring(0, excelPath.LastIndexOf("\\") + 1);
                    excelPath = path + excelname;
                }
                string yName = lxcomboBox1.Text + ".xlsx";
                try
                {
                    File.Copy(path0 + "\\JYXConfig\\汇总设备表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\汇总设备表：下没有找到" + yName + "文件");
                    return;
                }
                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();

                app = new MOIE.Application();
                MOIE.Workbooks wkb = app.Workbooks;
                MOIE.Workbook wb = wkb.Open(excelPath);
                //app.Visible = true;
                //wb=wkb.Open(excelPath1);
                MOIE.Worksheet s;
                s = (MOIE.Worksheet)wb.Worksheets["压力管道设备汇总表-国内"];

                bool isChenked = false;
                if (wlbmGroupcheckBox1.Checked)
                {
                    isChenked = true;
                }
                //bool isgd = false;
                //if (lxcomboBox1.SelectedIndex == 2 || lxcomboBox1.SelectedIndex == 3 || lxcomboBox1.SelectedIndex == 4)
                //{
                //    isgd = true;
                //}
                int row = 7, index = 0;
                if (eqDicList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in eqDicList)
                    {
                        string wlbm = "";
                        if (kv.Value[0].GetPropertyValue("CERI_Equipment_Code") != null)
                        {
                            wlbm = kv.Value[0]["CERI_Equipment_Code"].StringValue;
                        }
                        bool isWl = false;
                        if (wlbm == "" || wlbm == " ")
                        {
                            isWl = true;
                        }
                        if (!isWl && isChenked)
                        {
                            #region 合并单元格
                            int irow = 0;
                            irow = row;
                            MOIE.Range r = s.Range[s.Cells[irow, 3], s.Cells[row + kv.Value.Count - 1, 3]];
                            r.Application.DisplayAlerts = false;
                            r.Merge(Type.Missing);
                            r.Application.DisplayAlerts = true;
                            MOIE.Range r1 = s.Range[s.Cells[irow, 14], s.Cells[row + kv.Value.Count - 1, 14]];
                            r1.Application.DisplayAlerts = false;
                            r1.Merge(Type.Missing);
                            r1.Application.DisplayAlerts = true;
                            #endregion
                            double zzW = 0;
                            foreach (IECInstance iecz in kv.Value)
                            {
                                string sbmc1 = "", xh1 = "", jscs1 = "", dw1 = "", bz1 = "", dn1 = string.Empty;
                                double dz1 = 0, zz1 = 0;
                                int sl1 = 0;
                                //row++;
                                if (iecz.GetPropertyValue("CERI_Equipment_Name") != null)
                                {
                                    sbmc1 = iecz["CERI_Equipment_Name"].StringValue;
                                }
                                if (iecz.GetPropertyValue("CERI_Model") != null)
                                {
                                    xh1 = iecz["CERI_Model"].StringValue;
                                }
                                if (iecz.GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    dn1 = iecz["NOMINAL_DIAMETER"].DoubleValue.ToString();
                                    dn1 = "DN" + dn1;
                                }
                                if (iecz.GetPropertyValue("CERI_Technical") != null)
                                {
                                    jscs1 = iecz["CERI_Technical"].StringValue;
                                }
                                dw1 = "个";
                                if (iecz.GetPropertyValue("CERI_Remark") != null)
                                {
                                    bz1 = iecz["CERI_Remark"].StringValue;
                                }
                                if (iecz.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                                {
                                    dz1 = iecz["CERI_DRY_WEIGHT"].DoubleValue;
                                }
                                if (iecz.GetPropertyValue("CERI_Quantity") != null)
                                {
                                    sl1 = iecz["CERI_Quantity"].IntValue;
                                }
                                if (dz1 < 0.005)
                                    dz1 = 0;
                                zz1 = dz1 * sl1;
                                zzW += zz1;
                                index++;
                                s.Cells[row, 1] = index;
                                string sbbm = "";
                                if (iecz.GetPropertyValue("CERI_Equipment_Num") != null)
                                {
                                    sbbm = iecz["CERI_Equipment_Num"].StringValue;
                                }
                                s.Cells[row, 2] = sbbm;
                                //s.Cells[row, 4] = wlbm1;
                                s.Cells[row, 4] = sbmc1;
                                s.Cells[row, 6] = xh1;
                                s.Cells[row, 7] = dn1;
                                s.Cells[row, 8] = jscs1;
                                s.Cells[row, 11] = dw1;
                                s.Cells[row, 12] = sl1;
                                s.Cells[row, 13] = Math.Round((decimal)dz1, 2, MidpointRounding.AwayFromZero);
                                //s.Cells[row, 16] = zz;
                                s.Cells[row, 15] = bz1;
                                row++;
                            }
                            s.Cells[irow, 3] = wlbm;
                            s.Cells[irow, 14] = Math.Round((decimal)zzW, 2, MidpointRounding.AwayFromZero);
                        }
                        else
                        {
                            #region 合并单元格
                            int irow = 0;
                            irow = row;
                            MOIE.Range r = s.Range[s.Cells[irow, 3], s.Cells[row + kv.Value.Count - 1, 3]];
                            r.Application.DisplayAlerts = false;
                            r.Merge(Type.Missing);
                            r.Application.DisplayAlerts = true;
                            MOIE.Range r1 = s.Range[s.Cells[irow, 14], s.Cells[row + kv.Value.Count - 1, 14]];
                            r1.Application.DisplayAlerts = false;
                            r1.Merge(Type.Missing);
                            r1.Application.DisplayAlerts = true;
                            MOIE.Range r2 = s.Range[s.Cells[irow, 4], s.Cells[row + kv.Value.Count - 1, 4]];
                            r2.Application.DisplayAlerts = false;
                            r2.Merge(Type.Missing);
                            r2.Application.DisplayAlerts = true;
                            MOIE.Range r3 = s.Range[s.Cells[irow, 6], s.Cells[row + kv.Value.Count - 1, 6]];
                            r3.Application.DisplayAlerts = false;
                            r3.Merge(Type.Missing);
                            r3.Application.DisplayAlerts = true;
                            MOIE.Range r4 = s.Range[s.Cells[irow, 7], s.Cells[row + kv.Value.Count - 1, 7]];
                            r4.Application.DisplayAlerts = false;
                            r4.Merge(Type.Missing);
                            r4.Application.DisplayAlerts = true;
                            MOIE.Range r5 = s.Range[s.Cells[irow, 8], s.Cells[row + kv.Value.Count - 1, 8]];
                            r5.Application.DisplayAlerts = false;
                            r5.Merge(Type.Missing);
                            r5.Application.DisplayAlerts = true;
                            MOIE.Range r6 = s.Range[s.Cells[irow, 11], s.Cells[row + kv.Value.Count - 1, 11]];
                            r6.Application.DisplayAlerts = false;
                            r6.Merge(Type.Missing);
                            r6.Application.DisplayAlerts = true;
                            MOIE.Range r7 = s.Range[s.Cells[irow, 12], s.Cells[row + kv.Value.Count - 1, 12]];
                            r7.Application.DisplayAlerts = false;
                            r7.Merge(Type.Missing);
                            r7.Application.DisplayAlerts = true;
                            MOIE.Range r8 = s.Range[s.Cells[irow, 13], s.Cells[row + kv.Value.Count - 1, 13]];
                            r8.Application.DisplayAlerts = false;
                            r8.Merge(Type.Missing);
                            r8.Application.DisplayAlerts = true;
                            MOIE.Range r9 = s.Range[s.Cells[irow, 15], s.Cells[row + kv.Value.Count - 1, 15]];
                            r9.Application.DisplayAlerts = false;
                            r9.Merge(Type.Missing);
                            r9.Application.DisplayAlerts = true;
                            #endregion
                            string sbmc = "", xh = "", jscs = "", dw = "", bz = "", dn = string.Empty;
                            double dz = 0, zz = 0;
                            int sl = 0;
                            if (kv.Value[0].GetPropertyValue("CERI_Equipment_Name") != null)
                            {
                                sbmc = kv.Value[0]["CERI_Equipment_Name"].StringValue;
                            }
                            if (kv.Value[0].GetPropertyValue("CERI_Model") != null)
                            {
                                xh = kv.Value[0]["CERI_Model"].StringValue;
                            }
                            if (kv.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                            {
                                dn = kv.Value[0]["NOMINAL_DIAMETER"].DoubleValue.ToString();
                                dn = "DN" + dn;
                            }
                            if (kv.Value[0].GetPropertyValue("CERI_Technical") != null)
                            {
                                jscs = kv.Value[0]["CERI_Technical"].StringValue;
                            }
                            dw = "个";
                            if (kv.Value[0].GetPropertyValue("CERI_Remark") != null)
                            {
                                bz = kv.Value[0]["CERI_Remark"].StringValue;
                            }
                            if (kv.Value[0].GetPropertyValue("CERI_DRY_WEIGHT") != null)
                            {
                                dz = kv.Value[0]["CERI_DRY_WEIGHT"].DoubleValue;
                            }
                            foreach (IECInstance iec1 in kv.Value)
                            {
                                if (iec1.GetPropertyValue("CERI_Quantity") != null)
                                {
                                    sl += iec1["CERI_Quantity"].IntValue;
                                }
                                else
                                {
                                    sl++;
                                }
                            }
                            if (dz < 0.005)
                                dz = 0;
                            zz = dz * sl;
                            //row++;
                            foreach (IECInstance iecsb in kv.Value)
                            {
                                index++;
                                s.Cells[row, 1] = index;
                                string sbbm2 = "";
                                if (iecsb.GetPropertyValue("CERI_Equipment_Num") != null)
                                {
                                    sbbm2 = iecsb["CERI_Equipment_Num"].StringValue;
                                }
                                s.Cells[row, 2] = sbbm2;
                                row++;
                            }

                            //s.Cells[irow, 2] = sbbm2;
                            s.Cells[irow, 3] = wlbm;
                            s.Cells[irow, 4] = sbmc;
                            s.Cells[irow, 6] = xh;
                            s.Cells[irow, 7] = dn;
                            s.Cells[irow, 8] = jscs;
                            s.Cells[irow, 11] = dw;
                            s.Cells[irow, 12] = sl;
                            s.Cells[irow, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                            s.Cells[irow, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                            s.Cells[irow, 15] = bz;
                            //row++;
                        }
                        #region
                        ////row++;
                        //index++;
                        //s.Cells[row, 1] = index;
                        //string sbbm = "";
                        //if (kv.Value[0][0].GetPropertyValue("CERI_Equipment_Num") != null)
                        //{
                        //    sbbm = kv.Value[0][0]["CERI_Equipment_Num"].StringValue;
                        //}
                        //s.Cells[row, 2] = sbbm;
                        ////double wZz = 0;                        
                        //foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                        //{
                        //    //row++;
                        //    int irow = 0;
                        //    string wlbm = "", sbmc = "", xh = "", jscs = "", dw = "", bz = "";
                        //    double dz = 0, zz = 0, dn = 0;
                        //    int sl = 0;
                        //    if (kvs.Value[0].GetPropertyValue("CERI_Equipment_Code") != null)
                        //    {
                        //        wlbm = kvs.Value[0]["CERI_Equipment_Code"].StringValue;
                        //    }
                        //    bool isWl = false;
                        //    if (wlbm == "" || wlbm == " ")
                        //    {
                        //        isWl = true;
                        //    }
                        //    if (!isWl)
                        //    {
                        //        #region 合并单元格
                        //        irow = row + 1;
                        //        MOIE.Range r = s.Range[s.Cells[irow, 3], s.Cells[row + kvs.Value.Count, 3]];
                        //        r.Application.DisplayAlerts = false;
                        //        r.Merge(Type.Missing);
                        //        r.Application.DisplayAlerts = true;
                        //        MOIE.Range r1 = s.Range[s.Cells[irow, 14], s.Cells[row + kvs.Value.Count, 14]];
                        //        r1.Application.DisplayAlerts = false;
                        //        r1.Merge(Type.Missing);
                        //        r1.Application.DisplayAlerts = true;
                        //        #endregion
                        //        double zzW = 0;
                        //        foreach (IECInstance iecz in kvs.Value)
                        //        {
                        //            string sbmc1 = "", xh1 = "", jscs1 = "", dw1 = "", bz1 = "";
                        //            double dz1 = 0, zz1 = 0, dn1 = 0;
                        //            int sl1 = 0;
                        //            //row++;
                        //            if (iecz.GetPropertyValue("CERI_Equipment_Name") != null)
                        //            {
                        //                sbmc1 = iecz["CERI_Equipment_Name"].StringValue;
                        //            }
                        //            if (iecz.GetPropertyValue("CERI_Model") != null)
                        //            {
                        //                xh1 = iecz["CERI_Model"].StringValue;
                        //            }
                        //            if (iecz.GetPropertyValue("NOMINAL_DIAMETER") != null)
                        //            {
                        //                dn1 = iecz["NOMINAL_DIAMETER"].DoubleValue;
                        //            }
                        //            if (iecz.GetPropertyValue("CERI_Technical") != null)
                        //            {
                        //                jscs1 = iecz["CERI_Technical"].StringValue;
                        //            }
                        //            dw1 = "个";
                        //            if (iecz.GetPropertyValue("CERI_Remark") != null)
                        //            {
                        //                bz1 = iecz["CERI_Remark"].StringValue;
                        //            }
                        //            if (iecz.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                        //            {
                        //                dz1 = iecz["CERI_DRY_WEIGHT"].DoubleValue;
                        //            }
                        //            if (iecz.GetPropertyValue("CERI_Quantity") != null)
                        //            {
                        //                sl1 = iecz["CERI_Quantity"].IntValue;
                        //            }
                        //            zz1 = dz1 * sl1;
                        //            zzW += zz1;
                        //            //s.Cells[row, 4] = wlbm1;
                        //            s.Cells[row, 4] = sbmc1;
                        //            s.Cells[row, 6] = xh1;
                        //            s.Cells[row, 7] = dn1;
                        //            s.Cells[row, 8] = jscs1;
                        //            s.Cells[row, 11] = dw1;
                        //            s.Cells[row, 12] = sl1;
                        //            s.Cells[row, 13] = Math.Round((decimal)dz1, 2, MidpointRounding.AwayFromZero);
                        //            //s.Cells[row, 16] = zz;
                        //            s.Cells[row, 15] = bz1;
                        //        }
                        //        s.Cells[irow, 3] = wlbm;
                        //        s.Cells[irow, 14] = Math.Round((decimal)zzW, 2, MidpointRounding.AwayFromZero);
                        //    }
                        //    else
                        //    {
                        //        if (kvs.Value[0].GetPropertyValue("CERI_Equipment_Name") != null)
                        //        {
                        //            sbmc = kvs.Value[0]["CERI_Equipment_Name"].StringValue;
                        //        }
                        //        if (kvs.Value[0].GetPropertyValue("CERI_Model") != null)
                        //        {
                        //            xh = kvs.Value[0]["CERI_Model"].StringValue;
                        //        }
                        //        if (kvs.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                        //        {
                        //            dn = kvs.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                        //        }
                        //        if (kvs.Value[0].GetPropertyValue("CERI_Technical") != null)
                        //        {
                        //            jscs = kvs.Value[0]["CERI_Technical"].StringValue;
                        //        }
                        //        dw = "个";
                        //        if (kvs.Value[0].GetPropertyValue("CERI_Remark") != null)
                        //        {
                        //            bz = kvs.Value[0]["CERI_Remark"].StringValue;
                        //        }
                        //        if (kvs.Value[0].GetPropertyValue("CERI_DRY_WEIGHT") != null)
                        //        {
                        //            dz = kvs.Value[0]["CERI_DRY_WEIGHT"].DoubleValue;
                        //        }
                        //        foreach (IECInstance iec1 in kvs.Value)
                        //        {
                        //            if (iec1.GetPropertyValue("CERI_Quantity") != null)
                        //            {
                        //                sl += iec1["CERI_Quantity"].IntValue;
                        //            }
                        //        }
                        //        zz = dz * sl;
                        //        row++;
                        //        s.Cells[row, 3] = wlbm;
                        //        s.Cells[row, 4] = sbmc;
                        //        s.Cells[row, 6] = xh;
                        //        s.Cells[row, 7] = dn;
                        //        s.Cells[row, 8] = jscs;
                        //        s.Cells[row, 11] = dw;
                        //        s.Cells[row, 12] = sl;
                        //        s.Cells[row, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                        //        s.Cells[row, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                        //        s.Cells[row, 15] = bz;
                        //    }
                        //    row++;
                        #endregion
                    }
                }
                if (piDicList.Count > 0)
                {
                    row--;
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in piDicList)
                    {
                        row++;
                        index++;
                        s.Cells[row, 1] = index;
                        string pipeLineName = "";
                        pipeLineName = kv.Value[0][0]["NAME"].StringValue;
                        //string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLineName);
                        s.Cells[row, 2] = pipeLineName;
                        foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                        {
                            //row++;
                            string wlbm = "", sbmc = "", xh = "", gg = "", dw = "", bz = "";
                            double dz = 0, nums = 0, zz = 0;
                            if (kvs.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm = kvs.Value[0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kvs.Value[0].GetPropertyValue("GRADE") != null)
                                {
                                    wlbm = kvs.Value[0]["GRADE"].StringValue;
                                }
                            }
                            bool isWl = false;
                            if (wlbm == "" || wlbm == " ")
                            {
                                isWl = true;
                            }
                            if (!isWl)
                            {
                                #region 合并单元格
                                int irow = row + 1;
                                MOIE.Range r = s.Range[s.Cells[irow, 3], s.Cells[row + kvs.Value.Count, 3]];
                                r.Application.DisplayAlerts = false;
                                r.Merge(Type.Missing);
                                r.Application.DisplayAlerts = true;
                                MOIE.Range r1 = s.Range[s.Cells[irow, 14], s.Cells[row + kvs.Value.Count, 14]];
                                r1.Application.DisplayAlerts = false;
                                r1.Merge(Type.Missing);
                                r1.Application.DisplayAlerts = true;
                                #endregion
                                double zzW = 0;
                                foreach (IECInstance iecz in kvs.Value)
                                {
                                    string sbmc1 = "", xh1 = "", gg1 = "", dw1 = "", bz1 = "";
                                    double dz1 = 0, zz1 = 0, nums1 = 0;
                                    //int sl1 = 0;

                                    if (iecz.GetPropertyValue("CERI_SHORT_DESC") != null)
                                    {
                                        sbmc1 = iecz["CERI_SHORT_DESC"].StringValue;
                                    }
                                    else
                                    {
                                        if (iecz.GetPropertyValue("SHORT_DESCRIPTION") != null)
                                        {
                                            sbmc1 = iecz["SHORT_DESCRIPTION"].StringValue;
                                        }
                                    }
                                    if (iecz.GetPropertyValue("CERI_PIECE_MARK") != null)
                                    {
                                        xh1 = iecz["CERI_PIECE_MARK"].StringValue;
                                    }
                                    else
                                    {
                                        if (iecz.GetPropertyValue("PIECE_MARK") != null)
                                        {
                                            xh1 = iecz["PIECE_MARK"].StringValue;
                                        }
                                    }
                                    if (iecz.GetPropertyValue("CERI_MAIN_SIZE") != null)
                                    {
                                        gg1 = iecz["CERI_MAIN_SIZE"].StringValue;
                                    }

                                    dw1 = "个";
                                    if (iecz.GetPropertyValue("CERI_NOTE") != null)
                                    {
                                        bz1 = iecz["CERI_NOTE"].StringValue;
                                    }
                                    else
                                    {                                        
                                        if (iecz.GetPropertyValue("NOTES") != null)
                                        {
                                            bz1 = iecz["NOTES"].StringValue;
                                        }
                                    }
                                    //if (iecz.GetPropertyValue("CERI_DRY_WEIGHT") != null)
                                    //{
                                    //    dz1 = iecz["CERI_DRY_WEIGHT"].DoubleValue;
                                    //}
                                    //if (iecz.GetPropertyValue("CERI_Quantity") != null)
                                    //{
                                    //    sl1 = iecz["CERI_Quantity"].IntValue;
                                    //}
                                    bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iecz, "PIPE", true);
                                    double dry_weight1 = 0;
                                    if (b1)
                                    {
                                        if (iecz.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight1 = iecz["CERI_WEIGHT_DRY"].DoubleValue;
                                            if (dry_weight1 < 0.005)
                                                dry_weight1 = 0;
                                            dz1 = dry_weight1;
                                        }
                                        else
                                        {
                                            if (iecz.GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight1 = iecz["DRY_WEIGHT"].DoubleValue;
                                                if (dry_weight1 < 0.005)
                                                    dry_weight1 = 0;
                                                dz1 = dry_weight1;
                                            }
                                        }
                                        dw1 = "米";
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums1 = iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                        zz1 = dz1 * nums1;
                                    }
                                    else
                                    {
                                        if (iecz.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight1 = iecz["CERI_WEIGHT_DRY"].DoubleValue;
                                            if (dry_weight1 < 0.005)
                                                dry_weight1 = 0;
                                            dz1 = dry_weight1;
                                        }
                                        else
                                        {
                                            if (iecz.GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight1 = iecz["DRY_WEIGHT"].DoubleValue;
                                                if (dry_weight1 < 0.005)
                                                    dry_weight1 = 0;
                                                dz1 = dry_weight1;
                                            }
                                        }
                                        dw1 = "个";
                                        nums1 = 1;
                                        zz1 = dz1 * nums1;
                                    }
                                    //zz1 = dz1 * sl1;
                                    zzW += zz1;
                                    row++;
                                    //s.Cells[row, 4] = wlbm1;
                                    s.Cells[row, 4] = sbmc1;
                                    s.Cells[row, 6] = xh1;
                                    s.Cells[row, 7] = gg1;
                                    //s.Cells[row, 8] = jscs1;
                                    s.Cells[row, 11] = dw1;
                                    s.Cells[row, 12] = Math.Round((decimal)nums1, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 13] = Math.Round((decimal)dz1, 2, MidpointRounding.AwayFromZero);
                                    //s.Cells[row, 16] = zz;
                                    s.Cells[row, 15] = bz1;
                                }
                                s.Cells[irow, 3] = wlbm;
                                s.Cells[irow, 14] = Math.Round((decimal)zzW, 2, MidpointRounding.AwayFromZero);
                                //row++;
                            }
                            else
                            {

                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    sbmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        sbmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                                {
                                    xh = kvs.Value[0]["CERI_PIECE_MARK"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("PIECE_MARK") != null)
                                    {
                                        xh = kvs.Value[0]["PIECE_MARK"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                {
                                    gg = kvs.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                {
                                    bz = kvs.Value[0]["CERI_NOTE"].StringValue;
                                }
                                else
                                {                                    
                                    if (kvs.Value[0].GetPropertyValue("NOTES") != null)
                                    {
                                        bz = kvs.Value[0]["NOTES"].StringValue;
                                    }
                                }
                                bool b = BMECApi.Instance.InstanceDefinedAsClass(kvs.Value[0], "PIPE", true);
                                double dry_weight = 0;
                                if (b)
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "米";
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kvs.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kvs.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kvs.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "个";
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                }
                                row++;
                                s.Cells[row, 3] = wlbm;
                                s.Cells[row, 4] = sbmc;
                                s.Cells[row, 6] = xh;
                                s.Cells[row, 7] = gg;
                                s.Cells[row, 11] = dw;
                                s.Cells[row, 12] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 13] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 14] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 15] = bz;
                                //row++;
                            }

                        }
                    }
                }
                eqDicList.Clear();
                piDicList.Clear();
                wb.Save();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app);

                openPath = excelPath;
                //wb = null;
                //wkb = null;
                //app = null;
                //GC.Collect();
                //MOIE.Application app1 = new MOIE.Application();
                //MOIE.Workbooks wbs = app1.Workbooks;
                //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                //app1.Visible = true;
                waitDialog.Close();
            }
        }

        /// <summary>
        /// 如果设备编码不足5位则用“ ”补齐
        /// </summary>
        /// <param name="sbbm"></param>
        /// <returns></returns>
        public string buqi(string sbbm)
        {
            string str = "";
            if (sbbm.Length > 4)
            {
                str = sbbm;
            }
            else
            {
                int j = 5 - sbbm.Length;
                for (int i = 0; i < j; i++)
                {
                    sbbm += " ";
                }
                str = sbbm;
            }
            return str;
        }

        /// <summary>
        /// 不按物料编码分组，按照前5位相同进行排序
        /// </summary>
        /// <param name="eqList"></param>
        /// <returns></returns>
        public Dictionary<int, List<IECInstance>> paixu(Dictionary<int, List<IECInstance>> eqList)
        {
            Dictionary<int, List<IECInstance>> pxList = new Dictionary<int, List<IECInstance>>();

            int keyNumber = 0;
            foreach (KeyValuePair<int, List<IECInstance>> kv in eqList)
            {
                foreach (IECInstance iec in kv.Value)
                {
                    string sbbmq = "";
                    string sbbm = "";
                    if (iec.GetPropertyValue("CERI_Equipment_Num") != null)
                    {
                        sbbm = iec["CERI_Equipment_Num"].StringValue;
                    }
                    sbbm = buqi(sbbm);
                    sbbmq = sbbm.Substring(0, 5);
                    bool isXt = false;
                    foreach (KeyValuePair<int, List<IECInstance>> kvs in pxList)
                    {
                        string sbbmq1 = "";
                        string sbbm1 = "";
                        if (kvs.Value[0].GetPropertyValue("CERI_Equipment_Num") != null)
                        {
                            sbbm1 = kvs.Value[0]["CERI_Equipment_Num"].StringValue;
                        }
                        sbbm1 = buqi(sbbm1);
                        sbbmq1 = sbbm1.Substring(0, 5);
                        if (sbbmq.Equals(sbbmq1))
                        {
                            isXt = true;
                            pxList[kvs.Key].Add(iec);
                        }
                    }
                    if (!isXt)
                    {
                        List<IECInstance> iecList = new List<IECInstance>();
                        iecList.Add(iec);
                        pxList.Add(keyNumber, iecList);
                        keyNumber++;
                    }
                }
            }

            return pxList;
        }

        public Dictionary<int, List<IECInstance>> tjpaixu(Dictionary<int, List<IECInstance>> eqList)
        {
            Dictionary<int, List<IECInstance>> dicPxList = new Dictionary<int, List<IECInstance>>();
            foreach (KeyValuePair<int, List<IECInstance>> kv in eqList)
            {
                Dictionary<int, List<IECInstance>> zzList = new Dictionary<int, List<IECInstance>>();
                int keyNumber = 0;
                foreach (IECInstance iec in kv.Value)
                {
                    bool xt = false;

                    string sbbmq = "";
                    string sbbm = "";
                    if (iec.GetPropertyValue("CERI_Equipment_Num") != null)
                    {
                        sbbm = iec["CERI_Equipment_Num"].StringValue;
                    }
                    sbbm = buqi(sbbm);
                    sbbmq = sbbm.Substring(0, 5);
                    foreach (KeyValuePair<int, List<IECInstance>> kvs in zzList)
                    {
                        string sbbmq1 = "";
                        string sbbm1 = "";
                        if (kvs.Value[0].GetPropertyValue("CERI_Equipment_Num") != null)
                        {
                            sbbm1 = kvs.Value[0]["CERI_Equipment_Num"].StringValue;
                        }
                        sbbm1 = buqi(sbbm1);
                        sbbmq1 = sbbm1.Substring(0, 5);
                        if (sbbmq.Equals(sbbmq1))
                        {
                            xt = true;
                            zzList[kvs.Key].Add(iec);
                        }
                    }

                    if (!xt)
                    {
                        List<IECInstance> kList = new List<IECInstance>();
                        kList.Add(iec);
                        zzList.Add(keyNumber, kList);
                        keyNumber++;
                    }
                }
                List<IECInstance> iecList = new List<IECInstance>();
                foreach (List<IECInstance> iList in zzList.Values)
                {
                    iecList.AddRange(iList);
                }
                dicPxList.Add(kv.Key, iecList);
                //dicPxList.Add(kv.Key, zzList.Values.ToList());
            }
            return dicPxList;
        }
    }
}
