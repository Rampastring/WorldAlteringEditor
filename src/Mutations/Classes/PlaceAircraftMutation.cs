using System;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation that allows placing aircraft on the map.
    /// </summary>
    public class PlaceAircraftMutation : Mutation
    {
        public PlaceAircraftMutation(IMutationTarget mutationTarget, AircraftType aircraftType, Point2D cellCoords) : base(mutationTarget)
        {
            this.aircraftType = aircraftType;
            this.cellCoords = cellCoords;
        }

        private readonly AircraftType aircraftType;
        private readonly Point2D cellCoords;

        public override void Perform()
        {
            var cell = MutationTarget.Map.GetTile(cellCoords);
            if (cell == null)
                return;

            if (cell.Aircraft != null)
                throw new InvalidOperationException(nameof(PlaceAircraftMutation) + ": the cell already has an aircraft!");

            var aircraft = new Aircraft(aircraftType);
            aircraft.Owner = MutationTarget.ObjectOwner;
            aircraft.Position = cellCoords;
            MutationTarget.Map.PlaceAircraft(aircraft);
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
