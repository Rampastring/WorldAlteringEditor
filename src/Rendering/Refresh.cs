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
        private const int RedrawObjectsMax = 1000;

        public Refresh(Map map)
        {
            this.map = map;
        }

        
        private readonly Map map;

        public Dictionary<int, MapTile> tilesToRedraw = new Dictionary<int, MapTile>();
        public Dictionary<int, GameObject> objectsToRedraw = new Dictionary<int, GameObject>();

        public void RedrawFromObject(GameObject gameObject)
        {
            if (objectsToRedraw.Count >= RedrawObjectsMax)
                return;

            int hash = GetGameObjectHash(gameObject);
            if (objectsToRedraw.ContainsKey(hash))
                return;

            objectsToRedraw[GetGameObjectHash(gameObject)] = gameObject;

            int redrawArea = 2;
            if (gameObject.WhatAmI() == RTTIType.Infantry)
                redrawArea = 0;
            else if (gameObject.WhatAmI() == RTTIType.Unit || gameObject.WhatAmI() == RTTIType.Aircraft)
                redrawArea = 1;
            else if (gameObject.WhatAmI() == RTTIType.Terrain)
                redrawArea = 1;

            for (int y = -redrawArea; y <= redrawArea; y++)
            {
                for (int x = -redrawArea; x <= redrawArea; x++)
                {
                    AddTileToRedrawFrom(gameObject.Position + new Point2D(x, y));
                }
            }
        }

        public void RedrawFromCell(int areaSize, Point2D cellCoords)
        {
            for (int y = -areaSize; y <= areaSize; y++)
            {
                for (int x = -areaSize; x <= areaSize; x++)
                {
                    var tile = map.GetTile(cellCoords + new Point2D(x, y));
                    if (tile == null)
                        continue;

                    RedrawTile(tile);
                }
            }
        }

        private void RedrawTile(MapTile mapTile)
        {
            if (objectsToRedraw.Count >= RedrawObjectsMax)
                return;

            if (AddTileToRedraw(mapTile))
            {
                // Point2D eastCellCoords = mapTile.CoordsToPoint() + new Point2D(1, -1);
                // Point2D southCellCoords = mapTile.CoordsToPoint() + new Point2D(1, 1);
                // Point2D southWestCellCoords = mapTile.CoordsToPoint() + new Point2D(0, 1);
                // Point2D southEastCellCoords = mapTile.CoordsToPoint() + new Point2D(1, 0);
                // 
                // if (mapTile.Overlay != null)
                // {
                //     AddTileToRedrawFrom(eastCellCoords);
                //     AddTileToRedrawFrom(southCellCoords);
                //     AddTileToRedrawFrom(southWestCellCoords);
                //     AddTileToRedrawFrom(southEastCellCoords);
                // }

                mapTile.DoForAllInfantry(inf => RedrawFromObject(inf));
                if (mapTile.Vehicle != null)
                    RedrawFromObject(mapTile.Vehicle);
                if (mapTile.Aircraft != null)
                    RedrawFromObject(mapTile.Aircraft);
                if (mapTile.TerrainObject != null)
                    RedrawFromObject(mapTile.TerrainObject);

                const int size = 1;
                for (int y = -size; y <= size; y++)
                {
                    for (int x = -size; x <= size; x++)
                    {
                        var cell = map.GetTile(mapTile.CoordsToPoint() + new Point2D(x, y));
                        if (cell != null)
                        {
                             if (cell.GetObject() != null)
                                RedrawFromObject(cell.GetObject());

                            if (cell.Overlay != null)
                                RedrawTile(cell);
                        }
                            
                    }
                }
            }
        }

        private void AddTileToRedrawFrom(Point2D cellCoords)
        {
            var mapTile = map.GetTile(cellCoords);
            if (mapTile != null)
                RedrawTile(mapTile);
        }

        private bool AddTileToRedraw(MapTile mapTile)
        {
            int hash = GetMapTileHash(mapTile);
            if (!tilesToRedraw.ContainsKey(hash))
            {
                tilesToRedraw.Add(hash, mapTile);
                return true;
            }

            return false;
        }

        private bool AddObjectToRedraw(GameObject gameObject)
        {
            int hash = GetGameObjectHash(gameObject);
            if (!objectsToRedraw.ContainsKey(hash))
            {
                objectsToRedraw.Add(hash, gameObject);
                return true;
            }

            return false;
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
