using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using System;
using Bentley.OpenPlant.Modeler.Api;
using System.Collections.Generic;
using BIM = Bentley.Interop.MicroStationDGN;
using BG = Bentley.GeometryNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.ECObjects.Instance;
using System.Windows.Forms;
using Bentley.Plant.StandardPreferences;
using Bentley.OpenPlantModeler.SDK.Components;
using Bentley.Building.Mechanical.ComponentLibrary.Equipment;
using Bentley.Building.Mechanical.Components;
using Bentley.ECObjects.Schema;
using System.Collections;
using System.Linq;
using System.Xml;
using System.IO;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class OPM_Public_Api
    {
        static double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//当前设计文件的主单位
        protected static BMECApi api = BMECApi.Instance;

        protected static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public static BMECObject create_elbow(double nominal_diameter, double angle, double center_to_outlet_end, double center_to_run_end, double insulation_thickness, string insulation, out string errorMessage)
        {

            errorMessage = string.Empty;
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("LONG_RADIUS_90_DEGREE_PIPE_ELBOW", true);
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = nominal_diameter;
            ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, true);
            BMECObject ec_object = null;

            if (ec_instance_list.Count == 0)
            {
                errorMessage = "当前Specification为配置弯管。";
                return null;
            }
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                instance["ANGLE"].DoubleValue = angle;
                instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = center_to_outlet_end;
                instance["EC_CLASS_NAME"].StringValue = "LONG_RADIUS_" + angle + "_DEGREE_PIPE_ELBOW";
                instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = center_to_run_end;
                instance["SPECIFICATION"].StringValue = DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                instance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
                instance["INSULATION"].StringValue = insulation;
                //instance["caizhi"].StringValue = caizhi;
                ec_object = new BMECObject(instance);
            }
            return ec_object;
        }

        /// <summary>
        /// 获取选中的管道
        /// </summary>
        /// <returns>选中管道的 BMECObject 的集合</returns>
        public static List<BMECObject> get_selected_pipes()
        {
            List<BMECObject> selectedpipes = new List<BMECObject>();//选中的管道的容器
            List<BMECObject> tempECObject = new List<BMECObject>();//临时容器
            ElementAgenda elementAgenda = new ElementAgenda();//选中Element的集合
            SelectionSetManager.BuildAgenda(ref elementAgenda);//获取选中的元素的集合
            for (uint i = 0; i < elementAgenda.GetCount(); i++)//获取选中 Element 的 BMECObject
            {
                Element element = elementAgenda.GetEntry(i);
                BMECObject ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);//通过ElementId 获取 BMECObject
                tempECObject.Add(ec_object);
            }
            if (tempECObject.Count > 0)
            {
                foreach (var ecobject in tempECObject)//将选中的类型为 PIPE 的 BMECObject 存到 selectedpipes 容器中
                {
                    if (ecobject.ClassName == "PIPE")
                    {
                        selectedpipes.Add(ecobject);
                    }
                }
            }
            return selectedpipes;
        }
        public static BMECObject create_pipe(BG.DPoint3d startPoint, BG.DPoint3d endPoint, double nominal_diameter, double insulation_thickness = 0)
        {
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);
            BMECApi api = BMECApi.Instance;
            ISpecProcessor specProcessor = api.SpecProcessor;

            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = nominal_diameter;
            //elbow_iec_instance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;

            ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, true);
            BMECObject ec_object = null;
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                instance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
                ec_object = new BMECObject(instance);
                ec_object.SetLinearPoints(startPoint, endPoint);
            }
            return ec_object;

        }

        public static bool huiqie_pipe(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage)
        {
            errorMessage = string.Empty;
            BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];
            BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];
            line1_end_pts[0] = Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(0).LocationInUors);
            line1_end_pts[1] = Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(1).LocationInUors);


            line2_end_pts[0] = Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(0).LocationInUors);
            line2_end_pts[1] = Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(1).LocationInUors);

            BIM.Ray3d ray3d1 = app.Ray3dFromPoint3dStartEnd(line1_end_pts[0], line1_end_pts[1]);
            BIM.Ray3d ray3d2 = app.Ray3dFromPoint3dStartEnd(line2_end_pts[0], line2_end_pts[1]);


            BIM.Point3d intersect_point1, intersect_point2;
            intersect_point1 = intersect_point2 = app.Point3dZero();
            double fraction1, fraction2;
            fraction1 = fraction2 = 0.0;
            bool reuslt = app.Ray3dRay3dClosestApproach(ray3d1, ray3d2, ref intersect_point1, ref fraction1, ref intersect_point2, ref fraction2);


            if (!reuslt)
            {
                errorMessage = "选中的管道不在一个平面上或平行";
                return false;
            }
            else
            {
                BIM.Point3d interset = intersect_point1;
                BIM.Point3d nearest_point1 = Mstn_Public_Api.get_nearest_point(interset, line1_end_pts[0], line1_end_pts[1]);
                BIM.Point3d faster_point1 = Mstn_Public_Api.get_faster_point(interset, line1_end_pts[0], line1_end_pts[1]);
                if (app.Point3dEqual(nearest_point1, faster_point1))
                {
                    nearest_point1 = line1_end_pts[0];
                    faster_point1 = line1_end_pts[1];
                }

                BIM.Point3d nearest_point2 = Mstn_Public_Api.get_nearest_point(interset, line2_end_pts[0], line2_end_pts[1]);
                BIM.Point3d faster_point2 = Mstn_Public_Api.get_faster_point(interset, line2_end_pts[0], line2_end_pts[1]);
                if (app.Point3dEqual(nearest_point2, faster_point2))
                {
                    nearest_point2 = line2_end_pts[0];
                    faster_point2 = line2_end_pts[1];
                }

                BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);
                BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);
                double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
                if (MyPublic_Api.is_double_xiangdeng(angle, 180.0))
                {
                    errorMessage = "选中的管道接近平行";
                    return false;
                }

                double baowen_houdu = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
                if (baowen_houdu < bmec_object2.Instance["INSULATION_THICKNESS"].DoubleValue)
                {
                    baowen_houdu = bmec_object2.Instance["INSULATION_THICKNESS"].DoubleValue;
                }

                double r = Convert.ToDouble(bmec_object1.Ports[0].Instance["OUTSIDE_DIAMETER"].DoubleValue) / 2.0 + baowen_houdu;
                double d = r * Math.Tan(BG.Angle.DegreesToRadians(angle / 2));
                double d1 = app.Point3dDistance(interset, faster_point1);
                double d2 = app.Point3dDistance(interset, faster_point2);

                BIM.Point3d line_start1 = faster_point1;
                BIM.Point3d line_start2 = faster_point2;
                BIM.Point3d line_v1 = app.Point3dSubtract(interset, faster_point1);
                BIM.Point3d line_v2 = app.Point3dSubtract(interset, faster_point2);
                BIM.Point3d line_end1 = app.Point3dAdd(interset, app.Point3dScale(line_v1, d / d1));
                BIM.Point3d line_end2 = app.Point3dAdd(interset, app.Point3dScale(line_v2, d / d2));

                bool has_huiqie1 = JYX_ZYJC_CLR.PublicMethod.has_huiqie(bmec_object1);
                bool has_huiqie2 = JYX_ZYJC_CLR.PublicMethod.has_huiqie(bmec_object2);
                bool has_baowenceng1 = false;
                bool has_baowenceng2 = false;
                ulong baowen_cone_surface_id1 = 0, baowen_cone_surface_id2 = 0;
                ulong cone_surface_id1 = 0;
                ulong cone_surface_id2 = 0;
                double baowen_houdu1 = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
                double baowen_houdu2 = bmec_object2.Instance["INSULATION_THICKNESS"].DoubleValue;
                double pipe_od1 = bmec_object1.Ports[0].Instance["OUTSIDE_DIAMETER"].DoubleValue;
                double pipe_od2 = bmec_object2.Ports[0].Instance["OUTSIDE_DIAMETER"].DoubleValue;
                double baowenceng_od1 = pipe_od1 + baowen_houdu1 * 2;
                double baowenceng_od2 = pipe_od2 + baowen_houdu2 * 2;
                ElementHolder new_cone_surface1;
                ElementHolder new_baowen_cone_surface1 = null;
                ElementHolder new_cone_surface2;
                ElementHolder new_baowen_cone_surface2 = null;

                if (!has_huiqie1)
                {
                    new_cone_surface1 = OPM_Public_Api.create_cone_surface(pipe_od1, line_start1, line_end1);
                    if (baowen_houdu1 > 0)
                    {
                        new_baowen_cone_surface1 = OPM_Public_Api.create_cone_surface(baowenceng_od1, line_start1, line_end1);
                        has_baowenceng1 = true;
                    }
                }
                else
                {

                    if (baowen_houdu1 > 0)
                    {
                        ulong[] element_ids = { 0, 0 };
                        element_ids = JYX_ZYJC_CLR.PublicMethod.get_pipe_baowenceng_geometry(bmec_object1);
                        cone_surface_id1 = element_ids[0];
                        baowen_cone_surface_id1 = element_ids[1];

                        ElementHolder add_cone_surface1 = OPM_Public_Api.create_cone_surface(pipe_od1, interset, line_end1);
                        ElementHolder add_baowen_cone_surface1 = OPM_Public_Api.create_cone_surface(baowenceng_od1, interset, line_end1);

                        new_cone_surface1 = JYX_ZYJC_CLR.PublicMethod.unite_elem(cone_surface_id1, add_cone_surface1);
                        new_baowen_cone_surface1 = JYX_ZYJC_CLR.PublicMethod.unite_elem(baowen_cone_surface_id1, add_baowen_cone_surface1);

                        //JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(add_cone_surface_id1);
                        //JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(add_baowen_cone_surface_id1);

                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(cone_surface_id1);
                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(baowen_cone_surface_id1);
                        has_baowenceng1 = true;
                    }
                    else
                    {
                        cone_surface_id1 = JYX_ZYJC_CLR.PublicMethod.get_pipe_geometry(bmec_object1);
                        ElementHolder add_cone_surface1 = create_cone_surface(pipe_od1, interset, line_end1);

                        new_cone_surface1 = JYX_ZYJC_CLR.PublicMethod.unite_elem(cone_surface_id1, add_cone_surface1);

                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(cone_surface_id1);
                    }
                }
                if (!has_huiqie2)
                {

                    new_cone_surface2 = create_cone_surface(pipe_od2, line_start2, line_end2);
                    if (baowen_houdu2 > 0)
                    {
                        new_baowen_cone_surface2 = create_cone_surface(baowenceng_od2, line_start2, line_end2);
                        has_baowenceng2 = true;
                    }
                }
                else
                {
                    if (baowen_houdu2 > 0)
                    {
                        ulong[] element_ids = { 0, 0 };
                        element_ids = JYX_ZYJC_CLR.PublicMethod.get_pipe_baowenceng_geometry(bmec_object2);
                        cone_surface_id2 = element_ids[0];
                        baowen_cone_surface_id2 = element_ids[1];

                        ElementHolder add_cone_surface2 = create_cone_surface(pipe_od2, interset, line_end2);
                        ElementHolder add_baowen_cone_surface2 = create_cone_surface(baowenceng_od2, interset, line_end2);

                        new_cone_surface2 = JYX_ZYJC_CLR.PublicMethod.unite_elem(cone_surface_id2, add_cone_surface2);
                        new_baowen_cone_surface2 = JYX_ZYJC_CLR.PublicMethod.unite_elem(baowen_cone_surface_id2, add_baowen_cone_surface2);

                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(cone_surface_id2);
                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(baowen_cone_surface_id2);

                        has_baowenceng2 = true;
                    }
                    else
                    {
                        ElementHolder add_cone_surface2 = create_cone_surface(pipe_od2, interset, line_end2);
                        cone_surface_id2 = JYX_ZYJC_CLR.PublicMethod.get_pipe_geometry(bmec_object2);
                        new_cone_surface2 = JYX_ZYJC_CLR.PublicMethod.unite_elem(cone_surface_id2, add_cone_surface2);

                        JYX_ZYJC_CLR.PublicMethod.delete_elem_by_id(cone_surface_id2);
                    }
                }

                if (d1 < Parameters_Setting.huitu_tolerance || d2 < Parameters_Setting.huitu_tolerance)
                {
                    errorMessage = "角度过小或弯曲半径过大，无法回切";
                    return false;
                }
                else
                {
                }

                BIM.Point3d new_faster_point1 = Mstn_Public_Api.get_faster_point(interset, line_start1, line_end1);
                BIM.Point3d new_faster_point2 = Mstn_Public_Api.get_faster_point(interset, line_start2, line_end2);
                BIM.Point3d new_nearest_point1 = Mstn_Public_Api.get_nearest_point(interset, line_start1, line_end1);
                BIM.Point3d new_nearest_point2 = Mstn_Public_Api.get_nearest_point(interset, line_start2, line_end2);

                if (app.Point3dEqual(new_faster_point1, new_nearest_point1))
                {
                    new_nearest_point1 = line_start1;
                    new_faster_point1 = line_end1;
                }
                if (app.Point3dEqual(new_faster_point2, new_nearest_point2))
                {
                    new_nearest_point2 = line_start2;
                    new_faster_point2 = line_end2;
                }
                double dn = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;


                //BMECObject tap_object = api.TapConnUtil.Create(bmec_object2.Instance["NOMINAL_DIAMETER"].DoubleValue);
                //JYX_ZYJC_CLR.PublicMethod.transform_tap(bmec_object2, tap_object, interset);
                //api.TapConnUtil.AddTapConnectionToComponent(bmec_object1, tap_object);

                //tap_object.UpdateConnections();

                //tap_object = null;
                string text = BMECInstanceManager.FindConfigVariableName("OPM_ALIGNMENT_TOLERANCE");
                BMECInstanceManager.SetConfigVariable("OPM_ALIGNMENT_TOLERANCE", (179).ToString());
                bmec_object1.DiscoverConnectionsEx();
                bmec_object1.UpdateConnections();
                bmec_object2.DiscoverConnectionsEx();
                bmec_object2.UpdateConnections();


                if (has_baowenceng1)
                {
                    try
                    {

                        JYX_ZYJC_CLR.PublicMethod.huiqie_replace_pipe_cone(bmec_object1, new_cone_surface1, new_baowen_cone_surface1);
                    }
                    catch { }

                }
                else
                {
                    try
                    {
                        //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(new_cone_surface1);

                        //ElementHolder new_cone_surface_temp2 = OPM_Public_Api.create_cone_surface(pipe_od2 * 2, line_start2, line_end2);
                        //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(new_cone_surface1);
                        //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(new_cone_surface_temp2);
                        //new_cone_surface1 = JYX_ZYJC_CLR.PublicMethod.solid_subtract(new_cone_surface1, new_cone_surface_temp2);


                        JYX_ZYJC_CLR.PublicMethod.huiqie_replace_pipe_cone(bmec_object1, new_cone_surface1);
                    }
                    catch { }

                }

                if (has_baowenceng2)
                {
                    try
                    {

                        JYX_ZYJC_CLR.PublicMethod.huiqie_replace_pipe_cone(bmec_object2, new_cone_surface2, new_baowen_cone_surface2);
                    }
                    catch { }

                }
                else
                {
                    try
                    {

                        JYX_ZYJC_CLR.PublicMethod.huiqie_replace_pipe_cone(bmec_object2, new_cone_surface2);
                        //JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(new_cone_surface2);
                    }
                    catch { }

                }

                double[] mianjis = new double[2];
                ulong cell_id1 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object1);
                ulong cell_id2 = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmec_object2);
                mianjis = JYX_ZYJC_CLR.PublicMethod.huiqie(cell_id1, cell_id2, new_faster_point1, new_faster_point2,
                    new_nearest_point1, new_nearest_point2, interset, dn, has_baowenceng1, has_baowenceng2);
                BMECInstanceManager.SetConfigVariable("OPM_ALIGNMENT_TOLERANCE", text);

                //double tiji1 = mianjis[0] * bmec_object1.Instance["WALL_THICKNESS"].DoubleValue / 1000;
                //double tiji2 = mianjis[1] * bmec_object2.Instance["WALL_THICKNESS"].DoubleValue / 1000;
                ////double midu1 = OPM_Public_Api.get_midu("MIDU_VALUE_MAP", "_" + bmec_object1.Instance["MATERIAL"].StringValue);
                ////double midu2 =OPM_Public_Api.get_midu("MIDU_VALUE_MAP", "_" + bmec_object2.Instance["MATERIAL"].StringValue);
                //IECInstance ec_instance1 = bmec_object1.Instance;
                //IECInstance ec_instance2 = bmec_object2.Instance;

                //ec_instance1["tiji"].StringValue = tiji1.ToString();
                // ec_instance2["tiji"].StringValue = tiji2.ToString();
                // ec_instance1["caizhi"].StringValue = "_" + bmec_object1.Instance["MATERIAL"].StringValue;
                // ec_instance2["caizhi"].StringValue = "_" + bmec_object2.Instance["MATERIAL"].StringValue;
                //ec_instance1["zhongliang"].StringValue = Convert.ToString(tiji1 * midu1);
                //ec_instance2["zhongliang"].StringValue = Convert.ToString(tiji2 * midu2);
                //ec_instance1["DRY_WEIGHT"].StringValue = Convert.ToString(tiji1 * midu1);
                //ec_instance2["DRY_WEIGHT"].StringValue = Convert.ToString(tiji2 * midu2);
                //var model_refP = (IntPtr)app.ActiveModelReference.MdlModelRefP();


                //bmec_object1.Instance.
                //XmlInstanceUpdate.UpdateInstance(ec_instance1, model_refP, false);
                // XmlInstanceUpdate.UpdateInstance(ec_instance2, model_refP, false);
                return true;
            }
        }
        public static double get_midu(string value_map, string caizhi)
        {
            IECClass iec_class = BMECInstanceManager.Instance.Schema.GetClass(value_map);
            Hashtable hashtable = new Hashtable();
            IEnumerator<IECProperty> enumerator = iec_class.GetEnumerator();
            double midu = 0;
            while (enumerator.MoveNext())
            {
                hashtable.Add(enumerator.Current.Name, enumerator.Current.DisplayLabel);
            }
            midu = Convert.ToDouble(hashtable[caizhi]);
            return midu;
        }
        public static ElementHolder create_cone_surface(double d, BIM.Point3d start_point, BIM.Point3d end_point)
        {

            d *= uor_per_master;
            double length = app.Point3dDistance(start_point, end_point) * uor_per_master;
            BG.DVector3d dir = BG.DVector3d.FromXYZ(end_point.X - start_point.X, end_point.Y - start_point.Y, end_point.Z - start_point.Z);
            BG.DPoint3d location = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point);
            Cylindrical cylindrical = new Cylindrical(d, d, length, 0, location, dir);

            List<ElementHolder> element_holder_list = cylindrical.CreateSimpleCylinderElements(false);
            element_holder_list[0].SetGraphicsType(BMdGraphicsType.bmdGraphicsTypeInsulation, false);

            //ulong id = JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder_list[0]);
            return element_holder_list[0];
        }
        public static ElementHolder create_cone_surface(double d, BG.DPoint3d start_point, BG.DPoint3d end_point)
        {
            d *= OPM_Public_Api.uor_per_master;
            double length = start_point.Distance(end_point);
            BG.DVector3d direction = BG.DVector3d.FromXYZ(end_point.X - start_point.X, end_point.Y - start_point.Y, end_point.Z - start_point.Z);
            List<ElementHolder> elementholder_list = new Cylindrical(d, d, length, 0.0, start_point, direction).CreateSimpleCylinderElements(false);
            elementholder_list[0].SetGraphicsType(BMdGraphicsType.bmdGraphicsTypeInsulation, false);
            return elementholder_list[0];
        }

        /// <summary>
        /// TODO 获取元素的类型
        /// </summary>
        public static string getBMECObjectTypeName(Element element)
        {
            BMApplication aa = new BMApplication();

            string elementTypeName = string.Empty;
            BMECObject ec_object = null;
            ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
            if (ec_object != null && ec_object.Instance != null)
            {
                elementTypeName = ec_object.ClassName;
            }
            else
            {
                elementTypeName = element.TypeName;
            }
            return elementTypeName;
        }

        public static ElementHolder create_elliptic_cylinder_solid(BG.DPoint3d center_dpoint, double xR, double yR, BG.DMatrix3d ma, double start, double sweep, double height)
        {
            BMApplication bm_app = new BMApplication();
            List<ElementHolder> element_holder_list = new List<ElementHolder>();

            ArcElement arc_temp = new ArcElement(Session.Instance.GetActiveDgnModel(), null, center_dpoint, xR, yR, ma, start, sweep);

            ElementHolder arc_element_holder = bm_app.CreateArcElement(center_dpoint, xR, yR, ma, start, sweep);
            element_holder_list.Add(arc_element_holder);
            BG.DPoint3d start_dpt, end_dpt;
            arc_temp.GetCurveVector().GetStartEnd(out start_dpt, out end_dpt);
            ElementHolder line_element_holder = bm_app.CreateLineElement(start_dpt, end_dpt);
            element_holder_list.Add(line_element_holder);

            ElementHolder complexshapeelement = bm_app.CreateComplexShapeElement(element_holder_list.ToArray(), 1);

            BMGatheredElement bmgatheredelement = bm_app.CreateGatheredElement();
            bmgatheredelement.Cursor.Initialize();
            bmgatheredelement.Cursor.Location = center_dpoint;
            bmgatheredelement.AddProjectedSurfaceWithFaceRemoval(complexshapeelement, null, BMdGraphicsType.bmdGraphicsTypeEdge, BMdComponentType.bmdComponentTypeBody, height, BG.DPoint3d.FromXYZ(0.0, 0.0, 1.0));

            return bmgatheredelement.Create();
        }

        public static ElementHolder create_prism_solid(double length, double height, int numSides, BG.DPoint3d location, BG.DVector3d direction)
        {
            BMApplication bm_app = new BMApplication();
            List<ElementHolder> list = new List<ElementHolder>();
            BG.DPoint3d[] array = new BG.DPoint3d[numSides];
            double a = Math.PI / (double)numSides;
            double r = length / (2.0 * Math.Sin(a));

            double num = 2 * Math.PI / (double)numSides;

            BG.Angle theta = default(BG.Angle);
            theta.Radians = num / 2.0;
            for (int i = 0; i < numSides; i++)
            {
                theta.Radians += num;

                array[i] = Utils.FromCylindrical(r, theta, height);
            }

            ElementHolder item2 = bm_app.CreateShapeElement(array, -1);
            ElementHolder path = bm_app.CreateLineElement(BG.DPoint3d.FromXYZ(0, 0, -height), BG.DPoint3d.Zero);
            list.Add(JYX_ZYJC_CLR.PublicMethod.solid_sweepbodywire(item2, path));

            return bm_app.TranslateElement(bm_app.TranslateElement(list, BG.DVector3d.FromXYZ(0.0, 0.0, 1.0), BG.DPoint3d.Zero), direction, location)[0];
        }
        public static ElementHolder create_bsplinecurve(BG.DPoint3d[] ptArr)
        {
            DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

            BG.MSBsplineCurve msBsplineCurve = BG.MSBsplineCurve.CreateFromPoles(ptArr, null, null, 3, false, true);
            BG.CurvePrimitive curvePri = BG.CurvePrimitive.CreateBsplineCurve(msBsplineCurve);
            Element ele = DraftingElementSchema.ToElement(dgnModel, curvePri, null);
            return JYX_ZYJC_CLR.PublicMethod.convertToElementHolder(ele.ElementHandle);
        }

        public static ElementHolder create_rectangular_surface(double widthInMM, double DepthInMM, double LengthInMM, BG.DPoint3d offsetVec, BG.DTransform3d dtran)
        {
            widthInMM *= 10.0;
            DepthInMM *= 10.0;
            LengthInMM *= 10.0;
            offsetVec.ScaleInPlace(10.0);
            BMApplication bmapp = new BMApplication();
            BMechEndData bMechEndData = new BMechEndData();
            bMechEndData.WidthInMM = widthInMM;
            bMechEndData.DepthInMM = DepthInMM;
            bMechEndData.Shape = BMdEndShape.bmdRectangular;
            BMGatheredElement bmgatheredelemnt = bmapp.CreateGatheredElement();
            bmgatheredelemnt.AddSurface(bMechEndData, bMechEndData, offsetVec, BMdGraphicsType.bmdGraphicsTypeEdge, BMdComponentType.bmdComponentTypeExtension, LengthInMM);
            bmgatheredelemnt.Cursor.Initialize();
            ElementHolder elementholder = bmgatheredelemnt.Create();
            elementholder.Transform(dtran, false);
            return elementholder;
        }

        private IECInstance elbowOrBendInstance;//应该从选择到创建都为同一个实例
        private BMECObject elbowOrBendECObject = new BMECObject();
        //private int chuangjianleixing = 1;
        /// <summary>
        /// 根据选中的两个管道创建自动生成的弯头
        /// </summary>
        /// <param name="bmec_object1"></param>
        /// <param name="bmec_object2"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public List<BMECObject> create_elbow1(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage, elbowOrBend elbowBend, ref ECInstanceList ecinList)
        {
            initelbowDic();
            List<BMECObject> elbows = new List<BMECObject>();
            bool isSingleElbow = true;
            int unbrokenElbow = 0;
            double brokenElbowDgree = 0.0f;
            double elbowDegree = 0.0f;
            double cankaoCenterToMainPort = 0.0f;
            errorMessage = string.Empty;
            //ecinList[0].Clone();
            eCInstanceList = ecinList;

            Bentley.GeometryNET.DPoint3d[] line1Point = GroupPipeTool.GetTowPortPoint(bmec_object1);
            Bentley.GeometryNET.DPoint3d[] line2Point = GroupPipeTool.GetTowPortPoint(bmec_object2);

            BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];//第一根管道的端点
            BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];//第二根管道的端点
            line1_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(0).LocationInUors);
            line1_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(1).LocationInUors);
            line2_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(0).LocationInUors);
            line2_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(1).LocationInUors);

            BIM.Ray3d ray3d1 = app.Ray3dFromPoint3dStartEnd(line1_end_pts[0], line1_end_pts[1]);//TODO 根据两个管道的端点构造两条射线
            BIM.Ray3d ray3d2 = app.Ray3dFromPoint3dStartEnd(line2_end_pts[0], line2_end_pts[1]);

            BIM.Point3d intersect_point1, intersect_point2;
            intersect_point1 = intersect_point2 = app.Point3dZero();
            double fraction1, fraction2;
            fraction1 = fraction2 = 0.0;
            bool reuslt = app.Ray3dRay3dClosestApproach(ray3d1, ray3d2, ref intersect_point1, ref fraction1, ref intersect_point2, ref fraction2);//两条射线是否有交点

            if (!reuslt)
            {
                errorMessage = "选中的管道不在一个平面上";
                return null;
            }
            BIM.Point3d intersect_point = intersect_point1;
            BG.DPoint3d intersect = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(intersect_point);
            BIM.LineElement line = app.CreateLineElement2(null, app.Point3dZero(), intersect_point);

            //m_myForm.textBox_guandao_guid2.Text = bmec_object2.Instance["GUID"].StringValue;
            BIM.Point3d nearest_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            BIM.Point3d faster_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            if (app.Point3dEqual(nearest_point1, faster_point1))
            {
                nearest_point1 = line1_end_pts[0];
                faster_point1 = line1_end_pts[1];
            }

            BIM.Point3d nearest_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            BIM.Point3d faster_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            if (app.Point3dEqual(nearest_point2, faster_point2))
            {
                nearest_point2 = line2_end_pts[0];
                faster_point2 = line2_end_pts[1];
            }
            BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);//管道一方向向量
            BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);//管道二方向向量
            double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
            if (JYX_ZYJC_Jianmo_Youhua_CS.MyPublic_Api.is_double_xiangdeng(angle, 180))
            {
                errorMessage = "两个管道互相平行!";
                return null;
            }

            #region MyRegion

            //double t = Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
            //double t2 = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
            //double wanqu_banjing = 0.0;
            //double nominalDiameter = bmec_object1.GetDoubleValueInMM("NOMINAL_DIAMETER");
            //double d = 0.0;
            //double d1, d2;
            //d1 = app.Point3dDistance(intersect_point, faster_point1);
            //d2 = app.Point3dDistance(intersect_point, faster_point2);
            //double ltb = 0.0;
            //double lab = 0.0;
            //double pipeLength1,pipeLength2;
            //double bihou = bmec_object1.Instance["WALL_THICKNESS"].DoubleValue;
            //string caizhi = bmec_object1.Instance["MATERIAL"].StringValue;
            //double new_angle = angle;
            //double insulation_thickness = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
            //string insulation = bmec_object1.Instance["INSULATION"].StringValue;
            //double center_to_outlet_end = 0.0;//应该根据半径变化
            //double center_to_run_end = 0.0;//应该根据半径变化
            //BIM.Point3d start_point1;
            //BIM.Point3d start_point2;
            //if (chuangjianleixing == 1)
            //{
            //    start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
            //    start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));
            //}
            //else
            //{
            //    start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d + ltb) / d1));
            //    start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d + lab) / d2));
            //}
            //if (chuangjianleixing == 1)
            //{
            //    elbowecclassname = getElbowECClassName(m_myForm.comboBox_elbow_radius.Text, m_myForm.comboBox_elbow_angle.Text);
            //    elbowOrBend = BMECInstanceManager.Instance.CreateECInstance(elbowecclassname, true);

            //    ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
            //    isp.FillCurrentPreferences(elbowOrBend, null);
            //    elbowOrBend["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
            //    ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBend, true);
            //    if (eCInstanceList.Count == 0)
            //    {
            //        errorMessage = "当前Specification为配置弯管。";
            //        return false;
            //    }
            //    elbowOrBend = eCInstanceList[0];
            //    double majorRadius = elbowOrBend["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
            //    //ecinstance["ANGLE"].DoubleValue = angle;
            //    //ecinstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = center_to_outlet_end;
            //    //ecinstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = center_to_run_end;
            //    //ecinstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
            //    //ecinstance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
            //    //ecinstance["INSULATION"].StringValue = insulation;
            //    wanqu_banjing = majorRadius;
            //    d = wanqu_banjing * Math.Tan((angle / 2.0) * Math.PI / 360.0);
            //    ltb = lab = 0.0;
            //    BMECObject m_ecObject = new BMECObject(elbowOrBend);
            //    if (m_ecObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
            //    {
            //        ltb = m_ecObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
            //        lab = m_ecObject.Ports[1].Instance["SOCKET_DEPTH"].DoubleValue;
            //    }
            //    else if (m_ecObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
            //    {
            //        ltb = m_ecObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
            //        lab = m_ecObject.Ports[1].Instance["THREADED_LENGTH"].DoubleValue;
            //    }
            //    pipeLength1 = d1 - d + ltb;
            //    pipeLength2 = d2 - d + lab;
            //    center_to_outlet_end = d;
            //    center_to_run_end = d;
            //}
            //else
            //{
            //    wanqu_banjing = Convert.ToDouble(m_myForm.textBox_radius.Text.Trim()) * Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
            //    d = wanqu_banjing * Math.Tan((angle / 2.0) * Math.PI / 360.0);
            //    ltb = Convert.ToDouble(m_myForm.textBox_lengthToBend.Text);
            //    lab = Convert.ToDouble(m_myForm.textBox_lengthAfterBend.Text);
            //    pipeLength1 = d1 - d - ltb;
            //    pipeLength2 = d2 - d - lab;
            //}
            //d = wanqu_banjing;
            //bmec_object1.Instance["LENGTH"].DoubleValue = pipeLength1;
            //bmec_object2.Instance["LENGTH"].DoubleValue = pipeLength2;
            //if (d1 - d < JYX_ZYJC_Jianmo_Youhua_CS.Parameters_Setting.huitu_tolerance || d2 - d < JYX_ZYJC_Jianmo_Youhua_CS.Parameters_Setting.huitu_tolerance)
            //{
            //    errorMessage="角度过小或弯曲半径过大，无法布置，请调整弯曲半径等参数";
            //    return false;
            //}
            //BIM.Point3d start_point1;
            //BIM.Point3d start_point2;
            //start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
            //start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));
            //double elbow_dn = Convert.ToDouble(m_myForm.textBox_elbow_dn.Text.Trim());
            //double bihou = Convert.ToDouble(m_myForm.textBox_elbow_bihou.Text.Trim());
            //string caizhi = m_myForm.comboBox_caizhi.SelectedValue.ToString();
            //double new_angle = Convert.ToDouble(m_myForm.textBox_elbow_wanqu_jiaodu.Text);
            //double insulation_thickness = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
            //string insulation= bmec_object1.Instance["INSULATION"].StringValue;
            //double center_to_outlet_end = Convert.ToDouble(m_myForm.xiaoshuBox_elbow_wanqu_banjing.Text);//应该根据半径变化
            //double center_to_run_end= Convert.ToDouble(m_myForm.xiaoshuBox_elbow_wanqu_banjing.Text);//应该根据半径变化
            //TODO 改类型
            //BMECObject ec_object = null;
            //ec_object = JYX_ZYJC_Jianmo_Youhua_CS.OPM_Public_Api.create_elbow(elbow_dn, new_angle, center_to_outlet_end, center_to_run_end, insulation_thickness, insulation, out errorMessage);
            //double lengthToBend;
            //double lengthAfterBend;
            //double radiusFactor;
            //switch (chuangjianleixing)
            //{
            //    case 1:
            //        elbowOrBendECObject = createElbow(elbowecclassname, nominalDiameter, new_angle, center_to_outlet_end, center_to_run_end, insulation_thickness, insulation, out errorMessage);
            //        break;
            //    case 2:
            //        elbowecclassname = "PIPE_BEND";
            //        lengthToBend = Convert.ToDouble(m_myForm.textBox_lengthToBend.Text);
            //        lengthAfterBend = Convert.ToDouble(m_myForm.textBox_lengthAfterBend.Text);
            //        radiusFactor = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
            //        elbowOrBendECObject = createBend(elbowecclassname, nominalDiameter, new_angle, center_to_outlet_end, center_to_run_end, insulation_thickness, insulation, out errorMessage, lengthToBend, lengthAfterBend, radiusFactor);
            //        break;
            //    case 3:
            //        elbowecclassname = "MITERED_PIPE_BEND";
            //        lengthToBend = Convert.ToDouble(m_myForm.textBox_lengthToBend.Text);
            //        lengthAfterBend = Convert.ToDouble(m_myForm.textBox_lengthAfterBend.Text);
            //        radiusFactor = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
            //        int xiamiwanjieshu = Convert.ToInt32(m_myForm.textBox_xiamiwan_jieshu.Text);
            //        elbowOrBendECObject = createBend(elbowecclassname, nominalDiameter, new_angle, center_to_outlet_end, center_to_run_end, insulation_thickness, insulation, out errorMessage, lengthToBend, lengthAfterBend, radiusFactor, xiamiwanjieshu);
            //        break;
            //    default:
            //        break;
            //}
            #endregion

            string elbowOrBendECClassName = "";
            double nominalDiameter = bmec_object1.GetDoubleValueInMM("NOMINAL_DIAMETER");
            double insulationThickness = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
            string insulation = bmec_object1.Instance["INSULATION"].StringValue;
            double wallThickness = bmec_object1.Instance["WALL_THICKNESS"].DoubleValue;
            string material = bmec_object1.Instance["MATERIAL"].StringValue;
            double dn1 = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double dn2 = bmec_object2.Instance["NOMINAL_DIAMETER"].DoubleValue;

            double centerToMainPort = 0.0;
            double centerToRunPort = 0.0;
            double lengthToBend = 0.0;
            double lengthAfterBend = 0.0;
            double pipeLength1 = 0.0;
            double pipeLength2 = 0.0;
            double d1, d2;
            d1 = app.Point3dDistance(intersect_point, faster_point1);
            d2 = app.Point3dDistance(intersect_point, faster_point2);
            double radius = 0.0;

            if (elbowBend.elbowOrBendName == "Elbow")//elbow
            {
                double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                if (!elbowBend.isBzQgWt)
                {
                    if (Math.Abs(cankaoAngle - angle) > 1)
                    {
                        errorMessage = "当前条件下，没有找到对应角度的弯头！";
                        //elbows = null;
                        return null;
                    }
                }
                else
                {
                    if (angle <= cankaoAngle)
                    {
                        isSingleElbow = true;
                    }
                    else
                    {
                        unbrokenElbow = (int)(angle / cankaoAngle);
                        brokenElbowDgree = angle % cankaoAngle;
                        if (unbrokenElbow > 2 || (brokenElbowDgree > 1e-1 && unbrokenElbow > 1))
                        {
                            errorMessage = "当前角度下需要连接的弯头数量大于2，请使用其它角度弯头连接或更改连接方式！";
                            //elbows = null;
                            return null;
                        }
                        elbowDegree = cankaoAngle;
                        isSingleElbow = false;
                    }
                }
                if (elbowBend.elbowRadius != "虾米弯")
                {
                    elbowOrBendECClassName = getElbowECClassName(elbowBend.elbowRadius, elbowBend.elbowAngle);
                }
                else if (elbowBend.elbowRadius == "虾米弯")
                {
                    elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX";
                }
                elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
                    return null;
                }
                if (elbowBend.typeNumber == 0)
                {
                    if (elbowBend.elbowRadius == "虾米弯")
                    {
                        ISpecProcessor isp = api.SpecProcessor;
                        isp.FillCurrentPreferences(elbowOrBendInstance, null);
                        elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                        //double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                        Hashtable whereClauseList = new Hashtable();
                        whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                        whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                        whereClauseList.Add("wanqu_jiaodu", cankaoAngle);
                        eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                        ecinList = eCInstanceList;
                    }
                    else
                    {
                        ISpecProcessor isp = api.SpecProcessor;
                        isp.FillCurrentPreferences(elbowOrBendInstance, null);
                        elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                        eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                        ecinList = eCInstanceList;
                    }
                }

                if (eCInstanceList.Count == 0)
                {
                    errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
                    return null;
                }
                BMECObject origanBmec = new BMECObject(eCInstanceList[0]);
                BMECObject copyBmec = new BMECObject();
                copyBmec.Copy(origanBmec);
                elbowOrBendInstance = copyBmec.Instance;
                //elbowOrBendInstance = eCInstanceList[0];

                centerToMainPort = elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                cankaoCenterToMainPort = centerToMainPort;
                centerToRunPort = centerToMainPort;

                BMECObject tempObject = new BMECObject(elbowOrBendInstance);

                if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["SOCKET_DEPTH"].DoubleValue;
                }
                else if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["THREADED_LENGTH"].DoubleValue;
                }
                double anglera = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                double radus = BG.Angle.DegreesToRadians(anglera);
                radius = (centerToMainPort - lengthToBend) / Math.Tan(radus / 2);
                double centerToPort = radius * Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
                centerToMainPort = centerToPort + lengthToBend;
                centerToRunPort = centerToMainPort;

                if (elbowBend.elbowRadius == "虾米弯")
                {
                    //elbowOrBendInstance["NUM_PIECES"].IntValue = elbowBend.pitchNumber;

                    radius = elbowOrBendInstance["wanqu_banjing"].DoubleValue;

                    //centerToMainPort = radius;
                    //centerToRunPort = centerToMainPort;
                    double centerToPort1 = radius * Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
                    centerToMainPort = centerToPort1 + lengthToBend;
                    centerToRunPort = centerToPort1 + lengthAfterBend;
                    //elbowOrBendInstance["jieshu"].IntValue = elbowBend.pitchNumber;
                    //elbowOrBendInstance["wanqu_banjing"].DoubleValue = radius;
                    elbowOrBendInstance["wanqu_jiaodu"].DoubleValue = angle;

                    pipeLength1 = d1 - centerToMainPort + lengthToBend;
                    pipeLength2 = d2 - centerToRunPort + lengthAfterBend;
                    elbowOrBendInstance["caizhi"].StringValue = "";
                    elbowOrBendInstance["bihou"].DoubleValue = wallThickness;
                }
                else
                {
                    pipeLength1 = d1 - centerToMainPort + lengthToBend;
                    pipeLength2 = d2 - centerToRunPort + lengthAfterBend;
                }

                //create elbow
                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                //DESIGN_LENGTH_CENTER_TO_RUN_END
                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;
                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
            }
            else//bend
            {
                double radiusFactor = elbowBend.bendRadiusRatio;
                radius = elbowBend.bendRadius;
                lengthToBend = elbowBend.bendFrontLong;
                lengthAfterBend = elbowBend.bendAfterLong;
                //m_myForm.textBox_radius.Text = radius.ToString();
                elbowOrBendECClassName = "PIPE_BEND";
                elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
                    return null;
                }
                if (elbowBend.typeNumber == 0)
                {
                    ISpecProcessor isp = api.SpecProcessor;
                    isp.FillCurrentPreferences(elbowOrBendInstance, null);
                    elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                    eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                    ecinList = eCInstanceList;
                }
                if (eCInstanceList.Count == 0)
                {
                    errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
                    return null;
                }
                elbowOrBendInstance = eCInstanceList[0];

                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
                elbowOrBendInstance["LENGTH_TO_BEND"].DoubleValue = lengthToBend;
                elbowOrBendInstance["LENGTH_AFTER_BEND"].DoubleValue = lengthAfterBend;
                elbowOrBendInstance["BEND_POINT_RADIUS"].DoubleValue = radiusFactor;
                elbowOrBendInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
                elbowOrBendInstance["MATERIAL"].StringValue = material;
                double tanA = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
                centerToMainPort = radius / tanA;
                centerToRunPort = centerToMainPort;

                pipeLength1 = d1 - centerToMainPort - lengthToBend;
                pipeLength2 = d2 - centerToRunPort - lengthAfterBend;
            }
            BMECObject bmec = new BMECObject(elbowOrBendInstance);
            elbowOrBendECObject.Copy(bmec);

            //当管道过短时，无法生成
            if (pipeLength1 <= 0 || pipeLength2 <= 0)
            {
                errorMessage = "所连接的管道过短，无法生成弯头";
                return null;
            }
            //TODO 随便写的
            //double temp1 = elbowOrBendECObject.GetDoubleValueInMM("OD");
            //double temp2 = elbowOrBendECObject.GetDoubleValueInMM("wanqu_banjing");
            //double temp3 = elbowOrBendECObject.GetDoubleValueInMM("wanqu_jiaodu");
            //double temp4 = elbowOrBendECObject.GetDoubleValueInMM("NOMINAL_DIAMETER");
            //double temp5 = elbowOrBendECObject.GetDoubleValueInMM("OUTSIDE_DIAMETER");

            //fill form
            //m_myForm.textBox_elbow_dn.Text = nominalDiameter.ToString();
            //m_myForm.textBox_elbow_bihou.Text = wallThickness.ToString();
            //m_myForm.textBox_elbow_wanqu_jiaodu.Text = Math.Round(angle, 2).ToString();
            //m_myForm.xiaoshuBox_elbow_wanqu_banjing.Text = radius.ToString();
            //m_myForm.comboBox_caizhi.Text = material;

            bmec_object1.Instance["LENGTH"].DoubleValue = pipeLength1;
            bmec_object2.Instance["LENGTH"].DoubleValue = pipeLength2;

            BIM.Point3d start_point1;
            BIM.Point3d start_point2;
            start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
            start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));

            if (elbowOrBendECObject == null)
            {
                errorMessage = "无法通过该实例创建对象";
                return null;
            }
            BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);

            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, start_point1);
            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, start_point2, faster_point2);
            if (isSingleElbow)
            {
                try
                {
                    elbowOrBendECObject.Create();
                }
                catch (System.Exception)
                {
                    errorMessage = "Pipeline不存在，请打开Create Pipeline创建处理。";
                    return null;
                }
                //ec_object.LegacyGraphicsId
                JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, v1, dir2, v2, start_point1, start_point2);

                bmec_object1.DiscoverConnectionsEx();
                bmec_object1.UpdateConnections();

                bmec_object2.DiscoverConnectionsEx();
                bmec_object2.UpdateConnections();

                elbowOrBendECObject.UpdateConnections();
                elbowOrBendECObject.DiscoverConnectionsEx();

                elbows.Add(elbowOrBendECObject);
            }
            else
            {
                if (elbowBend.isBzQgWt && elbowBend.isYgWt)
                {
                    if (!elbowBend.isLDQ)
                    {
                        double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                        elbowOrBendECObject.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        BG.DPoint3d guandaoJiaodian = intersect;//两管道交点

                        BG.DVector3d oldguandao1Vec = new BG.DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        BG.DVector3d oldguandao2Vec = new BG.DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        BG.DVector3d guandao1Vec;//管道一方向向量
                        BG.DPoint3d fasterPoint1CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1);
                        guandao1Vec = new BG.DVector3d(fasterPoint1CE, intersect);
                        BG.DVector3d guandao2Vec;//管道二方向向量
                        BG.DPoint3d fasterPoint2CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2);
                        guandao2Vec = new BG.DVector3d(intersect, fasterPoint2CE);

                        BG.DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        BG.DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        BG.DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);
                        BG.DVector3d xiangxinRadiusVec = faxiangliang.CrossProduct(guandao1VecNormal);
                        BG.DVector3d xiangxinRadiusVecNormal;
                        xiangxinRadiusVec.TryNormalize(out xiangxinRadiusVecNormal);
                        //向心半径向量
                        if (xiangxinRadiusVecNormal.DotProduct(guandao2VecNormal) < 0)
                        {
                            xiangxinRadiusVecNormal = -xiangxinRadiusVecNormal;
                        }
                        BG.DVector3d xiangxinRadiusVecL;//半径向量
                        double magnitude;
                        xiangxinRadiusVecNormal.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out xiangxinRadiusVecL, out magnitude);

                        //求管道外侧交点到管道外侧切点的分量
                        double elbowOutRadius = (radius + elbowOrBendECObject.Instance["OUTSIDE_DIAMETER"].DoubleValue / 2) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//弯头外半径
                        double lengthQiedianXian = elbowOutRadius * (1 - Math.Cos(cankaoAngle * Math.PI / 180.0));
                        BG.DVector3d lengthQiedianXianVec;
                        xiangxinRadiusVecNormal.TryScaleToLength(lengthQiedianXian, out lengthQiedianXianVec, out magnitude);

                        BG.Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);
                        double tempAngle = (pipeTopipeAngle.Degrees % 180) - 90.0;
                        double xianAngle = Math.Abs(pipeTopipeAngle.Degrees - 90.0);
                        double hengxiangLen = Math.Tan(xianAngle * Math.PI / 180.0) * lengthQiedianXian;
                        BG.DVector3d hengxiangVect;
                        guandao1VecNormal.TryScaleToLength(hengxiangLen, out hengxiangVect, out magnitude);

                        //求管道外侧交点
                        double pipe2Radius = bmec_object2.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        BG.DVector3d waiceShuXian;
                        xiangxinRadiusVecL.TryScaleToLength(pipe2Radius, out waiceShuXian, out magnitude);
                        BG.DVector3d waiceHengxian;
                        double waiceHengxianL = pipe2Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        BG.DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        BG.DPoint3d pipe2Qiedian;//管道外侧切点坐标
                        if (tempAngle < 0)
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec + hengxiangVect;
                        }
                        else
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec - hengxiangVect;
                        }
                        //求管道2起点
                        BG.DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }
                        BG.DVector3d pipe2XiangxinVecL;
                        pipe2XiangxinVec.TryScaleToLength(pipe2Radius, out pipe2XiangxinVecL, out magnitude);

                        BG.DPoint3d pipe2StarPoint = pipe2Qiedian + pipe2XiangxinVecL;//管道2起点

                        double chuiXianToRadius = elbowOutRadius * Math.Sin(cankaoAngle * Math.PI / 180.0);
                        BG.DVector3d chuiXianToRadiusV;
                        (-guandao1VecNormal).TryScaleToLength(chuiXianToRadius, out chuiXianToRadiusV, out magnitude);
                        double xianOnRadius = elbowOutRadius * Math.Cos(cankaoAngle * Math.PI / 180.0);
                        BG.DVector3d xianOnRadiusV;
                        xiangxinRadiusVecNormal.TryScaleToLength(xianOnRadius, out xianOnRadiusV, out magnitude);

                        BG.DPoint3d elbowCenter = pipe2Qiedian + chuiXianToRadiusV + xianOnRadiusV;//弯头圆心位置
                        BG.DPoint3d pipe1EndPoint = elbowCenter - xiangxinRadiusVecL;//管道1终点

                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1EndPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1EndPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1EndPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1EndPoint).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StarPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StarPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StarPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StarPoint, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                            //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return null;
                        }
                        else
                        {
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        }

                        try
                        {
                            elbowOrBendECObject.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return null;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = guandao1VecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = guandao1VecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = guandao1VecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = xiangxinRadiusVecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = xiangxinRadiusVecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = xiangxinRadiusVecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

                        BG.DVector3d tempVect;
                        BG.DPoint3d CEspoint2 = elbow.GetPointOnArcWithAngle(elbowDegree, elbowCenter, pipe1EndPoint, guandao1VecNormal, out tempVect);
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = CEspoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = CEspoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = CEspoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, tv1, dir2, tv2, currentStartPoint1, spoint2);

                        bmec_object1.DiscoverConnectionsEx();
                        bmec_object1.UpdateConnections();

                        bmec_object2.DiscoverConnectionsEx();
                        bmec_object2.UpdateConnections();

                        elbowOrBendECObject.UpdateConnections();
                        elbowOrBendECObject.DiscoverConnectionsEx();
                        elbows.Add(elbowOrBendECObject);
                    }
                    else if (elbowBend.isLDQ)
                    {
                        double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                        elbowOrBendECObject.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        BG.DVector3d oldguandao1Vec = new BG.DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        BG.DVector3d oldguandao2Vec = new BG.DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        BG.DVector3d guandao1Vec;//管道一方向向量
                        BG.DPoint3d fasterPoint1CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1);
                        guandao1Vec = new BG.DVector3d(fasterPoint1CE, intersect);
                        BG.DVector3d guandao2Vec;//管道二方向向量
                        BG.DPoint3d fasterPoint2CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2);
                        guandao2Vec = new BG.DVector3d(intersect, fasterPoint2CE);

                        BG.DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        BG.DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        BG.DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);

                        double magnitude;
                        //求管道外侧交点
                        BG.Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);//两管道夹角
                        double pipe1Radius = bmec_object1.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        BG.DVector3d pipe1XiangxinVec = faxiangliang.CrossProduct(guandao1VecNormal);//管道1外侧切点向心方向向量
                        if (pipe1XiangxinVec.DotProduct(guandao2VecNormal) < 0)
                        {
                            pipe1XiangxinVec = -pipe1XiangxinVec;
                        }

                        BG.DVector3d waiceShuXian;//管道1向心向量
                        pipe1XiangxinVec.TryScaleToLength(pipe1Radius, out waiceShuXian, out magnitude);
                        BG.DVector3d waiceHengxian;//管道方向
                        double waiceHengxianL = pipe1Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        BG.DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        BG.DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);//管道2外侧切点向心方向向量
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }

                        BG.DVector3d pipe2XiangxinVecL;//管道2向心向量
                        pipe2XiangxinVec.TryScaleToLength(pipe1Radius, out pipe2XiangxinVecL, out magnitude);

                        //弯头弦长
                        double elbowOutSideDia = elbowOrBendECObject.Instance["OUTSIDE_DIAMETER"].DoubleValue;//弯头管径
                        double elbowOutSideRadius = (elbowOutSideDia / 2 + radius) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        double elbowXianchang = elbowOutSideRadius * Math.Sin(cankaoAngle / 2 * Math.PI / 180.0);//弯头角度对应的弦长的一半

                        //两管道外侧切点
                        double pipe1QieDianL = elbowXianchang / Math.Sin(waiceAngle * Math.PI / 180.0);//切点据外侧交点距离
                        BG.DVector3d pipe1QieDianVec;
                        guandao1VecNormal.TryScaleToLength(pipe1QieDianL, out pipe1QieDianVec, out magnitude);
                        BG.DPoint3d pipe1Qiedian = guandaoWaiceJiaodian - pipe1QieDianVec;//管道1的切点

                        BG.DVector3d pipe2QieDianVec;
                        guandao2VecNormal.TryScaleToLength(pipe1QieDianL, out pipe2QieDianVec, out magnitude);
                        BG.DPoint3d pipe2Qiedian = guandaoWaiceJiaodian + pipe2QieDianVec;//管道2的切点

                        //两管道靠近弯头的端点
                        BG.DPoint3d pipe1StartPointCE = pipe1Qiedian + waiceShuXian;//管道1端点
                        BG.DPoint3d pipe2StartPointCE = pipe2Qiedian + pipe2XiangxinVecL;//管道2端点

                        //弯头圆心位置
                        BG.DVector3d pingfenxianVec = waiceShuXian - waiceHengxian;//两管道角平分线向量
                        double xianToCenterLen = elbowOutSideRadius * Math.Cos(cankaoAngle / 2 * Math.PI / 180.0);//圆心到弦上距离
                        double xianToWaiJiaodianLen = pipe1QieDianL * Math.Cos(waiceAngle * Math.PI / 180.0);//弦外到外侧交点距离
                        BG.DVector3d outsideIntersectVecToCenter;//外侧交点到弯头圆心向量；
                        pingfenxianVec.TryScaleToLength(xianToCenterLen + xianToWaiJiaodianLen, out outsideIntersectVecToCenter, out magnitude);
                        BG.DPoint3d centerPoint = guandaoWaiceJiaodian + outsideIntersectVecToCenter;//弯头圆心

                        //弯头两端点及其方向向量
                        BG.DPoint3d elbowPoint1, elbowPoint2;
                        BG.DVector3d point1Vec, point2Vec;
                        BG.DVector3d point1TempVec, point2TmepVec;
                        point1TempVec = new BG.DVector3d(centerPoint, pipe1Qiedian);
                        point2TmepVec = new BG.DVector3d(centerPoint, pipe2Qiedian);
                        point1TempVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point1TempVec, out magnitude);
                        point2TmepVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point2TmepVec, out magnitude);

                        elbowPoint1 = centerPoint + point1TempVec;
                        elbowPoint2 = centerPoint + point2TmepVec;

                        point1Vec = faxiangliang.CrossProduct(point1TempVec);
                        if (point1Vec.DotProduct(guandao1VecNormal) < 0)
                        {
                            point1Vec = -point1Vec;
                        }
                        point2Vec = faxiangliang.CrossProduct(point2TmepVec);
                        if (point2Vec.DotProduct(guandao2VecNormal) < 0)
                        {
                            point2Vec = -point2Vec;
                        }

                        //改变管道长度并移动管道
                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1StartPointCE).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StartPointCE, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                            //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return null;
                        }
                        else
                        {
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        }

                        //创建弯头并移动
                        try
                        {
                            elbowOrBendECObject.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return null;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = point1Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = point1Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = point1Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = point2Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = point2Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = point2Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = elbowPoint1.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = elbowPoint1.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = elbowPoint1.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = elbowPoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = elbowPoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = elbowPoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, tv1, dir2, tv2, spoint1, spoint2);

                        bmec_object1.DiscoverConnectionsEx();
                        bmec_object1.UpdateConnections();

                        bmec_object2.DiscoverConnectionsEx();
                        bmec_object2.UpdateConnections();

                        elbowOrBendECObject.UpdateConnections();
                        elbowOrBendECObject.DiscoverConnectionsEx();
                        elbows.Add(elbowOrBendECObject);
                    }
                }
                else
                {
                    BG.DVector3d pipe1Vect = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    BG.DVector3d pipe2Vect = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2));
                    BG.DVector3d pipe1VN;
                    pipe1Vect.TryNormalize(out pipe1VN);
                    BG.DVector3d pipe1PointToCircleCenterPointVect = pipe1VN.CrossProduct(pipe2Vect).CrossProduct(pipe1VN);
                    BG.DPoint3d radiusPoint;
                    if (pipe1PointToCircleCenterPointVect.DotProduct(pipe2Vect) < 0)
                    {
                        pipe1PointToCircleCenterPointVect = -pipe1PointToCircleCenterPointVect;
                    }
                    double magnitude;
                    BG.DVector3d radiusVector;
                    pipe1PointToCircleCenterPointVect.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out radiusVector, out magnitude);
                    radiusPoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1) + radiusVector;

                    List<BG.DPoint3d> points = new List<BG.DPoint3d>();//
                    List<BG.DVector3d> tangleVec = new List<BG.DVector3d>();
                    points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    tangleVec.Add(pipe1Vect);
                    for (int i = 0; i < unbrokenElbow; i++)
                    {
                        BG.DVector3d tangleVecAtResultPoint = new BG.DVector3d();
                        BG.DPoint3d tanglePoint = elbow.GetPointOnArcWithAngle(elbowDegree, radiusPoint, points.Last(), tangleVec.Last(), out tangleVecAtResultPoint);
                        points.Add(tanglePoint);
                        tangleVec.Add(tangleVecAtResultPoint);
                    }
                    if (brokenElbowDgree > 1e-1)
                    {
                        points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2));
                        tangleVec.Add(pipe2Vect);
                    }

                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        if (i < unbrokenElbow)
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowOrBendECObject);
                            elbow.Instance["ANGLE"].DoubleValue = elbowDegree;

                            double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                            double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                        else
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowOrBendECObject);
                            elbow.Instance["ANGLE"].DoubleValue = brokenElbowDgree;

                            double cankaoAngle = Convert.ToDouble(elbowBend.elbowAngle.Substring(0, 2));
                            double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(BG.Angle.DegreesToRadians(brokenElbowDgree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                    }

                    for (int i = 0; i < elbows.Count; i++)
                    {
                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = tangleVec[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = tangleVec[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = tangleVec[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = tangleVec[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = tangleVec[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = tangleVec[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = points[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = points[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = points[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = points[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = points[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = points[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbows[i], dir1, tv1, dir2, tv2, spoint1, spoint2);
                    }
                    bmec_object1.DiscoverConnectionsEx();
                    bmec_object1.UpdateConnections();

                    bmec_object2.DiscoverConnectionsEx();
                    bmec_object2.UpdateConnections();

                    foreach (BMECObject bmecE in elbows)
                    {
                        bmecE.DiscoverConnectionsEx();
                        bmecE.UpdateConnections();
                    }
                }
            }

            return elbows;
        }
        private ECInstanceList eCInstanceList = new ECInstanceList();
        private Dictionary<string, string> elbowradiusdic = new Dictionary<string, string>();
        private Dictionary<string, string> elbowangledic = new Dictionary<string, string>();
        private string getElbowECClassName(string radius, string angle)
        {
            string elbowecclassname = "";
            //特殊的两个
            //if (angle.Contains("30度弯头") && radius.Contains("长半径")) return "PIPE_ELBOW_30_DEGREE_LONG_RADIUS";
            //if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";

            //if (radius.Contains("长半径"))
            //{
            //    elbowecclassname = elbowradiusdic[radius] + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + "PIPE_ELBOW";
            //}
            //else
            //{
            //    elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];
            //}
            if (angle.Contains("15度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_15_DEGREE";
            if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";
            if (angle.Contains("45度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_45_DEGREE_PIPE_ELBOW";
            if (angle.Contains("60度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_60_DEGREE_PIPE_ELBOW";
            if (angle.Contains("90度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_90_DEGREE_PIPE_ELBOW";

            if (radius.Contains("普通弯头")) return "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE";

            elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];

            return elbowecclassname;
        }
        //TODO
        //改了原本的ECClass类中的命名
        //PIPE_ELBOW_30_DEGREE_LONG_RADIUS  ---
        //PIPE_ELBOW_SHORT_RADIUS_30_DEGREE ---
        private void initelbowDic()
        {
            elbowradiusdic.Clear();
            elbowangledic.Clear();
            elbowradiusdic.Add("长半径弯头", "LONG_RADIUS");
            elbowradiusdic.Add("短半径弯头", "SHORT_RADIUS");
            elbowradiusdic.Add("内外丝弯头", "INNER_AND_OUTER_WIRE_ELBOWS");
            elbowradiusdic.Add("1.5倍弯曲半径弯头", "1_POINT_5R");
            elbowradiusdic.Add("2倍弯曲半径弯头", "2R");
            elbowradiusdic.Add("2.5倍弯曲半径弯头", "2_POINT_5R");
            elbowradiusdic.Add("3倍弯曲半径弯头", "3R");
            elbowradiusdic.Add("4倍弯曲半径弯头", "4R");
            elbowangledic.Add("15度弯头", "15");
            elbowangledic.Add("22.5度弯头", "22.5");
            elbowangledic.Add("30度弯头", "30");
            elbowangledic.Add("45度弯头", "45");
            elbowangledic.Add("60度弯头", "60");
            elbowangledic.Add("90度弯头", "90");
        }

        public static string repicePipeLine(string pipeLine)
        {
            string pipeNumber = pipeLine;
            //pipeLine = pipeLine.Replace("_bs_", @"\");
            //pipeLine = pipeLine.Replace("_fs_", @"/");
            //pipeLine = pipeLine.Replace("_p_", ".");
            pipeLine = pipeLine.Replace("~", "\"");
            //pipeLine = pipeLine.Replace("_sq_", "'");
            pipeNumber = pipeLine;
            return pipeNumber;
        }

        public static string repicePipeLine1(string pipeLine)
        {
            string pipeNumber = pipeLine;
            pipeLine = pipeLine.Replace(@"\", "_bs_");
            pipeLine = pipeLine.Replace(@"/", "_fs_");
            pipeLine = pipeLine.Replace(".", "_p_");
            pipeLine = pipeLine.Replace("\"", "_dqm_");
            pipeLine = pipeLine.Replace("'", "_sq_");
            pipeNumber = pipeLine;
            return pipeNumber;
        }

        /// <summary>
        /// 刘校
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> displayConnection()
        {
            string path1 = BMECInstanceManager.FindConfigVariableName("OP_SPEC_DIR") + "\\" + "ConnectionTypeDisplay.xml";
            if (!File.Exists(path1))
            {
                MessageBox.Show("文件" + path1 + "不存在", "造价表实际连接方式映射失败!");
                return null;
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();


            XmlDocument doc = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreComments = true; //忽略文档注释
            //读取文件
            XmlReader reader = XmlReader.Create(path1, settings);
            doc.Load(reader);
            //获取根节点（一级/二级/...）
            XmlNode xn = doc.SelectSingleNode("DataParameters/ConnectionType");
            //获取子节点
            XmlNodeList xnl = xn.ChildNodes;

            foreach (XmlNode xmlNode in xnl)
            {
                XmlElement xe = (XmlElement)xmlNode;
                //获取子节点属性的属性值
                string k = xe.GetAttribute("abbreviation").ToString();
                string v = xe.GetAttribute("propertyName").ToString();
                bool isK = dic.Keys.Contains(k);
                if (!isK)
                {
                    dic.Add(k, v);
                }
            }

            return dic;
        }

        /// <summary>
        /// 彭胜鹏
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> connectionDis()
        {
            string path1 = BMECInstanceManager.FindConfigVariableName("OP_SPEC_DIR") + "\\" + "ConnectionTypeDisplay.xml";
            if (!File.Exists(path1))
            {
                MessageBox.Show("文件" + path1 + "不存在", "造价表实际连接方式映射失败!");
                return null;
            }

            string key;
            string value;
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(path1);
            }
            catch (Exception ex)
            {
                string ee = ex.ToString();
            }
            Dictionary<string, string> xmllist = new Dictionary<string, string>();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "ECProperty")
                    {
                        value = reader.GetAttribute(0);
                        key = reader.GetAttribute(1);
                        if(!xmllist.Keys.Contains(key))
                        {
                            xmllist.Add(key, value);
                        }                        
                    }
                }
            }

            return xmllist;
        }

        #region 关联管道
        /// <summary>
        /// 添加支吊架与父类为Device下的组件的关系
        /// </summary>
        /// <param name="component">父类为Device的组件</param>
        /// <param name="attachment">支吊架</param>
        /// <returns></returns>
        public static bool AddDeviceHasSupport(IECInstance component, IECInstance attachment)
        {
            Bentley.EC.Persistence.ChangeSet changeSet = new Bentley.EC.Persistence.ChangeSet();
            BMECApi instance = BMECApi.Instance;
            bool flag;
            flag = createSourcetHasTargetRelationship(changeSet, component, attachment, "DEVICE_HAS_SUPPORT");
            if (flag)
            {
                if (null == PersistenceManager.GetInstance())
                {
                    PersistenceManager.Initialize(Bentley.Plant.Utilities.DgnUtilities.GetInstance().GetDGNConnectionForPipelineManager());
                }
                PersistenceManager.GetInstance().CommitChangeSet(changeSet);
            }
            if (changeSet != null)
            {
                changeSet.Dispose();
            }
            return flag;
        }
        public static bool createSourcetHasTargetRelationship(Bentley.EC.Persistence.ChangeSet changeSet, IECInstance source, IECInstance target, string relationshipName)
        {
            if (null == changeSet || null == source || null == target)
            {
                return false;
            }
            BMECApi arg_1A_0 = BMECApi.Instance;
            Bentley.ECObjects.Schema.IECSchema schema = BMECInstanceManager.Instance.Schema;
            if (null == schema)
            {
                return false;
            }
            IECRelationshipInstance iECRelationshipInstance = null;
            Bentley.ECObjects.Schema.IECRelationshipClass[] relationshipClasses = schema.GetRelationshipClasses();
            int num = 0;
            if (0 < relationshipClasses.Length)
            {
                Bentley.ECObjects.Schema.IECRelationshipClass iECRelationshipClass;
                do
                {
                    iECRelationshipClass = relationshipClasses[num];
                    if (iECRelationshipClass.Name.Equals(relationshipName))
                    {
                        goto IL_5C;
                    }
                    num++;
                }
                while (num < relationshipClasses.Length);
                goto IL_68;
                IL_5C:
                iECRelationshipInstance = (iECRelationshipClass.CreateInstance() as IECRelationshipInstance);
            }
            IL_68:
            if (null != iECRelationshipInstance)
            {
                try
                {
                    iECRelationshipInstance.Source = source;
                    iECRelationshipInstance.Target = target;

                    changeSet.Add(iECRelationshipInstance);
                    changeSet.MarkNew(iECRelationshipInstance);
                }
                catch (System.Exception e)
                {

                    //System.Windows.Forms.MessageBox.Show(e.ToString());
                }

                return true;
            }
            return false;
        }
        #endregion
    }
}

