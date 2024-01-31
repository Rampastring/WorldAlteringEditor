using Rampastring.Tools;
using System;
using System.IO;
using TSMapEditor.Extensions;

namespace TSMapEditor.CCEngine
{
    public class GameConfigINIFiles
    {
        public GameConfigINIFiles(string gameDirectory, CCFileManager fileManager)
        {
            RulesIni = IniFileEx.FromPathOrMix(Constants.RulesIniPath, gameDirectory, fileManager);
            FirestormIni = IniFileEx.FromPathOrMix(Constants.FirestormIniPath, gameDirectory, fileManager);
            ArtIni = IniFileEx.FromPathOrMix(Constants.ArtIniPath, gameDirectory, fileManager);
            ArtFSIni = IniFileEx.FromPathOrMix(Constants.FirestormArtIniPath, gameDirectory, fileManager);
            AIIni = IniFileEx.FromPathOrMix(Constants.AIIniPath, gameDirectory, fileManager);
            AIFSIni = IniFileEx.FromPathOrMix(Constants.FirestormAIIniPath, gameDirectory, fileManager);

            IniFile artOverridesIni = new(Path.Combine(Environment.CurrentDirectory, "Config/ArtOverrides.ini"));
            IniFile.ConsolidateIniFiles(ArtFSIni, artOverridesIni);
        }

        public IniFileEx RulesIni { get; }
        public IniFileEx FirestormIni { get; }
        public IniFileEx ArtIni { get; }
        public IniFileEx ArtFSIni { get; }
        public IniFileEx AIIni { get; }
        public IniFileEx AIFSIni { get; }
    }
}
