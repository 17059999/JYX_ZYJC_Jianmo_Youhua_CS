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
    public class DrawingzhijiaTool : DgnElementSetTool
    {
        public int i = 0, k = 0;
        public bool j = true;
        public static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public DPoint3d dpFirst = DPoint3d.Zero;
        public DPoint3d dpSecond = new DPoint3d();
        public DPoint3d dpThird = new DPoint3d();
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        public DgnModelType modelType;
        string myString = "";
        static BMECApi api = BMECApi.Instance;

        double UorPerMas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        double myModelScal = 1;
        double myScal = 1;
        public DrawingzhijiaTool() : base(0, 0)
        {
        }
        //动态显示
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

            LineElement line = null;
            //LineElement line1 = null;
            Element ele = null;
            //double AngleValue = 0;
            modelType = dgnModel.ModelType;
            myModelScal = dgnModel.GetModelInfo().AnnotationScaleFactor;
            DgnAttachmentCollection attList0 = dgnModel.GetDgnAttachments();
            foreach (var att0 in attList0)
            {
                DgnAttachmentCollection attList1 = dgnModel.GetDgnAttachments();
                foreach (var att1 in attList1)
                {
                    double scal = att1.DisplayScale;
                    myScal = scal;
                }
                //double scal = att0.DisplayScale;
                //myScal = scal;
            }
            DPoint3d orgin = new DPoint3d();
            //动态显示最初状态
            if (k == 0)
            {
                dpSecond = ev.Point + (200 * UorPerMas * unitDv1 + 400 * UorPerMas * unitDv2) * myScal;
                dpThird = dpSecond + (500 * UorPerMas * unitDv1);
                DSegment3d ds = new DSegment3d(ev.Point, dpSecond);
                line = new LineElement(dgnModel, null, ds);
                ele = textLine(myString, DPoint3d.Zero, true);

                DTransform3d dt1 = DTransform3d.Identity;

                orgin = dpSecond;

                dt1.Translation = orgin;
                TransformInfo tran1 = new TransformInfo(dt1);
                ele.ApplyTransform(tran1);
            }
            //确定一点后的动态显示
            else if (k == 1)
            {
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, ev.Point));

                DVector3d dv = new DVector3d(dpFirst, ev.Point);

                double pd = unitDv1.DotProduct(dv);

                if (pd > 0)
                {
                    ele = textLine(myString, DPoint3d.Zero, true);
                }
                else
                {
                    ele = textLine(myString, DPoint3d.Zero, false);
                }
                //ele = TextString(myString, dpFirst, ev.Point);

                orgin = ev.Point;

                DTransform3d dt1 = DTransform3d.Identity;

                //orgin = dpSecond;

                dt1.Translation = orgin;
                TransformInfo tran1 = new TransformInfo(dt1);
                ele.ApplyTransform(tran1);
            }
            else if (k == 2)
            {
                line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));

                DVector3d dv = new DVector3d(dpFirst, dpSecond);

                double pd = unitDv1.DotProduct(dv);

                DVector3d dvqs = new DVector3d(1, 0, 0);

                if (pd > 0)
                {
                    ele = textLine(myString, DPoint3d.Zero, true);
                }
                else
                {
                    ele = textLine(myString, DPoint3d.Zero, false);
                    dvqs = new DVector3d(-1, 0, 0);
                }
                //ele = TextString(myString, dpFirst, ev.Point);

                orgin = dpSecond;


                DVector3d dvz = new DVector3d(dpSecond, ev.Point);
                DMatrix3d dm = DMatrix3d.Identity;
                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvqs, dvz, ref dm);

                DTransform3d dt1 = DTransform3d.Identity;
                dt1.Matrix = dm;
                //orgin = dpSecond;

                dt1.Translation = orgin;
                TransformInfo tran1 = new TransformInfo(dt1);
                ele.ApplyTransform(tran1);
            }

            //DTransform3d dt = DTransform3d.Identity;
            //DMatrix3d dm = DMatrix3d.Identity;
            //DMatrix3d dm1 = DMatrix3d.Identity;
            //DVector3d dvt1 = new DVector3d(1, 0, 0);
            //DVector3d dvt2 = unitDv1;
            //DVector3d dvt3 = new DVector3d(0, 1, 0);
            //DVector3d dvt4 = unitDv2;
            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvt1, dvt2, ref dm);
            //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvt3, dvt4, ref dm1);
            //DMatrix3d xx = dm * dm1;
            //dt.Matrix = xx;
            //dt.Translation = orgin;
            //TransformInfo tran = new TransformInfo(dt);
            //ele.ApplyTransform(tran);

            RedrawElems redrawElems = new RedrawElems();
            redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            redrawElems.DrawMode = DgnDrawMode.TempDraw;
            redrawElems.DrawPurpose = DrawPurpose.Dynamics;

            string dimName = app.ActiveSettings.DimensionStyle.Name;
            BIM.DimensionStyle dims = app.ActiveDesignFile.DimensionStyles.Find(dimName);
            if (dims != null)
            {
                int color = dims.OverallColor;
            }

            if ((line != null) && (ele != null))
            {
                redrawElems.DoRedraw(line);
                //redrawElems.DoRedraw(line1);
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
                        //判断是否是PIPING(包含管道，阀门，逻辑支吊架，支吊架)
                        bool isPi = api.InstanceDefinedAsClass(iec1, "SUPPORT_BASE", true);
                        if (isPi)
                        {
                            string Nname = iec1["NAME"].StringValue; //PIPINFG其他的NAME
                            bool isgb = isGenbuZhijia(iec1);
                            if(isgb)
                            {
                                Nname = iec1["CERI_TYPE_NAME"].StringValue; //PIPINFG其他的NAME
                            }
                            myString = Nname;
                        }
                        else return true;
                        //string keyIn = "dimstyle active ";
                        //keyIn += "CERI_引线标注";
                        //app.CadInputQueue.SendKeyin(keyIn);
                        BeginDynamics();
                        j = false;
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
                    //Element ele = null;
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
                        dpSecond = ev.Point;
                        //ele = TextString(myString, dpFirst, dpSecond);
                        //ele = TextStringSendCommand(myString, dpFirst, dpSecond);
                        #region Note旋转

                        //DTransform3d dt = DTransform3d.Identity;
                        //DMatrix3d dm = DMatrix3d.Identity;
                        //DMatrix3d dm1 = DMatrix3d.Identity;
                        //DVector3d dvt1 = new DVector3d(1, 0, 0);
                        //DVector3d dvt2 = unitDv1;
                        //DVector3d dvt3 = new DVector3d(0, 1, 0);
                        //DVector3d dvt4 = unitDv2;
                        //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvt1, dvt2, ref dm); //横向旋转
                        //JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvt3, dvt4, ref dm1);//纵向旋转
                        //DMatrix3d xx = dm * dm1;
                        //dt.Matrix = xx;
                        //dt.Translation = dpSecond;
                        //TransformInfo tran = new TransformInfo(dt);
                        //ele.ApplyTransform(tran);
                        #endregion
                        //LineElement line = new LineElement(dgnModel, null, new DSegment3d(dpFirst, dpSecond));
                        //line.AddToModel();
                        //ele.AddToModel();
                        //OnRestartTool();
                        i++;
                        k++;
                    }
                    //第三次点击
                    else
                    {
                        //ele = TextStringSendCommand(myString, dpFirst, dpSecond);
                        DVector3d dv = new DVector3d(dpFirst, dpSecond);

                        double pd = unitDv1.DotProduct(dv);

                        double angle = Math.Abs(unitDv1.AngleTo(dv).Degrees);

                        DVector3d dvqs = new DVector3d(1, 0, 0);

                        if (pd < 0) dvqs = new DVector3d(-1, 0, 0);

                        if(angle==90) dvqs= new DVector3d(-1, 0, 0);

                        DVector3d dvz = new DVector3d(dpSecond, ev.Point);
                        DMatrix3d dm = DMatrix3d.Identity;
                        JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dvqs, dvz, ref dm);

                        NoteElement(myString, dpFirst, dpSecond, dm);

                        OnRestartTool();
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

        //鼠标右击
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            /*if (i == 2)
            {
                i--;
                k--;
            }*/
            if (i == 1)
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
            else if (i == 2)
            {
                i--;
                k--;
            }

            return true;
        }

        protected override void OnRestartTool()
        {
            DrawingzhijiaTool tool = new DrawingzhijiaTool();
            tool.InstallTool();
            //throw new NotImplementedException();
        }

        protected override void OnReinitialize()
        {

        }
        //启动命令
        protected override void OnPostInstall()
        {
            //AccuSnap.SnapEnabled = true;
            base.OnPostInstall();
            dgnModel = Session.Instance.GetActiveDgnModel();
            app.ShowCommand("请选择支架");
            //BeginDynamics();

        }

        //文本
        //public Element TextString(string myTest, DPoint3d apoint, DPoint3d bpoint, DPoint3d cpoint)
        public Element TextString(string myTest, DPoint3d apoint, DPoint3d bpoint)
        {
            Element elem;
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);
            DPoint3d[] dpt3ds = new DPoint3d[2];
            dpt3ds[0] = apoint;
            dpt3ds[1] = bpoint;
            dpt3ds[0] = new DPoint3d();
            dpt3ds[1] = new DPoint3d();
            //dpt3ds[0]=
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel model = Session.Instance.GetActiveDgnModel();
            TextBlockProperties tbProps = new TextBlockProperties(model);
            //tbProps.IsViewIndependent = true; //文字始终正对着视图   （问题非顶视图 比如前视图线会在文字中间）
            ParagraphProperties paraProps = new ParagraphProperties(model);
            string txtName = app.ActiveSettings.TextStyle.Name;
            BIM.TextStyle tt = app.ActiveDesignFile.TextStyles.Find("CERI_Standard-5.0mm");
            app.ActiveSettings.TextStyle = tt;
            string textstylename = Settings.TextStyleName;
            DgnTextStyle dtStyle = DgnTextStyle.GetByName(textstylename, Session.Instance.GetActiveDgnFile());
            if (dtStyle == null)
            {
                //BIM.DimensionStyle a = app.ActiveSettings.DimensionStyle;
                dtStyle = DgnTextStyle.GetSettings(dgnFile);

            }

            double txtwidth, txtHeight, widthDistance, heightDistance;
            dtStyle.GetProperty(TextStyleProperty.Width, out txtwidth);
            //dtStyle.SetProperty(TextStyleProperty.Width, txtwidth / myModelScal * myScal);    //画note不需要设置
            dtStyle.GetProperty(TextStyleProperty.Height, out txtHeight);
            //dtStyle.SetProperty(TextStyleProperty.Height, txtHeight / myModelScal * myScal);
            RunProperties runProps = new RunProperties(dtStyle, model);
            TextBlock TextBlock = new TextBlock(tbProps, paraProps, runProps, Session.Instance.GetActiveDgnModel());
            TextBlock.SetProperties(tbProps);
            TextBlock.AppendText(myTest);

            DimensionStyle dim_style = DimensionStyle.GetByName(Settings.DimensionStyleName, Session.Instance.GetActiveDgnFile());
            //dim_style = DimensionStyle.GetSettings(dgnFile);
            string dimName = app.ActiveSettings.DimensionStyle.Name;
            BIM.DimensionStyle dimS = app.ActiveSettings.DimensionStyle;
            string dimNam1 = Settings.DimensionStyleName;
            if (dim_style == null)
            {
                dim_style = DimensionStyle.GetSettings(dgnFile);

                dim_style.GetDistanceProp(out widthDistance, DimStyleProp.Text_Width_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.GetDistanceProp(out heightDistance, DimStyleProp.Text_Height_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.SetDistanceProp(1, DimStyleProp.Text_Height_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.SetDistanceProp(1, DimStyleProp.Text_Width_DISTANCE, Session.Instance.GetActiveDgnModel());

            }

            NoteCellHeaderElement NoteCellHeaderElement = new NoteCellHeaderElement(out elem, TextBlock, dim_style, model, dpt3ds);
            return NoteCellHeaderElement;
        }

        public double bili = 1;
        public Element textLine(string name, DPoint3d startDp, bool isF)
        {
            DgnModel dgnModel1 = Session.Instance.GetActiveDgnModel();
            DgnModelType dgnType = dgnModel1.ModelType;
            if (dgnType == DgnModelType.Drawing)
            {
                ModelInfo modelInfo = dgnModel1.GetModelInfo();
                bili = modelInfo.AnnotationScaleFactor;
            }

            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
            unitDv1 = new DVector3d(1, 0, 0);

            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);
            unitDv2 = new DVector3d(0, 1, 0);

            DVector3d dv3 = new DVector3d(viewBox[0], viewBox[4]);
            DVector3d unitDv3 = dv3 * (1 / dv3.Magnitude);
            unitDv3 = new DVector3d(0, 0, 1);

            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();


            TextBlockProperties txtBlockProp = new TextBlockProperties(dgnModel);
            txtBlockProp.IsViewIndependent = true;
            ParagraphProperties paraProp = new ParagraphProperties(dgnModel);

            BIM.TextStyle tt = app.ActiveDesignFile.TextStyles.Find("CERI_Standard-5.0mm");
            if(tt!=null)
            {
                app.ActiveSettings.TextStyle = tt;
            }
            
            string textstylename = Settings.TextStyleName;
            DgnTextStyle txtStyle = DgnTextStyle.GetByName(textstylename, Session.Instance.GetActiveDgnFile());
            if (txtStyle == null)
            {
                //BIM.DimensionStyle a = app.ActiveSettings.DimensionStyle;
                txtStyle = DgnTextStyle.GetSettings(dgnFile);

            }

            //double width = (Math.Max(dp2.X, dp3.X) - Math.Min(dp2.X, dp3.X));
            //double width = Math.Abs(new DVector3d(dp2, dp3).DotProduct(unitDv1));

            txtStyle.SetProperty(TextStyleProperty.Width, 5 * UorPerMas * unitDv1.DotProduct(unitDv1));
            txtStyle.SetProperty(TextStyleProperty.Height, 5 * UorPerMas * unitDv1.DotProduct(unitDv1));


            RunProperties runProp = new RunProperties(txtStyle, dgnModel);
            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, dgnModel);
            txtBlock.AppendText(name);

            TextHandlerBase txtHandlerBase = TextHandlerBase.CreateElement(null, txtBlock);

            DRange3d dr;
            txtHandlerBase.CalcElementRange(out dr);

            double width = dr.High.X - dr.Low.X;
            DPoint3d endDp = startDp + (width + 15 * UorPerMas * bili) * unitDv1; //要加比例
            if (!isF)
            {
                endDp = startDp - (width + 15 * UorPerMas * bili) * unitDv1;
            }
            DSegment3d ds = new DSegment3d(startDp, endDp);
            LineElement line1 = new LineElement(dgnModel, null, ds);
            //width = new DVector3d(dr.High, dr.Low).DotProduct(unitDv1);
            DPoint3d textDp = startDp + 7.5 * UorPerMas * unitDv1 * bili + 7.5 * UorPerMas * unitDv2 * bili;
            if (!isF)
            {
                textDp = endDp + 7.5 * UorPerMas * unitDv1 * bili + 7.5 * UorPerMas * unitDv2 * bili;
            }
            DTransform3d trans = DTransform3d.Identity;

            //trans.Translation =new DVector3d(dpFirst , dpFirst + new DVector3d(dpFirst, dp2).DotProduct(unitDv1) * unitDv1 + new DVector3d(dpFirst, dp2).DotProduct(unitDv2) * unitDv2 + new DVector3d(dp2, dp3).DotProduct(unitDv1) * unitDv1  - width / 2 * unitDv1) ;  //UOR unit
            trans.Translation = new DPoint3d(textDp);
            TransformInfo transInfo = new TransformInfo(trans);
            txtHandlerBase.ApplyTransform(transInfo);

            List<Element> eleList = new List<Element>();

            eleList.Add(line1);
            eleList.Add(txtHandlerBase);
            CellHeaderElement cell = new CellHeaderElement(dgnModel, "cell", startDp, DMatrix3d.Identity, eleList);

            return cell;
        }

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Success;
        }

        protected override bool IsModifyOriginal()
        {
            return false;
        }
        protected override bool NeedPointForSelection() { return false; }

        public Element TextStringSendCommand(string myTest, DPoint3d apoint, DPoint3d bpoint)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel model = Session.Instance.GetActiveDgnModel();
            //string textstylename = Settings.TextStyleName;

            //DgnTextStyle dtStyle = DgnTextStyle.GetByName(Settings.TextStyleName, Session.Instance.GetActiveDgnFile());
            //if (dtStyle == null)
            //{
            //    dtStyle = DgnTextStyle.GetSettings(dgnFile);
            //}
            Viewport active_view_port = Session.GetActiveViewport();
            int ViewNum = active_view_port.ViewNumber;
            //ViewNum = active_view_port.ScreenNumber;

            //DimensionStyle dim_style = DimensionStyle.GetByName(Settings.DimensionStyleName, Session.Instance.GetActiveDgnFile());
            //dtStyle.GetProperty(TextStyleProperty.Width, out double txtwidth);
            //dtStyle.SetProperty(TextStyleProperty.Width, txtwidth / myModelScal * myScal);
            //dtStyle.GetProperty(TextStyleProperty.Height, out double txtHeight);
            //dtStyle.SetProperty(TextStyleProperty.Height, txtHeight / myModelScal * myScal);
            //if (dim_style == null)
            //{
            //    dim_style = DimensionStyle.GetSettings(dgnFile);

            //}
            BIM.Point3d[] pts = new BIM.Point3d[2];
            pts[0] = app.Point3dFromXYZ(apoint.X * (1 / UorPerMas), apoint.Y * (1 / UorPerMas), apoint.Z * (1 / UorPerMas));
            pts[1] = app.Point3dFromXYZ(bpoint.X * (1 / UorPerMas), bpoint.Y * (1 / UorPerMas), bpoint.Z * (1 / UorPerMas));

            app.CadInputQueue.SendKeyin("Place Note");
            app.CadInputQueue.SendKeyin("TEXTEDITOR PLAYCOMMAND INSERT_TEXT \"" + myTest + "\"");
            app.CadInputQueue.SendDataPoint(pts[0], ViewNum + 1);
            app.CadInputQueue.SendDataPoint(pts[1], ViewNum + 1);
            long elemId = app.ActiveModelReference.GetLastValidGraphicalElement().ID;
            return Session.Instance.GetActiveDgnModel().FindElementById(new ElementId(ref elemId));

        }

        private Element NoteElement(string noteText, DPoint3d dp1, DPoint3d dp2,DMatrix3d rotationDm)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel model = Session.Instance.GetActiveDgnModel();
            TextBlockProperties tbProps = new TextBlockProperties(model);
            //tbProps.IsViewIndependent = true; //文字始终正对着视图   （问题非顶视图 比如前视图线会在文字中间）
            ParagraphProperties paraProps = new ParagraphProperties(model);
            string txtName = app.ActiveSettings.TextStyle.Name;
            BIM.TextStyle tt = app.ActiveDesignFile.TextStyles.Find("CERI_Standard-5.0mm");
            if(tt!=null)
            {
                app.ActiveSettings.TextStyle = tt;
            }
            
            string textstylename = Settings.TextStyleName;
            DgnTextStyle dtStyle = DgnTextStyle.GetByName(textstylename, Session.Instance.GetActiveDgnFile());
            if (dtStyle == null)
            {
                //BIM.DimensionStyle a = app.ActiveSettings.DimensionStyle;
                dtStyle = DgnTextStyle.GetSettings(dgnFile);

            }

            double txtwidth, txtHeight, widthDistance, heightDistance;
            dtStyle.GetProperty(TextStyleProperty.Width, out txtwidth);
            //dtStyle.SetProperty(TextStyleProperty.Width, txtwidth / myModelScal * myScal);    //画note不需要设置
            dtStyle.GetProperty(TextStyleProperty.Height, out txtHeight);
            //dtStyle.SetProperty(TextStyleProperty.Height, txtHeight / myModelScal * myScal);
            RunProperties runProps = new RunProperties(dtStyle, model);
            TextBlock TextBlock = new TextBlock(tbProps, paraProps, runProps, Session.Instance.GetActiveDgnModel());
            TextBlock.SetProperties(tbProps);
            TextBlock.AppendText(noteText);

            DimensionStyle dim_style = DimensionStyle.GetByName(Settings.DimensionStyleName, Session.Instance.GetActiveDgnFile());
            //dim_style = DimensionStyle.GetSettings(dgnFile);
            string dimName = app.ActiveSettings.DimensionStyle.Name;
            BIM.DimensionStyle dimS = app.ActiveSettings.DimensionStyle;
            string dimNam1 = Settings.DimensionStyleName;
            if (dim_style == null)
            {
                dim_style = DimensionStyle.GetSettings(dgnFile);

                dim_style.GetDistanceProp(out widthDistance, DimStyleProp.Text_Width_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.GetDistanceProp(out heightDistance, DimStyleProp.Text_Height_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.SetDistanceProp(1, DimStyleProp.Text_Height_DISTANCE, Session.Instance.GetActiveDgnModel());
                dim_style.SetDistanceProp(1, DimStyleProp.Text_Width_DISTANCE, Session.Instance.GetActiveDgnModel());

            }

            Element leaderElement = null;
            DPoint3d dp3 = dp2 + new DPoint3d(6800 * UorPerMas, 0, 0);
            DPoint3d[] leaderPoints = new DPoint3d[2] { dp1, dp2 }; //3 pts for leader already defined
            NoteCellHeaderElement noteCellHeaderElement = new NoteCellHeaderElement(out leaderElement, TextBlock, dim_style, model, leaderPoints);

            //DMatrix3d dm = DMatrix3d.Rotation(new DVector3d(0, 0, 1), Angle.FromDegrees(45));

            //DTransform3d dt = DTransform3d.FromMatrixAndFixedPoint(dm, dp2);

            //TransformInfo ta = new TransformInfo(dt);

            //noteCellHeaderElement.ApplyTransform(ta);
            
            noteCellHeaderElement.AddToModel(out leaderElement, dgnModel);

            //List<Element> eleList = noteCellHeaderElement.GetDependants();
            //Element dim = null;

            //if (!eleList[0].IsInvisible) dim = eleList[0];
            //else dim = eleList[1];

            DimensionElement dimEle = leaderElement as DimensionElement;

            dimEle.SetRotationMatrix(rotationDm);

            try
            {
                ElementPropertiesSetter element_properties_setter = new ElementPropertiesSetter();
                DgnFile active_file = Session.Instance.GetActiveDgnFile();
                FileLevelCache level_cache = active_file.GetLevelCache();
                LevelHandle level_handle = level_cache.GetLevelByName("标注");
                if (level_handle.IsValid)
                {
                    element_properties_setter.SetLevel(level_handle.LevelId);
                }
                else
                {
                    EditLevelHandle levelHandle = level_cache.CreateLevel("标注");
                    element_properties_setter.SetLevel(levelHandle.LevelId);
                }
                element_properties_setter.Apply(dimEle);
                element_properties_setter.Apply(noteCellHeaderElement);
                noteCellHeaderElement.ReplaceInModel(noteCellHeaderElement);
                dimEle.ReplaceInModel(dimEle);
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            
            return noteCellHeaderElement;
        }

        public static bool isGenbuZhijia(IECInstance iec)
        {
            bool isgb = false;

            if ("SUPPORT_TYPEE".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEB".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPED".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEG".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEH".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEI".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEJ".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEK".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEL".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }
            else if ("SUPPORT_TYPEA".Equals(iec.ClassDefinition.Name) || "SUPPORT_TYPEC".Equals(iec.ClassDefinition.Name) || "SUPPORT_TYPEF".Equals(iec.ClassDefinition.Name))
            {
                isgb = true;
            }

            return isgb;
        }
    }
}
