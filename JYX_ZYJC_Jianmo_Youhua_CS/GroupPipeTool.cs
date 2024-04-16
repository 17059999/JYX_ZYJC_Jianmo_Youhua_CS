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
using Bentley.MstnPlatformNET;
using Bentley.Building.Mechanical.Components;
using Bentley.Building.Mechanical.ComponentLibrary.Equipment;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.OpenPlant.Modeler.Api;
using System.Collections;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class GroupPipeTool : DgnElementSetTool
    {
        protected static BMECApi api = BMECApi.Instance;  //CE
        static BIM.Application app = Utilities.ComApp;  //V8I
        public bool is_drag_selected = false;

        /// <summary>
        /// 选中的那根管道形成的所以点的集合
        /// </summary>
        private List<DPoint3d> pointList = new List<DPoint3d>(); //选中的那根管道形成的所以点的集合

        /// <summary>
        /// 选中的那根管道的BMECObject的集合
        /// </summary>
        List<BMECObject> bmecObjectList = new List<BMECObject>(); //选中的那根管道的BMECObject的集合

        /// <summary>
        /// 全局 选择的那一根管道 用来判断是否平行或者是否选择了同一根
        /// </summary>
        BMECObject bmec_object = null; //全局 选择的那一根管道 用来判断是否平行或者是否选择了同一根

        /// <summary>
        /// 用于和from交互
        /// </summary>
        GroupPipeToolFrom from = new GroupPipeToolFrom(); //用于和from交互
        //List<BMECObject> orginBmecObjectList = new List<BMECObject>();

        /// <summary>
        /// 选择管道后得到的管道端点的集合 未排序
        /// </summary>
        Dictionary<int, List<DPoint3d>> duanjianList = new Dictionary<int, List<DPoint3d>>(); //选择管道后得到的管道端点的集合 未排序

        /// <summary>
        /// 选择的所以管道的object
        /// </summary>
        Dictionary<int, BMECObject> bmecDic = new Dictionary<int, BMECObject>(); //选择的所以管道的object

        /// <summary>
        /// 字典类的Key
        /// </summary>
        int i = 0; //字典类的Key

        /// <summary>
        /// 用于判断选择的是第几根管道
        /// </summary>
        int firstPipe = 0; //用于判断选择的是第几根管道
        //DVector3d normalDV = new DVector3d();

        /// <summary>
        /// 生成的管道的点的集合 不包括选中管道生成的管道
        /// </summary>
        Dictionary<int, List<DPoint3d>> shengchengList = new Dictionary<int, List<DPoint3d>>(); //生成的管道的点的集合 不包括选中管道生成的管道

        /// <summary>
        /// 生成的管道的object 同上
        /// </summary>
        Dictionary<int, List<BMECObject>> shencBmecList = new Dictionary<int, List<BMECObject>>(); //生成的管道的object 同上

        /// <summary>
        /// 创建弯头时弯头的入口点
        /// </summary>
        DPoint3d start_dpoint1 = new DPoint3d(); //创建弯头时弯头的入口点

        /// <summary>
        /// 创建弯头时弯头的出口点
        /// </summary>
        DPoint3d start_dpoint2 = new DPoint3d(); //创建弯头时弯头的出口点
        //Dictionary<int, List<DPoint3d>> dongtaiList = new Dictionary<int, List<DPoint3d>>();

        /// <summary>
        /// 选中的管道的Key
        /// </summary>
        int select = -1; //选中的管道的Key

        /// <summary>
        /// 选中的元素
        /// </summary>
        Element elem = null; //选中的元素

        /// <summary>
        /// 弯头的IECInstance
        /// </summary>
        private IECInstance elbowOrBendInstance; //弯头的IECInstance

        /// <summary>
        /// 是否框选
        /// </summary>
        public bool boxType = false;

        /// <summary>
        /// 是否开始动态显示管道
        /// </summary>
        public bool isDyPipe = false;

        /// <summary>
        /// 构造函数 初始化时给from赋值
        /// </summary>
        /// <param name="groupfrom"></param>
        public GroupPipeTool(GroupPipeToolFrom groupfrom)
        {
            from = groupfrom;
        }

        /// <summary>
        /// 重置值
        /// </summary>
        public void clearup()
        {
            pointList.Clear();
            bmecObjectList.Clear();
            bmec_object = null;
            //orginBmecObjectList.Clear();
            duanjianList.Clear();
            bmecDic.Clear();
            i = 0;
            firstPipe = 0;
            //normalDV = new DVector3d();
            shengchengList.Clear();
            shencBmecList.Clear();
        }

        /// <summary>
        /// 动态显示?
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Success;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            //AccuSnap.SnapEnabled = true;
            GetPipeLine();
            //AccuSnap.LocateEnabled = true;
            ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
            if (ecList.Count > 0)
            {
                bool typeSelect = true;
                bool typePx = true;
                //BMECObject firstBmec = new BMECObject();
                foreach (IECInstance ecinstance in ecList)
                {
                    string ecClassName = ecinstance.ClassDefinition.Name;
                    if (ecClassName != "PIPE")
                    {
                        typeSelect = false;
                    }
                }
                if (!typeSelect)
                {
                    System.Windows.Forms.MessageBox.Show("存在不是直管的元素请重新选择");
                    app.CommandState.StartDefaultCommand();
                    return;
                }
                for (int ii = 0; ii < ecList.Count - 1; ii++)
                {
                    BMECObject nowBmec = new BMECObject(ecList[ii]);
                    BMECObject nextBmec = new BMECObject(ecList[ii + 1]);
                    //DPoint3d dp1 = DPoint3d.Zero;
                    //DPoint3d dp2 = DPoint3d.Zero;
                    //DPoint3d dp3 = DPoint3d.Zero;
                    //DPoint3d dp4 = DPoint3d.Zero;
                    DPoint3d[] dparr1 = GetTowPortPoint(nowBmec);
                    DPoint3d[] dparr2 = GetTowPortPoint(nextBmec);
                    DVector3d dv1 = new DVector3d(dparr1[0], dparr1[1]);
                    DVector3d dv2 = new DVector3d(dparr2[0], dparr2[1]);
                    bool px = dv1.IsParallelOrOppositeTo(dv2);
                    if (!px)
                    {
                        typePx = false;
                    }
                }
                if (!typePx)
                {
                    System.Windows.Forms.MessageBox.Show("所选则的管道不平行");
                    app.CommandState.StartDefaultCommand();
                    return;
                }
                foreach (IECInstance ecinstance in ecList)
                {
                    bmec_object = new BMECObject(ecinstance);
                    //elem = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(bmec_object);
                    ulong beId = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object);
                    ElementId eid = new ElementId(ref beId);
                    elem = Session.Instance.GetActiveDgnModel().FindElementById(eid);
                    //ulong iid = elem.ElementId;
                    fuzhiList();
                }

            }
            SelectionSetManager.EmptyAll();
            app.ShowCommand("成组布管");
            app.ShowPrompt("添加管道(shift切换框选、ctrl切换点选)");
            initelbowDic();
            //app.ShowCommand("请选择管道的一个端口");
        }

        /// <summary>
        /// 执行app.CommandState.StartDefaultCommand()时发生
        /// </summary>
        protected override void OnCleanup()
        {
            base.OnCleanup();
            pointList.Clear();
        }

        /// <summary>
        /// 鼠标右键时发生
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            if (pointList.Count < 3)
            {
                if (pointList.Count == 0)
                {
                    app.CommandState.StartDefaultCommand();
                }
                else
                {
                    //pointList.RemoveAt(pointList.Count - 1);
                    bmecObjectList.RemoveAt(bmecObjectList.Count - 1);
                    shengchengList.Clear();
                    pointList.Clear();
                    app.ShowCommand("成组布管");
                    app.ShowPrompt("请选择管道的一个端口");
                    EndDynamics();
                    return true;
                    //BMEC_Object_list.RemoveAt(BMEC_Object_list.Count - 1);
                }
            }
            else
            {
                pointList.Clear();
                app.CommandState.StartDefaultCommand();
                //绘制完管道，更新连接性
                #region
                //foreach (var item in innerPipes)
                //{
                //    //item.DiscoverConnectionsEx();
                //    //item.UpdateConnections();
                //}
                //foreach (var item in outerPipes)
                //{
                //    //item.DiscoverConnectionsEx();
                //    //item.UpdateConnections();
                //}
                //for (int i = 0; i < innerPipes.Count - 1; i++)
                //{
                //    innerPipes[i].DiscoverConnectionsEx();
                //    api.DoSettingsForFastenerUtility(innerPipes[i], innerPipes[i + 1].Ports[0]);
                //    api.ConnectObjectsAtPorts(innerPipes[i].Ports[1], innerPipes[i], innerPipes[i + 1].Ports[0], innerPipes[i + 1]);


                //    //api.CreateJointForCompatiblePorts(innerPipes[i].Instance, innerPipes[i + 1].Instance, innerPipes[i].Ports[1].Location);
                //}
                //for (int i = 0; i < outerPipes.Count - 1; i++)
                //{
                //    outerPipes[i].DiscoverConnectionsEx();
                //    api.DoSettingsForFastenerUtility(outerPipes[i], outerPipes[i + 1].Ports[0]);
                //    api.ConnectObjectsAtPorts(outerPipes[i].Ports[1], outerPipes[i], outerPipes[i + 1].Ports[0], outerPipes[i + 1]);
                //    //api.ConnectPorts(outerPipes[i].Ports[1], outerPipes[i + 1].Ports[0]);
                //}

                //foreach (var item in innerPipes)
                //{
                //    List<BMECObject> connectedComponents = item.ConnectedComponents;
                //    //item.DiscoverConnectionsEx();
                //    //item.UpdateConnections();
                //}
                //foreach (var item in outerPipes)
                //{
                //    List<BMECObject> connectedComponents = item.ConnectedComponents;
                //}
                #endregion
            }
            from.Close();
            firstPipe = 0;
            clearup();
            return true;
        }

        /// <summary>
        /// 在鼠标右键里调用?
        /// </summary>
        protected override void OnRestartTool()
        {
            //base.OnRestartTool();
        }

        /// <summary>
        /// ??
        /// </summary>
        /// <returns></returns>
        protected override bool NeedAcceptPoint()
        {
            return base.NeedAcceptPoint();
        }

        public List<DPoint3d> testPointList = new List<DPoint3d>();
        /// <summary>
        /// 鼠标点击时发生
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            #region 选择管道
            if (!from.type)
            {
                #region 测试框选
                if (boxType)
                {
                    SetElementSource(ElementSource.Fence);
                    if (testPointList.Count == 0)
                    {
                        testPointList.Add(ev.Point);

                        this.BeginDynamics();
                    }
                    else if (testPointList.Count > 0)
                    {
                        DgnModel active_model = Session.Instance.GetActiveDgnModel();
                        Viewport active_view_port = Session.GetActiveViewport();
                        FenceManager.DefineByPoints(fanwei, active_view_port);
                        ElementAgenda ElementAgenda = new ElementAgenda();
                        if (ElementSource.Fence == GetElementSource())
                        {
                            //base.OnDataButton(ev);
                            FenceParameters fence_pa = new FenceParameters(Session.Instance.GetActiveDgnModelRef(), DTransform3d.Identity);
                            //fence_pa.ClipMode = FenceClipMode.None;
                            bool ss = fence_pa.AllowOverlaps();
                            //fence_pa.SetOverlapMode(true);

                            ss = fence_pa.AllowOverlaps();
                            FenceManager.InitFromActiveFence(fence_pa, true, true, FenceClipMode.Original);

                            DgnModelRef[] modelRefList = new DgnModelRef[1];
                            //ss = FenceManager.IsOverlapMode;
                            //ss = FenceManager.IsVoidMode;
                            //ss = FenceManager.IsClipMode;
                            modelRefList[0] = Session.Instance.GetActiveDgnModelRef();
                            // ElementAgenda ElementAgenda = new ElementAgenda();
                            FenceManager.BuildAgenda(fence_pa, ElementAgenda, modelRefList, true, false, false);

                            //ss = FenceManager.IsOverlapMode;
                            //ss = FenceManager.IsClipMode;
                            //uint count1 = ElementAgenda.GetCount();
                        }
                        uint count = ElementAgenda.GetCount();
                        if (count > 0)
                        {
                            for (uint k = 0; k < count; k++)
                            {
                                Element seleEle = ElementAgenda.GetEntry(k);
                                BMECObject kuanxbmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(seleEle.ElementId);
                                if (kuanxbmec == null || kuanxbmec.ClassName != "PIPE")
                                {
                                    System.Windows.Forms.MessageBox.Show("所选的元素中存在不是管道的元素，请重新选择！");
                                    testPointList.Clear();
                                    FenceManager.ClearFence();
                                    return true;
                                }
                            }
                            bool ptype = false;
                            for (uint k = 0; k < count - 1; k++)
                            {
                                Element seleEle1 = ElementAgenda.GetEntry(k);
                                Element seleEle2 = ElementAgenda.GetEntry(k + 1);
                                BMECObject bmec1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(seleEle1.ElementId);
                                BMECObject bmec2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(seleEle2.ElementId);
                                DPoint3d[] dpo1 = GetTowPortPoint(bmec1);
                                DPoint3d[] dpo2 = GetTowPortPoint(bmec2);
                                DVector3d dv1 = new DVector3d(dpo1[0], dpo1[1]);
                                DVector3d dv2 = new DVector3d(dpo2[0], dpo2[1]);
                                bool pxtype = dv1.IsParallelOrOppositeTo(dv2);
                                if (!pxtype)
                                {
                                    ptype = true;
                                }
                            }
                            if (ptype)
                            {
                                System.Windows.Forms.MessageBox.Show("所选管道不平行");
                                testPointList.Clear();
                                FenceManager.ClearFence();
                                return true;
                            }
                            if (firstPipe == 0)
                            {
                                for (uint k = 0; k < count; k++)
                                {
                                    elem = ElementAgenda.GetEntry(k);
                                    bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                                    fuzhiList();
                                }
                            }
                            else
                            {
                                Element eles = ElementAgenda.GetEntry(0);
                                BMECObject bmecs = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(eles.ElementId);
                                DPoint3d[] sdpoint1 = GetTowPortPoint(bmecs);
                                DPoint3d[] sdpoint2 = GetTowPortPoint(bmecDic[0]);
                                DVector3d sdv = new DVector3d(sdpoint1[0], sdpoint1[1]);
                                DVector3d sdv1 = new DVector3d(sdpoint2[0], sdpoint2[1]);
                                bool px = sdv.IsParallelOrOppositeTo(sdv1);
                                if (!px)
                                {
                                    System.Windows.Forms.MessageBox.Show("所选管道于先前所选管道不平行");
                                    testPointList.Clear();
                                    FenceManager.ClearFence();
                                    return true;
                                }
                                for (uint k = 0; k < count; k++)
                                {
                                    elem = ElementAgenda.GetEntry(k);
                                    bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                                    bool same1 = true;
                                    foreach (KeyValuePair<int, BMECObject> kv in bmecDic)
                                    {
                                        if (kv.Value.Instance["GUID"].StringValue == bmec_object.Instance["GUID"].StringValue)
                                            same1 = false;
                                    }
                                    if (same1)
                                    {
                                        fuzhiList();
                                    }
                                }
                            }
                        }
                        testPointList.Clear();
                        FenceManager.ClearFence();
                        //testPointList.Add(ev.Point);
                    }
                    SetupAndPromptForNextAction();
                    return true;
                }

                #endregion
                HitPath hit_path = DoLocate(ev, true, 0);
                if (hit_path == null)
                {
                    return false;
                }
                elem = hit_path.GetHeadElement(); //得到选中的元素

                bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                //SelectionSetManager.AddElement(elem, Session.Instance.GetActiveDgnModelRef());
                //Element element = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(bmec_object);
                if (bmec_object == null)
                {
                    app.ShowCommand("成组布管");
                    app.ShowPrompt("请选择一个管道");
                    return false;
                }
                else if (bmec_object.ClassName != "PIPE")
                {
                    app.ShowCommand("成组布管");
                    app.ShowPrompt("选择的元素不是圆管，请重新选择");
                    return false;
                }
                else
                {
                    if (firstPipe == 0) //第一根管道
                    {
                        //bmecObjectList.Add(bmec_object);
                        fuzhiList();
                        app.ShowCommand("成组布管");
                        app.ShowPrompt("添加成功");
                        #region
                        //double dn = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");
                        //from.addPipe(bmec_object.ClassName, dn);
                        //firstPipe = 1;
                        ////orginBmecObjectList.Add(bmec_object);
                        //DPoint3d dpoint1 = bmec_object.GetNthPort(0).LocationInUors;
                        //DPoint3d dpoint2 = bmec_object.GetNthPort(1).LocationInUors;
                        //List<DPoint3d> listDpoint = new List<DPoint3d>();
                        //listDpoint.Add(dpoint1);
                        //listDpoint.Add(dpoint2);
                        //duanjianList.Add(i, listDpoint);
                        //bmecDic.Add(i, bmec_object);
                        //i++;
                        //DPoint3d dpoint3 = orginBmecObjectList[0].GetNthPort(0).LocationInUors;
                        //DPoint3d dpoint4 = orginBmecObjectList[0].GetNthPort(1).LocationInUors;
                        //Port port0 = bmec_object.GetNthPort(0);
                        //Port port1 = orginBmecObjectList[0].GetNthPort(0);
                        //BMECObject ob=createOriginalPipe(dpoint3, dpoint4, orginBmecObjectList[0]);
                        #endregion
                    }
                    #region 选中多根管道后判断平行
                    else
                    {
                        if (firstPipe == 1)
                        {
                            if (bmecDic[0].Instance["GUID"].StringValue != bmec_object.Instance["GUID"].StringValue)
                            {
                                bool b = judgeParallel();
                                if (b)
                                {
                                    fuzhiList();
                                    app.ShowCommand("成组布管");
                                    app.ShowPrompt("添加成功");
                                    //DVector3d dv1 = new DVector3d(duanjianList[0][0], duanjianList[0][1]);
                                    //DVector3d dv2 = new DVector3d(duanjianList[0][0], duanjianList[1][0]);
                                    //normalDV = dv1.CrossProduct(dv2);
                                }
                                else
                                {
                                    app.ShowCommand("成组布管");
                                    app.ShowPrompt("所选管道不平行");
                                    return false;
                                }
                            }
                            else
                            {
                                app.ShowCommand("成组布管");
                                app.ShowPrompt("请不要选择同一根管道");
                                return false;
                            }
                            //BIM.PlanarElement
                        }
                        else
                        {
                            bool same = true;
                            foreach (KeyValuePair<int, BMECObject> kv in bmecDic)
                            {
                                if (kv.Value.Instance["GUID"].StringValue == bmec_object.Instance["GUID"].StringValue)
                                    same = false;
                            }
                            //foreach(BMECObject bmecob in orginBmecObjectList)
                            //{
                            //    if (bmecob.LegacyGraphicsId == bmec_object.LegacyGraphicsId)
                            //        same = false;
                            //}
                            if (same)
                            {
                                #region
                                //List<BIM.Point3d> pointList = new List<BIM.Point3d>();
                                //pointList = getDuandian(orginBmecObjectList[0]);
                                //List<BIM.Point3d> pointList2 = new List<BIM.Point3d>();
                                //pointList2 = getDuandian(bmec_object);
                                ////BIM.LineElement line1 = app.CreateLineElement2(null, pointList[0], pointList2[1]);
                                ////BIM.Point3d[] pointsz = new BIM.Point3d[] { pointList[0], pointList[1],pointList2[0],pointList[1] };
                                ////BIM.LineElement line1_ = app.CreateLineElement1(null, pointList.ToArray());
                                ////BIM.ShapeElement shap = app.CreateShapeElement1(null, pointsz);
                                //BIM.Point3d normalPoint = shap.Normal;
                                //DPoint3d normaldPoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(normalPoint);
                                //DPoint3d zero = new DPoint3d(0, 0, 0);
                                //DVector3d normalve = new DVector3d(zero, normaldPoint);
                                //DPoint3d dpoint1 = orginBmecObjectList[0].GetNthPort(0).LocationInUors;
                                //DPoint3d dpoint2 = bmec_object.GetNthPort(0).LocationInUors;
                                //DVector3d dv = new DVector3d(dpoint1, dpoint2);
                                //double degree = normalve.AngleTo(dv).Degrees;
                                //if (degree != 90)
                                //{
                                //    app.ShowCommand("所选的管道不在同一平面上");
                                //    return true;
                                //}
                                //else
                                //{
                                //    judgeParallel();
                                //}
                                #endregion
                                bool b = judgeParallel();
                                if (!b)
                                {
                                    app.ShowCommand("成组布管");
                                    app.ShowPrompt("所选的管道不平行");
                                    return false;
                                }
                                else
                                {
                                    #region 判断是否共面
                                    //DPoint3d dpoint3 = DPoint3d.Zero;
                                    //DPoint3d dpoint4 = DPoint3d.Zero;
                                    //DPoint3d[] pointshuz = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object);
                                    //dpoint3 = pointshuz[0];
                                    //dpoint4 = pointshuz[1];
                                    //DVector3d dv = new DVector3d(duanjianList[0][0], dpoint3);
                                    //bool Perpendicular = normalDV.IsPerpendicularTo(dv);
                                    ////double product = normalDV.DotProduct(dv);
                                    //if (Perpendicular)
                                    //{
                                    //    fuzhiList();
                                    //    app.ShowCommand("成组布管");
                                    //    app.ShowPrompt("添加成功");
                                    //}
                                    //else
                                    //{
                                    //    app.ShowCommand("成组布管");
                                    //    app.ShowPrompt("所选的管道不在一个平面上");
                                    //    return true;
                                    //}
                                    #endregion
                                    fuzhiList();
                                    //app.ShowCommand("成组布管");
                                    //app.ShowPrompt("所选的管道不在一个平面上");
                                    return false;
                                }
                            }
                            else
                            {
                                app.ShowCommand("成组布管");
                                app.ShowPrompt("请不要选择相同的管道");
                                return false;
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion
            #region 绘制
            else
            {
                //EndDynamics();

                if (boxType)
                {
                    //EndDynamics();
                    boxType = false;
                }
                isDyPipe = true;
                if (bmecDic.Count == 0) //没有选中管道直接点击了绘制
                {
                    from.type = false;
                    System.Windows.Forms.MessageBox.Show("请先添加管道");
                    return false;
                }
                if (from.elementList.Count > 0) //取消高亮
                {
                    Bentley.Interop.MicroStationDGN.Element element1 = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(from.elementList[from.selectindex]);
                    element1.IsHighlighted = false;
                }
                #region 选择管道的一个端口以这个端口开始绘制              
                if (pointList.Count == 0)
                {
                    HitPath hit_path = DoLocate(ev, true, 0);
                    if (hit_path == null)
                    {
                        return false;
                    }
                    Element elem = hit_path.GetHeadElement();

                    bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                    if (bmec_object == null)
                    {
                        app.ShowCommand("请选择管道的一个端口");
                        return false;
                    }
                    else if (bmec_object.ClassName != "PIPE")
                    {
                        app.ShowCommand("选择的元素不是圆管，请重新选择");
                        return false;
                    }
                    else
                    {
                        #region 判断选择的点是不是所添加管道的端点，是则确定方向
                        bool same = false;
                        foreach (KeyValuePair<int, BMECObject> kv in bmecDic)
                        {
                            if (kv.Value.Instance["GUID"].StringValue == bmec_object.Instance["GUID"].StringValue)
                                same = true;
                        }
                        if (same) //选中的管道在添加的管道里面
                        {
                            DVector3d dv = new DVector3d();
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in duanjianList)
                            {
                                if (ev.Point == kv.Value[0])
                                {
                                    select = kv.Key;
                                    dv = new DVector3d(kv.Value[1], kv.Value[0]);
                                    pointList.Add(kv.Value[1]);
                                    pointList.Add(kv.Value[0]);
                                    //shengchengList.Add(select, pointList);
                                }
                                else if (ev.Point == kv.Value[1])
                                {
                                    select = kv.Key;
                                    dv = new DVector3d(kv.Value[0], kv.Value[1]);
                                    pointList.Add(kv.Value[0]);
                                    pointList.Add(kv.Value[1]);
                                    //shengchengList.Add(select, pointList);
                                }
                            }
                            if (select == -1)
                            {
                                app.ShowCommand("选择的点不是管道的端点，请重新选择");
                                return false;
                            }
                            else
                            {
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in duanjianList)
                                {
                                    List<DPoint3d> scDpointList = new List<DPoint3d>();
                                    if (kv.Key != select)
                                    {
                                        DVector3d dvPipe = new DVector3d(kv.Value[0], kv.Value[1]);
                                        bool gongxian = dv.IsParallelTo(dvPipe);
                                        //double jiaodu = dv.AngleTo(dvPipe).Degrees;
                                        if (gongxian)
                                        {
                                            scDpointList.Add(kv.Value[0]);
                                            scDpointList.Add(kv.Value[1]);
                                            shengchengList.Add(kv.Key, scDpointList);//这里字典类重复了，其实可以用list.reverse()进行反转，就用原来的字典就行
                                        }
                                        else
                                        {
                                            scDpointList.Add(kv.Value[1]);
                                            scDpointList.Add(kv.Value[0]);
                                            shengchengList.Add(kv.Key, scDpointList);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            app.ShowCommand("选择的管道不在添加的管道里，请重新选择");
                            return false;
                        }
                        #endregion
                        #region 选择不是端点的情况，可以这样处理
                        //List<Port> portList = bmec_object.Ports;
                        //double seleceDistence = 1000000000;
                        //DPoint3d dpoint3d = new DPoint3d();
                        //for (int i = 0; i < portList.Count; i++)
                        //{
                        //    Port port = bmec_object.GetNthPort(i);
                        //    double seleceDistence1 = ev.Point.Distance(port.Location);
                        //    if (seleceDistence1 < seleceDistence)
                        //    {
                        //        dpoint3d = port.LocationInUors;
                        //        seleceDistence = seleceDistence1;
                        //    }
                        //}
                        //pointList.Add(dpoint3d);
                        #endregion
                        bool bb = base.OnDataButton(ev);
                        //AccuDraw.Active = true;
                        //bool acc = AccuSnap.IsActive(ev);
                        //bool a5 = AccuSnap.LocateEnabled;
                        //bool a6 = AccuSnap.SnapEnabled;
                        //if (ev.Point!=AccuDraw.Origin)
                        //{
                        //    //AccuDraw.Active = true;
                        //    //bool acc=AccuSnap.IsActive(ev);
                        //    //AccuDraw.Origin = ev.Point;
                        //}
                        app.ShowCommand("成组布管");
                        app.ShowPrompt("请选择下一个点");
                        double odc2 = bmecDic[select].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                        double bwchd = bmecDic[select].Instance["INSULATION_THICKNESS"].DoubleValue;
                        if (from.radioTop.Checked)
                        {
                            DMatrix3d dmPipe = bmecDic[select].Transform3d.Matrix; //管道的旋转矩阵
                            //app.Point3dFromTransform3dTimesPoint3d
                            DVector3d dvTop = new DVector3d(0, 0, 1);
                            DVector3d dvM = DMatrix3d.Multiply(dmPipe, dvTop); //向量与旋转矩阵的乘积
                            AccuDraw.Active = true;
                            DPoint3d dpTop = DPoint3d.Add(ev.Point, dvM, (odc2 + bwchd) * 1000);
                            AccuDraw.Origin = dpTop;
                            //ev.Point = dpTop;
                            //base.OnDataButton(ev);
                        }
                        if (from.radioDown.Checked)
                        {
                            DMatrix3d dmpipe = bmecDic[select].Transform3d.Matrix;
                            DVector3d dvDown = new DVector3d(0, 0, -1);
                            DVector3d dvM = DMatrix3d.Multiply(dmpipe, dvDown);
                            DPoint3d dpDown = DPoint3d.Add(ev.Point, dvM, (odc2 + bwchd) * 1000);
                            AccuDraw.Origin = dpDown;
                        }
                        if (from.radioLeft.Checked)
                        {
                            DMatrix3d dmpipe = bmecDic[select].Transform3d.Matrix;
                            DVector3d dvLeft = new DVector3d(0, 1, 0);
                            DVector3d dvm = DMatrix3d.Multiply(dmpipe, dvLeft);
                            AccuDraw.Active = true;
                            DPoint3d dpLeft = DPoint3d.Add(ev.Point, dvm, (odc2 + bwchd) * 1000);
                            //ev.Point = dpLeft;
                            //base.OnDataButton(ev);
                            AccuDraw.Origin = dpLeft;
                        }
                        if (from.radioRight.Checked)
                        {
                            DMatrix3d dmPipe = bmecDic[select].Transform3d.Matrix;
                            DVector3d dvRight = new DVector3d(0, -1, 0);
                            DVector3d dvM = DMatrix3d.Multiply(dmPipe, dvRight);
                            DPoint3d dpRight = DPoint3d.Add(ev.Point, dvM, (odc2 + bwchd) * 1000);
                            AccuDraw.Origin = dpRight;
                        }
                        BeginDynamics(); //开始动态显示，不结束则一直动态显示
                        bmecObjectList.Add(bmecDic[select]);
                    }
                }
                #endregion
                else //第二个点点击后进入
                {
                    bool bb = base.OnDataButton(ev);
                    //pointList.Add(ev.Point);
                    //DVector3d nomal = new DVector3d();
                    //int index = 0;
                    //if (select == 0)
                    //{
                    //    index = select + 1;
                    //}
                    double odc2 = bmecDic[select].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                    double bwchd = bmecDic[select].Instance["INSULATION_THICKNESS"].DoubleValue;
                    DPoint3d scDpoint = ev.Point;
                    DVector3d qsDv1 = new DVector3d(0, 0, 1);
                    if (from.radioTop.Checked)
                    {
                        DPoint3d orginP = pointList[pointList.Count - 1];

                        bool bbb1 = distence(orginP, ev.Point);
                        if (bbb1)
                        {
                            return false;
                        }

                        DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                        DPoint3d dp2 = pointList[pointList.Count - 2];
                        DVector3d dv1 = new DVector3d(dp2, orginP);
                        DVector3d dv2 = new DVector3d(1, 0, 0);

                        DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d3);

                        DVector3d dvTop = new DVector3d(0, 0, 1);

                        qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvTop);

                        dvTop = qsDv1;

                        DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                        DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                        DPoint3d mubiao = orginP + yPipeDv;

                        bool isMUbiao = distence(mubiao, ev.Point);
                        if (isMUbiao)
                        {
                            return false;
                        }

                        DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                        Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                        List<DPoint3d> zhongDpList = new List<DPoint3d>();
                        zhongDpList.Add(pointList[pointList.Count - 2]);
                        zhongDpList.Add(pointList[pointList.Count - 1]);
                        zhongdianList.Add(0, zhongDpList);
                        Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                        DVector3d scDv = new DVector3d(mubiao, ev.Point);
                        DVector3d yDv = new DVector3d(1, 0, 0);
                        DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                        DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                        DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                        //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                        scDpoint = ev.Point - suofDv;
                        scDpoint = scZD[0][1];
                        AccuDraw.Origin = ev.Point;
                    }
                    if (from.radioDown.Checked)
                    {
                        DPoint3d orginP = pointList[pointList.Count - 1];

                        bool bbb1 = distence(orginP, ev.Point);
                        if (bbb1)
                        {
                            return false;
                        }

                        DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                        DPoint3d dpt1 = pointList[pointList.Count - 2];
                        DVector3d dvyt = new DVector3d(dpt1, orginP);
                        DVector3d yDv1 = new DVector3d(1, 0, 0);
                        DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv1, dvyt, ref dm3d1);
                        DVector3d dvDown = new DVector3d(0, 0, -1);

                        qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvDown);

                        dvDown = qsDv1;

                        DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvDown);
                        DVector3d yPipeUnirDv1 = DMatrix3d.Multiply(dm3d1, dvDown);
                        DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                        DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnirDv1;
                        DPoint3d mubiao = orginP + yPipeDv;

                        bool isMubiao = distence(mubiao, ev.Point);
                        if (isMubiao)
                        {
                            return false;
                        }
                        //DPoint3d mubiao1 = orginP + yPipeDv1;

                        DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                        Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                        List<DPoint3d> zhongDpList = new List<DPoint3d>();
                        zhongDpList.Add(pointList[pointList.Count - 2]);
                        zhongDpList.Add(pointList[pointList.Count - 1]);
                        zhongdianList.Add(0, zhongDpList);
                        Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                        DVector3d scDv = new DVector3d(mubiao, ev.Point);
                        DVector3d yDv = new DVector3d(1, 0, 0);
                        DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                        //DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                        DVector3d dvM = DMatrix3d.Multiply(dm3d, dvDown);
                        DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                        scDpoint = ev.Point - suofDv;
                        scDpoint = scZD[0][1];
                        //DPoint3d dpDown = orginP + suofDv;
                        AccuDraw.Origin = ev.Point;
                    }
                    if (from.radioLeft.Checked)
                    {
                        DPoint3d orginP = pointList[pointList.Count - 1];

                        bool bbb1 = distence(orginP, ev.Point);
                        if (bbb1)
                        {
                            return false;
                        }

                        DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                        DPoint3d dp2 = pointList[pointList.Count - 2];
                        DVector3d dv1 = new DVector3d(dp2, orginP);
                        DVector3d dv2 = new DVector3d(1, 0, 0);

                        DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                        DVector3d dvLeft = new DVector3d(0, 1, 0);

                        qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvLeft);

                        dvLeft = qsDv1;

                        DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvLeft);
                        DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                        DPoint3d mubiao = orginP + yPipeDv;

                        bool isMubiao = distence(mubiao, ev.Point);
                        if (isMubiao)
                        {
                            return false;
                        }

                        DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                        Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                        List<DPoint3d> zhongDpList = new List<DPoint3d>();
                        zhongDpList.Add(pointList[pointList.Count - 2]);
                        zhongDpList.Add(pointList[pointList.Count - 1]);
                        zhongdianList.Add(0, zhongDpList);
                        Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                        DVector3d scDv = new DVector3d(mubiao, ev.Point);
                        DVector3d yDv = new DVector3d(1, 0, 0);
                        DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                        //DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                        DVector3d dvM = DMatrix3d.Multiply(dm3d, dvLeft);
                        DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                        scDpoint = ev.Point - suofDv;
                        scDpoint = scZD[0][1];
                        //DPoint3d dpLeft = orginP + suofDv;
                        AccuDraw.Origin = ev.Point;
                    }
                    if (from.radioRight.Checked)
                    {
                        DPoint3d orginP = pointList[pointList.Count - 1];

                        bool bbb1 = distence(orginP, ev.Point);
                        if (bbb1)
                        {
                            return false;
                        }

                        DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                        DPoint3d dp2 = pointList[pointList.Count - 2];
                        DVector3d dv1 = new DVector3d(dp2, orginP);
                        DVector3d dv2 = new DVector3d(1, 0, 0);

                        DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                        DVector3d dvRight = new DVector3d(0, -1, 0);

                        qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvRight);

                        dvRight = qsDv1;

                        DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvRight);
                        DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                        DPoint3d mubiao = orginP + yPipeDv;

                        bool isMubiao = distence(mubiao, ev.Point);
                        if (isMubiao)
                        {
                            return false;
                        }

                        DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                        Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                        List<DPoint3d> zhongDpList = new List<DPoint3d>();
                        zhongDpList.Add(pointList[pointList.Count - 2]);
                        zhongDpList.Add(pointList[pointList.Count - 1]);
                        zhongdianList.Add(0, zhongDpList);
                        Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                        DVector3d scDv = new DVector3d(mubiao, ev.Point);
                        DVector3d yDv = new DVector3d(1, 0, 0);
                        DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                        //DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                        DVector3d dvM = DMatrix3d.Multiply(dm3d, dvRight);
                        DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                        scDpoint = ev.Point - suofDv;
                        scDpoint = scZD[0][1];
                        //DPoint3d dpRight = orginP + suofDv;
                        AccuDraw.Origin = ev.Point;
                    }
                    #region 选择多根管道
                    if (shengchengList.Count > 0)
                    {
                        //base.OnDataButton(ev);
                        DPoint3d dp0 = pointList[pointList.Count - 2];
                        DPoint3d dp1 = pointList[pointList.Count - 1];
                        DVector3d dv1 = new DVector3d(dp0, dp1); //选中管道所生成的最后一根管道的方向向量
                        DVector3d dv2 = new DVector3d(dp1, scDpoint); //生成管道的方向向量
                        bool p = dv1.IsParallelOrOppositeTo(dv2);
                        #region 平行
                        if (p)
                        {
                            bool g = dv1.IsParallelTo(dv2); //是否同向
                            if (!g)
                            {
                                System.Windows.Forms.MessageBox.Show("不要反向布管");
                                return false;
                            }
                            bool yesorno = false;
                            System.Windows.Forms.DialogResult dialogresult = System.Windows.Forms.MessageBox.Show("是否合并管道？", "消息", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (dialogresult == System.Windows.Forms.DialogResult.Yes)
                            {
                                yesorno = true;
                            }
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                            {
                                DPoint3d sCDpoint = kv.Value[kv.Value.Count - 1] + dv2; //其他管道的生成点
                                //updateOriginalPipe(pointList[pointList.Count - 2], start_dpoint1, bmecObjectList[bmecObjectList.Count - 1]);
                                if (yesorno)
                                {
                                    updateOriginalPipe(kv.Value[kv.Value.Count - 2], sCDpoint, shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                                }
                                else
                                {
                                    shencBmecList[kv.Key].Add(createOriginalPipe(kv.Value[kv.Value.Count - 1], sCDpoint, bmecDic[kv.Key])); //平行不需要发生偏移直接生成
                                }
                                string spec1 = shencBmecList[kv.Key][0].Instance["SPECIFICATION"].StringValue;
                                string lineNumber1 = shencBmecList[kv.Key][0].Instance["LINENUMBER"].StringValue;
                                if (pipelinesName.Count > 0)
                                {
                                    int index = -1;
                                    index = pipelinesName.IndexOf(lineNumber1);
                                    if (index != -1)
                                    {
                                        StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                        StandardPreferencesUtilities.ChangeSpecification(spec1);
                                    }
                                }
                                //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber1);
                                //StandardPreferencesUtilities.ChangeSpecification(spec1);
                                //shencBmecList[kv.Key].Add(createOriginalPipe(kv.Value[kv.Value.Count - 1], sCDpoint, bmecDic[kv.Key])); //平行不需要发生偏移直接生成
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                                //shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = kv.Value[0];
                                if (yesorno)
                                {
                                    shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = sCDpoint;
                                }
                                else
                                {
                                    //shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = kv.Value[0];
                                    shengchengList[kv.Key].Add(sCDpoint);
                                }
                                //shengchengList[kv.Key].Add(sCDpoint);
                            }
                            //pointList.Add(ev.Point);
                            if (yesorno)
                            {
                                pointList[pointList.Count - 1] = scDpoint;
                                updateOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecObjectList[bmecObjectList.Count - 1]);
                            }
                            else
                            {
                                pointList.Add(scDpoint);
                                bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                            }
                            //app.Point3dFromTransform3dTimesPoint3d
                            string spec = bmecObjectList[0].Instance["SPECIFICATION"].StringValue;
                            string lineNumber = bmecObjectList[0].Instance["LINENUMBER"].StringValue;
                            if (pipelinesName.Count > 0)
                            {
                                int index = -1;
                                index = pipelinesName.IndexOf(lineNumber);
                                if (index != -1)
                                {
                                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                    StandardPreferencesUtilities.ChangeSpecification(spec);
                                }
                            }
                            //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
                            //StandardPreferencesUtilities.ChangeSpecification(spec);
                            //bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                            bmecObjectList[bmecObjectList.Count - 1].Create();
                            bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                            bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                        }
                        #endregion
                        #region 生成的管道不平行
                        else
                        {
                            Dictionary<int, List<DPoint3d>> dongtList = new Dictionary<int, List<DPoint3d>>();
                            if (from.radioCenter.Checked)
                            {
                                DPoint3d startDp = pointList[pointList.Count - 2];
                                DPoint3d endDp = pointList[pointList.Count - 1];
                                dongtList = pianyiPipe(scDpoint, startDp, endDp, shengchengList);
                            }
                            if (from.radioTop.Checked)
                            {
                                Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                                {
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                    DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                    DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                    DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                    DVector3d scDv2 = new DVector3d(1, 0, 0);

                                    DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                    DVector3d dvTop1 = new DVector3d(0, 0, 1);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    List<DPoint3d> mgList = new List<DPoint3d>();
                                    mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                    mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                    topList.Add(kv.Key, mgList);
                                }
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp1 = pointList[pointList.Count - 2];
                                DPoint3d yDp2 = pointList[pointList.Count - 1];
                                DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                                DVector3d dvTop = new DVector3d(0, 0, 1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                                DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> dongtList1 = pianyiPipe(ev.Point, startDp, endDp, topList);
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList1)
                                {
                                    bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                    if (isscDp)
                                    {
                                        return false;
                                    }
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                    DVector3d dvTop1 = new DVector3d(0, 0, 1);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;
                                    DPoint3d zhongdian = dtscStartDp;

                                    Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                    List<DPoint3d> zList = new List<DPoint3d>();
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                    zZDList.Add(0, zList);
                                    Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                    //BMECObject pipeDy1 = null;
                                    //pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                    DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                    DVector3d yDv = new DVector3d(1, 0, 0);
                                    DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);


                                    DPoint3d qsTDp = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] + yPipeDv1;
                                    DVector3d tDv1 = new DVector3d(qsTDp, kv.Value[0]);
                                    DVector3d tDv2 = new DVector3d(kv.Value[0], kv.Value[1]);
                                    double ra1 = tDv1.AngleTo(tDv2).Radians;
                                    double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);


                                    DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                    DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                    DPoint3d scDp1 = kv.Value[0] - suofDv;
                                    DPoint3d scDp2 = kv.Value[1] - suofDv;

                                    DVector3d tDv3 = new DVector3d(scDp2, scDp1);
                                    DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                    DPoint3d jiaodain1 = scDp1 + xyDv;
                                    //BMECObject pipeDy = null;
                                    //pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                                    DSegment3d dr1 = new DSegment3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp);
                                    DSegment3d dr2 = new DSegment3d(scDp1, scDp2);
                                    DSegment3d ds1;
                                    double fa, fb;
                                    bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                    bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                    if (xd)
                                    {
                                        zhongdian = ds1.StartPoint;
                                    }
                                    zhongdian = scZDList[0][0];
                                    scDp2 = scZDList[0][1];
                                    List<DPoint3d> dpList1 = new List<DPoint3d>();
                                    dpList1.Add(zhongdian);
                                    dpList1.Add(scDp2);
                                    dongtList.Add(kv.Key, dpList1);
                                }
                            }
                            if (from.radioDown.Checked)
                            {
                                Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                                {
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                    DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                    DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                    DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                    DVector3d scDv2 = new DVector3d(1, 0, 0);

                                    DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                    DVector3d dvTop1 = new DVector3d(0, 0, -1);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd1) * 1000 * yPipeUnitDv1;
                                    List<DPoint3d> mgList = new List<DPoint3d>();
                                    mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                    mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                    topList.Add(kv.Key, mgList);
                                }
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp1 = pointList[pointList.Count - 2];
                                DPoint3d yDp2 = pointList[pointList.Count - 1];
                                DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                                DVector3d dvTop = new DVector3d(0, 0, -1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                                DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> dongtList1 = pianyiPipe(ev.Point, startDp, endDp, topList);
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList1)
                                {
                                    bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                    if (isscDp)
                                    {
                                        return false;
                                    }
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                    DVector3d dvTop1 = new DVector3d(0, 0, -1);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;
                                    DPoint3d zhongdian = dtscStartDp;

                                    Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                    List<DPoint3d> zList = new List<DPoint3d>();
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                    zZDList.Add(0, zList);
                                    Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                    //BMECObject pipeDy1 = null;
                                    //pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                    DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                    DVector3d yDv = new DVector3d(1, 0, 0);
                                    DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                    DPoint3d qsTDp = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] + yPipeDv1;
                                    DVector3d tDv1 = new DVector3d(qsTDp, kv.Value[0]);
                                    DVector3d tDv2 = new DVector3d(kv.Value[0], kv.Value[1]);
                                    double ra1 = tDv1.AngleTo(tDv2).Radians;
                                    double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);


                                    DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                    DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                    DPoint3d scDp1 = kv.Value[0] - suofDv;
                                    DPoint3d scDp2 = kv.Value[1] - suofDv;

                                    DVector3d tDv3 = new DVector3d(scDp2, scDp1);
                                    DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                    DPoint3d jiaodain1 = scDp1 + xyDv;
                                    //BMECObject pipeDy = null;
                                    //pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                                    DSegment3d dr1 = new DSegment3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp);
                                    DSegment3d dr2 = new DSegment3d(scDp1, scDp2);
                                    DSegment3d ds1;
                                    double fa, fb;
                                    bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                    bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                    if (xd)
                                    {
                                        zhongdian = ds1.StartPoint;
                                    }
                                    zhongdian = scZDList[0][0];
                                    scDp2 = scZDList[0][1];
                                    List<DPoint3d> dpList1 = new List<DPoint3d>();
                                    dpList1.Add(zhongdian);
                                    dpList1.Add(scDp2);
                                    dongtList.Add(kv.Key, dpList1);
                                }
                            }
                            if (from.radioLeft.Checked)
                            {
                                Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                                {
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                    DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                    DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                    DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                    DVector3d scDv2 = new DVector3d(1, 0, 0);

                                    DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                    DVector3d dvTop1 = new DVector3d(0, 1, 0);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    List<DPoint3d> mgList = new List<DPoint3d>();
                                    mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                    mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                    topList.Add(kv.Key, mgList);
                                }
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp1 = pointList[pointList.Count - 2];
                                DPoint3d yDp2 = pointList[pointList.Count - 1];
                                DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                                DVector3d dvTop = new DVector3d(0, 1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                                DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> dongtList1 = pianyiPipe(ev.Point, startDp, endDp, topList);
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList1)
                                {
                                    bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                    if (isscDp)
                                    {
                                        return false;
                                    }
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                    DVector3d dvTop1 = new DVector3d(0, 1, 0);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;
                                    DPoint3d zhongdian = dtscStartDp;

                                    Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                    List<DPoint3d> zList = new List<DPoint3d>();
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                    zZDList.Add(0, zList);
                                    Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                    //BMECObject pipeDy1 = null;
                                    //pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                    DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                    DVector3d yDv = new DVector3d(1, 0, 0);
                                    DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                    DPoint3d qsTDp = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] + yPipeDv1;
                                    DVector3d tDv1 = new DVector3d(qsTDp, kv.Value[0]);
                                    DVector3d tDv2 = new DVector3d(kv.Value[0], kv.Value[1]);
                                    double ra1 = tDv1.AngleTo(tDv2).Radians;
                                    double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);


                                    DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                    DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                    DPoint3d scDp1 = kv.Value[0] - suofDv;
                                    DPoint3d scDp2 = kv.Value[1] - suofDv;

                                    DVector3d tDv3 = new DVector3d(scDp2, scDp1);
                                    DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                    DPoint3d jiaodain1 = scDp1 + xyDv;
                                    //BMECObject pipeDy = null;
                                    //pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                                    DSegment3d dr1 = new DSegment3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp);
                                    DSegment3d dr2 = new DSegment3d(scDp1, scDp2);
                                    DSegment3d ds1;
                                    double fa, fb;
                                    bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                    bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                    if (xd)
                                    {
                                        zhongdian = ds1.StartPoint;
                                    }
                                    zhongdian = scZDList[0][0];
                                    scDp2 = scZDList[0][1];
                                    List<DPoint3d> dpList1 = new List<DPoint3d>();
                                    dpList1.Add(zhongdian);
                                    dpList1.Add(scDp2);
                                    dongtList.Add(kv.Key, dpList1);
                                }
                            }
                            if (from.radioRight.Checked)
                            {
                                Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                                {
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                    DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                    DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                    DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                    DVector3d scDv2 = new DVector3d(1, 0, 0);

                                    DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                    DVector3d dvTop1 = new DVector3d(0, -1, 0);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    List<DPoint3d> mgList = new List<DPoint3d>();
                                    mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                    mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                    topList.Add(kv.Key, mgList);
                                }
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp1 = pointList[pointList.Count - 2];
                                DPoint3d yDp2 = pointList[pointList.Count - 1];
                                DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                                DVector3d dvTop = new DVector3d(0, -1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                                DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> dongtList1 = pianyiPipe(ev.Point, startDp, endDp, topList);
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList1)
                                {
                                    bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                    if (isscDp)
                                    {
                                        return false;
                                    }
                                    double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                    double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                    DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                    DVector3d dvTop1 = new DVector3d(0, -1, 0);

                                    dvTop1 = qsDv1;

                                    DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                    DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                    DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;
                                    DPoint3d zhongdian = dtscStartDp;

                                    Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                    List<DPoint3d> zList = new List<DPoint3d>();
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                    zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                    zZDList.Add(0, zList);
                                    Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                    //BMECObject pipeDy1 = null;
                                    //pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                    DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                    DVector3d yDv = new DVector3d(1, 0, 0);
                                    DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                    DPoint3d qsTDp = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] + yPipeDv1;
                                    DVector3d tDv1 = new DVector3d(qsTDp, kv.Value[0]);
                                    DVector3d tDv2 = new DVector3d(kv.Value[0], kv.Value[1]);
                                    double ra1 = tDv1.AngleTo(tDv2).Radians;
                                    double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);


                                    DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                    DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                    DPoint3d scDp1 = kv.Value[0] - suofDv;
                                    DPoint3d scDp2 = kv.Value[1] - suofDv;

                                    DVector3d tDv3 = new DVector3d(scDp2, scDp1);
                                    DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                    DPoint3d jiaodain1 = scDp1 + xyDv;
                                    //BMECObject pipeDy = null;
                                    //pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                                    DSegment3d dr1 = new DSegment3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp);
                                    DSegment3d dr2 = new DSegment3d(scDp1, scDp2);
                                    DSegment3d ds1;
                                    double fa, fb;
                                    bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                    bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                    if (xd)
                                    {
                                        zhongdian = ds1.StartPoint;
                                    }
                                    zhongdian = scZDList[0][0];
                                    scDp2 = scZDList[0][1];
                                    List<DPoint3d> dpList1 = new List<DPoint3d>();
                                    dpList1.Add(zhongdian);
                                    dpList1.Add(scDp2);
                                    dongtList.Add(kv.Key, dpList1);
                                }
                            }
                            //Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(scDpoint); //得到偏移后点的集合
                            Dictionary<int, List<DPoint3d>> cxscDpoint = new Dictionary<int, List<DPoint3d>>(); //创建弯头后 弯头的2点
                            Dictionary<int, List<BMECObject>> wantList = new Dictionary<int, List<BMECObject>>(); //将生成的管道添加进来 用于出错后删除
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                List<DPoint3d> wanDpoint = new List<DPoint3d>();
                                DVector3d dvsc = new DVector3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0]);
                                bool bsc = dv1.IsParallelTo(dvsc); //判断发生偏移后的管道是否同向
                                if (!bsc)
                                {
                                    if (wantList.Count > 0)
                                    {
                                        foreach (KeyValuePair<int, List<BMECObject>> kv1 in wantList)
                                        {
                                            foreach (BMECObject wtBmec1 in kv1.Value)
                                            {
                                                api.DeleteFromModel(wtBmec1); //将生成的管道删除
                                            }

                                        }
                                    }
                                    System.Windows.Forms.MessageBox.Show("存在管道长度过短");
                                    return false;
                                }
                                string eero = "";
                                List<BMECObject> bmecob = createElbowOrBend(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0], kv.Value[1], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1], out eero, kv.Key);
                                if (eero != "")
                                {
                                    if (wantList.Count > 0)
                                    {
                                        foreach (KeyValuePair<int, List<BMECObject>> kv1 in wantList)
                                        {
                                            foreach (BMECObject wtBmec1 in kv1.Value)
                                            {
                                                api.DeleteFromModel(wtBmec1); //将生成的管道删除
                                            }

                                        }
                                    }
                                    System.Windows.Forms.MessageBox.Show(eero);
                                    return false;
                                }
                                wanDpoint.Add(start_dpoint1);
                                wanDpoint.Add(start_dpoint2);
                                cxscDpoint.Add(kv.Key, wanDpoint);
                                wantList.Add(kv.Key, bmecob);

                            }
                            DPoint3d sczhongdian = pointList[pointList.Count - 1];
                            if (from.radioTop.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 0, 1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DPoint3d dpt1 = pointList[pointList.Count - 2];
                                DVector3d dvyt = new DVector3d(dpt1, orginP);
                                DVector3d yDv1 = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv1, dvyt, ref dm3d1);
                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d mubiao1 = orginP + yPipeDv1;

                                DVector3d scDv = new DVector3d(mubiao1, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv1;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao1);
                                DVector3d tDv2 = new DVector3d(mubiao1, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao1 - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;

                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioDown.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 0, -1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DPoint3d dpt1 = pointList[pointList.Count - 2];
                                DVector3d dvyt = new DVector3d(dpt1, orginP);
                                DVector3d yDv1 = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv1, dvyt, ref dm3d1);
                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d mubiao1 = orginP + yPipeDv1;

                                DVector3d scDv = new DVector3d(mubiao1, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv1;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao1);
                                DVector3d tDv2 = new DVector3d(mubiao1, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao1 - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                LineElement lineELE1 = new LineElement(Session.Instance.GetActiveDgnModel(), null, new DSegment3d(sczhongdian, scDpoint));
                                LineElement line2 = new LineElement(Session.Instance.GetActiveDgnModel(), null, new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]));
                                //line2.AddToModel();
                                //lineELE1.AddToModel();
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioLeft.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DPoint3d dpt1 = pointList[pointList.Count - 2];
                                DVector3d dvyt = new DVector3d(dpt1, orginP);
                                DVector3d yDv1 = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv1, dvyt, ref dm3d1);
                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d mubiao1 = orginP + yPipeDv1;

                                DVector3d scDv = new DVector3d(mubiao1, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv1;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao1);
                                DVector3d tDv2 = new DVector3d(mubiao1, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao1 - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioRight.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, -1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DPoint3d dpt1 = pointList[pointList.Count - 2];
                                DVector3d dvyt = new DVector3d(dpt1, orginP);
                                DVector3d yDv1 = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv1, dvyt, ref dm3d1);
                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d1, dvTop);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d mubiao1 = orginP + yPipeDv1;

                                DVector3d scDv = new DVector3d(mubiao1, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv1;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao1);
                                DVector3d tDv2 = new DVector3d(mubiao1, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao1 - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            string eer = "";
                            List<BMECObject> wtBmec = createElbowOrBend(pointList[pointList.Count - 2], sczhongdian, scDpoint, bmecObjectList[bmecObjectList.Count - 1], out eer, select);
                            if (eer != "")
                            {
                                foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                                {
                                    foreach (BMECObject wtBmec1 in wantList[kv.Key])
                                    {
                                        api.DeleteFromModel(wtBmec1); //将生成的管道删除
                                    }
                                    //api.DeleteFromModel(wantList[kv.Key]);
                                }
                                System.Windows.Forms.MessageBox.Show(eer);
                                return false;
                            }
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                updateOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], cxscDpoint[kv.Key][0], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]); //创建弯头后修改最后一根管道
                                shencBmecList[kv.Key].Add(createOriginalPipe(cxscDpoint[kv.Key][1], kv.Value[1], bmecDic[kv.Key])); //新生成的管道
                                string spec2 = shencBmecList[kv.Key][0].Instance["SPECIFICATION"].StringValue;
                                string lineNumber2 = shencBmecList[kv.Key][0].Instance["LINENUMBER"].StringValue;
                                //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber1);
                                //StandardPreferencesUtilities.ChangeSpecification(spec1);
                                if (pipelinesName.Count > 0)
                                {
                                    int index = -1;
                                    index = pipelinesName.IndexOf(lineNumber2);
                                    if (index != -1)
                                    {
                                        StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                        StandardPreferencesUtilities.ChangeSpecification(spec2);
                                    }
                                }
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                                shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                                foreach(BMECObject bbobject in wantList[kv.Key])
                                {
                                    bbobject.DiscoverConnectionsEx();
                                    bbobject.UpdateConnections();
                                }
                                //wantList[kv.Key].DiscoverConnectionsEx();
                                //wantList[kv.Key].UpdateConnections();
                                shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = kv.Value[0];
                                shengchengList[kv.Key].Add(kv.Value[1]);
                                shengchengList[kv.Key].Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                shengchengList[kv.Key][shengchengList[kv.Key].Count - 3] = cxscDpoint[kv.Key][0];
                                shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] = cxscDpoint[kv.Key][1];
                            }
                            updateOriginalPipe(pointList[pointList.Count - 2], start_dpoint1, bmecObjectList[bmecObjectList.Count - 1]);
                            pointList.Add(scDpoint);
                            bmecObjectList.Add(createOriginalPipe(start_dpoint2, pointList[pointList.Count - 1], bmecDic[select]));
                            string spec = bmecObjectList[0].Instance["SPECIFICATION"].StringValue;
                            string lineNumber = bmecObjectList[0].Instance["LINENUMBER"].StringValue;
                            if (pipelinesName.Count > 0)
                            {
                                int index = -1;
                                index = pipelinesName.IndexOf(lineNumber);
                                if (index != -1)
                                {
                                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                    StandardPreferencesUtilities.ChangeSpecification(spec);
                                }
                            }
                            //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
                            //StandardPreferencesUtilities.ChangeSpecification(spec);
                            bmecObjectList[bmecObjectList.Count - 1].Create();
                            bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                            bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                            foreach (BMECObject bmec1 in wtBmec)
                            {
                                bmec1.DiscoverConnectionsEx();
                                bmec1.UpdateConnections();
                            }

                            pointList.Add(pointList[pointList.Count - 1]);
                            pointList[pointList.Count - 3] = start_dpoint1;
                            pointList[pointList.Count - 2] = start_dpoint2;
                        }
                        #endregion
                        #region 以前代码
                        //DPoint3d dpo = shengchengList[index][shengchengList[index].Count - 1];
                        //DPoint3d dp1 = pointList[pointList.Count - 2];
                        //DPoint3d dp2 = pointList[pointList.Count - 1];
                        //DVector3d dv1 = new DVector3d(dp1, dpo);
                        //DVector3d dv2 = new DVector3d(dp1, dp2);
                        //nomal = dv1.CrossProduct(dv2);
                        //DVector3d dv3 = new DVector3d(pointList[pointList.Count - 1], ev.Point);
                        //bool cz = nomal.IsPerpendicularTo(dv3);
                        //if (!cz)
                        //{
                        //    Dictionary<int, List<DPoint3d>> dongtList = ceyimian(ev.Point);
                        //    Dictionary<int, List<DPoint3d>> cxscDpoint = new Dictionary<int, List<DPoint3d>>();
                        //    Dictionary<int, BMECObject> wantList = new Dictionary<int, BMECObject>();
                        //    foreach (KeyValuePair<int,List<DPoint3d>> kv in dongtList)
                        //    {
                        //        List<DPoint3d> wanDpoint = new List<DPoint3d>();
                        //        string eero = "";
                        //        BMECObject bmecob = createElbowOrBend(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], shengchengList[kv.Key][shengchengList[kv.Key].Count - 1], kv.Value[1], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1], out eero, kv.Key);
                        //        if(eero!="")
                        //        {
                        //            System.Windows.Forms.MessageBox.Show(eero);
                        //            return true;
                        //        }
                        //        wanDpoint.Add(start_dpoint1);
                        //        wanDpoint.Add(start_dpoint2);
                        //        cxscDpoint.Add(kv.Key, wanDpoint);
                        //        wantList.Add(kv.Key, bmecob);
                        //    }

                        //    string eer = "";
                        //    createElbowOrBend(pointList[pointList.Count - 2], pointList[pointList.Count - 1], ev.Point, bmecObjectList[bmecObjectList.Count - 1], out eer, select);
                        //    if(eer!="")
                        //    {
                        //        foreach(KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //        {
                        //            api.DeleteFromModel(wantList[kv.Key]);
                        //        }                               
                        //        System.Windows.Forms.MessageBox.Show(eer);
                        //        return true;
                        //    }
                        //    foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //    {
                        //        updateOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], cxscDpoint[kv.Key][0], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                        //        shencBmecList[kv.Key].Add(createOriginalPipe(cxscDpoint[kv.Key][1], kv.Value[1], bmecDic[kv.Key]));
                        //        shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                        //        shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                        //        shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                        //        shengchengList[kv.Key].Add(kv.Value[1]);
                        //        shengchengList[kv.Key].Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                        //        shengchengList[kv.Key][shengchengList[kv.Key].Count - 3] = cxscDpoint[kv.Key][0];
                        //        shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] = cxscDpoint[kv.Key][1];
                        //    }
                        //    updateOriginalPipe(pointList[pointList.Count - 2], start_dpoint1, bmecObjectList[bmecObjectList.Count - 1]);
                        //    pointList.Add(ev.Point);
                        //    bmecObjectList.Add(createOriginalPipe(start_dpoint2, pointList[pointList.Count - 1], bmecDic[select]));
                        //    bmecObjectList[bmecObjectList.Count - 1].Create();
                        //    bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                        //    bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                        //    pointList.Add(pointList[pointList.Count - 1]);
                        //    pointList[pointList.Count - 3] = start_dpoint1;
                        //    pointList[pointList.Count - 2] = start_dpoint2;
                        //}

                        //else
                        //{
                        //    bool b = dv2.IsParallelOrOppositeTo(dv3);
                        //    if (b)
                        //    {
                        //        Dictionary<int, List<DPoint3d>> dongtList = cegongmian(ev.Point);
                        //        foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //        {
                        //            DVector3d dvsc = new DVector3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1], kv.Value[1]);
                        //            bool bsc = dv2.IsParallelTo(dvsc);
                        //            if(!bsc)
                        //            {
                        //                System.Windows.Forms.MessageBox.Show("不要反向布管");
                        //                return true;
                        //            }
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].SetLinearPoints(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0]);
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                        //            shencBmecList[kv.Key].Add(createOriginalPipe(kv.Value[0], kv.Value[1], bmecDic[kv.Key]));
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                        //            shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = kv.Value[0];
                        //            shengchengList[kv.Key].Add(kv.Value[1]);
                        //        }
                        //        pointList.Add(ev.Point);
                        //        bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                        //        bmecObjectList[bmecObjectList.Count - 1].Create();
                        //        bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                        //        bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                        //    }
                        //    else
                        //    {
                        //        Dictionary<int, List<DPoint3d>> dongtList = cegongmian(ev.Point);
                        //        Dictionary<int, List<DPoint3d>> cxscDpoint = new Dictionary<int, List<DPoint3d>>();
                        //        Dictionary<int, BMECObject> wantList = new Dictionary<int, BMECObject>();
                        //        foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //        {
                        //            List<DPoint3d> wanDpoint = new List<DPoint3d>();
                        //            DVector3d dvsc = new DVector3d(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0]);
                        //            bool bsc = dv2.IsParallelTo(dvsc);
                        //            if (!bsc)
                        //            {
                        //                if(wantList.Count>0)
                        //                {
                        //                    foreach(KeyValuePair<int,BMECObject> kv1 in  wantList)
                        //                    {
                        //                        api.DeleteFromModel(wantList[kv1.Key]);
                        //                    }
                        //                }
                        //                System.Windows.Forms.MessageBox.Show("存在管道长度过短");
                        //                return true;
                        //            }
                        //            string eero = "";
                        //            BMECObject bmecob = createElbowOrBend(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0], kv.Value[1], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1], out eero, kv.Key);
                        //            if (eero != "")
                        //            {
                        //                if (wantList.Count > 0)
                        //                {
                        //                    foreach (KeyValuePair<int, BMECObject> kv1 in wantList)
                        //                    {
                        //                        api.DeleteFromModel(wantList[kv1.Key]);
                        //                    }
                        //                }
                        //                System.Windows.Forms.MessageBox.Show(eero);
                        //                return true;
                        //            }
                        //            wanDpoint.Add(start_dpoint1);
                        //            wanDpoint.Add(start_dpoint2);
                        //            cxscDpoint.Add(kv.Key, wanDpoint);
                        //            wantList.Add(kv.Key, bmecob);
                        //        }
                        //        string eer = "";
                        //        createElbowOrBend(pointList[pointList.Count - 2], pointList[pointList.Count - 1], ev.Point, bmecObjectList[bmecObjectList.Count - 1], out eer, select);
                        //        if (eer != "")
                        //        {
                        //            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //            {
                        //                api.DeleteFromModel(wantList[kv.Key]);
                        //            }
                        //            System.Windows.Forms.MessageBox.Show(eer);
                        //            return true;
                        //        }
                        //        foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                        //        {
                        //            updateOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], cxscDpoint[kv.Key][0], shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                        //            shencBmecList[kv.Key].Add(createOriginalPipe(cxscDpoint[kv.Key][1], kv.Value[1], bmecDic[kv.Key]));
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Create();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].DiscoverConnectionsEx();
                        //            shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].UpdateConnections();
                        //            shengchengList[kv.Key][shengchengList[kv.Key].Count - 1] = kv.Value[0];
                        //            shengchengList[kv.Key].Add(kv.Value[1]);
                        //            shengchengList[kv.Key].Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                        //            shengchengList[kv.Key][shengchengList[kv.Key].Count - 3] = cxscDpoint[kv.Key][0];
                        //            shengchengList[kv.Key][shengchengList[kv.Key].Count - 2] = cxscDpoint[kv.Key][1];
                        //        }
                        //        updateOriginalPipe(pointList[pointList.Count - 2], start_dpoint1, bmecObjectList[bmecObjectList.Count - 1]);
                        //        pointList.Add(ev.Point);
                        //        bmecObjectList.Add(createOriginalPipe(start_dpoint2, pointList[pointList.Count - 1], bmecDic[select]));
                        //        bmecObjectList[bmecObjectList.Count - 1].Create();
                        //        bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                        //        bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                        //        pointList.Add(pointList[pointList.Count - 1]);
                        //        pointList[pointList.Count - 3] = start_dpoint1;
                        //        pointList[pointList.Count - 2] = start_dpoint2;
                        //    }
                        //}
                        #endregion
                    }
                    #endregion
                    #region 只有一根管道时
                    else
                    {
                        //base.OnDataButton(ev);
                        DVector3d dv1 = new DVector3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                        DVector3d dv2 = new DVector3d(pointList[pointList.Count - 1], scDpoint);
                        bool b1 = dv1.IsParallelOrOppositeTo(dv2);
                        if (b1)
                        {
                            bool b2 = dv2.IsParallelTo(dv1);
                            if (!b2)
                            {
                                System.Windows.Forms.MessageBox.Show("不要反向布管");
                                return false;
                            }
                            bool yesorno = false;
                            System.Windows.Forms.DialogResult dialogresult = System.Windows.Forms.MessageBox.Show("是否合并管道？", "消息", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (dialogresult == System.Windows.Forms.DialogResult.Yes)
                            {
                                yesorno = true;
                            }
                            //pointList.Add(ev.Point);
                            if (yesorno)
                            {
                                pointList[pointList.Count - 1] = scDpoint;
                                updateOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecObjectList[bmecObjectList.Count - 1]);
                                //string spec = bmecObjectList[bmecObjectList.Count - 1].Instance["SPECIFICATION"].StringValue;
                                //string number = bmecObjectList[bmecObjectList.Count - 1].Instance["NUMBER"].StringValue;
                                //string linenumber = bmecObjectList[bmecObjectList.Count - 1].Instance["LINENUMBER"].StringValue;
                            }
                            else
                            {
                                pointList.Add(scDpoint);
                                bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                            }
                            //bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                            string spec = bmecObjectList[0].Instance["SPECIFICATION"].StringValue;
                            string lineNumber = bmecObjectList[0].Instance["LINENUMBER"].StringValue;
                            //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
                            //StandardPreferencesUtilities.ChangeSpecification(spec);
                            if (pipelinesName.Count > 0)
                            {
                                int index = -1;
                                index = pipelinesName.IndexOf(lineNumber);
                                if (index != -1)
                                {
                                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                    StandardPreferencesUtilities.ChangeSpecification(spec);
                                }
                            }
                            bmecObjectList[bmecObjectList.Count - 1].Create();
                            bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                            bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                        }
                        else
                        {
                            DPoint3d sczhongdian = pointList[pointList.Count - 1];
                            if (from.radioTop.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 0, 1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DVector3d scDv = new DVector3d(mubiao, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao);
                                DVector3d tDv2 = new DVector3d(mubiao, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioDown.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 0, -1);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DVector3d scDv = new DVector3d(mubiao, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao);
                                DVector3d tDv2 = new DVector3d(mubiao, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioLeft.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, 1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DVector3d scDv = new DVector3d(mubiao, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao);
                                DVector3d tDv2 = new DVector3d(mubiao, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            if (from.radioRight.Checked)
                            {
                                DPoint3d orginP = pointList[pointList.Count - 1];
                                DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d yDp3 = pointList[pointList.Count - 2];
                                DPoint3d yDp4 = pointList[pointList.Count - 1];
                                DVector3d yDv3 = new DVector3d(yDp3, yDp4);
                                DVector3d yDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv3, ref dm3d3);

                                DVector3d dvTop = new DVector3d(0, -1, 0);

                                dvTop = qsDv1;

                                DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, dvTop);
                                DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                                DPoint3d mubiao = orginP + yPipeDv;

                                DPoint3d mubiao2 = pointList[pointList.Count - 2] + yPipeDv;
                                Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zhongDpList = new List<DPoint3d>();
                                zhongDpList.Add(pointList[pointList.Count - 2]);
                                zhongDpList.Add(pointList[pointList.Count - 1]);
                                zhongdianList.Add(0, zhongDpList);
                                Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao2, mubiao, zhongdianList);

                                DVector3d scDv = new DVector3d(mubiao, ev.Point);
                                DVector3d yDv = new DVector3d(1, 0, 0);
                                DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                DPoint3d qsTDp = pointList[pointList.Count - 2] + yPipeDv;
                                DVector3d tDv1 = new DVector3d(qsTDp, mubiao);
                                DVector3d tDv2 = new DVector3d(mubiao, ev.Point);
                                double ra1 = tDv1.AngleTo(tDv2).Radians;
                                double juli = (odc2 + bwchd) * 1000 * Math.Tan(ra1 / 2);

                                DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                                DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                                //DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                                DPoint3d scDpointz = ev.Point - suofDv;
                                DPoint3d zhongjsc = mubiao - suofDv;

                                DVector3d tDv3 = new DVector3d(scDpointz, zhongjsc);
                                DVector3d xyDv = DVector3d.Multiply(tDv3, juli / tDv3.Magnitude);
                                DPoint3d jiaodain1 = zhongjsc + xyDv;
                                DSegment3d dr1 = new DSegment3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                                DSegment3d dr2 = new DSegment3d(scDpointz, zhongjsc);
                                DSegment3d ds1;
                                double fa, fb;
                                bool isxj = elbow.CalculateMaleVerticalLine(dr1, dr2, out ds1, out fa, out fb);
                                bool xd = distence(ds1.StartPoint, ds1.EndPoint);
                                if (xd)
                                {
                                    sczhongdian = ds1.StartPoint;
                                }
                                sczhongdian = scZD[0][0];
                                //sczhongdian = jiaodain1;
                                //AccuDraw.Origin = ev.Point;
                            }
                            string eer = "";
                            createElbowOrBend(pointList[pointList.Count - 2], sczhongdian, scDpoint, bmecObjectList[bmecObjectList.Count - 1], out eer, select);
                            if (eer != "")
                            {
                                System.Windows.Forms.MessageBox.Show(eer);
                                return false;
                            }
                            updateOriginalPipe(pointList[pointList.Count - 2], start_dpoint1, bmecObjectList[bmecObjectList.Count - 1]);
                            pointList.Add(scDpoint);
                            bmecObjectList.Add(createOriginalPipe(start_dpoint2, pointList[pointList.Count - 1], bmecDic[select]));
                            string spec = bmecObjectList[0].Instance["SPECIFICATION"].StringValue;
                            string lineNumber = bmecObjectList[0].Instance["LINENUMBER"].StringValue;
                            //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
                            //StandardPreferencesUtilities.ChangeSpecification(spec);
                            if (pipelinesName.Count > 0)
                            {
                                int index = -1;
                                index = pipelinesName.IndexOf(lineNumber);
                                if (index != -1)
                                {
                                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                                    StandardPreferencesUtilities.ChangeSpecification(spec);
                                }
                            }
                            bmecObjectList[bmecObjectList.Count - 1].Create();
                            bmecObjectList[bmecObjectList.Count - 1].DiscoverConnectionsEx();
                            bmecObjectList[bmecObjectList.Count - 1].UpdateConnections();
                            pointList.Add(pointList[pointList.Count - 1]);
                            pointList[pointList.Count - 3] = start_dpoint1;
                            pointList[pointList.Count - 2] = start_dpoint2;
                        }
                    }
                    #endregion
                    //EndDynamics();
                }
                #region
                //else
                //{
                //    pointList.Add(ev.Point);
                //    DPoint3d start = pointList[pointList.Count - 3];
                //    //DPoint3d end = pointList[pointList.Count - 2];
                //    double distence = start.Distance(pointList[pointList.Count - 2]);
                //    DVector3d PipeLine1 = new DVector3d(pointList[pointList.Count - 3], pointList[pointList.Count - 2]);
                //    DVector3d PipeLine2 = new DVector3d(pointList[pointList.Count - 2], pointList[pointList.Count - 1]);
                //    Angle angle = PipeLine1.AngleTo(PipeLine2);
                //    double degree = angle.Degrees;
                //    if (degree != 0)
                //    {
                //        //IECInstance ecInstance = bmecObjectList[bmecObjectList.Count - 1].Instance;
                //        double r = distence / 4;
                //        double lenth = Math.Tan(degree / 2) * r;
                //        double uro = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                //        lenth = lenth / uro;
                //        //ecInstance["LENGTH"].DoubleValue = distence - lenth; //还要算像素
                //        //bmecObjectList[bmecObjectList.Count - 1].Create();
                //        string ecClass = "PIPE_ELBOW";
                //        string erro = "";
                //        #region 得到弯头2个端口的点
                //        BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
                //        BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
                //        BIM.Point3d zero = app.Point3dZero();
                //        BIM.Point3d startPoint = ceDpoint_v8Point(pointList[pointList.Count - 3]);
                //        BIM.Point3d centerPoint = ceDpoint_v8Point(pointList[pointList.Count - 2]);
                //        BIM.Point3d centerPoint1 = centerPoint;
                //        BIM.Point3d centerpoint2 = centerPoint;
                //        BIM.Point3d endPoint = ceDpoint_v8Point(pointList[pointList.Count - 1]);
                //        BIM.Point3d vector1 = app.Point3dFromXYZ(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y, startPoint.Z - centerPoint.Z);
                //        double vector1Length = app.Point3dDistance(zero, vector1);
                //        BIM.Point3d unitVector1 = app.Point3dFromXYZ(vector1.X / vector1Length, vector1.Y / vector1Length, vector1.Z / vector1Length);
                //        BIM.Point3d pt_startPoint1 = app.Point3dAddScaled(ref centerPoint1, unitVector1, lenth);
                //        BIM.Point3d vector2 = app.Point3dFromXYZ(endPoint.X - centerPoint.X, endPoint.Y - centerPoint.Y, endPoint.Z - endPoint.Z);
                //        double vector2Length = app.Point3dDistance(zero, vector2);
                //        BIM.Point3d unitVector2 = app.Point3dFromXYZ(vector2.X / vector2Length, vector2.Y / vector2Length, vector2.Z / vector2Length);
                //        BIM.Point3d pt_startPoint2 = app.Point3dAddScaled(ref centerpoint2, unitVector2, lenth);
                //        BIM.Point3d pt_v1 = app.Point3dFromXYZ(pt_startPoint1.X - startPoint.X, pt_startPoint1.Y - startPoint.Y, pt_startPoint1.Z - startPoint.Z);
                //        BIM.Point3d pt_v2 = app.Point3dFromXYZ(endPoint.X - pt_startPoint2.X, endPoint.Y - pt_startPoint2.Y, endPoint.Z - pt_startPoint2.Z);
                //        #endregion
                //        //bmecObjectList.Add(createElbow(ecClass, 100, degree, lenth, lenth, 0, "", out erro));
                //        //JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(bmecObjectList[bmecObjectList.Count - 1], dir1, pt_v1, dir2, pt_v2, pt_startPoint1, pt_startPoint2); //放置弯头
                //        DPoint3d startDpoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(pt_startPoint2);
                //        DPoint3d endDpoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(pt_startPoint1);
                //        //bmecObjectList.Add(createPipe(startDpoint, pointList[pointList.Count - 1], 100));
                //        #region 测试有弯头的管道
                //        //pointList[pointList.Count - 2] = endDpoint;
                //        //pointList[pointList.Count - 1] = startDpoint;
                //        //pointList.Add(ev.Point);
                //        //bmecObjectList[bmecObjectList.Count - 1].SetLinearPoints(pointList[pointList.Count - 3], endDpoint);
                //        //bmecObjectList[bmecObjectList.Count - 1].Create();

                //        //bmecObjectList.Add(createPipe(startDpoint, ev.Point, 100));
                //        #endregion
                //    }
                //    //bmecObjectList.Add(createPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], 100));
                //    bmecObjectList.Add(createOriginalPipe(pointList[pointList.Count - 2], pointList[pointList.Count - 1], bmecDic[select]));
                //}
                #endregion
                app.ShowCommand("成组布管");
                app.ShowPrompt("绘制管道");
            }
            #endregion
            return false;
        }

        public DPoint3d last_dynamic_point = new DPoint3d();
        public DPoint3d[] fanwei = new DPoint3d[4];
        /// <summary>
        /// 动态显示
        /// </summary>
        /// <param name="ev"></param>
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            try
            {
                #region 测试
                if (!isDyPipe)
                {
                    Viewport active_view_port = Session.GetActiveViewport();
                    //last_dynamic_point = ev.Point;+++
                    if (testPointList.Count == 1)
                    {
                        Element redraw_elem = null;
                        DPoint3d[] redraw_points = new DPoint3d[4];
                        DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
                        DVector3d v1 = new DVector3d(viewBox[0], viewBox[1]);
                        DVector3d v2 = new DVector3d(viewBox[3], viewBox[1]);
                        DVector3d v3 = new DVector3d(viewBox[0], viewBox[2]);
                        DVector3d v4 = new DVector3d(viewBox[3], viewBox[2]);
                        DRay3d ray1 = new DRay3d(testPointList[0], v1);
                        DRay3d ray2 = new DRay3d(ev.Point, v2);
                        DRay3d ray3 = new DRay3d(testPointList[0], v3);
                        DRay3d ray4 = new DRay3d(ev.Point, v4);
                        DSegment3d segment, segment1;
                        double b1, b2, b3, b4;
                        bool bbb = DRay3d.ClosestApproachSegment(ray1, ray2, out segment, out b1, out b2);
                        bool bbbb = DRay3d.ClosestApproachSegment(ray3, ray4, out segment1, out b3, out b4);
                        List<DPoint3d> dpointList = new List<DPoint3d>();
                        dpointList.Add(testPointList[0]);
                        dpointList.Add(segment.StartPoint);
                        dpointList.Add(ev.Point);
                        dpointList.Add(segment1.StartPoint);
                        redraw_points = dpointList.ToArray();
                        ShapeElement shapeEle = new ShapeElement(Session.Instance.GetActiveDgnModel(), null, redraw_points);
                        fanwei = redraw_points;
                        redraw_elem = shapeEle as Element;
                        //if (testPointList.Count==1)
                        //{
                        //    LineStringElement line = new LineStringElement(Session.Instance.GetActiveDgnModel(), null, redraw_points);
                        //    redraw_elem = line as Element;
                        //}
                        //else
                        //{
                        //    ShapeElement shape = new ShapeElement(Session.Instance.GetActiveDgnModel(), null, redraw_points);
                        //    redraw_elem = shape as Element;
                        //}
                        if (redraw_elem != null)
                        {
                            RedrawElems redraw_elems = new RedrawElems();
                            redraw_elems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                            redraw_elems.DrawMode = DgnDrawMode.TempDraw;
                            redraw_elems.DrawPurpose = DrawPurpose.Dynamics;
                            redraw_elems.DoRedraw(redraw_elem);
                        }
                    }
                    return;
                }

                #endregion
                Element element = myDynamicFrameElement(ev);//动态显示的图形元素
                if (element == null) return;
                RedrawElems redrawElems = new RedrawElems();
                redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                redrawElems.DrawMode = DgnDrawMode.TempDraw;
                redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                redrawElems.DoRedraw(element);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

        }
        /// <summary>
        /// 动态显示调用的绘制图形的方法
        /// </summary>
        private Element myDynamicFrameElement(DgnButtonEvent ev)
        {
            if (pointList.Count == 0) return null;
            try
            {
                DVector3d qsDv1 = new DVector3d(0, 0, 1);
                DPoint3d dynamicPoint = ev.Point;
                double odc2 = bmecDic[select].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                double bwchd = bmecDic[select].Instance["INSULATION_THICKNESS"].DoubleValue;
                DPoint3d qdDp = pointList[pointList.Count - 1];
                if (from.radioCenter.Checked)
                {
                    if (from.isCenter)
                    {
                        AccuDraw.Active = true;
                        AccuDraw.Origin = pointList[pointList.Count - 1];
                        from.isCenter = false;
                        app.StartBusyCursor();
                    }
                }
                if (from.radioTop.Checked)
                {
                    DPoint3d orginP = pointList[pointList.Count - 1];

                    bool bbb1 = distence(orginP, ev.Point);
                    if (bbb1)
                    {
                        return null;
                    }

                    DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                    DPoint3d dp2 = pointList[pointList.Count - 2];
                    DVector3d dv1 = new DVector3d(dp2, orginP);
                    DVector3d dv2 = new DVector3d(1, 0, 0);

                    DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                    DVector3d dvTop = new DVector3d(0, 0, 1);

                    qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvTop);

                    dvTop = qsDv1;

                    DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                    DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                    DPoint3d mubiao = orginP + yPipeDv;

                    bool bbb = distence(mubiao, ev.Point);
                    if (bbb)
                    {
                        //System.Windows.Forms.MessageBox.Show("崩溃！");
                        return null;
                    }

                    DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                    Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                    List<DPoint3d> zhongDpList = new List<DPoint3d>();
                    zhongDpList.Add(pointList[pointList.Count - 2]);
                    zhongDpList.Add(pointList[pointList.Count - 1]);
                    zhongdianList.Add(0, zhongDpList);
                    Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                    //if (from.isTop)
                    //{
                    //    AccuDraw.Active = true;
                    //    AccuDraw.Origin = mubiao;
                    //    from.isTop = false;
                    //    app.StartBusyCursor();
                    //}
                    bool isThAcc = distence(mubiao, AccuDraw.Origin);
                    if (!isThAcc)
                    {
                        AccuDraw.Active = true;
                        AccuDraw.Origin = mubiao;
                        app.StartBusyCursor();
                    }
                    //DVector3d scDv = new DVector3d(mubiao, ev.Point);
                    //DVector3d yDv = new DVector3d(1, 0, 0);
                    //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                    //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop);
                    //DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                    ////DPoint3d dpTop = DPoint3d.Add(orginP, dvM, (odc2 + bwchd) * 1000);
                    //DPoint3d dpTop = orginP + suofDv;
                    ////AccuDraw.Origin = dpTop;
                    //dynamicPoint = ev.Point - suofDv;
                    //qdDp = mubiao - suofDv;
                    dynamicPoint = scZD[0][1];
                    qdDp = scZD[0][0];
                    DPoint3d touyingDp = touyin(mubiao, scZD[0][0], scZD[0][1]);
                    qdDp = touyingDp;
                    //DMatrix3d dmPipe = bmecDic[select].Transform3d.Matrix; //管道的旋转矩阵
                    //                                                       //app.Point3dFromTransform3dTimesPoint3d
                    //DVector3d dvTop = new DVector3d(0, 0, 1);
                    //DVector3d dvM = DMatrix3d.Multiply(dmPipe, dvTop); //向量与旋转矩阵的乘积
                    //AccuDraw.Active = true;
                    //DPoint3d dpTop = DPoint3d.Add(ev.Point, dvM, (odc2 + bwchd) * 1000);
                    //AccuDraw.Origin = dpTop;
                }
                if (from.radioDown.Checked)
                {
                    DPoint3d orginP = pointList[pointList.Count - 1];

                    bool bbb1 = distence(orginP, ev.Point);
                    if (bbb1)
                    {
                        return null;
                    }


                    DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                    DPoint3d dp2 = pointList[pointList.Count - 2];
                    DVector3d dv1 = new DVector3d(dp2, orginP);
                    DVector3d dv2 = new DVector3d(1, 0, 0);

                    DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                    DVector3d dvDown = new DVector3d(0, 0, -1);

                    qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvDown);

                    dvDown = qsDv1;

                    DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvDown);
                    DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                    DPoint3d mubiao = orginP + yPipeDv;
                    bool bbb = distence(mubiao, ev.Point);
                    if (bbb)
                    {
                        //System.Windows.Forms.MessageBox.Show("崩溃！");
                        return null;
                    }

                    DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                    Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                    List<DPoint3d> zhongDpList = new List<DPoint3d>();
                    zhongDpList.Add(pointList[pointList.Count - 2]);
                    zhongDpList.Add(pointList[pointList.Count - 1]);
                    zhongdianList.Add(0, zhongDpList);
                    Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                    //if (from.isDown)
                    //{
                    //    AccuDraw.Active = true;
                    //    AccuDraw.Origin = mubiao;
                    //    from.isDown = false;
                    //    app.StartBusyCursor();
                    //}

                    bool isThAcc = distence(mubiao, AccuDraw.Origin);
                    if (!isThAcc)
                    {
                        AccuDraw.Active = true;
                        AccuDraw.Origin = mubiao;
                        app.StartBusyCursor();
                    }

                    //DVector3d scDv = new DVector3d(mubiao, ev.Point);
                    //DVector3d yDv = new DVector3d(1, 0, 0);
                    //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                    ////DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                    //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvDown);
                    //DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                    //DPoint3d dpDown = orginP + suofDv;
                    ////AccuDraw.Origin = dpDown;
                    //dynamicPoint = ev.Point - suofDv;
                    //qdDp = mubiao - suofDv;
                    dynamicPoint = scZD[0][1];
                    qdDp = scZD[0][0];
                    DPoint3d touyingDp = touyin(mubiao, scZD[0][0], scZD[0][1]);
                    qdDp = touyingDp;
                }
                if (from.radioLeft.Checked)
                {
                    DPoint3d orginP = pointList[pointList.Count - 1];

                    bool bbb1 = distence(orginP, ev.Point);
                    if (bbb1)
                    {
                        return null;
                    }

                    DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                    DPoint3d dp2 = pointList[pointList.Count - 2];
                    DVector3d dv1 = new DVector3d(dp2, orginP);
                    DVector3d dv2 = new DVector3d(1, 0, 0);

                    DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                    DVector3d dvLeft = new DVector3d(0, 1, 0);

                    qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvLeft);

                    dvLeft = qsDv1;

                    DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvLeft);
                    DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                    DPoint3d mubiao = orginP + yPipeDv;

                    bool bbb = distence(mubiao, ev.Point);
                    if (bbb)
                    {
                        //System.Windows.Forms.MessageBox.Show("崩溃！");
                        return null;
                    }

                    DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                    Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                    List<DPoint3d> zhongDpList = new List<DPoint3d>();
                    zhongDpList.Add(pointList[pointList.Count - 2]);
                    zhongDpList.Add(pointList[pointList.Count - 1]);
                    zhongdianList.Add(0, zhongDpList);
                    Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                    //if (from.isLeft)
                    //{
                    //    AccuDraw.Active = true;
                    //    AccuDraw.Origin = mubiao;
                    //    from.isLeft = false;
                    //    app.StartBusyCursor();
                    //}
                    bool isThAcc = distence(mubiao, AccuDraw.Origin);
                    if (!isThAcc)
                    {
                        AccuDraw.Active = true;
                        AccuDraw.Origin = mubiao;
                        app.StartBusyCursor();
                    }
                    //DVector3d scDv = new DVector3d(mubiao, ev.Point);
                    //DVector3d yDv = new DVector3d(1, 0, 0);
                    //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                    ////DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                    //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvLeft);
                    //DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                    //DPoint3d dpLeft = orginP + suofDv;
                    ////AccuDraw.Origin = dpLeft;
                    //dynamicPoint = ev.Point - suofDv;
                    //qdDp = mubiao - suofDv;
                    dynamicPoint = scZD[0][1];
                    qdDp = scZD[0][0];
                    DPoint3d touyingDp = touyin(mubiao, scZD[0][0], scZD[0][1]);
                    qdDp = touyingDp;
                }
                if (from.radioRight.Checked)
                {
                    DPoint3d orginP = pointList[pointList.Count - 1];

                    bool bbb1 = distence(orginP, ev.Point);
                    if (bbb1)
                    {
                        return null;
                    }

                    DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                    DPoint3d dp2 = pointList[pointList.Count - 2];
                    DVector3d dv1 = new DVector3d(dp2, orginP);
                    DVector3d dv2 = new DVector3d(1, 0, 0);

                    DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv2, dv1, ref dm3d1);

                    DVector3d dvRight = new DVector3d(0, -1, 0);

                    qsDv1 = qsDv(pointList[pointList.Count - 1], ev.Point, pointList[pointList.Count - 2], dvRight);

                    dvRight = qsDv1;

                    DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvRight);
                    DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                    DPoint3d mubiao = orginP + yPipeDv;

                    bool bbb = distence(mubiao, ev.Point);
                    if (bbb)
                    {
                        //System.Windows.Forms.MessageBox.Show("崩溃！");
                        return null;
                    }

                    DPoint3d mubiao1 = pointList[pointList.Count - 2] + yPipeDv;
                    Dictionary<int, List<DPoint3d>> zhongdianList = new Dictionary<int, List<DPoint3d>>();
                    List<DPoint3d> zhongDpList = new List<DPoint3d>();
                    zhongDpList.Add(pointList[pointList.Count - 2]);
                    zhongDpList.Add(pointList[pointList.Count - 1]);
                    zhongdianList.Add(0, zhongDpList);
                    Dictionary<int, List<DPoint3d>> scZD = pianyiPipe(ev.Point, mubiao1, mubiao, zhongdianList);

                    //if (from.isRegiht)
                    //{
                    //    AccuDraw.Active = true;
                    //    AccuDraw.Origin = mubiao;
                    //    from.isRegiht = false;
                    //    app.StartBusyCursor();
                    //}
                    bool isThAcc = distence(mubiao, AccuDraw.Origin);
                    if (!isThAcc)
                    {
                        AccuDraw.Active = true;
                        AccuDraw.Origin = mubiao;
                        app.StartBusyCursor();
                    }
                    //DVector3d scDv = new DVector3d(mubiao, ev.Point);
                    //DVector3d yDv = new DVector3d(1, 0, 0);
                    //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                    //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);
                    ////DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;

                    //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvRight);
                    //DVector3d suofDv = (odc2 + bwchd) * 1000 * dvM;
                    //DPoint3d dpRight = orginP + suofDv;
                    ////AccuDraw.Origin = dpRight;
                    //dynamicPoint = ev.Point - suofDv;
                    //qdDp = mubiao - suofDv;
                    dynamicPoint = scZD[0][1];
                    qdDp = scZD[0][0];
                    DPoint3d touyingDp = touyin(mubiao, scZD[0][0], scZD[0][1]);
                    qdDp = touyingDp;
                }
                //dynamicPoint = ev.Point;
                //DVector3d nomal = new DVector3d();
                //int index = 0;
                //if (select == 0)
                //{
                //    index = select + 1;
                //}
                if (shengchengList.Count > 0)
                {
                    //DPoint3d dpo = shengchengList[index][shengchengList[index].Count - 1];
                    DPoint3d dp1 = pointList[pointList.Count - 2];
                    DPoint3d dp2 = pointList[pointList.Count - 1];
                    //DVector3d dv1 = new DVector3d(dp1, dpo);
                    DVector3d dv2 = new DVector3d(dp1, dp2);
                    //nomal = dv1.CrossProduct(dv2);
                    DVector3d dv3 = new DVector3d(pointList[pointList.Count - 1], dynamicPoint);
                    bool pingx = dv2.IsParallelOrOppositeTo(dv3);
                    if (pingx)
                    {
                        foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                        {
                            DPoint3d dpoint = kv.Value[kv.Value.Count - 1] + dv3;
                            BMECObject pipeDy = null;
                            pipeDy = createOriginalPipe(kv.Value[kv.Value.Count - 1], dpoint, bmecDic[kv.Key]);
                            JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                        }
                    }
                    else
                    {
                        if (from.radioCenter.Checked)
                        {
                            DPoint3d startDp = pointList[pointList.Count - 2];
                            DPoint3d endDp = pointList[pointList.Count - 1];
                            Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(dynamicPoint, startDp, endDp, shengchengList);
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                BMECObject pipeDy1 = null;
                                pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0], bmecDic[kv.Key]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                BMECObject pipeDy = null;
                                pipeDy = createOriginalPipe(kv.Value[0], kv.Value[1], bmecDic[kv.Key]); ;
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                            }
                        }
                        if (from.radioTop.Checked)
                        {
                            Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                            {
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                DVector3d scDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                DVector3d dvTop1 = new DVector3d(0, 0, 1);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                List<DPoint3d> mgList = new List<DPoint3d>();
                                mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                topList.Add(kv.Key, mgList);
                            }
                            DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                            DPoint3d yDp1 = pointList[pointList.Count - 2];
                            DPoint3d yDp2 = pointList[pointList.Count - 1];
                            DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                            DVector3d yDv2 = new DVector3d(1, 0, 0);

                            DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                            DVector3d dvTop = new DVector3d(0, 0, 1);

                            dvTop = qsDv1;

                            DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                            DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                            DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                            DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                            Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(ev.Point, startDp, endDp, topList);
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                if (isscDp)
                                {
                                    return null;
                                }
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                DVector3d dvTop1 = new DVector3d(0, 0, 1);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;

                                Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zList = new List<DPoint3d>();
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                zZDList.Add(0, zList);
                                Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                dtscStartDp = scZDList[0][0];

                                BMECObject pipeDy1 = null;
                                pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                //DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                //DVector3d yDv = new DVector3d(1, 0, 0);
                                //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                //DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                //DPoint3d scDp1 = kv.Value[0] - suofDv;
                                //DPoint3d scDp2 = kv.Value[1] - suofDv;

                                DPoint3d scDp1 = scZDList[0][0];
                                DPoint3d scDp2 = scZDList[0][1];
                                DPoint3d touyingDp = touyin(kv.Value[0], scZDList[0][0], scZDList[0][1]);
                                scDp1 = touyingDp;

                                BMECObject pipeDy = null;
                                pipeDy = createOriginalPipe(scDp1, scDp2, shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                            }
                        }
                        if (from.radioDown.Checked)
                        {
                            Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                            {
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                DVector3d scDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                DVector3d dvTop1 = new DVector3d(0, 0, -1);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                List<DPoint3d> mgList = new List<DPoint3d>();
                                mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                topList.Add(kv.Key, mgList);
                            }
                            DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                            DPoint3d yDp1 = pointList[pointList.Count - 2];
                            DPoint3d yDp2 = pointList[pointList.Count - 1];
                            DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                            DVector3d yDv2 = new DVector3d(1, 0, 0);

                            DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                            DVector3d dvTop = new DVector3d(0, 0, -1);

                            dvTop = qsDv1;

                            DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                            DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                            DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                            DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                            Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(ev.Point, startDp, endDp, topList);
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                if (isscDp)
                                {
                                    return null;
                                }
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                DVector3d dvTop1 = new DVector3d(0, 0, -1);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;

                                Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zList = new List<DPoint3d>();
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                zZDList.Add(0, zList);
                                Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                dtscStartDp = scZDList[0][0];

                                BMECObject pipeDy1 = null;
                                pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                //DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                //DVector3d yDv = new DVector3d(1, 0, 0);
                                //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                //DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                //DPoint3d scDp1 = kv.Value[0] - suofDv;
                                //DPoint3d scDp2 = kv.Value[1] - suofDv;
                                DPoint3d scDp1 = scZDList[0][0];
                                DPoint3d scDp2 = scZDList[0][1];
                                DPoint3d touyingDp = touyin(kv.Value[0], scZDList[0][0], scZDList[0][1]);
                                scDp1 = touyingDp;
                                BMECObject pipeDy = null;
                                pipeDy = createOriginalPipe(scDp1, scDp2, shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                            }
                        }
                        if (from.radioLeft.Checked)
                        {
                            Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                            {
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                DVector3d scDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                DVector3d dvTop1 = new DVector3d(0, 1, 0);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                List<DPoint3d> mgList = new List<DPoint3d>();
                                mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                topList.Add(kv.Key, mgList);
                            }
                            DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                            DPoint3d yDp1 = pointList[pointList.Count - 2];
                            DPoint3d yDp2 = pointList[pointList.Count - 1];
                            DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                            DVector3d yDv2 = new DVector3d(1, 0, 0);

                            DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                            DVector3d dvTop = new DVector3d(0, 1, 0);

                            dvTop = qsDv1;

                            DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                            DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                            DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                            DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                            Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(ev.Point, startDp, endDp, topList);
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                if (isscDp)
                                {
                                    return null;
                                }
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                DVector3d dvTop1 = new DVector3d(0, 1, 0);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;

                                Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zList = new List<DPoint3d>();
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                zZDList.Add(0, zList);
                                Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                dtscStartDp = scZDList[0][0];

                                BMECObject pipeDy1 = null;
                                pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                //DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                //DVector3d yDv = new DVector3d(1, 0, 0);
                                //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                //DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                //DPoint3d scDp1 = kv.Value[0] - suofDv;
                                //DPoint3d scDp2 = kv.Value[1] - suofDv;
                                DPoint3d scDp1 = scZDList[0][0];
                                DPoint3d scDp2 = scZDList[0][1];
                                DPoint3d touyingDp = touyin(kv.Value[0], scZDList[0][0], scZDList[0][1]);
                                scDp1 = touyingDp;
                                BMECObject pipeDy = null;
                                pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                            }
                        }
                        if (from.radioRight.Checked)
                        {
                            Dictionary<int, List<DPoint3d>> topList = new Dictionary<int, List<DPoint3d>>();
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                            {
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                                DPoint3d scDp1 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 2];
                                DPoint3d scDp2 = shengchengList[kv.Key][shengchengList[kv.Key].Count - 1];
                                DVector3d scDv1 = new DVector3d(scDp1, scDp2);
                                DVector3d scDv2 = new DVector3d(1, 0, 0);

                                DMatrix3d dm3d2 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(scDv2, scDv1, ref dm3d2);

                                DVector3d dvTop1 = new DVector3d(0, -1, 0);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dm3d2, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                List<DPoint3d> mgList = new List<DPoint3d>();
                                mgList.Add(kv.Value[kv.Value.Count - 2] + yPipeDv1);
                                mgList.Add(kv.Value[kv.Value.Count - 1] + yPipeDv1);
                                topList.Add(kv.Key, mgList);
                            }
                            DMatrix3d dmPipe = bmecObjectList[bmecObjectList.Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵

                            DPoint3d yDp1 = pointList[pointList.Count - 2];
                            DPoint3d yDp2 = pointList[pointList.Count - 1];
                            DVector3d yDv1 = new DVector3d(yDp1, yDp2);
                            DVector3d yDv2 = new DVector3d(1, 0, 0);

                            DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, yDv1, ref dm3d1);

                            DVector3d dvTop = new DVector3d(0, -1, 0);

                            dvTop = qsDv1;

                            DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d1, dvTop);
                            DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
                            DPoint3d startDp = pointList[pointList.Count - 2] + yPipeDv;
                            DPoint3d endDp = pointList[pointList.Count - 1] + yPipeDv;
                            Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(ev.Point, startDp, endDp, topList);
                            foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                            {
                                bool isscDp = distence(kv.Value[0], kv.Value[1]);
                                if (isscDp)
                                {
                                    return null;
                                }
                                double odc21 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["OUTSIDE_DIAMETER"].DoubleValue / 2;
                                double bwchd1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Instance["INSULATION_THICKNESS"].DoubleValue;
                                DMatrix3d dmPipe1 = shencBmecList[kv.Key][shencBmecList[kv.Key].Count - 1].Transform3d.Matrix;//不要用这个旋转矩阵
                                DVector3d dvTop1 = new DVector3d(0, -1, 0);

                                dvTop1 = qsDv1;

                                DVector3d yPipeUnitDv1 = DMatrix3d.Multiply(dmPipe1, dvTop1);
                                DVector3d yPipeDv1 = (odc2 + bwchd) * 1000 * yPipeUnitDv1;
                                DPoint3d dtscStartDp = kv.Value[0] - yPipeDv1;

                                Dictionary<int, List<DPoint3d>> zZDList = new Dictionary<int, List<DPoint3d>>();
                                List<DPoint3d> zList = new List<DPoint3d>();
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2]);
                                zList.Add(shengchengList[kv.Key][shengchengList[kv.Key].Count - 1]);
                                zZDList.Add(0, zList);
                                Dictionary<int, List<DPoint3d>> scZDList = pianyiPipe(kv.Value[1], topList[kv.Key][topList[kv.Key].Count - 2], kv.Value[0], zZDList);

                                dtscStartDp = scZDList[0][0];

                                BMECObject pipeDy1 = null;
                                pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], dtscStartDp, bmecDic[kv.Key]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                                //DVector3d scDv = new DVector3d(kv.Value[0], kv.Value[1]);
                                //DVector3d yDv = new DVector3d(1, 0, 0);
                                //DMatrix3d dm3d = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
                                //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv, scDv, ref dm3d);

                                //DVector3d dvM = DMatrix3d.Multiply(dm3d, dvTop1);
                                //DVector3d suofDv = (odc21 + bwchd1) * 1000 * dvM;
                                //DPoint3d scDp1 = kv.Value[0] - suofDv;
                                //DPoint3d scDp2 = kv.Value[1] - suofDv;
                                DPoint3d scDp1 = scZDList[0][0];
                                DPoint3d scDp2 = scZDList[0][1];
                                DPoint3d touyingDp = touyin(kv.Value[0], scZDList[0][0], scZDList[0][1]);
                                scDp1 = touyingDp;
                                BMECObject pipeDy = null;
                                pipeDy = createOriginalPipe(scDp1, scDp2, bmecDic[kv.Key]);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                            }
                        }
                        //Dictionary<int, List<DPoint3d>> dongtList = pianyiPipe(dynamicPoint);

                    }
                    #region 共面时处理
                    //bool cz = nomal.IsPerpendicularTo(dv3);
                    //if (!cz)
                    //{
                    //    Dictionary<int, List<DPoint3d>> dongtList = ceyimian(dynamicPoint);
                    //    foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                    //    {
                    //        BMECObject pipeDy = null;
                    //        pipeDy = createOriginalPipe(kv.Value[0], kv.Value[1],bmecDic[kv.Key]);
                    //        JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                    //    }
                    //}
                    //else
                    //{
                    //    Dictionary<int, List<DPoint3d>> dongtList = cegongmian(dynamicPoint);
                    //    foreach (KeyValuePair<int, List<DPoint3d>> kv in dongtList)
                    //    {
                    //        BMECObject pipeDy1 = null;
                    //        pipeDy1 = createOriginalPipe(shengchengList[kv.Key][shengchengList[kv.Key].Count - 2], kv.Value[0], bmecDic[kv.Key]);
                    //        JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy1);
                    //        BMECObject pipeDy = null;
                    //        pipeDy = createOriginalPipe(kv.Value[0], kv.Value[1], bmecDic[kv.Key]); ;
                    //        JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDy);
                    //    }
                    //}
                    #endregion
                }

                BMECObject pipeDynamic = null;
                if (bmec_object != null)
                {
                    pipeDynamic = createOriginalPipe(qdDp, dynamicPoint, bmec_object);
                    //DTransform3d dtran = pipeDynamic.Transform3d;

                    //pipeDynamic.SetLinearPoints(qdDp, dynamicPoint);

                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDynamic);
                    //dtran =pipeDynamic.Transform3d;
                    //DTransform3d dtran1  ;
                    //dtran.TryInvert(out dtran1);
                    //pipeDynamic.Transform3d = dtran1;
                    //DVector3d dvec =pipeDynamic.Transform3d.RowX;
                    //pipeDynamic.Transform3d = DTransform3d.Identity;
                    //DPoint3d[] dpts  =JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(pipeDynamic);
                    //DVector3d dv2 = new DVector3d(dpts[0], dpts[1]);
                    //JYX_ZYJC_CLR.PublicMethod.display_bmec_object(pipeDynamic);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                return null;
            }
            return null;
        }

        /// <summary>
        /// 创建管道
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="dn"></param>
        /// <returns></returns>
        private BMECObject createPipe(DPoint3d startPoint, DPoint3d endPoint, double dn)
        {
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);//创建一个PIPE的ECInstance
            BMECApi api = BMECApi.Instance;
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = dn;//设置管径

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
        /// 绘制选中的管道
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="originalBmecobject"></param>
        /// <returns></returns>
        private BMECObject createOriginalPipe(DPoint3d startPoint, DPoint3d endPoint, BMECObject originalBmecobject)
        {
            //IECInstance orginInstance = BMECInstanceManager.Instance.CreateECInstance(originalBmecobject.ClassName, true);
            //BMECApi api = BMECApi.Instance;
            //ISpecProcessor spec = api.SpecProcessor;
            //spec.FillCurrentPreferences(orginInstance, null);
            //orginInstance["NOMINAL_DIAMETER"].DoubleValue = originalBmecobject.GetDoubleValueInMM("NOMINAL_DIAMETER");

            //ECInstanceList ecList = spec.SelectSpec(orginInstance, true);
            BMECObject bmecobject = new BMECObject();
            bmecobject.Copy(originalBmecobject);
            bmecobject.SetLinearPoints(startPoint, endPoint);
            //bmecobject.Create();
            double tempod = bmecobject.GetDoubleValueInMM("OUTSIDE_DIAMETER");
            //bmecobject.DiscoverConnectionsEx();
            //bmecobject.UpdateConnections();
            return bmecobject;
        }

        private void updateOriginalPipe(DPoint3d startPoint, DPoint3d endPoint, BMECObject originalBmecobject)
        {
            string spec = originalBmecobject.Instance["SPECIFICATION"].StringValue;
            string lineNumber = originalBmecobject.Instance["LINENUMBER"].StringValue;
            //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
            //StandardPreferencesUtilities.ChangeSpecification(spec);
            if (pipelinesName.Count > 0)
            {
                int index = -1;
                index = pipelinesName.IndexOf(lineNumber);
                if (index != -1)
                {
                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index]);
                    StandardPreferencesUtilities.ChangeSpecification(spec);
                }
            }
            originalBmecobject.SetLinearPoints(startPoint, endPoint);
            originalBmecobject.Create();
            originalBmecobject.DiscoverConnectionsEx();
            originalBmecobject.UpdateConnections();
        }

        /// <summary>
        /// 创建弯头(弃用)
        /// </summary>
        /// <param name="elbowECClassName"></param>
        /// <param name="nominal_diameter"></param>
        /// <param name="angle"></param>
        /// <param name="center_to_outlet_end"></param>
        /// <param name="center_to_run_end"></param>
        /// <param name="insulation_thickness"></param>
        /// <param name="insulation"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static BMECObject createElbow(string elbowECClassName, double nominal_diameter, double angle, double center_to_outlet_end, double center_to_run_end, double insulation_thickness, string insulation, out string errorMessage)
        {
            errorMessage = string.Empty;
            IECInstance iECInstance = BMECInstanceManager.Instance.CreateECInstance(elbowECClassName, true);
            ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
            isp.FillCurrentPreferences(iECInstance, null);
            iECInstance["NOMINAL_DIAMETER"].DoubleValue = nominal_diameter;
            ECInstanceList eCInstanceList = isp.SelectSpec(iECInstance, true);
            BMECObject result = null;
            if (eCInstanceList.Count == 0)
            {
                errorMessage = "当前Specification未配置弯管。";
                return null;
            }
            if (eCInstanceList != null && eCInstanceList.Count > 0)
            {
                IECInstance ecinstance = eCInstanceList[0];
                ecinstance["ANGLE"].DoubleValue = angle; //角度
                ecinstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = center_to_outlet_end;
                //ecinstance["EC_CLASS_NAME"].StringValue = elbowECClassName;
                ecinstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = center_to_run_end;
                ecinstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                ecinstance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
                ecinstance["INSULATION"].StringValue = insulation;
                result = new BMECObject(ecinstance);
                result.Create();
            }
            return result;
        }

        /// <summary>
        /// 创建弯头
        /// </summary>
        /// <param name="startPipeDpoint"></param>
        /// <param name="lianjieDpoint"></param>
        /// <param name="endPipeDpoint"></param>
        /// <param name="bmec"></param>
        /// <param name="bmec1"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public List<BMECObject> createElbowOrBend(DPoint3d startPipeDpoint, DPoint3d lianjieDpoint, DPoint3d endPipeDpoint, BMECObject bmec, out string errorMessage, int index)
        {
            DPoint3d intersect = lianjieDpoint;
            Bentley.GeometryNET.DPoint3d[] line1Point = new DPoint3d[] { startPipeDpoint, lianjieDpoint };
            Bentley.GeometryNET.DPoint3d[] line2Point = new DPoint3d[] { lianjieDpoint, endPipeDpoint };
            List<BMECObject> elbows = new List<BMECObject>();
            bool isSingleElbow = true;
            int unbrokenElbow = 0;
            double brokenElbowDgree = 0.0f;
            double elbowDegree = 0.0f;
            double cankaoCenterToMainPort = 0.0f;
            string spec = bmec.Instance["SPECIFICATION"].StringValue;
            string lineNumber = bmec.Instance["LINENUMBER"].StringValue;
            if (pipelinesName.Count > 0)
            {
                int index1 = -1;
                index1 = pipelinesName.IndexOf(lineNumber);
                if (index1 != -1)
                {
                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index1]);
                    StandardPreferencesUtilities.ChangeSpecification(spec);
                }
            }
            //BMECObject bmec1 = createOriginalPipe(lianjieDpoint,endPipeDpoint,bmecobject);
            app.ShowCommand("成组布管");
            app.ShowPrompt("序号" + (index + 1) + "布管");
            elbowOrBend elbowOrBendClass = from.pipeElbowOrBendList[index];
            errorMessage = string.Empty;
            BMECObject elbowBend = new BMECObject();
            BIM.Point3d firstP = ceDpoint_v8Point(startPipeDpoint);
            BIM.Point3d secondP = ceDpoint_v8Point(lianjieDpoint);
            BIM.Point3d threeP = ceDpoint_v8Point(endPipeDpoint);
            BIM.Point3d v1 = app.Point3dSubtract(secondP, firstP);
            BIM.Point3d v2 = app.Point3dSubtract(threeP, secondP);
            double angle = Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
            string elbowOrBendECClassName = "";
            double nominalDiameter = bmec.GetDoubleValueInMM("NOMINAL_DIAMETER");
            double dn1 = bmec.GetDoubleValueInMM("NOMINAL_DIAMETER");
            double dn2 = bmec.GetDoubleValueInMM("NOMINAL_DIAMETER");
            double insulationThickness = bmec.Instance["INSULATION_THICKNESS"].DoubleValue;
            string insulation = bmec.Instance["INSULATION"].StringValue;
            double wallThickness = bmec.Instance["WALL_THICKNESS"].DoubleValue;
            string material = bmec.Instance["MATERIAL"].StringValue;
            double centerToMainPort = 0.0;
            double centerToRunPort = 0.0;
            double lengthToBend = 0.0;
            double lengthAfterBend = 0.0;
            double length1 = 0.0;
            double length2 = 0.0;
            double d1, d2;
            d1 = app.Point3dDistance(firstP, secondP);
            d2 = app.Point3dDistance(secondP, threeP);
            double radius = 0.0;

            if (elbowOrBendClass.elbowOrBendName == "Elbow")
            {
                //if (elbowOrBendClass.bendType == "Bend")
                //{
                //    elbowOrBendECClassName = "PIPE_BEND";
                //}
                //else if (elbowOrBendClass.bendType == "虾米弯")
                //{
                //    elbowOrBendECClassName = "MITERED_PIPE_BEND_JYX";
                //}
                double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                if (!elbowOrBendClass.isBzQgWt)
                {
                    if (Math.Abs(cankaoAngle - angle) > 1)
                    {
                        errorMessage = "当前条件下，没有找到对应角度的弯头！";
                        //elbows = null;
                        return null;
                    }
                }
                else
                {
                    if (angle <= cankaoAngle)
                    {
                        isSingleElbow = true;
                    }
                    else
                    {
                        unbrokenElbow = (int)(angle / cankaoAngle);
                        brokenElbowDgree = angle % cankaoAngle;
                        if (unbrokenElbow > 2 || (brokenElbowDgree > 1e-1 && unbrokenElbow > 1))
                        {
                            errorMessage = "当前角度下需要连接的弯头数量大于2，请使用其它角度弯头连接或更改连接方式！";
                            //elbows = null;
                            return null;
                        }
                        elbowDegree = cankaoAngle;
                        isSingleElbow = false;
                    }
                }
                if (elbowOrBendClass.elbowRadius != "虾米弯")
                {
                    elbowOrBendECClassName = getElbowECClassName(elbowOrBendClass.elbowRadius, elbowOrBendClass.elbowAngle);
                }
                else if (elbowOrBendClass.elbowRadius == "虾米弯")
                {
                    elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX";
                }
                elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass为"+ elbowOrBendECClassName + "类型，请确认已配置该类型";
                    return null;
                }
                ECInstanceList eCInstanceList = null;
                if (elbowOrBendClass.elbowRadius == "虾米弯")
                {
                    ISpecProcessor isp = api.SpecProcessor;
                    isp.FillCurrentPreferences(elbowOrBendInstance, null);
                    elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                    //double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                    Hashtable whereClauseList = new Hashtable();
                    whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                    whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                    whereClauseList.Add("wanqu_jiaodu", cankaoAngle);
                    eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                }
                else
                {
                    ISpecProcessor isp = api.SpecProcessor;
                    isp.FillCurrentPreferences(elbowOrBendInstance, null);
                    elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                    eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                }

                if (eCInstanceList.Count == 0)
                {
                    errorMessage = "没有找到该ECClass为"+ elbowOrBendECClassName + "的对应数据项，请确认已配置数据";
                    return null;
                }
                elbowOrBendInstance = eCInstanceList[0];

                centerToMainPort = elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                cankaoCenterToMainPort = centerToMainPort;
                centerToRunPort = centerToMainPort;

                BMECObject tempObject = new BMECObject(elbowOrBendInstance);

                if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["SOCKET_DEPTH"].DoubleValue;
                }
                else if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["THREADED_LENGTH"].DoubleValue;
                }
                double anglera = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                double radus = Angle.DegreesToRadians(anglera);
                radius = (centerToMainPort - lengthToBend) / Math.Tan(radus / 2);
                double centerToPort = radius * Math.Tan(Angle.DegreesToRadians(angle / 2.0));
                centerToMainPort = centerToPort + lengthToBend;
                centerToRunPort = centerToMainPort;
                //double tanA = Math.Tan(Angle.DegreesToRadians((180.0 - angle) / 2.0));
                //radius = (centerToMainPort - lengthToBend) * tanA; //半径

                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                //DESIGN_LENGTH_CENTER_TO_RUN_END
                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;
                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
                if (elbowOrBendClass.elbowRadius == "虾米弯")
                {
                    //elbowOrBendInstance["NUM_PIECES"].IntValue = elbowOrBendClass.pitchNumber;
                    radius = elbowOrBendInstance["wanqu_banjing"].DoubleValue;
                    //centerToMainPort = radius;
                    //centerToRunPort = centerToMainPort;
                    double centerToPort1 = radius * Math.Tan(Angle.DegreesToRadians(angle / 2.0));
                    centerToMainPort = centerToPort1 + lengthToBend;
                    centerToRunPort = centerToPort1 + lengthAfterBend;

                    //elbowOrBendInstance["jieshu"].IntValue = elbowOrBendClass.pitchNumber;
                    //elbowOrBendInstance["wanqu_banjing"].DoubleValue = radius;
                    elbowOrBendInstance["wanqu_jiaodu"].DoubleValue = angle;

                    length1 = centerToMainPort - lengthToBend;
                    length2 = centerToRunPort - lengthAfterBend;
                    elbowOrBendInstance["caizhi"].StringValue = "";
                    elbowOrBendInstance["bihou"].DoubleValue = wallThickness;
                }
                else
                {
                    length1 = centerToMainPort - lengthToBend;
                    length2 = centerToRunPort - lengthAfterBend;
                }
            }

            else
            {
                lengthToBend = elbowOrBendClass.bendFrontLong;
                lengthAfterBend = elbowOrBendClass.bendAfterLong;
                radius = elbowOrBendClass.bendRadius;
                elbowOrBendECClassName = "PIPE_BEND";
                elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
                    return null;
                }
                ISpecProcessor isp = api.SpecProcessor;
                isp.FillCurrentPreferences(elbowOrBendInstance, null);
                elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                if (eCInstanceList.Count == 0)
                {
                    errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
                    return null;
                }
                elbowOrBendInstance = eCInstanceList[0];

                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                //elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
                elbowOrBendInstance["LENGTH_TO_BEND"].DoubleValue = lengthToBend;
                elbowOrBendInstance["LENGTH_AFTER_BEND"].DoubleValue = lengthAfterBend;
                elbowOrBendInstance["BEND_POINT_RADIUS"].DoubleValue = elbowOrBendClass.bendRadiusRatio;

                double tanA = Math.Tan(Angle.DegreesToRadians((180.0 - angle) / 2.0));
                centerToMainPort = radius / tanA;
                centerToRunPort = centerToMainPort;

                length1 = centerToMainPort + lengthToBend;
                length2 = centerToRunPort + lengthAfterBend;


            }

            //length1 = centerToMainPort - lengthToBend;
            //length2 = centerToRunPort - lengthAfterBend;

            BIM.Point3d start_point1;
            BIM.Point3d start_point2;
            start_point1 = app.Point3dSubtract(secondP, app.Point3dScale(app.Point3dSubtract(secondP, firstP), length1 / d1)); //点减向量 得到新的点
            start_point2 = app.Point3dSubtract(secondP, app.Point3dScale(app.Point3dSubtract(secondP, threeP), length2 / d2));
            elbowBend = new BMECObject(elbowOrBendInstance);
            if (elbowBend == null)
            {
                errorMessage = "无法通过该实例创建对象";
                return null;
            }
            BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
            //BIM.Point3d test = app.Point3dFromXYZ(1000, 0, 0);
            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec, firstP, start_point1); //v8i方法需要修改长度
            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec1, start_point2, threeP);
            start_dpoint1 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1);
            DVector3d dv1 = new DVector3d(startPipeDpoint, lianjieDpoint);
            DVector3d dv2 = new DVector3d(startPipeDpoint, start_dpoint1);
            bool b1 = dv1.IsParallelTo(dv2);
            if (!b1)
            {
                errorMessage = "存在序号为" + (index + 1) + "的管道生成弯头后过短";
                //System.Windows.Forms.MessageBox.Show("存在序号为"+(index+1)+"的管道生成弯头后过短");
                return null;
            }
            start_dpoint2 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2);
            DVector3d dv3 = new DVector3d(lianjieDpoint, endPipeDpoint);
            DVector3d dv4 = new DVector3d(start_dpoint2, endPipeDpoint);
            bool b2 = dv3.IsParallelTo(dv4);
            if (!b2)
            {
                errorMessage = "序号为" + (index + 1) + "的管道所生成的管道长度过短";
                //System.Windows.Forms.MessageBox.Show("序号为"+(index+1)+"的管道所生成的管道长度过短");
                return null;
            }

            //updateOriginalPipe(start_dpoint2, endPipeDpoint, bmec1);
            if (isSingleElbow)
            {
                try
                {

                    //StandardPreferencesUtilities.SetActivePipingNetworkSystem(lineNumber);
                    //StandardPreferencesUtilities.ChangeSpecification(spec);
                    elbowBend.Create();
                }
                catch (System.Exception)
                {
                    errorMessage = "Pipeline不存在，请打开Create Pipeline创建处理。";
                    return null;
                }
                //updateOriginalPipe(startPipeDpoint, start_dpoint1, bmec);
                //ec_object.LegacyGraphicsId
                JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowBend, dir1, v1, dir2, v2, start_point1, start_point2);

                bmec.DiscoverConnectionsEx();
                bmec.UpdateConnections();

                //bmec1.DiscoverConnectionsEx();
                //bmec1.UpdateConnections();

                elbowBend.UpdateConnections();
                elbowBend.DiscoverConnectionsEx();

                elbows.Add(elbowBend);
            }
            else
            {
                if (elbowOrBendClass.isBzQgWt && elbowOrBendClass.isYgWt)
                {
                    if (!elbowOrBendClass.isLDQ)
                    {
                        double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                        elbowBend.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowBend.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowBend.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        DPoint3d guandaoJiaodian = intersect;//两管道交点

                        DVector3d oldguandao1Vec = new DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        DVector3d oldguandao2Vec = new DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        DVector3d guandao1Vec;//管道一方向向量
                        DPoint3d fasterPoint1CE = startPipeDpoint;
                        guandao1Vec = new DVector3d(fasterPoint1CE, intersect);
                        DVector3d guandao2Vec;//管道二方向向量
                        DPoint3d fasterPoint2CE = endPipeDpoint;
                        guandao2Vec = new DVector3d(intersect, fasterPoint2CE);

                        DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);
                        DVector3d xiangxinRadiusVec = faxiangliang.CrossProduct(guandao1VecNormal);
                        DVector3d xiangxinRadiusVecNormal;
                        xiangxinRadiusVec.TryNormalize(out xiangxinRadiusVecNormal);
                        //向心半径向量
                        if (xiangxinRadiusVecNormal.DotProduct(guandao2VecNormal) < 0)
                        {
                            xiangxinRadiusVecNormal = -xiangxinRadiusVecNormal;
                        }
                        DVector3d xiangxinRadiusVecL;//半径向量
                        double magnitude;
                        xiangxinRadiusVecNormal.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out xiangxinRadiusVecL, out magnitude);

                        //求管道外侧交点到管道外侧切点的分量
                        double elbowOutRadius = (radius + elbowBend.Instance["OUTSIDE_DIAMETER"].DoubleValue / 2) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//弯头外半径
                        double lengthQiedianXian = elbowOutRadius * (1 - Math.Cos(cankaoAngle * Math.PI / 180.0));
                        DVector3d lengthQiedianXianVec;
                        xiangxinRadiusVecNormal.TryScaleToLength(lengthQiedianXian, out lengthQiedianXianVec, out magnitude);

                        Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);
                        double tempAngle = (pipeTopipeAngle.Degrees % 180) - 90.0;
                        double xianAngle = Math.Abs(pipeTopipeAngle.Degrees - 90.0);
                        double hengxiangLen = Math.Tan(xianAngle * Math.PI / 180.0) * lengthQiedianXian;
                        DVector3d hengxiangVect;
                        guandao1VecNormal.TryScaleToLength(hengxiangLen, out hengxiangVect, out magnitude);

                        //求管道外侧交点
                        double pipe2Radius = bmec.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        DVector3d waiceShuXian;
                        xiangxinRadiusVecL.TryScaleToLength(pipe2Radius, out waiceShuXian, out magnitude);
                        DVector3d waiceHengxian;
                        double waiceHengxianL = pipe2Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        DPoint3d pipe2Qiedian;//管道外侧切点坐标
                        if (tempAngle < 0)
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec + hengxiangVect;
                        }
                        else
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec - hengxiangVect;
                        }
                        //求管道2起点
                        DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }
                        DVector3d pipe2XiangxinVecL;
                        pipe2XiangxinVec.TryScaleToLength(pipe2Radius, out pipe2XiangxinVecL, out magnitude);

                        DPoint3d pipe2StarPoint = pipe2Qiedian + pipe2XiangxinVecL;//管道2起点

                        double chuiXianToRadius = elbowOutRadius * Math.Sin(cankaoAngle * Math.PI / 180.0);
                        DVector3d chuiXianToRadiusV;
                        (-guandao1VecNormal).TryScaleToLength(chuiXianToRadius, out chuiXianToRadiusV, out magnitude);
                        double xianOnRadius = elbowOutRadius * Math.Cos(cankaoAngle * Math.PI / 180.0);
                        DVector3d xianOnRadiusV;
                        xiangxinRadiusVecNormal.TryScaleToLength(xianOnRadius, out xianOnRadiusV, out magnitude);

                        DPoint3d elbowCenter = pipe2Qiedian + chuiXianToRadiusV + xianOnRadiusV;//弯头圆心位置
                        DPoint3d pipe1EndPoint = elbowCenter - xiangxinRadiusVecL;//管道1终点

                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1EndPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1EndPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1EndPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1EndPoint).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StarPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StarPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StarPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StarPoint, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                            //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return null;
                        }
                        else
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                            start_dpoint1 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(currentStartPoint1);
                            start_dpoint2 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(currentStartPoint2);
                        }

                        try
                        {
                            elbowBend.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return null;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = guandao1VecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = guandao1VecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = guandao1VecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = xiangxinRadiusVecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = xiangxinRadiusVecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = xiangxinRadiusVecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

                        DVector3d tempVect;
                        DPoint3d CEspoint2 = elbow.GetPointOnArcWithAngle(elbowDegree, elbowCenter, pipe1EndPoint, guandao1VecNormal, out tempVect);
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = CEspoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = CEspoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = CEspoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowBend, dir1, tv1, dir2, tv2, currentStartPoint1, spoint2);

                        bmec.DiscoverConnectionsEx();
                        bmec.UpdateConnections();

                        elbowBend.UpdateConnections();
                        elbowBend.DiscoverConnectionsEx();
                        elbows.Add(elbowBend);
                    }
                    else if (elbowOrBendClass.isLDQ)
                    {
                        double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                        elbowBend.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowBend.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowBend.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        DVector3d oldguandao1Vec = new DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        DVector3d oldguandao2Vec = new DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        DVector3d guandao1Vec;//管道一方向向量
                        DPoint3d fasterPoint1CE = startPipeDpoint;
                        guandao1Vec = new DVector3d(fasterPoint1CE, intersect);
                        DVector3d guandao2Vec;//管道二方向向量
                        DPoint3d fasterPoint2CE = endPipeDpoint;
                        guandao2Vec = new DVector3d(intersect, fasterPoint2CE);

                        DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);

                        double magnitude;
                        //求管道外侧交点
                        Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);//两管道夹角
                        double pipe1Radius = bmec.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        DVector3d pipe1XiangxinVec = faxiangliang.CrossProduct(guandao1VecNormal);//管道1外侧切点向心方向向量
                        if (pipe1XiangxinVec.DotProduct(guandao2VecNormal) < 0)
                        {
                            pipe1XiangxinVec = -pipe1XiangxinVec;
                        }

                        DVector3d waiceShuXian;//管道1向心向量
                        pipe1XiangxinVec.TryScaleToLength(pipe1Radius, out waiceShuXian, out magnitude);
                        DVector3d waiceHengxian;//管道方向
                        double waiceHengxianL = pipe1Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);//管道2外侧切点向心方向向量
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }

                        DVector3d pipe2XiangxinVecL;//管道2向心向量
                        pipe2XiangxinVec.TryScaleToLength(pipe1Radius, out pipe2XiangxinVecL, out magnitude);

                        //弯头弦长
                        double elbowOutSideDia = elbowBend.Instance["OUTSIDE_DIAMETER"].DoubleValue;//弯头管径
                        double elbowOutSideRadius = (elbowOutSideDia / 2 + radius) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        double elbowXianchang = elbowOutSideRadius * Math.Sin(cankaoAngle / 2 * Math.PI / 180.0);//弯头角度对应的弦长的一半

                        //两管道外侧切点
                        double pipe1QieDianL = elbowXianchang / Math.Sin(waiceAngle * Math.PI / 180.0);//切点据外侧交点距离
                        DVector3d pipe1QieDianVec;
                        guandao1VecNormal.TryScaleToLength(pipe1QieDianL, out pipe1QieDianVec, out magnitude);
                        DPoint3d pipe1Qiedian = guandaoWaiceJiaodian - pipe1QieDianVec;//管道1的切点

                        DVector3d pipe2QieDianVec;
                        guandao2VecNormal.TryScaleToLength(pipe1QieDianL, out pipe2QieDianVec, out magnitude);
                        DPoint3d pipe2Qiedian = guandaoWaiceJiaodian + pipe2QieDianVec;//管道2的切点

                        //两管道靠近弯头的端点
                        DPoint3d pipe1StartPointCE = pipe1Qiedian + waiceShuXian;//管道1端点
                        DPoint3d pipe2StartPointCE = pipe2Qiedian + pipe2XiangxinVecL;//管道2端点

                        //弯头圆心位置
                        DVector3d pingfenxianVec = waiceShuXian - waiceHengxian;//两管道角平分线向量
                        double xianToCenterLen = elbowOutSideRadius * Math.Cos(cankaoAngle / 2 * Math.PI / 180.0);//圆心到弦上距离
                        double xianToWaiJiaodianLen = pipe1QieDianL * Math.Cos(waiceAngle * Math.PI / 180.0);//弦外到外侧交点距离
                        DVector3d outsideIntersectVecToCenter;//外侧交点到弯头圆心向量；
                        pingfenxianVec.TryScaleToLength(xianToCenterLen + xianToWaiJiaodianLen, out outsideIntersectVecToCenter, out magnitude);
                        DPoint3d centerPoint = guandaoWaiceJiaodian + outsideIntersectVecToCenter;//弯头圆心

                        //弯头两端点及其方向向量
                        DPoint3d elbowPoint1, elbowPoint2;
                        DVector3d point1Vec, point2Vec;
                        DVector3d point1TempVec, point2TmepVec;
                        point1TempVec = new DVector3d(centerPoint, pipe1Qiedian);
                        point2TmepVec = new DVector3d(centerPoint, pipe2Qiedian);
                        point1TempVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point1TempVec, out magnitude);
                        point2TmepVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point2TmepVec, out magnitude);

                        elbowPoint1 = centerPoint + point1TempVec;
                        elbowPoint2 = centerPoint + point2TmepVec;

                        point1Vec = faxiangliang.CrossProduct(point1TempVec);
                        if (point1Vec.DotProduct(guandao1VecNormal) < 0)
                        {
                            point1Vec = -point1Vec;
                        }
                        point2Vec = faxiangliang.CrossProduct(point2TmepVec);
                        if (point2Vec.DotProduct(guandao2VecNormal) < 0)
                        {
                            point2Vec = -point2Vec;
                        }

                        //改变管道长度并移动管道
                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1StartPointCE).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StartPointCE, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                            //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return null;
                        }
                        else
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                            start_dpoint1 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(currentStartPoint1);
                            start_dpoint2 = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(currentStartPoint2);
                        }

                        //创建弯头并移动
                        try
                        {
                            elbowBend.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return null;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = point1Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = point1Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = point1Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = point2Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = point2Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = point2Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = elbowPoint1.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = elbowPoint1.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = elbowPoint1.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = elbowPoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = elbowPoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = elbowPoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowBend, dir1, tv1, dir2, tv2, spoint1, spoint2);

                        bmec.DiscoverConnectionsEx();
                        bmec.UpdateConnections();

                        elbowBend.UpdateConnections();
                        elbowBend.DiscoverConnectionsEx();
                        elbows.Add(elbowBend);
                    }
                }
                else
                {
                    DVector3d pipe1Vect = new DVector3d(startPipeDpoint, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    DVector3d pipe2Vect = new DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2), endPipeDpoint);
                    DVector3d pipe1VN;
                    pipe1Vect.TryNormalize(out pipe1VN);
                    DVector3d pipe1PointToCircleCenterPointVect = pipe1VN.CrossProduct(pipe2Vect).CrossProduct(pipe1VN);
                    DPoint3d radiusPoint;
                    if (pipe1PointToCircleCenterPointVect.DotProduct(pipe2Vect) < 0)
                    {
                        pipe1PointToCircleCenterPointVect = -pipe1PointToCircleCenterPointVect;
                    }
                    double magnitude;
                    DVector3d radiusVector;
                    pipe1PointToCircleCenterPointVect.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out radiusVector, out magnitude);
                    radiusPoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1) + radiusVector;

                    List<DPoint3d> points = new List<DPoint3d>();//
                    List<DVector3d> tangleVec = new List<DVector3d>();
                    points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    tangleVec.Add(pipe1Vect);
                    for (int i = 0; i < unbrokenElbow; i++)
                    {
                        DVector3d tangleVecAtResultPoint = new DVector3d();
                        DPoint3d tanglePoint = elbow.GetPointOnArcWithAngle(elbowDegree, radiusPoint, points.Last(), tangleVec.Last(), out tangleVecAtResultPoint);
                        points.Add(tanglePoint);
                        tangleVec.Add(tangleVecAtResultPoint);
                    }
                    if (brokenElbowDgree > 1e-1)
                    {
                        points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2));
                        tangleVec.Add(pipe2Vect);
                    }

                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        if (i < unbrokenElbow)
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowBend);
                            elbow.Instance["ANGLE"].DoubleValue = elbowDegree;

                            double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                            double cankaoTan = Math.Tan(Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(Angle.DegreesToRadians(elbowDegree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                        else
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowBend);
                            elbow.Instance["ANGLE"].DoubleValue = brokenElbowDgree;

                            double cankaoAngle = Convert.ToDouble(elbowOrBendClass.elbowAngle.Substring(0, 2));
                            double cankaoTan = Math.Tan(Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(Angle.DegreesToRadians(brokenElbowDgree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                    }

                    for (int i = 0; i < elbows.Count; i++)
                    {
                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = tangleVec[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = tangleVec[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = tangleVec[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = tangleVec[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = tangleVec[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = tangleVec[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = points[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = points[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = points[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = points[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = points[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = points[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbows[i], dir1, tv1, dir2, tv2, spoint1, spoint2);
                    }
                    bmec.DiscoverConnectionsEx();
                    bmec.UpdateConnections();

                    foreach (BMECObject bmecE in elbows)
                    {
                        bmecE.DiscoverConnectionsEx();
                        bmecE.UpdateConnections();
                    }
                }
            }

            //return true;

            return elbows;
        }

        public static void GroupPipeMain(string unparsed)
        {
            GroupPipeToolFrom from1 = GroupPipeToolFrom.instence();
#if DEBUG
#else
            from1.AttachAsTopLevelForm(MyAddin.s_addin, false);
            
#endif
            from1.Show();
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

        /// <summary>
        /// 得到管道的两个端点
        /// </summary>
        /// <param name="bmec"></param>
        /// <returns></returns>
        public static List<BIM.Point3d> getDuandian(BMECObject bmec)
        {
            List<BIM.Point3d> pointList = new List<BIM.Point3d>();
            DPoint3d dpoint1 = bmec.GetNthPort(0).LocationInUors;
            DPoint3d dpoint2 = bmec.GetNthPort(1).LocationInUors;
            pointList.Add(ceDpoint_v8Point(dpoint1));
            pointList.Add(ceDpoint_v8Point(dpoint2));
            return pointList;
        }

        /// <summary>
        /// 判断所选管道是否平行
        /// </summary>
        public bool judgeParallel()
        {
            DPoint3d dpoint1 = duanjianList[0][0];
            DPoint3d dpoint2 = duanjianList[0][1];
            DPoint3d dpoint3 = DPoint3d.Zero;
            DPoint3d dpoint4 = DPoint3d.Zero;
            DPoint3d[] pointshuz = GetTowPortPoint(bmec_object);
            //JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object,ref dpoint3,ref dpoint4);// bmec_object.GetNthPort(0).LocationInUors;
            dpoint3 = pointshuz[0];
            dpoint4 = pointshuz[1];
            DVector3d ve1 = new DVector3d(dpoint1, dpoint2);
            DVector3d ve2 = new DVector3d(dpoint3, dpoint4);
            bool b = ve2.IsParallelOrOppositeTo(ve1); //判断是否平行
            double degree = ve1.AngleTo(ve2).Degrees;
            return b;
        }

        /// <summary>
        /// 给获取的管道赋值
        /// </summary>
        public void fuzhiList()
        {
            double dn = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");
            double od = bmec_object.GetDoubleValueInMM("OUTSIDE_DIAMETER");
            string wall = bmec_object.GetStringValue("WALL_THICKNESS");
            string line_lumber = bmec_object.GetStringValue("LINENUMBER");
            //double length = bmec_object.GetDoubleValueInMM("LENGTH");
            //double weight = bmec_object.GetDoubleValueInMM("WEIGHT");
            from.addPipe(i, bmec_object.ClassName, dn, od, elem, wall, line_lumber);//将数据添加到from中
            DPoint3d dpoint3 = DPoint3d.Zero;
            DPoint3d dpoint4 = DPoint3d.Zero;
            DPoint3d[] pointshuz = GetTowPortPoint(bmec_object);
            //JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object,ref dpoint3,ref dpoint4);// bmec_object.GetNthPort(0).LocationInUors;
            dpoint3 = pointshuz[0];
            dpoint4 = pointshuz[1]; //得到管道的两个端点
            firstPipe++;
            List<DPoint3d> listDpoint = new List<DPoint3d>();
            listDpoint.Add(dpoint3);
            listDpoint.Add(dpoint4);
            duanjianList.Add(i, listDpoint);
            bmecDic.Add(i, bmec_object);
            List<BMECObject> Listbmec = new List<BMECObject>();
            Listbmec.Add(bmec_object);
            shencBmecList.Add(i, Listbmec);
            i++;
        }

        /// <summary>
        /// 得到点下的点为平面外时的点的集合V8I
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public Dictionary<int, List<DPoint3d>> yimiandian(DPoint3d dp)
        {
            BIM.Point3d po1 = ceDpoint_v8Point(pointList[pointList.Count - 1]);
            BIM.Point3d po2 = ceDpoint_v8Point(dp);
            //BIM.Point3d vector1 = app.Point3dFromXYZ(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y, startPoint.Z - centerPoint.Z);
            //double vector1Length = app.Point3dDistance(zero, vector1);
            //BIM.Point3d unitVector1 = app.Point3dFromXYZ(vector1.X / vector1Length, vector1.Y / vector1Length, vector1.Z / vector1Length);
            BIM.Point3d ver = app.Point3dFromXYZ(po2.X - po1.X, po2.Y - po1.Y, po2.Z - po1.Z);
            double verLength = app.Point3dDistance(po1, po2);
            BIM.Point3d unitVer = app.Point3dFromXYZ(ver.X / verLength, ver.Y / verLength, ver.Z / verLength);
            Dictionary<int, List<DPoint3d>> dongtList = new Dictionary<int, List<DPoint3d>>();
            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
            {
                BIM.Point3d falst = ceDpoint_v8Point(kv.Value[kv.Value.Count - 1]);
                BIM.Point3d pt = app.Point3dAddScaled(falst, unitVer, verLength);
                DPoint3d Dpt = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(pt);
                List<DPoint3d> dpList = new List<DPoint3d>();
                dpList.Add(kv.Value[kv.Value.Count - 1]);
                dpList.Add(Dpt);
                dongtList.Add(kv.Key, dpList);
            }
            //BIM.Point3d pt_startPoint1 = app.Point3dAddScaled(ref centerPoint1, unitVer, verLength);
            return dongtList;
        }

        /// <summary>
        /// 得到点下的点为平面外时的点的集合CE
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public Dictionary<int, List<DPoint3d>> ceyimian(DPoint3d dp)
        {
            DPoint3d po1 = pointList[pointList.Count - 1];
            DVector3d dv1 = new DVector3d(po1, dp);
            Dictionary<int, List<DPoint3d>> dongtList = new Dictionary<int, List<DPoint3d>>();
            foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
            {
                List<DPoint3d> dpList = new List<DPoint3d>();
                dpList.Add(kv.Value[kv.Value.Count - 1]);
                DPoint3d dpoint = kv.Value[kv.Value.Count - 1] + dv1;
                dpList.Add(dpoint);
                dongtList.Add(kv.Key, dpList);
            }
            return dongtList;
        }

        /// <summary>
        /// 共面时点的集合
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public Dictionary<int, List<DPoint3d>> cegongmian(DPoint3d dp)
        {
            DPoint3d po1 = pointList[pointList.Count - 1];
            DVector3d dv1 = new DVector3d(po1, dp);//生成管道向量
            double distencedv1 = po1.Distance(dp);
            Dictionary<int, List<DPoint3d>> dongtList = new Dictionary<int, List<DPoint3d>>();
            DPoint3d po2 = pointList[pointList.Count - 2];
            DVector3d dv3 = new DVector3d(po2, po1);
            bool b = dv1.IsParallelOrOppositeTo(dv3);
            double a = dv1.AngleTo(dv3).Degrees;
            if (b)
            {
                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                {
                    List<DPoint3d> dpList = new List<DPoint3d>();
                    dpList.Add(kv.Value[kv.Value.Count - 1]);
                    DPoint3d dpoint = kv.Value[kv.Value.Count - 1] + dv1;
                    dpList.Add(dpoint);
                    dongtList.Add(kv.Key, dpList);
                }
            }
            else
            {
                foreach (KeyValuePair<int, List<DPoint3d>> kv in shengchengList)
                {
                    DPoint3d dp1 = kv.Value[kv.Value.Count - 2];
                    DPoint3d dp2 = kv.Value[kv.Value.Count - 1];
                    BIM.Point3d p1 = ceDpoint_v8Point(dp1);
                    BIM.Point3d p2 = ceDpoint_v8Point(dp2);
                    BIM.Point3d p3 = ceDpoint_v8Point(po1);
                    BIM.LineElement line_elem = app.CreateLineElement2(null, p1, p2);
                    BIM.Point3d touyingdian = line_elem.ProjectPointOnPerpendicular(p3, app.Matrix3dIdentity());
                    DPoint3d touyingdiandp = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(touyingdian);
                    DVector3d dv2 = new DVector3d(po1, touyingdiandp);
                    double distence = po1.Distance(touyingdiandp);
                    double age = dv1.AngleTo(dv2).Degrees;
                    if (age < 90)
                    {
                        DVector3d dv4 = new DVector3d(touyingdiandp, kv.Value[kv.Value.Count - 2]);
                        double distence1 = touyingdiandp.Distance(kv.Value[kv.Value.Count - 2]);
                        DVector3d unit1 = DVector3d.Multiply(dv1, 1 / distencedv1);  //生成管道的单位向量
                        DVector3d unit2 = DVector3d.Multiply(dv4, 1 / distence1); //原管道单位向量
                        double juli = distence * Math.Tan(a / 2 * (Math.PI / 180));
                        double pi = Math.Tan(a / 2 * (Math.PI / 180));
                        DPoint3d dpointtouy = DPoint3d.Add(touyingdiandp, unit2, juli);
                        List<DPoint3d> dpList = new List<DPoint3d>();
                        dpList.Add(dpointtouy);
                        double shengc = distencedv1 - juli;
                        DPoint3d shengcDpoint = DPoint3d.Add(dpointtouy, unit1, shengc);
                        dpList.Add(shengcDpoint);
                        dongtList.Add(kv.Key, dpList);
                    }
                    else
                    {
                        DVector3d dv4 = new DVector3d(kv.Value[kv.Value.Count - 2], touyingdiandp);
                        double distence1 = touyingdiandp.Distance(kv.Value[kv.Value.Count - 2]);
                        DVector3d unit1 = DVector3d.Multiply(dv1, 1 / distencedv1);  //生成管道的单位向量
                        DVector3d unit2 = DVector3d.Multiply(dv4, 1 / distence1); //原管道单位向量
                        double juli = distence * Math.Tan(a / 2 * (Math.PI / 180));
                        double pi = Math.Tan(a / 2 * (Math.PI / 180));
                        DPoint3d dpointtouy = DPoint3d.Add(touyingdiandp, unit2, juli);
                        List<DPoint3d> dpList = new List<DPoint3d>();
                        dpList.Add(dpointtouy);
                        double shengc = distencedv1 + juli;
                        DPoint3d shengcDpoint = DPoint3d.Add(dpointtouy, unit1, shengc);
                        dpList.Add(shengcDpoint);
                        dongtList.Add(kv.Key, dpList);
                    }
                }
            }

            return dongtList;
        }

        private Dictionary<string, string> elbowradiusdic = new Dictionary<string, string>();
        private Dictionary<string, string> elbowangledic = new Dictionary<string, string>();
        private string getElbowECClassName(string radius, string angle)
        {
            string elbowecclassname = "";
            //特殊的两个
            //if (angle.Contains("30度弯头") && radius.Contains("长半径")) return "PIPE_ELBOW_30_DEGREE_LONG_RADIUS";
            //if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";

            //if (radius.Contains("长半径"))
            //{
            //    elbowecclassname = elbowradiusdic[radius] + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + "PIPE_ELBOW";
            //}
            //else
            //{
            //    elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];
            //}
            if (angle.Contains("15度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_15_DEGREE";
            if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";
            if (angle.Contains("45度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_45_DEGREE_PIPE_ELBOW";
            if (angle.Contains("60度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_60_DEGREE_PIPE_ELBOW";
            if (angle.Contains("90度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_90_DEGREE_PIPE_ELBOW";
            if (radius.Contains("普通弯头")) return "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE";
            elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];
            return elbowecclassname;
        }
        //TODO
        //改了原本的ECClass类中的命名
        //PIPE_ELBOW_30_DEGREE_LONG_RADIUS  ---
        //PIPE_ELBOW_SHORT_RADIUS_30_DEGREE ---
        private void initelbowDic()
        {
            elbowradiusdic.Clear();
            elbowangledic.Clear();
            elbowradiusdic.Add("长半径弯头", "LONG_RADIUS");
            elbowradiusdic.Add("短半径弯头", "SHORT_RADIUS");
            elbowradiusdic.Add("内外丝弯头", "INNER_AND_OUTER_WIRE_ELBOWS");
            elbowradiusdic.Add("1.5倍弯曲半径弯头", "1_POINT_5R");
            elbowradiusdic.Add("2倍弯曲半径弯头", "2R");
            elbowradiusdic.Add("2.5倍弯曲半径弯头", "2_POINT_5R");
            elbowradiusdic.Add("3倍弯曲半径弯头", "3R");
            elbowradiusdic.Add("4倍弯曲半径弯头", "4R");
            elbowangledic.Add("15度弯头", "15");
            elbowangledic.Add("30度弯头", "30");
            elbowangledic.Add("45度弯头", "45");
            elbowangledic.Add("60度弯头", "60");
            elbowangledic.Add("90度弯头", "90");
        }

        /// <summary>
        /// 选中多根管道，绘出的管道不平行时得到的点的集合
        /// </summary>
        /// <param name="dp"></param>
        /// <returns></returns>
        public Dictionary<int, List<DPoint3d>> pianyiPipe(DPoint3d dp, DPoint3d startDp, DPoint3d endDp, Dictionary<int, List<DPoint3d>> dicDpList)
        {
            Dictionary<int, List<DPoint3d>> dongtList = new Dictionary<int, List<DPoint3d>>();
            DPoint3d po1 = endDp;
            DVector3d dv1 = new DVector3d(po1, dp);
            double distencedv1 = po1.Distance(dp);
            DPoint3d po2 = startDp;
            DVector3d dv3 = new DVector3d(po2, po1);
            DVector3d normal = dv1.CrossProduct(dv3);
            double a = dv1.AngleTo(dv3).Radians;
            foreach (KeyValuePair<int, List<DPoint3d>> kv in dicDpList)
            {
                DPoint3d dp1 = kv.Value[kv.Value.Count - 2];
                DPoint3d dp2 = kv.Value[kv.Value.Count - 1];
                BIM.Point3d p1 = ceDpoint_v8Point(dp1);
                BIM.Point3d p2 = ceDpoint_v8Point(dp2);
                BIM.Point3d p3 = ceDpoint_v8Point(po1);
                BIM.LineElement line_elem = app.CreateLineElement2(null, p1, p2);
                BIM.Point3d touyingdian = line_elem.ProjectPointOnPerpendicular(p3, app.Matrix3dIdentity());
                DPoint3d touyingdiandp = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(touyingdian);
                DVector3d dv2 = new DVector3d(po1, touyingdiandp);
                double distence = po1.Distance(touyingdiandp);
                double age = dv2.AngleTo(normal).Radians;
                double touyinDistence = distence * Math.Sin(age);
                double aged = dv1.AngleTo(dv2).Degrees;
                if (aged < 90)
                {
                    DVector3d dv4 = new DVector3d(touyingdiandp, kv.Value[kv.Value.Count - 2]);
                    double distence1 = touyingdiandp.Distance(kv.Value[kv.Value.Count - 2]);
                    DVector3d unit1 = DVector3d.Multiply(dv1, 1 / distencedv1);  //生成管道的单位向量
                    DVector3d unit2 = DVector3d.Multiply(dv4, 1 / distence1); //原管道单位向量
                    double juli = touyinDistence * Math.Tan(a / 2);
                    double shengc = distencedv1 - juli;
                    List<DPoint3d> dpList = new List<DPoint3d>();
                    if (shengc > 0)
                    {
                        DPoint3d dpointtouy = DPoint3d.Add(touyingdiandp, unit2, juli);
                        dpList.Add(dpointtouy);
                        DPoint3d shengcDpoint = DPoint3d.Add(dpointtouy, unit1, shengc);
                        dpList.Add(shengcDpoint);
                        dongtList.Add(kv.Key, dpList);
                    }
                    else
                    {
                        DPoint3d dpointtouy = DPoint3d.Add(touyingdiandp, unit2, juli);
                        dpList.Add(dpointtouy);
                        DPoint3d shengcDpoint = DPoint3d.Add(dpointtouy, unit1, distencedv1);
                        dpList.Add(shengcDpoint);
                        dongtList.Add(kv.Key, dpList);
                    }
                }
                else
                {
                    DVector3d dv4 = new DVector3d(kv.Value[kv.Value.Count - 2], touyingdiandp);
                    double distence1 = touyingdiandp.Distance(kv.Value[kv.Value.Count - 2]);
                    DVector3d unit1 = DVector3d.Multiply(dv1, 1 / distencedv1);  //生成管道的单位向量
                    DVector3d unit2 = DVector3d.Multiply(dv4, 1 / distence1); //原管道单位向量
                    double juli = touyinDistence * Math.Tan(a / 2);
                    //double pi = Math.Tan(a / 2 * (Math.PI / 180));
                    DPoint3d dpointtouy = DPoint3d.Add(touyingdiandp, unit2, juli);
                    List<DPoint3d> dpList = new List<DPoint3d>();
                    dpList.Add(dpointtouy);
                    double shengc = distencedv1 + juli;
                    DPoint3d shengcDpoint = DPoint3d.Add(dpointtouy, unit1, shengc);
                    dpList.Add(shengcDpoint);
                    dongtList.Add(kv.Key, dpList);
                }
            }
            return dongtList;
        }

        public DPoint3d touyin(DPoint3d origin, DPoint3d dp1, DPoint3d dp2)
        {
            DPoint3d toudianDp = new DPoint3d();
            BIM.Point3d p1 = ceDpoint_v8Point(dp1);
            BIM.Point3d p2 = ceDpoint_v8Point(dp2);
            BIM.Point3d p3 = ceDpoint_v8Point(origin);
            try
            {
                BIM.LineElement line_elem = app.CreateLineElement2(null, p1, p2);
                BIM.Point3d touyingdian = line_elem.ProjectPointOnPerpendicular(p3, app.Matrix3dIdentity());
                DPoint3d touyingdiandp = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(touyingdian);
                toudianDp = touyingdiandp;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

            return toudianDp;
        }

        protected override UsesDragSelect AllowDragSelect()
        {
            return base.AllowDragSelect();
        }

        protected override UsesSelection AllowSelection()
        {
            return base.AllowSelection();
        }

        protected override UsesFence AllowFence()
        {
            return UsesFence.Check;
            //return base.AllowFence();
        }

        protected override UsesDragSelect IsDragSelectActive()
        {
            return base.IsDragSelectActive();
        }

        protected override bool OnModifierKeyTransition(bool wentDown, int key)
        {
            #region
            //if (key != 8)//Control key
            //{
            //    return false;
            //}
            //else
            //{
            //    if (is_drag_selected)
            //    {
            //        if (wentDown)
            //        {
            //            base.SetLocateCursor(true);
            //        }
            //        else
            //        {
            //            base.SetLocateCursor(false);
            //        }
            //        return true;

            //    }
            //    else
            //    {
            //        return false;
            //    }
            //}
            #endregion
            if (key == 4)//shift
            {
                boxType = true;
            }
            if (key == 8)//ctrl
            {
                boxType = false;
                EndDynamics();
            }
            return true;
        }

        protected override bool OnModelStartDrag(DgnButtonEvent ev)
        {
            return base.OnModelStartDrag(ev);
        }

        protected override bool OnModelEndDrag(DgnButtonEvent ev)
        {
            return base.OnModelEndDrag(ev);
        }

        protected override bool OnModelMotion(DgnButtonEvent ev)
        {
            return base.OnModelMotion(ev);
        }

        protected override bool GetDragAnchorPoint(out Bentley.GeometryNET.DPoint3d anchorPt)
        {
            anchorPt = new DPoint3d();
            return base.GetDragAnchorPoint(out anchorPt);
        }

        protected override bool BuildDragSelectAgenda(FenceParameters fp, DgnButtonEvent ev)
        {
            return base.BuildDragSelectAgenda(fp, ev);
        }

        protected override bool GetDragSelectOverlapMode(DgnButtonEvent ev)
        {
            return base.GetDragSelectOverlapMode(ev);
        }

        protected override void GetDragSelectSymbology(out uint color, out uint fillColor, out uint style, out uint weight, DgnButtonEvent ev)
        {
            base.GetDragSelectSymbology(out color, out fillColor, out style, out weight, ev);
            //return true;
        }

        public bool typeBox = true;
        protected override DPoint3d[] GetBoxPoints(DgnCoordSystem sys, DPoint3d activeOrigin, DPoint3d activeCorner, Viewport vp)
        {
            DPoint3d[] viewBox = vp.GetViewBox(sys, true);
            if (typeBox)
            {
                typeBox = false;
                DPoint3d baseOrgion = new DPoint3d((viewBox[0].X + viewBox[2].X) / 2, (viewBox[0].Y + viewBox[2].Y) / 2, (viewBox[0].Z + viewBox[2].Z) / 2);
                DPoint3d topOrgion = new DPoint3d((viewBox[4].X + viewBox[6].X) / 2, (viewBox[4].Y + viewBox[6].Y) / 2, (viewBox[4].Z + viewBox[6].Z) / 2);
                double length = viewBox[0].Distance(viewBox[1]);
                double width = viewBox[0].Distance(viewBox[3]);
                DgnBoxDetail dgnBox = new DgnBoxDetail(viewBox[0], viewBox[7], new DVector3d(1, 0, 0), new DVector3d(0, 1, 0), length, width, length, width, true);
                SolidPrimitive soBox = SolidPrimitive.CreateDgnBox(dgnBox);
                Element ele = DraftingElementSchema.ToElement(Session.Instance.GetActiveDgnModel(), soBox, null);
                ele.AddToModel();
                //double length = viewBox[0].Distance(viewBox[1]);
                //double width = viewBox[0].Distance(viewBox[3]);
                double height = viewBox[0].Distance(viewBox[4]);
                DPoint3d dp = new DPoint3d((viewBox[0].X + viewBox[7].X) / 2, (viewBox[0].Y + viewBox[7].Y) / 2, (viewBox[0].Z + viewBox[7].Z) / 2);
                DVector3d dv = new DVector3d(0, 0, 1);
                Plate plate = new Plate(length, width, height, 0, dp, dv);
                plate.Create3DElements();
            }
            DVector3d v1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d v2 = new DVector3d(viewBox[2], viewBox[1]);
            DVector3d v3 = new DVector3d(viewBox[0], viewBox[3]);
            DVector3d v4 = new DVector3d(viewBox[2], viewBox[3]);
            DRay3d ray1 = new DRay3d(activeOrigin, v1);
            DRay3d ray2 = new DRay3d(activeCorner, v2);
            DRay3d ray3 = new DRay3d(activeOrigin, v3);
            DRay3d ray4 = new DRay3d(activeCorner, v4);
            DSegment3d segment, segment1;
            double b1, b2, b3, b4;
            bool bbb = DRay3d.ClosestApproachSegment(ray1, ray2, out segment, out b1, out b2);
            bool bbbb = DRay3d.ClosestApproachSegment(ray3, ray4, out segment1, out b3, out b4);
            List<DPoint3d> dpointList = new List<DPoint3d>();
            dpointList.Add(activeOrigin);
            dpointList.Add(segment.StartPoint);
            dpointList.Add(activeCorner);
            dpointList.Add(segment1.StartPoint);
            DVector3d v5 = new DVector3d(viewBox[0], viewBox[4]);
            dpointList.Add(activeOrigin + v5);
            dpointList.Add(segment.StartPoint + v5);
            dpointList.Add(activeCorner + v5);
            dpointList.Add(segment1.StartPoint + v5);
            DPoint3d[] dpList = dpointList.ToArray();
            return base.GetBoxPoints(sys, activeOrigin, activeCorner, vp);
        }

        protected override void DecorateScreen(Viewport vp)
        {
            base.DecorateScreen(vp);
        }

        protected override void Dispose(bool A_0)
        {
            base.Dispose(A_0);
        }

        #region 重写这个后捕捉不到管道的端点
        //protected override void SetupAndPromptForNextAction()
        //{
        //    base.SetLocateCursor(!is_drag_selected ? true : false);
        //}
        #endregion

        //protected override bool WantAdditionalLocate(DgnButtonEvent ev)
        //{

        //    if (ev == null)
        //    {
        //        return true;
        //    }
        //    if (is_drag_selected || ev.IsControlKey)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        private List<Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem> pipingNetworkSystems = null;
        private List<string> pipelinesName = null;
        public void GetPipeLine()
        {
            pipingNetworkSystems = Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem.GetExistingPipingNetworkSystems();
            pipelinesName = new List<string>();
            foreach (var item in pipingNetworkSystems)
            {
                string name = item.Name;
                pipelinesName.Add(name);
            }
            //return pipelinesName;
        }

        public bool distence(DPoint3d dp1, DPoint3d dp2)
        {
            bool b = false;
            double dis = dp1.Distance(dp2);
            if (dis < 0.1)
            {
                b = true;
            }
            return b;
        }

        public DVector3d qsDv(DPoint3d scDvJ1, DPoint3d scDvJ2, DPoint3d qDv2, DVector3d byDv)
        {
            DVector3d yPyDv = byDv;
            //DPoint3d yDp3 = pointList[pointList.Count - 2];
            //DPoint3d yDp4 = pointList[pointList.Count - 1];
            //DVector3d yDv3 = new DVector3d(yDp3, yDp4);
            DVector3d yDv2 = new DVector3d(1, 0, 0);
            DVector3d scDvJ = new DVector3d(scDvJ1, scDvJ2);

            DMatrix3d dm3d3 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(yDv2, scDvJ, ref dm3d3);
            DVector3d yPipeUnitDv = DMatrix3d.Multiply(dm3d3, byDv);
            //DVector3d yPipeDv = (odc2 + bwchd) * 1000 * yPipeUnitDv;
            DPoint3d dp1 = scDvJ2 + yPipeUnitDv;
            DPoint3d dp2 = scDvJ1 + yPipeUnitDv;
            Dictionary<int, List<DPoint3d>> dicDp = new Dictionary<int, List<DPoint3d>>();
            List<DPoint3d> dpList = new List<DPoint3d>();
            dpList.Add(dp1);
            dpList.Add(dp2);
            dicDp.Add(0, dpList);
            Dictionary<int, List<DPoint3d>> scDicDp = pianyiPipe(qDv2, scDvJ2, scDvJ1, dicDp);
            DPoint3d touyinDp = touyin(scDvJ1, scDicDp[0][0], scDicDp[0][1]);
            DVector3d jDv = new DVector3d(scDvJ1, touyinDp);
            DVector3d qDv1 = new DVector3d(qDv2, scDvJ1);
            DMatrix3d dm3d1 = new DMatrix3d(0, 0, 0, 0, 0, 0, 0, 0, 0);
            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(qDv1, yDv2, ref dm3d1);
            DVector3d dv3 = DMatrix3d.Multiply(dm3d1, jDv);

            DVector3d topDv = new DVector3d(0, 0, 1);
            double topDis = dv3.DotProduct(topDv);
            DVector3d downDv = new DVector3d(0, 0, -1);
            double downDis = dv3.DotProduct(downDv);
            DVector3d leftDv = new DVector3d(0, 1, 0);
            double leftDis = dv3.DotProduct(leftDv);
            DVector3d rightDv = new DVector3d(0, -1, 0);
            double rightDis = dv3.DotProduct(rightDv);
            List<double> dList = new List<double>();
            dList.Add(topDis);
            dList.Add(downDis);
            dList.Add(leftDis);
            dList.Add(rightDis);
            dList.Sort();
            if (topDis == dList[3])
            {
                yPyDv = topDv;
            }
            else if (downDis == dList[3])
            {
                yPyDv = downDv;
            }
            else if (leftDis == dList[3])
            {
                yPyDv = leftDv;
            }
            else if (rightDis == dList[3])
            {
                yPyDv = rightDv;
            }
            return yPyDv;
        }

        public static DPoint3d[] GetTowPortPoint(BMECObject twoPortPipeObject)
        {
            return new DPoint3d[] { twoPortPipeObject.GetNthPoint(0), twoPortPipeObject.GetNthPoint(1) };
        }
    }
}
