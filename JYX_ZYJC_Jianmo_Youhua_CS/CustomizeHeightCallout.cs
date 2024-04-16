using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET;
using Bentley.GeometryNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class CustomizeHeightCallout : DgnElementSetTool
    {
        /// <summary>
        /// 用户输入的高度
        /// </summary>
        public double dz = 0;

        static BIM.Application app = Utilities.ComApp;  //V8I

        public DPoint3d firstDp = new DPoint3d();

        public DPoint3d secondDp = new DPoint3d();

        public string type = "";

        DRange3d dr = new DRange3d();

        /// <summary>
        /// 点击次数
        /// </summary>
        public int n = 0;

        public CustomizeHeightCallout(double z, string calloutType)
        {
            dz = z;
            type = calloutType;
        }

        public override StatusInt OnElementModify(Element element)
        {
            //throw new NotImplementedException();
            return StatusInt.Success;
        }

        protected override void OnRestartTool()
        {
            //throw new NotImplementedException();
        }


        protected override void OnPostInstall()
        {
            base.OnPostInstall();

            Session.Instance.GetActiveDgnModel().GetRange(out dr);//得到包围所有元素的最小range

            #region 将当前视图设置为前视图
            int viewNumber = Session.GetActiveViewport().ViewNumber; //CE view[0-7]

            BIM.View vw = app.ActiveDesignFile.Views[viewNumber + 1];//v8i view[1-8]

            vw.SetToFront();
            vw.Fit(true);
            vw.Redraw();
            #endregion

            BeginDynamics();

            app.ShowCommand("切图");
            app.ShowPrompt("选择callout起点");
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            base.OnDataButton(ev);
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            if (n == 0)
            {
                n++;
                firstDp = new DPoint3d(ev.Point.X, dr.Low.Y, dz);
                app.ShowCommand("切图");
                app.ShowPrompt("选择callout终点");
            }
            else if (n == 1)
            {
                //n++;
                secondDp = new DPoint3d(ev.Point.X, dr.Low.Y, dz);
                if (firstDp.Distance(secondDp) > 0.1)
                {
                    n++;
                    app.ShowCommand("切图");
                    app.ShowPrompt("选择callout方向");
                }
            }
            else if (n == 2)
            {
                if (Math.Abs(ev.Point.Z - dz) > 0.1)
                {
                    try
                    {
                        //所需参数：callout起点终点、切面朝向、范围
                        //DisplayableElement dis = new DisplayableElement();
                        double width = Math.Abs(dr.High.Y - dr.Low.Y);
                        double height = Math.Abs(ev.Point.Z - dz);
                        DPoint3d zd = new DPoint3d(0, 0, ev.Point.Z - dz);
                        if (type.Equals("Plan Callout"))
                        {
                            JYX_ZYJC_CLR.PublicMethod.createPlanCallout(firstDp, secondDp, zd, width, firstDp.Distance(secondDp), height);
                        }
                        else if (type.Equals("Section Callout"))
                        {
                            JYX_ZYJC_CLR.PublicMethod.createSectionCallout(firstDp, secondDp, zd, width, firstDp.Distance(secondDp), height);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }

                    ExitTool();
                }
            }
            return false;
        }

        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DPoint3d dp1 = new DPoint3d(viewBox[0].X, dr.Low.Y, dz);
            DPoint3d dp2 = new DPoint3d(viewBox[1].X, dr.Low.Y, dz);
            DSegment3d ds = new DSegment3d(dp1, dp2);
            LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);
            ElementPropertiesSetter elem_pro_setter = new ElementPropertiesSetter();
            elem_pro_setter.SetLinestyle(3, null);
            elem_pro_setter.Apply(line);

            RedrawElems redraw_elems = new RedrawElems();
            redraw_elems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            redraw_elems.DrawMode = DgnDrawMode.TempDraw;
            redraw_elems.DrawPurpose = DrawPurpose.Dynamics;
            redraw_elems.DoRedraw(line);
            if (n == 1)
            {
                DPoint3d dpSecond = new DPoint3d(ev.Point.X, dr.Low.Y, dz);
                if (firstDp.Distance(dpSecond) > 0.1)
                {
                    DSegment3d ds1 = new DSegment3d(firstDp, dpSecond);
                    LineElement line1 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds1);
                    redraw_elems.DoRedraw(line1);
                }
            }
            if (n == 2)
            {
                DSegment3d ds2 = new DSegment3d(firstDp, secondDp);
                LineElement line2 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds2);
                redraw_elems.DoRedraw(line2);

                if (Math.Abs(ev.Point.Z - dz) > 0.1)
                {
                    DPoint3d dp3 = new DPoint3d(firstDp.X, firstDp.Y, ev.Point.Z);
                    DPoint3d dp4 = new DPoint3d(secondDp.X, secondDp.Y, ev.Point.Z);
                    DSegment3d ds3 = new DSegment3d(firstDp, dp3);
                    DSegment3d ds4 = new DSegment3d(dp3, dp4);
                    DSegment3d ds5 = new DSegment3d(secondDp, dp4);
                    LineElement line3 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds3);
                    LineElement line4 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds4);
                    LineElement line5 = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds5);

                    elem_pro_setter.Apply(line3);
                    elem_pro_setter.Apply(line4);
                    elem_pro_setter.Apply(line5);

                    redraw_elems.DoRedraw(line3);
                    redraw_elems.DoRedraw(line4);
                    redraw_elems.DoRedraw(line5);
                }
            }
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            if(n==2)
            {
                n--;
                app.ShowCommand("切图");
                app.ShowPrompt("选择callout终点");
            }
            else if(n==1)
            {
                n--;
                app.ShowCommand("切图");
                app.ShowPrompt("选择callout起点");
            }
            else
            {
                ExitTool();
            }
            return true;
        }
    }
}
