﻿using Bentley.OpenPlant.Modeler.Api;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.Plant.StandardPreferences;
using JYX_ZYJC_Jianmo_Youhua_CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bentley.GeometryNET.CurvePrimitive;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class multiZhinengxianPipe
    {
        //private static List<Element> elementList = new List<Element>();
        private  int i_line = 0;
        private  Dictionary<int, List<DPoint3d>> PointLine = new Dictionary<int, List<DPoint3d>>(); //选中多个智能线 后得到的字典类 点的集合
        public Dictionary<int, List<DPoint3d>> new_PointLine = new Dictionary<int, List<DPoint3d>>(); //处理了 共线的点 （如果一个智能线得到的点存在交叉的情况 就无法解决）
        private  int firstPoint = 1; //开始点的索引
        private  List<DPoint3d> PointLineString = new List<DPoint3d>(); //以前的递归 连接线时用
        private  Dictionary<int, Element> indexElement = new Dictionary<int, Element>(); //选中元素的字典类 用来
        private  List<Element> outLine = new List<Element>(); //没用
        private  Dictionary<List<DPoint3d>, int> indexPoint = new Dictionary<List<DPoint3d>, int>(); //用来更具值 找索引  （没必要这样写  以前的逻辑 没改）
        private  Dictionary<int, List<DPoint3d>> outLineList = new Dictionary<int, List<DPoint3d>>(); //没有交点的智能线的点的集合  //没有必要 以前写的 逻辑没改
        private  Dictionary<int, double> xishuList = new Dictionary<int, double>(); //参考模型的比例系数
        BMECApi api = BMECApi.Instance;
        DPoint3d pianyiDpoint = new DPoint3d(); //引用模型移动的距离
        public Dictionary<int, List<DSegment3d>> xDuanList = new Dictionary<int, List<DSegment3d>>();//将点的集合转换程成线段的集合
        public List<DSegment3d> dsTreeList = new List<DSegment3d>();//将点的集合转换程成线段的集合,用来生成树时遍历
        BoTree<Segment> startTree = new BoTree<Segment>(); //根结点
        public void start()
        {
            //testSanTong();
            //return;
            double angle_tolerant = 1.0;
            //double normal_diameter = 100;
            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请选中一个元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }
            #region 得到比例 已经偏移点
            int index = 0;
            for (uint i = 0; i < selected_elem_count; i++)
            {
                Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(i);
                if (elem.DgnModelRef.IsDgnAttachment)
                {
                    DPoint3d origin = DPoint3d.Zero;
                    elem.DgnModelRef.AsDgnAttachment().GetMasterOrigin(ref origin);
                    double uorpermas = elem.DgnModel.GetModelInfo().UorPerMaster;
                    double refUorpermas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                    UnitDefinition unitdef = elem.DgnModel.GetModelInfo().GetMasterUnit();
                    double xiangshu = 1;
                    if (unitdef.Label == "m")
                    {
                        xiangshu = refUorpermas / uorpermas * 1000;
                    }
                    pianyiDpoint = new DPoint3d(origin.X / xiangshu, origin.Y / xiangshu, origin.Z / xiangshu);
                }
                indexElement.Add(index,elem);
                index++;
            }
            #endregion
            //Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetFirst();
            foreach (KeyValuePair<int,Element> kv in indexElement)
            {
                CurveVector curvevector = CurvePathQuery.ElementToCurveVector(kv.Value);
                CurveVector new_curvevector = CurveVector.Create(CurveVector.BoundaryType.Open);
                CurvePrimitive curveprimitive;
                try
                {
                    curveprimitive = curvevector.GetPrimitive(0);
                }
                catch
                {
                    CleraPublic();
                    System.Windows.Forms.MessageBox.Show("选中的元素无法转换为管道");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }
                #region 判断选中的元素是不是线型
                if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.LineString)
                {
                    List<DPoint3d> points_list = new List<DPoint3d>();
                    curveprimitive.TryGetLineString(points_list);
                    for (int shi = 0; shi < points_list.Count; shi++)
                    {
                        points_list[shi] = points_list[shi] + pianyiDpoint;
                    }
                    i_line++;
                    PointLine.Add(i_line, points_list);
                    indexPoint.Add(points_list, kv.Key);                   
                }
                else if(curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.Line)
                {
                    List<DPoint3d> points_list = new List<DPoint3d>();
                    DSegment3d LinePoint;
                    curveprimitive.TryGetLine(out LinePoint);
                    points_list.Add(LinePoint.StartPoint);
                    points_list.Add(LinePoint.EndPoint);
                    points_list[0] += pianyiDpoint;
                    points_list[1] += pianyiDpoint;
                    i_line++;
                    PointLine.Add(i_line, points_list);
                    indexPoint.Add(points_list, kv.Key);
                }
                else
                {
                    CleraPublic();
                    System.Windows.Forms.MessageBox.Show(curveprimitive.GetCurvePrimitiveType().ToString() + "类型元素暂未尝试处理成管道");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }
                #endregion
                double elem_xianshu = kv.Value.DgnModel.GetModelInfo().UorPerMaster; //当前像素
                double uro = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//引用的像素
                UnitDefinition unitdef = kv.Value.DgnModel.GetModelInfo().GetMasterUnit();//当前单位

                if(unitdef.Label=="m")
                {
                    xishuList.Add(i_line, 1 / elem_xianshu * 1000 * uro);
                }
                else
                {
                    xishuList.Add(i_line, 1);
                }
            }
            //转化点
            foreach(KeyValuePair<int,List<DPoint3d>> kv in PointLine)
            {
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    kv.Value[i] = kv.Value[i] * xishuList[kv.Key];
                }
            }
            #region 处理同一个元素共线的点 （因为树的递归死循环了 暂时这样处理）
            foreach(KeyValuePair<int,List<DPoint3d>> kv in PointLine)
            {
                List<DPoint3d> yPointList = kv.Value;
                List<DPoint3d> new_PointList = new List<DPoint3d>();
                for(int y=0;y<yPointList.Count-1;y++)
                {
                    if(y==0)
                    {
                        new_PointList.Add(yPointList[y]);
                        new_PointList.Add(yPointList[y + 1]);
                    }
                    else
                    {
                        DVector3d new_dv1 = new DVector3d(new_PointList[new_PointList.Count - 2], new_PointList[new_PointList.Count - 1]);
                        DVector3d new_dv2 = new DVector3d(yPointList[y], yPointList[y + 1]);
                        Angle angle = new_dv1.AngleTo(new_dv2);
                        if(angle.Degrees- angle_tolerant < 0.001||((angle.Degrees-(180-angle_tolerant)>0.001)&&(angle.Degrees-180)<0.001))
                        {
                            new_PointList[new_PointList.Count - 1] = yPointList[y + 1];
                        }
                        else
                        {
                            new_PointList.Add(yPointList[y + 1]);
                        }
                    }
                    #region
                    //List<DPoint3d> new_points_list = new List<DPoint3d>();
                    //for (int i = 0; i < PointList.Count - 1; i++)
                    //{
                    //    if (i == 0)
                    //    {
                    //        new_points_list.Add(PointList[i]);
                    //        new_points_list.Add(PointList[i + 1]);
                    //    }
                    //    else
                    //    {
                    //        DVector3d new_vector1 = new DVector3d(new_points_list[new_points_list.Count - 2], new_points_list[new_points_list.Count - 1]);
                    //        DVector3d new_vector2 = new DVector3d(PointList[i], PointList[i + 1]);

                    //        Angle angle = new_vector1.AngleTo(new_vector2);
                    //        if ((angle.Degrees - angle_tolerant < 0.001) || ((angle.Degrees - (180 - angle_tolerant) > 0.001) && (angle.Degrees - 180) < 0.001))
                    //        {
                    //            new_points_list[new_points_list.Count - 1] = PointList[i + 1];
                    //        }
                    //        else
                    //        {
                    //            new_points_list.Add(PointList[i + 1]);
                    //        }

                    //    }
                    //}
                    #endregion
                }
                new_PointLine.Add(kv.Key, new_PointList);
            }
            #endregion
            //int firstPoint = 0;
            //int outLinePoint = 0, duandian = 0, santong = 0;
            #region
            bool result = PdDian(); //判断点是否满足生产的条件
            if (!result)
            {
                return;
            }
            foreach(KeyValuePair<int,List<DSegment3d>> kv in xDuanList)
            {
                dsTreeList.AddRange(kv.Value);
            }
            #region 得到根结点
            double distence1 = 0;
            DPoint3d lianjiedian = new DPoint3d();
            bool issantong = false, isotherdp = false;
            DSegment3d xianduan = new DSegment3d();
            bool zuo = false, you = false;
            int xianjiaoshu = 0;
            foreach(KeyValuePair<int,List<DSegment3d>> kv in xDuanList)
            {
                if(kv.Key!=firstPoint)
                {
                    if(!zuo)
                    {
                        zuo = isPdDpDs(xDuanList[firstPoint][0].StartPoint, kv.Value);
                    }
                    if(!you)
                    {
                        you = isPdDpDs(xDuanList[firstPoint][xDuanList[firstPoint].Count - 1].EndPoint, kv.Value);
                    }
                }
            }
            if(zuo)
            {
                lianjiedian = xDuanList[firstPoint][xDuanList[firstPoint].Count - 1].EndPoint;
                xianduan = xDuanList[firstPoint][xDuanList[firstPoint].Count - 1];
            }
            else
            {
                lianjiedian = xDuanList[firstPoint][0].StartPoint;
                xianduan = xDuanList[firstPoint][0];
            }
            //BoTree<Segment> startTree = new BoTree<Segment>();
            startTree.Data = new Segment(distence1, issantong, isotherdp, lianjiedian, xianduan,xianjiaoshu);
            #endregion
            scTree(startTree); //将线段按树结构存储
            if(keyList.Count>0)
            {
                System.Windows.Forms.MessageBox.Show("存在非垂直的线段，无法生成三通");
                List<Element> listEle = new List<Element>();
                foreach(int keyk in keyList)
                {
                    listEle.Add(indexElement[keyk - 1]);
                }
                isolateElementFrom isoFrom = new isolateElementFrom(listEle);
#if DEBUG
#else
            isoFrom.AttachAsTopLevelForm(MyAddin.s_addin, false);
            
#endif
                isoFrom.Show();
                CleraPublic();
                return;
            }
            List<DPoint3d> testDpList = new List<DPoint3d>();
            ZhinengxianPipeFrom zhinengxianfrom = new ZhinengxianPipeFrom(testDpList, indexElement, startTree);
