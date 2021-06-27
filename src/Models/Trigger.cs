using Rampastring.Tools;

namespace TSMapEditor.Models
{
    /// <summary>
    /// A map trigger.
    /// </summary>
    public class Trigger
    {
        public string ID { get; set; }
        public string House { get; set; }

        /// <summary>
        /// The linked trigger ID loaded from the map.
        /// Do not use post map loading.
        /// </summary>
        public string LinkedTriggerId { get; set; } = Constants.NoneValue2;
        public Trigger LinkedTrigger { get; set; }
        public string Name { get; set; }
        public bool Disabled { get; set; }
        public bool Easy { get; set; }
        public bool Normal { get; set; }
        public bool Hard { get; set; }

        public void WriteToIniSection(IniSection iniSection)
        {
            string linkedTriggerId = LinkedTrigger == null ? Constants.NoneValue2 : LinkedTrigger.ID;
            iniSection.SetStringValue(ID,
                $"{House},{linkedTriggerId},{Name}," +
                $"{Helpers.BoolToIntString(Disabled)}," +
                $"{Helpers.BoolToIntString(Easy)},{Helpers.BoolToIntString(Normal)},{Helpers.BoolToIntString(Hard)},0");
        }

        /// <summary>
        /// Parses and creates a trigger instance from a trigger data line
        /// in a Tiberian Sun / Red Alert 2 map file.
        /// </summary>
        /// <param name="id">The ID of the trigger.</param>
        /// <param name="data">The data line.</param>
        /// <returns>A Trigger instance if the parsing succeeds, otherwise null.</returns>
        public static Trigger ParseTrigger(string id, string data)
        {
            // [Triggers]
            // ID=HOUSE,LINKED_TRIGGER,NAME,DISABLED,EASY,NORMAL,HARD,REPEATING
            // https://modenc.renegadeprojects.com/Triggers
            // the 'REPEATING' field here is unused by the game, so we ignore it

            string[] parts = data.Split(',');
            if (parts.Length < 7)
                return null;

            return new Trigger()
            {
                ID = id,
                House = parts[0],
                LinkedTriggerId = parts[1],
                Name = parts[2],
                Disabled = Conversions.BooleanFromString(parts[3], false),
                Easy = Conversions.BooleanFromString(parts[4], true),
                Normal = Conversions.BooleanFromString(parts[5], true),
                Hard = Conversions.BooleanFromString(parts[6], true),
            };
        }
    }
}
