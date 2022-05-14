using Rampastring.Tools;
using System.Collections.Generic;
using TSMapEditor.Models;

namespace TSMapEditor.UI
{
    /// <summary>
    /// Combines many overlays into a single entry.
    /// </summary>
    public class OverlayCollection
    {
        public string Name { get; set; }
        public OverlayType[] OverlayTypes;

        public static OverlayCollection InitFromIniSection(IniSection iniSection, List<OverlayType> overlayTypes)
        {
            var overlayCollection = new OverlayCollection();
            overlayCollection.Name = iniSection.GetStringValue("Name", "Unnamed Collection");

            var overlayTypeList = new List<OverlayType>();

            int i = 0;
            while (true)
            {
                string overlayTypeName = iniSection.GetStringValue("OverlayType" + i, null);
                if (string.IsNullOrWhiteSpace(overlayTypeName))
                    break;

                var overlayType = overlayTypes.Find(o => o.ININame == overlayTypeName);
                if (overlayType != null)
                    overlayTypeList.Add(overlayType);

                i++;
            }

            overlayCollection.OverlayTypes = overlayTypeList.ToArray();
            return overlayCollection;
        }
    }
}
