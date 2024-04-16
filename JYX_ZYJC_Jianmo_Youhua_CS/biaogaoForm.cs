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

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class biaogaoForm : //Form
#if DEBUG
        Form
#else 
        Adapter
#endif
    {
        public static biaogaoForm from = null;

        public biaogaoForm()
        {
            InitializeComponent();
        }

        public static biaogaoForm instance()
        {
            if (from == null)
            {
                from = new biaogaoForm();
            }
            else
            {
                from.Close();
                from = new biaogaoForm();
            }

            return from;
        }

        private void biaogaoForm_Load(object sender, EventArgs e)
        {
            biaogao_text.Text = "3";
        }
    }
}
