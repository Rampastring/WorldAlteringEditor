using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TSMapEditor.UI
{
    public class KeyboardCommands
    {
        public KeyboardCommands()
        {
            Commands = new List<KeyboardCommand>()
            {
                Undo,
                Redo,
                NextTile,
                PreviousTile,
                NextTileSet,
                PreviousTileSet,
                NextSidebarNode,
                PreviousSidebarNode,
                FrameworkMode
            };
        }

        public static KeyboardCommands Instance { get; set; }

        public List<KeyboardCommand> Commands { get; }

        public KeyboardCommand Undo { get; } = new KeyboardCommand("Undo", "Undo", new KeyboardCommandInput(Keys.Z, KeyboardModifiers.Ctrl));
        public KeyboardCommand Redo { get; } = new KeyboardCommand("Redo", "Redo", new KeyboardCommandInput(Keys.Y, KeyboardModifiers.Ctrl));
        public KeyboardCommand NextTile { get; } = new KeyboardCommand("NextTile", "Select Next Tile", new KeyboardCommandInput(Keys.M, KeyboardModifiers.None));
        public KeyboardCommand PreviousTile { get; } = new KeyboardCommand("PreviousTile", "Select Previous Tile", new KeyboardCommandInput(Keys.N, KeyboardModifiers.None));
        public KeyboardCommand NextTileSet { get; } = new KeyboardCommand("NextTileSet", "Select Next TileSet", new KeyboardCommandInput(Keys.J, KeyboardModifiers.None));
        public KeyboardCommand PreviousTileSet { get; } = new KeyboardCommand("PreviousTileSet", "Select Previous TileSet", new KeyboardCommandInput(Keys.H, KeyboardModifiers.None));
        public KeyboardCommand NextSidebarNode { get; } = new KeyboardCommand("NextSidebarNode", "Select Next Sidebar Node", new KeyboardCommandInput(Keys.O, KeyboardModifiers.None));
        public KeyboardCommand PreviousSidebarNode { get; } = new KeyboardCommand("PreviousSidebarNode", "Select Previous Sidebar Node", new KeyboardCommandInput(Keys.P, KeyboardModifiers.None));
        public KeyboardCommand FrameworkMode { get; } = new KeyboardCommand("MarbleMadness", "Framework Mode (Marble Madness)", new KeyboardCommandInput(Keys.F, KeyboardModifiers.Shift));
    }
}
