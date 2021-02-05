using Rampastring.Tools;
using System;

namespace TSMapEditor.Models.ArtData
{
    public class BuildingArtConfig
    {
        public BuildingArtConfig() { }

        public int FoundationX { get; set; }
        public int FoundationY { get; set; }
        public int Height { get; set; }
        public bool Remapable { get; set; }

        public void ReadFromIniSection(IniSection iniSection)
        {
            if (iniSection == null)
                return;

            string foundationString = iniSection.GetStringValue("Foundation", string.Empty);
            if (!string.IsNullOrWhiteSpace(foundationString))
            {
                string[] foundationParts = foundationString.Split('x');
                if (foundationParts.Length != 2)
                {
                    throw new InvalidOperationException("Invalid Foundation= specified in Art.ini section " + iniSection.SectionName);
                }

                FoundationX = Conversions.IntFromString(foundationParts[0], 0);
                FoundationY = Conversions.IntFromString(foundationParts[1], 0);
            }

            Height = iniSection.GetIntValue("Height", 0);
            Remapable = iniSection.GetBooleanValue("Remapable", false);
        }
    }
}
