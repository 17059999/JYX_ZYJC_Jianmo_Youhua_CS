using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    #region 进度条线程
    /// <summary>进度条线程</summary>
    public class jindutiaoThread
    {
        #region 私有委托
        /// <summary>线程关闭的时候调用的委托</summary>
        private delegate void CloseDelegate();

        /// <summary>线程执行中调用的委托</summary>
        private delegate void nowValueDelegate(double dNowTime);
        #endregion

        #region 私有字段

        /// <summary>线程执行时调用的窗体</summary>
        private jindutiaoForm m_ProgressBarForm = null;

        /// <summary>进度条是否自动前进</summary>
        private bool m_bEnableTimer;

        /// <summary>预计消耗时间</summary>
     //   private double m_dFinishTimes;

        /// <summary>进度条标题</summary>
        private string m_sTitle;
        #endregion

        #region 公共构造
        /// <summary>构造方法</summary>
        /// <param name="dFinishTimes">预计消耗时间</param>
        /// <param name="bEnableTimer">进度条是否自动前进</param>
        public jindutiaoThread(string sTitle, bool bEnableTimer = true)
        {
           // m_dFinishTimes = dFinishTimes;
            m_bEnableTimer = bEnableTimer;
            m_sTitle = sTitle;
        }
        #endregion

        #region 析构
        /// <summary>析构</summary>
        ~jindutiaoThread()
        {
            //if (m_ProgressBarForm.IsDisposed || m_ProgressBarForm == null)
            //    return;
            //m_ProgressBarForm.progressBar1.Value = 100;
            //m_ProgressBarForm.lbtimer.Text = "100";
            //m_ProgressBarForm.timer1.Enabled = false;
            //m_ProgressBarForm.Close();
            //m_ProgressBarForm.Dispose();
        }
        #endregion

        #region 公共方法
        /// <summary>创建并启动线程</summary>
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(this.runMethod));           
            thread.Start();
        }

        /// <summary>启动委托设置新线程中窗体进度条的值</summary>
        /// <param name="dNowTime">新的进度条时间</param>
        public void SetValue(double dNowTime)
        {
            while (m_ProgressBarForm == null)
                Thread.Sleep(10);
            if (m_ProgressBarForm.IsHandleCreated)
            {
                nowValueDelegate now = new nowValueDelegate(setNow);
                //while (m_ProgressBarForm == null)
                //    Thread.Sleep(10);
                m_ProgressBarForm.BeginInvoke(now, dNowTime);
            }
            //Thread.Sleep(100);
        }

        /// <summary>启动委托完成进度条、关闭窗体</summary>
        public void End()
        {
            Thread.Sleep(1000);
            CloseDelegate close = new CloseDelegate(Close);
            m_ProgressBarForm.BeginInvoke(close);
        }
        #endregion

        #region 私有方法
        /// <summary>我被委托调用,专门设置进度条当前值的</summary>
        /// <param name="dNowTime">新的进度条时间</param>
        private void setNow(double dNowValue)
        {

            m_ProgressBarForm.progressBar1.Value = (int)Math.Floor(dNowValue);
            m_ProgressBarForm.lbtimer.Text = dNowValue.ToString("0.0")+"%";
        }

        /// <summary>新进程启动时的委托函数</summary>
        private void runMethod()
        {
            m_ProgressBarForm = new jindutiaoForm( m_sTitle, m_bEnableTimer);
            m_ProgressBarForm.progressBar1.Maximum = 100;
            m_ProgressBarForm.timer1.Enabled = true;
           // m_ProgressBarForm.Show();
            m_ProgressBarForm.ShowDialog();
        }
        /// <summary>我被委托调用,完成进度条、关闭窗体</summary>
        private void Close()
        {
            m_ProgressBarForm.progressBar1.Value = 100;
            m_ProgressBarForm.lbtimer.Text = "100";
            m_ProgressBarForm.timer1.Enabled = false;
            m_ProgressBarForm.Close();
            m_ProgressBarForm.Dispose();
        }
        #endregion
    }
    #endregion

}
