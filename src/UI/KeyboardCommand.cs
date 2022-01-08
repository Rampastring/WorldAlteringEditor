using Microsoft.Xna.Framework.Input;
using Rampastring.Tools;
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

        public string GetDataString()
        {
            return Key.ToString() + ":" + ((int)Modifiers).ToString();
        }

        public void ApplyDataString(string str)
        {
            string[] parts = str.Split(':');
            if (parts.Length != 2)
                throw new ArgumentException("Incorrect KeyboardCommandInput data format: " + str);

            Key = (Keys)Enum.Parse(typeof(Keys), parts[0]);
            Modifiers = (KeyboardModifiers)Conversions.IntFromString(parts[1], 0);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is KeyboardCommandInput objAsInput))
                return false;

            return objAsInput.Key == Key && objAsInput.Modifiers == Modifiers;
        }

        public override int GetHashCode()
        {
            return (int)Modifiers * 10000 + (int)Key;
        }
    }

    public class KeyboardCommand
    {
        public KeyboardCommand(string iniName, string uiName, KeyboardCommandInput defaultKey)
        {
            ININame = iniName;
            UIName = uiName;
            DefaultKey = defaultKey;
            Key = new KeyboardCommandInput(defaultKey.Key, defaultKey.Modifiers);
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

        private bool AreModifiersDown(RKeyboard keyboard)
        {
            if ((Key.Modifiers & KeyboardModifiers.Alt) == KeyboardModifiers.Alt && !keyboard.IsAltHeldDown())
                return false;

            if ((Key.Modifiers & KeyboardModifiers.Ctrl) == KeyboardModifiers.Ctrl && !keyboard.IsCtrlHeldDown())
                return false;

            if ((Key.Modifiers & KeyboardModifiers.Shift) == KeyboardModifiers.Shift && !keyboard.IsShiftHeldDown())
                return false;

            return true;
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

            if (!AreModifiersDown(keyboard))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the keys of this command are held down.
        /// If the primary key of this command is None, it's ignored
        /// and only the key modifiers determine whether the key is held down.
        /// 
        /// If the primary key is None and no modifiers are specified,
        /// this function behaves will always return False.
        /// </summary>
        /// <param name="keyboard">The Rampastring.XNAUI RKeyboard instance.</param>
        /// <returns>True if the keys are currently held down, otherwise false.</returns>
        public bool AreKeysDownOrModifiersDownWithNoPrimaryKeySpecified(RKeyboard keyboard)
        {
            if (Key.Key == Keys.None)
            {
                if (Key.Modifiers == KeyboardModifiers.None)
                    return false;
            }
            else
                return AreKeysDown(keyboard);

            return AreModifiersDown(keyboard);
        }
    }
}
