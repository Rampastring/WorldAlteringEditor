using Rampastring.Tools;

namespace TSMapEditor.Models
{
    public class TaskForceTechnoEntry
    {
        public TechnoType TechnoType { get; set; }
        public int Count { get; set; }

        public static TaskForceTechnoEntry CreateEntry(Rules rules, string data)
        {
            string[] parts = data.Split(',');
            if (parts.Length != 2)
                return null;

            int count = Conversions.IntFromString(parts[0], -1);
            if (count < 1)
                return null;

            string objectININame = parts[1];
            TechnoType technoType = null;
            technoType = rules.AircraftTypes.Find(a => a.ININame == objectININame);

            if (technoType == null)
                technoType = rules.UnitTypes.Find(u => u.ININame == objectININame);

            if (technoType == null)
                technoType = rules.InfantryTypes.Find(i => i.ININame == objectININame);

            if (technoType == null)
                return null;

            return new TaskForceTechnoEntry()
            {
                Count = count,
                TechnoType = technoType
            };
        }
    }

    public class TaskForce
    {
        public const int MaxTechnoCount = 6;

        public TaskForce(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }

        public string Name { get; set; }

        public TaskForceTechnoEntry[] TechnoTypes = new TaskForceTechnoEntry[MaxTechnoCount];

        public int Group { get; set; } = -1;

        public static TaskForce ReadTaskForce(Rules rules, IniSection taskforceSection)
        {
            if (taskforceSection == null)
                return null;

            var taskForce = new TaskForce(taskforceSection.SectionName);
            taskForce.Group = taskforceSection.GetIntValue(nameof(Group), taskForce.Group);

            for (int i = 0; i < MaxTechnoCount; i++)
            {
                if (!taskforceSection.KeyExists(i.ToString()))
                    break;

                string value = taskforceSection.GetStringValue(i.ToString(), null);
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var entry = TaskForceTechnoEntry.CreateEntry(rules, value);
                if (entry == null)
                    break;

                taskForce.TechnoTypes[i] = entry;
            }

            return taskForce;
        }
    }
}
