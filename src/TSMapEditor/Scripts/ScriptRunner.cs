using Rampastring.Tools;
using System;
using System.IO;
using TSMapEditor.Models;
using Westwind.Scripting;

namespace TSMapEditor.Scripts
{
    public static class ScriptRunner
    {
        public static string RunScript(Map map, string scriptPath)
        {
            var iniFile = FindIniFileForScript(scriptPath);
            if (iniFile == null)
                return "The script was not found! Maybe it was deleted?";

            string source = iniFile.GetStringValue("$Code", "Value", null);
            if (source == null)
                return "An error occurred while running the script: the script file has no source code included!";

            string error = RunSource(map, source);

            if (error == null)
                return iniFile.GetStringValue("$Editor", "Success", "Script executed successfully.");

            return "An error occurred while running the script: " + Environment.NewLine + Environment.NewLine + error;

            // string source = "TSMapEditor.Models.Map map = (TSMapEditor.Models.Map)@0; map.Units.ForEach(u => { if (u.Owner.ININame == \"Civilians\") u.Mission = \"Sleep\"; });";
            // string source = "TSMapEditor.Models.Map map = (TSMapEditor.Models.Map)@0; int cc = map.GetCellCount(); System.Windows.Forms.MessageBox.Show(cc.ToString());";
            // string source = "Map map = (Map)@0; map.DoForAllTechnos(techno => { if (techno.WhatAmI() == RTTIType.Unit && techno.Owner.ININame == \"Civilians\") { var unit = (Unit)techno; unit.Mission = \"Sleep\"; } }); return null;";
        }

        public static string GetDescriptionFromScript(string scriptPath)
        {
            var iniFile = FindIniFileForScript(scriptPath);
            if (iniFile == null)
                return null;

            return iniFile.GetStringValue("$Editor", "Confirmation", null);
        }

        private static IniFile FindIniFileForScript(string scriptPath)
        {
            if (!File.Exists(scriptPath))
                return null;

            var iniFile = new IniFile(scriptPath);
            return iniFile;
        }

        private static string RunSource(Map map, string source)
        {
            var script = new CSharpScriptExecution() { SaveGeneratedCode = true };
            script.AddDefaultReferencesAndNamespaces();
            script.AddAssembly(typeof(System.Windows.Forms.MessageBox));
            script.AddAssembly(map.Units.GetType());
            script.AddAssembly(typeof(Map));
            script.AddNamespace("TSMapEditor");
            script.AddNamespace("TSMapEditor.Models");

            script.ExecuteCode(source, map);

            if (!script.Error)
                return null;

            return script.ErrorMessage;
        }
    }
}
