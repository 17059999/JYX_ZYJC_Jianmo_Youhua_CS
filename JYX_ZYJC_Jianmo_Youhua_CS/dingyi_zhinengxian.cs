﻿
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.OpenPlantModeler.SDK.Utilities;
using System.Collections.Generic;
using static Bentley.GeometryNET.CurvePrimitive;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class dingyi_zhinengxian
    {

        Dictionary<int, List<DPoint3d>> xiabiao_elem = new Dictionary<int, List<DPoint3d>>();
        List<Element> index_elem = new List<Element>();
        int index = 0;
        int first_point = 0;
        int xiangshu_index = 0;
        int outLinePoint = 0;
        int bihe_index = 0;
        List<DPoint3d> list_paixu = new List<DPoint3d>();
        Dictionary<int, List<DPoint3d>> outLineList = new Dictionary<int, List<DPoint3d>>();
        Dictionary<List<DPoint3d>, int> elem_index = new Dictionary<List<DPoint3d>, int>();
        Dictionary<int, double> xiangshu_fangda1 = new Dictionary<int, double>();
        Dictionary<int, List<DPoint3d>> bihe_points = new Dictionary<int, List<DPoint3d>>();
        public BIM.Application app = Utilities.ComApp;  //V8I
        DPoint3d pianyiDpoint = new DPoint3d();

        public dingyi_zhinengxian()
        {

        }

        /// <summary>
        /// 一键生成智能线
        /// </summary>
        /// <param name="unparsed"></param>
        public void start()
        {

            Bentley.DgnPlatformNET.ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda);

            #region v8i获取元素
            //BIM.ElementEnumerator eleE = app.ActiveModelReference.GetSelectedElements();
            //BIM.Element[] eleList = eleE.BuildArrayFromContents();
            //foreach(BIM.Element vEle in eleList)
            //{
            //    Element elece = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(vEle);
            //    //DisplayableElement disele = (DisplayableElement)elece;
            //    //DPoint3d dp = new DPoint3d();
            //    //disele.GetTransformOrigin(out dp);
            //    index_elem.Add(elece);
            //}
            #endregion
            #region
            //uint tt = SelectionSetManager.NumSelected();
            //Element ee = new Element();
            //DgnModelRef dgnRef = Session.Instance.GetActiveDgnModelRef();
            //SelectionSetManager.GetElement(tt, ref ee, ref dgnRef);
            //ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
            #endregion

            double angle_tolerant = 1.0;
            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count == 0)
            {
                System.Windows.Forms.MessageBox.Show("请选中一个元素");
                
                BMECApi.Instance.StartDefaultCommand();
                CleraPublic();
                return;
            }
            //将选择元素放入集合中
            Element elem_xiabiao;
            for (uint i = 0; i < element_agenda.GetCount(); i++)
            {
                elem_xiabiao = element_agenda.GetEntry(i);
                if (elem_xiabiao.DgnModelRef.IsDgnAttachment)
                {
                    DPoint3d origin = DPoint3d.Zero;
                    elem_xiabiao.DgnModelRef.AsDgnAttachment().GetMasterOrigin(ref origin);
                    double uorpermas = elem_xiabiao.DgnModel.GetModelInfo().UorPerMaster;
                    double refUorpermas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                    UnitDefinition unitdef1 = elem_xiabiao.DgnModel.GetModelInfo().GetMasterUnit();
                    double xiangshu = 1;
                    if (unitdef1.Label == "m")
                    {
                        xiangshu = refUorpermas / uorpermas * 1000;
                    }
                    pianyiDpoint = new DPoint3d(origin.X / xiangshu, origin.Y / xiangshu, origin.Z / xiangshu);
                }
                index_elem.Add(elem_xiabiao);
            }

            bool mesh = false;
            foreach(Element ele1 in index_elem)
            {
                if(ele1.ElementType==MSElementType.MeshHeader)
                {
                    mesh = true;
                }
            }
            if(mesh)
            {
                //引用图纸，引用模型，引用元素
                //当前的像素
                int indexele = -1;
                for(int ii=0;ii<index_elem.Count;ii++)
                {
                    if(index_elem[ii].ElementType==MSElementType.LineString||index_elem[ii].ElementType==MSElementType.Line)
                    {
                        indexele = ii;
                        break;
                    }
                }
                if(indexele==-1)
                {
                    System.Windows.Forms.MessageBox.Show("所选择的元素中不存在智能线无法生成智能线!");
                    return;
                }
                Element ele = index_elem[indexele];
                double elem_uorpermas = ele.DgnModel.GetModelInfo().UorPerMaster;
                if (ele.DgnModelRef.IsDgnAttachment)
                {
                    DPoint3d origin = DPoint3d.Zero;

                    ele.DgnModelRef.AsDgnAttachment().GetMasterOrigin(ref origin);

                }
                //引用的像素
                double UorPerMas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                //当前的单位
                UnitDefinition unitdef = ele.DgnModel.GetModelInfo().GetMasterUnit();


                if (unitdef.Label == "m")
                {
                    //xiangshu_fangda = 1 / elem_uorpermas * 1000 * UorPerMas;
                    xiangshu_fangda1.Add(xiangshu_index, 1 / elem_uorpermas * 1000 * UorPerMas);
                    xiangshu_index++;
                }

                else
                {
                    xiangshu_fangda1.Add(xiangshu_index, 1);
                    xiangshu_index++;
                }

                //选中元素
                CurveVector curvevector = CurvePathQuery.ElementToCurveVector(ele);
                CurvePrimitive curveprimitive;
                try
                {
                    curveprimitive = curvevector.GetPrimitive(0);
                }
                catch
                {
                    System.Windows.Forms.MessageBox.Show("选中的元素无法转换为智能线");
                    BMECApi.Instance.StartDefaultCommand();
                    CleraPublic();
                    return;
                }
                if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.LineString)
                {
                    List<DPoint3d> points_list = new List<DPoint3d>();
                    curveprimitive.TryGetLineString(points_list);
                    for (int shi = 0; shi < points_list.Count; shi++)
                    {
                        points_list[shi] = points_list[shi] + pianyiDpoint;
                    }
                    xiabiao_elem.Add(index, points_list);
                    elem_index.Add(points_list, index);
                    index++;

                }

                else if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.Line)
                {
                    DSegment3d ds = new DSegment3d();
                    curveprimitive.TryGetLine(out ds);
                    List<DPoint3d> points_list1 = new List<DPoint3d>();
                    points_list1.Add(ds.StartPoint);
                    points_list1.Add(ds.EndPoint);
                    points_list1[0] += pianyiDpoint;
                    points_list1[1] += pianyiDpoint;
                    xiabiao_elem.Add(index, points_list1);
                    elem_index.Add(points_list1, index);
                    index++;
                }
            }
            
            //遍历集合的元素，将元素放入字典类进行修改和查找
            #region
            else
            {
                foreach (Element ele in index_elem)
                {
                    //引用图纸，引用模型，引用元素
                    //当前的像素
                    double elem_uorpermas = ele.DgnModel.GetModelInfo().UorPerMaster;
                    if (ele.DgnModelRef.IsDgnAttachment)
                    {
                        DPoint3d origin = DPoint3d.Zero;

                        ele.DgnModelRef.AsDgnAttachment().GetMasterOrigin(ref origin);

                    }
                    //引用的像素
                    double UorPerMas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                    //当前的单位
                    UnitDefinition unitdef = ele.DgnModel.GetModelInfo().GetMasterUnit();


                    if (unitdef.Label == "m")
                    {
                        //xiangshu_fangda = 1 / elem_uorpermas * 1000 * UorPerMas;
                        xiangshu_fangda1.Add(xiangshu_index, 1 / elem_uorpermas * 1000 * UorPerMas);
                        xiangshu_index++;
                    }

                    else
                    {
                        xiangshu_fangda1.Add(xiangshu_index, 1);
                        xiangshu_index++;
                    }

                    //选中元素
                    CurveVector curvevector = CurvePathQuery.ElementToCurveVector(ele);
                    CurvePrimitive curveprimitive;
                    try
                    {
                        curveprimitive = curvevector.GetPrimitive(0);
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("选中的元素无法转换为智能线");
                        BMECApi.Instance.StartDefaultCommand();
                        CleraPublic();
                        return;
                    }
                    if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.LineString)
                    {
                        List<DPoint3d> points_list = new List<DPoint3d>();
                        curveprimitive.TryGetLineString(points_list);
                        for (int shi = 0; shi < points_list.Count; shi++)
                        {
                            points_list[shi] = points_list[shi] + pianyiDpoint;
                        }
                        xiabiao_elem.Add(index, points_list);
                        elem_index.Add(points_list, index);
                        index++;

                    }

                    else if (curveprimitive.GetCurvePrimitiveType() == CurvePrimitiveType.Line)
                    {
                        DSegment3d ds = new DSegment3d();
                        curveprimitive.TryGetLine(out ds);
                        List<DPoint3d> points_list1 = new List<DPoint3d>();
                        points_list1.Add(ds.StartPoint);
                        points_list1.Add(ds.EndPoint);
                        points_list1[0] += pianyiDpoint;
                        points_list1[1] += pianyiDpoint;
                        xiabiao_elem.Add(index, points_list1);
                        elem_index.Add(points_list1, index);
                        index++;
                    }
                }
            }

            #endregion
            //如果元素的单位为"m"，则以"mm"为标准放大点的像素
            for (int i = 0; i < xiabiao_elem.Count; i++)
            {
                List<DPoint3d> list_1 = new List<DPoint3d>();
                list_1 = xiabiao_elem[i];
                for (int j = 0; j < list_1.Count; j++)
                {
                    list_1[j] = list_1[j] * xiangshu_fangda1[i];
                }
            }


            //找到满足起始元素的条件，把它作为起始元素，之后将所有元素进行排序
            for (int i = 0; i < xiabiao_elem.Count; i++)
            {
                List<DPoint3d> list_1 = new List<DPoint3d>();
                List<DPoint3d> list_2 = new List<DPoint3d>();
                list_1 = xiabiao_elem[i];
                int lian_jie_point = 0;
                for (int j = 0; j < xiabiao_elem.Count; j++)
                {

                    if (i != j)
                    {
                        list_2 = xiabiao_elem[j];
                        if ((list_1[0] != list_1[list_1.Count - 1]) && (list_2[0] != list_2[list_2.Count - 1]))
                        {

                            if ((list_1[0] == list_2[0] || list_1[0] == list_2[list_2.Count - 1]) && (list_1[list_1.Count - 1] == list_2[0] || list_1[list_1.Count - 1] == list_2[list_2.Count - 1]))
                            {
                                System.Windows.Forms.MessageBox.Show("不能生成闭合的线");
                                BMECApi.Instance.StartDefaultCommand();
                                CleraPublic();
                                return;
                            }
                            else
                            {
                                if (list_1[0] == list_2[0] || list_1[0] == list_2[list_2.Count - 1])
                                {
                                    lian_jie_point++;
                                }
                                else if (list_1[list_1.Count - 1] == list_2[0] || list_1[list_1.Count - 1] == list_2[list_2.Count - 1])
                                {
                                    lian_jie_point++;
                                }
                            }
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("不能生成单个点的线");
                            BMECApi.Instance.StartDefaultCommand();
                            CleraPublic();
                            return;
                        }
                    }
                }

                if (lian_jie_point == 0 && selected_elem_count > 1)
                {
                    outLineList.Add(outLinePoint, list_1);
                    outLinePoint++;

                }
                else if (lian_jie_point == 1)
                {
                    //找到只能是头或者尾，对应的选中的元素的索引
                    first_point = i;
                }
                else if (lian_jie_point == 2)
                {
                    bihe_points.Add(bihe_index, list_1);
                    bihe_index++;
                }
                else if (lian_jie_point > 2)
                {
                    System.Windows.Forms.MessageBox.Show("不能生成此类型的智能线");
                    BMECApi.Instance.DeferStartDefaultCommand();
                    CleraPublic();
                    return;
                }

            }

            //闭合判断
            List<DPoint3d> list_start = new List<DPoint3d>();
            List<DPoint3d> list_focus = new List<DPoint3d>();
            List<DPoint3d> list_keep = new List<DPoint3d>();

            foreach (KeyValuePair<int, List<DPoint3d>> points in bihe_points)
            {

                List<DPoint3d> list_1 = points.Value;
                for (int i = 0; i < bihe_points.Count; i++)
                {
                    if (points.Key != i)
                    {
                        List<DPoint3d> list_2 = new List<DPoint3d>();
                        list_2 = bihe_points[i];
                        if (list_1[0] == list_2[0] || list_1[0] == list_2[list_2.Count - 1])
                        {
                            //找到一个元素的起始点有交点的元素作为第一个元素
                            list_start = list_1;
                            //和第一个的起始点有交点的元素
                            list_focus = list_2;
                            break;
                        }
                    }
                }
                if (list_start.Count > 0 && list_focus.Count > 0)
                {
                    break;
                }

            }


            //保留集合，防止重复判断
            list_keep = list_start;

            for (int i = 0; i < bihe_points.Count; i++)
            {
                List<DPoint3d> list_1 = new List<DPoint3d>();

                for (int j = 0; j < bihe_points.Count; j++)
                {
                    list_1 = bihe_points[j];
                    if (list_focus != list_1 && bihe_points.Count > 2 && (list_keep != list_1))
                    {
                        if (list_focus[0] == list_1[0] || list_focus[0] == list_1[list_1.Count - 1] || list_focus[list_focus.Count - 1] == list_1[0] || list_focus[list_focus.Count - 1] == list_1[0])
                        {
                            if (list_1[0] == list_start[list_start.Count - 1] || list_1[list_1.Count - 1] == list_start[list_start.Count - 1])
                            {
                                System.Windows.Forms.MessageBox.Show("不能选择闭合的线");
                                BMECApi.Instance.StartDefaultCommand();
                                CleraPublic();
                                return;
                            }
                            list_keep = list_focus;
                            list_focus = list_1;
                            break;
                        }

                    }
                }

            }

            //高亮显示
            if (outLineList.Count > 0)
            {
                if (xiabiao_elem.Count != 1)
                {
                    List<Element> elementList = new List<Element>();
                    System.Windows.Forms.MessageBox.Show("存在没有交点的线或者存在交叉的线，不生成智能线");
                    BMECApi.Instance.StartDefaultCommand();
                    SelectionSetManager.EmptyAll();
                    foreach (KeyValuePair<int, List<DPoint3d>> kv in outLineList)
                    {
                        int indexOut = elem_index[kv.Value];
                        Element outLine = index_elem[indexOut];
                        elementList.Add(outLine);
                        //SelectionSetManager.AddElement(outLine, Session.Instance.GetActiveDgnModelRef());
                    }
                    isolateElementFrom from = new isolateElementFrom(elementList);
#if DEBUG
#else
                    from.AttachAsTopLevelForm(MyAddin.s_addin, false);
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(isolateElementFrom));
                    from.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            
#endif
                    from.Show();
                    CleraPublic();
                    return;
                }
            }

            CurveVector new_curvevector = CurveVector.Create(CurveVector.BoundaryType.Open);
            List<DPoint3d> points_list_paixu = new List<DPoint3d>();
            points_list_paixu = paixu_zhinengxian(first_point);
            List<DPoint3d> new_points_list = new List<DPoint3d>();

            //将排序好的集合生成线
            for (int i = 0; i < points_list_paixu.Count - 1; i++)
            {

                points_list_paixu[i] = points_list_paixu[i];
                DPoint3d temp_point = points_list_paixu[i + 1];
                if (i == 0)
                {
                    new_points_list.Add(points_list_paixu[i]);
                    new_points_list.Add(temp_point);
                }
                else
                {
                    DVector3d new_vector1 = new DVector3d(new_points_list[new_points_list.Count - 2], new_points_list[new_points_list.Count - 1]);
                    DVector3d new_vector2 = new DVector3d(points_list_paixu[i], temp_point);

                    Angle angle = new_vector1.AngleTo(new_vector2);
                    //特殊形体能满足条件
                    if ((angle.Degrees - angle_tolerant < 0.001) || ((angle.Degrees - (180 - angle_tolerant) > 0.001) && (180 - angle.Degrees) < 0.001))
                    {
                        new_points_list[new_points_list.Count - 1] = temp_point;
                    }
                    else
                    {
                        new_points_list.Add(temp_point);
                    }
                }
            }
            if (new_points_list.Count == 2)
            {

                DSegment3d dm = new DSegment3d();
                dm.StartPoint = new_points_list[0];
                dm.EndPoint = new_points_list[1];
                new_curvevector.Add(CurvePrimitive.CreateLine(dm));
            }
            else
            {
                new_curvevector.Add(CurvePrimitive.CreateLineString(new_points_list));
            }

            Element elem_chain = DraftingElementSchema.ToElement(Session.Instance.GetActiveDgnModel(), new_curvevector, index_elem[0]);
            elem_chain.AddToModel();
            System.Windows.Forms.DialogResult dialogresult = System.Windows.Forms.MessageBox.Show("生成智能线成功！是否删除原智能线？", "消息", System.Windows.Forms.MessageBoxButtons.YesNo);
            if (dialogresult == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (Element elem in index_elem)
                {
                    elem.DeleteFromModel();
                }
            }
            CleraPublic();

        }

        /// <summary>
        /// 清空所有的全局变量
        /// </summary>
        private void CleraPublic()
        {
            xiabiao_elem.Clear();
            list_paixu.Clear();
            index_elem.Clear();
            outLineList.Clear();
            elem_index.Clear();
            xiangshu_fangda1.Clear();
            bihe_points.Clear();
            index = 0;
            first_point = 0;
            xiangshu_index = 0;
            outLinePoint = 0;
            bihe_index = 0;

        }

        /// <summary>
        /// 排序选择的智能线
        /// </summary>
        /// <param name="first_dian"></param>
        /// <returns></returns>            
        public List<DPoint3d> paixu_zhinengxian(int first_dian)
        {

            bool chong_fu_point = true;
            List<DPoint3d> first_line = xiabiao_elem[first_dian];
            if (xiabiao_elem.Count == 1)
            {
                for (int i = 0; i < first_line.Count; i++)
                {
                    list_paixu.Add(first_line[i]);
                }

            }
            for (int s = 0; s < xiabiao_elem.Count; s++)
            {
                //不跟它本身作比较
                if (s != first_dian)
                {
                    List<DPoint3d> next_lines = xiabiao_elem[s];
                    if (first_dian == first_point)
                    {
                        if (next_lines[0] == first_line[0] || next_lines[next_lines.Count - 1] == first_line[0])
                        {
                            for (int i = first_line.Count - 1; i > -1; i--)
                            {
                                list_paixu.Add(first_line[i]);
                            }
                            paixu_zhinengxian(s);
                        }
                        else if (first_line[first_line.Count - 1] == next_lines[0] || first_line[first_line.Count - 1] == next_lines[next_lines.Count - 1])
                        {
                            for (int i = 0; i < first_line.Count; i++)
                            {
                                list_paixu.Add(first_line[i]);
                            }
                            paixu_zhinengxian(s);
                        }
                    }
                    else
                    {
                        if (first_line[0] == list_paixu[list_paixu.Count - 1])
                        {
                            if (chong_fu_point)
                            {
                                for (int i = 1; i < first_line.Count; i++)
                                {
                                    list_paixu.Add(first_line[i]);
                                }
                                chong_fu_point = false;
                            }
                            if (list_paixu[list_paixu.Count - 1] == next_lines[0] || list_paixu[list_paixu.Count - 1] == next_lines[next_lines.Count - 1])
                            {
                                paixu_zhinengxian(s);
                            }
                        }
                        else if (first_line[first_line.Count - 1] == list_paixu[list_paixu.Count - 1])
                        {
                            if (chong_fu_point)
                            {
                                for (int i = first_line.Count - 2; i > -1; i--)
                                {
                                    list_paixu.Add(first_line[i]);
                                }
                                chong_fu_point = false;
                            }
                            if (list_paixu[list_paixu.Count - 1] == next_lines[0] || list_paixu[list_paixu.Count - 1] == next_lines[next_lines.Count - 1])
                            {
                                paixu_zhinengxian(s);
                            }
                        }
                    }
                }
            }
            return list_paixu;
        }
    }
}



