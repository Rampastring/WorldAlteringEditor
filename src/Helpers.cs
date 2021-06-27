using Rampastring.Tools;
using System;
using TSMapEditor.GameMath;

namespace TSMapEditor
{
    public static class Helpers
    {
        public static bool IsStringNoneValue(string str)
        {
            return str.Equals(Constants.NoneValue1, StringComparison.InvariantCultureIgnoreCase) ||
                str.Equals(Constants.NoneValue2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string BoolToIntString(bool value)
        {
            return value ? "1" : "0";
        }

        public static Point2D? CoordStringToPoint(string coordsString)
        {
            int coords = Conversions.IntFromString(coordsString, -1);
            if (coords < 0 || coordsString.Length < 5)
            {
                Logger.Log("CoordStringToPoint: invalid coord string " + coordsString);
                return null;
            }

            string xCoordPart = coordsString.Substring(coordsString.Length - 3);
            int x = Conversions.IntFromString(xCoordPart, -1);
            if (x < 0)
            {
                Logger.Log("CoordStringToPoint: invalid X coord " + x);
                return null;
            }

            string yCoordPart = coordsString.Substring(0, coordsString.Length - 3);
            int y = Conversions.IntFromString(yCoordPart, -1);
            if (y < 0)
            {
                Logger.Log("CoordStringToPoint: invalid Y coord " + y);
                return null;
            }

            return new Point2D(x, y);
        }
    }
}