#if DEBUG
#else
            zhinengxianfrom.AttachAsTopLevelForm(MyAddin.s_addin, false);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZhinengxianPipeFrom));
            zhinengxianfrom.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            //zhinengxianfrom.AttachAsTopLevelForm(Class1.s_addin, false);
            zhinengxianfrom.Show();
            //BMECObject pipeBmec = Createpipe(PointLine[firstPoint][0], PointLine[firstPoint][1]);
            return;
            #endregion
            #region 以前代码 没有三通
//            #region 得到起始线以及判断是否满足生成管道的要求
//            int outLinePoint = 0, duandian = 0, santong = 0;
//            bool firstL = true;
//            for (int i = 1; i <=i_line; i++)
//            {
//                List<DPoint3d> dpList = new List<DPoint3d>();
//                List<DPoint3d> dpList2 = new List<DPoint3d>();
//                dpList = PointLine[i];
//                int jiaodian = 0;
//                int zuojiaodian = 0, youjiaodian = 0;
//                for (int dp2 = 1; dp2 <= i_line; dp2++)
//                {
//                    if (dp2 != i)
//                    {
//                        dpList2 = PointLine[dp2];
//                        bool b1 = distence(dpList[0], dpList[dpList.Count - 1]);
//                        bool b2 = distence(dpList2[0], dpList2[dpList2.Count - 1]);
//                        bool b3 = distence(dpList[0], dpList2[0]);
//                        bool b4 = distence(dpList[0], dpList2[dpList2.Count - 1]);
//                        bool b5 = distence(dpList[dpList.Count - 1], dpList2[0]);
//                        bool b6 = distence(dpList[dpList.Count - 1], dpList2[dpList2.Count - 1]);
//                        if (!b1 && !b2)//dpList[0] != dpList[dpList.Count - 1] && dpList2[0] != dpList2[dpList2.Count - 1]
//                        {
//                            if ((b3 || b4) && (b5 || b6))
//                            {
//                                CleraPublic();
//                                System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
//                                BMECApi.Instance.StartDefaultCommand();
//                                return;
//                            }
//                            else
//                            {
//                                if (b3 || b4)
//                                {
//                                    zuojiaodian++;
//                                    jiaodian++;
//                                }
//                                else if (b5 || b6)
//                                {
//                                    youjiaodian++;
//                                    jiaodian++;
//                                }
//                            }
//                        }
//                        else
//                        {
//                            CleraPublic();
//                            System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
//                            BMECApi.Instance.StartDefaultCommand();
//                            return;
//                        }
//                    }
//                }
//                if (zuojiaodian+youjiaodian==0)
//                {
//                    outLineList.Add(outLinePoint, dpList);
//                    outLinePoint++;
//                }
//                else if(zuojiaodian>2||youjiaodian>2)
//                {
//                    CleraPublic();
//                    System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
//                    BMECApi.Instance.StartDefaultCommand();
//                    return;
//                }
//                else if ((zuojiaodian>0&&youjiaodian==0)||(zuojiaodian==0&&youjiaodian>0))
//                {
//                    if(firstL)
//                    {
//                        firstPoint = i;
//                        firstL = false;
//                    }
//                    duandian++;                    
//                }
//                if(zuojiaodian==2)
//                {
//                    santong++;
//                }
//                if(youjiaodian==2)
//                {
//                    santong++;
//                }
//                //else if (jiaodian == 2)
//                //{

//                //}

//                //else
//                //{
//                //    CleraPublic();
//                //    System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
//                //    BMECApi.Instance.StartDefaultCommand();
//                //    return;
//                //}
//            }

//            if(outLineList.Count>0)
//            {
//                if(indexElement.Count!=1)
//                {
//                    List<Element> elementList = new List<Element>();
//                    System.Windows.Forms.MessageBox.Show("存在没有交点的线");
//                    BMECApi.Instance.StartDefaultCommand();
//                    SelectionSetManager.EmptyAll();
//                    foreach (KeyValuePair<int, List<DPoint3d>> kv in outLineList)
//                    {
//                        int indexOut = indexPoint[kv.Value];
//                        Element outLine = indexElement[indexOut];
//                        elementList.Add(outLine);
//                        //SelectionSetManager.AddElement(outLine, Session.Instance.GetActiveDgnModelRef());
//                    }
//                    isolateElementFrom isoFrom = new isolateElementFrom(elementList);
//#if DEBUG
//#else
//            isoFrom.AttachAsTopLevelForm(MyAddin.s_addin, false);
            
