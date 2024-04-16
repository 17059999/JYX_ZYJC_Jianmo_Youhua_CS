using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using System;
using Bentley.OpenPlant.Modeler.Api;
using System.Collections.Generic;
using BIM = Bentley.Interop.MicroStationDGN;
using BG = Bentley.GeometryNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.ECObjects.Instance;
using System.Windows.Forms;
using Bentley.Plant.StandardPreferences;
using Bentley.OpenPlantModeler.SDK.Components;
using Bentley.ECObjects;
using Bentley.Plant.PipelineManager;
using System.Collections;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class UpdateConnectTool : DgnElementSetTool
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        int point_n = 0;
        //BMECObject component1=null;
        //BMECObject component2=null;
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }
        public static void InstallNewTool()
        {
            UpdateConnectTool updateconnecttool = new UpdateConnectTool();
            updateconnecttool.InstallTool();
        }
        protected override void OnPostInstall()
        {

            base.OnPostInstall();
            app.ShowCommand("修复管嘴焊点");
            app.ShowPrompt("请选择管道");
        }
        protected override void OnCleanup()
        {
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
        protected override void OnRestartTool()
        {
            InstallNewTool();
        }
        protected override bool NeedAcceptPoint()
        {
            return false;
        }

        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            HitPath hit_path = DoLocate(ev, true, 0);
            if (hit_path == null)
            {
                return true;
            }
            if (point_n == 0)
            {
                Element elem = hit_path.GetHeadElement();

                BMECObject pipe = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);

                if (pipe == null)
                {
                    System.Windows.Forms.MessageBox.Show("选择的组件不是管件，请重新选择！");
                    return true;
                }
                else if (BMECApi.Instance.InstanceDefinedAsClass(pipe.Instance, "PIPE", true))
                {
                    UpdatePipeAndNozzleConnect(pipe);
                    app.ShowPrompt("请选择管道");
                    return true;
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("选择的组件不是管道，请重新选择！");
                }
            }

            return true;

        }

        private bool UpdatePipeAndNozzleConnect(BMECObject pipe)
        {
            BMECApi api = BMECApi.Instance;
            int d =pipe.ConnectedComponents.Count;
            int nozzle_port_index = -1;
            int nozzle_index = -1;
            //IECInstance ciec =api.GetJointInstance(pipe.Ports[0]);
            //api.ValidateJointAtPort();
            //    api.UpdateJoint();
                
            //api.CreateJointForCompatiblePorts();
            List<List<BMECObject>> bmecobject_list_list = new List<List<BMECObject>>();

            for (int i=0;i<pipe.Ports.Count;i++)
            {
                BIM.Element[] elems =Mstn_Public_Api.scan_element_at_point(Mstn_Public_Api.DPoint3d_To_Point3d(pipe.GetNthPoint(i)), true);
                List<BMECObject> bmecobject_list = new List<BMECObject>();

                foreach (var elem in elems)
                {

                    IECInstance find_Instance =JYX_ZYJC_CLR.PublicMethod.FindInstance(elem);
                    if (find_Instance==null)
                    {
                        continue;
                    }
                    if (find_Instance.InstanceId== pipe.Instance.InstanceId)
                    {
                        continue;
                    }
                    if (find_Instance.ClassDefinition.Name.Equals("NOZZLE"))
                    {
                        nozzle_port_index = pipe.Ports[i].ID - 1;
                        nozzle_index = bmecobject_list.Count;
                    }
                    bmecobject_list.Add(new BMECObject(find_Instance));
                }
                bmecobject_list_list.Add(bmecobject_list);
            }
            if (nozzle_port_index == -1)
            {
                //pipe.DiscoverConnectionsEx();
                //pipe.UpdateConnections();
                MessageBox.Show("相邻处未找到管嘴!");
                return true;
            }
            else
            {
                //BMECObject nozzle_object = bmecobject_list_list[nozzle_port_index][nozzle_index];
                //api.CreateJointForIncompatiblePorts(pipe.Instance, nozzle_object.Instance,pipe.GetNthPoint(nozzle_port_index));
                IECInstance weld_iec_instance = BMECInstanceManager.Instance.CreateECInstance("WELD", true);
                BMECObject weld_object = new BMECObject(weld_iec_instance);
                
                BG.DTransform3d tran =BG.DTransform3d.FromMatrixAndTranslation(weld_object.Transform3d.Matrix, pipe.GetNthPoint(nozzle_port_index));
                weld_object.Transform3d= tran;
                weld_object.CopyDoubleValue(pipe.Instance, "NOMINAL_DIAMETER");
                weld_object.Instance["LINENUMBER"].StringValue = pipe.Instance["LINENUMBER"].StringValue;
                weld_object.Create();
                IECInstance relatedISOSheet = api.GetRelatedISOSheetForComponent(pipe.Instance);
                if (null != relatedISOSheet)
                {
                    api.AssociateComponentWithISOSheet(weld_object.Instance, relatedISOSheet);
                }

                //IECInstance relatedISOSheet2 = api.GetRelatedISOSheetForComponent(bmecobject_list_list[nozzle_port_index][0].Instance);
                //if (null != relatedISOSheet2)
                //{
                //    api.AssociateComponentWithISOSheet(weld_object.Instance, relatedISOSheet2);
                //}

                //pipe.DiscoverConnectionsEx();
                //pipe.UpdateConnections();
                //pipe.Create();
                //pipe.DiscoverConnectionsEx();
                //api.DoSettingsForFastenerUtility(pipe, pipe.Ports[nozzle_port_index]);//删除连接性

                //BMECObject nozzle_object = bmecobject_list_list[nozzle_port_index][nozzle_index];

                //api.ConnectObjectsAtPorts(pipe.Ports[nozzle_port_index], pipe, nozzle_object.Ports[0], nozzle_object);
            }
            //return true;
            for (int i = 0; i < bmecobject_list_list.Count; i++)
            {
                if (i == nozzle_port_index)
                {
                    foreach (BMECObject temp_object in bmecobject_list_list[i])
                    {
                        temp_object.DiscoverConnectionsEx();
                        temp_object.UpdateConnections();
                        //temp_object.Create();
                    }
                }
            }
            MessageBox.Show("修复完成!");
            return true;
        }
    }
}
