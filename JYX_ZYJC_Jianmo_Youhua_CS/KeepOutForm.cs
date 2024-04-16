using Bentley.DgnPlatformNET;
using Bentley.Internal.MstnPlatformNET;
using Bentley.MstnPlatformNET;
using BIM = Bentley.Interop.MicroStationDGN;
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
using System.Reflection;
using Bentley.Building.Mechanical;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    public partial class KeepOutForm : //Form
#if DEBUG
        Form
#else
        Adapter
#endif
    {
        public static KeepOutForm keepOutForm = null;

        public static DgnFile dgnFile = Session.Instance.GetActiveDgnFile();

        public DisplayStyle disFor = null;

        public DisplayStyle disCut = null;

        public MethodInfo meSet = null;

        public MethodInfo meGet = null;

        IntPtr intptr = new IntPtr();

        public MechViewAddIn meAddin = new MechViewAddIn();

        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        public KeepOutForm()
        {
            InitializeComponent();
        }

        public static KeepOutForm instance()
        {
            if (keepOutForm == null)
            {
                keepOutForm = new KeepOutForm();
            }
            else
            {
                keepOutForm.Close();
                keepOutForm = new KeepOutForm();
            }

            return keepOutForm;
        }

        private void keepOutRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (disCut != null && disFor != null)
            {
                DisplayStyleFlags flag1 = disCut.GetFlags();
                flag1.DisplayHiddenEdges = false;
                disCut.SetFlags(flag1);

                DisplayStyleFlags flag2 = disFor.GetFlags();
                flag2.DisplayHiddenEdges = false;
                disFor.SetFlags(flag2);

                DisplayStyleManager.WriteDisplayStyleToFile(disCut, dgnFile);
                DisplayStyleManager.WriteDisplayStyleToFile(disFor, dgnFile);
            }
        }

        private void noKeepOutRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (disCut != null && disFor != null)
            {
                DisplayStyleFlags flag1 = disCut.GetFlags();
                flag1.DisplayHiddenEdges = true;
                disCut.SetFlags(flag1);

                DisplayStyleFlags flag2 = disFor.GetFlags();
                flag2.DisplayHiddenEdges = true;
                disFor.SetFlags(flag2);

                DisplayStyleManager.WriteDisplayStyleToFile(disCut, dgnFile);
                DisplayStyleManager.WriteDisplayStyleToFile(disFor, dgnFile);
            }
        }

        private void KeepOutForm_Load(object sender, EventArgs e)
        {
            #region 遮挡关系
            DisplayStyleList disList = new DisplayStyleList(dgnFile, false, false);
            IEnumerator<DisplayStyle> disIEtor = disList.GetEnumerator();
            bool isKeepOut = false;
            while (disIEtor.MoveNext())
            {
                DisplayStyle sty = disIEtor.Current;
                string name = sty.Name;
                if (name.Equals("剪切"))  //TODO 先默认切图按照Cut  Forward样式切图
                {
                    disCut = sty;
                }
                else if (name.Equals("向前"))
                {
                    disFor = sty;
                    DisplayStyleFlags flag = sty.GetFlags();
                    isKeepOut = flag.DisplayHiddenEdges;
                }
            }

            if (isKeepOut) noKeepOutRadioButton.Checked = true;
            else keepOutRadioButton.Checked = true;
            #endregion

            #region 中心线
            if (meGet == null) setMeInfo();

            object value = meGet.Invoke(meAddin, new object[] { intptr });

            bool b = Convert.ToBoolean(value);

            //if (b)
            //{
            //    radioButton1.Checked = true;
            //}
            //else
            //{
            //    radioButton2.Checked = true;
            //}

            radioButton1.Checked = true;
            #endregion
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
