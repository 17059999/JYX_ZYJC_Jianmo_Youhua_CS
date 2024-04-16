using Bentley.MstnPlatformNET.WinForms;
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

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class updateToleranceForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        public static updateToleranceForm utForm = null;
        public updateToleranceForm()
        {
            InitializeComponent();
        }

        public static updateToleranceForm instence()
        {
            if (utForm == null)
            {
                utForm = new updateToleranceForm();
            }
            else
            {
                utForm.Close();
                utForm = new updateToleranceForm();
            }
            return utForm;
        }

        private void TolerancetextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b' && !Char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string text = TolerancetextBox.Text;
            //string text = BMECInstanceManager.FindConfigVariableName("OPM_ALIGNMENT_TOLERANCE");
            //BMECInstanceManager.SetConfigVariable("OPM_ALIGNMENT_TOLERANCE", (179).ToString());
            try
            {
                BMECInstanceManager.SetConfigVariable("OPM_ALIGNMENT_TOLERANCE", text);
            }
            catch
            {
                MessageBox.Show("设置失败！");
                return;
            }
            MessageBox.Show("设置成功！");
        }

        private void updateToleranceForm_Load(object sender, EventArgs e)
        {
            string text = BMECInstanceManager.FindConfigVariableName("OPM_ALIGNMENT_TOLERANCE");
            TolerancetextBox.Text = text;
        }
    }
}
