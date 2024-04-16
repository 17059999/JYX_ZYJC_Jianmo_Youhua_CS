using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Xml;
using System.IO;
using System.Collections;
using Microsoft.Office.Interop.Excel;
using Bentley.GeometryNET;
using System.Reflection;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class excelProcessor
    {
        public static Boolean to_xml(string xls_patch, string xml_patch)
        {
            string str_conn;
            string ext = Path.GetExtension(xls_patch);
            if (ext == ".xml" || ext == ".XML")
            {
                return true;
            }
            else if (ext == ".xls" || ext == ".XLS")
            {
                str_conn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + xls_patch + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + xls_patch + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            }
            try
            {
                DataSet data_set = new DataSet();
                OleDbConnection ole_db_conn = new OleDbConnection(str_conn);
                OleDbDataAdapter ole_db_data_adapter = new OleDbDataAdapter("select * from [Sheet1$]", ole_db_conn);
                ole_db_data_adapter.Fill(data_set, "luoji_maijian_ziliao");
                data_set.WriteXml(xml_patch);
                return true;
            }
            catch
            {
                MessageBox.Show("文件格式异常，请检查数据或重启电脑再试！");
                return false;
            }
        }
        public static DataSet to_dataset(string file_path, string sql_excel)
        {

            string ext = Path.GetExtension(file_path);
            string str_conn = null;
            if (ext == ".xls")
            {
                str_conn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + file_path + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + file_path + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            }

            OleDbConnection conn = null;
            OleDbDataAdapter ole_db_data_adapter = null;
            DataSet data_set = new DataSet();
            try
            {
                conn = new OleDbConnection(str_conn);
                conn.Open();

                //sqlExcel = "select * from [sheet1$]";
                ole_db_data_adapter = new OleDbDataAdapter(sql_excel, conn);
                ole_db_data_adapter.Fill(data_set, "mjdata");
            }
            catch
            {
            }
            finally
            {
                // 关闭连接 
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    ole_db_data_adapter.Dispose();
                    conn.Dispose();
                }
            }
            return data_set;

        }

        //public static string get_xinghao_by_data(Hashtable maijian_data)
        //{
        //    string maijianxinghao = "";
        //    string sql = "select 埋件编号 from [Sheet1$] where 长=" + Convert.ToDouble(maijian_data["maoban_changdu"]).ToString("f0") + "and  宽=" + Convert.ToDouble(maijian_data["maoban_kuandu"]).ToString("f0") + "and  厚=" + Convert.ToDouble(maijian_data["maoban_houdu"]).ToString("f0") + "and 直径=" + Convert.ToDouble(maijian_data["maojin_zhijing"]).ToString("f0") + "and 长度=" + Convert.ToDouble(maijian_data["maojin_changdu"]).ToString("f0") +
        //                 "and TL=" + Convert.ToDouble(maijian_data["maojin_shuiping_bianju"]).ToString("f0") + "and TR=" + Convert.ToDouble(maijian_data["maojin_shuiping_bianjuc0"]).ToString("f0") + "and LT=" + Convert.ToDouble(maijian_data["maojin_chuizhi_bianju"]).ToString("f0") + "and LB=" + Convert.ToDouble(maijian_data["maojin_chuizhi_bianjuc10"]).ToString("f0") + "and LM=" + Convert.ToDouble(maijian_data["maojin_chuizhi_jianju"]).ToString("f1") + "and TM=" + Convert.ToDouble(maijian_data["maojin_shuiping_jianju"]).ToString("f1");
        //    DataSet xinghaoData = excelProcessor.to_dataset(@"C:\maijian_data\湖北院埋件图集数据表.xls", sql);
        //    try
        //    {
        //        maijianxinghao = xinghaoData.Tables[0].Rows[0][0].ToString();
        //    }
        //    catch (Exception) { }
        //    return maijianxinghao;
        //}

        //public static DataSet get_all_dataset()
        //{
        //    string sql = "select * from [Sheet1$]";
        //    return excelProcessor.to_dataset(@"C:\maijian_data\湖北院埋件图集数据表.xls", sql);
        //}

        //public static void create_excel(DataGridView data_grid_view, string file_name, string file_name_path = "")
        //{
        //    #region   验证可操作性
        //    string file_name_string = string.Empty;
        //    if (file_name_path == string.Empty)
        //    {
        //        //申明保存对话框      
        //        SaveFileDialog save_file_dialog = new SaveFileDialog();
        //        save_file_dialog.FileName = file_name;
        //        //默认文件后缀
        //        //dlg.DefaultExt = "xls";
        //        //文件后缀列表      
        //        //dlg.Filter = "EXCEL文件(*.XLS)|*.xls ";
        //        save_file_dialog.Filter = "Execl 2003 files (*.xls)|*.xls|Excel 2007 file (*.xlsx）|*.xlsx";
        //        //默认路径是系统当前路径      
        //        save_file_dialog.InitialDirectory = Directory.GetCurrentDirectory();
        //        //打开保存对话框      
        //        if (save_file_dialog.ShowDialog() == DialogResult.Cancel) return;
        //        //返回文件路径      
        //        file_name_string = save_file_dialog.FileName;
        //        //验证strFileName是否为空或值无效      
        //        if (file_name_string.Trim() == "")
        //        {
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        file_name_string = file_name_path + file_name;
        //    }
        //    //定义表格内数据的行数和列数      

        //    int rows_count = data_grid_view.Rows.Count;
        //    int cols_count = data_grid_view.Columns.Count;

        //    //行数必须大于0      
        //    if (rows_count < 1)
        //    {
        //        MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }

        //    //列数必须大于0      
        //    if (cols_count < 1)
        //    {
        //        MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }

        //    //行数不可以大于65536      
        //    if (rows_count > 65536)
        //    {
        //        MessageBox.Show("数据记录数太多(最多不能超过65536条)，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }

        //    //列数不可以大于255      
        //    if (cols_count > 255)
        //    {
        //        MessageBox.Show("数据记录行数太多，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }

        //    //验证以fileNameString命名的文件是否存在，如果存在删除它      
        //    FileInfo file_info = new FileInfo(file_name_string);
        //    if (file_info.Exists)
        //    {
        //        try
        //        {
        //            DialogResult dialog_result = MessageBox.Show("文件\"" + file_info + "\"已存在，是否删除?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
        //            if (dialog_result == DialogResult.OK)
        //            {
        //                file_info.Delete();
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        }
        //        catch (Exception error)
        //        {
        //            MessageBox.Show(error.Message, "删除失败 ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }
        //    }
        //    #endregion

        //    Workbook work_book_obj = null;
        //    Worksheet sheet_obj = null;

        //    //新建sheet数量
        //    int sheet_shuliang = 1;
        //    //申明对象
        //    _Application excel_obj = new Microsoft.Office.Interop.Excel.Application();
        //    work_book_obj = excel_obj.Workbooks.Add(Missing.Value);

        //    Worksheet sheet = (Worksheet)work_book_obj.Sheets.get_Item("Sheet1");
        //    work_book_obj.Sheets.Add(sheet, Type.Missing, sheet_shuliang, Type.Missing);
        //    sheet_obj = (Worksheet)work_book_obj.Worksheets[1];
        //    //设置EXCEL不可见      
        //    excel_obj.Visible = false;

        //    int row_index = 1;
        //    //设置格式
        //    excel_obj.StandardFont = "华文中宋";
        //    excel_obj.StandardFontSize = 10;

        //    //向Excel中写入表格的表头 
        //    for (int i = 0; i < data_grid_view.Columns.Count; i++)
        //    {
        //        try {
        //            excel_obj.Cells[1, i + 1] = Convert.ToString(data_grid_view.Columns[i].HeaderText.Trim());
        //        }
        //        catch
        //        {
        //            MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
        //            return;
        //        }
        //    }


        //    //向Excel中逐行逐列写入表格中的数据      
        //    for (int row = 0; row <= data_grid_view.RowCount - 1; row++)
        //    {
        //        //displayColumnsCount的值为数据开始的列
        //        int display_columns_count = 1;
        //        for (int col = 0; col < cols_count; col++)
        //        {
        //           try
        //           {
        //                sheet_obj.Cells[row + 2, display_columns_count] = data_grid_view.Rows[row].Cells[col].Value.ToString().Trim();
        //                display_columns_count++;
        //            }
        //            catch (Exception e)
        //            {
        //                MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
        //                return;
        //            }
        //        }
        //    }

        //    //宽度自适应
        //    sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].EntireColumn.AutoFit();
        //    sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
        //    sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + 1, cols_count]].Borders.LineStyle = 1;
        //    string ext = Path.GetExtension(file_name_string);
        //    //保存文件   
        //    try
        //    {
        //        if (ext == ".xls")
        //        {
        //            ((Worksheet)work_book_obj.Worksheets[1]).Name = file_name;
        //            work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
        //            Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
        //        }
        //        else
        //        {
        //            ((Worksheet)work_book_obj.Worksheets[1]).Name = file_name;
        //            work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
        //            Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
        //        }
        //    }
        //    catch(Exception e)
        //    {
        //        MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
        //        return;
        //    }

        //    if (MessageBox.Show(file_name_string + "\n\n导出完毕!\n\n是否需要查看该文件？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //    {
        //        excel_obj.Visible = true;
        //    }
        //}

        public static void create_excel(DataGridView data_grid_view, string file_name, string sheet_name, string file_name_path = "")
        {
            #region   验证可操作性
            string file_name_string = string.Empty;
            if (file_name_path == string.Empty)
            {
                //申明保存对话框      
                SaveFileDialog save_file_dialog = new SaveFileDialog();
                save_file_dialog.FileName = file_name;
                //默认文件后缀
                //dlg.DefaultExt = "xls";
                //文件后缀列表      
                //dlg.Filter = "EXCEL文件(*.XLS)|*.xls ";
                save_file_dialog.Filter = "Execl 2003 files (*.xls)|*.xls|Excel 2007 file (*.xlsx）|*.xlsx";
                //默认路径是系统当前路径      
                save_file_dialog.InitialDirectory = Directory.GetCurrentDirectory();
                //打开保存对话框      
                if (save_file_dialog.ShowDialog() == DialogResult.Cancel) return;
                //返回文件路径      
                file_name_string = save_file_dialog.FileName;
                //验证strFileName是否为空或值无效      
                if (file_name_string.Trim() == "")
                {
                    return;
                }
            }
            else
            {
                file_name_string = file_name_path + file_name;
            }
            //定义表格内数据的行数和列数      

            int rows_count = data_grid_view.Rows.Count;
            int cols_count = data_grid_view.Columns.Count;

            //行数必须大于0      
            if (rows_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数必须大于0      
            if (cols_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //行数不可以大于65536      
            if (rows_count > 65536)
            {
                MessageBox.Show("数据记录数太多(最多不能超过65536条)，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数不可以大于255      
            if (cols_count > 255)
            {
                MessageBox.Show("数据记录行数太多，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //验证以fileNameString命名的文件是否存在，如果存在删除它      
            FileInfo file_info = new FileInfo(file_name_string);
            if (file_info.Exists)
            {
                try
                {
                    DialogResult dialog_result = MessageBox.Show("文件\"" + file_info + "\"已存在，是否删除?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog_result == DialogResult.OK)
                    {
                        file_info.Delete();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "删除失败 ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            Workbook work_book_obj = null;
            Worksheet sheet_obj = null;

            //新建sheet数量
            int sheet_shuliang = 1;
            //申明对象
            _Application excel_obj = new Microsoft.Office.Interop.Excel.Application();
            work_book_obj = excel_obj.Workbooks.Add(Missing.Value);

            Worksheet sheet = (Worksheet)work_book_obj.Sheets.get_Item("Sheet1");
            work_book_obj.Sheets.Add(sheet, Type.Missing, sheet_shuliang, Type.Missing);
            sheet_obj = (Worksheet)work_book_obj.Worksheets[1];
            //设置EXCEL不可见      
            excel_obj.Visible = false;

            int row_index = 1;
            //设置格式
            excel_obj.StandardFont = "华文中宋";
            excel_obj.StandardFontSize = 10;

            //向Excel中写入表格的表头 
            for (int i = 0; i < data_grid_view.Columns.Count; i++)
            {
                try
                {
                    excel_obj.Cells[1, i + 1] = Convert.ToString(data_grid_view.Columns[i].HeaderText.Trim());
                }
                catch
                {
                    MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                    return;
                }
            }


            //向Excel中逐行逐列写入表格中的数据      
            for (int row = 0; row <= data_grid_view.RowCount - 1; row++)
            {
                //displayColumnsCount的值为数据开始的列
                int display_columns_count = 1;
                for (int col = 0; col < cols_count; col++)
                {
                    try
                    {
                        sheet_obj.Cells[row + 2, display_columns_count] = data_grid_view.Rows[row].Cells[col].Value.ToString().Trim();
                        display_columns_count++;
                    }
                    catch (Exception ee)
                    {
                        MessageBox.Show("出现错误"+ee.Message+"，请检查是否为Excel未激活或者其他错误");
                        return;
                    }
                }
            }

            //宽度自适应
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].EntireColumn.AutoFit();
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + 1, cols_count]].Borders.LineStyle = 1;
            string ext = Path.GetExtension(file_name_string);
            //保存文件   
            try
            {
                if (ext == ".xls")
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                else
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                return;
            }

            if (MessageBox.Show(file_name_string + "\n\n导出完毕!\n\n是否需要查看该文件？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                excel_obj.Visible = true;
            }
        }
        public static void create_excel(System.Data.DataTable data_grid_view, string file_name, string sheet_name, string file_name_path = "")
        {
            #region   验证可操作性
            string file_name_string = string.Empty;
            if (file_name_path == string.Empty)
            {
                //申明保存对话框      
                SaveFileDialog save_file_dialog = new SaveFileDialog();
                save_file_dialog.FileName = file_name;
                //默认文件后缀
                //dlg.DefaultExt = "xls";
                //文件后缀列表      
                //dlg.Filter = "EXCEL文件(*.XLS)|*.xls ";
                save_file_dialog.Filter = "Execl 2003 files (*.xls)|*.xls|Excel 2007 file (*.xlsx）|*.xlsx";
                //默认路径是系统当前路径      
                save_file_dialog.InitialDirectory = Directory.GetCurrentDirectory();
                //打开保存对话框      
                if (save_file_dialog.ShowDialog() == DialogResult.Cancel) return;
                //返回文件路径      
                file_name_string = save_file_dialog.FileName;
                //验证strFileName是否为空或值无效      
                if (file_name_string.Trim() == "")
                {
                    return;
                }
            }
            else
            {
                file_name_string = file_name_path + file_name;
            }
            //定义表格内数据的行数和列数      

            int rows_count = data_grid_view.Rows.Count;
            int cols_count = data_grid_view.Columns.Count;

            //行数必须大于0      
            if (rows_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数必须大于0      
            if (cols_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //行数不可以大于65536      
            if (rows_count > 65536)
            {
                MessageBox.Show("数据记录数太多(最多不能超过65536条)，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数不可以大于255      
            if (cols_count > 255)
            {
                MessageBox.Show("数据记录行数太多，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //验证以fileNameString命名的文件是否存在，如果存在删除它      
            FileInfo file_info = new FileInfo(file_name_string);
            if (file_info.Exists)
            {
                try
                {
                    DialogResult dialog_result = MessageBox.Show("文件\"" + file_info + "\"已存在，是否删除?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog_result == DialogResult.OK)
                    {
                        file_info.Delete();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "删除失败 ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            Workbook work_book_obj = null;
            Worksheet sheet_obj = null;

            //新建sheet数量
            int sheet_shuliang = 1;
            //申明对象
            _Application excel_obj = new Microsoft.Office.Interop.Excel.Application();
            work_book_obj = excel_obj.Workbooks.Add(Missing.Value);

            Worksheet sheet = (Worksheet)work_book_obj.Sheets.get_Item("Sheet1");
            work_book_obj.Sheets.Add(sheet, Type.Missing, sheet_shuliang, Type.Missing);
            sheet_obj = (Worksheet)work_book_obj.Worksheets[1];
            //设置EXCEL不可见      
            excel_obj.Visible = false;

            int row_index = 1;
            //设置格式
            excel_obj.StandardFont = "华文中宋";
            excel_obj.StandardFontSize = 10;

            //向Excel中写入表格的表头 
            for (int i = 0; i < data_grid_view.Columns.Count; i++)
            {
                try
                {
                    excel_obj.Cells[1, i + 1] = Convert.ToString(data_grid_view.Columns[i]);
                }
                catch
                {
                    MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                    return;
                }
            }

            //向Excel中逐行逐列写入表格中的数据      
            for (int row = 0; row <= data_grid_view.Rows.Count - 1; row++)
            {
                //displayColumnsCount的值为数据开始的列
                int display_columns_count = 1;
                for (int col = 0; col < cols_count; col++)
                {
                    try
                    {
                        sheet_obj.Cells[row + 2, display_columns_count] = data_grid_view.Rows[row][col].ToString().Trim();
                        display_columns_count++;
                    }
                    catch
                    {
                        MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                        return;
                    }
                }
            }
            //宽度自适应
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].EntireColumn.AutoFit();
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + 1, cols_count]].Borders.LineStyle = 1;
            string ext = Path.GetExtension(file_name_string);
            //保存文件   
            try
            {
                if (ext == ".xls")
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                else
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
            }
            catch
            {
                MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                return;
            }

            if (MessageBox.Show(file_name_string + "\n\n导出完毕!\n\n是否需要查看该文件？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                excel_obj.Visible = true;
            }
        }
        public static void create_excel(System.Data.DataTable data_grid_view, string file_name, string sheet_name, int counts, string file_name_path = "")
        {
            #region   验证可操作性
            string file_name_string = string.Empty;
            if (file_name_path == string.Empty)
            {
                //申明保存对话框      
                SaveFileDialog save_file_dialog = new SaveFileDialog();
                save_file_dialog.FileName = file_name;
                //默认文件后缀
                //dlg.DefaultExt = "xls";
                //文件后缀列表      
                //dlg.Filter = "EXCEL文件(*.XLS)|*.xls ";
                save_file_dialog.Filter = "Execl 2003 files (*.xls)|*.xls|Excel 2007 file (*.xlsx）|*.xlsx";
                //默认路径是系统当前路径      
                save_file_dialog.InitialDirectory = Directory.GetCurrentDirectory();
                //打开保存对话框      
                if (save_file_dialog.ShowDialog() == DialogResult.Cancel) return;
                //返回文件路径      
                file_name_string = save_file_dialog.FileName;
                //验证strFileName是否为空或值无效      
                if (file_name_string.Trim() == "")
                {
                    return;
                }
            }
            else
            {
                file_name_string = file_name_path + file_name;
            }
            //定义表格内数据的行数和列数      

            int rows_count = data_grid_view.Rows.Count;
            int cols_count = data_grid_view.Columns.Count;

            //行数必须大于0      
            if (rows_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数必须大于0      
            if (cols_count < 1)
            {
                MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //行数不可以大于65536      
            if (rows_count > 65536)
            {
                MessageBox.Show("数据记录数太多(最多不能超过65536条)，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //列数不可以大于255      
            if (cols_count > 255)
            {
                MessageBox.Show("数据记录行数太多，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //验证以fileNameString命名的文件是否存在，如果存在删除它      
            FileInfo file_info = new FileInfo(file_name_string);
            if (file_info.Exists)
            {
                try
                {
                    DialogResult dialog_result = MessageBox.Show("文件\"" + file_info + "\"已存在，是否删除?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog_result == DialogResult.OK)
                    {
                        file_info.Delete();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "删除失败 ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            Workbook work_book_obj = null;
            Worksheet sheet_obj = null;

            //新建sheet数量
            int sheet_shuliang = 1;
            //申明对象
            _Application excel_obj = new Microsoft.Office.Interop.Excel.Application();
            work_book_obj = excel_obj.Workbooks.Add(Missing.Value);

            Worksheet sheet = (Worksheet)work_book_obj.Sheets.get_Item("Sheet1");
            work_book_obj.Sheets.Add(sheet, Type.Missing, sheet_shuliang, Type.Missing);
            sheet_obj = (Worksheet)work_book_obj.Worksheets[1];
            //设置EXCEL不可见      
            excel_obj.Visible = false;

            int row_index = 1;
            //设置格式
            excel_obj.StandardFont = "华文中宋";
            excel_obj.StandardFontSize = 10;

            //向Excel中写入表格的表头 
            for (int i = 0; i < data_grid_view.Columns.Count; i++)
            {
                try
                {
                    excel_obj.Cells[1, i + 1] = Convert.ToString(data_grid_view.Columns[i]);
                }
                catch
                {
                    MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                    return;
                }
            }

            //向Excel中逐行逐列写入表格中的数据      
            for (int row = 0; row <= data_grid_view.Rows.Count - 1; row++)
            {
                //displayColumnsCount的值为数据开始的列
                int display_columns_count = 1;
                for (int col = 0; col < cols_count; col++)
                {
                    try
                    {
                        sheet_obj.Cells[row + 2, display_columns_count] = data_grid_view.Rows[row][col].ToString().Trim();
                        display_columns_count++;
                    }
                    catch
                    {
                        MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                        return;
                    }
                }
            }
            Range range = (Range)sheet_obj.get_Range("A2", "A" + counts);
            ///合并方法，0的时候直接合并为一个单元格  
            range.Merge(0);
            //宽度自适应
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].EntireColumn.AutoFit();
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + row_index, cols_count]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
            sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[rows_count + 1, cols_count]].Borders.LineStyle = 1;
            string ext = Path.GetExtension(file_name_string);
            //保存文件   
            try
            {
                if (ext == ".xls")
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
                else
                {
                    ((Worksheet)work_book_obj.Worksheets[1]).Name = sheet_name;
                    work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                    Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                return;
            }

            if (MessageBox.Show(file_name_string + "\n\n导出完毕!\n\n是否需要查看该文件？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                excel_obj.Visible = true;
            }
        }
        public static void create_excel(DataSet ds, string file_name, string file_name_path = "")
        {
            #region   验证可操作性
            string file_name_string = string.Empty;
            if (file_name_path == string.Empty)
            {
                //申明保存对话框      
                SaveFileDialog save_file_dialog = new SaveFileDialog();
                save_file_dialog.FileName = file_name;
                //默认文件后缀
                //dlg.DefaultExt = "xls";
                //文件后缀列表      
                //dlg.Filter = "EXCEL文件(*.XLS)|*.xls ";
                save_file_dialog.Filter = "Execl 2003 files (*.xls)|*.xls|Excel 2007 file (*.xlsx）|*.xlsx";
                //默认路径是系统当前路径      
                save_file_dialog.InitialDirectory = Directory.GetCurrentDirectory();
                //打开保存对话框      
                if (save_file_dialog.ShowDialog() == DialogResult.Cancel) return;
                //返回文件路径      
                file_name_string = save_file_dialog.FileName;
                //验证strFileName是否为空或值无效      
                if (file_name_string.Trim() == "")
                {
                    return;
                }
            }
            else
            {
                file_name_string = file_name_path + file_name;
            }
            //定义表格内数据的行数和列数      
            int rows_count = 0;
            int cols_count = 0;
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                rows_count = ds.Tables[i].Rows.Count;
                cols_count = ds.Tables[i].Columns.Count;
                //行数必须大于0      
                if (rows_count < 1)
                {
                    MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //列数必须大于0      
                if (cols_count < 1)
                {
                    MessageBox.Show("没有数据可供保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //行数不可以大于65536      
                if (rows_count > 65536)
                {
                    MessageBox.Show("数据记录数太多(最多不能超过65536条)，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //列数不可以大于255      
                if (cols_count > 255)
                {
                    MessageBox.Show("数据记录行数太多，不能保存 ", "提示 ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }




            //验证以fileNameString命名的文件是否存在，如果存在删除它      
            FileInfo file_info = new FileInfo(file_name_string);
            if (file_info.Exists)
            {
                try
                {
                    DialogResult dialog_result = MessageBox.Show("文件\"" + file_info + "\"已存在，是否删除?", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dialog_result == DialogResult.OK)
                    {
                        file_info.Delete();
                    }
                    else
                    {
                        return;
                    }
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "删除失败 ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            #endregion

            Workbook work_book_obj = null;
            Worksheet sheet_obj = null;

            //申明对象
            _Application excel_obj = new Microsoft.Office.Interop.Excel.Application();
            work_book_obj = excel_obj.Workbooks.Add(Missing.Value);

            //创建excel的sheet名称
            List<string> SheetNames = new List<string>();
            SheetNames.Add("气候信息表");
            SheetNames.Add("辐射信息表");
            for (int i = 0; i < ds.Tables.Count; i++)
            {
                work_book_obj.Worksheets.Add(); //添加新的sheet到excel中

            }

            //设置EXCEL不可见      
            excel_obj.Visible = false;

            //int row_index = 1;
            //设置格式
            excel_obj.StandardFont = "华文中宋";
            excel_obj.StandardFontSize = 10;

            for (int i = 0; i < ds.Tables.Count; i++)
            {
                int r = 1; // 初始化excel的第一行Position=1
                sheet_obj = (Microsoft.Office.Interop.Excel.Worksheet)work_book_obj.Worksheets[i + 1];

                //把列的名字写进sheet内
                for (int col = 1; col <= ds.Tables[i].Columns.Count; col++)
                    sheet_obj.Cells[r, col] = ds.Tables[i].Columns[col - 1].ColumnName;
                r++;

                //把每一行写进excel的sheet中
                for (int row = 0; row < ds.Tables[i].Rows.Count; row++) //r是excelRow，col是excelColumn
                {
                    //Excel的行和列开始位置写Row=1 ，Col=1
                    for (int col = 1; col <= ds.Tables[i].Columns.Count; col++)
                        sheet_obj.Cells[r, col] = ds.Tables[i].Rows[row][col - 1].ToString();
                    r++;
                }
                sheet_obj.Name = SheetNames[i];
                //Range range = (Range)sheet_obj.get_Range("A2", "A10");
                /////合并方法，0的时候直接合并为一个单元格  
                //range.Merge(0);
                //宽度自适应
                sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[ds.Tables[i].Rows.Count + 1, ds.Tables[i].Columns.Count]].EntireColumn.AutoFit();
                sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[ds.Tables[i].Rows.Count + 1, ds.Tables[i].Columns.Count]].HorizontalAlignment = Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;//居中对齐
                sheet_obj.Range[sheet_obj.Cells[1, 1], sheet_obj.Cells[ds.Tables[i].Rows.Count + 1, ds.Tables[i].Columns.Count]].Borders.LineStyle = 1;
                string ext = Path.GetExtension(file_name_string);
                //保存文件   
                try
                {
                    if (ext == ".xls")
                    {
                        work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                        Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                    }
                    else
                    {
                        work_book_obj.SaveAs(file_name_string, XlFileFormat.xlWorkbookNormal, Missing.Value, Missing.Value, Missing.Value,
                        Missing.Value, XlSaveAsAccessMode.xlShared, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                    }
                }
                catch
                {
                    MessageBox.Show("出现错误，请检查是否为Excel未激活或者其他错误");
                    return;
                }

            }
            if (MessageBox.Show(file_name_string + "\n\n导出完毕!\n\n是否需要查看该文件？ ", "提示", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                excel_obj.Visible = true;
            }
        }
        public static DataSet get_excel_ds()
        {
            DataSet ds = new DataSet();
            OpenFileDialog open_file_dialog = new OpenFileDialog();
            open_file_dialog.Title = "请选择要导入的Excel文件";
            open_file_dialog.Filter = "Excel 2007/2010 file (*.xlsx）|*.xlsx|Execl 2003 files (*.xls)|*.xls";
            string str_conn = null;
            if (open_file_dialog.ShowDialog() == DialogResult.OK)
            {
                string file_name = open_file_dialog.FileName;
                string file_type = System.IO.Path.GetExtension(file_name);
                if (file_type == ".xls")
                {
                    str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + file_name + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
                }
                else
                {
                    str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + file_name + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
                }
                //建立连接 
                OleDbConnection conn = new OleDbConnection(str_conn);
                try
                {
                    //打开连接
                    if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    System.Data.DataTable dt = null;
                    dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    List<string> list_sheet_name = new List<string>();
                    ArrayList arrayNames = new ArrayList();
                    bool flag = false;
                    foreach (DataRow dr in dt.Rows)
                    {
                        list_sheet_name.Add(dr["TABLE_NAME"].ToString().Trim());
                    }
                    if (list_sheet_name.Contains("气候信息表$") && list_sheet_name.Contains("辐射信息表$"))
                    {
                        arrayNames.Add("气候信息表");
                        arrayNames.Add("辐射信息表");
                        for (int i = 0; i < arrayNames.Count; i++)
                        {
                            str_conn = "select * from [" + arrayNames[i].ToString() + "$]";
                            OleDbDataAdapter myCommand = new OleDbDataAdapter(str_conn, conn);

                            dt = new System.Data.DataTable(arrayNames[i].ToString());
                            myCommand.Fill(dt);
                            ds.Tables.Add(dt);
                        }
                        flag = true;
                    }
                    if (!flag)
                    {
                        MessageBox.Show("excel模板格式错误！请将表1更名为'气候信息表',表2更名为'辐射信息表'");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导入的Excel的格式错误，请检查是否符合要求！" + ex.ToString());
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
                return ds;
            }
            else
            {
                return ds;
            }
        }
        public static System.Data.DataTable get_Excel_guanxian_data(string wenjian_lujing,string guanxian_leixing)
        {
            System.Data.DataTable data_table = null;           
            string str_conn = null;

            string file_type = System.IO.Path.GetExtension(wenjian_lujing);
            if (file_type == ".xls")
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + wenjian_lujing + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + wenjian_lujing + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            }
            //建立连接 
            OleDbConnection conn = new OleDbConnection(str_conn);
            try
            {
                //打开连接
                if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }


                System.Data.DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                //获取Excel的第一个Sheet名称
                bool flag = false;
                foreach (DataRow dr in schemaTable.Rows)
                {
                    if (Convert.ToString(dr["TABLE_NAME"]) == guanxian_leixing+"$")
                    {
                        flag = true;
                        string sheetName = dr["TABLE_NAME"].ToString().Trim();
                        //查询sheet中的数据
                        string strSql = "select * from [" + sheetName + "]";
                        OleDbDataAdapter da = new OleDbDataAdapter(strSql, conn);
                        DataSet ds = new DataSet();
                        da.Fill(ds, sheetName);
                        data_table = ds.Tables[sheetName];
                    }
                }
                if (!flag)
                {
                    MessageBox.Show("excel模板格式错误！找不到表名："+ guanxian_leixing);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("导入的Excel的格式错误，请检查是否符合要求！" + ex.ToString());
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
            return data_table;



        }

        public static System.Data.DataTable get_Excel_cedian_data(string wenjian_lujing)
        {
            System.Data.DataTable data_table = null;
            string str_conn = null;

            string file_type = System.IO.Path.GetExtension(wenjian_lujing);
            if (file_type == ".xls")
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + wenjian_lujing + ";" + ";Extended Properties=\"Excel 8.0;HDR=YES;IMEX=1\"";
            }
            else
            {
                str_conn = "Provider=Microsoft.ACE.OLEDB.12.0;" + "Data Source=" + wenjian_lujing + ";" + ";Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"";
            }
            //建立连接 
            OleDbConnection conn = new OleDbConnection(str_conn);
            try
            {
                //打开连接
                if (conn.State == ConnectionState.Broken || conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }


                System.Data.DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                //获取Excel的第一个Sheet名称
                //bool flag = false;
                foreach (DataRow dr in schemaTable.Rows)
                {
                    string sheetName = dr["TABLE_NAME"].ToString().Trim();
                    //查询sheet中的数据
                    string strSql = "select * from [" + sheetName + "]";
                    OleDbDataAdapter da = new OleDbDataAdapter(strSql, conn);
                    DataSet ds = new DataSet();
                    da.Fill(ds, sheetName);
                    data_table = ds.Tables[sheetName];
                    return data_table;

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("导入的Excel的格式错误，请检查是否符合要求！" + ex.ToString());
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }

            return data_table;

        }


    }
}
