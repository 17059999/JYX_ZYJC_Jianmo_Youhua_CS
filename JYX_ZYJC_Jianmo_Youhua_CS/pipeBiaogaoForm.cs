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
    public partial class pipeBiaogaoForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static pipeBiaogaoForm form = null;

        public static double uro = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

        public static pipeBiaogaoForm instence()
        {
            if(form==null)
            {
                form = new pipeBiaogaoForm();
            }
            else
            {
                form.Close();
                form = new pipeBiaogaoForm();
            }

            return form;
        }

        public pipeBiaogaoForm()
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
            DrawingpipeBiaoGaoTool Dbiaogao = new DrawingpipeBiaoGaoTool(type);

            Dbiaogao.InstallTool();
        }

        public static string getpipexinxi(BMECObject bmec, pipeBiaogaoType type,bool ispipeLine=false)
        {
            string xinxi = "";

            DPoint3d[] dps = GroupPipeTool.GetTowPortPoint(bmec);
            if(dps.Length==2)
            {
                if(Math.Abs(dps[0].Z-dps[1].Z)/uro<1)
                {
                    double top_level, bottom_level, center_level, bottom_inside_level;
                    double diameter = bmec.Instance["OUTSIDE_DIAMETER"].DoubleValue;
                    double wall_thickness = bmec.Instance["WALL_THICKNESS"].DoubleValue;

                    top_level = dps[0].Z / uro + diameter / 2;
                    bottom_level = dps[0].Z / uro - diameter / 2;
                    center_level = dps[0].Z / uro;
                    bottom_inside_level = dps[0].Z / uro - diameter / 2 + wall_thickness;
                    if(type==pipeBiaogaoType.topBg)
                    {
                        xinxi= string.Format("{0:F2}", top_level)+"(管顶)";
                    }
                    else if(type == pipeBiaogaoType.bottonBg)
                    {
                        xinxi = string.Format("{0:F2}", bottom_level) + "(管外底)";
                    }
                    else if (type == pipeBiaogaoType.centerBg)
                    {
                        xinxi = string.Format("{0:F2}", center_level) + "(管中心)";
                    }
                    else if (type == pipeBiaogaoType.pipeNeiBg)
                    {
                        xinxi = string.Format("{0:F2}", bottom_inside_level) + "(管内底)";
                    }
                }
            }

            if(ispipeLine)
            {
                if(type == pipeBiaogaoType.none)
                {
                    xinxi = bmec.Instance["LINENUMBER"].StringValue;
                }
                else
                {
                    xinxi = bmec.Instance["LINENUMBER"].StringValue + " " + xinxi;
                }
            }

            if (xinxi=="")
            {
                xinxi = " ";
            }
            return xinxi;
        }
    }
}
