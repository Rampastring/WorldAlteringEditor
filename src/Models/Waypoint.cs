using Rampastring.Tools;
using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    public class Waypoint
    {
        private const int Coefficient = 1000;

        public int Identifier { get; set; }
        public Point2D Position { get; set; }

        public static Waypoint Read(string id, string coordsString)
        {
            int waypointIndex = Conversions.IntFromString(id, -1);

            if (waypointIndex < 0 || waypointIndex > Constants.MaxWaypoint)
            {
                Logger.Log("Waypoint.Read: invalid waypoint index " + waypointIndex);
                return null;
            }
                
            int coords = Conversions.IntFromString(coordsString, -1);
            if (coords < 0 || coordsString.Length < 5)
            {
                Logger.Log("Waypoint.Read: invalid coord string " + coordsString);
                return null;
            }

            string xCoordPart = coordsString.Substring(coordsString.Length - 3);
            int x = Conversions.IntFromString(xCoordPart, -1);
            if (x < 0)
            {
                Logger.Log("Waypoint.Read: invalid X coord " + x);
                return null;
            }

            string yCoordPart = coordsString.Substring(0, coordsString.Length - 3);
            int y = Conversions.IntFromString(yCoordPart, -1);
            if (y < 0)
            {
                Logger.Log("Waypoint.Read: invalid Y coord " + y);
                return null;
            }

            return new Waypoint() { Identifier = waypointIndex, Position = new Point2D(x, y) };
        }
    }
}
