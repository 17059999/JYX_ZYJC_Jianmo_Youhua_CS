﻿using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using JYX_ZYJC_CLR;
using System.Runtime.InteropServices;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    
    public class MyPipeSupportPlacementTool : PipeSupportPlacementTool
    {
        #region 属性

        private BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;//此处用于选择元素
        private SupportForm supportForm;//窗体类
        private SupportView supportView;//窗体父类
        private int number = 0;//点击次数控制
        private DPoint3d dPoint3D;//放置点
        private DVector3d dVector3D = new DVector3d(0, 0, 0);//放置向量
        private BIM.Element v_element;
        private bool L1 = false;
        private double H;
        private double L;
        private double H1;
        BMECObject dynamics_ecobject = new BMECObject();//用于动态显示时，不改动原有值
        double uor_per_master = 1000;
        #endregion

        #region 构造方法
        public MyPipeSupportPlacementTool(MyPipeSupportPlacementTool supportTool) : base(supportTool)
        {
        }

        public MyPipeSupportPlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        #endregion
        
        #region 创建形体

        //创建形体方法
        private void CreateElement()
        {
            string s_l = (L == 0 ? "0" : string.Format("{0:N3}", L));
            string s_h = (H == 0 ? "0" : string.Format("{0:N3}", H));
            string s_h1 = (H1 == 0 ? "0" : string.Format("{0:N3}", H1));
            string JYX_TYPE_NAME = "A06-G-" + s_l + "-" + s_h + "-" + s_h1;
            this.ECObject.SetStringValue("JYX_TYPE_NAME", JYX_TYPE_NAME);
            this.ECObject.Create();
            number = 10;
        }
        #endregion

        #region 点击事件
        protected override void _OnDataButton(DgnButtonEvent e)
        {
            //第一次点击
            if (0 == number)
            {
                BIM.Point3d point3D = new BIM.Point3d();
                point3D.X = e.Point.X / uor_per_master;
                point3D.Y = e.Point.Y / uor_per_master;
                point3D.Z = e.Point.Z / uor_per_master;
                v_element = scan_element_at_point(point3D, true);
                //判断是否选中元素
                if (null != v_element)
                {//选中元素

                    long v_id = v_element.ID;
                    ElementId ce_element = new ElementId(ref v_id);
                    Element ce_elem = Session.Instance.GetActiveDgnModel().FindElementById(ce_element);

                    bool result = get_wall_near_shape_normal(ce_elem, e.Point,ref dVector3D);
                    if (!result || dVector3D.Equals(new DVector3d(0,0,0)))
                    {
                        
                        v_element = null;
                    }
                    else
                    {
                        //锁定朝向
                        PublicMethod.transform_bmec_object(this.ECObject, new DVector3d(1, 0, 0), new DVector3d(dVector3D.Y, -dVector3D.X, 0));

                        //锁定ACS
                        AccuDraw.Origin = e.Point;
                        AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.Y, -dVector3D.X, 0), new DVector3d(-dVector3D.X, -dVector3D.Y, 0));
                    }

                }
                if (null == v_element)
                {//未选中元素
                    //锁定放置点
                    dPoint3D = e.Point;
                    PublicMethod.transform_bmec_object(this.ECObject, dPoint3D);

                    //锁定ACS
                    AccuDraw.Origin = dPoint3D;
                    AccuDraw.SetRotationFromXZVectors(new DVector3d(1, 0, 0), new DVector3d(0, 0, 1));

                }
                number++;
                return;
                
            }

            if (null != v_element)
            {//选中元素
                //第二次点击
                if (1 == number)
                {
                    //确定物体位置
                     dPoint3D = e.Point;
                     PublicMethod.transform_bmec_object(this.ECObject, dPoint3D);
                    if (supportForm.checkBox_setParam.Checked)
                    {
                        if (supportForm.textBox_height.Text != "" && supportForm.textBox_length.Text != "" && supportForm.textBox_height1.Text != "")
                        {
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_HEIGHT")) this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT", double.Parse(supportForm.textBox_height.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH")) this.ECObject.SetDoubleValueFromMM("JYX_LENGTH", double.Parse(supportForm.textBox_length.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH1")) this.ECObject.SetDoubleValueFromMM("JYX_LENGTH1", double.Parse(supportForm.textBox_length.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_HEIGHT1")) this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT1", double.Parse(supportForm.textBox_height1.Text));
                            CreateElement();
                            return;
                        }
                    }
                    AccuDraw.Origin = dPoint3D;
                    //判断是否有高度参数
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_HEIGHT"))
                    {//没有高度参数
                     //设置ACS朝向

                        AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.Y, -dVector3D.X, 0), new DVector3d(-dVector3D.X, -dVector3D.Y, 0));
                        number++;
                    }
                    else
                    {
                        AccuDraw.SetRotationFromXZVectors(new DVector3d(0, 0, 1), new DVector3d(-dVector3D.X, -dVector3D.Y, 0));
                    }



                    number++;
                    return;
                }
                //第三次点击
                if (2 == number)
                {
                    //判断是否有长度参数
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH") && 0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH1")) CreateElement();
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH"))
                    {//L1
                        AccuDraw.SetRotationFromXZVectors(new DVector3d(-dVector3D.X, -dVector3D.Y, 0), new DVector3d(0, 0, 1));
                        L1 = true;
                    }
                    else
                    {//L
                        AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.Y, -dVector3D.X, 0), new DVector3d(-dVector3D.X, -dVector3D.Y, 0));
                    }
                    
                    AccuDraw.Origin = dPoint3D;

                    number++;
                    return;
                }
                //第四次点击
                if (3 == number)
                {
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_HEIGHT1")) CreateElement();
                    AccuDraw.Origin = dPoint3D;
                    AccuDraw.SetRotationFromXZVectors(new DVector3d(0, 0, 1), new DVector3d(-dVector3D.X, -dVector3D.Y, 0));
                    number++;
                    return;
                }
                if (4 == number)
                {
                    CreateElement();
                }
            }
            else
            {//未选中元素
                //第二次点击
                if (1 == number)
                {
                    //确定物体朝向
                    dVector3D = new DVector3d(e.Point.X - dPoint3D.X, e.Point.Y - dPoint3D.Y, 0);
                    if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH1"))
                    {
                        PublicMethod.transform_bmec_object(this.ECObject, new DVector3d(0, -1, 0), dVector3D);

                    }
                    else
                    {
                        PublicMethod.transform_bmec_object(this.ECObject, new DVector3d(1, 0, 0), dVector3D);
                    }

                    if (supportForm.checkBox_setParam.Checked)
                    {
                        if (supportForm.textBox_height.Text != "" && supportForm.textBox_length.Text != "" && supportForm.textBox_height1.Text != "")
                        {
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_HEIGHT")) this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT", double.Parse(supportForm.textBox_height.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH")) this.ECObject.SetDoubleValueFromMM("JYX_LENGTH", double.Parse(supportForm.textBox_length.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH1")) this.ECObject.SetDoubleValueFromMM("JYX_LENGTH1", double.Parse(supportForm.textBox_length.Text));
                            if (0 != this.ECObject.GetDoubleValueInMM("JYX_HEIGHT1")) this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT1", double.Parse(supportForm.textBox_height1.Text));
                            CreateElement();
                            return;
                        }
                    }

                    AccuDraw.Origin = dPoint3D;
                    //判断是否有高度参数
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_HEIGHT"))
                    {//没有高度参数
                     //设置ACS朝向

                        AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.X, dVector3D.Y, 0), new DVector3d(0, 0, 1));
                        number++;
                    }
                    else
                    {
                        AccuDraw.SetRotationFromXZVectors(new DVector3d(0, 0, 1), new DVector3d(0, 1, 0));
                    }
                    number++;
                    return;
                }
                if (2 == number)
                {
                    //判断是否有长度参数
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH") && 0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH1")) CreateElement();
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_LENGTH")) L1 = true;
                    
                    AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.X, dVector3D.Y, 0), new DVector3d(0, 0, 1));

                    AccuDraw.Origin = dPoint3D;


                    number++;
                    return;
                }
                if (3 == number)
                {
                    if (0 == this.ECObject.GetDoubleValueInMM("JYX_HEIGHT1")) CreateElement();
                    AccuDraw.Origin = dPoint3D;
                    AccuDraw.SetRotationFromXZVectors(new DVector3d(0, 0, 1), new DVector3d(0, 1, 0));
                    number++;
                    return;
                }
                if (4 == number)
                {
                    CreateElement();
                }
            }
            
        }
        #endregion

        #region 动态显示

        protected override void _OnComplexDynamics(DgnButtonEvent e)
        {
            //跟随鼠标动态
            if (0 == number)
            {
                SetProfile();
                //动态显示跟随鼠标
                PublicMethod.transform_bmec_object(this.ECObject, e.Point);
                //绘制临时图形，实现动态显示
                PublicMethod.display_bmec_object(this.ECObject);

            }
            //旋转动态
            if (1 == number)
            {
                SetProfile();
                dynamics_ecobject.Copy(this.ECObject);

                if (null != v_element)
                {
                    //动态显示跟随鼠标
                    PublicMethod.transform_bmec_object(this.ECObject, e.Point);
                }
                else
                {
                    dVector3D = new DVector3d(e.Point.X - dPoint3D.X, e.Point.Y - dPoint3D.Y, 0);
                    if(dVector3D.Y == 0 && (dVector3D.X<0))
                    {
                        dVector3D.X = -1000000000;
                        dVector3D.Y = 1;
                    }
                    if (0 != this.ECObject.GetDoubleValueInMM("JYX_LENGTH1"))
                    {
                        PublicMethod.transform_bmec_object(dynamics_ecobject, new DVector3d(0, -1, 0), dVector3D);

                    }
                    else
                    {
                        PublicMethod.transform_bmec_object(dynamics_ecobject, new DVector3d(1, 0, 0), dVector3D);
                    }
                }
                //跟随鼠标旋转
                //绘制临时图形，实现动态显示
                PublicMethod.display_bmec_object(dynamics_ecobject);
            }
            //高度动态
            if (2 == number)
            {
                SetProfile();
                //判断是否由用户输入参数
                if (supportForm.checkBox_setParam.Checked)
                {//用户输入参数
                    if (supportForm.textBox_height.Text == "")
                    {
                        H = 800;
                    }
                    else
                    {
                        H = double.Parse(supportForm.textBox_height.Text);
                    }
                }
                else
                {//参数随鼠标变化
                    //高度随鼠标变化
                    //H = (e.Point.Z - dPoint3D.Z) / uor_per_master;
                    H = AccuDraw.Delta.X / uor_per_master;
                    H = H < 0 ? -H : H;
                }
                this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT", H);


                dynamics_ecobject.Copy(this.ECObject);
                //绘制临时图形，实现动态显示
                PublicMethod.display_bmec_object(dynamics_ecobject);
            }
            //长度动态
            if (3 == number)
            {
                SetProfile();
                //判断是否由用户输入参数
                if (supportForm.checkBox_setParam.Checked)
                {//用户输入参数
                    if (supportForm.textBox_length.Text == "")
                    {
                        L = 800;
                    }
                    else
                    {
                        L = double.Parse(supportForm.textBox_length.Text);
                    }
                }
                else
                {//参数随鼠标变化
                    //高度随鼠标变化
                    //L = (e.Point.Z - dPoint3D.Z) / uor_per_master;
                    L = AccuDraw.Delta.X / uor_per_master;
                    L = L < 0 ? -L : L;
                }
                if (L1)
                {
                    this.ECObject.SetDoubleValueFromMM("JYX_LENGTH1", L);
                }
                else
                {

                    this.ECObject.SetDoubleValueFromMM("JYX_LENGTH", L);
                }


                dynamics_ecobject.Copy(this.ECObject);
                //绘制临时图形，实现动态显示
                PublicMethod.display_bmec_object(dynamics_ecobject);
            }
            //H1动态
            if (4 == number)
            {
                SetProfile();
                //判断是否由用户输入参数
                if (supportForm.checkBox_setParam.Checked)
                {//用户输入参数
                    if (supportForm.textBox_height1.Text == "")
                    {
                        H1 = 800;
                    }
                    else
                    {
                        H1 = double.Parse(supportForm.textBox_height1.Text);
                    }
                }
                else
                {//参数随鼠标变化
                    //高度随鼠标变化
                    //H1 = (e.Point.Z - dPoint3D.Z) / uor_per_master;
                    H1 = AccuDraw.Delta.X / uor_per_master;
                    H1 = H1 < 0 ? -H1 : H1;
                }
                this.ECObject.SetDoubleValueFromMM("JYX_HEIGHT1", H1);


                dynamics_ecobject.Copy(this.ECObject);
                //绘制临时图形，实现动态显示
                PublicMethod.display_bmec_object(dynamics_ecobject);
            }
        }
        #endregion
        
        #region 窗体控制

        //启动窗体
        public override void CreateTool()
        {
            
            new MyPipeSupportPlacementTool(this).InstallTool();
            //开启动态绘制
            BeginDynamics();
        }
        //初始化窗体类
        //public override void OnPostInstall()
        //{
        //    //判断窗体是否初始化,已经初始化则跳过
        //    if (supportForm == null)
        //    {
        //        supportForm = new SupportForm();//初始化窗体对象
        //        //通过下面两步将myForm显示在工具设置框中
        //        supportForm.AttachToToolSettings(MyAddin.s_addin);
        //        supportForm.Show();//显示窗体
        //    }
        //    AccuSnap.SnapEnabled = true;
        //    base.OnPostInstall();//调用的父类此方法，是空的
        //}
        //卸载窗体
        //public override void OnCleanup()
        //{
        //    base.OnCleanup();
        //    supportForm.DetachFromMicroStation();
        //}


        //右键点击事件，右键点击卸载窗体
        protected override void _OnResetButton(DgnButtonEvent e)
        {
            base._OnResetButton(e);
        }
        #endregion

        #region 选择元素

        public BIM.Element scan_element_at_point(BIM.Point3d point3d, bool scan_child_model, BIM.ModelReference model = null)
        {
            BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            if (model == null)
            {
                model = app.ActiveModelReference;
            }
            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();

            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = app.ActiveModelReference.Scan(esc);

            BIM.Element[] elems = ee.BuildArrayFromContents();
            if (elems.Length == 0)
            {
                if (scan_child_model)
                {
                    foreach (BIM.Attachment attachment in app.ActiveModelReference.Attachments)
                    {
                        BIM.Element attach_elem = scan_element_at_point(point3d, scan_child_model, attachment);
                        if (attach_elem != null)
                        {
                            return attach_elem;
                        }
                    }
                }
            }
            else
            {
                return elems[0];
            }

            return null;
        }
        public  BIM.Element scan_element_at_point(BIM.Point3d point3d, bool scan_child_model, BIM.Attachment attachment)
        {

            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();

            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = attachment.Scan(esc);

            BIM.Element[] elems = ee.BuildArrayFromContents();
            if (elems.Length == 0)
            {
                if (scan_child_model)
                {
                    foreach (BIM.Attachment child_attachment in attachment.Attachments)
                    {
                        BIM.Element attach_elem = scan_element_at_point(point3d, scan_child_model, child_attachment);
                        if (attach_elem != null)
                        {
                            return attach_elem;
                        }
                    }
                }
            }
            else
            {
                return elems[0];
            }
            return null;
        }
        bool get_wall_near_shape_normal(Element elem,DPoint3d position,ref DVector3d dvecter)
        {
            double target_area=0.0;
            if (elem != null)
            {
                if (elem.ElementType == MSElementType.CellHeader)
                {
                    CellHeaderElement cellheaderelement = null;
                    try
                    {
                        cellheaderelement=(CellHeaderElement)elem;
                    }
                    catch
                    {
                        return false;
                    }
                    ChildElementCollection cec = cellheaderelement.GetChildren();

                    foreach (Element child_elem in cec)
                    {
                        if (child_elem == null)
                        {

                        }
                        else if (child_elem.ElementType == MSElementType.CellHeader)
                        {
                            CellHeaderElement child_cellheaderelement = (CellHeaderElement)child_elem;
                            ChildElementCollection child_cec = child_cellheaderelement.GetChildren();

                            foreach (Element child_child_elem in child_cec)
                            {
                                if (child_child_elem.ElementType == MSElementType.Shape)
                                {
                                    ShapeElement shape = (ShapeElement)child_child_elem;
                                    CurveVector.InOutClassification inoutclassification = shape.GetCurveVector().PointInOnOutXY(position);
                                    if (inoutclassification == CurveVector.InOutClassification.In)
                                    {
                                        double area;
                                        DPoint3d cent;
                                        DVector3d nor;
                                        shape.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                        dvecter = nor;
                                        target_area = area;
                                        break;
                                    }
                                    else if (inoutclassification == CurveVector.InOutClassification.On)
                                    {
                                        if (dvecter == DVector3d.Zero)
                                        {
                                            double area;
                                            DPoint3d cent;
                                            DVector3d nor;
                                            shape.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                            dvecter = nor;
                                            target_area = area;
                                        }
                                        else
                                        {
                                            double area;
                                            DPoint3d cent;
                                            DVector3d nor ;
                                            
                                            shape.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                            if (area > target_area)
                                            {
                                                dvecter = nor;
                                                target_area = area;
                                            }
                                        }
                                    }

                                }
                                else if (child_child_elem.ElementType == MSElementType.ComplexShape)
                                {
                                    ComplexShapeElement complexShapeElement = (ComplexShapeElement)child_child_elem;
                                    CurveVector.InOutClassification inoutclassification = complexShapeElement.GetCurveVector().PointInOnOutXY(position);
                                    if (inoutclassification == CurveVector.InOutClassification.In)
                                    {
                                        double area;
                                        DPoint3d cent;
                                        DVector3d nor;
                                        complexShapeElement.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                        dvecter = nor;
                                        target_area = area;
                                        break;
                                    }
                                    else if (inoutclassification == CurveVector.InOutClassification.On)
                                    {
                                        if (dvecter == DVector3d.Zero)
                                        {
                                            double area;
                                            DPoint3d cent;
                                            DVector3d nor;
                                            complexShapeElement.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                            dvecter = nor;
                                            target_area = area;
                                        }
                                        else
                                        {
                                            double area;
                                            DPoint3d cent;
                                            DVector3d nor;

                                            complexShapeElement.GetCurveVector().CentroidNormalArea(out cent, out nor, out area);
                                            if (area > target_area)
                                            {
                                                dvecter = nor;
                                                target_area = area;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                }
            }
            return true;
        }
        #endregion

        #region 初始化窗体

        public override IPropertyContainerView CreateContainerView()
        {
            this.supportView = new SupportView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
            return supportView;
        }

        protected override void Dispose([MarshalAs(UnmanagedType.U1)] bool flag)
        {
            if (flag)
            {
                try
                {
                    return;
                }
                finally
                {
                    base.Dispose(true);
                }
            }
            base.Dispose(false);
        }
        #endregion

        #region 设置截面

        private void SetProfile()
        {

            supportForm = ((SupportView)this.ContainerManager.PropertyContainerView).supportForm;
            this.ECObject.SetDoubleValueFromMM("JYX_PROFILE_WIDTH", supportForm.cWidth);
            this.ECObject.SetDoubleValueFromMM("JYX_PROFILE_HEIGHT", supportForm.cHeight);
            this.ECObject.SetDoubleValueFromMM("JYX_PROFILE_THICKNESS", supportForm.cThickness);

            this.ECObject.SetDoubleValueFromMM("JYX_WEIGHT_DRY", supportForm.jyx_weight_dry);
            this.ECObject.SetStringValue("JYX_MATERIAL", supportForm.jyx_material);
            this.ECObject.SetStringValue("JYX_CATALOG", supportForm.jyx_catalog);
        }
        #endregion
        

    }
}
