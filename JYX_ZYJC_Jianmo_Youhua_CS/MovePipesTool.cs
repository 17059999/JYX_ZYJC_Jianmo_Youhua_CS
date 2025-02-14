﻿
using Bentley.OpenPlant.Modeler.Api;
using Bentley.Building.Mechanical.Components;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using System;
using System.Collections.Generic;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class MovePipesTool : DgnElementSetTool
    {
        //properties
        protected static BMECApi api = BMECApi.Instance;
        protected static Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public static double uorPerMaster = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        public List<BMECObject> pipes = new List<BMECObject>();//选中的要移动的管道
        private int dataButtonCount = 0;//记录点击状态，没有点击，选中管道，选择移动点
        protected string errorMessage = "";
        private DPoint3d startPoint = DPoint3d.Zero;//移动的起点与终点
        private DPoint3d endPoint = DPoint3d.Zero;
        private DVector3d vec = DVector3d.Zero;//移动方向
        private DVector3d dynamicVec = DVector3d.Zero;//动态显示方向
        private PipeConnectTree<BMECObject> PipeRoot;
        private List<PipeConnectTree<BMECObject>> reSizePipes = new List<PipeConnectTree<BMECObject>>();
        private List<PipeConnectTree<BMECObject>> movePipes = new List<PipeConnectTree<BMECObject>>();
        private List<BMECObject> moveOthers = new List<BMECObject>();
        private List<BMECObject> dynamicPipes = new List<BMECObject>();
        private /*static*/ BMECObject selectedComponent;
        private static MovePipesToolForm m_form;
        private bool _hasParallelPipe;
        private enum MOVE_STATE
        {
            SINGLE,
            ALL
        }
        private MOVE_STATE moveState;
        //method
        public static void InstallNewTool()
        {
            MovePipesTool movePipeTool = new MovePipesTool();
            movePipeTool.InstallTool();
        }
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }
        public static void m_formClosed()
        {
            m_form = null;
        }
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            //TODO 动态显示太卡了，操作过多，简化连接性查询
            if (dataButtonCount == 0) return;
            dynamicPipes.Clear();
            moveOthers.Clear();
            if (m_form.checkBox_isWorking.CheckState != System.Windows.Forms.CheckState.Checked)
            {
                dynamicVec = new DVector3d(startPoint, ev.Point);
            }
            else
            {
                double x = 0.0, y = 0.0, z = 0.0;
                try
                {
                    x = Convert.ToDouble(m_form.textBox_X.Text) * uorPerMaster;
                    y = Convert.ToDouble(m_form.textBox_Y.Text) * uorPerMaster;
                    z = Convert.ToDouble(m_form.textBox_Z.Text) * uorPerMaster;
                }
                catch (Exception)
                {
                }
                if (m_form.radioButton_offset.Checked)
                {
                    dynamicVec = new DVector3d(x, y, z);
                }
                else
                {
                    dynamicVec = new DVector3d(x, y, z) - startPoint;
                }
            }
            reSizePipes.Clear();
            movePipes.Clear();
            if (moveState == MOVE_STATE.ALL)
            {
                dynamicPipes = GetSelectedPipingComponentBMECObject();
                if (dynamicPipes != null && dynamicPipes.Count != 0)
                {
                    List<BMECObject> tempdynamicPipes = new List<BMECObject>();
                    foreach (var pipe in dynamicPipes)
                    {
                        BMECObject tempObject = new BMECObject();
                        tempObject.Copy(pipe);
                        DTransform3d tempTransform3d = tempObject.Transform3d;
                        DPoint3d tempLocation = tempTransform3d.Translation;
                        tempTransform3d.Translation = tempLocation + dynamicVec;
                        tempObject.Transform3d = tempTransform3d;
                        tempdynamicPipes.Add(tempObject);
                    }
                    foreach (var pipe in tempdynamicPipes)
                    {
                        JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipe);
                    }
                }
            }
            else
            {
                #region MyRegion
                bool hasParallelPipe = false;
                SearchPipesToReSize(dynamicVec, PipeRoot, ref reSizePipes, ref movePipes, ref hasParallelPipe);
                _hasParallelPipe = hasParallelPipe;
                //超出范围单独移动，只显示移动的管道
                bool isOutRange = false;
                bool isMoveSingle = false;
                foreach (PipeConnectTree<BMECObject> item in movePipes)
                {
                    if (item.isOutrange)
                    {
                        isOutRange = true;
                        break;
                    }
                }
                isMoveSingle = !hasParallelPipe || (hasParallelPipe && isOutRange);
                if (isMoveSingle)
                {
                    BMECObject tempDynamic = new BMECObject();
                    tempDynamic.Copy(selectedComponent);
                    DTransform3d tempTransform3dDynamic = tempDynamic.Transform3d;
                    DPoint3d tempLocationDynamic = tempTransform3dDynamic.Translation;
                    tempTransform3dDynamic.Translation = tempLocationDynamic + dynamicVec;
                    tempDynamic.Transform3d = tempTransform3dDynamic;
                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(tempDynamic);
                    return;
                }

                List<IECInstance> fastenerInstance = new List<IECInstance>();
                foreach (PipeConnectTree<BMECObject> item in movePipes)
                {
                    dynamicPipes.Add(item.Data);
                    List<Port> ports = item.Data.Ports;
                    for (int i = 0; i < ports.Count; i++)
                    {
                        IECInstance portInstance = ports[i].Instance;
                        IECInstance allInformationPortInstance = api.FindAllInformationOnInstance(portInstance);
                        if (null != allInformationPortInstance)
                        {
                            foreach (IECRelationshipInstance current2 in allInformationPortInstance.GetRelationshipInstances())
                            {
                                //port 的连接性
                                if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == allInformationPortInstance.InstanceId)//找到port的相关组件
                                {
                                    System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByDirection(current2.Target, true).GetEnumerator();
                                    while (enumerator3.MoveNext())
                                    {
                                        IECInstance current3 = enumerator3.Current;
                                        fastenerInstance.Add(current3);
                                    }
                                }
                            }
                        }
                    }
                    //执行机构
                    if (api.InstanceDefinedAsClass(item.Data.Instance, "VALVE", true))
                    {
                        IECInstance allInformationPortInstance = api.FindAllInformationOnInstance(item.Data.Instance);
                        if (null != allInformationPortInstance)
                        {
                            foreach (IECRelationshipInstance current2 in allInformationPortInstance.GetRelationshipInstances())
                            {
                                if (current2.ClassDefinition.Name.Equals("VALVE_HAS_VALVE_OPERATING_DEVICE") && current2.Source.InstanceId == allInformationPortInstance.InstanceId)
                                {
                                    IECInstance current3 = current2.Target;
                                    fastenerInstance.Add(current3);
                                }
                            }
                        }
                    }
                }
                foreach (IECInstance item in fastenerInstance)
                {
                    BMECObject tempObj = new BMECObject(item);
                    if (!BMECApi.Instance.ObjectContained(tempObj, dynamicPipes))
                    {
                        dynamicPipes.Add(tempObj);
                        moveOthers.Add(tempObj);
                    }
                }
                if (dynamicPipes != null && dynamicPipes.Count != 0)
                {
                    List<BMECObject> tempdynamicPipes = new List<BMECObject>();
                    foreach (var pipe in dynamicPipes)
                    {
                        BMECObject tempObject = new BMECObject();
                        tempObject.Copy(pipe);
                        DTransform3d tempTransform3d = tempObject.Transform3d;
                        DPoint3d tempLocation = tempTransform3d.Translation;
                        tempTransform3d.Translation = tempLocation + dynamicVec;
                        tempObject.Transform3d = tempTransform3d;
                        tempdynamicPipes.Add(tempObject);
                    }
                    foreach (var pipe in tempdynamicPipes)
                    {
                        JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipe);
                    }
                }
                #endregion
            }
        }
        protected override void OnRestartTool()
        {
            InstallNewTool();
        }
        protected override void OnCleanup()
        {
            if (m_form == null) return;
#if DEBUG
            m_form.Hide();
#else
            m_form.Hide();
            m_form.DetachFromMicroStation();
#endif
            base.OnCleanup();
        }
        protected override void OnPostInstall()
        {
            //TODO 启动命令之前选中，整体移动，否则按现有逻辑移动

            base.OnPostInstall();
            dynamicPipes.Clear();
            pipes.Clear();
            if (m_form == null)
            {
                m_form = new MovePipesToolForm();
            }
#if DEBUG
#else
            m_form.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MovePipesToolForm));
            m_form.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            m_form.Show();
            app.ShowCommand("移动管道");
            if (GetSelectedPipingComponentBMECObject().Count > 0)
            {
                moveState = MOVE_STATE.ALL;
                app.ShowPrompt("请选择移动的起点");
            }
            else
            {
                moveState = MOVE_STATE.SINGLE;
                app.ShowPrompt("请选择需要移动的管道");
            }
        }
        /// <summary>
        /// 获取当前选中的管道相关，目前已知的有 PIPING_COMPONENT、FASTENER、SEAL
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
                    if (BMECApi.Instance.InstanceDefinedAsClass(ec_object.Instance, "PIPING_COMPONENT", true) || BMECApi.Instance.InstanceDefinedAsClass(ec_object.Instance, "FASTENER", true) || BMECApi.Instance.InstanceDefinedAsClass(ec_object.Instance, "SEAL", true))
                    {
                        result.Add(ec_object);
                    }
                    //加支吊架
                    IECInstance iec_object = JYX_ZYJC_CLR.PublicMethod.FindInstance(element);//通过Element 获取 IECInstance
                    if (iec_object != null)
                    {
                        bool isPipe = api.InstanceDefinedAsClass(iec_object, "PIPE", true); //判断是否为管道 以及他的子类
                        if(isPipe)
                        {
                            foreach (IECRelationshipInstance item in ec_object.Instance.GetRelationshipInstances())
                            {
                                if (item.ClassDefinition.Name == "DEVICE_HAS_SUPPORT")
                                {
                                    result.Add(api.CreateBMECObjectForInstance(item.Target));
                                }
                            }
                        }
                        
                    }
                    
                }
            }
            return result;
        }
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            app.CommandState.StartDefaultCommand();
            return true;
        }
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            if (moveState == MOVE_STATE.ALL)
            {
                if (dataButtonCount == 0)
                {
                    startPoint = ev.Point;
                    app.ShowPrompt("请选择移动的终点");
                    dataButtonCount = 1;
                    BeginDynamics();
                }
                else
                {
                    endPoint = ev.Point;
                    vec = new DVector3d(startPoint, endPoint);
                    try
                    {
                        movePipesWithVector(GetSelectedPipingComponentBMECObject(), vec);
                        System.Windows.Forms.MessageBox.Show("移动成功");
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    finally
                    {
                        dataButtonCount = 0;
                        InstallNewTool();
                    }
                }
            }
            else
            {
                #region MyRegion
                if (dataButtonCount == 0)//选择移动的组件
                {
                    HitPath hitPath = DoLocate(ev, true, 0);
                    if (hitPath == null)
                    {
                        return false;
                    }
                    Element element = hitPath.GetHeadElement();
                    selectedComponent = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
                    pipes.Clear();
                    //pipes.AddRange(GetSelectedPipingComponentBMECObject());
                    //if (!BMECApi.Instance.ObjectContained(selectedComponent, pipes))
                    //{
                    //    pipes.Clear();
                    //}
                    PipeRoot = CreateTreeWithConnectedBMECObject(selectedComponent, element, pipes);
                    startPoint = ev.Point;
                    dataButtonCount = 1;
                    app.ShowCommand("移动管道");
                    app.ShowPrompt("请确定移动的终点");
                    BeginDynamics();
                }
                else//移动
                {
                    endPoint = ev.Point;
                    if (m_form.checkBox_isWorking.CheckState != System.Windows.Forms.CheckState.Checked)
                    {
                        vec = new DVector3d(startPoint, endPoint);
                    }
                    else
                    {
                        double x = 0.0, y = 0.0, z = 0.0;
                        try
                        {
                            x = Convert.ToDouble(m_form.textBox_X.Text) * uorPerMaster;
                            y = Convert.ToDouble(m_form.textBox_Y.Text) * uorPerMaster;
                            z = Convert.ToDouble(m_form.textBox_Z.Text) * uorPerMaster;
                        }
                        catch (Exception)
                        {
                        }
                        if (m_form.radioButton_offset.Checked)
                        {
                            vec = new DVector3d(x, y, z);
                        }
                        else
                        {
                            vec = new DVector3d(x, y, z) - startPoint;
                        }
                    }
                    //EndDynamics();
                    bool isMovePipe = true;
                    //移动超出范围整体移动
                    //foreach (PipeConnectTree<BMECObject> item in movePipes)
                    //{
                    //    if (item.isOutrange)
                    //    {
                    //        isMovePipe = false;
                    //        errorMessage = "移动超出范围，是否移动?";
                    //        if (System.Windows.Forms.MessageBox.Show(errorMessage, "移动管道", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    //        {
                    //            isMovePipe = true;
                    //            break;
                    //        }
                    //    }
                    //}
                    //移动超出范围单独移动
                    if (!_hasParallelPipe)
                    {
                        isMovePipe = false;
                        errorMessage = "与选择的管道相连的管道组上没有找到方向相同的管道，是否单独移动该管道?";
                        if (System.Windows.Forms.MessageBox.Show(errorMessage, "移动管道", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            //TODO 单独移动管道
                            try
                            {
                                movePipesWithVector(new List<BMECObject>() { this.selectedComponent }, vec);
                                errorMessage = "移动成功";
                                System.Windows.Forms.MessageBox.Show(errorMessage);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            finally
                            {
                                dataButtonCount = 0;
                                InstallNewTool();
                            }
                        }
                    }
                    else
                    {
                        foreach (PipeConnectTree<BMECObject> item in movePipes)
                        {
                            if (item.isOutrange)
                            {
                                isMovePipe = false;
                                errorMessage = "移动超出范围，是否移动?";
                                if (System.Windows.Forms.MessageBox.Show(errorMessage, "移动管道", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    //TODO 单独移动管道
                                    try
                                    {
                                        movePipesWithVector(new List<BMECObject>() { this.selectedComponent }, vec);
                                        errorMessage = "移动成功";
                                        System.Windows.Forms.MessageBox.Show(errorMessage);
                                    }
                                    catch (Exception)
                                    {
                                        throw;
                                    }
                                    finally
                                    {
                                        dataButtonCount = 0;
                                        InstallNewTool();
                                    }
                                    break;
                                }
                                //else
                                //{
                                //    //dataButtonCount = 0;
                                //    return true;
                                //}
                            }
                        }
                    }
                    if (isMovePipe)
                    {
                        //去重
                        List<BMECObject> tempMovePipes = new List<BMECObject>();
                        List<BMECObject> tempReSizePipes = new List<BMECObject>();
                        foreach (PipeConnectTree<BMECObject> item in movePipes)
                        {
                            tempMovePipes.Add(item.Data);
                        }
                        foreach (PipeConnectTree<BMECObject> item in reSizePipes)
                        {
                            tempReSizePipes.Add(item.Data);
                        }
                        for (int i = 0; i < tempMovePipes.Count; i++)
                        {
                            if (api.ObjectContained(tempMovePipes[i], tempReSizePipes))
                            {
                                movePipes.RemoveAt(i);
                                tempMovePipes.RemoveAt(i);
                                i--;
                            }
                        }
                        MovePipe(vec, out errorMessage, ref movePipes, ref reSizePipes, moveOthers);
                        errorMessage = "移动成功";
                        System.Windows.Forms.MessageBox.Show(errorMessage);
                        dataButtonCount = 0;
                        InstallNewTool();
                    }
                    //else
                    //{
                    //    return false;
                    //}
                }
                #endregion
            }
            return true;
        }
        /// <summary>
        /// TODO 临时先用着，获取整个管道线的管道
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static List<BMECObject> FilterPipe(List<BMECObject> filterpipes)
        {
            if (filterpipes == null) return null;
            List<BMECObject> result = new List<BMECObject>();
            foreach (BMECObject ecobject in filterpipes)
            {
                if (ecobject != null && ecobject.Instance != null)
                {
                    if (api.InstanceDefinedAsClass(ecobject.Instance, "PIPE", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_ELBOW", true) ||
                        api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_BEND", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_BRANCH", true) ||
                        api.InstanceDefinedAsClass(ecobject.Instance, "FLUID_REGULATOR", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_FLANGE", true) ||
                        api.InstanceDefinedAsClass(ecobject.Instance, "VALVE_OPERATING_DEVICE", true)
                        )//TODO 类型为管道 或 执行机构
                    {
                        result.Add(ecobject);
                    }
                }
            }
            return result;
        }
        /// <summary>
        /// 获取选中的管道
        /// TODO 和动态显示合并
        /// </summary>
        /// <returns></returns>
        public static List<BMECObject> getSelectedPipes()
        {
            List<BMECObject> selectedpipes = new List<BMECObject>();//选中的管道的容器
            List<BMECObject> tempECObject = new List<BMECObject>();//临时容器
            ElementAgenda elementAgenda = new ElementAgenda();//选中Element的集合
            SelectionSetManager.BuildAgenda(ref elementAgenda);//获取选中的元素的集合
            for (uint i = 0; i < elementAgenda.GetCount(); i++)//获取选中 Element 的 BMECObject
            {
                Element element = elementAgenda.GetEntry(i);
                BMECObject ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);//通过ElementId 获取 BMECObject
                tempECObject.Add(ec_object);
            }
            if (tempECObject.Count > 0)
            {
                foreach (var ecobject in tempECObject)//将选中的类型为 PIPE 的 BMECObject 存到 selectedpipes 容器中
                {
                    if (ecobject != null && ecobject.Instance != null)
                    {
                        if (api.InstanceDefinedAsClass(ecobject.Instance, "PIPE", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_ELBOW", true) ||
                            api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_BEND", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_BRANCH", true) ||
                            api.InstanceDefinedAsClass(ecobject.Instance, "FLUID_REGULATOR", true) || api.InstanceDefinedAsClass(ecobject.Instance, "PIPE_FLANGE", true) ||
                            api.InstanceDefinedAsClass(ecobject.Instance, "VALVE_OPERATING_DEVICE", true)
                            )//TODO 类型为管道即可
                        {
                            selectedpipes.Add(ecobject);
                        }
                    }
                }
            }
            return selectedpipes;
        }
        //public static bool movePipe(List<BMECObject> pipes, DVector3d vec, out string errorMessage)
        //{
        //    //TODO 处理单根管道前两边连接的为非直管的情况
        //    errorMessage = "";
        //    if (pipes == null)
        //    {
        //        errorMessage = "获取需要移动的管道异常";
        //        return false;
        //    }
        //    //这样的话，需要先扫描并整理所有连接的组件与该组件是哪个端口与选中的需要移动的组件相连接的，当外部连接组件是非管道时，需要操作自己这边的组件，否则操作外部连接组件？
        //    List<BMECObject> bianyuanlianjieguandao = new List<BMECObject>();//与外部有连接的管道
        //    List<int> bianyuanPortsIndex = new List<int>();//与外部有连接的管道的连接端口
        //    List<double> bianyuanguandaoLength = new List<double>();
        //    List<DPoint3d> bianyuanguandaoLocation = new List<DPoint3d>();
        //    List<BMECObject> waibulianjieguandao = new List<BMECObject>();//外部连接边缘的管道
        //    List<int> waibuPortsIndex = new List<int>();//外部连接组件对应的连接端口
        //    List<double> waibuguadnaoLength = new List<double>();
        //    List<DPoint3d> waibuguandaoLocation = new List<DPoint3d>();
        //    List<DVector3d> directions = new List<DVector3d>();
        //    //拿到与外部有连接的成对管道
        //    foreach (var pipe in pipes)
        //    {
        //        if (api.InstanceDefinedAsClass(pipe.Instance, "VALVE_OPERATING_DEVICE", true))
        //        {
        //            continue;
        //        }
        //        //List<BMECObject> tempConnectedPipes = new List<BMECObject>();
        //        List<Port> tempConnectedPipesPorts = pipe.Ports;
        //        for (int i = 0; i < tempConnectedPipesPorts.Count; i++)
        //        {
        //            //Port tempInitialPort = pipe.GetNthPort(i);//单纯是想初始化 port
        //            BMECObject tempConnectedComponent = pipe.GetConnectedComponentAtPort(tempConnectedPipesPorts[i].ID);

        //            DPoint3d tempInitialPipePortsLocation = pipe.GetNthPort(i).LocationInUors;//单纯初始化

        //            if (tempConnectedComponent == null || tempConnectedComponent.Instance == null) continue;
        //            if (!api.ObjectContained(tempConnectedComponent, pipes))
        //            {
        //                bianyuanlianjieguandao.Add(pipe);//将对外部有连接的管道添加进容器中
        //                bianyuanPortsIndex.Add(tempConnectedPipesPorts[i].ID);//记录该连接端口ID
        //                //判断外部连接管道哪个端口连接的
        //                List<Port> tempConnectedComponentPorts = tempConnectedComponent.Ports;
        //                for (int j = 0; j < tempConnectedComponentPorts.Count; j++)
        //                {
        //                    //Port tempInitialPort2 = tempConnectedComponent.GetNthPort(j);
        //                    BMECObject tempConnectedComponent1 = tempConnectedComponent.GetConnectedComponentAtPort(tempConnectedComponentPorts[j].ID);

        //                    DPoint3d tempInitialConnectedComponentPortsLocation = tempConnectedComponent.GetNthPort(j).LocationInUors;//单纯的初始化

        //                    if (tempConnectedComponent1 == null || tempConnectedComponent.Instance == null) continue;
        //                    if (api.ObjectContained(tempConnectedComponent1, pipes))
        //                    {
        //                        //将外部有连接的管道添加到容器并记录连接端口ID
        //                        waibulianjieguandao.Add(tempConnectedComponent);
        //                        waibuPortsIndex.Add(tempConnectedComponentPorts[j].ID);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    //看与外部连接的管道方向是否一致判断是否可移动
        //    int icount = 0;
        //    foreach (var pipe in bianyuanlianjieguandao)
        //    {
        //        int pindex = bianyuanPortsIndex[icount];
        //        //DPoint3d tempinitPortLocation = pipe.GetNthPort(pindex - 1).LocationInUors;//单纯的想初始化
        //        DVector3d direction = pipe.Ports[pindex - 1].Direction;
        //        directions.Add(direction);
        //        icount++;
        //    }
        //    DVector3d tempDir = DVector3d.Zero;
        //    if (directions.Count > 0)
        //    {
        //        tempDir = directions[0];
        //    }
        //    else
        //    {
        //        //只选中了单根管道
        //        //errorMessage = "无法移动，没有可供移动的方向选择";
        //        //return false;
        //        List<BMECObject> dangenguandao = movePipesWithVector(pipes, vec);
        //        foreach (var pipe in dangenguandao)
        //        {
        //            pipe.DiscoverConnectionsEx();
        //            pipe.UpdateConnections();
        //        }
        //        return true;
        //    }

        //    //TODO 先随便改下用着
        //    bool DirectionsIsnAllParallel = true;//组件群的外部连接组件允许的移动方向是否一致，false : 不一致 true : 一致
        //    foreach (var dir in directions)
        //    {
        //        //如果没有顺着的管道则直接移动
        //        if (!vec.IsParallelOrOppositeTo(dir))
        //        {
        //            //errorMessage = "该组件群的外部连接组件允许的移动方向不一致，无法对当前组件群进行移动";
        //            //return false;

        //            //List<BMECObject> newPipes = movePipesWithVector(pipes, vec);
        //            //foreach (var pipe in newPipes)
        //            //{
        //            //    pipe.DiscoverConnectionsEx();
        //            //    pipe.UpdateConnections();
        //            //}
        //            //return true;

        //            DirectionsIsnAllParallel = false;
        //        }
        //    }

        //    //如果没有顺着的管道则直接移动
        //    if (!DirectionsIsnAllParallel)
        //    {
        //        //找顺着方向的
        //        List<int> PipesIndexWhichParalleOrOppositeToVec = new List<int>();
        //        for (int i = 0; i < directions.Count; i++)
        //        {
        //            if (vec.IsParallelOrOppositeTo(directions[i]))
        //            {
        //                PipesIndexWhichParalleOrOppositeToVec.Add(i);
        //            }
        //        }
        //        //处理顺着方向的管道
        //        int feipingxingIndex = 0;
        //        DVector3d tempvectorfeipingxing = DVector3d.Zero;
        //        DPoint3d tempLocationfeipingxing = DPoint3d.Zero;
        //        double tempLengthfeipingxing = 0.0;
        //        for (int i = 0; i < PipesIndexWhichParalleOrOppositeToVec.Count; i++)
        //        {
        //            feipingxingIndex = PipesIndexWhichParalleOrOppositeToVec[i];
        //            //要么外部连接是管道，要么内部连接是管道
        //            if (api.InstanceDefinedAsClass(waibulianjieguandao[feipingxingIndex].Instance, "PIPE", true))
        //            {
        //                int pindex = waibuPortsIndex[feipingxingIndex];
        //                //从连接的端口指向非连接端口
        //                if (pindex == 1)//main -> run
        //                {
        //                    //DPoint3d initwaibulianjiePortPoint = waibulianjieguandao[i].GetNthPort(1).LocationInUors;
        //                    tempvectorfeipingxing = new DVector3d(waibulianjieguandao[feipingxingIndex].Ports[0].Location, waibulianjieguandao[feipingxingIndex].Ports[1].Location);

        //                    tempLengthfeipingxing = (tempvectorfeipingxing - vec).Magnitude / uorPerMaster;
        //                    tempLocationfeipingxing = waibulianjieguandao[feipingxingIndex].Transform3d.Translation + vec;
        //                }
        //                else if (pindex == 2)//run -> main
        //                {
        //                    //DPoint3d initwaibulianjiePortPoint = waibulianjieguandao[i].GetNthPort(0).LocationInUors;
        //                    tempvectorfeipingxing = new DVector3d(waibulianjieguandao[feipingxingIndex].Ports[1].Location, waibulianjieguandao[feipingxingIndex].Ports[0].Location);

        //                    tempLengthfeipingxing = (tempvectorfeipingxing - vec).Magnitude / uorPerMaster;
        //                    tempLocationfeipingxing = DPoint3d.Zero;
        //                }
        //                if (vec.IsParallelTo(tempvectorfeipingxing))//同向需判断移动范围，反向不用管
        //                {
        //                    if (vec.IsParallelTo(vec - tempvectorfeipingxing))
        //                    {
        //                        //TODO 暂时是这样的，之后需要改成控制了范围，用户随意选点，但只会移动到最大范围
        //                        errorMessage = "移动超出范围";
        //                        return false;
        //                    }
        //                }
        //            }
        //            else if (api.InstanceDefinedAsClass(bianyuanlianjieguandao[feipingxingIndex].Instance, "PIPE", true))
        //            {
        //                int pindex = bianyuanPortsIndex[feipingxingIndex];
        //                if (pindex == 1)//main -> run
        //                {
        //                    //DPoint3d initbianyuanlianjiePortPoint = bianyuanlianjieguandao[i].GetNthPort(1).LocationInUors;
        //                    tempvectorfeipingxing = new DVector3d(bianyuanlianjieguandao[feipingxingIndex].Ports[0].Location, bianyuanlianjieguandao[feipingxingIndex].Ports[1].Location);

        //                    tempLengthfeipingxing = (tempvectorfeipingxing + vec).Magnitude / uorPerMaster;
        //                    tempLocationfeipingxing = bianyuanlianjieguandao[feipingxingIndex].Transform3d.Translation;
        //                }
        //                else if (pindex == 2)//run -> main
        //                {
        //                    //DPoint3d initbianyuanlianjiePortPoint = bianyuanlianjieguandao[i].GetNthPort(0).LocationInUors;
        //                    tempvectorfeipingxing = new DVector3d(bianyuanlianjieguandao[feipingxingIndex].Ports[1].Location, bianyuanlianjieguandao[feipingxingIndex].Ports[0].Location);

        //                    tempLengthfeipingxing = (tempvectorfeipingxing + vec).Magnitude / uorPerMaster;
        //                    tempLocationfeipingxing = DPoint3d.Zero;
        //                }
        //                if (!vec.IsParallelTo(tempvectorfeipingxing))//反向需判断移动范围，同向不用管
        //                {
        //                    if (vec.IsParallelTo(vec + tempvectorfeipingxing))
        //                    {
        //                        //TODO 暂时是这样的，之后需要改成控制了范围，用户随意选点，但只会移动到最大范围
        //                        errorMessage = "移动超出范围";
        //                        return false;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                errorMessage = "检测到连接异常，存在非直管连续连接";
        //                return false;
        //            }
        //            //记录长度变化，位置变化，没有赋值过的就是跳过的，默认为0
        //            waibuguandaoLocation.Add(tempLocationfeipingxing);
        //            waibuguadnaoLength.Add(tempLengthfeipingxing);
        //            bianyuanguandaoLocation.Add(tempLocationfeipingxing);
        //            bianyuanguandaoLength.Add(tempLengthfeipingxing);

        //        }
        //        //记录完了移动管道
        //        List<BMECObject> movePipesfeipingxing = movePipesWithVector(pipes, vec);
        //        foreach (var pipe in movePipesfeipingxing)
        //        {
        //            pipe.Create();
        //            pipe.DiscoverConnectionsEx();
        //            pipe.UpdateConnections();
        //        }
        //        //修改边缘连接管道的长度并更新连结性

        //        DVector3d pipeVectorfeipingxing = DVector3d.Zero;
        //        double pipeLengthfeipingxing = 0.0;
        //        for (int i = 0; i < PipesIndexWhichParalleOrOppositeToVec.Count; i++)
        //        {
        //            int feipingxingIndex2 = PipesIndexWhichParalleOrOppositeToVec[i];
        //            if (api.InstanceDefinedAsClass(waibulianjieguandao[feipingxingIndex2].Instance, "PIPE", true))
        //            {

        //                pipeLengthfeipingxing = waibuguadnaoLength[i];
        //                waibulianjieguandao[feipingxingIndex2].SetDoubleValue("LENGTH", pipeLengthfeipingxing);
        //                if (!(waibuguandaoLocation[i] == DPoint3d.Zero))
        //                {
        //                    api.TranslateComponent(waibulianjieguandao[feipingxingIndex2], waibuguandaoLocation[i]);
        //                }
        //                waibulianjieguandao[feipingxingIndex2].Create();
        //                waibulianjieguandao[feipingxingIndex2].DiscoverConnectionsEx();
        //                waibulianjieguandao[feipingxingIndex2].UpdateConnections();
        //            }
        //            else
        //            {
        //                if (api.InstanceDefinedAsClass(bianyuanlianjieguandao[feipingxingIndex2].Instance, "PIPE", true))
        //                {
        //                    pipeLengthfeipingxing = bianyuanguandaoLength[i];
        //                    bianyuanlianjieguandao[feipingxingIndex2].SetDoubleValue("LENGTH", pipeLengthfeipingxing);
        //                    if (!(bianyuanguandaoLocation[i] == DPoint3d.Zero))
        //                    {
        //                        api.TranslateComponent(bianyuanlianjieguandao[feipingxingIndex2], bianyuanguandaoLocation[i]);
        //                    }
        //                    bianyuanlianjieguandao[feipingxingIndex2].Create();
        //                    bianyuanlianjieguandao[feipingxingIndex2].DiscoverConnectionsEx();
        //                    bianyuanlianjieguandao[feipingxingIndex2].UpdateConnections();
        //                }
        //            }
        //        }
        //        return true;
        //    }

        //    //将用户鼠标指示的移动方向投影到可选的移动方向上
        //    //求投影点，通过投影点构造新的向量
        //    //double projectVectorMag =  vec.DotProduct(tempDir) / tempDir.Magnitude;
        //    //Angle vectorTovectorAngle = vec.AngleTo(tempDir);
        //    //DVector3d vector = DVector3d.Zero;
        //    //double magnitude = 0.0;
        //    //if (tempDir.TryScaleToLength(projectVectorMag, out vector, out magnitude))
        //    //{
        //    //    vec = vector;
        //    //}

        //    //可以移动，做后续处理
        //    //遍历外部连接组件，当不是直管时，就要试图改变内部连接组件的长度，否则改变外部连接组件的长度
        //    //即，外部连接组件是直管，判断外部的移动允许位移大小，否则判断内部可移动位移大小
        //    DVector3d minMoveVectorZheng = DVector3d.Zero;
        //    DVector3d minMoveVectorFu = DVector3d.Zero;
        //    DVector3d tempvector = DVector3d.Zero;
        //    DPoint3d tempLocation = DPoint3d.Zero;
        //    double tempLength = 0.0;
        //    for(int i = 0; i < waibulianjieguandao.Count; i++)
        //    {
        //        //要么外部连接组件是管道，要么内部连接组件是管道，如果内外连接没有一个是管道则不合法？
        //        if (waibulianjieguandao[i].ClassName == "PIPE")
        //        {
        //            int pindex = waibuPortsIndex[i];
        //            //从连接的端口指向非连接端口
        //            if (pindex == 1)//main -> run
        //            {
        //                //DPoint3d initwaibulianjiePortPoint = waibulianjieguandao[i].GetNthPort(1).LocationInUors;
        //                tempvector = new DVector3d(waibulianjieguandao[i].Ports[0].Location, waibulianjieguandao[i].Ports[1].Location);

        //                tempLength = (tempvector - vec).Magnitude / uorPerMaster;
        //                tempLocation = waibulianjieguandao[i].Transform3d.Translation + vec;
        //            }
        //            else if (pindex == 2)//run -> main
        //            {
        //                //DPoint3d initwaibulianjiePortPoint = waibulianjieguandao[i].GetNthPort(0).LocationInUors;
        //                tempvector = new DVector3d(waibulianjieguandao[i].Ports[1].Location, waibulianjieguandao[i].Ports[0].Location);

        //                tempLength = (tempvector - vec).Magnitude / uorPerMaster;
        //                tempLocation = DPoint3d.Zero;
        //            }
        //            if (vec.IsParallelTo(tempvector))//同向需判断移动范围，反向不用管
        //            {
        //                if (vec.IsParallelTo(vec - tempvector))
        //                {
        //                    //TODO 暂时是这样的，之后需要改成控制了范围，用户随意选点，但只会移动到最大范围
        //                    errorMessage = "移动超出范围";
        //                    return false;
        //                }
        //            }
        //        }
        //        else if (bianyuanlianjieguandao[i].ClassName == "PIPE")//否则内部就必须是管道
        //        {
        //            int pindex = bianyuanPortsIndex[i];
        //            if (pindex == 1)//main -> run
        //            {
        //                //DPoint3d initbianyuanlianjiePortPoint = bianyuanlianjieguandao[i].GetNthPort(1).LocationInUors;
        //                tempvector = new DVector3d(bianyuanlianjieguandao[i].Ports[0].Location, bianyuanlianjieguandao[i].Ports[1].Location);

        //                tempLength = (tempvector + vec).Magnitude / uorPerMaster;
        //                tempLocation = bianyuanlianjieguandao[i].Transform3d.Translation;
        //            }
        //            else if (pindex == 2)//run -> main
        //            {
        //                //DPoint3d initbianyuanlianjiePortPoint = bianyuanlianjieguandao[i].GetNthPort(0).LocationInUors;
        //                tempvector = new DVector3d(bianyuanlianjieguandao[i].Ports[1].Location, bianyuanlianjieguandao[i].Ports[0].Location);

        //                tempLength = (tempvector + vec).Magnitude / uorPerMaster;
        //                tempLocation = DPoint3d.Zero;
        //            }
        //            if (!vec.IsParallelTo(tempvector))//反向需判断移动范围，同向不用管
        //            {
        //                if (vec.IsParallelTo(vec + tempvector))
        //                {
        //                    //TODO 暂时是这样的，之后需要改成控制了范围，用户随意选点，但只会移动到最大范围
        //                    errorMessage = "移动超出范围";
        //                    return false;
        //                }
        //            }
        //        }
        //        else
        //        {
        //            errorMessage = "检测到连接异常，存在非直管连续连接";
        //            return false;
        //        }
        //        //记录长度变化，位置变化，没有赋值过的就是跳过的，默认为0
        //        waibuguandaoLocation.Add(tempLocation);
        //        waibuguadnaoLength.Add(tempLength);
        //        bianyuanguandaoLocation.Add(tempLocation);
        //        bianyuanguandaoLength.Add(tempLength);
        //    }
        //    //但上面漏了一种情况：选中的只有一根管道，且两端连接的为非直管时
        //    //过滤这种情况
        //    if (waibulianjieguandao.Count == 2 && api.ObjectContained(bianyuanlianjieguandao[0], new List<BMECObject>() { bianyuanlianjieguandao[1]}))
        //    {
        //        //暂时过滤掉这种情况
        //        //errorMessage = "该移动不会产生任何效果";
        //        //return false;
        //        //double maxMoveDis = bianyuanlianjieguandao[0].GetDoubleValueInMM("LENGTH");
        //        //double uorPerMaster = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        //        //if (vec.Magnitude >= maxMoveDis * uorPerMaster)
        //        //{
        //        //    errorMessage = "移动超出范围";
        //        //    return false;
        //        //}
        //        List<BMECObject> movePipesyigenguandao = movePipesWithVector(pipes, vec);
        //        foreach (var pipe in movePipesyigenguandao)
        //        {
        //            pipe.Create();
        //            pipe.DiscoverConnectionsEx();
        //            pipe.UpdateConnections();
        //        }
        //        return true;
        //    }
        //    List<BMECObject> movePipes = movePipesWithVector(pipes, vec);
        //    foreach (var pipe in movePipes)
        //    {
        //        pipe.Create();
        //        pipe.DiscoverConnectionsEx();
        //        pipe.UpdateConnections();
        //    }
        //    //修改边缘连接管道的长度并更新连结性

        //    DVector3d pipeVector = DVector3d.Zero;
        //    double pipeLength = 0.0;
        //    for (int i = 0; i < waibulianjieguandao.Count; i++)
        //    {
        //        if (api.InstanceDefinedAsClass(waibulianjieguandao[i].Instance, "PIPE", true))
        //        {

        //            pipeLength = waibuguadnaoLength[i];
        //            waibulianjieguandao[i].SetDoubleValue("LENGTH", pipeLength);
        //            if (!(waibuguandaoLocation[i] == DPoint3d.Zero))
        //            {
        //                api.TranslateComponent(waibulianjieguandao[i], waibuguandaoLocation[i]);
        //            }
        //            waibulianjieguandao[i].Create();
        //            waibulianjieguandao[i].DiscoverConnectionsEx();
        //            waibulianjieguandao[i].UpdateConnections();
        //        }
        //        else
        //        {
        //            if (api.InstanceDefinedAsClass(bianyuanlianjieguandao[i].Instance, "PIPE", true))
        //            {
        //                pipeLength = bianyuanguandaoLength[i];
        //                bianyuanlianjieguandao[i].SetDoubleValue("LENGTH", pipeLength);
        //                if (!(bianyuanguandaoLocation[i] == DPoint3d.Zero))
        //                {
        //                    api.TranslateComponent(bianyuanlianjieguandao[i], bianyuanguandaoLocation[i]);
        //                }
        //                bianyuanlianjieguandao[i].Create();
        //                bianyuanlianjieguandao[i].DiscoverConnectionsEx();
        //                bianyuanlianjieguandao[i].UpdateConnections();
        //            }
        //        }
        //    }

        //    return true;
        //}
        public static bool MovePipe(DVector3d vec, out string errorMessage, ref List<PipeConnectTree<BMECObject>> movedComponent, ref List<PipeConnectTree<BMECObject>> resizeComponent, List<BMECObject> moveOthers)
        {
            errorMessage = "";
            List<BMECObject> movedPipes = new List<BMECObject>();
            foreach (PipeConnectTree<BMECObject> item in movedComponent)
            {
                movedPipes.Add(item.Data);
            }
            movedPipes = movePipesWithVector(movedPipes, vec);
            double temlen = 0.0;
            double vecMani = vec.Magnitude / uorPerMaster;

            foreach (PipeConnectTree<BMECObject> item in resizeComponent)
            {
                temlen = item.Data.GetDoubleValueInMM("LENGTH");
                if (item.direction == 1)//方向与移动方向相同
                {
                    //缩短后再平移缩短的长度
                    temlen -= vecMani;
                }
                else
                {
                    //直接伸长
                    temlen += vecMani;
                }
                if (item.fluidDir == 1 && item.direction == 1 || item.fluidDir == 2 && item.direction == 2)
                {
                    DPoint3d location = DPoint3d.Zero;
                    location = item.Data.Transform3d.Translation;
                    location += vec;
                    api.TranslateComponent(item.Data, location);
                }
                item.Data.SetDoubleValueFromMM("LENGTH", temlen);
                item.Data.Create();
            }

            movePipesWithVector(moveOthers, vec);
            //update connection
            foreach (BMECObject item in movedPipes)
            {
                item.DiscoverConnectionsEx();
                item.UpdateConnections();
            }
            foreach (PipeConnectTree<BMECObject> item in resizeComponent)
            {
                item.Data.DiscoverConnectionsEx();
                item.Data.UpdateConnections();
            }
            return true;
        }
        /// <summary>
        /// 给定一个向量，对管道群进行移动
        /// 此方法不会过滤移动是否合法
        /// </summary>
        /// <param name="pipes"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static List<BMECObject> movePipesWithVector(List<BMECObject> pipes, DVector3d dir)
        {
            DPoint3d location = DPoint3d.Zero;
            foreach (var pipe in pipes)
            {
                location = pipe.Transform3d.Translation;
                location += dir;
                api.TranslateComponent(pipe, location);
                pipe.Create();
            }
            return pipes;
        }
        /// <summary>
        /// TODO 支持点选任意组件，不仅是管道
        /// </summary>
        /// <param name="ecobject"></param>
        /// <param name="connectPipesInPipeline"></param>
        /// <returns></returns>
        public static bool SelectPipesOfPipeLineWithPipe(BMECObject ecobject, out List<BMECObject> connectPipesInPipeline)
        {
            connectPipesInPipeline = null;
            if (ecobject == null || ecobject.Instance == null)
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return false;
            }
            else if (ecobject.ClassName != "PIPE")
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return false;
            }
            else
            {
                List<BMECObject> result = new List<BMECObject>();
                List<BMECObject> connected_bmecobject_list = ecobject.ConnectedComponents;
                string pipe_line = ecobject.Instance["LINENUMBER"].StringValue;

                if (connected_bmecobject_list.Count == 0)
                {
                    BMECApi.Instance.SelectComponent(ecobject.Instance, true);
                    result.Add(ecobject);
                    connectPipesInPipeline = result;
                    return true;
                }

                IECInstance joint_instance = BMECApi.Instance.GetConnectedPortInstance(ecobject.Ports[0]);

                if (joint_instance != null)
                {
                    List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);
                    if (joint_bmecobject_list != null)
                    {
                        result.AddRange(joint_bmecobject_list);
                    }
                }
                joint_instance = BMECApi.Instance.GetConnectedPortInstance(ecobject.Ports[1]);
                if (joint_instance != null)
                {
                    List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);
                    if (joint_bmecobject_list != null)
                    {
                        result.AddRange(joint_bmecobject_list);
                    }
                }
                foreach (BMECObject connected_bmecobject in connected_bmecobject_list)
                {
                    get_all_extend_connectedComponents(connected_bmecobject, result, pipe_line);
                }
                foreach (BMECObject connected_bmecobject in result)
                {
                    BMECApi.Instance.SelectComponent(connected_bmecobject.Instance, true);
                }
                connectPipesInPipeline = result;
            }
            return true;
        }
        public static void get_all_extend_connectedComponents(BMECObject bmec_object, List<BMECObject> result_bmec_object_list, string pipe_line)
        {
            result_bmec_object_list.Add(bmec_object);
            List<BMECObject> connected_bmec_object_list = bmec_object.ConnectedComponents;
            foreach (BMECObject bmec_object_temp in connected_bmec_object_list)
            {
                BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
                {
                    return (exit_object_temp.Instance.InstanceId.Equals(bmec_object_temp.Instance.InstanceId));
                });
                if (exit_object == null)
                {
                    IECInstance joint_instance = BMECApi.Instance.GetConnectedPortInstance(bmec_object.Ports[0]);
                    if (joint_instance != null)
                    {
                        List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);
                        if (joint_bmecobject_list != null)
                        {
                            result_bmec_object_list.AddRange(joint_bmecobject_list);
                        }
                    }
                    joint_instance = BMECApi.Instance.GetConnectedPortInstance(bmec_object.Ports[1]);
                    if (joint_instance != null)
                    {
                        //Bentley.Building.Mechanical.ComponentLibrary.Common.DrawGeometry.BMEndPrep
                        List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);
                        if (joint_bmecobject_list != null)
                        {
                            result_bmec_object_list.AddRange(joint_bmecobject_list);
                        }
                    }
                    if (bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].Name.Equals("VALVE") || bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].BaseClasses[0].Name.Equals("INLINE_VALVE"))
                    {
                        ECInstanceList ec_list = BMECApi.Instance.GetRelatedInstances(bmec_object.Instance, "VALVE_HAS_VALVE_OPERATING_DEVICE", true, true);
                        if (ec_list.Count != 0)
                        {
                            result_bmec_object_list.Add(new BMECObject(ec_list[0]));
                        }

                    }
                    if (bmec_object_temp.Instance["LINENUMBER"].StringValue.Equals(pipe_line))
                    {
                        get_all_extend_connectedComponents(bmec_object_temp, result_bmec_object_list, pipe_line);
                    }

                }

            }
        }
        /// <summary>
        /// 根据相连接的组件创建树结构
        /// </summary>
        /// <returns></returns>
        public static PipeConnectTree<BMECObject> CreateTreeWithConnectedBMECObject(BMECObject pipeComponent, Element ele, List<BMECObject> range = null)
        {
            PipeConnectTree<BMECObject> pipeTreeRoot;//根节点
            List<BMECObject> noRepeateConnectComponents;
            GetConnectedComponentWithNoRepeate(pipeComponent, ele, ref range, out noRepeateConnectComponents, out pipeTreeRoot);
            return pipeTreeRoot;
        }
        /// <summary>
        /// 查找未在 range 中与 pipeComponent 相连接的组件
        /// </summary>
        /// <param name="pipeComponent">要查找的组件</param>
        /// <param name="range">判定是否重复添加了管件的范围，在方法离开后会更新该容器的内容</param>
        /// <param name="noRepeateConectComponents">不包含重复的连接组件</param>
        /// <returns>是否添加了新的组件</returns>·
        public static bool GetConnectedComponentWithNoRepeate(BMECObject pipeComponent, Element ele, ref List<BMECObject> range, out List<BMECObject> noRepeateConnectComponents, out PipeConnectTree<BMECObject> Node)
        {
            bool bRet = false;
            Node = new PipeConnectTree<BMECObject>(pipeComponent);
            if (range == null)
            {
                range = new List<BMECObject>();
            }

            if (!BMECApi.Instance.ObjectContained(pipeComponent, range))
            {
                range.Add(pipeComponent);
            }
            List<BMECObject> connectedComponents = pipeComponent.ConnectedComponents;
            //connectedComponents加支吊架
            IECInstance iec_object = JYX_ZYJC_CLR.PublicMethod.FindInstance(ele);//通过Element 获取 IECInstance
            if (iec_object != null)
            {
                bool isPipe = api.InstanceDefinedAsClass(iec_object, "PIPE", true); //判断是否为管道 以及他的子类
                if (isPipe)
                {
                    foreach (IECRelationshipInstance item in pipeComponent.Instance.GetRelationshipInstances())
                    {
                        if (item.ClassDefinition.Name == "DEVICE_HAS_SUPPORT")
                        {
                            connectedComponents.Add(api.CreateBMECObjectForInstance(item.Target));
                        }
                    }
                }

            }
            noRepeateConnectComponents = new List<BMECObject>();
            foreach (BMECObject item in connectedComponents)
            {
                if (!BMECApi.Instance.ObjectContained(item, range))
                {
                    range.Add(item);
                    noRepeateConnectComponents.Add(item);
                    bRet = true;
                }
            }
            foreach (BMECObject item in noRepeateConnectComponents)
            {
                PipeConnectTree<BMECObject> pipeNode;
                List<BMECObject> noRepeateConectComponents2;
                GetConnectedComponentWithNoRepeate(item, ele, ref range, out noRepeateConectComponents2, out pipeNode);
                Node.AddNode(pipeNode);
            }
            return bRet;
        }
        public static bool SearchPipesToReSize(DVector3d dir, PipeConnectTree<BMECObject> pipeTreeRoot, ref List<PipeConnectTree<BMECObject>> reSizePipes, ref List<PipeConnectTree<BMECObject>> movePipes, ref bool hasParallelPipe)
        {
            if (pipeTreeRoot == null || dir == DVector3d.Zero)
            {
                return false;
            }
            if (reSizePipes == null)
            {
                reSizePipes = new List<PipeConnectTree<BMECObject>>();
            }
            if (movePipes == null)
            {
                movePipes = new List<PipeConnectTree<BMECObject>>();
            }
            movePipes.Add(pipeTreeRoot);
            if (pipeTreeRoot.Nodes.Count > 0)
            {
                foreach (PipeConnectTree<BMECObject> item in pipeTreeRoot.Nodes)
                {
                    item.isTargetPipe = false;
                    item.direction = -1;
                    item.fluidDir = -1;
                    item.isOutrange = false;
                    item.hasParallel = item.Parent.hasParallel;
                    if (BMECApi.Instance.InstanceDefinedAsClass(item.Data.Instance, "PIPE", true))
                    {
                        DPoint3d[] portPoints = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(item.Data);
                        BMECObject connectPipeTemp = item.Data.GetConnectedComponentAtPort(item.Data.Ports[0].ID);
                        DVector3d pipeDir = new DVector3d(portPoints[0], portPoints[1]);
                        //判断管道流向
                        if (pipeDir.IsParallelOrOppositeTo(dir))
                        {
                            hasParallelPipe = true;
                            if (pipeDir.IsParallelTo(dir))
                            {
                                item.fluidDir = 1;
                            }
                            else
                            {
                                item.fluidDir = 2;
                            }
                        }
                        //判断管道的方向(不是流向)
                        if (connectPipeTemp != null && BMECApi.Instance.ObjectContained(item.Parent.Data, new List<BMECObject>() { connectPipeTemp }))
                        {
                            pipeDir = new DVector3d(portPoints[0], portPoints[1]);
                        }
                        else
                        {
                            pipeDir = new DVector3d(portPoints[1], portPoints[0]);
                        }
                        if (pipeDir.IsParallelOrOppositeTo(dir))//共线
                        {
                            hasParallelPipe = true;
                            if (pipeDir.IsParallelTo(dir))//同向
                            {
                                item.hasParallel = true;
                                item.direction = 1;
                                if (dir.Magnitude > item.Data.GetDoubleValueInMM("LENGTH") * uorPerMaster)//长度不足
                                {
                                    item.isOutrange = true;
                                    SearchPipesToReSize(dir, item, ref reSizePipes, ref movePipes, ref hasParallelPipe);
                                }
                                else
                                {
                                    item.isTargetPipe = true;
                                    reSizePipes.Add(item);
                                }
                            }
                            else//反向
                            {
                                item.direction = 2;
                                if (item.hasParallel)
                                {
                                    SearchPipesToReSize(dir, item, ref reSizePipes, ref movePipes, ref hasParallelPipe);
                                }
                                else
                                {
                                    movePipes.Add(item);
                                    reSizePipes.Add(item);
                                }
                            }
                        }
                        else
                        {
                            SearchPipesToReSize(dir, item, ref reSizePipes, ref movePipes, ref hasParallelPipe);
                        }
                    }
                    else
                    {
                        SearchPipesToReSize(dir, item, ref reSizePipes, ref movePipes, ref hasParallelPipe);
                    }
                }
            }

            return false;
        }
    }
}
