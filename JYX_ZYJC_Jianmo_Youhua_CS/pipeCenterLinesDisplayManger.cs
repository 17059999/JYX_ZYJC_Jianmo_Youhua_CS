using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.MstnPlatformNET.WinForms;
using BIM = Bentley.Interop.MicroStationDGN;
using System.Windows.Forms;
using System.Reflection;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.MstnPlatformNET;
using Bentley.Building.Mechanical;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class pipeCenterLinesDisplayManger : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static pipeCenterLinesDisplayManger pipeCenterForm = null;

        public MethodInfo meSet = null;

        public MethodInfo meGet = null;

        IntPtr intptr = new IntPtr();

        public MechViewAddIn meAddin = new MechViewAddIn();

        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;

        public pipeCenterLinesDisplayManger()
        {
            InitializeComponent();
        }

        public static pipeCenterLinesDisplayManger instance()
        {
            if (pipeCenterForm == null)
            {
                pipeCenterForm = new pipeCenterLinesDisplayManger();
            }
            else
            {
                pipeCenterForm.Close();
                pipeCenterForm = new pipeCenterLinesDisplayManger();
            }

            return pipeCenterForm;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (meSet == null) setMeInfo();

            if (radioButton1.Checked)
            {
                object value = meSet.Invoke(BMECApi.Instance, new object[] { intptr, true });
                int index = Session.GetActiveViewport().ViewNumber;
                BIM.View vw = app.ActiveDesignFile.Views[index + 1];
                vw.Redraw(); //刷新当前视图
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (meSet == null) setMeInfo();

            if (radioButton2.Checked)
            {
                object value = meSet.Invoke(BMECApi.Instance, new object[] { intptr, false });
                int index = Session.GetActiveViewport().ViewNumber;
                BIM.View vw = app.ActiveDesignFile.Views[index + 1];
                vw.Redraw(); //刷新当前视图
            }
        }

        private void pipeCenterLinesDisplayManger_Load(object sender, EventArgs e)
        {
            if (meGet == null) setMeInfo();

            object value = meGet.Invoke(meAddin, new object[] { intptr });

            bool b = Convert.ToBoolean(value);

            if (b)
            {
                radioButton1.Checked = true;
            }
            else
            {
                radioButton2.Checked = true;
            }
        }

        public void setMeInfo()
        {
            Type getT = meAddin.GetType();
            //获取类型信息
            Type t = BMECApi.Instance.GetType();

            //调用方法的一些标注位（NonPublic、Public、Static）
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

            //获取方法信息  不写flag默认只能获取public类型
            meSet = t.GetMethod("SetShowCenterlines", flag);
            meGet = getT.GetMethod("GetShowCenterlines", flag);

            intptr = JYX_ZYJC_CLR.PublicMethod.SetShowCenterlines(false);
        }
    }
}
