﻿using Bentley.OpenPlant.Modeler.Api;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.Plant.StandardPreferences;
using JYX_ZYJC_Jianmo_Youhua_CS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.DgnPlatformNET.Elements;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ZhinengxianPipeFrom :
#if DEBUG
        Form
#else  
        Adapter
#endif
    {
        int fromHeight = 0;
        int froupHeigth = 0;
        int uFroupHeigth = 0;
        Point bendButt = new Point();
        //Point elbowButt = new Point();
        Point bendButt1 = new Point();
        //Point elbowButt1 = new Point();
        public List<string> PipelineList = new List<string>();
        public List<string> specsList = new List<string>();
        public List<double> dnList = new List<double>();
        public List<string> insulationMaterial = new List<string>();
        public ECInstanceList ec_instance_list = new ECInstanceList();
        protected BMECApi api = BMECApi.Instance;
        public List<DPoint3d> new_points_list = new List<DPoint3d>();
        public bool b = false;
        public Dictionary<int, Element> elementList = new Dictionary<int, Element>();
        public BoTree<Segment> startTrees = new BoTree<Segment>();
        bool qgwt = false;
        List<ulong> elesanIdList = new List<ulong>();
        bool isscCw = false;
        static BIM.Application app = Utilities.ComApp;  //V8I
        elbowOrBend eOb = new elbowOrBend();
        OPM_Public_Api opmapi1 = new OPM_Public_Api();
        public ZhinengxianPipeFrom(List<DPoint3d> dpList, Dictionary<int, Element> ele, BoTree<Segment> startTree)
        {
            new_points_list = dpList;
            elementList = ele;
            startTrees = startTree;
            InitializeComponent();
            init();
        }

        /// <summary>
        /// 初始化给控件赋初始值
        /// </summary>
        private void init()
        {
            fromHeight = Height;
            bendButt = button1.Location;
            //elbowButt = button2.Location;
            bendButt1 = new Point(bendButt.X, bendButt.Y - (groupBox_elbow.Height - groupBox_bend.Height));
            //elbowButt1 = new Point(elbowButt.X, elbowButt.Y - (groupBox_bend.Height - groupBox_elbow.Height));
            froupHeigth = groupBox3.Height;
            uFroupHeigth = froupHeigth - (groupBox_elbow.Height - groupBox_bend.Height);
            PipelineList = GetPipeLine();
            this.comboBox_outer_pipeline.DataSource = PipelineList;
            string specName = DlgStandardPreference.GetPreferenceValue("SPECIFICATION");
            specsList = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSpecifications();
            this.comboBox_outer_spec.DataSource = specsList;

            if (specsList.Contains(specName))
            {
                this.comboBox_outer_spec.Text = specName;
            }
            else
            {
                this.comboBox_outer_spec.SelectedIndex = 0;
            }

            string insulation_thickness = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            this.textBox_outer_insulation_thickness.Text = insulation_thickness;

            insulationMaterial = getInsulationMaterial();
            insulationMaterial.Insert(0, "");
            this.comboBox_outer_insulation_material.DataSource = insulationMaterial;
            string insulation_material = DlgStandardPreference.GetPreferenceValue("INSULATION");
            if (insulationMaterial.Contains(insulation_material))
            {
                this.comboBox_outer_insulation_material.Text = insulation_material;
            }
            else
            {
                this.comboBox_outer_insulation_material.SelectedIndex = 0;
            }

            string compState = DlgStandardPreference.GetPreferenceValue("STATE");
            this.ComboxComponentSate.Text = compState;

            this.textPipePressure.Text = "0 kPaG";

            this.comboBox_elbow_radius.Text = "长半径弯头";
            this.comboBox_elbow_angle.Text = "90度弯头";

            this.comboBox_elbowOrBend.Text = "Elbow";

            checkBox1.Checked = true;
            radioButton1.Checked = true;
            radioButton4.Checked = true;
            //this.comboBox_bendOrXiamiwan.Text = "Elbow";
        }

        private static List<Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem> pipingNetworkSystems = null;
        private static List<string> pipelinesName = null;
        /// <summary>
        /// 获取管道线
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPipeLine()
        {
            pipingNetworkSystems = Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem.GetExistingPipingNetworkSystems();
            pipelinesName = new List<string>();
            foreach (var item in pipingNetworkSystems)
            {
                string name = item.Name;
                pipelinesName.Add(name);
            }
            return pipelinesName;
        }

        /// <summary>
        /// 获取保护层材料
        /// </summary>
        /// <returns></returns>
        public static List<string> getInsulationMaterial()
        {
            List<string> insulationMaterialList = new List<string>();
            IECInstance insulationMaterialInstance = BMECInstanceManager.Instance.CreateECInstance("INSULATION_MATERIAL_VALUE_MAP", true);
            foreach (var item in insulationMaterialInstance)
            {
                insulationMaterialList.Add(item.AccessString);
            }
            return insulationMaterialList;
        }

        /// <summary>
        /// 找到对应等级库下的dn
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_outer_spec_SelectedIndexChanged(object sender, EventArgs e)
        {
            double dn = Convert.ToDouble(DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER"));
            dnList = Bentley.OpenPlantModeler.SDK.Utilities.DatabaseUtilities.GetAllSizes(comboBox_outer_spec.Text);
            comboBox_outer_dn.DataSource = dnList;

            if (dnList.Contains(dn))
            {
                this.comboBox_outer_dn.Text = Convert.ToString(dn);
            }
            else
            {
                this.comboBox_outer_dn.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 根据等级库dn让用户选择一条数据，对控件进行赋值
        /// </summary>
        /// <param name="dn"></param>
        /// <param name="spec"></param>
        private void fuzhi(double dn, string spec)
        {
            int index = pipelinesName.IndexOf(comboBox_outer_pipeline.Text);
            StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
            StandardPreferencesUtilities.ChangeSpecification(spec);
            IECInstance iecInstance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);
            iecInstance["NOMINAL_DIAMETER"].DoubleValue = Convert.ToDouble(dn);
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(iecInstance, null);
            ec_instance_list = specProcessor.SelectSpec(iecInstance, true);
            //this.textBox_outer_OD.Text = Convert.ToString(instance["OUTSIDE_DIAMETER"].DoubleValue);
            //this.textBox_outer_weight.Text = Convert.ToString(instance["WEIGHT"].DoubleValue);
            //this.textBox_outer_shortDesc.Text = instance["SHORT_DESCRIPTION"].StringValue;
            //this.textBox_outer_wallThickness.Text = Convert.ToString(instance["WALL_THICKNESS"].DoubleValue);
            //this.textBox_outer_pipeMaterial.Text = instance["MATERIAL"].StringValue;
            //this.textBox_outer_dryWeight.Text = Convert.ToString(instance["DRY_WEIGHT"].DoubleValue);
        }

        /// <summary>
        /// 当等级库或者dn改变时重新赋值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_outer_dn_SelectedIndexChanged(object sender, EventArgs e)
        {
            //fuzhi(Convert.ToDouble(comboBox_outer_dn.Text), comboBox_outer_spec.Text);
        }

        /// <summary>
        /// 生成管道
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox_elbowOrBend.Text == "Elbow" && comboBox_elbow_radius.Text != "虾米弯" && checkBox1.Checked == false)
            {
                pdsfmz(startTrees);
                if (qgwt)
                {
                    MessageBox.Show("存在智能线的夹角与选择的弯头角度不同！");
                    this.Close();
                    return;
                }
            }
            #region 未加三通时切割弯头 判断
            //if (comboBox_elbowOrBend.Text == "Elbow" && comboBox_elbow_radius.Text != "虾米弯" && checkBox1.Checked == false)
            //{
            //    double anglera = Convert.ToDouble(comboBox_elbow_angle.Text.Substring(0, 2));
            //    bool xt = false;
            //    if (new_points_list.Count > 2)
            //    {
            //        for (int i = 0; i < new_points_list.Count - 2; i++)
            //        {
            //            DVector3d dv1 = new DVector3d(new_points_list[i], new_points_list[i + 1]);
            //            DVector3d dv2 = new DVector3d(new_points_list[i + 2], new_points_list[i + 1]);
            //            Angle ag = dv1.AngleTo(dv2);
            //            double ang = ag.Degrees;
            //            if (Math.Abs(ang - anglera) > 0.1)
            //            {
            //                xt = true;
            //            }
            //        }
            //        if (xt)
            //        {
            //            MessageBox.Show("存在智能线的夹角与选择的弯头角度不同！");
            //            this.Close();
            //            return;
            //        }
            //    }
            //}
            #endregion
            //b = true;
            #region 赋值
            elbowOrBend elbowOrBendClass = new elbowOrBend();
            if (comboBox_elbowOrBend.Text == "Elbow")
            {
                //elbowOrBendClass.elbowType = comboBox_bendOrXiamiwan.Text;
                elbowOrBendClass.elbowOrBendName = comboBox_elbowOrBend.Text;
                elbowOrBendClass.elbowRadius = comboBox_elbow_radius.Text;
                elbowOrBendClass.elbowAngle = comboBox_elbow_angle.Text;
                int jieshu = 3;
                //int.TryParse(textBox_xiamiwan_jieshu.Text, out jieshu);
                //if (jieshu < 3)
                //{
                //    jieshu = 3;
                //}
                elbowOrBendClass.pitchNumber = jieshu;
                elbowOrBendClass.isBzQgWt = checkBox1.Checked;
                elbowOrBendClass.isYgWt = radioButton1.Checked;
                elbowOrBendClass.isLDQ = radioButton4.Checked;
            }
            else
            {
                double radiusRatio = 1.0;
                double.TryParse(textBox_radiusFactor.Text, out radiusRatio);
                elbowOrBendClass.bendRadiusRatio = radiusRatio;
                double radius = 100.0;
                double.TryParse(textBox_radius.Text, out radius);
                elbowOrBendClass.bendRadius = radius;
                double frontLong = 0;
                double afterLong = 0;
                double.TryParse(textBox_lengthToBend.Text, out frontLong);
                double.TryParse(textBox_lengthAfterBend.Text, out afterLong);
                elbowOrBendClass.bendFrontLong = frontLong;
                elbowOrBendClass.bendAfterLong = afterLong;
            }
            string pipePressure = pressure(textPipePressure.Text);
            if (pipePressure == null)
            {
                return;
            }
            textPipePressure.Text = pipePressure;
            fuzhi(Convert.ToDouble(comboBox_outer_dn.Text), comboBox_outer_spec.Text);
            eOb = elbowOrBendClass;
            #endregion
            bool fqstart = distence(startTrees.Data.Lianjiedian, startTrees.Data.Xianduan.StartPoint);
            if (fqstart)
            {
                BMECObject bmec = createPipe(startTrees.Data.Xianduan.StartPoint, startTrees.Data.Xianduan.EndPoint);
                startTrees.Data.scBmec.Add(bmec);
                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec);
                elesanIdList.Add(eleid);
            }
            else
            {
                BMECObject bmec = createPipe(startTrees.Data.Xianduan.EndPoint, startTrees.Data.Xianduan.StartPoint);
                startTrees.Data.scBmec.Add(bmec);
                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec);
                elesanIdList.Add(eleid);
            }
            scBmecList(startTrees);
            if(isscCw)
            {
                if(elesanIdList.Count>0)
                {
                    foreach (ulong id in elesanIdList)
                    {
                        BMECObject obj = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
                        api.DeleteFromModel(obj);
                    }
                }
                return;
            }
            #region 以前的生成管道、弯头
            //List<BMECObject> bmec_object_list = new List<BMECObject>();
            //for (int i = 0; i < new_points_list.Count - 1; i++)
            //{

            //    DPoint3d dpt_start = new_points_list[i];
            //    DPoint3d dpt_end = new_points_list[i + 1];
            //    //BMECObject bmec_object = OPM_Public_Api.create_pipe(dpt_start, dpt_end, Convert.ToDouble(comboBox_outer_dn.Text));
            //    //bmec_object.Create();              
            //    BMECObject bmec_object = createPipe(dpt_start, dpt_end);
            //    if (bmec_object == null)
            //    {
            //        if (null != ec_instance_list && ec_instance_list.Count > 0)
            //            MessageBox.Show("请选择一条数据");
            //        return;
            //    }
            //    //bmec_object.SetIntValue(null, 1001);
            //    try
            //    {
            //        bmec_object.DiscoverConnectionsEx();
            //        bmec_object.UpdateConnections();
            //    }
            //    catch
            //    {
            //        MessageBox.Show("PipeLine不能为空");
            //        return;
            //    }
            //    bmec_object_list.Add(bmec_object);
            //}

            //string errorMessage = string.Empty;
            //List<BMECObject> wtList = new List<BMECObject>();
            //List<ulong> eleIdList = new List<ulong>();
            //OPM_Public_Api opmapi = new OPM_Public_Api();
            //ECInstanceList ecList = new ECInstanceList();
            //for (int i = 0; i < bmec_object_list.Count - 1; i++)
            //{
            //    elbowOrBendClass.typeNumber = i;
            //    BMECObject wtObject = opmapi.create_elbow1(bmec_object_list[i], bmec_object_list[i + 1], out errorMessage, elbowOrBendClass, ref ecList);
            //    //api.DeleteFromModel(wtObject);

            //    if (errorMessage != "")
            //    {
            //        if (eleIdList.Count > 0)
            //        {
            //            foreach (ulong id in eleIdList)
            //            {
            //                BMECObject obj = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
            //                api.DeleteFromModel(obj);
            //            }
            //            //foreach(BMECObject bmecwt in wtList)
            //            //{
            //            //    api.DeleteFromModel(bmecwt);
            //            //}

            //        }
            //        foreach (BMECObject bmecoBject in bmec_object_list)
            //        {
            //            api.DeleteFromModel(bmecoBject);
            //        }
            //        MessageBox.Show(errorMessage);
            //        return;
            //    }
            //    ulong ele = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(wtObject);
            //    eleIdList.Add(ele);
            //    wtList.Add(wtObject);
            //}
            #endregion
            #region 对生成的管道进行回切操作
            //string errorMessage = string.Empty;
            //for (int i = 0; i < bmec_object_list.Count - 1; i++)
            //{
            //    string errorMessage_temp;
            //    string PipeLine = bmec_object_list[i].Instance["LINENUMBER"].StringValue; //获取pdm中PipeLine的值
            //    //bmec_object_list[i].Instance.InstanceId
            //    if (PipeLine != "")
            //    {
            //        try
            //        {
            //            BMECObject bmec_object1 = bmec_object_list[i];
            //            ulong id1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object1);
            //            bmec_object1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id1);
            //            BMECObject bmec_object2 = bmec_object_list[i + 1];
            //            ulong id2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object2);
            //            bmec_object2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id2);

            //            OPM_Public_Api.huiqie_pipe(bmec_object1, bmec_object2, out errorMessage_temp);
            //            errorMessage += errorMessage_temp;
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
            //            //CleraPublic();
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        //CleraPublic();
            //        System.Windows.Forms.MessageBox.Show("请先设置PipeLine");
            //        BMECApi.Instance.StartDefaultCommand();
            //        //return;
            //        this.Close();
            //    }
            //}
            #endregion
            //if (errorMessage.Length != 0)
            //{
            //    System.Windows.Forms.MessageBox.Show(errorMessage);
            //    this.Close();
            //}
            System.Windows.Forms.DialogResult dialogresult = System.Windows.Forms.MessageBox.Show("生成管道成功！是否删除原智能线？", "消息", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (dialogresult == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (KeyValuePair<int, Element> elem in elementList)
                {
                    elem.Value.DeleteFromModel();
                }
            }
            //System.Windows.Forms.MessageBox.Show("生成成功");
            BMECApi.Instance.StartDefaultCommand();
            this.Close();
        }

        /// <summary>
        /// 创建管道
        /// </summary>
        /// <param name="dpt_start"></param>
        /// <param name="dpt_end"></param>
        /// <returns></returns>
        private BMECObject createPipe(DPoint3d dpt_start, DPoint3d dpt_end)
        {
            BMECObject ec_object = null;
            BMECObject ecObject = new BMECObject();
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                instance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(textBox_outer_insulation_thickness.Text);
                if (comboBox_outer_insulation_material.Text != "")
                {
                    instance["INSULATION"].StringValue = comboBox_outer_insulation_material.Text;
                }
                instance["STATE"].StringValue = ComboxComponentSate.Text;
                string PipePre = textPipePressure.Text;
                int i = PipePre.IndexOf(" ");
                string pipePresu = PipePre.Substring(0, i);
                instance["NORMAL_OPERATING_PRESSURE"].DoubleValue = Convert.ToDouble(pipePresu);
                ec_object = new BMECObject(instance);
                ecObject.Copy(ec_object);
                ecObject.SetLinearPoints(dpt_start, dpt_end);
                try
                {
                    ecObject.Create();
                }
                catch
                {
                    MessageBox.Show("PipeLine不能为空");
                    return null;
                }
            }
            return ecObject;
        }

        /// <summary>
        /// 设置用户只能输入数字
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_outer_insulation_thickness_KeyPress(object sender, KeyPressEventArgs e)
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

        /// <summary>
        /// 匹配用户输入的值，开头为.数字或者数字.数字，数字才是正确的输入
        /// </summary>
        /// <param name="pressure"></param>
        /// <returns></returns>
        private string pressure(string pressure)
        {
            //string Pipepressure = "0 kPaG";
            Regex reg = new Regex(@"^(\d*\.*\d*){1}"); //正则匹配从开始匹配找到一个为\d*\.*\d*的字符串
            MatchCollection result = reg.Matches(pressure);
            string m = result[0].ToString();
            if (m == "" || result.Count == 0)
            {
                MessageBox.Show("管道压力输入格式不正确");
                return null;
            }
            char[] ch = m.ToCharArray();
            if (ch[0] == '.')
            {
                if (ch[1] == '.')
                {
                    MessageBox.Show("管道压力输入格式不正确");
                    return null;
                }
                else
                {
                    m = "0" + m + " kPaG";
                    return m;
                }
            }
            else
            {
                for (int i = 0; i < ch.Length - 1; i++)
                {
                    if (ch[i] == '.')
                    {
                        if (i == ch.Length - 1)
                        {
                            string mm = m.Substring(0, ch.Length - 1);
                            return mm;
                        }
                        else
                        {
                            if (ch[i + 1] == '.')
                            {
                                MessageBox.Show("管道压力输入格式不正确");
                                return null;
                            }
                            else
                            {
                                m = m + " kPaG";
                                return m;
                            }
                        }
                    }
                }
                m += " kPaG";
                return m;
            }
            //return Pipepressure;
        }

        ///// <summary>
        ///// 选择elbow或者xiamiwan时，显示对应文本框是否可用
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void comboBox_bendOrXiamiwan_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    ComboBox tempComboBox = (ComboBox)sender;
        //    switch (tempComboBox.Text)
        //    {
        //        case "Elbow":
        //            this.textBox_xiamiwan_jieshu.Enabled = false;
        //            this.comboBox_elbow_angle.Enabled = true;
        //            this.comboBox_elbow_radius.Enabled = true;
        //            break;
        //        case "虾米弯":
        //            this.textBox_xiamiwan_jieshu.Enabled = true;
        //            this.comboBox_elbow_radius.Enabled = false;
        //            this.comboBox_elbow_angle.Enabled = false;
        //            break;
        //        default:
        //            break;
        //    }
        //}

        /// <summary>
        /// 选择elbow或者bend时改变窗体大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_elbowOrBend_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox tempComboBox = (ComboBox)sender;
            int groupBoxBendHeight = this.groupBox_bend.Height;
            int groupBoxElbowHeight = this.groupBox_elbow.Height;
            int difHeight = groupBoxElbowHeight - groupBoxBendHeight;
            switch (tempComboBox.Text)
            {
                case "Elbow":
                    Height = fromHeight;
                    //Point butt1 = new Point(12,277);
                    //butt1.Y = butt1.Y - difHeight/2;
                    button1.Location = bendButt;
                    //Point butt2 = new Point(156,277);
                    //butt2.Y = butt2.Y - difHeight/2;
                    //button2.Location = elbowButt1;
                    groupBox3.Height = froupHeigth;
                    groupBox_elbow.Visible = true;
                    groupBox_bend.Visible = false;
                    break;
                case "Bend":
                    this.Height = fromHeight - difHeight;
                    //Point butt3 = new Point(12,387);
                    //butt3.Y = butt3.Y + difHeight/2;
                    button1.Location = bendButt1;
                    //Point butt4 = new Point(156,387);
                    //butt4.Y = butt4.Y + difHeight/2;
                    //button2.Location = elbowButt;
                    groupBox3.Height = uFroupHeigth;//uFroupHeigth
                    this.groupBox_bend.Visible = true;
                    this.groupBox_elbow.Visible = false;
                    break;
                default:
                    break;
            }
        }

        private static string[] putongwantoujiaodu = new string[] { "15度弯头", "30度弯头", "45度弯头", "60度弯头", "90度弯头" };
        private static string[] xiamiwangjiaodu = new string[] { "22.5度弯头", "30度弯头", "45度弯头", "60度弯头", "90度弯头" };
        private void comboBox_elbow_radius_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox tempComboBox = (ComboBox)sender;
            if (tempComboBox.Text == "虾米弯")
            {
                //this.textBox_xiamiwan_jieshu.Enabled = true;
                //this.comboBox_elbow_radius.Enabled = false;
                this.comboBox_elbow_angle.Enabled = true;
                this.checkBox1.Enabled = true;
                this.comboBox_elbow_angle.Items.Clear();
                this.comboBox_elbow_angle.Items.AddRange(xiamiwangjiaodu);
            }
            else
            {
                //this.textBox_xiamiwan_jieshu.Enabled = false;
                this.comboBox_elbow_angle.Enabled = true;
                this.checkBox1.Enabled = true;
                this.comboBox_elbow_angle.Items.Clear();
                this.comboBox_elbow_angle.Items.AddRange(putongwantoujiaodu);
                //this.comboBox_elbow_radius.Enabled = true;
            }
        }

        /// <summary>
        /// 切割弯头未勾选时判断是否满足条件
        /// </summary>
        /// <param name="trees"></param>
        public void pdsfmz(BoTree<Segment> trees)
        {
            double anglera = Convert.ToDouble(comboBox_elbow_angle.Text.Substring(0, 2));
            if (trees.Nodes.Count > 0)
            {
                foreach (BoTree<Segment> tree in trees.Nodes)
                {
                    if (tree.Data.OtherDpoint && tree.Data.Xianduanshu == 1)
                    {
                        DPoint3d zilianDp = tree.Data.Lianjiedian;
                        bool startz = distence(zilianDp, tree.Data.Xianduan.StartPoint);
                        if (startz)
                        {
                            DVector3d zdv = new DVector3d(tree.Data.Xianduan.EndPoint, tree.Data.Xianduan.StartPoint);
                            bool fb = distence(zilianDp, trees.Data.Xianduan.StartPoint);
                            DVector3d fdv;
                            if (fb)
                            {
                                fdv = new DVector3d(trees.Data.Xianduan.EndPoint, trees.Data.Xianduan.StartPoint);
                            }
                            else
                            {
                                fdv = new DVector3d(trees.Data.Xianduan.StartPoint, trees.Data.Xianduan.EndPoint);
                            }
                            bool gx = fdv.IsParallelOrOppositeTo(zdv);
                            if (!gx)
                            {
                                Angle ag = zdv.AngleTo(fdv);
                                double ang = ag.Degrees;
                                if (Math.Abs(ang - anglera) > 0.1)
                                {
                                    qgwt = true;
                                }
                            }

                        }
                        bool endz = distence(zilianDp, tree.Data.Xianduan.EndPoint);
                        if (endz)
                        {
                            DVector3d zdv = new DVector3d(tree.Data.Xianduan.StartPoint, tree.Data.Xianduan.EndPoint);
                            bool fb = distence(zilianDp, trees.Data.Xianduan.StartPoint);
                            DVector3d fdv;
                            if (fb)
                            {
                                fdv = new DVector3d(trees.Data.Xianduan.EndPoint, trees.Data.Xianduan.StartPoint);
                            }
                            else
                            {
                                fdv = new DVector3d(trees.Data.Xianduan.StartPoint, trees.Data.Xianduan.EndPoint);
                            }
                            bool gx = fdv.IsParallelOrOppositeTo(zdv);
                            if (!gx)
                            {
                                Angle ag = zdv.AngleTo(fdv);
                                double ang = ag.Degrees;
                                if (Math.Abs(ang - anglera) > 0.1)
                                {
                                    qgwt = true;
                                }
                            }
                        }
                    }
                    pdsfmz(tree);
                }
            }
            //return false;
        }

        /// <summary>
        /// 根据点的距离判断相等
        /// </summary>
        /// <param name="dp1"></param>
        /// <param name="dp2"></param>
        /// <returns></returns>
        public bool distence(DPoint3d dp1, DPoint3d dp2)
        {
            bool b = false;
            double dis = dp1.Distance(dp2);
            if (dis < 0.00000001)
            {
                b = true;
            }
            return b;
        }

        int issantong = 0, chishu = 0;
        ECInstanceList santEcList = new ECInstanceList();
        ECInstanceList ecList = new ECInstanceList();
        /// <summary>
        /// 生成管道、弯头、三通
        /// </summary>
        /// <param name="trees"></param>
        public void scBmecList(BoTree<Segment> trees)
        {
            bool stfd = distence(trees.Data.Lianjiedian, trees.Data.Xianduan.StartPoint);
            bool endfd = distence(trees.Data.Lianjiedian, trees.Data.Xianduan.EndPoint);
            if (stfd || endfd)
            {
                #region
                if (trees.Nodes.Count > 0)
                {
                    int ddcs = 0;
                    for (int indexNode = 0; indexNode < trees.Nodes.Count; indexNode++)
                    {
                        if (!trees.Nodes[indexNode].Data.OtherDpoint)
                        {
                            bool startz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                            if (startz)
                            {
                                BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                elesanIdList.Add(eleid);
                                if (issantong == 0)
                                {
                                    fhuzhisant();
                                }
                                BMECObject santBmec = santong(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0]);
                                if (santBmec != null)
                                {
                                    ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                    elesanIdList.Add(eleid1);
                                }
                            }
                            bool endz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                            if (endz)
                            {
                                BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                elesanIdList.Add(eleid);
                                if (issantong == 0)
                                {
                                    fhuzhisant();
                                }
                                BMECObject santBmec = santong(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0]);
                                if (santBmec != null)
                                {
                                    ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                    elesanIdList.Add(eleid1);
                                }
                            }
                        }
                        else
                        {
                            if (!trees.Nodes[indexNode].Data.IsSanTong)
                            {
                                DPoint3d[] fqDp = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(trees.Data.scBmec[0]);
                                DVector3d fqdv = new DVector3d(fqDp[0], fqDp[1]);
                                DVector3d ezDv = new DVector3d(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                bool hb = fqdv.IsParallelOrOppositeTo(ezDv);
                                bool startEz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                if (startEz)
                                {
                                    if (hb)
                                    {
                                        trees.Data.scBmec[0].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                        trees.Data.scBmec[0].Create();
                                        trees.Data.scBmec[0].DiscoverConnectionsEx();
                                        trees.Data.scBmec[0].UpdateConnections();
                                        trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[0]);
                                    }
                                    else
                                    {
                                        BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                        ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                        elesanIdList.Add(eleid);
                                        eOb.typeNumber = chishu;
                                        
                                        string errorMessage = string.Empty;
                                        List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[0], bmecz, out errorMessage, eOb, ref ecList);
                                        if (errorMessage != string.Empty || errorMessage != "")
                                        {
                                            MessageBox.Show(errorMessage);
                                            //return;
                                            isscCw = true;
                                        }
                                        else
                                        {
                                            if (wtObject != null)
                                            {
                                                foreach(BMECObject bmeObj in wtObject)
                                                {
                                                    ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmeObj);
                                                    elesanIdList.Add(eleid1);
                                                }
                                                
                                            }
                                        }
                                        chishu++;
                                    }
                                }
                                else
                                {
                                    if (hb)
                                    {
                                        trees.Data.scBmec[0].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                        trees.Data.scBmec[0].Create();
                                        trees.Data.scBmec[0].DiscoverConnectionsEx();
                                        trees.Data.scBmec[0].UpdateConnections();
                                        trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[0]);
                                    }
                                    else
                                    {
                                        BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                        ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                        elesanIdList.Add(eleid);
                                        eOb.typeNumber = chishu;
                                        //ECInstanceList ecList = new ECInstanceList();
                                        string errorMessage = string.Empty;
                                        List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[0], bmecz, out errorMessage, eOb, ref ecList);
                                        if (errorMessage != string.Empty || errorMessage != "")
                                        {
                                            MessageBox.Show(errorMessage);
                                            //return;
                                            isscCw = true;
                                        }
                                        else
                                        {
                                            foreach (BMECObject bmeObj in wtObject)
                                            {
                                                ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmeObj);
                                                elesanIdList.Add(eleid1);
                                            }
                                        }
                                        chishu++;
                                    }
                                }
                            }

                            else
                            {
                                if (issantong == 0)
                                {
                                    fhuzhisant();
                                }
                                if (trees.Nodes[indexNode].Data.Xianduanshu==1)
                                {
                                    BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                    elesanIdList.Add(eleid);
                                    BMECObject pipe2 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(pipe2);
                                    ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe2);
                                    elesanIdList.Add(eleid1);
                                    BMECObject santBmec = santong1(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[1]);
                                    if(santBmec!=null)
                                    {
                                        ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                        elesanIdList.Add(eleid2);
                                    }
                                }
                                else if(trees.Nodes[indexNode].Data.Xianduanshu==2)
                                {
                                    if(ddcs==0)
                                    {
                                        ddcs++;
                                        bool fstartdp = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                        if(fstartdp)
                                        {
                                            BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                            elesanIdList.Add(eleid);
                                        }
                                        else
                                        {
                                            BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                            elesanIdList.Add(eleid);
                                        }
                                        bool estartdp = distence(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                        if(estartdp)
                                        {
                                            BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.EndPoint);
                                            trees.Nodes[indexNode+1].Data.scBmec.Add(pipe1);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                            elesanIdList.Add(eleid);
                                        }
                                        else
                                        {
                                            BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                            trees.Nodes[indexNode + 1].Data.scBmec.Add(pipe1);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                            elesanIdList.Add(eleid);
                                        }
                                        BMECObject santBmec = santong1(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode+1].Data.scBmec[0]);
                                        if (santBmec != null)
                                        {
                                            ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                            elesanIdList.Add(eleid2);
                                        }
                                    }
                                }
                            }
                        }
                        scBmecList(trees.Nodes[indexNode]);
                    }

                }
                #endregion
            }
            else
            {
                #region
                if (trees.Nodes.Count > 0)
                {
                    int ddcs = 0, ddcs1 = 0;
                    double fqdistence = trees.Data.Lianjiedian.Distance(trees.Data.Xianduan.StartPoint);
                    for (int indexNode = 0; indexNode < trees.Nodes.Count; indexNode++)
                    {
                        if(trees.Nodes[indexNode].Data.Distence<fqdistence)
                        {
                            #region
                            if (!trees.Nodes[indexNode].Data.OtherDpoint)
                            {
                                bool startz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                if (startz)
                                {
                                    BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                    elesanIdList.Add(eleid);
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    BMECObject santBmec = santong(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0]);
                                    if (santBmec != null)
                                    {
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                        elesanIdList.Add(eleid1);
                                    }
                                }
                                bool endz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                if (endz)
                                {
                                    BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                    elesanIdList.Add(eleid);
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    BMECObject santBmec = santong(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0]);
                                    if (santBmec != null)
                                    {
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                        elesanIdList.Add(eleid1);
                                    }
                                }
                            }
                            else
                            {
                                if (!trees.Nodes[indexNode].Data.IsSanTong)
                                {
                                    DPoint3d[] fqDp = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(trees.Data.scBmec[0]);
                                    DVector3d fqdv = new DVector3d(fqDp[0], fqDp[1]);
                                    DVector3d ezDv = new DVector3d(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                    bool hb = fqdv.IsParallelOrOppositeTo(ezDv);
                                    bool startEz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                    if (startEz)
                                    {
                                        if (hb)
                                        {
                                            trees.Data.scBmec[0].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                            trees.Data.scBmec[0].Create();
                                            trees.Data.scBmec[0].DiscoverConnectionsEx();
                                            trees.Data.scBmec[0].UpdateConnections();
                                            trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[0]);
                                        }
                                        else
                                        {
                                            BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                            elesanIdList.Add(eleid);
                                            eOb.typeNumber = chishu;
                                            //ECInstanceList ecList = new ECInstanceList();
                                            string errorMessage = string.Empty;
                                            List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[0], bmecz, out errorMessage, eOb, ref ecList);
                                            if (errorMessage != string.Empty || errorMessage != "")
                                            {
                                                MessageBox.Show(errorMessage);
                                                //return;
                                                isscCw = true;
                                            }
                                            else
                                            {
                                                if (wtObject != null)
                                                {
                                                    foreach(BMECObject bmecobj in wtObject)
                                                    {
                                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecobj);
                                                        elesanIdList.Add(eleid1);
                                                    }
                                                    
                                                }
                                            }
                                            chishu++;
                                        }
                                    }
                                    else
                                    {
                                        if (hb)
                                        {
                                            trees.Data.scBmec[0].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            trees.Data.scBmec[0].Create();
                                            trees.Data.scBmec[0].DiscoverConnectionsEx();
                                            trees.Data.scBmec[0].UpdateConnections();
                                            trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[0]);
                                        }
                                        else
                                        {
                                            BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                            elesanIdList.Add(eleid);
                                            eOb.typeNumber = chishu;
                                            //ECInstanceList ecList = new ECInstanceList();
                                            string errorMessage = string.Empty;
                                            List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[0], bmecz, out errorMessage, eOb, ref ecList);
                                            if (errorMessage != string.Empty || errorMessage != "")
                                            {
                                                MessageBox.Show(errorMessage);
                                                //return;
                                                isscCw = true;
                                            }
                                            else
                                            {
                                                if (wtObject != null)
                                                {
                                                    foreach (BMECObject bmecobj in wtObject)
                                                    {
                                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecobj);
                                                        elesanIdList.Add(eleid1);
                                                    }
                                                }
                                            }
                                            chishu++;
                                        }
                                    }
                                }

                                else
                                {
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    if (trees.Nodes[indexNode].Data.Xianduanshu == 1)
                                    {
                                        BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                        ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                        elesanIdList.Add(eleid);
                                        BMECObject pipe2 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(pipe2);
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe2);
                                        elesanIdList.Add(eleid1);
                                        BMECObject santBmec = santong1(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[1]);
                                        if (santBmec != null)
                                        {
                                            ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                            elesanIdList.Add(eleid2);
                                        }
                                    }
                                    else if (trees.Nodes[indexNode].Data.Xianduanshu == 2)
                                    {
                                        if (ddcs == 0)
                                        {
                                            ddcs++;
                                            bool fstartdp = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            if (fstartdp)
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                                trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            else
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                                trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            bool estartdp = distence(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                            if (estartdp)
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.EndPoint);
                                                trees.Nodes[indexNode + 1].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            else
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                                trees.Nodes[indexNode + 1].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            BMECObject santBmec = santong1(trees.Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode + 1].Data.scBmec[0]);
                                            if (santBmec != null)
                                            {
                                                ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                                elesanIdList.Add(eleid2);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        else
                        {
                            #region
                            if (!trees.Nodes[indexNode].Data.OtherDpoint)
                            {
                                bool startz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                if (startz)
                                {
                                    BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                    elesanIdList.Add(eleid);
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    BMECObject santBmec = santong(trees.Data.scBmec[1], trees.Nodes[indexNode].Data.scBmec[0]);
                                    if (santBmec != null)
                                    {
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                        elesanIdList.Add(eleid1);
                                    }
                                }
                                bool endz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                if (endz)
                                {
                                    BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                    trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                    elesanIdList.Add(eleid);
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    BMECObject santBmec = santong(trees.Data.scBmec[1], trees.Nodes[indexNode].Data.scBmec[0]);
                                    if (santBmec != null)
                                    {
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                        elesanIdList.Add(eleid1);
                                    }
                                }
                            }
                            else
                            {
                                if (!trees.Nodes[indexNode].Data.IsSanTong)
                                {
                                    DPoint3d[] fqDp = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(trees.Data.scBmec[1]);
                                    DVector3d fqdv = new DVector3d(fqDp[0], fqDp[1]);
                                    DVector3d ezDv = new DVector3d(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                    bool hb = fqdv.IsParallelOrOppositeTo(ezDv);
                                    bool startEz = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                    if (startEz)
                                    {
                                        if (hb)
                                        {
                                            trees.Data.scBmec[1].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                            trees.Data.scBmec[1].Create();
                                            trees.Data.scBmec[1].DiscoverConnectionsEx();
                                            trees.Data.scBmec[1].UpdateConnections();
                                            trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[1]);
                                        }
                                        else
                                        {
                                            BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.StartPoint, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                            elesanIdList.Add(eleid);
                                            eOb.typeNumber = chishu;
                                            //ECInstanceList ecList = new ECInstanceList();
                                            string errorMessage = string.Empty;
                                            List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[1], bmecz, out errorMessage, eOb, ref ecList);
                                            if (errorMessage != string.Empty || errorMessage != "")
                                            {
                                                MessageBox.Show(errorMessage);
                                                //return;
                                                isscCw = true;
                                            }
                                            else
                                            {
                                                if (wtObject != null)
                                                {
                                                    foreach (BMECObject bmecobj in wtObject)
                                                    {
                                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecobj);
                                                        elesanIdList.Add(eleid1);
                                                    }
                                                }
                                            }
                                            chishu++;
                                        }
                                    }
                                    else
                                    {
                                        if (hb)
                                        {
                                            trees.Data.scBmec[1].SetLinearPoints(fqDp[0], trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            trees.Data.scBmec[1].Create();
                                            trees.Data.scBmec[1].DiscoverConnectionsEx();
                                            trees.Data.scBmec[1].UpdateConnections();
                                            trees.Nodes[indexNode].Data.scBmec.Add(trees.Data.scBmec[1]);
                                        }
                                        else
                                        {
                                            BMECObject bmecz = createPipe(trees.Nodes[indexNode].Data.Xianduan.EndPoint, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            trees.Nodes[indexNode].Data.scBmec.Add(bmecz);
                                            ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecz);
                                            elesanIdList.Add(eleid);
                                            eOb.typeNumber = chishu;
                                            //ECInstanceList ecList = new ECInstanceList();
                                            string errorMessage = string.Empty;
                                            List<BMECObject> wtObject = opmapi1.create_elbow1(trees.Data.scBmec[1], bmecz, out errorMessage, eOb, ref ecList);
                                            if (errorMessage != string.Empty || errorMessage != "")
                                            {
                                                MessageBox.Show(errorMessage);
                                                //return;
                                                isscCw = true;
                                            }
                                            else
                                            {
                                                if (wtObject != null)
                                                {
                                                    foreach (BMECObject bmecobj in wtObject)
                                                    {
                                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecobj);
                                                        elesanIdList.Add(eleid1);
                                                    }
                                                }
                                            }
                                            chishu++;
                                        }
                                    }
                                }

                                else
                                {
                                    if (issantong == 0)
                                    {
                                        fhuzhisant();
                                    }
                                    if (trees.Nodes[indexNode].Data.Xianduanshu == 1)
                                    {
                                        BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                        ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                        elesanIdList.Add(eleid);
                                        BMECObject pipe2 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                        trees.Nodes[indexNode].Data.scBmec.Add(pipe2);
                                        ulong eleid1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe2);
                                        elesanIdList.Add(eleid1);
                                        BMECObject santBmec = santong1(trees.Data.scBmec[1], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode].Data.scBmec[1]);
                                        if (santBmec != null)
                                        {
                                            ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                            elesanIdList.Add(eleid2);
                                        }
                                    }
                                    else if (trees.Nodes[indexNode].Data.Xianduanshu == 2)
                                    {
                                        if (ddcs1 == 0)
                                        {
                                            ddcs1++;
                                            bool fstartdp = distence(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                            if (fstartdp)
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.EndPoint);
                                                trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            else
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode].Data.Lianjiedian, trees.Nodes[indexNode].Data.Xianduan.StartPoint);
                                                trees.Nodes[indexNode].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            bool estartdp = distence(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                            if (estartdp)
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.EndPoint);
                                                trees.Nodes[indexNode + 1].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            else
                                            {
                                                BMECObject pipe1 = createPipe(trees.Nodes[indexNode + 1].Data.Lianjiedian, trees.Nodes[indexNode + 1].Data.Xianduan.StartPoint);
                                                trees.Nodes[indexNode + 1].Data.scBmec.Add(pipe1);
                                                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe1);
                                                elesanIdList.Add(eleid);
                                            }
                                            BMECObject santBmec = santong1(trees.Data.scBmec[1], trees.Nodes[indexNode].Data.scBmec[0], trees.Nodes[indexNode + 1].Data.scBmec[0]);
                                            if (santBmec != null)
                                            {
                                                ulong eleid2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(santBmec);
                                                elesanIdList.Add(eleid2);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                        scBmecList(trees.Nodes[indexNode]);
                    }

                }
                #endregion
            }
        }

        public void fhuzhisant()
        {
            IECInstance iecInstance = BMECInstanceManager.Instance.CreateECInstance("EQUAL_PIPE_TEE", true);
            iecInstance["NOMINAL_DIAMETER"].DoubleValue = Convert.ToDouble(comboBox_outer_dn.Text);
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(iecInstance, null);
            santEcList = specProcessor.SelectSpec(iecInstance, true);
            issantong++;
        }

        public BMECObject santong(BMECObject fqBmec, BMECObject ezBmec)
        {
            BMECObject santongBmec = null;
            DPoint3d[] fqshuzhu = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(fqBmec);
            DPoint3d[] erzishuzhu = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(ezBmec);
            DVector3d fqdv = new DVector3d(fqshuzhu[0], fqshuzhu[1]);
            DVector3d scdv = new DVector3d(fqshuzhu[0], erzishuzhu[0]);
            bool tx = fqdv.IsParallelTo(scdv);
            if (!tx)
            {
                MessageBox.Show("存在智能线长度过短！");
                isscCw = true;
                return null;
            }
            if (santEcList != null && santEcList.Count > 0)
            {
                IECInstance santEcinstance = santEcList[0];
                santEcinstance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(textBox_outer_insulation_thickness.Text);
                if (comboBox_outer_insulation_material.Text != "")
                {
                    santEcinstance["INSULATION"].StringValue = comboBox_outer_insulation_material.Text;
                }
                santEcinstance["STATE"].StringValue = ComboxComponentSate.Text;
                string PipePre = textPipePressure.Text;
                int i = PipePre.IndexOf(" ");
                string pipePresu = PipePre.Substring(0, i);
                santEcinstance["NORMAL_OPERATING_PRESSURE"].DoubleValue = Convert.ToDouble(pipePresu);
                double outlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                double runlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue;
                double branchlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_BRANCH_END"].DoubleValue;
                DVector3d dv1 = new DVector3d(erzishuzhu[0], fqshuzhu[0]);
                DPoint3d dp1 = yidong(erzishuzhu[0], dv1, outlength * 1000);
                DVector3d dv2 = new DVector3d(erzishuzhu[0], fqshuzhu[1]);
                DPoint3d dp2 = yidong(erzishuzhu[0], dv2, runlength * 1000);
                DVector3d dv3 = new DVector3d(erzishuzhu[0], erzishuzhu[1]);
                DPoint3d dp3 = yidong(erzishuzhu[0], dv3, branchlength * 1000);
                DVector3d scdv1 = new DVector3d(fqshuzhu[0], dp1);
                bool issc1 = scdv1.IsParallelTo(fqdv);
                DVector3d scdv2 = new DVector3d(dp2, fqshuzhu[1]);
                bool issc2 = scdv2.IsParallelTo(fqdv);
                DVector3d scdv3 = new DVector3d(dp3, erzishuzhu[1]);
                bool issc3 = scdv3.IsParallelTo(new DVector3d(erzishuzhu[0], erzishuzhu[1]));
                if (!issc1 || !issc2 || !issc3)
                {
                    MessageBox.Show("存在智能线长度过短！");
                    isscCw = true;
                    return null;
                }
                fqBmec.SetLinearPoints(dp2, fqshuzhu[1]);
                fqBmec.Create();
                ezBmec.SetLinearPoints(dp3, erzishuzhu[1]);
                ezBmec.Create();
                BMECObject xscBmec = createPipe(fqshuzhu[0], dp1);
                ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(xscBmec);
                elesanIdList.Add(eleid);
                BMECObject santongy = new BMECObject(santEcinstance);
                santongBmec = new BMECObject();
                santongBmec.Copy(santongy);
                santongBmec.Create();
                BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
                BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
                BIM.Point3d vip1 = ceDpoint_v8Point(fqshuzhu[0]);
                BIM.Point3d vip2 = ceDpoint_v8Point(dp1);
                BIM.Point3d vip3 = ceDpoint_v8Point(dp3);
                BIM.Point3d vip4 = ceDpoint_v8Point(erzishuzhu[1]);
                BIM.Point3d v1 = app.Point3dSubtract(vip2, vip1);
                BIM.Point3d v2 = app.Point3dSubtract(vip4, vip3);
                JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(santongBmec, dir1, v1, dir2, v2, vip2, vip3);
                try
                {
                    fqBmec.DiscoverConnectionsEx();
                    fqBmec.UpdateConnections();
                    ezBmec.DiscoverConnectionsEx();
                    ezBmec.UpdateConnections();
                    xscBmec.DiscoverConnectionsEx();
                    xscBmec.UpdateConnections();
                    santongBmec.DiscoverConnectionsEx();
                    santongBmec.UpdateConnections();
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("PipeLine不能为空");
                    isscCw = true;
                    return null;
                }
                //fqBmec.SetLinearPoints()
            }
            return santongBmec;
        }

        public BMECObject santong1(BMECObject fqBmec, BMECObject ezBmec1, BMECObject ezBmec2)
        {
            BMECObject santBmec = null;
            DPoint3d[] fqshuzhu = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(fqBmec);
            DPoint3d[] erzishuzhu = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(ezBmec1);
            DPoint3d[] erzhishuzhu1 = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(ezBmec2);
            DVector3d dv1 = new DVector3d(fqshuzhu[0], fqshuzhu[1]);
            DVector3d dv2 = new DVector3d(erzishuzhu[0], erzishuzhu[1]);
            DVector3d dv3 = new DVector3d(erzhishuzhu1[0], erzhishuzhu1[1]);
            bool isOW = dv1.IsParallelOrOppositeTo(dv2);
            bool isOT = dv1.IsParallelOrOppositeTo(dv3);
            bool isWT = dv2.IsParallelOrOppositeTo(dv3);
            if (santEcList != null && santEcList.Count > 0)
            {
                IECInstance santEcinstance = santEcList[0];
                santEcinstance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(textBox_outer_insulation_thickness.Text);
                if (comboBox_outer_insulation_material.Text != "")
                {
                    santEcinstance["INSULATION"].StringValue = comboBox_outer_insulation_material.Text;
                }
                santEcinstance["STATE"].StringValue = ComboxComponentSate.Text;
                string PipePre = textPipePressure.Text;
                int i = PipePre.IndexOf(" ");
                string pipePresu = PipePre.Substring(0, i);
                santEcinstance["NORMAL_OPERATING_PRESSURE"].DoubleValue = Convert.ToDouble(pipePresu);
                double outlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                double runlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue;
                double branchlength = santEcinstance["DESIGN_LENGTH_CENTER_TO_BRANCH_END"].DoubleValue;
                if (isOW)
                {
                    DPoint3d scdp1, scdp2, scdp3;
                    santBmec = santong2(santEcinstance, fqshuzhu[0], erzhishuzhu1[0], erzishuzhu[1], erzhishuzhu1[1], outlength, runlength, branchlength,out scdp1,out scdp2,out scdp3);
                    if (santBmec != null)
                    {
                        fqBmec.SetLinearPoints(fqshuzhu[0], scdp1);
                        fqBmec.Create();
                        ezBmec1.SetLinearPoints(scdp2, erzishuzhu[1]);
                        ezBmec1.Create();
                        ezBmec2.SetLinearPoints(scdp3, erzhishuzhu1[1]);
                        ezBmec2.Create();
                        #region
                        //DVector3d dv1 = new DVector3d(erzishuzhu[0], fqshuzhu[0]);
                        //DPoint3d dp1 = yidong(erzishuzhu[0], dv1, outlength * 1000);
                        //DVector3d dv2 = new DVector3d(erzishuzhu[0], fqshuzhu[1]);
                        //DPoint3d dp2 = yidong(erzishuzhu[0], dv2, runlength * 1000);
                        //DVector3d dv3 = new DVector3d(erzishuzhu[0], erzishuzhu[1]);
                        //DPoint3d dp3 = yidong(erzishuzhu[0], dv3, branchlength * 1000);
                        //DVector3d scdv1 = new DVector3d(fqshuzhu[0], dp1);
                        //bool issc1 = scdv1.IsParallelTo(fqdv);
                        //DVector3d scdv2 = new DVector3d(dp2, fqshuzhu[1]);
                        //bool issc2 = scdv2.IsParallelTo(fqdv);
                        //DVector3d scdv3 = new DVector3d(dp3, erzishuzhu[1]);
                        //bool issc3 = scdv3.IsParallelTo(new DVector3d(erzishuzhu[0], erzishuzhu[1]));
                        //if (!issc1 || !issc2 || !issc3)
                        //{
                        //    MessageBox.Show("存在智能线长度过短！");
                        //    isscCw = true;
                        //    return null;
                        //}
                        #endregion
                    }
                }
                if(isOT)
                {
                    DPoint3d scdp1, scdp2, scdp3;
                    santBmec = santong2(santEcinstance, fqshuzhu[0], erzishuzhu[0], erzhishuzhu1[1], erzishuzhu[1], outlength, runlength, branchlength, out scdp1, out scdp2, out scdp3);
                    if(santBmec!=null)
                    {
                        fqBmec.SetLinearPoints(fqshuzhu[0], scdp1);
                        fqBmec.Create();
                        ezBmec2.SetLinearPoints(scdp2, erzhishuzhu1[1]);
                        ezBmec2.Create();
                        ezBmec1.SetLinearPoints(scdp3, erzishuzhu[1]);
                        ezBmec1.Create();
                    }
                }
                if(isWT)
                {
                    DPoint3d scdp1, scdp2, scdp3;
                    santBmec = santong2(santEcinstance, erzishuzhu[0], fqshuzhu[0], erzhishuzhu1[1], fqshuzhu[1], outlength, runlength, branchlength, out scdp1, out scdp2, out scdp3);
                    if(santBmec!=null)
                    {
                        ezBmec1.SetLinearPoints(scdp1, erzishuzhu[1]);
                        ezBmec1.Create();
                        ezBmec2.SetLinearPoints(scdp2, erzhishuzhu1[1]);
                        ezBmec2.Create();
                        fqBmec.SetLinearPoints(fqshuzhu[0], scdp3);
                        fqBmec.Create();
                    }
                }
                if(santBmec!=null)
                {
                    try
                    {
                        fqBmec.DiscoverConnectionsEx();
                        fqBmec.UpdateConnections();
                        ezBmec1.DiscoverConnectionsEx();
                        ezBmec1.UpdateConnections();
                        ezBmec2.DiscoverConnectionsEx();
                        ezBmec2.UpdateConnections();
                        santBmec.DiscoverConnectionsEx();
                        santBmec.UpdateConnections();
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("PipeLine不能为空");
                        isscCw = true;
                        return null;
                    }
                }
            }
            return santBmec;
        }

        public DPoint3d yidong(DPoint3d originDp, DVector3d dv, double juli)
        {
            DPoint3d targetDp = new DPoint3d();
            double dvLength = dv.Magnitude;
            DVector3d unitDv = DVector3d.Multiply(dv, 1 / dvLength);
            targetDp = DPoint3d.Add(originDp, unitDv, juli);
            return targetDp;
        }

        /// <summary>
        /// 将ce的DPoint转化为V8I的Point3D
        /// </summary>
        /// <param name="dpoint3d"></param>
        /// <returns></returns>
        public static BIM.Point3d ceDpoint_v8Point(DPoint3d dpoint3d)
        {
            double uro = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

            BIM.Point3d point = app.Point3dFromXYZ(dpoint3d.X / uro, dpoint3d.Y / uro, dpoint3d.Z / uro);
            return point;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Enabled = checkBox1.Checked;
            radioButton2.Enabled = checkBox1.Checked;
            if (radioButton1.Checked)
            {
                radioButton3.Enabled = checkBox1.Checked;
                radioButton4.Enabled = checkBox1.Checked;
            }
            
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton3.Enabled = radioButton1.Checked;
            radioButton4.Enabled = radioButton1.Checked;
        }

        public BMECObject santong2(IECInstance sanInstance, DPoint3d dp1, DPoint3d dp2, DPoint3d dp3, DPoint3d dp4, double mLength, double rLength, double bLength,out DPoint3d scdp1,out DPoint3d scdp2,out DPoint3d scdp3)
        {
            BMECObject santong2Bmec = null;
            DVector3d ydv1 = new DVector3d(dp1, dp3);
            DVector3d ydv2 = new DVector3d(dp2, dp4);
            #region
            //DVector3d dv1 = new DVector3d(erzishuzhu[0], fqshuzhu[0]);
            //DPoint3d dp1 = yidong(erzishuzhu[0], dv1, outlength * 1000);
            //DVector3d dv2 = new DVector3d(erzishuzhu[0], fqshuzhu[1]);
            //DPoint3d dp2 = yidong(erzishuzhu[0], dv2, runlength * 1000);
            //DVector3d dv3 = new DVector3d(erzishuzhu[0], erzishuzhu[1]);
            //DPoint3d dp3 = yidong(erzishuzhu[0], dv3, branchlength * 1000);
            //DVector3d scdv1 = new DVector3d(fqshuzhu[0], dp1);
            //bool issc1 = scdv1.IsParallelTo(fqdv);
            //DVector3d scdv2 = new DVector3d(dp2, fqshuzhu[1]);
            //bool issc2 = scdv2.IsParallelTo(fqdv);
            //DVector3d scdv3 = new DVector3d(dp3, erzishuzhu[1]);
            //bool issc3 = scdv3.IsParallelTo(new DVector3d(erzishuzhu[0], erzishuzhu[1]));
            //if (!issc1 || !issc2 || !issc3)
            //{
            //    MessageBox.Show("存在智能线长度过短！");
            //    isscCw = true;
            //    return null;
            //}
            #endregion
            DVector3d dv1 = new DVector3d(dp2, dp1);
            scdp1 = yidong(dp2, dv1, mLength * 1000);
            DVector3d dv2 = new DVector3d(dp2, dp3);
            scdp2 = yidong(dp2, dv2, rLength * 1000);
            DVector3d dv3 = new DVector3d(dp2, dp4);
            scdp3 = yidong(dp2, dv3, bLength * 1000);
            DVector3d scdv1 = new DVector3d(dp1, scdp1);
            bool issc1 = scdv1.IsParallelTo(ydv1);
            DVector3d scdv2 = new DVector3d(scdp2, dp3);
            bool issc2 = scdv2.IsParallelTo(ydv1);
            DVector3d scdv3 = new DVector3d(scdp3, dp4);
            bool issc3 = scdv3.IsParallelTo(ydv2);
            if (!issc1 || !issc2 || !issc3)
            {
                MessageBox.Show("存在智能线长度过短！");
                isscCw = true;
                return null;
            }
            BMECObject santongy = new BMECObject(sanInstance);
            santong2Bmec = new BMECObject();
            santong2Bmec.Copy(santongy);
            santong2Bmec.Create();
            #region
            //BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            //BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
            //BIM.Point3d vip1 = ceDpoint_v8Point(fqshuzhu[0]);
            //BIM.Point3d vip2 = ceDpoint_v8Point(dp1);
            //BIM.Point3d vip3 = ceDpoint_v8Point(dp3);
            //BIM.Point3d vip4 = ceDpoint_v8Point(erzishuzhu[1]);
            //BIM.Point3d v1 = app.Point3dSubtract(vip2, vip1);
            //BIM.Point3d v2 = app.Point3dSubtract(vip4, vip3);
            //JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(santongBmec, dir1, v1, dir2, v2, vip2, vip3);
            #endregion
            BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
            BIM.Point3d vip1 = ceDpoint_v8Point(dp1);
            BIM.Point3d vip2 = ceDpoint_v8Point(scdp1);
            BIM.Point3d vip3 = ceDpoint_v8Point(scdp3);
            BIM.Point3d vip4 = ceDpoint_v8Point(dp4);
            BIM.Point3d v1 = app.Point3dSubtract(vip2, vip1);
            BIM.Point3d v2 = app.Point3dSubtract(vip4, vip3);
            JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(santong2Bmec, dir1, v1, dir2, v2, vip2, vip3);
            return santong2Bmec;
        }
    }
}
