﻿using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.GeometryNET;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.MstnPlatformNET.InteropServices;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class refreshChecked : DgnElementSetTool
    {
        static BIM.Application app = Utilities.ComApp;
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            //throw new NotImplementedException();
        }

        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
            if (ecList.Count == 0)
            {
                //System.Windows.Forms.MessageBox.Show("请先选择元素");
                //app.CommandState.StartDefaultCommand();
                //return;
                ECInstanceList ecListAll = DgnUtilities.GetAllInstancesFromDgn();
                ECInstanceList ecSx = new ECInstanceList();
                foreach(IECInstance ecIn in ecListAll)
                {
                    bool sheb = BMECApi.Instance.InstanceDefinedAsClass(ecIn, "EQUIPMENT", true);
                    bool b = BMECApi.Instance.InstanceDefinedAsClass(ecIn, "PIPING_COMPONENT", true); //查找ec的父类是否含有PIPING_COMPONENT
                    if(sheb||b)
                    {
                        ecSx.Add(ecIn);
                    }
                }
                ecList = ecSx;
            }
            try
            {
                foreach (IECInstance ecinstance in ecList)
                {
                    BMECObject bmec = new BMECObject(ecinstance);
                    bmec.Refresh();
                    bmec.Create();
                }
            }
            catch(Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                app.CommandState.StartDefaultCommand();
                return;
            }            
            System.Windows.Forms.MessageBox.Show("刷新成功！");
            app.CommandState.StartDefaultCommand();
            #region 刷新
            //bool b = true;
            ////System.Windows.Forms.MessageBox("请选择要刷新的元素");
            //ECInstanceList ecList = DgnUtilities.GetSelectedInstances();
            //if (ecList.Count == 0)
            //{
            //    System.Windows.Forms.MessageBox.Show("请先选择元素");
            //    return;
            //}
            //try
            //{
            //    foreach (IECInstance ecinstance in ecList)
            //    {
            //        //BMECObject bmec = new BMECObject(ecinstance);
            //        //DPoint3d dp = bmec.Transform3d.Translation;

            //        //bmec.Refresh();
            //        //DPoint3d dp1 = bmec.Transform3d.Translation;
            //        //DTransform3d dtForm = bmec.Transform3d;
            //        //bmec.Transform3d = dtForm;

            //        //bmec.Create();
            //        BMECObject bmec = new BMECObject(ecinstance);
            //        //DPoint3d dp=bmec.GetNthPort(0).LocationInUors;
            //        //DPoint3d dp1 = bmec.GetNthPort(1).LocationInUors;
            //        DPoint3d dp1 = new DPoint3d();

            //        bmec.Refresh();
            //        IECPropertyValue iECPropertyValue = ecinstance.FindPropertyValue("TRANSFORMATION_MATRIX", true, true, false);
            //        DPoint3d dpoint = new DPoint3d();
            //        dpoint = bmec.Transform3d.Translation;
            //        double x = dpoint.X / 1000;
            //        string orgin_x = string.Format("{0:F}", x);

            //        double y = dpoint.Y / 1000;
            //        double z = dpoint.Z / 1000;
            //        string orgin_z = string.Format("{0:F}", z);
            //        string orgin_y = string.Format("{0:F}", y);
            //        if (bmec.Instance.GetPropertyValue("Origin_X") != null)
            //        {
            //            bmec.Instance["Origin_X"].StringValue = orgin_x + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_Coordinate_X") != null)
            //        {
            //            bmec.Instance["D_Coordinate_X"].StringValue = orgin_x + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_zuobiaoX") != null)
            //        {
            //            bmec.Instance["D_zuobiaoX"].StringValue = orgin_x + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("Origin_Y") != null)
            //        {
            //            bmec.Instance["Origin_Y"].StringValue = orgin_y + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_Coordinate_Y") != null)
            //        {
            //            bmec.Instance["D_Coordinate_Y"].StringValue = orgin_y + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_zuobiaoY") != null)
            //        {
            //            bmec.Instance["D_zuobiaoY"].StringValue = orgin_y + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("Origin_Z") != null)
            //        {
            //            bmec.Instance["Origin_Z"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_Coordinate_Z") != null)
            //        {
            //            bmec.Instance["D_Coordinate_Z"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_zuobaioZ") != null)
            //        {
            //            bmec.Instance["D_zuobaioZ"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("zhongxinbiaogao") != null)
            //        {
            //            bmec.Instance["zhongxinbiaogao"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_Valve_Center_Elevation") != null)
            //        {
            //            bmec.Instance["D_Valve_Center_Elevation"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_guanzhongxinbiaogao") != null)
            //        {
            //            bmec.Instance["D_guanzhongxinbiaogao"].StringValue = orgin_z + "mm";
            //        }
            //        if (bmec.Instance.GetPropertyValue("D_guandingwaibiaogao") != null)
            //        {
            //            double od = bmec.Instance["OUTSIDE_DIAMETER"].DoubleValue;
            //            double wall = bmec.Instance["WALL_THICKNESS"].DoubleValue;
            //            double orginz = Convert.ToDouble(orgin_z);
            //            //double topbiaogao = orgin_z + od + wall;
            //            bmec.Instance["D_guandingwaibiaogao"].StringValue = (orginz + od + wall) + "mm";
            //            if (bmec.Instance.GetPropertyValue("D_guandiwaibiaogao") != null)
            //            {
            //                bmec.Instance["D_guandiwaibiaogao"].StringValue = (orginz - od - wall) + "mm";
            //            }
            //            if (bmec.Instance.GetPropertyValue("D_guandineibiaogao") != null)
            //            {
            //                bmec.Instance["D_guandineibiaogao"].StringValue = (orginz - od) + "mm";
            //            }
            //        }
            //        try
            //        {
            //            DPoint3d[] pointshuz = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec);
            //            DVector3d dv_x = new DVector3d(1, 0, 0);
            //            DVector3d dv_pipe = new DVector3d(pointshuz[0], pointshuz[1]);
            //            double angle = dv_pipe.AngleTo(dv_x).Degrees;
            //            string podu = string.Empty;
            //            if (pointshuz[0].Z == pointshuz[1].Z)
            //            {
            //                podu = "0°";
            //                angle = 0;
            //            }
            //            else
            //            {
            //                DPoint3d touy = new DPoint3d(pointshuz[1].X, pointshuz[1].Y, pointshuz[0].Z);
            //                DVector3d touyin = new DVector3d(pointshuz[0], touy);
            //                bool b1 = dv_pipe.IsPerpendicularTo(touyin);
            //                if (b1)
            //                {
            //                    podu = "90°";
            //                    angle = 90;
            //                }
            //                else
            //                {
            //                    double ra = dv_pipe.AngleTo(touyin).Radians;
            //                    angle = dv_pipe.AngleTo(touyin).Degrees;
            //                    double tan = Math.Tan(ra);
            //                    podu = (tan * 100) + "%";
            //                }
            //            }
            //            if (bmec.Instance.GetPropertyValue("D_jiaodu") != null)
            //            {
            //                bmec.Instance["D_jiaodu"].StringValue = angle + "°";
            //            }
            //            if (bmec.Instance.GetPropertyValue("D_podu") != null)
            //            {
            //                bmec.Instance["D_podu"].StringValue = podu;
            //            }
            //        }
            //        catch
            //        {

            //        }
            //        bmec.Create();
            //    }
            //}
            //catch
            //{
            //    b = false;
            //}
            //if (b)
            //{
            //    System.Windows.Forms.MessageBox.Show("刷新成功!");
            //}
            //else
            //{
            //    System.Windows.Forms.MessageBox.Show("刷新失败！");
            //}
            #endregion
        }
    }
}
