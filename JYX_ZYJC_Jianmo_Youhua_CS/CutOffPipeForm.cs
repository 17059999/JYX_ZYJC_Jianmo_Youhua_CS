﻿using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class CutOffPipeForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        private CutOffPipeTool cutOffPipeTool = null;
        private string pipeLengthDefault = "6000";
        private static Bentley.Interop.MicroStationDGN.Application app = Utilities.ComApp;
        private static string path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\OPM_JYXConfig\CutOffPipeData.txt";/*app.ActiveWorkspace.ConfigurationVariableValue("OPENPLANT_WORKSET_STANDARDS")+@"\CutOffPipeData.txt"*//*;"D:\\Bentley\\opm5\\OpenPlantModeler"*/
        /// <summary>
        /// 构造
        /// </summary>
        public CutOffPipeForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="cutOffPipeTool"></param>
        public CutOffPipeForm(CutOffPipeTool cutOffPipeTool)
        {
            this.cutOffPipeTool = cutOffPipeTool;
            InitializeComponent();
            this.init();
        }
        /// <summary>
        /// 组件的一些初始化
        /// </summary>
        public void init()
        {
            if (System.IO.File.Exists(path))
            {
                string data = "";
                try
                {
                    data = System.IO.File.ReadAllText(path);
                }
                catch (Exception)
                {
                    throw;
                }
                if (data != "" && isPositiveInteger(data))
                {
                    this.textBox_length_pipe.Text = data;
                }
            }
            else
            {
                this.textBox_length_pipe.Text = this.pipeLengthDefault;
            }
        }
        private string str_textBox_length_pipe = "";//管道预制长度
        /// <summary>
        /// 调用断开管道方法，将预制长度传入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_ok_Click(object sender, EventArgs e)
        {
            //获取预制长度
            this.str_textBox_length_pipe = this.textBox_length_pipe.Text;
            if (null != str_textBox_length_pipe && str_textBox_length_pipe != "" && isPositiveInteger(str_textBox_length_pipe) /*isNumber(str_textBox_length_pipe)*/)
            {
                //调用断开管道方法
                double length = Convert.ToDouble(this.str_textBox_length_pipe);
                this.cutOffPipeTool.MyOnDataButton(length);
            }
            else
            {
                //请输入正确的管道长度
                MessageBox.Show("请输入正确的管道长度！");
            }
        }

        private void CutOffPipeForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //CutOffPipeTool.m_cutOffPipeForm = null;
            //Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            //app.CommandState.StartDefaultCommand();
            base.OnClosed(e);
            string data = this.textBox_length_pipe.Text;
            if (isPositiveInteger(data))
            {
                try
                {
                    System.IO.File.WriteAllText(path, data);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            CutOffPipeTool.m_cutOffPipeForm = null;
            if (CutOffPipeTool.isClosedFromCode == CutOffPipeTool.StatusCloseFormEvent.DEFAULT) CutOffPipeTool.isClosedFromCode = CutOffPipeTool.StatusCloseFormEvent.FORM;
            CutOffPipeTool.MyCleanUp();
        }

        /// <summary>
        /// 是否为正确的数字
        /// </summary>
        /// <param name="str">要检验的字符串</param>
        /// <returns>true：传入的字符串为数字</returns>
        public static bool isNumber(string str)
        {
            if (str == null) return false;
            return Regex.IsMatch(str, @"^[+]?\d*[.]?\d*$");
        }
        /// <summary>
        /// 不超过6位数的正整数
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool isPositiveInteger(string text)
        {
            bool bRet = false;
            //string pattern = @"^[1-9]+\d*$";
            string pattern = @"^[1-9]?\d{0,5}$";
            try
            {
                bRet = Regex.IsMatch(text, pattern);
            }
            catch (Exception)
            {
                throw;
            }
            return bRet;
        }
        public static bool isPositiveIntegerUnlimited(string text)
        {
            bool bRet = false;
            string pattern = @"^[1-9]?\d*$";
            try
            {
                bRet = Regex.IsMatch(text, pattern);
            }
            catch (Exception)
            {
                throw;
            }
            return bRet;
        }

    }
}
