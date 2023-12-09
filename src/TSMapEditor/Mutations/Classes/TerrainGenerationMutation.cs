using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    public class TerrainGeneratorConfiguration
    {
        private const string TerrainTypeGroupString = "TerrainTypeGroup";
        private const string TileGroupString = "TileGroup";
        private const string OverlayGroupString = "OverlayGroup";
        private const string SmudgeGroupString = "SmudgeGroup";

        public TerrainGeneratorConfiguration(string name,
            string theater,
            bool isUserConfiguration,
            List<TerrainGeneratorTerrainTypeGroup> terrainTypeGroups,
            List<TerrainGeneratorTileGroup> tileGroups,
            List<TerrainGeneratorOverlayGroup> overlayGroups,
            List<TerrainGeneratorSmudgeGroup> smudgeGroups,
            Color? color = null)
        {
            Name = name;
            Theater = theater;
            Color = color;
            IsUserConfiguration = isUserConfiguration;
            TerrainTypeGroups = terrainTypeGroups;
            TileGroups = tileGroups;
            OverlayGroups = overlayGroups;
            SmudgeGroups = smudgeGroups;
        }

        public string Name { get; }
        public string Theater { get; }
        public Color? Color { get; }
        public bool IsUserConfiguration { get; }
        public List<TerrainGeneratorTerrainTypeGroup> TerrainTypeGroups { get; }
        public List<TerrainGeneratorTileGroup> TileGroups { get; }
        public List<TerrainGeneratorOverlayGroup> OverlayGroups { get; }
        public List<TerrainGeneratorSmudgeGroup> SmudgeGroups { get; }

        public IniSection GetIniConfigSection(string sectionName)
        {
            var iniSection = new IniSection(sectionName);

            iniSection.SetStringValue("Name", Name);
            iniSection.SetStringValue("Theater", Theater);
            if (Color != null)
                iniSection.SetStringValue("Color", Helpers.ColorToString(Color.Value, false));

            for (int i = 0; i < TerrainTypeGroups.Count; i++)
            {
                iniSection.SetStringValue(TerrainTypeGroupString + i, TerrainTypeGroups[i].GetConfigString());
            }

            for (int i = 0; i < TileGroups.Count; i++)
            {
                iniSection.SetStringValue(TileGroupString + i, TileGroups[i].GetConfigString());
            }

            for (int i = 0; i < OverlayGroups.Count; i++)
            {
                iniSection.SetStringValue(OverlayGroupString + i, OverlayGroups[i].GetConfigString());
            }

            for (int i = 0; i < SmudgeGroups.Count; i++)
            {
                iniSection.SetStringValue(SmudgeGroupString + i, SmudgeGroups[i].GetConfigString());
            }

            return iniSection;
        }

        public static TerrainGeneratorConfiguration FromConfigSection(IniSection section, Rules rules, Theater theater, bool isUserConfiguration)
            => FromConfigSection(section, isUserConfiguration, rules.TerrainTypes, theater.TileSets, rules.OverlayTypes, rules.SmudgeTypes);

        public static TerrainGeneratorConfiguration FromConfigSection(IniSection section, bool isUserConfiguration, 
            List<TerrainType> terrainTypes, List<TileSet> tilesets, List<OverlayType> overlayTypes, List<SmudgeType> smudgeTypes)
        {
            var terrainTypeGroups = new List<TerrainGeneratorTerrainTypeGroup>();
            var tileGroups = new List<TerrainGeneratorTileGroup>();
            var overlayGroups = new List<TerrainGeneratorOverlayGroup>();
            var smudgeGroups = new List<TerrainGeneratorSmudgeGroup>();

            string configName = section.GetStringValue("Name", "Unnamed Configuration");
            string theater = section.GetStringValue("Theater", string.Empty);
            Color? color = null;
            if (section.KeyExists("Color"))
                color = Helpers.ColorFromString(section.GetStringValue("Color", "255,255,255"));

            int i = 0;
            while (true)
            {
                string value = section.GetStringValue(TerrainTypeGroupString + i, null);
                if (string.IsNullOrWhiteSpace(value))
                    break;

                var terrainTypeGroup = TerrainGeneratorTerrainTypeGroup.FromConfigString(terrainTypes, value);
                if (terrainTypeGroup == null)
                {
                    Logger.Log($"Failed to load terrain type group #{i} from terrain generator configuration section {section.SectionName}. User preset: {isUserConfiguration}");
                }
                else
                {
                    terrainTypeGroups.Add(terrainTypeGroup);
                }

                i++;
            }

            i = 0;
            while (true)
            {
                string value = section.GetStringValue(TileGroupString + i, null);
                if (string.IsNullOrWhiteSpace(value))
                    break;

                var tileGroup = TerrainGeneratorTileGroup.FromConfigString(tilesets, value);
                if (tileGroup == null)
                {
                    Logger.Log($"Failed to load tile group #{i} from terrain generator configuration section {section.SectionName}. User preset: {isUserConfiguration}");
                }
                else
                {
                    tileGroups.Add(tileGroup);
                }

                i++;
            }

            i = 0;
            while (true)
            {
                string value = section.GetStringValue(OverlayGroupString + i, null);
                if (string.IsNullOrWhiteSpace(value))
                    break;

                var overlayGroup = TerrainGeneratorOverlayGroup.FromConfigString(overlayTypes, value);
                if (overlayGroup == null)
                {
                    Logger.Log($"Failed to load overlay group #{i} from terrain generator configuration section {section.SectionName}. User preset: {isUserConfiguration}");
                }
                else
                {
                    overlayGroups.Add(overlayGroup);
                }

                i++;
            }

            i = 0;
            while (true)
            {
                string value = section.GetStringValue(SmudgeGroupString + i, null);
                if (string.IsNullOrWhiteSpace(value))
                    break;

                var smudgeGroup = TerrainGeneratorSmudgeGroup.FromConfigString(smudgeTypes, value);
                if (smudgeGroup == null)
                {
                    Logger.Log($"Failed to load smudge group #{i} from terrain generator configuration section {section.SectionName}. User preset: {isUserConfiguration}");
                }
                else
                {
                    smudgeGroups.Add(smudgeGroup);
                }

                i++;
            }

            if (terrainTypeGroups.Count > 0 || tileGroups.Count > 0 || overlayGroups.Count > 0 || smudgeGroups.Count > 0)
                return new TerrainGeneratorConfiguration(configName, theater, isUserConfiguration, terrainTypeGroups, tileGroups, overlayGroups, smudgeGroups, color);

            return null;
        }
    }

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

        public string GetConfigString()
        {
            return $"{OpenChance},{OverlapChance}," + string.Join(",", TerrainTypes.Select(tt => tt.ININame));
        }

        public static TerrainGeneratorTerrainTypeGroup FromConfigString(List<TerrainType> allTerrainTypes, string config)
        {
            string[] parts = config.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return null;

            double openChance = Conversions.DoubleFromString(parts[0], 0.0);
            double overlapChance = Conversions.DoubleFromString(parts[1], 0.0);

            string[] terrainTypeStrings = new string[parts.Length - 2];
            Array.Copy(parts, 2, terrainTypeStrings, 0, terrainTypeStrings.Length);
            var terrainTypes = new List<TerrainType>();
            Array.ForEach(terrainTypeStrings, (terrainTypeName) =>
            {
                var terrainType = allTerrainTypes.Find(tt => tt.ININame == terrainTypeName);
                if (terrainType != null)
                    terrainTypes.Add(terrainType);
            });

            return new TerrainGeneratorTerrainTypeGroup(terrainTypes, openChance, overlapChance);
        }
    }

    public class TerrainGeneratorTileGroup
    {
        public TerrainGeneratorTileGroup(TileSet tileSet, List<int> tileIndicesInSet, double openChance, double overlapChance)
        {
            TileSet = tileSet ?? throw new ArgumentNullException(nameof(tileSet));
            TileIndicesInSet = tileIndicesInSet;
            OpenChance = openChance;
            OverlapChance = overlapChance;
        }

        public TileSet TileSet { get; }
        public List<int> TileIndicesInSet { get; }
        public double OpenChance { get; }
        public double OverlapChance { get; }

        public string GetConfigString()
        {
            string config = $"{OpenChance},{OverlapChance},{TileSet.SetName}";
            if (TileIndicesInSet != null && TileIndicesInSet.Count > 0)
            {
                config += "," + string.Join(",", TileIndicesInSet);
            }

            return config;
        }

        public static TerrainGeneratorTileGroup FromConfigString(List<TileSet> allTileSets, string config)
        {
            string[] parts = config.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return null;

            double openChance = Conversions.DoubleFromString(parts[0], 0.0);
            double overlapChance = Conversions.DoubleFromString(parts[1], 0.0);
            var tileSet = allTileSets.Find(ts => ts.AllowToPlace && ts.LoadedTileCount > 0 && ts.SetName == parts[2]);
            if (tileSet == null)
                return null;

            List<int> tileIndices = null;
            if (parts.Length > 3)
            {
                tileIndices = new List<int>();

                for (int i = 3; i < parts.Length; i++)
                {
                    tileIndices.Add(Conversions.IntFromString(parts[i], 0));
                }
            }

            return new TerrainGeneratorTileGroup(tileSet, tileIndices, openChance, overlapChance);
        }
    }

    public class TerrainGeneratorOverlayGroup
    {
        public TerrainGeneratorOverlayGroup(OverlayType overlayType, List<int> frameIndices, double openChance, double overlapChance)
        {
            OverlayType = overlayType;
            FrameIndices = frameIndices;
            OpenChance = openChance;
            OverlapChance = overlapChance;
        }

        public OverlayType OverlayType { get; }
        public List<int> FrameIndices { get; }
        public double OpenChance { get; }
        public double OverlapChance { get; }

        public string GetConfigString()
        {
            string config = $"{OpenChance},{OverlapChance},{OverlayType.ININame}";
            if (FrameIndices != null && FrameIndices.Count > 0)
            {
                config += "," + string.Join(",", FrameIndices);
            }

            return config;
        }

        public static TerrainGeneratorOverlayGroup FromConfigString(List<OverlayType> allOverlayTypes, string config)
        {
            string[] parts = config.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 3)
                return null;

            double openChance = Conversions.DoubleFromString(parts[0], 0.0);
            double overlapChance = Conversions.DoubleFromString(parts[1], 0.0);
            var overlayType = allOverlayTypes.Find(ot => ot.ININame == parts[2]);
            List<int> frameIndices = null;
            if (parts.Length > 3)
            {
                frameIndices = new List<int>();

                for (int i = 3; i < parts.Length; i++)
                {
                    frameIndices.Add(Conversions.IntFromString(parts[i], 0));
                }
            }

            return new TerrainGeneratorOverlayGroup(overlayType, frameIndices, openChance, overlapChance);
        }
    }

    public class TerrainGeneratorSmudgeGroup
    {
        public TerrainGeneratorSmudgeGroup(List<SmudgeType> smudgeTypes, double openChance, double overlapChance)
        {
            SmudgeTypes = smudgeTypes;
            OpenChance = openChance;
            OverlapChance = overlapChance;
        }

        public List<SmudgeType> SmudgeTypes { get; }
        public double OpenChance { get; }
        public double OverlapChance { get; }

        public string GetConfigString()
        {
            return $",{OpenChance},{OverlapChance}," + string.Join(",", SmudgeTypes.Select(tt => tt.ININame));
        }

        public static TerrainGeneratorSmudgeGroup FromConfigString(List<SmudgeType> allSmudgeTypes, string config)
        {
            string[] parts = config.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3)
                return null;

            double openChance = Conversions.DoubleFromString(parts[0], 0.0);
            double overlapChance = Conversions.DoubleFromString(parts[1], 0.0);

            string[] smudgeTypeStrings = new string[parts.Length - 2];
            Array.Copy(parts, 2, smudgeTypeStrings, 0, smudgeTypeStrings.Length);
            var smudgeTypes = new List<SmudgeType>();
            Array.ForEach(smudgeTypeStrings, (smudgeTypeName) =>
            {
                var smudgeType = allSmudgeTypes.Find(st => st.ININame == smudgeTypeName);
                if (smudgeType != null)
                    smudgeTypes.Add(smudgeType);
            });

            return new TerrainGeneratorSmudgeGroup(smudgeTypes, openChance, overlapChance);
        }
    }

    public class TerrainGenerationMutation : Mutation
    {
        public TerrainGenerationMutation(IMutationTarget mutationTarget, List<Point2D> cells, TerrainGeneratorConfiguration configuration) : base(mutationTarget)
        {
            seed = DateTime.Now.Millisecond;
            random = new Random();
            this.cells = cells;
            this.terrainGeneratorConfiguration = configuration;
        }

        private readonly int seed;
        private readonly List<Point2D> cells;
        private readonly TerrainGeneratorConfiguration terrainGeneratorConfiguration;

        private HashSet<Point2D> occupiedCells = new HashSet<Point2D>();
        private Random random;

        private List<OriginalTerrainData> undoData;
        private List<TerrainObject> placedTerrainObjects;
        private List<Point2D> placedOverlayCellCoords;
        private List<Point2D> placedSmudgeCellCoords;

        private bool wasPerformedWithAutoLatOn;

        private static readonly Point2D[] surroundingTiles = new Point2D[] { new Point2D(-1, 0), new Point2D(1, 0), new Point2D(0, -1), new Point2D(0, 1) };

        public override void Perform()
        {
            Generate();
        }

        public override void Undo()
        {
            foreach (var originalTerrainData in undoData)
            {
                var mapCell = MutationTarget.Map.GetTile(originalTerrainData.CellCoords);
                mapCell.ChangeTileIndex(originalTerrainData.TileIndex, originalTerrainData.SubTileIndex);
                mapCell.Level = originalTerrainData.Level;
            }

            foreach (var terrainObject in placedTerrainObjects)
            {
                MutationTarget.Map.RemoveTerrainObject(terrainObject);
            }

            foreach (var cellCoords in placedOverlayCellCoords)
            {
                var mapCell = MutationTarget.Map.GetTile(cellCoords);
                mapCell.Overlay = null;
            }

            foreach (var cellCoords in placedSmudgeCellCoords)
            {
                var mapCell = MutationTarget.Map.GetTile(cellCoords);
                mapCell.Smudge = null;
            }

            if (wasPerformedWithAutoLatOn)
            {
                ApplyAutoLATOnArea();
            }

            MutationTarget.InvalidateMap();

            occupiedCells.Clear();
        }

        public void Generate()
        {
            random = new Random(seed);

            undoData = new List<OriginalTerrainData>();
            placedTerrainObjects = new List<TerrainObject>();
            placedOverlayCellCoords = new List<Point2D>();
            placedSmudgeCellCoords = new List<Point2D>();

            var terrainTypeGroups = terrainGeneratorConfiguration.TerrainTypeGroups;
            var tileGroups = terrainGeneratorConfiguration.TileGroups;
            var overlayGroups = terrainGeneratorConfiguration.OverlayGroups;
            var smudgeGroups = terrainGeneratorConfiguration.SmudgeGroups;

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
                    var tile = MutationTarget.Map.TheaterInstance.GetTile(totalIndex);

                    double chance = regularTileGroup.OpenChance;
                    if (IsPlacingTileOnOccupiedArea(cellCoords, tile))
                        chance = regularTileGroup.OverlapChance;

                    if (random.NextDouble() < chance)
                    {
                        if (!AllowPlacingTileOnCell(cellCoords, tile))
                            continue;

                        PlaceTerrainTileAt(tile, cellCoords);
                    }
                }
            }

            // Place overlay
            foreach (var overlayGroup in overlayGroups)
            {
                foreach (Point2D cellCoords in cells)
                {
                    double chance = overlayGroup.OpenChance;

                    var cell = MutationTarget.Map.GetTile(cellCoords);
                    ITileImage tile = MutationTarget.Map.TheaterInstance.GetTile(cell.TileIndex);
                    ISubTileImage subTile = tile.GetSubTile(cell.SubTileIndex);

                    if (overlayGroup.OverlayType.WaterBound)
                    {
                        if (!Helpers.IsLandTypeWater(subTile.TmpImage.TerrainType))
                            continue;
                    }
                    else if (Helpers.IsLandTypeImpassable(subTile.TmpImage.TerrainType, true))
                    {
                        continue;
                    }

                    if (cell.TerrainObject != null)
                        chance = overlayGroup.OverlapChance;

                    if (cell.Overlay != null && cell.Overlay.OverlayType != null)
                        continue;

                    if (random.NextDouble() < chance)
                    {
                        int frameIndex;
                        if (overlayGroup.FrameIndices != null && overlayGroup.FrameIndices.Count > 0)
                            frameIndex = overlayGroup.FrameIndices[random.Next(overlayGroup.FrameIndices.Count)];
                        else
                            frameIndex = random.Next(MutationTarget.Map.TheaterInstance.GetOverlayFrameCount(overlayGroup.OverlayType));

                        placedOverlayCellCoords.Add(cellCoords);
                        cell.Overlay = new Overlay()
                        {
                            OverlayType = overlayGroup.OverlayType,
                            FrameIndex = frameIndex,
                            Position = cellCoords
                        };
                    }
                }
            }

            // Place smudges
            foreach (var smudgeGroup in smudgeGroups)
            {
                foreach (Point2D cellCoords in cells)
                {
                    bool isOccupied = occupiedCells.Contains(cellCoords);
                    double chance = isOccupied ? smudgeGroup.OverlapChance : smudgeGroup.OpenChance;

                    if (random.NextDouble() < chance)
                    {
                        int index = random.Next(0, smudgeGroup.SmudgeTypes.Count);
                        var smudgeType = smudgeGroup.SmudgeTypes[index];
                        var mapCell = MutationTarget.Map.GetTile(cellCoords);
                        if (mapCell.Smudge == null)
                            mapCell.Smudge = new Smudge() { SmudgeType = smudgeType, Position = cellCoords };

                        placedSmudgeCellCoords.Add(cellCoords);
                    }
                }
            }

            // Apply auto-LAT
            if (MutationTarget.AutoLATEnabled)
            {
                ApplyAutoLATOnArea();
                wasPerformedWithAutoLatOn = true;
            }

            MutationTarget.InvalidateMap();
        }

        private void ApplyAutoLATOnArea()
        {
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

        private void PlaceTerrainTileAt(ITileImage tile, Point2D cellCoords)
        {
            for (int i = 0; i < tile.SubTileCount; i++)
            {
                var subTile = tile.GetSubTile(i);
                if (subTile.TmpImage == null)
                    continue;

                Point2D offset = tile.GetSubTileCoordOffset(i).Value;

                var mapTile = MutationTarget.Map.GetTile(cellCoords + offset);
                if (mapTile == null)
                    continue;

                undoData.Add(new OriginalTerrainData(mapTile.TileIndex, mapTile.SubTileIndex, mapTile.Level, cellCoords + offset));

                mapTile.TileImage = null;
                mapTile.TileIndex = tile.TileID;
                mapTile.SubTileIndex = (byte)i;
            }
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

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var mapTile = MutationTarget.Map.GetTile(x, y);
                    if (mapTile == null)
                        continue;

                    int tileSetIndex = MutationTarget.Map.TheaterInstance.GetTileSetId(mapTile.TileIndex);

                    var latGrounds = MutationTarget.Map.TheaterInstance.Theater.LATGrounds;

                    // If we're not on a tile can be auto-LAT'd in the first place, skip
                    var ourLatGround = latGrounds.Find(lg => lg.GroundTileSet.Index == tileSetIndex || lg.TransitionTileSet.Index == tileSetIndex);
                    if (ourLatGround == null)
                        continue;

                    // Look at the surrounding tiles to figure out the base tile set ID we should use
                    int baseTileSetId = -1;

                    foreach (var otherTileOffset in surroundingTiles)
                    {
                        var otherTile = MutationTarget.Map.GetTile(x + otherTileOffset.X, y + otherTileOffset.Y);
                        if (otherTile == null)
                            continue;

                        int otherTileSetId = MutationTarget.Map.TheaterInstance.GetTileSetId(otherTile.TileIndex);
                        if (otherTileSetId != tileSetIndex)
                        {
                            // Check that the other tile is not a transitional tile type
                            var otherLatGround = latGrounds.Find(lg => lg.TransitionTileSet.Index == otherTileSetId);

                            if (otherLatGround == null)
                            {
                                if (otherTileSetId == 0 || latGrounds.Exists(lg => lg.BaseTileSet.Index == otherTileSetId))
                                {
                                    baseTileSetId = otherTileSetId;
                                    break;
                                }
                                else if (otherTileSetId != 0 && !latGrounds.Exists(lg => lg.BaseTileSet.Index == otherTileSetId))
                                {
                                    baseTileSetId = 0;
                                    continue;
                                }
                            }
                            else
                            {
                                // If it is a transitional tile type, then take its base tile set for our base tile set
                                // .. UNLESS we can connect to the transition smoothly as indicated by the non-transition
                                // ground tileset of the other cell's LAT being our base tileset,
                                // then take the actual non-transition ground for our base
                                if (ourLatGround.BaseTileSet == otherLatGround.GroundTileSet)
                                    baseTileSetId = otherLatGround.GroundTileSet.Index;
                                else
                                    baseTileSetId = otherLatGround.BaseTileSet.Index;

                                break;
                            }
                        }
                    }

                    if (baseTileSetId == -1)
                    {
                        // Based on the surrounding tiles, we shouldn't need to use any base tile set
                        mapTile.TileIndex = MutationTarget.Map.TheaterInstance.Theater.TileSets[tileSetIndex].StartTileIndex;
                        mapTile.SubTileIndex = 0;
                        mapTile.TileImage = null;
                        continue;
                    }

                    var tileSet = MutationTarget.Map.TheaterInstance.Theater.TileSets[tileSetIndex];
                    // Don't auto-lat ground that is a base for our placed ground type
                    // if ((baseTileSet != null && tileSetIndex == baseTileSet.Index) ||
                    //     (altBaseTileSet != null && tileSetIndex == altBaseTileSet.Index))
                    //     return;

                    // MutationTarget.Map.TheaterInstance.Theater.TileSets[tileSetIndex].SetName.StartsWith("~~~")

                    // When applying auto-LAT to an alt. terrain tile set, don't apply a transition when we are
                    // evaluating a base alt. terrain tile set next to ground that is supposed on place on that
                    // alt. terrain
                    // For example, ~~~Snow shouldn't be auto-LAT'd when it's next to a tile belonging to ~~~Straight Dirt Roads

                    Func<TileSet, bool> miscChecker = null;
                    if (tileSet.SetName.StartsWith("~~~") && latGrounds.Exists(g => g.BaseTileSet == tileSet))
                    {
                        miscChecker = (ts) =>
                        {
                            // On its own line so it's possible to debug this
                            return ts.SetName.StartsWith("~~~") && !latGrounds.Exists(g => g.GroundTileSet == ts);
                        };
                    }

                    var autoLatGround = latGrounds.Find(g => (g.GroundTileSet.Index == tileSetIndex || g.TransitionTileSet.Index == tileSetIndex) &&
                        g.TransitionTileSet.Index != baseTileSetId && g.BaseTileSet.Index == baseTileSetId);

                    if (autoLatGround != null)
                    {
                        int autoLatIndex = MutationTarget.Map.GetAutoLATIndex(mapTile, autoLatGround.GroundTileSet.Index, autoLatGround.TransitionTileSet.Index, miscChecker);
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

                var mapTile = MutationTarget.Map.GetTile(cellCoords + offset);

                if (mapTile == null)
                    return false;

                var tileSetId = MutationTarget.Map.TheaterInstance.GetTileSetId(mapTile.TileIndex);
                bool isBase = MutationTarget.Map.TheaterInstance.Theater.LATGrounds.Exists(latg => latg.BaseTileSet != null && latg.BaseTileSet.Index == tileSetId);

                if (!mapTile.IsClearGround() && !isBase)
                    return false;

                if (mapTile.HasTiberium() && Helpers.IsLandTypeImpassable(subTile.TmpImage.TerrainType, true))
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
            var cell = MutationTarget.Map.GetTile(cellCoords);
            if (cell == null)
                return false;

            if (cell.TerrainObject != null)
                return false;

            ITileImage tile = MutationTarget.Map.TheaterInstance.GetTile(cell.TileIndex);
            ISubTileImage subTile = tile.GetSubTile(cell.SubTileIndex);
            if (Helpers.IsLandTypeImpassable(subTile.TmpImage.TerrainType, true))
                return false;

            if (treeGroup.ImpassableCells == null)
                return !occupiedCells.Contains(cellCoords);

            foreach (var offset in treeGroup.ImpassableCells)
            {
                var otherCellCoords = cellCoords + offset;
                var otherCell = MutationTarget.Map.GetTile(otherCellCoords);
                if (otherCell == null)
                    continue;

                tile = MutationTarget.Map.TheaterInstance.GetTile(otherCell.TileIndex);
                subTile = tile.GetSubTile(otherCell.SubTileIndex);
                if (subTile.TmpImage.TerrainType != 0x0)
                    return false;

                if (otherCell.TerrainObject != null)
                    return false;

                if (treeGroup.ImpassableCells != null && occupiedCells.Contains(otherCellCoords))
                    return false;
            }

            return true;
        }

        private void PlaceTreeGroupOnCell(Point2D cellCoords, TerrainType treeGroup)
        {
            var cell = MutationTarget.Map.GetTile(cellCoords);
            var terrainObject = new TerrainObject(treeGroup, cellCoords);
            MutationTarget.Map.AddTerrainObject(new TerrainObject(treeGroup, cellCoords));

            if (treeGroup.ImpassableCells != null)
            {
                foreach (var offset in treeGroup.ImpassableCells)
                    occupiedCells.Add(cellCoords + offset);
            }
            else
            {
                occupiedCells.Add(cellCoords);
            }

            placedTerrainObjects.Add(terrainObject);
        }
    }
}
