using System.Collections.Generic;
using TSMapEditor.Extensions;

namespace TSMapEditor.Models
{
    public struct EvaSpeech
    {
        public EvaSpeech(int index, string name, string text)
        {
            Index = index;
            Name = name;
            Text = text;
        }

        public override string ToString()
        {
            return $"{Name} {Text}";
        }

        public int Index;
        public string Name;
        public string Text;
    }

    public class EvaSpeeches
    {
        public EvaSpeeches(IniFileEx evaIni)
        {
            Initialize(evaIni);
        }

        private List<EvaSpeech> speeches;
        public List<EvaSpeech> Speeches => new(speeches);


        private void Initialize(IniFileEx evaIni)
        {
            speeches = new List<EvaSpeech>();

            const string speechSectionName = "DialogList";

            evaIni.DoForEveryValueInSection(speechSectionName, name =>
            {
                if (string.IsNullOrEmpty(name))
                    return;

                var speechSection = evaIni.GetSection(name);
                string text = string.Empty;

                if (speechSection != null)
                {
                    text = speechSection.GetStringValue("Text", text);
                }

                speeches.Add(new EvaSpeech(speeches.Count, name, text));
            });
        }
    }
}
