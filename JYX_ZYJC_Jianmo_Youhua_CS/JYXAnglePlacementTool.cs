using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.OpenPlant.Modeler.Api;
using BIM = Bentley.Interop.MicroStationDGN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.MstnPlatformNET.InteropServices;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class JYXAnglePlacementTool : InlinePlacementTool
    {
        static BIM.Application app = Utilities.ComApp;  //V8I

        /// <summary>
        /// 是否选中管道端点
        /// </summary>
        public bool isPort = false;

        /// <summary>
        /// 点击次数
        /// </summary>
        public int dataPointNice = 0;

        /// <summary>
        /// 起点
        /// </summary>
        public DPoint3d startDp = new DPoint3d();

        /// <summary>
        /// 形体入口向量
        /// </summary>
        public DVector3d mDv = new DVector3d();


        public JYXAnglePlacementTool(JYXAnglePlacementTool valveTool) : base(valveTool)
        {
        }

        public JYXAnglePlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }

        protected override void _OnResetButton(DgnButtonEvent e)
        {
            if (isPort)
            {
                isPort = false;
            }
            else
            {
                base._OnResetButton(e);
            }
            dataPointNice = 0;
        }
        protected override void _OnComplexDynamics(DgnButtonEvent e)
        {
            try
            {
                if (!isPort)
                {
                    base._OnComplexDynamics(e);
                }
                else
                {
                    BMECObject dynamic_object = new BMECObject();
                    dynamic_object.Copy(this._lastPersistedComponent);

                    int count = dynamic_object.Ports.Count;
                    if (count <= 1)
                    {
                        base._OnComplexDynamics(e);
                    }
                    else
                    {
                        Bentley.GeometryNET.DPoint3d[] dpts = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(dynamic_object);

                        DPoint3d endDp = startDp - mDv;

                        BIM.Point3d startP = GroupPipeTool.ceDpoint_v8Point(startDp);
                        BIM.Point3d endP = GroupPipeTool.ceDpoint_v8Point(endDp);
                        BIM.Point3d RunP = GroupPipeTool.ceDpoint_v8Point(dpts[1]);

                        BIM.LineElement line = app.CreateLineElement2(null, startP, endP);

                        BIM.Point3d touyingdian = line.ProjectPointOnPerpendicular(RunP, app.Matrix3dIdentity());

                        DPoint3d orgin = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(touyingdian);

                        DPlane3d dplane = new DPlane3d(orgin, mDv);
                        DRay3d ray = new DRay3d(e.Point, dplane.Normal);
                        DPoint3d new_point;
                        double fraction;
                        bool result = DPlane3d.Intersect(dplane, ray, out fraction, out new_point);

                        DVector3d qsDv = new DVector3d(orgin, dpts[1]);
                        DVector3d newDv = new DVector3d(orgin, new_point);

                        if (newDv.Distance(DVector3d.Zero) < 0.0001 || !result)
                        {
                            newDv = qsDv;
                        }
                        else
                        {
                            #region 判断是否反向
                            bool isGonx = qsDv.IsParallelOrOppositeTo(newDv); //判断是否共线

                            bool isTongx = qsDv.IsParallelTo(newDv); //判断是否同向
                            #endregion

                            #region 反向转2次
                            if (isGonx && !isTongx)
                            {
                                DVector3d midDv = mDv.CrossProduct(qsDv);

                                JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(dynamic_object, qsDv, midDv);

                                newDv = midDv;
                            }
                            #endregion

                            JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(dynamic_object, qsDv, newDv);
                            JYX_ZYJC_CLR.PublicMethod.display_bmec_object(dynamic_object);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                ExitTool();
            }
        }
        protected override void _OnDataButton(DgnButtonEvent e)
        {
            try
            {
                if (dataPointNice == 0)
                {
                    bool isOnPipe;
                    BMECObject eCObject = isPipe(e.Point, out isOnPipe);//判断是否选中管道,获取bool值Checked,与管道实例eCObject
                    //在base._OnDataButton(e)前面写 因为如果是法兰连接 会带出法兰形体 这个时候点击的位置变成了法兰 不再是管道端点了
                    base._OnDataButton(e);
                    dataPointNice++;                                       
                    if (isOnPipe)
                    {
                        DPoint3d mainDp = new DPoint3d(), runDp = new DPoint3d();

                        JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(eCObject, ref mainDp, ref runDp);//获取选中管道进出口位置

                        if (e.Point.Distance(mainDp) <= 0.001)
                        {
                            isPort = true;
                            startDp = mainDp;
                            mDv = new DVector3d(runDp, mainDp);
                        }
                        else if (e.Point.Distance(runDp) <= 0.001)
                        {
                            isPort = true;
                            startDp = mainDp;
                            mDv = new DVector3d(runDp, mainDp);
                        }
                        else
                        {
                            isPort = false;
                        }
                    }
                    else
                    {
                        isPort = false;
                    }

                }
                else if (dataPointNice == 1)
                {
                    if (isPort)
                    {
                        //BMECObject dynamic_object = new BMECObject();
                        //dynamic_object.Copy(this._lastPersistedComponent);
                        if(this._lastPersistedComponent==null)
                        {
                            base._OnDataButton(e);
                        }
                        else
                        {
                            Bentley.GeometryNET.DPoint3d[] dpts = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(this._lastPersistedComponent);

                            DPoint3d endDp = startDp - mDv;

                            BIM.Point3d startP = GroupPipeTool.ceDpoint_v8Point(startDp);
                            BIM.Point3d endP = GroupPipeTool.ceDpoint_v8Point(endDp);
                            BIM.Point3d RunP = GroupPipeTool.ceDpoint_v8Point(dpts[1]);

                            BIM.LineElement line = app.CreateLineElement2(null, startP, endP);

                            BIM.Point3d touyingdian = line.ProjectPointOnPerpendicular(RunP, app.Matrix3dIdentity());

                            DPoint3d orgin = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(touyingdian);

                            DPlane3d dplane = new DPlane3d(orgin, mDv);
                            DRay3d ray = new DRay3d(e.Point, dplane.Normal);
                            DPoint3d new_point;
                            double fraction;
                            bool result = DPlane3d.Intersect(dplane, ray, out fraction, out new_point);

                            DVector3d qsDv = new DVector3d(orgin, dpts[1]);
                            DVector3d newDv = new DVector3d(orgin, new_point);

                            if (newDv.Distance(DVector3d.Zero) < 0.0001 || !result)
                            {
                                newDv = qsDv;
                            }
                            else
                            {
                                #region 判断是否反向
                                bool isGonx = qsDv.IsParallelOrOppositeTo(newDv); //判断是否共线

                                bool isTongx = qsDv.IsParallelTo(newDv); //判断是否同向
                                #endregion

                                #region 反向转2次
                                if (isGonx && !isTongx)
                                {
                                    DVector3d midDv = mDv.CrossProduct(qsDv);

                                    JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(this._lastPersistedComponent, qsDv, midDv);

                                    newDv = midDv;
                                }
                                #endregion

                                JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(this._lastPersistedComponent, qsDv, newDv);
                                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(this._lastPersistedComponent);

                                this._lastPersistedComponent.Create();
                                this._lastPersistedComponent.DiscoverConnectionsEx();
                                this._lastPersistedComponent.UpdateConnections();
                            }
                            isPort = false;
                        }
                        
                    }
                    else
                    {
                        base._OnDataButton(e);
                    }
                    dataPointNice = 0;
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }

        }

        public override IPropertyContainerView CreateContainerView()
        {
            return new ValveView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
        }

        public override void CreateTool()
        {
            new JYXAnglePlacementTool(this).InstallTool();
        }

        public override void OnRestartCommand()
        {
            base.RestartCommand(new JYXAnglePlacementTool(this));
        }


        /// <summary>
        /// 判断是否选中管道
        /// </summary>
        /// <param name="point3D">点击点</param>
        /// <param name="Checked">是否选中</param>
        /// <returns></returns>
        public static BMECObject isPipe(DPoint3d point3D, out bool Checked)
        {
            BMECObject bMECObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectAtPoint(point3D);
            if (bMECObject != null && bMECObject.ClassName.Equals("PIPE"))
            {
                Checked = true;
                return bMECObject;
            }
            else
            {
                Checked = false;
                return null;
            }

        }
    }
}
