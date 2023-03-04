namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'vehicle', but let's respect the original game's naming.
    /// </summary>
    public class Unit : Foot<UnitType>
    {
        public Unit(UnitType objectType) : base(objectType)
        {
        }

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public UnitType UnitType => ObjectType;
        public int FollowsID { get; set; } = -1;
        public Unit FollowedUnit { get; set; }

        public override int GetFrameIndex(int frameCount)
        {
            return GetStandingFrame(frameCount);
        }

        public int GetTurretFrameIndex() => UnitType.GetTurretStartFrame() + GetTurretFacingIndex();

        private int GetTurretFacingIndex()
        {
            // Seems that there is no standard way of doing things with this engine...
            int facingIndex = Facing / (256 / UnitType.ArtConfig.Facings);
            facingIndex += 4;
            facingIndex = facingIndex % UnitType.ArtConfig.Facings;
            return facingIndex;
        }

        private int GetFacingIndex()
        {
            int facingIndex = 0;
            if (UnitType.ArtConfig.Facings == 8)
            {
                facingIndex = Facing / 32;
            }
            else if (UnitType.ArtConfig.Facings > 1)
            {
                facingIndex = Facing / (256 / UnitType.ArtConfig.Facings);

                if (facingIndex >= UnitType.ArtConfig.Facings)
                    facingIndex = 0;
            }

            return facingIndex;
        }

        private int GetStandingFrame(int frameCount)
        {
            int facingIndex = GetFacingIndex();

            if (!UnitType.AdvancedFacingsHack)
            {
                // The legacy facing hack has facing #31 as frame#0 and actually frame#1 is north-east,
                // so offset facing index by one to take this into account
                facingIndex++;

                if (facingIndex >= UnitType.ArtConfig.Facings)
                    facingIndex = 0;
            }
            else
            {
                // Facings start to the north instead of north-east, we need to offset the facing index accordingly
                // by one "full facing" (one-eight of 256). How many frames this means depends on how many facings the unit has.

                facingIndex += (UnitType.ArtConfig.Facings / 8);

                // If the facing index turned out-of-bounds, spin it around
                if (facingIndex >= UnitType.ArtConfig.Facings)
                    facingIndex -= UnitType.ArtConfig.Facings;
            }

            if (UnitType.ArtConfig.StartStandFrame == -1)
            {
                if (UnitType.ArtConfig.StartWalkFrame == -1)
                {
                    return facingIndex;
                }

                // Use start walk frames
                return UnitType.ArtConfig.StartWalkFrame + (UnitType.ArtConfig.WalkFrames * facingIndex);
            }

            // Use stand frames
            return UnitType.ArtConfig.StartStandFrame + (facingIndex * UnitType.ArtConfig.StandingFrames);
        }

        public override bool Remapable() => ObjectType.ArtConfig.Remapable;

        public override int GetShadowFrameIndex(int frameCount)
        {
            return GetFrameIndex(frameCount) + frameCount / 2;
        }
    }
}
