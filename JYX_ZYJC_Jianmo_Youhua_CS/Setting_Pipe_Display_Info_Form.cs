using Bentley.OpenPlant.Modeler.Api;
using Bentley.Interop.MicroStationDGN;
using Bentley.MstnPlatformNET.InteropServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class Setting_Pipe_Display_Info_Form : Form
    {
        private static Bentley.Interop.MicroStationDGN.Application app = Utilities.ComApp;
        private static string path0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //C:\ProgramData\Bentley\OpenPlant CONNECT Edition\Configuration\Workspaces\WorkSpace\WorkSets\OpenPlantMetric\Standards\OpenPlant
        public int pipe_display_info = 0;
        public string path = string.Empty;
        public string readPath = string.Empty;
        public Setting_Pipe_Display_Info_Form()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 窗体加载时读取xml文件给窗体添加默认值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Setting_Pipe_Display_Info_Form_Load(object sender, EventArgs e)
        {
            #region
            //this.checkBox_top_level.Checked = true;
            //this.checkBox_top_level.Tag = (int)pipe_level_display_info.Top_Level;
            //this.checkBox_bottom_level.Checked = true;
            //this.checkBox_bottom_level.Tag= (int)pipe_level_display_info.Bottom_Level;
            //this.checkBox_center_level.Checked = true;
            //this.checkBox_center_level.Tag = (int)pipe_level_display_info.Center_Level;
            //this.checkBox_inside_bottom_level.Checked = true;
            //this.checkBox_inside_bottom_level.Tag = (int)pipe_level_display_info.Inside_Bottom_Level;
            //pipe_display_info = (int)this.checkBox_top_level.Tag | (int)this.checkBox_bottom_level.Tag | (int)this.checkBox_center_level.Tag | (int)this.checkBox_inside_bottom_level.Tag;
            #endregion

            pipe_display_info = xmlReade();
            int checkBox1 = pipe_display_info & (int)pipe_level_display_info.Top_Level;
            if (checkBox1 != 0)
                checkBox_top_level.Checked = true;
            else
                checkBox_top_level.Checked = false;
            int checkBox2 = pipe_display_info & (int)pipe_level_display_info.Bottom_Level;
            if (checkBox2 != 0)
                checkBox_bottom_level.Checked = true;
            else
                checkBox_bottom_level.Checked = false;
            int checkBox3 = pipe_display_info & (int)pipe_level_display_info.Center_Level;
            if (checkBox3 != 0)
                checkBox_center_level.Checked = true;
            else
                checkBox_center_level.Checked = false;
            int checkBox4 = pipe_display_info & (int)pipe_level_display_info.Inside_Bottom_Level;
            if (checkBox4 != 0)
                checkBox_inside_bottom_level.Checked = true;
            else
                checkBox_inside_bottom_level.Checked = false;
        }

        /// <summary>
        /// 保存按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            #region
            //pipe_display_info = 0;
            //if (this.checkBox_top_level.Checked)

            //{
            //    pipe_display_info |= (int)this.checkBox_top_level.Tag;
            //}
            //if (this.checkBox_bottom_level.Checked)
            //{
            //    pipe_display_info |= (int)this.checkBox_bottom_level.Tag;
            //}
            //if (this.checkBox_center_level.Checked)
            //{
            //    pipe_display_info |= (int)this.checkBox_center_level.Tag;
            //}
            //if (this.checkBox_inside_bottom_level.Checked)
            //{
            //    pipe_display_info |= (int)this.checkBox_inside_bottom_level.Tag;
            //}
            #endregion
            save_parameters_setting_value();
        }

        /// <summary>
        /// 将参数保存在xml中，并执行change_display_pipe_level_info方法
        /// </summary>
        public void save_parameters_setting_value()
        {
            #region
            //Parameters_Setting.display_pipe_top_level = this.checkBox_top_level.Checked;
            //Parameters_Setting.display_pipe_bottom_level = this.checkBox_bottom_level.Checked;
            //Parameters_Setting.display_pipe_center_level = this.checkBox_center_level.Checked;
            //Parameters_Setting.display_pipe_inside_bottom_level = this.checkBox_inside_bottom_level.Checked;
            #endregion
            pipe_display_info = 0;
            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xml_Declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xml_Declaration);

            XmlElement rootElement = xmlDoc.CreateElement("DataParameters");
            xmlDoc.AppendChild(rootElement);

            XmlElement xmlPipeDisplay = xmlDoc.CreateElement("PipeDisplay");
            rootElement.AppendChild(xmlPipeDisplay);

            XmlElement xmlTop = xmlDoc.CreateElement("top_level");
            if (checkBox_top_level.Checked)
            {
                xmlTop.InnerText = Convert.ToString((int)pipe_level_display_info.Top_Level);
                pipe_display_info |= (int)pipe_level_display_info.Top_Level;
            }
            else
            {
                xmlTop.InnerText = Convert.ToString(0);
            }
            xmlPipeDisplay.AppendChild(xmlTop);

            XmlElement xmlBottom = xmlDoc.CreateElement("bottom_level");
            if (checkBox_bottom_level.Checked)
            {
                xmlBottom.InnerText = Convert.ToString((int)pipe_level_display_info.Bottom_Level);
                pipe_display_info |= (int)pipe_level_display_info.Bottom_Level;
            }
            else
            {
                xmlBottom.InnerText = Convert.ToString(0);
            }
            xmlPipeDisplay.AppendChild(xmlBottom);

            XmlElement xmlCenter = xmlDoc.CreateElement("center_Level");
            if (checkBox_center_level.Checked)
            {
                xmlCenter.InnerText = Convert.ToString((int)pipe_level_display_info.Center_Level);
                pipe_display_info |= (int)pipe_level_display_info.Center_Level;
            }
            else
            {
                xmlCenter.InnerText = Convert.ToString(0);
            }
            xmlPipeDisplay.AppendChild(xmlCenter);

            XmlElement xmlInsideBottom = xmlDoc.CreateElement("inside_bottom_level");
            if (checkBox_inside_bottom_level.Checked)
            {
                xmlInsideBottom.InnerText = Convert.ToString((int)pipe_level_display_info.Inside_Bottom_Level);
                pipe_display_info |= (int)pipe_level_display_info.Inside_Bottom_Level;
            }
            else
            {
                xmlInsideBottom.InnerText = Convert.ToString(0);
            }
            xmlPipeDisplay.AppendChild(xmlInsideBottom);

            DialogResult res = MessageBox.Show("确定保存吗？", "提示", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                path = path0.Replace("\\", "/");
                xmlDoc.Save(path + "/OPM_JYXConfig/PipeDisplay.xml");
                
            }
        }

        /// <summary>
        /// 读xml文化返回得到的二进制|运算后的值
        /// </summary>
        /// <returns></returns>
        public static int xmlReade()
        {
            int pipe_display_info = 0;
            if (!File.Exists(path0 + "\\OPM_JYXConfig\\PipeDisplay.xml"))
            {
                pipe_display_info = (int)pipe_level_display_info.Top_Level | (int)pipe_level_display_info.Center_Level | (int)pipe_level_display_info.Bottom_Level | (int)pipe_level_display_info.Inside_Bottom_Level;
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path0 + "\\OPM_JYXConfig\\PipeDisplay.xml");

                XmlNode xn = doc.SelectSingleNode("DataParameters").SelectSingleNode("PipeDisplay");
                XmlNodeList xnDisplay = xn.ChildNodes;

                pipe_display_info = Convert.ToInt32(xnDisplay.Item(0).InnerText) | Convert.ToInt32(xnDisplay.Item(1).InnerText) | Convert.ToInt32(xnDisplay.Item(2).InnerText) | Convert.ToInt32(xnDisplay.Item(3).InnerText);
            }
            return pipe_display_info;
        }
        /// <summary>
        /// 取消按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
