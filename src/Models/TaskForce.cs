namespace TSMapEditor.Models
{
    public class TaskForceTechnoEntry
    {
        public Techno Techno { get; set; }
        public int Count { get; set; }
    }

    public class TaskForce : AbstractObject
    {
        public const int MaxTechnoCount = 6;

        public TaskForce(string iniName)
        {
            ININame = iniName;
        }

        public override RTTIType WhatAmI() => RTTIType.TaskForce;

        public string ININame { get; }

        public string Name { get; set; }

        public TaskForceTechnoEntry[] Technos = new TaskForceTechnoEntry[MaxTechnoCount];

        public int Group { get; set; } = -1;
    }
}
