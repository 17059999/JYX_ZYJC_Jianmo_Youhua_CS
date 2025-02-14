﻿using Bentley.DgnPlatformNET;
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
    class select_zhenggen_pipe : DgnElementSetTool
    {
        static BIM.Application app = Bentley.MstnPlatformNET.InteropServices.Utilities.ComApp;
        ShowPipingComponentInfosForm showpipingcomponentinfosform;
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }
        public static void InstallNewTool()
        {
            select_zhenggen_pipe select_zhenggen_pipe = new select_zhenggen_pipe();
            select_zhenggen_pipe.InstallTool();
        }
        protected override void OnPostInstall()
        {
            
            base.OnPostInstall();
            app.ShowCommand("选择整根管道");
            app.ShowPrompt("请选择管道\\管件");
        }
        protected override void OnCleanup()
        {
            if (showpipingcomponentinfosform != null && !showpipingcomponentinfosform.IsDisposed)
            {
                showpipingcomponentinfosform.Close();
            }
        }
        protected override bool OnResetButton(DgnButtonEvent ev)
        {

            app.CommandState.StartDefaultCommand();

            return true;
        }
        protected override void OnRestartTool()
        {
            
            InstallNewTool();
        }
        protected override bool NeedAcceptPoint()
        {
            return false;
        }
        #region 第二版更改
        //        protected override bool OnDataButton(DgnButtonEvent ev)
        //        {
        //            HitPath hit_path = DoLocate(ev, true, 1);
        //            if (hit_path == null)
        //            {
        //                return true;
        //            }
        //            Element elem = hit_path.GetHeadElement();

        //            BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
        //            if (bmec_object == null)
        //            {
        //                System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
        //                return true;
        //            }
        //            else if (BMECApi.Instance.InstanceDefinedAsClass(bmec_object.Instance, "PIPING_COMPONENT", true))
        //            {
        //                string pipe_line = bmec_object.Instance["LINENUMBER"].StringValue;
        //                select_components_by_pipelinename(pipe_line);
        //            }
        //            else
        //            {
        //                System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
        //                return true;
        //            }
        //            return true;
        //        }

        //        private void select_components_by_pipelinename(string pipe_line)
        //        {
        //            PipelineUtilities _Utilities = new PipelineUtilities();
        //            Hashtable pipelines = _Utilities.GetAllPipelines();
        //            ArrayList allComponents = new ArrayList();
        //            bool pipeline_has_finded = false;
        //            List<BMECObject> unconnected_components = new List<BMECObject>();
        //            List<BMECObject> result_bmec_object_list = new List<BMECObject>();
        //            foreach (PipelineCacheData pipelinecachedata in pipelines.Values)
        //            {
        //                if (pipelinecachedata.Name.Equals(pipe_line))
        //                {
        //                    pipeline_has_finded = true;
        //                    allComponents = pipelinecachedata.GetAllComponents(true);
        //                }
        //            }
        //            if (!pipeline_has_finded)
        //            {
        //                MessageBox.Show("当前图纸管线管理器未包含'" + pipe_line + "'的定义,无法使用该功能");
        //                return ;
        //            }
        //            else
        //            {
        //                for (int i = 0; i < allComponents.Count; i++)
        //                {
        //                    IECInstance iECInstance = allComponents[i] as IECInstance;
        //                    BMECObject bmecobject = new BMECObject(iECInstance);
        //                    if (bmecobject.ConnectedComponents.Count == 0)
        //                    {
        //                        unconnected_components.Add(bmecobject);
        //                    }
        //                    else
        //                    {
        //                        result_bmec_object_list.Add(bmecobject);
        //                        for (int m = 0; m < bmecobject.Ports.Count; m++)
        //                        {
        //                            IECInstance joint_instance = BMECApi.Instance.GetJointInstance(bmecobject.Ports[m]);
        //                            if (joint_instance != null)
        //                            {
        //                                List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);

        //                                foreach (BMECObject joint_bmecobject in joint_bmecobject_list)
        //                                {

        //                                    BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
        //                                    {
        //                                        return (exit_object_temp.Instance.InstanceId.Equals(joint_bmecobject.Instance.InstanceId));
        //                                    });
        //                                    if (exit_object == null)
        //                                    {
        //                                        result_bmec_object_list.Add(joint_bmecobject);
        //                                    }
        //                                }

        //                                if (bmecobject.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].Name.Equals("VALVE") || bmecobject.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].BaseClasses[0].Name.Equals("INLINE_VALVE"))
        //                                {
        //                                    ECInstanceList ec_list = BMECApi.Instance.GetRelatedInstancesByStrength(bmecobject.Instance, "VALVE_HAS_VALVE_OPERATING_DEVICE", (Bentley.ECObjects.Schema.StrengthType)(-1), true);
        //                                    if (ec_list.Count != 0)
        //                                    {
        //                                        foreach (BMECObject exit_object_temp in result_bmec_object_list)
        //                                        {
        //                                            if (!(exit_object_temp.Instance.InstanceId.Equals(ec_list[0].InstanceId)))
        //                                            {
        //                                                result_bmec_object_list.Add(new BMECObject(ec_list[0]));
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                foreach (BMECObject connected_bmecobject in result_bmec_object_list)
        //                {
        //                    BMECApi.Instance.SelectComponent(connected_bmecobject.Instance, true);
        //                }
        //                if (unconnected_components.Count > 0)
        //                {
        //                    //MessageBox.Show("管线编号为'" + pipe_line + "'的构件中，无连接性的构件个数为" + unconnected_components.Count.ToString());
        //                    showpipingcomponentinfosform = new ShowPipingComponentInfosForm(unconnected_components);
        //#if DEBUG

        //#else
        //                    showpipingcomponentinfosform.AttachAsTopLevelForm(MyAddin.s_addin, true);
        //                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowPipingComponentInfosForm));
        //                    showpipingcomponentinfosform.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        //#endif

        //                    showpipingcomponentinfosform.Show();
        //                }
        //            }
        //        }
        #endregion

        #region 前版本需求功能
        //protected override bool OnDataButton(DgnButtonEvent ev)
        //{
        //    HitPath hit_path = DoLocate(ev, true, 1);
        //    if (hit_path == null)
        //    {
        //        return true;
        //    }
        //    Element elem = hit_path.GetHeadElement();

        //    BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
        //    if (bmec_object == null)
        //    {
        //        System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
        //        return true;
        //    }
        //    else if (BMECApi.Instance.InstanceDefinedAsClass(bmec_object.Instance, "PIPING_COMPONENT", true))
        //    {
        //        List<BMECObject> result = new List<BMECObject>();
        //        List<BMECObject> connected_bmecobject_list = bmec_object.ConnectedComponents;
        //        string pipe_line = bmec_object.Instance["LINENUMBER"].StringValue;


        //        if (connected_bmecobject_list.Count == 0)
        //        {
        //            BMECApi.Instance.SelectComponent(bmec_object.Instance, true);
        //            return true;
        //        }

        //        get_all_extend_connectedComponents(bmec_object, result, pipe_line);


        //        foreach (BMECObject connected_bmecobject in result)
        //        {
        //            BMECApi.Instance.SelectComponent(connected_bmecobject.Instance, true);
        //        }

        //    }
        //    else
        //    {
        //        System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
        //        return true;
        //    }
        //    return true;
        //}

        //public void get_all_extend_connectedComponents(BMECObject bmec_object, List<BMECObject> result_bmec_object_list,string pipe_line)
        //{
        //    result_bmec_object_list.Add(bmec_object);
        //    for (int i = 0; i < bmec_object.Ports.Count; i++)
        //    {
        //        IECInstance joint_instance = BMECApi.Instance.GetJointInstance(bmec_object.Ports[i]);
        //        if (joint_instance != null)
        //        {
        //        List<BMECObject> joint_bmecobject_list = BMECApi.Instance.GetJointObjects(joint_instance);

        //        foreach (BMECObject joint_bmecobject in joint_bmecobject_list)
        //        {

        //            BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
        //            {
        //                return (exit_object_temp.Instance.InstanceId.Equals(joint_bmecobject.Instance.InstanceId));
        //            });
        //            if (exit_object==null)
        //            {
        //                result_bmec_object_list.Add(joint_bmecobject);
        //            }
        //        }

        //            if (bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].Name.Equals("VALVE") || bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].BaseClasses[0].Name.Equals("INLINE_VALVE"))
        //            {
        //                ECInstanceList ec_list = BMECApi.Instance.GetRelatedInstancesByStrength(bmec_object.Instance, "VALVE_HAS_VALVE_OPERATING_DEVICE", (Bentley.ECObjects.Schema.StrengthType)(-1), true);
        //                if (ec_list.Count != 0)
        //                {
        //                    foreach (BMECObject exit_object_temp in result_bmec_object_list)
        //                    {
        //                        if (!(exit_object_temp.Instance.InstanceId.Equals(ec_list[0].InstanceId)))
        //                        {
        //                            result_bmec_object_list.Add(new BMECObject(ec_list[0]));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    List<BMECObject> connected_bmec_object_list = bmec_object.ConnectedComponents;
        //    foreach (BMECObject bmec_object_temp in connected_bmec_object_list)
        //    {
        //        BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
        //         {
        //             return (exit_object_temp.Instance.InstanceId.Equals(bmec_object_temp.Instance.InstanceId));
        //         });
        //        if(exit_object==null)
        //        {
        //            if (bmec_object_temp.Instance["LINENUMBER"].StringValue.Equals(pipe_line))
        //            {
        //                get_all_extend_connectedComponents(bmec_object_temp, result_bmec_object_list, pipe_line);
        //            }
        //        }

        //    }
        //}
        #endregion

        #region 新版本需求功能
        protected override bool OnDataButton(DgnButtonEvent ev)
        {
            HitPath hit_path = DoLocate(ev, true, 0);
            if (hit_path == null)
            {
                return true;
            }
            Element elem = hit_path.GetHeadElement();

            BMECObject bmec_object = JYX_ZYJC_CLR.PublicMethod.get_bmecobject_by_id(elem.ElementId);
            if (bmec_object == null)
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
                return true;
            }
            else if (BMECApi.Instance.InstanceDefinedAsClass(bmec_object.Instance, "PIPING_COMPONENT", true))
            {
                List<BMECObject> result_bmec_object_list = new List<BMECObject>();
                List<BMECObject> connected_bmecobject_list = bmec_object.ConnectedComponents;
                string pipe_line = bmec_object.Instance["LINENUMBER"].StringValue;


                if (connected_bmecobject_list.Count == 0)
                {
                    BMECApi.Instance.SelectComponent(bmec_object.Instance, true);
                    return true;
                }

                get_all_extend_connectedComponents(bmec_object, result_bmec_object_list, pipe_line);


                foreach (BMECObject connected_bmecobject in result_bmec_object_list)
                {
                    BMECApi.Instance.SelectComponent(connected_bmecobject.Instance, true);
                }
                PipelineUtilities _Utilities = new PipelineUtilities();
                Hashtable pipelines = _Utilities.GetAllPipelines();
                ArrayList allComponents = new ArrayList();

                List<BMECObject> unconnected_components = new List<BMECObject>();

                foreach (PipelineCacheData pipelinecachedata in pipelines.Values)
                {
                    if (pipelinecachedata.Name.Equals(pipe_line))
                    {
                        allComponents = pipelinecachedata.GetAllComponents(true);
                    }
                }

                for (int i = 0; i < allComponents.Count; i++)
                {
                    IECInstance iECInstance = allComponents[i] as IECInstance;

                    BMECObject exit_bmecobject = result_bmec_object_list.Find(delegate(BMECObject bmecobject) {
                        return (iECInstance.InstanceId== bmecobject.Instance.InstanceId);
                    });
                    if (exit_bmecobject==null)
                    {
                        unconnected_components.Add(new BMECObject(iECInstance));
                    }
                }
                if (unconnected_components.Count > 0)
                {
                    //MessageBox.Show("管线编号为'" + pipe_line + "'的构件中，无连接性的构件个数为" + unconnected_components.Count.ToString());
                    showpipingcomponentinfosform = new ShowPipingComponentInfosForm(unconnected_components);
                    #if !DEBUG
                    showpipingcomponentinfosform.AttachAsTopLevelForm(MyAddin.s_addin, true);
                    System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShowPipingComponentInfosForm));
                    showpipingcomponentinfosform.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
                    #endif
                    showpipingcomponentinfosform.Show();
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("选择的组件不是管道\\管件，请重新选择！");
                return true;
            }
            return true;
        }

        public void get_all_extend_connectedComponents(BMECObject bmec_object, List<BMECObject> result_bmec_object_list, string pipe_line)
        {
            result_bmec_object_list.Add(bmec_object);
            for (int i = 0; i < bmec_object.Ports.Count; i++)
            {
                IECInstance port_instance = BMECApi.Instance.GetConnectedPortInstance(bmec_object.Ports[i]);
                if (port_instance != null)
                {
                    IECInstance joint_instance = BMECApi.Instance.GetRelatedJointForPort(port_instance);
                    if (joint_instance!=null)
                    {
                        ECInstanceList fasteners = BMECApi.Instance.GetSealAndFastenersForJoint(joint_instance);

                        if (fasteners.Count != 0)
                        {
                            foreach (IECInstance fastener in fasteners)
                            {

                                BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
                                {
                                    return (exit_object_temp.Instance.InstanceId.Equals(fastener.InstanceId));
                                });
                                if (exit_object == null)
                                {
                                    result_bmec_object_list.Add(BMECApi.Instance.CreateBMECObjectForInstance(fastener));
                                }
                            }
                        }
                    }
                    
                    if (bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].Name.Equals("VALVE") || bmec_object.Instance.ClassDefinition.BaseClasses[0].BaseClasses[0].BaseClasses[0].Name.Equals("INLINE_VALVE"))
                    {
                        ECInstanceList ec_list = BMECApi.Instance.GetRelatedInstances(bmec_object.Instance, "VALVE_HAS_VALVE_OPERATING_DEVICE", true, true);
                        if (ec_list.Count != 0)
                        {
                            BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
                            {
                                return (exit_object_temp.Instance.InstanceId.Equals(ec_list[0].InstanceId));
                            });
                            if (exit_object == null)
                            {
                                result_bmec_object_list.Add(new BMECObject(ec_list[0]));
                            }
                        }
                    }
                }
            }

            List<BMECObject> connected_bmec_object_list = bmec_object.ConnectedComponents;
            foreach (BMECObject bmec_object_temp in connected_bmec_object_list)
            {
                BMECObject exit_object = result_bmec_object_list.Find(delegate (BMECObject exit_object_temp)
                 {
                     return (exit_object_temp.Instance.InstanceId.Equals(bmec_object_temp.Instance.InstanceId));
                 });
                if (exit_object == null)
                {
                    if (bmec_object_temp.Instance["LINENUMBER"].StringValue.Equals(pipe_line))
                    {
                        get_all_extend_connectedComponents(bmec_object_temp, result_bmec_object_list, pipe_line);
                    }
                }

            }
        }
        #endregion
    }
}
