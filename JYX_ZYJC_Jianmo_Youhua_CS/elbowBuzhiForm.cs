using System.Windows.Forms;
using BCOM = Bentley.Interop.MicroStationDGN;
using System.Collections;
using Bentley.MstnPlatformNET.WinForms;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.ECObjects.Schema;

using System.Collections.Generic;
using System;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 
    /// </summary>
    public partial class elbowBuzhiForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif

    {
        //private static int DEFAULT_HEIGHT = 250;
        private static int DEFAULT_HEIGHT = 3;
        private static int DEFAULT_WIDTH = 3;
        private static int COMPONENT_DISTANCE_X = 5;
        private static int COMPONENT_DISTANCE_Y = 5;

        public bool isOnlyOneElbowToCut = true;//是否只使用一根弯头切割
        /// <summary>
        /// 
        /// </summary>
        public elbowBuzhiForm()
        {
                InitializeComponent();
                //this.Height = DEFAULT_HEIGHT;
                //SetComponentLocation();
                this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.groupBox_elbow.Location.Y + this.groupBoxElbowHeight + DEFAULT_HEIGHT);

                xiaoshuBox_elbow_wanqu_banjing.Text = "";

                this.checkBox_isPlaceTrimmedElbow.Checked = true;
                this.radioButton_elbow.Checked = true;

                this.radioButton_yigenwantou.Checked = true;
                this.radioButton1.Checked = true;
                //this.radioButton1.Enabled = false;
                //this.radioButton2.Enabled = false;

                init_comboBox_caizhi();

                initRadiusNAngle();

        }

        //private static int windowSizeHeight = 380; 
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            //if (this.comboBox_elbowOrBend.Text == "Elbow")
            //{
            //    this.Height = windowSizeHeight - (this.groupBox_bend.Height - this.groupBox_elbow.Height);
            //}
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void SetComponentLocation()
        {
            this.panel3.Location = new System.Drawing.Point(DEFAULT_WIDTH, DEFAULT_HEIGHT);
            this.radioButton_elbow.Location = new System.Drawing.Point(COMPONENT_DISTANCE_X, COMPONENT_DISTANCE_Y);
            this.label2.Location = new System.Drawing.Point(this.radioButton_elbow.Location.X + this.radioButton_elbow.Width + COMPONENT_DISTANCE_X, DEFAULT_HEIGHT);
            this.comboBox_lianjielujing.Location = new System.Drawing.Point(this.label2.Location.X + this.label2.Width + COMPONENT_DISTANCE_X, DEFAULT_HEIGHT);
            this.radioButton_tee.Location = new System.Drawing.Point(DEFAULT_WIDTH, this.radioButton_elbow.Location.Y + this.radioButton_elbow.Height + COMPONENT_DISTANCE_Y);
            this.label13.Location = new System.Drawing.Point(this.radioButton_tee.Location.X + this.radioButton_tee.Width + COMPONENT_DISTANCE_X, this.radioButton_tee.Location.Y);
            this.comboBox_elbowOrBend.Location = new System.Drawing.Point(this.label13.Location.X + this.label13.Width + COMPONENT_DISTANCE_X, this.radioButton_tee.Location.Y);
            this.panel3.Height = this.radioButton_tee.Location.Y + this.radioButton_tee.Height + COMPONENT_DISTANCE_Y;

            groupBox_elbow.Location = new System.Drawing.Point(DEFAULT_WIDTH, panel3.Location.Y + panel3.Height + COMPONENT_DISTANCE_Y);
            label_wanqubanjing.Location = new System.Drawing.Point(COMPONENT_DISTANCE_X, COMPONENT_DISTANCE_Y);
            comboBox_elbow_radius.Location = new System.Drawing.Point(label_wanqubanjing.Location.X + label_wanqubanjing.Width + COMPONENT_DISTANCE_X, label_wanqubanjing.Location.Y);
            label3.Location = new System.Drawing.Point(label_wanqubanjing.Location.X, label_wanqubanjing.Location.Y + label_wanqubanjing.Height + COMPONENT_DISTANCE_Y);
            comboBox_elbow_angle.Location = new System.Drawing.Point(label3.Location.X + label3.Width + COMPONENT_DISTANCE_X, label3.Location.Y);

            checkBox_isPlaceTrimmedElbow.Location = new System.Drawing.Point(label3.Location.X, label3.Location.Y + label3.Height + COMPONENT_DISTANCE_Y);

            panel1.Location = new System.Drawing.Point(checkBox_isPlaceTrimmedElbow.Location.X, checkBox_isPlaceTrimmedElbow.Location.Y + checkBox_isPlaceTrimmedElbow.Height + COMPONENT_DISTANCE_Y);
            radioButton_yigenwantou.Location = new System.Drawing.Point(COMPONENT_DISTANCE_X, COMPONENT_DISTANCE_Y);
            radioButton_duogenwantou.Location = new System.Drawing.Point(radioButton_yigenwantou.Location.X, radioButton_yigenwantou.Location.Y + radioButton_yigenwantou.Height + COMPONENT_DISTANCE_Y);

            panel2.Location = new System.Drawing.Point(panel1.Location.X + panel1.Width + COMPONENT_DISTANCE_X, panel1.Location.Y);
            radioButton1.Location = new System.Drawing.Point(COMPONENT_DISTANCE_X, COMPONENT_DISTANCE_Y);
            radioButton2.Location = new System.Drawing.Point(radioButton1.Location.X, radioButton1.Location.Y + radioButton1.Height + COMPONENT_DISTANCE_Y);

            ClientSize = new System.Drawing.Size(panel3.Width > groupBox_elbow.Width ? panel3.Width : groupBox_elbow.Width, groupBox_elbow.Location.Y + groupBox_elbow.Height + DEFAULT_HEIGHT);
        }

        private void initRadiusNAngle() {
            this.comboBox_elbow_radius.Text = "长半径弯头";
            this.comboBox_elbow_angle.Text = "90度弯头";

            this.comboBox_elbowOrBend.Text = "Elbow";
            this.comboBox_bendOrXiamiwan.Text = "Elbow";

            this.comboBox_lianjielujing.Text = "OFF";
        }

        private void elbowBuzhiForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            elbow.m_formClosed();
            BCOM.Application app = Utilities.ComApp;
            app.CommandState.StartDefaultCommand();
        }

        private void init_comboBox_caizhi()
        {
            //Hashtable caizhi_list = get_caizhi_list("CAIZHI_VALUE_MAP");
            ArrayList caizhi_arraylist = new ArrayList();
            caizhi_arraylist.Add(new DictionaryEntry("default", ""));
            //foreach (DictionaryEntry caizhi in caizhi_list) //ht为一个Hashtable实例
            //{
               // caizhi_arraylist.Add(caizhi);
            //}
            comboBox_caizhi.DataSource = caizhi_arraylist;
            comboBox_caizhi.DisplayMember = "Value";
            comboBox_caizhi.ValueMember = "Key";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value_map"></param>
        /// <returns></returns>
        public Hashtable get_caizhi_list(string value_map)
        {
            IECClass iec_class = BMECInstanceManager.Instance.Schema.GetClass(value_map);
            Hashtable hashtable = new Hashtable();
            IEnumerator<IECProperty> enumerator = iec_class.GetEnumerator();
            while (enumerator.MoveNext())
            {
                hashtable.Add(enumerator.Current.Name, enumerator.Current.DisplayLabel);
            }
            return hashtable;
            
        }
        private int groupBoxBendHeight = 0;
        private int groupBoxElbowHeight = 0;
        private void comboBox1_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            groupBoxBendHeight = this.groupBox_bend.Height;
            groupBoxElbowHeight = this.groupBox_elbow.Height;
            int difHeight = groupBoxBendHeight - groupBoxElbowHeight;
            //this.Height = DEFAULT_HEIGHT;
            ComboBox tempComboBox = (ComboBox)sender;
            switch (tempComboBox.Text)
            {
                case "Elbow":
                    //this.groupBox_elbow.Enabled = true;
                    this.groupBox_elbow.Visible = true;
                    this.groupBox_bend.Visible = false;
                    this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.groupBox_elbow.Location.Y + this.groupBoxElbowHeight + DEFAULT_HEIGHT);
                    //this.Height = this.groupBox_elbow.Location.Y + this.groupBoxElbowHeight + DEFAULT_HEIGHT;
                    break;
                case "Bend":
                    //this.groupBox_bend.Enabled = true;
                    this.groupBox_bend.Visible = true;
                    this.groupBox_elbow.Visible = false;
                    //this.Height = this.Height + difHeight;
                    //this.Height = windowSizeHeight;
                    this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, this.groupBox_bend.Location.Y + this.groupBoxBendHeight + DEFAULT_HEIGHT);
                    //this.Height = this.groupBox_bend.Location.Y + this.groupBoxBendHeight + DEFAULT_HEIGHT;
                    break;
                default:
                    break;
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, System.EventArgs e)
        {
        }
        public static double dn = 100;
        public static int flag = 2;//1 for radius, 2 for radiusFactor

        private static string[] yijingwantoujiaodu = new string[] { "45度弯头", "90度弯头" };
        private static string[] putongwantoujiaodu = new string[] { "15度弯头","30度弯头","45度弯头","60度弯头","90度弯头" };
        private static string[] xiamiwangjiaodu = new string[] { "22.5度弯头", "30度弯头", "45度弯头", "60度弯头", "90度弯头" };
        private static string currentjiaodu = "";
        private void comboBox_elbow_radius_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox tempComboBox = (ComboBox)sender;
            if (tempComboBox.Text == "虾米弯")
            {
                //this.textBox_xiamiwan_jieshu.Enabled = true;
                this.comboBox_elbow_angle.Enabled = true;
                //this.checkBox_isPlaceTrimmedElbow.Enabled = true;
                this.comboBox_elbow_angle.Items.Clear();
                this.comboBox_elbow_angle.Items.AddRange(xiamiwangjiaodu);

                this.checkBox_isPlaceTrimmedElbow.Enabled = false;
                this.radioButton_yigenwantou.Enabled = false;
                this.radioButton_duogenwantou.Enabled = false;
                this.radioButton1.Enabled = false;
                this.radioButton2.Enabled = false;
            }
            else if (tempComboBox.Text == "异径弯头")
            {
                //this.checkBox_isPlaceTrimmedElbow.Enabled = true;
                //this.textBox_xiamiwan_jieshu.Enabled = false;
                this.comboBox_elbow_angle.Enabled = true;
                this.comboBox_elbow_angle.Items.Clear();
                this.comboBox_elbow_angle.Items.AddRange(yijingwantoujiaodu);

                this.checkBox_isPlaceTrimmedElbow.Enabled = false;
                this.radioButton_yigenwantou.Enabled = false;
                this.radioButton_duogenwantou.Enabled = false;
                this.radioButton1.Enabled = false;
                this.radioButton2.Enabled = false;
            }
            else
            {
                //this.checkBox_isPlaceTrimmedElbow.Enabled = true;
                //this.textBox_xiamiwan_jieshu.Enabled = false;
                this.comboBox_elbow_angle.Enabled = true;
                this.comboBox_elbow_angle.Items.Clear();
                this.comboBox_elbow_angle.Items.AddRange(putongwantoujiaodu);

                this.checkBox_isPlaceTrimmedElbow.Enabled = true;
                this.radioButton_yigenwantou.Enabled = true;
                this.radioButton_duogenwantou.Enabled = true;
                this.radioButton1.Enabled = true;
                this.radioButton2.Enabled = true;
            }
            if (this.comboBox_elbow_angle.Items.Contains(currentjiaodu))
            {
                this.comboBox_elbow_angle.Text = currentjiaodu;
            }
            else
            {
                this.comboBox_elbow_angle.SelectedIndex = 0;
            }
            currentjiaodu = this.comboBox_elbow_angle.Text;
        }

        private void comboBox_elbow_angle_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentjiaodu = ((ComboBox)sender).Text;
        }
        private void textBox_radiusFactor_TextChanged(object sender, EventArgs e)
        {
            this.textBox_radius.Text = (Convert.ToDouble(this.textBox_radiusFactor.Text) * dn).ToString();
        }

        private void checkBox_isPlaceTrimmedElbow_CheckStateChanged(object sender, EventArgs e)
        {
            if (this.checkBox_isPlaceTrimmedElbow.Checked)
            {
                //this.checkBox1.Enabled = true;
                this.radioButton_yigenwantou.Enabled = true;
                this.radioButton_duogenwantou.Enabled = true;
            }
            else
            {
                //this.checkBox1.Enabled = false;
                this.radioButton_yigenwantou.Enabled = false;
                this.radioButton_duogenwantou.Enabled = false;
            }
        }

        private void radioButton_yigenwantou_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_isPlaceTrimmedElbow.Checked && this.radioButton_yigenwantou.Checked)
            {
                this.isOnlyOneElbowToCut = true;
                this.radioButton1.Enabled = true;
                this.radioButton2.Enabled = true;
            }
            else
            {
                this.isOnlyOneElbowToCut = false;
                this.radioButton1.Enabled = false;
                this.radioButton2.Enabled = false;
            }
        }

        private void radioButton_duogenwantou_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_isPlaceTrimmedElbow.Checked && !this.radioButton_duogenwantou.Checked)
            {
                this.isOnlyOneElbowToCut = true;
                this.radioButton1.Enabled = true;
                this.radioButton2.Enabled = true;
            }
            else
            {
                this.isOnlyOneElbowToCut = false;
                this.radioButton1.Enabled = false;
                this.radioButton2.Enabled = false;
            }
        }
    }
}
