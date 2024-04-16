using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET.InteropServices;
using Bentley.OpenPlantModeler.SDK.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BIM = Bentley.Interop.MicroStationDGN;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Instance;
using Bentley.OpenPlant.Modeler.Api;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class refreshNozzle : DgnElementSetTool
    {
        static BIM.Application app = Utilities.ComApp;
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
            //throw new NotImplementedException();
        }

        protected override void OnPostInstall()
        {
            base.OnPostInstall();
            ECInstanceList ecListAll = DgnUtilities.GetAllInstancesFromDgn();
            ECInstanceList ecSx = new ECInstanceList();
            foreach (IECInstance ecIn in ecListAll)
            {
                bool b = BMECApi.Instance.InstanceDefinedAsClass(ecIn, "NOZZLE", true);
                if (b)
                {
                    ecSx.Add(ecIn);
                }
            }
            try
            {
                foreach (IECInstance ecinstance in ecSx)
                {
                    BMECObject bmec = new BMECObject(ecinstance);
                    bmec.Refresh();
                    bmec.Create();
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
                app.CommandState.StartDefaultCommand();
                return;
            }
            System.Windows.Forms.MessageBox.Show("刷新成功！");
            app.CommandState.StartDefaultCommand();
        }
    }
}
