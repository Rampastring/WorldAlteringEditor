using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.Models
{
    public class AITriggerType
    {
        // [AITriggerTypes]
        // ID=Name,Team1,OwnerHouse,TechLevel,ConditionType,ConditionObject,Comparator,StartingWeight,MinimumWeight,MaximumWeight,IsForSkirmish,unused,Side,IsBaseDefense,Team2,EnabledInE,EnabledInM,EnabledInH

        public AITriggerType(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }

        public string Name { get; set; }
        public TeamType PrimaryTeam { get; set; }
        public House Owner { get; set; }
        public int TechLevel { get; set; }
        public AITriggerConditionType ConditionType { get; set; }
        public TechnoType ConditionObject { get; set; }
        public string Comparator { get; set; } // TODO structure this
        public double InitialWeight { get; set; }
        public double MinimumWeight { get; set; }
        public double MaximumWeight { get; set; }
        public bool ForMultiplayer { get; set; }
        public bool Unused { get; set; }

        /// <summary>
        /// The side of the AITrigger. In vanilla Tiberian Sun, 0 = all sides, 1 = GDI, 2 = Nod.
        /// </summary>
        public int Side { get; set; }
        public bool IsBaseDefense { get; set; }
        public TeamType SecondaryTeam { get; set; }
        public bool Easy { get; set; }
        public bool Medium { get; set; }
        public bool Hard { get; set; }
    }
}
