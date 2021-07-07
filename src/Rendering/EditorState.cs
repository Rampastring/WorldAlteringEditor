using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.UI;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// Contains run-time settings related to the state of the editor.
    /// </summary>
    public class EditorState
    {
        public bool IsMarbleMadness { get; set; } = false;
        public CursorAction CursorAction { get; set; }
        public House ObjectOwner { get; set; }
    }
}
