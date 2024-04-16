using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.GeometryNET;
using Bentley.ECObjects.Instance;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.Building.Mechanical.Components;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class MoveSupportTool : DgnElementSetTool
    {
        //properties
        protected static BMECApi api = BMECApi.Instance;
        protected static Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public static double uorPerMaster = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        private int dataButtonCount = 0;//记录点击状态，没有点击，选中管道，选择移动点
        protected string errorMessage = "";
        private DPoint3d startPoint = DPoint3d.Zero;//移动的起点与终点
        private DPoint3d endPoint = DPoint3d.Zero;
        private DVector3d vec = DVector3d.Zero;//移动方向
        private DVector3d dynamicVec = DVector3d.Zero;//动态显示方向
        private BMECObject supportBmec = null;
        public static double uor_per_master = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//获取项目空间的单位

        public bool isD = false;
        public List<BMECObject> moveBmecs = new List<BMECObject>();
        public static void InstallNewTool()
        {
            MoveSupportTool moveSupportTool = new MoveSupportTool();
            moveSupportTool.InstallTool();
        }

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            InstallNewTool();
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
        }
        protected override void OnPostInstall()
        {
            base.OnPostInstall();

            ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
            if (ecList.Count > 0)
            {
                foreach (IECInstance ecinstance in ecList)
                {
                    bool b = api.InstanceDefinedAsClass(ecinstance, "SUPPORT", true); //查找ec的父类是否含有SUPPORT
                    if (b)
                    {
                        isD = true;
                        ulong eleid = JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(ecinstance);
                        BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(eleid);
                        moveBmecs.Add(bmec);
                    }
                }
                if (isD)
                {
                    app.ShowCommand("批量移动支吊架");
                    app.ShowPrompt("请选择第一个点");
                }


            }
            if (!isD)
            {
                app.ShowCommand("移动支吊架");
                app.ShowPrompt("请选择需要移动的支吊架");
            }
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            base.OnDataButton(ev);

            #region 单个移动
            if (!isD)
            {
                if (dataButtonCount == 0)
                {
                    HitPath hitPath = DoLocate(ev, true, 0);
                    if (hitPath == null)
                    {
                        return false;
                    }

                    Element elem = hitPath.GetHeadElement();

                    BMECObject bmec = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

                    if (bmec == null)
                    {
                        return false;
                    }

                    bool b = api.InstanceDefinedAsClass(bmec.Instance, "SUPPORT", true); //查找ec的父类是否含有SUPPORT

                    if (!b)
                    {
                        return false;
                    }
                    else
                    {
                        supportBmec = bmec;

                        DVector3d dv1 = new DVector3d(0, 1, 0);
                        dv1 = supportBmec.Transform3d.Matrix * dv1;

                        DPlane3d dplane3d = new DPlane3d(supportBmec.Transform3d.Translation, dv1);

                        DPoint3d closePoint;
                        dplane3d.ClosestPoint(ev.Point, out closePoint);

                        dataButtonCount = 1;
                        AccuDraw.Active = true;
                        BeginDynamics();
                        startPoint = closePoint;
                        app.ShowPrompt("请选择移动的终点");
                    }
                }
                else
                {
                    DMatrix3d dm = AccuDraw.Rotation.Transpose();

                    DTransform3d dt = new DTransform3d(dm);

                    DVector3d dv = dt * AccuDraw.Delta;

                    endPoint = startPoint + dv;
                    //supportBmec.Transform3d = 
                    DPoint3d orginDp = supportBmec.Transform3d.Translation + new DVector3d(startPoint, ev.Point);
                    api.TranslateComponent(supportBmec, orginDp);
                    try
                    {
                        SetPropertyAsDouble("SUPPORT_POINT_X", supportBmec, orginDp.X / uor_per_master);
                        SetPropertyAsDouble("SUPPORT_POINT_Y", supportBmec, orginDp.Y / uor_per_master);
                        SetPropertyAsDouble("SUPPORT_POINT_Z", supportBmec, orginDp.Z / uor_per_master);
                        supportBmec.Create();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }
                    finally
                    {
                        dataButtonCount = 0;
                        supportBmec = null;
                        InstallNewTool();
                    }
                }
            }
            #endregion

            #region 批量移动
            else
            {
                if (dataButtonCount == 0)
                {
                    dataButtonCount = 1;
                    AccuDraw.Active = true;
                    BeginDynamics();
                    startPoint = ev.Point;
                    app.ShowPrompt("请选择放置点");
                }
                else
                {
                    jindutiaoThread jindutiao = new jindutiaoThread("支架移动");

                    try
                    {
                        DVector3d dv = new DVector3d(startPoint, ev.Point);
                        if (moveBmecs.Count > 20)
                        {
                            jindutiao.Start();
                        }
                        int x = 0;
                        foreach (var v in moveBmecs)
                        {
                            DPoint3d orginDp = v.Transform3d.Translation + new DVector3d(startPoint, ev.Point);
                            api.TranslateComponent(v, orginDp);
                            SetPropertyAsDouble("SUPPORT_POINT_X", v, orginDp.X / uor_per_master);
                            SetPropertyAsDouble("SUPPORT_POINT_Y", v, orginDp.Y / uor_per_master);
                            SetPropertyAsDouble("SUPPORT_POINT_Z", v, orginDp.Z / uor_per_master);
                            v.Create();
                            if (moveBmecs.Count > 20)
                            {
                                jindutiao.SetValue(x * 100 / moveBmecs.Count);
                                x++;
                            }
                        }
                        if (moveBmecs.Count > 20)
                        {
                            jindutiao.SetValue(100);
                            jindutiao.End();
                        }

                        System.Windows.Forms.MessageBox.Show("移动成功！");
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                        if (moveBmecs.Count > 20)
                        {
                            jindutiao.End();
                        }
                    }
                    finally
                    {
                        ExitTool();
                    }
                }
            }
            #endregion

            

            return true;
        }

        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            if (dataButtonCount == 0) return;

            #region 单个移动
            if(!isD)
            {
                if (supportBmec != null)
                {
                    BMECObject bmec = new BMECObject();
                    bmec.Copy(supportBmec);

                    DTransform3d dt = bmec.Transform3d;

                    DMatrix3d dm = AccuDraw.Rotation.Transpose();

                    DTransform3d dt1 = new DTransform3d(dm);

                    DVector3d dv = dt1 * AccuDraw.Delta;

                    //dt.Translation = bmec.Transform3d.Translation + dv;
                    dt.Translation = dt.Translation + new DVector3d(startPoint, ev.Point);

                    bmec.Transform3d = dt;

                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(bmec);
                }
            }            
            #endregion

            #region 多个移动
            else
            {
                foreach (var v in moveBmecs)
                {
                    BMECObject bmec = new BMECObject();
                    bmec.Copy(v);

                    DTransform3d dt = bmec.Transform3d;

                    //dt.Translation = bmec.Transform3d.Translation + dv;
                    dt.Translation = dt.Translation + new DVector3d(startPoint, ev.Point);

                    bmec.Transform3d = dt;

                    JYX_ZYJC_CLR.PublicMethod.display_bmec_object(bmec);
                }
            }
            #endregion
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            #region 单个移动
            if(!isD)
            {
                if (dataButtonCount == 1)
                {
                    dataButtonCount = 0;
                    app.ShowCommand("移动支吊架");
                    app.ShowPrompt("请选择需要移动的支吊架");
                    supportBmec = null;
                    InstallNewTool();
                }
                else
                {
                    //app.CommandState.StartDefaultCommand();
                    ExitTool();
                }
            }
            #endregion

            #region 多个移动
            else
            {
                if (dataButtonCount == 1)
                {
                    dataButtonCount = 0;
                    app.ShowCommand("批量移动支吊架");
                    app.ShowPrompt("请选择第一个点");
                    EndDynamics();
                    startPoint = new DPoint3d();
                }
                else
                {
                    //app.CommandState.StartDefaultCommand();
                    ExitTool();
                }
            }
            #endregion

            return true;
        }

        public static bool SetPropertyAsDouble(string propertyName, IBMECObject _bmECObject, double Value)
        {
            IECPropertyValue iECPropertyValue = _bmECObject.Instance.FindPropertyValue(propertyName, true, true, true);
            if (iECPropertyValue == null)
            {
                return false;
            }
            if (!iECPropertyValue.IsNull)
            {
                iECPropertyValue.DoubleValue = Value;
                return true;
            }
            return false;
        }
    }
}