//#endif
//                    isoFrom.Show();
//                    CleraPublic();
//                    return;
//                }               
//            }
//            if(duandian!=santong/3+2)
//            {
//                System.Windows.Forms.MessageBox.Show("存在闭合的线");
//                BMECApi.Instance.StartDefaultCommand();
//                CleraPublic();
//                return;
//            }
//            #endregion
//            List<DPoint3d> PointList = new List<DPoint3d>();
//            BoTree<Segment> startTree1 = new BoTree<Segment>();
//            PointList = lianjieLine(firstPoint); //将选中的元素连成线
//            #region 判断相邻三个点是不是在一条直线上
//            List<DPoint3d> new_points_list = new List<DPoint3d>();
//            for (int i = 0; i < PointList.Count - 1; i++)
//            {
//                if (i == 0)
//                {
//                    new_points_list.Add(PointList[i]);
//                    new_points_list.Add(PointList[i + 1]);
//                }
//                else
//                {
//                    DVector3d new_vector1 = new DVector3d(new_points_list[new_points_list.Count - 2], new_points_list[new_points_list.Count - 1]);
//                    DVector3d new_vector2 = new DVector3d(PointList[i], PointList[i + 1]);

//                    Angle angle = new_vector1.AngleTo(new_vector2);
//                    if ((angle.Degrees - angle_tolerant < 0.001) || ((angle.Degrees - (180 - angle_tolerant) > 0.001) && (angle.Degrees-180) < 0.001))
//                    {
//                        new_points_list[new_points_list.Count - 1] = PointList[i + 1];
//                    }
//                    else
//                    {
//                        new_points_list.Add(PointList[i + 1]);
//                    }

//                }
//            }
//            #endregion
//            ZhinengxianPipeFrom zhinengxianfrom1 = new ZhinengxianPipeFrom(new_points_list,indexElement,startTree1);
//#if DEBUG
//#else
//            zhinengxianfrom1.AttachAsTopLevelForm(MyAddin.s_addin, false);
//            System.ComponentModel.ComponentResourceManager resources1 = new System.ComponentModel.ComponentResourceManager(typeof(ZhinengxianPipeFrom));
//            zhinengxianfrom1.Icon = ((System.Drawing.Icon)(resources1.GetObject("$this.Icon")));
//#endif
//            //zhinengxianfrom.AttachAsTopLevelForm(Class1.s_addin, false);
//            zhinengxianfrom1.Show();
//            #region
//            //#region 生成管道
//            //List<BMECObject> bmec_object_list = new List<BMECObject>();
//            //for (int i = 0; i < new_points_list.Count - 1; i++)
//            //{

//            //    DPoint3d dpt_start = new_points_list[i];
//            //    DPoint3d dpt_end = new_points_list[i + 1];
//            //    BMECObject bmec_object = OPM_Public_Api.create_pipe(dpt_start, dpt_end, normal_diameter);
//            //    bmec_object.Create();
//            //    //bmec_object.SetIntValue(null, 1001);
//            //    bmec_object.DiscoverConnectionsEx();
//            //    bmec_object.UpdateConnections();
//            //    bmec_object_list.Add(bmec_object);
//            //}
//            //#endregion
//            //#region 对生成的管道进行回切操作
//            //string errorMessage = string.Empty;
//            //for (int i = 0; i < bmec_object_list.Count - 1; i++)
//            //{
//            //    string errorMessage_temp;
//            //    string PipeLine = bmec_object_list[i].Instance["LINENUMBER"].StringValue; //获取pdm中PipeLine的值
//            //    //bmec_object_list[i].Instance.InstanceId
//            //    if (PipeLine != "")
//            //    {
//            //        try
//            //        {
//            //            BMECObject bmec_object1 = bmec_object_list[i];
//            //            ulong id1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object1);
//            //            bmec_object1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id1);
//            //            BMECObject bmec_object2 = bmec_object_list[i + 1];
//            //            ulong id2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object2);
//            //            bmec_object2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id2);

