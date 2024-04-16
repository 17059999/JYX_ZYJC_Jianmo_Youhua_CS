using Bentley.OpenPlant.Modeler.Api;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.ECObjects.Instance;
using Bentley.ECObjects.Schema;
using System.Xml;
using System.IO;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ReportFLPZForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static ReportFLPZForm pzFrom = null;
        public static List<string> pzGxList = new List<string>();
        public static string path0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public bool isQX = true;
        public ReportFLPZForm()
        {
            InitializeComponent();
            //BMECInstanceManager.Instance.GetChildClasses
        }

        public static ReportFLPZForm instence(List<string> gxList)
        {
            pzGxList = gxList;
            if (pzFrom == null)
                pzFrom = new ReportFLPZForm();
            else
            {
                pzFrom.Close();
                pzFrom = new ReportFLPZForm();
            }
            return pzFrom;
        }

        private void ReportFLPZForm_Load(object sender, EventArgs e)
        {
            AddCheckBoxToDataGridView.dgv = dataGridView1;
            AddCheckBoxToDataGridView.AddFullSelect();

            List<string> allList = new List<string>();

            #region 获取PIPE_FLANGE下的子名称 一级没有更深级获取 如需要全部获取 需要递归
            IECSchema iecS = BMECInstanceManager.Instance.Schema;
            foreach (IECClass iecClass in iecS)
            {
                IECClass[] baseClass = iecClass.BaseClasses;
                if(baseClass!=null)
                {
                    foreach (IECClass iec in baseClass)
                    {
                        string ecName = iec.Name;
                        if (ecName.Equals("PIPE_FLANGE"))
                        {
                            if(!iecClass.Name.Equals("PIPE_FLANGE"))
                            {
                                if (allList.Count > 0)
                                {
                                    bool b = allList.Contains(iecClass.Name);
                                    if (!b)
                                    {
                                        allList.Add(iecClass.Name);
                                    }
                                }
                                else
                                {
                                    allList.Add(iecClass.Name);
                                }
                            }                            
                        }
                    }
                }                
            }
            #endregion
            #region 没有OpenPlant_3D_Placeable_Child_Classesxml
            //IECClass IECclass = iecS.GetClass("PIPING_COMPONENT");
            //IECInstance iECInstance = null;
            //IECInstance[] cusA = IECclass.GetCustomAttributes();
            //foreach(IECInstance iec in cusA)
            //{
            //    if(iec.ClassDefinition.Name.Equals("OpenPlant_3D_Placeable_Child_Classes"))
            //    {
            //        iECInstance = iec;
            //        break;
            //    }
            //}
            #endregion
            //System.Collections.ArrayList arList = BMECInstanceManager.Instance.GetChildClasses("PIPE_FLANGE");
            foreach (var ar in allList)
            {
                dataGridView1.Rows.Add(false, ar);
            }

            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    string ecName = dataGridView1.Rows[i].Cells["FlName"].Value.ToString();
                    if (pzGxList.Count > 0)
                    {
                        bool b = pzGxList.Contains(ecName);
                        if (b)
                        {
                            dataGridView1.Rows[i].Cells[0].Value = true;
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> pzList = new List<string>();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                bool b = Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value);
                if (b)
                {
                    pzList.Add(dataGridView1.Rows[i].Cells["FlName"].Value.ToString());
                }
            }

            string nameList = "";
            for (int i = 0; i < pzList.Count; i++)
            {
                if (i == 0)
                {
                    nameList = pzList[i];
                }
                else
                {
                    nameList = nameList + "," + pzList[i];
                }
            }
            bool b1 = saveFlXml(nameList);
            if (!b1)
            {
                MessageBox.Show("配置失败！");
            }
            if (isQX)
            {
                return;
            }

            ReportFLForm flForm = ReportFLForm.instence();
#if DEBUG

#else
            flForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportFLForm));
            flForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            flForm.Show();

            pzFrom.Close();
        }

        public bool saveFlXml(string name)
        {
            bool secc = true;
            string path1 = path0 + "\\OPM_JYXConfig"; //C: \Users\Lenovo\Documents\OPM_JYXConfig
            if (!Directory.Exists(path1))
            {
                System.Windows.Forms.MessageBox.Show("路径：" + path0 + "下没有找到OPM_JYXConfig文件");
                return false;
            }

            XmlDocument xmlDoc = new XmlDocument();

            XmlDeclaration xml_Declaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
            xmlDoc.AppendChild(xml_Declaration);

            XmlElement rootElement = xmlDoc.CreateElement("DataParameters");
            xmlDoc.AppendChild(rootElement);

            XmlElement xmlFlList = xmlDoc.CreateElement("FlNameList");
            rootElement.AppendChild(xmlFlList);

            XmlElement xmlFl = xmlDoc.CreateElement("FlName");
            xmlFl.InnerText = name;

            xmlFlList.AppendChild(xmlFl);

            DialogResult res = MessageBox.Show("确定保存吗？", "提示", MessageBoxButtons.OKCancel);
            if (res == DialogResult.OK)
            {
                string path = path1.Replace("\\", "/");
                xmlDoc.Save(path + "/FlPz.xml");
                isQX = false;
            }
            else
            {
                isQX = true;
            }

            return secc;
        }
    }
}
