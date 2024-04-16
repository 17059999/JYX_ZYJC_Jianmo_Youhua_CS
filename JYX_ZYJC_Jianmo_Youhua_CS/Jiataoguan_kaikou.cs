
using System;
using System.Windows.Forms;
using Bentley.Interop.MicroStationDGN;
using BIM = Bentley.Interop.MicroStationDGN;
using System.Collections.Generic;
using Bentley.ECObjects.Instance;

using Bentley.MstnPlatformNET.InteropServices;
using BD = Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using BG = Bentley.GeometryNET;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class Jiataoguan_kaikou : BD.DgnElementSetTool
    {
        protected BMECApi api = BMECApi.Instance;
        private BIM.Application app = Utilities.ComApp;
        private int data_point_n = 0;
        public BMECObject m_bmec_object1, m_bmec_object2;

        public override BD.StatusInt OnElementModify(Bentley.DgnPlatformNET.Elements.Element element)
        {
            return BD.StatusInt.Error;
        }
        protected override void OnPostInstall()
        {
            base.OnPostInstall();

            data_point_n = 0;

            app.CommandState.EnableAccuSnap();
            Bentley.MstnPlatformNET.Settings.SnapMode = BD.SnapMode.Nearest;

            app.ShowCommand("夹套管回切");
            app.ShowPrompt("请选择第一个圆管");
        }

        protected override void OnCleanup()
        {
        }
        public static void InstallNewTool()
        {
            Jiataoguan_kaikou jiataoguan_kaikou = new Jiataoguan_kaikou();
            jiataoguan_kaikou.InstallTool();
        }
        protected override bool OnResetButton(BD.DgnButtonEvent ev)
        {
            if (data_point_n == 0)
            {
                BMECApi.Instance.StartDefaultCommand();
            }
            else
            {
                BMECApi.Instance.SelectComponent(m_bmec_object1.Instance,false);
                data_point_n = 0;
                m_bmec_object1 = null;
                m_bmec_object2 = null;
                app.ShowCommand("夹套管回切");
                app.ShowPrompt("请选择第一个圆管");
            }
            return true;
        }
        protected override void OnRestartTool()
        {
            return;
        }
        protected override bool NeedAcceptPoint()
        {
            return false;
        }
        protected override bool OnDataButton(BD.DgnButtonEvent ev)
        {
            BD.HitPath hit_path = DoLocate(ev, true, 0);
            if (hit_path == null)
            {
                return true;
            }
            BD.Elements.Element elem = hit_path.GetHeadElement();

            BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

            if (bmec_object == null)
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return true;
            }
            else if (bmec_object.ClassName != "PIPE")
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return true;
            }
            else if (data_point_n==0)
            {
                BMECApi.Instance.SelectComponent(bmec_object.Instance, true);
                m_bmec_object1 = bmec_object;
                data_point_n = 1;
                app.ShowPrompt("请选择第二个圆管！");
            }else if (data_point_n==1)
            {
                if(bmec_object.Instance["GUID"].StringValue == m_bmec_object1.Instance["GUID"].StringValue)
                {
                    MessageBox.Show("前后两次选中的管道相同，请重新选择！");
                    return true;
                }
                m_bmec_object2 = bmec_object;
                string errorMessage;
                BMECApi.Instance.SelectComponent(m_bmec_object1.Instance, false);
                kaikouhan(m_bmec_object1, m_bmec_object2,out errorMessage);

                InstallNewTool();
            }
            return true;
        }

        public bool kaikouhan(BMECObject bmec_Object1, BMECObject bmec_Object2,out string errorMessage)
        {
            errorMessage = string.Empty;

            BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];
            BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];
            line1_end_pts[0] = Mstn_Public_Api.DPoint3d_To_Point3d(m_bmec_object1.GetNthPort(0).LocationInUors);
            line1_end_pts[1] = Mstn_Public_Api.DPoint3d_To_Point3d(m_bmec_object1.GetNthPort(1).LocationInUors);


            line2_end_pts[0] = Mstn_Public_Api.DPoint3d_To_Point3d(m_bmec_object2.GetNthPort(0).LocationInUors);
            line2_end_pts[1] = Mstn_Public_Api.DPoint3d_To_Point3d(m_bmec_object2.GetNthPort(1).LocationInUors);

            BIM.Ray3d ray3d1 = app.Ray3dFromPoint3dStartEnd(line1_end_pts[0], line1_end_pts[1]);
            BIM.Ray3d ray3d2 = app.Ray3dFromPoint3dStartEnd(line2_end_pts[0], line2_end_pts[1]);

            BIM.Point3d intersect_point1, intersect_point2;
            intersect_point1 = intersect_point2 = app.Point3dZero();
            double fraction1, fraction2;
            fraction1 = fraction2 = 0.0;
            bool reuslt = app.Ray3dRay3dClosestApproach(ray3d1, ray3d2, ref intersect_point1, ref fraction1, ref intersect_point2, ref fraction2);


            if (!reuslt)
            {
                errorMessage = "选中的管道不在一个平面上";
                return false;
            }
            else
            {
                BIM.Point3d intersect_point = intersect_point1;

                BIM.LineElement line = app.CreateLineElement2(null, app.Point3dZero(), intersect_point);


                BIM.Point3d nearest_point1 = Mstn_Public_Api.get_nearest_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
                BIM.Point3d faster_point1 = Mstn_Public_Api.get_faster_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
                if (app.Point3dEqual(nearest_point1, faster_point1))
                {
                    nearest_point1 = line1_end_pts[0];
                    faster_point1 = line1_end_pts[1];
                }

                BIM.Point3d nearest_point2 = Mstn_Public_Api.get_nearest_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
                BIM.Point3d faster_point2 = Mstn_Public_Api.get_faster_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
                if (app.Point3dEqual(nearest_point2, faster_point2))
                {
                    nearest_point2 = line2_end_pts[0];
                    faster_point2 = line2_end_pts[1];
                }

                BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);
                BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);
                double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
                if (MyPublic_Api.is_double_xiangdeng(angle, 180))
                {
                    errorMessage = "两个管道互相平行!";
                    return false;
                }
                //double distance1 = app.Point3dDistance(nearest_point1,intersect_point);
                //double distance2 = app.Point3dDistance(nearest_point2, intersect_point);

                //bool first_is_charu_pipe=(distance1< distance2);

                //if (first_is_charu_pipe)
                //{
                //    BMECObject tap_object = api.TapConnUtil.Create(bmec_object2.Instance["NOMINAL_DIAMETER"].DoubleValue);
                //    CliPublicMethod.transform_tap(bmec_object2, tap_object, point_near2);
                //    api.TapConnUtil.AddTapConnectionToComponent(bmec_object1, tap_object);

                //    BBMA::_dPoint3d _dp_near2 = public_method.Point3d_to_dPoint3d(point_near2);
                //    Port port1 = bmec_object2.FindClosestPort(&_dp_near2);
                //    Port port2 = tap_object.FindClosestPort(&_dp_near2);



                //    api.FastenerPlacementUtil.ClearRefComponents();
                //    api.FastenerPlacementUtil.Cache.Clear();
                //    api.FastenerPlacementUtil.WaferedAdjustment = true;
                //    api.FastenerPlacementUtil.AddRefComponent(bmec_object2);

                //    api.ConnectObjectsAtPorts(port1, bmec_object2, port2, tap_object);


                //}
                //else
                //{
                //    BMECObject tap_object = api.TapConnUtil.Create(bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue);
                //    CliPublicMethod.transform_tap(bmec_object1, tap_object, point_near1);
                //    api.TapConnUtil.AddTapConnectionToComponent(bmec_object2, tap_object);

                //    BBMA::_dPoint3d _dp_near1 = public_method.Point3d_to_dPoint3d(point_near1);
                //    Port port1 = bmec_object1.FindClosestPort(&_dp_near1);
                //    Port port2 = tap_object.FindClosestPort(&_dp_near1);

                //    api.FastenerPlacementUtil.ClearRefComponents();
                //    api.FastenerPlacementUtil.Cache.Clear();
                //    api.FastenerPlacementUtil.WaferedAdjustment = true;
                //    api.FastenerPlacementUtil.AddRefComponent(bmec_object1);

                //    api.ConnectObjectsAtPorts(port1, bmec_object1, port2, tap_object);
                //}
                
                bool has_thickness1 = false, has_thickness2 = false;
                if (m_bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue > 0)
                {
                    has_thickness1 = true;
                }
                if (m_bmec_object2.Instance["INSULATION_THICKNESS"].DoubleValue > 0)
                {
                    has_thickness2 = true;
                }
                ulong cell_id1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(m_bmec_object1);
                ulong cell_id2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(m_bmec_object2);
                double[] mianjis = new double[2];
                mianjis = JYX_ZYJC_CLR.PublicMethod.kaikouhan(cell_id1, cell_id2, nearest_point1, nearest_point2, faster_point1, faster_point2, has_thickness1, has_thickness2);

                //double tiji1 = mianjis[0] * bmec_object1.Instance["WALL_THICKNESS"].DoubleValue / 1000;
                //double tiji2 = mianjis[1] * bmec_object2.Instance["WALL_THICKNESS"].DoubleValue / 1000;
                //double midu1 = xmlProcessor.get_midu("MIDU_VALUE_MAP", "_" + bmec_object1.Instance["MATERIAL"].StringValue);
                //double midu2 = xmlProcessor.get_midu("MIDU_VALUE_MAP", "_" + bmec_object2.Instance["MATERIAL"].StringValue);
                //IECInstance ec_instance1 = bmec_object1.Instance;
                //IECInstance ec_instance2 = bmec_object2.Instance;

                //ec_instance1["tiji"].StringValue = tiji1.ToString();
                //ec_instance2["tiji"].StringValue = tiji2.ToString();
                //ec_instance1["caizhi"].StringValue = "_" + bmec_object1.Instance["MATERIAL"].StringValue;
                //ec_instance2["caizhi"].StringValue = "_" + bmec_object2.Instance["MATERIAL"].StringValue;
                //ec_instance1["zhongliang"].StringValue = Convert.ToString(tiji1 * midu1);
                //ec_instance2["zhongliang"].StringValue = Convert.ToString(tiji2 * midu2);
                //ec_instance1["DRY_WEIGHT"].StringValue = Convert.ToString(tiji1 * midu1);
                //ec_instance2["DRY_WEIGHT"].StringValue = Convert.ToString(tiji2 * midu2);
                //var model_refP = (IntPtr)app.ActiveModelReference.MdlModelRefP();

                //XmlInstanceUpdate.UpdateInstance(ec_instance1, model_refP, false);
                //XmlInstanceUpdate.UpdateInstance(ec_instance2, model_refP, false);
                return true;
            }
        }
    }
}
