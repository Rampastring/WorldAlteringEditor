using Rampastring.Tools;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TSMapEditor.Models;

namespace TSMapEditor.Scripts
{
    /// <summary>
    /// Can execute limited parts of FinalSun script files. For backwards compatibility.
    /// </summary>
    public class FinalSunScriptEngine
    {
        public FinalSunScriptEngine(Map map)
        {
            this.map = map;
        }

        private readonly Map map;

        public void Run(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("FinalSunScriptEngine: Can't find script " + path);
            }

            string[] lines = File.ReadAllLines(path);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                int commentIndex = line.IndexOf("//");
                if (commentIndex > -1)
                    line = line.Substring(0, commentIndex);

                line = line.Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                int paramStartIndex = line.IndexOf('(');
                int paramEndIndex = line.IndexOf(')');
                if (paramStartIndex == -1 || paramEndIndex == -1)
                {
                    Logger.Log("FinalSunScriptEngine: Can't parse parameters from line " + line);
                    return;
                }

                string paramPart = line.Substring(paramStartIndex, paramEndIndex - paramStartIndex);
                string[] paramValues = paramPart.Split(',');

                // Remove quote marks from INI keys and values
                paramValues = paramValues.Select(pv => pv.Replace("\"", string.Empty)).ToArray();

                if (line.StartsWith("SetIniKey"))
                {
                    if (paramValues.Length != 3)
                    {
                        Logger.Log("FinalSunScriptEngine: Invalid param count for SetIniKey on script " + path + ", line " + line);
                        return;
                    }

                    map.LoadedINI.SetStringValue(paramValues[0], paramValues[1], paramValues[2]);
                }
                else if (line.StartsWith("AskContinue"))
                {
                    if (MessageBox.Show(paramValues[0], "Script Message", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    {
                        Logger.Log("FinalSunScriptEngine: Asked permission to continue, the user didn't give it.");
                        return;
                    }
                }
                else
                {
                    Logger.Log($"FinalSunScriptEngine: Ignoring unknown command on line '{line}'");
                }
            }
        }
    }
}
