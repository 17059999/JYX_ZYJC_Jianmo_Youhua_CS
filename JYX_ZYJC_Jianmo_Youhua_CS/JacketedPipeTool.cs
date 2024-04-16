﻿using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using BIM = Bentley.Interop.MicroStationDGN;

using Bentley.MstnPlatformNET.InteropServices;
using Bentley.GeometryNET;
using Bentley.ECObjects.Instance;
using Bentley.Building.Mechanical.Components;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.Plant.StandardPreferences;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 
    /// </summary>
    public class JacketedPipeTool : DgnElementSetTool
    {
        /// <summary>
        /// 
        /// </summary>
        protected BMECApi api = BMECApi.Instance;
        private static BIM.Application app = Utilities.ComApp;
        List<BMECObject> BMEC_Object_list = new List<BMECObject>();
        DPoint3d startPoint = DPoint3d.Zero;//第一点
        DPoint3d endPoint = DPoint3d.Zero;//第二点
        int pointCount = 0;//点计数
        public static JacketedPipeToolForm m_jacketedPipeToolForm = null;
        private IECInstance outerDynamicPipe = null;
        private string nominalDiameter = "";//OPM界面的管径

        /// <summary>
        /// 程序关闭窗体状态
        /// </summary>
        public enum StatusCloseFormEvent
        {
            DEFAULT = 0,
            FORM,
            RESETBUTTON,
            CLEANUP
        }
        /// <summary>
        /// 是否由程序关闭窗体
        /// </summary>
        public static StatusCloseFormEvent isClosedFromCode = StatusCloseFormEvent.DEFAULT;

        /// <summary>
        /// 初始化数据
        /// </summary>
        protected void init() {
            BMEC_Object_list.Clear();
            startPoint = DPoint3d.Zero;
            endPoint = DPoint3d.Zero;
            pointCount = 0;
            //refresh();
        }
        //试着通过重写父类单步找到最近的调用函数
        protected override bool OnModelMotion(DgnButtonEvent ev)
        {
            //Test
            //Bentley.Interop.MicroStationDGN.Point3d origin = new Bentley.Interop.MicroStationDGN.Point3d();
            //origin.X = 0.0;
            //origin.Y = 0.0;
            //origin.Z = 0.0;
            //app.CommandState.AccuDrawHints.SetOrigin(ref origin);
            //AccuDraw.LockedStates tempstates = AccuDraw.Locked;
            //DPoint3d anchorPt = DPoint3d.Zero;
            //GetAnchorPoint(out anchorPt);
            //AccuDraw.SetContext(AccuDrawFlags.OrientDefault);

            //bool flag = AccuDraw.FloatingOrigin;
            //bool isactive = AccuDraw.Active;
            //AccuDraw.RotationMode currentRotaion = AccuDraw.ActiveRotationMode;
            //AccuDraw.CompassMode currentCompassMode = AccuDraw.ActiveCompassMode;
            //DPoint3d currentorigin = AccuDraw.Origin;
            //double currentangle = AccuDraw.Angle;
            //IndexedViewport currentViePort = AccuDraw.CompassViewport;
            //DVector3d currentDelta = AccuDraw.Delta;

            //HitPath tempHitPath = DoLocate(ev, true, 1);
            //if (tempHitPath != null)
            //{
            //    //捕捉到元素
            //    AccuSnap.SnapEnabled = true;//精确捕捉
            //}
            //else
            //{
            //    AccuSnap.SnapEnabled = false;//精确捕捉

            //    //tempOrigin.Z = 0.0;
            //    //AccuDraw.Origin = tempOrigin;
            //    //AccuDraw.Active = false;
            //}

            //DPoint3d tempOrigin = ev.Point;
            //Bentley.Interop.MicroStationDGN.Point3d origin = new Bentley.Interop.MicroStationDGN.Point3d();
            //origin.X = tempOrigin.X;
            //origin.Y = tempOrigin.Y;
            //origin.Z = 0.0;
            //app.CommandState.AccuDrawHints.SetOrigin(ref origin);
            //bool flagLocate = AccuSnap.LocateEnabled;



            //AccuSnap.LocateEnabled = false;

            //isactive = AccuDraw.Active;
            //currentRotaion = AccuDraw.ActiveRotationMode;
            return base.OnModelMotion(ev);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override StatusInt OnElementModify(Element element)
        {
            
            return StatusInt.Error;
        }
        /// <summary>
        /// 
        /// </summary>
        public static void InstallNewTool()
        {
            JacketedPipeTool jacketedPipeTool = new JacketedPipeTool();
            jacketedPipeTool.InstallTool();
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnPostInstall()
        {
            isClosedFromCode = StatusCloseFormEvent.DEFAULT;
            if (m_jacketedPipeToolForm == null)
            {
                m_jacketedPipeToolForm = new JacketedPipeToolForm();
            }
#if DEBUG
#else
            m_jacketedPipeToolForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JacketedPipeToolForm));
            m_jacketedPipeToolForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            
#endif
            if (m_jacketedPipeToolForm.allPipeLine.Count < 2)
            {
                System.Windows.Forms.MessageBox.Show("由于夹套管要求内外管道管线相异，当前管线不足两条，请添加管线！");
                m_jacketedPipeToolForm = null;
                this.ExitTool();
                return;
            }
            m_jacketedPipeToolForm.Show();
            //从OPM中读预设值
            DlgStandardPreference dlginstance = DlgStandardPreference.GetInstance();
            //管线
            string OPMPipeline = DlgStandardPreference.GetPreferenceValue("LINENUMBER");
            //spec
            string OPMSpecification = DlgStandardPreference.GetPreferenceValue("SPECIFICATION");
            //直径
            string OPMDN = DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER");
            //保护层厚度
            string OPMInsulationThickness = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            //保护层材料
            string OPMInsulation = DlgStandardPreference.GetPreferenceValue("INSULATION");

            //m_jacketedPipeToolForm.comboBox_inner_pipeline.Text = OPMPipeline;
            m_jacketedPipeToolForm.comboBox_inner_spec.Text = OPMSpecification;
            m_jacketedPipeToolForm.comboBox_inner_dn.Text = OPMDN;
            m_jacketedPipeToolForm.textBox_inner_insulation_thickness.Text = OPMInsulationThickness;
            m_jacketedPipeToolForm.comboBox_inner_insulation_material.Text = OPMInsulation;

            //m_jacketedPipeToolForm.comboBox_outer_pipeline.Text = OPMPipeline;
            m_jacketedPipeToolForm.comboBox_outer_spec.Text = OPMSpecification;
            m_jacketedPipeToolForm.comboBox_outer_dn.Text = OPMDN;
            m_jacketedPipeToolForm.textBox_outer_insulation_thickness.Text = OPMInsulationThickness;
            m_jacketedPipeToolForm.comboBox_outer_insulation_material.Text = OPMInsulation;

            init();
            base.BeginPickElements();
            AccuSnap.SnapEnabled = true;//精确捕捉
            AccuDraw.Active = true;

            base.OnPostInstall();
            app.ShowCommand("夹套管布置");
            app.ShowPrompt("请选择第一点");
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnCleanup()
        {
            //selectedPipes.Clear();
            init();
            MyCleanUp();
            base.OnCleanup();

            //if (m_jacketedPipeToolForm == null) return;
            //if (isCloseForm)
            //{
            //    m_jacketedPipeToolForm.Close();
            //    m_jacketedPipeToolForm = null;
            //}

//#if DEBUG
//            m_myForm.Hide();
//#else
//            m_myForm.DetachFromMicroStation();
//#endif
        }

        //public static bool isCloseForm = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnResetButton(DgnButtonEvent ev)
        {

            //isClosedFromCode = StatusCloseFormEvent.RESETBUTTON;
            //MyCleanUp();

            if (pointCount == 0)
            {
                isClosedFromCode = StatusCloseFormEvent.RESETBUTTON;
                MyCleanUp();
                //isCloseForm = true;
                //app.CommandState.StartDefaultCommand();
            }
            else if (pointCount == 1)
            {
                app.ShowPrompt("请选择第一点");
                //isCloseForm = false;
            }
            else
            {
                //isCloseForm = false;
                isClosedFromCode = StatusCloseFormEvent.RESETBUTTON;
                OnRestartTool();
            }
            pointCount = 0;
            return true;
#region MyRegion

            //if (points.Count < 2)
            //{
            //    if (points.Count == 0)
            //    {
            //        app.CommandState.StartDefaultCommand();
            //    }
            //    else
            //    {
            //        points.RemoveAt(points.Count - 1);
            //        app.ShowCommand("请选择第一点");
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < innerPipes.Count - 1; i++)
            //    {
            //        //api.ConnectPorts(innerPipes[i].Ports[1], innerPipes[i + 1].Ports[0]);
            //        //api.ConnectObjectsAtPorts(innerPipes[i].Ports[1], innerPipes[i] ,innerPipes[i + 1].Ports[0], innerPipes[i + 1]);
            //        innerPipes[i].DiscoverConnectionsEx();
            //        api.DoSettingsForFastenerUtility(innerPipes[i], innerPipes[i + 1].Ports[0]);
            //        //api.CreateJointForCompatiblePorts(innerPipes[i].Instance, innerPipes[i + 1].Instance, innerPipes[i].Ports[1].Location);
            //        api.ConnectObjectsAtPorts(innerPipes[i].Ports[1], innerPipes[i], innerPipes[i + 1].Ports[1], innerPipes[i + 1]);
            //    }
            //    for (int i = 0; i < outerPipes.Count - 1; i++)
            //    {
            //        //api.ConnectPorts(outerPipes[i].Ports[1], outerPipes[i + 1].Ports[0]);
            //        outerPipes[i].DiscoverConnectionsEx();
            //        api.DoSettingsForFastenerUtility(outerPipes[i], outerPipes[i + 1].Ports[0]);
            //        api.ConnectObjectsAtPorts(outerPipes[i].Ports[1], outerPipes[i], outerPipes[i + 1].Ports[1], outerPipes[i + 1]);
            //    }

            //    foreach (var item in innerPipes)
            //    {
            //        List<BMECObject> connectedComponents = item.ConnectedComponents;
            //        //item.DiscoverConnectionsEx();
            //        //item.UpdateConnections();
            //    }
            //    foreach (var item in outerPipes)
            //    {
            //        List<BMECObject> connectedComponents = item.ConnectedComponents;
            //    }
            //}
            //points.Clear();//清除此次记录的点
            //app.CommandState.StartDefaultCommand();//切换到默认命令
            //return true;

#endregion
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void OnRestartTool()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //protected override bool NeedAcceptPoint()
        //{
        //    return false;
        //}
        BMECObject innerPipe = null;
        BMECObject outerPipe = null;
        private string pipeECClassName = "PIPE";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            
            nominalDiameter = DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER");
            if (!CutOffPipeForm.isPositiveInteger(m_jacketedPipeToolForm.textBox_outer_insulation_thickness.Text)) return false;
            if (m_jacketedPipeToolForm.comboBox_inner_pipeline.Text == m_jacketedPipeToolForm.comboBox_outer_pipeline.Text)
            {
                System.Windows.Forms.MessageBox.Show("内管与外管的管线编号相同，请重新选择！");
                return false;
            }
            if (Convert.ToDouble(m_jacketedPipeToolForm.comboBox_inner_dn.Text) >= Convert.ToDouble(m_jacketedPipeToolForm.comboBox_outer_dn.Text))
            {
                System.Windows.Forms.MessageBox.Show("内管直径应小于外管");
                return false;
            }
            try
            {
                double innerthick = Convert.ToDouble(m_jacketedPipeToolForm.textBox_inner_insulation_thickness.Text);
                double outerthick = Convert.ToDouble(m_jacketedPipeToolForm.textBox_outer_insulation_thickness.Text);
                if (innerthick < 0.0 || outerthick < 0.0)
                {
                    if (innerthick >= 0.0)
                    {
                        System.Windows.Forms.MessageBox.Show("内管保护层厚度请输入大于零的数");
                    }
                    else if (outerthick >= 0.0)
                    {
                        System.Windows.Forms.MessageBox.Show("外管保护层厚度请输入大于零的数");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("内外管保护层厚度请输入大于零的数");
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                System.Windows.Forms.MessageBox.Show("请输入数字");
                return false;
            }
            if (pointCount == 0)
            {
                startPoint = ev.Point;
                pointCount = 1;
                app.ShowPrompt("请选择第二点");

                IECInstance pipeInstance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);
                pipeInstance["NOMINAL_DIAMETER"].DoubleValue = Convert.ToDouble(m_jacketedPipeToolForm.comboBox_outer_dn.Text);
                ISpecProcessor specProcessor = api.SpecProcessor;
                specProcessor.FillCurrentPreferences(pipeInstance, null);
                ECInstanceList ec_instance_list = specProcessor.SelectSpec(pipeInstance, false);
                if (null != ec_instance_list && ec_instance_list.Count > 0)
                {
                    outerDynamicPipe = ec_instance_list[0];
                    outerDynamicPipe["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(m_jacketedPipeToolForm.textBox_outer_insulation_thickness.Text);
                }
                
                BeginDynamics();
                return true;
            }
            else
            {
                endPoint = ev.Point;
                pointCount = 2;
                innerPipe = createJacketedPipe(startPoint, endPoint, pipeECClassName, Convert.ToDouble(m_jacketedPipeToolForm.comboBox_inner_dn.Text), Convert.ToDouble(m_jacketedPipeToolForm.textBox_inner_insulation_thickness.Text), m_jacketedPipeToolForm.comboBox_inner_insulation_material.Text, m_jacketedPipeToolForm.comboBox_inner_spec.Text, m_jacketedPipeToolForm.comboBox_inner_pipeline.Text);//创建内管道
                outerPipe = createJacketedPipe(startPoint, endPoint, pipeECClassName, Convert.ToDouble(m_jacketedPipeToolForm.comboBox_outer_dn.Text), Convert.ToDouble(m_jacketedPipeToolForm.textBox_outer_insulation_thickness.Text), m_jacketedPipeToolForm.comboBox_outer_insulation_material.Text, m_jacketedPipeToolForm.comboBox_outer_spec.Text, m_jacketedPipeToolForm.comboBox_outer_pipeline.Text);//创建外管道
                if (innerPipe == null || outerPipe == null)
                {
                    if (innerPipe != null)
                    {
                        System.Windows.Forms.MessageBox.Show("外管在所选spec中没有找到对应管径的数据");
                    }
                    else if (outerPipe != null)
                    {
                        System.Windows.Forms.MessageBox.Show("外管在所选spec中没有找到对应管径的数据");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("内管与外管在所选spec中没有找到对应管径的数据");
                    }
                    return false;
                }
                //get pipe info
                //Dictionary<string, Dictionary<string, string>> innerInfoDictionary = setDataToForm(innerPipe.Instance);
                //Dictionary<string, Dictionary<string, string>> outerInfoDictionary = setDataToForm(outerPipe.Instance);
                //fill form
                m_jacketedPipeToolForm.textBox_inner_OD.Text = innerPipe.GetDoubleValueInMM("OUTSIDE_DIAMETER").ToString();
                m_jacketedPipeToolForm.textBox_inner_weight.Text = innerPipe.GetDoubleValueInMM("WEIGHT").ToString();
                m_jacketedPipeToolForm.textBox_inner_shortDesc.Text = innerPipe.GetStringValue("SHORT_DESCRIPTION");
                m_jacketedPipeToolForm.textBox_inner_wallThickness.Text = innerPipe.GetDoubleValueInMM("WALL_THICKNESS").ToString();
                m_jacketedPipeToolForm.textBox_inner_pipeMaterial.Text = innerPipe.GetStringValue("MATERIAL");
                m_jacketedPipeToolForm.textBox_inner_dryWeight.Text = innerPipe.GetDoubleValueInMM("DRY_WEIGHT").ToString();

                m_jacketedPipeToolForm.textBox_outer_OD.Text = outerPipe.GetDoubleValueInMM("OUTSIDE_DIAMETER").ToString();
                m_jacketedPipeToolForm.textBox_outer_weight.Text = outerPipe.GetDoubleValueInMM("WEIGHT").ToString();
                m_jacketedPipeToolForm.textBox_outer_shortDesc.Text = outerPipe.GetStringValue("SHORT_DESCRIPTION");
                m_jacketedPipeToolForm.textBox_outer_wallThickness.Text = outerPipe.GetDoubleValueInMM("WALL_THICKNESS").ToString();
                m_jacketedPipeToolForm.textBox_outer_pipeMaterial.Text = outerPipe.GetStringValue("MATERIAL");
                m_jacketedPipeToolForm.textBox_outer_dryWeight.Text = outerPipe.GetDoubleValueInMM("DRY_WEIGHT").ToString();

                m_jacketedPipeToolForm.textBox_jacketedPipeLength.Text = innerPipe.GetDoubleValueInMM("LENGTH").ToString();

                //update connection
                innerPipe.DiscoverConnectionsEx();
                List<BMECObject> innerConnceted = innerPipe.ConnectedComponents;
                if (innerConnceted != null && innerConnceted.Count != 0)
                {
                    api.DoSettingsForFastenerUtility(innerPipe, innerPipe.Ports[0]);
                }
                startPoint = endPoint;
                //把系统窗口上的管径改回去
                if (nominalDiameter != null && nominalDiameter != "")
                {
                    DlgStandardPreference.SetPreferenceValue("NOMINAL_DIAMETER", nominalDiameter);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("获取系统管径异常");
                }
            }
            return true;
        }
        /// <summary>
        /// 动态显示
        /// </summary>
        /// <param name="ev"></param>
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            #region MyRegion
            //Element element = myDynamicFrameElement(ev);//动态显示的图形元素
            //if (element == null) return;
            //RedrawElems redrawElems = new RedrawElems();
            //redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            //redrawElems.DrawMode = DgnDrawMode.TempDraw;
            //redrawElems.DrawPurpose = DrawPurpose.Dynamics;
            //redrawElems.DoRedraw(element);
            #endregion
            
            if (pointCount == 0) return;
            DPoint3d dynamicPoint = DPoint3d.Zero;
            dynamicPoint = ev.Point;
            BMECObject pipeDynamic = null;
            if (null != outerDynamicPipe)
            {
                pipeDynamic = new BMECObject(outerDynamicPipe);
                pipeDynamic.SetLinearPoints(startPoint, dynamicPoint);//设置管道起点与终点
                pipeDynamic.GetDoubleValueInMM("LENGTH");
                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDynamic);
            }
            base.OnDynamicFrame(ev);
        }
        /// <summary>
        /// TODO 绘制夹套管
        /// </summary>
        private BMECObject createJacketedPipe(DPoint3d startPoint, DPoint3d endPoint, double nd) {
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);//创建一个PIPE的ECInstance
            BMECApi api = BMECApi.Instance;
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = nd;//设置管径

            ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, true);//选择数据
            BMECObject ec_object = new BMECObject();
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                ec_object = new BMECObject(instance);
                ec_object.SetLinearPoints(startPoint, endPoint);//设置管道起点与终点
                ec_object.Create();//将修改应用到程序

                //ec_object.DiscoverConnectionsEx();
                //ec_object.UpdateConnections();
                //List<BMECObject> connectedComponents = ec_object.ConnectedComponents;
            }
            return ec_object;
        }
        /// <summary>
        /// 绘制夹套管
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="pipeECClassName"></param>
        /// <param name="nominalDiamiter"></param>
        /// <param name="insulationThickness"></param>
        /// <param name="insulationMaterial"></param>
        /// <param name="spec"></param>
        /// <returns></returns>
        private BMECObject createJacketedPipe(DPoint3d startPoint, DPoint3d endPoint, string pipeECClassName, double nominalDiamiter, double insulationThickness, string insulationMaterial, string spec, string pipeline) {
            int index = pipelinesName.IndexOf(pipeline);
            StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
            StandardPreferencesUtilities.ChangeSpecification(spec);
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance(pipeECClassName, true);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = Convert.ToDouble(nominalDiamiter);
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, true);
            BMECObject ec_object = null;
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                instance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                //instance["UNIT"].StringValue = "asdf";
                if (insulationMaterial != "")
                {
                    instance["INSULATION"].StringValue = insulationMaterial;
                }
                ec_object = new BMECObject(instance);
                ec_object.SetLinearPoints(startPoint, endPoint);
                ec_object.Create();
            }
            return ec_object;
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
        public static List<string> getInsulationMaterial() {
            List<string> insulationMaterialList = new List<string>();
            IECInstance insulationMaterialInstance = BMECInstanceManager.Instance.CreateECInstance("INSULATION_MATERIAL_VALUE_MAP",true);
            foreach (var item in insulationMaterialInstance)
            {
                insulationMaterialList.Add(item.AccessString);
            }
            return insulationMaterialList;
        }
        //TODO
        public static void getAllProperties() {
            //拿到所有的 properties 并根据每个 properties 的设置显示信息将其对应状态显示到对应的页签中
            //拿到 Category 属性，判断是否需要显示、显示在哪个页签
            //判断是否只读并设置其在 form 中的可编辑状态
            //根据拿到的每条属性信息在对应位置显示

            //也就是目前只需要拿到 Category 这一个 customAttribute 一个自定义属性
            IECInstance instance = BMECInstanceManager.Instance.CreateECInstance("PIPE",true);
            instance["NOMINAL_DIAMETER"].DoubleValue = 100;
            ISpecProcessor specProcessor = BMECApi.Instance.SpecProcessor;
            specProcessor.FillCurrentPreferences(instance, null);
            ECInstanceList ec_instance_list = specProcessor.SelectSpec(instance, false);
            List<IECPropertyValue> properties = new List<IECPropertyValue>();
            if (ec_instance_list.Count > 0)
            {
                instance = ec_instance_list[0];

                foreach (var item in instance)
                {
                    IECPropertyValue property = item;
                    if (item.AccessString == "NOMINAL_DIAMETER")
                    {
                        IECInstance[] customAttributes = property.Property.GetCustomAttributes();
                        for (int i = 0; i < customAttributes.Length; i++)
                        {
                            IECInstance caInstance = customAttributes[i];
                            string CAName = caInstance.ClassDefinition.Name;
                            if (CAName == "Category")
                            {
                                //取 Category的各项属性，Name 决定在哪个区块，priority 决定先后，DisplayLabel 为区块名
                                foreach (var item1 in caInstance)
                                {
                                    IECPropertyValue pv = item1;
                                    string accessingString = pv.AccessString;
                                    if (accessingString == "Name")
                                    {
                                        string categoryName = pv.StringValue;
                                    }
                                    if (accessingString == "Priority")
                                    {
                                        string categoryPriority = pv.StringValue;
                                    }
                                }
                            }
                            if (CAName == "PROPERTY_DISPLAY_STATUS")
                            {
                                bool tempbool = caInstance.IsReadOnly;
                                foreach (var item3 in caInstance)//这个已经是 property 的 customattribute 中的 instance
                                {
                                    string tempstring = item3.AccessString;
                                }
                            }
                        }
                    }
#region MyRegion
                    //获取自定义属性
                    //IECInstance[] customAttributes = property.Property.GetCustomAttributes();
                    //for (int i = 0; i < customAttributes.Length; i++)
                    //{
                    //    IECInstance caInstance = customAttributes[i];
                    //    string CAName = caInstance.ClassDefinition.Name;
                    //    if (CAName == "Category")
                    //    {
                    //        //取 Category的各项属性，Name 决定在哪个区块，priority 决定先后，DisplayLabel 为区块名
                    //        foreach (var item1 in caInstance)
                    //        {
                    //            IECPropertyValue pv = item1;
                    //            string accessingString = pv.AccessString;
                    //            if (accessingString == "Name")
                    //            {
                    //                string categoryName = pv.StringValue;
                    //            }
                    //            if (accessingString == "Priority")
                    //            {
                    //                string categoryPriority = pv.StringValue;
                    //            }
                    //        }
                    //    }
                    //}

                    //IList<IECInstance> primaryAttributes = property.Property.GetLocalCustomAttributes();
                    //for (int i = 0; i < primaryAttributes.Count; i++)
                    //{
                    //    string PAName = primaryAttributes[i].ClassDefinition.Name;
                    //    if (PAName == "")
                    //    {

                    //    }
                    //}
#endregion
                    properties.Add(property);//16 dn
                }
            }
            else
            {
                instance = null;
                //没有对应的数据
            }
        }

        public static IECPropertyValue getPropertyOfECInstanceByName(IECInstance instance, string propertyName) {
            IECPropertyValue result = null;
            foreach (var item in instance)
            {
                if (item.AccessString == propertyName)
                {
                    result = item;
                    break;
                }
            }
            return result;
        }

        public static IECInstance getCustomAttributeByName(IECPropertyValue propertyValue, string customAttributeName) {
            IECInstance result = null;
            IECInstance[] customAttributeInstances = propertyValue.Property.GetCustomAttributes();
            for (int i = 0; i < customAttributeInstances.Length; i++)
            {
                string CAName = customAttributeInstances[i].ClassDefinition.Name;
                if (CAName == customAttributeName)
                {
                    result = customAttributeInstances[i];
                    break;
                }
            }
            return result;
        }

        public void temp(IECInstance pipeInstance) {
            List<IECPropertyValue> properties = new List<IECPropertyValue>();
            //ELEMENTINFO_STATUS 1:read only 2:editable
            //GENERAL_STATUS
            //Category
            foreach (var property in pipeInstance)
            {
                IECInstance customAttribute = getCustomAttributeByName(property, "Category");
                if (customAttribute == null)
                {


                }
            }
        }

        public static Dictionary<string, List<IECPropertyValue>> getDisplayProperty(IECInstance pipeInstance) {
            List<IECPropertyValue> allCategoryProperties = new List<IECPropertyValue>();
            Dictionary<string, List<IECPropertyValue>> groupProperty = new Dictionary<string, List<IECPropertyValue>>();
            foreach (var property in pipeInstance)
            {
                //IECInstance customAttribute = getCustomAttributeByName(property, "Category");
                //if (customAttribute != null)
                //{
                //    allCategoryProperties.Add(property);//has Category Attribute
                //    IECPropertyValue customAttributePropertyValue = getPropertyOfECInstanceByName(customAttribute, "DisplayLabel");
                //    string displayLabel = customAttributePropertyValue.StringValue;
                //    if (groupProperty.ContainsKey(displayLabel))
                //    {
                //        //add property to group
                //        groupProperty[displayLabel].Add(property);
                //    }
                //    else
                //    {
                //        //add key to dictionary
                //        groupProperty.Add(displayLabel, new List<IECPropertyValue>());
                //        groupProperty[displayLabel].Add(property);
                //    }
                //}
            }
            return groupProperty;
        }

        public static Dictionary<string, Dictionary<string, string>> getDisplayDataByProperty(Dictionary<string, List<IECPropertyValue>> groupProperty) {
            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();
            foreach (var group in groupProperty)
            {
                string key = group.Key;
                Dictionary<string, string> value = new Dictionary<string, string>();
                foreach (var property in group.Value)
                {
                    string displayLabel = property.AccessString;
                    string propertyValue = "";
                    if (property.TryGetStringValue(out propertyValue))
                    {
                        //success to get string value
                    }
                    else
                    {
                        //failed to get string value
                    }
                    value.Add(displayLabel, propertyValue);
                }
                result.Add(key, value);
            }
            return result;
        }

        public Dictionary<string, Dictionary<string, string>> setDataToForm(IECInstance instance) {
            //Label Name
            string[] labelName = new string[] { "ComponentName", "DesignValues", "DesignConditions", "OperationConditions", "Info", "Record" };
            //Component Name
            string[] componentName = new string[] { "NAME" };
            //Design Values
            string[] designValues = new string[] {
                "DESIGN_STATE", "INSULATION", "INSULATION_THICKNESS", "STATE", "DESIGN_LENGTH_CENTER_TO_BRANCH_END_EFFECTIVE",
                "DESIGN_LENGTH_CENTER_TO_OUTLET_END_EFFECTIVE", "DESIGN_LENGTH_CENTER_TO_RUN_END_EFFECTIVE", "NOMINAL_DIAMETER", "NOMINAL_DIAMETER_RUN_END", "RATING",
                "SCHEDULE", "SPECIFICATION", "STANDARD"
            };
            //Design Conditions
            string[] designConditions = new string[] { "MODEL", "DESIGNER", "NOMINAL_SIZE" };
            //Operating Conditions
            string[] operationConditions = new string[] {
                "FABRICATION_CATEGORY", "LENGTH", "SHOP_FIELD", "CODE", "WEIGHT",
                "GRADE", "INSIDE_DIAMETER", "OUTSIDE_DIAMETER", "WALL_THICKNESS", "SEQUENCE_NUMBER",
                "LENGTH_EFFECTIVE", "PIPE_FLANGE_TYPE", "NORMAL_OPERATING_PRESSURE", "PIECE_MARK"
            };
            //General Info
            string[] info = new string[] {
                "DEVICE_TYPE_CODE", "ALIAS", "NUMBER", "PAINT_CODE", "CREATE_TIMESTAMP",
                "DESCRIPTION", "DRY_WEIGHT", "MANUFACTURER", "MATERIAL", "ORDER_NUMBER",
                "STOCK_NUMBER", "SUFFIX", "TOTAL_WEIGHT"
            };
            //Record
            string[] record = new string[] { "CreatedBy", "ModifiedBy", "CreateTime", "ModifyTime" };
            //Miscellaneous
            string[] miscellaneous = new string[] {
                "NOTES", "CATALOG_NAME", "EC_CLASS_NAME", "BRANCH_CODE", "COMPONENT_NAME",
                "SPOOL_ID", "LINENUMBER", "OPTION_CODE", "PDX_STATE", "SHORT_DESCRIPTION",
                "PLANT_AREA", "PSDS_STATE", "SERVICE", "SPECID", "UNIT",
                "UNIT_OF_MEASURE", "TRACING", "SPOOL_NUMBER", "PLANT",
                "FUNC_INST_ID", "DETAIL_SKETCH"
            };
            List<string[]> tempList = new List<string[]> { componentName, designValues, designConditions, operationConditions, info, record, miscellaneous};
            int count = 0;
            Dictionary<string, Dictionary<string, string>> group = new Dictionary<string, Dictionary<string, string>>();
            foreach (var item in tempList)
            {
                Dictionary<string, string> property = new Dictionary<string, string>();
                for (int i = 0; i < item.Length; i++)
                {
                    string key = item[i];
                    IECPropertyValue propertyValue = instance.FindPropertyValue(key, false, false, false);
                    string value;
                    propertyValue.TryGetStringValue(out value);
                    if (value == null) continue;
                    try
                    {
                        property.Add(key, value);

                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                try
                {
                    string key = labelName[count++];
                    group.Add(key, property);
                }
                catch (Exception)
                {
                }
            }
            return group;
        }

        public static void MyCleanUp()
        {
            switch (isClosedFromCode)
            {
                case StatusCloseFormEvent.DEFAULT:
                    isClosedFromCode = StatusCloseFormEvent.CLEANUP;
                    if (m_jacketedPipeToolForm != null)
                    {
                        m_jacketedPipeToolForm.Close();
                    }
                    break;
                case StatusCloseFormEvent.FORM:
                    isClosedFromCode = StatusCloseFormEvent.CLEANUP;
                    app.CommandState.StartDefaultCommand();
                    break;
                case StatusCloseFormEvent.RESETBUTTON:
                    isClosedFromCode = StatusCloseFormEvent.CLEANUP;
                    if (m_jacketedPipeToolForm != null)
                    {
                        m_jacketedPipeToolForm.Close();
                        app.CommandState.StartDefaultCommand();
                    }
                    break;
                case StatusCloseFormEvent.CLEANUP:
                    break;
                default:
                    break;
            }
        }

    }
}
