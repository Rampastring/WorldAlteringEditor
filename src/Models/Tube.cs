using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;

namespace TSMapEditor.Models
{
    public enum TubeDirection
    {
        None = -1,
        NorthEast = 0,
        East = 1,
        SouthEast = 2,
        South = 3,
        SouthWest = 4,
        West = 5,
        NorthWest = 6,
        North = 7,

        Max = 7
    }

    /// <summary>
    /// A tunnel tube on the map.
    /// </summary>
    public class Tube
    {
        public Tube()
        {

        }

        public Tube(Point2D entryPoint, Point2D exitPoint, TubeDirection unitInitialFacing, List<TubeDirection> directions)
        {
            EntryPoint = entryPoint;
            ExitPoint = exitPoint;
            UnitInitialFacing = unitInitialFacing;
            Directions = directions;
        }

        public Point2D EntryPoint { get; set; }
        public Point2D ExitPoint { get; set; }
        public TubeDirection UnitInitialFacing { get; set; }
        public List<TubeDirection> Directions { get; set; } = new List<TubeDirection>();
    }
}
