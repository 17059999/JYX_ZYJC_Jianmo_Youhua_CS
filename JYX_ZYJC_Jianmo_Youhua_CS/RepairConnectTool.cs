using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.OpenPlant.Modeler.Api;
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class RepairConnectTool : DgnElementSetTool
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;

        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            InstallNewTool();
        }

        public static void InstallNewTool()
        {
            RepairConnectTool updateconnecttool = new RepairConnectTool();
            updateconnecttool.InstallTool();
        }

        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            app.ShowCommand("更新连接性");
            app.ShowPrompt("请选择管件");
        }

        protected override bool OnResetButton(DgnButtonEvent ev)
        {
            app.CommandState.StartDefaultCommand();

            return true;
        }
        protected override bool IsModifyOriginal()
        {
            return false;
        }

        protected override bool NeedAcceptPoint()
        {
            return false;
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            base.OnDataButton(ev);
            HitPath hit_path = DoLocate(ev, true, 0);
            if (hit_path == null)
            {
                return true;
            }
            Element elem = hit_path.GetHeadElement();

            BMECObject pipe = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

            if (pipe == null)
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是管件，请重新选择！");
                return true;
            }
            else if (BMECApi.Instance.InstanceDefinedAsClass(pipe.Instance, "PIPING_COMPONENT", true))
            {
                string err = "";
                string ljfs = pipe.Ports[1].Instance["END_PREPARATION"].StringValue;
                string ljfs1 = pipe.Ports[0].Instance["END_PREPARATION"].StringValue;
                updateConnectTool(pipe,out err);
                System.Windows.Forms.MessageBox.Show(err);
                app.ShowPrompt("请选择管件");
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是管件，请重新选择！");
            }

            return true;

        }

        public static void updateConnectTool(BMECObject bmec,out string err)
        {
            try
            {
                #region 缺少焊点则添加焊点
                if (bmec.Ports!=null&&bmec.Ports.Count>0)
                {
                    if (bmec.ConnectedComponents != null && bmec.ConnectedComponents.Count > 0)
                    {
                        for (int i = 0; i < bmec.Ports.Count; i++)
                        {
                            IECInstance portInstance = bmec.Ports[i].Instance;

                            IECInstance allInformationPortInstance = BMECApi.Instance.FindAllInformationOnInstance(portInstance);

                            string jointTypeName = "";

                            if (null != allInformationPortInstance)
                            {
                                foreach (IECRelationshipInstance current2 in allInformationPortInstance.GetRelationshipInstances())
                                {
                                    //port 的连接性
                                    if (current2.ClassDefinition.Name.Equals("PORT_HAS_JOINT") && current2.Source.InstanceId == allInformationPortInstance.InstanceId)//找到port的相关组件
                                    {
                                        IECInstance targetJoint = current2.Target;
                                        List<string> fastenerName = new List<string>();
                                        if (targetJoint != null)
                                        {
                                            jointTypeName = targetJoint["TYPE"].StringValue;
                                            IECPropertyValue myJointType = ElementClear.FindJointTypeByJointName(jointTypeName);
                                            ElementClear.GetJointTypeInfo(myJointType, out fastenerName);
                                        }

                                        List<IECInstance> fastenerInstance = new List<IECInstance>();
                                        //System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = api.GetRelatedInstancesByStrength(current2.Target, (StrengthType)(-1)).GetEnumerator();
                                        System.Collections.Generic.List<IECInstance>.Enumerator enumerator3 = BMECApi.Instance.GetRelatedInstancesByDirection(current2.Target, true).GetEnumerator();
                                        while (enumerator3.MoveNext())
                                        {
                                            IECInstance current3 = enumerator3.Current;
                                            fastenerInstance.Add(current3);
                                        }

                                        bool isWeld = false;
                                        isWeld = fastenerName.Contains("WELD");
                                        if(fastenerName.Count>fastenerInstance.Count&&isWeld)
                                        {
                                            IECInstance weld_iec_instance = BMECInstanceManager.Instance.CreateECInstance("WELD", true);
                                            BMECObject weld_object = new BMECObject(weld_iec_instance);

                                            DTransform3d tran = DTransform3d.FromMatrixAndTranslation(weld_object.Transform3d.Matrix, bmec.GetNthPort(i).LocationInUors);
                                            weld_object.Transform3d = tran;
                                            weld_object.CopyDoubleValue(bmec.Instance, "NOMINAL_DIAMETER");
                                            weld_object.Instance["LINENUMBER"].StringValue = bmec.Instance["LINENUMBER"].StringValue;
                                            weld_object.Create();
                                            IECInstance relatedISOSheet = BMECApi.Instance.GetRelatedISOSheetForComponent(bmec.Instance);
                                            if (null != relatedISOSheet)
                                            {
                                                BMECApi.Instance.AssociateComponentWithISOSheet(weld_object.Instance, relatedISOSheet);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                

                bmec.DiscoverConnectionsEx();
                bmec.UpdateConnections();
                err = "更新成功！";
            }
            catch
            {
                err = "更新失败！";
            }
        }
    }
}
