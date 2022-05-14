using Rampastring.Tools;

namespace TSMapEditor.Models
{
    public class TeamType : AbstractObject
    {
        public TeamType(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; private set; }

        public int Max { get; set; }
        public bool Full { get; set; }
        public string Name { get; set; }
        public int Group { get; set; } = -1;
        public House House { get; set; }
        public Script Script { get; set; }
        public TaskForce TaskForce { get; set; }
        public Tag Tag { get; set; }
        public bool Whiner { get; set; }
        public bool Droppod { get; set; }
        public bool Suicide { get; set; }
        public bool Loadable { get; set; }
        public bool Prebuild { get; set; }
        public int Priority { get; set; }
        public string Waypoint { get; set; } = "A";
        public bool Annoyance { get; set; }
        public bool IonImmune { get; set; }
        public bool Recruiter { get; set; }
        public bool Reinforce { get; set; }
        public int TechLevel { get; set; }
        public bool Aggressive { get; set; }
        public bool Autocreate { get; set; }
        public bool GuardSlower { get; set; }
        public bool OnTransOnly { get; set; }
        public bool AvoidThreats { get; set; }
        public bool LooseRecruit { get; set; }
        public int VeteranLevel { get; set; }
        public bool IsBaseDefense { get; set; }
        public bool OnlyTargetHouseEnemy { get; set; }
        public bool TransportsReturnOnUnload { get; set; }
        public bool AreTeamMembersRecruitable { get; set; }

        /// <summary>
        /// Creates and returns a clone of this TeamType.
        /// </summary>
        /// <param name="iniName">The INI name of the created TeamType.</param>
        public TeamType Clone(string iniName)
        {
            var clone = MemberwiseClone() as TeamType;
            clone.ININame = iniName;
            clone.Name = "Clone of " + Name;

            // This class has no members that we'd need to deep clone
            return clone;
        }

        public void WriteToIniSection(IniSection iniSection)
        {
            // This cuts it for all properties of standard types
            WritePropertiesToIniSection(iniSection);

            if (House != null)
                iniSection.SetStringValue("House", House.ININame);
            if (Script != null)
                iniSection.SetStringValue("Script", Script.ININame);
            if (TaskForce != null)
                iniSection.SetStringValue("TaskForce", TaskForce.ININame);
            if (Tag != null)
                iniSection.SetStringValue("Tag", Tag.ID);
        }

        public override RTTIType WhatAmI()
        {
            return RTTIType.TeamType;
        }
    }
}
