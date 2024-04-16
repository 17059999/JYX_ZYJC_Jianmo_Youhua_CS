﻿
using Bentley.ECObjects.Instance;
using Bentley.GeometryNET;
using Bentley.OpenPlant.Modeler.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class JacketedPipe
    {
        //public BMECObject createJacketedPipe(DPoint3d startPoint, DPoint3d endPoint) {
        //    BMECObject 
        //    return;
        //}
        public BMECObject createPipe(DPoint3d startPoint, DPoint3d endPoint, double nd) {
            IECInstance elbow_iec_instance = BMECInstanceManager.Instance.CreateECInstance("PIPE", true);//创建一个PIPE的ECInstance
            BMECApi api = BMECApi.Instance;
            ISpecProcessor specProcessor = api.SpecProcessor;
            specProcessor.FillCurrentPreferences(elbow_iec_instance, null);
            elbow_iec_instance["NOMINAL_DIAMETER"].DoubleValue = nd;//设置管径

            ECInstanceList ec_instance_list = specProcessor.SelectSpec(elbow_iec_instance, true);//选择数据
            BMECObject ec_object = new BMECObject();
            if (null != ec_instance_list && ec_instance_list.Count > 0)
            {
                IECInstance instance = ec_instance_list[0];
                ec_object = new BMECObject(instance);
                ec_object.SetLinearPoints(startPoint, endPoint);//设置管道起点与终点
                ec_object.Create();//将修改应用到程序
                ec_object.DiscoverConnectionsEx();
                ec_object.UpdateConnections();
                List<BMECObject> connectedComponents = ec_object.ConnectedComponents;
            }
            return ec_object;
        }


    }
}
