namespace TSMapEditor.Models
{
    /// <summary>
    /// A base class for mobile objects.
    /// </summary>
    public abstract class Foot : Techno
    {
        public const int VeterancyNone = 0;
        public const int VeterancyVeteran = 100;
        public const int VeterancyElite = 200;


        public Mission Mission { get; set; }
        public bool High { get; set; }
        public int Veterancy { get; set; }
        public int Group { get; set; } = -1;

        /// <summary>
        /// Is this unit available for recruitment for TeamTypes that have Autocreate=no?
        /// </summary>
        public bool AutocreateNoRecruitable { get; set; }

        /// <summary>
        /// Is this unit available for recruitment for TeamTypes that have Autocreate=yes?
        /// </summary>
        public bool AutocreateYesRecruitable { get; set; }
    }
}
