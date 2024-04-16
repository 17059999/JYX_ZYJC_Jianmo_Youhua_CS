using Bentley.GeometryNET;              //几何绘图相关对象
using Bentley.MstnPlatformNET;          //MicroStation相关对象
using System.Collections.Generic;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.Building.Mechanical.Components;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.ECObjects.Instance;
using Bentley.EC.Persistence;
using Bentley.ECObjects.Schema;
using Bentley.Plant.Utilities;
using Bentley.Plant.StandardPreferences;
using System;
using Bentley.DgnPlatformNET;
using Bentley.Internal.MstnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using System.Reflection;
using OPMCommon;
using Bentley.Building.Mechanical;
using Bentley.DgnPlatformNET.DgnEC;
using System.CodeDom.Compiler;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    //京城验证方式
    class CommandsList
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        static BMApplication bm_app = new BMApplication();
        static Session session = Session.Instance;
        
        static BMECApi api = BMECApi.Instance;
        public static bool createSourcetHasTargetRelationship(ChangeSet changeSet, IECInstance source, IECInstance target, string relationshipName)
        {
            if (null == changeSet || null == source || null == target)
            {
                return false;
            }
            BMECApi arg_1A_0 = BMECApi.Instance;
            IECSchema schema = BMECInstanceManager.Instance.Schema;
            if (null == schema)
            {
                return false;
            }
            IECRelationshipInstance iECRelationshipInstance = null;
            IECRelationshipClass[] relationshipClasses = schema.GetRelationshipClasses();
            int num = 0;
            if (0 < relationshipClasses.Length)
            {
                IECRelationshipClass iECRelationshipClass;
                do
                {
                    iECRelationshipClass = relationshipClasses[num];
                    if (iECRelationshipClass.Name.Equals(relationshipName))
                    {
                        goto IL_5C;
                    }
                    num++;
                }
                while (num < relationshipClasses.Length);
                goto IL_68;
                IL_5C:
                iECRelationshipInstance = (iECRelationshipClass.CreateInstance() as IECRelationshipInstance);
            }
            IL_68:
            if (null != iECRelationshipInstance)
            {
                iECRelationshipInstance.Target = target;
                iECRelationshipInstance.Source = source;
                changeSet.Add(iECRelationshipInstance);
                changeSet.MarkNew(iECRelationshipInstance);
                return true;
            }
            return false;
        }

        public static bool AddxxxToComponent(IECInstance component, IECInstance attachment)
        {
            ChangeSet changeSet = new ChangeSet();
            BMECApi instance = BMECApi.Instance;
            bool flag;
            //if (!(attachment.ClassDefinition.Name == "SUPPORT_LOCATION") && !instance.InstanceDefinedAsClass(attachment, "SUPPORT", true))
            //{
            //    if (instance.InstanceDefinedAsClass(component, "EQUIPMENT", true))
            //    {
            //        flag = api.createSourcetHasTargetRelationship(changeSet, component, attachment, "DEVICE_HAS_NOTE");
            //    }
            //    else
            //    {
            //        flag = < Module >.Bentley.OpenPlant.Modeler.Api.createSourcetHasTargetRelationship(changeSet, component, attachment, "PIPING_COMPONENT_HAS_NOTE");
            //    }
            //}
            //else
            //{
            flag = createSourcetHasTargetRelationship(changeSet, component, attachment, "test");
            //}
            if (flag)
            {
                if (null == PersistenceManager.GetInstance())
                {
                    PersistenceManager.Initialize(DgnUtilities.GetInstance().GetDGNConnectionForPipelineManager());
                }
                PersistenceManager.GetInstance().CommitChangeSet(changeSet);
            }
            if (changeSet != null)
            {
                changeSet.Dispose();
            }
            return flag;
        }
        public static void test(string unparsed)//画线
        {
            //Bentley.DgnPlatformNET.ElementAgenda elementAgenda = new Bentley.DgnPlatformNET.ElementAgenda();
            //Bentley.DgnPlatformNET.SelectionSetManager.BuildAgenda(ref elementAgenda); //获取选中的元素
            //uint selectedElemCount = elementAgenda.GetCount();
            //if (selectedElemCount == 0)
            //{
            //    return;
            //}
            //for (uint i=0;i< elementAgenda.GetCount();i++)
            //{
            //    elementAgenda.GetEntry(i).DeleteFromModel();;
            //}

            ////=:56FF00000001:1567431E00;
            //ECInstanceList ecListAll = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetAllInstancesFromDgn();
            //ECInstanceList ecSx = new ECInstanceList();
            //foreach (IECInstance ecIn in ecListAll)
            //{

            //    bool b = BMECApi.Instance.InstanceDefinedAsClass(ecIn, "PIPE", true); //查找ec的父类是否含有PIPING_COMPONENT
            //    if (b)
            //    {
            //        string id = ecIn.InstanceId;
            //        double length = ecIn["LENGTH"].DoubleValue;
            //        if (length < 1)
            //        {
            //            SelectionSetManager.AddElement((ecIn as IDgnECInstance).Element, Session.Instance.GetActiveDgnModel());

            //        }
            //    }
            //}

            //ElementId id = new ElementId(ref idd);
            //BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
            //ECInstanceList joint_instances = BMECApi.Instance.GetRelatedJointsForComponent(bmec_object.Instance);
            //foreach (IECInstance joint_instance in joint_instances)
            //{

            //    ECInstanceList iess = BMECApi.Instance.GetSealAndFastenersForJoint(joint_instance);
            //    ElementId ids = (iess[0] as IDgnECInstance).Element.ElementId;

            //}

            //ECInstanceList instance_list = api.GetSelectedInstances();
            //BMECObject bm, bm1;
            //bm = bm1 = null;
            //int i = 0;
            //BMECObject bm2 = null;
            //foreach (IECInstance ins in instance_list)
            //{
            //    if (i == 0)
            //    {
            //        bm = new BMECObject(ins);
            //    }
            //    else if (i == 1)
            //    {
            //        bm1 = new BMECObject(ins);
            //    }
            //    else
            //    {
            //        bm2 = new BMECObject(ins);
            //    }

            //    i++;
            //}
            ////bm.DiscoverConnectionsEx();
            ////bm.UpdateConnections();

            ////return;
            //////ulong id = 34817;
            //////BMECObject bm = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
            //////ulong id2 = 79204;
            //////BMECObject bm2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id2);
            //////bm2.Instance.GetRelationshipInstances();
            //bm.DiscoverConnectionsEx();
            ////api.DoSettingsForFastenerUtility(bm1, bm1.Ports[0]);
            //api.DoSettingsForFastenerUtility(bm, bm.Ports[0]);
            //api.ConnectObjectsAtPorts(bm.Ports[0], bm, bm1.Ports[0], bm1);

            //api.DoSettingsForFastenerUtility(bm1, bm1.Ports[1]);
            //api.ConnectObjectsAtPorts(bm1.Ports[1], bm1, bm2.Ports[1], bm2);
            //api.DoSettingsForFastenerUtility(bm1, bm1.Ports[1]);
            //api.ConnectObjectsAtPorts(bm1.Ports[1], bm1, bm.Ports[0], bm);
            ////MyThread my = new MyThread(app.Point3dFromXYZ(0,0,0),app.Point3dFromXYZ(0,0,1000), @"D:\OpenPlant3D.dgn");
            ////System.Threading.Thread thread = new System.Threading.Thread(my.CreateLine);

            ////thread.Start();

            ////while (!f.IsAlive) ;
            ////dgnfile.Close();
            //string filename =@"D:\OpenPlant3D.dgn";
            //BIM.Point3d pt1, pt2;
            //pt1 = app.Point3dFromXYZ(0,0,0);
            //pt2 = app.Point3dFromXYZ(2000,0,0);
            //BIM.DesignFile dgnfile = app.OpenDesignFileForProgram(filename, false);

            //BIM.LineElement line = app.CreateLineElement2(null, pt1, pt2);
            //dgnfile.DefaultModelReference.CopyElement(line);
            //dgnfile.Save();
            //Bentley.DgnPlatformNET.DgnDocument document = Bentley.DgnPlatformNET.DgnDocument.CreateForLocalFile(filename);
            //if (document != null)
            //{
            //    Bentley.DgnPlatformNET.DgnFileOwner file = Bentley.DgnPlatformNET.DgnFile.Create(document, Bentley.DgnPlatformNET.DgnFileOpenMode.ReadWrite);

            //    Bentley.DgnPlatformNET.StatusInt ss;
            //    file.DgnFile.LoadDgnFile(out ss);
            //    file.DgnFile.FillDictionaryModel();
            //    Bentley.DgnPlatformNET.DgnModel modle = file.DgnFile.LoadRootModelById(out ss, file.DgnFile.DefaultModelId);
            //    file.DgnFile.FillSectionsInModel(modle, Bentley.DgnPlatformNET.DgnModelSections.All);
            //    DSegment3d seg = new DSegment3d(0, 0, 0, 0, 0, 10000);
            //    CurvePrimitive line = LineSegment.CreateLine(seg);
            //    Bentley.DgnPlatformNET.Elements.Element elem = Bentley.DgnPlatformNET.Elements.DraftingElementSchema.ToElement(modle, line, null);
            //    elem.AddToModel();
            //    file.DgnFile.ProcessChanges(Bentley.DgnPlatformNET.DgnSaveReason.FileClose);

            //}
            #region 创建管道
            //BMECObject obj =OPM_Public_Api.create_pipe(DPoint3d.FromXYZ(10000,0,0),DPoint3d.FromXYZ(20000,0,0),100);
            //obj.Create();
            //DPoint3d[] dpts =JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(obj);
            //DPoint3d p1, p2;
            //p1 = p2 = DPoint3d.Zero;
            //JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(obj, ref p1, ref p2);
            //DPoint3d dpt0 = obj.Ports[0].LocationInUors;
            //DPoint3d dpt1=obj.Ports[1].LocationInMM;
            //dpt0 = obj.GetNthPoint(0);
            // dpt1 = obj.GetNthPoint(1);
            #endregion

            #region 类似管子模型
            //DPoint3d dp_start = DPoint3d.FromXYZ(0, 0, 0);
            //DPoint3d dp_end = DPoint3d.FromXYZ(0, 0, 5000);
            //double length =dp_start.Distance(dp_end);
            //DMatrix3d dma = DMatrix3d.Identity;

            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(DVector3d.FromXYZ(1, 0, 0), dp_end - dp_start, ref dma);
            //DTransform3d dtran = DTransform3d.FromMatrixAndTranslation(dma, dp_start);
            //ElementHolder element_holder = OPM_Public_Api.create_rectangular_surface(1000,2000, length, DPoint3d.FromXYZ(0,0,0),dtran);

            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
            #endregion

            #region 绑定管道
            //ulong id = 734000;
            //BMECObject bm = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);


            ////Bentley.ECObjects.Instance.IECInstance iECInstance = api.FindAllInformationOnInstance(bm.Instance);

            //ulong id1 = 733094;
            //BMECObject bm1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id1);
            //AddxxxToComponent(bm1.Instance, bm.Instance);
            #endregion

            #region 绕固定点旋转
            //ulong id = 33794;
            //BMECObject bm = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
            //DMatrix3d dma = DMatrix3d.Identity;
            //DMatrix3d dma1 = bm.Transform3d.Matrix;

            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(DVector3d.FromXYZ(1, 0, 0), DVector3d.FromXYZ(0, 1, 0), ref dma);

            ////bm.Transform3d = DTransform3d.FromMatrixAndFixedPoint(DMatrix3d.Multiply(dma,dma1), DPoint3d.FromXYZ(1000, 0, 0));
            //DTransform3d tran = DTransform3d.FromMatrixAndFixedPoint(dma, DPoint3d.FromXYZ(1000000, 0, 0));
            //DPoint3d new_origin = DPoint3d.Zero;
            //tran.Multiply(out new_origin, bm.Transform3d.Translation);
            //bm.Transform3d = DTransform3d.FromMatrixAndTranslation(DMatrix3d.Multiply(bm.Transform3d.Matrix, tran.Matrix), new_origin);

            //bm.UpdateConnections();
            //bm.Create();
            #endregion

            #region 搜索元素
            //Session.Instance.GetActiveDgnModel().GetGraphicElements();
            //ScanCriteria dd = new ScanCriteria();

            #endregion

            #region 反射
            //string content = string str = BMECInstanceManager.FindConfigVariableName("OPM_DIR_WORKSPACE_CELLS"); ("OPM_DIR_ASSEMBLIES");
            //Assembly dll = Assembly.LoadFile(content + @"MechAddin.dll");
            //Type linear_container_view_type = dll.GetType("Bentley.Building.Mechanical.LinearContainerView", true);

            //Type[] params_type = new Type[0];
            //ParameterModifier[] parameter_modifier = new ParameterModifier[0];
            //BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            //MethodInfo method = linear_container_view_type.GetMethod("AddAutoFittingGroupPanel", flag, null, params_type, parameter_modifier);
            //object value = method.Invoke(this, null);

            //Assembly assembly = Assembly.LoadFrom(@"..\..\..\TicketLibrary\bin\Debug\TicketLibrary.dll");
            //object obj = assembly.CreateInstance("ReflectTest.TicketInfo");
            //Type type = obj.GetType();
            //FieldInfo fieldInfo = type.GetField("ticketList", BindingFlags.NonPublic | BindingFlags.Static);
            //object value = fieldInfo.GetValue(null);
            //Console.WriteLine(value.ToString());
            //Console.WriteLine((value as List<String>).Count);
            //foreach (String a in (value as List<String>))
            //{
            //    Console.Write(a + " ");
            //}
            //Console.WriteLine();



            //MethodInfo method = type.GetMethod("GetAge", BindingFlags.NonPublic | BindingFlags.Instance);
            //var methodValue = method.Invoke(obj, null);
            //Console.WriteLine(methodValue.ToString());

            //object[] customAtt = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            //foreach(var customAttribute in customAtt)
            //{
            //    Console.WriteLine(((AssemblyDescriptionAttribute)customAttribute).Description);
            //}
            #endregion

            #region 旋转
            //ulong id = 186344;
            //ElementId element_id = new ElementId(ref id);
            //BMECObject bmecobject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element_id);

            ////JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(bmecobject, new DVector3d(0, -1, 0), new DVector3d(0,1,0));
            //bmecobject.GetOrigin()
            //bmecobject.Create();
            #endregion

            #region 获取单元名和单元库名
            //BMECInstanceManager arg_21_0 = BMECApi.Instance.InstanceManager;
            //string name = this.PropertyContainer.Instance.ClassDefinition.Name;
            //IECInstance customAttributes = arg_21_0.Schema[name].GetCustomAttributes("CREATION_ATTRIBUTE");
            //if (customAttributes != null && customAttributes.GetPropertyValue("DIAGRAM_CELL_NAME") != null && customAttributes.GetPropertyValue("CELL_LIBRARY") != null)
            //{
            //    string text = customAttributes["DIAGRAM_CELL_NAME"].StringValue.Trim();
            //    string text2 = customAttributes["CELL_LIBRARY"].StringValue.Trim();
            //    if (!(text == string.Empty) && !(text2 == string.Empty))
            //    {
            //        string str = BMECInstanceManager.FindConfigVariableName("OPM_DIR_WORKSPACE_CELLS");
            //        this.DisplayCell(str + "\\" + text2, text);
            //    }
            //}

            //ElementAgenda ae = new ElementAgenda();

            //SelectionSetManager.BuildAgenda(ref ae);
            //Element elem = ae.GetFirst();
            //BMECObject bm_object =JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
            //ElementPreview ff = new ElementPreview();
            //ff.Dock = System.Windows.Forms.DockStyle.Fill;
            //ff.BackColor = Color.White;
            //base.PreviewControl.ToolStripContainer.ContentPanel.Controls.Add(ff);

            //DisplayableElement m_elem = (CellHeaderElement)elem;

            //ViewFlags myViewFlags = ViewInformation.GetDefaultFlags();

            //DRange3d elemRng;

            //m_elem.CalcElementRange(out elemRng);
            //DPoint3d myOrg = elemRng.Low;
            //DVector3d myRng = DPoint3d.Subtract(elemRng.High, elemRng.Low);
            //Rectangle myRect = new Rectangle(ff.Location, ff.Size);

            //ff.DisplayElemHandle(MyPublic_Api.BytesToIntptr(m_elem.ElementHandle), myViewFlags, myRect,null,elemRng.Low,DPoint3d.Subtract(elemRng.High,elemRng.Low));
            #endregion

            #region 椭圆
            //ElementHolder elem_holder =JYX_ZYJC_CLR.PublicMethod.create_elliptic_cylinder(100000, 200000, 100000,DPoint3d.Zero,DPoint3d.FromXYZ(0,0,1));
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem_holder);
            //ElementHolder elem_holder1 = JYX_ZYJC_CLR.PublicMethod.create_elliptic_cylinder_solid(100000,200000,150000,DPoint3d.Zero,DPoint3d.FromXYZ(0,0,1));
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem_holder1);
            #endregion

            #region 测试取ABD墙体表面
            //ulong id = 156183;
            //ElementId element_id = new ElementId(ref id);
            //LineElement line =(LineElement)Session.Instance.GetActiveDgnModel().FindElementById(element_id);
            //DPoint3d dpt = DPoint3d.FromXYZ(105555710, 2860190, 750000);
            //line.GetCurveVector().GetStartPoint(out dpt);
            //ElementAgenda ae=new ElementAgenda();

            //SelectionSetManager.BuildAgenda(ref ae);
            //Element elem = ae.GetFirst();

            //ShapeElement target_shape=null;
            //ComplexShapeElement target_complex_shape = null;
            //if (elem!=null)
            //{
            //    if (elem.ElementType == MSElementType.CellHeader)
            //    {
            //        CellHeaderElement cellheaderelement = (CellHeaderElement)elem;
            //        ChildElementCollection cec = cellheaderelement.GetChildren();


            //        foreach(Element child_elem in cec)
            //        {
            //            if (child_elem == null)
            //            {

            //            }
            //            else if(child_elem.ElementType==MSElementType.CellHeader)
            //            {
            //                CellHeaderElement child_cellheaderelement = (CellHeaderElement)child_elem;
            //                ChildElementCollection child_cec = child_cellheaderelement.GetChildren();

            //                foreach (Element child_child_elem in child_cec)
            //                {
            //                    if (child_child_elem.ElementType==MSElementType.Shape)
            //                    {
            //                        ShapeElement shape = (ShapeElement)child_child_elem;
            //                        CurveVector.InOutClassification inoutclassification = shape.GetCurveVector().PointInOnOutXY(dpt);
            //                        if(inoutclassification== CurveVector.InOutClassification.In)
            //                        {
            //                            target_shape = shape;
            //                            break;
            //                        }else if (inoutclassification == CurveVector.InOutClassification.On)
            //                        {
            //                            if (target_shape == null)
            //                            {
            //                                target_shape = shape;
            //                            }
            //                            else
            //                            {
            //                                double area1, area2;
            //                                DPoint3d cent1, cent2;
            //                                DVector3d nor1, nor2;
            //                                target_shape.GetCurveVector().CentroidNormalArea(out cent1, out nor1, out area1);
            //                                shape.GetCurveVector().CentroidNormalArea(out cent2, out nor2, out area2);
            //                                if(area2> area1)
            //                                {
            //                                    target_shape = shape;
            //                                }
            //                            }
            //                        }

            //                    }
            //                    else if (child_child_elem.ElementType == MSElementType.ComplexShape)
            //                    {
            //                        ComplexShapeElement shape = (ComplexShapeElement)child_child_elem;
            //                        CurveVector.InOutClassification inoutclassification = shape.GetCurveVector().PointInOnOutXY(dpt);

            //                        if (inoutclassification == CurveVector.InOutClassification.In)
            //                        {
            //                            target_complex_shape = shape;
            //                            break;
            //                        }
            //                        else if (inoutclassification == CurveVector.InOutClassification.On)
            //                        {
            //                            if (target_complex_shape == null)
            //                            {
            //                                target_complex_shape = shape;
            //                            }
            //                            else
            //                            {
            //                                double area1, area2;
            //                                DPoint3d cent1, cent2;
            //                                DVector3d nor1, nor2;
            //                                target_complex_shape.GetCurveVector().CentroidNormalArea(out cent1, out nor1, out area1);
            //                                shape.GetCurveVector().CentroidNormalArea(out cent2, out nor2, out area2);
            //                                if (area2 > area1)
            //                                {
            //                                    target_complex_shape = shape;
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {

            //    }
            //}
            #endregion

            #region 复制管道测试
            //ulong id = 72949;
            //BMECObject pipe = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id);
            //BMECObject new_pipe1=new BMECObject();
            //new_pipe1.Copy(pipe);
            //new_pipe1.Create();
            #endregion

            #region 管道三维模型
            //ElementHolder element_holder = OPM_Public_Api.create_cone_surface(100, app.Point3dFromXYZ(0,0,0), app.Point3dFromXYZ(1000, 0, 0));
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
            #endregion

            #region 获取设置的保温层值等
            //string ss =DlgStandardPreference.GetPreferenceValue("INSULATION");
            //ss = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            //ss = DlgStandardPreference.GetPreferenceValue("UNIT");
            //ss = DlgStandardPreference.GetPreferenceValue("LINENUMBER");
            //ss = DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER");
            //ss = DlgStandardPreference.GetPreferenceValue("COMPONENT");
            #endregion

            #region C型钢回切
            //double length = 1000 * uor_per_master;
            //double diameter = 20 * uor_per_master;
            //double depth = 15 * uor_per_master;
            //DVector3d direction = DVector3d.FromXYZ(1,0,0);
            //double thickness = 5 * uor_per_master;
            //DPoint3d location = DPoint3d.FromXYZ(0, 0, 0);
            //Channel channel = new Channel(diameter, diameter, depth, length, DPoint3d.FromXYZ(0,0,0), location, direction, thickness);
            //List<ElementHolder> element_holder_list =channel.Create3DElements();
            //foreach(ElementHolder element_holder in element_holder_list)
            //{
            //    JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
            //}

            //DPoint3d path_start, path_end;
            //path_start = DPoint3d.Zero;
            //path_end = DPoint3d.FromXYZ(0,0, length);
            //DPoint3d[] pts = new DPoint3d[4];
            //pts[0] = DPoint3d.FromXYZ(-(depth)/2,-(diameter+2* thickness)/2,0);
            //pts[1] = DPoint3d.FromXYZ(-(depth) / 2, (diameter + 2 * thickness) / 2, 0);
            //pts[2] = DPoint3d.FromXYZ((depth + thickness*2) / 2, (diameter + 2 * thickness) / 2, 0);
            //pts[3] = DPoint3d.FromXYZ((depth + thickness*2) / 2, -(diameter + 2 * thickness) / 2, 0);
            //ElementHolder profile = bm_app.CreateShapeElement(pts, 1);
            //ElementHolder path = bm_app.CreateLineElement(path_start, path_end);

            //DMatrix3d ma=DMatrix3d.Identity;
            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(DVector3d.FromXYZ(0,0,1),direction,ref ma);

            //ElementHolder  d =JYX_ZYJC_CLR.PublicMethod.solid_sweepbodywire(profile,path);
            //d.Transform(DTransform3d.FromMatrixAndTranslation(ma, location),false);

            //DVector3d direction1 = DVector3d.FromXYZ(0,0,1);

            //Channel channel1 = new Channel(diameter, diameter, depth, length, DPoint3d.FromXYZ(0,0,0), location, direction1, thickness);
            //List<ElementHolder> element_holder_list1 =channel1.Create3DElements();
            //foreach(ElementHolder element_holder1 in element_holder_list1)
            //{
            //    ElementHolder element_holder1_new =JYX_ZYJC_CLR.PublicMethod.solid_subtract(element_holder1, d);
            //    JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder1_new);
            //}

            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(d);
            #endregion

            #region 椭圆柱拉伸体测试
            //ElementHolder elem =create_elliptic_cylinder_solid(DPoint3d.Zero,100000,200000,DMatrix3d.Identity,Math.PI/2,Math.PI,100000);
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem);
            #endregion

            #region 多边柱拉伸体测试
            //ElementHolder elem = OPM_Public_Api.create_prism_solid(10000, 15000, 6, DPoint3d.FromXYZ(100000, 0, 0), DVector3d.FromXYZ(0, 0, 1));
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem);

            //ElementHolder elem1 = create_prism_solid(8000, 15000, 6, DPoint3d.FromXYZ(100000, 0, 0), DVector3d.FromXYZ(0, 0, 1));
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem1);
            #endregion

            #region 实体剪切测试
            //SolidCylinder solid_cylinder1 = new SolidCylinder(1000, 2000, 0);
            //List<ElementHolder> element_holder_list1 = solid_cylinder1.Create3DElements();
            //SolidCylinder solid_cylinder2 = new SolidCylinder(800, 2000, 0);
            //List<ElementHolder> element_holder_list2 = solid_cylinder2.Create3DElements();
            //ElementHolder elem =JYX_ZYJC_CLR.PublicMethod.solid_subtract(element_holder_list1[0], element_holder_list2[0]);
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(elem);
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder_list1[0]);
            //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder_list2[0]);
            #endregion

            #region 出图测试
            //JYX_ZYJC_CLR.PublicMethod.detailingSymbolValidation();
            #endregion

            #region 夹套管测试
            //JacketedPipeTool JacketedPipeTool = new JacketedPipeTool();
            //JacketedPipeTool.InstallTool();
            #endregion

            #region 同port管道测试
            //DPoint3d start_dpt = new DPoint3d(1000 * uor_per_master, 0, 0);
            //DPoint3d end_dpt = new DPoint3d(2000 * uor_per_master, 0, 0);
            //BMECObject pipe1 = OPM_Public_Api.create_pipe(start_dpt, end_dpt, 100);
            //pipe1.DiscoverConnections();
            //pipe1.UpdateConnections();
            //pipe1.Create();
            //BMECObject pipe2 = OPM_Public_Api.create_pipe(start_dpt, end_dpt, 150);
            //pipe2.DiscoverConnections();
            //pipe2.UpdateConnections();
            //pipe2.Create();
            #endregion

            #region 测试替换管道子元素
            //long id = 10774;
            //BIM.CellElement cell = app.ActiveModelReference.GetElementByID(id).AsCellElement();

            //ulong uid = 10774;

            //BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(uid);
            //DPoint3d pt1 = bmec_object.GetNthPort(0).LocationInUors;
            //DPoint3d pt2 = bmec_object.GetNthPort(1).LocationInUors;
            //BIM.CellElement new_cell=cell.Clone().AsCellElement();
            //cell.ResetElementEnumeration();
            //while (cell.MoveToNextElement(true,1))
            //{
            //    BIM.Element elem = cell.CopyCurrentElement();
            //    if (elem.IsCellElement())
            //    {
            //        BIM.CellElement sub_cell = elem.AsCellElement();
            //        if (sub_cell.Name.Equals("Graphics"))
            //        {
            //            sub_cell.ResetElementEnumeration();

            //            while (sub_cell.MoveToNextElement(true,1))
            //            {
            //                BIM.Element sub_elem = sub_cell.CopyCurrentElement();

            //                if (sub_elem.Type == BIM.MsdElementType.Cone)
            //                {
            //                    BIM.Element new_cone = OPM_Public_Api.create_cone_surface(200, pt1, pt2);
            //                    //Element sub_elem_ = JYX_ZYJC_CLR.PublicMethod.ConvertToDgnNetEle(sub_elem);
            //                    //ConeElement new_cone = new ConeElement(Session.Instance.GetActiveDgnModel(), sub_elem_, 100* uor_per_master, 100* uor_per_master, pt1, pt2,DMatrix3d.Identity,true);
            //                    //BIM.Element new_elem=JYX_ZYJC_CLR.PublicMethod.ConvertToInteropEle(new_cone);
            //                    //BIM.Element new_cone =app.SmartSolid.CreateCone(null, 100, 100, pt1.Distance(pt2));
            //                    //Element new_elem = JYX_ZYJC_CLR.PublicMethod.ConvertToDgnNetEle(new_cone);

            //                    //app.ActiveModelReference.AddElement(new_cone);

            //                    //app.ActiveModelReference.ReplaceElement(sub_elem, new_cone);
            //                    //app.ActiveModelReference.AddElement(sub_elem);

            //                    //sub_cell.AppendToCellOfCurrentElement(new_cone);
            //                    //app.ActiveModelReference.AddElement(sub_elem);
            //                    //sub_cell.Rewrite();
            //                    //sub_cell.Name = "test";
            //                    //cell.ReplaceCurrentElement(sub_cell);

            //                    //sub_cell.AppendToCellOfCurrentElement(new_cone);
            //                    //sub_cell.DeleteCurrentElement();
            //                    //new_sub_cell = sub_cell.Clone();
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}





            //BMECApi.Instance.ReplaceComponentEx(bmec_object, bmec_object,2);

            //BMECObject ff;
            //ff.LegacyGraphicsId

            //DgnModel active_model = Session.Instance.GetActiveDgnModel();           //当前激活的Model
            //DSegment3d segment = new DSegment3d(0, 0, 0, 10 * uor_per_master, 0, 0);
            //System.Windows.Forms.MessageBox.Show(uor_per_master.ToString());
            //LineElement line = new LineElement(active_model, null, segment);
            ////设置元素的样式
            //ElementPropertiesSetter prop_setter = new ElementPropertiesSetter();
            //prop_setter.SetColor(0);
            //prop_setter.SetWeight(2);
            //prop_setter.Apply(line);
            //line.AddToModel();  //将元素添加到当前激活的Model中

            //DPoint3d[] points = new DPoint3d[5];
            //points[0] = DPoint3d.FromXYZ(0, 0, 0);
            //points[1] = DPoint3d.FromXYZ(uor_per_master, 2 * uor_per_master, 0);
            //points[2] = DPoint3d.FromXYZ(3 * uor_per_master, -2 * uor_per_master, 0);
            //points[3] = DPoint3d.FromXYZ(5 * uor_per_master, 2 * uor_per_master, 0);
            //points[4] = DPoint3d.FromXYZ(6 * uor_per_master, 0 * uor_per_master, 0);
            //LineStringElement linestring = new LineStringElement(active_model, null, points);

            //prop_setter.SetColor(1);
            //prop_setter.Apply(linestring);
            //linestring.AddToModel();
            #endregion
        }

        //private static void PipelineManagerDialog2_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    PipelineUtilities.IsPipelineManagerRunning = false;
        //    CommandsList.s_PipelineManagerDialog.FormClosed -= new FormClosedEventHandler(CommandsList.PipelineManagerDialog2_FormClosed);
        //    CommandsList.s_PipelineManagerDialog = null;
        //}
        public static BIM.Element testtt(BIM.Point3d center_point, double xR, double yR, BIM.Matrix3d ma, double start, double sweep, double height)
        {

            BIM.ArcElement arc = app.CreateArcElement2(null, center_point, xR, yR, ma, start, sweep);

            BIM.LineElement line = app.CreateLineElement2(null, arc.EndPoint, arc.StartPoint);
            List<BIM.ChainableElement> base_elem_all_list = new List<BIM.ChainableElement>();
            base_elem_all_list.Add(arc);
            base_elem_all_list.Add(line);
            BIM.ComplexShapeElement ComplexShapeElement = app.CreateComplexShapeElement1(base_elem_all_list.ToArray(), BIM.MsdFillMode.Filled);

            BIM.LineElement line1 = app.CreateLineElement2(null, center_point, app.Point3dFromXYZ(0, 0, height));
            BIM.SmartSolidElement solid = app.SmartSolid.SweepProfileAlongPath(ComplexShapeElement, line1);
            return solid;
        }

        public static void lianjie_guandao(string unparsed)
        {
            elbow.InstallNewTool();
        }

        public static void select_zhenggen_pipe(string unparsed)
        {
            
            select_zhenggen_pipe select_zhenggen_pipe = new select_zhenggen_pipe();
            select_zhenggen_pipe.InstallTool();
        }
        public static void select_start_end_pipe(string unparsed)
        {
            
            select_start_end_pipe select_start_end_pipe = new select_start_end_pipe();
            select_start_end_pipe.InstallTool();
        }
        public static void dingyi_zhinengxian(string unparsed)
        {
            
            dingyi_zhinengxian dingyi_zhinengxian = new dingyi_zhinengxian();
            dingyi_zhinengxian.start();
        }

        public static void zhinengxian_to_pipe(string unparsed)
        {
            
            multiZhinengxianPipe multi_zhinengxian_to_pipe = new multiZhinengxianPipe();
            multi_zhinengxian_to_pipe.start();
        }

        public static void setting_pipe_display_info(string unparsed)
        {
            
            Setting_Pipe_Display_Info_Form setting_pipe_display_info_form = new Setting_Pipe_Display_Info_Form();
            if (setting_pipe_display_info_form.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                JYX_ZYJC_CLR.PublicMethod.change_display_pipe_level_info(setting_pipe_display_info_form.pipe_display_info);
                BMECApi.Instance.StartDefaultCommand();
            }
        }

        /// <summary>
        /// 断开管道为指定长度
        /// </summary>
        /// <param name="unparsed"></param>
        public static void CutOffPipeTool(string unparsed)
        {
            
            CutOffPipeTool cutOffPipeTool = new CutOffPipeTool();
            cutOffPipeTool.InstallTool();
        }
        /// <summary>
        /// 合并一组管道
        /// </summary>
        /// <param name="unparsed"></param>
        public static void MergePipeTool(string unparsed)
        {
            
            MergePipeTool mergePipeTool = new MergePipeTool();
            mergePipeTool.InstallTool();
        }
        public static void create_jiataoguan(string unparsed)
        {
            
            JacketedPipeTool.InstallNewTool();
        }

        public static void jiataoguan_huiqie(string unparsed)
        {
            
            Jiataoguan_kaikou jiataoguan_kaikou = new Jiataoguan_kaikou();
            jiataoguan_kaikou.InstallTool();
        }

        /// <summary>
        /// 清除多余图形
        /// </summary>
        /// <param name="unparsed"></param>
        public static void elementClear(string unparsed)
        {
            
            ElementClearForm myForm = ElementClearForm.getInstance();
#if DEBUG

#else
            myForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ElementClearForm));
            myForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            myForm.Show();
        }

        /// <summary>
        /// 移动管道
        /// </summary>
        /// <param name="unparsed"></param>
        public static void MovePipe(string unparsed)
        {
            
            MovePipesTool.InstallNewTool();
        }

        /// <summary>
        /// 成组布管
        /// </summary>
        /// <param name="unparsed"></param>
        public static void GroupPipeMain(string unparsed)
        {
            
            GroupPipeToolFrom from1 = GroupPipeToolFrom.instence();
#if DEBUG
#else
            from1.AttachAsTopLevelForm(MyAddin.s_addin, false);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GroupPipeToolFrom));
            from1.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            from1.Show();
        }

        public static void createLine(string unparsed)
        {
            
            BIM.Point3d point1 = app.Point3dFromXYZ(0, 0, 0);
            BIM.Point3d point2 = app.Point3dFromXYZ(100, 0, 0);
            BIM.LineElement line = app.CreateLineElement2(null, point1, point2);
            app.ActiveModelReference.AddElement(line);
        }

        public static void shuaxin(string unparsed)
        {
            
            refreshChecked refre = new refreshChecked();
            refre.InstallTool();

        }

        public static void excelEmportCailiao(string unparsed)
        {
            
            ReportFrom from1 = ReportFrom.instence(true);
#if DEBUG
#else
            from1.AttachAsTopLevelForm(MyAddin.s_addin, false);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportFrom));
            from1.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            from1.Show();
        }

        public static void excelEmportEquipment(string unparsed)
        {
            
            EquipmentReportFrom from1 = EquipmentReportFrom.instence();
#if DEBUG
#else
            from1.AttachAsTopLevelForm(MyAddin.s_addin, false);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EquipmentReportFrom));
            from1.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            from1.Show();
        }

        public static void textTool(string unparsed)
        {
            
            ToolsTemplate textTooll = new ToolsTemplate();
            textTooll.InstallTool();
        }

        public static void EquipmentManager(string unparsed)
        {
            
            EquipmentManagerForm equipmentManagerForm = EquipmentManagerForm.getInstance();
#if DEBUG

#else
            equipmentManagerForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EquipmentManagerForm));
            equipmentManagerForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            equipmentManagerForm.Show();

            //Test.ToolsTemplate toolstemp = new Test.ToolsTemplate();
            //toolstemp.InstallTool();
        }

        public static void SetCellChildClassType(string unparsed)
        {
            
            CellElementEditor.SetCellChildClassTypeToCon();
        }

        public static void MaterialsProManager(string unparsed)
        {
            
            MaterialsProManagerForm materialsForm = MaterialsProManagerForm.instance();
#if DEBUG
#else
            materialsForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MaterialsProManagerForm));
            materialsForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            materialsForm.Show();
        }

        public static void shengcheng_guanxian(string unparsed)
        {
            
            if (DlgStandardPreference.GetPreferenceValue("LINENUMBER").Length == 0)
            {
                System.Windows.Forms.MessageBox.Show("PipeLine未设置！");
                return;
            }
            shengchengGuanxianForm shengcheng_guanxian_form = new shengchengGuanxianForm();
#if DEBUG
#else
            shengcheng_guanxian_form.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(shengchengGuanxianForm));
            shengcheng_guanxian_form.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            shengcheng_guanxian_form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
