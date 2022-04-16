using Rampastring.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TSMapEditor.Models
{
    public struct TutorialLine
    {
        public TutorialLine(int id, string text)
        {
            ID = id;
            Text = text;
        }

        public int ID;
        public string Text;
    }

    public class TutorialLines
    {
        public TutorialLines(string iniPath)
        {
            Read(iniPath);
        }

        private Dictionary<int, string> tutorialLines = new Dictionary<int, string>();

        public List<TutorialLine> GetLines() => tutorialLines.Select(tl => new TutorialLine(tl.Key, tl.Value)).OrderBy(tl => tl.ID).ToList();

        /// <summary>
        /// Fetches a tutorial text line with the given ID.
        /// If the text line doesn't exist, returns an empty string.
        /// </summary>
        public string GetStringByIdOrEmptyString(int id)
        {
            if (tutorialLines.TryGetValue(id, out string value))
                return value;

            return string.Empty;
        }

        private void Read(string iniPath)
        {
            const string TutorialSectionName = "Tutorial";

            if (!File.Exists(iniPath))
                return;

            IniFile tutorialIni = new IniFile(iniPath);
            var keys = tutorialIni.GetSectionKeys(TutorialSectionName);
            if (keys == null)
                return;

            foreach (string key in keys)
            {
                int id = Conversions.IntFromString(key, -1);

                if (id > -1)
                {
                    tutorialLines.Add(id, tutorialIni.GetStringValue(TutorialSectionName, key, string.Empty));
                }
            }
        }
    }
}
