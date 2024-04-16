
using System;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class isRegisterForm : Form
    {
        private bool enable_shiyong;
        public isRegisterForm(bool enable_shiyong)
        {
            InitializeComponent();
            this.enable_shiyong = enable_shiyong;
        }

        private void isRegisterForm_Load(object sender, EventArgs e)
        {
            string mac = MyPublic_Api.GetMac();
            if (mac != "")
            {
                textBox_mac.Text = mac;
            }
            this.button_shiyong.Enabled = this.enable_shiyong;
        } 

        private void button_register_Click(object sender, EventArgs e)
        {
            string register_code = textBox_register.Text.Trim();
           
            string mac = textBox_mac.Text;

            string key = MyPublic_Api.jisuan_key(mac);
            if (key == register_code)
            {
                if (MyPublic_Api.create_register(register_code))
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
            else
            {
                MessageBox.Show("注册码错误,请联系管理员!");
                return;
            }
        }

        private void button_shiyong_Click(object sender, EventArgs e)
        {
            bool result = MyPublic_Api.create_shiyong_register();
            if(result)
            this.DialogResult = DialogResult.OK;
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
