using Rampastring.Tools;
using System;

namespace TSMapEditor.Settings
{
    public class UserSettings
    {
        private const string General = "General";
        private const string Display = "Display";
        private const string MapView = "MapView";

        public UserSettings()
        {
            if (Instance != null)
                throw new InvalidOperationException("User settings can only be initialized once.");

            Instance = this;

            UserSettingsIni = new IniFile(Environment.CurrentDirectory + "/MapEditorSettings.ini");

            settings = new IINILoadable[]
            {
                TargetFPS,
                ResolutionWidth,
                ResolutionHeight,
                RenderResolutionWidth,
                RenderResolutionHeight,
                Borderless,
                FullscreenWindowed,

                ScrollRate,
                MapWideOverlayOpacity,

                UpscaleUI,
                Theme,
                UseBoldFont,

                GameDirectory,
                LastScenarioPath
            };

            foreach (var setting in settings)
                setting.LoadValue(UserSettingsIni);
        }

        public IniFile UserSettingsIni { get; }

        public void SaveSettings()
        {
            foreach (var setting in settings)
            {
                setting.WriteValue(UserSettingsIni, false);
            }

            UserSettingsIni.WriteIniFile();
        }

        public static UserSettings Instance { get; private set; }

        private readonly IINILoadable[] settings;

        public IntSetting TargetFPS = new IntSetting(Display, "TargetFPS", 240);
        public IntSetting ResolutionWidth = new IntSetting(Display, "ResolutionWidth", 1280);
        public IntSetting ResolutionHeight = new IntSetting(Display, "ResolutionHeight", 720);
        public IntSetting RenderResolutionWidth = new IntSetting(Display, "RenderResolutionWidth", 1280);
        public IntSetting RenderResolutionHeight = new IntSetting(Display, "RenderResolutionHeight", 720);
        public BoolSetting Borderless = new BoolSetting(Display, "Borderless", false);
        public BoolSetting FullscreenWindowed = new BoolSetting(Display, "FullscreenWindowed", false);

        public IntSetting ScrollRate = new IntSetting(MapView, nameof(ScrollRate), 15);
        public IntSetting MapWideOverlayOpacity = new IntSetting(MapView, "MapWideOverlayOpacity", 50);

        public BoolSetting UpscaleUI = new BoolSetting(General, "UpscaleUI", false);
        public StringSetting Theme = new StringSetting(General, "Theme", "Default");
        public BoolSetting UseBoldFont = new BoolSetting(General, "UseBoldFont", false);

        public StringSetting GameDirectory = new StringSetting(General, "GameDirectory", string.Empty);
        public StringSetting LastScenarioPath = new StringSetting(General, nameof(LastScenarioPath), "Maps/Custom/");
    }
}
