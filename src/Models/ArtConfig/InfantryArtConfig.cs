using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.Tools;

namespace TSMapEditor.Models.ArtConfig
{
    public class InfantryArtConfig : IArtConfig
    {
        public string SequenceName { get; private set; }
        public InfantrySequence Sequence { get; set; }
        public string Image { get; set; }
        public bool Remapable { get; private set; }

        public void ReadFromIniSection(IniSection iniSection)
        {
            SequenceName = iniSection.GetStringValue("Sequence", SequenceName);
            Image = iniSection.GetStringValue(nameof(Image), Image);
            Remapable = iniSection.GetBooleanValue(nameof(Remapable), Remapable);
        }
    }
}
