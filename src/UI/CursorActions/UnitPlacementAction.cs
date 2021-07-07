using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows placing units on the map.
    /// </summary>
    class UnitPlacementAction : CursorAction
    {
        public UnitPlacementAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        private Unit unit;

        private UnitType _unitType;

        public UnitType UnitType
        {
            get => _unitType;
            set
            {
                if (_unitType != value)
                {
                    _unitType = value;

                    if (_unitType == null)
                    {
                        unit = null;
                    }
                    else
                    {
                        unit = new Unit(_unitType) { Owner = CursorActionTarget.MutationTarget.ObjectOwner };
                    }
                }
            }
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            // Assign preview data
            unit.Position = cellCoords;

            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Vehicle == null)
            {
                tile.Vehicle = unit;
                CursorActionTarget.AddRefreshPoint(cellCoords);
            }
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            // Clear preview data
            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Vehicle == unit)
            {
                tile.Vehicle = null;
                CursorActionTarget.AddRefreshPoint(cellCoords);
            }
        }

        public override void LeftDown(Point2D cellPoint)
        {
            if (UnitType == null)
                throw new InvalidOperationException("UnitType cannot be null");

            var tile = CursorActionTarget.Map.GetTile(cellPoint);
            if (tile.Vehicle != null)
                return;

            var mutation = new PlaceVehicleMutation(CursorActionTarget.MutationTarget, UnitType, cellPoint);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellPoint)
        {
            LeftDown(cellPoint);
        }
    }
}
