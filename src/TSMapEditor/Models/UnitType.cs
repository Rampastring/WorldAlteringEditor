using System.Collections.Generic;
using TSMapEditor.Models.ArtConfig;

namespace TSMapEditor.Models
{
    /// <summary>
    /// Could also be called 'VehicleType', but let's respect the original game's naming.
    /// </summary>
    public class UnitType : TechnoType, IArtConfigContainer
    {
        public const int STANDARD_STANDING_FRAME_COUNT = 8;

        public UnitType(string iniName) : base(iniName)
        {
        }

        public VehicleArtConfig ArtConfig { get; private set; } = new VehicleArtConfig();
        public IArtConfig GetArtConfig() => ArtConfig;

        public override RTTIType WhatAmI() => RTTIType.Unit;

        public bool Turret { get; set; }


        public int GetTurretStartFrame()
        {
            return STANDARD_STANDING_FRAME_COUNT * ArtConfig.WalkFrames;
        }

        public List<int> GetIdleFrameIndexes()
        {
            var returnValue = new List<int>();

            if (ArtConfig.StartStandFrame == -1)
            {
                if (ArtConfig.StartWalkFrame == -1)
                {
                    for (int i = 0; i < ArtConfig.Facings; i++)
                        returnValue.Add(i);
                }
                else
                {
                    // Use walk frames
                    for (int i = 0; i < ArtConfig.Facings; i++)
                    {
                        returnValue.Add(ArtConfig.StartWalkFrame + i * ArtConfig.WalkFrames);
                    }
                }
            }
            else
            {
                // Use StartStandFrame
                for (int i = 0; i < ArtConfig.Facings; i++)
                {
                    returnValue.Add(ArtConfig.StartStandFrame + i * ArtConfig.StandingFrames);
                }
            }

            return returnValue;
        }
    }
}
