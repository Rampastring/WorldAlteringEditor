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
            if (OverlayType.Tiberium || OverlayType.Wall)
                return Constants.CellSizeY / -2;

            return 0;
        }
    }
}
