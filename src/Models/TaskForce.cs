namespace TSMapEditor.Models
{
    public class TaskForceTechnoEntry
    {
        public TechnoBase Techno { get; set; }
        public int Count { get; set; }
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

        public TaskForceTechnoEntry[] Technos = new TaskForceTechnoEntry[MaxTechnoCount];

        public int Group { get; set; } = -1;
    }
}
