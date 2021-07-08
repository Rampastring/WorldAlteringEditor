using Microsoft.Xna.Framework.Input;
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
    }
}
