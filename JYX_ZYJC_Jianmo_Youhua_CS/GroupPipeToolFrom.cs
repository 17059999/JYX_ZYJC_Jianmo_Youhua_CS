
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BIM = Bentley.Interop.MicroStationDGN;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class GroupPipeToolFrom : //Form
#if DEBUG
        Form
#else  
        Adapter
#endif
    {
        public static GroupPipeToolFrom groupfrom = null;
        public bool type = false;
        public bool isCenter = false, isTop = false, isDown = false, isLeft = false, isRegiht = false;
        int fromHeight = 0;
        int froupHeigth = 0;
        int uFroupHeigth = 0;
        Point bendButt = new Point();
        Point elbowButt = new Point();
        Point bendButt1 = new Point();
        Point elbowButt1 = new Point();
        public Dictionary<int, elbowOrBend> pipeElbowOrBendList = new Dictionary<int, elbowOrBend>();
        public Dictionary<int, Element> elementList = new Dictionary<int, Element>();
        static GroupPipeTool groupPipeTool = null;
        static BIM.Application app = Utilities.ComApp;
        public int selectindex = 0;
        public GroupPipeToolFrom()
        {
            InitializeComponent();
            initRadiusNAngle();
            //BMECApi.Instance.StartDefaultCommand();
            //SelectionSetManager.EmptyAll();
        }

        public static GroupPipeToolFrom instence()
        {
            if (groupfrom == null)
            {
                groupfrom = new GroupPipeToolFrom();
            }
            else
            {
                groupfrom.Close();
                if (groupPipeTool != null)
                {
                    groupPipeTool.clearup();
                }
                groupfrom = new GroupPipeToolFrom();
            }
            return groupfrom;
        }

        /// <summary>
        /// 点击添加管道后进入GroupPipeTool方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (elementList.Count > 0)
            {
                Bentley.Interop.MicroStationDGN.Element element1 = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(elementList[selectindex]);
                element1.IsHighlighted = false;
            }
            type = false;
            dataGridView_PipeMessage.Rows.Clear();
            pipeElbowOrBendList.Clear();
            elementList.Clear();

            selectindex = 0;
            groupPipeTool = new GroupPipeTool(this);
            groupPipeTool.InstallTool();
            //MessageBox.Show("11");
        }

        /// <summary>
        /// 在GroupPipeTool获取值后给dataGridView赋值
        /// </summary>
        /// <param name="i"></param>
        /// <param name="ecClassName"></param>
        /// <param name="dn"></param>
        /// <param name="od"></param>
        public void addPipe(int i, string ecClassName, double dn, double od, Element elem, string wall, string lineNumber)
        {
            int j = i + 1;
            dataGridView_PipeMessage.Rows.Add(j, lineNumber, ecClassName, dn, od, wall);
            //elbowOrBend elbowbendclass = new elbowOrBend();
            //elbowbendclass.elbowOrBendName = comboBox_elbowOrBend.Text;

            //pipeElbowOrBendList.Add()
            accessValue(i);
            elementList.Add(i, elem);
            double bili = pipeElbowOrBendList[i].bendRadiusRatio;
            double.TryParse(textBox_radiusFactor.Text, out bili);
            double ra = bili * dn;
            pipeElbowOrBendList[i].bendRadius = ra;
            //Element ele = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(bmec);
            //ele.ElementId.ToString();
            //SelectionSetManager.AddElement(elem, Session.Instance.GetActiveDgnModelRef());
        }

        /// <summary>
        /// 点击确定后开始成组布管
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            app.ShowCommand("成组布管");
            app.ShowPrompt("请选择管道的一个端口");
            type = true;
        }

        /// <summary>
        /// 初始化默认值
        /// </summary>
        private void initRadiusNAngle()
        {
            fromHeight = Height;
            bendButt = button1.Location;
            elbowButt = button2.Location;
            bendButt1 = new Point(bendButt.X, bendButt.Y - (groupBox_elbow.Height - groupBox_bend.Height));
            elbowButt1 = new Point(elbowButt.X, elbowButt.Y - (groupBox_elbow.Height - groupBox_bend.Height));
            froupHeigth = groupBox2.Height;
            uFroupHeigth = froupHeigth - (groupBox_elbow.Height - groupBox_bend.Height);
            this.comboBox_elbow_radius.Text = "长半径弯头";
            this.comboBox_elbow_angle.Text = "90度弯头";

            this.comboBox_elbowOrBend.Text = "Elbow";
            //this.comboBox_bendOrXiamiwan.Text = "Elbow";
            checkBox1.Checked = false;

            checkBox2.Checked = true;

            radioButton1.Checked = true;

            radioButton4.Checked = true;
        }

        /// <summary>
        /// 加载时设置窗体大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GroupPipeToolFrom_Load(object sender, EventArgs e)
        {
            //if (comboBox_elbowOrBend.Text == "Elbow")
            //{
            //    Height = fromHeight - (groupBox_bend.Height - groupBox_elbow.Height);
            //    //Point butt1 = new Point(12,277);
            //    //butt1.Y = butt1.Y - (groupBox_bend.Height - groupBox_elbow.Height)/2;
            //    button1.Location = bendButt1;
            //    //Point butt2 = new Point(156,277);
            //    //butt2.Y = butt2.Y - (groupBox_bend.Height - groupBox_elbow.Height)/2;
            //    button2.Location = elbowButt1;
            //    //button1.Location.Y = button1.Location.Y - (groupBox_bend.Height - groupBox_elbow.Height);
            //    groupBox2.Height = uFroupHeigth;
            //}
            StartPosition = FormStartPosition.CenterScreen;
        }

        /// <summary>
        /// 选择elbow或者bend时改变窗体大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_elbowOrBend_SelectedIndexChanged(object sender, EventArgs e)
        {
            int groupBoxBendHeight = this.groupBox_bend.Height;
            int groupBoxElbowHeight = this.groupBox_elbow.Height;
            int difHeight = groupBoxElbowHeight - groupBoxBendHeight;
            ComboBox tempComboBox = (ComboBox)sender;
            switch (tempComboBox.Text)
            {
                case "Elbow":
                    groupBox_elbow.Visible = true;
                    groupBox_bend.Visible = false;
                    Height = fromHeight;
                    //Point butt1 = new Point(12,277);
                    //butt1.Y = butt1.Y - difHeight/2;
                    button1.Location = bendButt;
                    //Point butt2 = new Point(156,277);
                    //butt2.Y = butt2.Y - difHeight/2;
                    button2.Location = elbowButt;
                    groupBox2.Height = froupHeigth;
                    break;
                case "Bend":
                    this.groupBox_bend.Visible = true;
                    this.groupBox_elbow.Visible = false;
                    this.Height = fromHeight - difHeight;
                    //Point butt3 = new Point(12,387);
                    //butt3.Y = butt3.Y + difHeight/2;
                    button1.Location = bendButt1;
                    //Point butt4 = new Point(156,387);
                    //butt4.Y = butt4.Y + difHeight/2;
                    button2.Location = elbowButt1;
                    groupBox2.Height = uFroupHeigth;//uFroupHeigth
                    break;
                default:
                    break;
            }
            int i = 0;
            try
            {
                i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value) - 1;
            }
            catch
            {
                i = 0;
            }
            updateValue(i);
        }

        ///// <summary>
        ///// 选择elbow或者xiamiwan时，显示对应文本框是否可用
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void comboBox_bendOrXiamiwan_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    ComboBox tempComboBox = (ComboBox)sender;
        //    switch (tempComboBox.Text)
        //    {
        //        case "Elbow":
        //            this.textBox_xiamiwan_jieshu.Enabled = false;
        //            this.comboBox_elbow_angle.Enabled = true;
        //            this.comboBox_elbow_radius.Enabled = true;
        //            break;
        //        case "虾米弯":
        //            this.textBox_xiamiwan_jieshu.Enabled = true;
        //            this.comboBox_elbow_radius.Enabled = false;
        //            this.comboBox_elbow_angle.Enabled = false;
        //            break;
        //        default:
        //            break;
        //    }
        //    int i = 0;
        //    try
        //    {
        //        i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value) - 1;
        //    }
        //    catch
        //    {
        //        i = 0;
        //    }
        //    updateValue(i);
        //}

        /// <summary>
        /// 存储值
        /// </summary>
        public void accessValue(int i)
        {
            elbowOrBend elbowOrBendClass = new elbowOrBend();
            if (comboBox_elbowOrBend.Text == "Elbow")
            {
                //elbowOrBendClass.elbowType = comboBox_bendOrXiamiwan.Text;
                elbowOrBendClass.elbowOrBendName = comboBox_elbowOrBend.Text;
                elbowOrBendClass.elbowRadius = comboBox_elbow_radius.Text;
                elbowOrBendClass.elbowAngle = comboBox_elbow_angle.Text;
                elbowOrBendClass.isBzQgWt = checkBox2.Checked;
                elbowOrBendClass.isYgWt = radioButton1.Checked;
                elbowOrBendClass.isLDQ = radioButton4.Checked;
                int jieshu = 3;
                //int.TryParse(textBox_xiamiwan_jieshu.Text, out jieshu);
                //if (jieshu < 3)
                //{
                //    jieshu = 3;
                //}
                elbowOrBendClass.pitchNumber = jieshu;
            }
            else
            {
                double radiusRatio = 1.0;
                double.TryParse(textBox_radiusFactor.Text, out radiusRatio);
                elbowOrBendClass.bendRadiusRatio = radiusRatio;
                double radius = 100.0;
                double.TryParse(textBox_radius.Text, out radius);
                elbowOrBendClass.bendRadius = radius;
                double frontLong = 0;
                double afterLong = 0;
                double.TryParse(textBox_lengthToBend.Text, out frontLong);
                double.TryParse(textBox_lengthAfterBend.Text, out afterLong);
                elbowOrBendClass.bendFrontLong = frontLong;
                elbowOrBendClass.bendAfterLong = afterLong;
            }
            pipeElbowOrBendList.Add(i, elbowOrBendClass);
        }

        /// <summary>
        /// 修改值
        /// </summary>
        /// <param name="i"></param>
        public void updateValue(int i)
        {
            if (pipeElbowOrBendList.Count > 0)
            {
                if(checkBox1.Checked==true)
                {
                    double selectdn = 100;
                    int index = 0;
                    if (dataGridView_PipeMessage.SelectedRows.Count>0)
                    {
                        index = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                        selectdn = pipeElbowOrBendList[index-1].bendRadius;
                        textBox_radius.Text = selectdn.ToString();
                    }
                    else
                    {
                        selectdn = pipeElbowOrBendList[0].bendRadius;
                        textBox_radius.Text = selectdn.ToString();
                    }
                    foreach (KeyValuePair<int,elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.elbowOrBendName = comboBox_elbowOrBend.Text;
                        kv.Value.elbowRadius = comboBox_elbow_radius.Text;
                        kv.Value.elbowAngle = comboBox_elbow_angle.Text;
                        kv.Value.isBzQgWt = checkBox2.Checked;
                        kv.Value.isYgWt = radioButton1.Checked;
                        kv.Value.isLDQ = radioButton4.Checked;
                        //kv.Value.elbowType = comboBox_bendOrXiamiwan.Text;
                        kv.Value.bendRadiusRatio = Convert.ToDouble(textBox_radiusFactor.Text);
                        kv.Value.bendRadius = Convert.ToDouble(dataGridView_PipeMessage.Rows[kv.Key].Cells[3].Value) * kv.Value.bendRadiusRatio;
                        //kv.Value.bendRadius = Convert.ToDouble(textBox_radius.Text);
                        kv.Value.bendFrontLong = Convert.ToDouble(textBox_lengthToBend.Text);
                        kv.Value.bendAfterLong = Convert.ToDouble(textBox_lengthAfterBend.Text);
                        kv.Value.pitchNumber = 3;
                    }
                }
                else
                {
                    pipeElbowOrBendList[i].elbowOrBendName = comboBox_elbowOrBend.Text;
                    pipeElbowOrBendList[i].elbowRadius = comboBox_elbow_radius.Text;
                    pipeElbowOrBendList[i].elbowAngle = comboBox_elbow_angle.Text;
                    pipeElbowOrBendList[i].isBzQgWt = checkBox2.Checked;
                    pipeElbowOrBendList[i].isYgWt = radioButton1.Checked;
                    pipeElbowOrBendList[i].isLDQ = radioButton4.Checked;
                    //pipeElbowOrBendList[i].elbowType = comboBox_bendOrXiamiwan.Text;
                    pipeElbowOrBendList[i].bendRadiusRatio = Convert.ToDouble(textBox_radiusFactor.Text);
                    pipeElbowOrBendList[i].bendRadius = Convert.ToDouble(dataGridView_PipeMessage.Rows[i].Cells["DN"].Value) * pipeElbowOrBendList[i].bendRadiusRatio;
                    pipeElbowOrBendList[i].bendFrontLong = Convert.ToDouble(textBox_lengthToBend.Text);
                    pipeElbowOrBendList[i].bendAfterLong = Convert.ToDouble(textBox_lengthAfterBend.Text);
                    pipeElbowOrBendList[i].pitchNumber = 3;
                }             
            }
        }

        private static string[] putongwantoujiaodu = new string[] { "15度弯头", "30度弯头", "45度弯头", "60度弯头", "90度弯头" };
        private static string[] xiamiwangjiaodu = new string[] { "22.5度弯头", "30度弯头", "45度弯头", "60度弯头", "90度弯头" };
        /// <summary>
        /// elbow弯曲半径修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_elbow_radius_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox tempComboBox = (ComboBox)sender;
            switch (tempComboBox.Text)
            {
                case "虾米弯":
                    //this.textBox_xiamiwan_jieshu.Enabled = true;
                    //this.comboBox_elbow_radius.Enabled = false;
                    this.comboBox_elbow_angle.Enabled = true;
                    comboBox_elbow_angle.Items.Clear();
                    comboBox_elbow_angle.Items.AddRange(xiamiwangjiaodu);
                    break;
                default:
                    //this.textBox_xiamiwan_jieshu.Enabled = false;
                    //this.comboBox_elbow_radius.Enabled = false;
                    this.comboBox_elbow_angle.Enabled = true;
                    comboBox_elbow_angle.Items.Clear();
                    comboBox_elbow_angle.Items.AddRange(putongwantoujiaodu);
                    break;
            }
            if (pipeElbowOrBendList.Count > 0)
            {
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.elbowRadius = comboBox_elbow_radius.Text;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].elbowRadius = comboBox_elbow_radius.Text;
                }         
            }
        }

        /// <summary>
        /// 点击对应的管道后读取值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_PipeMessage_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {
                int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                #region 高亮显示
                Bentley.Interop.MicroStationDGN.Element element = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(elementList[i - 1]);
                //Element ele = JYX_ZYJC_CLR.PublicMethod.convertToDgnNetElem(bmec);
                //SelectionSetManager.AddElement(ele, Session.Instance.GetActiveDgnModelRef());
                //Bentley.Interop.MicroStationDGN.Element element = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(ele);
                element.IsHighlighted = true;
                if (selectindex != i - 1)
                {
                    Bentley.Interop.MicroStationDGN.Element element1 = JYX_ZYJC_CLR.PublicMethod.convertToInteropElem(elementList[selectindex]);
                    element1.IsHighlighted = false;
                    selectindex = i - 1;
                }
                #endregion
                #region 读值
                elbowOrBend elboworbendclass = pipeElbowOrBendList[i - 1];
                if (elboworbendclass.elbowOrBendName == "Elbow")
                {
                    comboBox_elbowOrBend.Text = elboworbendclass.elbowOrBendName;
                    comboBox_elbow_radius.Text = elboworbendclass.elbowRadius;
                    comboBox_elbow_angle.Text = elboworbendclass.elbowAngle;
                    checkBox2.Checked = elboworbendclass.isBzQgWt;
                    radioButton1.Checked = elboworbendclass.isYgWt;
                    radioButton2.Checked = !elboworbendclass.isYgWt;
                    radioButton3.Checked = !elboworbendclass.isLDQ;
                    radioButton4.Checked = elboworbendclass.isLDQ;
                    //comboBox_bendOrXiamiwan.Text = elboworbendclass.elbowType;
                    //if (elboworbendclass.elbowRadius != "虾米弯")
                    //{
                        
                    //}
                    //else
                    //{
                    //    textBox_xiamiwan_jieshu.Text = Convert.ToString(elboworbendclass.pitchNumber);
                    //}
                }
                else
                {
                    comboBox_elbowOrBend.Text = elboworbendclass.elbowOrBendName;
                    //comboBox_bendOrXiamiwan.Text = elboworbendclass.elbowType;
                    textBox_radiusFactor.Text = Convert.ToString(elboworbendclass.bendRadiusRatio);
                    textBox_radius.Text = Convert.ToString(elboworbendclass.bendRadius);
                    textBox_lengthToBend.Text = Convert.ToString(elboworbendclass.bendFrontLong);
                    textBox_lengthAfterBend.Text = Convert.ToString(elboworbendclass.bendAfterLong);
                }
                #endregion
            }
        }

        /// <summary>
        /// 修改elbow弯曲角度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox_elbow_angle_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pipeElbowOrBendList.Count > 0)
            {
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.elbowAngle = comboBox_elbow_angle.Text;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].elbowAngle = comboBox_elbow_angle.Text;
                }
            }
        }

        /// <summary>
        /// 弯曲半径倍率改变时修改存储值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_radiusFactor_TextChanged(object sender, EventArgs e)
        {
            double bili = 1.5;
            double.TryParse(textBox_radiusFactor.Text, out bili);
            if (pipeElbowOrBendList.Count > 0)
            {
                if(checkBox1.Checked==true)
                {
                    foreach(KeyValuePair<int,elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.bendRadiusRatio = bili;
                        //double dn = Convert.ToDouble(dataGridView_PipeMessage.Rows[kv.Key].Cells[3].Value);
                        double ra = Convert.ToDouble(dataGridView_PipeMessage.Rows[kv.Key].Cells["DN"].Value) * bili;
                        textBox_radius.Text = Convert.ToString(ra);
                        kv.Value.bendRadius = ra;
                    }
                    if(dataGridView_PipeMessage.SelectedRows.Count>0)
                    {
                        int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                        double rbili = pipeElbowOrBendList[i - 1].bendRadiusRatio;
                        double od = Convert.ToDouble(dataGridView_PipeMessage.SelectedRows[0].Cells["DN"].Value) * rbili;
                        textBox_radius.Text = Convert.ToString(od);
                    }
                    else
                    {
                        double rbili = pipeElbowOrBendList[0].bendRadiusRatio;
                        double od = Convert.ToDouble(dataGridView_PipeMessage.Rows[0].Cells["DN"].Value) * rbili;
                        textBox_radius.Text = Convert.ToString(od);
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].bendRadiusRatio = bili;
                    double ra = Convert.ToDouble(dataGridView_PipeMessage.SelectedRows[0].Cells["DN"].Value) * bili;
                    textBox_radius.Text = Convert.ToString(ra);
                    pipeElbowOrBendList[i - 1].bendRadius = ra;
                }         
            }
        }

        /// <summary>
        /// 弯头前段长发生更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_lengthToBend_TextChanged(object sender, EventArgs e)
        {
            if (pipeElbowOrBendList.Count > 0)
            {
                double frontLong = 100.0;
                double.TryParse(textBox_lengthToBend.Text, out frontLong);
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.bendFrontLong = frontLong;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].bendFrontLong = frontLong;
                }           
            }
        }

        /// <summary>
        /// 弯头后段长发生更改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_lengthAfterBend_TextChanged(object sender, EventArgs e)
        {
            if (pipeElbowOrBendList.Count > 0)
            {
                double afterLong = 100.0;
                double.TryParse(textBox_lengthAfterBend.Text, out afterLong);
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.bendAfterLong = afterLong;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].bendAfterLong = afterLong;
                }
                
            }
        }

        /// <summary>
        /// 虾米弯节数发生改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void textBox_xiamiwan_jieshu_TextChanged(object sender, EventArgs e)
        //{
        //    if (pipeElbowOrBendList.Count > 0)
        //    {
        //        int jieshu = 3;
        //        int.TryParse(textBox_xiamiwan_jieshu.Text, out jieshu);
        //        if (jieshu < 3)
        //        {
        //            jieshu = 3;
        //        }
        //        if(checkBox1.Checked==true)
        //        {
        //            foreach(KeyValuePair<int,elbowOrBend> kv in pipeElbowOrBendList)
        //            {
        //                kv.Value.pitchNumber = jieshu;
        //            }
        //        }
        //        else
        //        {
        //            int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
        //            pipeElbowOrBendList[i - 1].pitchNumber = jieshu;
        //        }          
        //    }
        //}

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(pipeElbowOrBendList.Count>0)
            {
                if(checkBox1.Checked==true)
                {
                    groupBox2.Text = "配置所有管道弯头";
                    if (comboBox_elbowOrBend.Text == "Elbow")
                    {
                        //string elbowType = comboBox_bendOrXiamiwan.Text;
                        string elbowOrBendName = comboBox_elbowOrBend.Text;
                        string elbowRadius = comboBox_elbow_radius.Text;
                        string elbowAngle = comboBox_elbow_angle.Text;
                        int jieshu = 3;
                        //int.TryParse(textBox_xiamiwan_jieshu.Text, out jieshu);
                        //if (jieshu < 3)
                        //{
                        //    jieshu = 3;
                        //}
                        foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                        {
                            //kv.Value.elbowType = elbowType;
                            kv.Value.elbowOrBendName = elbowOrBendName;
                            kv.Value.elbowRadius = elbowRadius;
                            kv.Value.elbowAngle = elbowAngle;
                            kv.Value.pitchNumber = jieshu;
                            kv.Value.isBzQgWt = checkBox2.Checked;
                            kv.Value.isYgWt = radioButton1.Checked;
                            kv.Value.isLDQ = radioButton4.Checked;
                        }
                        //elbowOrBendClass.pitchNumber = jieshu;
                    }
                    else
                    {
                        double radiusRatio = 1.0;
                        double.TryParse(textBox_radiusFactor.Text, out radiusRatio);
                        //elbowOrBendClass.bendRadiusRatio = radiusRatio;
                        double radius = 100.0;
                        double.TryParse(textBox_radius.Text, out radius);
                        //elbowOrBendClass.bendRadius = radius;
                        double frontLong = 0;
                        double afterLong = 0;
                        double.TryParse(textBox_lengthToBend.Text, out frontLong);
                        double.TryParse(textBox_lengthAfterBend.Text, out afterLong);
                        foreach(KeyValuePair<int,elbowOrBend> kv in pipeElbowOrBendList)
                        {
                            kv.Value.bendRadiusRatio = radiusRatio;
                            kv.Value.bendRadius = Convert.ToDouble(dataGridView_PipeMessage.Rows[kv.Key].Cells["DN"].Value) * kv.Value.bendRadiusRatio;
                            kv.Value.bendFrontLong = frontLong;
                            kv.Value.bendAfterLong = afterLong;
                        }
                        //elbowOrBendClass.bendFrontLong = frontLong;
                        //elbowOrBendClass.bendAfterLong = afterLong;
                    }

                }
                else
                {
                    groupBox2.Text = "配置所选管道的弯头";
                }
            }
        }

        private void radioCenter_CheckedChanged(object sender, EventArgs e)
        {
            isCenter = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton1.Enabled = checkBox2.Checked;
            radioButton2.Enabled = checkBox2.Checked;
            if (radioButton1.Checked)
            {
                radioButton3.Enabled = checkBox2.Checked;
                radioButton4.Enabled = checkBox2.Checked;
            }
            

            if (pipeElbowOrBendList.Count > 0)
            {
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.isBzQgWt = checkBox2.Checked;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].isBzQgWt = checkBox2.Checked;
                }
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            radioButton3.Enabled = radioButton1.Checked;
            radioButton4.Enabled = radioButton1.Checked;

            if (pipeElbowOrBendList.Count > 0)
            {
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.isYgWt = radioButton1.Checked;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].isYgWt = radioButton1.Checked;
                }
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (pipeElbowOrBendList.Count > 0)
            {
                if (checkBox1.Checked == true)
                {
                    foreach (KeyValuePair<int, elbowOrBend> kv in pipeElbowOrBendList)
                    {
                        kv.Value.isLDQ = radioButton4.Checked;
                    }
                }
                else
                {
                    int i = Convert.ToInt32(dataGridView_PipeMessage.SelectedRows[0].Cells[0].Value);
                    pipeElbowOrBendList[i - 1].isLDQ = radioButton4.Checked;
                }
            }
        }

        private void radioTop_CheckedChanged(object sender, EventArgs e)
        {
            isTop = true;
        }

        private void radioDown_CheckedChanged(object sender, EventArgs e)
        {
            isDown = true;
        }

        private void radioLeft_CheckedChanged(object sender, EventArgs e)
        {
            isLeft = true;
        }

        private void radioRight_CheckedChanged(object sender, EventArgs e)
        {
            isRegiht = true;
        }
    }
}
