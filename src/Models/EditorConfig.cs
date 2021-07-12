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
        public List<BrushSize> BrushSizes { get; } = new List<BrushSize>() { new BrushSize(1, 1) };

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

        public void ReadBrushSizes()
        {
            var iniFile = new IniFile(Environment.CurrentDirectory + "/Config/BrushSizes.ini");
            var section = iniFile.GetSection("BrushSizes");
            if (section == null)
                return;

            BrushSizes.Clear();

            foreach (var kvp in section.Keys)
            {
                string[] parts = kvp.Value.Split('x');
                if (parts.Length != 2)
                    continue;

                int[] sizes = Array.ConvertAll(parts, s => Conversions.IntFromString(s, 0));
                if (sizes[0] < 1 || sizes[1] < 1)
                    continue;

                BrushSizes.Add(new BrushSize(sizes[0], sizes[1]));
            }
        }
    }
}
