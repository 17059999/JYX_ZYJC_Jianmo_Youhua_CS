using Bentley.DgnPlatformNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bentley.DgnPlatformNET.Elements;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class MergePipeTool : DgnElementSetTool
    {
        public override StatusInt OnElementModify(Element element)
        {
            return StatusInt.Error;
        }

        protected override void OnRestartTool()
        {
        }

        protected override void OnPostInstall()
        {
            CutOffPipe cutOffPipe = new CutOffPipe();
            cutOffPipe.mergePipe();
            this.ExitTool();
        }
    }
}
