
using BIM = Bentley.Interop.MicroStationDGN;
using BG = Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET.Elements;
using Bentley.DgnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class Mstn_Public_Api
    {
        public static BIM.Application app = Utilities.ComApp;
        static double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//当前设计文件的主单位
        public  static BIM.Point3d DPoint3d_To_Point3d(BG.DPoint3d dpoint3d)
        {
            BIM.Point3d point = app.Point3dFromXYZ(dpoint3d.X / uor_per_master, dpoint3d.Y / uor_per_master, dpoint3d.Z / uor_per_master);
            return point;
        }
        public static BG.DPoint3d Point3d_to_GDPoint3d(BIM.Point3d point3d)
        {
            
            BG.DPoint3d gdpoint = new BG.DPoint3d();
            gdpoint.X = point3d.X * uor_per_master;
            gdpoint.Y = point3d.Y * uor_per_master;
            gdpoint.Z = point3d.Z * uor_per_master;
            return gdpoint;
        }

        public static BIM.Point3d get_nearest_point(BIM.Point3d p, BIM.Point3d p1, BIM.Point3d p2)
        {
            BIM.Point3d nearest_point;

            double distance1 = app.Point3dDistance(p, p1);
            double distance2 = app.Point3dDistance(p, p2);
            if (distance1 < distance2)
            {
                nearest_point = p1;
            }
            else
            {
                nearest_point = p2;
            }
            return nearest_point;
        }
        public static BIM.Point3d get_faster_point(BIM.Point3d p, BIM.Point3d p1, BIM.Point3d p2)
        {
            BIM.Point3d faster_point;
            
            double distance1 = app.Point3dDistance(p, p1);
            double distance2 = app.Point3dDistance(p, p2);
            if (distance1 > distance2)
            {
                faster_point = p1;
            }
            else
            {
                faster_point = p2;
            }
            return faster_point;
        }

        public static BIM.Element[] scan_element_at_point(BIM.Point3d point3d,bool scan_child_model,BIM.ModelReference model=null)
        {
            if (model == null)
            {
                model = app.ActiveModelReference;
            }
            List<BIM.Element> result_elems = new List<Bentley.Interop.MicroStationDGN.Element>();
            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();

            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = model.Scan(esc);

            BIM.Element[] elems =ee.BuildArrayFromContents();
            if (elems.Length != 0)
            {
                result_elems.AddRange(elems);
            }
            if (scan_child_model)
            {
                foreach (BIM.Attachment attachment in app.ActiveModelReference.Attachments)
                {
                    BIM.Element[] attach_elem = scan_element_at_point(point3d, scan_child_model, attachment);
                    if (attach_elem != null)
                    {
                        result_elems.AddRange(attach_elem);
                    }
                }
            }
            return result_elems.ToArray();
        }
        public static BIM.Element[] scan_element_at_point(BIM.Point3d point3d, bool scan_child_model, BIM.Attachment attachment)
        {

            BIM.ElementScanCriteria esc = new BIM.ElementScanCriteriaClass();
            List<BIM.Element> result_elems = new List<Bentley.Interop.MicroStationDGN.Element>();
            BIM.Range3d range3d = app.Range3dFromPoint3d(point3d);

            esc.IncludeOnlyWithinRange(range3d);
            BIM.ElementEnumerator ee = attachment.Scan(esc);

            BIM.Element[] elems = ee.BuildArrayFromContents();
            if (elems.Length != 0)
            {
                result_elems.AddRange( elems);
            }
            if (scan_child_model)
            {
                foreach (BIM.Attachment child_attachment in attachment.Attachments)
                {
                    BIM.Element[] attach_elem = scan_element_at_point(point3d, scan_child_model, child_attachment);
                    if (attach_elem.Length != 0)
                    {
                        result_elems.AddRange(attach_elem);
                    }
                }
            }
            return result_elems.ToArray();
        }

        public static DimensionElement create_dimension_arrow(BG.DPoint3d pt1, BG.DPoint3d pt2,double dimension_height, LevelId level_id, BG.DMatrix3d rMatrix, DimensionStyle dim_style=null, DgnTextStyle dgntext_style=null)
        {
            DgnFile dgnfile = Session.Instance.GetActiveDgnFile();
            
            if (dim_style==null)
            {
                dim_style = DimensionStyle.GetSettings(dgnfile);
            }
            if (dgntext_style==null)
            {
                dgntext_style = DgnTextStyle.GetSettings(dgnfile);
            }
            DimensionProperty dimension_property = new DimensionProperty(dim_style, dgntext_style, new Symbology(), level_id, null, BG.DMatrix3d.Identity, BG.DMatrix3d.Identity, 0);

            DimensionElement oDim = new DimensionElement(Session.Instance.GetActiveDgnModel(), dimension_property, DimensionType.SizeArrow);
            if (oDim.IsValid)
            {
                oDim.InsertPoint(pt1, null, dim_style, -1);
                oDim.InsertPoint(pt2, null, dim_style, -1);
                oDim.SetHeight(dimension_height);
                oDim.SetRotationMatrix(rMatrix);
                ElementPropertiesSetter setter = new ElementPropertiesSetter();
                setter.SetLevel(level_id);
                setter.SetColor(2);
                setter.Apply(oDim);
                return oDim;
            }
            return null;
        }
    }
    public class DimensionProperty : DimensionCreateData
    {

        DimensionStyle m_dimstyle;
        DgnTextStyle m_textStyle;
        Symbology m_symbology;
        LevelId m_levelId;
        DirectionFormatter m_directionFormatter;
        BG.DMatrix3d m_dimrot;
        BG.DMatrix3d m_viewrot;
        int m_view_num;

        public DimensionProperty(DimensionStyle dimstyle, DgnTextStyle textstyle, Symbology symb, LevelId levelId, DirectionFormatter formatter, BG.DMatrix3d dim_rot, BG.DMatrix3d view_rot, int view_num)
        {
            m_dimstyle = dimstyle;
            m_textStyle = textstyle;
            m_symbology = symb;
            m_levelId = levelId;
            m_directionFormatter = formatter;
            m_dimrot = dim_rot;
            m_viewrot = view_rot;
            m_view_num = view_num;
        }

        public override DimensionStyle GetDimensionStyle()
        {
            return m_dimstyle;
        }

        public override DgnTextStyle GetTextStyle()
        {
            return m_textStyle;
        }

        public override Symbology GetSymbology()
        {
            return m_symbology;
        }

        public override LevelId GetLevelId()
        {
            return m_levelId;
        }

        public override int GetViewNumber()
        {
            return m_view_num;
        }

        public override BG.DMatrix3d GetDimensionRotation()
        {
            return m_dimrot;
        }

        public override BG.DMatrix3d GetViewRotation()
        {
            return m_viewrot;
        }

        public override DirectionFormatter GetDirectionFormatter()
        {
            return m_directionFormatter;
        }
    }
}