//            //            OPM_Public_Api.huiqie_pipe(bmec_object1, bmec_object2, out errorMessage_temp);
//            //            errorMessage += errorMessage_temp;
//            //        }
//            //        catch (Exception ex)
//            //        {
//            //            System.Windows.Forms.MessageBox.Show(ex.Message.ToString());
//            //            CleraPublic();
//            //            return;
//            //        }
//            //    }
//            //    else
//            //    {
//            //        CleraPublic();
//            //        System.Windows.Forms.MessageBox.Show("请先设置PipeLine");
//            //        BMECApi.Instance.StartDefaultCommand();
//            //        return;
//            //    }
//            //}
//            //#endregion
//            //if (errorMessage.Length != 0)
//            //{
//            //    System.Windows.Forms.MessageBox.Show(errorMessage);
//            //}
//            //System.Windows.Forms.MessageBox.Show("生成成功");
//            //BMECApi.Instance.StartDefaultCommand();
//            #endregion
            //CleraPublic();
            #endregion
        }

        /// <summary>
        /// 将智能线连接起来
        /// </summary>
        /// <param name="firstDp"></param>
        /// <returns></returns>
        public List<DPoint3d> lianjieLine(int firstDp)
        {
            //List<DPoint3d> PointList = new List<DPoint3d>();
            int nextDP = firstDp;
            bool b = true;
            List<DPoint3d> firstDpLine = PointLine[nextDP];
            if(PointLine.Count==1)
            {
                for (int s = 0; s < firstDpLine.Count; s++)
                {
                    PointLineString.Add(firstDpLine[s]);
                }
            }
            for (int i = 1; i <= i_line; i++)
            {
                if (i != nextDP)
                {
                    List<DPoint3d> nextPoint = PointLine[i];
                    if(nextDP==firstPoint)
                    {
                        bool b1 = distence(firstDpLine[0], nextPoint[0]);
                        bool b2 = distence(firstDpLine[0], nextPoint[nextPoint.Count - 1]);
                        bool b3 = distence(firstDpLine[firstDpLine.Count - 1], nextPoint[0]);
                        bool b4 = distence(firstDpLine[firstDpLine.Count - 1], nextPoint[nextPoint.Count - 1]);
                        if (b1 || b2)
                        {
                            for(int s=firstDpLine.Count-1;s>=0;s--)
                            {
                                PointLineString.Add(firstDpLine[s]);
                            }
                            lianjieLine(i);
                        }
                        else if(b3||b4)
                        {
                            for(int s=0;s<firstDpLine.Count;s++)
                            {
                                PointLineString.Add(firstDpLine[s]);
                            }
                            lianjieLine(i);
                        }
                    }
                    else
                    {
                        bool b1 = distence(firstDpLine[0], PointLineString[PointLineString.Count - 1]);
                        bool b4 = distence(firstDpLine[firstDpLine.Count - 1], PointLineString[PointLineString.Count - 1]);
                        if(b1)
                        {
                            if(b)
                            {
                                for (int s = 1; s < firstDpLine.Count; s++)
                                {
                                    PointLineString.Add(firstDpLine[s]);
                                }
                                b = false;
                            }
                            bool b2 = distence(PointLineString[PointLineString.Count - 1], nextPoint[0]);
                            bool b3 = distence(PointLineString[PointLineString.Count - 1], nextPoint[nextPoint.Count - 1]);
                            if (b2 || b3)
                            {
                                lianjieLine(i);
                            }
                        }
                        else if(b4)
                        {
                            if(b)
                            {
                                for (int s = firstDpLine.Count - 2; s >= 0; s--)
                                {
                                    PointLineString.Add(firstDpLine[s]);
                                }
                                b = false;
                            }
                            bool b2 = distence(PointLineString[PointLineString.Count - 1], nextPoint[0]);
                            bool b3 = distence(PointLineString[PointLineString.Count - 1], nextPoint[nextPoint.Count - 1]);
                            if (b2||b3)
                            {
                                lianjieLine(i);
                            }
                        }
                    }
                 
                }
            }

            return PointLineString;
        }

        /// <summary>
        /// 重置值
        /// </summary>
        public  void CleraPublic()
        {
            i_line = 0;
            firstPoint = 1;
            //elementList.Clear();
            PointLine.Clear();
            PointLineString.Clear();
            indexElement.Clear();
            outLine.Clear();
            indexPoint.Clear();
            outLineList.Clear();
            xishuList.Clear();
        }

        /// <summary>
        /// 根据点的距离判断相等
        /// </summary>
        /// <param name="dp1"></param>
        /// <param name="dp2"></param>
        /// <returns></returns>
        public  bool distence(DPoint3d dp1,DPoint3d dp2)
        {
            bool b = false;
            double dis = dp1.Distance(dp2);
            if(dis<0.1)
            {
                b = true;
            }
            return b;
        }

        /// <summary>
        /// 测试三通
        /// </summary>
        public void testSanTong()
        {
            ElementAgenda selectAgenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref selectAgenda);
            if (selectAgenda.GetCount() == 0)
            {
                System.Windows.Forms.MessageBox.Show("请先选择元素");
                return;
            }
            List<Element> eleList = new List<Element>();
            for (uint i = 0; i < selectAgenda.GetCount(); i++)
            {
                eleList.Add(selectAgenda.GetEntry(i));
            }
            Dictionary<int, List<DPoint3d>> dicDpoint = new Dictionary<int, List<DPoint3d>>();
            //int index = 0;
            List<DSegment3d> dsList = new List<DSegment3d>();
            foreach (Element ele in eleList)
            {
                CurveVector curvevector = CurvePathQuery.ElementToCurveVector(ele);
                CurveVector new_curvevector = CurveVector.Create(CurveVector.BoundaryType.Open);
                CurvePrimitive curveprimitive;
                try
                {
                    curveprimitive = curvevector.GetPrimitive(0);
                }
                catch
                {
                    //CleraPublic();
                    eleList.Clear();
                    System.Windows.Forms.MessageBox.Show("选中的元素无法转换为管道");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }
                if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.Line)
                {
                    List<DPoint3d> points_list = new List<DPoint3d>();
                    DSegment3d LinePoint;
                    curveprimitive.TryGetLine(out LinePoint);
                    dsList.Add(LinePoint);
                    //points_list.Add(LinePoint.StartPoint);
                    //points_list.Add(LinePoint.EndPoint);
                    //i_line++;
                    //PointLine.Add(i_line, points_list);
                    //indexPoint.Add(points_list, kv.Key);
                }

            }
            double a = 0, b = 0;
            DSegment3d closestDs = new DSegment3d();
            bool bb = DSegment3d.ClosestApproachSegment(dsList[0], dsList[1], out closestDs, out a, out b);
            List<DPoint3d> dpList = new List<DPoint3d>();
            bool julb = distence(closestDs.StartPoint, closestDs.EndPoint);
            if (julb)
            {
                if ((a - 0 > 0.000001 && a < 1) && (b - 0 > 0.000001 && b < 1))
                {
                    System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
                    return;
                }
                if ((a - 0 > 0.000001) && a < 1)
                {
                    if (b - 0 < 0.000001)
                    {
                        dpList.Add(dsList[0].StartPoint);
                        dpList.Add(dsList[0].EndPoint);
                        dpList.Add(dsList[1].StartPoint);
                        dpList.Add(dsList[1].EndPoint);
                    }
                    else
                    {
                        dpList.Add(dsList[0].StartPoint);
                        dpList.Add(dsList[0].EndPoint);
                        dpList.Add(dsList[1].EndPoint);
                        dpList.Add(dsList[1].StartPoint);
                    }
                }
                else
                {
                    if (a - 0 < 0.000001)
                    {
                        dpList.Add(dsList[1].StartPoint);
                        dpList.Add(dsList[1].EndPoint);
                        dpList.Add(dsList[0].StartPoint);
                        dpList.Add(dsList[0].EndPoint);
                    }
                    else
                    {
                        dpList.Add(dsList[1].StartPoint);
                        dpList.Add(dsList[1].EndPoint);
                        dpList.Add(dsList[0].EndPoint);
                        dpList.Add(dsList[0].StartPoint);
                    }
                }

            }
            #region
            //if(closestDs.StartPoint==closestDs.EndPoint)
            //{
            //    DPoint3d jd = closestDs.StartPoint;
            //    switch(jd)
            //    {
            //        case dsList[0].StartPoint:
            //            dpList.Add(dsList[1].StartPoint);
            //            dpList.Add(dsList[1].EndPoint);
            //            dpList.Add(dsList[0].StartPoint);
            //            dpList.Add(dsList[0].EndPoint);
            //            break;
            //        case dsList[0].EndPoint:
            //            dpList.Add(dsList[1].StartPoint);
            //            dpList.Add(dsList[1].EndPoint);
            //            dpList.Add(dsList[0].EndPoint);
            //            dpList.Add(dsList[0].StartPoint);
            //            break;
            //        case dsList[1].StartPoint:
            //            dpList.Add(dsList[0].StartPoint);
            //            dpList.Add(dsList[0].EndPoint);
            //            dpList.Add(dsList[1].StartPoint);
            //            dpList.Add(dsList[1].EndPoint);
            //            break;
            //        case dsList[1].EndPoint:
            //            dpList.Add(dsList[0].StartPoint);
            //            dpList.Add(dsList[0].EndPoint);
            //            dpList.Add(dsList[1].EndPoint);
            //            dpList.Add(dsList[1].StartPoint);
            //            break;
            //        default:
            //            System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
            //            return;
            //    }
            #endregion
            BMECObject sanB = Santong(dpList);

        }

        public BMECObject Santong(List<DPoint3d> dpList)
        {
            BMECObject sanBmec = new BMECObject();
            BMECObject pipe1 = Createpipe(dpList[0], dpList[2]);
            BMECObject pipe2 = Createpipe(dpList[2], dpList[1]);
            BMECObject pipe3 = Createpipe(dpList[2], dpList[3]);
            IECInstance iecInstance = BMECInstanceManager.Instance.CreateECInstance("EQUAL_PIPE_TEE", true);
            double dn = Convert.ToDouble(DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER"));
            iecInstance["NOMINAL_DIAMETER"].DoubleValue = dn;
            string insulation_thickness = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            string insulation = DlgStandardPreference.GetPreferenceValue("INSULATION");
            ISpecProcessor spec = api.SpecProcessor;
            spec.FillCurrentPreferences(iecInstance, null);
            ECInstanceList ecList = spec.SelectSpec(iecInstance, true);
            if (ecList != null && ecList.Count > 0)
            {
                iecInstance = ecList[0];
                iecInstance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(insulation_thickness);
                iecInstance["INSULATION"].StringValue = insulation;
                double outlength = iecInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                double runlength = iecInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue;
                double branchlength = iecInstance["DESIGN_LENGTH_CENTER_TO_BRANCH_END"].DoubleValue;
                DVector3d dv1 = new DVector3d(dpList[2], dpList[0]);
                //double dv1length = dv1.Magnitude;
                //DVector3d unitdv1 = DVector3d.Multiply(dv1, 1 / dv1length);
                DPoint3d dp1 = yidong(dpList[2], dv1, outlength * 1000);
                DVector3d dv2 = new DVector3d(dpList[2], dpList[1]);
                DPoint3d dp2 = yidong(dpList[2], dv2, runlength * 1000);
                DVector3d dv3 = new DVector3d(dpList[2], dpList[3]);
                DPoint3d dp3 = yidong(dpList[2], dv3, branchlength * 1000);
                pipe1.SetLinearPoints(dpList[0], dp1);
                pipe1.Create();
                pipe2.SetLinearPoints(dp2, dpList[1]);
                pipe2.Create();
                pipe3.SetLinearPoints(dp3, dpList[3]);
                pipe3.Create();
                sanBmec = new BMECObject(iecInstance);
                //sanBmec.SetLinearPoints(dp1, dp2);

                //DTransform3d pipeTran = pipe1.Transform3d;
                DVector3d dvc = new DVector3d(0, 0, 1);
                DMatrix3d dm3d=new DMatrix3d(0,0,0,0,0,0,0,0,0);
                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvc,dv3,ref dm3d);

                DTransform3d dtranform = DTransform3d.FromTranslation(dp1);
                DMatrix3d dm = DMatrix3d.FromRows(new DVector3d(dv2.X, 0, 0), new DVector3d(0, dv2.Y, 0), new DVector3d(0, 0, dv2.Z));
                //dtranform.Matrix = dm;
                //sanBmec.Transform3d = pipeTran;
                //sanBmec.Transform3d.Translation = dp1;
                //sanBmec.SetTransform
                dtranform.Matrix = dm3d;
                sanBmec.Transform3d = dtranform;

                sanBmec.Create();
                try
                {
                    pipe1.DiscoverConnectionsEx();
                    pipe1.UpdateConnections();
                    pipe2.DiscoverConnectionsEx();
                    pipe2.UpdateConnections();
                    pipe3.DiscoverConnectionsEx();
                    pipe3.UpdateConnections();
                    sanBmec.DiscoverConnectionsEx();
                    sanBmec.UpdateConnections();
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("PipeLine不能为空");
                    return null;
                }
                //api.TranslateComponent(sanBmec, dp1); //平移
                //sanBmec.Create();
                //ec_object.Transform3d(DTransform3d.FromMatrixAndFixedPoint(,));
            }
            return sanBmec;
        }

        public BMECObject Createpipe(DPoint3d startDpoint, DPoint3d endDpoint)
        {
            //DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            IECInstance iecInstance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);
            double dn = Convert.ToDouble(DlgStandardPreference.GetPreferenceValue("NOMINAL_DIAMETER"));
            iecInstance["NOMINAL_DIAMETER"].DoubleValue = dn;
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(iecInstance, null);
            ECInstanceList ec_instance_list = specProcessor.SelectSpec(iecInstance, true);
            BMECObject ec_object = new BMECObject();
            string insulation_thickness = DlgStandardPreference.GetPreferenceValue("INSULATION_THICKNESS");
            string insulation = DlgStandardPreference.GetPreferenceValue("INSULATION");
            if (ec_instance_list != null && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                instance["INSULATION_THICKNESS"].DoubleValue = Convert.ToDouble(insulation_thickness);
                instance["INSULATION"].StringValue = insulation;
                ec_object = new BMECObject(instance);
                ec_object.SetLinearPoints(startDpoint, endDpoint);
                ec_object.Create();
            }            
            return ec_object;
        }

        public DPoint3d yidong(DPoint3d originDp, DVector3d dv, double juli)
        {
            DPoint3d targetDp = new DPoint3d();
            double dvLength = dv.Magnitude;
            DVector3d unitDv = DVector3d.Multiply(dv, 1 / dvLength);
            targetDp = DPoint3d.Add(originDp, unitDv, juli);
            return targetDp;
        }

        public bool PdDian()
        {
            //bool result = true;
            #region 得到起始线以及判断是否满足生成管道的要求
            dpZds();
            int outLinePoint = 0, duandian = 0, santong = 0;
            bool firstL = true;
            for (int i = 1; i <= i_line; i++)
            {
                List<DPoint3d> dpList = new List<DPoint3d>();
                List<DPoint3d> dpList2 = new List<DPoint3d>();
                dpList = new_PointLine[i];
                int jiaodian = 0;
                int zuojiaodian = 0, youjiaodian = 0;
                bool iscw = false;
                for (int dp2 = 1; dp2 <= i_line; dp2++)
                {
                    if (dp2 != i)
                    {
                        dpList2 = new_PointLine[dp2];
                        bool b1 = distence(dpList[0], dpList[dpList.Count - 1]);
                        bool b2 = distence(dpList2[0], dpList2[dpList2.Count - 1]);
                        bool b3 = distence(dpList[0], dpList2[0]);
                        bool b4 = distence(dpList[0], dpList2[dpList2.Count - 1]);
                        bool b5 = distence(dpList[dpList.Count - 1], dpList2[0]);
                        bool b6 = distence(dpList[dpList.Count - 1], dpList2[dpList2.Count - 1]);
                        if (!b1 && !b2)//dpList[0] != dpList[dpList.Count - 1] && dpList2[0] != dpList2[dpList2.Count - 1]
                        {
                            if ((b3 || b4) && (b5 || b6))
                            {
                                //CleraPublic();
                                //System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
                                //BMECApi.Instance.StartDefaultCommand();
                                //return false;
                                outLineList.Add(i, dpList);
                                outLinePoint++;
                                iscw = true;
                                break;
                            }
                            else
                            {
                                if (b3 || b4)
                                {
                                    zuojiaodian++;
                                    jiaodian++;
                                }
                                else if (b5 || b6)
                                {
                                    youjiaodian++;
                                    jiaodian++;
                                }
                            }
                        }
                        else
                        {
                            //CleraPublic();
                            //System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
                            //BMECApi.Instance.StartDefaultCommand();
                            //return false;
                            outLineList.Add(i, dpList);
                            outLinePoint++;
                            iscw = true;
                            break;
                        }
                    }
                }
                if (zuojiaodian + youjiaodian == 0 && !iscw)
                {
                    bool qsds = false, zdds = false, zguli = true,yguli=true;
                    int zhongj = 0;
                    foreach(KeyValuePair<int,List<DSegment3d>> kv in xDuanList)
                    {
                        if(kv.Key!=i)
                        {
                            if(!qsds)
                            {
                                qsds = isPdDpDs(dpList[0], kv.Value);
                                if(qsds)
                                {
                                    zguli=isGuli(kv.Key);
                                }
                            }
                            if(!zdds)
                            {
                                zdds = isPdDpDs(dpList[dpList.Count - 1], kv.Value);
                                if(zdds)
                                {
                                    yguli = isGuli(kv.Key);
                                }
                            }
                            bool zjds = false, zjguli = true;
                            zjds = isPdDpDs(kv.Value[0].StartPoint, xDuanList[i]);
                            if (zjds)
                            {
                                zjguli = isGuli(kv.Key);
                                if (zjguli)
                                {
                                    zhongj += 2;
                                }
                                else
                                {
                                    zhongj++;
                                }
                                //zjds = isPdDpDs(kv.Value[0].StartPoint, xDuanList[outLinePoint]);
                                zhongj++;
                            }
                            bool zjds1 = false, zjguli1 = true;
                            zjds1 = isPdDpDs(kv.Value[kv.Value.Count - 1].EndPoint, xDuanList[i]);
                            if (zjds1)
                            {
                                zjguli1 = isGuli(kv.Key);
                                if (zjguli1)
                                {
                                    zhongj += 2;
                                }
                                else
                                {
                                    zhongj++;
                                }
                                //zjds = isPdDpDs(kv.Value[kv.Value.Count - 1].EndPoint, xDuanList[outLinePoint]);
                                //zhongj++;
                            }
                        }
                    }
                    //foreach
                    if(!qsds&&!zdds&&zhongj==0)
                    {
                        outLineList.Add(i, dpList);
                        outLinePoint++;
                    }
                    else if(!qsds&&!zdds)
                    {
                        duandian += 2;
                        //santong = santong + zhongj * 3 / 2;
                        if (firstL)
                        {
                            firstPoint = i;
                            firstL = false;
                        }
                    }
                    else if((qsds&&!zdds)||(!qsds&&zdds))
                    {
                        duandian++;
                        //santong = santong + zhongj * 3 / 2;
                        santong += 3;
                        if (firstL)
                        {
                            firstPoint = i;
                            firstL = false;
                        }
                    }
                    else
                    {
                        //santong = santong + zhongj * 3 / 2;
                    }                    
                }
                else if (zuojiaodian > 2 || youjiaodian > 2)
                {
                    //CleraPublic();
                    //System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
                    //BMECApi.Instance.StartDefaultCommand();
                    //return false;
                    outLineList.Add(i, dpList);
                    outLinePoint++;
                    //iscw = true;
                    //break;
                }
                else if ((zuojiaodian > 0 && youjiaodian == 0) || (zuojiaodian == 0 && youjiaodian > 0))
                {
                    bool qsds = false, zdds = false;
                    //int zhongj = 0;
                    if(zuojiaodian==0)
                    {
                        foreach (KeyValuePair<int, List<DSegment3d>> kv in xDuanList)
                        {
                            if (kv.Key != i)
                            {
                                if (!qsds)
                                {
                                    qsds = isPdDpDs(dpList[0], kv.Value);
                                }
                            }
                        }
                    }
                    if(youjiaodian==0)
                    {
                        foreach (KeyValuePair<int, List<DSegment3d>> kv in xDuanList)
                        {
                            if (kv.Key != i)
                            {
                                if (!zdds)
                                {
                                    qsds = isPdDpDs(dpList[dpList.Count-1], kv.Value);
                                }
                            }
                        }
                    }
                    
                    //foreach
                    if (!qsds && !zdds)
                    {
                        if (firstL)
                        {
                            firstPoint = i;
                            firstL = false;
                        }
                        duandian++;
                    }
                    else
                    {
                        santong += 3;
                    }
                }
                if (zuojiaodian == 2)
                {
                    santong++;
                }
                if (youjiaodian == 2)
                {
                    santong++;
                }
                //else if (jiaodian == 2)
                //{

                //}

                //else
                //{
                //    CleraPublic();
                //    System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
                //    BMECApi.Instance.StartDefaultCommand();
                //    return;
                //}
            }

            if (outLineList.Count > 0)
            {
                if (indexElement.Count != 1)
                {
                    List<Element> elementList = new List<Element>();
                    System.Windows.Forms.MessageBox.Show("存在无法生成管道的线");
                    BMECApi.Instance.StartDefaultCommand();
                    SelectionSetManager.EmptyAll();
                    foreach (KeyValuePair<int, List<DPoint3d>> kv in outLineList)
                    {
                        //int indexOut = indexPoint[kv.Value];
                        Element outLine = indexElement[kv.Key-1];
                        elementList.Add(outLine);
                        //SelectionSetManager.AddElement(outLine, Session.Instance.GetActiveDgnModelRef());
                    }
                    isolateElementFrom isoFrom = new isolateElementFrom(elementList);
#if DEBUG
#else
            isoFrom.AttachAsTopLevelForm(MyAddin.s_addin, false);
            
#endif
                    isoFrom.Show();
                    CleraPublic();
                    return false;
                }
            }
            if (indexElement.Count != 1)
            {
                if (duandian != santong / 3 + 2)
                {
                    System.Windows.Forms.MessageBox.Show("存在闭合的线");
                    BMECApi.Instance.StartDefaultCommand();
                    CleraPublic();
                    return false;
                }
            }
            return true;    
            #endregion
        }

        /// <summary>
        /// 将点转化为线段
        /// </summary>
        public void dpZds()
        {
            foreach(KeyValuePair<int,List<DPoint3d>> kv in new_PointLine)
            {
                List<DSegment3d> dsList = new List<DSegment3d>();
                for(int dp=0;dp<kv.Value.Count-1;dp++)
                {
                    DSegment3d dsxl = new DSegment3d(kv.Value[dp], kv.Value[dp + 1]);
                    dsList.Add(dsxl);
                }
                xDuanList.Add(kv.Key, dsList);
            }
        }

        /// <summary>
        /// 判断点是否在线段上
        /// </summary>
        /// <param name="dp">需要判断的点</param>
        /// <param name="dsList">线段的集合</param>
        /// <returns>true为在线段上</returns>
        public bool isPdDpDs(DPoint3d dp,List<DSegment3d> dsList)
        {
            bool isps = false;
            double bili = 0;
            DPoint3d jindian = new DPoint3d();
            foreach(DSegment3d ds in dsList)
            {
                ds.ClosestFractionAndPoint(dp, true, out bili, out jindian);
                bool isdp = distence(dp, jindian);
                if(isdp)
                {
                    isps = true;
                }
            }
            return isps;
        }
        
        /// <summary>
        /// 判断是否是孤立元素
        /// </summary>
        /// <param name="key">索引</param>
        /// <returns>false为孤立元素</returns>
        public bool isGuli(int key)
        {
            bool isgu = false;
            foreach(KeyValuePair<int,List<DPoint3d>> kv in new_PointLine)
            {
                if(kv.Key!=key)
                {
                    bool b1 = distence(new_PointLine[key][0], new_PointLine[kv.Key][0]);
                    bool b2 = distence(new_PointLine[key][0], new_PointLine[kv.Key][new_PointLine[kv.Key].Count-1]);
                    bool b3 = distence(new_PointLine[key][new_PointLine[key].Count - 1], new_PointLine[kv.Key][0]);
                    bool b4 = distence(new_PointLine[key][new_PointLine[key].Count - 1], new_PointLine[kv.Key][new_PointLine[kv.Key].Count - 1]);
                    if(b1||b2||b3||b4)
                    {
                        isgu = true;
                    }
                }
            }
            return isgu;
        }

        public int dgchishu = 0;
        public List<int> keyList = new List<int>();
        public void scTree(BoTree<Segment> tree)
        {
            dgchishu++;
            if (dgchishu>dsTreeList.Count)
            {
                return;
            }
            DPoint3d lianjieDp = tree.Data.Lianjiedian;
            DSegment3d xiandaun = tree.Data.Xianduan;
            //DPoint3d otherDp = new DPoint3d();
            double lianjiest = lianjieDp.Distance(xiandaun.StartPoint);//读的时候用，确定连接点是线段中时的流向
            //if(lianjieDp==xiandaun.StartPoint)
            //{
            //    otherDp = xiandaun.EndPoint;
            //}
            //else
            //{
            //    otherDp = xiandaun.StartPoint;
            //}
            List<BoTree<Segment>> nodeList = new List<BoTree<Segment>>();
            List<DSegment3d> startDs = new List<DSegment3d>();
            List<DSegment3d> endDs = new List<DSegment3d>();
            foreach(DSegment3d ds in dsTreeList)
            {
                if(!(ds.StartPoint==xiandaun.StartPoint&&ds.EndPoint==xiandaun.EndPoint))
                {
                    DSegment3d closestSegment = new DSegment3d();
                    double fa = 0, fb = 0;
                    DPoint3d jiaodian = new DPoint3d();
                    bool isjiaodian = pdDsegment(xiandaun, ds, out jiaodian);
                    bool isczds = DSegment3d.ClosestApproachSegment(xiandaun, ds, out closestSegment, out fa, out fb);
                    bool isxs = distence(closestSegment.StartPoint, closestSegment.EndPoint);
                    if(isjiaodian)
                    {
                        bool islianjiedp = distence(jiaodian, lianjieDp);
                        if(!islianjiedp)
                        {
                            bool isotherDp = distence(jiaodian, xiandaun.StartPoint);
                            bool isotherDp1 = distence(jiaodian, xiandaun.EndPoint);
                            if(!isotherDp&&!isotherDp1)
                            {
                                bool dss = distence(jiaodian, ds.StartPoint);
                                bool dse = distence(jiaodian, ds.EndPoint);
                                if(dss||dse)
                                {
                                    double distence = jiaodian.Distance(xiandaun.StartPoint);
                                    bool issantong = true;
                                    bool otherdp = false;
                                    int xianjiaoshu = 0;
                                    BoTree<Segment> lianjieTree = new BoTree<Segment>();
                                    lianjieTree.Data = new Segment(distence, issantong, otherdp, jiaodian, ds, xianjiaoshu);
                                    nodeList.Add(lianjieTree);
                                    List<DSegment3d> isdsList = new List<DSegment3d>();
                                    isdsList.Add(xiandaun);
                                    isdsList.Add(ds);
                                    bool isCz = pdCz(isdsList);
                                    if(!isCz)
                                    {
                                        keyList.AddRange(seleKey(isdsList));
                                    }
                                }
                            }
                            else if(isotherDp)
                            {
                                bool dss = distence(jiaodian, ds.StartPoint);
                                bool dse = distence(jiaodian, ds.EndPoint);
                                if(!dss&&!dse)
                                {
                                    double distence = jiaodian.Distance(xiandaun.StartPoint);
                                    bool issantong = true;
                                    bool otherdp = true;
                                    int xianjiaoshu = 1;
                                    BoTree<Segment> lianjieTree = new BoTree<Segment>();
                                    lianjieTree.Data = new Segment(distence, issantong, otherdp, jiaodian, ds, xianjiaoshu);
                                    nodeList.Add(lianjieTree);
                                    List<DSegment3d> isdsList = new List<DSegment3d>();
                                    isdsList.Add(xiandaun);
                                    isdsList.Add(ds);
                                    bool isCz = pdCz(isdsList);
                                    if (!isCz)
                                    {
                                        keyList.AddRange(seleKey(isdsList));
                                    }
                                }
                                else
                                {
                                    startDs.Add(ds);
                                }
                            }
                            else
                            {
                                bool dss = distence(jiaodian, ds.StartPoint);
                                bool dse = distence(jiaodian, ds.EndPoint);
                                if (!dss && !dse)
                                {
                                    double distence = jiaodian.Distance(xiandaun.StartPoint);
                                    bool issantong = true;
                                    bool otherdp = true;
                                    int xianjiaoshu = 1;
                                    BoTree<Segment> lianjieTree = new BoTree<Segment>();
                                    lianjieTree.Data = new Segment(distence, issantong, otherdp, jiaodian, ds, xianjiaoshu);
                                    nodeList.Add(lianjieTree);
                                    List<DSegment3d> isdsList = new List<DSegment3d>();
                                    isdsList.Add(xiandaun);
                                    isdsList.Add(ds);
                                    bool isCz = pdCz(isdsList);
                                    if (!isCz)
                                    {
                                        keyList.AddRange(seleKey(isdsList));
                                    }
                                }
                                else
                                {
                                    endDs.Add(ds);
                                }
                            }
                        }
                    }
                }
            }

            if(startDs.Count>0)
            {
                if(startDs.Count==1)
                {
                    double stdistence = 0;
                    bool stsantong = false;
                    bool stother = true;
                    int stxianjiaoshu = 1;
                    BoTree<Segment> stlianjieTree = new BoTree<Segment>();
                    stlianjieTree.Data = new Segment(stdistence, stsantong, stother, xiandaun.StartPoint, startDs[0], stxianjiaoshu);
                    nodeList.Add(stlianjieTree);
                }
                else
                {
                    double stdistence = 0;
                    bool stsantong = true;
                    bool stother = true;
                    int stxianjiaoshu = 2;
                    BoTree<Segment> stlianjieTree1 = new BoTree<Segment>();
                    stlianjieTree1.Data = new Segment(stdistence, stsantong, stother, xiandaun.StartPoint, startDs[0], stxianjiaoshu);
                    BoTree<Segment> stlianjieTree2 = new BoTree<Segment>();
                    stlianjieTree2.Data = new Segment(stdistence, stsantong, stother, xiandaun.StartPoint, startDs[1], stxianjiaoshu);
                    nodeList.Add(stlianjieTree1);
                    nodeList.Add(stlianjieTree2);
                    List<DSegment3d> isdsList = new List<DSegment3d>();
                    isdsList.Add(xiandaun);
                    isdsList.Add(startDs[0]);
                    isdsList.Add(startDs[1]);
                    bool isCz = pdCz(isdsList);
                    if (!isCz)
                    {
                        keyList.AddRange(seleKey(isdsList));
                    }
                }
            }

            if(endDs.Count>0)
            {
                if(endDs.Count==1)
                {
                    double enddistence = xiandaun.EndPoint.Distance(xiandaun.StartPoint);
                    bool endsantong = false, endother = true;
                    int endxianjiaoshu = 1;
                    BoTree<Segment> endlianjieTree = new BoTree<Segment>();
                    endlianjieTree.Data = new Segment(enddistence, endsantong, endother, xiandaun.EndPoint, endDs[0], endxianjiaoshu);
                    nodeList.Add(endlianjieTree);
                }
                else
                {
                    double enddistence = xiandaun.EndPoint.Distance(xiandaun.StartPoint);
                    bool endsantong = true, endother = true;
                    int endxianjiaoshu = 2;
                    BoTree<Segment> endlianjieTree1 = new BoTree<Segment>();
                    endlianjieTree1.Data = new Segment(enddistence, endsantong, endother, xiandaun.EndPoint, endDs[0], endxianjiaoshu);
                    BoTree<Segment> endlianjieTree2 = new BoTree<Segment>();
                    endlianjieTree2.Data = new Segment(enddistence, endsantong, endother, xiandaun.EndPoint, endDs[1], endxianjiaoshu);
                    nodeList.Add(endlianjieTree1);
                    nodeList.Add(endlianjieTree2);
                    List<DSegment3d> isdsList = new List<DSegment3d>();
                    isdsList.Add(xiandaun);
                    isdsList.Add(endDs[0]);
                    isdsList.Add(endDs[1]);
                    bool isCz = pdCz(isdsList);
                    if (!isCz)
                    {
                        keyList.AddRange(seleKey(isdsList));
                    }
                }
            }

            if(nodeList.Count>0)
            {
                List<BoTree<Segment>> startNodeList = new List<BoTree<Segment>>();
                List<BoTree<Segment>> endNodeList = new List<BoTree<Segment>>();
                foreach(BoTree<Segment> treeds in nodeList)
                {
                    if(treeds.Data.Distence< lianjiest)
                    {
                        startNodeList.Add(treeds);
                    }
                    else
                    {
                        endNodeList.Add(treeds);
                    }
                }
                if(startNodeList.Count>0)
                {
                    if(startNodeList.Count==1)
                    {
                        //bool pdcz = pdIsczTree(startNodeList[0]);
                        //if(!pdcz)
                        //{
                        //    tree.AddNode(startNodeList[0]);
                        //}
                        tree.AddNode(startNodeList[0]);
                    }
                    else
                    {
                        for (int st = 0; st < startNodeList.Count - 1; st++)
                        {
                            for (int st1 = st + 1; st1 < startNodeList.Count; st1++)
                            {
                                if (startNodeList[st].Data.Distence <= startNodeList[st1].Data.Distence)
                                {
                                    BoTree<Segment> seg1 = new BoTree<Segment>();
                                    seg1 = startNodeList[st];
                                    startNodeList[st] = startNodeList[st1];
                                    startNodeList[st1] = seg1;
                                }
                            }
                            //bool pdcz1 = pdIsczTree(startNodeList[st]);
                            //if(!pdcz1)
                            //{
                            //    tree.AddNode(startNodeList[st]);
                            //}
                            tree.AddNode(startNodeList[st]);
                        }
                        //bool pdcz2 = pdIsczTree(startNodeList[startNodeList.Count - 1]);
                        //if(!pdcz2)
                        //{
                        //    tree.AddNode(startNodeList[startNodeList.Count - 1]);
                        //}
                        tree.AddNode(startNodeList[startNodeList.Count - 1]);
                    }
                    
                }
                if(endNodeList.Count>0)
                {
                    if(endNodeList.Count==1)
                    {
                        //bool pdcz3 = pdIsczTree(endNodeList[0]);
                        //if (!pdcz3)
                        //{
                        //    tree.AddNode(endNodeList[0]);
                        //}
                        //tree.AddNode(endNodeList[0]);
                        tree.AddNode(endNodeList[0]);
                    }
                    else
                    {
                        for (int end = 0; end < endNodeList.Count - 1; end++)
                        {
                            for (int end1 = end + 1; end1 < endNodeList.Count; end1++)
                            {
                                if (endNodeList[end].Data.Distence >= endNodeList[end1].Data.Distence)
                                {
                                    BoTree<Segment> seg1 = new BoTree<Segment>();
                                    seg1 = endNodeList[end];
                                    endNodeList[end] = endNodeList[end1];
                                    endNodeList[end1] = seg1;
                                }
                            }
                            //bool pdcz4 = pdIsczTree(endNodeList[end]);
                            //if (!pdcz4)
                            //{
                            //    tree.AddNode(endNodeList[end]);
                            //}
                            tree.AddNode(endNodeList[end]);
                        }
                        //bool pdcz5 = pdIsczTree(endNodeList[endNodeList.Count - 1]);
                        //if (!pdcz5)
                        //{
                        //    tree.AddNode(endNodeList[endNodeList.Count - 1]);
                        //}
                        tree.AddNode(endNodeList[endNodeList.Count - 1]);
                    }                    
                }
            }
            if(tree.Nodes.Count>0)
            {
                foreach(var item in tree.Nodes)
                {
                    scTree(item);
                }
            }
        }

        public bool pdDsegment(DSegment3d ds1,DSegment3d ds2,out DPoint3d jd)
        {
            jd = new DPoint3d();
            bool isjiaodain = false;
            DPoint3d dp1 = ds1.StartPoint;
            DPoint3d dp2 = ds1.EndPoint;
            DPoint3d dp3 = ds2.StartPoint;
            DPoint3d dp4 = ds2.EndPoint;
            bool b1 = distence(dp1, dp3);
            bool b2 = distence(dp1, dp4);
            bool b3 = distence(dp2, dp3);
            bool b4 = distence(dp2, dp4);
            if(b1)
            {
                isjiaodain = true;
                jd = dp1;
            }
            else if(b2)
            {
                isjiaodain = true;
                jd = dp1;
            }
            else if(b3)
            {
                isjiaodain = true;
                jd = dp2;
            }
            else if(b4)
            {
                isjiaodain = true;
                jd = dp2;
            }
            else
            {
                DVector3d dv1 = new DVector3d(dp1,dp2);
                DVector3d dv2 = new DVector3d(dp3, dp4);
                double jiaodu = dv1.AngleTo(dv2).Degrees;
                DVector3d dv12s = new DVector3d(dp1, dp3);
                DVector3d dv12e = new DVector3d(dp1, dp4);
                DVector3d dv21s = new DVector3d(dp3, dp1);
                DVector3d dv21e = new DVector3d(dp3, dp2);
                if(jiaodu!=0)
                {
                    bool is12s = dv1.IsParallelTo(dv12s);
                    if (is12s)
                    {
                        DVector3d dv12se = new DVector3d(dp3, dp2);
                        bool is12se = dv1.IsParallelTo(dv12se);
                        if (is12se)
                        {
                            //double jiaodu = dv1.AngleTo(dv2).Degrees;
                            isjiaodain = true;
                            jd = dp3;
                        }
                    }
                    bool is12e = dv1.IsParallelTo(dv12e);
                    if (is12e)
                    {
                        DVector3d dv12es = new DVector3d(dp4, dp2);
                        bool is12es = dv1.IsParallelTo(dv12es);
                        if (is12es)
                        {
                            isjiaodain = true;
                            jd = dp4;
                        }
                    }
                    bool is21s = dv2.IsParallelTo(dv21s);
                    if (is21s)
                    {
                        DVector3d dv21se = new DVector3d(dp1, dp4);
                        bool is21se = dv2.IsParallelTo(dv21se);
                        if (is21se)
                        {
                            isjiaodain = true;
                            jd = dp1;
                        }
                    }
                    bool is21e = dv2.IsParallelTo(dv21e);
                    if (is21e)
                    {
                        DVector3d dv21es = new DVector3d(dp2, dp4);
                        bool is21es = dv2.IsParallelTo(dv21es);
                        if (is21es)
                        {
                            isjiaodain = true;
                            jd = dp2;
                        }
                    }
                }
                
            }
            return isjiaodain;
        }

        public bool pdIsczTree(BoTree<Segment> tree)
        {
            bool pd = false;
            dg(startTree, tree);
            if(isdg)
            {
                pd = true;
            }
            return pd;
        }

        bool isdg = false;
        public void dg(BoTree<Segment> tree,BoTree<Segment> orgiontree)
        {
            bool b1 = distence(tree.Data.Xianduan.StartPoint, orgiontree.Data.Xianduan.StartPoint);
            bool b2 = distence(tree.Data.Xianduan.EndPoint, orgiontree.Data.Xianduan.EndPoint);
            if(b1&&b2)
            {
                isdg = true;
            }
            if(tree.Nodes.Count>0)
            {
                foreach(BoTree<Segment> tree1 in tree.Nodes)
                {
                    dg(tree1, orgiontree);
                }
            }
            //return isdg;
        }

        public bool pdCz(List<DSegment3d> dsList)
        {
            bool iscz = true;
            if(dsList.Count==2)
            {
                DVector3d dv1 = new DVector3d(dsList[0].StartPoint, dsList[0].EndPoint);
                DVector3d dv2 = new DVector3d(dsList[1].StartPoint, dsList[1].EndPoint);
                double jiaodu = dv1.AngleTo(dv2).Degrees;
                if(Math.Abs(90-jiaodu)>1)
                {
                    iscz = false;
                }
            }
            if(dsList.Count==3)
            {
                DVector3d dv1 = new DVector3d(dsList[0].StartPoint, dsList[0].EndPoint);
                DVector3d dv2 = new DVector3d(dsList[1].StartPoint, dsList[1].EndPoint);
                DVector3d dv3 = new DVector3d(dsList[2].StartPoint, dsList[2].EndPoint);
                bool b1 = dv1.IsParallelOrOppositeTo(dv2);
                bool b2 = dv1.IsParallelOrOppositeTo(dv3);
                bool b3 = dv2.IsParallelOrOppositeTo(dv3);
                if(b1||b2||b3)
                {
                    if(b1)
                    {
                        double jiaodu = dv1.AngleTo(dv3).Degrees;
                        if (Math.Abs(90 - jiaodu) > 1)
                        {
                            iscz = false;
                        }
                    }
                    else if(b2)
                    {
                        double jiaodu = dv1.AngleTo(dv2).Degrees;
                        if (Math.Abs(90 - jiaodu) > 1)
                        {
                            iscz = false;
                        }
                    }
                    else
                    {
                        double jiaodu = dv1.AngleTo(dv2).Degrees;
                        if (Math.Abs(90 - jiaodu) > 1)
                        {
                            iscz = false;
                        }
                    }
                }
                else
                {
                    iscz = false;
                }
            }
            return iscz;
        }

        public List<int> seleKey(List<DSegment3d> dsList)
        {
            List<int> dsKeyList = new List<int>();
            foreach(DSegment3d ds in dsList)
            {
                foreach(KeyValuePair<int,List<DSegment3d>> kv in xDuanList)
                {
                    bool isCz = kv.Value.Contains(ds);
                    if(isCz)
                    {
                        if(dsKeyList.Count==0)
                        {
                            dsKeyList.Add(kv.Key);
                        }
                        else
                        {
                            bool isCz2 = dsKeyList.Contains(kv.Key);
                            if(!isCz2)
                            {
                                dsKeyList.Add(kv.Key);
                            }
                        }
                    }
                }
            }
            return dsKeyList;
        }
    }
}
