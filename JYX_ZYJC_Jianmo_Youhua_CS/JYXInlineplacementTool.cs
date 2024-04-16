using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using Bentley.OpenPlant.Modeler.Api;
using System;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    internal class JYXInlineplacementTool : InlinePlacementTool
    {
        public JYXInlineplacementTool(JYXInlineplacementTool valveTool) : base(valveTool)
        {
        }

        public JYXInlineplacementTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }

        public override IPropertyContainerView CreateContainerView()
        {
            return new ValveView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
        }

        public override void CreateTool()
        {
            new JYXInlineplacementTool(this).InstallTool();
        }

        public override void OnRestartCommand()
        {
            base.RestartCommand(new JYXInlineplacementTool(this));
        }

    }
}
