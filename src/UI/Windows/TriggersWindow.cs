using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rampastring.XNAUI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class TriggersWindow : INItializableWindow
    {
        public TriggersWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            Name = nameof(TriggersWindow);
            base.Initialize();
        }

        public void Open()
        {
            Show();
        }
    }
}
