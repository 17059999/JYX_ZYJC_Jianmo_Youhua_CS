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

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class CopySupportTool : DgnElementSetTool
    {
        //properties
        protected static BMECApi api = BMECApi.Instance;
        protected static Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public static double uorPerMM = Bentley.MstnPlatformNET.Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter/1000;
        private int dataButtonCount = 0;//记录点击状态，没有点击，选中管道，选择移动点
        protected string errorMessage = "";
        private DPoint3d startPoint = DPoint3d.Zero;//移动的起点与终点
        private DPoint3d endPoint = DPoint3d.Zero;
     
        private BMECObject supportBmec = new BMECObject();
        public bool isD = false;
        public List<BMECObject> copeBmecs = new List<BMECObject>();

        public static void InstallNewTool()
        {
            CopySupportTool copySupportTool = new CopySupportTool();
            copySupportTool.InstallTool();
        }

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            //InstallNewTool();
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
        }
        protected override void OnPostInstall()
        {
            //base.OnPostInstall();

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
                        BMECObject bmec = new BMECObject(ecinstance);
                        copeBmecs.Add(bmec);
                    }
                }
                if (isD)
                {
                    app.ShowCommand("复制支吊架");
                    app.ShowPrompt("请选择第一个点");
                }

                
            }
            if(!isD)
            {
                base.OnPostInstall();
                app.ShowCommand("复制支吊架");
                app.ShowPrompt("请选择需要复制的支吊架");
            }            
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            base.OnDataButton(ev);

            if(!isD)
            {
                #region 单个复制
                if (dataButtonCount == 0)
                {
                    HitPath hitPath = DoLocate(ev, true, 0);
                    if (hitPath == null)
                    {
                        return false;
                    }

                    Element elem = hitPath.GetHeadElement();
                    IECInstance bmeciec = JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                    BMECObject bmec = new BMECObject(bmeciec);

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
                        
                        supportBmec.Copy(bmec);
                        
                        DVector3d dv1 = new DVector3d(0, 1, 0);
                        dv1 = supportBmec.Transform3d.Matrix * dv1;

                        DPlane3d dplane3d = new DPlane3d(supportBmec.Transform3d.Translation, dv1);

                        DPoint3d closePoint;
                        dplane3d.ClosestPoint(ev.Point, out closePoint);

                        dataButtonCount = 1;
                        AccuDraw.Active = true;
                        BeginDynamics();
                        startPoint = closePoint;
                        app.ShowPrompt("请选择放置点");

                        MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_X", supportBmec, startPoint.X / MoveSupportTool.uor_per_master);
                        MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Y", supportBmec, startPoint.Y / MoveSupportTool.uor_per_master);
                        MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Z", supportBmec, startPoint.Z / MoveSupportTool.uor_per_master);
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

                    MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_X", supportBmec, orginDp.X / MoveSupportTool.uor_per_master);
                    MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Y", supportBmec, orginDp.Y / MoveSupportTool.uor_per_master);
                    MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Z", supportBmec, orginDp.Z / MoveSupportTool.uor_per_master);
                    try
                    {
                        startPoint = ev.Point;
                        BMECObject bMECObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(ev.Point);
                        if (bMECObject != null && bMECObject.Instance != null && BMECApi.Instance.InstanceDefinedAsClass(bMECObject.Instance, "PIPE", true))
                        {
                            api.TranslateComponent(supportBmec, ev.Point);
                            supportBmec.RelatedInstance = bMECObject.Instance;
                            supportBmec.Create();
                            //OPM_Public_Api.AddDeviceHasSupport(bMECObject.Instance, copyBmec.Instance);
                        }
                        else
                        {
                            supportBmec.Create();
                        }

                        BMECObject newSupportBmec = new BMECObject();
                        newSupportBmec.Copy(supportBmec);
                        supportBmec = newSupportBmec;
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());

                        dataButtonCount = 0;
                        supportBmec = new BMECObject();
                        InstallNewTool();
                    }
                }
                #endregion
            }

            else
            {
                #region 多个支架
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
                    jindutiaoThread jindutiao = new jindutiaoThread("支架复制");

                    try
                    {
                        DVector3d dv = new DVector3d(startPoint, ev.Point);
                        if (copeBmecs.Count > 20)
                        {
                            jindutiao.Start();
                        }
                        int x = 0;
                        foreach (var v in copeBmecs)
                        {
                            DPoint3d orginDp = v.Transform3d.Translation + new DVector3d(startPoint, ev.Point);
                            BMECObject copyBmec = new BMECObject();
                            copyBmec.Copy(v);
                            api.TranslateComponent(copyBmec, orginDp);
                            Bentley.Interop.MicroStationDGN.Point3d point3D = new Bentley.Interop.MicroStationDGN.Point3d();
                            point3D.X = orginDp.X / uorPerMM;
                            point3D.Y = orginDp.Y / uorPerMM;
                            point3D.Z = orginDp.Z / uorPerMM;
                            Bentley.Interop.MicroStationDGN.Element[] v_elements = CommandsList.scan_element_at_point(point3D, true);
                            if(v_elements!=null)
                            {
                                for (int i = 0; i < v_elements.Length; i++)
                                {
                                    BMECObject bMECObject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id((ulong)v_elements[i].ID);
                                    if (bMECObject != null && bMECObject.ClassName.Equals("PIPE"))
                                    {
                                        copyBmec.RelatedInstance = bMECObject.Instance;
                                    }
                                }
                            }
                            MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_X", copyBmec, orginDp.X / MoveSupportTool.uor_per_master);
                            MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Y", copyBmec, orginDp.Y / MoveSupportTool.uor_per_master);
                            MoveSupportTool.SetPropertyAsDouble("SUPPORT_POINT_Z", copyBmec, orginDp.Z / MoveSupportTool.uor_per_master);
                            copyBmec.Create();
                            if (copeBmecs.Count > 20)
                            {
                                jindutiao.SetValue(x * 100 / copeBmecs.Count);
                                x++;
                            }
                        }
                        if (copeBmecs.Count > 20)
                        {
                            jindutiao.SetValue(100);
                            jindutiao.End();
                        }
                    }
                    catch(Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                        if(copeBmecs.Count>20)
                        {
                            jindutiao.End();
                        }
                    }
                }
                #endregion
            }


            return true;
        }

        protected override void OnDynamicFrame(DgnButtonEvent ev)
        {
            if (dataButtonCount == 0) return;
            #region 单个支架
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

            #region 多个支架
            else
            {
                foreach(var v in copeBmecs)
                {
                    BMECObject bmec = new BMECObject();
                    bmec.Copy(v);

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
            
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            #region 单个支架
            if(!isD)
            {
                if (dataButtonCount == 1)
                {
                    dataButtonCount = 0;
                    app.ShowCommand("复制支吊架");
                    app.ShowPrompt("请选择需要复制的支吊架");
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
            #region 多个支架
            else
            {
                if (dataButtonCount == 1)
                {
                    dataButtonCount = 0;
                    app.ShowCommand("复制支吊架");
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

        protected override bool IsModifyOriginal()
        {
            return false;
        }

        protected override bool NeedPointForSelection() { return false; }
    }
}
