

using Bentley;
using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.Building.Mechanical.Components;
using Bentley.DgnPlatformNET;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Bentley.Internal.MstnPlatformNET;
using System.Reflection;
using Bentley.OpenPlantModeler.SDK.Utilities;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 
    /// </summary>
    public class MyLinearPlacementTool : LinearPlacementTool
    {
        public int index = 0;

        public BMECObject supportBmec = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearTool"></param>
        public MyLinearPlacementTool(MyLinearPlacementTool linearTool) : base(linearTool)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="addIn"></param>
        /// <param name="cmdNumber"></param>
        public MyLinearPlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void _OnDataButton(Bentley.DgnPlatformNET.DgnButtonEvent e)
        {
            if (this.ECObject.ClassName.Equals("PIPE"))
            {
                OpenPlantSymbolProvider.AddOpenPlantSymbolProvider();
                string NamedExpressionName = SymbologyTypeLookup.GetModelSymbology(BMSymbolBasisType.PipingSymbology);
                NamedExpression expressionByName = NamedExpressionManager.GetExpressionByName(NamedExpressionName);
                string text = (string)NamedExpressionManager.EvaluateExpression(expressionByName.Expression, this.ECObject.Component.BMECObject, expressionByName.RequiredSymbolSets);
                string[] array = text.Split(new char[] { ':' });
                string levelName = array[1];

                FileLevelCache level_cache = Session.Instance.GetActiveDgnFile().GetLevelCache();
                LevelHandle level_handle = level_cache.GetLevelByName(levelName);
                if (level_handle.IsValid)
                {
                    Bentley.DgnPlatformNET.Elements.Element elem = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(this.ECObject);
                    ElementPropertiesGetter epg = new ElementPropertiesGetter(elem);
                    LevelDefinitionColor color = new LevelDefinitionColor(epg.Color, elem.DgnModel.GetDgnFile());

                    EditLevelHandle editlevel = level_handle.GetEditHandle();
                    editlevel.SetByLevelColor(color);
                    editlevel.ByLevelWeight = epg.Weight;
                    LevelDefinitionLineStyle ls = new LevelDefinitionLineStyle(0, epg.LineStyle, elem.DgnModel.GetDgnFile());
                    editlevel.SetByLevelLineStyle(ls);
                    level_cache.Write();

                }

            }

            if(index==0)
            {
                index++;
                BMECObject bMECObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(e.Point);
                if (bMECObject != null &&bMECObject.Instance!=null&& BMECApi.Instance.InstanceDefinedAsClass(bMECObject.Instance, "ANCHOR_SERIES", true))
                {
                    try
                    {
                        supportBmec = bMECObject;
                        BMECApi instance = BMECApi.Instance;
                        AutoFittingManager fittingManager = instance.FittingManager;
                        int viewNumber = e.ViewNumber;
                        ComponentHelper instance2 = ComponentHelper.Instance;
                        Bentley.GeometryNET.DPoint3d lastDataPoint = base.LastDataPoint;
                        //bool flag = false;
                        Bentley.GeometryNET.DPoint3d dPoint3d = lastDataPoint;

                        // 指明当前对象
                        object obj = (LinearPlacementTool)this;
                        // 获取对象的类型
                        Type type = obj.GetType();
                        // 对象的父类类型
                        type = type.BaseType;
                        //字段绑定标志
                        BindingFlags flag1 = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;


                        //获取对象的私有方法print 
                        MethodInfo mf = type.GetMethod("ResetStubinLength", flag1);
                        // 实现对象中的方法
                        mf.Invoke(obj, null);

                        // 获取type 中的 私有变量a
                        FieldInfo ff = type.GetField("_origin", flag1);
                        // 给变量赋值
                        ff.SetValue(obj, dPoint3d);

                        // 获取type 中的 私有变量a
                        FieldInfo ff1 = type.GetField("_index", flag1);
                        // 给变量赋值
                        ff1.SetValue(obj, 1);

                        // 读取变量值
                        object value = ff.GetValue(obj);

                        //this._origin.X = dPoint3d;
                        //this._origin.Y = *(ref dPoint3d + 8);
                        //this._origin.Z = *(ref dPoint3d + 16);
                        //this._index++;
                        //base.ElementDescr = null;

                        ECObject.Transform3d = new DTransform3d(DMatrix3d.Identity);

                        this.ECObject.SetDoubleValueFromMM("LENGTH", 0.0);
                        instance.FastenerPlacementUtil.ClearRefComponents();
                        //ConnectionInfo validConnectionInfoAtPoint = instance.GetValidConnectionInfoAtPoint(&dPoint3d);
                        //BMECObject componentObject = validConnectionInfoAtPoint.ComponentObject;
                        //if (null != componentObject)
                        //{
                        //    this._enableValidation = true;
                        //    if (BMdElementType.bmdEquipment == componentObject.ElementType && !this._bLinearSanpOnEquip)
                        //    {
                        //        flag = true;
                        //    }
                        //}
                        //else
                        //{
                        //    this._enableValidation = false;
                        //}
                        //if (!this._enableValidation && validConnectionInfoAtPoint.LocatedObjectsCount > 0)
                        //{
                        //    BasePlacementTool.ShowMessage("Prompt_ForInvalidPlacementLocation", (Bentley.DgnPlatform.OutputMessagePriority)10);
                        //    this.OnRestartCommand();
                        //}

                        base.BeginComplexDynamics();

                        //获取对象的私有方法print 
                        MethodInfo mf1 = type.GetMethod("HideLocatedComponent", flag1);
                        // 实现对象中的方法
                        mf1.Invoke(obj, new object[] { false });

                        //this.HideLocatedComponent(false);
                        BasePlacementTool.ShowPrompt("Prompt_SecondPoint");

                        //获取对象的私有方法print 
                        MethodInfo mf2 = type.GetMethod("CreatePlacementToolState", flag1);
                        // 实现对象中的方法
                        IPlacementToolState tt = (IPlacementToolState)mf2.Invoke(obj, new object[] { viewNumber });

                        base.ToolHistory.AddToolState(/*this.CreatePlacementToolState(viewNumber)*/tt);

                        AccuDraw.Active = true;
                    }
                    catch(Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }                    
                }
                else
                {
                    base._OnDataButton(e);
                    
                }                
            }
            else
            {
                base._OnDataButton(e);

                if (supportBmec != null)
                {
                    bool flag = false;

                    foreach (IECRelationshipInstance item in supportBmec.Instance.GetRelationshipInstances())
                    {
                        if (item.ClassDefinition.Name.Equals("DEVICE_HAS_SUPPORT"))
                        {
                            flag = true;
                        }
                    }

                    if (!flag)
                    {
                        //supportBmec.RelatedInstance = base.ObjectAtOrigin.Instance;
                        supportBmec.Create();
                        OPM_Public_Api.AddDeviceHasSupport(base.ObjectAtOrigin.Instance, supportBmec.Instance);
                    }
                }
            }
                        
            if (base.ObjectAtOrigin != null)
            {
                if (base.ObjectAtOrigin.ClassName.Equals("PIPE"))
                {
                    foreach (BMECObject bmec_object in base.ObjectAtOrigin.ConnectedComponents)
                    {
                        if (bmec_object.ClassName.Equals("EQUAL_PIPE_TEE"))
                        {
                            //通过创建一个于原来的 EQUAL_PIPE_TEE 相同的 ECInstance 来获取 mdb 中对应字段的值
                            try
                            {
                                IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("EQUAL_PIPE_TEE", true);//创建一个ECInstance
                                BMECApi api = BMECApi.Instance;
                                ISpecProcessor specProcessor = api.SpecProcessor;
                                specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
                                elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");//设置管径
                                ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, false);//获取选择数据

                                string standard = string.Empty;
                                if (null != ec_instance_list && ec_instance_list.Count > 0)
                                {
                                    IECInstance instance = ec_instance_list[0];
                                    standard = instance["STANDARD"].StringValue;
                                }
                                bmec_object.Instance["STANDARD"].StringValue = standard;
                                bmec_object.Create();
                                //MessageBox.Show("当前生成的Tee中Standard属性为：" + standard);//TODO 测试完了记得注释掉
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }
                        else if (bmec_object.ClassName.Equals("PIPE"))
                        {
                            Bentley.GeometryNET.DPoint3d new_start_dpt, new_end_dpt, old_start_dpt, old_end_dpt;

                            new_start_dpt = new_end_dpt = old_start_dpt = old_end_dpt = Bentley.GeometryNET.DPoint3d.Zero;
                            JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(base.ObjectAtOrigin, ref new_start_dpt, ref new_end_dpt);
                            JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object, ref old_start_dpt, ref old_end_dpt);

                            DVector3d new_vec = Bentley.GeometryNET.DPoint3d.Subtract(new_start_dpt, new_end_dpt);
                            DVector3d old_vec = Bentley.GeometryNET.DPoint3d.Subtract(old_start_dpt, old_end_dpt);
                            if (new_vec.IsParallelTo(old_vec))
                            {
                                if (MessageBox.Show("是否自动合并管道？", "合并管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    base.ObjectAtOrigin.Instance["LENGTH"].DoubleValue = base.ObjectAtOrigin.GetDoubleValueInMM("LENGTH") + bmec_object.GetDoubleValueInMM("LENGTH");
                                    List<IECInstance> supportIec = clearRelationshipInstance(bmec_object.Instance, "DEVICE_HAS_SUPPORT");
                                    BMECApi.Instance.DeleteFromModel(bmec_object);
                                    JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(base.ObjectAtOrigin, old_start_dpt);
                                    base.ObjectAtOrigin.Create();
                                    base.ObjectAtOrigin.DiscoverConnectionsEx();
                                    base.ObjectAtOrigin.UpdateConnections();
                                    foreach(IECInstance suppIec in supportIec)
                                    {
                                        OPM_Public_Api.AddDeviceHasSupport(base.ObjectAtOrigin.Instance, suppIec);
                                    }
                                }
                            }
                            else if(new_vec.IsParallelOrOppositeTo(old_vec))
                            {
                                if (MessageBox.Show("是否自动合并管道？", "合并管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    //bmecobject.SetLinearPoints(startPoint, endPoint);

                                    base.ObjectAtOrigin.Instance["LENGTH"].DoubleValue = base.ObjectAtOrigin.GetDoubleValueInMM("LENGTH") + bmec_object.GetDoubleValueInMM("LENGTH");

                                    List<IECInstance> supportIec = clearRelationshipInstance(bmec_object.Instance, "DEVICE_HAS_SUPPORT");

                                    BMECApi.Instance.DeleteFromModel(bmec_object);

                                    base.ObjectAtOrigin.SetLinearPoints(new_end_dpt, old_end_dpt);
                                    //JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(base.ObjectAtOrigin, old_start_dpt);
                                    base.ObjectAtOrigin.Create();
                                    base.ObjectAtOrigin.DiscoverConnectionsEx();
                                    base.ObjectAtOrigin.UpdateConnections();

                                    foreach (IECInstance suppIec in supportIec)
                                    {
                                        OPM_Public_Api.AddDeviceHasSupport(base.ObjectAtOrigin.Instance, suppIec);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                BMECObject bm1 = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(this.Origin);
                if (bm1 == null)
                {
                    return;
                }else if(bm1.ClassName.Equals("EQUAL_PIPE_TEE"))
                {
                    //通过创建一个于原来的 EQUAL_PIPE_TEE 相同的 ECInstance 来获取 mdb 中对应字段的值
                    try
                    {
                        foreach (BMECObject bmec_object in bm1.ConnectedComponents)
                        {
                            if (bmec_object.ClassName.Equals("PIPE"))
                            {
                                IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("EQUAL_PIPE_TEE", true);//创建一个ECInstance
                                BMECApi api = BMECApi.Instance;
                                ISpecProcessor specProcessor = api.SpecProcessor;
                                specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
                                elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");//设置管径
                                ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, false);//获取选择数据

                                string standard = string.Empty;
                                if (null != ec_instance_list && ec_instance_list.Count > 0)
                                {
                                    IECInstance instance = ec_instance_list[0];
                                    standard = instance["STANDARD"].StringValue;
                                }
                                bm1.Instance["STANDARD"].StringValue = standard;
                                bm1.Create();
                                break;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                BMECObject bm2 = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(this.LastDataPoint);
                if (bm2 == null)
                {
                    return;
                }
                
                if (bm2.ClassName.Equals("EQUAL_PIPE_TEE"))
                {
                    //通过创建一个于原来的 EQUAL_PIPE_TEE 相同的 ECInstance 来获取 mdb 中对应字段的值
                    try
                    {
                        foreach (BMECObject bmec_object in bm2.ConnectedComponents)
                        {
                            if (bmec_object.ClassName.Equals("PIPE"))
                            {
                                IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("EQUAL_PIPE_TEE", true);//创建一个ECInstance
                                BMECApi api = BMECApi.Instance;
                                ISpecProcessor specProcessor = api.SpecProcessor;
                                specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
                                elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");//设置管径
                                ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, false);//获取选择数据

                                string standard = string.Empty;
                                if (null != ec_instance_list && ec_instance_list.Count > 0)
                                {
                                    IECInstance instance = ec_instance_list[0];
                                    standard = instance["STANDARD"].StringValue;
                                }
                                bm2.Instance["STANDARD"].StringValue = standard;
                                bm2.Create();
                                break;
                            }
                        }

                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public override void CreateTool() { new MyLinearPlacementTool(this).InstallTool(); }
        /// <summary>
        /// 
        /// </summary>
        public override void OnRestartCommand()
        {
            base.RestartCommand(new MyLinearPlacementTool(this));
        }


        public static List<IECInstance> clearRelationshipInstance(IECInstance iec,string relationshipName)
        {
            List<IECInstance> iecList = new List<IECInstance>();

            foreach (IECRelationshipInstance item in iec.GetRelationshipInstances())
            {
                if (item.ClassDefinition.Name.Equals(relationshipName))
                {
                    //flag = true;
                    iecList.Add(item.Target);

                    Bentley.EC.Persistence.ChangeSet changeSet = new Bentley.EC.Persistence.ChangeSet();
                    changeSet.Remove(item);
                    changeSet.MarkDeleted(item);

                    if (null == PersistenceManager.GetInstance())
                    {
                        PersistenceManager.Initialize(Bentley.Plant.Utilities.DgnUtilities.GetInstance().GetDGNConnectionForPipelineManager());
                    }
                    PersistenceManager.GetInstance().CommitChangeSet(changeSet);

                    if (changeSet != null)
                    {
                        changeSet.Dispose();
                    } 
                }
            }

            #region
            //BMECApi arg_1A_0 = BMECApi.Instance;
            //Bentley.ECObjects.Schema.IECSchema schema = BMECInstanceManager.Instance.Schema;
            //if (null == schema)
            //{
            //    return iecList;
            //}
            //Bentley.ECObjects.Schema.IECRelationshipClass[] relationshipClasses = schema.GetRelationshipClasses();
            //int num = 0;
            //if (0 < relationshipClasses.Length)
            //{
            //    Bentley.ECObjects.Schema.IECRelationshipClass iECRelationshipClass;
            //    do
            //    {
            //        iECRelationshipClass = relationshipClasses[num];
            //        if (iECRelationshipClass.Name.Equals(relationshipName))
            //        {
            //            iec.GetRelationshipInstances().Clear(iECRelationshipClass);
            //            BMECObject pipeBmec = new BMECObject(iec);
            //            pipeBmec.Create();
            //            return iecList;
            //        }
            //        num++;
            //    }
            //    while (num < relationshipClasses.Length);
            //}
            #endregion

            return iecList;
        }
    }
}
