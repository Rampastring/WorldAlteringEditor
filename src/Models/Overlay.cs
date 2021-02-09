using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class Overlay : GameObject
    {
        public override RTTIType WhatAmI() => RTTIType.Overlay;

        public OverlayType OverlayType { get; set; }
        public int FrameIndex { get; set; }


        public override int GetFrameIndex(int frameCount)
        {
            return FrameIndex;
        }

        public override int GetShadowFrameIndex(int frameCount)
        {
            return 100;
        }

        public override int GetYDrawOffset()
        {
            return Constants.CellSizeY / -2;
        }
    }
}
