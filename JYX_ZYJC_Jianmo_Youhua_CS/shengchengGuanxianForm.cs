using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.MstnPlatformNET.WinForms.Controls;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.GeometryNET;
using Bentley.UI.Controls.WinForms;
using Bentley.DgnPlatformNET.Elements;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.Building.Mechanical.Components;
//using Bentley.Interop.MicroStationDGN;
namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class shengchengGuanxianForm:
#if DEBUG
    Form
#else
                     Adapter
#endif
    {
        public static Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public LevelHandleCollection levels;
        public shengchengGuanxianForm()
        {
            InitializeComponent();
            comboBox_guanxian_leixing.SelectedIndex = 0;

            ElementColor element_color = new ElementColor(0, Session.Instance.GetActiveDgnModel());

            ColorPickerPopup color_picker_popup = new ColorPickerPopup();
            color_picker_popup.SelectedValue = element_color;
            color_picker_popup.ModelRef = Session.Instance.GetActiveDgnModel();
            color_picker_popup.Left = this.button_guanxian_liulan.Left;
            color_picker_popup.Top = this.comboBox_guanxian_leixing.Top;
            color_picker_popup.Name = "color_picker_popup";

            color_picker_popup.MinimumSize = new Size(75, 18);

            color_picker_popup.IconSize = new Size(20, 20);
            this.groupBox1.Controls.Add(color_picker_popup);
        }




        private void button_guanxian_liulan_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog();
            open_file_dialog.Title = "请选择要导入的管线数据文件";
            open_file_dialog.Filter = "Excel 2007/2010 file (*.xlsx）|*.xlsx|Execl 2003 files (*.xls)|*.xls";
            //  open_file_dialog.InitialDirectory = @"D:\";
            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_guanxian_wenjian_lujing.Text = open_file_dialog.FileName;
            }
            else
            {
                return;
            }


        }

        private void button_kancedian_liulan_Click(object sender, EventArgs e)
        {
            OpenFileDialog open_file_dialog = new OpenFileDialog();
            open_file_dialog.Title = "请选择要导入的勘测点数据文件";
            open_file_dialog.Filter = "Excel 2007/2010 file (*.xlsx）|*.xlsx|Execl 2003 files (*.xls)|*.xls";
            //  open_file_dialog.InitialDirectory = @"D:\";
            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_kancedian_data_lujing.Text = open_file_dialog.FileName;
            }
            else
            {
                return;
            }
        }
        private void button_shengcheng_guanxian_Click(object sender, EventArgs e)
        {
            double pt_to_dpt = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;


            //读取勘测点数据
            string wenjian_lujing = this.textBox_kancedian_data_lujing.Text.Trim();
            DataTable table_kancedian = excelProcessor.get_Excel_cedian_data(wenjian_lujing);
            Dictionary<string, DPoint3d> dic_kancedian_zuobiao = new Dictionary<string, DPoint3d>();
            shaixuan_kancedian_zuobiao(table_kancedian, ref dic_kancedian_zuobiao);

            //读取管线数据
            wenjian_lujing = this.textBox_guanxian_wenjian_lujing.Text.Trim();
            string guanxian_leixing = this.comboBox_guanxian_leixing.Text.Trim();
            DataTable table_guanxian = excelProcessor.get_Excel_guanxian_data(wenjian_lujing, guanxian_leixing);

            //获取属性设置器
            object o = this.Controls.Find("color_picker_popup", true)[0];
            ElementColor element_color = ((ColorPickerPopup)o).SelectedValue;
            
            int color = element_color.Index;
            ElementPropertiesSetter prop_setter = get_prop_setter(guanxian_leixing, color);

            int i = 0;
            if (table_guanxian != null)
            {
                foreach (DataRow data_row in table_guanxian.Rows)
                {
                    i++;
                    string current_code = Convert.ToString(data_row["当前点编号"]);
                    string connect_code = Convert.ToString(data_row["连接点编号"]);
                    string duanmian_guige = Convert.ToString(data_row["断面规格"]);
                    string fushuwu_name = Convert.ToString(data_row["附属物"]);
                    string dimian_gaocheng = Convert.ToString(data_row["地面"]);
                    DPoint3d current_origin, connect_origin;
                    
                    if (string.Empty == duanmian_guige)
                    {
                        try
                        {
                            current_origin = dic_kancedian_zuobiao[current_code];
                            
                        }
                        catch
                        {
                            MessageBox.Show("未找到编号为" + current_code + "的勘测点数据，请检查勘测点数据文件！");
                            break;
                        }
                        if (string.Empty != fushuwu_name)
                        {
                            string fushuwu_duanmian = get_fushuwu_jiemian(guanxian_leixing, fushuwu_name);
                            if (fushuwu_duanmian.Length == 0)
                            {
                                continue;
                            }
                            DPoint3d fushuwu_ding = current_origin;
                            DPoint3d fushuwu_di = current_origin;
                            
                            
                            string[] f = fushuwu_duanmian.Split('X');
                            if (f.Length == 2)
                            {
                                fushuwu_ding.Z += double.Parse(f[1]) * pt_to_dpt;

                                DPoint3d dp_start = current_origin;
                                DPoint3d dp_end = fushuwu_ding;
                                double length = dp_start.Distance(dp_end) / pt_to_dpt;
                                DMatrix3d dma = DMatrix3d.Identity;

                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(DVector3d.FromXYZ(1, 0, 0), dp_end - dp_start, ref dma);
                                DTransform3d dtran = DTransform3d.FromMatrixAndTranslation(dma, dp_start);
                                ElementHolder element_holder = OPM_Public_Api.create_rectangular_surface(double.Parse(f[0]), double.Parse(f[1]), length, DPoint3d.FromXYZ(0, 0, 0), dtran);
                                JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);

                            }
                            else if (f.Length == 1)
                            {

                                ElementHolder element_holder = OPM_Public_Api.create_cone_surface(double.Parse(f[0]), fushuwu_di, fushuwu_ding);
                                JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
                            }
                            else
                            {

                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            current_origin = dic_kancedian_zuobiao[current_code];
                            connect_origin = dic_kancedian_zuobiao[connect_code];
                        }
                        catch
                        {
                            MessageBox.Show("未找到编号为" + current_code + "或" + connect_code + "的勘测点数据，请检查勘测点数据文件！");
                            break;
                        }
                        if (duanmian_guige.Contains('X'))
                        {
                            continue;

                        }
                        else
                        {
                            double zhijing = Convert.ToDouble(duanmian_guige);
                            ElementHolder element_holder = OPM_Public_Api.create_cone_surface(zhijing, current_origin, connect_origin);
                            element_holder.Color = (uint)color;
                            JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
                        }

                        if (string.Empty != fushuwu_name)
                        {
                            string fushuwu_duanmian = get_fushuwu_jiemian(guanxian_leixing, fushuwu_name);
                            if (fushuwu_duanmian.Length == 0)
                            {
                                continue;
                            }
                            DPoint3d fushuwu_ding = current_origin;
                            DPoint3d fushuwu_di = current_origin;
                            fushuwu_ding.Z = Convert.ToDouble(dimian_gaocheng) * 1000 * pt_to_dpt;
                            double zhijing = Convert.ToDouble(duanmian_guige);
                            fushuwu_di.Z -= zhijing / 2 * pt_to_dpt;
                            string[] f = fushuwu_duanmian.Split('X');
                            if (f.Length == 2)
                            {
                                DPoint3d dp_start = current_origin;
                                DPoint3d dp_end = fushuwu_ding;
                                double length = dp_start.Distance(dp_end) / pt_to_dpt;
                                DMatrix3d dma = DMatrix3d.Identity;

                                JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(DVector3d.FromXYZ(1, 0, 0), dp_end - dp_start, ref dma);
                                DTransform3d dtran = DTransform3d.FromMatrixAndTranslation(dma, dp_start);
                                ElementHolder element_holder = OPM_Public_Api.create_rectangular_surface(double.Parse(f[0]), double.Parse(f[1]), length, DPoint3d.FromXYZ(0, 0, 0), dtran);
                                JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);

                            }
                            else if (f.Length == 1)
                            {

                                ElementHolder element_holder = OPM_Public_Api.create_cone_surface(double.Parse(f[0]), fushuwu_di, fushuwu_ding);
                                JYX_ZYJC_CLR.PublicMethod.add_element_holder_to_model(element_holder);
                            }
                            else
                            {

                            }
                        }
                    }

                }
                MessageBox.Show("管线生成完毕！");
            }
        }

        private void shaixuan_kancedian_zuobiao(DataTable table_kancedian, ref Dictionary<string, DPoint3d> dic_kancedian_zuobiao)
        {
            double pt_to_dpt = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
            foreach (DataRow data_row in table_kancedian.Rows)
            {

                string code = Convert.ToString(data_row["exp_NUM"]);
                if (dic_kancedian_zuobiao.Keys.Contains(code))
                {
                    continue;
                }

                string duanmian_guige = Convert.ToString(data_row["D_S"]);
                double gaodu = 0;
                if (duanmian_guige.Contains('X'))
                {
                    int index = duanmian_guige.IndexOf('X');
                    gaodu = Convert.ToDouble(duanmian_guige.Substring(index + 1)) * pt_to_dpt;

                }
                else
                {
                    if (string.Empty != duanmian_guige)
                    {
                        //断面规格为空的情况怎么处理？
                        gaodu = Convert.ToDouble(duanmian_guige) * pt_to_dpt;
                    }
                    else
                    {

                    }


                }

                double x = Convert.ToDouble(data_row["x"]) * 1000 * pt_to_dpt;
                double y = Convert.ToDouble(data_row["y"]) * 1000 * pt_to_dpt;
                double z = 0;
                string str_z = Convert.ToString(data_row["top_h"]);
                if (str_z == string.Empty)
                {
                    z = Convert.ToDouble(data_row["bottom_h"]) * 1000 * pt_to_dpt + gaodu / 2;
                }
                else
                {
                    z = Convert.ToDouble(data_row["top_h"]) * 1000 * pt_to_dpt - gaodu / 2;

                }
                dic_kancedian_zuobiao.Add(code, DPoint3d.FromXYZ(x, y, z));



            }

        }
        private string get_fushuwu_jiemian(string guanxian_leixing, string fushuwu_name)
        {
            string famenjing_jiemian = "1000";
            string paishajing_jiemian = "500";
            string jiancha_jing = famenjing_jiemian, jianxiujing = famenjing_jiemian, renkong = famenjing_jiemian;
            string huafen_chi = "1500X1000";
            string shoukong = "1000X1000";
            if (guanxian_leixing == "给水")
            {
                if (fushuwu_name == "阀门井")
                {
                    return famenjing_jiemian;
                }
                else if (fushuwu_name == "排沙井")
                {
                    return paishajing_jiemian;
                }
            }
            else if (guanxian_leixing == "污水")
            {
                if (fushuwu_name == "化粪池")
                {
                    return huafen_chi;
                }
                else if (fushuwu_name == "检查井")
                {
                    return jiancha_jing;
                }

            }
            else if (guanxian_leixing == "雨水")
            {
                if (fushuwu_name == "检查井")
                {
                    return jiancha_jing;
                }
            }
            else if (guanxian_leixing == "电力")
            {

                if (fushuwu_name == "检修井")
                {
                    return jianxiujing;
                }

            }
            else if (guanxian_leixing == "电信")
            {

                if (fushuwu_name == "人孔")
                {
                    return renkong;
                }
                else if (fushuwu_name == "手孔")
                {
                    return shoukong;
                }

            }
            else if (guanxian_leixing == "路灯")
            {

                if (fushuwu_name == "检修井")
                {
                    return shoukong;
                }
            }
            return string.Empty;

        }
        private void draw_pipe(DPoint3d origin, DVector3d extrudeVec, string duanmian_guige, ElementPropertiesSetter prop_setter)
        {
            DgnModel active_moddel = Session.Instance.GetActiveDgnModel();
            double pt_to_dpt = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

            Element duanmian_shape = null;
            try
            {
                if (duanmian_guige.Contains('X'))
                {
                    int index = duanmian_guige.IndexOf('X');
                    double length = Convert.ToDouble(duanmian_guige.Substring(0, index)) * pt_to_dpt;
                    double width = Convert.ToDouble(duanmian_guige.Substring(index + 1)) * pt_to_dpt;
                    DPoint3d[] duanmian_dpt = new DPoint3d[4];
                    duanmian_dpt[0] = duanmian_dpt[1] = duanmian_dpt[2] = duanmian_dpt[3] = origin;
                    duanmian_dpt[0].X = duanmian_dpt[3].X = origin.X - length / 2;
                    duanmian_dpt[1].X = duanmian_dpt[2].X = origin.X + length / 2;

                    duanmian_dpt[0].Z = duanmian_dpt[1].Z = origin.Z + width / 2;
                    duanmian_dpt[2].Z = duanmian_dpt[3].Z = origin.Z - width / 2;

                    duanmian_shape = new ShapeElement(active_moddel, null, duanmian_dpt);

                }
                else
                {
                    double zhijing = Convert.ToDouble(duanmian_guige) * pt_to_dpt; ;
                    duanmian_shape = new EllipseElement(active_moddel, null, origin, zhijing / 2, zhijing / 2, DMatrix3d.Rotation(0, Angle.FromDegrees(90)));
                }
                Angle angle = DVector3d.AngleXYBetween(extrudeVec, DVector3d.UnitX);
                duanmian_shape.ApplyTransform(new TransformInfo(DTransform3d.FromMatrixAndFixedPoint(DMatrix3d.Rotation(2, angle), origin)));
                SurfaceOrSolidElement guanxian = SurfaceOrSolidElement.CreateProjectionElement(active_moddel, null, duanmian_shape, origin, extrudeVec, DTransform3d.Identity, false);
                prop_setter.Apply(guanxian);
                guanxian.AddToModel();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }


        }
        private ElementPropertiesSetter get_prop_setter(string level_name, int color)
        {
            ElementPropertiesSetter prop_setter = new ElementPropertiesSetter();
            FileLevelCache level_cache = Session.Instance.GetActiveDgnFile().GetLevelCache();
            LevelHandle level_handle = level_cache.GetLevelByName(level_name);
            if (level_handle.IsValid)
            {
                prop_setter.SetLevel(level_handle.LevelId);
            }
            else
            {
                EditLevelHandle level = level_cache.CreateLevel(level_name);
                if (level_cache.Write() == LevelCacheErrorCode.None)
                {
                    level_handle = level_cache.GetLevelByName(level_name);
                    prop_setter.SetLevel(level_handle.LevelId);
                }
            }

            prop_setter.SetColor((uint)color);
            prop_setter.SetWeight(1);
            return prop_setter;
        }

    }



}



