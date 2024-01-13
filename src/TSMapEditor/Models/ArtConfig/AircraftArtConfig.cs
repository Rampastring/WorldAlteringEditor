using Rampastring.Tools;

namespace TSMapEditor.Models.ArtConfig
{
    public class AircraftArtConfig : IArtConfig
    {
        public bool Remapable => true;
        public int Facings { get; set; } = 8;

        public void ReadFromIniSection(IniSection iniSection)
        {
            if (iniSection == null)
                return;

            Facings = iniSection.GetIntValue(nameof(Facings), Facings);
        }
    }
}