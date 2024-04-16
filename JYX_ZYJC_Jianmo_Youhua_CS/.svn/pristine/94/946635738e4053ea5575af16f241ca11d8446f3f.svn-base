using Bentley.ApplicationFramework;
using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.ECObjects.Instance;
using Bentley.MstnPlatformNET;
using System;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class JYXCloupingPlacementTool : InlinePlacementTool
    {

        #region 构造方法

        public JYXCloupingPlacementTool(JYXCloupingPlacementTool valveTool) : base(valveTool)
        {
        }

        public JYXCloupingPlacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        #endregion

        public override IPropertyContainerView CreateContainerView()
        {
            return new CloupingView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
        }

        public override void CreateTool()
        {

            new JYXCloupingPlacementTool(this).InstallTool();
        }

    }
}
