using Rampastring.Tools;
using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    public class Waypoint
    {
        private const int Coefficient = 1000;

        public int Identifier { get; set; }
        public Point2D Position { get; set; }

        public static Waypoint ParseWaypoint(string id, string coordsString)
        {
            int waypointIndex = Conversions.IntFromString(id, -1);

            if (waypointIndex < 0 || waypointIndex > Constants.MaxWaypoint)
            {
                Logger.Log("Waypoint.Read: invalid waypoint index " + waypointIndex);
                return null;
            }

            Point2D? coords = Helpers.CoordStringToPoint(coordsString);
            if (coords == null)
                return null;

            return new Waypoint() { Identifier = waypointIndex, Position = coords.Value };
        }
    }
}
