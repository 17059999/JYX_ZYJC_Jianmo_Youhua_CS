﻿
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.Plant.StandardPreferences;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
//TODO 加一个连接形式是承插焊的(SWF)
namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 管道操作的相关方法
    /// </summary>
    public class CutOffPipe
    {
        /// <summary>
        /// 当前设计文件的主单位
        /// </summary>
        public static double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
        /// <summary>
        /// 管道长度
        /// </summary>
        public static string LENGTH = "LENGTH";
        /// <summary>
        /// 类型为管道
        /// </summary>
        public static string PIPE = "PIPE";
        /// <summary>
        /// 管道直径，这个不是管道的实际直径，只类似于一种型号标识
        /// </summary>
        public static string NOMINAL_DIAMETER = "NOMINAL_DIAMETER";
        /// <summary>
        /// 管道上流端口ID
        /// </summary>
        public static int MAIN_PORT_ID = 0;
        /// <summary>
        /// 管道下流端口ID
        /// </summary>
        public static int RUN_PORT_ID = 1;
        /// <summary>
        /// 公差
        /// </summary>
        public static double GONGCHA = 0.1;
        /// <summary>
        /// 要处理的管道
        /// </summary>
        private List<BMECObject> pipes = null;
        /// <summary>
        /// 获取要处理的管道 pipes
        /// </summary>
        /// <returns></returns>
        public List<BMECObject> getPipes() {
            if (this.pipes != null && this.pipes.Count > 0)
            {
                return this.pipes;
            }
            return null;
        }
        /// <summary>
        /// 给要处理的管道 pipes 赋值
        /// </summary>
        /// <param name="list"></param>
        public void setPipes(List<BMECObject> list) {
            if (list != null)
            {
                this.pipes = list;
            }
        }
        /// <summary>
        /// 获取选中的管道
        /// </summary>
        /// <returns>选中管道的 BMECObject 的集合</returns>
        public List<BMECObject> getPipe()
        {
            List<BMECObject> selectedpipes = new List<BMECObject>();//选中的管道的容器
            List<BMECObject> tempECObject = new List<BMECObject>();//临时容器
            ElementAgenda elementAgenda = new ElementAgenda();//选中Element的集合
            SelectionSetManager.BuildAgenda(ref elementAgenda);//获取选中的元素的集合
            for (uint i = 0; i < elementAgenda.GetCount(); i++)//获取选中 Element 的 BMECObject
            {
                Element element = elementAgenda.GetEntry(i);
                BMECObject ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);//通过ElementId 获取 BMECObject
                tempECObject.Add(ec_object);
            }
            if (tempECObject.Count > 0)
            {
                foreach (var ecobject in tempECObject)//将选中的类型为 PIPE 的 BMECObject 存到 selectedpipes 容器中
                {
                    if (ecobject.ClassName == "PIPE")
                    {
                        selectedpipes.Add(ecobject);
                    }
                }
            }
            this.setPipes(selectedpipes);//将获取到的管道保存到成员变量 pipes
            return selectedpipes;
        }
        /// <summary>
        /// 将过长的管道断开成指定长度的多段管道
        /// </summary>
        /// <param name="pipe">需要断开的管道</param>
        /// <param name="length">断开后管道的最大长度</param>
        /// <returns>断开后的多段管道</returns>
        public List<BMECObject> cutOffPipe(BMECObject pipe, double length)
        {
            List<BMECObject> cutPipes = new List<BMECObject>();//断开后的多段管道的容器
            if (pipe == null)
            {
                return null;
            }
            else if(length <= 0.0)
            {
                cutPipes.Add(pipe);
                return cutPipes;
            }
            double length_pipe = pipe.GetDoubleValueInMM(LENGTH);//原管道长度
            if (length_pipe > length)//当管道长度大于预制长度，进行断开
            {
                string[] endPreparations = GetEndPreparations(pipe);//管道两端口的连接形式
                bool isSCFEndPreparation = false;
                if (endPreparations.Length == 2 && endPreparations[0].Contains("THREADED") && endPreparations[1].Contains("THREADED"))
                {
                    isSCFEndPreparation = true;
                }
                if (isSCFEndPreparation)
                {
                    List<string> ecclassName = new List<string>() {
                        "PIPE_COUPLING","DRESSER_COUPLING","FLEXIBLE_COUPLING","HALF_PIPE_COUPLING","HOSE_COUPLING",
                        "REDUCING_PIPE_COUPLING","CERI_PIPE_COUPLING_DZTGJT","CERI_PIPE_COUPLING_DZTZHJT","CERI_PIPE_COUPLING_ZTHJJT","CERI_PIPE_COUPLING_ZTJT"
                    };
                    List<IECInstance> ecclassInstance = new List<IECInstance>();
                    IECInstance instance = BMECInstanceManager.Instance.CreateECInstance("PIPE_COUPLING", true);
                    foreach (string item in ecclassName)
                    {
                        IECInstance tempinstance = BMECInstanceManager.Instance.CreateECInstance(item, true);
                        ecclassInstance.Add(tempinstance);
                    }
                    List<Bentley.ECObjects.Schema.IECClass> ecclassesss = new List<Bentley.ECObjects.Schema.IECClass>();
                    foreach (IECInstance item in ecclassInstance)
                    {
                        ecclassesss.Add(item.ClassDefinition);
                    }
                    ISpecProcessor specProcessor = m_BMECApi.SpecProcessor;
                    specProcessor.FillCurrentPreferences(instance, true);
                    instance["NOMINAL_DIAMETER"].DoubleValue = pipe.GetDoubleValueInMM("NOMINAL_DIAMETER");
                    System.Collections.Hashtable whereClauseList = new System.Collections.Hashtable();
                    whereClauseList.Add("NOMINAL_DIAMETER", pipe.GetDoubleValueInMM("NOMINAL_DIAMETER").ToString());
                    //whereClauseList.Add("NOMINAL_DIAMETER_RUN_END", pipe.GetDoubleValueInMM("NOMINAL_DIAMETER").ToString());
                    ECInstanceList instanceList = specProcessor.SelectSpec(ecclassesss, whereClauseList, DlgStandardPreference.GetPreferenceValue("SPECIFICATION"), true, "请选择螺纹连接件");
                    IECInstance coupling = null;
                    BMECObject ECObject = null;
                    if (null != instanceList && instanceList.Count > 0)
                    {
                        coupling = instanceList[0];
                        coupling.SetDouble("INSULATION_THICKNESS", pipe.GetDoubleValueInMM("INSULATION_THICKNESS"));
                        ECObject = new BMECObject(coupling);
                        double couplingLen = ECObject.GetDoubleValueInMM("LENGTH_EFFECTIVE");//这是在此值(LENGTH_EFFECTIVE)正确设置的前提下
                        couplingLen = (ECObject.Ports[0].LocationInUors - ECObject.Ports[1].LocationInUors).Magnitude;//故暂时使用的是此值(通过两端点计算)
                        string[] endPreparationsCouplings = GetEndPreparations(ECObject);

                        if (!(endPreparationsCouplings.Length == 2 && endPreparationsCouplings[0].Contains("THREADED") && endPreparationsCouplings[1].Contains("THREADED")))
                        {
                            cutPipes.Add(pipe);
                            System.Windows.Forms.MessageBox.Show("所选连接件的连接形式为非螺纹");
                            return cutPipes;
                        }
                        if (!(length_pipe > length + couplingLen))
                        {
                            cutPipes.Add(pipe);
                            System.Windows.Forms.MessageBox.Show("管道过短无法断开");
                            return cutPipes;
                        }

                        DPoint3d[] portsPoint = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(pipe);
                        int count = (int)(length_pipe / (length + couplingLen));
                        double duanguanLength = length_pipe % (length + couplingLen);
                        DPoint3d startPoint = portsPoint[0];
                        DPoint3d endPoint = portsPoint[1];
                        DVector3d pipeVector = new DVector3d(startPoint, endPoint);
                        pipe.SetDoubleValueFromMM(LENGTH, length);
                        pipe.Create();

                        List<DPoint3d> points = new List<DPoint3d>();
                        points.Add((JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(pipe))[1]);

                        for (int i = 0; i < count - 1; i++)
                        {
                            DVector3d offsetVector = new DVector3d();
                            double magnitude;
                            pipeVector.TryScaleToLength((couplingLen) * uor_per_master, out offsetVector, out magnitude);
                            DPoint3d point1 = DPoint3d.Add(points.Last(), offsetVector);
                            points.Add(point1);
                            pipeVector.TryScaleToLength((length) * uor_per_master, out offsetVector, out magnitude);
                            DPoint3d point2 = DPoint3d.Add(points.Last(), offsetVector);
                            points.Add(point2);
                        }
                        if (duanguanLength > 0.0)
                        {
                            DVector3d offsetVector = new DVector3d();
                            double magnitude;
                            pipeVector.TryScaleToLength((couplingLen) * uor_per_master, out offsetVector, out magnitude);
                            DPoint3d point1 = DPoint3d.Add(points.Last(), offsetVector);
                            points.Add(point1);
                            points.Add(endPoint);
                        }
                        List<BMECObject> couplings = new List<BMECObject>();
                        for (int i = 0; i < (points.Count - 1) / 2; i++)
                        {
                            IECInstance pipeInstance = (IECInstance)pipe.Instance.Clone();
                            BMECObject newPipe = CreatePipe(points[i * 2 + 1], points[i * 2 + 2], pipe);
                            if (newPipe != null)
                            {
                                cutPipes.Add(newPipe);
                            }
                            BMECObject couplingObject = new BMECObject();
                            couplingObject.Copy(ECObject);
                            couplingObject.Transform3d = pipe.Transform3d;
                            BMECApi.Instance.TranslateComponent(couplingObject, points[i * 2]);
                            couplingObject.Create();
                            if (couplingObject != null)
                            {
                                couplings.Add(couplingObject);
                            }
                        }
                        cutPipes.Insert(0, pipe);
                        //DPoint3d[] couplingsPortsPoint;
                        //for (int i = 0; i < couplings.Count; i++)
                        //{
                        //    couplingsPortsPoint = JYX_ZYJC_CLR.PublicMethod.get_two_port_object_end_points(couplings[i]);
                        //    string[] couplingsEndPreparations = GetEndPreparations(couplings[i]);
                        //    if (couplingsEndPreparations.Length == 2 && !(couplingsEndPreparations[0].Contains("THREADED") && couplingsEndPreparations[1].Contains("THREADED")))
                        //    {
                        //        BMECApi.Instance.CreateJointForIncompatiblePorts(cutPipes[i].Instance, couplings[i].Instance, couplingsPortsPoint[0]);
                        //        if (i + 1 < cutPipes.Count)
                        //        {
                        //            BMECApi.Instance.CreateJointForIncompatiblePorts(cutPipes[i + 1].Instance, couplings[i].Instance, couplingsPortsPoint[1]);
                        //        }
                        //    }
                        //}
                        foreach (var newpipe in cutPipes)
                        {
                            if (newpipe != null)
                            {
                                newpipe.DiscoverConnectionsEx();
                                newpipe.UpdateConnections();
                            }
                        }
                        foreach (var newpipe in couplings)
                        {
                            if (newpipe != null)
                            {
                                newpipe.DiscoverConnectionsEx();
                                newpipe.UpdateConnections();
                            }
                        }
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("未找到可供选择的断开连接组件或未选择断开连接组件");
                    }
                }
                else
                {
                    int count = (int)(length_pipe / length);//length 长度的管道数量，包含了缩短的最初一节管道
                    double duanguanLength = length_pipe % length;//剩余一节短管长度
                    DPoint3d startPoint = pipe.GetNthPort(0).LocationInUors;//最初管道的起点
                    DPoint3d endPoint = pipe.GetNthPort(1).LocationInUors;//最初管道的终点
                    DVector3d pipeVector = new DVector3d(startPoint, endPoint);//管道从起点到终点的方向向量

                    pipe.SetDoubleValueFromMM(LENGTH, length);//将原管道缩短到 length 长度
                    pipe.Create();

                    List<DPoint3d> points = new List<DPoint3d>();//创建管道的几个端点
                    points.Add(pipe.GetNthPort(1).LocationInUors);//将缩短的原管道的终点加到端点容器中

                    for (int i = 0; i < count - 1; i++)//将长度为 length 的管道的端点添加到容器中
                    {
                        DVector3d changguanVector = new DVector3d();//长管的向量
                        double magnitude;//向量的模长
                        pipeVector.TryScaleToLength((length) * uor_per_master, out changguanVector, out magnitude);
                        DPoint3d point = DPoint3d.Add(points.Last(), changguanVector);//要创建管道的端点
                        points.Add(point);
                    }
                    if (duanguanLength != 0.0)//将长度不足 length 的管道的端点添加到容器中，即原管道原来的终点
                    {
                        points.Add(endPoint);
                    }
                    for (int i = 0; i < points.Count - 1; i++)//通过端点循环创建管道
                    {
                        IECInstance pipeInstance = (IECInstance)pipe.Instance.Clone();
                        //BMECObject newPipe = this.create_pipe(points[i], points[i + 1], pipeInstance);
                        BMECObject newPipe = CreatePipe(points[i], points[i + 1], pipe);
                        if (newPipe != null)//由于当传入的两点相同时无法创建管道，故做一次判断
                        {
                            cutPipes.Add(newPipe);
                        }
                    }
                    cutPipes.Add(pipe);//最后将原来的管道也添加到集合中
                    foreach (var newpipe in cutPipes)//更新连结性
                    {
                        if (newpipe != null)
                        {
                            newpipe.DiscoverConnectionsEx();
                            newpipe.UpdateConnections();
                        }
                    }
                }
            }
            else
            {
                cutPipes.Add(pipe);
            }
            return cutPipes;
        }
        private BMECApi m_BMECApi = BMECApi.Instance;//管理 ECInstance 工具?
        /// <summary>
        /// 该方法已弃用
        /// </summary>
        /// <param name="startPoint">起点mainPort</param>
        /// <param name="endPoint">终点runPort</param>
        /// <param name="pipeInstance">管道样例</param>
        /// <returns>创建的管道对象BMECObject</returns>
        public static BMECObject create_pipe(DPoint3d startPoint, DPoint3d endPoint, IECInstance pipeInstance)
        {
            if (pipeInstance == null)
            {
                return null;
            }
            if (startPoint == endPoint)
            {
                return null;
            }
            IECInstance pipe_instance = BMECInstanceManager.Instance.CreateECInstance(PIPE, true);//创建一个 PIPE的ECInstance，为什么会是空
            ISpecProcessor specProcessor = BMECApi.Instance.SpecProcessor;
            specProcessor.FillCurrentPreferences(pipe_instance, pipeInstance);//填充样式？TODO 但样例似乎并没有起作用。原因在于 BMECApi 有时并没有正确获取到 SpecProcessor 的对象实例
            pipe_instance[NOMINAL_DIAMETER].DoubleValue = pipeInstance[NOMINAL_DIAMETER].DoubleValue;//设置管径
            pipe_instance["SCHEDULE"].StringValue = pipeInstance["SCHEDULE"].StringValue;//设置 SCHEDULE
            ECInstanceList ec_instance_list = specProcessor.SelectSpec(pipe_instance, false);//选择数据集，就是会弹出几条数据让选择的那个界面
            BMECObject ec_object = null;//通过 IECInstance 要创建的 BMECObject
            if (null != ec_instance_list && ec_instance_list.Count > 0)//当数据源有数据时
            {
                for (int i = 0; i < ec_instance_list.Count; i++)//找到符合条件的数据
                {
                    if (ec_instance_list[i]["SCHEDULE"].StringValue == pipeInstance["SCHEDULE"].StringValue)
                    {
                        pipe_instance = ec_instance_list[i];
                        pipe_instance["INSULATION_THICKNESS"].DoubleValue = pipeInstance["INSULATION_THICKNESS"].DoubleValue;//保温层厚度
                        ec_object = new BMECObject(pipe_instance);
                        ec_object.SetLinearPoints(startPoint, endPoint);//设置管道起点与终点，TODO 当反复断开同一根管道会出现 double 的误差，导致获取的端点相同无法创建
                        ec_object.Create();//创建管道
                        break;//当找到符合条件的数据时跳出
                    }
                }
            }
            return ec_object;
        }
        /// <summary>
        /// 创建管道
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        public static BMECObject CreatePipe(DPoint3d startPoint, DPoint3d endPoint, BMECObject template) {
            if (template == null)
            {
                return null;
            }
            if (startPoint == endPoint)
            {
                return null;
            }
            BMECObject result = new BMECObject();
            result.Copy(template);
            result.SetLinearPoints(startPoint, endPoint);
            result.Create();
            return result;
        }
        /// <summary>
        /// 创建管接头
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="dn"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        public static BMECObject CreateCouplings(DPoint3d startPoint, double dn, DTransform3d transform) {
            IECInstance instance = BMECInstanceManager.Instance.CreateECInstance("FLEXIBLE_COUPLING", true);
            ISpecProcessor specProcessor = BMECApi.Instance.SpecProcessor;
            specProcessor.FillCurrentPreferences(instance, true);
            instance["NOMINAL_DIAMETER"].DoubleValue = dn;
            ECInstanceList instanceList = specProcessor.SelectSpec(instance, true);
            BMECObject ECObject = null;
            if (null != instanceList && instanceList.Count > 0)
            {
                instance = instanceList[0];
                ECObject = new BMECObject(instance);
                ECObject.Transform3d = transform;
                ECObject.Create();
                BMECApi.Instance.TranslateComponent(ECObject, startPoint);
                ECObject.Create();
                ECObject.DiscoverConnectionsEx();
                ECObject.UpdateConnections();
            }
            return ECObject;
        }
        /// <summary>
        /// 合并管道
        /// </summary>
        public static List<BMECObject> hebing_guandao(List<BMECObject> pipes, out int hebingGuandaozuNum,bool sort) {
            if (pipes == null) {
                hebingGuandaozuNum = 0;
                return null;
            }
            //合并完成后的管道容器
            List<BMECObject> hebingGuandao = new List<BMECObject>();
            //将管道按连接分组
            List<List<BMECObject>> pipe_fenzu = new List<List<BMECObject>>();
            pipe_fenzu = getPipeConnect(pipes);
            if (pipe_fenzu != null && pipe_fenzu.Count > 0)
            {
                hebingGuandaozuNum = 0;//记录需要合并的管道组的数量
                if (sort)
                {
                    //将组中的管道排序
                    for (int i = 0; i < pipe_fenzu.Count; i++)
                    {
                        if (pipe_fenzu[i].Count > 1)
                        {
                            //pipe_fenzu[i] = sortPipes(pipe_fenzu[i]);
                        }
                    }
                }
                //合并每组管道
                for (int i = 0; i < pipe_fenzu.Count; i++)
                {
                    if (pipe_fenzu[i].Count > 1)
                    {

                        //得到管道总长
                        double zongchang = getLengthOfPipes(pipe_fenzu[i]);
                        //删除第一根之外的其他管道
                        for (int j = 1; j < pipe_fenzu[i].Count; j++)
                        {
                            BMECApi.Instance.DeleteFromModel(pipe_fenzu[i][j]);
                        }
                        //将第一根管道延申至长度
                        pipe_fenzu[i].First().SetDoubleValueFromMM(LENGTH, zongchang);
                        pipe_fenzu[i].First().Create();
                        hebingGuandaozuNum++;
                    }
                }
                //返回合并完成后剩下的管道
                foreach (var zu in pipe_fenzu)
                {
                    //更新连接性
                    zu.First().DiscoverConnectionsEx();
                    zu.First().UpdateConnections();
                    hebingGuandao.Add(zu.First());//将更新完的管道添加到容器中
                }
            }
            else
            {
                hebingGuandaozuNum = 0;
            }
            return hebingGuandao;
        }
        /// <summary>
        /// 合并管道
        /// </summary>
        public void mergePipe(List<BMECObject> selectedPipesArg = null) {
            List<BMECObject> selectedPipes = new List<BMECObject>();
            if (selectedPipesArg != null)
            {
                selectedPipes = selectedPipesArg;
            }
            else
            {
                selectedPipes = this.getPipe();//被选中的管道的容器
            }
            if (selectedPipes != null && selectedPipes.Count > 0)
            {
                int selectedPipesNum = selectedPipes.Count;//被选中的管道数量
                int hebingGuandaozuNum = 0;//经过合并的管道组数量
                List<BMECObject> hebingGuandao = new List<BMECObject>();//合并后的管道容器
                hebingGuandao = hebing_guandao(selectedPipes, out hebingGuandaozuNum,true);//合并管道
                if (hebingGuandao != null && hebingGuandao.Count > 0)
                {
                    SelectionSetManager.EmptyAll();//清空之前选中的元素
                    DgnModelRef modelRef = Session.Instance.GetActiveDgnModelRef();
                    if (hebingGuandaozuNum > 0)
                    {
                        foreach (var pipe in hebingGuandao)
                        {
                            ulong elementId = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe);//获取ecobject的element的id
                            Element element = Session.Instance.GetActiveDgnModel().FindElementById(new ElementId(ref elementId));//通过elementId获取Element
                            SelectionSetManager.AddElement(element, modelRef);//添加需要选中的元素
                        }
                        MessageBox.Show("已选中管道：" + selectedPipesNum + ",合并的管道段数：" + hebingGuandaozuNum);//给出winform窗口提示信息
                        MessageCenter.Instance.ShowInfoMessage("合并管道完成！", "已选中管道：" + selectedPipesNum + ",合并的管道段数：" + hebingGuandaozuNum, false);//在OPM消息中心显示消息
                    }
                    else
                    {
                        MessageBox.Show("未合并管道。");
                        MessageCenter.Instance.ShowInfoMessage("未合并管道！", "没有能够合并的管道。", false);
                    }
                }
            }
            else
            {
                MessageBox.Show("未选中管道！");
                MessageCenter.Instance.ShowInfoMessage("未选中管道！", "没有检测到选择的管道。", false);
            }
        }
        public static List<BMECObject> mergePipes(List<BMECObject> selectedPipesArg)
        {
            List<BMECObject> selectedPipes = selectedPipesArg;
            if (selectedPipes != null && selectedPipes.Count > 0)
            {
                int selectedPipesNum = selectedPipes.Count;//被选中的管道数量
                int hebingGuandaozuNum = 0;//经过合并的管道组数量
                List<BMECObject> hebingGuandao = new List<BMECObject>();//合并后的管道容器


                hebingGuandao = hebing_guandao(selectedPipes, out hebingGuandaozuNum,false);//合并管道
                return hebingGuandao;
                //if (hebingGuandao != null && hebingGuandao.Count > 0)
                //{
                //    SelectionSetManager.EmptyAll();//清空之前选中的元素
                //    DgnModelRef modelRef = Session.Instance.GetActiveDgnModelRef();
                //    if (hebingGuandaozuNum > 0)
                //    {
                //        foreach (var pipe in hebingGuandao)
                //        {
                //            ulong elementId = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(pipe);//获取ecobject的element的id
                //            Element element = Session.Instance.GetActiveDgnModel().FindElementById(new ElementId(ref elementId));//通过elementId获取Element
                //            SelectionSetManager.AddElement(element, modelRef);//添加需要选中的元素
                //        }
                //        MessageBox.Show("已选中管道：" + selectedPipesNum + ",合并的管道段数：" + hebingGuandaozuNum);//给出winform窗口提示信息
                //        MessageCenter.Instance.ShowInfoMessage("合并管道完成！", "已选中管道：" + selectedPipesNum + ",合并的管道段数：" + hebingGuandaozuNum, false);//在OPM消息中心显示消息
                //    }
                //    else
                //    {
                //        MessageBox.Show("未合并管道。");
                //        MessageCenter.Instance.ShowInfoMessage("未合并管道！", "没有能够合并的管道。", false);
                //    }
                //}
            }
            return new List<BMECObject>();
            //else
            //{
            //    MessageBox.Show("未选中管道！");
            //    MessageCenter.Instance.ShowInfoMessage("未选中管道！", "没有检测到选择的管道。", false);
            //}
        }
        /// <summary>
        /// 一组管道的总长
        /// </summary>
        /// <param name="pipes">相连的一组管道</param>
        /// <returns></returns>
        public static double getLengthOfPipes(List<BMECObject> pipes) {
            double zongchang = 0.0;
            if (pipes == null)
            {
                return zongchang;
            }
            foreach (var pipe in pipes)
            {
                double templength = pipe.GetDoubleValueInMM(LENGTH);
                zongchang += templength;
            }
            return zongchang;
        }
        /// <summary>
        /// List是否全是管道
        /// </summary>
        /// <param name="pipes">一组BMECObject</param>
        /// <returns>true：全为管道，false：不全为管道</returns>
        public static bool isPipe(List<BMECObject> pipes) {
            bool bRet = true;
            if (pipes == null)
            {
                bRet = false;
                return bRet;
            }
            foreach (var pipe in pipes)
            {
                if (pipe.Instance.ClassDefinition.Name != PIPE)//TODO 父类是管道
                {
                    bRet = false;
                }
            }
            return bRet;
        }
        /// <summary>
        /// 将相连的管道分为一组
        /// 不应该添加没选中但有连通性的管道
        /// </summary>
        /// <param name="pipes">要处理的管道容器</param>
        /// <returns></returns>
        public static List<List<BMECObject>> getPipeConnect(List<BMECObject> pipes)
        {
            if (null == pipes || pipes.Count < 1) return null;
            List<List<BMECObject>> pipes_fenzu = new List<List<BMECObject>>();//分组后的管道存储容器
            //分组，随便找一根管道，迭代找他的上游管道和下游管道
            for (int i = 0; i < pipes.Count; i++)
            {
                bool yifenzu = false;//管道是否分组标志
                foreach (var zu in pipes_fenzu)//如果分组容器中已经包含此管道，不在进行迭代寻找上下游管道
                {
                    if (BMECApi.Instance.ObjectContained(pipes[i], zu)) yifenzu = true;
                }
                if (!yifenzu)//没包含在分组容器中，将寻找他的上下游管道
                {
                    //List<BMECObject> fenzu = new List<BMECObject>();//管道分组容器
                    //bool shangyou_guandao = false;//是否存在上游管道，不存在时置为true
                    //bool xiayou_guandao = false;//是否存在下游管道，不存在时置为true
                    //fenzu.Add(pipes[i]);//将选中管道容器中的第一个放到新分组中

                    List<BMECObject> fenzu = new List<BMECObject>();//管道分组容器
                    bool shangyou_guandao = false;//是否存在上游管道，不存在时置为true
                    bool xiayou_guandao = false;//是否存在下游管道，不存在时置为true
                    fenzu.Add(pipes[i]);//将选中管道容器中的第一个放到新分组中
                    BMECObject shangyou_current_object = pipes[i];
                    BMECObject xiayou_current_object = pipes[i];

                    while (!(shangyou_guandao && xiayou_guandao))//寻找上游与下游管道直到找不到
                    {
                        if (!shangyou_guandao)//没有上游管道就不用再执行查找上游管道了
                        {
                            #region MyRegion

                            //BMECObject ec_object_mainPort = fenzu.First().GetConnectedComponentAtPort(fenzu.First().Ports[0].ID);//查找上游管道
                            //if (ec_object_mainPort != null && ec_object_mainPort.ClassName == PIPE && this.myListContainsToBMECObject(pipes, ec_object_mainPort))//当上游连接组件是不为空的管道且在选择集中 TODO　父类是管道
                            //{
                            //    BMECObject tempecobjectrunport = ec_object_mainPort.GetConnectedComponentAtPort(ec_object_mainPort.Ports[0].ID);
                            //    if (tempecobjectrunport != null && BMECApi.Instance.ObjectContained(tempecobjectrunport, fenzu/*new List<BMECObject>() { fenzu.First() }*/))
                            //    {
                            //        shangyou_guandao = true;
                            //    }
                            //    else
                            //    {
                            //        fenzu.Insert(0, ec_object_mainPort);//将上游管道添加到分组中
                            //    }
                            //}
                            //else
                            //{
                            //    //建立没有上游管道的flag
                            //    shangyou_guandao = true;
                            //}
                            #endregion

                            BMECObject ec_object_mainPort = shangyou_current_object.GetConnectedComponentAtPort(shangyou_current_object.Ports[0].ID);//查找上游管道
                            if (ec_object_mainPort != null && ec_object_mainPort.ClassName == PIPE && BMECApi.Instance.ObjectContained(ec_object_mainPort, pipes))//当上游连接组件是不为空的管道且在选择集中 TODO　父类是管道
                            {
                                
                                if (!BMECApi.Instance.ObjectContained(ec_object_mainPort, fenzu)&& shangyou_current_object.GetNthPort(0).Direction.IsParallelOrOppositeTo(ec_object_mainPort.GetNthPort(1).Direction))
                                {
                                    fenzu.Insert(0, ec_object_mainPort);//将上游管道添加到分组中
                                    shangyou_current_object = ec_object_mainPort;
                                }
                                else
                                {
                                    shangyou_guandao = true;
                                }
                            }
                            else
                            {
                                //建立没有上游管道的flag
                                shangyou_guandao = true;
                            }

                        }
                        if (!xiayou_guandao)//没有下游管道就不用再执行查找上游管道了
                        {
                            #region MyRegion

                            //BMECObject ec_object_runPort = fenzu.Last().GetConnectedComponentAtPort(fenzu.Last().Ports[1].ID);//查找下游管道
                            //if (ec_object_runPort != null && ec_object_runPort.ClassName == PIPE && this.myListContainsToBMECObject(pipes, ec_object_runPort))//当下游连接组件是不为空的管道且在选择集中
                            //{
                            //    BMECObject tempecobjectrunport = ec_object_runPort.GetConnectedComponentAtPort(ec_object_runPort.Ports[1].ID);
                            //    if (tempecobjectrunport != null && BMECApi.Instance.ObjectContained(tempecobjectrunport, fenzu/*new List<BMECObject>() { fenzu.Last() }*/))
                            //    {
                            //        xiayou_guandao = true;
                            //    }
                            //    else
                            //    {
                            //        fenzu.Add(ec_object_runPort);//将上游管道添加到分组中
                            //    }
                            //}
                            //else
                            //{
                            //    //建立没有上游管道的flag
                            //    xiayou_guandao = true;
                            //}
                            #endregion

                            BMECObject ec_object_runPort = xiayou_current_object.GetConnectedComponentAtPort(xiayou_current_object.Ports[1].ID);//查找下游管道
                            if (ec_object_runPort != null && ec_object_runPort.ClassName == PIPE && BMECApi.Instance.ObjectContained(ec_object_runPort, pipes))//当下游连接组件是不为空的管道且在选择集中
                            {
                                if (!BMECApi.Instance.ObjectContained(ec_object_runPort, fenzu)&& xiayou_current_object.GetNthPort(1).Direction.IsParallelOrOppositeTo(ec_object_runPort.GetNthPort(0).Direction))
                                {
                                    fenzu.Add(ec_object_runPort);//将上游管道添加到分组中
                                    xiayou_current_object = ec_object_runPort;
                                }
                                else
                                {
                                    xiayou_guandao = true;
                                }
                            }
                            else
                            {
                                xiayou_guandao = true;
                            }
                        }
                    }
                    if (fenzu != null && fenzu.Count > 0)
                    {
                        pipes_fenzu.Add(fenzu);//将一组相连的管道添加到容器中
                    }
                }
            }

            List<List<BMECObject>> xinfenzu = new List<List<BMECObject>>();
            foreach (var item in pipes_fenzu)
            {
                List<List<BMECObject>> tempfenzu = new List<List<BMECObject>>();
                tempfenzu = PipeLianjiexingshi(item);
                foreach (var item2 in tempfenzu)
                {
                    xinfenzu.Add(item2);
                }
            }
            pipes_fenzu = xinfenzu;
            return pipes_fenzu;
        }
        //public List<List<BMECObject>> getPipeConnect(List<BMECObject> pipes)
        //{
        //    if (null == pipes || pipes.Count < 1) return null;
        //    List<List<BMECObject>> pipes_fenzu = new List<List<BMECObject>>();//分组后的管道存储容器
        //    //分组，随便找一根管道，迭代找他的上游管道和下游管道
        //    for (int i = 0; i < pipes.Count; i++)
        //    {
        //        bool yifenzu = false;//管道是否分组标志
        //        foreach (var zu in pipes_fenzu)//如果分组容器中已经包含此管道，不在进行迭代寻找上下游管道
        //        {
        //            if (BMECApi.Instance.ObjectContained(pipes[i], zu)) yifenzu = true;
        //        }
        //        if (!yifenzu)//没包含在分组容器中，将寻找他的上下游管道
        //        {
        //            List<BMECObject> fenzu = new List<BMECObject>();//管道分组容器
        //            bool shangyou_guandao = false;//是否存在上游管道，不存在时置为true
        //            bool xiayou_guandao = false;//是否存在下游管道，不存在时置为true
        //            fenzu.Add(pipes[i]);//将选中管道容器中的第一个放到新分组中
        //            BMECObject shangyou_current_object = pipes[i];
        //            BMECObject xiayou_current_object = pipes[i];
        //            while (!(shangyou_guandao && xiayou_guandao))//寻找上游与下游管道直到找不到
        //            {
        //                if (!shangyou_guandao)//没有上游管道就不用再执行查找上游管道了
        //                {
        //                    BMECObject ec_object_mainPort = shangyou_current_object.GetConnectedComponentAtPort(shangyou_current_object.Ports[0].ID);//查找上游管道
        //                    if (ec_object_mainPort != null && ec_object_mainPort.ClassName == PIPE && BMECApi.Instance.ObjectContained(ec_object_mainPort, pipes))//当上游连接组件是不为空的管道且在选择集中 TODO　父类是管道
        //                    {
        //                        if (!BMECApi.Instance.ObjectContained(ec_object_mainPort, fenzu))
        //                        {
        //                            fenzu.Insert(0, ec_object_mainPort);//将上游管道添加到分组中
        //                            shangyou_current_object = ec_object_mainPort;
        //                        }
        //                        else
        //                        {
        //                            shangyou_guandao = true;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        //建立没有上游管道的flag
        //                        shangyou_guandao = true;
        //                    }
        //                }
        //                if (!xiayou_guandao)//没有下游管道就不用再执行查找上游管道了
        //                {
        //                    BMECObject ec_object_runPort = xiayou_current_object.GetConnectedComponentAtPort(xiayou_current_object.Ports[1].ID);//查找下游管道
        //                    if (ec_object_runPort != null && ec_object_runPort.ClassName == PIPE && BMECApi.Instance.ObjectContained(ec_object_runPort, pipes))//当下游连接组件是不为空的管道且在选择集中
        //                    {
        //                        if (!BMECApi.Instance.ObjectContained(ec_object_runPort, fenzu))
        //                        {
        //                            fenzu.Add(ec_object_runPort);//将上游管道添加到分组中
        //                            xiayou_current_object = ec_object_runPort;
        //                        }
        //                        else
        //                        {
        //                            xiayou_guandao = true;
        //                        }
        //                    }

        /// <summary>
        /// 判断pipes中是否包含pipe
        /// </summary>
        /// <param name="pipes">检测的容器</param>
        /// <param name="pipe">要检测的管道</param>
        /// <returns>pipes是否包含pipe，true：包含，false：不包含</returns>
        public static bool myListContainsToBMECObject(List<BMECObject> pipes, BMECObject pipe)
        {
            bool bRet = false;
            if (pipes == null || pipe == null)
            {
                return bRet;
            }
            foreach (var pipelist in pipes)
            {
                //只要InstanceId相等就认为是同一个Instance
                if (pipelist.Instance.InstanceId == pipe.Instance.InstanceId)
                {
                    bRet = true;
                }
            }
            return bRet;
        }
        /// <summary>
        /// 添加不重复的BMECObject到list中
        /// </summary>
        /// <param name="list"></param>
        /// <param name="ec_object"></param>
        public static void addNoRepeatECObejctToList(List<BMECObject> list, BMECObject ec_object)
        {
            if (list == null || ec_object == null) return;
            //遍历判断InstanceId是否相等，一旦有相等的就将跳出函数
            foreach (var ecobject in list)
            {
                if (ecobject.Instance.InstanceId == ec_object.Instance.InstanceId)
                {
                    return;
                }
            }
            //没有相等的instance时，将ec_object添加进list中
            list.Add(ec_object);
        }
        /// <summary>
        /// 给一组管道排序
        /// </summary>
        /// <param name="pipes">彼此相连的一组管道</param>
        /// <returns>按顺序排列的管道</returns>
        public static List<BMECObject> sortPipes(List<BMECObject> pipes)
        {
            if (pipes == null || pipes.Count < 0) return null;
            List<BMECObject> sortPipes = new List<BMECObject>();
            for(int i=0;i<pipes.Count;i++)
            {
                BMECObject bmec = pipes[i];
                BMECObject mainPortConnectPipe = bmec.GetConnectedComponentAtPort(bmec.Ports[MAIN_PORT_ID].ID);
                if (null != mainPortConnectPipe && mainPortConnectPipe.ClassName == PIPE && BMECApi.Instance.ObjectContained(mainPortConnectPipe, pipes)/*this.myListContainsToBMECObject(pipes, mainPortConnectPipe)*/)//当连接的管道在pipes中，则将其添加到sortPipes中
                {
                    //sortPipes.Insert(0, mainPortConnectPipe);
                }
                else
                {
                    sortPipes.Add(bmec);
                    break;
                }
            }
            bool isz = false;
            while(!isz)
            {
                BMECObject bmec = sortPipes[sortPipes.Count-1];
                BMECObject runPortConnectPipe = bmec.GetConnectedComponentAtPort(bmec.Ports[RUN_PORT_ID].ID);
                if (null != runPortConnectPipe && runPortConnectPipe.ClassName == PIPE && BMECApi.Instance.ObjectContained(runPortConnectPipe, pipes)/*this.myListContainsToBMECObject(pipes, runPortConnectPipe)*/)//当连接的管道在pipes中，则将其添加到sortPipes中
                {
                    sortPipes.Add(runPortConnectPipe);
                }
                else
                {
                    isz = true;
                }
            }
            //排序，正常的管道连接是上一个管道的runPort连接下一个管道的mainPort，所以依次找这样的一对管道就行。
            //sortPipes.Add(pipes.First());//先随便取一根管道添加到排序管道中
            //while (sortPipes.Count < pipes.Count)
            //{
            //    //找头元素的mainPort连接的管道
            //    BMECObject firstPipe = sortPipes.First();
            //    BMECObject mainPortConnectPipe = firstPipe.GetConnectedComponentAtPort(firstPipe.Ports[MAIN_PORT_ID].ID);
            //    if (null != mainPortConnectPipe && mainPortConnectPipe.ClassName == PIPE && BMECApi.Instance.ObjectContained(mainPortConnectPipe, pipes)/*this.myListContainsToBMECObject(pipes, mainPortConnectPipe)*/)//当连接的管道在pipes中，则将其添加到sortPipes中
            //    {
            //        sortPipes.Insert(0, mainPortConnectPipe);
            //    }
            //    //找尾元素的runPort连接的管道
            //    BMECObject lastPipe = sortPipes.Last();
            //    BMECObject runPortConnectPipe = lastPipe.GetConnectedComponentAtPort(lastPipe.Ports[RUN_PORT_ID].ID);
            //    if (null != runPortConnectPipe && runPortConnectPipe.ClassName == PIPE && BMECApi.Instance.ObjectContained(runPortConnectPipe, pipes)/*this.myListContainsToBMECObject(pipes, runPortConnectPipe)*/)//当连接的管道在pipes中，则将其添加到sortPipes中
            //    {
            //        sortPipes.Add(runPortConnectPipe);
            //    }
            //    break;
            //}
            return sortPipes;
        }
        /// <summary>
        /// 根据不同连接形式分组
        /// </summary>
        /// <returns></returns>
        public static List<List<BMECObject>> PipeLianjiexingshi(List<BMECObject> yifenzu) {
            List<List<BMECObject>> xinfenzu = new List<List<BMECObject>>();
            string defaultEndPreparation = yifenzu[0].Ports[0].GetStringValue("END_PREPARATION") + "-" + yifenzu[0].Ports[1].GetStringValue("END_PREPARATION");
            xinfenzu.Add(new List<BMECObject>());
            foreach (BMECObject item in yifenzu)
            {
                string currentEndPreparation = item.Ports[0].GetStringValue("END_PREPARATION") + "-" + item.Ports[1].GetStringValue("END_PREPARATION");
                if (defaultEndPreparation.Equals(currentEndPreparation))//连接形式没变
                {
                    xinfenzu.Last().Add(item);
                }
                else//中途改变了连接形式
                {
                    xinfenzu.Add(new List<BMECObject>());
                    xinfenzu.Last().Add(item);
                    defaultEndPreparation = currentEndPreparation;
                }
            }
            return xinfenzu;
        }
        /// <summary>
        /// 获取管件的连接形式
        /// </summary>
        /// <param name="pipe"></param>
        /// <returns></returns>
        public static string[] GetEndPreparations(BMECObject pipe) {
            string[] endPreparations = new string[pipe.Ports.Count];
            for (int i = 0; i < pipe.Ports.Count; i++)
            {
                endPreparations[i] = pipe.Ports[i].GetStringValue("END_PREPARATION");
            }
            return endPreparations;
        }
        /*
         * 1.”断开管道并添加焊点“命令需求用例

用户选中需要断开的一根圆管（支持按管线编号选中）。
用户单击该命令按钮。
程序筛选出管子。
用户在弹出对话框Dialog1-2中输入预制长度，起始方向（优先从左往右），点击确认按钮。对话框中应记录上一次输入预制长度。
程序将管道按预制长度打断，并插入焊点，更新连接性。


2.”批量删除焊点并合并管道“命令需求用例

用户选中需要合并的所有管道。（先按点选和框选开发，后面结合一键选中管子的功能综合考虑）
用户单击该命令按钮。
程序计算所有合并管道总长。
程序删除第二根及第二根后面连接的管道。
程序删除所有焊点。
程序将第一根管道延长至合并管道总长。
程序更新连接性。

         */
    }
}
