using System;

namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'vehicle', but let's respect the original game's naming.
    /// </summary>
    public class Unit : Foot<UnitType>
    {
        private const int STANDARD_STANDING_FRAME_COUNT = 8;

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
            return GetStandingFrame(frameCount);
        }

        public int GetTurretFrameIndex()
        {
            int facingIndex = GetTurretFacingIndex();
            int framesToSkip = UnitType.ArtConfig.WalkFrames;

            return STANDARD_STANDING_FRAME_COUNT * framesToSkip + facingIndex;
        }

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
                facingIndex = 1 + Facing / (256 / UnitType.ArtConfig.Facings);
                if (facingIndex > UnitType.ArtConfig.Facings)
                    facingIndex = 0;
            }

            return facingIndex;
        }

        private int GetStandingFrame(int frameCount)
        {
            int facingIndex = GetFacingIndex();

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

        public override int GetShadowFrameIndex(int frameCount)
        {
            return GetFrameIndex(frameCount) + frameCount / 2;
        }
    }
}
