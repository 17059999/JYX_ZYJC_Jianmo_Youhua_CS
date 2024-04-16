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
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class drawBiaogao : DgnElementSetTool
    {
        public static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        static BMECApi api = BMECApi.Instance;
        double UorPerMM = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter/1000;
        double height = 0; // 标高
        string biaogao_text = "";
        biaogaoForm bgForm = null;
        int weishu = 2;
        public DMatrix3d ma = new DMatrix3d();
        public DMatrix3d ma1 = new DMatrix3d();
        public DTransform3d dt = DTransform3d.Identity;
        public DTransform3d dt1 = DTransform3d.Identity;
        public DVector3d pydv=new DVector3d(), pydv1=new DVector3d();
        double myScal = 1, annotationScale = 1; //Drawing
        double myScal1 = 1, annotationScale1 = 1; //Sheet
        Element underLine;
        Element cellHeader;
        int i = 0;
        DPoint3d firstPoint = new DPoint3d();
        DPoint3d underLeftPoint = new DPoint3d();
        DPoint3d underRightPoint = new DPoint3d();
        DVector3d dvL = new DVector3d();
        DVector3d dvR = new DVector3d();

        //启动命令
        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            bgForm = biaogaoForm.instance();
#if DEBUG
#else
            bgForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(biaogaoForm));
            bgForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            bgForm.Show();
            BeginDynamics();
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
            weishu = Convert.ToInt16(bgForm.biaogao_text.Text);
            DPoint3d dPoint = new DPoint3d();

            //DPoint3d underLeftPoint = new DPoint3d();
            //DPoint3d underRightPoint = new DPoint3d();
            if (i == 0)
            {
                dPoint = Get_att_point(ev.Point);
                height = dPoint.Z / UorPerMM / 1000;
                biaogao_text = string.Format("{0:F" + weishu.ToString() + "}", height);
                if (height > 0)
                {
                    biaogao_text = "+" + biaogao_text;
                }
                cellHeader = Biaogao(biaogao_text, ev.Point);

                underLeftPoint = ev.Point - 3.5 * annotationScale * unitDv1 * UorPerMM;
                dvL = 3.5 * annotationScale * unitDv1 * UorPerMM;
                underRightPoint = ev.Point + 30 * annotationScale * unitDv1 * UorPerMM;
                dvR = 30 * annotationScale * unitDv1 * UorPerMM;
                underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, underRightPoint)); //下划线
            }
            else if (i == 1)
            {
                cellHeader = Biaogao(biaogao_text, new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z));
                if (ev.Point.X > underRightPoint.X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z)));
                }
                else if ((firstPoint - dvR).X <= ev.Point.X && ev.Point.X <= underLeftPoint.X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(firstPoint - dvR, firstPoint + dvL));
                }
                else if (ev.Point.X < (firstPoint - dvR).X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z), firstPoint + dvL));
                }

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

        //左击
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            Viewport active_view_port = Session.GetActiveViewport();
            DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
            DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
            DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
            DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
            DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);

            i++;
            DgnModelType modelType = Session.Instance.GetActiveDgnModel().ModelType;
            DPoint3d dPoint = new DPoint3d();
            if (i == 1)
            {
                dPoint = Get_att_point(ev.Point);
                height = dPoint.Z / UorPerMM / 1000;
                string biaogao_text = string.Format("{0:F" + weishu.ToString() + "}", height);
                if (height > 0)
                {
                    biaogao_text = "+" + biaogao_text;
                }
                underLeftPoint = ev.Point - 3.5 * annotationScale * unitDv1 * UorPerMM;
                underRightPoint = ev.Point + 30 * annotationScale * unitDv1 * UorPerMM;
                firstPoint = ev.Point;
                underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, underRightPoint));
                //underLine.AddToModel();
            }
            else if (i == 2)
            {
                cellHeader = Biaogao(biaogao_text, new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z));
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
                element_properties_setter.Apply(cellHeader);
                cellHeader.AddToModel();
                if (ev.Point.X > underRightPoint.X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(underLeftPoint, new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z)));
                }
                else if ((firstPoint - dvR).X <= ev.Point.X && ev.Point.X <= underLeftPoint.X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(firstPoint - dvR, firstPoint + dvL));
                }
                else if (ev.Point.X < (firstPoint - dvR).X)
                {
                    underLine = new LineElement(dgnModel, null, new DSegment3d(new DPoint3d(ev.Point.X, firstPoint.Y, firstPoint.Z), firstPoint + dvL));
                }
                if (underLine != null)
                {
                    element_properties_setter.Apply(underLine);
                    underLine.AddToModel();
                }
                i = 0;
            }

            return true;
        }

        //右键
        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            if (i == 0)
            {
                EndDynamics();
                bgForm.Close();
                ExitTool();
            }
            else
            {
                i--;
            }

            return true;
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

        protected override void OnRestartTool()
        {
            throw new NotImplementedException();
        }
        //画标高
        private Element Biaogao(string myString, DPoint3d dPoint)
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
            DPoint3d dp2 = dp1 - 3.5 * unitDv1 * UorPerMM + 3.5 * unitDv2 * UorPerMM;
            DPoint3d dp3 = dp1 + 30 * unitDv1 * UorPerMM + 3.5 * unitDv2 * UorPerMM;
            DPoint3d dp4 = dp1 + 3.5 * unitDv1 * UorPerMM + 3.5 * unitDv2 * UorPerMM;
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
            trans.Translation = new DPoint3d(dp1.X - 3.5 * UorPerMM, dp1.Y + 3.5 * UorPerMM + txtheight / annotationScale * 0.75, dp1.Z);
            TransformInfo transInfo = new TransformInfo(trans);
            txtHandlerBase.ApplyTransform(transInfo);
            IList<Element> elements = new List<Element>();
            if (line1 != null && line2 != null && line3 != null /*&& line4 != null*/ && txtHandlerBase != null)
            {
                elements.Add(line1);
                elements.Add(line2);
                elements.Add(line3);


                //elements.Add(line4);
                elements.Add(txtHandlerBase);
            }
            CellHeaderElement cellHeaderElement = new CellHeaderElement(dgnModel, "cell", dp1, DMatrix3d.Identity, elements);
            JYX_ZYJC_CLR.PublicMethod.SetCellAnnotationTrue(cellHeaderElement);
            return cellHeaderElement;
        }

        //获取对应三维视图上的点
        private DPoint3d Get_att_point(DPoint3d inpoint)
        {
            DgnFile dgnFile = Session.Instance.GetActiveDgnFile();
            DgnModel reList = Session.Instance.GetActiveDgnModel(); //取当前激活图纸的model           
            DgnModelType dgnModelType = reList.ModelType;
            DgnAttachmentCollection attList = reList.GetDgnAttachments();//引用上层default的model，进行转化，获取上层的元素集；

            foreach (var att in attList)
            {
                DgnModel dgn = att.GetDgnModel();
                double scal = att.DisplayScale;
                myScal = scal;
                ma = att.GetRotation();//原图矩阵
                ma = ma.Transpose();
                if (dgn == null) continue;
                DTransform3d dtran;//点偏移
                att.GetTransformToParent(out dtran, true);
                pydv = new DVector3d(dtran.Translation);//偏移向量
                dt = new DTransform3d(ma);//旋转后的矩阵
                if (dgn == null) continue;
                DgnAttachmentCollection attList1 = dgn.GetDgnAttachments();
                if (dgnModelType == DgnModelType.Sheet)
                {
                    foreach (var att1 in attList1)
                    {
                        DTransform3d dtran2;//点偏移
                        att1.GetTransformToParent(out dtran2, true);
                        double scal1 = att1.DisplayScale;
                        myScal1 = scal1;
                        pydv1 = new DVector3d(dtran2.Translation);//偏移向量
                        ma1 = att1.GetRotation();//def旋转矩阵
                        dt1 = new DTransform3d(ma1);//drawing旋转矩阵
                        dgn = att1.GetDgnModel();
                        //dt1 = dtran1;
                        if (dgn == null) continue;
                    }
                }
            }

            if (reList.ModelType == DgnModelType.Normal)
            {
                return inpoint;
            }

            DPoint3d outpoint = new DPoint3d();
            DPoint3d outpoint1 = new DPoint3d();
            DTransform3d dn = dt;
            //DTransform3d dn1 = DTransform3d.Negate(dt1);

            DPoint3d inpoint1 = new DPoint3d(inpoint.X + 50, inpoint.Y + 50, inpoint.Z);
            //inpoint = inpoint + pydv1;
            //inpoint = inpoint * myScal1;
            //inpoint1 = inpoint1 + pydv1;
            //inpoint1 = inpoint1 * myScal1;

            //dn1.Multiply(out outpoint, inpoint);
            //dn1.Multiply(out outpoint1, inpoint1);

            //inpoint = outpoint;
            //inpoint1 = outpoint1;
            inpoint = inpoint - pydv;
            inpoint = inpoint * (1/myScal);
            inpoint1 = inpoint1 - pydv;
            inpoint1 = inpoint1 * (1 / myScal);

            dn.Multiply(out outpoint, inpoint);
            dn.Multiply(out outpoint1, inpoint1);

            if (outpoint.Z != outpoint1.Z) //切的剖面不水平
            {
                if (outpoint.Z != inpoint.Y)
                {
                    //outpoint = new DPoint3d(outpoint.X, outpoint.Y, -outpoint.Z);
                }
            }
            else if (outpoint.Z == outpoint1.Z) //切出来的剖面水平
            {
                outpoint = new DPoint3d(outpoint.X, outpoint.Y, outpoint.Z);
            }
            return outpoint;
        }
    }
}
