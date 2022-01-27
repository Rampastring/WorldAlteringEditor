using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Scripts
{
    public class TerrainGeneratorTerrainTypeGroup
    {
        public TerrainGeneratorTerrainTypeGroup(List<TerrainType> terrainTypes, double openChance, double overlapChance)
        {
            TerrainTypes = terrainTypes;
            OpenChance = openChance;
            OverlapChance = overlapChance;
        }

        public List<TerrainType> TerrainTypes { get; }
        public double OpenChance { get; }
        public double OverlapChance { get; }
    }

    public class TerrainGeneratorTileGroup
    {
        public TerrainGeneratorTileGroup(TileSet tileSet, List<int> tileIndicesInSet, double openChance, double overlapChance)
        {
            TileSet = tileSet;
            TileIndicesInSet = tileIndicesInSet;
            OpenChance = openChance;
            OverlapChance = overlapChance;
        }

        public TileSet TileSet { get; }
        public List<int> TileIndicesInSet { get; }
        public double OpenChance { get; }
        public double OverlapChance { get; }
    }

    public class ForestGenerator
    {
        private Map map;
        private List<Point2D> cells = new List<Point2D>();
        private HashSet<Point2D> occupiedCells = new HashSet<Point2D>();
        private Random random;

        public void Generate(Map map, List<Point2D> cells, List<TerrainGeneratorTerrainTypeGroup> terrainTypeGroups, List<TerrainGeneratorTileGroup> tileGroups)
        {
            this.map = map;
            this.cells = cells;
            
            random = new Random();

            // Place terrain objects

            foreach (var terrainTypeGroup in terrainTypeGroups)
            {
                foreach (Point2D cellCoords in cells)
                {
                    bool isOccupied = occupiedCells.Contains(cellCoords);
                    double chance = isOccupied ? terrainTypeGroup.OverlapChance : terrainTypeGroup.OpenChance;

                    if (random.NextDouble() < chance)
                    {
                        int index = random.Next(0, terrainTypeGroup.TerrainTypes.Count);
                        var terrainType = terrainTypeGroup.TerrainTypes[index];
                        if (AllowTreeGroupOnCell(cellCoords, terrainType))
                            PlaceTreeGroupOnCell(cellCoords, terrainType);
                    }
                }
            }

            // Place terrain
            foreach (var regularTileGroup in tileGroups)
            {
                foreach (Point2D cellCoords in cells)
                {
                    int indexInSet;
                    if (regularTileGroup.TileIndicesInSet == null || regularTileGroup.TileIndicesInSet.Count == 0)
                        indexInSet = random.Next(0, regularTileGroup.TileSet.TilesInSet);
                    else
                        indexInSet = regularTileGroup.TileIndicesInSet[random.Next(0, regularTileGroup.TileIndicesInSet.Count)];
                    int totalIndex = regularTileGroup.TileSet.StartTileIndex + indexInSet;
                    var tile = map.TheaterInstance.GetTile(totalIndex);

                    double chance = regularTileGroup.OpenChance;
                    if (IsPlacingTileOnOccupiedArea(cellCoords, tile))
                        chance = regularTileGroup.OverlapChance;

                    if (random.NextDouble() < chance)
                    {
                        if (!AllowPlacingTileOnCell(cellCoords, tile))
                            continue;

                        map.PlaceTerrainTileAt(tile, cellCoords);
                    }
                }
            }

            // Apply auto-LAT
            int minY = cells.Select(p => p.Y).Aggregate((y1, y2) => Math.Min(y1, y2));
            int maxY = cells.Select(p => p.Y).Aggregate((y1, y2) => Math.Max(y1, y2));
            int minX = cells.Select(p => p.X).Aggregate((x1, x2) => Math.Min(x1, x2));
            int maxX = cells.Select(p => p.X).Aggregate((x1, x2) => Math.Max(x1, x2));

            minY--;
            maxY++;
            minX--;
            maxX++;
            ApplyAutoLAT(minX, minY, maxX, maxY);
        }

        private TileSet FindTileSet(string tileSetUIName)
        {
            return map.TheaterInstance.Theater.TileSets.Find(ts => ts.SetName == tileSetUIName && ts.FileName != "blank");
        }

        private void ApplyAutoLAT(int minX, int minY, int maxX, int maxY)
        {
            // Get potential base tilesets of the placed LAT (if we're placing LAT)
            // This allows placing certain LATs on top of other LATs (example: snowy dirt on snow, when snow is also placed on grass)
            // TileSet baseTileSet = null;
            // TileSet altBaseTileSet = null;
            // var tileAutoLatGround = map.TheaterInstance.Theater.LATGrounds.Find(
            //     g => g.GroundTileSet.Index == tileSetId || g.TransitionTileSet.Index == tileSetId);
            // 
            // if (tileAutoLatGround != null && tileAutoLatGround.BaseTileSet != null)
            // {
            //     int baseTileSetId = tileAutoLatGround.BaseTileSet.Index;
            //     var baseLatGround = map.TheaterInstance.Theater.LATGrounds.Find(
            //         g => g.GroundTileSet.Index == baseTileSetId || g.TransitionTileSet.Index == baseTileSetId);
            // 
            //     if (baseLatGround != null)
            //     {
            //         baseTileSet = baseLatGround.GroundTileSet;
            //         altBaseTileSet = baseLatGround.TransitionTileSet;
            //     }
            // }

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    var mapTile = map.GetTile(x, y);
                    if (mapTile == null)
                        continue;

                    int tileSetIndex = map.TheaterInstance.GetTileSetId(mapTile.TileIndex);
                    // Don't auto-lat ground that is a base for our placed ground type
                    // if ((baseTileSet != null && tileSetIndex == baseTileSet.Index) ||
                    //     (altBaseTileSet != null && tileSetIndex == altBaseTileSet.Index))
                    //     return;

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

        private bool AllowPlacingTileOnCell(Point2D cellCoords, ITileImage tile)
        {
            for (int i = 0; i < tile.SubTileCount; i++)
            {
                var subTile = tile.GetSubTile(i);
                if (subTile.TmpImage == null)
                    continue;

                Point2D offset = tile.GetSubTileCoordOffset(i).Value;

                var mapTile = map.GetTile(cellCoords + offset);

                if (mapTile == null || !mapTile.IsClearGround())
                    return false;
            }

            return true;
        }

        private bool IsPlacingTileOnOccupiedArea(Point2D cellCoords, ITileImage tile)
        {
            for (int i = 0; i < tile.SubTileCount; i++)
            {
                var subTile = tile.GetSubTile(i);
                if (subTile.TmpImage == null)
                    continue;

                Point2D offset = tile.GetSubTileCoordOffset(i).Value;

                if (occupiedCells.Contains(cellCoords + offset))
                    return true;
            }

            return false;
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

                if (otherCell.TerrainObject != null)
                    return false;

                if (treeGroup.ImpassableCells != null && occupiedCells.Contains(otherCellCoords))
                    return false;
            }

            return true;
        }

        private void PlaceTreeGroupOnCell(Point2D cellCoords, TerrainType treeGroup)
        {
            var cell = map.GetTile(cellCoords);
            map.AddTerrainObject(new TerrainObject(treeGroup, cellCoords));

            if (treeGroup.ImpassableCells != null)
            {
                foreach (var offset in treeGroup.ImpassableCells)
                    occupiedCells.Add(cellCoords + offset);
            }
            else
            {
                occupiedCells.Add(cellCoords);
            }
        }
    }
}
