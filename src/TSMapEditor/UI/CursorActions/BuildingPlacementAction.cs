using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.CursorActions
{
    /// <summary>
    /// A cursor action that allows placing buildings on the map.
    /// </summary>
    public class BuildingPlacementAction : CursorAction
    {
        public BuildingPlacementAction(ICursorActionTarget cursorActionTarget) : base(cursorActionTarget)
        {
        }

        public override string GetName() => "Place Building";

        private Structure structure;

        private BuildingType _buildingType;

        public BuildingType BuildingType
        {
            get => _buildingType;
            set
            {
                if (_buildingType != value)
                {
                    _buildingType = value;

                    if (_buildingType == null)
                    {
                        structure = null;
                    }
                    else
                    {
                        structure = new Structure(_buildingType) { Owner = CursorActionTarget.MutationTarget.ObjectOwner };
                    }
                }
            }
        }

        public override void PreMapDraw(Point2D cellCoords)
        {
            // Assign preview data
            structure.Position = cellCoords;

            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Structure != null)
                return;

            bool foundationAreaHasStructure = false;
            structure.ObjectType.ArtConfig.DoForFoundationCoords(offset =>
            {
                var cell = CursorActionTarget.Map.GetTile(cellCoords + offset);

                if (cell != null && cell.Structure != null)
                    foundationAreaHasStructure = true;
            });

            if (!foundationAreaHasStructure)
            {
                tile.Structure = structure;
                CursorActionTarget.TechnoUnderCursor = structure;
                CursorActionTarget.AddRefreshPoint(cellCoords, 10);
            }
        }

        public override void PostMapDraw(Point2D cellCoords)
        {
            // Clear preview data
            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Structure == structure)
            {
                tile.Structure = null;
                CursorActionTarget.TechnoUnderCursor = null;
                CursorActionTarget.AddRefreshPoint(cellCoords, 10);
            }
        }

        public override void LeftDown(Point2D cellCoords)
        {
            if (BuildingType == null)
                throw new InvalidOperationException(nameof(BuildingType) + " cannot be null");

            var tile = CursorActionTarget.Map.GetTile(cellCoords);
            if (tile.Structure != null)
                return;

            bool foundationInvalid = false;
            structure.ObjectType.ArtConfig.DoForFoundationCoords(offset =>
            {
                var cell = CursorActionTarget.Map.GetTile(cellCoords + offset);
                if (cell == null || cell.Structure != null)
                    foundationInvalid = true;
            });

            if (foundationInvalid)
                return;

            var mutation = new PlaceBuildingMutation(CursorActionTarget.MutationTarget, BuildingType, cellCoords);
            CursorActionTarget.MutationManager.PerformMutation(mutation);
        }

        public override void LeftClick(Point2D cellCoords)
        {
            LeftDown(cellCoords);
        }
    }
}
