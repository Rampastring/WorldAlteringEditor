namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'vehicle', but let's respect the original game's naming.
    /// </summary>
    public class Unit : Foot<UnitType>
    {
        public Unit(UnitType objectType) : base(objectType)
        {
            UnitType = objectType;
        }

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public UnitType UnitType { get; }
        public int FollowsID { get; set; }
        public Unit FollowedUnit { get; set; }

        public override int GetFrameIndex(int frameCount)
        {
            if (UnitType.ArtConfig.Facings == 32)
            {
                int frame = 1 + Facing / 8;
                if (frame > 32)
                    frame = 0;
                return frame;
            }

            if (UnitType.ArtConfig.Facings == 8)
            {
                return Facing / 8;
            }

            return 0;
        }

        public override int GetShadowFrameIndex(int frameCount)
        {
            return GetFrameIndex(frameCount) + frameCount / 2;
        }
    }
}
