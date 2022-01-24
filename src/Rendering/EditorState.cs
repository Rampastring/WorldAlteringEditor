using System;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.UI;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// Contains run-time settings related to the state of the editor.
    /// </summary>
    public class EditorState
    {
        public event EventHandler CursorActionChanged;
        public event EventHandler ObjectOwnerChanged;
        public event EventHandler AutoLATEnabledChanged;
        public event EventHandler OnlyPaintOnClearGroundChanged;

        public bool IsMarbleMadness { get; set; } = false;

        private CursorAction _cursorAction;
        public CursorAction CursorAction
        {
            get => _cursorAction;
            set
            {
                if (_cursorAction != value)
                {
                    if (_cursorAction != null)
                    {
                        _cursorAction.OnActionExit();
                        _cursorAction.OnExitingAction -= CursorAction_OnExitingAction;
                    }

                    _cursorAction = value;
                    if (value != null)
                        value.OnExitingAction += CursorAction_OnExitingAction;

                    CursorActionChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void CursorAction_OnExitingAction(object sender, EventArgs e)
        {
            CursorAction = null;
        }

        private House _objectOwner;
        public House ObjectOwner
        {
            get => _objectOwner;
            set
            {
                if (_objectOwner != value)
                {
                    _objectOwner = value;
                    ObjectOwnerChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public BrushSize BrushSize { get; set; } = new BrushSize(1, 1);

        public Randomizer Randomizer { get; } = new Randomizer();

        private bool _autoLatEnabled = true;
        public bool AutoLATEnabled
        {
            get => _autoLatEnabled;
            set
            {
                if (value != _autoLatEnabled)
                {
                    _autoLatEnabled = value;
                    AutoLATEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool _onlyPaintOnClearGround = false;
        public bool OnlyPaintOnClearGround
        {
            get => _onlyPaintOnClearGround;
            set
            {
                if (value != _onlyPaintOnClearGround)
                {
                    _onlyPaintOnClearGround = value;
                    OnlyPaintOnClearGroundChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public List<CopiedTerrainData> CopiedTerrainData { get; } = new List<CopiedTerrainData>();
    }
}
