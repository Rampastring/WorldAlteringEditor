using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using TSMapEditor.UI;

namespace TSMapEditor.Models
{
    /// <summary>
    /// Contains and handles editor-related configuration options.
    /// </summary>
    public class EditorConfig
    {
        public EditorConfig() { }

        public List<OverlayCollection> OverlayCollections { get; } = new List<OverlayCollection>();

        public void ReadOverlayCollections(Rules rules)
        {
            OverlayCollections.Clear();

            var iniFile = new IniFile(Environment.CurrentDirectory + "/Config/OverlayCollections.ini");

            var keys = iniFile.GetSectionKeys("OverlayCollections");
            if (keys == null)
                return;

            foreach (var key in keys)
            {
                var value = iniFile.GetStringValue("OverlayCollections", key, string.Empty);
                var collectionSection = iniFile.GetSection(value);
                if (collectionSection == null)
                    continue;

                var overlayCollection = OverlayCollection.InitFromIniSection(collectionSection, rules.OverlayTypes);
                OverlayCollections.Add(overlayCollection);
            }
        }
    }
}
