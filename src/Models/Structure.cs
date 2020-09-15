namespace TSMapEditor.Models
{
    public class Structure : Techno
    {
        // [Structures]
        // INDEX=OWNER,ID,HEALTH,X,Y,FACING,TAG,AI_SELLABLE,AI_REBUILDABLE,POWERED_ON,UPGRADES,SPOTLIGHT,UPGRADE_1,UPGRADE_2,UPGRADE_3,AI_REPAIRABLE,NOMINAL

        public const int MaxUpgradeCount = 3;

        public override RTTIType WhatAmI() => RTTIType.Building;

        public bool AISellable { get; set; }

        /// <summary>
        /// According to ModEnc, this is an obsolete key from Red Alert.
        /// </summary>
        public bool AIRebuildable { get; set; }
        public bool PoweredOn { get; set; }
        public bool Powered { get; set; }
        public int UpgradeCount { get; set; }
        public SpotlightType Spotlight { get; set; }
        public int[] UpgradeIds { get; private set; } = new int[MaxUpgradeCount];
        public Structure[] Upgrades { get; private set; } = new Structure[MaxUpgradeCount];
        public bool AIRepairable { get; set; }
        public bool Nominal { get; set; }
    }
}
