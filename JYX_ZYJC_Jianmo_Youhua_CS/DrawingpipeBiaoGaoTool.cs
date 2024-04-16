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
    public class DrawingpipeBiaoGaoTool : DgnElementSetTool
    {
        public int i = 0;
        public bool j = true;
        public static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public DPoint3d dpFirst = DPoint3d.Zero;
        public DPoint3d dpSecond = new DPoint3d();
        public DPoint3d dpThird = new DPoint3d();
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        public DgnModelType modelType;
        string myString = "";
        static BMECApi api = BMECApi.Instance;
        public pipeBiaogaoType type = pipeBiaogaoType.none;
        DPoint3d underLeftPoint = new DPoint3d();
        DPoint3d underRightPoint = new DPoint3d();
        DVector3d dvL = new DVector3d();
        DVector3d dvR = new DVector3d();

        double UorPerMM = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter/1000;
         
        double myScal = 1, annotationScale = 1; //Drawing
        double myScal1 = 1, annotationScale1 = 1; //Sheet
        LineElement underLine;
        Element cellHeader;
        public DrawingpipeBiaoGaoTool(pipeBiaogaoType pipeType) : base(0, 0)
        {
            type = pipeType;
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

            DgnModelType modelType = Session.Instance.GetActiveDgnModel().ModelType;
            //DPoint3d dPoint = new DPoint3d();

            //DPoint3d underLeftPoint = new DPoint3d();
            //DPoint3d underRightPoint = new DPoint3d();
            if (i == 0)
            {

                

                underLeftPoint = ev.Point - 3.5 * annotationScale * unitDv1 * UorPerMM;
                dvL = 3.5 * annotationScale * unitDv1 * UorPerMM;
                underRightPoint = ev.Point + 30 * annotationScale * unitDv1 * UorPerMM;
                dvR = 30 * annotationScale * unitDv1 * UorPerMM;

                underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, underRightPoint)); //下划线

                

                cellHeader = Biaogao(myString, ev.Point, null);
            }
            else if (i == 1)
            {
                
                if (ev.Point.X > underRightPoint.X)
                {
                    
                    underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z)));
                    
                }
                else if ((dpFirst - dvR).X <= ev.Point.X && ev.Point.X <= underLeftPoint.X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(dpFirst - dvR, dpFirst + dvL));

                    
                    
                }
                else if (ev.Point.X < (dpFirst - dvR).X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z), dpFirst + dvL));

                    
                }
                cellHeader = Biaogao(myString, new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z), null);
            }
            RedrawElems redrawElems = new RedrawElems();
            redrawElems.SetDynamicsViewsFromActiveViewSet(Bentley.MstnPlatformNET.Session.GetActiveViewport());
            redrawElems.DrawMode = DgnDrawMode.TempDraw;
            redrawElems.DrawPurpose = DrawPurpose.Dynamics;
            if (cellHeader != null)
            {
                redrawElems.DoRedraw(cellHeader); //文本，上划线，倒三角
            }
            if (underLine != null)
            {
                redrawElems.DoRedraw(underLine); //下划线
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
                        IECInstance iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                        //if (bmec.Instance == null) return true;
                        if (iec == null) return true;
                        //IECInstance iec1 = bmec.Instance;
                        IECInstance iec1 = iec;
                        //判断是否是管道以及它的子类
                        bool ispipe = api.InstanceDefinedAsClass(iec1, "PIPE", true);
                        if (ispipe)
                        {
                            BMECObject bmec = new BMECObject(iec1);
                            if (bmec != null)
                            {
                                myString = pipeBiaogaoForm.getpipexinxi(bmec, type);
                            }
                        }
                        else return true;
                        //string keyIn = "dimstyle active ";
                        //keyIn += "CERI_引线标注";
                        //app.CadInputQueue.SendKeyin(keyIn);
                        BeginDynamics();
                        j = false;
                    }
                }
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
                        

                        underLeftPoint = ev.Point - 3.5 * annotationScale * unitDv1 * UorPerMM;
                        underRightPoint = ev.Point + 30 * annotationScale * unitDv1 * UorPerMM;
                        underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, underRightPoint));

                        
                    }
                    //第二次点击
                    else if (i == 1)
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
                        
                        if (ev.Point.X > underRightPoint.X)
                        {
                            underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z)));
                        }
                        else if ((dpFirst - dvR).X <= ev.Point.X && ev.Point.X <= underLeftPoint.X)
                        {
                            underLine = new LineElement(dgnModel, null, new DSegment3d(dpFirst - dvR, dpFirst + dvL));

                        }
                        else if (ev.Point.X < (dpFirst - dvR).X)
                        {
                            underLine = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z), dpFirst + dvL));
                        }

                        //DPoint3d dvL1 = dvL;
                        //dvL1.ScaleInPlace(1/annotationScale);
                        //DPoint3d dvR1 = dvR;
                        //dvR1.ScaleInPlace(1 / annotationScale);
                        //LineElement line = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z), new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z) + dvL1));

                        cellHeader = Biaogao(myString, new DPoint3d(ev.Point.X, dpFirst.Y, dpFirst.Z), null);

                        element_properties_setter.Apply(cellHeader);
                        cellHeader.AddToModel();

                        if (underLine != null)
                        {
                            element_properties_setter.Apply(underLine);
                            underLine.AddToModel();
                        }
                        i = 0;
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
            if(!j)
            {
                EndDynamics();
                app.ShowCommand("请选择一个管道");
                j = true;
                i = 0;
            }
            else
            {
                ExitTool();
            }
            
            return true;
        }

        protected override void OnRestartTool()
        {
            DrawingpipeBiaoGaoTool tool = new DrawingpipeBiaoGaoTool(type);
            tool.InstallTool();
            //throw new NotImplementedException();
        }

        protected override void OnReinitialize()
        {

        }
        //启动命令
        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            dgnModel = Session.Instance.GetActiveDgnModel();
            app.ShowCommand("请选择一个管道");
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
            if (tt != null)
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

            txtStyle.SetProperty(TextStyleProperty.Width, 5 * UorPerMM * unitDv1.DotProduct(unitDv1));
            txtStyle.SetProperty(TextStyleProperty.Height, 5 * UorPerMM * unitDv1.DotProduct(unitDv1));


            RunProperties runProp = new RunProperties(txtStyle, dgnModel);
            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, dgnModel);
            txtBlock.AppendText(name);

            TextHandlerBase txtHandlerBase = TextHandlerBase.CreateElement(null, txtBlock);

            DRange3d dr;
            txtHandlerBase.CalcElementRange(out dr);

            double width = dr.High.X - dr.Low.X;
            DPoint3d endDp = startDp + (width + 15 * UorPerMM * bili) * unitDv1; //要加比例
            if (!isF)
            {
                endDp = startDp - (width + 15 * UorPerMM * bili) * unitDv1;
            }
            DSegment3d ds = new DSegment3d(startDp, endDp);
            LineElement line1 = new LineElement(dgnModel, null, ds);
            //width = new DVector3d(dr.High, dr.Low).DotProduct(unitDv1);
            DPoint3d textDp = startDp + 7.5 * UorPerMM * unitDv1 * bili + 7.5 * UorPerMM * unitDv2 * bili;
            if (!isF)
            {
                textDp = endDp + 7.5 * UorPerMM * unitDv1 * bili + 7.5 * UorPerMM * unitDv2 * bili;
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

        private Element NoteElement(string noteText, DPoint3d dp1, DPoint3d dp2, DMatrix3d rotationDm)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel model = Session.Instance.GetActiveDgnModel();
            TextBlockProperties tbProps = new TextBlockProperties(model);
            //tbProps.IsViewIndependent = true; //文字始终正对着视图   （问题非顶视图 比如前视图线会在文字中间）
            ParagraphProperties paraProps = new ParagraphProperties(model);
            string txtName = app.ActiveSettings.TextStyle.Name;
            BIM.TextStyle tt = app.ActiveDesignFile.TextStyles.Find("CERI_Standard-5.0mm");
            if (tt != null)
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
            DPoint3d dp3 = dp2 + new DPoint3d(6800 * UorPerMM, 0, 0);
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
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

            return noteCellHeaderElement;
        }

        //画标高
        private Element Biaogao(string myString, DPoint3d dPoint,LineElement line )
        {

            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel reList = Session.Instance.GetActiveDgnModel(); //取当前激活图纸的model
            DgnModelType dgnModelType = reList.ModelType;
            DgnAttachmentCollection attList = reList.GetDgnAttachments();//引用上层default的model，进行转化，获取上层的元素集；
            if (dgnModelType == DgnModelType.Drawing || dgnModelType == DgnModelType.Sheet)
            {
                foreach (var att in attList)
                {
                    DgnModel dgn = att.GetDgnModel();
                    myScal = att.DisplayScale;
                    annotationScale = dgnModel.GetModelInfo().AnnotationScaleFactor;
                    if (dgn == null) continue;
                    DgnAttachmentCollection attList1 = dgn.GetDgnAttachments();
                    if (dgnModelType == DgnModelType.Sheet)
                    {
                        foreach (var att1 in attList1)
                        {
                            dgn = att1.GetDgnModel();
                            myScal1 = att1.DisplayScale;
                            annotationScale1 = dgnModel.GetModelInfo().AnnotationScaleFactor;
                            if (dgn == null) continue;
                        }
                    }
                }
            }

            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);

            DPoint3d dp1 = dPoint;
            DPoint3d dp2 = dp1 - 3.5  * unitDv1 * UorPerMM + 3.5  * unitDv2 * UorPerMM;
            DPoint3d dp3 = dp1 + 30  * unitDv1 * UorPerMM + 3.5  * unitDv2 * UorPerMM;
            DPoint3d dp4 = dp1 + 3.5  * unitDv1 * UorPerMM + 3.5  * unitDv2 * UorPerMM;
            //DPoint3d dp5 = dp1 - 3.5 * annotationScale * unitDv1 * UorPerMM;
            //DPoint3d dp6 = dp1 + 30 * annotationScale * unitDv1 * UorPerMM;
            LineElement line1 = new LineElement(dgnModel, null, new DSegment3d(dp1, dp2));
            LineElement line2 = new LineElement(dgnModel, null, new DSegment3d(dp2, dp3));
            LineElement line3 = new LineElement(dgnModel, null, new DSegment3d(dp4, dp1));
            //LineElement line4 = new LineElement(dgnModel, null, new DSegment3d(dp5, dp6));
            TextBlockProperties txtBlockProp = new TextBlockProperties(dgnModel);
            txtBlockProp.IsViewIndependent = true;
            ParagraphProperties paraProp = new ParagraphProperties(dgnModel);

            BIM.TextStyle tt = app.ActiveDesignFile.TextStyles.Find("CERI_Standard-5.0mm");
            if (tt != null)
            {
                app.ActiveSettings.TextStyle = tt;
            }

            string textstylename = Settings.TextStyleName;
            DgnTextStyle txtStyle = DgnTextStyle.GetByName(textstylename, Session.Instance.GetActiveDgnFile());

            double wordwidth, wordheigh;
            txtStyle.GetProperty(TextStyleProperty.Width, out wordwidth);
            txtStyle.GetProperty(TextStyleProperty.Width, out wordheigh);
            txtStyle.SetProperty(TextStyleProperty.Width, 5 * 1000 /** myScal / annotationScale * myScal1 / annotationScale1*/);
            txtStyle.SetProperty(TextStyleProperty.Height, 5 * 1000 /** myScal / annotationScale * myScal1 / annotationScale1*/);
            RunProperties runProp = new RunProperties(txtStyle, dgnModel);
            TextBlock txtBlock = new TextBlock(txtBlockProp, paraProp, runProp, dgnModel);
            txtBlock.AppendText(myString);
            TextHandlerBase txtHandlerBase = TextHandlerBase.CreateElement(null, txtBlock);
            DRange3d dr;
            txtHandlerBase.CalcElementRange(out dr);
            double txtwidth = dr.High.X - dr.Low.X;
            double txtheight = dr.High.Y - dr.Low.Y;
            DTransform3d trans = DTransform3d.Identity;
            trans.Translation = new DPoint3d(dp1.X - 3.5  * UorPerMM, dp1.Y + 3.5  * UorPerMM + txtheight/annotationScale * 0.75, dp1.Z);
            TransformInfo transInfo = new TransformInfo(trans);
            txtHandlerBase.ApplyTransform(transInfo);
            IList<Element> elements = new List<Element>();
            if (line1 != null && line2 != null && line3 != null /*&& line4 != null*/ && txtHandlerBase != null)
            {
                elements.Add(line1);
                elements.Add(line2);
                elements.Add(line3);
                if (line!=null)
                {
                    elements.Add(line);
                }
                
                //elements.Add(line4);
                elements.Add(txtHandlerBase); 
            }
            CellHeaderElement cellHeaderElement = new CellHeaderElement(dgnModel, "cell", dp1, DMatrix3d.Identity, elements);
            JYX_ZYJC_CLR.PublicMethod.SetCellAnnotationTrue(cellHeaderElement);
            return cellHeaderElement;
        }
    }
}
