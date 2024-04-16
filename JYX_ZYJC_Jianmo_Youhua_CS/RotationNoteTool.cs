using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.MstnPlatformNET;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class RotationNoteTool : DgnElementSetTool
    {
        protected static Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public static double uorPerMaster = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

        /// <summary>
        /// note关联元素（dimEle） 用于替换原图纸元素实现放置
        /// </summary>
        public Element noteEle = null;

        /// <summary>
        /// note元素 用于动态显示
        /// </summary>
        public Element copyEle = null;

        public Element noteHearderEle = null;

        /// <summary>
        /// note放置点
        /// </summary>
        public DPoint3d orginNoteDp = new DPoint3d();

        /// <summary>
        /// 初始朝向
        /// </summary>

        public DVector3d initialDv = new DVector3d(1, 0, 0);

        /// <summary>
        /// copy后保存的element会丢失id,无法替换。需要记录id重新查找
        /// </summary>
        public ElementId eleId;

        public DVector3d zDv = new DVector3d(-1, 0, 0);

        public DVector3d yDv = new DVector3d(1, 0, 0);

        public DVector3d sDv = new DVector3d(0, 1, 0);

        public DVector3d xDv = new DVector3d(0, -1, 0);

        public DPoint3d[] dimDp = new DPoint3d[2];

        /// <summary>
        /// 动态显示 反向时需要位移量
        /// </summary>
        public double noteMove = 0;

        public static void InstallNewTool()
        {
            RotationNoteTool rotationNoteTool = new RotationNoteTool();
            rotationNoteTool.InstallTool();
        }

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            InstallNewTool();
        }

        protected override void OnPostInstall()
        {
            base.OnPostInstall();

            ElementAgenda element_agenda = new ElementAgenda();
            SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

            uint selected_elem_count = element_agenda.GetCount();

            if (selected_elem_count != 1)
            {
                System.Windows.Forms.MessageBox.Show("请选择一个note元素");
                BMECApi.Instance.StartDefaultCommand();
                return;
            }

            Element dim = null;

            dim = element_agenda.GetEntry(0);
            noteHearderEle = dim;

            List<Element> eleList = dim.GetDependants();
            eleId = dim.ElementId;
            //return;
            if (eleList.Count <= 0) return;
            if (!eleList[0].IsInvisible) noteEle = eleList[0];
            else noteEle = eleList[1];

            DimensionElement dimEle = noteEle as DimensionElement;

            DPoint3d dp1, dp2;
            dimEle.ExtractPoint(out dp1, 0);
            dimEle.ExtractPoint(out dp2, 1);

            dimDp[0] = dp1;
            dimDp[1] = dp2;

            #region 获取初始朝向
            NoteCellHeaderElement noteCellEle = dim as NoteCellHeaderElement;

            DPoint3d orginDp;
            noteCellEle.GetTransformOrigin(out orginDp);
            orginNoteDp = orginDp;

            TextPartIdCollection idLIst = noteCellEle.GetTextPartIds(new TextQueryOptions());

            try
            {
                TextBlock tt = noteCellEle.GetTextPart(idLIst[0]);

                DPoint3d textDp = tt.GetUserOrigin();

                DVector3d textDv = new DVector3d(1, 0, 0);

                DMatrix3d textDm = tt.GetOrientation();

                textDv = textDm * textDv;

                DPoint3d textDp2 = textDp + textDv;

                if (orginDp.Distance(textDp) > orginDp.Distance(textDp2))
                {
                    initialDv = new DVector3d(-1, 0, 0);
                }

            }
            catch
            {
                initialDv = new DVector3d(1, 0, 0);
            }
            #endregion

            #region 复制元素用于动态显示
            DgnModel dgnMoel = Session.Instance.GetActiveDgnModel();

            using (ElementCopyContext eleCopyCon = new ElementCopyContext(dgnMoel))
            {
                eleCopyCon.WriteElements = false;
                copyEle = eleCopyCon.DoCopy(dim);
            }

            NoteCellHeaderElement nn = copyEle as NoteCellHeaderElement;

            DMatrix3d ydm;
            nn.GetOrientation(out ydm);

            DVector3d dvv = ydm * initialDv;

            DMatrix3d dmm = DMatrix3d.Identity;
            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvv, initialDv, ref dmm);

            DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dmm, orginNoteDp);

            TransformInfo ta = new TransformInfo(dt);

            nn.ApplyTransform(ta);

            DRange3d dr;
            nn.CalcElementRange(out dr);

            DVector3d drDv = new DVector3d(dr.Low, dr.High);

            DMatrix3d dm;
            nn.GetOrientation(out dm);
            //DMatrix3d neDm = DMatrix3d.Negate(dm);

            DVector3d dv1 =  dm* initialDv;

            noteMove = drDv.DotProduct(dv1);
            noteMove = Math.Abs(noteMove);
            #endregion

            app.ShowCommand("Note旋转");
            app.ShowPrompt("请选择放置点");

            BeginDynamics();
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            base.OnDataButton(ev);

            DimensionElement dimEle = noteEle as DimensionElement;

            //DMatrix3d dimDm;
            //dimEle.GetOrientation(out dimDm);

            //DVector3d dv = dimDm * initialDv;

            DVector3d dvz = new DVector3d(orginNoteDp, ev.Point);
            DMatrix3d dm = DMatrix3d.Identity;
            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, dvz, ref dm);

            //dimEle.SetRotationMatrix(dm);

            //DependencyManager.ProcessAffected();更新依赖关系

            Element nn = Session.Instance.GetActiveDgnModel().FindElementById(eleId);
            //nn.ApplyTransform(ta);
            bool isyd = false;

            double angle1 = yDv.AngleTo(dvz).Radians;
            double angle2 = zDv.AngleTo(dvz).Radians;
            double angle3 = sDv.AngleTo(dvz).Radians;
            double angle4 = xDv.AngleTo(dvz).Radians;
            if (angle1 <= Math.PI / 4 && angle1 > -Math.PI / 4)
            {
                DVector3d dimDv = new DVector3d(dimDp[0], dimDp[1]);
                double angle = yDv.AngleTo(dimDv).Degrees;
                if (Math.Abs(angle) < 90)
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, yDv, ref dm);

                    dimEle.SetRotationMatrix(dm);
                }
                else if (Math.Abs(angle) == 90)
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, zDv, ref dm);

                    dimEle.SetRotationMatrix(dm);

                    DPoint3d xDp = 1 * yDv;

                    DTransform3d dt = DTransform3d.FromTranslation(xDp);

                    TransformInfo ta = new TransformInfo(dt);

                    nn.ApplyTransform(ta);
                    isyd = true;
                }
                else
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, zDv, ref dm);
                    dimEle.SetRotationMatrix(dm);

                    double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(sDv).Radians));

                    DPoint3d xDp = 2 * juli * yDv;

                    DTransform3d dt = DTransform3d.FromTranslation(xDp);

                    TransformInfo ta = new TransformInfo(dt);

                    nn.ApplyTransform(ta);
                    isyd = true;
                }
            }

            if (angle2 <= Math.PI / 4 && angle2 > -Math.PI / 4)
            {
                DVector3d dimDv = new DVector3d(dimDp[0], dimDp[1]);
                double angle = zDv.AngleTo(dimDv).Degrees;
                if (Math.Abs(angle) <= 90)
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, zDv, ref dm);

                    dimEle.SetRotationMatrix(dm);
                }
                else
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, yDv, ref dm);
                    dimEle.SetRotationMatrix(dm);

                    double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(sDv).Radians));

                    DPoint3d xDp = 2 * juli * zDv;

                    DTransform3d dt = DTransform3d.FromTranslation(xDp);

                    TransformInfo ta = new TransformInfo(dt);

                    nn.ApplyTransform(ta);
                    isyd = true;
                }
            }

            if (angle3 <= Math.PI / 4 && angle3 > -Math.PI / 4)
            {
                DVector3d dimDv = new DVector3d(dimDp[0], dimDp[1]);
                double angle = sDv.AngleTo(dimDv).Degrees;
                if (Math.Abs(angle) < 90)
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, sDv, ref dm);

                    dimEle.SetRotationMatrix(dm);
                }
                else
                {
                    double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(yDv).Radians));

                    DPoint3d xDp = 2 * juli * sDv;

                    DTransform3d dt = DTransform3d.FromTranslation(xDp);

                    TransformInfo ta = new TransformInfo(dt);

                    nn.ApplyTransform(ta);

                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, sDv, ref dm);
                    dimEle.SetRotationMatrix(dm);
                    isyd = true;
                }
            }

            if (angle4 <= Math.PI / 4 && angle4 > -Math.PI / 4)
            {
                DVector3d dimDv = new DVector3d(dimDp[0], dimDp[1]);
                double angle = xDv.AngleTo(dimDv).Degrees;
                if (Math.Abs(angle) < 90)
                {
                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, xDv, ref dm);

                    dimEle.SetRotationMatrix(dm);
                }
                else
                {
                    double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(yDv).Radians));

                    DPoint3d xDp = 2 * juli * xDv;

                    DTransform3d dt = DTransform3d.FromTranslation(xDp);

                    TransformInfo ta = new TransformInfo(dt);

                    nn.ApplyTransform(ta);

                    JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(initialDv, xDv, ref dm);
                    dimEle.SetRotationMatrix(dm);
                    isyd = true;
                }
            }

            try
            {
                dimEle.ReplaceInModel(dimEle);
                if(isyd) nn.ReplaceInModel(nn);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                ExitTool();
            }

            ExitTool();
            return true;
        }

        List<DPoint3d> dyDp = new List<DPoint3d>();
        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            if (noteEle != null)
            {
                Element nn = Session.Instance.GetActiveDgnModel().FindElementById(eleId);

                DgnModel dgnMoel = Session.Instance.GetActiveDgnModel();

                using (ElementCopyContext eleCopyCon = new ElementCopyContext(dgnMoel))
                {
                    eleCopyCon.WriteElements = false;
                    copyEle = eleCopyCon.DoCopy(nn);
                }

                if (dyDp.Count==0)
                {
                    dyDp.Add(dimDp[0]);
                    dyDp.Add(dimDp[1]);
                }

                DVector3d dvqs = initialDv;

                NoteCellHeaderElement note = copyEle as NoteCellHeaderElement;

                DMatrix3d ydm;
                note.GetOrientation(out ydm);

                DPoint3d dp;
                note.GetTransformOrigin(out dp);
                dp = dyDp[1];

                DVector3d dv1 = ydm * dvqs;

                DVector3d dvz = new DVector3d(orginNoteDp, ev.Point);

                double angle1 = yDv.AngleTo(dvz).Radians;
                double angle2 = zDv.AngleTo(dvz).Radians;
                double angle3 = sDv.AngleTo(dvz).Radians;
                double angle4 = xDv.AngleTo(dvz).Radians;
                if (angle1 <= Math.PI / 4 && angle1 > -Math.PI / 4)
                {
                    DVector3d dimDv = new DVector3d(dyDp[0], dyDp[1]);
                    double angle = yDv.AngleTo(dimDv).Degrees;
                    if (Math.Abs(angle) < 90)
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, yDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(note);
                    }
                    else if (Math.Abs(angle) == 90)
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, zDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        DVector3d move = noteMove * yDv;
                        DTransform3d dt1 = DTransform3d.FromTranslation(move);
                        TransformInfo ta1 = new TransformInfo(dt1);
                        note.ApplyTransform(ta1);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(note);
                    }
                    else
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, zDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(sDv).Radians));

                        DPoint3d xDp = orginNoteDp + 2 * juli * yDv;
                        DSegment3d ds = new DSegment3d(dyDp[0], xDp);

                        //dyDp[1] = xDp;

                        LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);

                        DVector3d move = (juli + noteMove) * yDv;
                        DTransform3d dt1 = DTransform3d.FromTranslation(move);
                        TransformInfo ta1 = new TransformInfo(dt1);
                        note.ApplyTransform(ta1);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(line);
                        redrawElems.DoRedraw(note);
                    }
                }
                if (angle2 <= Math.PI / 4 && angle2 > -Math.PI / 4)
                {
                    DVector3d dimDv = new DVector3d(dyDp[0], dyDp[1]);
                    double angle = zDv.AngleTo(dimDv).Degrees;
                    if (Math.Abs(angle) <= 90)
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, zDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(note);
                    }
                    else
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, yDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(sDv).Radians));

                        DPoint3d xDp = orginNoteDp + 2 * juli * zDv;
                        DSegment3d ds = new DSegment3d(dyDp[0], xDp);

                        //dyDp[1] = xDp;

                        LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);

                        DVector3d move = (juli + noteMove) * zDv;
                        DTransform3d dt1 = DTransform3d.FromTranslation(move);
                        TransformInfo ta1 = new TransformInfo(dt1);
                        note.ApplyTransform(ta1);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(line);
                        redrawElems.DoRedraw(note);
                    }
                }
                if (angle3 <= Math.PI / 4 && angle3 > -Math.PI / 4)
                {
                    DVector3d dimDv = new DVector3d(dyDp[0], dyDp[1]);
                    double angle = sDv.AngleTo(dimDv).Degrees;
                    if (Math.Abs(angle) <= 90)
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, sDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(note);
                    }
                    else
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, sDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(yDv).Radians));

                        DPoint3d xDp = orginNoteDp + 2 * juli * sDv;
                        DSegment3d ds = new DSegment3d(dyDp[0], xDp);

                        //dyDp[1] = xDp;

                        LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);

                        DVector3d move = 2 * juli * sDv;
                        DTransform3d dt1 = DTransform3d.FromTranslation(move);
                        TransformInfo ta1 = new TransformInfo(dt1);
                        note.ApplyTransform(ta1);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(line);
                        redrawElems.DoRedraw(note);
                    }
                }
                if (angle4 <= Math.PI / 4 && angle4 > -Math.PI / 4)
                {
                    DVector3d dimDv = new DVector3d(dyDp[0], dyDp[1]);
                    double angle = xDv.AngleTo(dimDv).Degrees;
                    if (Math.Abs(angle) <= 90)
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, xDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(note);
                    }
                    else
                    {
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dv1, xDv, ref dm);

                        DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp);

                        TransformInfo ta = new TransformInfo(dt);

                        note.ApplyTransform(ta);

                        double juli = Math.Abs(dimDv.Magnitude * Math.Sin(dimDv.AngleTo(yDv).Radians));

                        DPoint3d xDp = orginNoteDp + 2 * juli * xDv;
                        DSegment3d ds = new DSegment3d(dyDp[0], xDp);

                        //dyDp[1] = xDp;

                        LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, ds);

                        DVector3d move = 2 * juli * xDv;
                        DTransform3d dt1 = DTransform3d.FromTranslation(move);
                        TransformInfo ta1 = new TransformInfo(dt1);
                        note.ApplyTransform(ta1);

                        RedrawElems redrawElems = new RedrawElems();
                        redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
                        redrawElems.DrawMode = DgnDrawMode.TempDraw;
                        redrawElems.DrawPurpose = DrawPurpose.Dynamics;
                        redrawElems.DoRedraw(line);
                        redrawElems.DoRedraw(note);
                    }
                }
            }
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            ExitTool();
            return true;
        }
    }
}
