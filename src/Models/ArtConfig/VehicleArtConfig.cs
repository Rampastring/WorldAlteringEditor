using Rampastring.Tools;

namespace TSMapEditor.Models.ArtConfig
{
    public class VehicleArtConfig : IArtConfig
    {
        public bool Voxel { get; set; }
        public bool Remapable { get; set; }
        public int StartStandFrame { get; set; }
        public int Facings { get; set; } = 1;

        public void ReadFromIniSection(IniSection iniSection)
        {
            if (iniSection == null)
                return;

            Voxel = iniSection.GetBooleanValue(nameof(Voxel), Voxel);
            Remapable = iniSection.GetBooleanValue(nameof(Remapable), Remapable);
            StartStandFrame = iniSection.GetIntValue(nameof(StartStandFrame), StartStandFrame);
            Facings = iniSection.GetIntValue(nameof(Facings), Facings);
        }
    }
}
