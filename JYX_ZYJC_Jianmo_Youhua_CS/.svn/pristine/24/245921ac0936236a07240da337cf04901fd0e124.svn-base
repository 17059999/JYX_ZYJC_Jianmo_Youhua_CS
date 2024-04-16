using Bentley.ApplicationFramework.Interfaces;
using Bentley.Building.Mechanical;
using Bentley.MstnPlatformNET;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class ReliefValveTool : ValvePlacementTool
    {
        #region 构造方法

        public ReliefValveTool(ReliefValveTool valveTool) : base(valveTool)
        {
        }
        public ReliefValveTool(AddIn addIn, int cmdNumber) : base(addIn, cmdNumber)
        {
        }
        #endregion

        public override IPropertyContainerView CreateContainerView()
        {
            return new ValveView(base.AddIn, MechAddIn.Instance.GetLocalizedString("PlaceComponentCmdName"));
        }

        public override void CreateTool()
        {
            new ReliefValveTool(this).InstallTool();
        }
        public override void OnRestartCommand()
        {
            base.RestartCommand(new ReliefValveTool(this));
        }
    }
}
