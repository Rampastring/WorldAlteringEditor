using System;
using System.IO;
using TSMapEditor.Models;
using TSMapEditor.Settings;

namespace TSMapEditor.Misc
{
    public class AutosaveTimer
    {
        public AutosaveTimer(Map map) 
        {
            this.map = map;
            AutoSaveTime = TimeSpan.FromSeconds(UserSettings.Instance.AutoSaveInterval);
        }

        private readonly Map map;

        public TimeSpan AutoSaveTime { get; set; }

        private void DoSave()
        {
            map.AutoSave(Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), "autosave.map"));
        }

        public void Update(TimeSpan elapsedTime)
        {
            AutoSaveTime -= elapsedTime;

            if (AutoSaveTime.TotalMilliseconds <= 0)
            {
                DoSave();
                AutoSaveTime = TimeSpan.FromSeconds(UserSettings.Instance.AutoSaveInterval);
            }
        }
    }
}
