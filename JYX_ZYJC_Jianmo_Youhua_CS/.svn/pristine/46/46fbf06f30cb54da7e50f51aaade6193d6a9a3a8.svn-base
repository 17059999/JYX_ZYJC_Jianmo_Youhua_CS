using Bentley.Building.Mechanical.Api;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using JYX_ZYJC_Jianmo_Youhua_CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Bentley.GeometryNET.CurvePrimitive;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class multi_zhinengxian_to_pipe
    {
        //private static List<Element> elementList = new List<Element>();
        private  int i_line = 0;
        private  Dictionary<int, List<DPoint3d>> PointLine = new Dictionary<int, List<DPoint3d>>();
        private  int firstPoint = 1;
        private  List<DPoint3d> PointLineString = new List<DPoint3d>();
        private  Dictionary<int, Element> indexElement = new Dictionary<int, Element>();
        private  List<Element> outLine = new List<Element>();
        private  Dictionary<List<DPoint3d>, int> indexPoint = new Dictionary<List<DPoint3d>, int>();
        private  Dictionary<int, List<DPoint3d>> outLineList = new Dictionary<int, List<DPoint3d>>();
        public   multi_zhinengxian_to_pipe()
        {
            
        }

        public void start()
        {
            double angle_tolerant = 1.0;
            double normal_diameter = 100;
            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请选中一个元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }
            int index = 0;
            for (uint i = 0; i < selected_elem_count; i++)
            {
                Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(i);
                indexElement.Add(index, elem);
                index++;
            }

            //Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetFirst();
            foreach (KeyValuePair<int, Element> kv in indexElement)
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
                    i_line++;
                    PointLine.Add(i_line, points_list);
                    indexPoint.Add(points_list, kv.Key);
                }
                else if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.Line)
                {
                    List<DPoint3d> points_list = new List<DPoint3d>();
                    DSegment3d LinePoint;
                    curveprimitive.TryGetLine(out LinePoint);
                    points_list.Add(LinePoint.StartPoint);
                    points_list.Add(LinePoint.EndPoint);
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
            }

            //int firstPoint = 0;
            int outLinePoint = 0;
            #region 得到起始线以及判断是否满足生成管道的要求
            for (int i = 1; i <= i_line; i++)
            {
                List<DPoint3d> dpList = new List<DPoint3d>();
                List<DPoint3d> dpList2 = new List<DPoint3d>();
                dpList = PointLine[i];
                int jiaodian = 0;
                for (int dp2 = 1; dp2 <= i_line; dp2++)
                {
                    if (dp2 != i)
                    {
                        dpList2 = PointLine[dp2];
                        if (dpList[0] != dpList[dpList.Count - 1] && dpList2[0] != dpList2[dpList2.Count - 1])
                        {
                            if ((dpList[0] == dpList2[0] || dpList[0] == dpList2[dpList2.Count - 1]) && (dpList[dpList.Count - 1] == dpList2[0] || dpList[dpList.Count - 1] == dpList2[dpList2.Count - 1]))
                            {
                                CleraPublic();
                                System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
                                BMECApi.Instance.StartDefaultCommand();
                                return;
                            }
                            else
                            {
                                if (dpList[0] == dpList2[0] || dpList[0] == dpList2[dpList2.Count - 1])
                                {
                                    jiaodian++;
                                }
                                else if (dpList[dpList.Count - 1] == dpList2[0] || dpList[dpList.Count - 1] == dpList2[dpList2.Count - 1])
                                {
                                    jiaodian++;
                                }
                            }
                        }
                        else
                        {
                            CleraPublic();
                            System.Windows.Forms.MessageBox.Show("不要选择闭合的线");
                            BMECApi.Instance.StartDefaultCommand();
                            return;
                        }
                    }
                }
                if (jiaodian == 0)
                {
                    outLineList.Add(outLinePoint, dpList);
                    outLinePoint++;
                }
                else if (jiaodian == 1)
                {
                    firstPoint = i;
                }
                else if (jiaodian == 2)
                {

                }
                else
                {
                    CleraPublic();
                    System.Windows.Forms.MessageBox.Show("不要选择交叉的线");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }
            }

            if (outLineList.Count > 0)
            {
                if (indexElement.Count != 1)
                {
                    System.Windows.Forms.MessageBox.Show("存在没有交点的线");
                    BMECApi.Instance.StartDefaultCommand();
                    SelectionSetManager.EmptyAll();
                    foreach (KeyValuePair<int, List<DPoint3d>> kv in outLineList)
                    {
                        int indexOut = indexPoint[kv.Value];
                        Element outLine = indexElement[indexOut];
                        SelectionSetManager.AddElement(outLine, Session.Instance.GetActiveDgnModelRef());
                    }
                    CleraPublic();
                    return;
                }
            }

            #endregion
            List<DPoint3d> PointList = new List<DPoint3d>();
            PointList = lianjieLine(firstPoint); //将选中的元素连成线
            #region 判断相邻三个点是不是在一条直线上
            List<DPoint3d> new_points_list = new List<DPoint3d>();
            for (int i = 0; i < PointList.Count - 1; i++)
            {
                if (i == 0)
                {
                    new_points_list.Add(PointList[i]);
                    new_points_list.Add(PointList[i + 1]);
                }
                else
                {
                    DVector3d new_vector1 = new DVector3d(new_points_list[new_points_list.Count - 2], new_points_list[new_points_list.Count - 1]);
                    DVector3d new_vector2 = new DVector3d(PointList[i], PointList[i + 1]);

                    Angle angle = new_vector1.AngleTo(new_vector2);
                    if ((angle.Degrees - angle_tolerant < 0.001) || ((angle.Degrees - (180 - angle_tolerant) > 0.001) && (angle.Degrees - 180) < 0.001))
                    {
                        new_points_list[new_points_list.Count - 1] = PointList[i + 1];
                    }
                    else
                    {
                        new_points_list.Add(PointList[i + 1]);
                    }

                }
            }
            #endregion
            #region 生成管道
            List<BMECObject> bmec_object_list = new List<BMECObject>();
            for (int i = 0; i < new_points_list.Count - 1; i++)
            {

                DPoint3d dpt_start = new_points_list[i];
                DPoint3d dpt_end = new_points_list[i + 1];
                BMECObject bmec_object = OPM_Public_Api.create_pipe(dpt_start, dpt_end, normal_diameter);
                bmec_object.Create();
                //bmec_object.SetIntValue(null, 1001);
                bmec_object.DiscoverConnectionsEx();
                bmec_object.UpdateConnections();
                bmec_object_list.Add(bmec_object);
            }
            #endregion
            #region 对生成的管道进行回切操作
            string errorMessage = string.Empty;
            for (int i = 0; i < bmec_object_list.Count - 1; i++)
            {
                string errorMessage_temp;
                string PipeLine = bmec_object_list[i].Instance["LINENUMBER"].StringValue; //获取pdm中PipeLine的值
                //bmec_object_list[i].Instance.InstanceId
                if (PipeLine != "")
                {
                    try
                    {
                        BMECObject bmec_object1 = bmec_object_list[i];
                        ulong id1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object1);
                        bmec_object1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id1);
                        BMECObject bmec_object2 = bmec_object_list[i + 1];
                        ulong id2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object2);
                        bmec_object2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(id2);

                        OPM_Public_Api.huiqie_pipe(bmec_object1, bmec_object2, out errorMessage_temp);
                        errorMessage += errorMessage_temp;
                    }
                    catch
                    {
                        return;
                    }
                }
                else
                {
                    CleraPublic();
                    System.Windows.Forms.MessageBox.Show("请先设置PipeLine");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }
            }
            #endregion
            if (errorMessage.Length != 0)
            {
                System.Windows.Forms.MessageBox.Show(errorMessage);
            }
            CleraPublic();
        }

        /// <summary>
        /// 将智能线连接起来
        /// </summary>
        /// <param name="firstDp"></param>
        /// <returns></returns>
        public  List<DPoint3d> lianjieLine(int firstDp)
        {
            //List<DPoint3d> PointList = new List<DPoint3d>();
            int nextDP = firstDp;
            bool b = true;
            List<DPoint3d> firstDpLine = PointLine[nextDP];
            if (PointLine.Count == 1)
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
                    if (nextDP == firstPoint)
                    {
                        if (firstDpLine[0] == nextPoint[0] || firstDpLine[0] == nextPoint[nextPoint.Count - 1])
                        {
                            for (int s = firstDpLine.Count - 1; s >= 0; s--)
                            {
                                PointLineString.Add(firstDpLine[s]);
                            }
                            lianjieLine(i);
                        }
                        else if (firstDpLine[firstDpLine.Count - 1] == nextPoint[0] || firstDpLine[firstDpLine.Count - 1] == nextPoint[nextPoint.Count - 1])
                        {
                            for (int s = 0; s < firstDpLine.Count; s++)
                            {
                                PointLineString.Add(firstDpLine[s]);
                            }
                            lianjieLine(i);
                        }
                    }
                    else
                    {
                        if (firstDpLine[0] == PointLineString[PointLineString.Count - 1])
                        {
                            if (b)
                            {
                                for (int s = 1; s < firstDpLine.Count; s++)
                                {
                                    PointLineString.Add(firstDpLine[s]);
                                }
                                b = false;
                            }

                            if (PointLineString[PointLineString.Count - 1] == nextPoint[0] || PointLineString[PointLineString.Count - 1] == nextPoint[nextPoint.Count - 1])
                            {
                                lianjieLine(i);
                            }
                        }
                        else if (firstDpLine[firstDpLine.Count - 1] == PointLineString[PointLineString.Count - 1])
                        {
                            if (b)
                            {
                                for (int s = firstDpLine.Count - 2; s >= 0; s--)
                                {
                                    PointLineString.Add(firstDpLine[s]);
                                }
                                b = false;
                            }
                            if (PointLineString[PointLineString.Count - 1] == nextPoint[0] || PointLineString[PointLineString.Count - 1] == nextPoint[nextPoint.Count - 1])
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
        public void CleraPublic()
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
        }
    }
}
