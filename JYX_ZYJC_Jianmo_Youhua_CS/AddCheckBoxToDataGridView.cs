using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public class AddCheckBoxToDataGridView
    {
        public static System.Windows.Forms.DataGridView dgv;
        public static System.Windows.Forms.DataGridView dgv1;
        public static System.Windows.Forms.DataGridView dgvm;
        public static System.Windows.Forms.DataGridView dgvm1;
        public static void AddFullSelect()
        {
            System.Windows.Forms.CheckBox ckBox = new System.Windows.Forms.CheckBox();
            ckBox.Text = "";
            ckBox.Checked = true;
            System.Drawing.Rectangle rect = dgv.GetCellDisplayRectangle(0, -1, true);
            ckBox.Size = new System.Drawing.Size(13, 13);
            ckBox.Location = new System.Drawing.Point(rect.Location.X + dgv.Columns[0].Width / 2 - 13 / 2 - 1, rect.Location.Y + 3);
            ckBox.CheckedChanged += new EventHandler(ckBox_CheckedChanged);
            dgv.Controls.Add(ckBox);
        }

        public static void AddFullSelect1()
        {
            System.Windows.Forms.CheckBox ckBox = new System.Windows.Forms.CheckBox();
            ckBox.Text = "";
            ckBox.Checked = true;
            System.Drawing.Rectangle rect = dgv1.GetCellDisplayRectangle(0, -1, true);
            ckBox.Size = new System.Drawing.Size(13, 13);
            ckBox.Location = new System.Drawing.Point(rect.Location.X + dgv1.Columns[0].Width / 2 - 13 / 2 - 1, rect.Location.Y + 3);
            ckBox.CheckedChanged += new EventHandler(ckBox_CheckedChanged1);
            dgv1.Controls.Add(ckBox);
        }

        public static void AddFullSelectm()
        {
            System.Windows.Forms.CheckBox ckBox = new System.Windows.Forms.CheckBox();
            ckBox.Text = "";
            ckBox.Checked = true;
            System.Drawing.Rectangle rect = dgvm.GetCellDisplayRectangle(0, -1, true);
            ckBox.Size = new System.Drawing.Size(13, 13);
            ckBox.Location = new System.Drawing.Point(rect.Location.X + dgvm.Columns[0].Width / 2 - 13 / 2 - 1, rect.Location.Y + 3);
            ckBox.CheckedChanged += new EventHandler(ckBox_CheckedChangedm);
            dgvm.Controls.Add(ckBox);
        }

        public static void AddFullSelectm1()
        {
            System.Windows.Forms.CheckBox ckBox = new System.Windows.Forms.CheckBox();
            ckBox.Text = "";
            ckBox.Checked = true;
            System.Drawing.Rectangle rect = dgvm1.GetCellDisplayRectangle(0, -1, true);
            ckBox.Size = new System.Drawing.Size(13, 13);
            ckBox.Location = new System.Drawing.Point(rect.Location.X + dgvm1.Columns[0].Width / 2 - 13 / 2 - 1, rect.Location.Y + 3);
            ckBox.CheckedChanged += new EventHandler(ckBox_CheckedChangedm1);
            dgvm1.Controls.Add(ckBox);
        }

        static void ckBox_CheckedChanged(object sender,EventArgs e)
        {
            if(dgv.Rows.Count>0)
            {
                for (int i = 0; i < dgv.Rows.Count; i++)
                {
                    dgv.Rows[i].Cells[0].Value = ((System.Windows.Forms.CheckBox)sender).Checked;
                }
                dgv.EndEdit();
            }            
        }

        static void ckBox_CheckedChanged1(object sender, EventArgs e)
        {
            if (dgv1.Rows.Count > 0)
            {
                for (int i = 0; i < dgv1.Rows.Count; i++)
                {
                    dgv1.Rows[i].Cells[0].Value = ((System.Windows.Forms.CheckBox)sender).Checked;
                }
                dgv1.EndEdit();
            }
        }

        static void ckBox_CheckedChangedm(object sender, EventArgs e)
        {
            if (dgvm.Rows.Count > 0)
            {
                for (int i = 0; i < dgvm.Rows.Count; i++)
                {
                    dgvm.Rows[i].Cells[0].Value = ((System.Windows.Forms.CheckBox)sender).Checked;
                }
                dgvm.EndEdit();
            }
        }

        static void ckBox_CheckedChangedm1(object sender, EventArgs e)
        {
            if (dgvm1.Rows.Count > 0)
            {
                for (int i = 0; i < dgvm1.Rows.Count; i++)
                {
                    dgvm1.Rows[i].Cells[0].Value = ((System.Windows.Forms.CheckBox)sender).Checked;
                }
                dgvm1.EndEdit();
            }
        }
    }
}
