using Rampastring.Tools;

namespace TSMapEditor.Models.ArtConfig
{
    public class VehicleArtConfig : IArtConfig
    {
        public bool Voxel { get; set; }
        public bool Remapable { get; set; }
        public int StartStandFrame { get; set; }
        public int Facings { get; set; }

        public void ReadFromIniSection(IniSection iniSection)
        {
            if (iniSection == null)
                return;

            Voxel = iniSection.GetBooleanValue(nameof(Voxel), false);
            Remapable = iniSection.GetBooleanValue(nameof(Remapable), false);
            StartStandFrame = iniSection.GetIntValue(nameof(StartStandFrame), 0);
            Facings = iniSection.GetIntValue(nameof(Facings), 1);
        }
    }
}
