using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.OpenPlant.Modeler.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /*
     *             //BIM.ElementEnumerator element = app.ActiveModelReference.GetSelectedElements();
            //BIM.Element[] elementList = element.BuildArrayFromContents();
            //BIM.Element el = elementList[0];
            //bool b = app.ActiveModelReference.GetSelectedElements().MoveNext();
            //BIM.Element el= app.ActiveModelReference.GetSelectedElements().Current;
            //GetSelectedElements





            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素


           // Element elem = hit_path.GetHeadElement();

            //BMECObject element_agenda = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count != 1)
            {
                //app.ShowCommand("请选中一个元素");
                System.Windows.Forms.MessageBox.Show("请选中一个元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }

            Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(0);

            //BIM.Element el = app.ActiveModelReference.GetElementByID64(elem.ElementId);

            //IECInstance iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(el);

            //获取ecInstence ce方法
            IECInstance iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);

            
            //判断是否是设备 以及他的子类
            bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true);


            //判断是否为 阀门以及他的子类
            bool isValve = api.InstanceDefinedAsClass(iec1, "FLUID_REGULATOR", true);


            //判断是否为支吊架  
            bool isSupport = api.InstanceDefinedAsClass(iec1, "SUPPORT", true);


            if (!(eqb || isValve || isSupport))
            {
                app.ShowCommand("选取无效目标，请重新选择");
                //System.Windows.Forms.MessageBox.Show("选取无效目标，请重新选择");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }
           

            string name = iec1.ClassDefinition.Name; //ecClass的Name字段  (可能不用)


            string name1 = iec1["DESCRIPTION"].StringValue; //短描述



            //bool isPipe = api.InstanceDefinedAsClass(iec1, "PIPE", true); //判断是否为管道 以及他的子类

            //string lineNumber = iec1["LINENUMBER"].StringValue; //管线编号

            //////////////////////////
            /////////////////////////
            ///进工具类实现放置
            /////////////////////////
            ///
     */


    public class DrawingVaSuEqTool : DgnElementSetTool
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        static BMECApi api = BMECApi.Instance;
        public int i = 0;
        public DPoint3d dpFirst = DPoint3d.Zero;
        public DPoint3d dpSecond;
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

        string myname;

        IECInstance iec1 = null;

        public override StatusInt OnElementModify(Element element)
        {
            throw new NotImplementedException();
        }

        //
        public DrawingVaSuEqTool(string name) : base(0, 0)
        {
            myname = name;
        }

        //文本
        public Element TextString(Boolean b, DPoint3d dp2, DPoint3d dp3, string name)
        {
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);

            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);

            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();


            TextBlockProperties txtBlockProp = new TextBlockProperties(dgnModel);
            txtBlockProp.IsViewIndependent = true;
            ParagraphProperties paraProp = new ParagraphProperties(dgnModel);
            DgnTextStyle txtStyle = DgnTextStyle.GetSettings(dgnFile);

            //double width = (Math.Max(dp2.X, dp3.X) - Math.Min(dp2.X, dp3.X));
            //double width = Math.Abs(new DVector3d(dp2, dp3).DotProduct(unitDv1));

            txtStyle.SetProperty(TextStyleProperty.Width, 50000 * unitDv1.DotProduct(unitDv1));
            txtStyle.SetProperty(TextStyleProperty.Height, 50000 * unitDv1.DotProduct(unitDv1));


            RunProperties runProp = new RunProperties(txtStyle, dgnModel);
            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, dgnModel);
            txtBlock.AppendText(myname);

            TextHandlerBase txtHandlerBase = TextHandlerBase.CreateElement(null, txtBlock);

            DRange3d dr;
            txtHandlerBase.CalcElementRange(out dr);

            double width = dr.High.X - dr.Low.X;
            //width = new DVector3d(dr.High, dr.Low).DotProduct(unitDv1);

            DTransform3d trans = DTransform3d.Identity;

            //trans.Translation =new DVector3d(dpFirst , dpFirst + new DVector3d(dpFirst, dp2).DotProduct(unitDv1) * unitDv1 + new DVector3d(dpFirst, dp2).DotProduct(unitDv2) * unitDv2 + new DVector3d(dp2, dp3).DotProduct(unitDv1) * unitDv1  - width / 2 * unitDv1) ;  //UOR unit
            trans.Translation = new DPoint3d(dp2 + new DVector3d(dp2, dp3).DotProduct(unitDv1) / 2 * unitDv1 - width / 2 * unitDv1 + 75000 * unitDv2);
            TransformInfo transInfo = new TransformInfo(trans);
            txtHandlerBase.ApplyTransform(transInfo);
            //if (b)
            //{
            //    RedrawElems redrawElems = new RedrawElems();
            //    redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            //    redrawElems.DrawMode = DgnDrawMode.TempDraw;
            //    redrawElems.DrawPurpose = DrawPurpose.Dynamics;
            //    redrawElems.DoRedraw(txtHandlerBase);
            //}
            //else
            //{
            //    //txtHandlerBase.AddToModel();
            //}
            return txtHandlerBase;


        }

        /// <summary>
        /// 左键点击后发生
        /// </summary>
        /// <param name="ev"></param>
        /// <returns></returns>
        protected override bool OnDataButton(DgnButtonEvent ev)
        {


            if (myname == "")
            {
                HitPath hit_path = DoLocate(ev, true, 0);

                if (hit_path == null)
                {
                    return true;
                }

                //ElementAgenda element_agenda = new ElementAgenda();
                //SelectionSetManager.BuildAgenda(ref element_agenda);
                Element elem = hit_path.GetHeadElement();
                //Bentley.DgnPlatformNET.Elements.Element elem = element_agenda.GetEntry(0);
                //获取ecInstence ce方法
                iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                //BMECObject ec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
                if (iec1== null)
                {
                    app.ShowCommand("选取无效目标，请重新选择");
                    return true;
                }
                //iec1 = ec.Instance;
            }

            //判断是否是设备 以及他的子类
            bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true);


            //判断是否为 阀门以及他的子类
            bool isValve = api.InstanceDefinedAsClass(iec1, "FLUID_REGULATOR", true);


            //判断是否为支吊架  
            bool isSupport = api.InstanceDefinedAsClass(iec1, "SUPPORT", true);


            if (!(eqb || isValve || isSupport))
            {
                app.ShowCommand("选取无效目标，请重新选择");
                //System.Windows.Forms.MessageBox.Show("选取无效目标，请重新选择");
                //BMECApi.Instance.StartDefaultCommand();
                return true;
            }
            else
            {
                myname = iec1.ClassDefinition.Name; //ecClass的Name字段  (可能不用)

                string name1 = iec1["DESCRIPTION"].StringValue; //短描述

                BeginDynamics();

                Viewport active_view_port = Session.GetActiveViewport();
                DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
                DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
                DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
                DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
                DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);


                //第一次点击，选取目标
                if (i == 0)
                {
                    i++;
                    app.ShowCommand("请选取第一个放置点");
                }

                //第二次点击，确定dp1
                else if (i == 1)
                {
                    dpFirst = ev.Point;
                    i++;
                    app.ShowCommand("请选取第二个放置点");
                }
                //第三次,确定dp2
                else if (i == 2)
                {

                    //LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, ev.Point));
                    //line.AddToModel();
                    dpSecond = ev.Point;
                    i++;
                    app.ShowCommand("请选取第三个放置点");
                    //ExitTool();
                }
                //第四次,完成放置
                else if (i == 3)
                {
                    List<Element> eleList = new List<Element>();
                    LineElement line1 = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));
                    Element ele = null;


                    if (new DVector3d(dpFirst, dpSecond).DotProduct(unitDv1) < 0)

                    {

                        if (new DVector3d(dpSecond, dpSecond + new DVector3d(dpSecond, ev.Point).DotProduct(-unitDv1) * (-unitDv1)).DotProduct(-unitDv1) < 500000)
                        {

                            LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpSecond, new DPoint3d(dpSecond - 500000 * unitDv1)));
                            eleList.Add(line);

                            ele = TextString(false, dpSecond, new DPoint3d(dpSecond - 500000 * unitDv1), myname);
                        }
                        else
                        {

                            LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpSecond, new DPoint3d(dpSecond + (new DVector3d(dpSecond, ev.Point).DotProduct(-unitDv1)) * (-unitDv1))));
                            eleList.Add(line);

                            ele = TextString(false, dpSecond, new DPoint3d(dpSecond + (new DVector3d(dpSecond, ev.Point).DotProduct(-unitDv1)) * (-unitDv1)), myname);
                        }


                    }
                    else
                    {

                        if (new DVector3d(dpSecond, dpSecond + new DVector3d(dpSecond, ev.Point).DotProduct(unitDv1) * unitDv1).DotProduct(unitDv1) < 500000)

                        {
                            LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpSecond, new DPoint3d(dpSecond + 500000 * unitDv1)));
                            eleList.Add(line);
                            ele = TextString(false, dpSecond, new DPoint3d(dpSecond + 500000 * unitDv1), myname);
                        }
                        else
                        {
                            LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpSecond, new DPoint3d(dpSecond + new DVector3d(dpSecond, ev.Point).DotProduct(unitDv1) * unitDv1)));
                            eleList.Add(line);
                            ele = TextString(false, dpSecond, new DPoint3d(dpSecond + new DVector3d(dpSecond, ev.Point).DotProduct(unitDv1) * unitDv1), myname);
                        }

                    }
                    eleList.Add(line1);
                    eleList.Add(ele);
                    CellHeaderElement cell = new CellHeaderElement(dgnModel, "cell", dpFirst, DMatrix3d.Identity, eleList);

                    cell.AddToModel();




                    ExitTool();
                }


            }


            return true;
        }

        //右键返回事件
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            if (i == 3)
            {
                i = 2;
                app.ShowCommand("请选取第二个放置点");
            }
            else if (i == 2)
            {
                i = 1;
                app.ShowCommand("请选取第一个放置点");
            }
            else if (i == 1)
            {
                EndDynamics();
                myname = "";
                i = 0;
                app.ShowCommand("请选中一个元素(设备、阀门、支吊架)");
            }
            else
            {
                ExitTool();
            }

            return true;
        }


        //
        protected override void OnRestartTool()
        {
            //throw new NotImplementedException();
        }


        //
        protected override void OnReinitialize()
        {

        }


        //命令启动
        protected override void OnPostInstall()
        {


            //base.OnPostInstall();
            app.ShowPrompt("请选中一个元素(设备、阀门、支吊架)");
            //AccuSnap.SnapEnabled = true;
            //base.OnPostInstall();

        }

        //动态显示
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {

            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);

            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);
            //base.OnDynamicFrame(ev);
            //DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
            LineElement line = null, line1 = null;
            //TextBlock text = 
            Element ele = null;

            if (i == 1)
            {
                //DPoint3d dp1 = new DPoint3d((ev.Point.X ) + 1000 * UorPerMas  / 2, ev.Point.Y + 1000 * UorPerMas  / 2, ev.Point.Z);
                DPoint3d dp1 = ev.Point + 500000 * unitDv1 + 500000 * unitDv2;
                DSegment3d ds = new DSegment3d(ev.Point, dp1);
                //DPoint3d dp2 = new DPoint3d(ev.Point.X + 1000 * UorPerMas , ev.Point.Y + 1000 * UorPerMas  / 2, ev.Point.Z);
                DPoint3d dp2 = ev.Point + 1000000 * unitDv1 + 500000 * unitDv2;
                DSegment3d ds1 = new DSegment3d(dp1, dp2);

                line = new LineElement(dgnModel, null, ds);
                line1 = new LineElement(dgnModel, null, ds1);

                ele = TextString(true, dp1, dp2, myname);
            }
            else if (i == 2)
            {
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, ev.Point));
                //if (dpFirst.X > ev.Point.X)
                if (new DVector3d(dpFirst, ev.Point).DotProduct(unitDv1) < 0)
                {

                    //line1 = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X - 1000 * UorPerMas, ev.Point.Y, ev.Point.Z), ev.Point));
                    line1 = new LineElement(dgnModel, null, new DSegment3d(ev.Point, ev.Point - 500000 * unitDv1));
                    //ele = TextString(true, ev.Point, new DPoint3d(ev.Point.X - 1000 * UorPerMas, ev.Point.Y, ev.Point.Z));
                    ele = TextString(true, ev.Point, new DPoint3d(ev.Point - 500000 * unitDv1), myname);
                }
                else
                {
                    //line1 = new LineElement(dgnModel, null, new DSegment3d(ev.Point, new DPoint3d(ev.Point.X + 1000 * UorPerMas , ev.Point.Y, ev.Point.Z)));
                    //ele = TextString(true, ev.Point, new DPoint3d(ev.Point.X + 1000 * UorPerMas , ev.Point.Y, ev.Point.Z));
                    line1 = new LineElement(dgnModel, null, new DSegment3d(ev.Point, (ev.Point + 500000 * unitDv1)));
                    ele = TextString(true, ev.Point, new DPoint3d(ev.Point + 500000 * unitDv1), myname);
                }

            }
            else if (i == 3)
            {
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));
                line1 = new LineElement(dgnModel, null, new DSegment3d(dpSecond, new DPoint3d(dpSecond + new DVector3d(dpSecond, ev.Point).DotProduct(unitDv1) * unitDv1)));
                ele = TextString(true, dpSecond, new DPoint3d(ev.Point), myname);

            }
            RedrawElems redrawElems = new RedrawElems();
            redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            redrawElems.DrawMode = DgnDrawMode.TempDraw;
            redrawElems.DrawPurpose = DrawPurpose.Dynamics;

            redrawElems.DoRedraw(line);
            redrawElems.DoRedraw(line1);
            redrawElems.DoRedraw(ele);

        }


    }
}