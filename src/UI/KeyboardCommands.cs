using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSMapEditor.UI
{
    public class KeyboardCommands
    {
        public KeyboardCommands()
        {
            Commands = new List<KeyboardCommand>()
            {
                Undo,
                Redo
            };
        }

        public List<KeyboardCommand> Commands { get; }

        public KeyboardCommand Undo { get; } = new KeyboardCommand("Undo", "Undo", new KeyboardCommandInput(Keys.Z, KeyboardModifiers.Ctrl));
        public KeyboardCommand Redo { get; } = new KeyboardCommand("Redo", "Redo", new KeyboardCommandInput(Keys.Y, KeyboardModifiers.Ctrl));
    }
}
