using Microsoft.Xna.Framework.Input;
using Rampastring.XNAUI.Input;
using System;

namespace TSMapEditor.UI
{
    [Flags]
    public enum KeyboardModifiers
    {
        None = 0,
        Shift = 1,
        Ctrl = 2,
        Alt = 4
    }

    public class KeyboardCommandInput
    {
        public KeyboardCommandInput(Keys key, KeyboardModifiers modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public Keys Key;
        public KeyboardModifiers Modifiers;

        public string GetKeyNameString()
        {
            if (Key == Keys.None)
                return "<no hotkey>";

            string key = "";
            if ((Modifiers & KeyboardModifiers.Ctrl) == KeyboardModifiers.Ctrl)
                key += "CTRL + ";

            if ((Modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift)
                key += "Shift + ";

            if ((Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt)
                key += "Alt + ";

            return key + Key.ToString();
        }
    }

    public class KeyboardCommand
    {
        public KeyboardCommand(string iniName, string uiName, KeyboardCommandInput defaultKey)
        {
            ININame = iniName;
            UIName = uiName;
            DefaultKey = defaultKey;
            Key = defaultKey;
        }

        public event EventHandler Triggered;

        public string ININame { get; }
        public string UIName { get; }
        public KeyboardCommandInput DefaultKey { get; }
        public KeyboardCommandInput Key { get; set; }

        private Action action;
        public Action Action
        {
            get => action;
            set
            {
                if (action != null)
                    throw new InvalidOperationException("KeyboardCommand.Action can only be set once.");

                action = value;
            }
        }

        public void DoTrigger()
        {
            Triggered?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Checks if the keys of this command are held down.
        /// </summary>
        /// <param name="keyboard">The Rampastring.XNAUI RKeyboard instance.</param>
        /// <returns>True if the keys are currently held down, otherwise false.</returns>
        public bool AreKeysDown(RKeyboard keyboard)
        {
            if (Key.Key == Keys.None)
                return false;

            if (!keyboard.IsKeyHeldDown(Key.Key))
                return false;

            if ((Key.Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt && !keyboard.IsAltHeldDown())
                return false;

            if ((Key.Modifiers & KeyboardModifiers.Ctrl) == KeyboardModifiers.Ctrl && !keyboard.IsCtrlHeldDown())
                return false;

            if ((Key.Modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift && !keyboard.IsShiftHeldDown())
                return false;

            return true;
        }
    }
}
