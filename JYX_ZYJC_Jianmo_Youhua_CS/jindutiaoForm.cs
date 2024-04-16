using Bentley.MstnPlatformNET.WinForms;
using System;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    #region 进度条窗体
    /// <summary>进度条窗体</summary>
    public partial class jindutiaoForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        #region 私有字段
        /// <summary>预计消耗时间</summary>
       // private double m_dFinishTimes;

        /// <summary>进度条是否自动前进</summary>
        private bool m_bEnableTimer;

        /// <summary>实际消耗时间</summary>
        //private double m_dTimeTotal = 0;
        #endregion

        #region 公共属性
        /// <summary>预计消耗时间</summary>
        //public double dFinishTimes
        //{
        //    get { return m_dFinishTimes; }
        //    private set { }
        //}
        #endregion

        #region 公共构造
        /// <summary>构造</summary>
        /// <param name="dFinishTimes">预计消耗时间</param>
        /// <param name="bEnableTimer">进度条是否自动前进</param>
        public jindutiaoForm(string sTitle, bool bEnableTimer = true)
        {
            //m_dFinishTimes = dFinishTimes;
            m_bEnableTimer = bEnableTimer;

            InitializeComponent();
         //   this.lbFinishTime.Text = dFinishTimes.ToString("0.0");
            this.Text = sTitle;
        }
        #endregion

        #region 事件
        /// <summary>时间计数器0.1秒自增长</summary>
        private void timer1_Tick(object sender, EventArgs e)
        {
            //m_dTimeTotal++;
            //int iSpotMax = 5;
            //int iSpotCurrentCount = (int)(m_dTimeTotal / 10) % iSpotMax;
            //string sSpot = ".";
            //for (int i = 0; i < iSpotCurrentCount; i++)
            //    sSpot += ".";
            //this.lbspot.Text = sSpot;
            //this.lbtime.Text = (m_dTimeTotal / 10).ToString("0.0");
            //double dNowValue = 10 / m_dFinishTimes + Convert.ToDouble(this.lbtimer.Text.ToString());
            //if (m_bEnableTimer)
            //    if (dNowValue < this.progressBar1.Maximum)
            //    {
            //        this.progressBar1.Value = (int)Math.Floor(dNowValue);
            //        this.lbtimer.Text = dNowValue.ToString("0.0");
            //    }
        }
        #endregion

        private void jindutiaoForm_Load(object sender, EventArgs e)
        {

        }

        private void jindutiaoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
    #endregion
}
