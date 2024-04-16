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
    public class DrawingtestMark
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        static BMECApi api = BMECApi.Instance;
        public DgnModel dgnModel = Session.Instance.GetActiveDgnModel();
        public string myname;
        public IECInstance iec1 = null;
        public DPoint3d dpoint1;
        public List<Element> element = new List<Element>(); //扫描后获取设备 以及他的子类的元素集
        public List<Element> elementPipe = new List<Element>(); //扫描后获取管道及其子类的元素集
        public List<Element> elementPi = new List<Element>(); //扫描后获取阀门及其子类，支吊架的元素集
        public double UorPerMas = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        public List<DPoint3d> dpointlist1 = new List<DPoint3d>();
        public List<DPoint3d> dpointlist2 = new List<DPoint3d>();
        public List<DPoint3d> dpointPipelist1 = new List<DPoint3d>();
        public List<DPoint3d> dpointPilist1 = new List<DPoint3d>();
        public List<DPoint3d> drawingdplists = new List<DPoint3d>();
        public List<DPoint3d> drawingPipedplists = new List<DPoint3d>();
        public List<DPoint3d> drawingPidplists = new List<DPoint3d>();
        public List<string> namelist = new List<string>();
        public List<string> namelistnew = new List<string>();
        public int i = 0;
        public DMatrix3d ma = DMatrix3d.Identity;
        public DMatrix3d ma1 = DMatrix3d.Identity;
        public DTransform3d dt = DTransform3d.Identity;
        public DTransform3d dt1 = DTransform3d.Identity;
        public DPoint3d pyDp;
        double myScal = 1;
        double myScal1 = 1;
        public DVector3d pydv=new DVector3d();
        public DVector3d pydv1 = new DVector3d();
        public int length = 100;
        public double fount = 5;
        double annotationScale = 1;
        public void test(pipeBiaogaoType type)
        {
            try
            {
                #region
                length = length * (int)UorPerMas * 2;
                fount = fount * (int)UorPerMas * 2;

                ElementAgenda element_agenda = new ElementAgenda();
                SelectionSetManager.BuildAgenda(ref element_agenda); //获取选中的元素

                uint selected_elem_count = element_agenda.GetCount();

                if (selected_elem_count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("请先选中元素");
                    BMECApi.Instance.StartDefaultCommand();
                    return;
                }

                List<Element> yeleList = new List<Element>();
                for (uint i = 0; i < selected_elem_count; i++)
                {
                    Element ele = element_agenda.GetEntry(i);
                    yeleList.Add(ele);
                }

                string keyIn = "dimstyle active ";
                keyIn += "CERI_引线标注";
                app.CadInputQueue.SendKeyin(keyIn);

                Viewport active_view_port = Session.GetActiveViewport();
                DPoint3d[] viewBox = active_view_port.GetViewBox(DgnCoordSystem.Active, true);
                DVector3d dv1 = new DVector3d(viewBox[0], viewBox[1]);
                DVector3d unitDv1 = dv1 * (1 / dv1.Magnitude);
                DVector3d dv2 = new DVector3d(viewBox[0], viewBox[2]);
                DVector3d unitDv2 = dv2 * (1 / dv2.Magnitude);
                //扫描当前图纸的所有元素
                Bentley.DgnPlatformNET.ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();
                DgnModel reList = Session.Instance.GetActiveDgnModel(); //取当前激活图纸的model
                DgnModelType dgnModelType = reList.ModelType;
                DgnAttachmentCollection attList = reList.GetDgnAttachments();//引用上层default的model，进行转化，获取上层的元素集；
                foreach (var att in attList)
                {
                    DgnModel dgn = att.GetDgnModel();
                    double scal = att.DisplayScale;
                    myScal = scal;
                    ma = att.GetRotation();//原图矩阵
                    dt = new DTransform3d(ma);//旋转后的矩阵
                    annotationScale = dgnModel.GetModelInfo().AnnotationScaleFactor;
                    DTransform3d dtran;//点偏移
                    att.GetTransformToParent(out dtran, true);
                    pydv = new DVector3d(dtran.Translation);//偏移向量
                    //dt = dtran;
                    if (dgn == null) continue;
                    DgnAttachmentCollection attList1 = dgn.GetDgnAttachments();
                    if(dgnModelType==DgnModelType.Sheet)
                    {
                        foreach (var att1 in attList1)
                        {
                            DTransform3d dtran1;//点偏移
                            att1.GetTransformToParent(out dtran1, true);
                            pydv1  = new DVector3d(dtran1.Translation);//偏移向量
                            ma1 = att1.GetRotation();//def旋转矩阵
                            dt1 = new DTransform3d(ma1);//drawing旋转矩阵
                            dgn = att1.GetDgnModel();
                            //dt1 = dtran1;
                            if (dgn == null) continue;
                        }
                    }                    
                    //elements = dgn.GetGraphicElements();

                }

                foreach (Bentley.DgnPlatformNET.Elements.Element elem in yeleList)
                {
                    IECInstance ec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                    active_view_port = Session.GetActiveViewport();
                    if (ec == null) continue;
                    iec1 = ec;
                    bool isPi = api.InstanceDefinedAsClass(iec1, "PIPING_COMPONENT", true);//判断 是不是管道及其子类，阀门及其子类，支吊架
                    bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true); //判断是否是设备 以及他的子类
                    bool isPipe = api.InstanceDefinedAsClass(iec1, "PIPE", true); //判断是否为管道 以及他的子类
                    DPoint3d dpoint2 = new DPoint3d();
                    //不是管道及其子类，阀门及其子类，支吊架，设备 以及他的子类
                    if (!(isPi || eqb)) continue;
                    //阀门及其子类，支吊架，
                    else if (isPi && !isPipe)
                    {
                        elementPi.Add(elem); //添加符合条件的元素
                        ec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                        if (ec == null) continue;
                        BMECObject bmec = new BMECObject(ec);
                        iec1 = ec;
                        dpoint1 = (bmec.Transform3d.Translation);
                        if (dgnModel.ModelType == DgnModelType.Normal)
                        {
                            dpoint2 = dpoint1;
                        }
                        else
                        {
                            dt1.Multiply(out dpoint2, dpoint1);
                            dpoint2.ScaleInPlace(myScal1);
                            dpoint2 = dpoint2 + pydv1;
                            dt.Multiply(out dpoint2, dpoint2);
                            dpoint2.ScaleInPlace(myScal);
                        }
                        drawingPidplists.Add(dpoint2 + pydv);
                    }
                    //设备 以及他的子类
                    else if (eqb)
                    {
                        element.Add(elem); //添加符合条件的元素
                        ec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                        if (ec == null) continue;
                        BMECObject bmec = new BMECObject(ec);
                        iec1 = ec;
                        dpoint1 = (bmec.Transform3d.Translation);
                        if (dgnModel.ModelType == DgnModelType.Normal)
                        {
                            dpoint2 = dpoint1;
                        }
                        else
                        {
                            dt1.Multiply(out dpoint2, dpoint1);
                            dpoint2.ScaleInPlace(myScal1);
                            dpoint2 = dpoint2 + pydv1;
                            dt.Multiply(out dpoint2, dpoint2);
                            dpoint2.ScaleInPlace(myScal);
                        }
                        drawingdplists.Add(dpoint2 + pydv);
                    }
                    //管道 以及他的子类
                    else
                    {
                        elementPipe.Add(elem); //添加符合条件的元素
                        ec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                        if (ec == null) continue;
                        BMECObject bmec1 = new BMECObject(ec);
                        DPoint3d dp1 = bmec1.GetNthPort(0).LocationInUors;
                        DPoint3d dp2 = bmec1.GetNthPort(1).LocationInUors; //管道两端点
                        dpoint1 = dp1 + new DVector3d(dp1, dp2) * 0.5;//myScal * ((dpoint1 + 0.5 * new DVector3d(dpoint1, dpoint2)) * Sscal + pydv); //管道中心点    
                        if (dgnModel.ModelType == DgnModelType.Normal)
                        {
                            dp2 = dpoint1;
                        }
                        else
                        {
                            dt1.Multiply(out dp2, dpoint1);
                            dp2.ScaleInPlace(myScal1);
                            dp2 = dp2 + pydv1;
                            dt.Multiply(out dp2, dp2);
                            dp2.ScaleInPlace(myScal);
                        }
                        drawingPipedplists.Add(dp2 + pydv);
                    }
                    myname = "";
                }

                //读取第一个设备的位置
                if (element.Count != 0)
                {
                    DPoint3d dpoint0 = new DPoint3d(drawingdplists[0]);
                    List<double> list = new List<double>();
                    for (int a = 0; a < element.Count; a++)
                    {
                        DPoint3d dpoint2 = new DPoint3d(drawingdplists[a]);
                        DVector3d dv = new DVector3d(dpoint0, dpoint2);
                        double x = dv.DotProduct(unitDv2);
                        list.Add(x);//分组，看竖直方向的距离
                    }
                    List<double> listnew = new List<double>();//去重后，集合元素得到个数就是分组的数量,降序排序
                    foreach (double x in list)
                    {
                        if (!listnew.Contains(x))
                        {
                            listnew.Add(x);
                        }
                    }
                    listnew.Sort((x, y) => -x.CompareTo(y));
                    double[,] arr = new double[listnew.Count, list.Count];//二维数组整理存储设备角标
                    for (int a = 0; a < listnew.Count; a++)
                    {
                        int c = 0;
                        List<Element> eleList = new List<Element>();
                        //LineElement line = null, line1 = null;
                        Element mark = null;
                        for (int b = 0; b < list.Count; b++)
                        {

                            if (list[b] == listnew[a])
                            {
                                arr[a, c] = (double)b;
                                dpoint1 = drawingdplists[(int)arr[a, c]];
                                dpointlist1.Add(dpoint1);
                                dpointlist2.Add(dpoint1 + length * myScal * unitDv1 + length * myScal * unitDv2);
                                c++;
                            }
                        }
                        dpointlist1.Sort((x, y) => (-new DVector3d(x, x).DotProduct(unitDv1).CompareTo((new DVector3d(y, x).DotProduct(unitDv1)))));//排序坐标点
                        dpointlist2.Sort((x, y) => (-new DVector3d(x, x).DotProduct(unitDv1).CompareTo((new DVector3d(y, x).DotProduct(unitDv1)))));
                        for (int x = 0; x < dpointlist1.Count; x++)
                        {
                            iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(element[(int)arr[a, x]]);

                            bool isPi = api.InstanceDefinedAsClass(iec1, "PIPING_COMPONENT", true);//判断 是不是管道及其子类，阀门及其子类，支吊架
                            bool eqb = api.InstanceDefinedAsClass(iec1, "EQUIPMENT", true); //判断是否是设备 以及他的子类
                            bool isPipe = api.InstanceDefinedAsClass(iec1, "PIPE", true); //判断是否为管道 以及他的子类
                            if (isPi && !isPipe)
                            {
                                myname = iec1["NAME"].StringValue;
                            }

                            else if (eqb)
                            {
                                myname = iec1["CERI_Equipment_Num"].StringValue;
                            }
                            else return;
                            DPoint3d dpoint2;
                            //DSegment3d ds;
                            //LineElement line;
                            if (x == 0)
                            {
                                dpoint1 = dpointlist1[x];
                                dpoint2 = dpointlist2[x];
                            }
                            else
                            {
                                if (Math.Abs(new DVector3d(dpointlist1[x - 1], dpointlist1[x]).DotProduct(unitDv1)) <= length * myScal)
                                {
                                    dpoint1 = dpointlist1[x];
                                    dpoint2 = new DPoint3d(dpoint1 + length * myScal * unitDv1 + new DVector3d(dpointlist1[x - 1], dpointlist2[x - 1]).DotProduct(unitDv2) * unitDv2 - 2 * fount * myScal * unitDv2);
                                    dpointlist2[x] = dpoint2;
                                }
                                else
                                {
                                    dpoint1 = dpointlist1[x];
                                    dpoint2 = dpointlist2[x];
                                }
                            }
                            mark = NoteElement(myname, dpoint1, dpoint2);
                            app.CommandState.StartDefaultCommand();
                            //ds = new DSegment3d(dpoint1, dpoint2);
                            //line = new LineElement(dgnModel, null, ds);
                            //line.AddToModel();
                            //mark.AddToModel();
                        }
                        dpointlist1.Clear();
                        dpointlist2.Clear();
                    }
                    element.Clear();
                }
                if (elementPi.Count != 0)
                {
                    //Element mark = null;
                    for (int i = 0; i < elementPi.Count; i++)
                    {
                        dpoint1 = drawingPidplists[i];
                        DPoint3d dpoint2 = dpoint1 + length * myScal * unitDv1 + length * myScal * unitDv2;
                        iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elementPi[i]);
                        myname = iec1["NAME"].StringValue;
                        if (!namelist.Contains(myname))
                        {
                            //mark = NoteElement(myname, dpoint1, dpoint2);
                            app.CommandState.StartDefaultCommand();
                            namelist.Add(myname);
                        }
                        //mark = TextStringSendCommand(myname, dpoint1, dpoint2);
                        //app.CommandState.StartDefaultCommand();
                    }
                    elementPi.Clear();
                }
                if (elementPipe.Count != 0)
                {
                    Element mark = null;
                    for (int i = 0; i < elementPipe.Count; i++)
                    {
                        dpoint1 = drawingPipedplists[i];
                        DPoint3d dpoint2 = dpoint1 + length * myScal * unitDv1 + length * myScal * unitDv2;
                        iec1 = JYX_ZYJC_CLR.PublicMethod.FindInstance(elementPipe[i]);
                        myname = iec1["LINENUMBER"].StringValue;
                        if (!namelist.Contains(myname))
                        {
                            BMECObject bmec = new BMECObject(iec1);
                            myname= pipeBiaogaoForm.getpipexinxi(bmec, type, true);
                            mark = NoteElement(myname, dpoint1, dpoint2);
                            app.CommandState.StartDefaultCommand();
                            namelist.Add(myname);
                        }
                        //mark = TextStringSendCommand(myname, dpoint1, dpoint2);
                        //app.CommandState.StartDefaultCommand();
                    }
                    elementPipe.Clear();
                }
                #endregion
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

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

        public Element NoteElement(string noteText, DPoint3d dp1, DPoint3d dp2)
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
                element_properties_setter.Apply(leaderElement);
                element_properties_setter.Apply(noteCellHeaderElement);
                noteCellHeaderElement.ReplaceInModel(noteCellHeaderElement);
                leaderElement.ReplaceInModel(leaderElement);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            return noteCellHeaderElement;
        }
    }
}
