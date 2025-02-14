﻿
using Bentley.Building.Mechanical.Components;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.EC.Persistence;
using Bentley.ECObjects.Instance;
using Bentley.ECObjects.Schema;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.Plant.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    /// <summary>
    /// 多余图形清理
    /// </summary>
    public class ElementClear
    {
        /*更新当前 Model 中元素的连结性*/
        /// <summary>
        /// 更新所有 ECInstance 的连结性
        /// </summary>
        /// <returns></returns>
        public static void updateConnectionOfAllECInstance()
        {
            ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            List<Element> elementlist = new List<Element>();
            foreach (var element in elements)//将扫描到的 Element 存放到 list 中
            {
                elementlist.Add(element);
            }
            List<BMECObject> ecObjectList = new List<BMECObject>();//所有图形的 ECInstance 容器

            foreach (var element in elementlist)
            {
                //首先需要是 ECInstance
                BMECObject ec_object = null;
                ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
                if (ec_object != null && ec_object.Instance != null)
                {
                    ecObjectList.Add(ec_object);
                }
            }
            BMECApi api = BMECApi.Instance;
            foreach (var ec_object in ecObjectList)
            {
                if (ec_object.Ports != null)
                {
                    if (ec_object.ConnectedComponents != null && ec_object.ConnectedComponents.Count != 0)
                    {
                        ec_object.DiscoverConnectionsEx();
                        ec_object.UpdateConnections();
                    }
                }
            }
        }
        public static void updateConnectionOfAllECInstance(long elementId)
        {
            BMECObject ecObject = null;
            ecObject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(Convert.ToUInt64(elementId));
            if (ecObject != null && ecObject.Instance != null)
            {
                //ecObjectList.Add(ecObject);
                BMECApi api = BMECApi.Instance;
                if (api.InstanceDefinedAsClass(ecObject.Instance, "PIPING_COMPONENT", true))
                {
                    ecObject.DiscoverConnectionsEx();
                    //api.DoSettingsForFastenerUtility(ecObject, ecObject.Ports[0]);
                    //api.DoSettingsForFastenerUtility(ecObject, ecObject.Ports[1]);
                    ecObject.UpdateConnections();
                    ecObject.Create();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("该元素不具备连接性更新功能，请选择管道");
                }
            }
        }

        /*物理上孤立*/
        /// <summary>
        /// 获取物理距离上孤立的元素
        /// 从元素的原点算距离是不合理的，当元素本身过大时，显然不合理，初步想法是将每个元素当成一个球体，算距离时，加上本身的半径，但并不是每个元素的原点都在其中心
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public List<Element> filterElementByDistance(double distance)
        {
            ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            List<Element> elementlist = new List<Element>();
            //foreach (var element in elements)//将扫描到的 Element 存放到 list 中
            //{
            //    elementlist.Add(element);
            //}
            elementlist = elements.ToList();
            List<Element> separateElements = new List<Element>();//距离上孤立元素的容器
            separateElements = this.getSeparateElements(elementlist, distance);//获取距离上孤立的元素
            return separateElements;
        }

        public List<IECInstance> filterInstanceByDistance(double distance)
        {
            //ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            ECInstanceList allInstanceFromDgn = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetAllInstancesFromDgn();
            List<Element> elementlist = new List<Element>();
            foreach(var instance in allInstanceFromDgn)
            {
                Element ele = JYX_ZYJC_CLR.PublicMethod.get_element_by_instance(instance);
                elementlist.Add(ele);
            }
            //foreach (var element in elements)//将扫描到的 Element 存放到 list 中
            //{
            //    elementlist.Add(element);
            //}
            List<Element> separateElements = new List<Element>();//距离上孤立元素的容器
            separateElements = this.getSeparateElements(elementlist, distance);//获取距离上孤立的元素

            List<IECInstance> iecList = new List<IECInstance>();
            foreach(var ee in separateElements)
            {
                IECInstance iec = JYX_ZYJC_CLR.PublicMethod.FindInstance(ee);
                iecList.Add(iec);
            }

            return iecList;
        }
        /// <summary>
        /// 获取大范围内没有其它元素的元素
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public List<Element> getSeparateElements(List<Element> elements, double radius)
        {
            double uor_per_master = Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;//当前设计文件的主单位
            List<Element> list = new List<Element>();//孤立元素容器
            List<DPoint3d> locations = new List<DPoint3d>();//孤立元素位置
            double bijiao_radius = 0.0;//两个元素的自身半径加上孤立判断距离
            double radius1 = 0.0;//元素一的自身半径
            double radius2 = 0.0;//元素二的自身半径
            DPoint3d point1 = new DPoint3d();//元素一的中心点
            DPoint3d point2 = new DPoint3d();//元素二的中心点
            double distance = 0.0;//两个元素中心点间距离
            for (int i = 0; i < elements.Count; i++)//TODO 这个比较方法的时间复杂度太高了，希望换一种方法
            {
                bool flag = true;
                for (int j = 0; j < elements.Count; j++)
                {
                    if (i == j) continue;
                    this.getElementCenterPointAndRadius(elements[i], out point1, out radius1);
                    this.getElementCenterPointAndRadius(elements[j], out point2, out radius2);
                    bijiao_radius = radius * uor_per_master + radius1 + radius2;
                    distance = point1.Distance(point2);
                    if (distance <= bijiao_radius)
                    {
                        flag = false;
                        break;
                    }

                }
                if (flag)
                {
                    list.Add(elements[i]);
                }
            }

            return list;
        }
        /// <summary>
        /// 获取元素的中心点(通过range.high - range.low)以及最大半径(XSize、YSize、ZSize)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="centerPoint"></param>
        /// <param name="radius"></param>
        public void getElementCenterPointAndRadius(Element element, out DPoint3d centerPoint, out double radius)
        {
            DRange3d range;
            ((DisplayableElement)element).CalcElementRange(out range);
            DPoint3d duijiaoxian = range.High - range.Low;
            centerPoint = DPoint3d.Add(range.Low, duijiaoxian, 0.5);
            radius = Math.Max(Math.Max(range.XSize, range.YSize), range.ZSize) / 2;
        }

        /*连接性上孤立*/
        /// <summary>
        /// 获取连结性上孤立的元素
        /// </summary>
        /// <returns></returns>
        public unsafe List<Element> filterElementByConnection()
        {
            BMECApi api = BMECApi.Instance;

            List<Element> separateElements = new List<Element>();//连结性上孤立元素的容器
            ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            List<BMECObject> elementlist = new List<BMECObject>();
            List<Element> solidElement = new List<Element>();//不具有 EC 属性的智能实体
            foreach (var element in elements)
            {
                BMECObject ec_object = null;
                ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
                if (ec_object != null && ec_object.Instance != null)//是否为 ECInstance
                {
                    #region demo

                    //ECInstanceList allRelatedInstance = api.GetAllRelatedInstance(ec_object.Instance);
                    //IECRelationshipInstanceCollection allRelationshipInstance = api.GetAllRelationshipInstances(ec_object.Instance);
                    //foreach (IECRelationshipInstance current in allRelationshipInstance)
                    //{
                    //    IECInstance tempIECInstance = api.FindAllInformationOnInstance(current);
                    //    if (current.ClassDefinition.Name.Equals("JOINT_HAS_FASTENER"))
                    //    {

                    //    }
                    //    else if (current.ClassDefinition.Name.Equals("JOINT_HAS_SEAL"))
                    //    {

                    //    }
                    //}
                    //ECInstanceList allJointInstance = api.GetAllJointInstances(ec_object.Instance);
                    #endregion

                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Point3d StartPoint = new Bentley.Interop.MicroStationDGN.Point3d();
                    Bentley.Interop.MicroStationDGN.Point3d EndPoint = new Bentley.Interop.MicroStationDGN.Point3d();
                    app.CreateLineElement2(null, ref StartPoint, ref EndPoint);
                    if (api.InstanceDefinedAsClass(ec_object.Instance, "PIPING_COMPONENT", true))//管件
                    {
                        if (ec_object.Ports != null && ec_object.Ports.Count > 0)//有port
                        {
                            //先检查是否有连接组件，没有的直接添加到结果中
                            if (ec_object.ConnectedComponents != null && ec_object.ConnectedComponents.Count > 0)
                            {
                                //foreach (Port port in ec_object.Ports)//不知道为什么 Port 不能用 foreach 遍历
                                for (int i = 0; i < ec_object.Ports.Count; i++)
                                {
                                    IECInstance portInstance = ec_object.Ports[i].Instance;
                                    #region test

                                    //EndPrepTypeInfo endPrepTypeInfo = api.GetEndPrepTypeInfo(port);
                                    //IECInstance connectedPortInstance = api.GetConnectedPortInstance(portInstance);
                                    //IECInstance iECInstance = api.FindAllInformationOnInstance(connectedPortInstance);
                                    //IECInstance getJointInstance = api.GetJointInstance(port);

                                    //if (null != iECInstance)
                                    //{
                                    //    foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                                    //    {
                                    //        if (current.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current.Source.InstanceId == iECInstance.InstanceId)
                                    //        {
                                    //        }
                                    //    }
                                    //}
                                    #endregion

                                    IECInstance allInformationPortInstance = api.FindAllInformationOnInstance(portInstance);

                                    double locationX = allInformationPortInstance["LOCATION_X"].DoubleValue;
                                    double locationY = allInformationPortInstance["LOCATION_X"].DoubleValue;

                                    string jointTypeName = "";

                                    if (null != allInformationPortInstance)
                                    {
                                        foreach (IECRelationshipInstance current2 in allInformationPortInstance.GetRelationshipInstances())
                                        {
                                            //port 的连接性
                                            if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == allInformationPortInstance.InstanceId)//找到port的相关组件
                                            {
                                                List<IECInstance> fastenerInstance = new List<IECInstance>();
                                                //System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByStrength(current2.Target, (StrengthType)(-1)).GetEnumerator();
                                                System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByDirection(current2.Target, true).GetEnumerator();
                                                while (enumerator3.MoveNext())
                                                {
                                                    IECInstance current3 = enumerator3.Current;
                                                    fastenerInstance.Add(current3);
                                                }
                                                IECInstance targetJoint = current2.Target;
                                                List<string> fastenerName = new List<string>();
                                                if (targetJoint != null)
                                                {
                                                    jointTypeName = targetJoint["TYPE"].StringValue;
                                                    IECPropertyValue myJointType = FindJointTypeByJointName(jointTypeName);
                                                    GetJointTypeInfo(myJointType, out fastenerName);
                                                }
                                                if (fastenerName.Count == fastenerInstance.Count)
                                                {
                                                    bool flag2 = false;//组件数量及名称是否正确
                                                    //需要的组件数量相等，判断是否是对应组件
                                                    foreach (var currentFastenerInstance in fastenerInstance)
                                                    {
                                                        bool flag = false;//组件名称是否正确
                                                        foreach (var name in fastenerName)
                                                        {
                                                            if (name.Contains(currentFastenerInstance.ClassDefinition.Name))
                                                            {
                                                                flag = true;
                                                                break;
                                                            }
                                                        }
                                                        if (!flag)
                                                        {
                                                            //组件缺失
                                                            flag2 = false;
                                                            separateElements.Add(element);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            flag2 = true;
                                                        }
                                                    }
                                                    //TODO 组件正确，检查各组件位置是否正确
                                                    if (flag2)
                                                    {
                                                        foreach (var currentFastenerInstance in fastenerInstance)
                                                        {
                                                            BMECObject fastenerObject = new BMECObject(currentFastenerInstance);
                                                            DPoint3d fastenerLocation = fastenerObject.Transform3d.Translation;
                                                            DPoint3d pipeLocation = ec_object.Transform3d.Translation;
                                                            DVector3d distance = new DVector3d(fastenerLocation, pipeLocation);
                                                            //int temp = 1;
                                                            //double od = ec_object.GetDoubleValueInMM("OUTSIDE_DIAMETER") * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                                                            //double tolerence = 100;//
                                                            //if (distance.Magnitude - od > tolerence)
                                                            //{
                                                            //    //超过容差值，判定为位置异常
                                                            //    separateElements.Add(element);
                                                            //}
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //组件缺失
                                                    separateElements.Add(element);
                                                }
                                            }
                                        }
                                    }
                                    #region test

                                    //if (getJointInstance != null)
                                    //{
                                    //    jointTypeName = getJointInstance["TYPE"].StringValue;
                                    //    FindJointTypeByJointName(jointTypeName);
                                    //}

                                    //IECInstance[] customAttributesInstances = getJointInstance.ClassDefinition.GetCustomAttributes();
                                    //int temp = 1;
                                    //Bentley.ECObjects.Schema.IECCustomAttributeContainer[] temp2 = getJointInstance.ClassDefinition.GetBaseContainers();
                                    //IList<IECInstance> temp3= getJointInstance.ClassDefinition.GetLocalCustomAttributes();
                                    //IECInstance[] temp4 = getJointInstance.ClassDefinition.GetPrimaryCustomAttributes();
                                    //IList<IECInstance> temp5= getJointInstance.ClassDefinition.GetLocalPrimaryCustomAttributes();
                                    //temp = 2;
                                    //IECPropertyValue jointTypeProperty = null;
                                    //foreach (var item in customAttributesInstances)
                                    //{
                                    //    if (item.ClassDefinition.Name.Equals("OpenPlant_3D_JointTypeProperties_Map"))
                                    //    {
                                    //        foreach (var item2 in item)
                                    //        {
                                    //            if (item2.AccessString.Equals("JOINT_TYPE"))
                                    //            {
                                    //                foreach (var item3 in item2.ContainedValues)
                                    //                {
                                    //                    try
                                    //                    {
                                    //                        foreach (var item4 in item3.ContainedValues)
                                    //                        {
                                    //                            if (item4.AccessString.Contains("JOINT_NAME"))
                                    //                            {
                                    //                                string jointType = item4.StringValue;
                                    //                                if (jointType.Equals(jointTypeName))
                                    //                                {
                                    //                                    //找到了该连接形式
                                    //                                    jointTypeProperty = item3;
                                    //                                }
                                    //                            }
                                    //                        }
                                    //                    }
                                    //                    catch (Exception)
                                    //                    {

                                    //                        throw;
                                    //                    }
                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    //}

                                    //IECInstance iECInstance2 = api.FindAllInformationOnInstance(portInstance);
                                    //if (null != iECInstance2)
                                    //{
                                    //    foreach (IECRelationshipInstance current2 in iECInstance2.GetRelationshipInstances())
                                    //    {
                                    //        if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == iECInstance2.InstanceId)//找到port的相关组件
                                    //        {
                                    //            System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstances(current2.Target, null, true).GetEnumerator();
                                    //            while (enumerator3.MoveNext())
                                    //            {
                                    //                IECInstance current3 = enumerator3.Current;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                }
                                //valve has operater
                                IECInstance ecInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                                if (api.InstanceDefinedAsClass(ecInstance, "VALVE", true))
                                {
                                    string operatorDeviceName = "";
                                    try
                                    {
                                        operatorDeviceName = ecInstance["OPERATOR"].StringValue;
                                    }
                                    catch (Exception e)
                                    {
                                        string estr = e.Message;
                                    }
                                    bool flag = false;//是否有执行机构关系
                                    if (operatorDeviceName != "")//有执行机构就必须有对应的关系，如果没有就证明组件缺失
                                    {
                                        IECInstance operatorDeviceInstance = null;
                                        foreach (IECRelationshipInstance item in ecInstance.GetRelationshipInstances())
                                        {
                                            if (item.ClassDefinition.Name.Equals("VALVE_HAS_VALVE_OPERATING_DEVICE") && item.Source.InstanceId == ecInstance.InstanceId)
                                            {
                                                flag = true;
                                                operatorDeviceInstance = item.Target;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            //缺失执行机构
                                            separateElements.Add(element);
                                        }
                                        else
                                        {
                                            //判断执行机构位置是否正确
                                            BMECObject operatorDeviceObject = new BMECObject(operatorDeviceInstance);
                                            DPoint3d operatorDeviceLocation = operatorDeviceObject.Transform3d.Translation;
                                            DPoint3d valveLocation = ec_object.Transform3d.Translation;
                                            DVector3d distance = new DVector3d(operatorDeviceLocation, valveLocation);
                                            double od = ec_object.GetDoubleValueInMM("OUTSIDE_DIAMETER") * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                                            double tolerence = 100;//
                                            if (distance.Magnitude - od > tolerence)
                                            {
                                                //超过容差值，判定为位置异常
                                                separateElements.Add(element);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                separateElements.Add(element);//没有连接性
                            }
                        }
                        else//没port
                        {
                            //TODO 处理方式
                            separateElements.Add(element);
                        }
                    }
                    else//非 PIPING_COMPONENT，相关的有 FASTENER、SEAL、VALVE_OPERATING_DEVICE
                    {
                        #region demo
                        bool hasJoint = false;//是否存在 Joint
                        if (api.InstanceDefinedAsClass(ec_object.Instance, "FASTENER", true))//FASTENER 的 relationship 有 JOINT_HAS_FASTENER(N,N)，有 JOINT 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("JOINT_HAS_FASTENER") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 Joint
                                        IECInstance targetJoint = current2.Source;
                                        string port1GUID = targetJoint["PORT1_GUID"].StringValue;
                                        string port2GUID = targetJoint["PORT2_GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid("PORT", true);
                                        if (hashTablePortInstance.ContainsKey(port1GUID) && hashTablePortInstance.ContainsKey(port2GUID))
                                        {
                                            //依附的连接组件存在，判断位置是否正确
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            foreach (var item in targetJoint.GetRelationshipInstances())
                                            {
                                                if (item.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && item.Target.InstanceId == targetJoint.InstanceId)//拿到对应的所有 Port
                                                {
                                                    portsInstance.Add(item.Source);
                                                }
                                            }
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;
                                            List<IECInstance> componentsInstance = new List<IECInstance>();//port 连接的 component
                                            if (portsInstance.Count > 0)
                                            {
                                                foreach (IECInstance item in portsInstance)
                                                {
                                                    foreach (var item2 in item.GetRelationshipInstances())
                                                    {
                                                        if (item2.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item2.Target.InstanceId == item.InstanceId)
                                                        {
                                                            componentsInstance.Add(item2.Source);
                                                        }
                                                    }
                                                }
                                            }
                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> componentsInstanceId = new List<string>();
                                                foreach (var item in componentsInstance)
                                                {
                                                    componentsInstanceId.Add(item.InstanceId);
                                                }
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                foreach (var item in scanComponentsInstanceId)
                                                {
                                                    if (!componentsInstanceId.Contains(item))
                                                    {
                                                        //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                        separateElements.Add(element);
                                                    }
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                separateElements.Add(element);
                                            }

                                            #region demo

                                            //Port finPortAtPoint1 = ec_object.FindPortAtPoint((Bentley.DPoint3d*)pPoint);
                                            //List<Port> finPort2 = ec_object.FindAllClosestPorts((Bentley.DPoint3d*)pPoint);
                                            //temp = 1;
                                            //foreach (IECInstance item in componentsInstance)
                                            //{
                                            //    BMECObject componentObject = new BMECObject(item);
                                            //    PortData portData1 = new PortData(item, componentObject.Ports[0].Instance);
                                            //    temp = 1;
                                            //    List<Port> findPort3 = componentObject.FindAllClosestPorts((Bentley.DPoint3d*)pPoint);
                                            //    temp = 1;
                                            //    Port finPort4 = componentObject.FindPortAtPoint((Bentley.DPoint3d*)pPoint);
                                            //    temp = 1;
                                            //    DPoint3d port1Location = componentObject.GetNthPort(0).LocationInUors;
                                            //    temp = 1;
                                            //    Port port1 = componentObject.GetNthPort(0);
                                            //    temp = 1;
                                            //    BMECObject connectedComponent = componentObject.GetConnectedComponentAtPort(1);
                                            //    temp = 1;
                                            //    BMECObject connectedComponent2 = componentObject.GetConnectedComponentAtPort(2);
                                            //    temp = 1;
                                            //    IECInstance allInfomationComponent = api.FindAllInformationOnInstance(item);
                                            //    BMECObject componentObject1 = new BMECObject(allInfomationComponent);
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;

                                            //}
                                            #endregion

                                            #region demo

                                            //IECInstance allInfoInstance1 = api.FindAllInformationOnInstance(hashTablePortInstance[port1GUID] as IECInstance);
                                            //if (null != allInfoInstance1)
                                            //{
                                            //    foreach (IECRelationshipInstance item in allInfoInstance1.GetRelationshipInstances())
                                            //    {
                                            //        if (item.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item.Target.InstanceId == allInfoInstance1.InstanceId)
                                            //        {
                                            //            IECInstance pipeComponent = item.Source;
                                            //            BMECObject newBMECObject = null;
                                            //            if (pipeComponent != null)
                                            //            {
                                            //                newBMECObject = new BMECObject(pipeComponent);
                                            //                int temp = 1;
                                            //                List<Port> componentPorts = newBMECObject.Ports;
                                            //                foreach (Port componentPort in componentPorts)
                                            //                {
                                            //                    string componentPortInstanceId = componentPort.Instance.InstanceId;
                                            //                }
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            //IECInstance allInfoInstance2 = api.FindAllInformationOnInstance(hashTablePortInstance[port1GUID] as IECInstance);
                                            //if (null != allInfoInstance2)
                                            //{
                                            //    foreach (IECRelationshipInstance item in allInfoInstance2.GetRelationshipInstances())
                                            //    {
                                            //        if (item.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item.Target.InstanceId == allInfoInstance2.InstanceId)
                                            //        {
                                            //            IECInstance pipeComponent = item.Source;
                                            //            BMECObject newBMECObject = null;
                                            //            if (pipeComponent != null)
                                            //            {
                                            //                newBMECObject = new BMECObject(pipeComponent);
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            #endregion
                                        }
                                        else if (port1GUID == "" || port2GUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else if (api.InstanceDefinedAsClass(ec_object.Instance, "SEAL", true))//SEAL 的 relationship 有 JOINT_HAS_SEAL(1,N)，有 JOINT 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("JOINT_HAS_SEAL") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 Joint
                                        IECInstance targetJoint = current2.Source;
                                        string port1GUID = targetJoint["PORT1_GUID"].StringValue;
                                        string port2GUID = targetJoint["PORT2_GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid("PORT", true);
                                        if (hashTablePortInstance.ContainsKey(port1GUID) && hashTablePortInstance.ContainsKey(port2GUID))
                                        {
                                            //依附的连接组件存在，判断物理位置
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            foreach (var item in targetJoint.GetRelationshipInstances())
                                            {
                                                if (item.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && item.Target.InstanceId == targetJoint.InstanceId)//拿到对应的所有 Port
                                                {
                                                    portsInstance.Add(item.Source);
                                                }
                                            }
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;
                                            List<IECInstance> componentsInstance = new List<IECInstance>();//port 连接的 component
                                            if (portsInstance.Count > 0)
                                            {
                                                foreach (IECInstance item in portsInstance)
                                                {
                                                    foreach (var item2 in item.GetRelationshipInstances())
                                                    {
                                                        if (item2.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item2.Target.InstanceId == item.InstanceId)
                                                        {
                                                            componentsInstance.Add(item2.Source);
                                                        }
                                                    }
                                                }
                                            }


                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> componentsInstanceId = new List<string>();
                                                foreach (var item in componentsInstance)
                                                {
                                                    componentsInstanceId.Add(item.InstanceId);
                                                }
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                foreach (var item in scanComponentsInstanceId)
                                                {
                                                    if (!componentsInstanceId.Contains(item))
                                                    {
                                                        //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                        separateElements.Add(element);
                                                    }
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                //LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(),null, new DSegment3d(jujianLocation, DPoint3d.Zero));
                                                //line.AddToModel();
                                                //separateElements.Add(element);
                                            }
                                        }
                                        else if (port1GUID == "" || port2GUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else if (api.InstanceDefinedAsClass(ec_object.Instance, "VALVE_OPERATING_DEVICE", true))//VALVE_OPERATING_DEVICE 的 relationship 有 VALVE_HAS_VALVE_OPERATING_DEVICE(1,1)，有 VALVE 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("VALVE_HAS_VALVE_OPERATING_DEVICE") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 valve
                                        IECInstance targetJoint = current2.Source;
                                        string componentGUID = targetJoint["GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid(targetJoint.ClassDefinition.Name, true);
                                        if (hashTablePortInstance.ContainsKey(componentGUID))
                                        {
                                            //依附的连接组件存在，判断物理位置
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;

                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                if (!scanComponentsInstanceId.Contains(targetJoint.InstanceId))
                                                {
                                                    //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                    separateElements.Add(element);
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                separateElements.Add(element);
                                            }
                                        }
                                        else if (componentGUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else//预期之外的组件
                        {
                            //TODO 处理方式
                        }
                        #endregion
                    }
                    #region demo

                    //start test
                    //if (ec_object.ClassName == "WELD_NECK_FLANGE")
                    //{
                    //    IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                    //    IECRelationshipInstanceCollection iECRelationshipInstance = iECInstance.GetRelationshipInstances();
                    //    foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                    //    {
                    //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(current);
                    //        IECRelationshipInstanceCollection iECRelationshipInstance2 = iECInstance2.GetRelationshipInstances();
                    //        foreach (IECRelationshipInstance current2 in iECRelationshipInstance2)
                    //        {

                    //        }
                    //    }
                    //}
                    //if (ec_object.Ports != null && ec_object.Ports.Count > 0)
                    //{
                    //    foreach (var port in ec_object.Ports)
                    //    {
                    //        ChangeSet changeSet = new ChangeSet();
                    //        IECInstance portInstance = port.Instance;
                    //        IECInstance connectedPortInstance = api.GetConnectedPortInstance(portInstance);
                    //        IECInstance iECInstance = api.FindAllInformationOnInstance(connectedPortInstance);
                    //        if (null != iECInstance)
                    //        {
                    //            foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                    //            {
                    //                if (current.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current.Source.InstanceId == iECInstance.InstanceId)
                    //                {
                    //                    changeSet.Add(current);


                    //                }
                    //            }
                    //        }
                    //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(portInstance);
                    //        if (null != iECInstance2)
                    //        {
                    //            foreach (IECRelationshipInstance current2 in iECInstance2.GetRelationshipInstances())
                    //            {
                    //                if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == iECInstance2.InstanceId)
                    //                {
                    //                    System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstances(current2.Target, null, true).GetEnumerator();
                    //                    while (enumerator3.MoveNext())
                    //                    {
                    //                        IECInstance current3 = enumerator3.Current;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //end
                    #endregion

                    #region 原来的判断逻辑

                    //if (ec_object.Ports != null && ec_object.Ports.Count != 0)//判断 ECInstance 是否有具连结性的可能
                    //{
                    //    if (ec_object.ConnectedComponents != null && ec_object.ConnectedComponents.Count == 0)// ECInstance 不具有连接元素
                    //    {
                    //        separateElements.Add(element);
                    //        elementlist.Add(ec_object);
                    //    }
                    //}
                    #endregion
                }
                else//为普通元素，普通元素全部没有连结性
                {
                    separateElements.Add(element);
                }
            }
            return separateElements;
        }

        /*连接性上孤立*/
        /// <summary>
        /// 获取连结性上孤立的元素
        /// </summary>
        /// <returns></returns>
        public unsafe List<IECInstance> filterInstanceByConnection()
        {
            BMECApi api = BMECApi.Instance;

            List<IECInstance> separateElements = new List<IECInstance>();//连结性上孤立元素的容器
            //ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素

            ECInstanceList allInstanceFromDgn = Bentley.OpenPlantModeler.SDK.Utilities.DgnUtilities.GetAllInstancesFromDgn();

            List<BMECObject> elementlist = new List<BMECObject>();
            List<IECInstance> solidElement = new List<IECInstance>();//不具有 EC 属性的智能实体
            foreach (var element in allInstanceFromDgn)
            {
                BMECObject ec_object = null;
                //ec_object = new BMECObject(element);
                ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(JYX_ZYJC_CLR.PublicMethod.get_element_id_by_instance(element));
                if (ec_object != null && ec_object.Instance != null)//是否为 ECInstance
                {
                    Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
                    Bentley.Interop.MicroStationDGN.Point3d StartPoint = new Bentley.Interop.MicroStationDGN.Point3d();
                    Bentley.Interop.MicroStationDGN.Point3d EndPoint = new Bentley.Interop.MicroStationDGN.Point3d();
                    app.CreateLineElement2(null, ref StartPoint, ref EndPoint);
                    if (api.InstanceDefinedAsClass(ec_object.Instance, "PIPING_COMPONENT", true))//管件
                    {
                        if (ec_object.Ports != null && ec_object.Ports.Count > 0)//有port
                        {
                            //先检查是否有连接组件，没有的直接添加到结果中
                            if (ec_object.ConnectedComponents != null && ec_object.ConnectedComponents.Count > 0)
                            {
                                //foreach (Port port in ec_object.Ports)//不知道为什么 Port 不能用 foreach 遍历
                                for (int i = 0; i < ec_object.Ports.Count; i++)
                                {
                                    IECInstance portInstance = ec_object.Ports[i].Instance;
                                    #region test

                                    //EndPrepTypeInfo endPrepTypeInfo = api.GetEndPrepTypeInfo(port);
                                    //IECInstance connectedPortInstance = api.GetConnectedPortInstance(portInstance);
                                    //IECInstance iECInstance = api.FindAllInformationOnInstance(connectedPortInstance);
                                    //IECInstance getJointInstance = api.GetJointInstance(port);

                                    //if (null != iECInstance)
                                    //{
                                    //    foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                                    //    {
                                    //        if (current.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current.Source.InstanceId == iECInstance.InstanceId)
                                    //        {
                                    //        }
                                    //    }
                                    //}
                                    #endregion

                                    IECInstance allInformationPortInstance = api.FindAllInformationOnInstance(portInstance);

                                    double locationX = allInformationPortInstance["LOCATION_X"].DoubleValue;
                                    double locationY = allInformationPortInstance["LOCATION_X"].DoubleValue;

                                    string jointTypeName = "";

                                    if (null != allInformationPortInstance)
                                    {
                                        foreach (IECRelationshipInstance current2 in allInformationPortInstance.GetRelationshipInstances())
                                        {
                                            //port 的连接性
                                            if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == allInformationPortInstance.InstanceId)//找到port的相关组件
                                            {
                                                List<IECInstance> fastenerInstance = new List<IECInstance>();
                                                //System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByStrength(current2.Target, (StrengthType)(-1)).GetEnumerator();
                                                System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByDirection(current2.Target, true).GetEnumerator();
                                                while (enumerator3.MoveNext())
                                                {
                                                    IECInstance current3 = enumerator3.Current;
                                                    fastenerInstance.Add(current3);
                                                }
                                                IECInstance targetJoint = current2.Target;
                                                List<string> fastenerName = new List<string>();
                                                if (targetJoint != null)
                                                {
                                                    jointTypeName = targetJoint["TYPE"].StringValue;
                                                    IECPropertyValue myJointType = FindJointTypeByJointName(jointTypeName);
                                                    GetJointTypeInfo(myJointType, out fastenerName);
                                                }
                                                if (fastenerName.Count == fastenerInstance.Count)
                                                {
                                                    bool flag2 = false;//组件数量及名称是否正确
                                                    //需要的组件数量相等，判断是否是对应组件
                                                    foreach (var currentFastenerInstance in fastenerInstance)
                                                    {
                                                        bool flag = false;//组件名称是否正确
                                                        foreach (var name in fastenerName)
                                                        {
                                                            if (name.Contains(currentFastenerInstance.ClassDefinition.Name))
                                                            {
                                                                flag = true;
                                                                break;
                                                            }
                                                        }
                                                        if (!flag)
                                                        {
                                                            //组件缺失
                                                            flag2 = false;
                                                            separateElements.Add(element);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            flag2 = true;
                                                        }
                                                    }
                                                    //TODO 组件正确，检查各组件位置是否正确
                                                    if (flag2)
                                                    {
                                                        foreach (var currentFastenerInstance in fastenerInstance)
                                                        {
                                                            BMECObject fastenerObject = new BMECObject(currentFastenerInstance);
                                                            DPoint3d fastenerLocation = fastenerObject.Transform3d.Translation;
                                                            DPoint3d pipeLocation = ec_object.Transform3d.Translation;
                                                            DVector3d distance = new DVector3d(fastenerLocation, pipeLocation);
                                                            //int temp = 1;
                                                            //double od = ec_object.GetDoubleValueInMM("OUTSIDE_DIAMETER") * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                                                            //double tolerence = 100;//
                                                            //if (distance.Magnitude - od > tolerence)
                                                            //{
                                                            //    //超过容差值，判定为位置异常
                                                            //    separateElements.Add(element);
                                                            //}
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //组件缺失
                                                    separateElements.Add(element);
                                                }
                                            }
                                        }
                                    }
                                    #region test

                                    //if (getJointInstance != null)
                                    //{
                                    //    jointTypeName = getJointInstance["TYPE"].StringValue;
                                    //    FindJointTypeByJointName(jointTypeName);
                                    //}

                                    //IECInstance[] customAttributesInstances = getJointInstance.ClassDefinition.GetCustomAttributes();
                                    //int temp = 1;
                                    //Bentley.ECObjects.Schema.IECCustomAttributeContainer[] temp2 = getJointInstance.ClassDefinition.GetBaseContainers();
                                    //IList<IECInstance> temp3= getJointInstance.ClassDefinition.GetLocalCustomAttributes();
                                    //IECInstance[] temp4 = getJointInstance.ClassDefinition.GetPrimaryCustomAttributes();
                                    //IList<IECInstance> temp5= getJointInstance.ClassDefinition.GetLocalPrimaryCustomAttributes();
                                    //temp = 2;
                                    //IECPropertyValue jointTypeProperty = null;
                                    //foreach (var item in customAttributesInstances)
                                    //{
                                    //    if (item.ClassDefinition.Name.Equals("OpenPlant_3D_JointTypeProperties_Map"))
                                    //    {
                                    //        foreach (var item2 in item)
                                    //        {
                                    //            if (item2.AccessString.Equals("JOINT_TYPE"))
                                    //            {
                                    //                foreach (var item3 in item2.ContainedValues)
                                    //                {
                                    //                    try
                                    //                    {
                                    //                        foreach (var item4 in item3.ContainedValues)
                                    //                        {
                                    //                            if (item4.AccessString.Contains("JOINT_NAME"))
                                    //                            {
                                    //                                string jointType = item4.StringValue;
                                    //                                if (jointType.Equals(jointTypeName))
                                    //                                {
                                    //                                    //找到了该连接形式
                                    //                                    jointTypeProperty = item3;
                                    //                                }
                                    //                            }
                                    //                        }
                                    //                    }
                                    //                    catch (Exception)
                                    //                    {

                                    //                        throw;
                                    //                    }
                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    //}

                                    //IECInstance iECInstance2 = api.FindAllInformationOnInstance(portInstance);
                                    //if (null != iECInstance2)
                                    //{
                                    //    foreach (IECRelationshipInstance current2 in iECInstance2.GetRelationshipInstances())
                                    //    {
                                    //        if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == iECInstance2.InstanceId)//找到port的相关组件
                                    //        {
                                    //            System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstances(current2.Target, null, true).GetEnumerator();
                                    //            while (enumerator3.MoveNext())
                                    //            {
                                    //                IECInstance current3 = enumerator3.Current;
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                    #endregion
                                }
                                //valve has operater
                                IECInstance ecInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                                if (api.InstanceDefinedAsClass(ecInstance, "VALVE", true))
                                {
                                    string operatorDeviceName = "";
                                    try
                                    {
                                        operatorDeviceName = ecInstance["OPERATOR"].StringValue;
                                    }
                                    catch (Exception e)
                                    {
                                        string estr = e.Message;
                                    }
                                    bool flag = false;//是否有执行机构关系
                                    if (operatorDeviceName != "")//有执行机构就必须有对应的关系，如果没有就证明组件缺失
                                    {
                                        IECInstance operatorDeviceInstance = null;
                                        foreach (IECRelationshipInstance item in ecInstance.GetRelationshipInstances())
                                        {
                                            if (item.ClassDefinition.Name.Equals("VALVE_HAS_VALVE_OPERATING_DEVICE") && item.Source.InstanceId == ecInstance.InstanceId)
                                            {
                                                flag = true;
                                                operatorDeviceInstance = item.Target;
                                            }
                                        }
                                        if (!flag)
                                        {
                                            //缺失执行机构
                                            separateElements.Add(element);
                                        }
                                        else
                                        {
                                            //判断执行机构位置是否正确
                                            BMECObject operatorDeviceObject = new BMECObject(operatorDeviceInstance);
                                            DPoint3d operatorDeviceLocation = operatorDeviceObject.Transform3d.Translation;
                                            DPoint3d valveLocation = ec_object.Transform3d.Translation;
                                            DVector3d distance = new DVector3d(operatorDeviceLocation, valveLocation);
                                            double od = ec_object.GetDoubleValueInMM("OUTSIDE_DIAMETER") * Session.Instance.GetActiveDgnModel().GetModelInfo().UorPerMaster;
                                            double tolerence = 100;//
                                            if (distance.Magnitude - od > tolerence)
                                            {
                                                //超过容差值，判定为位置异常
                                                separateElements.Add(element);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                separateElements.Add(element);//没有连接性
                            }
                        }
                        else//没port
                        {
                            //TODO 处理方式
                            separateElements.Add(element);
                        }
                    }
                    else//非 PIPING_COMPONENT，相关的有 FASTENER、SEAL、VALVE_OPERATING_DEVICE
                    {
                        #region demo
                        bool hasJoint = false;//是否存在 Joint
                        if (api.InstanceDefinedAsClass(ec_object.Instance, "FASTENER", true))//FASTENER 的 relationship 有 JOINT_HAS_FASTENER(N,N)，有 JOINT 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("JOINT_HAS_FASTENER") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 Joint
                                        IECInstance targetJoint = current2.Source;
                                        string port1GUID = targetJoint["PORT1_GUID"].StringValue;
                                        string port2GUID = targetJoint["PORT2_GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid("PORT", true);
                                        if (hashTablePortInstance.ContainsKey(port1GUID) && hashTablePortInstance.ContainsKey(port2GUID))
                                        {
                                            //依附的连接组件存在，判断位置是否正确
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            foreach (var item in targetJoint.GetRelationshipInstances())
                                            {
                                                if (item.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && item.Target.InstanceId == targetJoint.InstanceId)//拿到对应的所有 Port
                                                {
                                                    portsInstance.Add(item.Source);
                                                }
                                            }
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;
                                            List<IECInstance> componentsInstance = new List<IECInstance>();//port 连接的 component
                                            if (portsInstance.Count > 0)
                                            {
                                                foreach (IECInstance item in portsInstance)
                                                {
                                                    foreach (var item2 in item.GetRelationshipInstances())
                                                    {
                                                        if (item2.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item2.Target.InstanceId == item.InstanceId)
                                                        {
                                                            componentsInstance.Add(item2.Source);
                                                        }
                                                    }
                                                }
                                            }
                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> componentsInstanceId = new List<string>();
                                                foreach (var item in componentsInstance)
                                                {
                                                    componentsInstanceId.Add(item.InstanceId);
                                                }
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                foreach (var item in scanComponentsInstanceId)
                                                {
                                                    if (!componentsInstanceId.Contains(item))
                                                    {
                                                        //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                        separateElements.Add(element);
                                                    }
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                separateElements.Add(element);
                                            }

                                            #region demo

                                            //Port finPortAtPoint1 = ec_object.FindPortAtPoint((Bentley.DPoint3d*)pPoint);
                                            //List<Port> finPort2 = ec_object.FindAllClosestPorts((Bentley.DPoint3d*)pPoint);
                                            //temp = 1;
                                            //foreach (IECInstance item in componentsInstance)
                                            //{
                                            //    BMECObject componentObject = new BMECObject(item);
                                            //    PortData portData1 = new PortData(item, componentObject.Ports[0].Instance);
                                            //    temp = 1;
                                            //    List<Port> findPort3 = componentObject.FindAllClosestPorts((Bentley.DPoint3d*)pPoint);
                                            //    temp = 1;
                                            //    Port finPort4 = componentObject.FindPortAtPoint((Bentley.DPoint3d*)pPoint);
                                            //    temp = 1;
                                            //    DPoint3d port1Location = componentObject.GetNthPort(0).LocationInUors;
                                            //    temp = 1;
                                            //    Port port1 = componentObject.GetNthPort(0);
                                            //    temp = 1;
                                            //    BMECObject connectedComponent = componentObject.GetConnectedComponentAtPort(1);
                                            //    temp = 1;
                                            //    BMECObject connectedComponent2 = componentObject.GetConnectedComponentAtPort(2);
                                            //    temp = 1;
                                            //    IECInstance allInfomationComponent = api.FindAllInformationOnInstance(item);
                                            //    BMECObject componentObject1 = new BMECObject(allInfomationComponent);
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;
                                            //    temp = 1;

                                            //}
                                            #endregion

                                            #region demo

                                            //IECInstance allInfoInstance1 = api.FindAllInformationOnInstance(hashTablePortInstance[port1GUID] as IECInstance);
                                            //if (null != allInfoInstance1)
                                            //{
                                            //    foreach (IECRelationshipInstance item in allInfoInstance1.GetRelationshipInstances())
                                            //    {
                                            //        if (item.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item.Target.InstanceId == allInfoInstance1.InstanceId)
                                            //        {
                                            //            IECInstance pipeComponent = item.Source;
                                            //            BMECObject newBMECObject = null;
                                            //            if (pipeComponent != null)
                                            //            {
                                            //                newBMECObject = new BMECObject(pipeComponent);
                                            //                int temp = 1;
                                            //                List<Port> componentPorts = newBMECObject.Ports;
                                            //                foreach (Port componentPort in componentPorts)
                                            //                {
                                            //                    string componentPortInstanceId = componentPort.Instance.InstanceId;
                                            //                }
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            //IECInstance allInfoInstance2 = api.FindAllInformationOnInstance(hashTablePortInstance[port1GUID] as IECInstance);
                                            //if (null != allInfoInstance2)
                                            //{
                                            //    foreach (IECRelationshipInstance item in allInfoInstance2.GetRelationshipInstances())
                                            //    {
                                            //        if (item.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item.Target.InstanceId == allInfoInstance2.InstanceId)
                                            //        {
                                            //            IECInstance pipeComponent = item.Source;
                                            //            BMECObject newBMECObject = null;
                                            //            if (pipeComponent != null)
                                            //            {
                                            //                newBMECObject = new BMECObject(pipeComponent);
                                            //            }
                                            //        }
                                            //    }
                                            //}
                                            #endregion
                                        }
                                        else if (port1GUID == "" || port2GUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else if (api.InstanceDefinedAsClass(ec_object.Instance, "SEAL", true))//SEAL 的 relationship 有 JOINT_HAS_SEAL(1,N)，有 JOINT 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("JOINT_HAS_SEAL") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 Joint
                                        IECInstance targetJoint = current2.Source;
                                        string port1GUID = targetJoint["PORT1_GUID"].StringValue;
                                        string port2GUID = targetJoint["PORT2_GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid("PORT", true);
                                        if (hashTablePortInstance.ContainsKey(port1GUID) && hashTablePortInstance.ContainsKey(port2GUID))
                                        {
                                            //依附的连接组件存在，判断物理位置
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            foreach (var item in targetJoint.GetRelationshipInstances())
                                            {
                                                if (item.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && item.Target.InstanceId == targetJoint.InstanceId)//拿到对应的所有 Port
                                                {
                                                    portsInstance.Add(item.Source);
                                                }
                                            }
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;
                                            List<IECInstance> componentsInstance = new List<IECInstance>();//port 连接的 component
                                            if (portsInstance.Count > 0)
                                            {
                                                foreach (IECInstance item in portsInstance)
                                                {
                                                    foreach (var item2 in item.GetRelationshipInstances())
                                                    {
                                                        if (item2.ClassDefinition.Name.Equals("PIPING_COMPONENT_HAS_PORT") && item2.Target.InstanceId == item.InstanceId)
                                                        {
                                                            componentsInstance.Add(item2.Source);
                                                        }
                                                    }
                                                }
                                            }


                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> componentsInstanceId = new List<string>();
                                                foreach (var item in componentsInstance)
                                                {
                                                    componentsInstanceId.Add(item.InstanceId);
                                                }
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                foreach (var item in scanComponentsInstanceId)
                                                {
                                                    if (!componentsInstanceId.Contains(item))
                                                    {
                                                        //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                        separateElements.Add(element);
                                                    }
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                //LineElement line = new LineElement(Session.Instance.GetActiveDgnModel(),null, new DSegment3d(jujianLocation, DPoint3d.Zero));
                                                //line.AddToModel();
                                                //separateElements.Add(element);
                                            }
                                        }
                                        else if (port1GUID == "" || port2GUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else if (api.InstanceDefinedAsClass(ec_object.Instance, "VALVE_OPERATING_DEVICE", true))//VALVE_OPERATING_DEVICE 的 relationship 有 VALVE_HAS_VALVE_OPERATING_DEVICE(1,1)，有 VALVE 就代表有连接，随后判断连接是否正确？
                        {
                            IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                            if (null != iECInstance)
                            {
                                foreach (IECRelationshipInstance current2 in iECInstance.GetRelationshipInstances())
                                {
                                    if (current2.ClassDefinition.Name.Equals("VALVE_HAS_VALVE_OPERATING_DEVICE") && current2.Target.InstanceId == iECInstance.InstanceId)
                                    {
                                        hasJoint = true;//有 valve
                                        IECInstance targetJoint = current2.Source;
                                        string componentGUID = targetJoint["GUID"].StringValue;
                                        System.Collections.Hashtable hashTablePortInstance = api.GetClassInstancesByGuid(targetJoint.ClassDefinition.Name, true);
                                        if (hashTablePortInstance.ContainsKey(componentGUID))
                                        {
                                            //依附的连接组件存在，判断物理位置
                                            List<IECInstance> portsInstance = new List<IECInstance>();
                                            DPoint3d jujianLocation = ec_object.Transform3d.Translation;

                                            List<BMECObject> scanObject = JYX_ZYJC_CLR.PublicMethod.ScanObjectsAtPoint(ec_object, jujianLocation);
                                            if (scanObject != null && scanObject.Count > 0)
                                            {
                                                List<string> scanComponentsInstanceId = new List<string>();
                                                foreach (var item in scanObject)
                                                {
                                                    scanComponentsInstanceId.Add(item.Instance.InstanceId);
                                                }
                                                if (!scanComponentsInstanceId.Contains(targetJoint.InstanceId))
                                                {
                                                    //在点附近扫描出来的 Object 不包含连接的 Component，判定为位置异常
                                                    separateElements.Add(element);
                                                }
                                            }
                                            else//没有扫描到任何物体
                                            {
                                                separateElements.Add(element);
                                            }
                                        }
                                        else if (componentGUID == "")
                                        {
                                            //TODO 目前已知的是复制会出现此问题
                                        }
                                        else
                                        {
                                            //依附的连接组件缺失
                                            separateElements.Add(element);
                                        }
                                    }
                                }
                                if (!hasJoint)
                                {
                                    separateElements.Add(element);
                                }
                            }
                        }
                        else//预期之外的组件
                        {
                            //TODO 处理方式
                        }
                        #endregion
                    }
                    #region demo

                    //start test
                    //if (ec_object.ClassName == "WELD_NECK_FLANGE")
                    //{
                    //    IECInstance iECInstance = api.FindAllInformationOnInstance(ec_object.Instance);
                    //    IECRelationshipInstanceCollection iECRelationshipInstance = iECInstance.GetRelationshipInstances();
                    //    foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                    //    {
                    //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(current);
                    //        IECRelationshipInstanceCollection iECRelationshipInstance2 = iECInstance2.GetRelationshipInstances();
                    //        foreach (IECRelationshipInstance current2 in iECRelationshipInstance2)
                    //        {

                    //        }
                    //    }
                    //}
                    //if (ec_object.Ports != null && ec_object.Ports.Count > 0)
                    //{
                    //    foreach (var port in ec_object.Ports)
                    //    {
                    //        ChangeSet changeSet = new ChangeSet();
                    //        IECInstance portInstance = port.Instance;
                    //        IECInstance connectedPortInstance = api.GetConnectedPortInstance(portInstance);
                    //        IECInstance iECInstance = api.FindAllInformationOnInstance(connectedPortInstance);
                    //        if (null != iECInstance)
                    //        {
                    //            foreach (IECRelationshipInstance current in iECInstance.GetRelationshipInstances())
                    //            {
                    //                if (current.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current.Source.InstanceId == iECInstance.InstanceId)
                    //                {
                    //                    changeSet.Add(current);


                    //                }
                    //            }
                    //        }
                    //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(portInstance);
                    //        if (null != iECInstance2)
                    //        {
                    //            foreach (IECRelationshipInstance current2 in iECInstance2.GetRelationshipInstances())
                    //            {
                    //                if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == iECInstance2.InstanceId)
                    //                {
                    //                    System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstances(current2.Target, null, true).GetEnumerator();
                    //                    while (enumerator3.MoveNext())
                    //                    {
                    //                        IECInstance current3 = enumerator3.Current;
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //end
                    #endregion

                    #region 原来的判断逻辑

                    //if (ec_object.Ports != null && ec_object.Ports.Count != 0)//判断 ECInstance 是否有具连结性的可能
                    //{
                    //    if (ec_object.ConnectedComponents != null && ec_object.ConnectedComponents.Count == 0)// ECInstance 不具有连接元素
                    //    {
                    //        separateElements.Add(element);
                    //        elementlist.Add(ec_object);
                    //    }
                    //}
                    #endregion
                }
                else//为普通元素，普通元素全部没有连结性
                {
                    //separateElements.Add(element);
                }
            }
            return separateElements;
        }

        public List<Element> filterElementByConnection2()
        {
            List<Element> separateElements = new List<Element>();//连结性上孤立元素的容器
            ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();//扫描所有元素
            List<BMECObject> elementlist = new List<BMECObject>();
            List<Element> solidElement = new List<Element>();//不具有 EC 属性的智能实体
            foreach (var element in elements)
            {
                BMECObject ec_object = null;
                ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
                if (ec_object != null && ec_object.Instance != null)//是否为 ECInstance
                {
                    IECInstance instance = ec_object.Instance;
                    IECInstance ralatedInstance = ec_object.RelatedInstance;
                    IECRelationshipInstanceCollection relationshipInstanceCollection = instance.GetRelationshipInstances();
                    foreach (var relationshipInstance in relationshipInstanceCollection)
                    {

                    }
                }
            }
            return separateElements;
        }

        /*其他功能*/
        /// <summary>
        ///定位到元素
        /// </summary>
        public static void locateElement(long elementId)
        {
            Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            Bentley.Interop.MicroStationDGN.Element ielement = app.ActiveModelReference.GetElementByID(elementId);
            Bentley.Interop.MicroStationDGN.View viewport = app.CommandState.LastView();
            int view = viewport.Index;
            JYX_ZYJC_CLR.PublicMethod.zoom_elem(ielement, view);
            ielement.IsHighlighted = true;

            //BMECObject ecobject = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id((ulong)elementId);
            //if (ecobject != null && ecobject.Instance != null)
            //{
            //    BMECApi.Instance.ViewComponent(ecobject.Instance, false, false, false);
            //}
        }
        /// <summary>
        /// 从当前 Model 中删除元素
        /// </summary>
        /// <param name="elementId"></param>
        public static void deleteElement(List<long> elementId)
        {
            List<Element> elements = ElementClear.getCEElementById(elementId);
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    try
                    {
                        element.DeleteFromModel();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        /// <summary>
        /// 通过 elementId 获取对应的 V8i 的 ELement
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public static List<Bentley.Interop.MicroStationDGN.Element> getV8iElementById(List<long> elementId)
        {
            List<Bentley.Interop.MicroStationDGN.Element> elements = new List<Bentley.Interop.MicroStationDGN.Element>();
            Bentley.Interop.MicroStationDGN.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
            foreach (var id in elementId)
            {
                Bentley.Interop.MicroStationDGN.Element element = app.ActiveModelReference.GetElementByID(id);
                elements.Add(element);
            }
            return elements;
        }
        /// <summary>
        /// 通过 elementId 获取对应的 CE 的 ELement
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        public static List<Element> getCEElementById(List<long> elementId)
        {
            List<Element> elements = new List<Element>();
            foreach (var id in elementId)
            {
                long tempid = id;
                Element element = Session.Instance.GetActiveDgnModel().FindElementById(new ElementId(ref tempid));
                elements.Add(element);
            }
            return elements;
        }
        /// <summary>
        /// 无重复元素添加
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        public static List<Element> addElementListNorepeat(List<Element> list1, List<Element> list2)
        {
            List<Element> elements = new List<Element>();
            elements = list1;
            foreach (var list2element in list2)
            {
                if (!ElementClear.isContainElement(list1, list2element))
                {
                    elements.Add(list2element);
                }
            }
            return elements;
        }

        public static List<IECInstance> addElementListNorepeat(List<IECInstance> list1, List<IECInstance> list2)
        {
            List<IECInstance> elements = new List<IECInstance>();
            elements = list1;
            foreach (var list2element in list2)
            {
                if (!ElementClear.isContainElement(list1, list2element))
                {
                    elements.Add(list2element);
                }
            }
            return elements;
        }
        /// <summary>
        /// List中是否包含某 Element 元素
        /// </summary>
        /// <param name="list"></param>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool isContainElement(List<Element> list, Element element)
        {
            bool isContain = false;
            foreach (var ele in list)
            {
                if (ele.ElementId == element.ElementId)
                {
                    isContain = true;
                }
            }
            return isContain;
        }

        public static bool isContainElement(List<IECInstance> list, IECInstance element)
        {
            bool isContain = false;
            foreach (var ele in list)
            {
                if (ele.InstanceId == element.InstanceId)
                {
                    isContain = true;
                }
            }
            return isContain;
        }

        /// <summary>
        /// 根据连接类型名查找对应连接类型信息
        /// </summary>
        /// <param name="JointName"></param>
        public static IECPropertyValue FindJointTypeByJointName(string JointName)
        {
            //IECPropertyValue myJoint = null;
            IECInstance joint = BMECInstanceManager.Instance.CreateECInstance("JOINT", true);
            IECInstance[] customAttributesInstances = joint.ClassDefinition.GetCustomAttributes();
            IECInstance myCustomAttributeInstance = null;
            foreach (IECInstance customAttributeInstance in customAttributesInstances)
            {
                if (customAttributeInstance.ClassDefinition.Name.Equals("OpenPlant_3D_JointTypeProperties_Map"))
                {
                    myCustomAttributeInstance = customAttributeInstance;
                    break;
                }
            }
            if (myCustomAttributeInstance == null) return null;
            IECPropertyValue myJointType = null;
            myJointType = myCustomAttributeInstance.GetPropertyValue("JOINT_TYPE");
            if (myJointType == null) return null;
            IECValueContainer myJointTypeContainer = null;
            myJointTypeContainer = myJointType.ContainedValues;
            foreach (var property in myJointTypeContainer)
            {
                foreach (var item in property.ContainedValues)
                {
                    if (item.AccessString.Contains("JOINT_NAME"))
                    {
                        if (item.StringValue.Equals(JointName))
                        {
                            //myJoint = property;
                            return property;
                        }
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 连接形式中 Fastener 的名称
        /// </summary>
        /// <param name="jointType"></param>
        /// <param name="fastnerName"></param>
        public static void GetJointTypeInfo(IECPropertyValue jointType, out List<string> fastnerName)
        {
            fastnerName = new List<string>();
            foreach (var item in jointType.ContainedValues)
            {
                if (item.AccessString.Contains("FASTENER"))
                {
                    foreach (var item2 in item.ContainedValues)
                    {
                        fastnerName.Add(item2.StringValue);
                    }
                }
            }
        }
    }
}
