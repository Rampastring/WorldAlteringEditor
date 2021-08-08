using System;
using Rampastring.XNAUI;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Mutations.Classes;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class PlaceWaypointWindow : INItializableWindow
    {
        private const int MaxWaypoints = 100;

        public PlaceWaypointWindow(WindowManager windowManager, Map map, MutationManager mutationManager, IMutationTarget mutationTarget) : base(windowManager)
        {
            this.map = map;
            this.mutationManager = mutationManager;
            this.mutationTarget = mutationTarget;
        }

        private readonly Map map;
        private readonly MutationManager mutationManager;
        private readonly IMutationTarget mutationTarget;

        private EditorNumberTextBox tbWaypointNumber;

        private Point2D cellCoords;

        public override void Initialize()
        {
            Name = nameof(PlaceWaypointWindow);
            base.Initialize();

            tbWaypointNumber = FindChild<EditorNumberTextBox>(nameof(tbWaypointNumber));

            FindChild<EditorButton>("btnPlace").LeftClick += BtnPlace_LeftClick;
        }

        private void BtnPlace_LeftClick(object sender, EventArgs e)
        {
            if (tbWaypointNumber.Value < 0 || tbWaypointNumber.Value >= MaxWaypoints)
                return;

            if (map.Waypoints.Exists(w => w.Identifier == tbWaypointNumber.Value))
            {
                EditorMessageBox.Show(WindowManager,
                    "Waypoint already exists",
                    $"A waypoint with the given number {tbWaypointNumber.Value} already exists on the map!",
                    MessageBoxButtons.OK);

                return;
            }

            mutationManager.PerformMutation(new PlaceWaypointMutation(mutationTarget, cellCoords, tbWaypointNumber.Value));

            Hide();
        }

        public void Open(Point2D cellCoords)
        {
            this.cellCoords = cellCoords;

            if (map.Waypoints.Count == MaxWaypoints)
            {
                EditorMessageBox.Show(WindowManager,
                    "Maximum waypoints reached",
                    "All valid waypoints on the map are already in use!",
                    MessageBoxButtons.OK);

                return;
            }

            for (int i = 0; i < MaxWaypoints; i++)
            {
                if (!map.Waypoints.Exists(w => w.Identifier == i))
                {
                    tbWaypointNumber.Value = i;
                    break;
                }
            }

            Show();
        }
    }
}
