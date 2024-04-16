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
using Bentley.OpenPlant.Modeler.Api;
using Bentley.MstnPlatformNET;
using Bentley.GeometryNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class plpipeBiaozhuForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static plpipeBiaozhuForm form = null;

        public static plpipeBiaozhuForm instence()
        {
            if(form==null)
            {
                form = new plpipeBiaozhuForm();
            }
            else
            {
                form.Close();
                form = new plpipeBiaozhuForm();
            }

            return form;
        }

        public plpipeBiaozhuForm()
        {
            InitializeComponent();
        }

        private void button_colse_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            pipeBiaogaoType type = pipeBiaogaoType.none;
            if(radioButton_top.Checked)
            {
                type = pipeBiaogaoType.topBg;
            }
            else if(radioButton_betton.Checked)
            {
                type = pipeBiaogaoType.bottonBg;
            }
            else if(radioButton_cent.Checked)
            {
                type = pipeBiaogaoType.centerBg;
            }
            else if(radioButton_pipenei.Checked)
            {
                type = pipeBiaogaoType.pipeNeiBg;
            }
            DrawingtestMark noteList = new DrawingtestMark();
            noteList.test(type);
        }
    }
}
