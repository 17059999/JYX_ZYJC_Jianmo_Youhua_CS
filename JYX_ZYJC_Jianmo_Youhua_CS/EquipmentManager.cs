
using Bentley.ECObjects.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.OpenPlantModeler.SDK.Utilities;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.MstnPlatformNET;
using Bentley.ECObjects.Schema;
using Bentley.OpenPlantModeler.SDK.AssociatedItems;
using Bentley.OpenPlant.Modeler.Api;
using System.Collections;

using BPU= Bentley.Plant.Utilities;
using System.Windows.Forms;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    //设备可能有三种关系：EQUIPMENT_HAS_DATUM, EQUIPMENT_HAS_NOZZLE, EQUIPMENT_HAS_PORT
    public class EquipmentManager
    {
        private static BMECApi api = BMECApi.Instance;
        //BMECObject 高亮、定位、单独显示
        public static void ViewComponent(IECInstance component, bool hiLite, bool zoom, bool isolate)
        {
            try
            {
                api.ViewComponent(component, hiLite, zoom, isolate);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }
        //显示设备管嘴信息
        public void ShowComponentsNozzleInfo(BMECObject component) {

        }

        //修改设备属性
        public static bool ModifyBMECObject(ref IECInstance component, string propertyName, string propertyValue)
        {
            //int temp = 1;
            Bentley.Plant.Utilities.DgnUtilities dgnutilities = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            if (dgnutilities.IsECInstanceFromReferenceFile(component))
            {
                return false;
            }
            try
            {
                Dictionary<string, IECInstance> allInstanceDic = new Dictionary<string, IECInstance>();

                if (!(propertyName.Equals("UNIT")|| propertyName.Equals("SERVICE")))
                {
                    //修改普通属性
                    component[propertyName].StringValue = propertyValue;

                    dgnutilities.SaveModifiedInstance(component, dgnutilities.GetDGNConnection());

                    return true;
                }
                else if (propertyName.Equals("SERVICE"))
                {
                    allInstanceDic = GetAllEquipmentService();

                    //foreach (var item in allInstanceDic)
                    //{
                    //    if (item.Key.Equals(propertyValue))
                    //    {
                    //        component[propertyName].StringValue = item.Key;
                    //    }
                    //}
                    //component[propertyName].StringValue = propertyValue;

                    //dgnutilities.SaveModifiedInstance(component, dgnutilities.GetDGNConnection());

                }
                else if (propertyName.Equals("UNIT"))
                {
                    allInstanceDic = GetAllEquipmentUnit();

                }

                
                string plantRelationName = plantBreakDownRelationDic[propertyName];
                string plantName = propertyValue;

                #region 修改 Relationship
                IECInstance componentinstanceclone = ECInstanceHelper.CloneInstanceGraph(component);

                IECInstance allInfoComponent = api.FindAllInformationOnInstance(/*component.Instance*/componentinstanceclone);

                if (null == allInfoComponent) return false;
                IECRelationshipInstanceCollection componentRelationshipInstance = allInfoComponent.GetRelationshipInstances();
                IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                bool s = false;
                while (componentRelationshipEnumerator.MoveNext())
                {
                    s = true;
                    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                    if (!allInstanceDic.ContainsKey(plantName)) return false;
                    IECInstance sourceInstance = allInstanceDic[plantName];
                    currentRelationship.Source = sourceInstance;

                    dgnutilities.SaveModifiedInstance(currentRelationship, dgnutilities.GetDGNConnection());
                    IECInstance newModifyInstance = DgnUtilities.GetInstanceByInstanceId(component.InstanceId);
                    //string temp = component.GetString(propertyName);
                    //string temp2 = newModifyInstance.GetString(propertyName);
                    component = newModifyInstance;
                    
                    //Bentley.Building.Mechanical.Api.BMECApi.Instance.InstanceManager.CommitInstance(modifyInstance);
                    return true;
                }
                if (!s)
                {
                    component[propertyName].StringValue = propertyValue;

                    dgnutilities.SaveModifiedInstance(component, dgnutilities.GetDGNConnection());

                    return true;
                }
                #endregion
                #region Myregion
                //temp = 1;
                //检查 Unit 的 关系表
                //IECRelationshipInstanceCollection unitRelationship = allInstanceDic[plantName].GetRelationshipInstances();
                //IEnumerator<IECRelationshipInstance> unitRelationEnumerator = unitRelationship.GetEnumerator();
                //while (unitRelationEnumerator.MoveNext())
                //{
                //    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                //}
                //temp = 1;
                //IECRelationshipInstanceCollection temprelation = allInfoComponent.GetRelationshipInstances();
                //IEnumerator<IECRelationshipInstance> tempcomponentRelationshipEnumerator = temprelation.GetEnumerator();
                //while (tempcomponentRelationshipEnumerator.MoveNext())
                //{
                //    IECRelationshipInstance currentRelationship = tempcomponentRelationshipEnumerator.Current;
                //    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                //    //IECInstance sourceInstance = allInstanceDic[plantName];
                //    //currentRelationship.Source = sourceInstance;
                //    temp = 1;
                //}
                //temp = 1;
                //string unitid = component.Instance["UNIT"].StringValue;
                //BMECObject newEquipment = new BMECObject(allInfoComponent);
                //string allinfoUnitId = allInfoComponent["UNIT"].StringValue;
                //PickListLibrary.ModifyInstance();
                //component.Instance = api.FindAllInformationOnInstance(allInfoComponent);
                //string unitid4 = component.Instance["UNIT"].StringValue;
                //string allinfoUnitId2 = allInfoComponent["UNIT"].StringValue;
                //IECRelationshipInstanceCollection temprelation1 = component.Instance.GetRelationshipInstances();
                //IEnumerator<IECRelationshipInstance> tempcomponentRelationshipEnumerator1 = temprelation1.GetEnumerator();
                //while (tempcomponentRelationshipEnumerator1.MoveNext())
                //{
                //    IECRelationshipInstance currentRelationship = tempcomponentRelationshipEnumerator1.Current;
                //    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                //    //IECInstance sourceInstance = allInstanceDic[plantName];
                //    //currentRelationship.Source = sourceInstance;
                //    string unitid3 = allInfoComponent["UNIT"].StringValue;
                //    temp = 1;
                //}
                #endregion
                #region MyRegion
                //component.Create();
                //component.DiscoverConnectionsEx();
                //component.UpdateConnections();
                #endregion
                #region MyRegion
                //temp = 1;

                //IECRelationshipInstanceCollection componentRelationshipInstance = components.GetRelationshipInstances();
                //IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                //while (componentRelationshipEnumerator.MoveNext())
                //{
                //    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                //    if (currentRelationship.ClassDefinition.Name != "UNIT_HAS_NAMED_ITEM") continue;
                //    foreach (IECInstance instance in allUnitInstance)
                //    {
                //        string unitName = instance["NAME"].StringValue;
                //        if (unitName == "equipUnit1")
                //        {

                //        }
                //        IECInstance allInfoInstance = api.FindAllInformationOnInstance(instance);
                //        IECRelationshipInstanceCollection plantBreakDownElementRelationship = allInfoInstance.GetRelationshipInstances();
                //        IEnumerator<IECRelationshipInstance> plantBreakDownRelationshipInstance = plantBreakDownElementRelationship.GetEnumerator();
                //        while (plantBreakDownRelationshipInstance.MoveNext())
                //        {
                //            if (plantBreakDownRelationshipInstance.Current.InstanceId == currentRelationship.InstanceId)
                //            {
                //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"
                //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"

                //            }
                //        }
                //        //IECRelationshipClass;//获取当前 Unit、Service 的关系结构定义
                //    }
                //}
                #endregion
                #region MyRegion

                //EquipmentModifyTool equipmentModifyTool = new EquipmentModifyTool();
                //equipmentModifyTool.InstallTool();
                //equipmentModifyTool.modifyEquipment(components, propertyName, propertyValue);

                ////Utilities.ComApp.CommandState.StartDefaultCommand();
                #endregion
                #region MyRegion

                //Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();

                //IECInstance instanceForCurrentSelection = this.getInstanceForCurrentSelection(e.RowIndex, text);//当前选择的 Unit、Service实例

                //IECRelationshipInstanceCollection relationshipInstances = components.GetRelationshipInstances();//选择的设备的所有 Relationship
                //IEnumerator<IECRelationshipInstance> enumerator2 = relationshipInstances.GetEnumerator();

                //IECRelationshipClass eCRelationshipClassInformationForCurrentSelection = this.getECRelationshipClassInformationForCurrentSelection(e.RowIndex);//获取当前选择的 Unit、Service 的 Relationship 结构

                //bool flag = false;
                //while (enumerator2.MoveNext())
                //{
                //    //找到要修改的 Relationship 实例，并修改其关系
                //    IECRelationshipInstance current2 = enumerator2.Current;
                //    if (current2.ClassDefinition == eCRelationshipClassInformationForCurrentSelection)
                //    {
                //        //TODO
                //        //if (text == LocalizableStrings.None || string.IsNullOrEmpty(text))//选择列表判空
                //        //{
                //        //    relationshipInstances.Remove(current2);
                //        //}
                //        else
                //        {
                //            current2.Source = instanceForCurrentSelection;
                //        }
                //        flag = true;
                //        break;
                //    }
                //}
                //if (!flag && instanceForCurrentSelection != null)
                //{
                //    IECRelationshipInstance iECRelationshipInstance = eCRelationshipClassInformationForCurrentSelection.CreateInstance() as IECRelationshipInstance;
                //    iECRelationshipInstance.Source = instanceForCurrentSelection;
                //    iECRelationshipInstance.Target = components;
                //    components.GetRelationshipInstances().Add(iECRelationshipInstance);
                //}
                //IECInstance iECInstance = instanceForCurrentSelection;
                //if (iECInstance != null && (instance.IsECInstanceFromReferenceFile(iECInstance) || instance.IsECInstanceReferencedOut(iECInstance) || iECInstance.IsReadOnly))
                //{
                //    iECInstance = null;
                //}
                ////(base["optionEdit", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                ////(base["optionDelete", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                ////IECPropertyValue propertyValue = BusinessKeyUtility.GetPropertyValue(current);
                ////if (propertyValue != null)
                ////{
                ////    current.OnECPropertyValueChanged(propertyValue);
                ////}

                ////this.SaveSettings();

                #endregion
                #region MyRegion

                //dgnutilities.DeleteAllRelationshipsToParent(component.Instance, Bentley.Plant.Utilities.SchemaUtilities.PlantSchema.GetClass(plantRelationName) as Bentley.ECObjects.Schema.IECRelationshipClass, Bentley.Plant.Utilities.SchemaUtilities.PlantSchema.GetClass(component.Instance.ClassDefinition.Name), dgnutilities.GetDGNConnection());
                //IECInstance componentInstanceClone = ECInstanceHelper.CloneInstanceGraph(component.Instance);
                //ECInstanceHelper.SetValue();
                //dgnutilities.SaveModifiedInstance(currentRelationship, dgnutilities.GetDGNConnection());
                #endregion
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                return false;
            }
            #region MyRegion

            ////IECInstance instance = (IECInstance)(components.Instance.Clone());
            ////instance[propertyName].StringValue = propertyValue;
            ////int instanceCloneHashCode = instance.GetHashCode();
            ////int instanceHashCode = components.GetHashCode();
            //int temp = 1;
            ////IECPropertyValue property = instance.GetPropertyValue("DESCRIPTION");
            ////temp = 1;
            ////property = instance.GetPropertyValue("COMPONENT_NAME");
            ////bool isRO = property.IsReadOnly;
            ////temp = 1;
            //try
            //{
            //    Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            //    NamedItem namedItem = NamedItem.GetUnitByName(components.Instance["UNIT"].StringValue);
            //    IECClass @class = SchemaUtilities.PlantSchema.GetClass(namedItem.NamedItemTypeName);
            //    temp = 1;
            //    Bentley.Plant.Utilities.DgnUtilities instance2 = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            //    IECRelationshipInstance iECRelationshipInstance = (SchemaUtilities.PlantSchema.GetClass(namedItem.NamedItemRelationshipName) as IECRelationshipClass).CreateInstance() as IECRelationshipInstance;
            //    IECInstance sourceInstance = instance2.GetSourceInstance(@class, propertyValue);
            //    iECRelationshipInstance.Source = sourceInstance;
            //    iECRelationshipInstance.Target = components.Instance;
            //    components.Create();
            //    //components.Instance[propertyName].StringValue = propertyValue;

            //    //instance.SaveModifiedInstance(components.Instance, instance.GetDGNConnection());
            //}
            //catch (Exception e)
            //{

            //    System.Windows.Forms.MessageBox.Show(e.ToString());
            //}
            ////instance2.WriteECInstanceToDgn(iECRelationshipInstance, instance2.GetDGNConnection());
            ////instance2.SaveModifiedInstance(sourceInstance, instance2.GetDGNConnection());
            ////components.Instance["UNIT"].StringValue = propertyName;
            ////temp = 1;

            ////int hashcode = components.Instance.GetHashCode();
            ////components.Instance[propertyName].StringValue = propertyValue;
            ////int hashcode2 = components.Instance.GetHashCode();
            ////string propervalue = components.Instance[propertyName].StringValue;
            ////BMECInstanceManager instanceManager = BMECInstanceManager.Instance;
            ////instanceManager.CommitInstance(components.Instance);
            ////int hashcode3 = components.Instance.GetHashCode();
            ////components.Create();
            ////int hashcode4 = components.Instance.GetHashCode();

            ////instance2.WriteECInstanceToDgn(iECRelationshipInstance, instance2.GetDGNConnection());
            ////instance.SaveModifiedInstance(components.Instance, instance.GetDGNConnection());
            //temp = 1;
            //temp = 1;
            ////Bentley.Building.Mechanical.Api.BMECApi.Instance.InstanceManager.CommitInstance(instance);
            #endregion

            #region MyRegion

            //IECInstance equipInstance = components.Instance;
            //IECInstance equipInstanceAllInfo = api.FindAllInformationOnInstance(equipInstance);

            //if (equipInstanceAllInfo != null)
            //{
            //    foreach (IECRelationshipInstance current in equipInstanceAllInfo.GetRelationshipInstances())
            //    {
            //        if (current.ClassDefinition.Name.Equals("UNIT_HAS_NAMED_ITEM"))
            //        {

            //        }
            //    }
            //}

            #endregion

            #region MyRegion
            //ECInstanceList allInstance = DgnUtilities.GetAllInstancesFromDgn();
            //List<IECInstance> unitInstance = new List<IECInstance>();
            //foreach (IECInstance item in allInstance)
            //{
            //    if (item.ClassDefinition.Name == "UNIT")
            //    {
            //        unitInstance.Add(item);
            //    }
            //}
            //temp = 1;
            //foreach (IECInstance item in unitInstance)
            //{
            //    string descr = item["DESCRIPTION"].StringValue;
            //    IECInstance allInfoUnit = api.FindAllInformationOnInstance(item);
            //    if (allInfoUnit != null)
            //    {
            //        foreach (IECRelationshipInstance item1 in allInfoUnit.GetRelationshipInstances())
            //        {
            //            if (item1.ClassDefinition.Name == "UNIT_HAS_NAMED_ITEM")
            //            {

            //            }
            //        }
            //    }
            //}

            #endregion

            #region MyRegion

            //List<IECInstance> arg_634_0 = api.GetRelatedInstances(components.Instance, "EQUIPMENT_HAS_NOZZLE", true);
            //List<OPMCommon.Equipment.DatumStructure> datumList = api.FindDatums(components.Instance);
            //List<IECInstance>.Enumerator enumerator7 = arg_634_0.GetEnumerator();
            //if (enumerator7.MoveNext())
            //{
            //    do
            //    {
            //        IECInstance current5 = enumerator7.Current;
            //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(current5);
            //        api.ComputeLocationVectors(iECInstance2, datumList);
            //        new BMECObject(iECInstance2).Create();
            //    }
            //    while (enumerator7.MoveNext());
            //}

            #endregion

            //ModifyBMECObject(components.Instance, propertyName, propertyValue);
            return false;
        }

        //批量修改设备属性
        public static bool ModifyBMECObjects(ref List<IECInstance> components, List<string> propertyNames, List<string> propertyValues)
        {
            //int temp = 1;
            for (int i = 0; i < components.Count; i++)
            {
                IECInstance component = components[i];
                string propertyName = propertyNames[i];
                string propertyValue = propertyValues[i];


                Bentley.Plant.Utilities.DgnUtilities dgnutilities = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
                if (dgnutilities.IsECInstanceFromReferenceFile(component))
                {
                    return false;
                }
                try
                {
                    Dictionary<string, IECInstance> allInstanceDic = new Dictionary<string, IECInstance>();

                    if (!(/*propertyName.Equals("NUMBER") || */propertyName.Equals("UNIT") || propertyName.Equals("SERVICE")))
                    {
                        //修改普通属性
                        component[propertyName].StringValue = propertyValue;

                        dgnutilities.SaveModifiedInstance(component, dgnutilities.GetDGNConnection());

                        return true;
                    }
                    else if (propertyName.Equals("SERVICE"))
                    {
                        allInstanceDic = GetAllEquipmentService();
                    }
                    else if (propertyName.Equals("UNIT"))
                    {
                        allInstanceDic = GetAllEquipmentUnit();

                    }


                    string plantRelationName = plantBreakDownRelationDic[propertyName];
                    string plantName = propertyValue;

                    #region 修改 Relationship
                    IECInstance componentinstanceclone = ECInstanceHelper.CloneInstanceGraph(component);

                    IECInstance allInfoComponent = api.FindAllInformationOnInstance(/*component.Instance*/componentinstanceclone);

                    if (null == allInfoComponent) return false;
                    IECRelationshipInstanceCollection componentRelationshipInstance = allInfoComponent.GetRelationshipInstances();
                    IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                    while (componentRelationshipEnumerator.MoveNext())
                    {
                        IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                        if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                        if (!allInstanceDic.ContainsKey(plantName)) return false;
                        IECInstance sourceInstance = allInstanceDic[plantName];
                        currentRelationship.Source = sourceInstance;

                        dgnutilities.SaveModifiedInstance(currentRelationship, dgnutilities.GetDGNConnection());
                        IECInstance newModifyInstance = DgnUtilities.GetInstanceByInstanceId(component.InstanceId);
                        //string temp = component.GetString(propertyName);
                        //string temp2 = newModifyInstance.GetString(propertyName);
                        component = newModifyInstance;
                        components[i] = newModifyInstance;

                        //Bentley.Building.Mechanical.Api.BMECApi.Instance.InstanceManager.CommitInstance(modifyInstance);
                        return true;
                    }
                    #endregion
                    #region Myregion
                    //temp = 1;
                    //检查 Unit 的 关系表
                    //IECRelationshipInstanceCollection unitRelationship = allInstanceDic[plantName].GetRelationshipInstances();
                    //IEnumerator<IECRelationshipInstance> unitRelationEnumerator = unitRelationship.GetEnumerator();
                    //while (unitRelationEnumerator.MoveNext())
                    //{
                    //    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                    //}
                    //temp = 1;
                    //IECRelationshipInstanceCollection temprelation = allInfoComponent.GetRelationshipInstances();
                    //IEnumerator<IECRelationshipInstance> tempcomponentRelationshipEnumerator = temprelation.GetEnumerator();
                    //while (tempcomponentRelationshipEnumerator.MoveNext())
                    //{
                    //    IECRelationshipInstance currentRelationship = tempcomponentRelationshipEnumerator.Current;
                    //    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                    //    //IECInstance sourceInstance = allInstanceDic[plantName];
                    //    //currentRelationship.Source = sourceInstance;
                    //    temp = 1;
                    //}
                    //temp = 1;
                    //string unitid = component.Instance["UNIT"].StringValue;
                    //BMECObject newEquipment = new BMECObject(allInfoComponent);
                    //string allinfoUnitId = allInfoComponent["UNIT"].StringValue;
                    //PickListLibrary.ModifyInstance();
                    //component.Instance = api.FindAllInformationOnInstance(allInfoComponent);
                    //string unitid4 = component.Instance["UNIT"].StringValue;
                    //string allinfoUnitId2 = allInfoComponent["UNIT"].StringValue;
                    //IECRelationshipInstanceCollection temprelation1 = component.Instance.GetRelationshipInstances();
                    //IEnumerator<IECRelationshipInstance> tempcomponentRelationshipEnumerator1 = temprelation1.GetEnumerator();
                    //while (tempcomponentRelationshipEnumerator1.MoveNext())
                    //{
                    //    IECRelationshipInstance currentRelationship = tempcomponentRelationshipEnumerator1.Current;
                    //    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                    //    //IECInstance sourceInstance = allInstanceDic[plantName];
                    //    //currentRelationship.Source = sourceInstance;
                    //    string unitid3 = allInfoComponent["UNIT"].StringValue;
                    //    temp = 1;
                    //}
                    #endregion
                    #region MyRegion
                    //component.Create();
                    //component.DiscoverConnectionsEx();
                    //component.UpdateConnections();
                    #endregion
                    #region MyRegion
                    //temp = 1;

                    //IECRelationshipInstanceCollection componentRelationshipInstance = components.GetRelationshipInstances();
                    //IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                    //while (componentRelationshipEnumerator.MoveNext())
                    //{
                    //    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                    //    if (currentRelationship.ClassDefinition.Name != "UNIT_HAS_NAMED_ITEM") continue;
                    //    foreach (IECInstance instance in allUnitInstance)
                    //    {
                    //        string unitName = instance["NAME"].StringValue;
                    //        if (unitName == "equipUnit1")
                    //        {

                    //        }
                    //        IECInstance allInfoInstance = api.FindAllInformationOnInstance(instance);
                    //        IECRelationshipInstanceCollection plantBreakDownElementRelationship = allInfoInstance.GetRelationshipInstances();
                    //        IEnumerator<IECRelationshipInstance> plantBreakDownRelationshipInstance = plantBreakDownElementRelationship.GetEnumerator();
                    //        while (plantBreakDownRelationshipInstance.MoveNext())
                    //        {
                    //            if (plantBreakDownRelationshipInstance.Current.InstanceId == currentRelationship.InstanceId)
                    //            {
                    //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"
                    //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"

                    //            }
                    //        }
                    //        //IECRelationshipClass;//获取当前 Unit、Service 的关系结构定义
                    //    }
                    //}
                    #endregion
                    #region MyRegion

                    //EquipmentModifyTool equipmentModifyTool = new EquipmentModifyTool();
                    //equipmentModifyTool.InstallTool();
                    //equipmentModifyTool.modifyEquipment(components, propertyName, propertyValue);

                    ////Utilities.ComApp.CommandState.StartDefaultCommand();
                    #endregion
                    #region MyRegion

                    //Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();

                    //IECInstance instanceForCurrentSelection = this.getInstanceForCurrentSelection(e.RowIndex, text);//当前选择的 Unit、Service实例

                    //IECRelationshipInstanceCollection relationshipInstances = components.GetRelationshipInstances();//选择的设备的所有 Relationship
                    //IEnumerator<IECRelationshipInstance> enumerator2 = relationshipInstances.GetEnumerator();

                    //IECRelationshipClass eCRelationshipClassInformationForCurrentSelection = this.getECRelationshipClassInformationForCurrentSelection(e.RowIndex);//获取当前选择的 Unit、Service 的 Relationship 结构

                    //bool flag = false;
                    //while (enumerator2.MoveNext())
                    //{
                    //    //找到要修改的 Relationship 实例，并修改其关系
                    //    IECRelationshipInstance current2 = enumerator2.Current;
                    //    if (current2.ClassDefinition == eCRelationshipClassInformationForCurrentSelection)
                    //    {
                    //        //TODO
                    //        //if (text == LocalizableStrings.None || string.IsNullOrEmpty(text))//选择列表判空
                    //        //{
                    //        //    relationshipInstances.Remove(current2);
                    //        //}
                    //        else
                    //        {
                    //            current2.Source = instanceForCurrentSelection;
                    //        }
                    //        flag = true;
                    //        break;
                    //    }
                    //}
                    //if (!flag && instanceForCurrentSelection != null)
                    //{
                    //    IECRelationshipInstance iECRelationshipInstance = eCRelationshipClassInformationForCurrentSelection.CreateInstance() as IECRelationshipInstance;
                    //    iECRelationshipInstance.Source = instanceForCurrentSelection;
                    //    iECRelationshipInstance.Target = components;
                    //    components.GetRelationshipInstances().Add(iECRelationshipInstance);
                    //}
                    //IECInstance iECInstance = instanceForCurrentSelection;
                    //if (iECInstance != null && (instance.IsECInstanceFromReferenceFile(iECInstance) || instance.IsECInstanceReferencedOut(iECInstance) || iECInstance.IsReadOnly))
                    //{
                    //    iECInstance = null;
                    //}
                    ////(base["optionEdit", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                    ////(base["optionDelete", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                    ////IECPropertyValue propertyValue = BusinessKeyUtility.GetPropertyValue(current);
                    ////if (propertyValue != null)
                    ////{
                    ////    current.OnECPropertyValueChanged(propertyValue);
                    ////}

                    ////this.SaveSettings();

                    #endregion
                    #region MyRegion

                    //dgnutilities.DeleteAllRelationshipsToParent(component.Instance, Bentley.Plant.Utilities.SchemaUtilities.PlantSchema.GetClass(plantRelationName) as Bentley.ECObjects.Schema.IECRelationshipClass, Bentley.Plant.Utilities.SchemaUtilities.PlantSchema.GetClass(component.Instance.ClassDefinition.Name), dgnutilities.GetDGNConnection());
                    //IECInstance componentInstanceClone = ECInstanceHelper.CloneInstanceGraph(component.Instance);
                    //ECInstanceHelper.SetValue();
                    //dgnutilities.SaveModifiedInstance(currentRelationship, dgnutilities.GetDGNConnection());
                    #endregion
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.ToString());
                    return false;
                }
                #region MyRegion

                ////IECInstance instance = (IECInstance)(components.Instance.Clone());
                ////instance[propertyName].StringValue = propertyValue;
                ////int instanceCloneHashCode = instance.GetHashCode();
                ////int instanceHashCode = components.GetHashCode();
                //int temp = 1;
                ////IECPropertyValue property = instance.GetPropertyValue("DESCRIPTION");
                ////temp = 1;
                ////property = instance.GetPropertyValue("COMPONENT_NAME");
                ////bool isRO = property.IsReadOnly;
                ////temp = 1;
                //try
                //{
                //    Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
                //    NamedItem namedItem = NamedItem.GetUnitByName(components.Instance["UNIT"].StringValue);
                //    IECClass @class = SchemaUtilities.PlantSchema.GetClass(namedItem.NamedItemTypeName);
                //    temp = 1;
                //    Bentley.Plant.Utilities.DgnUtilities instance2 = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
                //    IECRelationshipInstance iECRelationshipInstance = (SchemaUtilities.PlantSchema.GetClass(namedItem.NamedItemRelationshipName) as IECRelationshipClass).CreateInstance() as IECRelationshipInstance;
                //    IECInstance sourceInstance = instance2.GetSourceInstance(@class, propertyValue);
                //    iECRelationshipInstance.Source = sourceInstance;
                //    iECRelationshipInstance.Target = components.Instance;
                //    components.Create();
                //    //components.Instance[propertyName].StringValue = propertyValue;

                //    //instance.SaveModifiedInstance(components.Instance, instance.GetDGNConnection());
                //}
                //catch (Exception e)
                //{

                //    System.Windows.Forms.MessageBox.Show(e.ToString());
                //}
                ////instance2.WriteECInstanceToDgn(iECRelationshipInstance, instance2.GetDGNConnection());
                ////instance2.SaveModifiedInstance(sourceInstance, instance2.GetDGNConnection());
                ////components.Instance["UNIT"].StringValue = propertyName;
                ////temp = 1;

                ////int hashcode = components.Instance.GetHashCode();
                ////components.Instance[propertyName].StringValue = propertyValue;
                ////int hashcode2 = components.Instance.GetHashCode();
                ////string propervalue = components.Instance[propertyName].StringValue;
                ////BMECInstanceManager instanceManager = BMECInstanceManager.Instance;
                ////instanceManager.CommitInstance(components.Instance);
                ////int hashcode3 = components.Instance.GetHashCode();
                ////components.Create();
                ////int hashcode4 = components.Instance.GetHashCode();

                ////instance2.WriteECInstanceToDgn(iECRelationshipInstance, instance2.GetDGNConnection());
                ////instance.SaveModifiedInstance(components.Instance, instance.GetDGNConnection());
                //temp = 1;
                //temp = 1;
                ////Bentley.Building.Mechanical.Api.BMECApi.Instance.InstanceManager.CommitInstance(instance);
                #endregion
                #region MyRegion

                //IECInstance equipInstance = components.Instance;
                //IECInstance equipInstanceAllInfo = api.FindAllInformationOnInstance(equipInstance);

                //if (equipInstanceAllInfo != null)
                //{
                //    foreach (IECRelationshipInstance current in equipInstanceAllInfo.GetRelationshipInstances())
                //    {
                //        if (current.ClassDefinition.Name.Equals("UNIT_HAS_NAMED_ITEM"))
                //        {

                //        }
                //    }
                //}

                #endregion
                #region MyRegion
                //ECInstanceList allInstance = DgnUtilities.GetAllInstancesFromDgn();
                //List<IECInstance> unitInstance = new List<IECInstance>();
                //foreach (IECInstance item in allInstance)
                //{
                //    if (item.ClassDefinition.Name == "UNIT")
                //    {
                //        unitInstance.Add(item);
                //    }
                //}
                //temp = 1;
                //foreach (IECInstance item in unitInstance)
                //{
                //    string descr = item["DESCRIPTION"].StringValue;
                //    IECInstance allInfoUnit = api.FindAllInformationOnInstance(item);
                //    if (allInfoUnit != null)
                //    {
                //        foreach (IECRelationshipInstance item1 in allInfoUnit.GetRelationshipInstances())
                //        {
                //            if (item1.ClassDefinition.Name == "UNIT_HAS_NAMED_ITEM")
                //            {

                //            }
                //        }
                //    }
                //}

                #endregion
                #region MyRegion

                //List<IECInstance> arg_634_0 = api.GetRelatedInstances(components.Instance, "EQUIPMENT_HAS_NOZZLE", true);
                //List<OPMCommon.Equipment.DatumStructure> datumList = api.FindDatums(components.Instance);
                //List<IECInstance>.Enumerator enumerator7 = arg_634_0.GetEnumerator();
                //if (enumerator7.MoveNext())
                //{
                //    do
                //    {
                //        IECInstance current5 = enumerator7.Current;
                //        IECInstance iECInstance2 = api.FindAllInformationOnInstance(current5);
                //        api.ComputeLocationVectors(iECInstance2, datumList);
                //        new BMECObject(iECInstance2).Create();
                //    }
                //    while (enumerator7.MoveNext());
                //}

                #endregion

                //ModifyBMECObject(components.Instance, propertyName, propertyValue);
            }

            return false;
        }

        public static bool ModifyBMECObjectUnitService(ref IECInstance component, string UnitValue, string ServiceValue)
        {
            //int temp = 1;
            Bentley.Plant.Utilities.DgnUtilities dgnutilities = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            if (dgnutilities.IsECInstanceFromReferenceFile(component))
            {
                return false;
            }
            try
            {
                #region 查找 Unit、Service 的 Instance
                ECInstanceList allInstance = DgnUtilities.GetAllInstancesFromDgn();
                Dictionary<string, IECInstance> allUnitInstanceDic = GetAllEquipmentUnit();
                Dictionary<string, IECInstance> allServiceInstanceDic = GetAllEquipmentService();
                string UnitplantRelationName = plantBreakDownRelationDic["UNIT"];
                string UnitplantValue = UnitValue;
                string ServiceplantRelationName = plantBreakDownRelationDic["SERVICE"];
                string ServiceplantValue = ServiceValue;
                #endregion
                #region 修改 Relationship
                IECInstance componentinstanceclone = ECInstanceHelper.CloneInstanceGraph(component);

                IECInstance allInfoComponent = api.FindAllInformationOnInstance(/*component.Instance*/componentinstanceclone);

                if (null == allInfoComponent) return false;
                IECRelationshipInstanceCollection componentRelationshipInstance = allInfoComponent.GetRelationshipInstances();

                IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                if (!componentRelationshipEnumerator.MoveNext())
                {
                    Bentley.EC.Persistence.ChangeSet changeSet = new Bentley.EC.Persistence.ChangeSet();
                    bool unitflag,serviceflag;
                    if (!allUnitInstanceDic.ContainsKey(UnitplantValue)) return false;
                    IECInstance uintsourceInstance = allUnitInstanceDic[UnitplantValue];
                    if (!allServiceInstanceDic.ContainsKey(ServiceplantValue)) return false;
                    IECInstance servicesourceInstance = allServiceInstanceDic[ServiceplantValue];
                    unitflag = CommandsList.createSourcetHasTargetRelationship(changeSet, uintsourceInstance, componentinstanceclone, "UNIT_HAS_NAMED_ITEM");
                    serviceflag = CommandsList.createSourcetHasTargetRelationship(changeSet, servicesourceInstance, componentinstanceclone, "SERVICE_IS_RELATED_TO_NAMED_ITEM");

                    if (unitflag&& serviceflag)
                    {
                        if (null == PersistenceManager.GetInstance())
                        {
                            PersistenceManager.Initialize(BPU.DgnUtilities.GetInstance().GetDGNConnectionForPipelineManager());
                        }
                        PersistenceManager.GetInstance().CommitChangeSet(changeSet);
                    }
                    if (changeSet != null)
                    {
                        changeSet.Dispose();
                    }

                    allInfoComponent = api.FindAllInformationOnInstance(/*component.Instance*/componentinstanceclone);

                    if (null == allInfoComponent) return false;
                    componentRelationshipInstance = allInfoComponent.GetRelationshipInstances();
                    componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                }
                else
                {
                    bool UnitChanged = false, ServiceChanged = false;
                    IECRelationshipInstance UnitRelationship = null, ServiceRelationship = null;
                    do
                    {
                        IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                        if (currentRelationship.ClassDefinition.Name.Contains("UNIT"))
                        {
                            if (!allUnitInstanceDic.ContainsKey(UnitplantValue)) return false;
                            IECInstance sourceInstance = allUnitInstanceDic[UnitplantValue];
                            currentRelationship.Source = sourceInstance;
                            UnitRelationship = currentRelationship;
                            UnitChanged = true;
                        }
                        else if (currentRelationship.ClassDefinition.Name.Contains("SERVICE"))
                        {
                            if (!allServiceInstanceDic.ContainsKey(ServiceplantValue)) return false;
                            IECInstance sourceInstance = allServiceInstanceDic[ServiceplantValue];
                            currentRelationship.Source = sourceInstance;
                            ServiceRelationship = currentRelationship;
                            ServiceChanged = true;
                        }
                    }
                    while (componentRelationshipEnumerator.MoveNext());

                    if (UnitChanged && ServiceChanged)
                    {
                        ArrayList ecInstances = new ArrayList();
                        ecInstances.Add(UnitRelationship);
                        ecInstances.Add(ServiceRelationship);

                        dgnutilities.SaveModifiedInstances(ecInstances, dgnutilities.GetDGNConnection());
                        IECInstance newModifyInstance = DgnUtilities.GetInstanceByInstanceId(component.InstanceId);
                        //string temp = component.GetString(propertyName);
                        //string temp2 = newModifyInstance.GetString(propertyName);
                        component = newModifyInstance;
                    }
                    else if (UnitChanged)
                    {
                        if (UnitRelationship != null)
                        {
                            dgnutilities.SaveModifiedInstance(UnitRelationship, dgnutilities.GetDGNConnection());
                            IECInstance newModifyInstance = DgnUtilities.GetInstanceByInstanceId(component.InstanceId);
                            //string temp = component.GetString(propertyName);
                            //string temp2 = newModifyInstance.GetString(propertyName);
                            component = newModifyInstance;
                        }
                    }
                    else if (ServiceChanged)
                    {
                        if (ServiceRelationship != null)
                        {
                            dgnutilities.SaveModifiedInstance(ServiceRelationship, dgnutilities.GetDGNConnection());
                            IECInstance newModifyInstance = DgnUtilities.GetInstanceByInstanceId(component.InstanceId);
                            //string temp = component.GetString(propertyName);
                            //string temp2 = newModifyInstance.GetString(propertyName);
                            component = newModifyInstance;
                        }

                    }
                }
                
                #endregion
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                return false;
            }
            return false;
        }

        private static Dictionary<string, string> plantBreakDownRelationDic = new Dictionary<string, string>() {
            ["UNIT"] = "UNIT_HAS_NAMED_ITEM",
            ["SERVICE"] = "SERVICE_IS_RELATED_TO_NAMED_ITEM",
        };
        //修改设备属性
        public static void ModifyBMECObject(IECInstance component, string propertyName, string propertyValue)
        {
            //
            try
            {
                #region MyRegion
                //int temp = 1;
                ECInstanceList allInstance = DgnUtilities.GetAllInstancesFromDgn();
                Dictionary<string, IECInstance> allInstanceDic = new Dictionary<string, IECInstance>();
                string plantRelationName = plantBreakDownRelationDic[propertyName];
                string plantName = propertyValue;
                foreach (IECInstance instance in allInstance)
                {
                    if (instance.ClassDefinition.Name == propertyName)
                    {
                        allInstanceDic.Add(instance["NAME"].StringValue, instance);
                    }
                }
                #endregion
                #region MyRegion
                IECInstance allInfoComponent = api.FindAllInformationOnInstance(component);
                if (null == allInfoComponent) return;
                IECRelationshipInstanceCollection componentRelationshipInstance = allInfoComponent.GetRelationshipInstances();
                IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                while (componentRelationshipEnumerator.MoveNext())
                {
                    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                    IECInstance sourceInstance = allInstanceDic[plantName];
                    currentRelationship.Source = sourceInstance;
                    //Bentley.Building.Mechanical.Api.BMECApi.Instance.InstanceManager.CommitInstance(modifyInstance);
                }
                //temp = 1;
                IECRelationshipInstanceCollection temprelation = allInfoComponent.GetRelationshipInstances();
                IEnumerator<IECRelationshipInstance> tempcomponentRelationshipEnumerator = temprelation.GetEnumerator();
                while (tempcomponentRelationshipEnumerator.MoveNext())
                {
                    IECRelationshipInstance currentRelationship = tempcomponentRelationshipEnumerator.Current;
                    if (!currentRelationship.ClassDefinition.Name.Equals(plantRelationName)) continue;
                    //IECInstance sourceInstance = allInstanceDic[plantName];
                    //currentRelationship.Source = sourceInstance;
                    //temp = 1;
                }
                //temp = 1;
                BMECObject newEquipment = new BMECObject(allInfoComponent);
                //temp = 1;
                newEquipment.Create();
                //":56FF00000001:15950A0000"
                //":56FF00000001:15950A0000"
                //temp = 1;
                //EquipmentModifyTool myEquipmentModifyTool = new EquipmentModifyTool();
                //myEquipmentModifyTool.InstallTool();
                //myEquipmentModifyTool.modifyEquipment(allInfoComponent, propertyName, propertyValue);
                //Utilities.ComApp.CommandState.StartDefaultCommand();
                //temp = 1;
                //temp = 1;
                //temp = 1;
                //temp = 1;
                #endregion

                #region MyRegion
                //temp = 1;

                //IECRelationshipInstanceCollection componentRelationshipInstance = components.GetRelationshipInstances();
                //IEnumerator<IECRelationshipInstance> componentRelationshipEnumerator = componentRelationshipInstance.GetEnumerator();
                //while (componentRelationshipEnumerator.MoveNext())
                //{
                //    IECRelationshipInstance currentRelationship = componentRelationshipEnumerator.Current;
                //    if (currentRelationship.ClassDefinition.Name != "UNIT_HAS_NAMED_ITEM") continue;
                //    foreach (IECInstance instance in allUnitInstance)
                //    {
                //        string unitName = instance["NAME"].StringValue;
                //        if (unitName == "equipUnit1")
                //        {

                //        }
                //        IECInstance allInfoInstance = api.FindAllInformationOnInstance(instance);
                //        IECRelationshipInstanceCollection plantBreakDownElementRelationship = allInfoInstance.GetRelationshipInstances();
                //        IEnumerator<IECRelationshipInstance> plantBreakDownRelationshipInstance = plantBreakDownElementRelationship.GetEnumerator();
                //        while (plantBreakDownRelationshipInstance.MoveNext())
                //        {
                //            if (plantBreakDownRelationshipInstance.Current.InstanceId == currentRelationship.InstanceId)
                //            {
                //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"
                //                //":56FF00000001:1561050000::::56FF00000001:15950A0000:::OpenPlant_3D.01.08:::UNIT_HAS_NAMED_ITEM"

                //            }
                //        }
                //        //IECRelationshipClass;//获取当前 Unit、Service 的关系结构定义
                //    }
                //}
                #endregion

                #region MyRegion

                //EquipmentModifyTool equipmentModifyTool = new EquipmentModifyTool();
                //equipmentModifyTool.InstallTool();
                //equipmentModifyTool.modifyEquipment(components, propertyName, propertyValue);

                ////Utilities.ComApp.CommandState.StartDefaultCommand();
                #endregion

                #region MyRegion

                //Bentley.Plant.Utilities.DgnUtilities instance = Bentley.Plant.Utilities.DgnUtilities.GetInstance();

                //IECInstance instanceForCurrentSelection = this.getInstanceForCurrentSelection(e.RowIndex, text);//当前选择的 Unit、Service实例

                //IECRelationshipInstanceCollection relationshipInstances = components.GetRelationshipInstances();//选择的设备的所有 Relationship
                //IEnumerator<IECRelationshipInstance> enumerator2 = relationshipInstances.GetEnumerator();

                //IECRelationshipClass eCRelationshipClassInformationForCurrentSelection = this.getECRelationshipClassInformationForCurrentSelection(e.RowIndex);//获取当前选择的 Unit、Service 的 Relationship 结构

                //bool flag = false;
                //while (enumerator2.MoveNext())
                //{
                //    //找到要修改的 Relationship 实例，并修改其关系
                //    IECRelationshipInstance current2 = enumerator2.Current;
                //    if (current2.ClassDefinition == eCRelationshipClassInformationForCurrentSelection)
                //    {
                //        //TODO
                //        //if (text == LocalizableStrings.None || string.IsNullOrEmpty(text))//选择列表判空
                //        //{
                //        //    relationshipInstances.Remove(current2);
                //        //}
                //        else
                //        {
                //            current2.Source = instanceForCurrentSelection;
                //        }
                //        flag = true;
                //        break;
                //    }
                //}
                //if (!flag && instanceForCurrentSelection != null)
                //{
                //    IECRelationshipInstance iECRelationshipInstance = eCRelationshipClassInformationForCurrentSelection.CreateInstance() as IECRelationshipInstance;
                //    iECRelationshipInstance.Source = instanceForCurrentSelection;
                //    iECRelationshipInstance.Target = components;
                //    components.GetRelationshipInstances().Add(iECRelationshipInstance);
                //}
                //IECInstance iECInstance = instanceForCurrentSelection;
                //if (iECInstance != null && (instance.IsECInstanceFromReferenceFile(iECInstance) || instance.IsECInstanceReferencedOut(iECInstance) || iECInstance.IsReadOnly))
                //{
                //    iECInstance = null;
                //}
                ////(base["optionEdit", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                ////(base["optionDelete", e.RowIndex] as DataGridViewImageCell).Tag = iECInstance;
                ////IECPropertyValue propertyValue = BusinessKeyUtility.GetPropertyValue(current);
                ////if (propertyValue != null)
                ////{
                ////    current.OnECPropertyValueChanged(propertyValue);
                ////}

                ////this.SaveSettings();

                #endregion
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }

        }
        //获取当前 Model 中所有的设备
        public static List<IECInstance> GetAllEquipment(bool isActiveModel=true) {
            List<IECInstance> allEquipments = new List<IECInstance>();
            ECInstanceList allInstanceFromDgn = DgnUtilities.GetAllInstancesFromDgn();
            foreach (IECInstance item in allInstanceFromDgn)
            {
                if (api.InstanceDefinedAsClass(item, "EQUIPMENT", true))
                {
                    if (isActiveModel)
                    {
                        BMECObject bmecobject = new BMECObject(item);
                        ulong bmecobject_id = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_id(bmecobject);
                        ElementId elem_id = new ElementId(ref bmecobject_id);
                        Element elem =Session.Instance.GetActiveDgnModel().FindElementById(elem_id);
                        if (elem != null)
                        {
                            bmecobject.Refresh();
                            allEquipments.Add(bmecobject.Instance);
                        }
                    }
                    else
                    {
                        allEquipments.Add(item);
                    }
                    
                }
            }
            return allEquipments;
        }

        public static List<BMECObject> GetAllEquipment2() {
            List<BMECObject> selectedpipes = new List<BMECObject>();
            List<BMECObject> tempECObject = new List<BMECObject>();
            ModelElementsCollection elements = Session.Instance.GetActiveDgnModel().GetGraphicElements();
            foreach (Element element in elements)
            {
                BMECObject ec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(element.ElementId);
                tempECObject.Add(ec_object);
            }
            foreach (BMECObject ecobject in tempECObject)
            {
                if (ecobject!= null && ecobject.Instance != null)
                {
                    if (api.InstanceDefinedAsClass(ecobject.Instance, "EQUIPMENT", true))
                    {
                        selectedpipes.Add(ecobject);
                    }
                }
            }
            return selectedpipes;
        }
        //TODO 获取 UNIT
        public static Dictionary<string, IECInstance> GetAllEquipmentUnit()
        {
            Bentley.Plant.Utilities.DgnUtilities dgnutilities = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            Dictionary<string, IECInstance> result = new Dictionary<string, IECInstance>();
            ECInstanceList allUnitInstances = DgnUtilities.GetInstancesFromDgn("UNIT");
            foreach (IECInstance unitInstance in allUnitInstances)
            {
                if (!dgnutilities.IsECInstanceFromReferenceFile(unitInstance))
                {
                    result.Add(unitInstance["NAME"].StringValue, unitInstance);
                }
            }
            return result;
        }
        //TODO 获取 SERVICE
        public static Dictionary<string,IECInstance> GetAllEquipmentService()
        {
            Bentley.Plant.Utilities.DgnUtilities dgnutilities = Bentley.Plant.Utilities.DgnUtilities.GetInstance();
            Dictionary<string, IECInstance> result = new Dictionary<string, IECInstance>();
            ECInstanceList allServiceInstances = DgnUtilities.GetInstancesFromDgn("SERVICE");
            foreach (IECInstance serviceInstance in allServiceInstances)
            {
                if (!dgnutilities.IsECInstanceFromReferenceFile(serviceInstance))
                {
                    result.Add(serviceInstance["NAME"].StringValue,serviceInstance);
                }
            }
            return result;
        }
        //TODO 获取 设备编号
        public static List<string> GetAllEquipmentBitNumber()
        {
            List<string> result = new List<string>();
            return result;
        }
        //TODO 获取设备的管嘴实例 IECInstance
        public static List<IECInstance> GetAllNozzleInstance(IECInstance equipmentInstance) {
            List<IECInstance> nozzles = new List<IECInstance>();
            IECInstance allInfoEquipmentInstance = api.FindAllInformationOnInstance(equipmentInstance);
            if (null != allInfoEquipmentInstance)
            {
                foreach (IECRelationshipInstance relationshipInstance in allInfoEquipmentInstance.GetRelationshipInstances())
                {
                    if (relationshipInstance.ClassDefinition.Name.Equals("EQUIPMENT_HAS_DATUM"))
                    {

                    }
                    if (relationshipInstance.ClassDefinition.Name.Equals("EQUIPMENT_HAS_NOZZLE"))
                    {
                        nozzles.Add(relationshipInstance.Target);
                    }
                    if (relationshipInstance.ClassDefinition.Name.Equals("EQUIPMENT_HAS_PORT"))
                    {

                    }
                }
            }

            return nozzles;
        }
        //TODO
        public static void CreateBMECObject(BMECObject components, string propertyName, string propertyValue) {
            components.SetStringValue(propertyName, propertyValue);
            components.Create();
        }
    }
    //变更属性的暂用工具，此类已废弃
    public class EquipmentModifyTool : DgnElementSetTool
    {
        protected override void OnPostInstall()
        {
            base.OnPostInstall();
        }

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
        }

        public void modifyEquipment(IECInstance components, string propertyName, string propertyValue)
        {
            BMECObject bmECObject = new BMECObject(components);
            bmECObject.Create();
        }
        public void modifyEquipment(BMECObject components, string propertyName, string propertyValue)
        {
            EquipmentManager.CreateBMECObject(components, propertyName, propertyValue);
            //components.SetStringValue(propertyName, propertyValue);
            //components.Create();
            //EquipmentManager.ModifyBMECObject(components, propertyName, propertyValue);
        }
        public void CreateBMECObject(BMECObject component) {
            component.Create();
        }
    }

}
