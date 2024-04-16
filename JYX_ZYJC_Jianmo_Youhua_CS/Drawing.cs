﻿
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.OpenPlant.Modeler.Api;
using System;
using BIM = Bentley.Interop.MicroStationDGN;
namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class Drawing
    {
        #region 属性

        private BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        private BIM.ModelReference modelRoferende;
        private BMECObject ec_object;
        private BMECApi bMECApi;
        private static Drawing drawing;
        #endregion

        #region 构造

        public Drawing()
        {
            drawing = this;
        }
        #endregion

        #region 命令入口

        public static void Start(string unparsed)
        {
            if(null == drawing)
            {
                drawing =  new Drawing();
            }
            drawing.support_drawing();
        }
        #endregion

        #region 出图控制

        public void support_drawing()
        {
            #region 判断选中元素是否为单个支吊架
            //判断是否为单个
            ElementAgenda elementAgenda = new ElementAgenda();//选Element的集合
            SelectionSetManager.BuildAgenda(ref elementAgenda);//获取选中的元素的集合
            if (1 != elementAgenda.GetCount())
            {
                System.Windows.Forms.MessageBox.Show("请选择单个支吊架");
                return;
            }
            //判断是否是支吊架
            bMECApi = new BMECApi();
            Element element = elementAgenda.GetEntry(0);//获取被选中的单个元素
            ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);//通过ElementId 获取 BMECObject
            if (ec_object.Instance == null)
            {
                System.Windows.Forms.MessageBox.Show("请选择支吊架");
                return;
            }

            if (!bMECApi.InstanceDefinedAsClass(ec_object.Instance, "SUPPORT_FRAME", true))//判断父类元素名
            {
                System.Windows.Forms.MessageBox.Show("请选择支吊架");
                return;
            }
            
            #endregion
            
            //画图形
            detail_drawing();


            app.CommandState.StartDefaultCommand();//唤醒线程
        }
        #endregion
        
        #region 画图

        #region 画详图

        //画详图
        public void detail_drawing()
        {
            double height;
            double cWidth;
            double cHeight;
            double l_x = 920, l_y = 3600, r_x = 4690, r_y = 3600;
            

            //画A型
            if (bMECApi.InstanceDefinedAsClass(ec_object.Instance, "SUPPORT_TYPEA", false))
            {
                CreateDng();//创建图纸
                height =  ec_object.GetDoubleValueInMM("JYX_HEIGHT");
                cWidth = ec_object.GetDoubleValueInMM("JYX_PROFILE_WIDTH");
                cHeight = ec_object.GetDoubleValueInMM("JYX_PROFILE_HEIGHT");
                //正视图
                //顶点    
                BIM.Point3d origin1 = app.Point3dFromXY(l_x, l_y);
                BIM.LineElement lineElement1 = app.CreateLineElement2(null, origin1, app.Point3dFromXY(l_x + cWidth, l_y));
                BIM.LineElement lineElement2 = app.CreateLineElement2(null, app.Point3dFromXY(l_x + cWidth, l_y), app.Point3dFromXY(l_x + cWidth, l_y - height));
                BIM.LineElement lineElement3 = app.CreateLineElement2(null, app.Point3dFromXY(l_x + cWidth, l_y - height), app.Point3dFromXY(l_x, l_y - height));
                BIM.LineElement lineElement4 = app.CreateLineElement2(null, app.Point3dFromXY(l_x, l_y - height), origin1);

                modelRoferende.AddElement(lineElement1);
                modelRoferende.AddElement(lineElement2);
                modelRoferende.AddElement(lineElement3);
                modelRoferende.AddElement(lineElement4);
                
                //左视图
                BIM.Point3d origin2 = app.Point3dFromXY(r_x,r_y);
                BIM.LineElement lineElement5 = app.CreateLineElement2(null, origin2, app.Point3dFromXY(r_x, r_y - height));
                BIM.LineElement lineElement6 = app.CreateLineElement2(null, app.Point3dFromXY(r_x, r_y - height), app.Point3dFromXY(r_x - cHeight, r_y - height));
                BIM.LineElement lineElement7 = app.CreateLineElement2(null, app.Point3dFromXY(r_x - cHeight, r_y - height), app.Point3dFromXY(r_x - cHeight, r_y));
                BIM.LineElement lineElement8 = app.CreateLineElement2(null, app.Point3dFromXY(r_x - cHeight, r_y), origin2);

                modelRoferende.AddElement(lineElement5);
                modelRoferende.AddElement(lineElement6);
                modelRoferende.AddElement(lineElement7);
                modelRoferende.AddElement(lineElement8);

                //画标注
                Dimension(0, height, 0, cWidth, cHeight);
                return;
            }
            System.Windows.Forms.MessageBox.Show("目前版本仅支持A型");
        }
        #endregion

        #region 画标注

        //画标注
        public void Dimension(double length, double height, double height1, double cWidth, double cHeight)
        {
            double l_x = 920, l_y = 3600, r_x = 4690, r_y = 3600;
            //标注线条
            //左2
            BIM.Matrix3d rotation = app.Matrix3dFromRotationBetweenVectors(app.Point3dFromXY(0, 1), app.Point3dFromXY(1, 0));//标注方向
            BIM.Transform3d tran = app.Transform3dFromMatrix3d(rotation);//app.Transform3dIdentity()
            //DimensionElement create_dim_size_arrow(Point3d dim_start_point, Point3d dim_end_point, Transform3d tran, double dim_height)
            BIM.Point3d dim_start_point = app.Point3dFromXY(l_x + cWidth, l_y);
            BIM.Point3d dim_end_point = app.Point3dFromXY(l_x + cWidth, l_y - height);
            modelRoferende.AddElement(create_dim_size_arrow(dim_start_point, dim_end_point, tran, 200));

            //左1
            BIM.Point3d dim_start_point2 = app.Point3dFromXY(l_x, l_y - height);
            BIM.Point3d dim_end_point2 = app.Point3dFromXY(l_x + cWidth, l_y - height);
            modelRoferende.AddElement(create_dim_size_arrow(dim_start_point2, dim_end_point2, app.Transform3dIdentity(), -200));

            //右1
            BIM.Point3d dim_start_point3 = app.Point3dFromXY(r_x, r_y);
            BIM.Point3d dim_end_point3 = app.Point3dFromXY(r_x - cHeight, r_y);
            modelRoferende.AddElement(create_dim_size_arrow(dim_start_point3, dim_end_point3, app.Transform3dIdentity(), 130));
            

            //注释文字
            Text(cWidth);

            //说明文字
            Text(height,cWidth);
        }
        #endregion

        #region 注释文字

        //注释文字
        public void Text(double cWidth)
        {
            double l_x = 920, l_y = 3600/*, r_x = 4690, r_y = 3600*/;
            BIM.LineElement lineElement1 = app.CreateLineElement2(null, app.Point3dFromXY(l_x + cWidth/2, l_y - 200), app.Point3dFromXY(l_x + cWidth / 2 -350, l_y - 200 +250));
            BIM.LineElement lineElement2 = app.CreateLineElement2(null, app.Point3dFromXY(l_x + cWidth / 2 - 350, l_y - 200 + 250), app.Point3dFromXY(l_x + cWidth / 2 - 350 - 150, l_y - 200 + 250));

            modelRoferende.AddElement(lineElement1);
            modelRoferende.AddElement(lineElement2);
            
            BIM.Point3d dim_start_point = app.Point3dFromXY(l_x + cWidth / 2 - 350 - 150+30, l_y - 200 + 250+12);
            modelRoferende.AddElement(create_textelement(dim_start_point, "001", BIM.MsdTextJustification.LeftBottom));
            
            
        }
        #endregion

        #region 材料说明

        //说明文字
        public void Text(double height, double cWidth)
        {
            //右下角说明

            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(4040+30, 1420+12), "001", BIM.MsdTextJustification.LeftBottom));//序号


            string name = ec_object.GetDoubleValueInMM("JYX_PROFILE_WIDTH") + "X" + ec_object.GetDoubleValueInMM("JYX_PROFILE_HEIGHT") + "X" + ec_object.GetDoubleValueInMM("JYX_PROFILE_THICKNESS");
            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(4160, 1420+12), "槽钢"+ name + " L="+ string.Format("{0:N2}", height), BIM.MsdTextJustification.LeftBottom));//名称

            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(4720+30, 1420+12), "1", BIM.MsdTextJustification.LeftBottom));//数量

            name = ec_object.GetStringValue("JYX_MATERIAL");
            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(4840+30, 1420+12), name, BIM.MsdTextJustification.LeftBottom));//材料

            name = string.Format("{0:N3}", ec_object.GetDoubleValueInMM("JYX_WEIGHT_DRY") / 1000 * height);
            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(5080+10, 1420+12), name, BIM.MsdTextJustification.LeftBottom));//单重

            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(5230+10, 1420+12), name, BIM.MsdTextJustification.LeftBottom));//总重JYX_CATALOG

            name = ec_object.GetStringValue("JYX_CATALOG");
            modelRoferende.AddElement(create_textelement(app.Point3dFromXY(5380, 1420+12), name, BIM.MsdTextJustification.LeftBottom));//图号或标准规格号
        }
        #endregion

        #endregion

        #region 创建图纸
        //创建图纸
        public void CreateDng()
        {
            //创建图纸
            string SeedFileName = BMECInstanceManager.FindConfigVariableName("MSDIR") + @"\JYXConfig\PlotTemplate.dgn";
            string NewDesignFileName = BMECInstanceManager.FindConfigVariableName("MSDIR") + @"\JYXConfig\PlotType" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".dgn";
            app.CreateDesignFile(SeedFileName, NewDesignFileName, false);
            //dgnFile = app.OpenDesignFileForProgram(NewDesignFileName, false);//后台打开图纸，后台打开图纸时使用默认model会报错
            BIM.DesignFile dgnFile = app.OpenDesignFile(NewDesignFileName, false);//前台打开图纸
            //获得默认model
            modelRoferende = dgnFile.DefaultModelReference;//获取默认model
        }
        #endregion

        #region 创建元素

        //创建标注
        public BIM.DimensionElement create_dim_size_arrow(BIM.Point3d dim_start_point, BIM.Point3d dim_end_point, BIM.Transform3d tran, double dim_height)
        {
            BIM.DimensionElement dim_elem = app.CreateDimensionElement1(null, app.Matrix3dFromTransform3d(tran), BIM.MsdDimType.SizeArrow);


            if ((app.Point3dAngleBetweenVectors(app.Point3dFromXYZ(-1, 0, 0), tran.RowX) < Math.PI / 2) || (app.Point3dAngleBetweenVectors(app.Point3dFromXYZ(0, 1, 0), tran.RowX) < 0.001))
            {
                dim_elem.AddReferencePoint(app.ActiveModelReference, ref dim_end_point);
                dim_elem.AddReferencePoint(app.ActiveModelReference, ref dim_start_point);
            }
            else
            {
                dim_elem.AddReferencePoint(app.ActiveModelReference, ref dim_start_point);
                dim_elem.AddReferencePoint(app.ActiveModelReference, ref dim_end_point);
            }


            dim_elem.DimHeight = dim_height;
            return dim_elem;
        }

        //创建文字
        public BIM.TextElement create_textelement(BIM.Point3d point_center, string text, BIM.MsdTextJustification justification, BIM.TextStyle textstyle = null)
        {
            if (textstyle == null)
            {
                textstyle = app.ActiveSettings.TextStyle;
            }
            BIM.Point3d text_offset = app.Point3dZero();
            if (textstyle.Justification == BIM.MsdTextJustification.CenterCenter)
            {
                text_offset = app.Point3dZero();
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.CenterBottom)
            {
                text_offset = app.Point3dFromXY(0, -1);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.CenterTop)
            {
                text_offset = app.Point3dFromXY(0, 1);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.LeftBottom)
            {
                text_offset = app.Point3dFromXY(-1, -1);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.LeftCenter)
            {
                text_offset = app.Point3dFromXY(-1, 0);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.LeftTop)
            {
                text_offset = app.Point3dFromXY(-1, 1);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.RightBottom)
            {
                text_offset = app.Point3dFromXY(1, -1);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.RightCenter)
            {
                text_offset = app.Point3dFromXY(1, 0);
            }
            else if (textstyle.Justification == BIM.MsdTextJustification.RightTop)
            {
                text_offset = app.Point3dFromXY(1, 1);
            }

            if (justification == BIM.MsdTextJustification.CenterCenter)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dZero());
            }
            else if (justification == BIM.MsdTextJustification.CenterBottom)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(0, 1));
            }
            else if (justification == BIM.MsdTextJustification.CenterTop)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(0, -1));

            }
            else if (justification == BIM.MsdTextJustification.LeftBottom)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(1, 1));

            }
            else if (justification == BIM.MsdTextJustification.LeftCenter)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(1, 0));

            }
            else if (justification == BIM.MsdTextJustification.LeftTop)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(1, -1));

            }
            else if (justification == BIM.MsdTextJustification.RightBottom)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(-1, 1));

            }
            else if (justification == BIM.MsdTextJustification.RightCenter)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(-1, 0));

            }
            else if (justification == BIM.MsdTextJustification.RightTop)
            {
                text_offset = app.Point3dAdd(text_offset, app.Point3dFromXY(-1, -1));
            }


            double height = textstyle.Height * app.ActiveModelReference.GetSheetDefinition().AnnotationScaleFactor * 1;//Mstn_Public_Api.get_mm_unit();
            BIM.TextElement textelement = app.CreateTextElement1(null, text, point_center, app.Matrix3dIdentity());
            textelement.TextStyle = textstyle;
            BIM.Point3d origin_new = app.Point3dFromXY(text_offset.X * (textelement.Range.High.X - textelement.Range.Low.X) / 2, text_offset.Y * height / 2);
            textelement.Move(origin_new);
            return textelement;
        }
        #endregion

    }
}
