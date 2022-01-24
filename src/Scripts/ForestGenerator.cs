using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Scripts
{
    public class ForestGenerator
    {
        private Map map;
        private List<Point2D> cells = new List<Point2D>();
        private List<TerrainType> treeGroupTerrainTypes;
        private HashSet<Point2D> occupiedCells = new HashSet<Point2D>();
        private Random random;

        public void Generate(Map map, List<Point2D> cells, List<TerrainType> treeGroupTerrainTypes, double treeGroupChance, double openTerrainChance, double openPebbleChance)
        {
            this.map = map;
            this.cells = cells;
            this.treeGroupTerrainTypes = treeGroupTerrainTypes;
            random = new Random();

            var tgrassSet = map.TheaterInstance.Theater.TileSets.Find(ts => ts.SetName == "Tall Grass");
            var pebbleSet = map.TheaterInstance.Theater.TileSets.Find(ts => ts.SetName == "Pebbles");
            int minY = cells.Select(p => p.Y).Aggregate((y1, y2) => Math.Min(y1, y2));
            int maxY = cells.Select(p => p.Y).Aggregate((y1, y2) => Math.Max(y1, y2));
            int minX = cells.Select(p => p.X).Aggregate((x1, x2) => Math.Min(x1, x2));
            int maxX = cells.Select(p => p.X).Aggregate((x1, x2) => Math.Max(x1, x2));

            foreach (Point2D cellCoord in cells)
            {
                if (random.NextDouble() < treeGroupChance)
                {
                    int index = random.Next(0, treeGroupTerrainTypes.Count);
                    var terrainType = treeGroupTerrainTypes[index];
                    if (AllowTreeGroupOnCell(cellCoord, terrainType))
                        PlaceTreeGroupOnCell(cellCoord, terrainType);
                }
            }

            var openTerrainCells = cells.Where(p => !occupiedCells.Contains(p));
            foreach (Point2D cellCoord in openTerrainCells)
            {
                if (random.NextDouble() < openTerrainChance)
                {
                    var mapTile = map.GetTile(cellCoord);
                    mapTile.TileImage = null;
                    mapTile.TileIndex = tgrassSet.StartTileIndex;
                    mapTile.SubTileIndex = 0;
                }
                else if (random.NextDouble() < openPebbleChance)
                {
                    var mapTile = map.GetTile(cellCoord);
                    mapTile.TileImage = null;
                    mapTile.TileIndex = pebbleSet.StartTileIndex;
                    mapTile.SubTileIndex = 0;
                }
            }

            minY--;
            maxY++;
            minX--;
            maxX++;
            ApplyAutoLAT(minX, minY, maxX, maxY, tgrassSet.Index);
        }

        private void ApplyAutoLAT(int minX, int minY, int maxX, int maxY, int tileSetId)
        {
            // Get potential base tilesets of the placed LAT (if we're placing LAT)
            // This allows placing certain LATs on top of other LATs (example: snowy dirt on snow, when snow is also placed on grass)
            TileSet baseTileSet = null;
            TileSet altBaseTileSet = null;
            var tileAutoLatGround = map.TheaterInstance.Theater.LATGrounds.Find(
                g => g.GroundTileSet.Index == tileSetId || g.TransitionTileSet.Index == tileSetId);

            if (tileAutoLatGround != null && tileAutoLatGround.BaseTileSet != null)
            {
                int baseTileSetId = tileAutoLatGround.BaseTileSet.Index;
                var baseLatGround = map.TheaterInstance.Theater.LATGrounds.Find(
                    g => g.GroundTileSet.Index == baseTileSetId || g.TransitionTileSet.Index == baseTileSetId);

                if (baseLatGround != null)
                {
                    baseTileSet = baseLatGround.GroundTileSet;
                    altBaseTileSet = baseLatGround.TransitionTileSet;
                }
            }

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    var mapTile = map.GetTile(x, y);
                    if (mapTile == null)
                        return;

                    int tileSetIndex = map.TheaterInstance.GetTileSetId(mapTile.TileIndex);
                    // Don't auto-lat ground that is a base for our placed ground type
                    if ((baseTileSet != null && tileSetIndex == baseTileSet.Index) ||
                    (altBaseTileSet != null && tileSetIndex == altBaseTileSet.Index))
                        return;

                    var autoLatGround = map.TheaterInstance.Theater.LATGrounds.Find(g => g.GroundTileSet.Index == tileSetIndex || g.TransitionTileSet.Index == tileSetIndex);
                    if (autoLatGround != null)
                    {
                        int autoLatIndex = map.GetAutoLATIndex(mapTile, autoLatGround.GroundTileSet.Index, autoLatGround.TransitionTileSet.Index);
                        if (autoLatIndex == -1)
                        {
                            mapTile.TileIndex = autoLatGround.GroundTileSet.StartTileIndex;
                        }
                        else
                        {
                            mapTile.TileIndex = autoLatGround.TransitionTileSet.StartTileIndex + autoLatIndex;
                        }

                        mapTile.SubTileIndex = 0;
                        mapTile.TileImage = null;
                    }
                }
            }
        }

        private bool AllowTreeGroupOnCell(Point2D cellCoords, TerrainType treeGroup)
        {
            var cell = map.GetTile(cellCoords);
            if (cell == null)
                return false;

            if (cell.TerrainObject != null)
                return false;

            if (treeGroup.ImpassableCells == null)
                return !occupiedCells.Contains(cellCoords);

            foreach (var offset in treeGroup.ImpassableCells)
            {
                var otherCellCoords = cellCoords + offset;
                var otherCell = map.GetTile(otherCellCoords);
                if (otherCell == null)
                    continue;

                if (otherCell.TerrainObject != null || occupiedCells.Contains(otherCellCoords))
                    return false;
            }

            return true;
        }

        private void PlaceTreeGroupOnCell(Point2D cellCoords, TerrainType treeGroup)
        {
            var cell = map.GetTile(cellCoords);
            map.AddTerrainObject(new TerrainObject(treeGroup, cellCoords));
            foreach (var offset in treeGroup.ImpassableCells)
                occupiedCells.Add(cellCoords + offset);
        }
    }
}
