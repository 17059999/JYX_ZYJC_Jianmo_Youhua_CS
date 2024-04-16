using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.MstnPlatformNET.WinForms;
using System.Windows.Forms;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
using Bentley.Internal.MstnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class CustomizeHeightSectionForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static CustomizeHeightSectionForm form = null;

        public static DgnFile dgnFile = Session.Instance.GetActiveDgnFile();

        public DisplayStyle disFor = null;

        public DisplayStyle disCut = null;

        public CustomizeHeightSectionForm()
        {
            InitializeComponent();
        }

        public static CustomizeHeightSectionForm instence()
        {
            if (form == null)
            {
                form = new CustomizeHeightSectionForm();
            }
            else
            {
                form.Close();
                form = new CustomizeHeightSectionForm();
            }

            return form;
        }

        private void CustomizeHeightSectionForm_Load(object sender, EventArgs e)
        {
            #region 读取当前图纸是否遮挡
            DisplayStyleList disList = new DisplayStyleList(dgnFile, false, false);
            IEnumerator<DisplayStyle> disIEtor = disList.GetEnumerator();
            bool isKeepOut = false;
            while (disIEtor.MoveNext())
            {
                DisplayStyle sty = disIEtor.Current;
                string name = sty.Name;
                if (name.Equals("剪切"))  //TODO 先默认切图按照Cut  Forward样式切图
                {
                    disCut = sty;
                }
                else if (name.Equals("向前"))
                {
                    disFor = sty;
                    DisplayStyleFlags flag = sty.GetFlags();
                    isKeepOut = flag.DisplayHiddenEdges;
                }
            }

            if (isKeepOut) noKeepOutRadioButton.Checked = true;
            else keepOutRadioButton.Checked = true;
            #endregion

            List<string> callList = new List<string>();
            callList.Add("Plan Callout");
            callList.Add("Section Callout");

            callOutComboBox.DataSource = callList;

            callOutComboBox.SelectedIndex = 0;
        }

        private void keepOutRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DisplayStyleFlags flag1 = disCut.GetFlags();
            flag1.DisplayHiddenEdges = false;
            disCut.SetFlags(flag1);

            DisplayStyleFlags flag2 = disFor.GetFlags();
            flag2.DisplayHiddenEdges = false;
            disFor.SetFlags(flag2);

            DisplayStyleManager.WriteDisplayStyleToFile(disCut, dgnFile);
            DisplayStyleManager.WriteDisplayStyleToFile(disFor, dgnFile);
        }

        private void noKeepOutRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            DisplayStyleFlags flag1 = disCut.GetFlags();
            flag1.DisplayHiddenEdges = true;
            disCut.SetFlags(flag1);

            DisplayStyleFlags flag2 = disFor.GetFlags();
            flag2.DisplayHiddenEdges = true;
            disFor.SetFlags(flag2);

            DisplayStyleManager.WriteDisplayStyleToFile(disCut, dgnFile);
            DisplayStyleManager.WriteDisplayStyleToFile(disFor, dgnFile);
        }

        private void calloutButton_Click(object sender, EventArgs e)
        {
            double z = Convert.ToDouble(heightTextBox.Text);
            double uro = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
            double dz = z * uro;
            string calltype = callOutComboBox.Text;
            CustomizeHeightCallout callout = new CustomizeHeightCallout(dz, calltype);
            callout.InstallTool();
        }

        private void heightTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            //允许输入数字、小数点、删除键和负号  
            if ((e.KeyChar < 48 || e.KeyChar > 57) && e.KeyChar != 8 && e.KeyChar != (char)('.') && e.KeyChar != (char)('-'))
            {
                //MessageBox.Show("请输入正确的数字");
                e.Handled = true;
            }
            if (e.KeyChar == (char)('-'))
            {
                if (heightTextBox.Text != "")
                {
                    //MessageBox.Show("请输入正确的数字");
                    e.Handled = true;
                }
            }
            /*小数点只能输入一次*/
            if (e.KeyChar == (char)('.') && ((TextBox)sender).Text.IndexOf('.') != -1)
            {
                //MessageBox.Show("请输入正确的数字");
                //this.heightTextBox.Text = "";
                e.Handled = true;
            }
            /*第一位不能为小数点*/
            if (e.KeyChar == (char)('.') && ((TextBox)sender).Text == "")
            {
                //MessageBox.Show("请输入正确的数字");
                //this.heightTextBox.Text = "";
                e.Handled = true;
            }
            /*第一位是0，第二位必须为小数点*/
            if ((e.KeyChar != (char)('.') && e.KeyChar != 8) && ((TextBox)sender).Text == "0")
            {
                //MessageBox.Show("请输入正确的数字");
                //this.heightTextBox.Text = "";
                e.Handled = true;
            }
            /*第一位是负号，第二位不能为小数点*/
            if (((TextBox)sender).Text == "-" && e.KeyChar == (char)('.'))
            {
                //MessageBox.Show("请输入正确的数字");
                //this.heightTextBox.Text = "";
                e.Handled = true;
            }
        }
    }
}
