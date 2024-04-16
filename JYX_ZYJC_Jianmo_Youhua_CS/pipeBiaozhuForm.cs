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
    public partial class pipeBiaozhuForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static pipeBiaozhuForm form = null;

        public static pipeBiaozhuForm instence()
        {
            if(form==null)
            {
                form = new pipeBiaozhuForm();
            }
            else
            {
                form.Close();
                form = new pipeBiaozhuForm();
            }

            return form;
        }

        public pipeBiaozhuForm()
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
            DrawingtestTool tool = new DrawingtestTool(type);
            tool.InstallTool();
        }
    }
}
