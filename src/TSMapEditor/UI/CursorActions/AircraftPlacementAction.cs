using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    public class AircraftPlacementAction : CursorAction
    {
        public AircraftPlacementAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        private Aircraft aircraft;

        private AircraftType _aircraftType;

        public AircraftType AircraftType
        {
            get => _aircraftType;
            set
            {
                if (_aircraftType != value)
                {
                    _aircraftType = value;

                    if (_aircraftType == null)
                    {
                        aircraft = null;
                    }
                    else
                    {
                        aircraft = new Aircraft(_aircraftType) { Owner = CursorActionTarget.MutationTarget.ObjectOwner };
                    }
                }
            }
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            // Assign preview data
            aircraft.Position = cellCoords;

            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Aircraft == null)
            {
                tile.Aircraft = aircraft;
                CursorActionTarget.AddRefreshPoint(cellCoords);
            }
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            // Clear preview data
            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Aircraft == aircraft)
            {
                tile.Aircraft = null;
                CursorActionTarget.AddRefreshPoint(cellCoords);
            }
        }

        public override void LeftDown(Point2D cellPoint)
        {
            if (AircraftType == null)
                throw new InvalidOperationException(nameof(AircraftType) + " cannot be null");

            var tile = CursorActionTarget.Map.GetTile(cellPoint);
            if (tile.Aircraft != null)
                return;

            var mutation = new PlaceAircraftMutation(CursorActionTarget.MutationTarget, AircraftType, cellPoint);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellPoint)
        {
            LeftDown(cellPoint);
        }
    }
}
