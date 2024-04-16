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
    public partial class MovePipesToolForm :
#if DEBUG
                     Form
#else
                     Adapter
#endif
    {
        public MovePipesToolForm()
        {
            InitializeComponent();
            this.checkBox_isWorking.Checked = false;
            this.radioButton_offset.Checked = true;
            this.textBox_X.Enabled = false;
            this.textBox_Y.Enabled = false;
            this.textBox_Z.Enabled = false;
        }

        private void checkBox_isWorking_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox currentCheckBox = (CheckBox)sender;
            if (currentCheckBox.CheckState == CheckState.Checked)
            {
                this.textBox_X.Enabled = true;
                this.textBox_Y.Enabled = true;
                this.textBox_Z.Enabled = true;
                this.radioButton_absolute.Enabled = true;
                this.radioButton_offset.Enabled = true;
                
            }
            else
            {
                this.textBox_X.Enabled = false;
                this.textBox_Y.Enabled = false;
                this.textBox_Z.Enabled = false;
                this.radioButton_absolute.Enabled = false;
                this.radioButton_offset.Enabled = false;
            }
            
        }

        private void MovePipesToolForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            MovePipesTool.m_formClosed();
            Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            app.CommandState.StartDefaultCommand();
        }
    }
}
