using Rampastring.Tools;

namespace TSMapEditor.Initialization
{
    /// <summary>
    /// Contains static methods for writing a map to a INI file.
    /// </summary>
    public static class MapWriter
    {
        private static IniSection FindOrMakeSection(string sectionName, IniFile mapIni)
        {
            var section = mapIni.GetSection(sectionName);
            if (section == null)
            {
                section = new IniSection(sectionName);
                mapIni.AddSection(section);
            }

            return section;
        }

        public static void WriteMapSection(IMap map, IniFile mapIni)
        {
            const string sectionName = "Map";

            var section = FindOrMakeSection(sectionName, mapIni);
            section.SetStringValue("Size", $"0,0,{map.Size.X},{map.Size.Y}");
            section.SetStringValue("Theater", map.Theater);
            section.SetStringValue("LocalSize", $"{map.LocalSize.X},{map.LocalSize.Y},{map.LocalSize.Width},{map.LocalSize.Height}");

            mapIni.AddSection(section);
        }

        public static void WriteBasicSection(IMap map, IniFile mapIni)
        {
            const string sectionName = "Basic";

            var section = FindOrMakeSection(sectionName, mapIni);
            map.Basic.WritePropertiesToIniSection(section);
        }
    }
}
