using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using BIM = Bentley.Interop.MicroStationDGN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.ECObjects.Instance;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class DrawingPipeLineTool : DgnElementSetTool
    {
        public int i = 0, k = 0;
        public bool j = true;
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public DPoint3d dpFirst = DPoint3d.Zero;
        public DPoint3d dpSecond = new DPoint3d();
        public DPoint3d dpThird = new DPoint3d();
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        string myString = "";
        static BMECApi api = BMECApi.Instance;


        static double UorPerMas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        public DrawingPipeLineTool() : base(0, 0)
        {

        }
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            //base.OnDynamicFrame(ev);
            //DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);

            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);


            LineElement line = null, line1 = null;
            Element ele = null;
            double AngleValue = 0;
            //动态显示最初状态
            if (k == 0)
            {
                dpSecond = ev.Point + 2000 * unitDv1 + 4000 * unitDv2;
                dpThird = dpSecond + 8000 * unitDv1;

                DSegment3d ds = new DSegment3d(ev.Point, dpSecond);
                DSegment3d ds1 = new DSegment3d(dpSecond, dpThird);
                line = new LineElement(dgnModel, null, ds);
                line1 = new LineElement(dgnModel, null, ds1);
                ele = TextString(myString, dpSecond, dpThird);

            }
            //确定一点后的动态显示
            else if (k == 1)
            {
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, ev.Point));
                DVector3d dv3 = new DVector3d(dpFirst, ev.Point);
                AngleValue = unitDv1.DotProduct(dv3);

                //向右偏
                if (AngleValue >= 0)
                {
                    line1 = new LineElement(dgnModel, null, new DSegment3d(ev.Point, ev.Point + 5000 * unitDv1));
                    ele = TextString(myString, ev.Point, ev.Point + 5000 * unitDv1);
                }
                //向左偏
                else
                {
                    line1 = new LineElement(dgnModel, null, new DSegment3d(ev.Point, ev.Point - 5000 * unitDv1));
                    ele = TextString(myString, ev.Point, ev.Point - 5000 * unitDv1);
                }
            }
            //确定两点后的动态显示
            else if (k == 2)
            {

                DVector3d dv4 = new DVector3d(dpSecond, ev.Point);
                double AngleValue2 = unitDv1.DotProduct(dv4);
                //向右偏
                if (AngleValue >= 0)
                {
                    if (AngleValue2 >= 0)
                    {
                        dpThird = dpSecond + dv4.DotProduct(unitDv1) * unitDv1;
                    }
                    else
                    {
                        dpThird = dpSecond - dv4.DotProduct(unitDv1) * unitDv1;
                    }
                }
                else
                {
                    if (AngleValue2 < 0)
                    {
                        dpThird = dpSecond + dv4.DotProduct(unitDv1) * unitDv1;
                    }
                    else
                    {
                        dpThird = dpSecond - dv4.DotProduct(unitDv1) * unitDv1;
                    }
                }
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));
                line1 = new LineElement(dgnModel, null, new DSegment3d(dpSecond, dpSecond + dv4.DotProduct(unitDv1) * unitDv1));
                ele = TextString(myString, dpSecond, dpSecond + dv4.DotProduct(unitDv1) * unitDv1);
            }

            RedrawElems redrawElems = new RedrawElems();
            redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            redrawElems.DrawMode = DgnDrawMode.TempDraw;
            redrawElems.DrawPurpose = DrawPurpose.Dynamics;

            if ((line != null) && (line != null) && (ele != null))
            {
                redrawElems.DoRedraw(line);
                redrawElems.DoRedraw(line1);
                redrawElems.DoRedraw(ele);
            }
            else
            {
                app.ShowCommand("请重新输入命令");
                ExitTool();
            }

        }

        /// <summary>
        /// 左键点击后发生
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            try
            {
                //base.OnDataButton(ev);
                //判断是否已选中对象
                if (j)
                {
                    HitPath hit_path = DoLocate(ev, true, 0);
                    if (hit_path == null)
                    {
                        app.ShowCommand("未选择对象，请选择一个对象");
                        return true;
                    }
                    else
                    {
                        Element elem = hit_path.GetHeadElement();
                        if (elem == null) return true;

                        //BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                        IECInstance bmec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                        //if (bmec.Instance == null) return true;
                        if (bmec == null) return true;
                        //IECInstance iec1 = bmec.Instance;
                        IECInstance iec1 = bmec;
                        bool ispipe = api.InstanceDefinedAsClass(iec1, "PIPE", true);
                        if (!ispipe)
                        {
                            return true;
                        }
                        else
                        {
                            string lineNumber = iec1["LINENUMBER"].StringValue; //管线编号
                            myString = lineNumber;
                            BeginDynamics();
                            j = false;
                        }
                    }
                }
                //BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                else
                {
                    #region
                    Viewport active_view_port = Session.GetActiveViewport();
                    DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
                    DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
                    DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);

                    DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
                    DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);

                    //第一次点击
                    if (i == 0)
                    {
                        dpFirst = ev.Point;
                        i++;
                        k++;
                    }
                    //第二次点击
                    else if (i == 1)
                    {
                        double AngleValue = 0;

                        DVector3d dv3 = new DVector3d(dpFirst, ev.Point);
                        AngleValue = unitDv1.DotProduct(dv3);
                        dpSecond = ev.Point;
                        i++;
                        k++;
                    }
                    //第三次点击
                    else if (i == 2)
                    {
                        dpThird = ev.Point;
                        DVector3d dv5 = new DVector3d(dpSecond, dpThird);//点2，3向量
                        double AngleValue = unitDv1.DotProduct(new DVector3d(dpFirst, dpSecond));//点12向量与横向量的映射值
                        double AngleValue2 = unitDv1.DotProduct(dv5);//点23向量与横向量的映射值
                        List<Element> eleList = new List<Element>();
                        LineElement line1 = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));
                        Element ele = null;

                        if (AngleValue >= 0)//斜线右偏
                        {
                            if (AngleValue2 >= 0)//横线向右偏
                            {
                                //ev.Point.X = dpSecond.X;
                                dpThird = dpSecond + dv5.DotProduct(unitDv1) * unitDv1;
                            }
                            else
                            {
                                dpThird = dpSecond - dv5.DotProduct(unitDv1) * unitDv1;
                            }
                        }
                        else//斜线左偏
                        {
                            if (AngleValue2 < 0)//横线向左偏
                            {
                                dpThird = dpSecond + dv5.DotProduct(unitDv1) * unitDv1;
                            }
                            else
                            {
                                dpThird = dpSecond - dv5.DotProduct(unitDv1) * unitDv1;
                            }
                        }
                        k++;
                        LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpSecond, dpThird));
                        //line.AddToModel();
                        eleList.Add(line);
                        eleList.Add(line1);
                        ele = TextString(myString, dpSecond, dpThird);
                        eleList.Add(ele);
                        CellHeaderElement cell = new CellHeaderElement(dgnModel, "cell", dpFirst, DMatrix3d.Identity, eleList);
                        cell.AddToModel();

                        //ExitTool();

                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }


            return true;
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            if (i == 2)
            {
                i--;
                k--;
            }
            else if (i == 1)
            {
                i--;
                k--;

            }
            else if (i == 0)
            {
                if (!j)
                {
                    EndDynamics();
                    j = true;
                    return true;
                }
                else
                {
                    ExitTool();
                }
            }

            return true;
        }

        protected override void OnRestartTool()
        {
            //throw new NotImplementedException();
        }

        protected override void OnReinitialize()
        {

        }

        protected override void OnPostInstall()
        {
            //AccuSnap.SnapEnabled = true;
            //base.OnPostInstall();
            dgnModel = Session.Instance.GetActiveDgnModel();
            app.ShowCommand("请选择管道");
            //BeginDynamics();

        }


        public Element TextString(string myTest, DPoint3d apoint, DPoint3d bpoint)
        {
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);

            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);

            double wordwidth = 1 * UorPerMas, heigh = 1 * UorPerMas;
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            TextBlockProperties txtBlockProp = new TextBlockProperties(dgnModel);
            txtBlockProp.IsViewIndependent = true;
            ParagraphProperties paraProp = new ParagraphProperties(dgnModel);
            DgnTextStyle txtStyle = DgnTextStyle.GetSettings(dgnFile);
            txtStyle.SetProperty(TextStyleProperty.Width, wordwidth);
            txtStyle.SetProperty(TextStyleProperty.Height, heigh);
            RunProperties runProp = new RunProperties(txtStyle, dgnModel);
            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, dgnModel);
            txtBlock.AppendText(myTest);
            TextHandlerBase txtHandlerBase = TextHandlerBase.CreateElement(null, txtBlock);
            DRange3d dr;
            txtHandlerBase.CalcElementRange(out dr);
            double txtwidth = dr.High.X - dr.Low.X;
            DTransform3d trans = DTransform3d.Identity;
            trans.Translation = apoint + (0.5 * new DVector3d(apoint, bpoint)) - (0.5 * txtwidth * unitDv1) + (unitDv2.DotProduct(new DVector3d(dr.Low, dr.High)) * unitDv2);  //UOR unit
            TransformInfo transInfo = new TransformInfo(trans);
            txtHandlerBase.ApplyTransform(transInfo);
            return txtHandlerBase;
        }

        public override StatusInt OnElementModify(Element element)
        {
            throw new NotImplementedException();
        }
    }
}

