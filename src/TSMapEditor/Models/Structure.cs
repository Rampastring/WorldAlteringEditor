using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.GameMath;
using TSMapEditor.Models.ArtConfig;

namespace TSMapEditor.Models
{
    public class Structure : Techno<BuildingType>
    {
        // [Structures]
        // INDEX=OWNER,ID,HEALTH,X,Y,FACING,TAG,AI_SELLABLE,AI_REBUILDABLE,POWERED_ON,UPGRADES,SPOTLIGHT,UPGRADE_1,UPGRADE_2,UPGRADE_3,AI_REPAIRABLE,NOMINAL

        public const int MaxUpgradeCount = 3;

        public Structure(BuildingType objectType) : base(objectType)
        {
            var anims = new List<Animation>();
            foreach (var animType in objectType.ArtConfig.Anims)
            {
                if (!animType.RenderInEditor)
                    continue;

                var animArtConfig = objectType.ArtConfig.BuildingAnimConfigs.Find(x => x.ININame == animType.ININame)
                                    ?? new BuildingAnimArtConfig();

                anims.Add(new Animation(animType)
                {
                    IsBuildingAnim = true,
                    ParentBuilding = this,
                    BuildingAnimDrawConfig = new BuildingAnimDrawConfig
                    {
                        X = animArtConfig.X,
                        Y = animArtConfig.Y,
                        YSort = animArtConfig.YSort,
                        ZAdjust = animArtConfig.ZAdjust
                    }
                });
            }
            Anims = anims.ToArray();

            if (objectType.Turret && !objectType.TurretAnimIsVoxel && objectType.ArtConfig.TurretAnim != null)
            {
                TurretAnim = new Animation(objectType.ArtConfig.TurretAnim)
                {
                    IsBuildingAnim = true,
                    IsTurretAnim = true,
                    ParentBuilding = this,
                    BuildingAnimDrawConfig = new BuildingAnimDrawConfig
                    {
                        X = objectType.TurretAnimX,
                        Y = objectType.TurretAnimY,
                        YSort = objectType.TurretAnimYSort,
                        ZAdjust = objectType.TurretAnimZAdjust
                    }
                };
            }

            UpdatePowerUpAnims();
        }

        public override RTTIType WhatAmI() => RTTIType.Building;

        private Point2D _position;
        public override Point2D Position
        {
            get => _position;
            set
            {
                _position = value;

                foreach (var anim in Anims)
                    anim.Position = value;

                foreach (var powerUpAnim in PowerUpAnims)
                {
                    if (powerUpAnim != null)
                    {
                        powerUpAnim.Position = value;
                    }
                }

                if (TurretAnim != null)
                    TurretAnim.Position = value;
            }
        }

        private House _owner;
        public override House Owner
        {
            get => _owner;
            set
            {
                _owner = value;

                foreach (var anim in Anims)
                    anim.Owner = value;

                foreach (var powerUpAnim in PowerUpAnims)
                {
                    if (powerUpAnim != null)
                    {
                        powerUpAnim.Owner = value;
                    }
                }

                if (TurretAnim != null)
                    TurretAnim.Owner = value;
            }
        }

        private byte _facing;
        public override byte Facing
        {
            get => _facing;
            set
            {
                _facing = value;

                foreach (var powerUpAnim in PowerUpAnims)
                {
                    if (powerUpAnim != null)
                    {
                        powerUpAnim.Facing = value;
                    }
                }

                if (TurretAnim != null)
                    TurretAnim.Facing = value;
            }
        }

        public bool AISellable { get; set; }

        /// <summary>
        /// According to ModEnc, this is an obsolete key from Red Alert.
        /// </summary>
        public bool AIRebuildable { get; set; }
        public bool Powered { get; set; } = true;
        
        public SpotlightType Spotlight { get; set; }
        public BuildingType[] Upgrades { get; private set; } = new BuildingType[MaxUpgradeCount];
        public int UpgradeCount => Upgrades.Count(u => u != null);

        public bool AIRepairable { get; set; }
        public bool Nominal { get; set; }
        public Animation[] Anims { get; set; }
        public Animation[] PowerUpAnims { get; set; }
        public Animation TurretAnim { get; set; }

        /// <summary>
        /// If set, this object instance only exists as a dummy for rendering base nodes.
        /// It is not actually present on the map.
        /// </summary>
        public bool IsBaseNodeDummy { get; set; }

        public void UpdatePowerUpAnims()
        {
            var anims = new List<Animation>();

            for (int i = 0; i < Upgrades.Length; i++)
            {
                var upgrade = Upgrades[i];
                if (upgrade == null)
                    continue;

                string upgradeImage = upgrade.ArtConfig.Image;
                if (string.IsNullOrWhiteSpace(upgradeImage))
                    upgradeImage = upgrade.Image;
                if (string.IsNullOrWhiteSpace(upgradeImage))
                    upgradeImage = upgrade.ININame;

                var config = ObjectType.ArtConfig.PowerUpAnimConfigs[i];
                var animType = Array.Find(ObjectType.ArtConfig.PowerUpAnims, at => at.ININame == upgradeImage);

                if (animType == null)
                    continue;

                anims.Add(new Animation(animType)
                {
                    Position = this.Position,
                    Owner = this.Owner,
                    Facing = this.Facing,
                    IsBuildingAnim = true,
                    IsTurretAnim = upgrade.Turret && !upgrade.TurretAnimIsVoxel,
                    ParentBuilding = this,
                    BuildingAnimDrawConfig = new BuildingAnimDrawConfig
                    {
                        X = config.LocXX,
                        Y = config.LocYY,
                        YSort = config.YSort,
                        ZAdjust = config.LocZZ
                    }
                });
            }

            PowerUpAnims = anims.ToArray();
        }

        public override double GetCloakGeneratorRange()
        {
            return ObjectType.CloakGenerator ? ObjectType.CloakRadiusInCells : 0.0;
        }

        public override double GetSensorArrayRange()
        {
            return ObjectType.SensorArray ? ObjectType.CloakRadiusInCells : 0.0;
        }

        public override Color GetRadialColor()
        {
            return ObjectType.RadialColor.GetValueOrDefault(base.GetRadialColor());
        }

        public override int GetYDrawOffset()
        {
            return Constants.CellSizeY / -2;
        }

        public override int GetFrameIndex(int frameCount)
        {
            if (frameCount > 1 && HP < Constants.ConditionYellowHP)
                return 1;

            return 0;
        }

        public override int GetShadowFrameIndex(int frameCount)
        {
            if (HP < Constants.ConditionYellowHP)
                return frameCount / 2 + 1;

            return frameCount / 2;
        }

        public override bool Remapable() => ObjectType.ArtConfig.Remapable;

        public override Color GetRemapColor() => IsBaseNodeDummy ? base.GetRemapColor() * 0.25f : base.GetRemapColor();

        public override Structure Clone()
        {
            var clone = MemberwiseClone() as Structure;

            clone.Anims = Anims.Select(anim => anim.Clone() as Animation).ToArray();
            foreach (var anim in clone.Anims)
                anim.ParentBuilding = clone;

            if (TurretAnim != null)
            {
                clone.TurretAnim = TurretAnim.Clone() as Animation;
                clone.TurretAnim.ParentBuilding = clone;
            }

            clone.UpdatePowerUpAnims();

            return clone;
        }
    }
}
