using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using Bentley.MstnPlatformNET;
using BCOM = Bentley.DgnPlatformNET.Elements;
using Bentley.DgnPlatformNET.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bentley.MstnPlatformNET.InteropServices;

namespace JYX_ZYJC_Jianmo_Youhua_CS
{
    class CreateDimensionCallbacks : DimensionCreateData
    {
        private DimensionStyle m_dimStyle;
        private DgnTextStyle m_textStyle;
        private Symbology m_symbology;
        private LevelId m_levelId;
        private DirectionFormatter m_directionFormatter;

        public CreateDimensionCallbacks(DimensionStyle dimStyle, DgnTextStyle textStyle, Symbology symb, LevelId levelId, DirectionFormatter formatter)
        {
            m_dimStyle = dimStyle;
            m_textStyle = textStyle;
            m_symbology = symb;
            m_levelId = levelId;
            m_directionFormatter = formatter;
        }

        public override DimensionStyle GetDimensionStyle()
        {
            return m_dimStyle;
        }

        public override DgnTextStyle GetTextStyle()
        {
            return m_textStyle;
        }

        public override Symbology GetSymbology()
        {
            return m_symbology;
        }

        public override LevelId GetLevelId()
        {
            return m_levelId;
        }

        public override int GetViewNumber()
        {
            return 0;
        }

        public override DMatrix3d GetDimensionRotation()
        {
            return DMatrix3d.Identity;
        }

        public override DMatrix3d GetViewRotation()
        {
            return DMatrix3d.Identity;
        }

        public override DirectionFormatter GetDirectionFormatter()
        {
            return m_directionFormatter;
        }
    }
}
