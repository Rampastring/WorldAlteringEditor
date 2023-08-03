using Microsoft.Xna.Framework;
using Rampastring.Tools;
using Rampastring.XNAUI;
using System;
using System.Globalization;
using System.Text;

namespace TSMapEditor.Models
{
    public class TeamType : AbstractObject, IIDContainer, IHintable
    {
        public TeamType(string iniName)
        {
            ININame = iniName;
        }

        public string GetInternalID() => ININame;
        public void SetInternalID(string id) => ININame = id;

        public string ININame { get; private set; }

        public string Name { get; set; }
        public int Group { get; set; } = -1;
        public House House { get; set; }
        public Script Script { get; set; }
        public TaskForce TaskForce { get; set; }
        public Tag Tag { get; set; }
        public int Max { get; set; }
        public bool Full { get; set; }
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

        public string GetHeaderText() => Name;

        public string GetHintText()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Owner: " + (House == null ? Constants.NoneValue2 : House.ININame));
            stringBuilder.Append(Environment.NewLine + Environment.NewLine);
            stringBuilder.Append("Script: " + (Script == null ? Constants.NoneValue2 : Script.Name));
            stringBuilder.Append(Environment.NewLine + Environment.NewLine);

            if (Tag != null)
            {
                stringBuilder.Append("Tag: " + Tag.Name);
                stringBuilder.Append(Environment.NewLine + Environment.NewLine);
            }

            stringBuilder.Append("Waypoint: " + (string.IsNullOrWhiteSpace(Waypoint) ? Constants.NoneValue2 : Helpers.GetWaypointNumberFromAlphabeticalString(Waypoint)));
            stringBuilder.Append(Environment.NewLine + Environment.NewLine);

            if (VeteranLevel > 1)
            {
                stringBuilder.Append("Veteran Level: " + (VeteranLevel > 2 ? "Elite" : "Veteran"));
                stringBuilder.Append(Environment.NewLine + Environment.NewLine);
            }

            if (TaskForce == null)
            {
                stringBuilder.Append("No TaskForce set");
            }
            else
            {
                stringBuilder.Append(TaskForce.GetHintText());
            }

            stringBuilder.Append(Environment.NewLine + Environment.NewLine);
            AppendFlag(stringBuilder, nameof(Max), Max);
            AppendFlag(stringBuilder, nameof(Full), Full);
            AppendFlag(stringBuilder, nameof(Whiner), Whiner);
            AppendFlag(stringBuilder, nameof(Droppod), Droppod);
            AppendFlag(stringBuilder, nameof(Suicide), Suicide);
            AppendFlag(stringBuilder, nameof(Loadable), Loadable);
            AppendFlag(stringBuilder, nameof(Prebuild), Prebuild);
            AppendFlag(stringBuilder, nameof(Priority), Priority);
            AppendFlag(stringBuilder, nameof(Annoyance), Annoyance);
            AppendFlag(stringBuilder, nameof(IonImmune), IonImmune);
            AppendFlag(stringBuilder, nameof(Recruiter), Recruiter);
            AppendFlag(stringBuilder, nameof(Reinforce), Reinforce);
            AppendFlag(stringBuilder, nameof(Aggressive), Aggressive);
            AppendFlag(stringBuilder, nameof(Autocreate), Autocreate);
            AppendFlag(stringBuilder, nameof(GuardSlower), GuardSlower);
            AppendFlag(stringBuilder, nameof(OnTransOnly), OnTransOnly);
            AppendFlag(stringBuilder, nameof(AvoidThreats), AvoidThreats);
            AppendFlag(stringBuilder, nameof(LooseRecruit), LooseRecruit);
            AppendFlag(stringBuilder, nameof(IsBaseDefense), IsBaseDefense);
            AppendFlag(stringBuilder, nameof(OnlyTargetHouseEnemy), OnlyTargetHouseEnemy);
            AppendFlag(stringBuilder, nameof(TransportsReturnOnUnload), TransportsReturnOnUnload);
            AppendFlag(stringBuilder, nameof(AreTeamMembersRecruitable), AreTeamMembersRecruitable);

            return stringBuilder.ToString();
        }

        private void AppendFlag<T>(StringBuilder stringBuilder, string name, T flagValue, T defValue = default) where T : struct
        {
            if (!flagValue.Equals(defValue))
            {
                if (flagValue.GetType() == typeof(bool))
                {
                    // Bool fields are only listed if their value is 'true', so just telling their name is enough
                    stringBuilder.Append(name);
                    stringBuilder.Append(Environment.NewLine);
                }
                else
                {
                    stringBuilder.Append(name + ": " + flagValue.ToString());
                    stringBuilder.Append(Environment.NewLine);
                }
            }
        }

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

        public Color GetXNAColor()
        {
            if (House == null || House.HasDarkHouseColor())
                return UISettings.ActiveSettings.AltColor;

            return House.XNAColor;
        }
    }
}
