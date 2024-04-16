using Bentley.OpenPlant.Modeler.Api;
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
using Bentley.Plant.StandardPreferences;
using Bentley.DgnPlatformNET;
using Bentley.OpenPlantModeler.SDK.AssociatedItems;
using Bentley.OpenPlantModeler.SDK.Enums;
using Bentley.ECObjects.Instance;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.ECObjects.Schema;
using Bentley.MstnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class UpdateProForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public Dictionary<string, string> dicList = new Dictionary<string, string>();
        public static UpdateProForm updateForm = null;
        public static List<BMECObject> bmecList = new List<BMECObject>();
        public List<string> czNameList = new List<string>();
        public static MaterialsProManagerForm maForm = null;
        public bool ishiden = false;
        public UpdateProForm()
        {
            InitializeComponent();
            dicFhuz();
        }

        public static UpdateProForm instance(List<BMECObject> listBmec,MaterialsProManagerForm form)
        {
            bmecList = listBmec;
            maForm = form;
            if (updateForm == null)
            {
                updateForm = new UpdateProForm();
            }
            else
            {
                updateForm.Close();
                updateForm = new UpdateProForm();
            }
            return updateForm;
        }

        public void dicFhuz()
        {
            dicList.Add("H", "保温");
            dicList.Add("C", "保冷");
            dicList.Add("P", "防烫");
            dicList.Add("D", "防结露");
            dicList.Add("S", "蒸汽伴热");
            dicList.Add("E", "电伴热");
            dicList.Add("N", "隔声");
        }

        private void UpdateProForm_Load(object sender, EventArgs e)
        {
            if(!ishiden)
            {
                ishiden = true;
                SelectionSetManager.EmptyAll(); //清空选中集合
                grdhcomboBox1.Text = "";
                cstextBox1.Text = "1";
                azbwcomboBox1.Text = "室内";
                mdcomboBox1.Text = "否";
                gnbffcomboBox1.Text = "否";
                dqyqzltextBox1.Text = "醇酸防锈漆G53-1";
                dqbstextBox1.Text = "2";
                zjqbstextBox1.Text = "0";
                mqyqzltextBox1.Text = "酚醛调和漆";
                mqbstextBox1.Text = "2";
                fhcltextBox1.Text = "镀锌铁皮";
                fhggtextBox1.Text = "0.5mm";
                tsfscomboBox1.Text = "无损";
                ylsycomboBox1.Text = "是";
                scxcomboBox1.Text = "是";
                kqcscomboBox1.SelectedIndex = 0;
                zqcscomboBox1.SelectedIndex = 0;
                jxcomboBox1.SelectedIndex = 0;
                sxcomboBox1.SelectedIndex = 0;
                gdtzcomboBox1.SelectedIndex = 0;
                yqxcomboBox1.SelectedIndex = 0;

                string insulation_thickness = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
                bhtextBox1.Text = insulation_thickness;

                List<string> insulationMaterial = new List<string>();
                insulationMaterial = getInsulationMaterial(out czNameList);
                insulationMaterial.Insert(0, "");
                czNameList.Insert(0, "");

                czcomboBox1.DataSource = insulationMaterial;

                string insulation_material = DlgStandardPreference.GetPreferenceValue("INSULATION");

                initial();

                if (bmecList.Count > 0)
                {
                    List<NetworkSystem> pipNetList = new List<NetworkSystem>();
                    pipNetList = NetworkSystem.GetExistingPipingNetworkSystems();
                    string pipName = bmecList[0].Instance["LINENUMBER"].StringValue;
                    string ljfs = "";
                    foreach (BMECObject bmec in bmecList)
                    {
                        if (bmec.Ports.Count > 0)
                        {
                            bool b = BMECApi.Instance.InstanceDefinedAsClass(bmec.Instance, "PIPE", true);
                            if (b)
                            {
                                ljfs = bmec.Ports[0].Instance["END_PREPARATION"].StringValue;
                                break;
                            }

                            ljfs = bmec.Ports[0].Instance["END_PREPARATION"].StringValue;
                        }
                    }
                    ljfstextBox1.Text = ljfs;
                    foreach (NetworkSystem net in pipNetList)
                    {
                        if (net.Name.Equals(pipName))
                        {
                            string cz = net.GetPropertyValue("INSULATION"); //这里是displayLable
                            string hd = net.GetPropertyValue("INSULATION_THICKNESS");

                            int index = czNameList.FindIndex(item => item.Equals(cz));

                            if (index < 0)
                            {
                                index = insulationMaterial.FindIndex(item => item.Equals(cz));
                                if (index < 0)
                                    index = 0;
                            }

                            czcomboBox1.SelectedIndex = index;

                            bhtextBox1.Text = hd;

                            break;
                        }
                    }
                }
            }
            
        }

        private void grdhcomboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (grdhcomboBox1.Text == "")
            {
                grdhtextBox1.Text = "";
            }
            else
            {
                grdhtextBox1.Text = dicList[grdhcomboBox1.Text];
            }
        }

        private void wdtextBox1_TextChanged(object sender, EventArgs e)
        {
            if (wdtextBox1.Text == "")
            {
                return;
            }
            double wdValue;
            bool b = double.TryParse(wdtextBox1.Text, out wdValue);
            if (!b)
            {
                MessageBox.Show("温度属性请输入数字");
                wdtextBox1.Text = "";
                return;
            }
        }

        private void cstextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char result = e.KeyChar;
            if (char.IsDigit(result) || result == 8)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void yldjtextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                #region 获取输入值
                string grdhValue = grdhcomboBox1.Text;
                string wdValue = wdtextBox1.Text + "℃";
                int csValue = Convert.ToInt32(cstextBox1.Text);
                string wlbmValue = wlbmtextBox1.Text;
                string xtmcValue = xtmctextBox1.Text;
                string ljfsValue = ljfstextBox1.Text;
                string yldjValue = yldjtextBox1.Text + "MPa";
                string azbwValue = azbwcomboBox1.Text;
                string mdValue = mdcomboBox1.Text;
                string gnbffValue = gnbffcomboBox1.Text;
                string gnbffjtyqValue = gnbffjtyqtextBox1.Text;
                string cxdjValue = cxdjtextBox1.Text;
                string dqyqzlValue = dqyqzltextBox1.Text;
                int dqbsValue = Convert.ToInt32(dqbstextBox1.Text);
                string zjqyqzlValue = zjqyqzltextBox1.Text;
                int zjqbsValue = Convert.ToInt32(zjqbstextBox1.Text);
                string mqyqzlValue = mqyqzltextBox1.Text;
                int mqbsValue = Convert.ToInt32(mqbstextBox1.Text);
                string fhclValue = fhcltextBox1.Text;
                string fhggValue = fhggtextBox1.Text;
                string tsblValue = tsbltextBox1.Text + "%";
                if (tsbltextBox1.Text == "" || tsbltextBox1.Text == " ")
                    tsblValue = tsbltextBox1.Text;
                string tsfsValue = tsfscomboBox1.Text;
                string ylsyValue = ylsycomboBox1.Text;
                string scxValue = scxcomboBox1.Text;
                string kqcsValue = kqcscomboBox1.Text;
                string zqcsValue = zqcscomboBox1.Text;
                string jxValue = jxcomboBox1.Text;
                string sxValue = sxcomboBox1.Text;
                string gdtzValue = gdtzcomboBox1.Text;
                string yqxValue = yqxcomboBox1.Text;

                string bhValue = bhtextBox1.Text;

                double tt;
                bool isBh = double.TryParse(bhValue, out tt);
                if (!isBh)
                {
                    MessageBox.Show("壁厚请输入数字！");
                    return;
                }
                if (tt < 0)
                {
                    MessageBox.Show("壁厚输入值小于0,将壁厚赋值为0");
                    bhValue = "0";
                }

                string czValue = czcomboBox1.Text;

                string czValue1 = czValue;

                int index = czcomboBox1.SelectedIndex;

                czValue = czNameList[index];
                #endregion

                #region
                if (bmecList.Count > 0)
                {
                    this.Hide();
                    maForm.Hide();
                    List<NetworkSystem> pipNetList = new List<NetworkSystem>();
                    pipNetList = NetworkSystem.GetExistingPipingNetworkSystems();
                    string pipName = bmecList[0].Instance["LINENUMBER"].StringValue;
                    string cz = bmecList[0].Instance["INSULATION"].StringValue;
                    foreach (NetworkSystem net in pipNetList)
                    {
                        if (net.Name.Equals(pipName))
                        {
                            double bh1 = Convert.ToDouble(bhValue);
                            try
                            {
                                ItemStatus mj = net.EditInsulation(bh1, czValue);
                            }
                            catch (Exception ex)
                            {
                                System.Windows.Forms.MessageBox.Show(ex.ToString());
                            }
                        }
                    }
                    foreach (BMECObject bmec in bmecList)
                    {
                        try
                        {
                            if (bmec.Instance != null)
                            {
                                if (bmec.Instance.GetPropertyValue("CERI_Insulation_code") != null)
                                {
                                    bmec.Instance["CERI_Insulation_code"].StringValue = grdhValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_TEMP") != null)
                                {
                                    bmec.Instance["CERI_TEMP"].StringValue = wdValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Layer") != null)
                                {
                                    bmec.Instance["CERI_Layer"].IntValue = csValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Material_Code") != null)
                                {
                                    bmec.Instance["CERI_Material_Code"].StringValue = wlbmValue;
                                }
                                //if(bmec.Instance.GetPropertyValue("CERI_System_Name")!=null)
                                //{
                                //    bmec.Instance["CERI_System_Name"].StringValue = xtmcValue;
                                //}
                                if (bmec.Instance.GetPropertyValue("CERI_Connection_Type") != null)
                                {
                                    bmec.Instance["CERI_Connection_Type"].StringValue = ljfsValue;
                                }
                                //if(bmec.Instance.GetPropertyValue("CERI_Pressure_Rating")!=null)
                                //{
                                //    bmec.Instance["CERI_Pressure_Rating"].StringValue = yldjValue;
                                //}
                                if (bmec.Instance.GetPropertyValue("CERI_Installation_Site") != null)
                                {
                                    bmec.Instance["CERI_Installation_Site"].StringValue = azbwValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Buried") != null)
                                {
                                    bmec.Instance["CERI_Buried"].StringValue = mdValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Pipe_Lining_Anticorrosion") != null)
                                {
                                    bmec.Instance["CERI_Pipe_Lining_Anticorrosion"].StringValue = gnbffValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Specification") != null)
                                {
                                    bmec.Instance["CERI_Specification"].StringValue = gnbffjtyqValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Derusting_Grade") != null)
                                {
                                    bmec.Instance["CERI_Derusting_Grade"].StringValue = cxdjValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Primer_Paint") != null)
                                {
                                    bmec.Instance["CERI_Primer_Paint"].StringValue = dqyqzlValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Primer_Pass") != null)
                                {
                                    bmec.Instance["CERI_Primer_Pass"].IntValue = dqbsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Intermediate_Paint") != null)
                                {
                                    bmec.Instance["CERI_Intermediate_Paint"].StringValue = zjqyqzlValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Middle_Number") != null)
                                {
                                    bmec.Instance["CERI_Middle_Number"].IntValue = zjqbsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Topcoat_Paint") != null)
                                {
                                    bmec.Instance["CERI_Topcoat_Paint"].StringValue = mqyqzlValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Finish_Times") != null)
                                {
                                    bmec.Instance["CERI_Finish_Times"].IntValue = mqbsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Shielding_Material") != null)
                                {
                                    bmec.Instance["CERI_Shielding_Material"].StringValue = fhclValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Protection_Specification") != null)
                                {
                                    bmec.Instance["CERI_Protection_Specification"].StringValue = fhggValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Percentage_Detection") != null)
                                {
                                    bmec.Instance["CERI_Percentage_Detection"].StringValue = tsblValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Inspection_Way") != null)
                                {
                                    bmec.Instance["CERI_Inspection_Way"].StringValue = tsfsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Pressure_Testing") != null)
                                {
                                    bmec.Instance["CERI_Pressure_Testing"].StringValue = ylsyValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Water_Washing") != null)
                                {
                                    bmec.Instance["CERI_Water_Washing"].StringValue = scxValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Air_Purge") != null)
                                {
                                    bmec.Instance["CERI_Air_Purge"].StringValue = kqcsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Steam_Blowing") != null)
                                {
                                    bmec.Instance["CERI_Steam_Blowing"].StringValue = zqcsValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Alkali_Wash") != null)
                                {
                                    bmec.Instance["CERI_Alkali_Wash"].StringValue = jxValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Acid_Pickling") != null)
                                {
                                    bmec.Instance["CERI_Acid_Pickling"].StringValue = sxValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Pipeline_Skim") != null)
                                {
                                    bmec.Instance["CERI_Pipeline_Skim"].StringValue = gdtzValue;
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Oil_Cleaning") != null)
                                {
                                    bmec.Instance["CERI_Oil_Cleaning"].StringValue = yqxValue;
                                }

                                if (bmec.Instance.GetPropertyValue("CERI_Insulation_Thickness") != null)
                                {
                                    bmec.Instance["CERI_Insulation_Thickness"].DoubleValue = Convert.ToDouble(bhValue);
                                }
                                if (bmec.Instance.GetPropertyValue("CERI_Insulation_Material") != null)
                                {
                                    bmec.Instance["CERI_Insulation_Material"].StringValue = czValue1;
                                }

                                if (bmec.Instance.GetPropertyValue("INSULATION_THICKNESS") != null)
                                {
                                    bmec.Instance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(bhValue);
                                }
                                if (bmec.Instance.GetPropertyValue("INSULATION") != null)
                                {
                                    bmec.Instance["INSULATION"].StringValue = czValue;
                                }
                            }
                            else
                            {
                                MessageBox.Show("图纸中存在 损坏的元件！");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("属性错误！");
                        }
                        

                        #region 测试
                        //if (bmec.Instance.GetPropertyValue("CERI_MAT_GRADE")!=null)
                        //{
                        //    bmec.Instance["CERI_MAT_GRADE"].StringValue = "555";
                        //}
                        #endregion
                        try
                        {
                            if (!bmec.IsReadOnly && bmec.IsValid)
                                bmec.Create();
                            //else
                            //    MessageBox.Show("存在只读或无效元素！");
                        }
                        catch
                        {
                            MessageBox.Show("报错了！！");
                        }
                    }
                    maForm.Show();
                    this.Show();                    
                    MessageBox.Show("修改成功！");
                }
                #endregion
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        private void tsbltextBox1_TextChanged(object sender, EventArgs e)
        {
            if (tsbltextBox1.Text == "")
            {
                return;
            }
            double tsblValue;
            bool b = double.TryParse(tsbltextBox1.Text, out tsblValue);
            if (!b)
            {
                MessageBox.Show("探伤比例属性请输入有效的数字");
                tsbltextBox1.Text = "";
                return;
            }
        }

        public void initial()
        {
            if (bmecList.Count > 0)
            {
                #region 获取值
                string grdhValue = "";
                string wdValue = "";
                int csValue = 1;
                string wlbmValue = "";
                string xtmcValue = "";
                string ljfsValue = "";
                string yldjValue = "";
                string azbwValue = "";
                string mdValue = "";
                string gnbffValue = "";
                string gnbffjtyqValue = "";
                string cxdjValue = "";
                string dqyqzlValue = "";
                int dqbsValue = 2;
                string zjqyqzlValue = "";
                int zjqbsValue = 0;
                string mqyqzlValue = "";
                int mqbsValue = 2;
                string fhclValue = "";
                string fhggValue = "";
                string tsblValue = "";
                string tsfsValue = "";
                string ylsyValue = "";
                string scxValue = "";
                string kqcsValue = "";
                string zqcsValue = "";
                string jxValue = "";
                string sxValue = "";
                string gdtzValue = "";
                string yqxValue = "";

                double bhValue = 0;

                string czValue = "";

                string jzmc = "";

                BMECObject bmec = bmecList[0];

                if (bmec.Instance.GetPropertyValue("CERI_Insulation_code") != null)
                {
                    grdhValue = bmec.Instance["CERI_Insulation_code"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_TEMP") != null)
                {
                    wdValue = bmec.Instance["CERI_TEMP"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Layer") != null)
                {
                    csValue = bmec.Instance["CERI_Layer"].IntValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Material_Code") != null)
                {
                    wlbmValue = bmec.Instance["CERI_Material_Code"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_System_Name") != null)
                {
                    xtmcValue = bmec.Instance["CERI_System_Name"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Connection_Type") != null)
                {
                    ljfsValue = bmec.Instance["CERI_Connection_Type"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Pressure_Rating") != null)
                {
                    yldjValue = bmec.Instance["CERI_Pressure_Rating"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Installation_Site") != null)
                {
                    azbwValue = bmec.Instance["CERI_Installation_Site"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Buried") != null)
                {
                    mdValue = bmec.Instance["CERI_Buried"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Pipe_Lining_Anticorrosion") != null)
                {
                    gnbffValue = bmec.Instance["CERI_Pipe_Lining_Anticorrosion"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Specification") != null)
                {
                    gnbffjtyqValue = bmec.Instance["CERI_Specification"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Derusting_Grade") != null)
                {
                    cxdjValue = bmec.Instance["CERI_Derusting_Grade"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Primer_Paint") != null)
                {
                    dqyqzlValue = bmec.Instance["CERI_Primer_Paint"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Primer_Pass") != null)
                {
                    dqbsValue = bmec.Instance["CERI_Primer_Pass"].IntValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Intermediate_Paint") != null)
                {
                    zjqyqzlValue = bmec.Instance["CERI_Intermediate_Paint"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Middle_Number") != null)
                {
                    zjqbsValue = bmec.Instance["CERI_Middle_Number"].IntValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Topcoat_Paint") != null)
                {
                    mqyqzlValue = bmec.Instance["CERI_Topcoat_Paint"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Finish_Times") != null)
                {
                    mqbsValue = bmec.Instance["CERI_Finish_Times"].IntValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Shielding_Material") != null)
                {
                    fhclValue = bmec.Instance["CERI_Shielding_Material"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Protection_Specification") != null)
                {
                    fhggValue = bmec.Instance["CERI_Protection_Specification"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Percentage_Detection") != null)
                {
                    tsblValue = bmec.Instance["CERI_Percentage_Detection"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Inspection_Way") != null)
                {
                    tsfsValue = bmec.Instance["CERI_Inspection_Way"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Pressure_Testing") != null)
                {
                    ylsyValue = bmec.Instance["CERI_Pressure_Testing"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Water_Washing") != null)
                {
                    scxValue = bmec.Instance["CERI_Water_Washing"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Air_Purge") != null)
                {
                    kqcsValue = bmec.Instance["CERI_Air_Purge"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Steam_Blowing") != null)
                {
                    zqcsValue = bmec.Instance["CERI_Steam_Blowing"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Alkali_Wash") != null)
                {
                    jxValue = bmec.Instance["CERI_Alkali_Wash"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Acid_Pickling") != null)
                {
                    sxValue = bmec.Instance["CERI_Acid_Pickling"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Pipeline_Skim") != null)
                {
                    gdtzValue = bmec.Instance["CERI_Pipeline_Skim"].StringValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Oil_Cleaning") != null)
                {
                    yqxValue = bmec.Instance["CERI_Oil_Cleaning"].StringValue;
                }

                if (bmec.Instance.GetPropertyValue("CERI_Insulation_Thickness") != null)
                {
                    bhValue = bmec.Instance["CERI_Insulation_Thickness"].DoubleValue;
                }
                if (bmec.Instance.GetPropertyValue("CERI_Insulation_Material") != null)
                {
                    czValue = bmec.Instance["CERI_Insulation_Material"].StringValue;
                }

                if (bmec.Instance.GetPropertyValue("CERI_Media_Name") != null)
                {
                    jzmc = bmec.Instance["CERI_Media_Name"].StringValue;
                }
                #endregion

                if (grdhValue != "" && grdhValue != " ")
                    grdhcomboBox1.Text = grdhValue;
                if (wdValue != "" && wdValue != " ")
                {
                    double wdd;
                    bool b = double.TryParse(wdValue, out wdd);
                    if (b)
                    {
                        wdtextBox1.Text = wdValue;
                    }
                    else
                    {
                        int i = wdValue.IndexOf("℃");
                        if (i > 0)
                        {
                            string tt = wdValue.Substring(0, wdValue.IndexOf("℃") - 1);
                            double wdd1;
                            bool b1 = double.TryParse(tt, out wdd1);
                            if (b1)
                            {
                                wdtextBox1.Text = wdd1.ToString();
                            }
                        }
                    }
                }
                cstextBox1.Text = csValue.ToString();
                if (wlbmValue != "" && wlbmValue != " ")
                {
                    wlbmtextBox1.Text = wlbmValue;
                }
                bhtextBox1.Text = bhValue.ToString();
                if (czValue != "" && czValue != " ")
                {
                    int index = czNameList.FindIndex(item => item.Equals(czValue));

                    if (index < 0)
                        index = 0;

                    czcomboBox1.SelectedIndex = index;
                }

                if (xtmcValue != "" && xtmcValue != " ")
                {
                    xtmctextBox1.Text = xtmcValue;
                }
                if (ljfsValue != "" && ljfsValue != " ")
                {
                    ljfstextBox1.Text = ljfsValue;
                }
                if (yldjValue != "" && yldjValue != " ")
                {
                    yldjtextBox1.Text = yldjValue;
                }
                if (azbwValue != "" && azbwValue != " ")
                {
                    azbwcomboBox1.Text = azbwValue;
                }
                if (mdValue != "" && mdValue != " ")
                {
                    mdcomboBox1.Text = mdValue;
                }
                if (gnbffValue != "" && gnbffValue != " ")
                {
                    gnbffcomboBox1.Text = gnbffValue;
                }
                if (gnbffjtyqValue != "" && gnbffjtyqValue != " ")
                {
                    gnbffjtyqtextBox1.Text = gnbffjtyqValue;
                }
                if (cxdjValue != "" && cxdjValue != " ")
                {
                    cxdjtextBox1.Text = cxdjValue;
                }
                if (dqyqzlValue != "" && dqyqzlValue != " ")
                {
                    dqyqzltextBox1.Text = dqyqzlValue;
                }
                dqbstextBox1.Text = dqbsValue.ToString();
                if (zjqyqzlValue != "" && zjqyqzlValue != " ")
                {
                    zjqyqzltextBox1.Text = zjqyqzlValue;
                }
                zjqbstextBox1.Text = zjqbsValue.ToString();
                if (mqyqzlValue != "" && mqyqzlValue != " ")
                {
                    mqyqzltextBox1.Text = mqyqzlValue;
                }
                mqbstextBox1.Text = mqbsValue.ToString();
                if (fhclValue != "" && fhclValue != " ")
                {
                    fhcltextBox1.Text = fhclValue;
                }
                if (fhggValue != "" && fhggValue != " ")
                {
                    fhggtextBox1.Text = fhggValue;
                }
                if (tsblValue != "" && tsblValue != " ")
                {
                    double wdd;
                    bool b = double.TryParse(tsblValue, out wdd);
                    if (b)
                    {
                        tsbltextBox1.Text = tsblValue;
                    }
                    else
                    {
                        int i = tsblValue.IndexOf("%");
                        if (i > 0)
                        {
                            string tt = tsblValue.Substring(0, i - 1);
                            double wdd1;
                            bool b1 = double.TryParse(tt, out wdd1);
                            if (b1)
                            {
                                tsbltextBox1.Text = wdd1.ToString();
                            }
                        }
                    }
                }
                if (tsfsValue != "" && tsfsValue != " ")
                {
                    tsfscomboBox1.Text = tsfsValue;
                }
                if (ylsyValue != "" && ylsyValue != " ")
                {
                    ylsycomboBox1.Text = ylsyValue;
                }
                if (scxValue != "" && scxValue != " ")
                {
                    scxcomboBox1.Text = scxValue;
                }
                if (kqcsValue != "" && kqcsValue != " ")
                {
                    kqcscomboBox1.Text = kqcsValue;
                }
                if (zqcsValue != "" && zqcsValue != " ")
                {
                    zqcscomboBox1.Text = zqcsValue;
                }
                if (jxValue != "" && jxValue != " ")
                {
                    jxcomboBox1.Text = jxValue;
                }
                if (sxValue != "" && sxValue != " ")
                {
                    sxcomboBox1.Text = sxValue;
                }
                if (gdtzValue != "" && gdtzValue != " ")
                {
                    gdtzcomboBox1.Text = gdtzValue;
                }
                if (yqxValue != "" && yqxValue != " ")
                {
                    yqxcomboBox1.Text = yqxValue;
                }
            }
        }

        /// <summary>
        /// 获得保温层材料的集合
        /// </summary>
        /// <returns></returns>
        public static List<string> getInsulationMaterial(out List<string> nameList)
        {
            nameList = new List<string>();
            List<string> insulationMaterialList = new List<string>();
            IECInstance insulationMaterialInstance = BMECInstanceManager.Instance.CreateECInstance("INSULATION_MATERIAL_VALUE_MAP", true);
            foreach (IECPropertyValue item in insulationMaterialInstance)
            {
                //string dis = item.Instance.ClassDefinition.DisplayLabel;
                string dis1 = item.Property.DisplayLabel;
                insulationMaterialList.Add(dis1);
                nameList.Add(item.AccessString);

            }
            return insulationMaterialList;
        }

        public static Dictionary<string, string> getInsulationMaterial()
        {
            Dictionary<string, string> dicList = new Dictionary<string, string>();
            IECInstance insulationMaterialInstance = BMECInstanceManager.Instance.CreateECInstance("INSULATION_MATERIAL_VALUE_MAP", true);
            foreach (IECPropertyValue item in insulationMaterialInstance)
            {
                //string dis = item.Instance.ClassDefinition.DisplayLabel;
                string dis1 = item.Property.DisplayLabel;
                dicList.Add(item.AccessString, dis1);
            }
            return dicList;
        }

        //public ItemStatus EditInsulation(double thickness, string insulationMaterial, NetworkSystemType NetworkSystemType)
        //{
        //    Dictionary<string, string> dictionary = new Dictionary<string, string>();
        //    dictionary.Add("INSULATION_THICKNESS", thickness.ToString());
        //    dictionary.Add("INSULATION", insulationMaterial);
        //    try
        //    {
        //        IECInstance networkSystemByName = NetworkSystem.GetNetworkSystemByName(NetworkSystemType, this.Name);
        //        if (networkSystemByName == null)
        //        {
        //            ItemStatus result = ItemStatus.DoesNotExist;
        //            return result;
        //        }
        //        NetworkSystem.SetProperties(networkSystemByName, dictionary);
        //        DgnUtilities instance = DgnUtilities.GetInstance();
        //        instance.SaveModifiedInstance(networkSystemByName, instance.GetDGNConnection());
        //        NetworkSystem.UpdateComponents(networkSystemByName);
        //    }
        //    catch
        //    {
        //        ItemStatus result = ItemStatus.Failed;
        //        return result;
        //    }
        //    return ItemStatus.Success;
        //}

        //private static IECInstance GetNetworkSystemByName(NetworkSystemType networkSystemType, string networkSystemName)
        //{
        //    switch (networkSystemType)
        //    {
        //        case NetworkSystemType.PipingNetworkSystem:
        //            {
        //                BMECApi.Instance.MarkFeature("918E5C87-44E3-42b2-A3E9-1B6F60A3C841");
        //                break;
        //            }
        //        case NetworkSystemType.HVACNetworkSystem:
        //            {
        //                BMECApi.Instance.MarkFeature("EBCAF6D9-324D-4716-ADBB-59DA91DE4021");
        //                break;
        //            }
        //        case NetworkSystemType.TrayNetworkSystem:
        //            {
        //                BMECApi.Instance.MarkFeature("7E1F78CD-BEA9-4970-865A-AA613ADA66AB");
        //                break;
        //            }
        //    }
        //    string networkSystem = GetNetworkSystem(networkSystemType);
        //    IECClass @class = SchemaUtilities.PlantSchema.GetClass(networkSystem);
        //    DgnUtilities instance = DgnUtilities.GetInstance();
        //    List<IECInstance> arg_6D_0 = instance.GetInstancesFromDgn(@class, instance.GetDGNConnection());
        //    IECInstance result = null;
        //    foreach (IECInstance current in arg_6D_0)
        //    {
        //        if (current["NAME"].StringValue == networkSystemName)
        //        {
        //            result = current;
        //            break;
        //        }
        //    }
        //    return result;
        //}

        //public static string GetNetworkSystem(NetworkSystemType networkSystemType)
        //{
        //    switch (networkSystemType)
        //    {
        //        case NetworkSystemType.PipingNetworkSystem:
        //            {
        //                return "PIPING_NETWORK_SYSTEM";
        //            }
        //        case NetworkSystemType.HVACNetworkSystem:
        //            {
        //                return "HVAC_NETWORK_SYSTEM";
        //            }
        //        case NetworkSystemType.TrayNetworkSystem:
        //            {
        //                return "TRAY_NETWORK_SYSTEM";
        //            }
        //        default:
        //            {
        //                return "INVALID";
        //            }
        //    }
        //}
    }
}
