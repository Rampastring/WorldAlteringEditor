using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering
{
    class Refresh
    {
        public Refresh(Map map)
        {
            this.map = map;
        }

        private readonly Map map;

        private Dictionary<int, MapTile> tilesToRedraw = new Dictionary<int, MapTile>();
        private Dictionary<int, GameObject> objectsToRedraw = new Dictionary<int, GameObject>();

        public void RedrawFromObject(GameObject gameObject)
        {
            objectsToRedraw[GetGameObjectHash(gameObject)] = gameObject;

            const int redrawArea = 2;
            for (int y = -redrawArea; y <= redrawArea; y++)
            {
                for (int x = -redrawArea; x <= redrawArea; x++)
                {

                }
            }
        }

        private void RedrawTiles(int areaSize, Point2D cellCoords)
        {
            for (int y = -areaSize; y <= areaSize; y++)
            {
                for (int x = -areaSize; x <= areaSize; x++)
                {
                    var tile = map.GetTile(x, y);
                    if (tile == null)
                        continue;

                    RedrawTile(tile);
                }
            }
        }

        private void RedrawTile(MapTile mapTile)
        {
            AddTileToRedraw(mapTile);

            if (mapTile.Overlay != null)
            {

            }
        }

        private void AddTileToRedraw(MapTile mapTile)
        {
            tilesToRedraw.Add(GetMapTileHash(mapTile), mapTile);
        }

        private int GetMapTileHash(MapTile mapTile)
        {
            return mapTile.Y * 1000 + mapTile.X;
        }

        private int GetGameObjectHash(GameObject gameObject)
        {
            if (gameObject.WhatAmI() == RTTIType.Infantry)
            {
                return (int)gameObject.WhatAmI() * 1000000 + gameObject.Position.Y * 10000 + gameObject.Position.X + (int)((Infantry)gameObject).SubCell;
            }

            return (int)gameObject.WhatAmI() * 100000 + gameObject.Position.Y * 1000 + gameObject.Position.X;
        }
    }
}
