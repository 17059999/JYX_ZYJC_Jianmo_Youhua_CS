using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.MstnPlatformNET;
using Bentley.GeometryNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using System;
using Bentley.Building.Mechanical.Components;
using System.Collections.Generic;
using Bentley.DgnPlatformNET;
using Bentley.Plant.StandardPreferences;
using Bentley.OpenPlantModeler.SDK.Utilities;
using BIM = Bentley.Interop.MicroStationDGN;
using System.Windows.Forms;
//将dll文件放到OpenPlant Modeler CONNECT Edition\MdlSys\Required下可以实现自加载
namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    [Bentley.MstnPlatformNET.AddIn(MdlTaskID = "JYX_ZYJC_Jianmo_Youhua_CS")]
    internal sealed class MyAddin : AddIn
    {
        public static MyAddin s_addin = null;
        private MyAddin(System.IntPtr mdlDesc) : base(mdlDesc)
        {
            s_addin = this;
        }
        protected override int Run(string[] commandLine)
        {
            //Bentley.Building.Mechanical.Api.BMECInstanceManager.SetConfigVariable("OPM_AUTOFITTING_COPY_STANDARD","false");

            BMECApi instance = BMECApi.Instance;
            instance.RegisterTool("MyLinearPlacementTool", new MyLinearPlacementTool(MechAddIn.Instance, 2000000000));
            BMECApi bmecApiInstance = BMECApi.Instance;
            //MyPipeSupportPlacementTool myPipeSupportPlacementTool = new MyPipeSupportPlacementTool(MechAddIn.Instance, 1999999999);
            //MyPipeAnchorPlacementTool myPipeAnchorPlacementTool = new MyPipeAnchorPlacementTool(MechAddIn.Instance, 1999999998);

            JYXInlineplacementTool jyxInlineplacementTool = new JYXInlineplacementTool(MechAddIn.Instance, 1999999999);
            JYXFlowMeterTool flowMeterTool = new JYXFlowMeterTool(MechAddIn.Instance, 1999999998);
            JYXValvePlacementTool valveTool = new JYXValvePlacementTool(MechAddIn.Instance, 1999999997);
            JYXCloupingPlacementTool cloupingTool = new JYXCloupingPlacementTool(MechAddIn.Instance, 1999999996);
            ReliefValveTool reliefvalvetool = new ReliefValveTool(MechAddIn.Instance, 1999999995);
            JYXAnglePlacementTool jyxAnglePlacementTool = new JYXAnglePlacementTool(MechAddIn.Instance, 2000000001);
            //bmecApiInstance.RegisterTool("MyPipeSupportPlacementTool", myPipeSupportPlacementTool);
            //bmecApiInstance.RegisterTool("MyPipeAnchorPlacementTool", myPipeAnchorPlacementTool);

            bmecApiInstance.RegisterTool("JYXInlineplacementTool", jyxInlineplacementTool);

            

            bmecApiInstance.RegisterTool("JYXFlowMeterTool", flowMeterTool);
            bmecApiInstance.RegisterTool("JYXValvePlacementTool", valveTool);
            bmecApiInstance.RegisterTool("JYXCloupingPlacementTool", cloupingTool);
            bmecApiInstance.RegisterTool("ReliefValveTool", reliefvalvetool);
            bmecApiInstance.RegisterTool("JYXAnglePlacementTool", jyxAnglePlacementTool);
            //s_addin = this;

            JYX_ZYJC_CLR.PublicMethod.display_pipe_level_info(Setting_Pipe_Display_Info_Form.xmlReade());

            //s_addin.ElementChangedEvent += S_addin_ElementChangedEvent;

            instance.PreGraphicsInstancePersistedEvent += Instance_PreGraphicsInstancePersistedEvent;

            //instance.SelectSetChangedEvent += Instance_SelectSetChangedEvent;

            //this.SelectionChangedEvent += MyAddin_SelectionChangedEvent;
            //instance.PostGraphicsInstancePersistedEvent += Instance_PostGraphicsInstancePersistedEvent;

            instance.SelectSetChangedEvent += Instance_SelectSetChangedEvent;

            //instance.LocatedObjectEvent += Instance_LocatedObjectEvent;
            //instance.PreNonGraphicsInstancePersistedEvent += Instance_PreNonGraphicsInstancePersistedEvent; ;
            return 0;
        }

        private Dictionary<string, List<ElementId>> pipe_and_dim_element_id_dic = new Dictionary<string, List<ElementId>>();

        private void Instance_SelectSetChangedEvent(object @object, SelectSetChangedEventArgs args)
        {
            if (args.Action == SelectionSetAction.Add)
            {
                if (args.Instance==null)
                {
                    return;
                }
                if (args.Instance.ClassDefinition.Name == "PIPE")
                {
                    string lineNumber1 = args.Instance["LINENUMBER"].StringValue;
                    DlgStandardPreference dlginstance = DlgStandardPreference.GetInstance();
                    dlginstance.SetValueInGrid("PIPING_NETWORK_SYSTEM", lineNumber1);
                    double dn = args.Instance["NOMINAL_DIAMETER"].DoubleValue;
                    StandardPreferencesUtilities.SetNominalDiameter(dn);

                    //double bwchd = instance["INSULATION_THICKNESS"].DoubleValue / 1000;
                    //string bwccz = instance["INSULATION"].StringValue;

                    double bwchd = args.Instance["INSULATION_THICKNESS"].DoubleValue;
                    string bzwccz = args.Instance["INSULATION"].StringValue;

                    DlgStandardPreference.SetPreferenceValue("INSULATION_THICKNESS", bwchd.ToString());
                    DlgStandardPreference.SetPreferenceValue("INSULATION", bzwccz);

                    //foreach(var attach in Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModelRef().GetDgnAttachments())
                    //{
                    //    attach.GetDgnModel().GetNative();
                    //}
                    //BMECObject bmobject = new BMECObject(args.Instance);
                    //DPoint3d dpt1 = bmobject.GetNthPoint(0);
                    //DPoint3d dpt2 = bmobject.GetNthPoint(1);

                    //DimensionElement dim =Mstn_Public_Api.create_dimension_arrow(dpt1, dpt2, 10, Settings.ActiveLevelId, bmobject.Transform3d.Matrix);
                    //pipe_and_dim_element_id_dic.Add(args.Instance.InstanceId,new List<ElementId> { dim.ElementId });
                }
                //修复焊点
                if (ElementClearForm.instance != null && !ElementClearForm.instance.IsDisposed)
                {
                    BMECObject bmobject = new BMECObject(args.Instance);
                    ulong id = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmobject);
                    ElementClearForm.instance.CheckedElement();
                }
            }
            else if (args.Action == SelectionSetAction.Remove)
            {
                //if (args.Instance.ClassDefinition.Name == "PIPE")
                //{
                //    if (pipe_and_dim_element_id_dic.ContainsKey(args.Instance.InstanceId))
                //    {
                //        List<ElementId> dim_element_id_list = pipe_and_dim_element_id_dic[args.Instance.InstanceId];
                //        foreach (ElementId dim_elem_id in dim_element_id_list)
                //        {
                //            Session.Instance.GetActiveDgnModel().FindElementById(dim_elem_id).DeleteFromModel();
                //        }
                //        pipe_and_dim_element_id_dic.Remove(args.Instance.InstanceId);
                //    }
                //}

            }
        }

        private void MyAddin_SelectionChangedEvent(AddIn sender, SelectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == SelectionChangedEventArgs.ActionKind.DoubleClickElement)
            {

                Element element = JYX_ZYJC_CLR.PublicMethod.GetElementFromFilePos(eventArgs.FilePosition, eventArgs.DgnModelRef.GetNative());
                System.Windows.Forms.MessageBox.Show(element.ElementId.ToString());
            }
        }
        private void Instance_PreGraphicsInstancePersistedEvent(object @object, IECInstance instance, InstancePersistedReason reason)
        {
            BMECApi api = (BMECApi)@object;
            //throw new NotImplementedException();
            #region 添加
            if (reason == InstancePersistedReason.Added)
            {
                bool eqb = api.InstanceDefinedAsClass(instance, "EQUIPMENT", true);
                if (eqb)
                {
                    double locationX = instance["CUSTOM_ORIGIN_X"].DoubleValue;
                    double locationY = instance["CUSTOM_ORIGIN_Y"].DoubleValue;
                    double locationZ = instance["CUSTOM_ORIGIN_Z"].DoubleValue;
                    double length = instance["E_LENGTH"].DoubleValue;
                    double width = instance["WIDTH"].DoubleValue;
                    double height = instance["HEIGHT"].DoubleValue;
                    string name = "";
                    name = instance["NAME"].StringValue;
                    if (instance.GetPropertyValue("CERI_Equipment_Num") != null)
                    {
                        instance["CERI_Equipment_Num"].StringValue = name;
                    }
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_X") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_X"].DoubleValue = locationX;
                    }
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_Y") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_Y"].DoubleValue = locationY;
                    }
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_Z") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_Z"].DoubleValue = locationZ;
                    }
                    if (instance.GetPropertyValue("CERI_E_LENGTH") != null)
                    {
                        instance["CERI_E_LENGTH"].DoubleValue = length;
                    }
                    if (instance.GetPropertyValue("CERI_WIDTH") != null)
                    {
                        instance["CERI_WIDTH"].DoubleValue = width;
                    }
                    if (instance.GetPropertyValue("CERI_HEIGHT") != null)
                    {
                        instance["CERI_HEIGHT"].DoubleValue = height;
                    }
                }
                bool b11 = api.InstanceDefinedAsClass(instance, "PIPING_COMPONENT", true); //查找ec的父类是否含有PIPING_COMPONENT
                if (b11)
                {
                    if (instance.GetPropertyValue("SHOP_FIELD") != null)
                    {
                        string cailai = "材料";
                        if (instance.GetPropertyValue("CERI_Classify") != null)
                        {
                            cailai = instance["CERI_Classify"].StringValue;
                        }
                        if (cailai == "cailiao" || cailai == "材料")
                        {
                            instance["SHOP_FIELD"].StringValue = "SHOP";
                        }
                        else
                        {
                            instance["SHOP_FIELD"].StringValue = "FIELD";
                        }
                    }

                    string unit = instance["UNIT"].StringValue;
                    if (instance.GetPropertyValue("CERI_System_Number") != null)
                    {
                        instance["CERI_System_Number"].StringValue = unit;
                    }
                    string service = instance["SERVICE"].StringValue;
                    if (instance.GetPropertyValue("CERI_System_Name") != null)
                    {
                        instance["CERI_System_Name"].StringValue = service;
                    }
                    if (instance.GetPropertyValue("CERI_Media_Name") != null)
                    {
                        instance["CERI_Media_Name"].StringValue = service;
                    }
                    #region
                    //bool bb = BMECApi.Instance.InstanceDefinedAsClass(instance, "REDUCING_PIPE_TEE", true);
                    //bool bb1 = BMECApi.Instance.InstanceDefinedAsClass(instance, "PIPE_REDUCER", true);
                    //if (bb1)
                    //{
                    //    if (instance.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    //    {
                    //        instance["CERI_MAIN_SIZE"].StringValue = instance["NOMINAL_DIAMETER"].DoubleValue + "X" + instance["NOMINAL_DIAMETER_RUN_END"].DoubleValue;
                    //    }
                    //}
                    //if (bb)
                    //{
                    //    if (instance.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    //    {
                    //        instance["CERI_MAIN_SIZE"].StringValue = instance["NOMINAL_DIAMETER"].DoubleValue + "X" + instance["NOMINAL_DIAMETER_RUN_END"].DoubleValue + "X" + instance["NOMINAL_DIAMETER_BRANCH_END"].DoubleValue;
                    //    }
                    //}
                    //if (!bb1 && !bb)
                    //{
                    //    if (instance.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    //    {
                    //        instance["CERI_MAIN_SIZE"].StringValue = Convert.ToString(instance["NOMINAL_DIAMETER"].DoubleValue);
                    //    }
                    //}
                    #endregion

                    string line_number = instance["LINENUMBER"].StringValue;
                    string spec = instance["SPECIFICATION"].StringValue;
                    double length = instance["LENGTH"].DoubleValue / 1000;
                    double od = instance["OUTSIDE_DIAMETER"].DoubleValue / 1000;
                    double wall = instance["WALL_THICKNESS"].DoubleValue / 1000;
                    double bwchd = instance["INSULATION_THICKNESS"].DoubleValue / 1000;
                    string bwccz = instance["INSULATION"].StringValue;
                    Dictionary<string, string> dicList = new Dictionary<string, string>();
                    dicList = UpdateProForm.getInsulationMaterial();
                    string czDis = "";
                    if (dicList.ContainsKey(bwccz)) czDis = dicList[bwccz];
                    //Instance.GetPropertyValue("Origin_X")
                    if (instance.GetPropertyValue("CERI_Pressure_Rating") != null)
                    {
                        instance["CERI_Pressure_Rating"].StringValue = spec;
                    }
                    if (instance.GetPropertyValue("CERI_Line_Number") != null)
                    {
                        instance["CERI_Line_Number"].StringValue = line_number;
                    }
                    if (instance.GetPropertyValue("CERI_specification") != null)
                    {
                        instance["CERI_specification"].StringValue = spec;
                    }
                    if (instance.GetPropertyValue("CERI_LENGTH") != null)
                    {
                        instance["CERI_LENGTH"].DoubleValue = length;
                    }
                    if (instance.GetPropertyValue("CERI_Centerline_Meter") != null)
                    {
                        instance["CERI_Centerline_Meter"].StringValue = length + "m";
                    }
                    if (instance.GetPropertyValue("CERI_Area") != null)
                    {
                        instance["CERI_Area"].DoubleValue = Math.PI * (od + 2.1 * bwchd + 0.0082) * length;
                    }
                    if (instance.GetPropertyValue("CERI_Volume") != null)
                    {
                        instance["CERI_Volume"].DoubleValue = Math.PI * (od + 1.033 * bwchd) * 1.033 * bwchd * length;
                    }
                    if (instance.GetPropertyValue("CERI_Insulation_Thickness") != null)
                    {
                        instance["CERI_Insulation_Thickness"].DoubleValue = bwchd * 1000;
                    }
                    if (instance.GetPropertyValue("CERI_Insulation_Material") != null)
                    {
                        instance["CERI_Insulation_Material"].StringValue = czDis;
                    }
                }

                bool b12 = api.InstanceDefinedAsClass(instance, "SUPPORT_BASE", true); //查找ec的父类是否含有PIPING_COMPONENT
                if (b12)
                {
                    string supportNumber = instance["NUMBER"].StringValue;
                    instance["DESCRIPTION"].StringValue = supportNumber;
                }
            }
            #endregion
            #region 修改
            if (reason == InstancePersistedReason.Updated)
            {
                bool eqb = api.InstanceDefinedAsClass(instance, "EQUIPMENT", true);
                if (eqb)
                {
                    //string name = "";
                    //name = instance["NAME"].StringValue;
                    //string number1 = instance["NUMBER"].StringValue;
                    //instance["NAME"].StringValue = "567-tt";

                    BMECObject bmec = new BMECObject(instance);
                    long elem_id = (long)JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec);

                    Bentley.Interop.MicroStationDGN.Element bim_elem = Mstn_Public_Api.app.ActiveModelReference.GetElementByID(ref elem_id);
                    Bentley.Interop.MicroStationDGN.MsdElementType eletype = bim_elem.Type;


                    double locationX = bim_elem.AsCellElement().Origin.X;
                    double locationY = bim_elem.AsCellElement().Origin.Y;
                    double locationZ = bim_elem.AsCellElement().Origin.Z;
                    double length = (bim_elem.Range.High.X - bim_elem.Range.Low.X) > (bim_elem.Range.High.Y - bim_elem.Range.Low.Y) ? (bim_elem.Range.High.X - bim_elem.Range.Low.X) : (bim_elem.Range.High.Y - bim_elem.Range.Low.Y);
                    double width = (bim_elem.Range.High.X - bim_elem.Range.Low.X) < (bim_elem.Range.High.Y - bim_elem.Range.Low.Y) ? (bim_elem.Range.High.X - bim_elem.Range.Low.X) : (bim_elem.Range.High.Y - bim_elem.Range.Low.Y); ;
                    double height = bim_elem.Range.High.Z - bim_elem.Range.Low.Z;


                    //if (instance.GetPropertyValue("CERI_Equipment_Num") != null)
                    //{
                    //    instance["CERI_Equipment_Num"].StringValue = name;
                    //}
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_X") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_X"].DoubleValue = locationX;
                    }
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_Y") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_Y"].DoubleValue = locationY;
                    }
                    if (instance.GetPropertyValue("CERI_CUSTOM_ORIGIN_Z") != null)
                    {
                        instance["CERI_CUSTOM_ORIGIN_Z"].DoubleValue = locationZ;
                    }
                    if (instance.GetPropertyValue("CERI_E_LENGTH") != null)
                    {
                        instance["CERI_E_LENGTH"].DoubleValue = length;
                    }
                    if (instance.GetPropertyValue("CERI_WIDTH") != null)
                    {
                        instance["CERI_WIDTH"].DoubleValue = width;
                    }
                    if (instance.GetPropertyValue("CERI_HEIGHT") != null)
                    {
                        instance["CERI_HEIGHT"].DoubleValue = height;
                    }

                }
                bool b11 = api.InstanceDefinedAsClass(instance, "PIPING_COMPONENT", true); //查找ec的父类是否含有PIPING_COMPONENT

                bool b111 = api.InstanceDefinedAsClass(instance, "PIPE", true); //查找ec的父类是否含有PIPING_COMPONENT

                if (b11)
                {
                    string comName = instance["COMPONENT_NAME"].StringValue;  //这里重新取值 会让属性重新读值 刷新了管线编号的值
                    if(instance.GetPropertyValue("SHOP_FIELD")!=null)
                    {
                        string cailai = "材料";
                        if(instance.GetPropertyValue("CERI_Classify") != null)
                        {
                            cailai = instance["CERI_Classify"].StringValue;
                        }
                        if(cailai== "cailiao"||cailai== "材料")
                        {
                            instance["SHOP_FIELD"].StringValue = "SHOP";
                        }
                        else
                        {
                            instance["SHOP_FIELD"].StringValue = "FIELD";
                        }
                    }
                    #region
                    Element el = JYX_ZYJC_CLR.PublicMethod.get_element_by_instance(instance);

                    eleList = new List<Element>();

                    Element lastele = getAllChildElement(el);
                    //ConeElement
                    Element coneEle = lastele;

                    double min = double.MaxValue;

                    foreach (Element elech in eleList)
                    {
                        if(elech.ElementType==MSElementType.Cone)
                        {
                            ConeElement cEle = elech as ConeElement;

                            double topR = cEle.TopRadius;

                            if(topR<=min)
                            {
                                coneEle = elech;
                                min = topR;
                            }                           
                        }
                    }

                    //ChildElementEnumerator child_elem_enumerator = new ChildElementEnumerator(el);

                    //while (child_elem_enumerator.MoveNext())
                    //{

                    //}

                    LevelId leId = coneEle.LevelId;

                    //string line_number1 = instance["LINENUMBER"].StringValue;

                    //line_number1 = "2-2-2--mA1-OPM";

                    FileLevelCache level_cache = Session.Instance.GetActiveDgnFile().GetLevelCache();
                    //LevelHandle level_handle = level_cache.GetLevel(leId);
                    LevelHandle level_handle = level_cache.GetLevel(leId);

                    if (level_handle.IsValid)
                    {
                        ElementPropertiesGetter epg = new ElementPropertiesGetter(coneEle);
                        LevelDefinitionColor color = new LevelDefinitionColor(epg.Color, el.DgnModel.GetDgnFile());

                        EditLevelHandle editlevel = level_handle.GetEditHandle();
                        editlevel.SetByLevelColor(color);
                        editlevel.ByLevelWeight = epg.Weight;
                        LevelDefinitionLineStyle ls = new LevelDefinitionLineStyle(0, epg.LineStyle, el.DgnModel.GetDgnFile());
                        editlevel.SetByLevelLineStyle(ls);
                        level_cache.Write();

                    }
                    #endregion
                    //string name11 = instance["NAME"].StringValue;
                    //instance["NAME"].StringValue = instance["SHORT_DESCRIPTION"].StringValue + " " + instance["NOMINAL_DIAMETER"].StringValue + " " + instance["SPECIFICATION"].StringValue + " " + instance["LINENUMBER"].StringValue;
                }

                if (b11)
                {
                    string unit = instance["UNIT"].StringValue;
                    if (instance.GetPropertyValue("CERI_System_Number") != null)
                    {
                        instance["CERI_System_Number"].StringValue = unit;
                    }
                    string service = instance["SERVICE"].StringValue;
                    if (instance.GetPropertyValue("CERI_System_Name") != null)
                    {
                        instance["CERI_System_Name"].StringValue = service;
                    }
                    if (instance.GetPropertyValue("CERI_Media_Name") != null)
                    {
                        instance["CERI_Media_Name"].StringValue = service;
                    }
                    //string line_number = instance["LINENUMBER"].StringValue;
                    //string spec = instance["SPECIFICATION"].StringValue;
                    string line_number = instance["LINENUMBER"].StringValue;
                    string spec = instance["SPECIFICATION"].StringValue;
                    double length = instance["LENGTH"].DoubleValue / 1000;
                    double od1 = instance["OUTSIDE_DIAMETER"].DoubleValue / 1000;
                    double wall1 = instance["WALL_THICKNESS"].DoubleValue / 1000;
                    double bwchd = instance["INSULATION_THICKNESS"].DoubleValue / 1000;
                    string bwccz = instance["INSULATION"].StringValue;

                    Dictionary<string, string> dicList = new Dictionary<string, string>();
                    dicList = UpdateProForm.getInsulationMaterial();
                    string czDis = "";
                    if (dicList.ContainsKey(bwccz)) czDis = dicList[bwccz];

                    if (instance.GetPropertyValue("CERI_Insulation_Thickness") != null)
                    {
                        instance["CERI_Insulation_Thickness"].DoubleValue = bwchd * 1000;
                    }
                    if (instance.GetPropertyValue("CERI_Insulation_Material") != null)
                    {
                        instance["CERI_Insulation_Material"].StringValue = czDis;
                    }
                    //instance["CERI_Line_Number"] = line_number;
                    //instance["CERI_specification"] = spec;
                    if (instance.GetPropertyValue("CERI_Pressure_Rating") != null)
                    {
                        instance["CERI_Pressure_Rating"].StringValue = spec;
                    }
                    if (instance.GetPropertyValue("CERI_Line_Number") != null)
                    {
                        instance["CERI_Line_Number"].StringValue = line_number;
                    }
                    if (instance.GetPropertyValue("CERI_specification") != null)
                    {
                        instance["CERI_specification"].StringValue = spec;
                    }
                    if (instance.GetPropertyValue("CERI_LENGTH") != null)
                    {
                        instance["CERI_LENGTH"].DoubleValue = length;
                    }
                    if (instance.GetPropertyValue("CERI_Centerline_Meter") != null)
                    {
                        instance["CERI_Centerline_Meter"].StringValue = length + "m";
                    }
                    if (instance.GetPropertyValue("CERI_Area") != null)
                    {
                        instance["CERI_Area"].DoubleValue = Math.PI * (od1 + 2.1 * bwchd + 0.0082) * length;
                    }
                    if (instance.GetPropertyValue("CERI_Volume") != null)
                    {
                        instance["CERI_Volume"].DoubleValue = Math.PI * (od1 + 1.033 * bwchd) * 1.033 * bwchd * length;
                    }
                    BMECObject bmec = new BMECObject(instance);

                    List<Port> portList = bmec.Ports;
                    string guige = "";
                    bool isYj = false;
                    guige = "DN" + instance["NOMINAL_DIAMETER"].DoubleValue;
                    if (portList[0].Instance.GetPropertyValue("NOMINAL_DIAMETER") != null)
                    {
                        if (portList.Count > 1)
                        {
                            int portCount = portList.Count;
                            double dn1 = portList[0].Instance["NOMINAL_DIAMETER"].DoubleValue;
                            for (int i = 1; i < portCount; i++)
                            {
                                double dni = portList[i].Instance["NOMINAL_DIAMETER"].DoubleValue;
                                if (dn1 != dni)
                                {
                                    isYj = true;
                                }
                            }
                            if (isYj)
                            {
                                //guige = "DN" + instance["NOMINAL_DIAMETER"].DoubleValue;
                                for (int j = 1; j < portCount; j++)
                                {
                                    double dnj = portList[j].Instance["NOMINAL_DIAMETER"].DoubleValue;
                                    guige = guige + "×" + "DN" + dnj;
                                }
                            }
                        }
                    }
                    if (instance.GetPropertyValue("CERI_MAIN_SIZE") != null)
                    {
                        instance["CERI_MAIN_SIZE"].StringValue = guige;
                    }
                    //DPoint3d dp=bmec.GetNthPort(0).LocationInUors;
                    //DPoint3d dp1 = bmec.GetNthPort(1).LocationInUors;
                    //DPoint3d dp1 = new DPoint3d();

                    //bmec.Refresh();
                    IECPropertyValue iECPropertyValue = instance.FindPropertyValue("TRANSFORMATION_MATRIX", true, true, false);
                    DPoint3d dpoint = new DPoint3d();
                    dpoint = bmec.Transform3d.Translation;
                    double x = dpoint.X / 1000;
                    string orgin_x = string.Format("{0:F}", x);

                    double y = dpoint.Y / 1000;
                    double z = dpoint.Z / 1000;
                    string orgin_z = string.Format("{0:F}", z);
                    string orgin_y = string.Format("{0:F}", y);
                    if (bmec.Instance.GetPropertyValue("Origin_X") != null)
                    {
                        bmec.Instance["Origin_X"].StringValue = orgin_x + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_Coordinate_X") != null)
                    {
                        bmec.Instance["D_Coordinate_X"].StringValue = orgin_x + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_zuobiaoX") != null)
                    {
                        bmec.Instance["D_zuobiaoX"].StringValue = orgin_x + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_LocationX") != null)
                    {
                        bmec.Instance["CERI_LocationX"].StringValue = orgin_x + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("Origin_Y") != null)
                    {
                        bmec.Instance["Origin_Y"].StringValue = orgin_y + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_Coordinate_Y") != null)
                    {
                        bmec.Instance["D_Coordinate_Y"].StringValue = orgin_y + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_zuobiaoY") != null)
                    {
                        bmec.Instance["D_zuobiaoY"].StringValue = orgin_y + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_LocationY") != null)
                    {
                        bmec.Instance["CERI_LocationY"].StringValue = orgin_y + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("Origin_Z") != null)
                    {
                        bmec.Instance["Origin_Z"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_Coordinate_Z") != null)
                    {
                        bmec.Instance["D_Coordinate_Z"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_zuobiaoZ") != null)
                    {
                        bmec.Instance["D_zuobiaoZ"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_LocationZ") != null)
                    {
                        bmec.Instance["CERI_LocationZ"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("zhongxinbiaogao") != null)
                    {
                        bmec.Instance["zhongxinbiaogao"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_Valve_Center_Elevation") != null)
                    {
                        bmec.Instance["D_Valve_Center_Elevation"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("D_guanzhongxinbiaogao") != null)
                    {
                        bmec.Instance["D_guanzhongxinbiaogao"].StringValue = orgin_z + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_Center_Elevation") != null)
                    {
                        bmec.Instance["CERI_Center_Elevation"].StringValue = orgin_z + "mm";
                    }
                    double od = bmec.Instance["OUTSIDE_DIAMETER"].DoubleValue;
                    double wall = bmec.Instance["WALL_THICKNESS"].DoubleValue;
                    double orginz = Convert.ToDouble(orgin_z);
                    //double topbiaogao = orgin_z + od + wall;
                    if (bmec.Instance.GetPropertyValue("CERI_Pipe_top_elevation") != null)
                    {
                        bmec.Instance["CERI_Pipe_top_elevation"].StringValue = (orginz + od + wall) + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_Topest_Center_Elevation_Pipe") != null)
                    {
                        bmec.Instance["CERI_Topest_Center_Elevation_Pipe"].StringValue = (orginz + od + wall) / 1000 + "m";
                    }
                    //bmec.Instance["D_guandingwaibiaogao"].StringValue = (orginz + od + wall) + "mm";
                    if (bmec.Instance.GetPropertyValue("CERI_Pipe_low_outer_elevation") != null)
                    {
                        bmec.Instance["CERI_Pipe_low_outer_elevation"].StringValue = (orginz - od - wall) + "mm";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_Lowest_Center_Elevation_Pipe") != null)
                    {
                        bmec.Instance["CERI_Lowest_Center_Elevation_Pipe"].StringValue = (orginz - od - wall) / 1000 + "m";
                    }
                    if (bmec.Instance.GetPropertyValue("CERI_Pipe_low_internal_elevation") != null)
                    {
                        bmec.Instance["CERI_Pipe_low_internal_elevation"].StringValue = (orginz - od) + "mm";
                    }
                    try
                    {
                        DPoint3d[] pointshuz = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec);
                        DVector3d dv_x = new DVector3d(1, 0, 0);
                        DVector3d dv_pipe = new DVector3d(pointshuz[0], pointshuz[1]);
                        double angle = dv_pipe.AngleTo(dv_x).Degrees;
                        string podu = string.Empty;
                        if (pointshuz[0].Z == pointshuz[1].Z)
                        {
                            podu = "0°";
                            angle = 0;
                        }
                        else
                        {
                            DPoint3d touy = new DPoint3d(pointshuz[1].X, pointshuz[1].Y, pointshuz[0].Z);
                            DVector3d touyin = new DVector3d(pointshuz[0], touy);
                            double aa = dv_pipe.AngleTo(touyin).Degrees;
                            bool b = dv_pipe.IsPerpendicularTo(touyin);
                            if (aa>89.9&&aa<90.1)
                            {
                                b = true;
                            }
                            else
                            {
                                b = false;
                            }
                            if(touyin.Magnitude<0.001)
                            {
                                b = true;
                            }
                            
                            if (b)
                            {
                                podu = "90°";
                                angle = 90;
                            }
                            else
                            {
                                double ra = dv_pipe.AngleTo(touyin).Radians;
                                angle = dv_pipe.AngleTo(touyin).Degrees;
                                double tan = Math.Tan(ra);
                                podu = (tan * 100) + "%";
                            }
                        }
                        if (bmec.Instance.GetPropertyValue("D_jiaodu") != null)
                        {
                            bmec.Instance["D_jiaodu"].StringValue = angle + "°";
                        }
                        if (bmec.Instance.GetPropertyValue("CERI_Angle") != null)
                        {
                            bmec.Instance["CERI_Angle"].StringValue = angle + "°";
                        }
                        if (bmec.Instance.GetPropertyValue("D_podu") != null)
                        {
                            bmec.Instance["D_podu"].StringValue = podu;
                        }
                        if (bmec.Instance.GetPropertyValue("CERI_Gradient") != null)
                        {
                            bmec.Instance["CERI_Gradient"].StringValue = podu;
                        }
                    }
                    catch
                    {

                    }
                }
                bool isnozzle= api.InstanceDefinedAsClass(instance, "NOZZLE", true); //查找ec的父类是否含有PIPING_COMPONENT
                if (isnozzle)
                {
                    //instance["NUMBER"].StringValue = "999";
                    string name = instance["NAME"].StringValue;
                    string tag = instance["EQUIPMENT_TAG"].StringValue;
                    IECRelationshipInstanceCollection iecReList = instance.GetRelationshipInstances();
                    foreach (var a in iecReList)
                    {
                        IECInstance souInstance = a.Source;
                        IECInstance tarInstance = a.Target;
                        string name0 = souInstance["NAME"].StringValue;
                    }
                }

                bool b12 = api.InstanceDefinedAsClass(instance, "SUPPORT_BASE", true); //查找ec的父类是否含有PIPING_COMPONENT
                if (b12)
                {
                    string supportNumber = instance["NUMBER"].StringValue;
                    instance["DESCRIPTION"].StringValue = supportNumber;
                }
                //bmec.Create();
            }
            #endregion
        }

        List<Element> eleList = new List<Element>();
        public  Element getAllChildElement(Element ele)
        {
            Element ee = ele;

            ChildElementEnumerator child_elem_enumerator = new ChildElementEnumerator(ele);

            while (child_elem_enumerator.MoveNext())
            {
                ee = getAllChildElement(child_elem_enumerator.Current);
            }

            eleList.Add(ee);

            return ee;
        }
    }
}
