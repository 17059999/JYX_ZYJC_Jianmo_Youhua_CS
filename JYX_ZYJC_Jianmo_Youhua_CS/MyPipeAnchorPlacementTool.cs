using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using JYX_ZYJC_CLR;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class MyPipeAnchorPlacementTool : PipeSupportPlacementTool
    {
        #region 属性

        private int number = 0;//控制点击次数
        //private double R;//管夹半径
        private DPoint3d dPoint3D;//管夹放置点
        private DVector3d dVector3D;//管夹朝向
        private DPoint3d mPoint = new DPoint3d();//管道入口
        private DPoint3d rPoint = new DPoint3d();//管道出口
        private BMECObject eCObject;//管道存放
        private BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        //获取项目空间的单位
        private double uor_per_master =  1000;

        public MyPipeAnchorPlacementTool(MyPipeAnchorPlacementTool AnchorTool) : base(AnchorTool)
        {
        }

        public MyPipeAnchorPlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        #endregion

        
        #region 创建形体

        //创建形体方法
        private void CreateElement()
        {
            
         
            //设置形体创建位置之前选定的放置点
            PublicMethod.transform_bmec_object(this.ECObject, dPoint3D);
            //绘制形体
            this.ECObject.Create();
            number = 10;
        }
        #endregion

        #region 点击事件
        protected override void _OnDataButton(DgnButtonEvent e)
        {
            if (0 == number)//第一次点击
            {
                #region MyRegion

                //BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                //app.CommandState.AccuDrawHints.SetOrientationByXAxis;
                //选择元素，筛选
                //BIM.ElementScanCriteria esc;//ew BIM.ElementScanCriteria();
                //esc.IncludeOnlyWithinRange(app.Range3dFromPoint3d());

                //app.ActiveModelReference.Scan(esc);
                //app.ActiveModelReference.Attachments[0].Scan();

                //Element elem = hit_path.GetHeadElement();


                //在这里获取参数框参数

                //Transform旋转
                //AccuSnap.SnapEnabled = true;//精确捕捉
                #endregion

                
                //判断是否选中管道
                eCObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(e.Point);
                if (eCObject != null && eCObject.ClassName.Equals("PIPE"))
                {
                    //选中管道
                    
                    PublicMethod.get_two_port_object_end_points(eCObject, ref mPoint, ref rPoint);
                    
                    //锁定方向
                    dVector3D = new DVector3d(rPoint.X - mPoint.X, rPoint.Y - mPoint.Y, rPoint.Z - mPoint.Z);
                    JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(this.ECObject, new DVector3d(0, -1, 0), dVector3D);

                    //锁定直径
                    
                    this.ECObject.SetDoubleValueFromMM("JYX_DIAMETER", eCObject.GetDoubleValueInMM("OUTSIDE_DIAMETER")/2);

                    number++;
                    return;
                }
                else
                {
                    //未选中管道
                    //选定放置点
                    dPoint3D = e.Point;
                    PublicMethod.transform_bmec_object(this.ECObject, dPoint3D);
                    number++;
                    return;
                }
            }
            else if (1 == number)
            {
                if (eCObject != null && eCObject.ClassName.Equals("PIPE"))
                {
                    CreateElement();
                }
                else
                {
                    //锁定角度
                    JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(this.ECObject, new DVector3d(1, 0, 0), dVector3D);
                    AccuDraw.Origin = dPoint3D;
                    AccuDraw.SetRotationFromXZVectors(new DVector3d(dVector3D.X, dVector3D.Y, 0), new DVector3d(0, 0, 1));
                }
                number++;
                return;
            }
            else if (2 == number)
            {
                
                CreateElement();
            }
            
        }
        #endregion

        #region 动态显示

        protected override void _OnComplexDynamics(DgnButtonEvent e)
        {
            if (0 == number)//启动时的动态显示
            {
                #region MyRegion
                //R = this.ECObject.GetDoubleValueInMM("JYX_DIAMETER");
                //r = this.ECObject.GetDoubleValueInMM("JYX_R");
                //if (f != (R / uor_per_master) / uor_per_master)
                //{
                //    this.ECObject.SetDoubleValue("JYX_DIAMETER", R * uor_per_master * uor_per_master);
                //    this.ECObject.SetDoubleValue("JYX_R", r * uor_per_master * uor_per_master);
                //    f = R;
                //}
                //else
                //{
                //    this.ECObject.SetDoubleValue("JYX_DIAMETER", R );
                //    this.ECObject.SetDoubleValue("JYX_R", r);
                //}
                #endregion

                //动态显示跟随鼠标
                PublicMethod.transform_bmec_object(this.ECObject, e.Point);
                //绘制临时图形，实现动态显示
                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(this.ECObject);
            }
            else if (1 == number)
            {
                if (eCObject != null && eCObject.ClassName.Equals("PIPE"))
                {
                    dPoint3D = e.Point;
                    double x_Max = (mPoint.X > rPoint.X ? mPoint.X : rPoint.X);
                    double y_Max = (mPoint.Y > rPoint.Y ? mPoint.Y : rPoint.Y);
                    double z_Max = (mPoint.Z > rPoint.Z ? mPoint.Z : rPoint.Z);
                    double x_Mini = (mPoint.X < rPoint.X ? mPoint.X : rPoint.X);
                    double y_Mini = (mPoint.Y < rPoint.Y ? mPoint.Y : rPoint.Y);
                    double z_Mini = (mPoint.Z < rPoint.Z ? mPoint.Z : rPoint.Z);
                    dPoint3D.X = dPoint3D.X > x_Max ? x_Max : dPoint3D.X;
                    dPoint3D.Y = dPoint3D.Y > y_Max ? y_Max : dPoint3D.Y;
                    dPoint3D.Z = dPoint3D.Z > z_Max ? z_Max : dPoint3D.Z;
                    dPoint3D.X = dPoint3D.X < x_Mini ? x_Mini : dPoint3D.X;
                    dPoint3D.Y = dPoint3D.Y < y_Mini ? y_Mini : dPoint3D.Y;
                    dPoint3D.Z = dPoint3D.Z < z_Mini ? z_Mini : dPoint3D.Z;

                    if(dVector3D.X != 0)
                    {
                        dPoint3D.Y = dVector3D.Y == 0 ? rPoint.Y : (dPoint3D.X - mPoint.X) * (dVector3D.Y / dVector3D.X) + mPoint.Y;
                        dPoint3D.Z = dVector3D.Z == 0 ? rPoint.Z : (dPoint3D.X - mPoint.X) * (dVector3D.Z / dVector3D.X) + mPoint.Z;
                    }
                    else if (dVector3D.X == 0 && dVector3D.Y != 0)
                    {
                        dPoint3D.X = rPoint.X;
                        dPoint3D.Z = dVector3D.Z == 0 ? rPoint.Z : (dPoint3D.Y - mPoint.Y) * (dVector3D.Z / dVector3D.Y) + mPoint.Z;
                    }
                    else
                    {
                        dPoint3D.X = rPoint.X;
                        dPoint3D.Y = rPoint.Y;
                    }

                    //动态显示
                    PublicMethod.transform_bmec_object(this.ECObject, dPoint3D);//位移
                    //绘制临时图形，实现动态显示
                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(this.ECObject);
                }
                else
                {
                    //旋转
                    BMECObject dynamics_ecobject = new BMECObject();
                    dynamics_ecobject.Copy(this.ECObject);
                    dVector3D = new DVector3d(e.Point.X- dPoint3D.X, e.Point.Y-dPoint3D.Y, 0);
                    JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(dynamics_ecobject, new DVector3d(1, 0, 0), dVector3D);
                
                    
                    //绘制临时图形，实现动态显示
                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(dynamics_ecobject);

                }



            }
            else if (2 == number)   
            {
                if (this.ECObject.GetStringValue("JYX_GEOMETRY_ENABLED").Equals("F"))
                {
                    //半径随鼠标变化
                    double R = e.Point.X - dPoint3D.X;
                    R = R < 0 ? -R : R;
                    this.ECObject.SetDoubleValueFromMM("JYX_DIAMETER", R / uor_per_master);
                }
                
                
                //绘制临时图形，实现动态显示
                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(this.ECObject);
            }

        }
        #endregion

        public override void CreateTool()
        {
            new MyPipeAnchorPlacementTool(this).InstallTool();
            uor_per_master = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
            //开启动态绘制
            BeginDynamics();

            #region MyRegion
            //double length = 1000 * uor_per_master;
            //double diameter = 20 * uor_per_master;
            //double depth = 15 * uor_per_master;
            //DVector3d direction = DVector3d.FromXYZ(1, 0, 0);
            //double thickness = 5 * uor_per_master;
            //DPoint3d location = DPoint3d.FromXYZ(0, 0, 0);
            //Channel channel = new Channel(diameter, diameter, depth, length, DPoint3d.FromXYZ(0, 0, 0), location, direction, thickness);
            //List<ElementHolder> element_holder_list = channel.Create3DElements();
            //foreach (ElementHolder element_holder in element_holder_list)
            //{
            //    JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
            //}


            //DVector3d direction1 = DVector3d.FromXYZ(0, 0, 1);

            //Channel channel1 = new Channel(diameter, diameter, depth, length, DPoint3d.FromXYZ(0, 0, 0), location, direction1, thickness);
            //List<ElementHolder> elem
            #endregion


        }

        
    }
}
