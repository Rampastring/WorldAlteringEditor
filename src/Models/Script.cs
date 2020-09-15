using System.Collections.Generic;

namespace TSMapEditor.Models
{
    public struct ScriptActionEntry
    {
        public int Action { get; set; }
        public int Argument { get; set; }
    }

    public class Script : AbstractObject
    {
        public const int MaxActionCount = 50;

        public Script(string iniName)
        {
            ININame = iniName;
        }

        public override RTTIType WhatAmI() => RTTIType.ScriptType;

        public string ININame { get; }

        public string Name { get; set; }

        public List<ScriptActionEntry> Actions = new List<ScriptActionEntry>();
    }
}