#endif
            shengcheng_guanxian_form.Show();
        }

        public static void hiddenElement(string unparsed)
        {
            

            Bentley.DgnPlatformNET.ElementAgenda element_agenda = new Bentley.DgnPlatformNET.ElementAgenda();
            Bentley.DgnPlatformNET.SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();
            for (uint i = 0; i < selected_elem_count; i++)
            {
                Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(i);
                elem.IsInvisible = true;
                elem.ReplaceInModel(elem);
            }
        }

        public static void displayElement(string unparsed)
        {
            
            Bentley.DgnPlatformNET.ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素

            foreach (Bentley.DgnPlatformNET.Elements.Element ele in elements)
            {
                if (ele.IsInvisible)
                {
                    ele.IsInvisible = false;
                    ele.ReplaceInModel(ele);
                }
            }

        }
        public static void updateTolerance(string unpared)
        {
            
            updateToleranceForm upForm = updateToleranceForm.instence();
#if DEBUG
#else
            upForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(updateToleranceForm));
            upForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            upForm.Show();
        }

        #region
        //public static void drawingTest(string unpard)
        //{
        //    BIM.ElementEnumerator element = app.ActiveModelReference.GetSelectedElements();
        //    BIM.Element[] elementList = element.BuildArrayFromContents();
        //    BIM.Element el = elementList[0];
        //    //bool b = app.ActiveModelReference.GetSelectedElements().MoveNext();
        //    //BIM.Element el = app.ActiveModelReference.GetSelectedElements().Current;
        //    //GetSelectedElements
        //    ElementAgenda element_agenda = new ElementAgenda();
        //    SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

        //    uint selected_elem_count = element_agenda.GetCount();

        //    if (selected_elem_count != 1)
        //    {
        //        System.Windows.Forms.MessageBox.Show("请选中一个元素");
        //        BMECApi.Instance.StartDefaultCommand();
        //        return;
        //    }

        //    Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(0);

        //    //BIM.Element el = app.ActiveModelReference.GetElementByID64(elem.ElementId);
        //    IECInstance iec = null;
        //    IECInstance iec1 = null;
        //    try
        //    {

        //        iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(el);

        //        iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);//获取ecInstence ce方法
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.Forms.MessageBox.Show(ex.ToString());
        //    }

        //    if (iec == null || iec1 == null)
        //    {
        //        return;
        //    }

        //    bool isPi = api.InstanceDefinedAsClass(iec1, "PIPING_COMPONENT", true);

        //    bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true); //判断是否是设备 以及他的子类

        //    //bool isValve = api.InstanceDefinedAsClass(iec1, "FLUID_REGULATOR", true);  //判断是否为 阀门以及他的子类

        //    //bool isSupport = api.InstanceDefinedAsClass(iec1, "SUPPORT", true);  //判断是否为支吊架  以及他的子类

        //    bool isPipe = api.InstanceDefinedAsClass(iec1, "PIPE", true); //判断是否为管道 以及他的子类

        //    string name = iec1["NAME"].StringValue; //ecClass的Name字段  (可能不用)  阀门、设备、逻辑

        //    string name1 = iec1["DESCRIPTION"].StringValue; //短描述

        //    string lineNumber = iec1["LINENUMBER"].StringValue; //管线编号 管道
        //    if (isPi && !isPipe)
        //    {
        //        string name11 = iec1["NAME"].StringValue; //PIPINFG其他
        //    }
        //    string eqNum = iec1["CERI_Equipment_Num"].StringValue;


        //    #region 定位
        //    //BMECObject bmec = new BMECObject(iec);

        //    //DPoint3d dp1 = bmec.Transform3d.Translation;
        //    //BIM.Point3d p1 = GroupPipeTool.ceDpoint_v8Point(dp1);

        //    //BIM.View view = app.CommandState.LastView();
        //    //view.ZoomAboutPoint(p1, 0.1);
        //    #endregion

        //    #region 视图
        //    //保存视图
        //    BIM.SavedViewElement dd = app.ActiveDesignFile.FindSavedView(app.ActiveModelReference.Name);
        //    BIM.View vw = app.ActiveDesignFile.Views[8];
        //    //Session
        //    //ViewGroup
        //    //ViewContext
        //    //ViewGroupCollection
        //    //ViewInformation
        //    //Viewport
        //    //IViewDraw
        //    //System.Collections.ArrayList
        //    //Bentley.Internal.MstnPlatformNET.DisplayStyleList
        //    vw.ApplySavedViewElement(dd, BIM.MsdCopyViewPort.ApplySize, true, true, true, true, true);
        //    vw.IsOpen = false;
        //    BIM.Range3d range3d;
        //    range3d.High = app.Point3dAdd(vw.get_Origin(), vw.get_Extents());
        //    range3d.Low = vw.get_Origin();
        //    BIM.LineElement line = app.CreateLineElement2(null, app.Point3dFromXY(range3d.Low.X, range3d.Low.Y), app.Point3dFromXY(range3d.High.X, range3d.High.Y));
        //    app.ActiveModelReference.AddElement(line);
        //    #endregion
        //    //////////////////////////
        //    /////////////////////////
        //    ///进工具类实现放置
        //    /////////////////////////
        //    ///////////////////////// 
        //}

        //public static void drawingVaSuEq(string unpard)
        //{
        //    //BIM.ElementEnumerator element = app.ActiveModelReference.GetSelectedElements();
        //    //BIM.Element[] elementList = element.BuildArrayFromContents();
        //    //BIM.Element el = elementList[0];
        //    //bool b = app.ActiveModelReference.GetSelectedElements().MoveNext();
        //    //BIM.Element el= app.ActiveModelReference.GetSelectedElements().Current;
        //    //GetSelectedElements





        //    //ElementAgenda element_agenda = new ElementAgenda();
        //    //SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素


        //    //// Element elem = hit_path.GetHeadElement();

        //    ////BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

        //    //uint selected_elem_count = element_agenda.GetCount();

        //    //if (selected_elem_count != 1)
        //    //{
        //    //    //app.ShowCommand("请选中一个元素");
        //    //    System.Windows.Forms.MessageBox.Show("请选中一个元素");
        //    //    BMECApi.Instance.StartDefaultCommand();
        //    //    return;
        //    //}

        //    //Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(0);

        //    ////BIM.Element el = app.ActiveModelReference.GetElementByID64(elem.ElementId);

        //    ////IECInstance iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(el);

        //    ////获取ecInstence ce方法
        //    //IECInstance iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);


        //    ////判断是否是设备 以及他的子类
        //    //bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true);


        //    ////判断是否为 阀门以及他的子类
        //    //bool isValve = api.InstanceDefinedAsClass(iec1, "FLUID_REGULATOR", true);


        //    ////判断是否为支吊架  
        //    //bool isSupport = api.InstanceDefinedAsClass(iec1, "SUPPORT", true);


        //    //if (!(eqb || isValve || isSupport))
        //    //{
        //    //    app.ShowCommand("选取无效目标，请重新选择");
        //    //    //System.Windows.Forms.MessageBox.Show("选取无效目标，请重新选择");
        //    //    BMECApi.Instance.StartDefaultCommand();
        //    //    return;
        //    //}


        //    //string name = iec1.ClassDefinition.Name; //ecClass的Name字段  (可能不用)


        //    //string name1 = iec1["DESCRIPTION"].StringValue; //短描述



        //    //bool isPipe = api.InstanceDefinedAsClass(iec1, "PIPE", true); //判断是否为管道 以及他的子类

        //    //string lineNumber = iec1["LINENUMBER"].StringValue; //管线编号

        //    //////////////////////////
        //    /////////////////////////
        //    ///进工具类实现放置
        //    /////////////////////////
        //    ///

        //    DrawingVaSuEqTool tool = new DrawingVaSuEqTool("");
        //    tool.InstallTool();


        //}

        //public static void drawingPipeLine(string unpard)
        //{
        //    DrawingPipeLineTool drawTool = new DrawingPipeLineTool();
        //    drawTool.InstallTool();
        //}
        #endregion

        public static void tesKeepOut(string unpard)
        {
            

            #region 测试给displayStyle的DisplayHiddenEdges赋值来实现遮挡
            //DisplayStyleList disList = new DisplayStyleList(Session.Instance.GetActiveDgnFile(), false, false);

            //IEnumerator<DisplayStyle> disEn = disList.GetEnumerator();
            //while(disEn.MoveNext())
            //{
            //    DisplayStyle sty2 = disEn.Current;
            //    string name1 = sty2.Name;
            //    if(name1.Equals("Cut"))
            //    {
            //        DisplayStyleFlags flag2 = sty2.GetFlags();
            //        flag2.DisplayHiddenEdges = true;
            //        sty2.SetFlags(flag2);
            //        DisplayStyle sty3=DisplayStyleManager.WriteDisplayStyleToFile(sty2, Session.Instance.GetActiveDgnFile());//可以用来更改文件的display style
            //    }
            //    else if(name1.Equals("Forward"))
            //    {
            //        DisplayStyleFlags flag2 = sty2.GetFlags();
            //        flag2.DisplayHiddenEdges = true;
            //        sty2.SetFlags(flag2);
            //        DisplayStyle sty3=DisplayStyleManager.WriteDisplayStyleToFile(sty2, Session.Instance.GetActiveDgnFile());
            //    }
            //}
            #endregion

            KeepOutForm koForm = KeepOutForm.instance();
#if DEBUG
#else
            koForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KeepOutForm));
            koForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            koForm.Show();
        }
        #region
        public static void testView(string unpard)
        {
            #region 测试保存视图
            //BIM.SavedViewElement saveView = app.ActiveDesignFile.FindSavedView(app.ActiveModelReference.Name);

            //BIM.View vw = app.ActiveDesignFile.Views[8];

            //vw.ApplySavedViewElement(saveView, BIM.MsdCopyViewPort.ApplySize, true, true, true, true, true);

            //vw.IsOpen = false;

            //BIM.Range3d range;

            //range.High = app.Point3dAdd(vw.get_Origin(), vw.get_Extents());

            //range.Low = vw.get_Origin();

            //BIM.LineElement line = app.CreateLineElement2(null, app.Point3dFromXY(range.Low.X, range.Low.Y), app.Point3dFromXY(range.High.X, range.High.Y));

            //app.ActiveModelReference.AddElement(line);
            #endregion
            //JYX_ZYJC_CLR.PublicMethod.createPlanCallout();

            //JYX_ZYJC_CLR.PublicMethod.createElevationCallout();
            CustomizeHeightSectionForm Form = CustomizeHeightSectionForm.instence();
#if DEBUG
#else
            Form.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomizeHeightSectionForm));
            Form.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            Form.Show();
        }

        //        public static void testRange(string unpard)
        //        {
        //            string saveViewName = "";
        //            DgnModel dgnmodel = Session.Instance.GetActiveDgnModel();
        //            DgnModel designModel = null;
        //            DgnAttachmentCollection dgnAttList = dgnmodel.GetDgnAttachments();
        //            foreach (DgnAttachment dgnAtt in dgnAttList)
        //            {
        //                DgnAttachment attach = dgnAtt;
        //                ElementId ids = dgnAtt.ClipElementId;
        //                DgnModelType type = dgnAtt.ModelType;
        //                string id = ids.ToString();
        //                ElementId ii = dgnAtt.GetElementId();
        //                if (type == DgnModelType.Drawing)
        //                {
        //                    DgnModel dgn = dgnAtt.GetDgnModel();
        //                    foreach (DgnAttachment a in dgn.GetDgnAttachments())
        //                    {
        //                        attach = a;
        //                        double fount = a.FrontClipDepth;
        //                        ElementId ids11 = a.ClipElementId;
        //                        double tttttt = a.BackClipDepth;
        //                        designModel = a.GetDgnModel();
        //                        a.FrontClipDepth = 5000;
        //                    }
        //                }
        //                ReferenceSynchOption syn = attach.SynchWithNamedView;
        //                ElementId ids1 = attach.ClipElementId;
        //                //double  d = attach.clip;
        //                //d = attach.FrontClipDepth;
        //                if (syn != ReferenceSynchOption.Notsynced)
        //                {
        //                    NamedView viewName = attach.GetNamedView();
        //                    string name = viewName.Name;
        //                    saveViewName = name;
        //                }
        //                DPoint2d[] dpList = attach.GetClipPoints();
        //            }
        //            BIM.SavedViewElement saveView = app.ActiveDesignFile.FindSavedView(saveViewName);

        //            if (saveView == null)
        //            {
        //                System.Windows.Forms.MessageBox.Show("未找到保存的视图");
        //                return;
        //            }

        //            BIM.View vw = app.ActiveDesignFile.Views[4];
        //            try
        //            {
        //                vw.ApplySavedViewElement(saveView, BIM.MsdCopyViewPort.ApplySize, true, true, true, true, true);
        //            }
        //            catch (Exception ex)
        //            {
        //                string t = ex.ToString();
        //                return;
        //            }

        //            vw.IsOpen = false;

        //            BIM.Range3d range;

        //            range.High = app.Point3dAdd(vw.get_Origin(), vw.get_Extents());

        //            range.Low = vw.get_Origin();

        //            DPoint3d h = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(range.High);
        //            DPoint3d l = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(range.Low);

        //            DSegment3d ds = new DSegment3d(h, l);

        //            Bentley.DgnPlatformNET.Elements.LineElement line = new Bentley.DgnPlatformNET.Elements.LineElement(designModel, null, ds);

        //            line.AddToModel();


        //            string modelName = designModel.ModelName;
        //            BIM.ModelReference model = app.ActiveDesignFile.Models[1];
        //            string name3 = model.Name;
        //            BIM.Element el = model.GetElementByID64(16468);
        //            range = el.Range;
        //            BIM.LineElement line1 = app.CreateLineElement2(null, range.High, range.Low);
        //            model.CopyElement(line1);

        //        }

        //        public static void testNote(string unpard)
        //        {
        //            DgnModel active_model = Session.Instance.GetActiveDgnModel();
        //            DgnFile active_file = Session.Instance.GetActiveDgnFile();
        //            TextBlockProperties txtBlockProp = new TextBlockProperties(active_model);
        //            txtBlockProp.IsViewIndependent = true;
        //            ParagraphProperties paraProp = new ParagraphProperties(active_model);
        //            DgnTextStyle txtStyle = DgnTextStyle.GetSettings(active_file);
        //            double width = 6;
        //            txtStyle.SetProperty(TextStyleProperty.Width, width);
        //            txtStyle.SetProperty(TextStyleProperty.Height, 6);
        //            RunProperties runProp = new RunProperties(txtStyle, active_model);
        //            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, active_model);
        //            txtBlock.AppendText("hello world");

        //            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        //            double uorPerMast = dgnModel.GetModelInfo().UorPerMaster;
        //            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
        //            DimensionStyle dimStyle = new DimensionStyle("CERI_Standard", dgnFile);
        //            //dimStyle.SetBooleanProp(true, DimStyleProp.Placement_UseStyleAnnotationScale_BOOLINT);
        //            //dimStyle.SetDoubleProp(50, DimStyleProp.Placement_AnnotationScale_DOUBLE);
        //            //dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideHeight_BOOLINT);
        //            //dimStyle.SetDistanceProp(G / 600 * uorPerMast, DimStyleProp.Text_Height_DISTANCE, dgnModel);
        //            //dimStyle.SetBooleanProp(true, DimStyleProp.Text_OverrideWidth_BOOLINT);
        //            //dimStyle.SetDistanceProp(G / 600 * uorPerMast, DimStyleProp.Text_Width_DISTANCE, dgnModel);
        //            //dimStyle.SetBooleanProp(true, DimStyleProp.General_UseMinLeader_BOOLINT);
        //            //dimStyle.SetDoubleProp(0.5, DimStyleProp.Terminator_MinLeader_DOUBLE);
        //            //dimStyle.SetBooleanProp(true, DimStyleProp.Value_AngleMeasure_BOOLINT);
        //            //dimStyle.SetAccuracyProp((byte)AnglePrecision.Use1Place, DimStyleProp.Value_AnglePrecision_INTEGER);
        //            int alignInt = (int)DimStyleProp_General_Alignment.True;
        //            StatusInt status = dimStyle.SetIntegerProp(alignInt, DimStyleProp.General_Alignment_INTEGER);
        //            int valueOut;
        //            dimStyle.GetIntegerProp(out valueOut, DimStyleProp.General_Alignment_INTEGER);
        //            DgnTextStyle textStyle = new DgnTextStyle("TestStyle", dgnFile);
        //            LevelId lvlId = Settings.GetLevelIdFromName("Default");
        //            CreateDimensionCallbacks callbacks = new CreateDimensionCallbacks(dimStyle, textStyle, new Symbology(), lvlId, null);
        //            DimensionElement dimEle = new DimensionElement(dgnModel, callbacks, DimensionType.Note);

        //            DPoint3d ptStart = new DPoint3d(0, 0, 0);
        //            DPoint3d ptEnd = new DPoint3d(10000, 10000, 0);
        //            dimEle.InsertPoint(ptStart, null, dimStyle, -1);
        //            dimEle.InsertPoint(ptEnd, null, dimStyle, -1);
        //            dimEle.InsertPoint(new DPoint3d(10000, 20000, 0), null, dimStyle, -1);
        //            TextQueryOptions tt = new TextQueryOptions();
        //            bool b1 = tt.ShouldIncludeEmptyParts;
        //            bool b2 = tt.ShouldRequireFieldSupport;
        //            tt.ShouldRequireFieldSupport = true;
        //            tt.ShouldIncludeEmptyParts = true;
        //            TextPartIdCollection idColl = dimEle.GetTextPartIds(tt);
        //            foreach (var aa in idColl)
        //            {
        //                dimEle.ReplaceTextPart(aa, txtBlock);
        //            }
        //            Element ele = dimEle;


        //            DimensionStyle dimstyle = new DimensionStyle("CERI_Standard", active_file);
        //            DPoint3d[] dpList = new DPoint3d[3];
        //            dpList[0] = new DPoint3d(0, 0, 0);
        //            dpList[1] = new DPoint3d(10000, 10000, 0);
        //            dpList[2] = new DPoint3d(20000, 10000, 0);
        //            NoteCellHeaderElement noteEle = new NoteCellHeaderElement(out ele, txtBlock, dimstyle, active_model, dpList);
        //            noteEle.AddToModel();



        //            dimEle.AddToModel();
        //        }
        #endregion

        public static void testNode1(string unpard)
        {
            
            string keyIn = "dimstyle active ";
            keyIn += "CERI_引线标注";
            app.CadInputQueue.SendKeyin(keyIn);
            pipeBiaozhuForm pipeForm = pipeBiaozhuForm.instence();
#if DEBUG
#else
            pipeForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
#endif
            pipeForm.Show();
        }

        public static void centerPipeLine(string unpard)
        {
            
            #region 反射（调用控制显示中心线方法）
            //MechViewAddIn meAddin = new MechViewAddIn();

            //Type getT = meAddin.GetType();
            ////根据类型创建对象
            //object dObj = Activator.CreateInstance(getT);
            ////获取类型信息
            //Type t = BMECApi.Instance.GetType();

            ////Type[] params_type = new Type[0];
            ////ParameterModifier[] parameter_modifier = new ParameterModifier[0];
            ////调用方法的一些标注位（NonPublic、Public、Static）
            //BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
            ////获取方法信息  不写flag默认只能获取public类型
            //MethodInfo method = t.GetMethod("SetShowCenterlines", flag);
            //MethodInfo meGet = t.GetMethod("GetShowCenterlines", flag);
            //MethodInfo getMet = getT.GetMethod("GetShowCenterlines", flag);
            //MethodInfo me = t.GetMethod("EndUndoGroup");
            //MethodInfo me2 = t.GetMethod("ApplyCustomSymbology");
            //IntPtr intptr = JYX_ZYJC_CLR.PublicMethod.SetShowCenterlines(false);
            ////反射调用 BMECApi.Instance中的SetShowCenterlines方法 参数：intptr, true
            //object value = method.Invoke(BMECApi.Instance, new object[] { intptr, false });
            //object value2 = getMet.Invoke(BMECApi.Instance, new object[] { intptr });
            //object value1 = getMet.Invoke(meAddin, new object[] { intptr });
            //int index = Session.GetActiveViewport().ViewNumber;
            //BIM.View vw = app.ActiveDesignFile.Views[index + 1];
            //vw.Redraw(); //刷新当前视图
            //             //FeaturesID.MarkFeature(FeaturesID.OPM_PROMOTE_CENTERLINE_SHOW);

            ////FeaturesID.MarkFeature(FeaturesID.OPM_PROMOTE_CENTERLINE_HIDE);
            #endregion
            pipeCenterLinesDisplayManger pipeForm = pipeCenterLinesDisplayManger.instance();
#if DEBUG
#else
            pipeForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(pipeCenterLinesDisplayManger));
            pipeForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            pipeForm.Show();
        }

        public static void nodeList(string unpard)
        {
            
            string keyIn = "dimstyle active ";
            keyIn += "CERI_引线标注";
            app.CadInputQueue.SendKeyin(keyIn);
            plpipeBiaozhuForm pipeForm = plpipeBiaozhuForm.instence();
#if DEBUG
#else
            pipeForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
#endif
            pipeForm.Show();
        }

        public static void textClipVolume(string unpard)
        {
            
            //Element ele = JYX_ZYJC_CLR.PublicMethod.getClipVolume();
            //ElementId eleid = ele.ElementId;
            UInt64 ii = JYX_ZYJC_CLR.PublicMethod.getClipVolume(1);

            UInt64 iii = JYX_ZYJC_CLR.PublicMethod.getClipVolume1(1);

            DRange3d dr;
            Session.Instance.GetActiveDgnModel().GetRange(out dr);//得到包围所有元素的最小range

            ModelElementsCollection eleList = Session.Instance.GetActiveDgnModel().GetElements();
            IEnumerator<Element> ie = eleList.GetEnumerator();
            while (ie.MoveNext())
            {
                Element ele = ie.Current;

                //CellHeaderElement ee = ele as CellHeaderElement;
                //DRange3d dre;
                //ee.CalcElementRange(out dre);
            }

            BIM.SavedViewElement dd = app.ActiveDesignFile.FindSavedView("556");
            long id = dd.ID64;
            //BIM.Range3d ra = dd.Range;
            Viewport vv = Session.GetActiveViewport();
            DPoint3d[] viewBox = vv.GetViewBox(DgnCoordSystem.Active, true);

            DSegment3d ds = new DSegment3d(viewBox[0], viewBox[3]);
            LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);
            line.AddToModel();
            Element cl = vv.GetClipBoundElement();
            DisplayableElement ddd = (DisplayableElement)cl;
            DRange3d drr;
            ddd.CalcElementRange(out drr);
            DSegment3d ds1 = new DSegment3d(drr.Low, drr.High);
            LineElement line1 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds1);
            line1.AddToModel();

            IECInstance iec = null;
            try
            {

                iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(cl);

                //iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);//获取ecInstence ce方法
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            CustomItemHost custom = new CustomItemHost(cl, true);
            IList<IDgnECInstance> idgnList = custom.CustomItems;
            //JYX_ZYJC_CLR.PublicMethod.getClipVolume2();
            //BIM.Point3d p1 = app.Point3dFromXYZ(0, 0, 0);
            //DPoint3d dp1 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d1(p1);
        }

        public static void moveSupport(string unpard)
        {
            
            MoveSupportTool.InstallNewTool();
        }

        public static void supportRelationshipTest(string unpard)
        {
            
            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请先选中元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }

            List<Element> yeleList = new List<Element>();
            for (uint i = 0; i < selected_elem_count; i++)
            {
                Element ele = element_agenda.GetEntry(i);
                yeleList.Add(ele);
            }

            BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(yeleList[0].ElementId);
            bmec.DiscoverConnectionsEx();
            if (bmec != null)
            {
                IECInstance iec3 = bmec.RelatedInstance;
                IECRelationshipInstanceCollection reList = bmec.Instance.GetRelationshipInstances();
                List<string> reNameList = new List<string>();
                foreach (var a in reList)
                {
                    string name = a.ClassDefinition.Name;
                    if (name.Equals("SEGMENT_HAS_PIPING_COMPONENT"))
                    {
                        IECInstance iec1 = a.Source;
                        IECInstance iec2 = a.Target;
                    }
                    reNameList.Add(name);
                }
            }
        }

        public static void rotateNote(string unpard)
        {
            
            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请先选中元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }

            List<Element> yeleList = new List<Element>();
            for (uint i = 0; i < selected_elem_count; i++)
            {
                Element ele = element_agenda.GetEntry(i);

                DPoint3d dp = new DPoint3d();

                DimensionElement dim = null;

                if (ele.ElementType == MSElementType.TextNode || ele.ElementType == MSElementType.CellHeader)
                {
                    CellHeaderElement cellEle = ele as CellHeaderElement;

                    NoteCellHeaderElement noteEle = ele as NoteCellHeaderElement;
                    List<Element> eleList = noteEle.GetDependants();

                    Element visableEle = null;

                    if (!eleList[0].IsInvisible) visableEle = eleList[0];
                    else visableEle = eleList[1];

                    DimensionElement dimEle = visableEle as DimensionElement;

                    dim = dimEle;

                    noteEle.GetTransformOrigin(out dp);

                    Element eee = Element.GetFromElementRef(dim.GetNativeElementRef());

                    //eleList.Clear();

                    //Element ee = getAllChildElement(noteEle);

                    DMatrix3d dm = DMatrix3d.Rotation(new DVector3d(0, 0, 1), Angle.FromDegrees(45));

                    DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                    TransformInfo ta = new TransformInfo(dt);

                    //dim.ApplyTransform(ta);

                    dim.SetRotationMatrix(dm);

                    try
                    {
                        //dim.ReplaceInModel(dim);
                        RotationNoteTool.InstallNewTool();
                        return;
                    }
                    catch (Exception e)
                    {
                        string erro = e.ToString();
                    }

                    //Element originalElement = Element.GetFromElementRef(_elementRef);
                    //StatusInt statusInt = element.ReplaceInModel(originalElement);
                }

                if (ele.ElementType == MSElementType.Line || ele.ElementType == MSElementType.LineString)
                {
                    List<Element> eleList = ele.GetDependants();

                    DimensionElement dimEle = eleList[0] as DimensionElement;

                    DMatrix3d dm = DMatrix3d.Rotation(new DVector3d(0, 0, 1), Angle.FromDegrees(90));

                    DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                    TransformInfo ta = new TransformInfo(dt);

                    //dimEle.ApplyTransform(ta);

                    dimEle.SetRotationMatrix(dm);

                    try
                    {
                        dimEle.ReplaceInModel(dimEle);
                    }
                    catch (Exception e)
                    {
                        string erro = e.ToString();
                    }
                }
                //ele.ReplaceInModel(ele);

                int index = Session.GetActiveViewport().ViewNumber;
                BIM.View vw = app.ActiveDesignFile.Views[index + 1];
                vw.Redraw(); //刷新当前视图

                app.CommandState.StartDefaultCommand();

                yeleList.Add(ele);
            }
        }

        public static void repairConnect(string unpard)
        {
            
            RepairConnectTool.InstallNewTool();
        }

        public static void Draw_biaogao(string unpard)
        {
           
            drawBiaogao Dbiaogao = new drawBiaogao();

            Dbiaogao.InstallTool();
        }

        static List<Element> eleList = new List<Element>();
        public static Element getAllChildElement(Element ele)
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

        public static void changeNozzle(string unpard)
        {
            
            #region 启动工具类 只有这样才能调用修改事件
            refreshNozzle refre = new refreshNozzle();
            refre.InstallTool();
            #endregion

            #region
            //Bentley.GeometryNET.Point2d p = new Point2d(100, 100);
            //JYX_ZYJC_CLR.PublicMethod.updateForm(1, p);
            //NOZZLE

            //ECInstanceList ecList = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetSelectedInstances();
            //Dictionary<string, string> dicList = new Dictionary<string, string>();
            //foreach (IECInstance iec in ecList)
            //{
            //    BMECObject bmec = null;
            //    bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(iec));
            //    //bmec = new BMECObject(iec);
            //    string nozzleName = bmec.Instance["NUMBER"].StringValue;
            //    nozzleName = "999";
            //    bmec.Instance["NUMBER"].StringValue = nozzleName;
            //    bmec.Create();
            //    //return;
            //    DPoint3d dpoint1 = bmec.GetNthPort(0).LocationInUors;
            //    //DPoint3d dpoint2 = bmec.GetNthPort(1).LocationInUors;

            //    List<BMECObject> bmecList = bmec.ConnectedComponents;
            //    IECInstance reinstence = bmec.RelatedInstance;

            //    IEnumerator<IECPropertyValue> proList = iec.GetEnumerator(true, true);

            //    try
            //    {
            //        while (proList.MoveNext())
            //        {
            //            IECPropertyValue pro = proList.Current;
            //            string text;
            //            bool b = pro.TryGetStringValue(out text);
            //            if (b)
            //            {
            //                dicList.Add(pro.AccessString, pro.StringValue);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Windows.Forms.MessageBox.Show(ex.ToString());
            //    }

            //    IECRelationshipInstanceCollection iecReList = bmec.Instance.GetRelationshipInstances();
            //    foreach (var a in iecReList)
            //    {
            //        IECInstance souInstance = a.Source;
            //        IECInstance tarInstance = a.Target;
            //        string name = souInstance["NAME"].StringValue;
            //    }

            //}
            #endregion

            #region
            //DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            ////calculateToolsForm calculate_form = new calculateToolsForm();
            ////calculate_form.ShowDialog();
            //List<DPoint3d> point3Ds = new List<DPoint3d>();
            //DPoint3d point1 = new DPoint3d(100, 100, 100);
            //DPoint3d point2 = new DPoint3d(100, 600, 100);
            //DPoint3d point3 = new DPoint3d(900, 600, 100);
            //DPoint3d point4 = new DPoint3d(900, 200, 100);
            //DPoint3d point5 = new DPoint3d(700, 200, 100);
            //point3Ds.Add(point1);
            //point3Ds.Add(point2);
            //point3Ds.Add(point3);
            //point3Ds.Add(point4);
            //point3Ds.Add(point5);
            //drawBridge(point3Ds, 300, 300, false, dgnModel);
            #endregion
        }

        public static void drawBridge(List<DPoint3d> BridgePoints, double High, double width, bool Isjiagai, DgnModel dgnModel)
        {

            double a = 100, b = 200; //a为厚度，b为内高
            List<BIM.Point3d> bridgePoints = new List<BIM.Point3d>(); //V8i点集

            for (int i = 0; i < BridgePoints.Count; i++)
            {
                BIM.Point3d bimPoint = app.Point3dFromXYZ(BridgePoints[i].X, BridgePoints[i].Y, BridgePoints[i].Z);
                bridgePoints.Add(bimPoint);
            }

            BIM.Point3d[] bridge_path = bridgePoints.ToArray();

            try
            {

                BIM.Point3d[] jiemianPoints = new BIM.Point3d[8]; //围成无盖截面的点集
                jiemianPoints[0] = app.Point3dFromXYZ(bridgePoints[0].X + (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z);
                jiemianPoints[1] = app.Point3dFromXYZ(bridgePoints[0].X + (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z + b);
                jiemianPoints[2] = app.Point3dFromXYZ(bridgePoints[0].X + width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b);
                jiemianPoints[3] = app.Point3dFromXYZ(bridgePoints[0].X + width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - High);
                jiemianPoints[4] = app.Point3dFromXYZ(bridgePoints[0].X - width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - High);
                jiemianPoints[5] = app.Point3dFromXYZ(bridgePoints[0].X - width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b);
                jiemianPoints[6] = app.Point3dFromXYZ(bridgePoints[0].X - (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z + b);
                jiemianPoints[7] = app.Point3dFromXYZ(bridgePoints[0].X - (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z);
                //截面
                BIM.ShapeElement jiemian = app.CreateShapeElement1(null, jiemianPoints, BIM.MsdFillMode.Filled);  //无盖截面

                app.ActiveModelReference.AddElement(jiemian);

                BIM.Point3d[] jiemianPoints2 = new BIM.Point3d[4]; //有盖截面外轮廓
                jiemianPoints2[0] = app.Point3dFromXYZ(bridgePoints[0].X + width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - (High - b));
                jiemianPoints2[1] = app.Point3dFromXYZ(bridgePoints[0].X + width / 2, bridgePoints[0].Y, bridgePoints[0].Z - (High - b));
                jiemianPoints2[2] = app.Point3dFromXYZ(bridgePoints[0].X - width / 2, bridgePoints[0].Y, bridgePoints[0].Z - (High - b));
                jiemianPoints2[3] = app.Point3dFromXYZ(bridgePoints[0].X - width / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - (High - b));
                BIM.ShapeElement jiemian_wailunkuo = app.CreateShapeElement1(null, jiemianPoints2, BIM.MsdFillMode.Filled);

                BIM.Point3d[] jiemianPoints3 = new BIM.Point3d[4]; //有盖截面内轮廓
                jiemianPoints3[0] = app.Point3dFromXYZ(bridgePoints[0].X + (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - (High - b));
                jiemianPoints3[1] = app.Point3dFromXYZ(bridgePoints[0].X + (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z);
                jiemianPoints3[2] = app.Point3dFromXYZ(bridgePoints[0].X - (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z + b - (High - b));
                jiemianPoints3[3] = app.Point3dFromXYZ(bridgePoints[0].X - (width - 2 * a) / 2, bridgePoints[0].Y, bridgePoints[0].Z);
                BIM.ShapeElement jiemian_neilunkuo = app.CreateShapeElement1(null, jiemianPoints3, BIM.MsdFillMode.Filled);

                BIM.Element[] elements1 = new BIM.Element[1];
                elements1[0] = jiemian_wailunkuo;
                BIM.Element[] elements2 = new BIM.Element[1];
                elements2[0] = jiemian_neilunkuo;

                BIM.ElementEnumerator ele = app.GetRegionDifference(ref elements1, elements2, null, BIM.MsdFillMode.Filled);
                BIM.Element[] eles = ele.BuildArrayFromContents(); //有盖截面

                BIM.Element path = app.CreateLineElement1(null, ref bridge_path); //拉伸路径

                app.ActiveModelReference.AddElement(path);

                BIM.SmartSolidElement bridge; //形体
                if (Isjiagai)  //有盖
                {
                    bridge = app.SmartSolid.SweepProfileAlongPath(eles[0], path);
                }
                else  //无盖
                {
                    bridge = app.SmartSolid.SweepProfileAlongPath(jiemian, path);
                }

                app.ActiveModelReference.AddElement(bridge);
            }
            catch (Exception ex)
            {
                string ee = ex.ToString();
            }
        }

        public static DMatrix3d v8iDvToCe(DVector3d dv1, DVector3d dv2)
        {
            DMatrix3d dm = DMatrix3d.Identity;
            BIM.Point3d p1 = GroupPipeTool.ceDpoint_v8Point(dv1);
            BIM.Point3d p2 = GroupPipeTool.ceDpoint_v8Point(dv2);
            BIM.Matrix3d m = app.Matrix3dFromRotationBetweenVectors(p1, p2);

            DVector3d u_dvec, v_dvec, w_dvec;
            u_dvec = DVector3d.FromXYZ(m.RowX.X, m.RowX.Y, m.RowX.Z);
            v_dvec = DVector3d.FromXYZ(m.RowY.X, m.RowY.Y, m.RowY.Z);
            w_dvec = DVector3d.FromXYZ(m.RowZ.X, m.RowZ.Y, m.RowZ.Z);
            dm.SetRows(u_dvec, v_dvec, w_dvec);

            return dm;
        }

        public static void copySupport(string unpard)
        {
            
            CopySupportTool.InstallNewTool();
        }

        public static void supportRelation(string unparsed)
        {

            
             double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//当前设计文件的主单位
            Bentley.Plant.Utilities.WaitDialog waitDialog = new Bentley.Plant.Utilities.WaitDialog();
            waitDialog.SetTitleString("更新关系");
            waitDialog.SetInformationSting("管部支吊架与管道");
            waitDialog.Show();
            try
            {                
                IECClass iec_class = BMECInstanceManager.Instance.Schema.GetClass("ANCHOR_SERIES");
                IECClass iec_class1 = BMECInstanceManager.Instance.Schema.GetClass("FRAME_RL");
                IECClass iec_class2 = BMECInstanceManager.Instance.Schema.GetClass("FRAME_SD");
                IECClass iec_class3 = BMECInstanceManager.Instance.Schema.GetClass("COMBINED_RL");
                IECClass iec_class4 = BMECInstanceManager.Instance.Schema.GetClass("COMBINED_SHUIDAO");
                ECInstanceList ecList = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn(iec_class, true);
                ECInstanceList ecList1 = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn(iec_class1, true);
                ECInstanceList ecList2 = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn(iec_class2, true);
                ECInstanceList ecList3 = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn(iec_class3, true);
                ECInstanceList ecList4 = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn(iec_class4, true);
                if (ecList1.Count>0)
                {
                    ecList.AddRange(ecList1);
                }
                if(ecList2.Count>0)
                {
                    ecList.AddRange(ecList2);
                }
                if (ecList3.Count > 0)
                {
                    ecList.AddRange(ecList3);
                }
                if (ecList4.Count > 0)
                {
                    ecList.AddRange(ecList4);
                }
                foreach (IECInstance iec in ecList)
                {
                    ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(iec);
                    BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(eleid)/*new BMECObject(iec)*/;
                    if (bmec == null||bmec.Instance==null) continue;
                    bool isgb = false;
                    foreach (IECRelationshipInstance item in bmec.Instance.GetRelationshipInstances())
                    {
                        if (item.ClassDefinition.Name.Equals("DEVICE_HAS_SUPPORT"))
                        {
                            isgb = true;
                            break;
                        }
                    }
                    if (!isgb)
                    {
                        DPoint3d orgin = bmec.Transform3d.Translation;
                        Bentley.Interop.MicroStationDGN.Point3d point3D = new Bentley.Interop.MicroStationDGN.Point3d();
                        point3D.X = orgin.X / uor_per_master;
                        point3D.Y = orgin.Y / uor_per_master;
                        point3D.Z = orgin.Z / uor_per_master;
                        Bentley.Interop.MicroStationDGN.Element[] v_elements = scan_element_at_point(point3D, true);
                        for (int i = 0; i < v_elements.Length; i++)
                        {
                            IECInstance iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(v_elements[i]);
                            BMECObject bMECObject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id((ulong)v_elements[i].ID)/*new BMECObject(iec1)*/;
                            if (bMECObject != null && bMECObject.ClassName.Equals("PIPE"))
                            {
                                //设置关联位置
                                DPoint3d dPoint3D = bmec.Transform3d.Translation;
                                SetPropertyAsDouble("SUPPORT_POINT_X", bmec, dPoint3D.X / uor_per_master);
                                SetPropertyAsDouble("SUPPORT_POINT_Y", bmec, dPoint3D.Y / uor_per_master);
                                SetPropertyAsDouble("SUPPORT_POINT_Z", bmec, dPoint3D.Z / uor_per_master);

                                //target.RelatedInstance = source.Instance;
                                //将形体创建到图纸上
                                bmec.Create();

                                OPM_Public_Api.AddDeviceHasSupport(bMECObject.Instance, bmec.Instance);
                                ReportFrom.setData(bmec);
                            }
                        }
                    }
                }
                waitDialog.Close();
                System.Windows.Forms.MessageBox.Show("更新成功！");
            }            
            catch(Exception ex)
            {
                waitDialog.Close();
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
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

        public static BIM.Element[] scan_element_at_point(BIM.Point3d point3d, bool scan_child_model, BIM.ModelReference model = null)
        {
            BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            if (model == null)
            {
                model = app.ActiveModelReference;
            }
            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();

            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = app.ActiveModelReference.Scan(esc);

            BIM.Element[] elems = ee.BuildArrayFromContents();
            if (elems.Length == 0)
            {
                if (scan_child_model)
                {
                    foreach (BIM.Attachment attachment in app.ActiveModelReference.Attachments)
                    {
                        BIM.Element[] attach_elem = scan_element_at_point(point3d, scan_child_model, attachment);
                        if (attach_elem != null)
                        {
                            return attach_elem;
                        }
                    }
                }
            }
            else
            {
                return elems;
            }

            return null;
        }
        public static BIM.Element[] scan_element_at_point(BIM.Point3d point3d, bool scan_child_model, BIM.Attachment attachment)
        {

            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();

            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = attachment.Scan(esc);

            BIM.Element[] elems = ee.BuildArrayFromContents();
            if (elems.Length == 0)
            {
                if (scan_child_model)
                {
                    foreach (BIM.Attachment child_attachment in attachment.Attachments)
                    {
                        BIM.Element[] attach_elem = scan_element_at_point(point3d, scan_child_model, child_attachment);
                        if (attach_elem != null)
                        {
                            return attach_elem;
                        }
                    }
                }
            }
            else
            {
                return elems;
            }
            return null;
        }

        public static void delPipe(string unpared)
        {
            
            List<BMECObject> delBmecs = GetSelectedPipingComponentBMECObject();
            if(delBmecs.Count==0)
            {
                System.Windows.Forms.MessageBox.Show("请先选择管道！");
            }
            else
            {
                foreach(BMECObject bmec in delBmecs)
                {
                    //IECRelationshipClass iec_class = BMECInstanceManager.Instance.Schema.GetClass("DEVICE_HAS_SUPPORT") as IECRelationshipClass;
                    //if(iec_class!=null)
                    //{
                    //    bmec.Instance.GetRelationshipInstances().Clear(iec_class);
                    //    bmec.Create();
                    //}
                    List<IECInstance> supportIec = MyLinearPlacementTool.clearRelationshipInstance(bmec.Instance, "DEVICE_HAS_SUPPORT");
                    BMECApi.Instance.DeleteFromModel(bmec);
                }
                
            }
        }

        /// <summary>
        /// 获取当前选中的管道
        /// </summary>
        /// <returns></returns>
        private static List<BMECObject> GetSelectedPipingComponentBMECObject()
        {
            List<BMECObject> result = new List<BMECObject>();
            ElementAgenda elementAgenda = new ElementAgenda();//选中Element的集合
            SelectionSetManager.BuildAgenda(ref elementAgenda);//获取选中的元素的集合
            for (uint i = 0; i < elementAgenda.GetCount(); i++)//获取选中 Element 的 BMECObject
            {
                Element element = elementAgenda.GetEntry(i);
                BMECObject ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);//通过ElementId 获取 BMECObject
                if (ec_object != null && ec_object.Instance != null)
                {
                    if (BMECApi.Instance.InstanceDefinedAsClass(ec_object.Instance, "PIPE", true))
                    {
                        result.Add(ec_object);
                    }                  
                }
            }
            return result;
        }

        public static void pipeBiaogao(string unpared)
        {
            
            pipeBiaogaoForm pipeForm = pipeBiaogaoForm.instence();
#if DEBUG
#else
            pipeForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
#endif
            pipeForm.Show();
        }

        public static void drawSupport(string unpard)
        {
            
            string keyIn = "dimstyle active ";
            keyIn += "CERI_引线标注";
            app.CadInputQueue.SendKeyin(keyIn);
            DrawingzhijiaTool tool = new DrawingzhijiaTool();
            tool.InstallTool();
        }

        public static void changeCailiao(string unpard)
        {
            
            #region 启动工具类 只有这样才能调用修改事件
            refreshcailiao refre = new refreshcailiao();
            refre.InstallTool();
            #endregion
        }
    }


    class MyThread
    {
        public BIM.Point3d pt1;
        public BIM.Point3d pt2;
        string filename;
        public MyThread(BIM.Point3d pt1, BIM.Point3d pt2, string filename)
        {
            this.pt1 = pt1;
            this.pt2 = pt2;
            this.filename = filename;
        }
        public void CreateLine()
        {

            Bentley.Interop.MicroStationDGN.ApplicationObjectConnector appParent = new Bentley.Interop.MicroStationDGN.ApplicationObjectConnectorClass();

            Bentley.Interop.MicroStationDGN.Application appnew = appParent.Application;

            System.Threading.Thread.Sleep(15000);

            appnew.Visible = true;


            while (!appnew.IsInitialized) { }


            BIM.DesignFile dgnfile = appnew.OpenDesignFile(filename, false, BIM.MsdV7Action.UpgradeToV8);

            BIM.LineElement line = appnew.CreateLineElement2(null, pt1, pt2);
            dgnfile.DefaultModelReference.AddElement(line);
            dgnfile.Save();
        }
    }
}
