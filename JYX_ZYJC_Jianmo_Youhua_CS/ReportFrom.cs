using Bentley.OpenPlant.Modeler.Api;
using Bentley.ECObjects.Instance;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.OpenPlantModeler.SDK.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MOIE = Microsoft.Office.Interop.Excel;
using System.Diagnostics;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET;
using Bentley.ECObjects.Schema;
using Bentley.EC.Persistence.Query;
using Bentley.EC.Persistence;
using Bentley.DgnPlatformNET.Elements;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.Building.Mechanical.Components;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using System.Data.OleDb;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ReportFrom : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        //bool isSele = false;
        static bool isCailiao = true;
        public static ReportFrom reportFrom = null;
        public Dictionary<int, Dictionary<int, List<IECInstance>>> itemList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
        private Dictionary<int, Dictionary<int, List<IECInstance>>> allItemList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
        private static Bentley.Interop.MicroStationDGN.Application app1 = Utilities.ComApp;
        private static string path0 = app1.ActiveWorkspace.ConfigurationVariableValue("MSDIR"); //相对路径
        //C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMetric\Standards\OpenPlant
        public List<string> MsName = new List<string>();
        public Dictionary<string, List<string>> msJtName = new Dictionary<string, List<string>>();
        MOIE.Application app = null;
        public static List<string> flNameList = new List<string>();
        public static bool isFl = false;
        public string openPath = "";
        protected static BMECApi api = BMECApi.Instance;

        static double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//当前设计文件的主单位
        public ReportFrom()
        {
            InitializeComponent();
            initialization();
        }

        /// <summary>
        /// 限制只能打开一个from
        /// </summary>
        /// <param name="isCai"></param>
        /// <returns></returns>
        public static ReportFrom instence(bool isCai)
        {
            isCailiao = isCai;
            if (reportFrom == null)
            {
                reportFrom = new ReportFrom();
            }
            else
            {
                reportFrom.Close();
                reportFrom = new ReportFrom();
            }
            return reportFrom;
        }

        /// <summary>
        /// 导出报表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ECInstanceList ecList = shaixuanEc();
            if (ecList == null)
            {
                return;
            }
            try
            {

                if (zlcomboBox1.Text.Equals("管道材料表"))
                {
                    itemList = fuzhi(ecList);
                    ExportExcel1(itemList);
                }
                else if (zlcomboBox1.Text.Equals("隔热材料表"))
                {
                    itemList = grfuzhi1(ecList);
                    grExportExcel1(itemList);
                }
                else if (zlcomboBox1.Text.Equals("管道造价表") || zlcomboBox1.Text.Equals("管道造价汇总表"))
                {
                    itemList = zjfuzhi(ecList);
                    zjExportExcel1(itemList);
                }
                else if (zlcomboBox1.Text.Equals("汇总材料表"))
                {
                    itemList = hzfuzhi(ecList);
                    hzExportExcel1(itemList);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                if (app != null)
                {
                    app.Quit();

                    KillP.Kill(app);
                }
            }
            finally
            {
                if (openPath != "")
                {
                    Process.Start(openPath); //用打开进程方式打开  不用app1.Visible = true因为会占用后台进程 导致关不掉
                    //app.Quit();
                }
            }
            //itemList = fuzhi(ecList);
            #region
            //Dictionary<int, string> dicPipeLine = new Dictionary<int, string>(); //与itemList字典类的key相同
            //Dictionary<int, List<pipingPro>> sxList = new Dictionary<int, List<pipingPro>>(); //筛选条件的集合
            //if (dataGridView1.Rows.Count > 0)
            //{
            //    #region 得到选择的管线编号
            //    for (int i = 0; i < dataGridView1.Rows.Count; i++)
            //    {
            //        bool b = Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value);
            //        if (b)
            //        {
            //            dicPipeLine.Add(i, dataGridView1.Rows[i].Cells["PipeLineName"].Value.ToString());
            //        }
            //    }
            //    if (dicPipeLine.Count == 0)
            //    {
            //        MessageBox.Show("请至少选择一个管线编号！");
            //        return;
            //    }
            //    #endregion
            //    #region 根据选择的管线编号 查找勾选的元件
            //    foreach (KeyValuePair<int, string> kv in dicPipeLine)
            //    {
            //        List<pipingPro> proList = new List<pipingPro>();
            //        for (int j = 0; j < dataGridView2.Rows.Count; j++)
            //        {
            //            bool b1 = Convert.ToBoolean(dataGridView2.Rows[j].Cells[0].Value);
            //            if (b1)
            //            {
            //                string pipeLine = dataGridView2.Rows[j].Cells["pipeLineName1"].Value.ToString();
            //                if (pipeLine == kv.Value)
            //                {
            //                    pipingPro pipClass = new pipingPro();
            //                    string ecName = dataGridView2.Rows[j].Cells["ecName"].Value.ToString();
            //                    double dn = Convert.ToDouble(dataGridView2.Rows[j].Cells["DN"].Value);
            //                    pipClass.pipeLineName = pipeLine;
            //                    pipClass.ecName = ecName;
            //                    pipClass.DN = dn;
            //                    proList.Add(pipClass);
            //                }
            //            }
            //        }
            //        if (proList.Count > 0)
            //        {
            //            sxList.Add(kv.Key, proList);
            //        }
            //    }
            //    if (sxList.Count == 0)
            //    {
            //        MessageBox.Show("所选管线编号上没有选择相应的元件！");
            //        return;
            //    }
            //    #endregion
            //    #region 得到筛选后的数据
            //    Dictionary<int, Dictionary<int, List<IECInstance>>> sxItemList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            //    foreach (KeyValuePair<int, List<pipingPro>> kv in sxList)
            //    {
            //        Dictionary<int, List<IECInstance>> sxIecList = new Dictionary<int, List<IECInstance>>();
            //        foreach (KeyValuePair<int, List<IECInstance>> kv1 in itemList[kv.Key])
            //        {
            //            string pipeLineName = kv1.Value[0]["LINENUMBER"].StringValue;
            //            string ecName = kv1.Value[0].ClassDefinition.Name;
            //            double dn = kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
            //            bool isCz = false;
            //            foreach (pipingPro proClass in kv.Value)
            //            {
            //                if (pipeLineName == proClass.pipeLineName && ecName == proClass.ecName && dn == proClass.DN)
            //                {
            //                    isCz = true;
            //                }
            //            }
            //            if (isCz)
            //            {
            //                sxIecList.Add(kv1.Key, kv1.Value);
            //            }
            //        }
            //        sxItemList.Add(kv.Key, sxIecList);
            //    }
            //    #endregion
            //    ExportExcel(itemList);
            //}
            #endregion
            //ExportExcel(itemList);
            #region 导出报表 （没有筛选条件）
            //ExportMaterials exClass = new ExportMaterials();
            //if(radioButton1.Checked==true)
            //{
            //    isSele = false;
            //}
            //if(radioButton2.Checked==true)
            //{
            //    isSele = true;
            //}
            //if(isCailiao)
            //{
            //    exClass.selectExportExcel(isSele);
            //}
            //else
            //{
            //    exClass.selectEquipmentExcel(isSele);
            //}
            #endregion
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        /// <summary>
        /// 窗体加载时，给DataGridView附加checkBox表头以及给dataGridView赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReportFrom_Load(object sender, EventArgs e)
        {
            AddCheckBoxToDataGridView.dgv = dataGridView1;
            AddCheckBoxToDataGridView.AddFullSelect();
            AddCheckBoxToDataGridView.dgv1 = dataGridView2;
            AddCheckBoxToDataGridView.AddFullSelect1();
            qxcheckBox1.Checked = true;
            zlcomboBox1.DataSource = MsName;
            zlcomboBox1.SelectedIndex = 1;
            wlbmGroupcheckBox1.Checked = true;
            dataGridView2.Visible = false;
            //zlcomboBox1.Text = "管道材料表";
            //lxcomboBox1.Items = msJtName[zlcomboBox1.Text.ToString()];
            //ECInstanceList ecList = DgnUtilities.GetAllInstancesFromDgn();
            //ecList = attDisplayFil(ecList);

        }

        /// <summary>
        /// 得到要导出的数据，并按字典类格式存储
        /// </summary>
        /// <param name="ecList"></param>
        /// <returns></returns>
        public Dictionary<int, Dictionary<int, List<IECInstance>>> fuzhi(ECInstanceList ecList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> dicEcList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (ecList.Count > 0)
            {
                Dictionary<int, string> lineList = new Dictionary<int, string>();
                Dictionary<int, List<IECInstance>> iecDicInstanceList = new Dictionary<int, List<IECInstance>>();
                #region 将数据按管线编号进行分组
                int keyNumber = 0;
                foreach (IECInstance ec in ecList)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(ec, "PIPING_COMPONENT", true);
                    if (ec.GetPropertyValue("LINENUMBER") != null && ec.GetPropertyValue("SPECIFICATION") != null)
                    {
                        b = true;
                    }
                    #region
                    if (b)
                    {
                        bool isc = false;
                        string fenlei = "";
                        if (ec.GetPropertyValue("CERI_Classify") == null)
                        {
                            isc = true;
                        }
                        else
                        {
                            fenlei = ec["CERI_Classify"].StringValue;
                        }
                        if (fenlei == "材料" || fenlei == "cailiao" || isc || fenlei == "")
                        {
                            string LineNumber = ec["LINENUMBER"].StringValue;
                            if (lineList.Count == 0)
                            {
                                lineList.Add(keyNumber, LineNumber);
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(ec);
                                iecDicInstanceList.Add(keyNumber, iecList);
                            }
                            else
                            {
                                bool b1 = lineList.ContainsValue(LineNumber);
                                if (!b1)
                                {
                                    keyNumber += 1;
                                    lineList.Add(keyNumber, LineNumber);
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(ec);
                                    iecDicInstanceList.Add(keyNumber, iecList);
                                }
                                else
                                {
                                    int indexKey = 0;
                                    foreach (KeyValuePair<int, string> kv in lineList)
                                    {
                                        if (kv.Value == LineNumber)
                                        {
                                            indexKey = kv.Key;
                                        }
                                    }
                                    iecDicInstanceList[indexKey].Add(ec);
                                }
                            }
                        }
                    }
                    #endregion
                    string gbpipe = "";
                    string pic = "";
                    bool isgb = isGbZj(ec, out gbpipe, out pic);
                    if (isgb)
                    {
                        if (lineList.Count == 0)
                        {
                            lineList.Add(keyNumber, gbpipe);
                            List<IECInstance> iecList = new List<IECInstance>();
                            iecList.Add(ec);
                            iecDicInstanceList.Add(keyNumber, iecList);
                        }
                        else
                        {
                            bool b1 = lineList.ContainsValue(gbpipe);
                            if (!b1)
                            {
                                keyNumber += 1;
                                lineList.Add(keyNumber, gbpipe);
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(ec);
                                iecDicInstanceList.Add(keyNumber, iecList);
                            }
                            else
                            {
                                int indexKey = 0;
                                foreach (KeyValuePair<int, string> kv in lineList)
                                {
                                    if (kv.Value == gbpipe)
                                    {
                                        indexKey = kv.Key;
                                    }
                                }
                                iecDicInstanceList[indexKey].Add(ec);
                            }
                        }
                    }
                }
                #endregion
                Dictionary<int, Dictionary<int, List<IECInstance>>> rowIecList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
                if (iecDicInstanceList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in iecDicInstanceList)
                    {
                        Dictionary<int, List<IECInstance>> rowList = new Dictionary<int, List<IECInstance>>();
                        #region 将元件按ecClass和DN进行分组
                        int rowm = 0;
                        foreach (IECInstance iec in iecDicInstanceList[kv.Key])
                        {

                            string ecName = iec.ClassDefinition.Name;
                            string pice = "";
                            if (iec.GetPropertyValue("CERI_PIECE_MARK") != null)
                            {
                                pice = iec["CERI_PIECE_MARK"].StringValue;
                            }
                            if (rowList.Count == 0)
                            {
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(iec);
                                rowList.Add(rowm, iecList);
                            }
                            else
                            {
                                int kv_key = 0;
                                bool bbb = true;
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in rowList)
                                {
                                    foreach (IECInstance iecinstan in kv1.Value)
                                    {
                                        string ecName1 = iecinstan.ClassDefinition.Name;
                                        string pice1 = "";
                                        bool isxt = isXt(iec, iecinstan);
                                        if (iecinstan.GetPropertyValue("CERI_PIECE_MARK") != null)
                                        {
                                            pice1 = iecinstan["CERI_PIECE_MARK"].StringValue;
                                        }
                                        if (isxt)
                                        {
                                            bbb = false;
                                            kv_key = kv1.Key;
                                        }
                                    }
                                }
                                if (bbb)
                                {
                                    rowm++;
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(iec);
                                    rowList.Add(rowm, iecList);
                                }
                                else
                                {
                                    rowList[kv_key].Add(iec);
                                }
                            }

                        }
                        rowIecList.Add(kv.Key, rowList);
                        #endregion
                    }

                    dicEcList = rowIecList;
                }
            }
            dicEcList = px(dicEcList);
            return dicEcList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> grfuzhi(ECInstanceList ecList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> dicEcList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (ecList.Count > 0)
            {
                Dictionary<int, string> lineList = new Dictionary<int, string>();
                Dictionary<int, List<IECInstance>> iecDicInstanceList = new Dictionary<int, List<IECInstance>>();
                #region 将数据按管线编号进行分组
                int keyNumber = 0;
                foreach (IECInstance ec in ecList)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(ec, "PIPING_COMPONENT", true);
                    if (ec.GetPropertyValue("LINENUMBER") != null && ec.GetPropertyValue("SPECIFICATION") != null)
                    {
                        b = true;
                    }
                    if (b)
                    {
                        bool isc = false;
                        string fenlei = "";
                        if (ec.GetPropertyValue("CERI_Classify") == null)
                        {
                            isc = true;
                        }
                        else
                        {
                            fenlei = ec["CERI_Classify"].StringValue;
                        }
                        if (fenlei == "材料" || fenlei == "cailiao" || isc || fenlei == "")
                        {
                            string LineNumber = ec["LINENUMBER"].StringValue;
                            if (lineList.Count == 0)
                            {
                                lineList.Add(keyNumber, LineNumber);
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(ec);
                                iecDicInstanceList.Add(keyNumber, iecList);
                            }
                            else
                            {
                                bool b1 = lineList.ContainsValue(LineNumber);
                                if (!b1)
                                {
                                    keyNumber += 1;
                                    lineList.Add(keyNumber, LineNumber);
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(ec);
                                    iecDicInstanceList.Add(keyNumber, iecList);
                                }
                                else
                                {
                                    int indexKey = 0;
                                    foreach (KeyValuePair<int, string> kv in lineList)
                                    {
                                        if (kv.Value == LineNumber)
                                        {
                                            indexKey = kv.Key;
                                        }
                                    }
                                    iecDicInstanceList[indexKey].Add(ec);
                                }
                            }
                        }
                    }
                }
                #endregion
                Dictionary<int, Dictionary<int, List<IECInstance>>> rowIecList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
                if (iecDicInstanceList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in iecDicInstanceList)
                    {
                        Dictionary<int, List<IECInstance>> rowList = new Dictionary<int, List<IECInstance>>();
                        #region 将元件按ecClass和DN进行分组
                        int rowm = 0;
                        foreach (IECInstance iec in iecDicInstanceList[kv.Key])
                        {

                            string ecName = iec.ClassDefinition.Name;
                            double dn = 0;
                            if (iec.GetPropertyValue("NOMINAL_DIAMETER") != null)
                            {
                                dn = iec["NOMINAL_DIAMETER"].DoubleValue;
                            }
                            if (rowList.Count == 0)
                            {
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(iec);
                                rowList.Add(rowm, iecList);
                            }
                            else
                            {
                                int kv_key = 0;
                                bool bbb = true;
                                bbb = grShaixuan(iec, rowList, out kv_key);
                                #region
                                //foreach (KeyValuePair<int, List<IECInstance>> kv1 in rowList)
                                //{
                                //    foreach (IECInstance iecinstan in kv1.Value)
                                //    {
                                //        string ecName1 = iecinstan.ClassDefinition.Name;
                                //        double dn1 = 0;
                                //        if (iecinstan.GetPropertyValue("NOMINAL_DIAMETER") != null)
                                //        {
                                //            dn1 = iecinstan["NOMINAL_DIAMETER"].DoubleValue;
                                //        }
                                //        if (ecName == ecName1 && dn == dn1)
                                //        {
                                //            bbb = false;
                                //            kv_key = kv1.Key;
                                //        }
                                //    }
                                //}
                                #endregion
                                if (bbb)
                                {
                                    rowm++;
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(iec);
                                    rowList.Add(rowm, iecList);
                                }
                                else
                                {
                                    rowList[kv_key].Add(iec);
                                }
                            }

                        }
                        rowIecList.Add(kv.Key, rowList);
                        #endregion
                    }

                    dicEcList = rowIecList;
                }
            }
            return dicEcList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> grfuzhi1(ECInstanceList ecList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> dicEcList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (ecList.Count > 0)
            {
                Dictionary<int, string> lineList = new Dictionary<int, string>();
                Dictionary<int, List<IECInstance>> iecDicInstanceList = new Dictionary<int, List<IECInstance>>();
                #region 将数据按管线编号进行分组
                int keyNumber = 0;
                foreach (IECInstance ec in ecList)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(ec, "PIPING_COMPONENT", true);
                    if (ec.GetPropertyValue("LINENUMBER") != null && ec.GetPropertyValue("SPECIFICATION") != null)
                    {
                        b = true;
                    }
                    #region
                    if (b)
                    {
                        bool isc = false;
                        string fenlei = "";
                        if (ec.GetPropertyValue("CERI_Classify") == null)
                        {
                            isc = true;
                        }
                        else
                        {
                            fenlei = ec["CERI_Classify"].StringValue;
                        }
                        if (fenlei == "材料" || fenlei == "cailiao" || isc || fenlei == "")
                        {
                            string LineNumber = ec["LINENUMBER"].StringValue;
                            if (lineList.Count == 0)
                            {
                                lineList.Add(keyNumber, LineNumber);
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(ec);
                                iecDicInstanceList.Add(keyNumber, iecList);
                            }
                            else
                            {
                                bool b1 = lineList.ContainsValue(LineNumber);
                                if (!b1)
                                {
                                    keyNumber += 1;
                                    lineList.Add(keyNumber, LineNumber);
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(ec);
                                    iecDicInstanceList.Add(keyNumber, iecList);
                                }
                                else
                                {
                                    int indexKey = 0;
                                    foreach (KeyValuePair<int, string> kv in lineList)
                                    {
                                        if (kv.Value == LineNumber)
                                        {
                                            indexKey = kv.Key;
                                        }
                                    }
                                    iecDicInstanceList[indexKey].Add(ec);
                                }
                            }
                        }
                    }
                    #endregion

                    string gbpipe = "";
                    string gbdn = "";
                    bool isgb = isGbZj(ec, out gbpipe, out gbdn);
                    if (isgb)
                    {
                        if (lineList.Count == 0)
                        {
                            lineList.Add(keyNumber, gbpipe);
                            List<IECInstance> iecList = new List<IECInstance>();
                            iecList.Add(ec);
                            iecDicInstanceList.Add(keyNumber, iecList);
                        }
                        else
                        {
                            bool b1 = lineList.ContainsValue(gbpipe);
                            if (!b1)
                            {
                                keyNumber += 1;
                                lineList.Add(keyNumber, gbpipe);
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(ec);
                                iecDicInstanceList.Add(keyNumber, iecList);
                            }
                            else
                            {
                                int indexKey = 0;
                                foreach (KeyValuePair<int, string> kv in lineList)
                                {
                                    if (kv.Value == gbpipe)
                                    {
                                        indexKey = kv.Key;
                                    }
                                }
                                iecDicInstanceList[indexKey].Add(ec);
                            }
                        }
                    }
                }
                #endregion
                Dictionary<int, Dictionary<int, List<IECInstance>>> rowIecList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
                if (iecDicInstanceList.Count > 0)
                {
                    foreach (KeyValuePair<int, List<IECInstance>> kv in iecDicInstanceList)
                    {
                        Dictionary<int, List<IECInstance>> rowList = new Dictionary<int, List<IECInstance>>();
                        #region 将元件按ecClass和DN进行分组
                        int rowm = 0;
                        foreach (IECInstance iec in iecDicInstanceList[kv.Key])
                        {

                            //string ecName = iec.ClassDefinition.Name;
                            string pic1 = "";
                            if (iec.GetPropertyValue("CERI_PIECE_MARK") != null)
                            {
                                pic1 = iec["CERI_PIECE_MARK"].StringValue;
                            }
                            if (rowList.Count == 0)
                            {
                                List<IECInstance> iecList = new List<IECInstance>();
                                iecList.Add(iec);
                                rowList.Add(rowm, iecList);
                            }
                            else
                            {
                                int kv_key = 0;
                                bool bbb = true;
                                //bbb = grShaixuan(iec, rowList, out kv_key);
                                #region
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in rowList)
                                {
                                    foreach (IECInstance iecinstan in kv1.Value)
                                    {
                                        //string ecName1 = iecinstan.ClassDefinition.Name;
                                        string pic2 = "";
                                        if (iecinstan.GetPropertyValue("CERI_PIECE_MARK") != null)
                                        {
                                            pic2 = iecinstan["CERI_PIECE_MARK"].StringValue;
                                        }
                                        if (pic1 == pic2)
                                        {
                                            bbb = false;
                                            kv_key = kv1.Key;
                                        }
                                    }
                                }
                                #endregion
                                if (bbb)
                                {
                                    rowm++;
                                    List<IECInstance> iecList = new List<IECInstance>();
                                    iecList.Add(iec);
                                    rowList.Add(rowm, iecList);
                                }
                                else
                                {
                                    string ecName = iec.ClassDefinition.Name;
                                    string ecName1 = rowList[kv_key][0].ClassDefinition.Name;
                                    if (!ecName.Equals("PIPE"))
                                    {
                                        rowList[kv_key].Add(iec);
                                    }
                                    else
                                    {
                                        if (!ecName1.Equals("PIPE"))
                                        {
                                            rowList[kv_key].Insert(0, iec);
                                        }
                                        else
                                        {
                                            rowList[kv_key].Add(iec);
                                        }
                                    }
                                }
                            }

                        }
                        rowIecList.Add(kv.Key, rowList);
                        #endregion
                    }

                    dicEcList = rowIecList;
                }
            }
            return dicEcList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> zjfuzhi(ECInstanceList ecList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> dicEcList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int keyNumber = 0;
            #region 管道造价汇总表
            if (zlcomboBox1.Text.Equals("管道造价汇总表"))
            {
                if (wlbmGroupcheckBox1.Checked)
                {
                    foreach (IECInstance iechz in ecList)
                    {
                        bool bz = false;
                        if (iechz.GetPropertyValue("LINENUMBER") != null && iechz.GetPropertyValue("SPECIFICATION") != null)
                        {
                            bz = true;
                        }
                        #region
                        if (bz)
                        {
                            string pipNumberName1 = iechz["LINENUMBER"].StringValue;
                            bool isCz1 = false;
                            int key1 = 0;

                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in dicEcList)
                            {
                                string pN1 = kvs.Value[0][0]["LINENUMBER"].StringValue;
                                if (pipNumberName1.Equals(pN1))
                                {
                                    isCz1 = true;
                                    key1 = kvs.Key;
                                }
                            }
                            if (isCz1)
                            {
                                int zkey = 0;
                                bool isz = false;
                                string wlbm = "";
                                if (iechz.GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = iechz["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (iechz.GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = iechz["GRADE"].StringValue;
                                    }
                                }
                                foreach (KeyValuePair<int, List<IECInstance>> kvz in dicEcList[key1])
                                {
                                    if (wlbm != "" && wlbm != null)
                                    {
                                        string wlbm1 = "";
                                        if (kvz.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                        {
                                            wlbm1 = kvz.Value[0]["CERI_MAT_GRADE"].StringValue;
                                        }
                                        else
                                        {
                                            if (kvz.Value[0].GetPropertyValue("GRADE") != null)
                                            {
                                                wlbm1 = kvz.Value[0]["GRADE"].StringValue;
                                            }
                                        }
                                        if (wlbm.Equals(wlbm1))
                                        {
                                            isz = true;
                                            zkey = kvz.Key;
                                        }
                                    }
                                    else
                                    {
                                        bool isK = zjBj(iechz, kvz.Value[0]);
                                        if (isK)
                                        {
                                            isz = true;
                                            zkey = kvz.Key;
                                        }
                                    }
                                }
                                if (isz)
                                {
                                    dicEcList[key1][zkey].Add(iechz);
                                }
                                else
                                {
                                    List<IECInstance> zList = new List<IECInstance>();
                                    zList.Add(iechz);
                                    dicEcList[key1].Add(dicEcList[key1].Count, zList);
                                }
                            }
                            else
                            {
                                Dictionary<int, List<IECInstance>> dicList = new Dictionary<int, List<IECInstance>>();
                                List<IECInstance> insList = new List<IECInstance>();
                                insList.Add(iechz);
                                dicList.Add(0, insList);
                                dicEcList.Add(keyNumber, dicList);
                                keyNumber++;
                            }
                        }
                        #endregion
                        string gbpipe = "";
                        string gbdn = "";
                        bool isgb = isGbZj(iechz, out gbpipe, out gbdn);
                        if (isgb)
                        {
                            bool isCz1 = false;
                            int key1 = 0;

                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in dicEcList)
                            {
                                string pN1 = "";
                                string gbdn1 = "";
                                bool isgb1 = isGbZj(kvs.Value[0][0], out pN1, out gbdn1);
                                if (!isgb1) pN1 = kvs.Value[0][0]["LINENUMBER"].StringValue;
                                if (gbpipe.Equals(pN1))
                                {
                                    isCz1 = true;
                                    key1 = kvs.Key;
                                }
                            }
                            if (isCz1)
                            {
                                int zkey = 0;
                                bool isz = false;
                                string wlbm = "";
                                if (iechz.GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = iechz["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (iechz.GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = iechz["GRADE"].StringValue;
                                    }
                                }
                                foreach (KeyValuePair<int, List<IECInstance>> kvz in dicEcList[key1])
                                {
                                    if (wlbm != "" && wlbm != null)
                                    {
                                        string wlbm1 = "";
                                        if (kvz.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                        {
                                            wlbm1 = kvz.Value[0]["CERI_MAT_GRADE"].StringValue;
                                        }
                                        else
                                        {
                                            if (kvz.Value[0].GetPropertyValue("GRADE") != null)
                                            {
                                                wlbm1 = kvz.Value[0]["GRADE"].StringValue;
                                            }
                                        }
                                        if (wlbm.Equals(wlbm1))
                                        {
                                            isz = true;
                                            zkey = kvz.Key;
                                        }
                                    }
                                    else
                                    {
                                        bool isK = zjBj(iechz, kvz.Value[0]);
                                        if (isK)
                                        {
                                            isz = true;
                                            zkey = kvz.Key;
                                        }
                                    }
                                }
                                if (isz)
                                {
                                    dicEcList[key1][zkey].Add(iechz);
                                }
                                else
                                {
                                    List<IECInstance> zList = new List<IECInstance>();
                                    zList.Add(iechz);
                                    dicEcList[key1].Add(dicEcList[key1].Count, zList);
                                }
                            }
                            else
                            {
                                Dictionary<int, List<IECInstance>> dicList = new Dictionary<int, List<IECInstance>>();
                                List<IECInstance> insList = new List<IECInstance>();
                                insList.Add(iechz);
                                dicList.Add(0, insList);
                                dicEcList.Add(keyNumber, dicList);
                                keyNumber++;
                            }
                        }
                    }
                    dicEcList = px(dicEcList);
                    dicEcList = hzpx(dicEcList);
                    return dicEcList;
                }
            }
            #endregion
            foreach (IECInstance iec in ecList)
            {
                bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPING_COMPONENT", true);
                bool isc = false, isPE = false;
                string fenlei = "";
                if (iec.GetPropertyValue("CERI_Classify") == null)
                {
                    isc = true;//判断是否有CERI_Classify这一属性
                }
                else
                {
                    fenlei = iec["CERI_Classify"].StringValue;
                }
                if (fenlei == "材料" || fenlei == "cailiao" || isc || fenlei == "")
                {
                    isPE = true;//true为材料
                }
                if (iec.GetPropertyValue("LINENUMBER") != null && iec.GetPropertyValue("SPECIFICATION") != null)
                {
                    b = true;
                }
                else
                {
                    b = false;
                }
                #region
                if (b && isPE)
                {
                    string pipeNumberName = iec["LINENUMBER"].StringValue;
                    bool isCz = false, isGx = false;
                    int key = 0;
                    string ecName = iec.ClassDefinition.Name;
                    double dn = 0;
                    if (iec.GetPropertyValue("NOMINAL_DIAMETER") != null)
                    {
                        dn = iec["NOMINAL_DIAMETER"].DoubleValue;
                    }
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in dicEcList)
                    {
                        string pN = "";
                        string dnx = "";
                        bool isgb1 = isGbZj(kv.Value[0][0], out pN, out dnx);
                        if (!isgb1) pN = kv.Value[0][0]["LINENUMBER"].StringValue;
                        //string pN = kv.Value[0][0]["LINENUMBER"].StringValue;
                        if (pipeNumberName.Equals(pN))
                        {
                            isGx = true;
                            bool isXt = false;
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                string ecName1 = kvs.Value[0].ClassDefinition.Name;
                                double dn1 = 0;
                                bool isxt = this.isXt(iec, kvs.Value[0]);
                                if (kvs.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    dn1 = kvs.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                                }
                                if (isxt)
                                {
                                    isXt = true;
                                    dicEcList[kv.Key][kvs.Key].Add(iec);
                                }
                            }
                            if (!isXt)
                            {
                                isCz = true;
                                key = kv.Key;
                            }
                        }
                    }
                    if (isCz)
                    {
                        List<IECInstance> iecli = new List<IECInstance>();
                        iecli.Add(iec);
                        dicEcList[key].Add(dicEcList[key].Count, iecli);
                    }
                    if (!isGx)
                    {
                        Dictionary<int, List<IECInstance>> dicList = new Dictionary<int, List<IECInstance>>();
                        List<IECInstance> insList = new List<IECInstance>();
                        insList.Add(iec);
                        dicList.Add(0, insList);
                        dicEcList.Add(keyNumber, dicList);
                        keyNumber++;
                    }
                }
                #endregion

                string gbpipe = "";
                string gbdn = "";
                bool isgb = isGbZj(iec, out gbpipe, out gbdn);
                if (isgb)
                {
                    bool isCz = false, isGx = false;
                    int key = 0;
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in dicEcList)
                    {
                        string pN = "";
                        string dnx = "";
                        bool isgb1 = isGbZj(kv.Value[0][0], out pN, out dnx);
                        if (!isgb1) pN = kv.Value[0][0]["LINENUMBER"].StringValue;
                        if (gbpipe.Equals(pN))
                        {
                            isGx = true;
                            bool isXt = false;
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                string ecName1 = kvs.Value[0].ClassDefinition.Name;
                                double dn1 = 0;
                                bool isxt = this.isXt(iec, kvs.Value[0]);
                                if (kvs.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    dn1 = kvs.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                                }
                                if (isxt)
                                {
                                    isXt = true;
                                    dicEcList[kv.Key][kvs.Key].Add(iec);
                                }
                            }
                            if (!isXt)
                            {
                                isCz = true;
                                key = kv.Key;
                            }
                        }
                    }
                    if (isCz)
                    {
                        List<IECInstance> iecli = new List<IECInstance>();
                        iecli.Add(iec);
                        dicEcList[key].Add(dicEcList[key].Count, iecli);
                    }
                    if (!isGx)
                    {
                        Dictionary<int, List<IECInstance>> dicList = new Dictionary<int, List<IECInstance>>();
                        List<IECInstance> insList = new List<IECInstance>();
                        insList.Add(iec);
                        dicList.Add(0, insList);
                        dicEcList.Add(keyNumber, dicList);
                        keyNumber++;
                    }
                }
            }
            dicEcList = px(dicEcList);
            return dicEcList;
        }

        /// <summary>
        /// 造价表 物料编码存在时 进行汇总
        /// </summary>
        /// <param name="ecList"></param>
        /// <returns></returns>
        public Dictionary<int, List<IECInstance>> zjwlHz(List<IECInstance> ecList)
        {
            Dictionary<int, List<IECInstance>> zjhzList = new Dictionary<int, List<IECInstance>>();

            foreach (IECInstance iec in ecList)
            {
                //bool isK = zjBj(iechz, kvz.Value[0]);
                bool isX = true, isJ = false;
                int key = 0;
                foreach (KeyValuePair<int, List<IECInstance>> kv in zjhzList)
                {
                    bool isK = zjBj(iec, kv.Value[0]);
                    if (isK)
                    {
                        isX = false;
                        isJ = true;
                        key = kv.Key;
                    }
                }
                if (isJ)
                {
                    zjhzList[key].Add(iec);
                }
                if (isX)
                {
                    List<IECInstance> iecList = new List<IECInstance>();
                    iecList.Add(iec);
                    zjhzList.Add(zjhzList.Count, iecList);
                }
            }

            return zjhzList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> hzfuzhi(ECInstanceList ecList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> dicEcList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            if (ecList.Count > 0)
            {
                #region 按物料编码进行分组
                int keyNum = 0;
                foreach (IECInstance iec in ecList)
                {
                    bool b = false;
                    if (iec.GetPropertyValue("LINENUMBER") != null && iec.GetPropertyValue("SPECIFICATION") != null)
                    {
                        bool isc = false;
                        string fenlei = "";
                        if (iec.GetPropertyValue("CERI_Classify") == null)
                        {
                            isc = true;
                        }
                        else
                        {
                            fenlei = iec["CERI_Classify"].StringValue;
                        }
                        if (fenlei == "材料" || fenlei == "cailiao" || isc || fenlei == "")
                        {
                            b = true;
                        }

                    }
                    #region
                    if (b)
                    {
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

                        bool iswl = false;
                        int keyDic = 0;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in dicEcList)
                        {
                            string wlbm1 = "";
                            if (kvs.Value[0][0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm1 = kvs.Value[0][0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kvs.Value[0][0].GetPropertyValue("GRADE") != null)
                                {
                                    wlbm1 = kvs.Value[0][0]["GRADE"].StringValue;
                                }
                            }
                            if (wlbm.Equals(wlbm1))
                            {
                                iswl = true;
                                keyDic = kvs.Key;
                                bool isDicz = false;
                                int keyz = 0;
                                foreach (KeyValuePair<int, List<IECInstance>> kvz in kvs.Value)
                                {
                                    bool isHz = hzPd(iec, kvz.Value[0]);
                                    if (isHz)
                                    {
                                        keyz = kvz.Key;
                                        isDicz = true;
                                    }
                                }
                                if (isDicz)
                                {
                                    dicEcList[keyDic][keyz].Add(iec);
                                }
                                else
                                {
                                    List<IECInstance> iecZList = new List<IECInstance>();
                                    iecZList.Add(iec);
                                    dicEcList[keyDic].Add(dicEcList[keyDic].Count, iecZList);
                                }
                            }
                        }

                        if (!iswl)
                        {
                            List<IECInstance> iList = new List<IECInstance>();
                            iList.Add(iec);
                            //dicEcList[keyNum].Add(0, iList);
                            Dictionary<int, List<IECInstance>> dicL = new Dictionary<int, List<IECInstance>>();
                            dicL.Add(0, iList);
                            dicEcList.Add(keyNum, dicL);
                            keyNum++;
                        }
                    }
                    #endregion

                    string gbpipe = "";
                    string gbdn = "";
                    bool isgb = isGbZj(iec, out gbpipe, out gbdn);
                    if (isgb)
                    {
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

                        bool iswl = false;
                        int keyDic = 0;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in dicEcList)
                        {
                            string wlbm1 = "";
                            if (kvs.Value[0][0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm1 = kvs.Value[0][0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kvs.Value[0][0].GetPropertyValue("GRADE") != null)
                                {
                                    wlbm1 = kvs.Value[0][0]["GRADE"].StringValue;
                                }
                            }
                            if (wlbm.Equals(wlbm1))
                            {
                                iswl = true;
                                keyDic = kvs.Key;
                                bool isDicz = false;
                                int keyz = 0;
                                foreach (KeyValuePair<int, List<IECInstance>> kvz in kvs.Value)
                                {
                                    bool isHz = hzPd(iec, kvz.Value[0]);
                                    if (isHz)
                                    {
                                        keyz = kvz.Key;
                                        isDicz = true;
                                    }
                                }
                                if (isDicz)
                                {
                                    dicEcList[keyDic][keyz].Add(iec);
                                }
                                else
                                {
                                    List<IECInstance> iecZList = new List<IECInstance>();
                                    iecZList.Add(iec);
                                    dicEcList[keyDic].Add(dicEcList[keyDic].Count, iecZList);
                                }
                            }
                        }

                        if (!iswl)
                        {
                            List<IECInstance> iList = new List<IECInstance>();
                            iList.Add(iec);
                            //dicEcList[keyNum].Add(0, iList);
                            Dictionary<int, List<IECInstance>> dicL = new Dictionary<int, List<IECInstance>>();
                            dicL.Add(0, iList);
                            dicEcList.Add(keyNum, dicL);
                            keyNum++;
                        }
                    }
                }
                #endregion
            }
            dicEcList = px(dicEcList);
            dicEcList = px2(dicEcList);
            return dicEcList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> px(Dictionary<int, Dictionary<int, List<IECInstance>>> DicList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> pxList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();

            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in DicList)
            {
                Dictionary<int, List<IECInstance>> pxDic = new Dictionary<int, List<IECInstance>>();
                int key = 0;

                #region 筛选dic
                Dictionary<int, List<IECInstance>> gdDic = new Dictionary<int, List<IECInstance>>();
                int gdKey = 0;

                Dictionary<int, List<IECInstance>> wtDic = new Dictionary<int, List<IECInstance>>();
                int wtKey = 0;

                Dictionary<int, List<IECInstance>> santDic = new Dictionary<int, List<IECInstance>>();
                int santKey = 0;

                Dictionary<int, List<IECInstance>> yjjtDic = new Dictionary<int, List<IECInstance>>();
                int yjjtKey = 0;

                Dictionary<int, List<IECInstance>> stDic = new Dictionary<int, List<IECInstance>>();
                int stKey = 0;

                Dictionary<int, List<IECInstance>> fmDic = new Dictionary<int, List<IECInstance>>();
                int fmKey = 0;

                Dictionary<int, List<IECInstance>> flDic = new Dictionary<int, List<IECInstance>>();
                int flKey = 0;

                Dictionary<int, List<IECInstance>> zxDic = new Dictionary<int, List<IECInstance>>();
                int zxKey = 0;
                #endregion

                #region 筛选dic赋值
                foreach (KeyValuePair<int, List<IECInstance>> kv in kvs.Value)
                {
                    bool isgd = false, iswt = false, issant = false, isyjjt = false, isst = false, isfm = false, isfl = false;

                    isgd = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE", true);
                    iswt = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE_ELBOW", true);
                    issant = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE_TEE", true);
                    bool b1 = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "BASKET_STRAINER", true);
                    if (b1)
                        issant = true;
                    isyjjt = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE_REDUCER", true);
                    isst = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE_CROSS", true);
                    isfm = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "FLUID_REGULATOR", true);
                    isfl = BMECApi.Instance.InstanceDefinedAsClass(kv.Value[0], "PIPE_FLANGE", true);
                    if (isgd)
                    {
                        gdDic.Add(gdKey, kv.Value);
                        gdKey++;
                    }
                    else if (iswt)
                    {
                        wtDic.Add(wtKey, kv.Value);
                        wtKey++;
                    }
                    else if (issant)
                    {
                        santDic.Add(santKey, kv.Value);
                        santKey++;
                    }
                    else if (isyjjt)
                    {
                        yjjtDic.Add(yjjtKey, kv.Value);
                        yjjtKey++;
                    }
                    else if (isst)
                    {
                        stDic.Add(stKey, kv.Value);
                        stKey++;
                    }
                    else if (isfm)
                    {
                        fmDic.Add(fmKey, kv.Value);
                        fmKey++;
                    }
                    else if (isfl)
                    {
                        flDic.Add(flKey, kv.Value);
                        flKey++;
                    }
                    else
                    {
                        zxDic.Add(zxKey, kv.Value);
                        zxKey++;
                    }
                }
                #endregion

                #region 将管道有值的放在第一个
                for (int i = 0; i < gdDic.Count; i++)
                {
                    List<IECInstance> iecList = new List<IECInstance>();
                    bool k = false;
                    List<IECInstance> iecK = new List<IECInstance>();
                    foreach (var a in gdDic[i])
                    {
                        if (k)
                        {
                            iecK.Add(a);
                        }
                        else
                        {
                            double dry_weight = 0;
                            if (a.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                            {
                                dry_weight = a["CERI_WEIGHT_DRY"].DoubleValue;
                            }
                            else
                            {
                                if (a.GetPropertyValue("DRY_WEIGHT") != null)
                                {
                                    dry_weight = a["DRY_WEIGHT"].DoubleValue;
                                }
                            }
                            if (dry_weight == 0)
                            {
                                iecK.Add(a);
                            }
                            else
                            {
                                iecList.Add(a);
                                k = true;
                            }
                        }
                    }

                    iecList.AddRange(iecK);

                    gdDic[i] = iecList;
                }
                #endregion

                #region pxDic赋值
                foreach (KeyValuePair<int, List<IECInstance>> kv in gdDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in wtDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in santDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in yjjtDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in stDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in fmDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in flDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                foreach (KeyValuePair<int, List<IECInstance>> kv in zxDic)
                {
                    pxDic.Add(key, kv.Value);
                    key++;
                }
                #endregion

                pxList.Add(kvs.Key, pxDic);
            }

            //Dictionary<int, Dictionary<int, List<IECInstance>>> xx = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            Dictionary<int, List<IECInstance>> value = new Dictionary<int, List<IECInstance>>();
            for (int i = 0; i < pxList.Count - 1; i++)
            {
                string LineNumber = " ";
                if (pxList[i][0][0].GetPropertyValue("LINENUMBER") != null) LineNumber = pxList[i][0][0]["LINENUMBER"].StringValue;
                //string LineNumber = pxList[i][0][0]["LINENUMBER"].StringValue;
                for (int j = i + 1; j < pxList.Count; j++)
                {
                    string LineNumber1 = " ";
                    if (pxList[j][0][0].GetPropertyValue("LINENUMBER") != null) LineNumber1 = pxList[j][0][0]["LINENUMBER"].StringValue;
                    //string LineNumber1 = pxList[j][0][0]["LINENUMBER"].StringValue;
                    if (string.Compare(LineNumber, LineNumber1, true) == 1)
                    {
                        value = pxList[i];
                        pxList[i] = pxList[j];
                        pxList[j] = value;
                        LineNumber = LineNumber1;
                    }
                }
            }

            return pxList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> px2(Dictionary<int, Dictionary<int, List<IECInstance>>> DicList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> pxList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int key = 0;

            #region 筛选集合
            Dictionary<int, Dictionary<int, List<IECInstance>>> gdDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int gdKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> wtDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int wtKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> santDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int santKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> yjjtDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int yjjtKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> stDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int stKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> fmDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int fmKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> flDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int flKey = 0;

            Dictionary<int, Dictionary<int, List<IECInstance>>> zxDic = new Dictionary<int, Dictionary<int, List<IECInstance>>>();
            int zxKey = 0;
            #endregion

            #region 赋值
            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in DicList)
            {
                IECInstance iec = null;
                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                {
                    iec = kv1.Value[0];
                    break;
                }
                bool isgd = false, iswt = false, issant = false, isyjjt = false, isst = false, isfm = false, isfl = false;

                isgd = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                iswt = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_ELBOW", true);
                issant = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_TEE", true);
                bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec, "BASKET_STRAINER", true);
                if (b1)
                    issant = true;
                isyjjt = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_REDUCER", true);
                isst = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_CROSS", true);
                isfm = BMECApi.Instance.InstanceDefinedAsClass(iec, "FLUID_REGULATOR", true);
                isfl = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_FLANGE", true);

                if (isgd)
                {
                    gdDic.Add(gdKey, kv.Value);
                    gdKey++;
                }
                else if (iswt)
                {
                    wtDic.Add(wtKey, kv.Value);
                    wtKey++;
                }
                else if (issant)
                {
                    santDic.Add(santKey, kv.Value);
                    santKey++;
                }
                else if (isyjjt)
                {
                    yjjtDic.Add(yjjtKey, kv.Value);
                    yjjtKey++;
                }
                else if (isst)
                {
                    stDic.Add(stKey, kv.Value);
                    stKey++;
                }
                else if (isfm)
                {
                    fmDic.Add(fmKey, kv.Value);
                    fmKey++;
                }
                else if (isfl)
                {
                    flDic.Add(flKey, kv.Value);
                    flKey++;
                }
                else
                {
                    zxDic.Add(zxKey, kv.Value);
                    zxKey++;
                }
            }
            #endregion

            #region pxList赋值
            foreach (var kv in gdDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in wtDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in santDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in yjjtDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in stDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in fmDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in flDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            foreach (var kv in zxDic)
            {
                pxList.Add(key, kv.Value);
                key++;
            }
            #endregion

            return pxList;
        }

        public Dictionary<int, Dictionary<int, List<IECInstance>>> hzpx(Dictionary<int, Dictionary<int, List<IECInstance>>> DicList)
        {
            Dictionary<int, Dictionary<int, List<IECInstance>>> pxList = new Dictionary<int, Dictionary<int, List<IECInstance>>>();

            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kvs in DicList)
            {
                Dictionary<int, List<IECInstance>> dicList = new Dictionary<int, List<IECInstance>>();
                foreach (KeyValuePair<int, List<IECInstance>> kv in kvs.Value)
                {
                    List<IECInstance> list = new List<IECInstance>();

                    #region 筛选list
                    List<IECInstance> gdList = new List<IECInstance>();

                    List<IECInstance> wtList = new List<IECInstance>();

                    List<IECInstance> santList = new List<IECInstance>();

                    List<IECInstance> yjjtList = new List<IECInstance>();

                    List<IECInstance> stList = new List<IECInstance>();

                    List<IECInstance> fmList = new List<IECInstance>();

                    List<IECInstance> flList = new List<IECInstance>();

                    List<IECInstance> zxList = new List<IECInstance>();
                    #endregion

                    #region 筛选list赋值
                    foreach (IECInstance iec in kv.Value)
                    {
                        bool isgd = false, iswt = false, issant = false, isyjjt = false, isst = false, isfm = false, isfl = false;

                        isgd = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                        iswt = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_ELBOW", true);
                        issant = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_TEE", true);
                        bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec, "BASKET_STRAINER", true);
                        if (b1)
                            issant = true;
                        isyjjt = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_REDUCER", true);
                        isst = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_CROSS", true);
                        isfm = BMECApi.Instance.InstanceDefinedAsClass(iec, "FLUID_REGULATOR", true);
                        isfl = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_FLANGE", true);

                        if (isgd)
                        {
                            gdList.Add(iec);
                        }
                        else if (iswt)
                        {
                            wtList.Add(iec);
                        }
                        else if (issant)
                        {
                            santList.Add(iec);
                        }
                        else if (isyjjt)
                        {
                            yjjtList.Add(iec);
                        }
                        else if (isst)
                        {
                            stList.Add(iec);
                        }
                        else if (isfm)
                        {
                            fmList.Add(iec);
                        }
                        else if (isfl)
                        {
                            flList.Add(iec);
                        }
                        else
                        {
                            zxList.Add(iec);
                        }
                    }
                    #endregion

                    #region list赋值
                    list.AddRange(gdList);
                    list.AddRange(wtList);
                    list.AddRange(santList);
                    list.AddRange(yjjtList);
                    list.AddRange(stList);
                    list.AddRange(fmList);
                    list.AddRange(flList);
                    list.AddRange(zxList);
                    #endregion

                    dicList.Add(kv.Key, list);
                }
                pxList.Add(kvs.Key, dicList);
            }

            return pxList;
        }

        /// <summary>
        /// 点击全部时，获取dgn下的所有元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked == true)
            {
                dataGridView1.Rows.Clear();
                dataGridView2.Rows.Clear();
                //ECInstanceList ecList = DgnUtilities.GetAllInstancesFromDgn();
                //ecList = attDisplayFil(ecList);
                if (allItemList.Count > 0) itemList = allItemList;
                else
                {
                    ECInstanceList ecList = shaixuanEc();
                    itemList = fuzhi(ecList);
                }
                if (itemList.Count > 0)
                {
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    {
                        dataGridView1.Rows.Add(true, kv.Value[0][0]["LINENUMBER"].StringValue, kv.Value[0][0]["SPECIFICATION"].StringValue);
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            dataGridView2.Rows.Add(true, kv1.Value[0].ClassDefinition.Name, kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue.ToString(), kv1.Value[0]["LINENUMBER"].StringValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 点击选中时获取选中的元素
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked == true)
            {
                ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
                if (ecList.Count > 0)
                {
                    dataGridView1.Rows.Clear();
                    dataGridView2.Rows.Clear();
                    itemList = fuzhi(ecList);
                    if (itemList.Count > 0)
                    {
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            if (kv.Value[0][0].GetPropertyValue("LINENUMBER") != null)
                            {
                                dataGridView1.Rows.Add(true, kv.Value[0][0]["LINENUMBER"].StringValue, kv.Value[0][0]["SPECIFICATION"].StringValue);
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                                {
                                    if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null)
                                    {
                                        dataGridView2.Rows.Add(true, kv1.Value[0].ClassDefinition.Name, kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue.ToString(), kv1.Value[0]["LINENUMBER"].StringValue);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("请先选择元素！");
                    radioButton1.Checked = true;
                }
            }
        }

        /// <summary>
        /// 导出材料报表
        /// </summary>
        /// <param name="itemList"></param>
        public void ExportExcel(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\管道材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道材料表：下没有找到" + yName + "文件");
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
                if (lxcomboBox1.SelectedIndex == 1)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道材料表-海外"];
                }
                else if (lxcomboBox1.SelectedIndex == 2)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["压力管道材料表-国内 "];
                }
                else
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道材料表-国内"];
                }
                if (itemList.Count > 0)
                {
                    int row = 6; //行号
                    int lineindex = 0; //序号
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    {
                        row++;
                        lineindex++;
                        s.Cells[row, 1] = lineindex;
                        bool biaoshi = true;//避免重复添加管线编号
                        //s.Cells[row, 2] = kv.Value.Values[0]["LINENUMBER"].StringValue;
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            if (biaoshi)
                            {
                                string pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                s.Cells[row, 2] = pipeNumber;
                                biaoshi = false;
                            }
                            row++;
                            string ecname = "";
                            string dn = "";
                            string unit = "";
                            double qty = 0;
                            string materialNum = "";
                            string pipece_mare = "";
                            string material = "";
                            double dry_weight = 0;
                            string catalog_name = "";
                            string media = "";
                            double totalweight = 0;
                            string bz = "";
                            ecname = kv1.Value[0].ClassDefinition.Name;
                            if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                            {
                                ecname = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                {
                                    ecname = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                            {
                                dn = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    dn = "DN" + kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                                }
                                //bool iszhij = api.InstanceDefinedAsClass(kv1.Value[0], "SUPPORT", true); //查找ec的父类是否含有SUPPORT
                                //if (iszhij) dn = " ";
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                materialNum = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                {
                                    materialNum = kv1.Value[0]["GRADE"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                            {
                                pipece_mare = kv1.Value[0]["CERI_PIECE_MARK"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("PIECE_MARK") != null)
                                {
                                    pipece_mare = kv1.Value[0]["PIECE_MARK"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                            {
                                material = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                {
                                    material = kv1.Value[0]["MATERIAL"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                            {
                                catalog_name = kv1.Value[0]["CERI_CATALOG"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                {
                                    catalog_name = kv1.Value[0]["CATALOG_NAME"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            {
                                media = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                            {
                                bz = kv1.Value[0]["NOTES"].StringValue;
                            }
                            bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                            if (b)
                            {
                                if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                {
                                    dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                    {
                                        dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                    }
                                }
                                if (dry_weight < 0.005)
                                    dry_weight = 0;
                                unit = "米";
                                double length = 0;
                                foreach (IECInstance iec2 in kv1.Value)
                                {
                                    length += iec2["LENGTH"].DoubleValue / 1000;
                                    //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                }
                                qty = length;
                                totalweight = qty * dry_weight;
                            }
                            else
                            {
                                unit = "个";
                                if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                {
                                    dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                    {
                                        dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                    }
                                }
                                if (dry_weight < 0.005)
                                    dry_weight = 0;
                                qty = kv1.Value.Count;
                                totalweight = qty * dry_weight;
                            }
                            if (lxcomboBox1.Text.Equals("CQ-B3004-1909-12A压力管道材料表（国内）"))
                            {
                                s.Cells[row, 3] = materialNum;
                                s.Cells[row, 4] = ecname;
                                s.Cells[row, 5] = pipece_mare;
                                s.Cells[row, 6] = dn;
                                s.Cells[row, 7] = material;
                                s.Cells[row, 8] = unit;
                                s.Cells[row, 9] = Math.Round((decimal)qty, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 10] = Math.Round((decimal)dry_weight, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 11] = Math.Round((decimal)totalweight, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 12] = catalog_name;
                                s.Cells[row, 13] = media;
                                s.Cells[row, 14] = bz;
                            }
                            else
                            {
                                s.Cells[row, 3] = materialNum;
                                s.Cells[row, 5] = ecname;
                                s.Cells[row, 6] = pipece_mare;
                                s.Cells[row, 7] = dn;
                                s.Cells[row, 8] = material;
                                s.Cells[row, 9] = unit;
                                s.Cells[row, 10] = Math.Round((decimal)qty, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 11] = Math.Round((decimal)dry_weight, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 12] = Math.Round((decimal)totalweight, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 13] = catalog_name;
                                s.Cells[row, 14] = media;
                                s.Cells[row, 15] = bz;
                            }
                        }
                    }
                    wb.Save();

                    //app.Visible = true;

                    //wb.Close();
                    //wkb.Close();
                    //app.Quit();
                    ////wb = null;
                    ////wkb = null;
                    ////app = null;
                    ////GC.Collect();

                    //KillP.Kill(app); //杀死后台进程
                    //openPath = excelPath;
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                }
                waitDialog.Close();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app); //杀死后台进程
                openPath = excelPath;
            }
        }

        public void ExportExcel1(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\管道材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道材料表：下没有找到" + yName + "文件");
                    return;
                }
                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();


                //app = new MOIE.Application();
                //MOIE.Workbooks wkb = app.Workbooks;
                //MOIE.Workbook wb = wkb.Open(excelPath);

                //MOIE.Worksheet s;

                IWorkbook workbook;
                using (FileStream file = new FileStream(excelPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    workbook = new XSSFWorkbook(file);//xls有问题

                }


                //ISheet sheetmodel;
                ISheet sheet;
                if (lxcomboBox1.SelectedIndex == 1)
                {
                    sheet = workbook.GetSheet("管道材料表-海外");

                }
                else if (lxcomboBox1.SelectedIndex == 2)
                {
                    sheet = workbook.GetSheet("压力管道材料表-国内");
                }
                else
                {
                    sheet = workbook.GetSheet("管道材料表-国内");
                }
                if (itemList.Count > 0)
                {
                    int row = 5; //行号
                    int lineindex = 0; //序号

                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    {
                        row++;
                        lineindex++;
                        IRow dataRow = sheet.CreateRow(row); //第7行开始编辑
                        ICell cell7Row0Column = dataRow.CreateCell(0);
                        cell7Row0Column.SetCellValue(lineindex);
                        //s.Cells[row, 1] = lineindex;
                        bool biaoshi = true;//避免重复添加管线编号
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            if (biaoshi)
                            {
                                string pipeLine = " ";
                                if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null) pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                ICell cell7Row1Column = dataRow.CreateCell(1);
                                cell7Row1Column.SetCellValue(pipeNumber);
                                biaoshi = false;
                            }
                            row++;
                            string ecname = "";
                            string dn = "";
                            string unit = "";
                            double qty = 0;
                            string materialNum = "";
                            string pipece_mare = "";
                            string material = "";
                            double dry_weight = 0;
                            string catalog_name = "";
                            string media = "";
                            double totalweight = 0;
                            string bz = "";
                            ecname = kv1.Value[0].ClassDefinition.Name;
                            if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                            {
                                ecname = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                {
                                    ecname = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                            {
                                dn = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    dn = "DN" + kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                                }
                                //bool iszhij = api.InstanceDefinedAsClass(kv1.Value[0], "SUPPORT", true); //查找ec的父类是否含有SUPPORT
                                //if (iszhij) dn = " ";
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                materialNum = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                {
                                    materialNum = kv1.Value[0]["GRADE"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                            {
                                pipece_mare = kv1.Value[0]["CERI_PIECE_MARK"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("PIECE_MARK") != null)
                                {
                                    pipece_mare = kv1.Value[0]["PIECE_MARK"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                            {
                                material = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                {
                                    material = kv1.Value[0]["MATERIAL"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                            {
                                catalog_name = kv1.Value[0]["CERI_CATALOG"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                {
                                    catalog_name = kv1.Value[0]["CATALOG_NAME"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            {
                                media = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_NOTE") != null)
                            {
                                bz = kv1.Value[0]["CERI_NOTE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                {
                                    bz = kv1.Value[0]["NOTES"].StringValue;
                                }
                            }
                            bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                            if (b)
                            {
                                if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                {
                                    dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                    {
                                        dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                    }
                                }
                                if (dry_weight < 0.005)
                                    dry_weight = 0;
                                unit = "米";
                                double length = 0;
                                foreach (IECInstance iec2 in kv1.Value)
                                {
                                    double ll = iec2["LENGTH"].DoubleValue / 1000;
                                    if(ll< 1.7976931348623157E+12) length += ll;
                                    //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                }
                                qty = length;
                                totalweight = qty * dry_weight;
                            }
                            else
                            {
                                unit = "个";
                                if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                {
                                    dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                    {
                                        dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                    }
                                }
                                if (dry_weight < 0.005)
                                    dry_weight = 0;
                                qty = kv1.Value.Count;
                                totalweight = qty * dry_weight;
                            }
                            IRow nextRow = sheet.CreateRow(row);
                            if (lxcomboBox1.Text.Equals("CQ-B3004-1909-12A压力管道材料表（国内）"))
                            {
                                ICell cell2column = nextRow.CreateCell(2);
                                cell2column.SetCellValue(materialNum);
                                //s.Cells[row, 3] = materialNum;
                                ICell cell3column = nextRow.CreateCell(3);
                                cell3column.SetCellValue(ecname);
                                //s.Cells[row, 4] = ecname;
                                ICell cell4column = nextRow.CreateCell(4);
                                cell4column.SetCellValue(pipece_mare);
                                //s.Cells[row, 5] = pipece_mare;
                                ICell cell5column = nextRow.CreateCell(5);
                                cell5column.SetCellValue(dn);
                                //s.Cells[row, 6] = dn;
                                ICell cell6column = nextRow.CreateCell(6);
                                cell6column.SetCellValue(material);
                                //s.Cells[row, 7] = material;
                                ICell cell7column = nextRow.CreateCell(7);
                                cell7column.SetCellValue(unit);
                                //s.Cells[row, 8] = unit;
                                ICell cell8column = nextRow.CreateCell(8);
                                cell8column.SetCellValue(Math.Round(/*(decimal)*/qty, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 9] = Math.Round((decimal)qty, 2, MidpointRounding.AwayFromZero);
                                ICell cell9column = nextRow.CreateCell(9);
                                cell9column.SetCellValue(Math.Round(/*(decimal)*/dry_weight, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 10] = Math.Round((decimal)dry_weight, 2, MidpointRounding.AwayFromZero);
                                ICell cell10column = nextRow.CreateCell(10);
                                cell10column.SetCellValue(Math.Round(/*(decimal)*/totalweight, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 11] = Math.Round((decimal)totalweight, 2, MidpointRounding.AwayFromZero);
                                ICell cell11column = nextRow.CreateCell(11);
                                cell11column.SetCellValue(catalog_name);
                                //s.Cells[row, 12] = catalog_name;
                                ICell cell12column = nextRow.CreateCell(12);
                                cell12column.SetCellValue(media);
                                //s.Cells[row, 13] = media;
                                ICell cell13column = nextRow.CreateCell(13);
                                cell13column.SetCellValue(bz);
                                //s.Cells[row, 14] = bz;
                            }
                            else
                            {
                                ICell cell2column = nextRow.CreateCell(2);
                                cell2column.SetCellValue(materialNum);
                                //s.Cells[row, 3] = materialNum;
                                ICell cell4column = nextRow.CreateCell(4);
                                cell4column.SetCellValue(ecname);
                                //s.Cells[row, 5] = ecname;
                                ICell cell5column = nextRow.CreateCell(5);
                                cell5column.SetCellValue(pipece_mare);
                                //s.Cells[row, 6] = pipece_mare;
                                ICell cell6column = nextRow.CreateCell(6);
                                cell6column.SetCellValue(dn);
                                //s.Cells[row, 7] = dn;
                                ICell cell7column = nextRow.CreateCell(7);
                                cell7column.SetCellValue(material);
                                //s.Cells[row, 8] = material;
                                ICell cell8column = nextRow.CreateCell(8);
                                cell8column.SetCellValue(unit);
                                //s.Cells[row, 9] = unit;
                                ICell cell9column = nextRow.CreateCell(9);
                                cell9column.SetCellValue(Math.Round(/*(decimal)*/qty, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 10] = Math.Round((decimal)qty, 2, MidpointRounding.AwayFromZero);
                                ICell cell10column = nextRow.CreateCell(10);
                                cell10column.SetCellValue(Math.Round(/*(decimal)*/dry_weight, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 11] = Math.Round((decimal)dry_weight, 2, MidpointRounding.AwayFromZero);
                                ICell cell11column = nextRow.CreateCell(11);
                                cell11column.SetCellValue(Math.Round(/*(decimal)*/totalweight, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 12] = Math.Round((decimal)totalweight, 2, MidpointRounding.AwayFromZero);
                                ICell cell12column = nextRow.CreateCell(12);
                                cell12column.SetCellValue(catalog_name);
                                //s.Cells[row, 13] = catalog_name;
                                ICell cell13column = nextRow.CreateCell(13);
                                cell13column.SetCellValue(media);
                                //s.Cells[row, 14] = media;
                                ICell cell14column = nextRow.CreateCell(14);
                                cell14column.SetCellValue(bz);
                                //s.Cells[row, 15] = bz;
                            }
                        }
                    }
                    int kongbaihangCount = sheet.LastRowNum - (row);
                    if (kongbaihangCount > 0)
                    {
                        int m = 1;
                        int n = 3;
                        while (kongbaihangCount >= m * n)
                        {
                            sheet.ShiftRows(sheet.LastRowNum + 1, sheet.LastRowNum + n, -n);
                            m++;
                        }
                    }


                }
                //写入到客户端
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    workbook.Write(ms);
                    using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] data = ms.ToArray();
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                    }
                    waitDialog.Close();
                    workbook.Close();
                }
                //FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write);
                
                //workbook.Write(fs);
                //fs.Close();
                //waitDialog.Close();
                //workbook.Close();
                //waitDialog.Close();
                ////wb.Close();
                ////wkb.Close();
                //app.Quit();

                //KillP.Kill(app); //杀死后台进程
                openPath = excelPath;
            }
        }

        public void grExportExcel(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\隔热材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\隔热材料表：下没有找到" + yName + "文件");
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
                if (lxcomboBox1.SelectedIndex == 1)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道隔热材料表-海外"];
                }
                else if (lxcomboBox1.SelectedIndex == 2)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["压力管道隔热材料表-国内"];
                }
                else
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道隔热材料表-国内"];
                }
                if (itemList.Count > 0)
                {
                    int row = 7; //行号
                    int lineindex = 0; //序号
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    {
                        row++;
                        lineindex++;
                        s.Cells[row, 1] = lineindex;
                        bool biaoshi = true;//避免重复添加管线编号
                        //s.Cells[row, 2] = kv.Value.Values[0]["LINENUMBER"].StringValue;
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            if (biaoshi)
                            {
                                string pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                s.Cells[row, 2] = pipeNumber;
                                //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                biaoshi = false;
                            }
                            row++;
                            string jzmc = "", wd = "", dh = "", cz = "", bz = "";
                            double pod = 0, hd = 0, dlcd = 0, mj = 0, tj = 0;
                            int cs = 0;
                            #region
                            //if (iec.GetPropertyValue("CERI_Media_Name") != null)
                            //{
                            //    jzmc = iec["CERI_Media_Name"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_PIPE_OD_M") != null)
                            //{
                            //    pod = iec["CERI_PIPE_OD_M"].DoubleValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("OUTSIDE_DIAMETER") != null)
                            //    {
                            //        pod = iec["OUTSIDE_DIAMETER"].DoubleValue;
                            //    }
                            //}
                            //if (iec.GetPropertyValue("CERI_TEMP") != null)
                            //{
                            //    wd = iec["CERI_TEMP"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_code") != null)
                            //{
                            //    dh = iec["CERI_Insulation_code"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_Materia") != null)
                            //{
                            //    cz = iec["CERI_Insulation_Materia"].StringValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("MATERIAL") != null)
                            //    {
                            //        cz = iec["MATERIAL"].StringValue;
                            //    }
                            //}
                            //if (iec.GetPropertyValue("CERI_Layer") != null)
                            //{
                            //    cs = iec["CERI_Layer"].IntValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_Thickness") != null)
                            //{
                            //    hd = iec["CERI_Insulation_Thickness"].DoubleValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("INSULATION_THICKNESS") != null)
                            //    {
                            //        hd = iec["INSULATION_THICKNESS"].DoubleValue;
                            //    }
                            //}
                            #endregion
                            if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            {
                                jzmc = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_PIPE_OD_M") != null)
                            {
                                pod = kv1.Value[0]["CERI_PIPE_OD_M"].DoubleValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("OUTSIDE_DIAMETER") != null)
                                {
                                    pod = kv1.Value[0]["OUTSIDE_DIAMETER"].DoubleValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_TEMP") != null)
                            {
                                wd = kv1.Value[0]["CERI_TEMP"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_code") != null)
                            {
                                dh = kv1.Value[0]["CERI_Insulation_code"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Material") != null)
                            {
                                cz = kv1.Value[0]["CERI_Insulation_Material"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("INSULATION") != null)
                                {
                                    cz = kv1.Value[0]["INSULATION"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Layer") != null)
                            {
                                cs = kv1.Value[0]["CERI_Layer"].IntValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                            {
                                hd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                                {
                                    hd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                                }
                            }
                            foreach (IECInstance iec1 in kv1.Value)
                            {
                                if (iec1.GetPropertyValue("CERI_LENGTH") != null)
                                {
                                    dlcd += iec1["CERI_LENGTH"].DoubleValue;
                                }
                                else
                                {
                                    if (iec1.GetPropertyValue("LENGTH") != null)
                                    {
                                        dlcd += iec1["LENGTH"].DoubleValue / 1000;
                                    }
                                }
                                if (iec1.GetPropertyValue("CERI_Area") != null)
                                {
                                    mj += iec1["CERI_Area"].DoubleValue;
                                }
                                if (iec1.GetPropertyValue("CERI_Volume") != null)
                                {
                                    tj += iec1["CERI_Volume"].DoubleValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                            {
                                bz = kv1.Value[0]["NOTES"].StringValue;
                            }
                            #region
                            //string ecname = "";
                            //string dn = "";
                            //string unit = "";
                            //double qty = 0;
                            //string materialNum = "";
                            //string pipece_mare = "";
                            //string material = "";
                            //double dry_weight = 0;
                            //string catalog_name = "";
                            //string media = "";
                            //double totalweight = 0;
                            //ecname = kv1.Value[0].ClassDefinition.Name;
                            //if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                            //{
                            //    dn = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                            //    {
                            //        dn = "DN" + kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            //{
                            //    materialNum = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                            //    {
                            //        materialNum = kv1.Value[0]["GRADE"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                            //{
                            //    pipece_mare = kv1.Value[0]["CERI_PIECE_MARK"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("PIECE_MARK") != null)
                            //    {
                            //        pipece_mare = kv1.Value[0]["PIECE_MARK"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                            //{
                            //    material = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                            //    {
                            //        material = kv1.Value[0]["MATERIAL"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                            //{
                            //    catalog_name = kv1.Value[0]["CERI_CATALOG"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                            //    {
                            //        catalog_name = kv1.Value[0]["CATALOG_NAME"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            //{
                            //    media = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            //}

                            //bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                            //if (b)
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                            //    {
                            //        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue * 1000;
                            //    }
                            //    else
                            //    {
                            //        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                            //        {
                            //            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                            //        }
                            //    }
                            //    unit = "米";
                            //    double length = 0;
                            //    foreach (IECInstance iec2 in kv1.Value)
                            //    {
                            //        length += iec2["LENGTH"].DoubleValue / 1000;
                            //        //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                            //    }
                            //    qty = length;
                            //    totalweight = qty * dry_weight;
                            //}
                            //else
                            //{
                            //    unit = "个";
                            //    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                            //    {
                            //        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                            //    }
                            //    else
                            //    {
                            //        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                            //        {
                            //            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                            //        }
                            //    }
                            //    qty = kv1.Value.Count;
                            //    totalweight = qty * dry_weight;
                            //}
                            #endregion
                            s.Cells[row, 3] = jzmc;
                            s.Cells[row, 4] = pod;
                            s.Cells[row, 5] = Math.Round((decimal)dlcd, 2, MidpointRounding.AwayFromZero);
                            s.Cells[row, 6] = wd;
                            s.Cells[row, 7] = dh;
                            s.Cells[row, 8] = cz;
                            s.Cells[row, 10] = cs;
                            s.Cells[row, 11] = hd;
                            s.Cells[row, 12] = Math.Round((decimal)mj, 2, MidpointRounding.AwayFromZero);
                            s.Cells[row, 13] = Math.Round((decimal)tj, 2, MidpointRounding.AwayFromZero);
                            s.Cells[row, 14] = bz;
                            //s.Cells[row, 13] = catalog_name;
                            //s.Cells[row, 14] = media;
                        }
                    }
                    wb.Save();

                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                }
                waitDialog.Close();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app);
                openPath = excelPath;
            }
        }

        public void grExportExcel1(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\隔热材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\隔热材料表：下没有找到" + yName + "文件");
                    return;
                }
                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();

                //app = new MOIE.Application();
                //MOIE.Workbooks wkb = app.Workbooks;
                //MOIE.Workbook wb = wkb.Open(excelPath);
                //app.Visible = true;
                //wb=wkb.Open(excelPath1);
                //MOIE.Worksheet s;
                //HSSFWorkbook workbook = new HSSFWorkbook();
                IWorkbook workbook;
                using (FileStream file = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new XSSFWorkbook(file);//xls有问题

                }
                ISheet sheet;
                if (lxcomboBox1.SelectedIndex == 1)
                {
                    //s = (MOIE.Worksheet)wb.Worksheets["管道隔热材料表-海外"];
                    sheet = workbook.GetSheet("管道隔热材料表-海外");
                }
                else if (lxcomboBox1.SelectedIndex == 2)
                {
                    //s = (MOIE.Worksheet)wb.Worksheets["压力管道隔热材料表-国内"];
                    sheet = workbook.GetSheet("压力管道隔热材料表-国内");
                }
                else
                {
                    //s = (MOIE.Worksheet)wb.Worksheets["管道隔热材料表-国内"];
                    sheet = workbook.GetSheet("管道隔热材料表-国内");
                }
                if (itemList.Count > 0)
                {
                    int row = 6; //行号
                    int lineindex = 0; //序号
                    foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    {
                        row++;
                        lineindex++;
                        IRow dataRow = sheet.CreateRow(row); //第7行开始编辑
                        ICell cell7Row0Column = dataRow.CreateCell(0);
                        cell7Row0Column.SetCellValue(lineindex);
                        //s.Cells[row, 1] = lineindex;
                        bool biaoshi = true;//避免重复添加管线编号
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            if (biaoshi)
                            {
                                string pipeLine = " ";
                                if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null) pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);

                                ICell cell7Row1Column = dataRow.CreateCell(1);
                                cell7Row1Column.SetCellValue(pipeNumber);
                                //s.Cells[row, 2] = pipeNumber;
                                //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                biaoshi = false;
                            }
                            row++;
                            string jzmc = "", wd = "", dh = "", cz = "", bz = "";
                            double pod = 0, hd = 0, dlcd = 0, mj = 0, tj = 0;
                            int cs = 0;
                            #region
                            //if (iec.GetPropertyValue("CERI_Media_Name") != null)
                            //{
                            //    jzmc = iec["CERI_Media_Name"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_PIPE_OD_M") != null)
                            //{
                            //    pod = iec["CERI_PIPE_OD_M"].DoubleValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("OUTSIDE_DIAMETER") != null)
                            //    {
                            //        pod = iec["OUTSIDE_DIAMETER"].DoubleValue;
                            //    }
                            //}
                            //if (iec.GetPropertyValue("CERI_TEMP") != null)
                            //{
                            //    wd = iec["CERI_TEMP"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_code") != null)
                            //{
                            //    dh = iec["CERI_Insulation_code"].StringValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_Materia") != null)
                            //{
                            //    cz = iec["CERI_Insulation_Materia"].StringValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("MATERIAL") != null)
                            //    {
                            //        cz = iec["MATERIAL"].StringValue;
                            //    }
                            //}
                            //if (iec.GetPropertyValue("CERI_Layer") != null)
                            //{
                            //    cs = iec["CERI_Layer"].IntValue;
                            //}
                            //if (iec.GetPropertyValue("CERI_Insulation_Thickness") != null)
                            //{
                            //    hd = iec["CERI_Insulation_Thickness"].DoubleValue;
                            //}
                            //else
                            //{
                            //    if (iec.GetPropertyValue("INSULATION_THICKNESS") != null)
                            //    {
                            //        hd = iec["INSULATION_THICKNESS"].DoubleValue;
                            //    }
                            //}
                            #endregion
                            if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            {
                                jzmc = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_PIPE_OD_M") != null)
                            {
                                pod = kv1.Value[0]["CERI_PIPE_OD_M"].DoubleValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("OUTSIDE_DIAMETER") != null)
                                {
                                    pod = kv1.Value[0]["OUTSIDE_DIAMETER"].DoubleValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_TEMP") != null)
                            {
                                wd = kv1.Value[0]["CERI_TEMP"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_code") != null)
                            {
                                dh = kv1.Value[0]["CERI_Insulation_code"].StringValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Material") != null)
                            {
                                cz = kv1.Value[0]["CERI_Insulation_Material"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("INSULATION") != null)
                                {
                                    cz = kv1.Value[0]["INSULATION"].StringValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Layer") != null)
                            {
                                cs = kv1.Value[0]["CERI_Layer"].IntValue;
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                            {
                                hd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                                {
                                    hd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                                }
                            }
                            foreach (IECInstance iec1 in kv1.Value)
                            {
                                if (iec1.GetPropertyValue("CERI_LENGTH") != null)
                                {
                                    dlcd += iec1["CERI_LENGTH"].DoubleValue;
                                }
                                else
                                {
                                    if (iec1.GetPropertyValue("LENGTH") != null)
                                    {
                                        dlcd += iec1["LENGTH"].DoubleValue / 1000;
                                    }
                                }
                                if (iec1.GetPropertyValue("CERI_Area") != null)
                                {
                                    mj += iec1["CERI_Area"].DoubleValue;
                                }
                                if (iec1.GetPropertyValue("CERI_Volume") != null)
                                {
                                    tj += iec1["CERI_Volume"].DoubleValue;
                                }
                            }
                            if (kv1.Value[0].GetPropertyValue("CERI_NOTE") != null)
                            {
                                bz = kv1.Value[0]["CERI_NOTE"].StringValue;
                            }
                            else
                            {
                                if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                {
                                    bz = kv1.Value[0]["NOTES"].StringValue;
                                }
                            }
                            #region
                            //string ecname = "";
                            //string dn = "";
                            //string unit = "";
                            //double qty = 0;
                            //string materialNum = "";
                            //string pipece_mare = "";
                            //string material = "";
                            //double dry_weight = 0;
                            //string catalog_name = "";
                            //string media = "";
                            //double totalweight = 0;
                            //ecname = kv1.Value[0].ClassDefinition.Name;
                            //if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                            //{
                            //    dn = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("NOMINAL_DIAMETER") != null)
                            //    {
                            //        dn = "DN" + kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            //{
                            //    materialNum = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                            //    {
                            //        materialNum = kv1.Value[0]["GRADE"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_PIECE_MARK") != null)
                            //{
                            //    pipece_mare = kv1.Value[0]["CERI_PIECE_MARK"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("PIECE_MARK") != null)
                            //    {
                            //        pipece_mare = kv1.Value[0]["PIECE_MARK"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                            //{
                            //    material = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                            //    {
                            //        material = kv1.Value[0]["MATERIAL"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                            //{
                            //    catalog_name = kv1.Value[0]["CERI_CATALOG"].StringValue;
                            //}
                            //else
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                            //    {
                            //        catalog_name = kv1.Value[0]["CATALOG_NAME"].StringValue;
                            //    }
                            //}
                            //if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                            //{
                            //    media = kv1.Value[0]["CERI_Media_Name"].StringValue;
                            //}

                            //bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                            //if (b)
                            //{
                            //    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                            //    {
                            //        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue * 1000;
                            //    }
                            //    else
                            //    {
                            //        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                            //        {
                            //            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                            //        }
                            //    }
                            //    unit = "米";
                            //    double length = 0;
                            //    foreach (IECInstance iec2 in kv1.Value)
                            //    {
                            //        length += iec2["LENGTH"].DoubleValue / 1000;
                            //        //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                            //    }
                            //    qty = length;
                            //    totalweight = qty * dry_weight;
                            //}
                            //else
                            //{
                            //    unit = "个";
                            //    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                            //    {
                            //        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                            //    }
                            //    else
                            //    {
                            //        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                            //        {
                            //            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                            //        }
                            //    }
                            //    qty = kv1.Value.Count;
                            //    totalweight = qty * dry_weight;
                            //}
                            #endregion
                            IRow nextRow = sheet.CreateRow(row);
                            ICell cell2column = nextRow.CreateCell(2);
                            cell2column.SetCellValue(jzmc);
                            //s.Cells[row, 3] = jzmc;
                            ICell cell3column = nextRow.CreateCell(3);
                            cell3column.SetCellValue(pod);
                            //s.Cells[row, 4] = pod;
                            ICell cell4column = nextRow.CreateCell(4);
                            cell4column.SetCellValue(Math.Round(/*(decimal)*/dlcd, 2, MidpointRounding.AwayFromZero));
                            //s.Cells[row, 5] = Math.Round((decimal)dlcd, 2, MidpointRounding.AwayFromZero);
                            ICell cell5column = nextRow.CreateCell(5);
                            cell5column.SetCellValue(wd);
                            //s.Cells[row, 6] = wd;
                            ICell cell6column = nextRow.CreateCell(6);
                            cell6column.SetCellValue(dh);
                            //s.Cells[row, 7] = dh;
                            ICell cell7column = nextRow.CreateCell(7);
                            cell7column.SetCellValue(cz);
                            //s.Cells[row, 8] = cz;
                            ICell cell9column = nextRow.CreateCell(9);
                            cell9column.SetCellValue(cs);
                            //s.Cells[row, 10] = cs;
                            ICell cell10column = nextRow.CreateCell(10);
                            cell10column.SetCellValue(hd);
                            //s.Cells[row, 11] = hd;
                            ICell cell11column = nextRow.CreateCell(11);
                            cell11column.SetCellValue(Math.Round(/*(decimal)*/mj, 2, MidpointRounding.AwayFromZero));
                            //s.Cells[row, 12] = Math.Round((decimal)mj, 2, MidpointRounding.AwayFromZero);
                            ICell cell12column = nextRow.CreateCell(12);
                            cell12column.SetCellValue(Math.Round(/*(decimal)*/tj, 2, MidpointRounding.AwayFromZero));
                            //s.Cells[row, 13] = Math.Round((decimal)tj, 2, MidpointRounding.AwayFromZero);
                            ICell cell13column = nextRow.CreateCell(13);
                            cell13column.SetCellValue(bz);
                            //s.Cells[row, 14] = bz;


                            //s.Cells[row, 13] = catalog_name;
                            //s.Cells[row, 14] = media;
                        }
                    }
                    int kongbaihangCount = sheet.LastRowNum - (row);
                    if (kongbaihangCount > 0)
                    {
                        int m = 1;
                        int n = 2;
                        while (kongbaihangCount >= m * n)
                        {
                            sheet.ShiftRows(sheet.LastRowNum + 1, sheet.LastRowNum + n, -n);
                            m++;
                        }
                    }
                    //wb.Save();

                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                }
                //waitDialog.Close();
                //wb.Close();
                //wkb.Close();
                //app.Quit();

                //KillP.Kill(app);
                openPath = excelPath;
                // 写入到客户端 
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    workbook.Write(ms);
                    using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] data = ms.ToArray();
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                    }
                    waitDialog.Close();
                    workbook.Close();
                }
            }
        }

        public void zjExportExcel(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                bool isHz = lxcomboBox1.Text.Equals("管道造价汇总表");
                try
                {
                    if (isHz)
                    {
                        File.Copy(path0 + "\\JYXConfig\\管道造价汇总表\\" + yName, excelPath);
                    }
                    else
                    {
                        File.Copy(path0 + "\\JYXConfig\\管道造价表\\" + yName, excelPath);
                    }

                }
                catch
                {
                    if (isHz)
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道造价汇总表：下没有找到" + yName + "文件");
                    }
                    else
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道造价表：下没有找到" + yName + "文件");
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
                if (isHz)
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道汇总表"];
                }
                else
                {
                    s = (MOIE.Worksheet)wb.Worksheets["管道明细表"];
                }

                if (itemList.Count > 0)
                {
                    //给s填值就行
                    #region 汇总表
                    if (isHz)
                    {
                        int row = 3, lineindex = 0;
                        #region 按物料编码分组
                        if (wlbmGroupcheckBox1.Checked)
                        {
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                            {
                                row++;
                                lineindex++;
                                s.Cells[row, 1] = lineindex;
                                bool biaoshi = true;
                                //int irow = 0;
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                                {
                                    int irow = 0;
                                    double xNum = 0, xzz = 0, zgNum = 0, zgzz = 0;
                                    if (biaoshi)
                                    {
                                        string pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                        string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                        s.Cells[row, 2] = pipeNumber;
                                        //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                        biaoshi = false;
                                    }
                                    irow = row + 1;
                                    string wlbm3 = "";
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                    {
                                        wlbm3 = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                        {
                                            wlbm3 = kv1.Value[0]["GRADE"].StringValue;
                                        }
                                    }
                                    bool isKW = false;
                                    if (wlbm3 == "" || wlbm3 == " ")
                                        isKW = true;
                                    if (isKW)
                                    {
                                        string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "";
                                        if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                        {
                                            xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                            {
                                                xtbh = kv1.Value[0]["UNIT"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                        {
                                            xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("SERVICE") != null)
                                            {
                                                xtmc = kv1.Value[0]["SERVICE"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                        {
                                            clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                            {
                                                clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                        {
                                            ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                        {
                                            gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                            {
                                                gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                        {
                                            bz = kv1.Value[0]["NOTES"].StringValue;
                                        }
                                        double zjsl = 0;
                                        zjsl = zjshulian(kv1.Value);

                                        string dw = "";
                                        double dz = 0;
                                        bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                                        if (b)
                                        {
                                            double dry_weight = 0;
                                            if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                            {
                                                dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                                if (dry_weight < 0.005)
                                                    dry_weight = 0;
                                                dz = dry_weight;
                                            }
                                            else
                                            {
                                                if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                                {
                                                    dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                    if (dry_weight < 0.005)
                                                        dry_weight = 0;
                                                    dz = dry_weight;
                                                }
                                            }
                                            dw = "米";
                                        }
                                        else
                                        {
                                            double dry_weight = 0;
                                            if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                            {
                                                dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                            }
                                            else
                                            {
                                                if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                                {
                                                    dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                }
                                            }
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                            dw = "个";
                                            xzz = dz * kv1.Value.Count;
                                        }
                                        foreach (IECInstance iecs in kv1.Value)
                                        {
                                            string xN = "0";
                                            if (iecs.GetPropertyValue("CERI_Centerline_Meter") != null)
                                            {
                                                xN = iecs["CERI_Centerline_Meter"].StringValue;
                                                if (xN != "" && xN != null)
                                                {
                                                    xN = xN.Substring(0, xN.IndexOf("m"));
                                                    double xn = Convert.ToDouble(xN);
                                                    xNum += xn;
                                                }

                                            }
                                            else
                                            {
                                                if (iecs.GetPropertyValue("LENGTH") != null)
                                                {
                                                    xNum += iecs["LENGTH"].DoubleValue / 1000;
                                                }
                                            }

                                            if (b)
                                            {
                                                //unit = "米";
                                                double length = 0;
                                                length += iecs["LENGTH"].DoubleValue / 1000;
                                                //qty = length;
                                                double totalweight = length * dz;
                                                zgNum += length;
                                                xzz += totalweight;
                                                zgzz += totalweight;
                                            }
                                        }
                                        row++;
                                        s.Cells[row, 3] = xtbh;
                                        s.Cells[row, 4] = xtmc;
                                        s.Cells[row, 5] = clmc;
                                        s.Cells[row, 6] = ggxh;
                                        s.Cells[row, 7] = gdcz;
                                        s.Cells[row, 8] = zjsl;
                                        s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                        s.Cells[row, 10] = dw;
                                        s.Cells[row, 15] = bz;
                                        s.Cells[row, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                        s.Cells[row, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                        s.Cells[row, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                        s.Cells[row, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                        s.Cells[row, 16] = wlbm3;
                                    }
                                    else
                                    {
                                        Dictionary<int, List<IECInstance>> zjDicList = zjwlHz(kv1.Value);
                                        #region 物料编码不为空 以前处理方式
                                        for (int j = irow + 1; j < irow + zjDicList.Count; j++) //需要改
                                        {
                                            MOIE.Range range = s.Range[s.Cells[irow, 11], s.Cells[j, 11]];
                                            range.Application.DisplayAlerts = false;
                                            range.Merge(Type.Missing);
                                            range.Application.DisplayAlerts = true;
                                            MOIE.Range range1 = s.Range[s.Cells[irow, 12], s.Cells[j, 12]];
                                            range1.Application.DisplayAlerts = false;
                                            range1.Merge(Type.Missing);
                                            range1.Application.DisplayAlerts = true;
                                            MOIE.Range range2 = s.Range[s.Cells[irow, 13], s.Cells[j, 13]];
                                            range2.Application.DisplayAlerts = false;
                                            range2.Merge(Type.Missing);
                                            range2.Application.DisplayAlerts = true;
                                            MOIE.Range range3 = s.Range[s.Cells[irow, 14], s.Cells[j, 14]];
                                            range3.Application.DisplayAlerts = false;
                                            range3.Merge(Type.Missing);
                                            range3.Application.DisplayAlerts = true;
                                            MOIE.Range range4 = s.Range[s.Cells[irow, 16], s.Cells[j, 16]];
                                            range4.Application.DisplayAlerts = false;
                                            range4.Merge(Type.Missing);
                                            range4.Application.DisplayAlerts = true;
                                        }
                                        #region 改
                                        foreach (KeyValuePair<int, List<IECInstance>> kv2 in zjDicList)
                                        {
                                            row++;
                                            //irow = row;                                        
                                            string xN = "0";
                                            string dw = "";
                                            double dz = 0;
                                            foreach (IECInstance iec in kv2.Value)
                                            {
                                                if (iec.GetPropertyValue("CERI_Centerline_Meter") != null)
                                                {
                                                    xN = iec["CERI_Centerline_Meter"].StringValue;
                                                    if (xN != "" && xN != null)
                                                    {
                                                        xN = xN.Substring(0, xN.IndexOf("m"));
                                                        double xn = Convert.ToDouble(xN);
                                                        xNum += xn;
                                                    }

                                                }
                                                else
                                                {
                                                    if (iec.GetPropertyValue("LENGTH") != null)
                                                    {
                                                        xNum += iec["LENGTH"].DoubleValue / 1000;
                                                    }
                                                }

                                                bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                                                if (b)
                                                {
                                                    double dry_weight = 0;
                                                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                                    {
                                                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue;
                                                        if (dry_weight < 0.005)
                                                            dry_weight = 0;
                                                        dz = dry_weight;
                                                    }
                                                    else
                                                    {
                                                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                                                        {
                                                            dry_weight = iec["DRY_WEIGHT"].DoubleValue;
                                                            if (dry_weight < 0.005)
                                                                dry_weight = 0;
                                                            dz = dry_weight;
                                                        }
                                                    }
                                                    dw = "米";
                                                    //unit = "米";
                                                    double length = 0;
                                                    length += iec["LENGTH"].DoubleValue / 1000;
                                                    //qty = length;
                                                    double totalweight = length * dry_weight;
                                                    zgNum += length;
                                                    xzz += totalweight;
                                                    zgzz += totalweight;
                                                }
                                                else
                                                {
                                                    double dry_weight = 0;
                                                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                                    {
                                                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue;
                                                    }
                                                    else
                                                    {
                                                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                                                        {
                                                            dry_weight = iec["DRY_WEIGHT"].DoubleValue;
                                                        }
                                                    }
                                                    if (dry_weight < 0.005)
                                                        dry_weight = 0;
                                                    dz = dry_weight;
                                                    dw = "个";
                                                    xzz += dry_weight;
                                                }
                                            }
                                            string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "";
                                            if (kv2.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                            {
                                                xtbh = kv2.Value[0]["CERI_System_Number"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("UNIT") != null)
                                                {
                                                    xtbh = kv2.Value[0]["UNIT"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                            {
                                                xtmc = kv2.Value[0]["CERI_System_Name"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("SERVICE") != null)
                                                {
                                                    xtmc = kv2.Value[0]["SERVICE"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                            {
                                                clmc = kv2.Value[0]["CERI_SHORT_DESC"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                                {
                                                    clmc = kv2.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                            {
                                                ggxh = kv2.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                            {
                                                gdcz = kv2.Value[0]["CERI_MATERIAL"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("MATERIAL") != null)
                                                {
                                                    gdcz = kv2.Value[0]["MATERIAL"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("NOTES") != null)
                                            {
                                                bz = kv2.Value[0]["NOTES"].StringValue;
                                            }
                                            double zjsl = 0;
                                            zjsl = zjshulian(kv2.Value);
                                            s.Cells[row, 3] = xtbh;
                                            s.Cells[row, 4] = xtmc;
                                            s.Cells[row, 5] = clmc;
                                            s.Cells[row, 6] = ggxh;
                                            s.Cells[row, 7] = gdcz;
                                            s.Cells[row, 8] = zjsl;
                                            s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                            s.Cells[row, 10] = dw;
                                            s.Cells[row, 15] = bz;
                                        }
                                        #endregion
                                        string wlbm = "";
                                        if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                        {
                                            wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                            {
                                                wlbm = kv1.Value[0]["GRADE"].StringValue;
                                            }
                                        }
                                        s.Cells[irow, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                        MOIE.Range r = s.Cells[irow, 11];
                                        r.NumberFormatLocal = "@";
                                        s.Cells[irow, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                        MOIE.Range r1 = s.Cells[irow, 12];
                                        r1.NumberFormatLocal = "@";
                                        s.Cells[irow, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                        MOIE.Range r2 = s.Cells[irow, 13];
                                        r2.NumberFormatLocal = "@";
                                        s.Cells[irow, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                        MOIE.Range r3 = s.Cells[irow, 14];
                                        r3.NumberFormatLocal = "@";
                                        s.Cells[irow, 16] = wlbm;
                                        MOIE.Range r4 = s.Cells[irow, 16];
                                        r4.NumberFormatLocal = "@";
                                        #endregion
                                    }

                                }
                            }
                        }
                        #endregion
                        else
                        {
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                            {
                                row++;
                                lineindex++;
                                s.Cells[row, 1] = lineindex;
                                bool biaoshi = true;
                                int irow = 0;
                                double xNum = 0, xzz = 0, zgNum = 0, zgzz = 0;
                                #region
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                                {
                                    if (biaoshi)
                                    {
                                        string pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                        string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                        s.Cells[row, 2] = pipeNumber;
                                        //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                        biaoshi = false;
                                        irow = row + 1;
                                        for (int j = irow + 1; j < irow + kv.Value.Count; j++)
                                        {
                                            MOIE.Range range = s.Range[s.Cells[irow, 10], s.Cells[j, 10]];
                                            range.Application.DisplayAlerts = false;
                                            range.Merge(Type.Missing);
                                            range.Application.DisplayAlerts = true;
                                            MOIE.Range range1 = s.Range[s.Cells[irow, 11], s.Cells[j, 11]];
                                            range1.Application.DisplayAlerts = false;
                                            range1.Merge(Type.Missing);
                                            range1.Application.DisplayAlerts = true;
                                            MOIE.Range range2 = s.Range[s.Cells[irow, 12], s.Cells[j, 12]];
                                            range2.Application.DisplayAlerts = false;
                                            range2.Merge(Type.Missing);
                                            range2.Application.DisplayAlerts = true;
                                            MOIE.Range range3 = s.Range[s.Cells[irow, 13], s.Cells[j, 13]];
                                            range3.Application.DisplayAlerts = false;
                                            range3.Merge(Type.Missing);
                                            range3.Application.DisplayAlerts = true;
                                        }
                                    }
                                    row++;
                                    string xN = "0";
                                    if (kv1.Value[0].GetPropertyValue("CERI_Centerline_Meter") != null)
                                    {
                                        xN = kv1.Value[0]["CERI_Centerline_Meter"].StringValue;
                                        if (xN != "" && xN != null)
                                        {
                                            xN = xN.Substring(0, xN.IndexOf("m"));
                                            double xn = Convert.ToDouble(xN);
                                            xNum += xn;
                                        }

                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("LENGTH") != null)
                                        {
                                            xNum += kv1.Value[0]["LENGTH"].DoubleValue / 1000;
                                        }
                                    }

                                    string dw = "";
                                    double dz = 0, sl = 0;
                                    bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                                    if (b)
                                    {
                                        double dry_weight = 0;
                                        if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                if (dry_weight < 0.005)
                                                    dry_weight = 0;
                                                dz = dry_weight;
                                            }
                                        }
                                        dw = "米";
                                        //unit = "米";
                                        double length = 0;
                                        foreach (IECInstance iec2 in kv1.Value)
                                        {
                                            length += iec2["LENGTH"].DoubleValue / 1000;
                                            //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                        }
                                        //qty = length;
                                        double totalweight = length * dry_weight;
                                        zgNum += length;
                                        xzz += totalweight;
                                        zgzz += totalweight;
                                    }
                                    else
                                    {
                                        double dry_weight = 0;
                                        if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            }
                                        }
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                        dw = "个";
                                        xzz += dry_weight;
                                    }
                                    string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "", wlbm = "";
                                    if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                    {
                                        xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                        {
                                            xtbh = kv1.Value[0]["UNIT"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                    {
                                        xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                    {
                                        clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                        {
                                            clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                    {
                                        ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                    {
                                        gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                        {
                                            gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                    {
                                        bz = kv1.Value[0]["NOTES"].StringValue;
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                    {
                                        wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                        {
                                            wlbm = kv1.Value[0]["GRADE"].StringValue;
                                        }
                                    }
                                    sl = zjshulian(kv1.Value);
                                    s.Cells[row, 3] = xtbh;
                                    s.Cells[row, 4] = xtmc;
                                    s.Cells[row, 5] = clmc;
                                    s.Cells[row, 6] = ggxh;
                                    s.Cells[row, 7] = gdcz;
                                    s.Cells[row, 8] = sl;
                                    s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                    s.Cells[row, 10] = dw;
                                    s.Cells[row, 15] = bz;
                                    s.Cells[row, 16] = wlbm;
                                }
                                s.Cells[irow, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                MOIE.Range r = s.Cells[irow, 10];
                                r.NumberFormatLocal = "@";
                                s.Cells[irow, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                MOIE.Range r1 = s.Cells[irow, 11];
                                r1.NumberFormatLocal = "@";
                                s.Cells[irow, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                MOIE.Range r2 = s.Cells[irow, 12];
                                r2.NumberFormatLocal = "@";
                                s.Cells[irow, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                MOIE.Range r3 = s.Cells[irow, 13];
                                r3.NumberFormatLocal = "@";
                                #endregion
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        int row = 3, lineindex = 0;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            row++;
                            lineindex++;
                            s.Cells[row, 1] = lineindex;
                            bool biaoshi = true;
                            int irow = 0;
                            double min = double.MaxValue, max = double.MinValue;
                            #region
                            foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                            {
                                if (biaoshi)
                                {
                                    //s.get_Range(s.Cells[5, 5], s.Cells[5, 6]).Merge(Type.Missing);
                                    string pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                    string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                    s.Cells[row, 2] = pipeNumber;
                                    //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                    biaoshi = false;
                                    irow = row + 1;
                                    for (int j = irow + 1; j < irow + kv.Value.Count; j++)
                                    {
                                        try
                                        {

                                            MOIE.Range range = s.Range[s.Cells[irow, 15], s.Cells[j, 15]];
                                            range.Application.DisplayAlerts = false;
                                            range.Merge(Type.Missing);
                                            range.Application.DisplayAlerts = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            string e = ex.ToString();
                                        }
                                        MOIE.Range range1 = s.Range[s.Cells[irow, 16], s.Cells[j, 16]];
                                        range1.Application.DisplayAlerts = false;
                                        range1.Merge(Type.Missing);
                                        range1.Application.DisplayAlerts = true;
                                        //MOIE.Range range2 = s.Range[s.Cells[irow, 20], s.Cells[j, 20]];
                                        //range2.Application.DisplayAlerts = false;
                                        //range2.Merge(Type.Missing);
                                        //range2.Application.DisplayAlerts = true;
                                        //MOIE.Range range3 = s.Range[s.Cells[irow, 21], s.Cells[j, 21]];
                                        //range3.Application.DisplayAlerts = false;
                                        //range3.Merge(Type.Missing);
                                        //range3.Application.DisplayAlerts = true;
                                        //MOIE.Range range4 = s.Range[s.Cells[irow, 22], s.Cells[j, 22]];
                                        //range4.Application.DisplayAlerts = false;
                                        //range4.Merge(Type.Missing);
                                        //range4.Application.DisplayAlerts = true;
                                        //MOIE.Range range5 = s.Range[s.Cells[irow, 23], s.Cells[j, 23]];
                                        //range5.Application.DisplayAlerts = false;
                                        //range5.Merge(Type.Missing);
                                        //range5.Application.DisplayAlerts = true;
                                    }
                                }
                                double zgNum = 0, zgzz = 0, xNum = 0, xzz = 0;
                                row++;

                                bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);

                                foreach (IECInstance iecs in kv1.Value)
                                {
                                    string pMin = "0", pMax = "0";
                                    if (b && iecs.GetPropertyValue("CERI_Lowest_Center_Elevation_Pipe") != null)
                                    {
                                        pMin = iecs["CERI_Lowest_Center_Elevation_Pipe"].StringValue;
                                        if (pMin != null && pMin != "")
                                        {
                                            pMin = pMin.Substring(0, pMin.IndexOf("m"));
                                            double pmin = Convert.ToDouble(pMin);
                                            if (pmin <= min)
                                            {
                                                min = pmin;
                                            }
                                        }
                                    }
                                    if (b && iecs.GetPropertyValue("CERI_Topest_Center_Elevation_Pipe") != null)
                                    {
                                        pMax = iecs["CERI_Topest_Center_Elevation_Pipe"].StringValue;
                                        if (pMax != "" && pMax != null)
                                        {
                                            pMax = pMax.Substring(0, pMax.IndexOf("m"));
                                            double pmax = Convert.ToDouble(pMax);
                                            if (pmax >= max)
                                            {
                                                max = pmax;
                                            }
                                        }

                                    }
                                    string xN = "0";
                                    if (iecs.GetPropertyValue("CERI_Centerline_Meter") != null)
                                    {
                                        xN = iecs["CERI_Centerline_Meter"].StringValue;
                                        if (xN != "" && xN != null)
                                        {
                                            xN = xN.Substring(0, xN.IndexOf("m"));
                                            double xn = Convert.ToDouble(xN);
                                            xNum += xn;
                                        }

                                    }
                                    else
                                    {
                                        if (iecs.GetPropertyValue("LENGTH") != null)
                                        {
                                            xNum += iecs["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                }

                                //if (b && kv1.Value[0].GetPropertyValue("CERI_Lowest_Center_Elevation_Pipe") != null)
                                //{
                                //    pMin = kv1.Value[0]["CERI_Lowest_Center_Elevation_Pipe"].StringValue;
                                //    if (pMin != null && pMin != "")
                                //    {
                                //        pMin = pMin.Substring(0, pMin.IndexOf("m"));
                                //        double pmin = Convert.ToDouble(pMin);
                                //        if (pmin <= min)
                                //        {
                                //            min = pmin;
                                //        }
                                //    }
                                //}
                                //if (b && kv1.Value[0].GetPropertyValue("CERI_Topest_Center_Elevation_Pipe") != null)
                                //{
                                //    pMax = kv1.Value[0]["CERI_Topest_Center_Elevation_Pipe"].StringValue;
                                //    if (pMax != "" && pMax != null)
                                //    {
                                //        pMax = pMax.Substring(0, pMax.IndexOf("m"));
                                //        double pmax = Convert.ToDouble(pMax);
                                //        if (pmax >= max)
                                //        {
                                //            max = pmax;
                                //        }
                                //    }

                                //}
                                //string xN = "0";
                                //if (kv1.Value[0].GetPropertyValue("CERI_Centerline_Meter") != null)
                                //{
                                //    xN = kv1.Value[0]["CERI_Centerline_Meter"].StringValue;
                                //    if (xN != "" && xN != null)
                                //    {
                                //        xN = xN.Substring(0, xN.IndexOf("m"));
                                //        double xn = Convert.ToDouble(xN);
                                //        xNum += xn;
                                //    }

                                //}
                                //else
                                //{
                                //    if (kv1.Value[0].GetPropertyValue("LENGTH") != null)
                                //    {
                                //        xNum += kv1.Value[0]["LENGTH"].DoubleValue / 1000;
                                //    }
                                //}

                                string dw = "";
                                double dz = 0, sl = 0;

                                if (b)
                                {
                                    double dry_weight = 0;
                                    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "米";
                                    //unit = "米";
                                    double length = 0;
                                    foreach (IECInstance iec2 in kv1.Value)
                                    {
                                        length += iec2["LENGTH"].DoubleValue / 1000;
                                        //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                    }
                                    //qty = length;
                                    double totalweight = length * dry_weight;
                                    zgNum = length;
                                    //sl = length;
                                    xzz += totalweight;
                                    zgzz = totalweight;
                                }
                                else
                                {
                                    double dry_weight = 0;
                                    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                        }
                                    }
                                    if (dry_weight < 0.005)
                                        dry_weight = 0;
                                    //sl = kv1.Value.Count;
                                    dz = dry_weight * kv1.Value.Count;
                                    dw = "个";
                                    xzz += dry_weight;
                                }

                                string wlbm = "", xtbh = "", xtmc = "", clmc = "", bzh = "", xtljfs = "", sjljfs = "", yldj = "", ggxh = "", gdcz = "", azbw = "", sfmd = "", gnbff = "", gnbffjtyq = "", cxdj = "", dqyqzl = "", zjqyqzl = "", mqyqzl = "", bwcl = "", fhcl = "", gg = "", tsbl = "", tsfs = "", ylsy = "", scx = "", kqcs = "", zqcs = "", jx = "", sx = "", gdtz = "", yqx = "";
                                double bwhd = 0;
                                int dqbs = 0, zjqbs = 0, mqbs = 0;
                                if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = kv1.Value[0]["GRADE"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                {
                                    xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                    {
                                        xtbh = kv1.Value[0]["UNIT"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                {
                                    xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SERVICE") != null)
                                    {
                                        xtmc = kv1.Value[0]["SERVICE"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kv1.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kv1.Value[0]["CATALOG_NAME"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_END_COND_1") != null)
                                {
                                    string ljxszh = kv1.Value[0]["CERI_END_COND_1"].StringValue;
                                    xtljfs = ljxszh;
                                    string ljxs2 = kv1.Value[0]["CERI_END_COND_2"].StringValue;
                                    if (ljxs2 != "" && ljxs2 != null)
                                    {
                                        xtljfs += "," + ljxs2;
                                    }
                                    string ljxs3 = kv1.Value[0]["CERI_END_COND_3"].StringValue;
                                    if (ljxs3 != "" && ljxs3 != null)
                                    {
                                        xtljfs += "," + ljxs3;
                                    }
                                }

                                if (kv1.Value[0].GetPropertyValue("CERI_Connection_Type") != null)
                                {
                                    sjljfs = kv1.Value[0]["CERI_Connection_Type"].StringValue;
                                }

                                Dictionary<string, string> dicStr = OPM_Public_Api.displayConnection();

                                if (dicStr == null)
                                {
                                    sjljfs = xtljfs;
                                }
                                else
                                {
                                    sjljfs = "";

                                    char[] separator = { ',' };

                                    string[] arr = xtljfs.Split(separator);

                                    foreach (var a in arr)
                                    {
                                        if (dicStr.Keys.Contains(a))
                                        {
                                            string strValue = dicStr[a];
                                            if (sjljfs.Equals(""))
                                            {
                                                sjljfs += strValue;
                                            }
                                            else
                                            {
                                                sjljfs = sjljfs + "," + strValue;
                                            }
                                        }
                                    }
                                }

                                if (kv1.Value[0].GetPropertyValue("CERI_Pressure_Rating") != null)
                                {
                                    yldj = kv1.Value[0]["CERI_Pressure_Rating"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SPECIFICATION") != null)
                                    {
                                        yldj = kv1.Value[0]["SPECIFICATION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                {
                                    ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Installation_Site") != null)
                                {
                                    azbw = kv1.Value[0]["CERI_Installation_Site"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Buried") != null)
                                {
                                    sfmd = kv1.Value[0]["CERI_Buried"].StringValue;
                                }
                                sl = zjshulian(kv1.Value);
                                if (kv1.Value[0].GetPropertyValue("CERI_Pipe_Lining_Anticorrosion") != null)
                                {
                                    gnbff = kv1.Value[0]["CERI_Pipe_Lining_Anticorrosion"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Specification") != null)
                                {
                                    gnbffjtyq = kv1.Value[0]["CERI_Specification"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Derusting_Grade") != null)
                                {
                                    cxdj = kv1.Value[0]["CERI_Derusting_Grade"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Primer_Paint") != null)
                                {
                                    dqyqzl = kv1.Value[0]["CERI_Primer_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Primer_Pass") != null)
                                {
                                    dqbs = kv1.Value[0]["CERI_Primer_Pass"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Intermediate_Paint") != null)
                                {
                                    zjqyqzl = kv1.Value[0]["CERI_Intermediate_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Middle_Number") != null)
                                {
                                    zjqbs = kv1.Value[0]["CERI_Middle_Number"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Topcoat_Paint") != null)
                                {
                                    mqyqzl = kv1.Value[0]["CERI_Topcoat_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Finish_Times") != null)
                                {
                                    mqbs = kv1.Value[0]["CERI_Finish_Times"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Material") != null)
                                {
                                    bwcl = kv1.Value[0]["CERI_Insulation_Material"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("INSULATION") != null)
                                    {
                                        bwcl = kv1.Value[0]["INSULATION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                                {
                                    bwhd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                                    {
                                        bwhd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Shielding_Material") != null)
                                {
                                    fhcl = kv1.Value[0]["CERI_Shielding_Material"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Protection_Specification") != null)
                                {
                                    gg = kv1.Value[0]["CERI_Protection_Specification"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Percentage_Detection") != null)
                                {
                                    tsbl = kv1.Value[0]["CERI_Percentage_Detection"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Inspection_Way") != null)
                                {
                                    tsfs = kv1.Value[0]["CERI_Inspection_Way"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Pressure_Testing") != null)
                                {
                                    ylsy = kv1.Value[0]["CERI_Pressure_Testing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Water_Washing") != null)
                                {
                                    scx = kv1.Value[0]["CERI_Water_Washing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Air_Purge") != null)
                                {
                                    kqcs = kv1.Value[0]["CERI_Air_Purge"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Steam_Blowing") != null)
                                {
                                    zqcs = kv1.Value[0]["CERI_Steam_Blowing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Alkali_Wash") != null)
                                {
                                    jx = kv1.Value[0]["CERI_Alkali_Wash"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Acid_Pickling") != null)
                                {
                                    sx = kv1.Value[0]["CERI_Acid_Pickling"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Pipeline_Skim") != null)
                                {
                                    gdtz = kv1.Value[0]["CERI_Pipeline_Skim"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Oil_Cleaning") != null)
                                {
                                    yqx = kv1.Value[0]["CERI_Oil_Cleaning"].StringValue;
                                }

                                s.Cells[row, 3] = wlbm;
                                s.Cells[row, 4] = xtbh;
                                s.Cells[row, 5] = xtmc;
                                s.Cells[row, 6] = clmc;
                                s.Cells[row, 7] = bzh;
                                s.Cells[row, 8] = xtljfs;
                                s.Cells[row, 9] = sjljfs;
                                s.Cells[row, 10] = yldj;
                                s.Cells[row, 11] = ggxh;
                                s.Cells[row, 12] = gdcz;
                                s.Cells[row, 13] = azbw;
                                s.Cells[row, 14] = sfmd;
                                s.Cells[row, 17] = sl;
                                s.Cells[row, 18] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 19] = dw;
                                s.Cells[row, 20] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 21] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 22] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero); ;
                                s.Cells[row, 23] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero); ;
                                s.Cells[row, 24] = gnbff;
                                s.Cells[row, 25] = gnbffjtyq;
                                s.Cells[row, 26] = cxdj;
                                s.Cells[row, 27] = dqyqzl;
                                s.Cells[row, 28] = dqbs;
                                s.Cells[row, 29] = zjqyqzl;
                                s.Cells[row, 30] = zjqbs;
                                s.Cells[row, 31] = mqyqzl;
                                s.Cells[row, 32] = mqbs;
                                s.Cells[row, 33] = bwcl;
                                s.Cells[row, 34] = bwhd;
                                s.Cells[row, 35] = fhcl;
                                s.Cells[row, 36] = gg;
                                s.Cells[row, 37] = tsbl;
                                s.Cells[row, 38] = tsfs;
                                s.Cells[row, 39] = ylsy;
                                s.Cells[row, 40] = scx;
                                s.Cells[row, 41] = kqcs;
                                s.Cells[row, 42] = zqcs;
                                s.Cells[row, 43] = jx;
                                s.Cells[row, 44] = sx;
                                s.Cells[row, 45] = gdtz;
                                s.Cells[row, 46] = yqx;
                            }
                            #endregion
                            if (min == double.MaxValue)
                            {
                                s.Cells[irow, 15] = "";
                            }
                            else
                            {
                                s.Cells[irow, 15] = min;
                            }
                            if (max == double.MinValue)
                            {
                                s.Cells[irow, 16] = "";
                            }
                            else
                            {
                                s.Cells[irow, 16] = max;
                            }
                            #region 总和
                            //s.Cells[irow, 20] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r = s.Cells[irow, 20];
                            //r.NumberFormatLocal = "@";
                            //s.Cells[irow, 21] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r1 = s.Cells[irow, 21];
                            //r1.NumberFormatLocal = "@";
                            //s.Cells[irow, 22] =Math.Round((decimal)zgNum,2,MidpointRounding.AwayFromZero);
                            //MOIE.Range r2 = s.Cells[irow, 22];
                            //r2.NumberFormatLocal = "@";
                            //s.Cells[irow, 23] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r3 = s.Cells[irow, 23];
                            //r3.NumberFormatLocal = "@";
                            #endregion
                        }
                    }
                    wb.Save();
                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                    #region
                    //int row = 7; //行号
                    //int lineindex = 0; //序号
                    //foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    //{
                    //    row++;
                    //    lineindex++;
                    //    s.Cells[row, 1] = lineindex;
                    //    bool biaoshi = true;//避免重复添加管线编号
                    //    //s.Cells[row, 2] = kv.Value.Values[0]["LINENUMBER"].StringValue;
                    //    foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                    //    {
                    //        if (biaoshi)
                    //        {
                    //            s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                    //            biaoshi = false;
                    //        }
                    //        row++;
                    //        string jzmc = "", wd = "", dh = "", cz = "";
                    //        double pod = 0, hd = 0, dlcd = 0, mj = 0, tj = 0;
                    //        int cs = 0;
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                    //        {
                    //            jzmc = kv1.Value[0]["CERI_Media_Name"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_PIPE_OD_M") != null)
                    //        {
                    //            pod = kv1.Value[0]["CERI_PIPE_OD_M"].DoubleValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("OUTSIDE_DIAMETER") != null)
                    //            {
                    //                pod = kv1.Value[0]["OUTSIDE_DIAMETER"].DoubleValue;
                    //            }
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_TEMP") != null)
                    //        {
                    //            wd = kv1.Value[0]["CERI_TEMP"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_code") != null)
                    //        {
                    //            dh = kv1.Value[0]["CERI_Insulation_code"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Materia") != null)
                    //        {
                    //            cz = kv1.Value[0]["CERI_Insulation_Materia"].StringValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                    //            {
                    //                cz = kv1.Value[0]["MATERIAL"].StringValue;
                    //            }
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Layer") != null)
                    //        {
                    //            cs = kv1.Value[0]["CERI_Layer"].IntValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                    //        {
                    //            hd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                    //            {
                    //                hd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                    //            }
                    //        }
                    //        foreach (IECInstance iec1 in kv1.Value)
                    //        {
                    //            if (iec1.GetPropertyValue("CERI_LENGTH") != null)
                    //            {
                    //                dlcd += iec1["CERI_LENGTH"].DoubleValue;
                    //            }
                    //            else
                    //            {
                    //                if (iec1.GetPropertyValue("LENGTH") != null)
                    //                {
                    //                    dlcd += iec1["LENGTH"] / 1000;
                    //                }
                    //            }
                    //            if (iec1.GetPropertyValue("CERI_Area") != null)
                    //            {
                    //                mj += iec1["CERI_Area"].DoubleValue;
                    //            }
                    //            if (iec1.GetPropertyValue("CERI_Volume") != null)
                    //            {
                    //                tj += iec1["CERI_Volume"].DoubleValue;
                    //            }
                    //        }
                    //        s.Cells[row, 3] = jzmc;
                    //        s.Cells[row, 4] = pod;
                    //        s.Cells[row, 5] = dlcd;
                    //        s.Cells[row, 6] = wd;
                    //        s.Cells[row, 7] = dh;
                    //        s.Cells[row, 10] = cs;
                    //        s.Cells[row, 11] = hd;
                    //        s.Cells[row, 12] = mj;
                    //        s.Cells[row, 13] = tj;
                    //        //s.Cells[row, 13] = catalog_name;
                    //        //s.Cells[row, 14] = media;
                    //    }
                    //}
                    //wb.Save();
                    //wb.Close();
                    //wkb.Close();
                    //app.Quit();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                    #endregion
                }
                waitDialog.Close();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app);

                openPath = excelPath;
            }
        }

        public void zjExportExcel1(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                bool isHz = lxcomboBox1.Text.Equals("管道造价汇总表");
                try
                {
                    if (isHz)
                    {
                        File.Copy(path0 + "\\JYXConfig\\管道造价汇总表\\" + yName, excelPath);
                    }
                    else
                    {
                        File.Copy(path0 + "\\JYXConfig\\管道造价表\\" + yName, excelPath);
                    }

                }
                catch
                {
                    if (isHz)
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道造价汇总表：下没有找到" + yName + "文件");
                    }
                    else
                    {
                        MessageBox.Show("路径" + path0 + "\\JYXConfig\\管道造价表：下没有找到" + yName + "文件");
                    }
                    return;
                }

                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();

                //app = new MOIE.Application();
                //MOIE.Workbooks wkb = app.Workbooks;
                //MOIE.Workbook wb = wkb.Open(excelPath);
                ////app.Visible = true;
                ////wb=wkb.Open(excelPath1);
                //MOIE.Worksheet s;
                IWorkbook workbook;
                using (FileStream file = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new XSSFWorkbook(file);//xls有问题

                }
                ISheet sheet;
                if (isHz)
                {
                    sheet = workbook.GetSheet("管道汇总表");
                }
                else
                {
                    sheet = workbook.GetSheet("管道明细表");
                }

                if (itemList.Count > 0)
                {
                    //给s填值就行
                    #region 汇总表
                    int row = 2, lineindex = 0;
                    if (isHz)
                    {
                        
                        #region 按物料编码分组
                        if (wlbmGroupcheckBox1.Checked)
                        {
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                            {
                                row++;
                                lineindex++;
                                //s.Cells[row, 1] = lineindex;
                                IRow dataRow = sheet.CreateRow(row); //第4行开始编辑
                                ICell cell4Row0Column = dataRow.CreateCell(0);
                                cell4Row0Column.SetCellValue(lineindex);
                                bool biaoshi = true;

                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                                {
                                    int irow = 0;
                                    double xNum = 0, xzz = 0, zgNum = 0, zgzz = 0;
                                    if (biaoshi)
                                    {
                                        string pipeLine = " ";
                                        if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null) pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                        string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                        //s.Cells[row, 2] = pipeNumber;
                                        ICell cell4Row1Column = dataRow.CreateCell(1); //第四行的第二列开始
                                        cell4Row1Column.SetCellValue(pipeNumber);
                                        //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                        biaoshi = false;
                                    }
                                    irow = row + 1;
                                    string wlbm3 = "";
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                    {
                                        wlbm3 = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                        {
                                            wlbm3 = kv1.Value[0]["GRADE"].StringValue;
                                        }
                                    }
                                    bool isKW = false;
                                    if (wlbm3 == "" || wlbm3 == " ")
                                        isKW = true;
                                    if (isKW)
                                    {
                                        string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "";
                                        if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                        {
                                            xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                            {
                                                xtbh = kv1.Value[0]["UNIT"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                        {
                                            xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("SERVICE") != null)
                                            {
                                                xtmc = kv1.Value[0]["SERVICE"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                        {
                                            clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                            {
                                                clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                        {
                                            ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                        {
                                            gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                            {
                                                gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                            }
                                        }
                                        if (kv1.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                        {
                                            bz = kv1.Value[0]["CERI_NOTE"].StringValue;
                                        }
                                        else
                                        {                                            
                                            if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                            {
                                                bz = kv1.Value[0]["NOTES"].StringValue;
                                            }
                                        }
                                        double zjsl = 0;
                                        zjsl = zjshulian(kv1.Value);

                                        string dw = "";
                                        double dz = 0;
                                        bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                                        if (b)
                                        {
                                            double dry_weight = 0;
                                            if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                            {
                                                dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                                if (dry_weight < 0.005)
                                                    dry_weight = 0;
                                                dz = dry_weight;
                                            }
                                            else
                                            {
                                                if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                                {
                                                    dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                    if (dry_weight < 0.005)
                                                        dry_weight = 0;
                                                    dz = dry_weight;
                                                }
                                            }
                                            dw = "米";
                                        }
                                        else
                                        {
                                            double dry_weight = 0;
                                            if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                            {
                                                dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                            }
                                            else
                                            {
                                                if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                                {
                                                    dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                }
                                            }
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                            dw = "个";
                                            xzz = dz * kv1.Value.Count;
                                        }
                                        foreach (IECInstance iecs in kv1.Value)
                                        {
                                            string xN = "0";
                                            if (iecs.GetPropertyValue("CERI_Centerline_Meter") != null)
                                            {
                                                xN = iecs["CERI_Centerline_Meter"].StringValue;
                                                if (xN != "" && xN != null)
                                                {
                                                    xN = xN.Substring(0, xN.IndexOf("m"));
                                                    double xn = Convert.ToDouble(xN);
                                                    xNum += xn;
                                                }

                                            }
                                            else
                                            {
                                                if (iecs.GetPropertyValue("LENGTH") != null)
                                                {
                                                    xNum += iecs["LENGTH"].DoubleValue / 1000;
                                                }
                                            }

                                            if (b)
                                            {
                                                //unit = "米";
                                                double length = 0;
                                                length += iecs["LENGTH"].DoubleValue / 1000;
                                                //qty = length;
                                                double totalweight = length * dz;
                                                zgNum += length;
                                                xzz += totalweight;
                                                zgzz += totalweight;
                                            }
                                        }
                                        row++;
                                        IRow nextRow = sheet.CreateRow(row); //第5行开始
                                        ICell cell2column = nextRow.CreateCell(2);
                                        cell2column.SetCellValue(xtbh);
                                        //s.Cells[row, 3] = xtbh;
                                        ICell cell3column = nextRow.CreateCell(3);
                                        cell3column.SetCellValue(xtmc);
                                        //s.Cells[row, 4] = xtmc;
                                        ICell cell4column = nextRow.CreateCell(4);
                                        cell4column.SetCellValue(clmc);
                                        //s.Cells[row, 5] = clmc;
                                        ICell cell5column = nextRow.CreateCell(5);
                                        cell5column.SetCellValue(ggxh);
                                        //s.Cells[row, 6] = ggxh;
                                        ICell cell6column = nextRow.CreateCell(6);
                                        cell6column.SetCellValue(gdcz);
                                        //s.Cells[row, 7] = gdcz;
                                        ICell cell7column = nextRow.CreateCell(7);
                                        cell7column.SetCellValue(zjsl);
                                        //s.Cells[row, 8] = zjsl;
                                        ICell cell8column = nextRow.CreateCell(8);
                                        cell8column.SetCellValue(Math.Round(/*(decimal)*/dz, 2, MidpointRounding.AwayFromZero));
                                        //s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                        ICell cell9column = nextRow.CreateCell(9);
                                        cell9column.SetCellValue(dw);
                                        //s.Cells[row, 10] = dw;
                                        ICell cell14column = nextRow.CreateCell(14);
                                        cell14column.SetCellValue(bz);
                                        //s.Cells[row, 15] = bz;
                                        ICell cell10column = nextRow.CreateCell(10);
                                        cell10column.SetCellValue(Math.Round(/*(decimal)*/xNum, 2, MidpointRounding.AwayFromZero));
                                        //s.Cells[row, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                        ICell cell11column = nextRow.CreateCell(11);
                                        cell11column.SetCellValue(Math.Round(/*(decimal)*/xzz, 2, MidpointRounding.AwayFromZero));
                                        //s.Cells[row, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                        ICell cell12column = nextRow.CreateCell(12);
                                        cell12column.SetCellValue(Math.Round(/*(decimal)*/zgNum, 2, MidpointRounding.AwayFromZero));
                                        //s.Cells[row, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                        ICell cell13column = nextRow.CreateCell(13);
                                        cell13column.SetCellValue(Math.Round(/*(decimal)*/zgzz, 2, MidpointRounding.AwayFromZero));
                                        //s.Cells[row, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                        ICell cell15column = nextRow.CreateCell(15);
                                        cell15column.SetCellValue(wlbm3);
                                        //s.Cells[row, 16] = wlbm3;
                                    }
                                    else
                                    {


                                        Dictionary<int, List<IECInstance>> zjDicList = zjwlHz(kv1.Value);
                                        #region 物料编码不为空 以前处理方式
                                        //irow = row + 1;
                                        #region 改
                                        foreach (KeyValuePair<int, List<IECInstance>> kv2 in zjDicList)
                                        {
                                            row++;
                                            //irow = row;                                        
                                            string xN = "0";
                                            string dw = "";
                                            double dz = 0;
                                            foreach (IECInstance iec in kv2.Value)
                                            {
                                                if (iec.GetPropertyValue("CERI_Centerline_Meter") != null)
                                                {
                                                    xN = iec["CERI_Centerline_Meter"].StringValue;
                                                    if (xN != "" && xN != null)
                                                    {
                                                        xN = xN.Substring(0, xN.IndexOf("m"));
                                                        double xn = Convert.ToDouble(xN);
                                                        xNum += xn;
                                                    }

                                                }
                                                else
                                                {
                                                    if (iec.GetPropertyValue("LENGTH") != null)
                                                    {
                                                        xNum += iec["LENGTH"].DoubleValue / 1000;
                                                    }
                                                }

                                                bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                                                if (b)
                                                {
                                                    double dry_weight = 0;
                                                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                                    {
                                                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue;
                                                        if (dry_weight < 0.005)
                                                            dry_weight = 0;
                                                        dz = dry_weight;
                                                    }
                                                    else
                                                    {
                                                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                                                        {
                                                            dry_weight = iec["DRY_WEIGHT"].DoubleValue;
                                                            if (dry_weight < 0.005)
                                                                dry_weight = 0;
                                                            dz = dry_weight;
                                                        }
                                                    }
                                                    dw = "米";
                                                    //unit = "米";
                                                    double length = 0;
                                                    length += iec["LENGTH"].DoubleValue / 1000;
                                                    //qty = length;
                                                    double totalweight = length * dry_weight;
                                                    zgNum += length;
                                                    xzz += totalweight;
                                                    zgzz += totalweight;
                                                }
                                                else
                                                {
                                                    double dry_weight = 0;
                                                    if (iec.GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                                    {
                                                        dry_weight = iec["CERI_WEIGHT_DRY"].DoubleValue;
                                                    }
                                                    else
                                                    {
                                                        if (iec.GetPropertyValue("DRY_WEIGHT") != null)
                                                        {
                                                            dry_weight = iec["DRY_WEIGHT"].DoubleValue;
                                                        }
                                                    }
                                                    if (dry_weight < 0.005)
                                                        dry_weight = 0;
                                                    dz = dry_weight;
                                                    dw = "个";
                                                    xzz += dry_weight;
                                                }
                                            }
                                            string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "备注";
                                            if (kv2.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                            {
                                                xtbh = kv2.Value[0]["CERI_System_Number"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("UNIT") != null)
                                                {
                                                    xtbh = kv2.Value[0]["UNIT"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                            {
                                                xtmc = kv2.Value[0]["CERI_System_Name"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("SERVICE") != null)
                                                {
                                                    xtmc = kv2.Value[0]["SERVICE"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                            {
                                                clmc = kv2.Value[0]["CERI_SHORT_DESC"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                                {
                                                    clmc = kv2.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                            {
                                                ggxh = kv2.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                            {
                                                gdcz = kv2.Value[0]["CERI_MATERIAL"].StringValue;
                                            }
                                            else
                                            {
                                                if (kv2.Value[0].GetPropertyValue("MATERIAL") != null)
                                                {
                                                    gdcz = kv2.Value[0]["MATERIAL"].StringValue;
                                                }
                                            }
                                            if (kv2.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                            {
                                                bz = kv2.Value[0]["CERI_NOTE"].StringValue;
                                                if (bz.Equals(""))
                                                    bz = " ";
                                            }
                                            else
                                            {                                                
                                                if (kv2.Value[0].GetPropertyValue("NOTES") != null)
                                                {
                                                    bz = kv2.Value[0]["NOTES"].StringValue;
                                                    if (bz.Equals(""))
                                                        bz = " ";
                                                }
                                            }
                                            double zjsl = 0;
                                            zjsl = zjshulian(kv2.Value);
                                            IRow nextRow = sheet.CreateRow(row);
                                            ICell cell2column = nextRow.CreateCell(2);
                                            cell2column.SetCellValue(xtbh);
                                            //s.Cells[row, 3] = xtbh;
                                            ICell cell3column = nextRow.CreateCell(3);
                                            cell3column.SetCellValue(xtmc);
                                            //s.Cells[row, 4] = xtmc;
                                            ICell cell4column = nextRow.CreateCell(4);
                                            cell4column.SetCellValue(clmc);
                                            //s.Cells[row, 5] = clmc;
                                            ICell cell5column = nextRow.CreateCell(5);
                                            cell5column.SetCellValue(ggxh);
                                            //s.Cells[row, 6] = ggxh;
                                            ICell cell6column = nextRow.CreateCell(6);
                                            cell6column.SetCellValue(gdcz);
                                            //s.Cells[row, 7] = gdcz;
                                            ICell cell7column = nextRow.CreateCell(7);
                                            cell7column.SetCellValue(zjsl);
                                            //s.Cells[row, 8] = zjsl;
                                            ICell cell8column = nextRow.CreateCell(8);
                                            cell8column.SetCellValue(Math.Round(/*(decimal)*/dz, 2, MidpointRounding.AwayFromZero));
                                            //s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                            ICell cell9column = nextRow.CreateCell(9);
                                            cell9column.SetCellValue(dw);
                                            //s.Cells[row, 10] = dw;
                                            ICell cell14column = nextRow.CreateCell(14);
                                            cell14column.SetCellValue(bz);
                                            ICell cell10column = nextRow.CreateCell(10);
                                            ICell cell11column = nextRow.CreateCell(11);
                                            ICell cell12column = nextRow.CreateCell(12);
                                            ICell cell13column = nextRow.CreateCell(13);
                                            ICell cell15column = nextRow.CreateCell(15);
                                            //s.Cells[row, 15] = bz;
                                        }
                                        #endregion
                                        string wlbm = "";
                                        if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                        {
                                            wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                            {
                                                wlbm = kv1.Value[0]["GRADE"].StringValue;
                                            }
                                        }

                                        #region 合并单元格（开始行号，结束行号，开始列号，结束列号）
                                        double value10 = Math.Round(xNum, 2, MidpointRounding.AwayFromZero);
                                        double value11 = Math.Round(xzz, 2, MidpointRounding.AwayFromZero);
                                        double value12 = Math.Round(zgNum, 2, MidpointRounding.AwayFromZero);
                                        double value13 = Math.Round(zgzz, 2, MidpointRounding.AwayFromZero);
                                        string value15 = wlbm;
                                        if (zjDicList.Count > 1)
                                        {
                                            for (int j = irow + 1; j < irow + zjDicList.Count; j++)
                                            {
                                                try
                                                {
                                                    sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 10, 10));
                                                }
                                                catch (Exception ex)
                                                {
                                                    string e = ex.ToString();
                                                }
                                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 10, 10));
                                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 11, 11));
                                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 12, 12));
                                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 13, 13));
                                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + zjDicList.Count - 1, 15, 15));
                                            }
                                            IRow iii = sheet.GetRow(irow);
                                            ICell cc = iii.GetCell(10);
                                            sheet.GetRow(irow).GetCell(10).SetCellValue(value10);
                                            //s.Cells[irow, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                            //MOIE.Range r = s.Cells[irow, 11];
                                            //r.NumberFormatLocal = "@";

                                            sheet.GetRow(irow).GetCell(11).SetCellValue(value11);
                                            //s.Cells[irow, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                            //MOIE.Range r1 = s.Cells[irow, 12];
                                            //r1.NumberFormatLocal = "@";

                                            sheet.GetRow(irow).GetCell(12).SetCellValue(value12);
                                            //s.Cells[irow, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                            //MOIE.Range r2 = s.Cells[irow, 13];
                                            //r2.NumberFormatLocal = "@";

                                            sheet.GetRow(irow).GetCell(13).SetCellValue(value13);
                                            //s.Cells[irow, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                            //MOIE.Range r3 = s.Cells[irow, 14];
                                            //r3.NumberFormatLocal = "@";

                                            sheet.GetRow(irow).GetCell(15).SetCellValue(value15);
                                            //s.Cells[irow, 16] = wlbm;
                                            //MOIE.Range r4 = s.Cells[irow, 16];
                                            //r4.NumberFormatLocal = "@";
                                        }
                                        else
                                        {
                                            //sheet.creat
                                            IRow rowx = sheet.GetRow(irow); //第4行开始编辑
                                            ICell cell10 = rowx.CreateCell(10);
                                            cell10.SetCellValue(value10);
                                            ICell cell11 = rowx.CreateCell(11);
                                            cell11.SetCellValue(value11);
                                            ICell cell12 = rowx.CreateCell(12);
                                            cell12.SetCellValue(value12);
                                            ICell cell13 = rowx.CreateCell(13);
                                            cell13.SetCellValue(value13);
                                            ICell cell15 = rowx.CreateCell(15);
                                            cell15.SetCellValue(value15);
                                        }

                                        //for (int j = irow + 1; j < irow + zjDicList.Count; j++) //需要改
                                        //{
                                        //    MOIE.Range range = s.Range[s.Cells[irow, 11], s.Cells[j, 11]];
                                        //    range.Application.DisplayAlerts = false;
                                        //    range.Merge(Type.Missing);
                                        //    range.Application.DisplayAlerts = true;
                                        //    MOIE.Range range1 = s.Range[s.Cells[irow, 12], s.Cells[j, 12]];
                                        //    range1.Application.DisplayAlerts = false;
                                        //    range1.Merge(Type.Missing);
                                        //    range1.Application.DisplayAlerts = true;
                                        //    MOIE.Range range2 = s.Range[s.Cells[irow, 13], s.Cells[j, 13]];
                                        //    range2.Application.DisplayAlerts = false;
                                        //    range2.Merge(Type.Missing);
                                        //    range2.Application.DisplayAlerts = true;
                                        //    MOIE.Range range3 = s.Range[s.Cells[irow, 14], s.Cells[j, 14]];
                                        //    range3.Application.DisplayAlerts = false;
                                        //    range3.Merge(Type.Missing);
                                        //    range3.Application.DisplayAlerts = true;
                                        //    MOIE.Range range4 = s.Range[s.Cells[irow, 16], s.Cells[j, 16]];
                                        //    range4.Application.DisplayAlerts = false;
                                        //    range4.Merge(Type.Missing);
                                        //    range4.Application.DisplayAlerts = true;
                                        //}
                                        #endregion


                                        #endregion
                                    }

                                }
                            }
                        }
                        #endregion
                        else
                        {
                            foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                            {
                                row++;
                                lineindex++;
                                IRow dataRow = sheet.CreateRow(row); //第4行开始编辑
                                ICell cell2Row0Column = dataRow.CreateCell(0);
                                cell2Row0Column.SetCellValue(lineindex);
                                //s.Cells[row, 1] = lineindex;
                                bool biaoshi = true;
                                int irow = 0;
                                double xNum = 0, xzz = 0, zgNum = 0, zgzz = 0;


                                #region
                                foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                                {
                                    if (biaoshi)
                                    {
                                        string pipeLine = " ";
                                        if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null) pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                        string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                        ICell cell2Row1Column = dataRow.CreateCell(1);
                                        cell2Row1Column.SetCellValue(pipeNumber);
                                        //s.Cells[row, 2] = pipeNumber;
                                        //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                        biaoshi = false;
                                        irow = row + 1;
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 9, 9));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 10, 10));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 11, 11));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 12, 12));

                                        //for (int j = irow + 1; j < irow + kv.Value.Count; j++)
                                        //{
                                        //    MOIE.Range range = s.Range[s.Cells[irow, 10], s.Cells[j, 10]];
                                        //    range.Application.DisplayAlerts = false;
                                        //    range.Merge(Type.Missing);
                                        //    range.Application.DisplayAlerts = true;
                                        //    MOIE.Range range1 = s.Range[s.Cells[irow, 11], s.Cells[j, 11]];
                                        //    range1.Application.DisplayAlerts = false;
                                        //    range1.Merge(Type.Missing);
                                        //    range1.Application.DisplayAlerts = true;
                                        //    MOIE.Range range2 = s.Range[s.Cells[irow, 12], s.Cells[j, 12]];
                                        //    range2.Application.DisplayAlerts = false;
                                        //    range2.Merge(Type.Missing);
                                        //    range2.Application.DisplayAlerts = true;
                                        //    MOIE.Range range3 = s.Range[s.Cells[irow, 13], s.Cells[j, 13]];
                                        //    range3.Application.DisplayAlerts = false;
                                        //    range3.Merge(Type.Missing);
                                        //    range3.Application.DisplayAlerts = true;
                                        //}
                                    }
                                    row++;

                                    string xN = "0";
                                    if (kv1.Value[0].GetPropertyValue("CERI_Centerline_Meter") != null)
                                    {
                                        xN = kv1.Value[0]["CERI_Centerline_Meter"].StringValue;
                                        if (xN != "" && xN != null)
                                        {
                                            xN = xN.Substring(0, xN.IndexOf("m"));
                                            double xn = Convert.ToDouble(xN);
                                            xNum += xn;
                                        }

                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("LENGTH") != null)
                                        {
                                            xNum += kv1.Value[0]["LENGTH"].DoubleValue / 1000;
                                        }
                                    }

                                    string dw = "";
                                    double dz = 0, sl = 0;
                                    bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);
                                    if (b)
                                    {
                                        double dry_weight = 0;
                                        if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                                if (dry_weight < 0.005)
                                                    dry_weight = 0;
                                                dz = dry_weight;
                                            }
                                        }
                                        dw = "米";
                                        //unit = "米";
                                        double length = 0;
                                        foreach (IECInstance iec2 in kv1.Value)
                                        {
                                            length += iec2["LENGTH"].DoubleValue / 1000;
                                            //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                        }
                                        //qty = length;
                                        double totalweight = length * dry_weight;
                                        zgNum += length;
                                        xzz += totalweight;
                                        zgzz += totalweight;
                                    }
                                    else
                                    {
                                        double dry_weight = 0;
                                        if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                        {
                                            dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        }
                                        else
                                        {
                                            if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                            {
                                                dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            }
                                        }
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                        dw = "个";
                                        xzz += dry_weight;
                                    }
                                    string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", bz = "", wlbm = "";
                                    if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                    {
                                        xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                        {
                                            xtbh = kv1.Value[0]["UNIT"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                    {
                                        xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                    {
                                        clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                        {
                                            clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                    {
                                        ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                    {
                                        gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                        {
                                            gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_NOTE") != null)
                                    {
                                        bz = kv1.Value[0]["CERI_NOTE"].StringValue;
                                    }
                                    else
                                    {                                       
                                        if (kv1.Value[0].GetPropertyValue("NOTES") != null)
                                        {
                                            bz = kv1.Value[0]["NOTES"].StringValue;
                                        }
                                    }
                                    if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                    {
                                        wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                        {
                                            wlbm = kv1.Value[0]["GRADE"].StringValue;
                                        }
                                    }
                                    sl = zjshulian(kv1.Value);

                                    IRow nextRow = sheet.CreateRow(row);
                                    ICell cell2column = nextRow.CreateCell(2);
                                    cell2column.SetCellValue(xtbh);
                                    //s.Cells[row, 3] = xtbh;
                                    ICell cell3column = nextRow.CreateCell(3);
                                    cell3column.SetCellValue(xtmc);
                                    //s.Cells[row, 4] = xtmc;
                                    ICell cell4column = nextRow.CreateCell(4);
                                    cell4column.SetCellValue(clmc);
                                    //s.Cells[row, 5] = clmc;
                                    ICell cell5column = nextRow.CreateCell(5);
                                    cell5column.SetCellValue(ggxh);
                                    //s.Cells[row, 6] = ggxh;
                                    ICell cell6column = nextRow.CreateCell(6);
                                    cell6column.SetCellValue(gdcz);
                                    //s.Cells[row, 7] = gdcz;
                                    ICell cell7column = nextRow.CreateCell(7);
                                    cell7column.SetCellValue(sl);
                                    //s.Cells[row, 8] = sl;
                                    ICell cell8column = nextRow.CreateCell(8);
                                    cell8column.SetCellValue(Math.Round(/*(decimal)*/dz, 2, MidpointRounding.AwayFromZero));
                                    //s.Cells[row, 9] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                    ICell cell9column = nextRow.CreateCell(9);
                                    cell9column.SetCellValue(dw);
                                    //s.Cells[row, 10] = dw;
                                    ICell cell14column = nextRow.CreateCell(14);
                                    cell14column.SetCellValue(bz);
                                    //s.Cells[row, 15] = bz;
                                    ICell cell15column = nextRow.CreateCell(15);
                                    cell15column.SetCellValue(wlbm);
                                    //s.Cells[row, 16] = wlbm;
                                }
                                sheet.GetRow(irow).GetCell(10).SetCellValue(Math.Round(/*(decimal)*/xNum, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[irow, 11] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                //MOIE.Range r = s.Cells[irow, 10];
                                //r.NumberFormatLocal = "@";
                                sheet.GetRow(irow).GetCell(11).SetCellValue(Math.Round(/*(decimal)*/xzz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[irow, 12] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                //MOIE.Range r1 = s.Cells[irow, 11];
                                //r1.NumberFormatLocal = "@";
                                sheet.GetRow(irow).GetCell(12).SetCellValue(Math.Round(/*(decimal)*/zgNum, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[irow, 13] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero);
                                //MOIE.Range r2 = s.Cells[irow, 12];
                                //r2.NumberFormatLocal = "@";
                                sheet.GetRow(irow).GetCell(13).SetCellValue(Math.Round(/*(decimal)*/zgzz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[irow, 14] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                //MOIE.Range r3 = s.Cells[irow, 13];
                                //r3.NumberFormatLocal = "@";
                                #endregion
                            }
                        }
                    }
                    #endregion
                    else
                    {
                        
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            row++;
                            lineindex++;
                            IRow dataRow = sheet.CreateRow(row); //第4行开始编辑
                            ICell cell4Row0Column = dataRow.CreateCell(0);
                            cell4Row0Column.SetCellValue(lineindex);
                            //s.Cells[row, 1] = lineindex;
                            bool biaoshi = true;
                            int irow = 0;
                            double min = double.MaxValue, max = double.MinValue;
                            #region
                            foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                            {
                                if (biaoshi)
                                {
                                    //s.get_Range(s.Cells[5, 5], s.Cells[5, 6]).Merge(Type.Missing);
                                    string pipeLine = " ";
                                    if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null) pipeLine = kv1.Value[0]["LINENUMBER"].StringValue;
                                    string pipeNumber = OPM_Public_Api.repicePipeLine(pipeLine);
                                    ICell cell4Row1Column = dataRow.CreateCell(1);
                                    cell4Row1Column.SetCellValue(pipeNumber);
                                    //s.Cells[row, 2] = pipeNumber;
                                    //s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                                    biaoshi = false;
                                    irow = row + 1;
                                    for (int j = irow + 1; j < irow + kv.Value.Count; j++)
                                    {
                                        try
                                        {
                                            sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 14, 14));
                                            //MOIE.Range range = s.Range[s.Cells[irow, 15], s.Cells[j, 15]];
                                            //range.Application.DisplayAlerts = false;
                                            //range.Merge(Type.Missing);
                                            //range.Application.DisplayAlerts = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            string e = ex.ToString();
                                        }
                                        //sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 14, 14));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 19, 19));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 20, 20));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 21, 21));
                                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count - 1, 22, 22));
                                        //MOIE.Range range1 = s.Range[s.Cells[irow, 16], s.Cells[j, 16]];
                                        //range1.Application.DisplayAlerts = false;
                                        //range1.Merge(Type.Missing);
                                        //range1.Application.DisplayAlerts = true;
                                        //MOIE.Range range2 = s.Range[s.Cells[irow, 20], s.Cells[j, 20]];
                                        //range2.Application.DisplayAlerts = false;
                                        //range2.Merge(Type.Missing);
                                        //range2.Application.DisplayAlerts = true;
                                        //MOIE.Range range3 = s.Range[s.Cells[irow, 21], s.Cells[j, 21]];
                                        //range3.Application.DisplayAlerts = false;
                                        //range3.Merge(Type.Missing);
                                        //range3.Application.DisplayAlerts = true;
                                        //MOIE.Range range4 = s.Range[s.Cells[irow, 22], s.Cells[j, 22]];
                                        //range4.Application.DisplayAlerts = false;
                                        //range4.Merge(Type.Missing);
                                        //range4.Application.DisplayAlerts = true;
                                        //MOIE.Range range5 = s.Range[s.Cells[irow, 23], s.Cells[j, 23]];
                                        //range5.Application.DisplayAlerts = false;
                                        //range5.Merge(Type.Missing);
                                        //range5.Application.DisplayAlerts = true;
                                    }
                                }
                                double zgNum = 0, zgzz = 0, xNum = 0, xzz = 0;
                                row++;

                                bool b = BMECApi.Instance.InstanceDefinedAsClass(kv1.Value[0], "PIPE", true);

                                foreach (IECInstance iecs in kv1.Value)
                                {
                                    string pMin = "0", pMax = "0";
                                    if (b && iecs.GetPropertyValue("CERI_Lowest_Center_Elevation_Pipe") != null)
                                    {
                                        pMin = iecs["CERI_Lowest_Center_Elevation_Pipe"].StringValue;
                                        if (pMin != null && pMin != "")
                                        {
                                            pMin = pMin.Substring(0, pMin.IndexOf("m"));
                                            double pmin = Convert.ToDouble(pMin);
                                            if (pmin <= min)
                                            {
                                                min = pmin;
                                            }
                                        }
                                    }
                                    if (b && iecs.GetPropertyValue("CERI_Topest_Center_Elevation_Pipe") != null)
                                    {
                                        pMax = iecs["CERI_Topest_Center_Elevation_Pipe"].StringValue;
                                        if (pMax != "" && pMax != null)
                                        {
                                            pMax = pMax.Substring(0, pMax.IndexOf("m"));
                                            double pmax = Convert.ToDouble(pMax);
                                            if (pmax >= max)
                                            {
                                                max = pmax;
                                            }
                                        }

                                    }
                                    string xN = "0";
                                    if (iecs.GetPropertyValue("CERI_Centerline_Meter") != null)
                                    {
                                        xN = iecs["CERI_Centerline_Meter"].StringValue;
                                        if (xN != "" && xN != null)
                                        {
                                            xN = xN.Substring(0, xN.IndexOf("m"));
                                            double xn = Convert.ToDouble(xN);
                                            xNum += xn;
                                        }

                                    }
                                    else
                                    {
                                        if (iecs.GetPropertyValue("LENGTH") != null)
                                        {
                                            xNum += iecs["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                }

                                //if (b && kv1.Value[0].GetPropertyValue("CERI_Lowest_Center_Elevation_Pipe") != null)
                                //{
                                //    pMin = kv1.Value[0]["CERI_Lowest_Center_Elevation_Pipe"].StringValue;
                                //    if (pMin != null && pMin != "")
                                //    {
                                //        pMin = pMin.Substring(0, pMin.IndexOf("m"));
                                //        double pmin = Convert.ToDouble(pMin);
                                //        if (pmin <= min)
                                //        {
                                //            min = pmin;
                                //        }
                                //    }
                                //}
                                //if (b && kv1.Value[0].GetPropertyValue("CERI_Topest_Center_Elevation_Pipe") != null)
                                //{
                                //    pMax = kv1.Value[0]["CERI_Topest_Center_Elevation_Pipe"].StringValue;
                                //    if (pMax != "" && pMax != null)
                                //    {
                                //        pMax = pMax.Substring(0, pMax.IndexOf("m"));
                                //        double pmax = Convert.ToDouble(pMax);
                                //        if (pmax >= max)
                                //        {
                                //            max = pmax;
                                //        }
                                //    }

                                //}
                                //string xN = "0";
                                //if (kv1.Value[0].GetPropertyValue("CERI_Centerline_Meter") != null)
                                //{
                                //    xN = kv1.Value[0]["CERI_Centerline_Meter"].StringValue;
                                //    if (xN != "" && xN != null)
                                //    {
                                //        xN = xN.Substring(0, xN.IndexOf("m"));
                                //        double xn = Convert.ToDouble(xN);
                                //        xNum += xn;
                                //    }

                                //}
                                //else
                                //{
                                //    if (kv1.Value[0].GetPropertyValue("LENGTH") != null)
                                //    {
                                //        xNum += kv1.Value[0]["LENGTH"].DoubleValue / 1000;
                                //    }
                                //}

                                string dw = "";
                                double dz = 0, sl = 0;

                                if (b)
                                {
                                    double dry_weight = 0;
                                    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                        if (dry_weight < 0.005)
                                            dry_weight = 0;
                                        dz = dry_weight;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                            if (dry_weight < 0.005)
                                                dry_weight = 0;
                                            dz = dry_weight;
                                        }
                                    }
                                    dw = "米";
                                    //unit = "米";
                                    double length = 0;
                                    foreach (IECInstance iec2 in kv1.Value)
                                    {
                                        length += iec2["LENGTH"].DoubleValue / 1000;
                                        //dry_weight = iec.Value[0]["DRY_WEIGHT"].DoubleValue * 1000;
                                    }
                                    //qty = length;
                                    double totalweight = length * dry_weight;
                                    zgNum = length;
                                    //sl = length;
                                    xzz += totalweight;
                                    zgzz = totalweight;
                                }
                                else
                                {
                                    double dry_weight = 0;
                                    if (kv1.Value[0].GetPropertyValue("CERI_WEIGHT_DRY") != null)
                                    {
                                        dry_weight = kv1.Value[0]["CERI_WEIGHT_DRY"].DoubleValue;
                                    }
                                    else
                                    {
                                        if (kv1.Value[0].GetPropertyValue("DRY_WEIGHT") != null)
                                        {
                                            dry_weight = kv1.Value[0]["DRY_WEIGHT"].DoubleValue;
                                        }
                                    }
                                    if (dry_weight < 0.005)
                                        dry_weight = 0;
                                    //sl = kv1.Value.Count;
                                    dz = dry_weight * kv1.Value.Count;
                                    dw = "个";
                                    xzz += dry_weight;
                                }

                                string wlbm = "", xtbh = "", xtmc = "", clmc = "", bzh = "", xtljfs = "", sjljfs = "", yldj = "", ggxh = "", gdcz = "", azbw = "", sfmd = "", gnbff = "", gnbffjtyq = "", cxdj = "", dqyqzl = "", zjqyqzl = "", mqyqzl = "", bwcl = "", fhcl = "", gg = "", tsbl = "", tsfs = "", ylsy = "", scx = "", kqcs = "", zqcs = "", jx = "", sx = "", gdtz = "", yqx = "";
                                double bwhd = 0;
                                int dqbs = 0, zjqbs = 0, mqbs = 0;
                                if (kv1.Value[0].GetPropertyValue("CERI_MAT_GRADE") != null)
                                {
                                    wlbm = kv1.Value[0]["CERI_MAT_GRADE"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("GRADE") != null)
                                    {
                                        wlbm = kv1.Value[0]["GRADE"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_System_Number") != null)
                                {
                                    xtbh = kv1.Value[0]["CERI_System_Number"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("UNIT") != null)
                                    {
                                        xtbh = kv1.Value[0]["UNIT"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_System_Name") != null)
                                {
                                    xtmc = kv1.Value[0]["CERI_System_Name"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SERVICE") != null)
                                    {
                                        xtmc = kv1.Value[0]["SERVICE"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kv1.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kv1.Value[0]["SHORT_DESCRIPTION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kv1.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kv1.Value[0]["CATALOG_NAME"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_END_COND_1") != null)
                                {
                                    string ljxszh = kv1.Value[0]["CERI_END_COND_1"].StringValue;
                                    xtljfs = ljxszh;
                                    string ljxs2 = kv1.Value[0]["CERI_END_COND_2"].StringValue;
                                    if (ljxs2 != "" && ljxs2 != null)
                                    {
                                        xtljfs += "," + ljxs2;
                                    }
                                    string ljxs3 = kv1.Value[0]["CERI_END_COND_3"].StringValue;
                                    if (ljxs3 != "" && ljxs3 != null)
                                    {
                                        xtljfs += "," + ljxs3;
                                    }
                                }

                                if (kv1.Value[0].GetPropertyValue("CERI_Connection_Type") != null)
                                {
                                    sjljfs = kv1.Value[0]["CERI_Connection_Type"].StringValue;
                                }

                                Dictionary<string, string> dicStr = OPM_Public_Api.displayConnection();

                                if (dicStr == null)
                                {
                                    sjljfs = xtljfs;
                                }
                                else
                                {
                                    sjljfs = "";

                                    char[] separator = { ',' };

                                    string[] arr = xtljfs.Split(separator);

                                    foreach (var a in arr)
                                    {
                                        if (dicStr.Keys.Contains(a))
                                        {
                                            string strValue = dicStr[a];
                                            if (sjljfs.Equals(""))
                                            {
                                                sjljfs += strValue;
                                            }
                                            else
                                            {
                                                sjljfs = sjljfs + "," + strValue;
                                            }
                                        }
                                    }
                                }

                                if (kv1.Value[0].GetPropertyValue("CERI_Pressure_Rating") != null)
                                {
                                    yldj = kv1.Value[0]["CERI_Pressure_Rating"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("SPECIFICATION") != null)
                                    {
                                        yldj = kv1.Value[0]["SPECIFICATION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_MAIN_SIZE") != null)
                                {
                                    ggxh = kv1.Value[0]["CERI_MAIN_SIZE"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    gdcz = kv1.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        gdcz = kv1.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Installation_Site") != null)
                                {
                                    azbw = kv1.Value[0]["CERI_Installation_Site"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Buried") != null)
                                {
                                    sfmd = kv1.Value[0]["CERI_Buried"].StringValue;
                                }
                                sl = zjshulian(kv1.Value);
                                if (kv1.Value[0].GetPropertyValue("CERI_Pipe_Lining_Anticorrosion") != null)
                                {
                                    gnbff = kv1.Value[0]["CERI_Pipe_Lining_Anticorrosion"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Specification") != null)
                                {
                                    gnbffjtyq = kv1.Value[0]["CERI_Specification"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Derusting_Grade") != null)
                                {
                                    cxdj = kv1.Value[0]["CERI_Derusting_Grade"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Primer_Paint") != null)
                                {
                                    dqyqzl = kv1.Value[0]["CERI_Primer_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Primer_Pass") != null)
                                {
                                    dqbs = kv1.Value[0]["CERI_Primer_Pass"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Intermediate_Paint") != null)
                                {
                                    zjqyqzl = kv1.Value[0]["CERI_Intermediate_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Middle_Number") != null)
                                {
                                    zjqbs = kv1.Value[0]["CERI_Middle_Number"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Topcoat_Paint") != null)
                                {
                                    mqyqzl = kv1.Value[0]["CERI_Topcoat_Paint"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Finish_Times") != null)
                                {
                                    mqbs = kv1.Value[0]["CERI_Finish_Times"].IntValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Material") != null)
                                {
                                    bwcl = kv1.Value[0]["CERI_Insulation_Material"].StringValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("INSULATION") != null)
                                    {
                                        bwcl = kv1.Value[0]["INSULATION"].StringValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                                {
                                    bwhd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                                }
                                else
                                {
                                    if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                                    {
                                        bwhd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                                    }
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Shielding_Material") != null)
                                {
                                    fhcl = kv1.Value[0]["CERI_Shielding_Material"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Protection_Specification") != null)
                                {
                                    gg = kv1.Value[0]["CERI_Protection_Specification"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Percentage_Detection") != null)
                                {
                                    tsbl = kv1.Value[0]["CERI_Percentage_Detection"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Inspection_Way") != null)
                                {
                                    tsfs = kv1.Value[0]["CERI_Inspection_Way"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Pressure_Testing") != null)
                                {
                                    ylsy = kv1.Value[0]["CERI_Pressure_Testing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Water_Washing") != null)
                                {
                                    scx = kv1.Value[0]["CERI_Water_Washing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Air_Purge") != null)
                                {
                                    kqcs = kv1.Value[0]["CERI_Air_Purge"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Steam_Blowing") != null)
                                {
                                    zqcs = kv1.Value[0]["CERI_Steam_Blowing"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Alkali_Wash") != null)
                                {
                                    jx = kv1.Value[0]["CERI_Alkali_Wash"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Acid_Pickling") != null)
                                {
                                    sx = kv1.Value[0]["CERI_Acid_Pickling"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Pipeline_Skim") != null)
                                {
                                    gdtz = kv1.Value[0]["CERI_Pipeline_Skim"].StringValue;
                                }
                                if (kv1.Value[0].GetPropertyValue("CERI_Oil_Cleaning") != null)
                                {
                                    yqx = kv1.Value[0]["CERI_Oil_Cleaning"].StringValue;
                                }
                                IRow nextRow = sheet.GetRow(row);
                                ICell cell2column = nextRow.CreateCell(2);
                                cell2column.SetCellValue(wlbm);
                                //s.Cells[row, 3] = wlbm;
                                ICell cell3column = nextRow.CreateCell(3);
                                cell3column.SetCellValue(xtbh);
                                //s.Cells[row, 4] = xtbh;
                                ICell cell4column = nextRow.CreateCell(4);
                                cell4column.SetCellValue(xtmc);
                                //s.Cells[row, 5] = xtmc;
                                ICell cell5column = nextRow.CreateCell(5);
                                cell5column.SetCellValue(clmc);
                                //s.Cells[row, 6] = clmc;
                                ICell cell6column = nextRow.CreateCell(6);
                                cell6column.SetCellValue(bzh);
                                //s.Cells[row, 7] = bzh;
                                ICell cell7column = nextRow.CreateCell(7);
                                cell7column.SetCellValue(xtljfs);
                                //s.Cells[row, 8] = xtljfs;
                                ICell cell8column = nextRow.CreateCell(8);
                                cell8column.SetCellValue(sjljfs);
                                //s.Cells[row, 9] = sjljfs;
                                ICell cell9column = nextRow.CreateCell(9);
                                cell9column.SetCellValue(yldj);
                                //s.Cells[row, 10] = yldj;
                                ICell cell10column = nextRow.CreateCell(10);
                                cell10column.SetCellValue(ggxh);
                                //s.Cells[row, 11] = ggxh;
                                ICell cell11column = nextRow.CreateCell(11);
                                cell11column.SetCellValue(gdcz);
                                //s.Cells[row, 12] = gdcz;
                                ICell cell12column = nextRow.CreateCell(12);
                                cell12column.SetCellValue(azbw);
                                //s.Cells[row, 13] = azbw;
                                ICell cell13column = nextRow.CreateCell(13);
                                cell13column.SetCellValue(sfmd);

                                ICell cell14column = nextRow.CreateCell(14);
                                ICell cell15column = nextRow.CreateCell(15);
                                //s.Cells[row, 14] = sfmd;
                                ICell cell16column = nextRow.CreateCell(16);
                                cell16column.SetCellValue(sl);
                                //s.Cells[row, 17] = sl;
                                ICell cell17column = nextRow.CreateCell(17);
                                cell17column.SetCellValue(Math.Round(dz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 18] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                ICell cell18column = nextRow.CreateCell(18);
                                cell18column.SetCellValue(dw);
                                //s.Cells[row, 19] = dw;
                                ICell cell19column = nextRow.CreateCell(19);
                                cell19column.SetCellValue(Math.Round(xNum, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 20] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                                ICell cell20column = nextRow.CreateCell(20);
                                cell20column.SetCellValue(Math.Round(xzz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 21] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                                ICell cell21column = nextRow.CreateCell(21);
                                cell21column.SetCellValue(Math.Round(zgNum, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 22] = Math.Round((decimal)zgNum, 2, MidpointRounding.AwayFromZero); 
                                ICell cell22column = nextRow.CreateCell(22);
                                cell22column.SetCellValue(Math.Round(zgzz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 23] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                                ICell cell23column = nextRow.CreateCell(23);
                                cell23column.SetCellValue(gnbff);
                                //s.Cells[row, 24] = gnbff;
                                ICell cell24column = nextRow.CreateCell(24);
                                cell24column.SetCellValue(gnbffjtyq);
                                //s.Cells[row, 25] = gnbffjtyq;
                                ICell cell25column = nextRow.CreateCell(25);
                                cell25column.SetCellValue(cxdj);
                                //s.Cells[row, 26] = cxdj;
                                ICell cell26column = nextRow.CreateCell(26);
                                cell26column.SetCellValue(dqyqzl);
                                //s.Cells[row, 27] = dqyqzl;
                                ICell cell27column = nextRow.CreateCell(27);
                                cell27column.SetCellValue(dqbs);
                                //s.Cells[row, 28] = dqbs;
                                ICell cell28column = nextRow.CreateCell(28);
                                cell28column.SetCellValue(zjqyqzl);
                                //s.Cells[row, 29] = zjqyqzl;
                                ICell cell29column = nextRow.CreateCell(29);
                                cell29column.SetCellValue(zjqbs);
                                //s.Cells[row, 30] = zjqbs;
                                ICell cell30column = nextRow.CreateCell(30);
                                cell30column.SetCellValue(mqyqzl);
                                //s.Cells[row, 31] = mqyqzl;
                                ICell cell31column = nextRow.CreateCell(31);
                                cell31column.SetCellValue(mqbs);
                                //s.Cells[row, 32] = mqbs;
                                ICell cell32column = nextRow.CreateCell(32);
                                cell32column.SetCellValue(bwcl);
                                //s.Cells[row, 33] = bwcl;
                                ICell cell33column = nextRow.CreateCell(33);
                                cell33column.SetCellValue(bwhd);
                                //s.Cells[row, 34] = bwhd;
                                ICell cell34column = nextRow.CreateCell(34);
                                cell34column.SetCellValue(fhcl);
                                //s.Cells[row, 35] = fhcl;
                                ICell cell35column = nextRow.CreateCell(35);
                                cell35column.SetCellValue(gg);
                                //s.Cells[row, 36] = gg;
                                ICell cell36column = nextRow.CreateCell(36);
                                cell36column.SetCellValue(tsbl);
                                //s.Cells[row, 37] = tsbl;
                                ICell cell37column = nextRow.CreateCell(37);
                                cell37column.SetCellValue(tsfs);
                                //s.Cells[row, 38] = tsfs;
                                ICell cell38column = nextRow.CreateCell(38);
                                cell38column.SetCellValue(ylsy);
                                //s.Cells[row, 39] = ylsy;
                                ICell cell39column = nextRow.CreateCell(39);
                                cell39column.SetCellValue(scx);
                                //s.Cells[row, 40] = scx;
                                ICell cell40column = nextRow.CreateCell(40);
                                cell40column.SetCellValue(kqcs);
                                //s.Cells[row, 41] = kqcs;
                                ICell cell41column = nextRow.CreateCell(41);
                                cell41column.SetCellValue(zqcs);
                                //s.Cells[row, 42] = zqcs;
                                ICell cell42column = nextRow.CreateCell(42);
                                cell42column.SetCellValue(jx);
                                //s.Cells[row, 43] = jx;
                                ICell cell43column = nextRow.CreateCell(43);
                                cell43column.SetCellValue(sx);
                                //s.Cells[row, 44] = sx;
                                ICell cell44column = nextRow.CreateCell(44);
                                cell44column.SetCellValue(gdtz);
                                //s.Cells[row, 45] = gdtz;
                                ICell cell46column = nextRow.CreateCell(45);
                                cell46column.SetCellValue(yqx);
                                //s.Cells[row, 46] = yqx;
                            }
                            #endregion
                            if (min == double.MaxValue)
                            {
                                sheet.GetRow(irow).GetCell(14).SetCellValue("");
                                //s.Cells[irow, 15] = "";
                            }
                            else
                            {
                                sheet.GetRow(irow).GetCell(14).SetCellValue(min);
                                //s.Cells[irow, 15] = min;
                            }
                            if (max == double.MinValue)
                            {
                                sheet.GetRow(irow).GetCell(15).SetCellValue(min);
                                //s.Cells[irow, 16] = "";
                            }
                            else
                            {
                                sheet.GetRow(irow).GetCell(16).SetCellValue(max);
                                //s.Cells[irow, 16] = max;
                            }
                            #region 总和
                            //s.Cells[irow, 20] = Math.Round((decimal)xNum, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r = s.Cells[irow, 20];
                            //r.NumberFormatLocal = "@";
                            //s.Cells[irow, 21] = Math.Round((decimal)xzz, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r1 = s.Cells[irow, 21];
                            //r1.NumberFormatLocal = "@";
                            //s.Cells[irow, 22] =Math.Round((decimal)zgNum,2,MidpointRounding.AwayFromZero);
                            //MOIE.Range r2 = s.Cells[irow, 22];
                            //r2.NumberFormatLocal = "@";
                            //s.Cells[irow, 23] = Math.Round((decimal)zgzz, 2, MidpointRounding.AwayFromZero);
                            //MOIE.Range r3 = s.Cells[irow, 23];
                            //r3.NumberFormatLocal = "@";
                            #endregion
                        }
                    }
                    int kongbaihangCount = sheet.LastRowNum - (row);
                    if (kongbaihangCount > 0)
                    {
                        int m = 1;
                        int n = 2;
                        while (kongbaihangCount >= m * n)
                        {
                            sheet.ShiftRows(sheet.LastRowNum + 1, sheet.LastRowNum + n, -n);
                            m++;
                        }
                    }
                    //wb.Save();
                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                    #region
                    //int row = 7; //行号
                    //int lineindex = 0; //序号
                    //foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                    //{
                    //    row++;
                    //    lineindex++;
                    //    s.Cells[row, 1] = lineindex;
                    //    bool biaoshi = true;//避免重复添加管线编号
                    //    //s.Cells[row, 2] = kv.Value.Values[0]["LINENUMBER"].StringValue;
                    //    foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                    //    {
                    //        if (biaoshi)
                    //        {
                    //            s.Cells[row, 2] = kv1.Value[0]["LINENUMBER"].StringValue;
                    //            biaoshi = false;
                    //        }
                    //        row++;
                    //        string jzmc = "", wd = "", dh = "", cz = "";
                    //        double pod = 0, hd = 0, dlcd = 0, mj = 0, tj = 0;
                    //        int cs = 0;
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Media_Name") != null)
                    //        {
                    //            jzmc = kv1.Value[0]["CERI_Media_Name"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_PIPE_OD_M") != null)
                    //        {
                    //            pod = kv1.Value[0]["CERI_PIPE_OD_M"].DoubleValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("OUTSIDE_DIAMETER") != null)
                    //            {
                    //                pod = kv1.Value[0]["OUTSIDE_DIAMETER"].DoubleValue;
                    //            }
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_TEMP") != null)
                    //        {
                    //            wd = kv1.Value[0]["CERI_TEMP"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_code") != null)
                    //        {
                    //            dh = kv1.Value[0]["CERI_Insulation_code"].StringValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Materia") != null)
                    //        {
                    //            cz = kv1.Value[0]["CERI_Insulation_Materia"].StringValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("MATERIAL") != null)
                    //            {
                    //                cz = kv1.Value[0]["MATERIAL"].StringValue;
                    //            }
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Layer") != null)
                    //        {
                    //            cs = kv1.Value[0]["CERI_Layer"].IntValue;
                    //        }
                    //        if (kv1.Value[0].GetPropertyValue("CERI_Insulation_Thickness") != null)
                    //        {
                    //            hd = kv1.Value[0]["CERI_Insulation_Thickness"].DoubleValue;
                    //        }
                    //        else
                    //        {
                    //            if (kv1.Value[0].GetPropertyValue("INSULATION_THICKNESS") != null)
                    //            {
                    //                hd = kv1.Value[0]["INSULATION_THICKNESS"].DoubleValue;
                    //            }
                    //        }
                    //        foreach (IECInstance iec1 in kv1.Value)
                    //        {
                    //            if (iec1.GetPropertyValue("CERI_LENGTH") != null)
                    //            {
                    //                dlcd += iec1["CERI_LENGTH"].DoubleValue;
                    //            }
                    //            else
                    //            {
                    //                if (iec1.GetPropertyValue("LENGTH") != null)
                    //                {
                    //                    dlcd += iec1["LENGTH"] / 1000;
                    //                }
                    //            }
                    //            if (iec1.GetPropertyValue("CERI_Area") != null)
                    //            {
                    //                mj += iec1["CERI_Area"].DoubleValue;
                    //            }
                    //            if (iec1.GetPropertyValue("CERI_Volume") != null)
                    //            {
                    //                tj += iec1["CERI_Volume"].DoubleValue;
                    //            }
                    //        }
                    //        s.Cells[row, 3] = jzmc;
                    //        s.Cells[row, 4] = pod;
                    //        s.Cells[row, 5] = dlcd;
                    //        s.Cells[row, 6] = wd;
                    //        s.Cells[row, 7] = dh;
                    //        s.Cells[row, 10] = cs;
                    //        s.Cells[row, 11] = hd;
                    //        s.Cells[row, 12] = mj;
                    //        s.Cells[row, 13] = tj;
                    //        //s.Cells[row, 13] = catalog_name;
                    //        //s.Cells[row, 14] = media;
                    //    }
                    //}
                    //wb.Save();
                    //wb.Close();
                    //wkb.Close();
                    //app.Quit();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                    #endregion
                }
                //waitDialog.Close();
                //wb.Close();
                //wkb.Close();
                //app.Quit();

                //KillP.Kill(app);
                // 写入到客户端  
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    workbook.Write(ms);
                    using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] data = ms.ToArray();
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                    }
                    waitDialog.Close();

                }

                openPath = excelPath;
            }
        }

        public void hzExportExcel(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\汇总材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\汇总材料表：下没有找到" + yName + "文件");
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
                s = (MOIE.Worksheet)wb.Worksheets["压力管道材料汇总表-国内"];
                if (itemList.Count > 0)
                {
                    if (wlbmGroupcheckBox1.Checked)
                    {
                        //bool isWl = true;
                        int index = 0, row = 6;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            index++;
                            row++;
                            s.Cells[row, 1] = index;
                            string wlbm = "";
                            if (kv.Value[0][0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm = kv.Value[0][0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kv.Value[0][0].GetPropertyValue("GRADE") != null)
                                {
                                    wlbm = kv.Value[0][0]["GRADE"].StringValue;
                                }
                            }
                            s.Cells[row, 2] = wlbm;
                            int irow = 0;
                            bool isK = true;//为false合并总重单元格
                            if (wlbm != "" && wlbm != " ")
                            {
                                isK = false;
                            }
                            if (!isK)
                            {
                                irow = row + 1;
                                MOIE.Range r = s.Range[s.Cells[irow, 11], s.Cells[row + kv.Value.Count, 11]];
                                r.Application.DisplayAlerts = false;
                                r.Merge(Type.Missing);
                                r.Application.DisplayAlerts = true;
                            }
                            double zzW = 0;
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                row++;
                                string clmc = "", xh = "", gg = "", cz = "", dw = "", bzh = "", bz = "";
                                double dz = 0;
                                double nums = 0, zz = 0;
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
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
                                if (kvs.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    cz = kvs.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        cz = kvs.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kvs.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kvs.Value[0]["CATALOG_NAME"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("NOTES") != null)
                                {
                                    bz = kvs.Value[0]["NOTES"].StringValue;
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
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        //length += iec2["LENGTH"].DoubleValue / 1000;
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                    dw = "米";
                                    zzW += zz;
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
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                    zzW += zz;
                                    dw = "个";
                                }

                                s.Cells[row, 3] = clmc;
                                s.Cells[row, 4] = xh;
                                s.Cells[row, 5] = gg;
                                s.Cells[row, 7] = cz;
                                s.Cells[row, 8] = dw;
                                s.Cells[row, 9] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 10] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                if (isK)
                                {
                                    s.Cells[row, 11] = zz;
                                }
                                s.Cells[row, 12] = bzh;
                                s.Cells[row, 14] = bz;
                            }
                            if (!isK)
                            {
                                s.Cells[irow, 11] = Math.Round((decimal)zzW, 2, MidpointRounding.AwayFromZero);
                            }
                        }
                    }
                    else
                    {
                        int index = 0, row = 6;
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                index++;
                                row++;
                                s.Cells[row, 1] = index;
                                string wlbm = "";
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
                                s.Cells[row, 2] = wlbm;
                                string clmc = "", xh = "", gg = "", cz = "", dw = "", bzh = "", bz = "";
                                double dz = 0;
                                double nums = 0, zz = 0;
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
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
                                if (kvs.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    cz = kvs.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        cz = kvs.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kvs.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kvs.Value[0]["CATALOG_NAME"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("NOTES") != null)
                                {
                                    bz = kvs.Value[0]["NOTES"].StringValue;
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
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        //length += iec2["LENGTH"].DoubleValue / 1000;
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                    dw = "米";
                                    //zzW += zz;
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
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                    //zzW += zz;
                                    dw = "个";
                                }

                                s.Cells[row, 3] = clmc;
                                s.Cells[row, 4] = xh;
                                s.Cells[row, 5] = gg;
                                s.Cells[row, 7] = cz;
                                s.Cells[row, 8] = dw;
                                s.Cells[row, 9] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 10] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 11] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                s.Cells[row, 12] = bzh;
                                s.Cells[row, 14] = bz;
                            }
                        }
                    }
                    wb.Save();
                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                }
                waitDialog.Close();
                wb.Close();
                wkb.Close();
                app.Quit();

                KillP.Kill(app);

                openPath = excelPath;
            }
        }

        public void hzExportExcel1(Dictionary<int, Dictionary<int, List<IECInstance>>> itemList)
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
                    File.Copy(path0 + "\\JYXConfig\\汇总材料表\\" + yName, excelPath);
                }
                catch
                {
                    MessageBox.Show("路径" + path0 + "\\JYXConfig\\汇总材料表：下没有找到" + yName + "文件");
                    return;
                }

                Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
                waitDialog.SetTitleString("导出EXCEL");
                waitDialog.SetInformationSting(zlcomboBox1.Text);
                waitDialog.Show();

                //app = new MOIE.Application();
                //MOIE.Workbooks wkb = app.Workbooks;
                //MOIE.Workbook wb = wkb.Open(excelPath);
                ////app.Visible = true;
                ////wb=wkb.Open(excelPath1);
                IWorkbook workbook;
                using (FileStream file = new FileStream(excelPath, FileMode.Open, FileAccess.Read))
                {
                    workbook = new XSSFWorkbook(file);

                }
                //MOIE.Worksheet s;
                ISheet sheet;
                sheet = workbook.GetSheet("压力管道材料汇总表-国内");
                if (itemList.Count > 0)
                {
                    int index = 0, row = 5;
                    if (wlbmGroupcheckBox1.Checked)
                    {
                        //bool isWl = true;
                        
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            index++;
                            row++;
                            IRow dataRow = sheet.CreateRow(row); //第7行开始编辑
                            ICell cell7Row0Column = dataRow.CreateCell(0);
                            cell7Row0Column.SetCellValue(index);
                            //s.Cells[row, 1] = index;
                            string wlbm = "";
                            if (kv.Value[0][0].GetPropertyValue("CERI_MAT_GRADE") != null)
                            {
                                wlbm = kv.Value[0][0]["CERI_MAT_GRADE"].StringValue;
                            }
                            else
                            {
                                if (kv.Value[0][0].GetPropertyValue("GRADE") != null)
                                {
                                    wlbm = kv.Value[0][0]["GRADE"].StringValue;
                                }
                            }
                            ICell cell7Row1Column = dataRow.CreateCell(1);
                            cell7Row1Column.SetCellValue(wlbm);
                            //s.Cells[row, 2] = wlbm;
                            int irow = 0;
                            bool isK = true;//为false合并总重单元格
                            if (wlbm != "" && wlbm != " ")
                            {
                                isK = false;
                            }
                            if (!isK)
                            {
                                irow = row + 1;
                                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(irow, irow + kv.Value.Count, 10, 10));
                                //MOIE.Range r = s.Range[s.Cells[irow, 11], s.Cells[row + kv.Value.Count, 11]];
                                //r.Application.DisplayAlerts = false;
                                //r.Merge(Type.Missing);
                                //r.Application.DisplayAlerts = true;
                            }
                            double zzW = 0;
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                row++;
                                string clmc = "", xh = "", gg = "", cz = "", dw = "", bzh = "", bz = "";
                                double dz = 0;
                                double nums = 0, zz = 0;
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
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
                                if (kvs.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    cz = kvs.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        cz = kvs.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kvs.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kvs.Value[0]["CATALOG_NAME"].StringValue;
                                    }
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
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        //length += iec2["LENGTH"].DoubleValue / 1000;
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                    dw = "米";
                                    zzW += zz;
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
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                    zzW += zz;
                                    dw = "个";
                                }
                                IRow nextRow = sheet.CreateRow(row);
                                ICell cell2column = nextRow.CreateCell(2);
                                cell2column.SetCellValue(clmc);
                                //s.Cells[row, 3] = clmc;
                                ICell cell3column = nextRow.CreateCell(3);
                                cell3column.SetCellValue(xh);
                                //s.Cells[row, 4] = xh;
                                ICell cell4column = nextRow.CreateCell(4);
                                cell4column.SetCellValue(gg);
                                //s.Cells[row, 5] = gg;
                                ICell cell6column = nextRow.CreateCell(6);
                                cell6column.SetCellValue(cz);
                                //s.Cells[row, 7] = cz;
                                ICell cell7column = nextRow.CreateCell(7);
                                cell7column.SetCellValue(dw);
                                //s.Cells[row, 8] = dw;
                                ICell cell8column = nextRow.CreateCell(8);
                                cell8column.SetCellValue(Math.Round(nums, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 9] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                ICell cell9column = nextRow.CreateCell(9);
                                cell9column.SetCellValue(Math.Round(dz, 2, MidpointRounding.AwayFromZero));

                                ICell cell10column = nextRow.CreateCell(10);
                                //s.Cells[row, 10] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                if (isK)
                                {
                                    cell10column.SetCellValue(zz);
                                    //s.Cells[row, 11] = zz;
                                }
                                ICell cell11column = nextRow.CreateCell(11);
                                cell11column.SetCellValue(bzh);
                                //s.Cells[row, 12] = bzh;
                                ICell cell13column = nextRow.CreateCell(13);
                                cell13column.SetCellValue(bz);
                                //s.Cells[row, 14] = bz;
                            }
                            if (!isK)
                            {
                                sheet.GetRow(irow).GetCell(10).SetCellValue(Math.Round(zzW, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[irow, 11] = Math.Round((decimal)zzW, 2, MidpointRounding.AwayFromZero);
                            }
                        }
                    }
                    else
                    {
                        
                        foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                        {
                            foreach (KeyValuePair<int, List<IECInstance>> kvs in kv.Value)
                            {
                                index++;
                                row++;
                                IRow dataRow = sheet.CreateRow(row); //第7行开始编辑
                                ICell cell7Row0Column = dataRow.CreateCell(0);
                                cell7Row0Column.SetCellValue(index);
                                //s.Cells[row, 1] = index;
                                string wlbm = "";
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
                                ICell cell7Row1Column = dataRow.CreateCell(1);
                                cell7Row1Column.SetCellValue(wlbm);
                                //s.Cells[row, 2] = wlbm;
                                string clmc = "", xh = "", gg = "", cz = "", dw = "", bzh = "", bz = "";
                                double dz = 0;
                                double nums = 0, zz = 0;
                                if (kvs.Value[0].GetPropertyValue("CERI_SHORT_DESC") != null)
                                {
                                    clmc = kvs.Value[0]["CERI_SHORT_DESC"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("SHORT_DESCRIPTION") != null)
                                    {
                                        clmc = kvs.Value[0]["SHORT_DESCRIPTION"].StringValue;
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
                                if (kvs.Value[0].GetPropertyValue("CERI_MATERIAL") != null)
                                {
                                    cz = kvs.Value[0]["CERI_MATERIAL"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("MATERIAL") != null)
                                    {
                                        cz = kvs.Value[0]["MATERIAL"].StringValue;
                                    }
                                }
                                if (kvs.Value[0].GetPropertyValue("CERI_CATALOG") != null)
                                {
                                    bzh = kvs.Value[0]["CERI_CATALOG"].StringValue;
                                }
                                else
                                {
                                    if (kvs.Value[0].GetPropertyValue("CATALOG_NAME") != null)
                                    {
                                        bzh = kvs.Value[0]["CATALOG_NAME"].StringValue;
                                    }
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
                                    foreach (IECInstance iecz in kvs.Value)
                                    {
                                        //length += iec2["LENGTH"].DoubleValue / 1000;
                                        if (iecz.GetPropertyValue("LENGTH") != null)
                                        {
                                            nums += iecz["LENGTH"].DoubleValue / 1000;
                                        }
                                    }
                                    zz = dz * nums;
                                    dw = "米";
                                    //zzW += zz;
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
                                    nums = kvs.Value.Count;
                                    zz = dz * nums;
                                    //zzW += zz;
                                    dw = "个";
                                }
                                ICell cell7Row2Column = dataRow.CreateCell(2);
                                cell7Row2Column.SetCellValue(clmc);
                                //s.Cells[row, 3] = clmc;
                                ICell cell7Row3Column = dataRow.CreateCell(3);
                                cell7Row3Column.SetCellValue(xh);
                                //s.Cells[row, 4] = xh;
                                ICell cell7Row4Column = dataRow.CreateCell(4);
                                cell7Row4Column.SetCellValue(gg);
                                //s.Cells[row, 5] = gg;
                                ICell cell7Row6Column = dataRow.CreateCell(6);
                                cell7Row6Column.SetCellValue(cz);
                                //s.Cells[row, 7] = cz;
                                ICell cell7Row7Column = dataRow.CreateCell(7);
                                cell7Row7Column.SetCellValue(dw);
                                //s.Cells[row, 8] = dw;
                                ICell cell7Row8Column = dataRow.CreateCell(8);
                                cell7Row8Column.SetCellValue(Math.Round(nums, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 9] = Math.Round((decimal)nums, 2, MidpointRounding.AwayFromZero);
                                ICell cell7Row9Column = dataRow.CreateCell(9);
                                cell7Row9Column.SetCellValue(Math.Round(dz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 10] = Math.Round((decimal)dz, 2, MidpointRounding.AwayFromZero);
                                ICell cell7Row10Column = dataRow.CreateCell(10);
                                cell7Row10Column.SetCellValue(Math.Round(zz, 2, MidpointRounding.AwayFromZero));
                                //s.Cells[row, 11] = Math.Round((decimal)zz, 2, MidpointRounding.AwayFromZero);
                                ICell cell7Row11Column = dataRow.CreateCell(11);
                                cell7Row11Column.SetCellValue(bzh);
                                //s.Cells[row, 12] = bzh;
                                ICell cell7Row13Column = dataRow.CreateCell(13);
                                cell7Row13Column.SetCellValue(bz);
                                //s.Cells[row, 14] = bz;
                            }
                        }
                    }
                    int kongbaihangCount = sheet.LastRowNum - (row);
                    if (kongbaihangCount > 0)
                    {
                        int m = 1;
                        int n = 2;
                        while (kongbaihangCount >= m * n)
                        {
                            sheet.ShiftRows(sheet.LastRowNum + 1, sheet.LastRowNum + n, -n);
                            m++;
                        }
                    }
                    //wb.Save();
                    //wb = null;
                    //wkb = null;
                    //app = null;
                    //GC.Collect();
                    //MOIE.Application app1 = new MOIE.Application();
                    //MOIE.Workbooks wbs = app1.Workbooks;
                    //MOIE.Workbook wb1 = app1.Workbooks.Open(excelPath);
                    //app1.Visible = true;
                }
                //waitDialog.Close();
                //wb.Close();
                //wkb.Close();
                //app.Quit();

                //KillP.Kill(app);

                openPath = excelPath;
                // 写入到客户端  
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    workbook.Write(ms);
                    using (FileStream fs = new FileStream(excelPath, FileMode.Create, FileAccess.Write))
                    {
                        byte[] data = ms.ToArray();
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                    }
                    waitDialog.Close();

                }
            }
        }

        private void qxcheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool b = qxcheckBox1.Checked;
            mtcheckBox.Checked = b;
            otcheckBox.Checked = b;
            pecheckBox.Checked = b;
            cscheckBox.Checked = b;
            techeckBox1.Checked = b;
            ewcheckBox.Checked = b;
            fmcheckBox.Checked = b;
            ybcheckBox.Checked = b;
            flcheckBox.Checked = b;
            checkBox_gbZj.Checked = b;
            checkBox_yj.Checked = b;
            checkBox_jt.Checked = b;
        }

        public ECInstanceList shaixuanEc()
        {
            ECInstanceList sxecList = new ECInstanceList();
            ECInstanceList ecList = DgnUtilities.GetAllInstancesFromDgn();
            //IECClass iec_class = BMECInstanceManager.Instance.Schema.GetClass(value_map);
            ecList = attDisplayFil(ecList);

            if (radioButton2.Checked)
            {
                ecList = DgnUtilities.GetSelectedInstances();
            }

            if (dataGridView1.Rows.Count > 0)
            {
                List<string> pipeList = new List<string>();
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    bool isxz = Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value);
                    if (isxz)
                    {
                        pipeList.Add(dataGridView1.Rows[i].Cells["PipeLineName"].Value.ToString());
                    }
                }
                if (pipeList.Count == 0)
                {
                    MessageBox.Show("请至少选择一个管线编号！");
                    return null;
                }
                ECInstanceList pecList = new ECInstanceList();
                foreach (IECInstance iecp in ecList)
                {
                    if (iecp.GetPropertyValue("LINENUMBER") != null)
                    {
                        string pipeName = iecp["LINENUMBER"].StringValue;
                        bool isp = pipeList.Contains(pipeName);
                        if (isp)
                        {
                            pecList.Add(iecp);
                        }
                    }
                    string pipLine = "";
                    string gdn = "";
                    bool isgbzj = isGbZj(iecp, out pipLine, out gdn);
                    bool issx= pipeList.Contains(pipLine);
                    if (isgbzj&&issx)
                    {
                        pecList.Add(iecp);
                    }
                }
                ecList = pecList;
            }

            foreach (IECInstance iec in ecList)
            {
                #region
                //if (FlcheckBox.Checked)
                //{
                //    //bool b = BMECApi.Instance.InstanceDefinedAsClass(ec, "PIPING_COMPONENT", true);
                //    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_FLANGE", true);
                //    if (b)
                //    {
                //        bool iscz = sxecList.Contains(iec);
                //        if (!iscz)
                //        {
                //            sxecList.Add(iec);
                //        }
                //    }
                //}

                //if (gtcheckBox.Checked)
                //{
                //    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "GASKET", true);
                //    if (b)
                //    {
                //        bool iscz = sxecList.Contains(iec);
                //        if (!iscz)
                //        {
                //            sxecList.Add(iec);
                //        }
                //    }
                //}
                #endregion
                  if (flcheckBox.Checked)
                {
                    if (!isFl)
                    {
                        if (flNameList.Count == 0)
                        {
                            flNameList.Add("BLIND_FLANGE");
                        }
                    }
                    if (flNameList.Count > 0)
                    {
                        string ecName = iec.ClassDefinition.Name;
                        bool isFl = flNameList.Contains(ecName);
                        if (isFl)
                        {
                            bool iscz1 = sxecList.Contains(iec);
                            if (!iscz1)
                            {
                                sxecList.Add(iec);
              
                            }
                        }
                        else
                        {
                            foreach (string flName in flNameList)
                            {
                                bool b11 = BMECApi.Instance.InstanceDefinedAsClass(iec, flName, true);
                                if (b11)
                                {
                                    bool iscz1 = sxecList.Contains(iec);
                                    if (!iscz1)
                                    {
                                        sxecList.Add(iec);
                                    }
                                }
                            }
                        }
                    }
                }
                if (mtcheckBox.Checked)
                {
                    bool isgm = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_CAP", true);
                    if (isgm)
                    {
                        bool isczgm = sxecList.Contains(iec);
                        if (!isczgm)
                        {
                            sxecList.Add(iec);
                        }
                    }

                    IECInstance[] proInstanceList = iec.ClassDefinition.GetCustomAttributes();
                    IECInstance proInstance = null;
                    foreach (IECInstance iecInstance in proInstanceList)
                    {
                        if (iecInstance.ClassDefinition.Name.Equals("OpenPlant_3D_Catalogue_ClassProperties"))
                        {
                            proInstance = iecInstance;
                            break;
                        }
                    }
                    if (proInstance != null)
                    {
                        if (proInstance.GetPropertyValue("Table") != null)
                        {
                            string tableName = proInstance["Table"].StringValue;
                            string[] tableList = tableName.Split(';');
                            bool b = tableList.Contains("Misc_Fit");
                            bool bb = tableList.Contains("MISC_FIT");
                            if (b || bb)
                            {
                                bool iscz = sxecList.Contains(iec);
                                if (!iscz)
                                {
                                    sxecList.Add(iec);
                                }
                            }
                        }
                    }
                    bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_CERI_COUPLING", true);
                    bool b2 = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_COUPLING", true);
                    bool b3 = BMECApi.Instance.InstanceDefinedAsClass(iec, "Coupling_ZJGBSJT", true);
                    bool b4 = BMECApi.Instance.InstanceDefinedAsClass(iec, "Coupling_ZJJT", true);
                    if (b1 || b2 || b3 || b4)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (otcheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "Olet", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (pecheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            double ll = iec["LENGTH"].DoubleValue / 1000;
                            if (ll < 1.7976931348623157E+12) sxecList.Add(iec);
                        }
                    }
                }

                if (cscheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_CROSS", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (techeckBox1.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_TEE", true);
                    bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec, "BASKET_STRAINER", true);
                    if (b || b1)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (ewcheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_ELBOW", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (fmcheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "FLUID_REGULATOR", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }

                if (ybcheckBox.Checked)
                {
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(iec, "FLOW_METER", true);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }
                if (checkBox_gbZj.Checked)
                {
                    string pipLine = "";
                    string gdn = "";
                    bool b = isGbZj(iec, out pipLine, out gdn);
                    if (b)
                    {
                        bool iscz = sxecList.Contains(iec);
                        if (!iscz)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }
                if(checkBox_yj.Checked)
                {
                    bool by = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_REDUCER", true);
                    if (by)
                    {
                        bool isyj = sxecList.Contains(iec);
                        if (!isyj)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }
                if (checkBox_jt.Checked)
                {
                    bool by_coupling = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_COUPLING", true);
                    bool by_bellows = BMECApi.Instance.InstanceDefinedAsClass(iec, "BELLOWS", true);
                    bool by_pipe_adapter = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_ADAPTER", true);
                    bool by_pipe_nipple = BMECApi.Instance.InstanceDefinedAsClass(iec, "PIPE_NIPPLE", true);
                    bool by_strainer = BMECApi.Instance.InstanceDefinedAsClass(iec, "STRAINER", true);

                    if (by_coupling|| by_bellows|| by_pipe_adapter|| by_pipe_nipple|| by_strainer)
                    {
                        bool isjt = sxecList.Contains(iec);
                        if (!isjt)
                        {
                            sxecList.Add(iec);
                        }
                    }
                }
            }

            return sxecList;
        }

        public void initialization()
        {
            MsName.Add("隔热材料表");
            MsName.Add("管道材料表");
            MsName.Add("管道造价表");
            MsName.Add("管道造价汇总表");
            MsName.Add("汇总材料表");
            List<string> grclbList = new List<string>();
            grclbList.Add("CQ-B3004-1909-6A管道隔热材料表（国内）");
            grclbList.Add("CQ-B3004-1909-6B管道隔热材料表（海外）");
            grclbList.Add("CQ-B3004-1909-13A压力管道隔热材料表（国内）");
            msJtName.Add("隔热材料表", grclbList);
            List<string> gdclbList = new List<string>();
            gdclbList.Add("CQ-B3004-1909-5A管道材料表（国内）");
            gdclbList.Add("CQ-B3004-1909-5B管道材料表（海外）");
            gdclbList.Add("CQ-B3004-1909-12A压力管道材料表（国内）");
            msJtName.Add("管道材料表", gdclbList);
            List<string> gdzjbList = new List<string>();
            gdzjbList.Add("管道造价表");
            msJtName.Add("管道造价表", gdzjbList);
            List<string> gdzjhzbList = new List<string>();
            gdzjhzbList.Add("管道造价汇总表");
            msJtName.Add("管道造价汇总表", gdzjhzbList);
            List<string> hzclbList = new List<string>();
            hzclbList.Add("CQ-B3004-1909-9A压力管道材料汇总表（国内）");
            msJtName.Add("汇总材料表", hzclbList);
        }

        private void zlcomboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lxcomboBox1.DataSource = msJtName[zlcomboBox1.Text.ToString()];
            if (zlcomboBox1.Text.Equals("管道造价汇总表") || zlcomboBox1.Text.Equals("汇总材料表"))
            {
                wlbmGroupcheckBox1.Enabled = true;
            }
            else
            {
                wlbmGroupcheckBox1.Enabled = false;
            }
        }

        public bool grShaixuan(IECInstance iec, Dictionary<int, List<IECInstance>> iecgrList, out int key)
        {
            key = 0;
            bool isCz = true;
            string jzmc = "", wd = "", dh = "", cz = "";
            double pod = 0, hd = 0;
            int cs = 0;
            if (iec.GetPropertyValue("CERI_Media_Name") != null)
            {
                jzmc = iec["CERI_Media_Name"].StringValue;
            }
            if (iec.GetPropertyValue("CERI_PIPE_OD_M") != null)
            {
                pod = iec["CERI_PIPE_OD_M"].DoubleValue;
            }
            else
            {
                if (iec.GetPropertyValue("OUTSIDE_DIAMETER") != null)
                {
                    pod = iec["OUTSIDE_DIAMETER"].DoubleValue;
                }
            }
            if (iec.GetPropertyValue("CERI_TEMP") != null)
            {
                wd = iec["CERI_TEMP"].StringValue;
            }
            if (iec.GetPropertyValue("CERI_Insulation_code") != null)
            {
                dh = iec["CERI_Insulation_code"].StringValue;
            }
            if (iec.GetPropertyValue("CERI_Insulation_Material") != null)
            {
                cz = iec["CERI_Insulation_Material"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("INSULATION") != null)
                {
                    cz = iec["INSULATION"].StringValue;
                }
            }
            if (iec.GetPropertyValue("CERI_Layer") != null)
            {
                cs = iec["CERI_Layer"].IntValue;
            }
            if (iec.GetPropertyValue("CERI_Insulation_Thickness") != null)
            {
                hd = iec["CERI_Insulation_Thickness"].DoubleValue;
            }
            else
            {
                if (iec.GetPropertyValue("INSULATION_THICKNESS") != null)
                {
                    hd = iec["INSULATION_THICKNESS"].DoubleValue;
                }
            }
            foreach (KeyValuePair<int, List<IECInstance>> kv in iecgrList)
            {
                foreach (IECInstance pdiec in kv.Value)
                {
                    string jzmc1 = "", wd1 = "", dh1 = "", cz1 = "";
                    double pod1 = 0, hd1 = 0;
                    int cs1 = 0;
                    if (pdiec.GetPropertyValue("CERI_Media_Name") != null)
                    {
                        jzmc1 = pdiec["CERI_Media_Name"].StringValue;
                    }
                    if (pdiec.GetPropertyValue("CERI_PIPE_OD_M") != null)
                    {
                        pod1 = pdiec["CERI_PIPE_OD_M"].DoubleValue;
                    }
                    else
                    {
                        if (pdiec.GetPropertyValue("OUTSIDE_DIAMETER") != null)
                        {
                            pod1 = pdiec["OUTSIDE_DIAMETER"].DoubleValue;
                        }
                    }
                    if (pdiec.GetPropertyValue("CERI_TEMP") != null)
                    {
                        wd1 = pdiec["CERI_TEMP"].StringValue;
                    }
                    if (pdiec.GetPropertyValue("CERI_Insulation_code") != null)
                    {
                        dh1 = pdiec["CERI_Insulation_code"].StringValue;
                    }
                    if (pdiec.GetPropertyValue("CERI_Insulation_Material") != null)
                    {
                        cz1 = pdiec["CERI_Insulation_Material"].StringValue;
                    }
                    else
                    {
                        if (pdiec.GetPropertyValue("INSULATION") != null)
                        {
                            cz1 = pdiec["INSULATION"].StringValue;
                        }
                    }
                    if (pdiec.GetPropertyValue("CERI_Layer") != null)
                    {
                        cs1 = pdiec["CERI_Layer"].IntValue;
                    }
                    if (pdiec.GetPropertyValue("CERI_Insulation_Thickness") != null)
                    {
                        hd1 = pdiec["CERI_Insulation_Thickness"].DoubleValue;
                    }
                    else
                    {
                        if (pdiec.GetPropertyValue("INSULATION_THICKNESS") != null)
                        {
                            hd1 = pdiec["INSULATION_THICKNESS"].DoubleValue;
                        }
                    }
                    if (jzmc == jzmc1 && pod == pod1 && wd == wd1 && dh == dh1 && cz == cz1 && cs == cs1 && hd == hd1)
                    {
                        isCz = false;
                        key = kv.Key;
                    }
                }
            }
            return isCz;
        }

        public bool zjBj(IECInstance iec, IECInstance iec1)
        {
            bool isXt = false;
            //NOTES备注
            string xtbh = "", xtmc = "", clmc = "", ggxh = "", gdcz = "", dw = "", bz = "", wlbm = "";
            double dz = 0;
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
            if (iec.GetPropertyValue("CERI_System_Number") != null)
            {
                xtbh = iec["CERI_System_Number"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("UNIT") != null)
                {
                    xtbh = iec["UNIT"].StringValue;
                }
            }
            if (iec.GetPropertyValue("CERI_System_Name") != null)
            {
                xtmc = iec["CERI_System_Name"].StringValue;
            }
            if (iec.GetPropertyValue("CERI_SHORT_DESC") != null)
            {
                clmc = iec["CERI_SHORT_DESC"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("SHORT_DESCRIPTION") != null)
                {
                    clmc = iec["SHORT_DESCRIPTION"].StringValue;
                }
            }
            if (iec.GetPropertyValue("CERI_MAIN_SIZE") != null)
            {
                ggxh = iec["CERI_MAIN_SIZE"].StringValue;
            }
            if (iec.GetPropertyValue("CERI_MATERIAL") != null)
            {
                gdcz = iec["CERI_MATERIAL"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("MATERIAL") != null)
                {
                    gdcz = iec["MATERIAL"].StringValue;
                }
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

            string xtbh1 = "", xtmc1 = "", clmc1 = "", ggxh1 = "", gdcz1 = "", dw1 = "", bz1 = "", wlbm1 = "";
            double dz1 = 0;
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
            if (iec1.GetPropertyValue("CERI_System_Number") != null)
            {
                xtbh1 = iec1["CERI_System_Number"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("UNIT") != null)
                {
                    xtbh1 = iec1["UNIT"].StringValue;
                }
            }
            if (iec1.GetPropertyValue("CERI_System_Name") != null)
            {
                xtmc1 = iec1["CERI_System_Name"].StringValue;
            }
            if (iec1.GetPropertyValue("CERI_SHORT_DESC") != null)
            {
                clmc1 = iec1["CERI_SHORT_DESC"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("SHORT_DESCRIPTION") != null)
                {
                    clmc1 = iec1["SHORT_DESCRIPTION"].StringValue;
                }
            }
            if (iec1.GetPropertyValue("CERI_MAIN_SIZE") != null)
            {
                ggxh1 = iec1["CERI_MAIN_SIZE"].StringValue;
            }
            if (iec1.GetPropertyValue("CERI_MATERIAL") != null)
            {
                gdcz1 = iec1["CERI_MATERIAL"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("MATERIAL") != null)
                {
                    gdcz1 = iec1["MATERIAL"].StringValue;
                }
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
            //string xtbh1 = "", xtmc1 = "", clmc1 = "", ggxh1 = "", gdcz1 = "", dw1 = "", bz1 = "", wlbm1 = "";
            //double dz1 = 0;
            if (xtbh == xtbh1 && xtmc == xtmc1 && clmc == clmc1 && ggxh == ggxh1 && gdcz == gdcz1 && dw == dw1 && bz == bz1 && wlbm == wlbm1 && dz == dz1)
            {
                isXt = true;
            }
            return isXt;
        }

        public bool hzPd(IECInstance iec, IECInstance iec1)
        {
            bool isY = false;
            string clmc = "", xh = "", gg = "", cz = "", dw = "", bzh = "", bz = "";
            double dz = 0;
            if (iec.GetPropertyValue("CERI_SHORT_DESC") != null)
            {
                clmc = iec["CERI_SHORT_DESC"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("SHORT_DESCRIPTION") != null)
                {
                    clmc = iec["SHORT_DESCRIPTION"].StringValue;
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
            if (iec.GetPropertyValue("CERI_MATERIAL") != null)
            {
                cz = iec["CERI_MATERIAL"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("MATERIAL") != null)
                {
                    cz = iec["MATERIAL"].StringValue;
                }
            }
            if (iec.GetPropertyValue("CERI_CATALOG") != null)
            {
                bzh = iec["CERI_CATALOG"].StringValue;
            }
            else
            {
                if (iec.GetPropertyValue("CATALOG_NAME") != null)
                {
                    bzh = iec["CATALOG_NAME"].StringValue;
                }
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
            string clmc1 = "", xh1 = "", gg1 = "", cz1 = "", dw1 = "", bzh1 = "", bz1 = "";
            double dz1 = 0;
            if (iec1.GetPropertyValue("CERI_SHORT_DESC") != null)
            {
                clmc1 = iec1["CERI_SHORT_DESC"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("SHORT_DESCRIPTION") != null)
                {
                    clmc1 = iec1["SHORT_DESCRIPTION"].StringValue;
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
            if (iec1.GetPropertyValue("CERI_MATERIAL") != null)
            {
                cz1 = iec1["CERI_MATERIAL"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("MATERIAL") != null)
                {
                    cz1 = iec1["MATERIAL"].StringValue;
                }
            }
            if (iec1.GetPropertyValue("CERI_CATALOG") != null)
            {
                bzh1 = iec1["CERI_CATALOG"].StringValue;
            }
            else
            {
                if (iec1.GetPropertyValue("CATALOG_NAME") != null)
                {
                    bzh1 = iec1["CATALOG_NAME"].StringValue;
                }
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
            if (clmc == clmc1 && xh == xh1 && gg == gg1 && cz == cz1 && dw == dw1 && bzh == bzh1 && bz == bz1 && dz == dz1)
            {
                isY = true;
            }
            return isY;
        }

        public double zjshulian(List<IECInstance> zjIecList)
        {
            double zjsl = 0;
            if (zjIecList.Count > 0)
            {
                //bool b1 = BMECApi.Instance.InstanceDefinedAsClass(iec1, "PIPE", true);
                bool isP = BMECApi.Instance.InstanceDefinedAsClass(zjIecList[0], "PIPE", true);//判断是否是管道
                if (isP)
                {
                    foreach (IECInstance zjIec in zjIecList)
                    {
                        //length += iec2["LENGTH"].DoubleValue / 1000;
                        zjsl += zjIec["LENGTH"].DoubleValue / 1000;
                    }
                    //Math.Round((decimal)qty, 2, MidpointRounding.AwayFromZero);
                    zjsl = Convert.ToDouble(Math.Round((decimal)zjsl, 2, MidpointRounding.AwayFromZero));
                }
                else
                {
                    zjsl = zjIecList.Count;
                }
            }
            return zjsl;
        }

        private void ReportFrom_FormClosed(object sender, FormClosedEventArgs e)
        {
            flNameList.Clear();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReportFLForm flForm = ReportFLForm.instence();
#if DEBUG

#else
            flForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportFLForm));
            flForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            flForm.Show();
        }

        private void flcheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (flcheckBox.Checked)
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        private void flcheckBox_CheckedChanged_1(object sender, EventArgs e)
        {
            if (flcheckBox.Checked)
            {
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        /// <summary>
        /// 筛选掉隐藏的引用模型
        /// </summary>
        /// <param name="eclist"></param>
        /// <returns></returns>
        public static ECInstanceList attDisplayFil(ECInstanceList eclist)
        {
            ECInstanceList ecL = new ECInstanceList();

            foreach (IECInstance iec in eclist)
            {
                //Element el = JYX_ZYJC_CLR.PublicMethod.get_element_by_instance(iec);
                bool isdisplay = true;
                IDgnECInstance idgnEc = iec as IDgnECInstance;

                DgnModelRef dgnModelRef = idgnEc.ModelRef;
                if (dgnModelRef.IsDgnAttachment)
                {
                    DgnAttachment dgnAtt = dgnModelRef.AsDgnAttachment();

                    isdisplay = dgnAtt.IsDisplayed;
                }

                if (isdisplay) ecL.Add(iec);
                #region
                //if (el!=null)
                //{
                //    if(iec.ClassDefinition.Name.Equals("PIPE"))
                //    {
                //        Type type = iec.GetType();
                //        //DgnXA
                //        IDgnECInstance idgnEc = iec as IDgnECInstance;
                //        var proNames = type.GetProperties().Select(a => a.Name).ToArray();
                //        bool isdisplay = JYX_ZYJC_CLR.PublicMethod.get_display_by_instance(iec);
                //        DgnModelRef dgnRef = idgnEc.ModelRef;
                //        //if(isdisplay)
                //        //{
                //        //    ecL.Add(iec);
                //        //}
                //        //DgnModelRef modelRef = el.DgnModelRef;

                //        //if (!modelRef.IsDgnAttachment)
                //        //{
                //        //    ecL.Add(iec);
                //        //}
                //        //else
                //        //{
                //        //    DgnAttachment dgnAtt = modelRef.AsDgnAttachment();
                //        //    if (dgnAtt.IsDisplayed)
                //        //    {
                //        //        ecL.Add(iec);
                //        //    }
                //        //}
                //    }

                //}
                #endregion
            }

            return ecL;
        }

        public bool isXt(IECInstance iec1, IECInstance iec2)
        {
            bool isxt = false;
            //NOMINAL_DIAMETER,CERI_MAIN_SIZE,NOMINAL_DIAMETER_BRANCH_END,NOMINAL_DIAMETER_RUN_END

            if (iec1.ClassDefinition.Name == iec2.ClassDefinition.Name)
            {
                string pice1 = iec1["CERI_PIECE_MARK"].StringValue;
                string pice2 = iec2["CERI_PIECE_MARK"].StringValue;
                if (pice1==pice2)
                {
                    string ceriDn1 = "", ceriDn2 = "";
                    if (iec1.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    {
                        ceriDn1 = iec1["CERI_MAIN_SIZE"].StringValue;
                    }
                    if (iec2.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    {
                        ceriDn2 = iec2["CERI_MAIN_SIZE"].StringValue;
                    }
                    if (ceriDn1 != "" && ceriDn1 != " " && ceriDn2 != "" && ceriDn2 != " ")
                    {
                        if (ceriDn1.Equals(ceriDn2)) isxt = true;
                    }
                    else
                    {
                        BMECObject bmec1 = new BMECObject(iec1);
                        BMECObject bmec2 = new BMECObject(iec2);

                        if (bmec1 != null && bmec2 != null)
                        {
                            List<Port> portList1 = bmec1.Ports;
                            List<Port> portList2 = bmec2.Ports;
                            if (portList1.Count > 0 && (portList1.Count == portList2.Count))
                            {
                                if (portList1.Count == 1)
                                {
                                    double dn1 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec1);
                                    double dn11 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec2);
                                    if (dn1 == dn11) isxt = true;
                                }
                                else if (portList1.Count == 2)
                                {
                                    double dn1 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec1);
                                    double dn11 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec2);
                                    double dn2 = GetPropertyAsDouble("NOMINAL_DIAMETER_RUN_END", iec1);
                                    double dn22 = GetPropertyAsDouble("NOMINAL_DIAMETER_RUN_END", iec2);
                                    if (dn1 == dn11 && dn2 == dn22) isxt = true;
                                }
                                else
                                {
                                    double dn1 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec1);
                                    double dn11 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec2);
                                    double dn2 = GetPropertyAsDouble("NOMINAL_DIAMETER_RUN_END", iec1);
                                    double dn22 = GetPropertyAsDouble("NOMINAL_DIAMETER_RUN_END", iec2);
                                    double dn3 = GetPropertyAsDouble("NOMINAL_DIAMETER_BRANCH_END", iec1);
                                    double dn33 = GetPropertyAsDouble("NOMINAL_DIAMETER_BRANCH_END", iec2);
                                    if (dn1 == dn11 && dn2 == dn22 && dn3 == dn33) isxt = true;
                                }
                            }
                            else
                            {
                                if (iec1.GetPropertyValue("NOMINAL_DIAMETER") != null && iec2.GetPropertyValue("NOMINAL_DIAMETER") != null)
                                {
                                    double dn1 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec1);
                                    double dn11 = GetPropertyAsDouble("NOMINAL_DIAMETER", iec2);
                                    if (dn1 == dn11) isxt = true;
                                }
                            }
                        }
                    }
                }
                
            }
            return isxt;
        }

        //获取输入参数
        public static double GetPropertyAsDouble(string propertyName, IECInstance iec, bool reportMissing = true)
        {
            IECPropertyValue iECPropertyValue = iec.FindPropertyValue(propertyName, true, true, true);
            if (iECPropertyValue == null)
            {
                return 0.0;
            }
            if (!iECPropertyValue.IsNull)
            {
                return iECPropertyValue.DoubleValue;
            }
            return 0.0;
        }

        private void ReportFrom_Shown(object sender, EventArgs e)
        {
            Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog(this);
            waitDialog.SetTitleString("导出EXCEL");
            waitDialog.SetInformationSting("请等待");
            waitDialog.Show();

            ECInstanceList ecList = shaixuanEc();
            itemList = fuzhi(ecList);
            allItemList = itemList;
            if (itemList.Count > 0)
            {
                foreach (KeyValuePair<int, Dictionary<int, List<IECInstance>>> kv in itemList)
                {
                    if (kv.Value[0][0].GetPropertyValue("LINENUMBER") != null)
                    {
                        dataGridView1.Rows.Add(true, kv.Value[0][0]["LINENUMBER"].StringValue, kv.Value[0][0]["SPECIFICATION"].StringValue);
                        foreach (KeyValuePair<int, List<IECInstance>> kv1 in kv.Value)
                        {
                            if (kv1.Value[0].GetPropertyValue("LINENUMBER") != null)
                            {
                                dataGridView2.Rows.Add(true, kv1.Value[0].ClassDefinition.Name, kv1.Value[0]["NOMINAL_DIAMETER"].DoubleValue.ToString(), kv1.Value[0]["LINENUMBER"].StringValue);
                            }
                        }
                    }
                }
            }

            waitDialog.Close();
        }

        public bool isGbZj(IECInstance iec, out string pipeLine, out string pic)
        {
            bool isgb = false;
            pipeLine = "";
            pic = "";
            string name = iec.ClassDefinition.Name;
            if (genbuList.Contains(name))
            {
                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(iec);
                BMECObject bmec = /*JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(eleid)*/new BMECObject(iec);
                if (bmec == null || bmec.Instance == null) return false;
                foreach (IECRelationshipInstance item in bmec.Instance.GetRelationshipInstances())
                {
                    if (item.ClassDefinition.Name.Equals("DEVICE_HAS_SUPPORT"))
                    {
                        IECInstance iecpipe = item.Source;
                        isgb = true;
                        pipeLine = iecpipe["LINENUMBER"].StringValue;
                        pic = iecpipe["CERI_PIECE_MARK"].StringValue;
                        break;
                    }
                }
                if (isgb)
                {

                    if (bmec.Instance.GetPropertyValue("NOMINAL_DIAMETER") != null)
                    {
                        double gnDn = bmec.Instance["NOMINAL_DIAMETER"].DoubleValue;
                        string xinghao = bmec.Instance["CERI_PIECE_MARK"].StringValue;
                        if (gnDn == 0||xinghao=="")
                        {
                            setData(bmec);
                        }
                    }
                }
                else
                {
                    Bentley.GeometryNET.DPoint3d orgin = bmec.Transform3d.Translation;
                    Bentley.Interop.MicroStationDGN.Point3d point3D = new Bentley.Interop.MicroStationDGN.Point3d();
                    point3D.X = orgin.X / uor_per_master;
                    point3D.Y = orgin.Y / uor_per_master;
                    point3D.Z = orgin.Z / uor_per_master;
                    Bentley.Interop.MicroStationDGN.Element[] v_elements = CommandsList.scan_element_at_point(point3D, true);
                    for (int i = 0; i < v_elements.Length; i++)
                    {
                        IECInstance iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(v_elements[i]);
                        if (iec1 == null) continue;
                        BMECObject bMECObject = /*JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id((ulong)v_elements[i].ID)*/new BMECObject(iec1);
                        if (bMECObject != null && bMECObject.Instance!=null && bMECObject.ClassName.Equals("PIPE"))
                        {
                            if (bMECObject.Instance.GetPropertyValue("LINENUMBER") == null || bMECObject.Instance.GetPropertyValue("CERI_PIECE_MARK") == null) continue;
                            isgb = true;
                            pipeLine = bMECObject.Instance["LINENUMBER"].StringValue;
                            pic = bMECObject.Instance["CERI_PIECE_MARK"].StringValue;

                            double gnDn = bmec.Instance["NOMINAL_DIAMETER"].DoubleValue;
                            string xinghao = bmec.Instance["CERI_PIECE_MARK"].StringValue;
                            if (gnDn == 0 || xinghao == "")
                            {
                                setData(bmec);
                            }
                        }
                    }
                }
            }

            return isgb;
        }

        public static List<string> genbuList = new List<string> { "SERIES_C2", "SERIES_C3", "SERIES_C4", "SERIES_C6", "SERIES_C7", "SERIES_C8", "SERIES_C9", "SERIES_C10", "SERIES_C11", "SERIES_C13", "SERIES_C14", "SERIES_C15", "SERIES_C16" };

        public static void setData(BMECObject bmec)
        {
            string ecName = bmec.Instance.ClassDefinition.Name;
            if (ecName.Equals("SERIES_C2"))
            {
                string tableName = "SeriesC2";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C3"))
            {
                string tableName = "SeriesC3";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C4"))
            {
                string tableName = "SeriesC4";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C6"))
            {
                string tableName = "SeriesC6";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C7"))
            {
                string tableName = "SeriesC7";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C8"))
            {
                string tableName = "SeriesC8";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C9"))
            {
                string tableName = "SeriesC9";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C10"))
            {
                string tableName = "SeriesC10";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C11"))
            {
                string tableName = "SeriesC11";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string ser = bmec.Instance["CERI_SERIES"].StringValue;
                int row = Setdate(bmec, data, od, ser);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C13"))
            {
                string tableName = "SeriesC13";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                int row = Setdate13(bmec, data, od);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C14"))
            {
                string tableName = "SeriesC14";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                int row = Setdate13(bmec, data, od);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C15"))
            {
                string tableName = "SeriesC15";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                int row = Setdate13(bmec, data, od);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }
            if (ecName.Equals("SERIES_C16"))
            {
                string tableName = "SeriesC16";
                DataTable data = GetMdbTable(tableName);
                double od = bmec.Instance["CERI_OD"].DoubleValue;
                string pn = bmec.Instance["CERI_PN"].StringValue;
                int row = Setdate16(bmec, data, od, pn);
                if (row == -1)
                {
                    SetPropertyAsDouble("NOMINAL_DIAMETER", bmec, -1);
                }
            }

            bmec.Create();
        }

        public static DataTable GetMdbTable(string typeName)
        {
            Dictionary<string, DataTable> name_data = new Dictionary<string, DataTable>();
            Bentley.Interop.MicroStationDGN.Application app = Utilities.ComApp;
            string path1 = BMECInstanceManager.FindConfigVariableName("OP_SPEC_DIR") + "\\" + "SUPPORT1.mdb";
            string path = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1;
            OleDbConnection _oleDbConn = new OleDbConnection(path);
            _oleDbConn.Open();
            DataTable dt = _oleDbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
            OleDbDataAdapter _oleDbAda = new OleDbDataAdapter("select * from " + typeName, _oleDbConn);
            DataTable t = new DataTable();
            _oleDbAda.Fill(t);


            _oleDbConn.Close();
            _oleDbConn.Dispose();
            return t;
        }

        public static bool SetPropertyAsDouble(string propertyName, IBMECObject _bmECObject, double Value)
        {
            IECPropertyValue iECPropertyValue = _bmECObject.Instance.FindPropertyValue(propertyName, true, true, true);
            if (iECPropertyValue == null)
            {
                return false;
            }
            if (!iECPropertyValue.IsNull)
            {
                iECPropertyValue.DoubleValue = Value;
                return true;
            }
            return false;
        }

        public static int Setdate(BMECObject bMECObject, DataTable Get, double n, string series)
        {
            int num = 0;
            for (int i = 0; i < Get.Rows.Count; i++)
            {
                if (n == Convert.ToDouble(Get.Rows[i]["OD"]) && series == Get.Rows[i]["SERIES"].ToString())
                {
                    num = i;
                    break;
                }
                if (i == Get.Rows.Count - 1)
                {
                    //MessageBox.Show("没有此型号管夹,请手动选择");

                    return -1;
                }
            }
            SetPropertyAsDouble("NOMINAL_DIAMETER", bMECObject, Convert.ToDouble(Get.Rows[num]["DN"]));
            bMECObject.SetStringValue("CERI_PIECE_MARK", Convert.ToString(Get.Rows[num]["PIECE_MARK"]));
            bMECObject.SetStringValue("CERI_MATERIAL", Convert.ToString(Get.Rows[num]["MATERIAL"]));
            SetPropertyAsDouble("CERI_WEIGHT_DRY", bMECObject, Convert.ToDouble(Get.Rows[num]["WEIGHT"]) / 1000);
            bMECObject.SetStringValue("CERI_SHORT_DESC", Convert.ToString(Get.Rows[num]["SHORT_DESC"]));
            //bMECObject.Create();
            return num;
        }

        public static int Setdate13(BMECObject bMECObject, DataTable Get, double n)
        {
            int num = 0;
            for (int i = 0; i < Get.Rows.Count; i++)
            {
                if (n == Convert.ToDouble(Get.Rows[i]["OD"]))
                {
                    num = i;
                    break;
                }
                if (i == Get.Rows.Count - 1)
                {
                    return -1;
                }
            }
            SetPropertyAsDouble("NOMINAL_DIAMETER", bMECObject, Convert.ToDouble(Get.Rows[num]["DN"]));
            bMECObject.SetStringValue("CERI_PIECE_MARK", Convert.ToString(Get.Rows[num]["PIECE_MARK"]));
            bMECObject.SetStringValue("CERI_MATERIAL", Convert.ToString(Get.Rows[num]["MATERIAL"]));
            SetPropertyAsDouble("CERI_WEIGHT_DRY", bMECObject, Convert.ToDouble(Get.Rows[num]["WEIGHT"]) / 1000);
            bMECObject.SetStringValue("CERI_SHORT_DESC", Convert.ToString(Get.Rows[num]["SHORT_DESC"]));
            return num;
        }

        public static int Setdate16(BMECObject bMECObject, DataTable Get, double n, string string_PN)
        {
            int num = 0;
            for (int i = 0; i < Get.Rows.Count; i++)
            {
                if (n == Convert.ToDouble(Get.Rows[i]["OD"]) && string_PN == Convert.ToString(Get.Rows[i]["PN"]))
                {
                    num = i;
                    break;
                }
                if (i == Get.Rows.Count - 1)
                {
                    return -1;
                }
            }

            SetPropertyAsDouble("NOMINAL_DIAMETER", bMECObject, Convert.ToDouble(Get.Rows[num]["DN"]));
            bMECObject.SetStringValue("CERI_PIECE_MARK", Convert.ToString(Get.Rows[num]["PIECE_MARK"]));
            bMECObject.SetStringValue("CERI_MATERIAL", Convert.ToString(Get.Rows[num]["MATERIAL"]));
            SetPropertyAsDouble("CERI_WEIGHT_DRY", bMECObject, Convert.ToDouble(Get.Rows[num]["WEIGHT"]) / 1000);
            bMECObject.SetStringValue("CERI_SHORT_DESC", Convert.ToString(Get.Rows[num]["SHORT_DESC"]));
            return num;
        }
    }

    /// <summary>
    /// 元件属性类
    /// </summary>
    public class pipingPro
    {
        /// <summary>
        /// 管线编号
        /// </summary>
        public string pipeLineName { get; set; }

        /// <summary>
        /// 元件名称
        /// </summary>
        public string ecName { get; set; }

        /// <summary>
        /// DN
        /// </summary>
        public double DN { get; set; }
    }
}
