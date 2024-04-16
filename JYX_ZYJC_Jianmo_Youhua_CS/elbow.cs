using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using System;
using System.Collections.Generic;
using BIM = Bentley.Interop.MicroStationDGN;
using BG = Bentley.GeometryNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.ECObjects.Instance;
using System.Windows.Forms;
using System.Linq;
using System.Collections;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.GeometryNET;
using Bentley.OpenPlantModeler.SDK.Utilities;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 先选择两个管道，根据两根管道的端点以及连接方式生成路径，如果生成路径成功，则让用户选择弯头，如果所选弯头符合路径大小则生成成功
    /// </summary>
    public class elbow : DgnElementSetTool
    {
        public static double angleTolerance = 0.5;//两管道异面判断的容差角度

        protected BMECApi api = BMECApi.Instance;
        static BIM.Application app = Utilities.ComApp;
        List<BMECObject> BMEC_Object_list = new List<BMECObject>();
        private static elbowBuzhiForm m_myForm;
        int pipe_count = 0;
        //private IECInstance elbowOrBendInstance;//应该从选择到创建都为同一个实例
        private BMECObject elbowOrBendECObject;
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Success;
        }
        public static void InstallNewTool()
        {
            elbow elbow = new elbow();
            elbow.InstallTool();
        }
        public static void m_formClosed()
        {
            m_myForm = null;
        }
        protected override void OnPostInstall()
        {
            try
            {
                GetPipeLine();
                if (m_myForm == null)
                {
                    m_myForm = new elbowBuzhiForm();

#if DEBUG
#else
                    m_myForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(elbowBuzhiForm));
                    m_myForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
                    m_myForm.Show();
                }
                else
                {
#if DEBUG
#else
                    m_myForm.AttachAsTopLevelForm(MyAddin.s_addin, true);
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(elbowBuzhiForm));
                    m_myForm.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
#endif
                    m_myForm.Show();
                }
            }
            catch (Exception ee)
            {
                System.Windows.Forms.MessageBox.Show(ee.ToString());
            }
            base.OnPostInstall();
            initelbowDic(elbowradiusdic, elbowangledic);
            elbowmitereddic.Clear();
            elbowmitereddic.Add("", "");//TODO
            app.ShowCommand("管件自动布置");
            app.ShowPrompt("请选择起点管道");
        }
        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (m_myForm == null) return;
#if DEBUG
            m_myForm.Hide();
#else
            m_myForm.Hide();
            m_myForm.DetachFromMicroStation();
