using Rampastring.Tools;
using System.Collections.Generic;

namespace TSMapEditor.Models
{
    public class ScriptActionEntry
    {
        public int Action { get; set; }
        public int Argument { get; set; }

        public static ScriptActionEntry ParseScriptActionEntry(string data)
        {
            string[] parts = data.Split(',');
            if (parts.Length != 2)
                return null;

            int action = Conversions.IntFromString(parts[0], -1);
            if (action < 0)
                return null;

            int argument = Conversions.IntFromString(parts[1], -1);

            return new ScriptActionEntry() { Action = action, Argument = argument };
        }
    }

    public class Script
    {
        public const int MaxActionCount = 50;

        public Script(string iniName)
        {
            ININame = iniName;
        }

        public string ININame { get; }

        public string Name { get; set; }

        public List<ScriptActionEntry> Actions = new List<ScriptActionEntry>();

        public void WriteToIniSection(IniSection scriptSection)
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                scriptSection.SetStringValue(i.ToString(), $"{Actions[i].Action},{Actions[i].Argument}");
            }

            scriptSection.SetStringValue("Name", Name);
        }

        public static Script ParseScript(string id, IniSection scriptSection)
        {
            if (string.IsNullOrWhiteSpace(id) || scriptSection == null)
                return null;

            var script = new Script(id);
            script.Name = scriptSection.GetStringValue("Name", string.Empty);

            for (int i = 0; i < MaxActionCount; i++)
            {
                if (!scriptSection.KeyExists(i.ToString()))
                    break;

                var scriptActionEntry = ScriptActionEntry.ParseScriptActionEntry(scriptSection.GetStringValue(i.ToString(), "-1,-1"));
                if (scriptActionEntry != null)
                    script.Actions.Add(scriptActionEntry);
            }

            return script;
        }
    }
}
