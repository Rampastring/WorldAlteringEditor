using Rampastring.Tools;

namespace TSMapEditor.Models.ArtData
{
    public class OverlayArtConfig
    {
        public bool Theater { get; set; }

        public void ReadFromIniSection(IniSection iniSection)
        {
            Theater = iniSection.GetBooleanValue(nameof(Theater), Theater);
        }
    }
}
