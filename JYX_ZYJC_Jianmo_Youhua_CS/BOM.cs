using Bentley.ECObjects.Instance;
using Bentley.OpenPlant.Modeler.Api;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class BOM
    {
        #region 属性
        public BOMForm bOMForm;
        private ECInstanceList ECinstance_list = new ECInstanceList();
        private BMECObject ec_object;
        public static BOM bom;
        private BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        private Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
        private List<string> jyx_type_name_list = new List<string>();
        private Application xApp;
        #endregion

        #region 构造

        public BOM()
        {
            bom = this;
        }
        #endregion

        #region 命令入口

        public static void Start(string unparsed)
        {
            if (null == bom)
            {
                bom = new BOM();
            }
            bom.Search_Files();
        }
        #endregion

        public void excel(List<string> jyx_type_name_list)
        {
            #region MyRegion

            //string path = BMECInstanceManager.FindConfigVariableName("OPENPLANT_WORKSET_STANDARDS") + @"\管道支架表.xlsx";
            //var xApp = new Application();
            //_Workbook _workbook_old = xApp.Workbooks.Open(path);//文件，打开
            //_Workbook _workbook_new = xApp.Workbooks.Add();//文件，新建
            //_Worksheet _worksheet = _workbook_old.Sheets[1];//表格，下标从1开始
            //_Chart _cell = _worksheet.Cells[1, 1];//单元格，下标从1开始
            //Range _range = _worksheet.Range[_worksheet.Cells[1, 1], _worksheet.Cells[100, 100]];//单元格枚举，从1，1到100,100的单元格
            ////打开表格
            //Worksheet worksheet = workbook.Sheets[1];//表格，下标从1开始
            ////填入相应数据
            //worksheet.Cells[6, 3] = "ads1";//单元格，下标从1开始
            #endregion
            this.jyx_type_name_list = jyx_type_name_list;
            bOMForm.Close();//关闭窗口

            //复制模板，创建新文件
            string path = BMECInstanceManager.FindConfigVariableName("MSDIR") + @"\JYXConfig\管道支架表.xlsx";
            //string new_path = BMECInstanceManager.FindConfigVariableName("OPENPLANT_WORKSET_STANDARDS") + @"管道支架表" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".xlsx";
            string new_path = SelectPath()+ @"\管道支架表" + string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now) + ".xlsx";
            File.Copy(path, new_path);
            
            //打开新建的文件
            xApp = new Application();
            _Workbook workbook = xApp.Workbooks.Open(new_path);//文件，打开

            //添加数据
            Search_Support();


            //保存
            workbook.Save();
            
            //关闭excel程序
            xApp.DisplayAlerts = false;
            workbook.Close();
            xApp.Quit();
            
            System.Windows.Forms.MessageBox.Show("成功生成材料报表");
        }

        //用户选择路径
        private string SelectPath()
        {
            string path = string.Empty;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
            }
            return path;
        }


        //搜索所有支吊架
        public void Search_Files()
        {
            try
            {

                //再次使用时清空上次记录
                ECinstance_list.Clear();
                //搜索
                ECinstance_list = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetInstancesFromDgn("SUPPORT_FRAME", true);
                if (ECinstance_list.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("没有检测到支吊架");
                    return;
                }
                jyx_type_name_list.Clear();
                //将name放入list
                foreach (IECInstance item in ECinstance_list)
                {
                    ec_object = new BMECObject(item);
                    if (!jyx_type_name_list.Contains(ec_object.GetStringValue("JYX_TYPE_NAME")))
                    {
                        Bentley.GeometryNET.DPoint3d location = ec_object.Transform3d.Translation;
                        jyx_type_name_list.Add(ec_object.GetStringValue("JYX_TYPE_NAME"));
                    }
                    //BMECApi.Instance.DeleteFromModel(ec_object);
                }
                bOMForm = new BOMForm(jyx_type_name_list);
#if DEBUG
#else
                bOMForm.AttachAsTopLevelForm(MyAddin.s_addin, false);
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BOMForm));
                bOMForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif

                bOMForm.Show();
            }
            catch (Exception d)
            {

                System.Windows.Forms.MessageBox.Show(d.ToString());
            }

        }


        //搜索符合条件支吊架
        public void Search_Support()
        {
            int row = 6;
            foreach (string item in jyx_type_name_list)
            {
                ECinstance_list = BMECApi.Instance.FindInstances(instance.GetDGNConnectionForPipelineManager(), "SUPPORT_TYPEA", "JYX_TYPE_NAME", item, false);
                int count =  ECinstance_list.Count;
                //单个支吊架参数
                IECInstance tempInstance = BMECApi.Instance.FindAllInformationOnInstance(ECinstance_list[0]);
                double H = tempInstance["JYX_HEIGHT"].DoubleValue;
                double jyx_weight_dry = tempInstance["JYX_WEIGHT_DRY"].DoubleValue;//单重
                string jyx_material = tempInstance["JYX_MATERIAL"].StringValue;//材质
                //同名支吊架参数总和
                //foreach (IECInstance instance in ECinstance_list)
                //{
                //    //ec_object = new BMECObject(instance);
                //    //double h = ec_object.GetDoubleValueInMM("JYX_HEIGHT"); 
                //    IECInstance tempInstance = BMECApi.Instance.FindAllInformationOnInstance(instance);
                //    double h = tempInstance["JYX_HEIGHT"].DoubleValue;
                //    H = H + h;
                //}
                AddData(item, H, count, row, jyx_weight_dry,jyx_material);
                row++;
            }
        }


        //添加数据
        public void AddData(string name,double H, int count,int row,double jyx_weight_dry,string jyx_material)
        {
            xApp.Cells[row, 2] = name;//支架编号
            xApp.Cells[row, 3] ="型钢支架";//型式
            //xApp.Cells[6, 4] = "ads2";//管径
            xApp.Cells[row, 5] = jyx_material;
            xApp.Cells[row, 7] = H;//技术参数
            xApp.Cells[row, 10] = count;//数量
            xApp.Cells[row, 11] = jyx_weight_dry;//单重(kg)
            xApp.Cells[row, 13] = "注：";//相关图号或备注
        }
        
    }
}