#endif
        }
        protected override bool OnResetButton(DgnButtonEvent ev)
        {

            if (pipe_count == 0)
            {
                app.CommandState.StartDefaultCommand();
            }
            else
            {
                pipe_count--;
                BMEC_Object_list.RemoveAt(BMEC_Object_list.Count - 1);
            }
            return true;
        }
        protected override void OnRestartTool()
        {
            //InstallNewTool();
        }
        protected override bool NeedAcceptPoint()
        {
            return base.NeedAcceptPoint();
        }
        //private string elbowecclassname = "";
        private int chuangjianleixing = 1;
        //private IECInstance m_elbow_iec_instance;
        public static double rf = 1.0;
        //public static double tempdn = 100.0;

        public bool iscxh = false;
        private void temp()
        {
            if (Convert.ToDouble(m_myForm.textBox_radiusFactor.Text) != rf)
            {
                rf = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
                m_myForm.textBox_radius.Text = (rf * elbowBuzhiForm.dn).ToString();
            }
            else
            {
                rf = Convert.ToDouble(m_myForm.textBox_radius.Text) / elbowBuzhiForm.dn;
                m_myForm.textBox_radiusFactor.Text = rf.ToString();
            }
        }

        /// <summary>
        /// 根据选中的两个管道创建自动生成的弯头
        /// </summary>
        /// <param name="bmec_object1"></param>
        /// <param name="bmec_object2"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            temp();
            HitPath hit_path = DoLocate(ev, true, 0);
            if (hit_path == null)
            {
                return true;
            }
            Element elem = hit_path.GetHeadElement();

            BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

            if (bmec_object == null)
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return true;
            }
            else if (bmec_object.ClassName != "PIPE")
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是圆管，请重新选择！");
                return true;
            }
            else
            {
                pathType = getPathType(m_myForm.comboBox_lianjielujing.Text);
                if (pipe_count == 0)
                {
                    pipe_count++;
                    BMEC_Object_list.Add(bmec_object);

                    if (elbowBuzhiForm.dn != bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER"))
                    {
                        rf = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
                        m_myForm.textBox_radius.Text = (rf * bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER")).ToString();
                    }
                    else
                    {
                        temp();
                    }
                    elbowBuzhiForm.dn = bmec_object.GetDoubleValueInMM("NOMINAL_DIAMETER");
                    m_myForm.textBox_radius.Text = (Convert.ToDouble(m_myForm.textBox_radiusFactor.Text) * elbowBuzhiForm.dn).ToString();

                    app.ShowCommand("管件自动布置");
                    app.ShowPrompt("请选择终点管道");
                }
                else
                {
                    base.OnDataButton(ev);
                    //选择合法后，计算路径
                    if (bmec_object.Instance["GUID"].StringValue == BMEC_Object_list[0].Instance["GUID"].StringValue)
                    {
                        MessageBox.Show("前后两次选中的管道相同，请重新选择！");
                        return true;
                    }
                    else
                    {
                        //同径管道
                        BMEC_Object_list.Add(bmec_object);
                        string errorMessage;
                        if (m_myForm.comboBox_elbowOrBend.Text == "Elbow")
                        {
                            if (m_myForm.comboBox_elbow_radius.Text != "虾米弯")
                            {
                                chuangjianleixing = 1;//Elbow
                            }
                            else
                            {
                                chuangjianleixing = 3;//虾米弯
                            }
                        }
                        else
                        {
                            chuangjianleixing = 2;//Bend
                        }

                        if (NewCreateElbow(BMEC_Object_list[0], BMEC_Object_list[1], out errorMessage))//TODO 注意换回改好的
                        {
                            InstallNewTool();
                        }
                        else
                        {
                            BMEC_Object_list[0].SetLinearPoints(pipe1Point[0], pipe1Point[1]);
                            BMEC_Object_list[1].SetLinearPoints(pipe2Point[0], pipe2Point[1]);
                            BMEC_Object_list[0].Create();
                            BMEC_Object_list[1].Create();
                            if (errorMessage.Length != 0)
                            {
                                MessageBox.Show(errorMessage);
                            }
                            InstallNewTool();
                        }
                    }
                }
            }

            return true;
        }
        //路径方向
        public enum PathType
        {
            OFF = 0,
            XYZ,
            XZY,
            YXZ,
            YZX,
            ZXY,
            ZYX
        }
        private PathType pathType;
        private List<BG.DPoint3d> points = new List<BG.DPoint3d>();//生成的路径端点
        List<BG.DPoint3d> pipe1Point = new List<BG.DPoint3d>(2);
        List<BG.DPoint3d> pipe2Point = new List<BG.DPoint3d>(2);
        List<BG.DPoint3d> maleVerticalPipePoint = new List<BG.DPoint3d>(2);
        BMECObject maleVerticalPipe = new BMECObject();
        private BG.DSegment3d maleVerticalSegment;//公垂线段
        private bool yimianpanding = false;//是否进行异面判定

        public bool isR = false;
        public bool isM = false;
        //public bool CreateElbow(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage)
        //{
        //    try
        //    {
        //        errorMessage = string.Empty;

        //        pipe1Point.Clear();
        //        pipe2Point.Clear();
        //        maleVerticalPipePoint.Clear();
        //        pipe1Point = new List<BG.DPoint3d>(elbow.GetTowPortPoint(bmec_object1));
        //        pipe2Point = new List<BG.DPoint3d>(elbow.GetTowPortPoint(bmec_object2));

        //        double fractionA, fractionB;
        //        CalculateMaleVerticalLine(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), out maleVerticalSegment, out fractionA, out fractionB);

        //        BG.DPoint3d intersection;
        //        int lineToLineRelationFlag = LineLineIntersection(out intersection, pipe1Point[0], pipe1Point[1], pipe2Point[0], pipe2Point[1]);

        //        BG.DPoint3d p1 = pipe1Point[0];
        //        BG.DPoint3d p2 = pipe2Point[0];
        //        if (p1 == p2)
        //        {
        //            p2 = pipe2Point[1];
        //        }
        //        BG.DVector3d vector1 = new BG.DVector3d(p1, p2);
        //        BG.DVector3d faxiangliang1 = vector1.CrossProduct(new BG.DVector3d(pipe1Point[0], pipe1Point[1]));
        //        BG.DVector3d faxiangliang2 = vector1.CrossProduct(new BG.DVector3d(pipe2Point[0], pipe2Point[1]));
        //        bool pingxing = new BG.DVector3d(pipe1Point[0], pipe1Point[1]).IsParallelOrOppositeTo(new BG.DVector3d(pipe2Point[0], pipe2Point[1]));
        //        bool gongmian = faxiangliang1.IsParallelOrOppositeTo(faxiangliang2);
        //        BG.DSegment3d pipe1 = new BG.DSegment3d(pipe1Point[0], pipe1Point[1]);
        //        BG.DSegment3d pipe2 = new BG.DSegment3d(pipe2Point[0], pipe2Point[1]);
        //        DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

        //        //TODO 判定 Tee 连接
        //        bool isTeeConnection = m_myForm.radioButton_elbow.Checked ? false : true;

        //        CutOffPipe cutOffPipe = new CutOffPipe();
        //        //double tolerence = 1;//TODO
        //        if (pingxing)
        //        {
        //            bool istongyizhixian = false;
        //            if (pipe1Point[0] != pipe2Point[0] && pipe1Point[0] != pipe2Point[1] && pipe1Point[1] != pipe2Point[0] && pipe1Point[1] != pipe2Point[1])
        //            {
        //                BG.DVector3d vec1 = new BG.DVector3d(pipe1Point[0], pipe2Point[0]);
        //                BG.DVector3d vec2 = new BG.DVector3d(pipe1Point[0], pipe2Point[1]);
        //                if (vec1.IsParallelOrOppositeTo(vec2))
        //                {
        //                    //在同一直线上
        //                    istongyizhixian = true;
        //                }
        //                else
        //                {
        //                    istongyizhixian = false;
        //                }
        //            }
        //            else
        //            {
        //                //在同一直线上
        //                istongyizhixian = true;
        //            }
        //            if (istongyizhixian)
        //            {
        //                System.Windows.Forms.MessageBox.Show("所连接的两条管道在同一直线上，无法创建弯头");
        //                return false;
        //            }
        //            else
        //            {
        //                ////平行
        //                if (pathType == PathType.OFF)
        //                {
        //                    //最短路径连接
        //                    double fraction;
        //                    BG.DPoint3d closePoint;
        //                    pipe1.ClosestFractionAndPoint(pipe2.StartPoint, true, false, out fraction, out closePoint);
        //                    if (closePoint == pipe1.StartPoint)
        //                    {
        //                        System.Windows.Forms.MessageBox.Show("请确认连接管道的流向或连接的先后顺序");
        //                        return false;
        //                    }
        //                    BG.DSegment3d newSegment = new BG.DSegment3d(closePoint, pipe2.StartPoint);
        //                    maleVerticalPipe = CutOffPipe.CreatePipe(newSegment.StartPoint, newSegment.EndPoint, bmec_object1);
        //                    try
        //                    {
        //                        maleVerticalPipe.DiscoverConnectionsEx();
        //                        maleVerticalPipe.UpdateConnections();
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        MessageBox.Show(e.ToString());
        //                    }
        //                    if (isTeeConnection)
        //                    {
        //                        //BMECObject tempelbow1;
        //                        List<BMECObject> tempElbow1;
        //                        //BMECObject tempelbow2;
        //                        List<BMECObject> tempElbow2;
        //                        List<BMECObject> tempelbows = new List<BMECObject>();
        //                        bool isconnect1 = create_tee(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
        //                        tempelbows.AddRange(tempElbow1);
        //                        bool isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, out errorMessage, out tempElbow2);
        //                        tempelbows.AddRange(tempElbow2);
        //                        if (isconnect1 && isconnect2)
        //                        {
        //                            foreach (var item in BMEC_Object_list)
        //                            {
        //                                item.DiscoverConnectionsEx();
        //                                item.UpdateConnections();
        //                            }
        //                            maleVerticalPipe.DiscoverConnectionsEx();
        //                            maleVerticalPipe.UpdateConnections();
        //                            return true;
        //                        }
        //                        else
        //                        {

        //                            if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                            {
        //                                api.DeleteFromModel(maleVerticalPipe);
        //                            }
        //                            foreach (BMECObject item in tempelbows)
        //                            {
        //                                if (item != null)
        //                                {
        //                                    api.DeleteFromModel(item);
        //                                }
        //                            }
        //                            return false;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //BMECObject tempelbow1;
        //                        //BMECObject tempelbow2;
        //                        List<BMECObject> tempElbow1;
        //                        List<BMECObject> tempElbow2;
        //                        List<BMECObject> tempelbows = new List<BMECObject>();
        //                        bool isconnect1 = create_elbow(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
        //                        tempelbows.AddRange(tempElbow1);
        //                        bool isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, out errorMessage, out tempElbow2);
        //                        tempelbows.AddRange(tempElbow2);
        //                        if (isconnect1 && isconnect2)
        //                        {
        //                            foreach (var item in BMEC_Object_list)
        //                            {
        //                                item.DiscoverConnectionsEx();
        //                                item.UpdateConnections();
        //                            }
        //                            maleVerticalPipe.DiscoverConnectionsEx();
        //                            maleVerticalPipe.UpdateConnections();
        //                            return true;
        //                        }
        //                        else
        //                        {
        //                            if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                            {
        //                                api.DeleteFromModel(maleVerticalPipe);
        //                            }
        //                            foreach (BMECObject item in tempelbows)
        //                            {
        //                                if (item != null)
        //                                {
        //                                    api.DeleteFromModel(item);
        //                                }
        //                            }
        //                            return false;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    //按路径连接,TODO 处理生成的管道在同一直线合并的问题
        //                    List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
        //                    List<BMECObject> lianjieguandao = new List<BMECObject>();
        //                    for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
        //                    {
        //                        lianjieguandao.Add(CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1));
        //                    }
        //                    lianjieguandao.Insert(0, bmec_object1);
        //                    lianjieguandao.Add(bmec_object2);
        //                    //TODO检查是否重叠或相交
        //                    if (HasOverlap(lianjieguandao) /*|| isIntersect(lianjieguandao)*/)
        //                    {
        //                        //删除创建的管道
        //                        for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                        {
        //                            api.DeleteFromModel(lianjieguandao[i]);
        //                        }
        //                        System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
        //                        return false;
        //                    }
        //                    //将生成的管道连接
        //                    bool shifounenglianjie = true;
        //                    List<BMECObject> tempelbows = new List<BMECObject>();
        //                    for (int i = 0; i < lianjieguandao.Count - 1; i++)
        //                    {
        //                        //BMECObject tempelbow;
        //                        List<BMECObject> tempElbow;
        //                        bool flagtemp = false;
        //                        if (isTeeConnection)
        //                        {
        //                            //TODO 系统的不让连，所以这里没实现，如果要连再说
        //                            shifounenglianjie = false;
        //                            break;
        //                        }
        //                        else
        //                        {
        //                            flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempElbow);
        //                            if (errorMessage == "管道在同一直线上")
        //                            {
        //                                flagtemp = true;
        //                                errorMessage = "";
        //                            }
        //                        }
        //                        shifounenglianjie = shifounenglianjie && flagtemp;
        //                        tempelbows.AddRange(tempElbow);
        //                    }

        //                    if (shifounenglianjie)
        //                    {
        //                        //更新连结性
        //                        foreach (BMECObject item in lianjieguandao)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        //TODO合并能合并的管道
        //                        CutOffPipe.mergePipes(lianjieguandao);
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                        {
        //                            //删除创建的管道
        //                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                            {
        //                                api.DeleteFromModel(lianjieguandao[i]);
        //                            }
        //                        }
        //                        //删除创建的弯头
        //                        foreach (BMECObject item in tempelbows)
        //                        {
        //                            if (item != null)
        //                            {
        //                                api.DeleteFromModel(item);
        //                            }
        //                        }
        //                        return false;
        //                    }
        //                }
        //            }
        //        }
        //        else if (!gongmian)
        //        {
        //            //异面
        //            #region 非三通连接
        //            //异面时，如果连接的两根管道距离过近，无法生成弯头，则不能连接
        //            yimianpanding = true;
        //            if (pathType == PathType.OFF)
        //            {
        //                double distance = 10;//TODO
        //                if (maleVerticalSegment.Length < distance)
        //                {
        //                    //管道过近
        //                    errorMessage = "两个管道距离过近，无法创建弯头!";
        //                    return false;
        //                }
        //                //先根据公垂线段创建一根管道
        //                maleVerticalPipePoint.Add(maleVerticalSegment.StartPoint);
        //                maleVerticalPipePoint.Add(maleVerticalSegment.EndPoint);
        //                maleVerticalPipe = CutOffPipe.CreatePipe(maleVerticalPipePoint[0], maleVerticalPipePoint[1], bmec_object1);
        //                try
        //                {
        //                    maleVerticalPipe.DiscoverConnectionsEx();
        //                    maleVerticalPipe.UpdateConnections();
        //                }
        //                catch (Exception e)
        //                {
        //                    MessageBox.Show(e.ToString());
        //                }
        //                if (isTeeConnection)
        //                {
        //                    //BMECObject tempelbow1;
        //                    List<BMECObject> tempElbow1;
        //                    //BMECObject tempelbow2;
        //                    List<BMECObject> tempElbow2;
        //                    List<BMECObject> tempelbows = new List<BMECObject>();
        //                    bool isconnect1 = create_tee(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
        //                    tempelbows.AddRange(tempElbow1);
        //                    bool isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, out errorMessage, out tempElbow2);
        //                    tempelbows.AddRange(tempElbow2);
        //                    if (isconnect1 && isconnect2)
        //                    {
        //                        foreach (var item in BMEC_Object_list)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        maleVerticalPipe.DiscoverConnectionsEx();
        //                        maleVerticalPipe.UpdateConnections();
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                        {
        //                            api.DeleteFromModel(maleVerticalPipe);
        //                        }
        //                        foreach (BMECObject item in tempelbows)
        //                        {
        //                            if (item != null)
        //                            {
        //                                api.DeleteFromModel(item);
        //                            }
        //                        }
        //                        return false;
        //                    }
        //                }
        //                else
        //                {
        //                    //BMECObject tempelbow1;
        //                    //BMECObject tempelbow2;
        //                    List<BMECObject> tempElbow1;
        //                    List<BMECObject> tempElbow2;
        //                    List<BMECObject> tempelbows = new List<BMECObject>();
        //                    bool isconnect1 = create_elbow(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
        //                    tempelbows.AddRange(tempElbow1);
        //                    bool isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, out errorMessage, out tempElbow2);
        //                    tempelbows.AddRange(tempElbow2);
        //                    if (isconnect1 && isconnect2)
        //                    {
        //                        foreach (var item in BMEC_Object_list)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        maleVerticalPipe.DiscoverConnectionsEx();
        //                        maleVerticalPipe.UpdateConnections();
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                        {
        //                            api.DeleteFromModel(maleVerticalPipe);
        //                        }
        //                        foreach (BMECObject item in tempelbows)
        //                        {
        //                            if (item != null)
        //                            {
        //                                api.DeleteFromModel(item);
        //                            }
        //                        }
        //                        return false;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //按路径生成管道,TODO 处理生成的管道在同一直线合并的问题
        //                List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
        //                List<BMECObject> lianjieguandao = new List<BMECObject>();
        //                for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
        //                {
        //                    lianjieguandao.Add(CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1));
        //                }
        //                lianjieguandao.Insert(0, bmec_object1);
        //                lianjieguandao.Add(bmec_object2);
        //                if (HasOverlap(lianjieguandao)/* || isIntersect(lianjieguandao)*/)
        //                {
        //                    //删除创建的管道
        //                    for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                    {
        //                        api.DeleteFromModel(lianjieguandao[i]);
        //                    }
        //                    System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
        //                    return false;
        //                }
        //                //将生成的管道连接
        //                bool shifounenglianjie = true;
        //                List<BMECObject> tempelbows = new List<BMECObject>();
        //                bool isLastPipeWithTee = true;
        //                BG.DVector3d fangxiang = new BG.DVector3d(lianjieguandaoduandian[0], lianjieguandaoduandian[1]);
        //                if (!faxiangliang1.IsParallelOrOppositeTo(fangxiang))
        //                {
        //                    isLastPipeWithTee = false;
        //                }
        //                for (int i = 0; i < lianjieguandao.Count - 1; i++)
        //                {
        //                    List<BMECObject> tempELbows = new List<BMECObject>();
        //                    bool flagtemp = false;
        //                    if (isTeeConnection)
        //                    {
        //                        if (isLastPipeWithTee)
        //                        {
        //                            //第一根
        //                            if (i == 0)
        //                            {
        //                                flagtemp = create_tee(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempELbows);
        //                            }
        //                            else
        //                            {
        //                                flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempELbows);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            //第二根
        //                            if (i == 1)
        //                            {
        //                                flagtemp = create_tee(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempELbows);
        //                            }
        //                            else
        //                            {
        //                                flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempELbows);
        //                            }
        //                        }
        //                    }
        //                    else
        //                    {
        //                        flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempELbows);
        //                    }
        //                    if (errorMessage == "管道在同一直线上")
        //                    {
        //                        flagtemp = true;
        //                        errorMessage = "";
        //                    }
        //                    shifounenglianjie = shifounenglianjie && flagtemp;
        //                    if (true)
        //                    {

        //                    tempelbows.AddRange(tempELbows);
        //                    }
        //                }

        //                if (shifounenglianjie)
        //                {
        //                    //更新连结性
        //                    foreach (BMECObject item in lianjieguandao)
        //                    {
        //                        item.DiscoverConnectionsEx();
        //                        item.UpdateConnections();
        //                    }
        //                    //TODO合并能合并的管道
        //                    CutOffPipe.mergePipes(lianjieguandao);
        //                    return true;
        //                }
        //                else
        //                {
        //                    if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                    {
        //                        //删除创建的管道
        //                        for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                        {
        //                            api.DeleteFromModel(lianjieguandao[i]);
        //                        }
        //                    }
        //                    //删除创建的弯头
        //                    foreach (BMECObject item in tempelbows)
        //                    {
        //                        api.DeleteFromModel(item);
        //                    }
        //                    return false;
        //                }

        //            }
        //            #endregion
        //        }
        //        else
        //        {
        //            //相交
        //            #region 非三通连接
        //            if (pathType == PathType.OFF)
        //            {
        //                //BMECObject tempelbow;
        //                List<BMECObject> tempElbows;
        //                if (isTeeConnection)
        //                {
        //                    if (create_tee(bmec_object1, bmec_object2, out errorMessage, out tempElbows))
        //                    {
        //                        foreach (var item in BMEC_Object_list)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        return false;
        //                    }
        //                }
        //                else
        //                {
        //                    if (create_elbow(bmec_object1, bmec_object2, out errorMessage, out tempElbows))
        //                    {
        //                        foreach (var item in BMEC_Object_list)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        return false;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                //按路径连接,TODO 处理生成的管道在同一直线合并的问题
        //                List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
        //                if (lianjieguandaoduandian == null || lianjieguandaoduandian.Count == 1)
        //                {
        //                    //BMECObject tempelbow;
        //                    List<BMECObject> tempELbows;
        //                    if (isTeeConnection)
        //                    {
        //                        create_tee(BMEC_Object_list[0], BMEC_Object_list[1], out errorMessage, out tempELbows);
        //                    }
        //                    else
        //                    {
        //                        create_elbow(BMEC_Object_list[0], BMEC_Object_list[1], out errorMessage, out tempELbows);
        //                    }
        //                }
        //                else
        //                {
        //                    //按照路径生成管道，管道两两连接
        //                    List<BMECObject> lianjieguandao = new List<BMECObject>();
        //                    for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
        //                    {
        //                        lianjieguandao.Add(CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1));
        //                    }
        //                    lianjieguandao.Insert(0, bmec_object1);
        //                    lianjieguandao.Add(bmec_object2);
        //                    if (HasOverlap(lianjieguandao)/* || isIntersect(lianjieguandao)*/)
        //                    {
        //                        //删除创建的管道
        //                        for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                        {
        //                            api.DeleteFromModel(lianjieguandao[i]);
        //                        }
        //                        System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
        //                        return false;
        //                    }
        //                    //将生成的管道连接
        //                    bool shifounenglianjie = true;
        //                    List<BMECObject> tempelbows = new List<BMECObject>();
        //                    for (int i = 0; i < lianjieguandao.Count - 1; i++)
        //                    {
        //                        //BMECObject tempelbow;
        //                        List<BMECObject> tempElbow;
        //                        bool flagtemp = false;
        //                        if (isTeeConnection)
        //                        {
        //                            //TODO 系统的不让连，所以这里没实现，如果要连再说
        //                            shifounenglianjie = false;
        //                            break;
        //                        }
        //                        else
        //                        {
        //                            flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempElbow);
        //                        }
        //                        shifounenglianjie = shifounenglianjie && flagtemp;
        //                        tempelbows.AddRange(tempElbow);
        //                    }

        //                    if (shifounenglianjie)
        //                    {
        //                        //更新连结性
        //                        foreach (BMECObject item in lianjieguandao)
        //                        {
        //                            item.DiscoverConnectionsEx();
        //                            item.UpdateConnections();
        //                        }
        //                        //TODO合并能合并的管道
        //                        CutOffPipe.mergePipes(lianjieguandao);
        //                        return true;
        //                    }
        //                    else
        //                    {
        //                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？","连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
        //                        {
        //                            //删除创建的管道
        //                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
        //                            {
        //                                api.DeleteFromModel(lianjieguandao[i]);
        //                            }
        //                        }
        //                        //删除创建的弯头
        //                        foreach (BMECObject item in tempelbows)
        //                        {
        //                            api.DeleteFromModel(item);
        //                        }
        //                        return false;
        //                    }
        //                }

        //                return false;
        //            }
        //            #endregion
        //            //}
        //        }
        //        //try end
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show(e.ToString());
        //        errorMessage = "exception";
        //        return false;
        //    }
        //}
        //TODO 修改了CreateElbow方法，记得该回去
        public bool NewCreateElbow(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage)
        {
            //string tempError;
            errorMessage = string.Empty;
            try
            {

                pipe1Point.Clear();
                pipe2Point.Clear();
                maleVerticalPipePoint.Clear();
                pipe1Point = new List<BG.DPoint3d>(elbow.GetTowPortPoint(bmec_object1));
                pipe2Point = new List<BG.DPoint3d>(elbow.GetTowPortPoint(bmec_object2));

                double fractionA, fractionB;
                CalculateMaleVerticalLine(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), out maleVerticalSegment, out fractionA, out fractionB);

                BG.DPoint3d intersection;
                int lineToLineRelationFlag = LineLineIntersection(out intersection, pipe1Point[0], pipe1Point[1], pipe2Point[0], pipe2Point[1]);

                BG.DPoint3d p1 = pipe1Point[0];
                BG.DPoint3d p2 = pipe2Point[0];
                double tempd = new BG.DVector3d(pipe1Point[0], pipe1Point[1]).AngleTo(new BG.DVector3d(p1, p2)).Degrees;
                tempd = tempd % 180.0;
                if (p1 == p2)
                {
                    p2 = pipe2Point[1];
                }
                else if (angleTolerance - Math.Abs(tempd - 180.0) > 0 || angleTolerance - tempd > 0)
                {
                    p2 = pipe2Point[1];
                }
                BG.DVector3d vector1 = new BG.DVector3d(p1, p2);
                BG.DVector3d faxiangliang1 = vector1.CrossProduct(new BG.DVector3d(pipe1Point[0], pipe1Point[1]));
                BG.DVector3d faxiangliang2 = vector1.CrossProduct(new BG.DVector3d(pipe2Point[0], pipe2Point[1]));
                bool pingxing = new BG.DVector3d(pipe1Point[0], pipe1Point[1]).IsParallelOrOppositeTo(new BG.DVector3d(pipe2Point[0], pipe2Point[1]));
                bool gongmian = faxiangliang1.IsParallelOrOppositeTo(faxiangliang2);

                double faxianjiajiao = faxiangliang1.AngleTo(faxiangliang2).Degrees;
                faxianjiajiao = faxianjiajiao % 180.0;
                gongmian = gongmian || angleTolerance - Math.Abs(faxianjiajiao - 180.0) > 0 || angleTolerance - faxianjiajiao > 0;

                BG.DSegment3d pipe1 = new BG.DSegment3d(pipe1Point[0], pipe1Point[1]);
                BG.DSegment3d pipe2 = new BG.DSegment3d(pipe2Point[0], pipe2Point[1]);
                DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

                //TODO 判定 Tee 连接
                bool isTeeConnection = m_myForm.radioButton_elbow.Checked ? false : true;

                CutOffPipe cutOffPipe = new CutOffPipe();
                //double tolerence = 1;//TODO
                if (pingxing)
                {
                    bool istongyizhixian = false;
                    if (pipe1Point[0] != pipe2Point[0] && pipe1Point[0] != pipe2Point[1] && pipe1Point[1] != pipe2Point[0] && pipe1Point[1] != pipe2Point[1])
                    {
                        BG.DVector3d vec1 = new BG.DVector3d(pipe1Point[0], pipe2Point[0]);
                        BG.DVector3d vec2 = new BG.DVector3d(pipe1Point[0], pipe2Point[1]);
                        if (vec1.IsParallelOrOppositeTo(vec2))
                        {
                            //在同一直线上
                            istongyizhixian = true;
                        }
                        else
                        {
                            istongyizhixian = false;
                        }
                    }
                    else
                    {
                        //在同一直线上
                        istongyizhixian = true;
                    }
                    if (istongyizhixian)
                    {
                        System.Windows.Forms.MessageBox.Show("所连接的两条管道在同一直线上，无法创建弯头");
                        return false;
                    }
                    else
                    {
                        ////平行
                        if (pathType == PathType.OFF)
                        {
                            //最短路径连接
                            double fraction;
                            BG.DPoint3d closePoint;
                            pipe1.ClosestFractionAndPoint(pipe2.StartPoint, true, false, out fraction, out closePoint);
                            if (closePoint == pipe1.StartPoint)
                            {
                                System.Windows.Forms.MessageBox.Show("请确认连接管道的流向或连接的先后顺序");
                                return false;
                            }
                            BG.DSegment3d newSegment = new BG.DSegment3d(closePoint, pipe2.StartPoint);

                            #region 生成三通
                            if (isTeeConnection)
                            {
                                maleVerticalPipe = CutOffPipe.CreatePipe(newSegment.StartPoint, newSegment.EndPoint, bmec_object1); //按照最短路径生成的管道
                                //maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1);
                                //maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1); //测试
                                //TODO 需要先验证用elbow生成时是否可以生成 不行 就计算可以生成的角度在构成 管道
                                try
                                {
                                    maleVerticalPipe.DiscoverConnectionsEx();
                                    maleVerticalPipe.UpdateConnections();
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                                //BMECObject tempelbow1;
                                //BMECObject tempelbow2;
                                List<BMECObject> tempElbow1;
                                List<BMECObject> tempElbow2 = new List<BMECObject>();
                                List<BMECObject> tempelbows = new List<BMECObject>();
                                bool isconnect1 = create_tee(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
                                if (isconnect1)
                                {
                                    tempelbows.AddRange(tempElbow1);
                                }
                                bool isconnect2, isL;
                                IECInstance iec;

                                bool isGetData = selectElbowData(maleVerticalPipe, bmec_object2, out iec, out errorMessage, out isL);

                                if (isGetData)
                                {
                                    isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, iec, isL, out errorMessage, out tempElbow2);
                                }
                                else
                                {
                                    isconnect2 = false;
                                }

                                if (isconnect2)
                                {
                                    tempelbows.AddRange(tempElbow2);
                                }
                                if (isconnect1 && isconnect2)
                                {
                                    foreach (var item in BMEC_Object_list)
                                    {
                                        item.DiscoverConnectionsEx();
                                        item.UpdateConnections();
                                    }
                                    foreach (BMECObject item in tempelbows)
                                    {
                                        item.DiscoverConnectionsEx();
                                        item.UpdateConnections();
                                    }
                                    maleVerticalPipe.DiscoverConnectionsEx();
                                    maleVerticalPipe.UpdateConnections();
                                    return true;
                                }
                                else
                                {
                                    if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                    {
                                        api.DeleteFromModel(maleVerticalPipe);

                                    }
                                    foreach (BMECObject item in tempelbows)
                                    {
                                        if (item != null)
                                        {
                                            api.DeleteFromModel(item);
                                        }
                                    }
                                    return false;
                                }
                            }
                            #endregion
                            //TODO 需要处理按最短路径连接时，最短路径太短无法生成的情况  （改变角度 直到可以生成）
                            else
                            {
                                List<BMECObject> tempElbow1 = new List<BMECObject>();
                                List<BMECObject> tempElbow2 = new List<BMECObject>();
                                List<BMECObject> tempelbows = new List<BMECObject>();

                                bool isconnect1, isconnect2, isL;
                                IECInstance iec;

                                bool isGetData = selectElbowData(bmec_object1, bmec_object1, out iec, out errorMessage, out isL);

                                if (isGetData)
                                {
                                    maleVerticalPipe = CutOffPipe.CreatePipe(newSegment.StartPoint, newSegment.EndPoint, bmec_object1);
                                }
                                else
                                {
                                    maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1);
                                }

                                //maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1); //测试
                                //TODO 需要先验证用elbow生成时是否可以生成 不行 就计算可以生成的角度在构成 管道
                                try
                                {
                                    maleVerticalPipe.DiscoverConnectionsEx();
                                    maleVerticalPipe.UpdateConnections();
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                                //BMECObject tempelbow1;
                                //BMECObject tempelbow2;


                                if (isGetData)
                                {
                                    isconnect1 = create_elbow(bmec_object1, maleVerticalPipe, iec, isL, out errorMessage, out tempElbow1);
                                }
                                else
                                {
                                    isconnect1 = false;
                                }
                                if (isconnect1)
                                {
                                    tempelbows.AddRange(tempElbow1);
                                }

                                bool isL1;
                                IECInstance iec1;

                                bool isGetData1 = selectElbowData(maleVerticalPipe, bmec_object2, out iec1, out errorMessage, out isL1);

                                if (isGetData1)
                                {
                                    isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, iec1, isL1, out errorMessage, out tempElbow2);
                                }
                                else
                                {
                                    isconnect2 = false;
                                }

                                if (isconnect2)
                                {
                                    tempelbows.AddRange(tempElbow2);
                                }
                                if (isconnect1 && isconnect2)
                                {
                                    foreach (var item in BMEC_Object_list)
                                    {
                                        item.DiscoverConnectionsEx();
                                        item.UpdateConnections();
                                    }
                                    maleVerticalPipe.DiscoverConnectionsEx();
                                    maleVerticalPipe.UpdateConnections();
                                    foreach (BMECObject item in tempelbows)
                                    {
                                        item.DiscoverConnectionsEx();
                                        item.UpdateConnections();
                                    }
                                    return true;
                                }
                                else
                                {
                                    if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                    {
                                        api.DeleteFromModel(maleVerticalPipe);
                                    }
                                    foreach (BMECObject item in tempelbows)
                                    {
                                        if (item != null)
                                        {
                                            api.DeleteFromModel(item);
                                        }
                                    }
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            //按路径连接,TODO 处理生成的管道在同一直线合并的问题
                            List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
                            List<BMECObject> lianjieguandao = new List<BMECObject>();
                            for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
                            {
                                BMECObject tempPipe = CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1);
                                double tempDistance = tempPipe.GetNthPoint(0).Distance(tempPipe.GetNthPoint(1));
                                if (tempDistance < 10)
                                {
                                    api.DeleteFromModel(tempPipe);
                                    continue;
                                }
                                lianjieguandao.Add(tempPipe);
                            }
                            lianjieguandao.Insert(0, bmec_object1);
                            lianjieguandao.Add(bmec_object2);
                            #region MyRegion
                            //TODO
                            //if (new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[0].GetNthPoint(1)).IsParallelOrOppositeTo(new BG.DVector3d(lianjieguandao[1].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1))))
                            //{
                            //    if (HasOverlap(new List<BMECObject>() { lianjieguandao[0], lianjieguandao[1] }))
                            //    {
                            //    }
                            //}
                            //if (new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).IsParallelOrOppositeTo(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(1))))
                            //{
                            //}
                            #endregion
                            if (lianjieguandao.Count < 3)
                            {
                                System.Windows.Forms.MessageBox.Show("未计算出合适的中间连接管道");
                                return false;
                            }
                            //TODO检查是否重叠或相交
                            if (HasOverlap(lianjieguandao) /*|| isIntersect(lianjieguandao)*/)
                            {
                                #region MyRegion
                                //删除创建的管道
                                //for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                //{
                                //    api.DeleteFromModel(lianjieguandao[i]);
                                //}
                                //System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                                //return false;
                                #endregion
                                if (HasOverlap(new List<BMECObject>() { lianjieguandao[0], lianjieguandao[1] }))
                                {
                                    if (new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[0].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1))) < 0)
                                    {
                                        if (MessageBox.Show("该路径连接下，起点管道与第一路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将起点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                            api.DeleteFromModel(lianjieguandao[1]);
                                            lianjieguandao.RemoveAt(1);
                                        }
                                        else
                                        {
                                            //删除创建的管道
                                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                            {
                                                api.DeleteFromModel(lianjieguandao[i]);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[1]);
                                        lianjieguandao.RemoveAt(1);
                                    }
                                }
                                else if (IsOverlap(lianjieguandao[lianjieguandao.Count - 1], lianjieguandao[lianjieguandao.Count - 2]))
                                {
                                    if ((new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0))) > 0) && (new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0))) > 0))
                                    {
                                        if (MessageBox.Show("该路径连接下，终点管道与第三路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将终点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                            api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                            lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                        }
                                        else
                                        {
                                            //删除创建的管道
                                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                            {
                                                api.DeleteFromModel(lianjieguandao[i]);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                        lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                    }
                                }
                                else
                                {
                                    System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                                    return false;
                                }
                            }
                            //将生成的管道连接
                            bool shifounenglianjie = true;
                            List<BMECObject> tempelbows = new List<BMECObject>();
                            for (int i = 0; i < lianjieguandao.Count - 1; i++)
                            {
                                //BMECObject tempelbow;
                                List<BMECObject> tempELbow;
                                bool flagtemp = false;
                                if (isTeeConnection)
                                {
                                    //TODO 系统的不让连，所以这里没实现，如果要连再说
                                    shifounenglianjie = false;
                                    break;
                                }
                                else
                                {
                                    bool isL;
                                    IECInstance iec;

                                    bool isGetData = selectElbowData(lianjieguandao[i], lianjieguandao[i + 1], out iec, out errorMessage, out isL);

                                    if (isGetData)
                                    {
                                        flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], iec, isL, out errorMessage, out tempELbow);
                                    }
                                    else
                                    {
                                        tempELbow = null;
                                    }

                                    if (errorMessage == "管道在同一直线上")
                                    {
                                        flagtemp = true;
                                        errorMessage = "";
                                    }
                                }
                                shifounenglianjie = shifounenglianjie && flagtemp;
                                if (tempELbow != null)
                                {
                                    tempelbows.AddRange(tempELbow);
                                }
                            }

                            if (shifounenglianjie)
                            {
                                //更新连结性
                                foreach (BMECObject item in lianjieguandao)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                foreach (BMECObject item in tempelbows)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }

                                return true;
                            }
                            else
                            {
                                if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    //删除创建的管道
                                    for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                    {
                                        api.DeleteFromModel(lianjieguandao[i]);
                                    }
                                }
                                //删除创建的弯头
                                foreach (BMECObject item in tempelbows)
                                {
                                    if (item != null)
                                    {
                                        api.DeleteFromModel(item);
                                    }
                                }
                                return false;
                            }
                        }
                    }
                }
                else if (!gongmian)
                {
                    //异面
                    #region 非三通连接
                    //异面时，如果连接的两根管道距离过近，无法生成弯头，则不能连接
                    yimianpanding = true;
                    if (pathType == PathType.OFF)
                    {
                        double distance = 10;//TODO
                        if (maleVerticalSegment.Length < distance)
                        {
                            //管道过近
                            errorMessage = "两个管道距离过近，无法创建弯头!";
                            return false;
                        }
                        //先根据公垂线段创建一根管道
                        maleVerticalPipePoint.Add(maleVerticalSegment.StartPoint);
                        maleVerticalPipePoint.Add(maleVerticalSegment.EndPoint);
                        if (isTeeConnection)
                        {
                            maleVerticalPipe = CutOffPipe.CreatePipe(maleVerticalPipePoint[0], maleVerticalPipePoint[1], bmec_object1);
                            try
                            {
                                maleVerticalPipe.DiscoverConnectionsEx();
                                maleVerticalPipe.UpdateConnections();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            //BMECObject tempelbow1;
                            //BMECObject tempelbow2;
                            List<BMECObject> tempElbow1;
                            List<BMECObject> tempElbow2 = new List<BMECObject>();
                            List<BMECObject> tempelbows = new List<BMECObject>();
                            bool isconnect1 = create_tee(bmec_object1, maleVerticalPipe, out errorMessage, out tempElbow1);
                            if (isconnect1)
                            {
                                tempelbows.AddRange(tempElbow1);
                            }

                            bool isconnect2, isL;
                            IECInstance iec;

                            bool isGetData = selectElbowData(maleVerticalPipe, bmec_object2, out iec, out errorMessage, out isL);

                            if (isGetData)
                            {
                                isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, iec, isL, out errorMessage, out tempElbow2);
                            }
                            else
                            {
                                isconnect2 = false;
                            }

                            if (isconnect2)
                            {
                                tempelbows.AddRange(tempElbow2);
                            }
                            if (isconnect1 && isconnect2)
                            {
                                foreach (var item in BMEC_Object_list)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                maleVerticalPipe.DiscoverConnectionsEx();
                                maleVerticalPipe.UpdateConnections();
                                foreach (BMECObject item in tempelbows)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                return true;
                            }
                            else
                            {
                                if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    api.DeleteFromModel(maleVerticalPipe);
                                }
                                foreach (BMECObject item in tempelbows)
                                {
                                    if (item != null)
                                    {
                                        api.DeleteFromModel(item);
                                    }
                                }
                                return false;
                            }
                        }
                        else
                        {
                            maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1);
                            try
                            {
                                maleVerticalPipe.DiscoverConnectionsEx();
                                maleVerticalPipe.UpdateConnections();
                            }
                            catch (Exception e)
                            {
                                MessageBox.Show(e.ToString());
                            }
                            //BMECObject tempelbow1;
                            //BMECObject tempelbow2;
                            List<BMECObject> tempElbow1 = new List<BMECObject>();
                            List<BMECObject> tempElbow2 = new List<BMECObject>();
                            List<BMECObject> tempelbows = new List<BMECObject>();

                            bool isconnect1, isconnect2, isL;
                            IECInstance iec;

                            bool isGetData = selectElbowData(bmec_object1, maleVerticalPipe, out iec, out errorMessage, out isL);

                            if (isGetData)
                            {
                                isconnect1 = create_elbow(bmec_object1, maleVerticalPipe, iec, isL, out errorMessage, out tempElbow1);
                            }
                            else
                            {
                                isconnect1 = false;
                            }
                            if (isconnect1)
                            {
                                tempelbows.AddRange(tempElbow1);
                            }

                            bool isL1;
                            IECInstance iec1;

                            bool isGetData1 = selectElbowData(maleVerticalPipe, bmec_object2, out iec1, out errorMessage, out isL1);

                            if (isGetData1)
                            {
                                isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, iec1, isL1, out errorMessage, out tempElbow2);
                            }
                            else
                            {
                                isconnect2 = false;
                            }

                            if (isconnect2)
                            {
                                tempelbows.AddRange(tempElbow2);
                            }
                            if (isconnect1 && isconnect2)
                            {
                                foreach (var item in BMEC_Object_list)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                maleVerticalPipe.DiscoverConnectionsEx();
                                maleVerticalPipe.UpdateConnections();
                                foreach (BMECObject item in tempelbows)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                return true;
                            }
                            else
                            {
                                if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    api.DeleteFromModel(maleVerticalPipe);
                                }
                                foreach (BMECObject item in tempelbows)
                                {
                                    if (item != null)
                                    {
                                        api.DeleteFromModel(item);
                                    }
                                }
                                return false;
                            }
                        }
                    }
                    else
                    {
                        //按路径生成管道,TODO 处理生成的管道在同一直线合并的问题
                        List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
                        List<BMECObject> lianjieguandao = new List<BMECObject>();
                        for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
                        {
                            lianjieguandao.Add(CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1));
                        }
                        lianjieguandao.Insert(0, bmec_object1);
                        lianjieguandao.Add(bmec_object2);
                        if (HasOverlap(lianjieguandao)/* || isIntersect(lianjieguandao)*/)
                        {
                            //删除创建的管道
                            //for (int i = 1; i < lianjieguandao.Count - 1; i++)
                            //{
                            //    api.DeleteFromModel(lianjieguandao[i]);
                            //}
                            //System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                            //return false;
                            if (HasOverlap(new List<BMECObject>() { lianjieguandao[0], lianjieguandao[1] }))
                            {
                                if (new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[0].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1))) < 0)
                                {
                                    if (MessageBox.Show("该路径连接下，起点管道与第一路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将起点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[1]);
                                        lianjieguandao.RemoveAt(1);
                                    }
                                    else
                                    {
                                        //删除创建的管道
                                        for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                        {
                                            api.DeleteFromModel(lianjieguandao[i]);
                                        }
                                        return false;
                                    }
                                }
                                else
                                {
                                    lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                    api.DeleteFromModel(lianjieguandao[1]);
                                    lianjieguandao.RemoveAt(1);
                                }
                            }
                            else if (IsOverlap(lianjieguandao[lianjieguandao.Count - 1], lianjieguandao[lianjieguandao.Count - 2]))
                            {
                                if ((new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0))) > 0) && (new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0))) > 0))
                                {
                                    if (MessageBox.Show("该路径连接下，终点管道与第三路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将终点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                    {
                                        lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                        lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                    }
                                    else
                                    {
                                        //删除创建的管道
                                        for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                        {
                                            api.DeleteFromModel(lianjieguandao[i]);
                                        }
                                        return false;
                                    }
                                }
                                else
                                {
                                    lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                    api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                    lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                }
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                                return false;
                            }
                        }
                        //TODO合并能合并的管道
                        lianjieguandao = CutOffPipe.mergePipes(lianjieguandao);
                        //将生成的管道连接
                        bool shifounenglianjie = true;
                        List<BMECObject> tempelbows = new List<BMECObject>();
                        bool isLastPipeWithTee = true;
                        BG.DVector3d fangxiang = new BG.DVector3d(lianjieguandaoduandian[0], lianjieguandaoduandian[1]);
                        if (!faxiangliang1.IsParallelOrOppositeTo(fangxiang))
                        {
                            isLastPipeWithTee = false;
                        }
                        for (int i = 0; i < lianjieguandao.Count - 1; i++)
                        {
                            //BMECObject tempelbow;
                            List<BMECObject> tempelbow;
                            bool flagtemp = false;
                            if (isTeeConnection)
                            {
                                if (isLastPipeWithTee)
                                {
                                    //第一根
                                    if (i == 0)
                                    {
                                        flagtemp = create_tee(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempelbow);
                                    }
                                    else
                                    {
                                        bool isL;
                                        IECInstance iec;
                                        bool isGetData = selectElbowData(lianjieguandao[i], lianjieguandao[i + 1], out iec, out errorMessage, out isL);

                                        if (isGetData)
                                        {
                                            flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], iec, isL, out errorMessage, out tempelbow);
                                        }
                                        else
                                        {
                                            tempelbow = null;
                                        }
                                    }
                                }
                                else
                                {
                                    //第二根
                                    if (i == 1)
                                    {
                                        flagtemp = create_tee(lianjieguandao[i], lianjieguandao[i + 1], out errorMessage, out tempelbow);
                                    }
                                    else
                                    {
                                        bool isL;
                                        IECInstance iec;
                                        bool isGetData = selectElbowData(lianjieguandao[i], lianjieguandao[i + 1], out iec, out errorMessage, out isL);

                                        if (isGetData)
                                        {
                                            flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], iec, isL, out errorMessage, out tempelbow);
                                        }
                                        else
                                        {
                                            tempelbow = null;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                bool isL;
                                IECInstance iec;
                                bool isGetData = selectElbowData(lianjieguandao[i], lianjieguandao[i + 1], out iec, out errorMessage, out isL);

                                if (isGetData)
                                {
                                    flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], iec, isL, out errorMessage, out tempelbow);
                                }
                                else
                                {
                                    tempelbow = null;
                                }
                            }
                            if (errorMessage == "管道在同一直线上")
                            {
                                flagtemp = true;
                                errorMessage = "";
                            }
                            shifounenglianjie = shifounenglianjie && flagtemp;
                            if (tempelbow != null)
                            {
                                tempelbows.AddRange(tempelbow);
                            }
                        }

                        if (shifounenglianjie)
                        {
                            //更新连结性
                            foreach (BMECObject item in lianjieguandao)
                            {
                                item.DiscoverConnectionsEx();
                                item.UpdateConnections();
                            }
                            foreach (BMECObject item in tempelbows)
                            {
                                item.DiscoverConnectionsEx();
                                item.UpdateConnections();
                            }
                            //TODO合并能合并的管道
                            //CutOffPipe.mergePipes(lianjieguandao);
                            return true;
                        }
                        else
                        {
                            if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                            {
                                //删除创建的管道
                                for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                {
                                    api.DeleteFromModel(lianjieguandao[i]);
                                }
                            }
                            //删除创建的弯头
                            foreach (BMECObject item in tempelbows)
                            {
                                api.DeleteFromModel(item);
                            }
                            return false;
                        }

                    }
                    #endregion
                }
                else
                {
                    //相交
                    #region range
                    if (pathType == PathType.OFF)
                    {
                        List<BMECObject> tempelbow = new List<BMECObject>();
                        if (isTeeConnection)
                        {
                            if (create_tee(bmec_object1, bmec_object2, out errorMessage, out tempelbow))
                            {
                                foreach (var item in BMEC_Object_list)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                foreach (BMECObject item in tempelbow)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        //TODO 需要处理按最短路径连接时，最短路径太短无法生成的情况  （改变角度 直到可以生成）
                        else
                        {
                            bool isL2;
                            IECInstance iec2;

                            bool isGetData2 = selectElbowData(bmec_object1, bmec_object2, out iec2, out errorMessage, out isL2);

                            if (isGetData2)
                            {
                                if (isL2)
                                {
                                    List<BMECObject> tempelbows = new List<BMECObject>();

                                    bool isC = create_elbow(bmec_object1, bmec_object2, iec2, isL2, out errorMessage, out tempelbows);

                                    if (isC)
                                    {
                                        foreach (var item in BMEC_Object_list)
                                        {
                                            item.DiscoverConnectionsEx();
                                            item.UpdateConnections();
                                        }
                                        foreach (BMECObject item in tempelbows)
                                        {
                                            item.DiscoverConnectionsEx();
                                            item.UpdateConnections();
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                        {
                                            //api.DeleteFromModel(maleVerticalPipe);
                                        }
                                        if (tempelbows != null)
                                        {
                                            foreach (BMECObject item in tempelbows)
                                            {
                                                if (item != null)
                                                {
                                                    api.DeleteFromModel(item);
                                                }
                                            }
                                        }
                                        return false;
                                    }
                                }
                                else
                                {
                                    maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1);
                                    //maleVerticalPipe = CutOffPipe.CreatePipe(pipe1.EndPoint, pipe2.StartPoint, bmec_object1); //测试
                                    //TODO 需要先验证用elbow生成时是否可以生成 不行 就计算可以生成的角度在构成 管道
                                    try
                                    {
                                        maleVerticalPipe.DiscoverConnectionsEx();
                                        maleVerticalPipe.UpdateConnections();
                                    }
                                    catch (Exception e)
                                    {
                                        MessageBox.Show(e.ToString());
                                    }
                                    //BMECObject tempelbow1;
                                    //BMECObject tempelbow2;
                                    List<BMECObject> tempElbow1 = new List<BMECObject>();
                                    List<BMECObject> tempElbow2 = new List<BMECObject>();
                                    List<BMECObject> tempelbows = new List<BMECObject>();

                                    bool isconnect1, isconnect2, isL;
                                    IECInstance iec;

                                    bool isGetData = selectElbowData(bmec_object1, maleVerticalPipe, out iec, out errorMessage, out isL);

                                    if (isGetData)
                                    {
                                        isconnect1 = create_elbow(bmec_object1, maleVerticalPipe, iec, isL, out errorMessage, out tempElbow1);
                                    }
                                    else
                                    {
                                        isconnect1 = false;
                                    }
                                    if (isconnect1)
                                    {
                                        tempelbows.AddRange(tempElbow1);
                                    }

                                    //bool isL1;
                                    //IECInstance iec1;

                                    //bool isGetData1 = selectElbowData(maleVerticalPipe, bmec_object2, out iec1, out errorMessage, out isL1);

                                    if (isGetData2)
                                    {
                                        isconnect2 = create_elbow(maleVerticalPipe, bmec_object2, iec2, isL2, out errorMessage, out tempElbow2);
                                    }
                                    else
                                    {
                                        isconnect2 = false;
                                    }

                                    if (isconnect2)
                                    {
                                        tempelbows.AddRange(tempElbow2);
                                    }
                                    if (isconnect1 && isconnect2)
                                    {
                                        foreach (var item in BMEC_Object_list)
                                        {
                                            item.DiscoverConnectionsEx();
                                            item.UpdateConnections();
                                        }
                                        maleVerticalPipe.DiscoverConnectionsEx();
                                        maleVerticalPipe.UpdateConnections();
                                        foreach (BMECObject item in tempelbows)
                                        {
                                            item.DiscoverConnectionsEx();
                                            item.UpdateConnections();
                                        }
                                        return true;
                                    }
                                    else
                                    {
                                        if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                        {
                                            api.DeleteFromModel(maleVerticalPipe);
                                        }
                                        foreach (BMECObject item in tempelbows)
                                        {
                                            if (item != null)
                                            {
                                                api.DeleteFromModel(item);
                                            }
                                        }
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                return false;
                            }


                        }
                    }
                    else
                    {
                        //按路径连接,TODO 处理生成的管道在同一直线合并的问题
                        List<BG.DPoint3d> lianjieguandaoduandian = Calculate(new BG.DSegment3d(pipe1Point[0], pipe1Point[1]), new BG.DSegment3d(pipe2Point[0], pipe2Point[1]), pathType, out errorMessage);
                        if (lianjieguandaoduandian == null || lianjieguandaoduandian.Count == 1)
                        {
                            //BMECObject tempelbow;
                            List<BMECObject> tempelbow;
                            if (isTeeConnection)
                            {
                                create_tee(BMEC_Object_list[0], BMEC_Object_list[1], out errorMessage, out tempelbow);
                            }
                            else
                            {
                                bool isL;
                                IECInstance iec;

                                bool isGetData = selectElbowData(BMEC_Object_list[0], BMEC_Object_list[1], out iec, out errorMessage, out isL);

                                if (isGetData)
                                {
                                    create_elbow(BMEC_Object_list[0], BMEC_Object_list[1], iec, isL, out errorMessage, out tempelbow);
                                }
                                else
                                {
                                    tempelbow = null;
                                }

                            }
                        }
                        else
                        {
                            //按照路径生成管道，管道两两连接
                            List<BMECObject> lianjieguandao = new List<BMECObject>();
                            for (int i = 0; i < lianjieguandaoduandian.Count - 1; i++)
                            {
                                BMECObject tempPipe = CutOffPipe.CreatePipe(lianjieguandaoduandian[i], lianjieguandaoduandian[i + 1], bmec_object1);
                                double tempDistance = tempPipe.GetNthPoint(0).Distance(tempPipe.GetNthPoint(1));
                                if (tempDistance < 10)
                                {
                                    api.DeleteFromModel(tempPipe);
                                    continue;
                                }
                                lianjieguandao.Add(tempPipe);
                            }
                            lianjieguandao.Insert(0, bmec_object1);
                            lianjieguandao.Add(bmec_object2);
                            if (HasOverlap(lianjieguandao)/* || isIntersect(lianjieguandao)*/)
                            {
                                //删除创建的管道
                                //for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                //{
                                //    api.DeleteFromModel(lianjieguandao[i]);
                                //}
                                //System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                                //return false;
                                if (HasOverlap(new List<BMECObject>() { lianjieguandao[0], lianjieguandao[1] }))
                                {
                                    if (new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[0].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1))) < 0)
                                    {
                                        if (MessageBox.Show("该路径连接下，起点管道与第一路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将起点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                            api.DeleteFromModel(lianjieguandao[1]);
                                            lianjieguandao.RemoveAt(1);
                                        }
                                        else
                                        {
                                            //删除创建的管道
                                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                            {
                                                api.DeleteFromModel(lianjieguandao[i]);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        lianjieguandao[0].SetLinearPoints(lianjieguandao[0].GetNthPoint(0), lianjieguandao[1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[1]);
                                        lianjieguandao.RemoveAt(1);
                                    }
                                }
                                else if (IsOverlap(lianjieguandao[lianjieguandao.Count - 1], lianjieguandao[lianjieguandao.Count - 2]))
                                {
                                    if ((new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0))) > 0) && (new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1)).DotProduct(new BG.DVector3d(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(0))) > 0))
                                    {
                                        if (MessageBox.Show("该路径连接下，终点管道与第三路径方向一致，但将管道缩短仍不足以满足该路径下的连接，为达成连接条件，须将终点管道反向延申，是否仍然执行连接操作？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                        {
                                            lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                            api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                            lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                        }
                                        else
                                        {
                                            //删除创建的管道
                                            for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                            {
                                                api.DeleteFromModel(lianjieguandao[i]);
                                            }
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        lianjieguandao[lianjieguandao.Count - 1].SetLinearPoints(lianjieguandao[lianjieguandao.Count - 2].GetNthPoint(0), lianjieguandao[lianjieguandao.Count - 1].GetNthPoint(1));
                                        api.DeleteFromModel(lianjieguandao[lianjieguandao.Count - 2]);
                                        lianjieguandao.RemoveAt(lianjieguandao.Count - 2);
                                    }
                                }
                                else
                                {
                                    System.Windows.Forms.MessageBox.Show("当前方式无法生成管道");
                                    return false;
                                }
                            }
                            lianjieguandao = CutOffPipe.mergePipes(lianjieguandao);
                            //将生成的管道连接
                            bool shifounenglianjie = true;
                            List<BMECObject> tempelbows = new List<BMECObject>();
                            for (int i = 0; i < lianjieguandao.Count - 1; i++)
                            {
                                //BMECObject tempelbow;
                                List<BMECObject> tempelbow;
                                bool flagtemp = false;
                                if (isTeeConnection)
                                {
                                    //TODO 系统的不让连，所以这里没实现，如果要连再说
                                    shifounenglianjie = false;
                                    break;
                                }
                                else
                                {
                                    bool isL;
                                    IECInstance iec;

                                    bool isGetData = selectElbowData(lianjieguandao[i], lianjieguandao[i + 1], out iec, out errorMessage, out isL);

                                    if (isGetData)
                                    {
                                        flagtemp = create_elbow(lianjieguandao[i], lianjieguandao[i + 1], iec, isL, out errorMessage, out tempelbow);
                                    }
                                    else
                                    {
                                        tempelbow = null;
                                    }
                                }
                                shifounenglianjie = shifounenglianjie && flagtemp;
                                if (tempelbow != null)
                                {
                                    tempelbows.AddRange(tempelbow);
                                }
                            }

                            if (shifounenglianjie)
                            {
                                //更新连结性
                                foreach (BMECObject item in lianjieguandao)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                foreach (BMECObject item in tempelbows)
                                {
                                    item.DiscoverConnectionsEx();
                                    item.UpdateConnections();
                                }
                                //TODO合并能合并的管道
                                //CutOffPipe.mergePipes(lianjieguandao);
                                return true;
                            }
                            else
                            {
                                if (System.Windows.Forms.MessageBox.Show("创建弯头时异常，将不会生成弯头，是否仍然生成管道？", "连接管道", MessageBoxButtons.YesNo) == DialogResult.No)
                                {
                                    //删除创建的管道
                                    for (int i = 1; i < lianjieguandao.Count - 1; i++)
                                    {
                                        api.DeleteFromModel(lianjieguandao[i]);
                                    }
                                }
                                //删除创建的弯头
                                foreach (BMECObject item in tempelbows)
                                {
                                    api.DeleteFromModel(item);
                                }
                                return false;
                            }
                        }

                        return false;
                    }
                    #endregion
                    //}
                }
                //try end
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                if (errorMessage.Equals(string.Empty))
                {
                    errorMessage = "存在未捕获的exception";
                }
                return false;
            }
        }

        private PathType getPathType(string pathType)
        {
            PathType result = PathType.OFF;
            switch (pathType)
            {
                case "OFF":
                    result = PathType.OFF;
                    break;
                case "XYZ":
                    result = PathType.XYZ;
                    break;
                case "XZY":
                    result = PathType.XZY;
                    break;
                case "YXZ":
                    result = PathType.YXZ;
                    break;
                case "YZX":
                    result = PathType.YZX;
                    break;
                case "ZXY":
                    result = PathType.ZXY;
                    break;
                case "ZYX":
                    result = PathType.ZYX;
                    break;
                default:
                    result = PathType.OFF;
                    break;
            }
            return result;
        }
        //private IECInstance elbowOrBendInstanceTemplate;//弯头类型
        private string elbowOrBendECClassName = "";//弯头类名
        double nominalDiameter;//弯头公称直径
        //public bool CreateElbowOrBendECInstance(BMECObject bmec_object1, out string errorMessage, out IECInstance elbowOrBendtemplate)
        //{
        //    elbowOrBendtemplate = null;
        //    errorMessage = "";
        //    nominalDiameter = bmec_object1.GetDoubleValueInMM("NOMINAL_DIAMETER");
        //    if (chuangjianleixing == 1)//elbow
        //    {
        //        elbowOrBendECClassName = getElbowECClassName(m_myForm.comboBox_elbow_radius.Text, m_myForm.comboBox_elbow_angle.Text);
        //        elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
        //        if (elbowOrBendInstance == null)
        //        {
        //            errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
        //            return false;
        //        }
        //        ISpecProcessor isp = api.SpecProcessor;
        //        isp.FillCurrentPreferences(elbowOrBendInstance, null);
        //        elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
        //        ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
        //        if (eCInstanceList.Count == 0)
        //        {
        //            errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
        //            return false;
        //        }
        //        elbowOrBendInstance = eCInstanceList[0];
        //    }
        //    else//bend
        //    {
        //        if (chuangjianleixing == 2)//bend
        //        {
        //            elbowOrBendECClassName = "PIPE_BEND";
        //        }
        //        else if (chuangjianleixing == 3)//xiamiwan
        //        {
        //            elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX"; //MITERED_PIPE_BEND_JYX PIPE_ELBOW_TRIMMED_JYX
        //        }
        //        elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
        //        if (elbowOrBendInstance == null)
        //        {
        //            errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
        //            return false;
        //        }
        //        ISpecProcessor isp = api.SpecProcessor;
        //        isp.FillCurrentPreferences(elbowOrBendInstance, null);
        //        elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
        //        ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
        //        if (eCInstanceList.Count == 0)
        //        {
        //            errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
        //            return false;
        //        }
        //        elbowOrBendInstance = eCInstanceList[0];
        //    }
        //    return true;
        //}
        //private bool isDeletemaleVerticalPipe = false;

        public bool selectElbowData(BMECObject bmec1, BMECObject bmec2, out IECInstance iec, out string errorMessage, out bool isL)
        {
            string spec = bmec1.Instance["SPECIFICATION"].StringValue;
            string lineNumber1 = bmec1.Instance["LINENUMBER"].StringValue;
            if (pipelinesName.Count > 0)
            {
                int index1 = -1;
                index1 = pipelinesName.IndexOf(lineNumber1);
                if (index1 != -1)
                {
                    StandardPreferencesUtilities.SetActivePipingNetworkSystem(pipingNetworkSystems[index1]);
                    StandardPreferencesUtilities.ChangeSpecification(spec);
                }
            }

            bool isData = true;

            iec = null;
            isL = false;
            errorMessage = string.Empty;

            double dn1 = bmec1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double dn2 = bmec2.Instance["NOMINAL_DIAMETER"].DoubleValue;
            bool isYijingWantou;//是否为异径弯头
            isYijingWantou = dn1 == dn2 ? false : true;
            nominalDiameter = bmec1.Instance["NOMINAL_DIAMETER"].DoubleValue;

            #region 获取elbow数据
            if (chuangjianleixing == 1)//elbow
            {

                if (!isYijingWantou)
                {
                    //同径弯头
                    if (m_myForm.comboBox_elbow_radius.Text == "异径弯头")
                    {
                        elbowOrBendECClassName = getElbowECClassName("短半径弯头", m_myForm.comboBox_elbow_angle.Text);
                    }
                    else
                    {
                        elbowOrBendECClassName = getElbowECClassName(m_myForm.comboBox_elbow_radius.Text, m_myForm.comboBox_elbow_angle.Text);
                    }
                }
                else
                {
                    if (m_myForm.comboBox_elbow_radius.Text == "异径弯头")
                    {
                        if (dn1 != dn2)
                        {
                            elbowOrBendECClassName = GetYijingElbow(m_myForm.comboBox_elbow_angle.Text);
                        }
                        else
                        {
                            elbowOrBendECClassName = getElbowECClassName("短半径弯头", m_myForm.comboBox_elbow_angle.Text);//TODO
                        }
                    }
                    else
                    {
                        errorMessage = "所选两根管道的管径不同，当前类型暂不支持连接！";
                        //elbows = null;
                        return false;
                    }
                }

                IECInstance elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass类型，请确认已配置该类型！";
                    //elbows = null;
                    return false;
                }
                ISpecProcessor isp = api.SpecProcessor;
                isp.FillCurrentPreferences(elbowOrBendInstance, null);
                elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                if (isYijingWantou)
                {
                    elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = dn1;
                    elbowOrBendInstance["NOMINAL_DIAMETER_RUN_END"].DoubleValue = dn2;
                }
                ECInstanceList eCInstanceList;
                if (!isYijingWantou)
                {
                    eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);//TODO 筛选条件
                }
                else
                {
                    Hashtable whereClauseList = new Hashtable();
                    whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                    whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                    eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                }

                if (eCInstanceList.Count == 0)
                {
                    errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + nominalDiameter.ToString() + "的对应数据项，请确认已配置数据！";
                    //errorMessage = "";
                    //elbows = null;
                    return false;
                }

                iec = eCInstanceList[0];
            }
            #endregion

            #region 获取bend数据
            else//bend
            {
                if (isYijingWantou)
                {
                    if (m_myForm.comboBox_elbow_radius.Text != "虾米弯")
                    {
                        errorMessage = "所选两根管道的管径不同，当前类型暂不支持连接！";
                        //elbows = null;
                        return false;
                    }
                }

                if (chuangjianleixing == 2)//bend
                {
                    elbowOrBendECClassName = "PIPE_BEND";
                }
                else if (chuangjianleixing == 3)//xiamiwan
                {
                    elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX"; //MITERED_PIPE_BEND_JYX PIPE_ELBOW_TRIMMED_JYX
                }
                IECInstance elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                if (elbowOrBendInstance == null)
                {
                    errorMessage = "没有找到该ECClass类型，请确认已配置该类型！";
                    //elbows = null;
                    return false;
                }
                ISpecProcessor isp = api.SpecProcessor;
                isp.FillCurrentPreferences(elbowOrBendInstance, null);
                elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;

                if (chuangjianleixing == 3)
                {
                    string dushu = m_myForm.comboBox_elbow_angle.Text;
                    int tempindex = dushu.IndexOf("度", 0);
                    char[] destination = new char[tempindex];
                    dushu.CopyTo(0, destination, 0, tempindex);
                    double cankaoAngle = Convert.ToDouble(new string(destination));
                    //if (!m_myForm.checkBox_isPlaceTrimmedElbow.Checked)
                    //{
                    //    if (Math.Abs(cankaoAngle - angle) > 1)
                    //    {
                    //        errorMessage = "当前角度不符！";
                    //        //elbows = null;
                    //        return false;
                    //    }
                    //}
                    Hashtable whereClauseList = new Hashtable();
                    whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                    whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                    whereClauseList.Add("wanqu_jiaodu", cankaoAngle);
                    ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                    if (eCInstanceList.Count == 0)
                    {
                        errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + dn1.ToString() + "的对应数据项，请确认已配置数据！";
                        //elbows = null;
                        return false;
                    }
                    iec = eCInstanceList[0];
                }
                else
                {
                    ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                    if (eCInstanceList.Count == 0)
                    {
                        errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + nominalDiameter.ToString() + "的对应数据项，请确认已配置数据！";
                        //elbows = null;
                        return false;
                    }
                    iec = eCInstanceList[0];
                }
            }
            #endregion

            isL = isLjfs(iec);

            return isData;
        }

        public bool isLjfs(IECInstance iec)
        {
            bool isL = false;

            string dm1 = "", dm2 = "";

            if (iec.GetPropertyValue("CERI_END_COND_1") != null)
            {
                dm1 = iec["CERI_END_COND_1"].StringValue;
            }

            if (iec.GetPropertyValue("CERI_END_COND_2") != null)
            {
                dm2 = iec["CERI_END_COND_1"].StringValue;
            }

            List<string> endList = new List<string> { "SCF", "SCM", "SWF", "SWM" };

            if (endList.Contains(dm1) || endList.Contains(dm2))
            {
                iscxh = true;
            }

            isL = m_myForm.radioButton_yigenwantou.Checked;

            return isL;
        }

        /// <summary>
        /// 根据选中的两个管道创建自动生成的弯头
        /// </summary>
        /// <param name="bmec_object1"></param>
        /// <param name="bmec_object2"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool create_elbow(BMECObject bmec_object1, BMECObject bmec_object2, IECInstance elbowOrBendInstance, bool isL, out string errorMessage, /*out BMECObject elbow*/out List<BMECObject> elbows)
        {
            bool isSingleElbow = true;
            int unbrokenElbow = 0;
            double brokenElbowDgree = 0.0f;
            double elbowDegree = 0.0f;
            elbows = new List<BMECObject>();
            //elbowOrBendInstance = null;
            List<IECInstance> elbowsInstanceList = new List<IECInstance>();
            double cankaoCenterToMainPort = 0.0f;
            double temp1 = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
            double temp2 = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMeter;
            double temp3 = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerGrid;
            double temp4 = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerStorage;
            double temp5 = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerSub;

            double dn1 = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double dn2 = bmec_object2.Instance["NOMINAL_DIAMETER"].DoubleValue;
            bool isYijingWantou;//是否为异径弯头
            isYijingWantou = dn1 == dn2 ? false : true;

            Bentley.GeometryNET.DPoint3d[] line1Point = GetTowPortPoint(bmec_object1);
            Bentley.GeometryNET.DPoint3d[] line2Point = GetTowPortPoint(bmec_object2);
            errorMessage = string.Empty;

            BG.DSegment3d lineA = new BG.DSegment3d(line1Point[0], line1Point[1]);
            BG.DSegment3d lineB = new BG.DSegment3d(line2Point[0], line2Point[1]);

            BG.DSegment3d segment;
            double fractionA, fractionB;
            CalculateMaleVerticalLine(lineA, lineB, out segment, out fractionA, out fractionB);
            BG.DPoint3d intersect = segment.StartPoint;

            BIM.Point3d intersect_point = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(intersect);
            BIM.LineElement line = app.CreateLineElement2(null, app.Point3dZero(), intersect_point);

            BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];//第一根管道的端点
            BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];//第二根管道的端点
            line1_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line1Point[0]);
            line1_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line1Point[1]);
            line2_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line2Point[0]);
            line2_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line2Point[1]);
            BIM.Point3d nearest_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            BIM.Point3d faster_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            if (app.Point3dEqual(nearest_point1, faster_point1))
            {
                nearest_point1 = line1_end_pts[0];
                faster_point1 = line1_end_pts[1];
            }

            BIM.Point3d nearest_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            BIM.Point3d faster_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            if (app.Point3dEqual(nearest_point2, faster_point2))
            {
                nearest_point2 = line2_end_pts[0];
                faster_point2 = line2_end_pts[1];
            }
            BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);//管道一方向向量
            BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);//管道二方向向量
            double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));

            nominalDiameter = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double insulationThickness = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
            string insulation = bmec_object1.Instance["INSULATION"].StringValue;
            double wallThickness = bmec_object1.Instance["WALL_THICKNESS"].DoubleValue;
            string material = bmec_object1.Instance["MATERIAL"].StringValue;

            double centerToMainPort = 0.0;
            double centerToRunPort = 0.0;
            double lengthToBend = 0.0;
            double lengthAfterBend = 0.0;
            double pipeLength1 = 0.0;
            double pipeLength2 = 0.0;
            double d1, d2;

            d1 = app.Point3dDistance(intersect_point, faster_point1);
            d2 = app.Point3dDistance(intersect_point, faster_point2);
            //TODO
            BG.DPoint3d dPoint_intersect = new DPoint3d(intersect_point.X, intersect_point.Y, intersect_point.Z);
            BG.DPoint3d d1Point_faster = new DPoint3d(faster_point1.X, faster_point1.Y, faster_point1.Z);
            BG.DPoint3d d2Point_faster = new DPoint3d(faster_point2.X, faster_point2.Y, faster_point2.Z);
            double templen1 = (d1Point_faster - dPoint_intersect).Magnitude;
            double templen2 = (d2Point_faster - dPoint_intersect).Magnitude;

            double radius = 0.0;

            if (chuangjianleixing == 1)//elbow
            {

                //if (!isYijingWantou)
                //{
                //    //同径弯头
                //    if (m_myForm.comboBox_elbow_radius.Text == "异径弯头")
                //    {
                //        elbowOrBendECClassName = getElbowECClassName("短半径弯头", m_myForm.comboBox_elbow_angle.Text);
                //    }
                //    else
                //    {
                //        elbowOrBendECClassName = getElbowECClassName(m_myForm.comboBox_elbow_radius.Text, m_myForm.comboBox_elbow_angle.Text);
                //    }
                //}
                //else
                //{
                //    if (m_myForm.comboBox_elbow_radius.Text == "异径弯头")
                //    {
                //        if (dn1 != dn2)
                //        {
                //            elbowOrBendECClassName = GetYijingElbow(m_myForm.comboBox_elbow_angle.Text);
                //        }
                //        else
                //        {
                //            elbowOrBendECClassName = getElbowECClassName("短半径弯头", m_myForm.comboBox_elbow_angle.Text);//TODO
                //        }
                //    }
                //    else
                //    {
                //        errorMessage = "所选两根管道的管径不同，当前类型暂不支持连接！";
                //        elbows = null;
                //        return false;
                //    }
                //}
                //TODO 容差值是？
                double cankaoAngle = Convert.ToDouble(elbowangledic[m_myForm.comboBox_elbow_angle.Text]);
                if (!m_myForm.checkBox_isPlaceTrimmedElbow.Checked || iscxh)
                {
                    if (Math.Abs(cankaoAngle - angle) > 1)
                    {
                        errorMessage = "当前条件下，没有找到对应角度的弯头！";
                        //elbows = null;
                        return false;
                    }
                }
                else
                {
                    if (angle <= cankaoAngle)
                    {
                        isSingleElbow = true;
                    }
                    else
                    {
                        unbrokenElbow = (int)(angle / cankaoAngle);
                        brokenElbowDgree = angle % cankaoAngle;
                        if (unbrokenElbow > 2 || (brokenElbowDgree > 1e-1 && unbrokenElbow > 1))
                        {
                            errorMessage = "当前角度下需要连接的弯头数量大于2，请使用其它角度弯头连接或更改连接方式！";
                            //elbows = null;
                            return false;
                        }
                        elbowDegree = cankaoAngle;
                        isSingleElbow = false;
                    }
                }

                //elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                //if (elbowOrBendInstance == null)
                //{
                //    errorMessage = "没有找到该ECClass类型，请确认已配置该类型！";
                //    elbows = null;
                //    return false;
                //}
                //ISpecProcessor isp = api.SpecProcessor;
                //isp.FillCurrentPreferences(elbowOrBendInstance, null);
                //elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
                //if (isYijingWantou)
                //{
                //    elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = dn1;
                //    elbowOrBendInstance["NOMINAL_DIAMETER_RUN_END"].DoubleValue = dn2;
                //}
                //ECInstanceList eCInstanceList;
                //if (!isYijingWantou)
                //{
                //    eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);//TODO 筛选条件
                //}
                //else
                //{
                //    Hashtable whereClauseList = new Hashtable();
                //    whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                //    whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                //    eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                //}

                //if (eCInstanceList.Count == 0)
                //{
                //    errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + nominalDiameter.ToString() + "的对应数据项，请确认已配置数据！";
                //    //errorMessage = "";
                //    elbows = null;
                //    return false;
                //}

                //elbowOrBendInstance = eCInstanceList[0];

                centerToMainPort = elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
                cankaoCenterToMainPort = centerToMainPort;
                centerToRunPort = centerToMainPort;

                BMECObject tempObject = new BMECObject(elbowOrBendInstance);

                if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["SOCKET_DEPTH"].DoubleValue;
                }
                else if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
                    lengthAfterBend = tempObject.Ports[1].Instance["THREADED_LENGTH"].DoubleValue;
                }
                //calculate radius CTR_END_M AND CTR_END_R
                double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                radius = centerToMainPort / cankaoTan;
                double curtan = Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
                double curcmp = curtan * radius;

                centerToMainPort = curcmp;
                centerToRunPort = centerToMainPort;

                pipeLength1 = d1 - centerToMainPort + lengthToBend;
                pipeLength2 = d2 - centerToRunPort + lengthAfterBend;
                //create elbow
                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
                elbowOrBendInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
                elbowOrBendInstance["MATERIAL"].StringValue = material;
                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;
            }
            else//bend
            {
                //if (isYijingWantou)
                //{
                //    if (m_myForm.comboBox_elbow_radius.Text != "虾米弯")
                //    {
                //        errorMessage = "所选两根管道的管径不同，当前类型暂不支持连接！";
                //        elbows = null;
                //        return false;
                //    }
                //}
                double radiusFactor = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
                radius = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text) * nominalDiameter;
                lengthToBend = Convert.ToDouble(m_myForm.textBox_lengthToBend.Text);
                lengthAfterBend = Convert.ToDouble(m_myForm.textBox_lengthAfterBend.Text);

                m_myForm.textBox_radius.Text = radius.ToString();

                //if (chuangjianleixing == 2)//bend
                //{
                //    elbowOrBendECClassName = "PIPE_BEND";
                //}
                //else if (chuangjianleixing == 3)//xiamiwan
                //{
                //    elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX"; //MITERED_PIPE_BEND_JYX PIPE_ELBOW_TRIMMED_JYX
                //}
                //elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
                //if (elbowOrBendInstance == null)
                //{
                //    errorMessage = "没有找到该ECClass类型，请确认已配置该类型！";
                //    elbows = null;
                //    return false;
                //}
                //ISpecProcessor isp = api.SpecProcessor;
                //isp.FillCurrentPreferences(elbowOrBendInstance, null);
                //elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;

                if (chuangjianleixing == 3)
                {
                    string dushu = m_myForm.comboBox_elbow_angle.Text;
                    int tempindex = dushu.IndexOf("度", 0);
                    char[] destination = new char[tempindex];
                    dushu.CopyTo(0, destination, 0, tempindex);
                    double cankaoAngle = Convert.ToDouble(new string(destination));
                    if (!m_myForm.checkBox_isPlaceTrimmedElbow.Checked || iscxh)
                    {
                        if (Math.Abs(cankaoAngle - angle) > 1)
                        {
                            errorMessage = "当前角度不符！";
                            //elbows = null;
                            return false;
                        }
                    }
                    //Hashtable whereClauseList = new Hashtable();
                    //whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                    //whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                    //whereClauseList.Add("wanqu_jiaodu", cankaoAngle);
                    //ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
                    //if (eCInstanceList.Count == 0)
                    //{
                    //    errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + dn1.ToString() + "的对应数据项，请确认已配置数据！";
                    //    elbows = null;
                    //    return false;
                    //}
                    //elbowOrBendInstance = eCInstanceList[0];
                }
                else
                {
                    //ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
                    //if (eCInstanceList.Count == 0)
                    //{
                    //    errorMessage = "没有找到ECClass为" + elbowOrBendInstance.ClassDefinition.Name + "  dn为" + nominalDiameter.ToString() + "的对应数据项，请确认已配置数据！";
                    //    elbows = null;
                    //    return false;
                    //}
                    //elbowOrBendInstance = eCInstanceList[0];
                }

                elbowOrBendInstance["ANGLE"].DoubleValue = angle;
                elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
                elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
                elbowOrBendInstance["INSULATION"].StringValue = insulation;
                elbowOrBendInstance["BEND_POINT_RADIUS"].DoubleValue = radiusFactor;
                elbowOrBendInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
                elbowOrBendInstance["MATERIAL"].StringValue = material;
                if (chuangjianleixing == 3)
                {
                    elbowOrBendInstance["wanqu_jiaodu"].DoubleValue = angle;

                    radius = elbowOrBendInstance["wanqu_banjing"].DoubleValue;
                    double tanA = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
                    centerToMainPort = radius / tanA;
                    centerToRunPort = centerToMainPort;

                    pipeLength1 = d1 - centerToMainPort;
                    pipeLength2 = d2 - centerToRunPort;
                    elbowOrBendInstance["caizhi"].StringValue = "";
                    elbowOrBendInstance["bihou"].DoubleValue = wallThickness;

                }
                else
                {
                    elbowOrBendInstance["LENGTH_TO_BEND"].DoubleValue = lengthToBend;
                    elbowOrBendInstance["LENGTH_AFTER_BEND"].DoubleValue = lengthAfterBend;
                    double tanA = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
                    centerToMainPort = radius / tanA;
                    centerToRunPort = centerToMainPort;

                    pipeLength1 = d1 - centerToMainPort - lengthToBend;
                    pipeLength2 = d2 - centerToRunPort - lengthAfterBend;
                }
            }


            //管道异面时，判断管道间距离是否足够
            if (yimianpanding)
            {
                double minDistanceTwoPipe = (chuangjianleixing == 2 ? (centerToMainPort + (lengthToBend + lengthAfterBend) / 2.0) : centerToMainPort) * 2 * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                double maleVerticalLength = maleVerticalSegment.Length;
                if (maleVerticalLength - minDistanceTwoPipe > 0.0)
                {
                    //符合条件
                }
                else
                {
                    //距离不够
                    errorMessage = "所连接的管道间距离过近，无法生成弯头！";
                    //elbows = null;
                    return false;
                }
            }
            //当管道过短时，无法生成
            if (pipeLength1 <= 0 || pipeLength2 <= 0)
            {
                if (pipeLength1 <= 0 && pipeLength2 > 0) isM = true;
                else if (pipeLength2 <= 0 && pipeLength1 > 0) isR = true;
                else
                {
                    isM = true;
                    isR = true;
                }
                errorMessage = "所连接的管道过短，无法生成弯头！";
                //elbows = null;
                return false;
            }
            //fill form
            m_myForm.textBox_elbow_dn.Text = nominalDiameter.ToString();
            m_myForm.textBox_elbow_bihou.Text = wallThickness.ToString();
            m_myForm.textBox_elbow_wanqu_jiaodu.Text = Math.Round(angle, 2).ToString();
            m_myForm.xiaoshuBox_elbow_wanqu_banjing.Text = radius.ToString();
            m_myForm.comboBox_caizhi.Text = material;

            bmec_object1.Instance["LENGTH"].DoubleValue = pipeLength1;
            bmec_object2.Instance["LENGTH"].DoubleValue = pipeLength2;

            BIM.Point3d start_point1;
            BIM.Point3d start_point2;
            start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
            start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));

            BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);

            BG.DVector3d oldVec1 = new BG.DVector3d(line1Point[0], line1Point[1]);
            BG.DVector3d oldVec2 = new BG.DVector3d(line2Point[0], line2Point[1]);
            BG.DVector3d currentVec1 = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
            BG.DVector3d currentVec2 = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2));

            bool isShowWarningMessage = false;
            if (oldVec1.DotProduct(currentVec1) < 0 || oldVec2.DotProduct(currentVec2) < 0)
            {
                errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                //elbows = null;
                return false;
            }
            else
            {
                JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, start_point1);
                JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, start_point2, faster_point2);
            }

            //根据角度生成弯头
            if (isSingleElbow || iscxh)//不用增加弯头补齐角度则直接生成一个弯头
            {
                if (isShowWarningMessage)
                {
                    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                    //elbows = null;
                    return false;
                }
                elbowOrBendECObject = new BMECObject(elbowOrBendInstance);
                if (elbowOrBendECObject == null)
                {
                    errorMessage = "无法通过该实例创建对象！";
                    //elbows = null;
                    return false;
                }
                //TODO
                try
                {
                    elbowOrBendECObject.Create();
                }
                catch (System.Exception)
                {
                    errorMessage = "Pipeline不存在，请打开Create Pipeline创建处理！";
                    //elbows = null;
                    return false;
                }
                JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, v1, dir2, v2, start_point1, start_point2);
                elbows.Add(elbowOrBendECObject);
            }
            else
            {
                //仅使用一个弯头连接两个管道，尽管两管道夹角大于弯头角度
                if (m_myForm.checkBox_isPlaceTrimmedElbow.Checked && m_myForm.isOnlyOneElbowToCut)
                {
                    //一头切和两头切
                    if (m_myForm.radioButton1.Checked)//一头切
                    {
                        elbowOrBendECObject = new BMECObject(elbowOrBendInstance);
                        if (elbowOrBendECObject == null)
                        {
                            errorMessage = "无法通过该实例创建对象！";
                            //elbows = null;
                            return false;
                        }
                        double cankaoAngle = Convert.ToDouble(elbowangledic[m_myForm.comboBox_elbow_angle.Text]);
                        elbowOrBendECObject.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        BG.DPoint3d guandaoJiaodian = intersect;//两管道交点

                        BG.DVector3d oldguandao1Vec = new BG.DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        BG.DVector3d oldguandao2Vec = new BG.DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        BG.DVector3d guandao1Vec;//管道一方向向量
                        BG.DPoint3d fasterPoint1CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1);
                        guandao1Vec = new BG.DVector3d(fasterPoint1CE, intersect);
                        BG.DVector3d guandao2Vec;//管道二方向向量
                        BG.DPoint3d fasterPoint2CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2);
                        guandao2Vec = new BG.DVector3d(intersect, fasterPoint2CE);

                        BG.DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        BG.DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        BG.DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);
                        BG.DVector3d xiangxinRadiusVec = faxiangliang.CrossProduct(guandao1VecNormal);
                        BG.DVector3d xiangxinRadiusVecNormal;
                        xiangxinRadiusVec.TryNormalize(out xiangxinRadiusVecNormal);
                        //向心半径向量
                        if (xiangxinRadiusVecNormal.DotProduct(guandao2VecNormal) < 0)
                        {
                            xiangxinRadiusVecNormal = -xiangxinRadiusVecNormal;
                        }
                        BG.DVector3d xiangxinRadiusVecL;//半径向量
                        double magnitude;
                        xiangxinRadiusVecNormal.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out xiangxinRadiusVecL, out magnitude);

                        //求管道外侧交点到管道外侧切点的分量
                        double elbowOutRadius = (radius + elbowOrBendECObject.Instance["OUTSIDE_DIAMETER"].DoubleValue / 2) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//弯头外半径
                        double lengthQiedianXian = elbowOutRadius * (1 - Math.Cos(cankaoAngle * Math.PI / 180.0));
                        BG.DVector3d lengthQiedianXianVec;
                        xiangxinRadiusVecNormal.TryScaleToLength(lengthQiedianXian, out lengthQiedianXianVec, out magnitude);

                        BG.Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);
                        double tempAngle = (pipeTopipeAngle.Degrees % 180) - 90.0;
                        double xianAngle = Math.Abs(pipeTopipeAngle.Degrees - 90.0);
                        double hengxiangLen = Math.Tan(xianAngle * Math.PI / 180.0) * lengthQiedianXian;
                        BG.DVector3d hengxiangVect;
                        guandao1VecNormal.TryScaleToLength(hengxiangLen, out hengxiangVect, out magnitude);

                        //求管道外侧交点
                        double pipe2Radius = bmec_object2.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        BG.DVector3d waiceShuXian;
                        xiangxinRadiusVecL.TryScaleToLength(pipe2Radius, out waiceShuXian, out magnitude);
                        BG.DVector3d waiceHengxian;
                        double waiceHengxianL = pipe2Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        BG.DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        BG.DPoint3d pipe2Qiedian;//管道外侧切点坐标
                        if (tempAngle < 0)
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec + hengxiangVect;
                        }
                        else
                        {
                            pipe2Qiedian = guandaoWaiceJiaodian + lengthQiedianXianVec - hengxiangVect;
                        }
                        //求管道2起点
                        BG.DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }
                        BG.DVector3d pipe2XiangxinVecL;
                        pipe2XiangxinVec.TryScaleToLength(pipe2Radius, out pipe2XiangxinVecL, out magnitude);

                        BG.DPoint3d pipe2StarPoint = pipe2Qiedian + pipe2XiangxinVecL;//管道2起点

                        double chuiXianToRadius = elbowOutRadius * Math.Sin(cankaoAngle * Math.PI / 180.0);
                        BG.DVector3d chuiXianToRadiusV;
                        (-guandao1VecNormal).TryScaleToLength(chuiXianToRadius, out chuiXianToRadiusV, out magnitude);
                        double xianOnRadius = elbowOutRadius * Math.Cos(cankaoAngle * Math.PI / 180.0);
                        BG.DVector3d xianOnRadiusV;
                        xiangxinRadiusVecNormal.TryScaleToLength(xianOnRadius, out xianOnRadiusV, out magnitude);

                        BG.DPoint3d elbowCenter = pipe2Qiedian + chuiXianToRadiusV + xianOnRadiusV;//弯头圆心位置
                        BG.DPoint3d pipe1EndPoint = elbowCenter - xiangxinRadiusVecL;//管道1终点

                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1EndPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1EndPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1EndPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1EndPoint).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StarPoint.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StarPoint.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StarPoint.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StarPoint, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                            //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return false;
                        }
                        else
                        {
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        }

                        try
                        {
                            elbowOrBendECObject.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return false;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = guandao1VecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = guandao1VecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = guandao1VecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = xiangxinRadiusVecNormal.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = xiangxinRadiusVecNormal.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = xiangxinRadiusVecNormal.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;

                        BG.DVector3d tempVect;
                        BG.DPoint3d CEspoint2 = GetPointOnArcWithAngle(elbowDegree, elbowCenter, pipe1EndPoint, guandao1VecNormal, out tempVect);
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = CEspoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = CEspoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = CEspoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, tv1, dir2, tv2, currentStartPoint1, spoint2);

                        elbows.Add(elbowOrBendECObject);
                    }
                    else if (m_myForm.radioButton2.Checked)//两头切
                    {
                        elbowOrBendECObject = new BMECObject(elbowOrBendInstance);
                        if (elbowOrBendECObject == null)
                        {
                            errorMessage = "无法通过该实例创建对象！";
                            elbows = null;
                            return false;
                        }
                        double cankaoAngle = Convert.ToDouble(elbowangledic[m_myForm.comboBox_elbow_angle.Text]);
                        elbowOrBendECObject.Instance["ANGLE"].DoubleValue = elbowDegree;
                        double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                        centerToMainPort = cankaoCenterToMainPort;
                        radius = centerToMainPort / cankaoTan;
                        double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                        double curcmp = curtan * radius;

                        centerToMainPort = curcmp;
                        centerToRunPort = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                        elbowOrBendECObject.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                        BG.DVector3d oldguandao1Vec = new BG.DVector3d(line1Point[0], line1Point[1]);//管道一方向向量
                        BG.DVector3d oldguandao2Vec = new BG.DVector3d(line2Point[0], line2Point[1]);//管道二方向向量
                        BG.DVector3d guandao1Vec;//管道一方向向量
                        BG.DPoint3d fasterPoint1CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1);
                        guandao1Vec = new BG.DVector3d(fasterPoint1CE, intersect);
                        BG.DVector3d guandao2Vec;//管道二方向向量
                        BG.DPoint3d fasterPoint2CE = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2);
                        guandao2Vec = new BG.DVector3d(intersect, fasterPoint2CE);

                        BG.DVector3d guandao1VecNormal;
                        guandao1Vec.TryNormalize(out guandao1VecNormal);
                        BG.DVector3d guandao2VecNormal;
                        guandao2Vec.TryNormalize(out guandao2VecNormal);
                        BG.DVector3d faxiangliang = guandao1VecNormal.CrossProduct(guandao2VecNormal);

                        double magnitude;
                        //求管道外侧交点
                        BG.Angle pipeTopipeAngle = guandao1VecNormal.AngleTo(guandao2VecNormal);//两管道夹角
                        double pipe1Radius = bmec_object1.Instance["OUTSIDE_DIAMETER"].DoubleValue * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2;//管道半径
                        double waiceAngle = (180.0 - (pipeTopipeAngle.Degrees % 180)) / 2;
                        BG.DVector3d pipe1XiangxinVec = faxiangliang.CrossProduct(guandao1VecNormal);//管道1外侧切点向心方向向量
                        if (pipe1XiangxinVec.DotProduct(guandao2VecNormal) < 0)
                        {
                            pipe1XiangxinVec = -pipe1XiangxinVec;
                        }

                        BG.DVector3d waiceShuXian;//管道1向心向量
                        pipe1XiangxinVec.TryScaleToLength(pipe1Radius, out waiceShuXian, out magnitude);
                        BG.DVector3d waiceHengxian;//管道方向
                        double waiceHengxianL = pipe1Radius / Math.Tan(waiceAngle * Math.PI / 180.0);
                        guandao1VecNormal.TryScaleToLength(waiceHengxianL, out waiceHengxian, out magnitude);
                        BG.DPoint3d guandaoWaiceJiaodian = intersect + waiceHengxian - waiceShuXian;//管道外侧交点

                        BG.DVector3d pipe2XiangxinVec = faxiangliang.CrossProduct(guandao2VecNormal);//管道2外侧切点向心方向向量
                        if (pipe2XiangxinVec.DotProduct(guandao1VecNormal) > 0)
                        {
                            pipe2XiangxinVec = -pipe2XiangxinVec;
                        }

                        BG.DVector3d pipe2XiangxinVecL;//管道2向心向量
                        pipe2XiangxinVec.TryScaleToLength(pipe1Radius, out pipe2XiangxinVecL, out magnitude);

                        //弯头弦长
                        double elbowOutSideDia = elbowOrBendECObject.Instance["OUTSIDE_DIAMETER"].DoubleValue;//弯头管径
                        double elbowOutSideRadius = (elbowOutSideDia / 2 + radius) * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        double elbowXianchang = elbowOutSideRadius * Math.Sin(cankaoAngle / 2 * Math.PI / 180.0);//弯头角度对应的弦长的一半

                        //两管道外侧切点
                        double pipe1QieDianL = elbowXianchang / Math.Sin(waiceAngle * Math.PI / 180.0);//切点据外侧交点距离
                        BG.DVector3d pipe1QieDianVec;
                        guandao1VecNormal.TryScaleToLength(pipe1QieDianL, out pipe1QieDianVec, out magnitude);
                        BG.DPoint3d pipe1Qiedian = guandaoWaiceJiaodian - pipe1QieDianVec;//管道1的切点

                        BG.DVector3d pipe2QieDianVec;
                        guandao2VecNormal.TryScaleToLength(pipe1QieDianL, out pipe2QieDianVec, out magnitude);
                        BG.DPoint3d pipe2Qiedian = guandaoWaiceJiaodian + pipe2QieDianVec;//管道2的切点

                        //两管道靠近弯头的端点
                        BG.DPoint3d pipe1StartPointCE = pipe1Qiedian + waiceShuXian;//管道1端点
                        BG.DPoint3d pipe2StartPointCE = pipe2Qiedian + pipe2XiangxinVecL;//管道2端点

                        //弯头圆心位置
                        BG.DVector3d pingfenxianVec = waiceShuXian - waiceHengxian;//两管道角平分线向量
                        double xianToCenterLen = elbowOutSideRadius * Math.Cos(cankaoAngle / 2 * Math.PI / 180.0);//圆心到弦上距离
                        double xianToWaiJiaodianLen = pipe1QieDianL * Math.Cos(waiceAngle * Math.PI / 180.0);//弦外到外侧交点距离
                        BG.DVector3d outsideIntersectVecToCenter;//外侧交点到弯头圆心向量；
                        pingfenxianVec.TryScaleToLength(xianToCenterLen + xianToWaiJiaodianLen, out outsideIntersectVecToCenter, out magnitude);
                        BG.DPoint3d centerPoint = guandaoWaiceJiaodian + outsideIntersectVecToCenter;//弯头圆心

                        //弯头两端点及其方向向量
                        BG.DPoint3d elbowPoint1, elbowPoint2;
                        BG.DVector3d point1Vec, point2Vec;
                        BG.DVector3d point1TempVec, point2TmepVec;
                        point1TempVec = new BG.DVector3d(centerPoint, pipe1Qiedian);
                        point2TmepVec = new BG.DVector3d(centerPoint, pipe2Qiedian);
                        point1TempVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point1TempVec, out magnitude);
                        point2TmepVec.TryScaleToLength(elbowOutSideRadius - elbowOutSideDia * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster / 2, out point2TmepVec, out magnitude);

                        elbowPoint1 = centerPoint + point1TempVec;
                        elbowPoint2 = centerPoint + point2TmepVec;

                        point1Vec = faxiangliang.CrossProduct(point1TempVec);
                        if (point1Vec.DotProduct(guandao1VecNormal) < 0)
                        {
                            point1Vec = -point1Vec;
                        }
                        point2Vec = faxiangliang.CrossProduct(point2TmepVec);
                        if (point2Vec.DotProduct(guandao2VecNormal) < 0)
                        {
                            point2Vec = -point2Vec;
                        }

                        //改变管道长度并移动管道
                        BIM.Point3d currentStartPoint1 = new BIM.Point3d();
                        currentStartPoint1.X = pipe1StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Y = pipe1StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint1.Z = pipe1StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object1.Instance["LENGTH"].DoubleValue = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), pipe1StartPointCE).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao1Vec.DotProduct(guandao1Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, currentStartPoint1, faster_point1);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                        //}
                        BIM.Point3d currentStartPoint2 = new BIM.Point3d();
                        currentStartPoint2.X = pipe2StartPointCE.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Y = pipe2StartPointCE.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        currentStartPoint2.Z = pipe2StartPointCE.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        bmec_object2.Instance["LENGTH"].DoubleValue = new BG.DVector3d(pipe2StartPointCE, JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2)).Magnitude / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        //if (oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        //{
                        //    //JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, faster_point2, currentStartPoint2);
                        //    //MessageCenter.Instance.ShowMessage(MessageType.Warning, "流向异常！", "当前所选两根连接管道流向不为顺流，请检查连接的管道流向。", MessageAlert.None);
                        //    errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                        //    elbows = null;
                        //    return false;
                        //}
                        //else
                        //{
                        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        //}

                        if (oldguandao1Vec.DotProduct(guandao1Vec) < 0 || oldguandao2Vec.DotProduct(guandao2Vec) < 0)
                        {
                            errorMessage = "流向异常！请确认起点管道到终点管道为顺流流向。";
                            //elbows = null;
                            return false;
                        }
                        else
                        {
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, currentStartPoint1);
                            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, currentStartPoint2, faster_point2);
                        }

                        //创建弯头并移动
                        try
                        {
                            elbowOrBendECObject.Create();
                        }
                        catch (Exception)
                        {
                            errorMessage = "创建弯头时出现异常！";
                            //elbows = null;
                            return false;
                        }

                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = point1Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = point1Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = point1Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = point2Vec.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = point2Vec.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = point2Vec.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = elbowPoint1.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = elbowPoint1.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = elbowPoint1.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = elbowPoint2.X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = elbowPoint2.Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = elbowPoint2.Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, tv1, dir2, tv2, spoint1, spoint2);

                        elbows.Add(elbowOrBendECObject);
                    }
                }
                else
                {
                    elbowOrBendECObject = new BMECObject(elbowOrBendInstance);
                    if (elbowOrBendECObject == null)
                    {
                        errorMessage = "无法通过该实例创建对象！";
                        //elbows = null;
                        return false;
                    }
                    BG.DVector3d pipe1Vect = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    BG.DVector3d pipe2Vect = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point2));
                    BG.DVector3d pipe1VN;
                    pipe1Vect.TryNormalize(out pipe1VN);
                    BG.DVector3d pipe1PointToCircleCenterPointVect = pipe1VN.CrossProduct(pipe2Vect).CrossProduct(pipe1VN);
                    BG.DPoint3d radiusPoint;
                    if (pipe1PointToCircleCenterPointVect.DotProduct(pipe2Vect) < 0)
                    {
                        pipe1PointToCircleCenterPointVect = -pipe1PointToCircleCenterPointVect;
                    }
                    double magnitude;
                    BG.DVector3d radiusVector;
                    pipe1PointToCircleCenterPointVect.TryScaleToLength(radius * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster, out radiusVector, out magnitude);
                    radiusPoint = JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1) + radiusVector;

                    List<BG.DPoint3d> points = new List<BG.DPoint3d>();//
                    List<BG.DVector3d> tangleVec = new List<BG.DVector3d>();
                    points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                    tangleVec.Add(pipe1Vect);
                    for (int i = 0; i < unbrokenElbow; i++)
                    {
                        BG.DVector3d tangleVecAtResultPoint = new BG.DVector3d();
                        BG.DPoint3d tanglePoint = GetPointOnArcWithAngle(elbowDegree, radiusPoint, points.Last(), tangleVec.Last(), out tangleVecAtResultPoint);
                        points.Add(tanglePoint);
                        tangleVec.Add(tangleVecAtResultPoint);
                    }
                    if (brokenElbowDgree > 1e-1)
                    {
                        points.Add(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point2));
                        tangleVec.Add(pipe2Vect);
                    }

                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        if (i < unbrokenElbow)
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowOrBendECObject);
                            elbow.Instance["ANGLE"].DoubleValue = elbowDegree;

                            double cankaoAngle = Convert.ToDouble(elbowangledic[m_myForm.comboBox_elbow_angle.Text]);
                            double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(BG.Angle.DegreesToRadians(elbowDegree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                        else
                        {
                            BMECObject elbow = new BMECObject();
                            elbow.Copy(elbowOrBendECObject);
                            elbow.Instance["ANGLE"].DoubleValue = brokenElbowDgree;

                            double cankaoAngle = Convert.ToDouble(elbowangledic[m_myForm.comboBox_elbow_angle.Text]);
                            double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
                            centerToMainPort = cankaoCenterToMainPort;
                            radius = centerToMainPort / cankaoTan;
                            double curtan = Math.Tan(BG.Angle.DegreesToRadians(brokenElbowDgree / 2.0));
                            double curcmp = curtan * radius;

                            centerToMainPort = curcmp;
                            centerToRunPort = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
                            elbow.Instance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

                            elbow.Create();
                            elbows.Add(elbow);
                        }
                    }

                    for (int i = 0; i < elbows.Count; i++)
                    {
                        BIM.Point3d tv1 = new BIM.Point3d();
                        tv1.X = tangleVec[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Y = tangleVec[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv1.Z = tangleVec[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d tv2 = new BIM.Point3d();
                        tv2.X = tangleVec[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Y = tangleVec[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        tv2.Z = tangleVec[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint1 = new BIM.Point3d();
                        spoint1.X = points[i].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Y = points[i].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint1.Z = points[i].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        BIM.Point3d spoint2 = new BIM.Point3d();
                        spoint2.X = points[i + 1].X / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Y = points[i + 1].Y / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        spoint2.Z = points[i + 1].Z / Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                        JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbows[i], dir1, tv1, dir2, tv2, spoint1, spoint2);
                    }
                }
            }

            return true;
        }

        //public static bool CalculatePointFromPipe(BMECObject pipe1, BMECObject pipe2, out DPoint3d intersection, ) { }

        //public static bool CreateElbowOrBendFromPipe(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage, int chuangjianleixing, IECInstance elbowOrBendInstance,)
        //{
        //    errorMessage = string.Empty;
        //    elbowOrBendInstance = null;

        //    BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];//第一根管道的端点
        //    BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];//第二根管道的端点
        //    line1_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(0).Location);
        //    line1_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object1.GetNthPort(1).Location);
        //    line2_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(0).Location);
        //    line2_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(bmec_object2.GetNthPort(1).Location);

        //    BIM.Ray3d ray3d1 = app.Ray3dFromPoint3dStartEnd(line1_end_pts[0], line1_end_pts[1]);//TODO 根据两个管道的端点构造两条射线
        //    BIM.Ray3d ray3d2 = app.Ray3dFromPoint3dStartEnd(line2_end_pts[0], line2_end_pts[1]);

        //    BIM.Point3d intersect_point1, intersect_point2;
        //    intersect_point1 = intersect_point2 = app.Point3dZero();
        //    double fraction1, fraction2;
        //    fraction1 = fraction2 = 0.0;
        //    bool reuslt = app.Ray3dRay3dClosestApproach(ray3d1, ray3d2, ref intersect_point1, ref fraction1, ref intersect_point2, ref fraction2);//两条射线是否有交点

        //    if (!reuslt)
        //    {
        //        errorMessage = "选中的管道不在一个平面上";
        //        return false;
        //    }
        //    BIM.Point3d intersect_point = intersect_point1;
        //    BIM.LineElement line = app.CreateLineElement2(null, app.Point3dZero(), intersect_point);

        //    //m_myForm.textBox_guandao_guid2.Text = bmec_object2.Instance["GUID"].StringValue;
        //    BIM.Point3d nearest_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
        //    BIM.Point3d faster_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
        //    if (app.Point3dEqual(nearest_point1, faster_point1))
        //    {
        //        nearest_point1 = line1_end_pts[0];
        //        faster_point1 = line1_end_pts[1];
        //    }

        //    BIM.Point3d nearest_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
        //    BIM.Point3d faster_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
        //    if (app.Point3dEqual(nearest_point2, faster_point2))
        //    {
        //        nearest_point2 = line2_end_pts[0];
        //        faster_point2 = line2_end_pts[1];
        //    }
        //    BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);//管道一方向向量
        //    BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);//管道二方向向量
        //    double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
        //    if (JYX_ZYJC_Jianmo_Youhua_CS.MyPublic_Api.is_double_xiangdeng(angle, 180))
        //    {
        //        errorMessage = "两个管道互相平行!";
        //        return false;
        //    }

        //    double centerToMainPort = 0.0;
        //    double centerToRunPort = 0.0;
        //    double lengthToBend = 0.0;
        //    double lengthAfterBend = 0.0;
        //    double pipeLength1 = 0.0;
        //    double pipeLength2 = 0.0;
        //    double d1, d2;
        //    d1 = app.Point3dDistance(intersect_point, faster_point1);
        //    d2 = app.Point3dDistance(intersect_point, faster_point2);
        //    double radius = 0.0;

        //    if (chuangjianleixing == 1)//elbow
        //    {
        //        //elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
        //        if (elbowOrBendInstance == null)
        //        {
        //            errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
        //            return false;
        //        }
        //        ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
        //        isp.FillCurrentPreferences(elbowOrBendInstance, null);
        //        //elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
        //        ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
        //        if (eCInstanceList.Count == 0)
        //        {
        //            errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
        //            return false;
        //        }
        //        elbowOrBendInstance = eCInstanceList[0];

        //        centerToMainPort = elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
        //        centerToRunPort = centerToMainPort;

        //        BMECObject tempObject = new BMECObject(elbowOrBendInstance);

        //        if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
        //        {
        //            lengthToBend = tempObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
        //            lengthAfterBend = tempObject.Ports[1].Instance["SOCKET_DEPTH"].DoubleValue;
        //        }
        //        else if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
        //        {
        //            lengthToBend = tempObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
        //            lengthAfterBend = tempObject.Ports[1].Instance["THREADED_LENGTH"].DoubleValue;
        //        }
        //        //calculate radius CTR_END_M AND CTR_END_R
        //        double cankaoAngle = Convert.ToDouble(elbowOrBendInstance["ANGLE"]);
        //        double cankaoTan = Math.Tan(BG.Angle.DegreesToRadians(cankaoAngle / 2.0));
        //        radius = centerToMainPort / cankaoTan;
        //        double curtan = Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
        //        double curcmp = curtan * radius;

        //        //double tanA = Math.Tan(BG.Angle.DegreesToRadians(angle / 2.0));
        //        //centerToMainPort = tanA * radius;

        //        centerToMainPort = curcmp;
        //        centerToRunPort = centerToMainPort;

        //        pipeLength1 = d1 - centerToMainPort + lengthToBend;
        //        pipeLength2 = d2 - centerToRunPort + lengthAfterBend;

        //        //create elbow
        //        elbowOrBendInstance["ANGLE"].DoubleValue = angle;
        //        elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
        //        elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
        //        elbowOrBendInstance["INSULATION"].StringValue = insulation;
        //        elbowOrBendInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
        //        elbowOrBendInstance["MATERIAL"].StringValue = material;
        //        elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
        //        elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

        //    }
        //    else//bend
        //    {
        //        double radiusFactor = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text);
        //        radius = Convert.ToDouble(m_myForm.textBox_radiusFactor.Text) * nominalDiameter;
        //        lengthToBend = Convert.ToDouble(m_myForm.textBox_lengthToBend.Text);
        //        lengthAfterBend = Convert.ToDouble(m_myForm.textBox_lengthAfterBend.Text);

        //        m_myForm.textBox_radius.Text = radius.ToString();

        //        if (chuangjianleixing == 2)//bend
        //        {
        //            elbowOrBendECClassName = "PIPE_BEND";
        //        }
        //        else if (chuangjianleixing == 3)//xiamiwan
        //        {
        //            elbowOrBendECClassName = "PIPE_ELBOW_TRIMMED_JYX"; //MITERED_PIPE_BEND_JYX PIPE_ELBOW_TRIMMED_JYX
        //        }
        //        elbowOrBendInstance = BMECInstanceManager.Instance.CreateECInstance(elbowOrBendECClassName, true);
        //        if (elbowOrBendInstance == null)
        //        {
        //            errorMessage = "没有找到该ECClass类型，请确认已配置该类型";
        //            return false;
        //        }
        //        ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
        //        isp.FillCurrentPreferences(elbowOrBendInstance, null);
        //        elbowOrBendInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
        //        ECInstanceList eCInstanceList = isp.SelectSpec(elbowOrBendInstance, true);
        //        if (eCInstanceList.Count == 0)
        //        {
        //            errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据";
        //            return false;
        //        }
        //        elbowOrBendInstance = eCInstanceList[0];

        //        elbowOrBendInstance["ANGLE"].DoubleValue = angle;
        //        elbowOrBendInstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
        //        elbowOrBendInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
        //        elbowOrBendInstance["INSULATION"].StringValue = insulation;
        //        elbowOrBendInstance["BEND_POINT_RADIUS"].DoubleValue = radiusFactor;
        //        elbowOrBendInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
        //        elbowOrBendInstance["MATERIAL"].StringValue = material;
        //        if (chuangjianleixing == 3)
        //        {
        //            //TODO 弯曲半径与弯曲角度
        //            int numPieces = Convert.ToInt32(m_myForm.textBox_xiamiwan_jieshu.Text);
        //            elbowOrBendInstance["NUM_PIECES"].IntValue = numPieces;

        //            //TODO 随便写的
        //            elbowOrBendInstance["jieshu"].IntValue = numPieces;
        //            //elbowOrBendInstance["wanqu_banjing"].DoubleValue = radius;
        //            elbowOrBendInstance["wanqu_jiaodu"].DoubleValue = angle;

        //            //int temp = 1;
        //            //int jieshu = elbowOrBendInstance["jieshu"].IntValue;
        //            //double wanqubanjing = elbowOrBendInstance["wanqu_banjing"].DoubleValue;
        //            //double wanqujiaodu = elbowOrBendInstance["wanqu_jiaodu"].DoubleValue;

        //            double tanA = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
        //            centerToMainPort = radius / tanA;
        //            centerToRunPort = centerToMainPort;

        //            //centerToMainPort = radius;
        //            //centerToRunPort = centerToMainPort;
        //            //elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = centerToMainPort;
        //            //elbowOrBendInstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = centerToRunPort;

        //            pipeLength1 = d1 - centerToMainPort;
        //            pipeLength2 = d2 - centerToRunPort;
        //            elbowOrBendInstance["caizhi"].StringValue = "";
        //            elbowOrBendInstance["bihou"].DoubleValue = wallThickness;
        //            //elbowOrBendInstance["midu"].StringValue = "";
        //        }
        //        else
        //        {
        //            elbowOrBendInstance["LENGTH_TO_BEND"].DoubleValue = lengthToBend;
        //            elbowOrBendInstance["LENGTH_AFTER_BEND"].DoubleValue = lengthAfterBend;
        //            double tanA = Math.Tan(BG.Angle.DegreesToRadians((180.0 - angle) / 2.0));
        //            centerToMainPort = radius / tanA;
        //            centerToRunPort = centerToMainPort;

        //            pipeLength1 = d1 - centerToMainPort - lengthToBend;
        //            pipeLength2 = d2 - centerToRunPort - lengthAfterBend;
        //        }
        //    }

        //    BMECObject elbowOrBendECObject = new BMECObject(elbowOrBendInstance);

        //    //当管道过短时，无法生成
        //    if (pipeLength1 <= 0 || pipeLength2 <= 0)
        //    {
        //        errorMessage = "所连接的管道过短，无法生成弯头";
        //        return false;
        //    }
        //    //TODO 随便写的
        //    //double temp1 = elbowOrBendECObject.GetDoubleValueInMM("OD");
        //    //double temp2 = elbowOrBendECObject.GetDoubleValueInMM("wanqu_banjing");
        //    //double temp3 = elbowOrBendECObject.GetDoubleValueInMM("wanqu_jiaodu");
        //    //double temp4 = elbowOrBendECObject.GetDoubleValueInMM("NOMINAL_DIAMETER");
        //    //double temp5 = elbowOrBendECObject.GetDoubleValueInMM("OUTSIDE_DIAMETER");

        //    //fill form
        //    m_myForm.textBox_elbow_dn.Text = nominalDiameter.ToString();
        //    m_myForm.textBox_elbow_bihou.Text = wallThickness.ToString();
        //    m_myForm.textBox_elbow_wanqu_jiaodu.Text = Math.Round(angle, 2).ToString();
        //    m_myForm.xiaoshuBox_elbow_wanqu_banjing.Text = radius.ToString();
        //    m_myForm.comboBox_caizhi.Text = material;

        //    bmec_object1.Instance["LENGTH"].DoubleValue = pipeLength1;
        //    bmec_object2.Instance["LENGTH"].DoubleValue = pipeLength2;

        //    BIM.Point3d start_point1;
        //    BIM.Point3d start_point2;
        //    start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
        //    start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));

        //    if (elbowOrBendECObject == null)
        //    {
        //        errorMessage = "无法通过该实例创建对象";
        //        return false;
        //    }
        //    BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
        //    BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);

        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, start_point1);
        //    JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, start_point2, faster_point2);
        //    try
        //    {
        //        elbowOrBendECObject.Create();
        //    }
        //    catch (System.Exception)
        //    {
        //        errorMessage = "Pipeline不存在，请打开Create Pipeline创建处理。";
        //        return false;
        //    }
        //    //ec_object.LegacyGraphicsId
        //    JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(elbowOrBendECObject, dir1, v1, dir2, v2, start_point1, start_point2);

        //    bmec_object1.DiscoverConnectionsEx();
        //    bmec_object1.UpdateConnections();

        //    bmec_object2.DiscoverConnectionsEx();
        //    bmec_object2.UpdateConnections();

        //    elbowOrBendECObject.UpdateConnections();
        //    elbowOrBendECObject.DiscoverConnectionsEx();

        //    return true;
        //}
        public bool create_tee(BMECObject bmec_object1, BMECObject bmec_object2, out string errorMessage, out List<BMECObject> elbows)
        {
            elbows = new List<BMECObject>();
            IECInstance elbowOrBendInstance = null;
            double dn1 = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double dn2 = bmec_object2.Instance["NOMINAL_DIAMETER"].DoubleValue;
            bool isYijingWantou;//是否为异径弯头
            isYijingWantou = dn1 == dn2 ? false : true;
            //Bentley.GeometryNET.DPoint3d[] line1Point = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object1);
            //Bentley.GeometryNET.DPoint3d[] line2Point = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(bmec_object2);
            Bentley.GeometryNET.DPoint3d[] line1Point = GetTowPortPoint(bmec_object1);
            Bentley.GeometryNET.DPoint3d[] line2Point = GetTowPortPoint(bmec_object2);
            errorMessage = string.Empty;
            BG.DSegment3d lineA = new BG.DSegment3d(line1Point[0], line1Point[1]);
            BG.DSegment3d lineB = new BG.DSegment3d(line2Point[0], line2Point[1]);
            //处理同一条直线上的管道
            BG.DPoint3d linePointVector = lineA.StartPoint == lineB.StartPoint ? lineA.EndPoint : lineA.StartPoint;
            BG.DVector3d lineAVector = new BG.DVector3d(linePointVector, lineB.StartPoint);
            BG.DVector3d lineBVector = new BG.DVector3d(linePointVector, lineB.EndPoint);
            if (lineAVector.IsParallelOrOppositeTo(lineBVector))
            {
                //TODO 自动给他连了还是提示连不了？
                errorMessage = "管道在同一直线上";
                elbows = null;
                return true;
            }
            BG.DSegment3d segment;
            double fractionA, fractionB;
            CalculateMaleVerticalLine(lineA, lineB, out segment, out fractionA, out fractionB);
            BG.DPoint3d intersect = segment.StartPoint;
            BIM.Point3d intersect_point = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(intersect);
            BIM.LineElement line = app.CreateLineElement2(null, app.Point3dZero(), intersect_point);
            BIM.Point3d[] line1_end_pts = new BIM.Point3d[2];//第一根管道的端点
            BIM.Point3d[] line2_end_pts = new BIM.Point3d[2];//第二根管道的端点
            line1_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line1Point[0]);
            line1_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line1Point[1]);
            line2_end_pts[0] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line2Point[0]);
            line2_end_pts[1] = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.DPoint3d_To_Point3d(line2Point[1]);
            BIM.Point3d nearest_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            BIM.Point3d faster_point1 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line1_end_pts[0], line1_end_pts[1]);
            if (app.Point3dEqual(nearest_point1, faster_point1))
            {
                nearest_point1 = line1_end_pts[0];
                faster_point1 = line1_end_pts[1];
            }
            BIM.Point3d nearest_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_nearest_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            BIM.Point3d faster_point2 = JYX_ZYJC_Jianmo_Youhua_CS.Mstn_Public_Api.get_faster_point(intersect_point, line2_end_pts[0], line2_end_pts[1]);
            if (app.Point3dEqual(nearest_point2, faster_point2))
            {
                nearest_point2 = line2_end_pts[0];
                faster_point2 = line2_end_pts[1];
            }
            BIM.Point3d v1 = app.Point3dSubtract(nearest_point1, faster_point1);//管道一方向向量
            BIM.Point3d v2 = app.Point3dSubtract(faster_point2, nearest_point2);//管道二方向向量
            double angle = BG.Angle.RadiansToDegrees(app.Point3dAngleBetweenVectors(v1, v2));
            //TODO 容差值
            double tolerence = 1;
            if (Math.Abs(angle - 90.0) > tolerence)
            {
                errorMessage = "该角度无法生成三通";
                elbows = null;
                return false;
            }

            nominalDiameter = bmec_object1.Instance["NOMINAL_DIAMETER"].DoubleValue;
            double insulationThickness = bmec_object1.Instance["INSULATION_THICKNESS"].DoubleValue;
            string insulation = bmec_object1.Instance["INSULATION"].StringValue;
            double wallThickness = bmec_object1.Instance["WALL_THICKNESS"].DoubleValue;
            string material = bmec_object1.Instance["MATERIAL"].StringValue;

            double centerToMainPort = 0.0;
            double centerToBranchPort = 0.0;
            double lengthToBend = 0.0;
            double lengthAfterBend = 0.0;
            double pipeLength1 = 0.0;
            double pipeLength2 = 0.0;
            double d1, d2;
            d1 = app.Point3dDistance(intersect_point, faster_point1);
            d2 = app.Point3dDistance(intersect_point, faster_point2);
            //异径生成三通？
            string pipeTeeName = "EQUAL_PIPE_TEE";
            if (isYijingWantou)
            {
                pipeTeeName = "REDUCING_PIPE_TEE";
            }
            else
            {
                pipeTeeName = "EQUAL_PIPE_TEE";
            }
            IECInstance pipeTeeInstance = BMECInstanceManager.Instance.CreateECInstance(pipeTeeName, true);
            if (pipeTeeInstance == null)
            {
                errorMessage = "没有找到该ECClass类型，请确认已配置该类型！";
                elbows = null;
                return false;
            }
            ISpecProcessor isp = api.SpecProcessor;
            isp.FillCurrentPreferences(elbowOrBendInstance, null);
            pipeTeeInstance["NOMINAL_DIAMETER"].DoubleValue = nominalDiameter;
            ECInstanceList pipeTeeECInstanceList;
            if (!isYijingWantou)
            {
                pipeTeeECInstanceList = isp.SelectSpec(pipeTeeInstance, true);//TODO 筛选条件
            }
            else
            {
                Hashtable whereClauseList = new Hashtable();
                whereClauseList.Add("NOMINAL_DIAMETER", dn1);
                whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", dn2);
                pipeTeeECInstanceList = isp.SelectSpec(pipeTeeInstance.ClassDefinition, whereClauseList, Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString(), true, "dialogTitle");
            }
            if (pipeTeeECInstanceList.Count == 0)
            {
                errorMessage = "没有找到该ECClass的对应数据项，请确认已配置数据！";
                elbows = null;
                return false;
            }
            pipeTeeInstance = pipeTeeECInstanceList[0];

            BMECObject tempObject = new BMECObject(pipeTeeInstance);
            if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE" || tempObject.Ports[2].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
            {
                if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["SOCKET_DEPTH"].DoubleValue;
                }
                if (tempObject.Ports[2].Instance["END_PREPARATION"].StringValue == "SOCKET_WELD_FEMALE")
                {
                    lengthAfterBend = tempObject.Ports[2].Instance["SOCKET_DEPTH"].DoubleValue;
                }
            }
            else if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE" || tempObject.Ports[2].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
            {
                if (tempObject.Ports[0].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
                {
                    lengthToBend = tempObject.Ports[0].Instance["THREADED_LENGTH"].DoubleValue;
                }
                if (tempObject.Ports[2].Instance["END_PREPARATION"].StringValue == "THREADED_FEMALE")
                {
                    lengthAfterBend = tempObject.Ports[2].Instance["THREADED_LENGTH"].DoubleValue;
                }
            }
            centerToMainPort = pipeTeeInstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue;
            centerToBranchPort = pipeTeeInstance["DESIGN_LENGTH_CENTER_TO_BRANCH_END"].DoubleValue;
            pipeLength1 = d1 - centerToMainPort + lengthToBend;
            pipeLength2 = d2 - centerToBranchPort + lengthAfterBend;
            pipeTeeInstance["INSULATION_THICKNESS"].DoubleValue = insulationThickness;
            pipeTeeInstance["INSULATION"].StringValue = insulation;
            pipeTeeInstance["WALL_THICKNESS"].DoubleValue = wallThickness;
            pipeTeeInstance["MATERIAL"].StringValue = material;
            BMECObject pipeTeeECObject = new BMECObject(pipeTeeInstance);
            //当管道过短时，无法生成
            if (pipeLength1 <= 0 || pipeLength2 <= 0)
            {
                errorMessage = "所连接的管道过短，无法生成弯头！";
                elbows = null;
                return false;
            }
            bmec_object1.Instance["LENGTH"].DoubleValue = pipeLength1;
            bmec_object2.Instance["LENGTH"].DoubleValue = pipeLength2;
            BIM.Point3d start_point1;
            BIM.Point3d start_point2;
            start_point1 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point1), (d1 - pipeLength1) / d1));
            start_point2 = app.Point3dSubtract(intersect_point, app.Point3dScale(app.Point3dSubtract(intersect_point, faster_point2), (d2 - pipeLength2) / d2));
            if (pipeTeeECObject == null)
            {
                errorMessage = "无法通过该实例创建对象！";
                elbows = null;
                return false;
            }
            BIM.Point3d dir1 = app.Point3dFromXYZ(1, 0, 0);
            BIM.Point3d dir2 = app.Point3dFromXYZ(0, 0, 1);
            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object1, faster_point1, start_point1);
            JYX_ZYJC_CLR.PublicMethod.transform_pipe(bmec_object2, start_point2, faster_point2);
            try
            {
                pipeTeeECObject.Create();
            }
            catch (System.Exception)
            {
                errorMessage = "Pipeline不存在，请打开Create Pipeline创建处理！";
                elbows = null;
                return false;
            }
            JYX_ZYJC_CLR.PublicMethod.transform_xiamiwan(pipeTeeECObject, dir1, v1, dir2, v2, start_point1, start_point2);

            double fraction_tee_outrange;
            BG.DPoint3d closePoint_tee_outrange;
            lineA.ClosestFractionAndPoint(intersect, true, out fraction_tee_outrange, out closePoint_tee_outrange);
            if (fraction_tee_outrange > 0 && fraction_tee_outrange < 1)
            {
                BG.DVector3d liuxiang = new BG.DVector3d(JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(faster_point1), JYX_ZYJC_CLR.PublicMethod.V8Point3d_to_CEDPoint3d(start_point1));
                BG.DVector3d yuanlailiuxiang = new BG.DVector3d(lineA.StartPoint, lineA.EndPoint);
                BG.DPoint3d tee_runPort_Point = pipeTeeECObject.GetNthPort(1).LocationInUors;//TODO
                CutOffPipe cop = new CutOffPipe();
                BMECObject duochuyijieguandao;
                if (liuxiang.IsParallelTo(yuanlailiuxiang))//没改变方向
                {
                    duochuyijieguandao = CutOffPipe.CreatePipe(tee_runPort_Point, lineA.EndPoint, bmec_object1);
                }
                else
                {
                    duochuyijieguandao = CutOffPipe.CreatePipe(lineA.StartPoint, tee_runPort_Point, bmec_object1);
                }
                try
                {
                    duochuyijieguandao.DiscoverConnectionsEx();
                    duochuyijieguandao.UpdateConnections();
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show(exc.ToString());
                }
            }
            elbows.Add(pipeTeeECObject);
            return true;
        }
        public unsafe void transform_tee(BMECObject @object, BG.DPoint3d pt_dir1, BG.DPoint3d pt_v1, BG.DPoint3d pt_dir2, BG.DPoint3d pt_v2, BG.DPoint3d pt_start_point1, BG.DPoint3d pt_start_point2)
        {
            BG.DVector3d dVec3d2 = new BG.DVector3d(pt_dir1);
            BG.DVector3d dVec3d4 = new BG.DVector3d(pt_v1);
            BG.DVector3d dVec3d6 = new BG.DVector3d(pt_dir2);
            BG.DVector3d dVec3d8 = new BG.DVector3d(pt_v2);
            BG.DVector3d dPoint3d = new BG.DVector3d(pt_start_point1);
            BG.DTransform3d transform;
            //@object.GetTransform(&transform);
            transform = @object.Transform3d;
            BG.DMatrix3d rotMatrix = transform.Matrix;
            BG.DPoint3d dPoint3d2 = transform.Translation;
            BG.DMatrix3d rotMatrix2 = new BG.DMatrix3d();
            JYX_ZYJC_CLR.PublicMethod.dMatrix3d_fromVectorToVector(dVec3d2, dVec3d4, ref rotMatrix2);
            rotMatrix2 *= rotMatrix;
            BG.DPoint3d dPoint3d3 = dVec3d6;
            dPoint3d3 = new BG.DVector3d(dPoint3d3) * rotMatrix2;
            dVec3d6 = new BG.DVector3d(dPoint3d3);
            double num = dVec3d4.DotProduct(dVec3d8) / dVec3d4.MagnitudeSquared;

            BG.DVector3d dVec3d9 = dVec3d4;
            //< Module >.mdlVec_scale((Bentley.DPoint3d*)(&dVec3d9), (Bentley.DPoint3d*)(&dVec3d4), num);
            dVec3d9.TryScaleToLengthInPlace(num);

            BG.DVector3d dVec3d10;
            dVec3d10 = dVec3d8 - dVec3d9;
            dVec3d4.NormalizeInPlace();
            //double num2 = < Module >.mdlVec_signedAngleBetweenVectors((Bentley.DPoint3d*)(&dVec3d10), (Bentley.DPoint3d*)(&dVec3d6), (Bentley.DPoint3d*)(&dVec3d4));
            double num2 = (dVec3d10.SignedAngleTo(dVec3d6, dVec3d4)).Degrees;

            BG.DMatrix3d rotMatrix3;
            //< Module >.mdlRMatrix_fromRotationAboutAxis(&rotMatrix3, (Bentley.DPoint3d*)(&dVec3d4), num2);
            rotMatrix3 = BG.DMatrix3d.Rotation(dVec3d4, BG.Angle.FromDegrees(num2));

            rotMatrix3 *= rotMatrix2;
            transform.Matrix = rotMatrix3;
            transform.Translation = dPoint3d;
            @object.Transform3d = transform;
            @object.Create();
        }

        private static Dictionary<string, string> elbowradiusdic = new Dictionary<string, string>();
        private static Dictionary<string, string> elbowangledic = new Dictionary<string, string>();
        private static Dictionary<string, string> elbowmitereddic = new Dictionary<string, string>();
        public static string getElbowECClassName(string radius, string angle)
        {
            string elbowecclassname = "";
            //特殊的两个
            //if (angle.Contains("30度弯头") && radius.Contains("长半径")) return "PIPE_ELBOW_30_DEGREE_LONG_RADIUS";
            //if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";

            //if (radius.Contains("长半径"))
            //{
            //    elbowecclassname = elbowradiusdic[radius] + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + "PIPE_ELBOW";
            //}
            //else
            //{
            //    elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];
            //}
            if (angle.Contains("15度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_15_DEGREE";
            if (angle.Contains("30度弯头") && radius.Contains("短半径")) return "PIPE_ELBOW_SHORT_RADIUS_30_DEGREE";
            if (angle.Contains("45度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_45_DEGREE_PIPE_ELBOW";
            if (angle.Contains("60度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_60_DEGREE_PIPE_ELBOW";
            if (angle.Contains("90度弯头") && radius.Contains("长半径")) return "LONG_RADIUS_90_DEGREE_PIPE_ELBOW";

            if (radius.Contains("普通弯头")) return "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE";

            elbowecclassname = "PIPE_ELBOW" + "_" + elbowangledic[angle] + "_" + "DEGREE" + "_" + elbowradiusdic[radius];

            return elbowecclassname;
        }
        //TODO
        //改了原本的ECClass类中的命名
        //PIPE_ELBOW_30_DEGREE_LONG_RADIUS  ---
        //PIPE_ELBOW_SHORT_RADIUS_30_DEGREE ---
        public static void initelbowDic(Dictionary<string, string> radiusdic, Dictionary<string, string> angledic)
        {
            radiusdic.Clear();
            radiusdic.Add("普通弯头", "");
            radiusdic.Add("长半径弯头", "LONG_RADIUS");
            radiusdic.Add("短半径弯头", "SHORT_RADIUS");
            radiusdic.Add("内外丝弯头", "INNER_AND_OUTER_WIRE_ELBOWS");
            radiusdic.Add("1.5倍弯曲半径弯头", "1_POINT_5R");
            radiusdic.Add("2倍弯曲半径弯头", "2R");
            radiusdic.Add("2.5倍弯曲半径弯头", "2_POINT_5R");
            radiusdic.Add("3倍弯曲半径弯头", "3R");
            radiusdic.Add("4倍弯曲半径弯头", "4R");
            angledic.Clear();
            angledic.Add("15度弯头", "15");
            angledic.Add("30度弯头", "30");
            angledic.Add("45度弯头", "45");
            angledic.Add("60度弯头", "60");
            angledic.Add("90度弯头", "90");
        }
        //public static BMECObject createElbow(string elbowECClassName, double nominal_diameter, double angle, double center_to_outlet_end, double center_to_run_end, double insulation_thickness, string insulation, out string errorMessage)
        //{
        //    errorMessage = string.Empty;
        //    IECInstance iECInstance = BMECInstanceManager.Instance.CreateECInstance(elbowECClassName, true);
        //    ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
        //    isp.FillCurrentPreferences(iECInstance, null);
        //    iECInstance["NOMINAL_DIAMETER"].DoubleValue = nominal_diameter;
        //    ECInstanceList eCInstanceList = isp.SelectSpec(iECInstance, true);
        //    BMECObject result = null;
        //    if (eCInstanceList.Count == 0)
        //    {
        //        errorMessage = "当前Specification为配置弯管。";
        //        return null;
        //    }
        //    if (eCInstanceList != null && eCInstanceList.Count > 0)
        //    {
        //        IECInstance ecinstance = eCInstanceList[0];
        //        ecinstance["ANGLE"].DoubleValue = angle;
        //        ecinstance["DESIGN_LENGTH_CENTER_TO_OUTLET_END"].DoubleValue = center_to_outlet_end;
        //        ecinstance["DESIGN_LENGTH_CENTER_TO_RUN_END"].DoubleValue = center_to_run_end;
        //        ecinstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
        //        ecinstance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
        //        ecinstance["INSULATION"].StringValue = insulation;
        //        result = new BMECObject(ecinstance);
        //    }
        //    return result;
        //}
        //public static BMECObject createBend(string elbowECClassName, double nominal_diameter, double angle, double center_to_outlet_end, double center_to_run_end, double insulation_thickness, string insulation, out string errorMessage, double lengthToBend, double lengthAfterBend, double radiusFactor, int xiamiwanjieshu = 0) {
        //    errorMessage = string.Empty;
        //    IECInstance iECInstance = BMECInstanceManager.Instance.CreateECInstance(elbowECClassName, true);
        //    ISpecProcessor isp = BMECApi.Instance.SpecProcessor;
        //    isp.FillCurrentPreferences(iECInstance, null);
        //    iECInstance["NOMINAL_DIAMETER"].DoubleValue = nominal_diameter;
        //    ECInstanceList eCInstanceList = isp.SelectSpec(iECInstance, true);
        //    BMECObject result = null;
        //    if (eCInstanceList.Count == 0)
        //    {
        //        errorMessage = "当前Specification为配置弯管。";
        //        return null;
        //    }
        //    if (eCInstanceList != null && eCInstanceList.Count > 0)
        //    {
        //        IECInstance ecinstance = eCInstanceList[0];
        //        ecinstance["ANGLE"].DoubleValue = angle;
        //        ecinstance["SPECIFICATION"].StringValue = Bentley.Plant.StandardPreferences.DlgStandardPreference.GetPreferenceValue("SPECIFICATION").ToString();
        //        ecinstance["INSULATION_THICKNESS"].DoubleValue = insulation_thickness;
        //        ecinstance["INSULATION"].StringValue = insulation;
        //        ecinstance["LENGTH_TO_BEND"].DoubleValue = lengthToBend;
        //        ecinstance["LENGTH_AFTER_BEND"].DoubleValue = lengthAfterBend;
        //        ecinstance["BEND_POINT_RADIUS"].DoubleValue = radiusFactor;
        //        if (xiamiwanjieshu > 0)
        //        {
        //            ecinstance["NUM_PIECES"].IntValue = xiamiwanjieshu;
        //        }
        //        result = new BMECObject(ecinstance);
        //    }
        //    return result;
        //}
        /// <summary>
        /// 求异面直线公垂线
        /// </summary>
        /// <param name="line1Point1"></param>
        /// <param name="line1Point2"></param>
        /// <param name="line2Point1"></param>
        /// <param name="line2Point2"></param>
        /// <param name="maleVerticalLinePoint1">线一上的垂点</param>
        /// <param name="maleVerticalLinePoint2">线二上的垂点</param>
        public static bool CalculateMaleVerticalLine(BG.DSegment3d lineA, BG.DSegment3d lineB, out BG.DSegment3d segment, out double fractionA, out double fractionB)
        {
            //maleVerticalLinePoint1 = Bentley.GeometryNET.DPoint3d.Zero;
            //maleVerticalLinePoint2 = Bentley.GeometryNET.DPoint3d.Zero;
            ////公垂线
            //BG.DSegment3d line1 = new BG.DSegment3d(line1Point1, line1Point2);
            //BG.DSegment3d line2 = new BG.DSegment3d(line2Point1, line2Point2);
            //BG.DSegment3d maleVerticalLine = new BG.DSegment3d();
            //double fractionA, fractionB;
            //bool flag = BG.DSegment3d.ClosestApproachSegment(line1, line2, out maleVerticalLine, out fractionA, out fractionB);
            //if (flag)
            //{
            //    maleVerticalLinePoint1 = maleVerticalLine.StartPoint;
            //    maleVerticalLinePoint2 = maleVerticalLine.EndPoint;
            //}
            ////LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(), null, maleVerticalLine);
            ////line.AddToModel();
            ////line = new LineElement(Session.Instance.GetActiveDgnModel(), null, new BG.DSegment3d(BG.DPoint3d.Zero, maleVerticalLinePoint1));
            ////line.AddToModel();
            fractionA = 0.0;
            fractionB = 0.0;
            BG.DPoint3d origin = lineA.StartPoint;
            BG.DPoint3d dPoint3d = lineB.StartPoint;
            BG.DPoint3d target = lineA.EndPoint;
            BG.DPoint3d target2 = lineB.EndPoint;
            BG.DVector3d dVector3d = new BG.DVector3d(origin, target);
            BG.DVector3d dVector3d2 = new BG.DVector3d(dPoint3d, target2);
            double num = dVector3d.NormalizeInPlace();
            double num2 = dVector3d2.NormalizeInPlace();
            BG.DVector3d dVector3d3 = new BG.DVector3d(origin, dPoint3d);
            double num3 = dVector3d.DotProduct(dVector3d2);
            double num4;
            double num5;
            bool result = BG.Geometry.Solve2x2(out num4, out num5, 1.0, -num3, num3, -1.0, dVector3d3.DotProduct(dVector3d), dVector3d3.DotProduct(dVector3d2));
            BG.DPoint3d pointA = BG.DPoint3d.Add(origin, dVector3d, num4);
            BG.DPoint3d pointB = BG.DPoint3d.Add(dPoint3d, dVector3d2, num5);
            fractionA = num4 / num;
            fractionB = num5 / num2;
            segment = new BG.DSegment3d(pointA, pointB);
            return result;

        }
        /// <summary>
        /// 判断空间中两线段相交
        /// </summary>
        /// <param name="intersection">交点</param>
        /// <param name="p1">线段一上一点</param>
        /// <param name="v1">线段一方向</param>
        /// <param name="p2">线段二上一点</param>
        /// <param name="v2">线段二方向</param>
        /// <returns>两线段关系，-2：异面，-1：平行，1：相交（线段直接相交或在延长线上相交）</returns>
        public static int LineLineIntersection(out BG.DPoint3d intersection, BG.DPoint3d p1, BG.DVector3d v1, BG.DPoint3d p2, BG.DVector3d v2)
        {
            intersection = BG.DPoint3d.Zero;
            if (v1.IsParallelOrOppositeTo(v2))//向量平行
            {
                return -1;
            }
            BG.DVector3d v3 = new BG.DVector3d(p1, p2);
            BG.DVector3d vArea1 = v1.CrossProduct(v2);//有向面积1
            BG.DVector3d vArea2 = v3.CrossProduct(v2);//有向面积2
            double num = v3.DotProduct(vArea1);
            if (num >= 100 || num <= -100)//异面 1E-05f
            {
                return -2;
            }
            double num2 = vArea1.DotProduct(vArea2) / vArea1.MagnitudeSquared;//有向面积比值

            intersection = p1 + v1 * num2;

            return 1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="intersection"></param>
        /// <param name="p11"></param>
        /// <param name="p12"></param>
        /// <param name="p21"></param>
        /// <param name="p22"></param>
        /// <returns>两线段关系，-2：异面，-1：平行，1：相交（线段直接相交或在延长线上相交）</returns>
        public static int LineLineIntersection(out BG.DPoint3d intersection, BG.DPoint3d p11, BG.DPoint3d p12, BG.DPoint3d p21, BG.DPoint3d p22)
        {
            BG.DVector3d v1 = new BG.DVector3d(p11, p12);
            BG.DVector3d v2 = new BG.DVector3d(p21, p22);
            return LineLineIntersection(out intersection, p11, v1, p21, v2);
        }
        /// <summary>
        /// 计算两线段的布线路线
        /// 从第一条线段的终点到第二条线段的起点
        /// </summary>
        /// <param name="elem1"></param>
        /// <param name="elem2"></param>
        public List<BG.DPoint3d> Calculate(BG.DSegment3d lineElem1, BG.DSegment3d lineElem2, PathType pathType, out string errorMessage)
        {
            errorMessage = "";

            string[] pathXYZ = new string[] { "X", "Y", "Z" };
            string[] pathXZY = new string[] { "X", "Z", "Y" };
            string[] pathYXZ = new string[] { "Y", "X", "Z" };
            string[] pathYZX = new string[] { "Y", "Z", "X" };
            string[] pathZXY = new string[] { "Z", "X", "Y" };
            string[] pathZYX = new string[] { "Z", "Y", "X" };

            string[] path = new string[3];

            //List<DPoint3d> points = new List<DPoint3d>();
            switch (pathType)
            {
                case PathType.OFF:
                    //points = ZuiduanLianjie(lineElem1, lineElem2);
                    break;
                case PathType.XYZ:
                    path = pathXYZ;
                    break;
                case PathType.XZY:
                    path = pathXZY;
                    break;
                case PathType.YXZ:
                    path = pathYXZ;
                    break;
                case PathType.YZX:
                    path = pathYZX;
                    break;
                case PathType.ZXY:
                    path = pathZXY;
                    break;
                case PathType.ZYX:
                    path = pathZYX;
                    break;
                default:
                    break;
            }
            if (pathType != PathType.OFF)
            {
                points = LujingLianjie(lineElem1, lineElem2, path);//计算按路径连接的路径
                //if (null == points)
                //{
                //    //直接连接了，但是不应该写在这
                //    //create_elbow(BMEC_Object_list[0], BMEC_Object_list[1], out errorMessage);
                //    return points;
                //}
            }
            else
            {
                points = ZuiduanLianjie(lineElem1, lineElem2);//计算最短连接的路径
            }
            #region MyRegion

            //DgnModel dgnModel = Session.Instance.GetActiveDgnModel();

            //LineElement line1 = new LineElement(dgnModel, null, lineElem1);
            //LineElement line2 = new LineElement(dgnModel, null, lineElem2);
            //ElementPropertiesSetter setter = new ElementPropertiesSetter();
            //setter.SetWeight(2);
            //setter.Apply(line1);
            //setter.Apply(line2);
            //if (points != null)
            //{
            //    if (points.Count == 1)
            //    {

            //    }
            //    else
            //    {
            //        //LineStringElement line = new LineStringElement(dgnModel, null, points.ToArray());
            //        //setter.Apply(line);
            //        //line.AddToModel();
            //    }
            //}
            //line1.AddToModel();
            //line2.AddToModel();
            #endregion

            return points;
        }
        /// <summary>
        /// 按路径连接
        /// </summary>
        /// <param name="lineElem1"></param>
        /// <param name="lineElem2"></param>
        /// <param name="path"></param>
        private List<BG.DPoint3d> LujingLianjie(BG.DSegment3d lineElem1, BG.DSegment3d lineElem2, string[] path)
        {
            List<BG.DPoint3d> points = new List<BG.DPoint3d>();
            BG.DPoint3d line1EndPoint = lineElem1.EndPoint;
            BG.DPoint3d line2StartPoint = lineElem2.StartPoint;

            if (line1EndPoint == line2StartPoint)
            {
                //首尾相连
                return null;
            }
            BG.DSegment3d segment;
            double fractionA, fractionB;
            BG.DSegment3d.ClosestApproachSegment(lineElem1, lineElem2, out segment, out fractionA, out fractionB);
            if (segment.StartPoint == segment.EndPoint)
            {
                //有交点
                double fractiontemp;
                BG.DPoint3d closePointtemp;
                lineElem1.ClosestFractionAndPoint(segment.StartPoint, true, out fractiontemp, out closePointtemp);
                if (closePointtemp == segment.StartPoint)
                {
                    //在直线内相交
                    //return null;
                    MessageBox.Show("直线内相交");
                }
                //points.Add(segment.StartPoint);
                //return points;
            }


            points.Add(line1EndPoint);
            for (int i = 0; i < path.Length; i++)
            {
                BG.DPoint3d point = new BG.DPoint3d();
                point = MovePoint(points.Last(), line2StartPoint, path[i]);
                if (point != line2StartPoint && point != line2StartPoint)
                {
                    if (!points.Contains(point))
                    {
                        points.Add(point);
                    }
                }
            }
            //points.Insert(0, line1EndPoint);
            points.Add(lineElem2.StartPoint);
            return points;
        }
        /// <summary>
        /// 最短路径连接，即按公垂线连接
        /// </summary>
        /// <param name="lineElem1"></param>
        /// <param name="lineElem2"></param>
        /// <returns></returns>
        private List<BG.DPoint3d> ZuiduanLianjie(BG.DSegment3d lineElem1, BG.DSegment3d lineElem2)
        {
            List<BG.DPoint3d> result = new List<BG.DPoint3d>();
            result.Add(lineElem1.EndPoint);
            bool isPingxing = false;//
            isPingxing = new BG.DVector3d(lineElem1.StartPoint, lineElem1.EndPoint).IsParallelOrOppositeTo(new BG.DVector3d(lineElem2.StartPoint, lineElem2.EndPoint));

            BG.DPoint3d p1 = lineElem1.EndPoint;
            BG.DPoint3d p2 = lineElem2.StartPoint;
            if (p1 == p2)
            {
                p2 = lineElem2.EndPoint;
            }
            BG.DVector3d vector1 = new BG.DVector3d(p1, p2);
            BG.DVector3d faxiangliang1 = vector1.CrossProduct(new BG.DVector3d(lineElem1.StartPoint, lineElem1.EndPoint));
            BG.DVector3d faxiangliang2 = vector1.CrossProduct(new BG.DVector3d(lineElem2.StartPoint, lineElem2.EndPoint));
            bool gongmian = faxiangliang1.IsParallelOrOppositeTo(faxiangliang2);

            if (isPingxing)
            {
                //平行

            }
            else if (!gongmian)
            {
                //异面
                BG.DSegment3d segment;
                double fractionA;
                double fractionB;
                BG.DSegment3d.ClosestApproachSegment(lineElem1, lineElem2, out segment, out fractionA, out fractionB);
                result.Add(segment.StartPoint);
                result.Add(segment.EndPoint);
            }
            else
            {
                //相交
            }
            result.Add(lineElem2.StartPoint);

            return result;
        }
        /// <summary>
        /// 一个点向另一个点移动
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static BG.DPoint3d MovePoint(BG.DPoint3d point1, BG.DPoint3d point2, string dir)
        {
            BG.DPoint3d result = point1;
            switch (dir)
            {
                case "X":
                    result.X = point2.X;
                    break;
                case "Y":
                    result.Y = point2.Y;
                    break;
                case "Z":
                    result.Z = point2.Z;
                    break;
                default:
                    break;
            }
            return result;
        }
        //TODO 异径弯头
        public static string GetYijingElbow(string angle)
        {
            string elbowecclassname = "PIPE_ELBOW_REDUCING_90_DEGREE";
            if (angle.Contains("45"))
            {
                elbowecclassname = "PIPE_ELBOW_REDUCING_45_DEGREE";
            }
            else if (angle.Contains("90"))
            {
                elbowecclassname = "PIPE_ELBOW_REDUCING_90_DEGREE";
            }
            return elbowecclassname;
        }

        public static bool HasOverlap(List<BMECObject> pipes)
        {
            for (int i = 0; i < pipes.Count; i++)
            {
                BG.DPoint3d[] p1 = GetTowPortPoint(pipes[i]);
                BG.DVector3d v1 = new BG.DVector3d(p1[0], p1[1]);
                for (int j = i + 1; j < pipes.Count; j++)
                {
                    BG.DPoint3d[] p2 = GetTowPortPoint(pipes[j]);
                    BG.DVector3d v2 = new BG.DVector3d(p2[0], p2[1]);
                    double m0 = v1.DotProduct(v1);
                    BG.DVector3d v3 = new BG.DVector3d(p1[0], p2[0]);
                    BG.DVector3d v4 = new BG.DVector3d(p1[0], p2[1]);
                    if (v1.IsParallelOrOppositeTo(v2) && v1.IsParallelOrOppositeTo(v3) && v1.IsParallelOrOppositeTo(v4))
                    {
                        double m1 = v1.DotProduct(v3);
                        double m2 = v1.DotProduct(v4);
                        if (m1 >= 0 && m2 >= 0)
                        {
                            if ((m1 <= m0 && m2 <= m0) || (Math.Max(m1, m2) > m0 && Math.Min(m1, m2) < m0))
                            {
                                return true;
                            }
                        }
                        else if (Math.Max(m1, m2) >= 0 && Math.Min(m1, m2) <= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsOverlap(BMECObject piep1, BMECObject pipe2)
        {
            BG.DPoint3d[] p1 = GetTowPortPoint(piep1);
            BG.DVector3d v1 = new BG.DVector3d(p1[1], p1[0]);
            BG.DPoint3d[] p2 = GetTowPortPoint(pipe2);
            BG.DVector3d v2 = new BG.DVector3d(p2[0], p2[1]);

            double m0 = v1.DotProduct(v1);
            BG.DVector3d v3 = new BG.DVector3d(p1[1], p2[0]);
            BG.DVector3d v4 = new BG.DVector3d(p1[1], p2[1]);
            if (v1.IsParallelOrOppositeTo(v2) && v1.IsParallelOrOppositeTo(v3) && v1.IsParallelOrOppositeTo(v4))
            {
                double m1 = v1.DotProduct(v3);
                double m2 = v1.DotProduct(v4);
                if (m1 >= 0 && m2 >= 0)
                {
                    if ((m1 <= m0 && m2 <= m0) || (Math.Max(m1, m2) > m0 && Math.Min(m1, m2) < m0))
                    {
                        return true;
                    }
                }
                else if (Math.Max(m1, m2) >= 0 && Math.Min(m1, m2) <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isIntersect(List<BMECObject> pipes)
        {
            for (int i = 0; i < pipes.Count; i++)
            {
                BG.DPoint3d[] p1 = GetTowPortPoint(pipes[i]);
                BG.DSegment3d s1 = new BG.DSegment3d(p1[0], p1[1]);
                for (int j = i + 1; j < pipes.Count; j++)
                {
                    BG.DPoint3d[] p2 = GetTowPortPoint(pipes[j]);
                    BG.DSegment3d s2 = new BG.DSegment3d(p2[0], p2[1]);
                    BG.DSegment3d segment;
                    double fractionA, fractionB;
                    bool isIntersect = BG.DSegment3d.ClosestApproachSegment(s1, s2, out segment, out fractionA, out fractionB);
                    if (segment.Length == 0 && (fractionA != 0) || (fractionA != 1) && (fractionB != 0) || (fractionB != 1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static BG.DPoint3d[] GetTowPortPoint(BMECObject twoPortPipeObject)
        {
            return new BG.DPoint3d[] { twoPortPipeObject.GetNthPoint(0), twoPortPipeObject.GetNthPoint(1) };
        }

        //just angle(0,90)
        public static BG.DPoint3d GetPointOnArcWithAngle(double angle, BG.DPoint3d centerPoint, BG.DPoint3d tanglePoint, BG.DVector3d tangleVec, out BG.DVector3d tangleVecAtResultPoint)
        {
            BG.DVector3d radiusVec = new BG.DVector3d(tanglePoint, centerPoint);
            BG.DVector3d tangleLengthVec;
            double magnitude;
            double length = Math.Tan(angle * Math.PI / 180.0) * radiusVec.Magnitude;
            tangleVec.TryScaleToLength(length, out tangleLengthVec, out magnitude);
            BG.DVector3d jiaodianVec;
            (tangleLengthVec - radiusVec).TryScaleToLength(radiusVec.Magnitude, out jiaodianVec, out magnitude);

            tangleVecAtResultPoint = radiusVec.CrossProduct(tangleVec).CrossProduct(jiaodianVec);
            if (tangleVecAtResultPoint.DotProduct(tangleVec) < 0)
            {
                tangleVecAtResultPoint = -tangleVecAtResultPoint;
            }

            return centerPoint + jiaodianVec;
        }

        private List<Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem> pipingNetworkSystems = null;
        private List<string> pipelinesName = null;
        public void GetPipeLine()
        {
            pipingNetworkSystems = Bentley.OpenPlantModeler.SDK.AssociatedItems.NetworkSystem.GetExistingPipingNetworkSystems();
            pipelinesName = new List<string>();
            foreach (var item in pipingNetworkSystems)
            {
                string name = item.Name;
                pipelinesName.Add(name);
            }
            //return pipelinesName;
        }
    }
}
