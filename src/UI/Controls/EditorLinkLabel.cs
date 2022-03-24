using Rampastring.Tools;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace TSMapEditor.UI.Controls
{
    public class EditorLinkLabel : XNALinkLabel
    {
        public EditorLinkLabel(WindowManager windowManager) : base(windowManager)
        {
        }

        public string URL { get; set; }

        public override void ParseAttributeFromINI(IniFile iniFile, string key, string value)
        {
            if (key == "URL")
            {
                URL = value;
                return;
            }

            base.ParseAttributeFromINI(iniFile, key, value);
        }

        public override void OnLeftClick()
        {
            if (!string.IsNullOrWhiteSpace(URL))
            {
                try
                {
                    Process.Start(URL);
                }
                catch (Win32Exception ex)
                {
                    Logger.Log($"Win32Exception when calling Process.Start from link label (URL: {URL}), exception message: {ex.Message}");
                }
                catch (FileNotFoundException ex)
                {
                    Logger.Log($"FileNotFoundException when calling Process.Start from link label (URL: {URL}), exception message: {ex.Message}");
                }
            }

            base.OnLeftClick();
        }
    }
}
