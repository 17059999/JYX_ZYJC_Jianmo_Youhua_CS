using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class JYXValvePlacementTool : ValvePlacementTool
    {
        int data_point_count = 0;
        #region 构造方法

        public JYXValvePlacementTool(JYXValvePlacementTool valveTool) : base(valveTool)
        {
        }
        public JYXValvePlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        #endregion
        protected override void _OnResetButton(DgnButtonEvent e)
        {
            base._OnResetButton(e);
            data_point_count = 0;
        }
        protected override void _OnComplexDynamics(DgnButtonEvent e)
        {
            if (data_point_count == 0 || data_point_count == 1)
            {
                base._OnComplexDynamics(e);

            }else if (base.Operator!=null)
            {
                base._OnComplexDynamics(e);
                
            }
            else
            {
                
                BMECObject dynamic_object=new BMECObject();
                dynamic_object.Copy(this._lastPersistedComponent);

                Bentley.GeometryNET.DPoint3d[] dpts = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(dynamic_object);
                Bentley.GeometryNET.DPoint3d origin = dynamic_object.Transform3d.Translation;
                Bentley.GeometryNET.DPlane3d dplane = new Bentley.GeometryNET.DPlane3d(origin, Bentley.GeometryNET.DVector3d.Subtract(Bentley.GeometryNET.DVector3d.FromXYZ(dpts[0].X, dpts[0].Y, dpts[0].Z), Bentley.GeometryNET.DVector3d.FromXYZ(dpts[1].X, dpts[1].Y, dpts[1].Z)));
                Bentley.GeometryNET.DPoint3d new_point;
                Bentley.GeometryNET.DRay3d ray = new Bentley.GeometryNET.DRay3d(e.Point, dplane.Normal);
                double rayFraction;
                bool result = Bentley.GeometryNET.DPlane3d.Intersect(dplane, ray, out rayFraction, out new_point);
                Bentley.GeometryNET.DVector3d dvec = Bentley.GeometryNET.DVector3d.Subtract(Bentley.GeometryNET.DVector3d.FromXYZ(new_point.X, new_point.Y, new_point.Z), Bentley.GeometryNET.DVector3d.FromXYZ(origin.X, origin.Y, origin.Z));



                if ((dvec.DistanceXY(Bentley.GeometryNET.DVector3d.Zero) < 0.0001) || !result)
                {
                    dvec = this._lastPersistedComponent.Transform3d.RowZ;
                }
                JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(dynamic_object, this._lastPersistedComponent.Transform3d.RowZ, dvec);
                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(dynamic_object);
                //this._lastPersistedComponent.Create();
            }
        }
        protected  override void _OnDataButton(DgnButtonEvent e)
        {
            if (data_point_count==0|| data_point_count==1)
            {
                base._OnDataButton(e);
                data_point_count++;
            }else if (base.Operator != null)
            {
                base._OnDataButton(e);
                data_point_count = 0;
            }
            else
            {

                Bentley.GeometryNET.DPoint3d[] dpts = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(_lastPersistedComponent);
                Bentley.GeometryNET.DPoint3d origin = this._lastPersistedComponent.Transform3d.Translation;
                Bentley.GeometryNET.DPlane3d dplane = new Bentley.GeometryNET.DPlane3d(origin, Bentley.GeometryNET.DVector3d.Subtract(Bentley.GeometryNET.DVector3d.FromXYZ(dpts[0].X, dpts[0].Y, dpts[0].Z), Bentley.GeometryNET.DVector3d.FromXYZ(dpts[1].X, dpts[1].Y, dpts[1].Z)));
                
                
                Bentley.GeometryNET.DPoint3d new_point;
                Bentley.GeometryNET.DRay3d ray = new Bentley.GeometryNET.DRay3d(e.Point, dplane.Normal);
                double rayFraction;
                bool result = Bentley.GeometryNET.DPlane3d.Intersect(dplane, ray, out rayFraction, out new_point);
                Bentley.GeometryNET.DVector3d dvec = Bentley.GeometryNET.DVector3d.Subtract(Bentley.GeometryNET.DVector3d.FromXYZ(new_point.X, new_point.Y, new_point.Z), Bentley.GeometryNET.DVector3d.FromXYZ(origin.X, origin.Y, origin.Z));

                if ((dvec.DistanceXY(Bentley.GeometryNET.DVector3d.Zero)<0.0001)|| !result)
                {
                    dvec = this._lastPersistedComponent.Transform3d.RowZ;
                }
                JYX_ZYJC_CLR.PublicMethod.transform_bmec_object(_lastPersistedComponent, this._lastPersistedComponent.Transform3d.RowZ, dvec);
                JYX_ZYJC_CLR.PublicMethod.display_bmec_object(_lastPersistedComponent);


                this._lastPersistedComponent.Create();
                this._lastPersistedComponent.DiscoverConnectionsEx();
                this._lastPersistedComponent.UpdateConnections();
                data_point_count = 0;
            }
        }

        public override IPropertyContainerView CreateContainerView()
        {
            return new ValveView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
        }

        public override void CreateTool()
        {
            new JYXValvePlacementTool(this).InstallTool();
        }
        public override void OnRestartCommand()
        {
            base.RestartCommand(new JYXValvePlacementTool(this));
        }
    }
}
