using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that allows placing a vehicle on the map.
    /// </summary>
    public class PlaceVehicleMutation : Mutation
    {
        public PlaceVehicleMutation(IMutationTarget mutationTarget, UnitType unitType, Point2D cellCoords) : base(mutationTarget)
        {
            this.unitType = unitType;
            this.cellCoords = cellCoords;
        }

        private readonly UnitType unitType;
        private readonly Point2D cellCoords;

        public override void Perform()
        {
            var cell = MutationTarget.Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (cell.Vehicle != null)
                throw new InvalidOperationException(nameof(PlaceVehicleMutation) + ": the cell already has a vehicle!");

            var unit = new Unit(unitType);
            unit.Owner = MutationTarget.ObjectOwner;
            unit.Position = cellCoords;
            MutationTarget.Map.PlaceUnit(unit);
            MutationTarget.AddRefreshPoint(cellCoords);
        }

        public override void Undo()
        {
            var cell = MutationTarget.Map.GetTile(cellCoords);
            MutationTarget.Map.RemoveUnit(cell.Vehicle);
            MutationTarget.AddRefreshPoint(cellCoords);
        }
    }
}
