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
using System.IO;
using System.Xml;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class ReportFLForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {

        public static ReportFLForm flForm = null;
        public static List<string> pzList = null;
        public static string path0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public ReportFLForm()
        {
            InitializeComponent();
        }

        public static ReportFLForm instence()
        {
            if (flForm == null)
                flForm = new ReportFLForm();
            else
            {
                flForm.Close();
                flForm = new ReportFLForm();
            }
            return flForm;
        }

        public static ReportFLForm instence(List<string> listStr)
        {
            pzList = listStr;
            if (flForm == null)
                flForm = new ReportFLForm();
            else
            {
                flForm.Close();
                flForm = new ReportFLForm();
            }
            return flForm;
        }

        //private void qxcheckBox1_CheckedChanged(object sender, EventArgs e)
        //{
        //    //bool b = qxcheckBox1.Checked;
        //    //A_AFA_Flange.Checked = b;
        //    //A_AFAB_Flange.Checked = b;
        //    //A_AFKB_Flange.Checked = b;
        //    //A_AFKV_Flange.Checked = b;
        //    //A_AFL_BW_Flange.Checked = b;
        //    //A_AFL_SCF_Flange.Checked = b;
        //    //A_AFL_SCM_Flange.Checked = b;
        //    //A_AFLA_Entirety_Flange.Checked = b;
        //    //A_AFLA_Flange.Checked = b;
        //    //A_AFW_Flange.Checked = b;
        //    //A_AFW_Flange_Incidental.Checked = b;
        //    //BLIND_FLANGE.Checked = b;
        //    //Centering_Entirety_Flange.Checked = b;
        //    //Centering_flange.Checked = b;
        //    //COMPANION_FLANGE.Checked = b;
        //    //EXPANDER_FLANGE.Checked = b;
        //    //FILLER_FLANGE.Checked = b;
        //    //JACKETED_FLANGE.Checked = b;
        //    //LAP_JOINT_FLANGE.Checked = b;
        //    //ORIFICE_FLANGE.Checked = b;
        //    //REDUCING_FLANGE.Checked = b;
        //    //SLIP_ON_FLANGE.Checked = b;
        //    //SOCKET_WELDED_FLANGE.Checked = b;
        //    //Square_Entirety_Flange.Checked = b;
        //    //Square_Flange.Checked = b;
        //    //THREADED_FLANGE.Checked = b;
        //    //VICTAULIC_FLANGE.Checked = b;
        //    //WELD_NECK_FLANGE.Checked = b;
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> ecNameList = new List<string>();
            //if (A_AFA_Flange.Checked)
            //    ecNameList.Add("A_AFA_Flange");
            //if (A_AFAB_Flange.Checked)
            //    ecNameList.Add("A_AFAB_Flange");
            //if (A_AFKB_Flange.Checked)
            //    ecNameList.Add("A_AFKB_Flange");
            //if (A_AFKV_Flange.Checked)
            //    ecNameList.Add("A_AFKV_Flange");
            //if (A_AFL_BW_Flange.Checked)
            //    ecNameList.Add("A_AFL_BW_Flange");
            //if (A_AFL_SCF_Flange.Checked)
            //    ecNameList.Add("A_AFL_SCF_Flange");
            //if (A_AFL_SCM_Flange.Checked)
            //    ecNameList.Add("A_AFL_SCM_Flange");
            //if (A_AFLA_Entirety_Flange.Checked)
            //    ecNameList.Add("A_AFLA_Entirety_Flange");
            //if (A_AFLA_Flange.Checked)
            //    ecNameList.Add("A_AFLA_Flange");
            //if (A_AFW_Flange.Checked)
            //    ecNameList.Add("A_AFW_Flange");
            //if (A_AFW_Flange_Incidental.Checked)
            //    ecNameList.Add("A_AFW_Flange_Incidental");
            //if (BLIND_FLANGE.Checked)
            //    ecNameList.Add("BLIND_FLANGE");
            //if (Centering_Entirety_Flange.Checked)
            //    ecNameList.Add("Centering_Entirety_Flange");
            //if(Centering_flange.Checked)
            //{
            //    ecNameList.Add("Centering_flange");
            //    ecNameList.Add("Centering_flange_Left");
            //}
            //if (COMPANION_FLANGE.Checked)
            //    ecNameList.Add("COMPANION_FLANGE");
            //if (EXPANDER_FLANGE.Checked)
            //    ecNameList.Add("EXPANDER_FLANGE");
            //if (FILLER_FLANGE.Checked)
            //{
            //    ecNameList.Add("FILLER_FLANGE");
            //    ecNameList.Add("REDUCING_FILLER_FLANGE");
            //}
            //if (JACKETED_FLANGE.Checked)
            //    ecNameList.Add("JACKETED_FLANGE");
            //if (LAP_JOINT_FLANGE.Checked)
            //    ecNameList.Add("LAP_JOINT_FLANGE");
            //if (ORIFICE_FLANGE.Checked)
            //    ecNameList.Add("ORIFICE_FLANGE");
            //if (REDUCING_FLANGE.Checked)
            //    ecNameList.Add("REDUCING_FLANGE");
            //if(SLIP_ON_FLANGE.Checked)
            //{
            //    ecNameList.Add("SLIP_ON_FLANGE");
            //    ecNameList.Add("REDUCING_SLIP_HY_ON_FLANGE");
            //}
            //if (SOCKET_WELDED_FLANGE.Checked)
            //    ecNameList.Add("SOCKET_WELDED_FLANGE");
            //if (Square_Entirety_Flange.Checked)
            //    ecNameList.Add("Square_Entirety_Flange");
            //if (Square_Flange.Checked)
            //{
            //    ecNameList.Add("Square_Flange");
            //    ecNameList.Add("Square_Flange_Left");
            //}
            //if(THREADED_FLANGE.Checked)
            //{
            //    ecNameList.Add("THREADED_FLANGE");
            //    ecNameList.Add("REDUCING_THREADED_FLANGE");
            //}
            //if(VICTAULIC_FLANGE.Checked)
            //{
            //    ecNameList.Add("VICTAULIC_FLANGE");
            //    ecNameList.Add("VICTAULIC_FLANGE_STYLE_341");
            //    ecNameList.Add("VICTAULIC_FLANGE_STYLE_641");
            //    ecNameList.Add("VICTAULIC_FLANGE_STYLE_741");
            //    ecNameList.Add("VICTAULIC_FLANGE_STYLE_743");
            //}
            //if(WELD_NECK_FLANGE.Checked)
            //{
            //    ecNameList.Add("WELD_NECK_FLANGE");
            //    ecNameList.Add("REDUCING_WELD_NECK_FLANGE");
            //    ecNameList.Add("WELD_NECK_ORIFICE_FLANGE");
            //}
            for(int i=0;i<dataGridView1.Rows.Count;i++)
            {
                bool b = Convert.ToBoolean(dataGridView1.Rows[i].Cells[0].Value);
                if (b)
                    ecNameList.Add(dataGridView1.Rows[i].Cells["FlName"].Value.ToString());
            }
            ReportFrom.flNameList = ecNameList;
            ReportFrom.isFl = true;
            flForm.Close();
        }

        private void ReportFLForm_Load(object sender, EventArgs e)
        {
            #region
            //if (ReportFrom.flNameList.Count > 0)
            //{
            //    foreach (string ecName in ReportFrom.flNameList)
            //    {
            //        bool b = true;
            //        //if(A_AFA_Flange.Name.Equals(ecName))
            //        //    A_AFA_Flange.Checked = b;

            //        //if(A_AFAB_Flange.Name.Equals(ecName))
            //        //    A_AFAB_Flange.Checked = b;

            //        //if(A_AFKB_Flange.Name.Equals(ecName))
            //        //    A_AFKB_Flange.Checked = b;

            //        //if(A_AFKV_Flange.Name.Equals(ecName))
            //        //    A_AFKV_Flange.Checked = b;

            //        //if(A_AFL_BW_Flange.Name.Equals(ecName))
            //        //    A_AFL_BW_Flange.Checked = b;

            //        //if(A_AFL_SCF_Flange.Name.Equals(ecName))
            //        //    A_AFL_SCF_Flange.Checked = b;

            //        //if(A_AFL_SCM_Flange.Name.Equals(ecName))
            //        //    A_AFL_SCM_Flange.Checked = b;

            //        //if(A_AFLA_Entirety_Flange.Name.Equals(ecName))
            //        //    A_AFLA_Entirety_Flange.Checked = b;

            //        //if(A_AFLA_Flange.Name.Equals(ecName))
            //        //    A_AFLA_Flange.Checked = b;

            //        //if(A_AFW_Flange.Name.Equals(ecName))
            //        //    A_AFW_Flange.Checked = b;

            //        //if(A_AFW_Flange_Incidental.Name.Equals(ecName))
            //        //    A_AFW_Flange_Incidental.Checked = b;

            //        //if(BLIND_FLANGE.Name.Equals(ecName))
            //        //    BLIND_FLANGE.Checked = b;

            //        //if(Centering_Entirety_Flange.Name.Equals(ecName))
            //        //    Centering_Entirety_Flange.Checked = b;

            //        //if(Centering_flange.Name.Equals(ecName))
            //        //    Centering_flange.Checked = b;

            //        //if(COMPANION_FLANGE.Name.Equals(ecName))
            //        //    COMPANION_FLANGE.Checked = b;

            //        //if(EXPANDER_FLANGE.Name.Equals(ecName))
            //        //    EXPANDER_FLANGE.Checked = b;

            //        //if(FILLER_FLANGE.Name.Equals(ecName))
            //        //    FILLER_FLANGE.Checked = b;

            //        //if(JACKETED_FLANGE.Name.Equals(ecName))
            //        //    JACKETED_FLANGE.Checked = b;

            //        //if(LAP_JOINT_FLANGE.Name.Equals(ecName))
            //        //    LAP_JOINT_FLANGE.Checked = b;

            //        //if(ORIFICE_FLANGE.Name.Equals(ecName))
            //        //    ORIFICE_FLANGE.Checked = b;

            //        //if(REDUCING_FLANGE.Name.Equals(ecName))
            //        //    REDUCING_FLANGE.Checked = b;

            //        //if(SLIP_ON_FLANGE.Name.Equals(ecName))
            //        //    SLIP_ON_FLANGE.Checked = b;

            //        //if(SOCKET_WELDED_FLANGE.Name.Equals(ecName))
            //        //    SOCKET_WELDED_FLANGE.Checked = b;

            //        //if(Square_Entirety_Flange.Name.Equals(ecName))
            //        //    Square_Entirety_Flange.Checked = b;

            //        //if(Square_Flange.Name.Equals(ecName))
            //        //    Square_Flange.Checked = b;

            //        //if(THREADED_FLANGE.Name.Equals(ecName))
            //        //    THREADED_FLANGE.Checked = b;

            //        //if(VICTAULIC_FLANGE.Name.Equals(ecName))
            //        //    VICTAULIC_FLANGE.Checked = b;

            //        //if(WELD_NECK_FLANGE.Name.Equals(ecName))
            //        //    WELD_NECK_FLANGE.Checked = b;
            //    }
            //}
            #endregion
            xmlReade();
            if (pzList==null)
            {
                dataGridView1.Rows.Add(false, "A_AFA_Flange");
                dataGridView1.Rows.Add(false, "A_AFAB_Flange");
                dataGridView1.Rows.Add(false, "A_AFKB_Flange");
                dataGridView1.Rows.Add(false, "A_AFKV_Flange");
                dataGridView1.Rows.Add(false, "A_AFL_BW_Flange");
                dataGridView1.Rows.Add(false, "A_AFL_SCF_Flange");
                dataGridView1.Rows.Add(false, "A_AFL_SCM_Flange");
                dataGridView1.Rows.Add(false, "A_AFLA_Entirety_Flange");
                dataGridView1.Rows.Add(false, "A_AFLA_Flange");
                dataGridView1.Rows.Add(false, "A_AFW_Flange");
                dataGridView1.Rows.Add(false, "A_AFW_Flange_Incidental");
                dataGridView1.Rows.Add(false, "BLIND_FLANGE");
                dataGridView1.Rows.Add(false, "Centering_Entirety_Flange");
                dataGridView1.Rows.Add(false, "Centering_flange");
                dataGridView1.Rows.Add(false, "Centering_flange_Left");
                dataGridView1.Rows.Add(false, "SLIP_ON_FLANGE");
                dataGridView1.Rows.Add(false, "SOCKET_WELDED_FLANGE");
                dataGridView1.Rows.Add(false, "Square_Entirety_Flange");
                dataGridView1.Rows.Add(false, "Square_Flange");
                dataGridView1.Rows.Add(false, "Square_Flange_Left");
                dataGridView1.Rows.Add(false, "THREADED_FLANGE");
                dataGridView1.Rows.Add(false, "WELD_NECK_FLANGE");
            }
            else
            {
                foreach(var name in pzList)
                {
                    dataGridView1.Rows.Add(false, name);
                }
            }

            for(int i=0;i<dataGridView1.Rows.Count;i++)
            {
                string ecName = dataGridView1.Rows[i].Cells["FlName"].Value.ToString();
                if(ecName.Equals("BLIND_FLANGE"))
                {
                    dataGridView1.Rows[i].Cells[0].Value = true;
                }
                else
                {
                    if(ReportFrom.flNameList.Count>0)
                    {
                        if (ReportFrom.flNameList.Contains(ecName))
                            dataGridView1.Rows[i].Cells[0].Value = true;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> ecNameList = new List<string>();
            for(int i=0;i<dataGridView1.Rows.Count;i++)
            {
                string ecName = dataGridView1.Rows[i].Cells["FlName"].Value.ToString();
                ecNameList.Add(ecName);
            }
            ReportFLPZForm pzForm = ReportFLPZForm.instence(ecNameList);
#if DEBUG

#else
            pzForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportFLPZForm));
            pzForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
            pzForm.Show();
        }

        public void xmlReade()
        {
            string path1 = path0 + "\\OPM_JYXConfig";
            if (!File.Exists(path1 + "\\FlPz.xml"))
            {
                return;
            }
            if (pzList != null)
            {
                pzList.Clear();
            }
            else
            {
                pzList = new List<string>();
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(path1 + "\\FlPz.xml");

            XmlNode xn = doc.SelectSingleNode("DataParameters").SelectSingleNode("FlNameList").SelectSingleNode("FlName");

            string nameList = xn.InnerText;

            string[] condition = { "," };

            string[] result = nameList.Split(condition, StringSplitOptions.RemoveEmptyEntries);

            foreach (string flName in result)
            {
                pzList.Add(flName);
            }
        }
    }
}
