using CNCMaps.FileFormats.Encodings;
using Microsoft.Xna.Framework;
using Rampastring.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Models.MapFormat;

namespace TSMapEditor.Initialization
{
    /// <summary>
    /// Contains functions for parsing and applying different sections of a map file.
    /// </summary>
    public static class MapLoader
    {
        private const int BUILDING_PROPERTY_FIELD_COUNT = 17;
        private const int UNIT_PROPERTY_FIELD_COUNT = 14;
        private const int INFANTRY_PROPERTY_FIELD_COUNT = 14;
        private const int AIRCRAFT_PROPERTY_FIELD_COUNT = 12;

        public static void ReadMapSection(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Map");
            if (section == null)
                throw new MapLoadException("[Map] does not exist in the loaded file!");

            string size = section.GetStringValue("Size", null);
            if (size == null)
                throw new MapLoadException("Invalid [Map] Size=");
            string[] parts = size.Split(',');
            if (parts.Length != 4)
                throw new MapLoadException("Invalid [Map] Size=");

            int width = int.Parse(parts[2]);
            int height = int.Parse(parts[3]);
            map.Size = new Point2D(width, height);

            string localSize = section.GetStringValue("LocalSize", null);
            if (localSize == null)
                throw new MapLoadException("Invalid [Map] LocalSize=");
            parts = localSize.Split(',');
            if (parts.Length != 4)
                throw new MapLoadException("Invalid [Map] LocalSize=");

            map.LocalSize = new Rectangle(
                Conversions.IntFromString(parts[0], 0),
                Conversions.IntFromString(parts[1], 0),
                Conversions.IntFromString(parts[2], width),
                Conversions.IntFromString(parts[3], height));

            map.TheaterName = section.GetStringValue("Theater", string.Empty);
        }

        public static void ReadIsoMapPack(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("IsoMapPack5");
            if (section == null)
                return;

            StringBuilder sb = new StringBuilder();
            section.Keys.ForEach(kvp => sb.Append(kvp.Value));

            byte[] compressedData = Convert.FromBase64String(sb.ToString());
            if (compressedData.Length < 4)
                throw new InvalidOperationException("Invalid IsoMapPack5 format");

            Logger.Log("IsoMapPack5 CompressedData length: " + compressedData.Length);

            List<byte> uncompressedData = new List<byte>();

            int position = 0;

            while (position < compressedData.Length)
            {
                ushort inputSize = BitConverter.ToUInt16(compressedData, position);
                ushort outputSize = BitConverter.ToUInt16(compressedData, position + 2);

                Logger.Log("Decoding IsoMapPack5 block: pos: " + position + ", inSize: " + inputSize + ", outSize: " + outputSize);

                if (position + inputSize + 4 > compressedData.Length)
                    throw new InvalidOperationException("Error decoding IsoMapPack5");

                byte[] inData = new byte[inputSize];
                Array.Copy(compressedData, position + 4, inData, 0, inputSize);
                byte[] outData = new byte[outputSize];
                MiniLZO.MiniLZO.Decompress(inData, outData);
                uncompressedData.AddRange(outData);

                position += inputSize + 4;
            }

            // if (uncompressedData.Count % IsoMapPack5Tile.Size != 0)
            //     throw new InvalidOperationException("Decompressed IsoMapPack5 size does not match expected struct size");

            var tiles = new List<MapTile>(uncompressedData.Count % IsoMapPack5Tile.Size);
            position = 0;
            while (position < uncompressedData.Count - IsoMapPack5Tile.Size)
            {
                // This could be optimized by not creating the tile at all if its tile index is 0xFFFF
                var mapTile = new MapTile(uncompressedData.GetRange(position, IsoMapPack5Tile.Size).ToArray());
                if (mapTile.TileIndex != ushort.MaxValue)
                {
                    tiles.Add(mapTile);
                }
                position += IsoMapPack5Tile.Size;
            }

            map.SetTileData(tiles);
        }

        public static void ReadBasicSection(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Basic");
            if (section == null)
                return;

            map.Basic.ReadPropertiesFromIniSection(section);
        }

        public static void ReadTerrainObjects(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Terrain");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                string coords = kvp.Key;
                int yLength = coords.Length - 3;
                int y = Conversions.IntFromString(coords.Substring(0, yLength), -1);
                int x = Conversions.IntFromString(coords.Substring(yLength), -1);
                if (y < 0 || x < 0)
                    continue;

                TerrainType terrainType = map.Rules.TerrainTypes.Find(tt => tt.ININame == kvp.Value);
                if (terrainType == null)
                {
                    Logger.Log($"Skipping loading of terrain type {kvp.Value} because it does not exist in Rules");
                    continue;
                }

                var terrainObject = new TerrainObject(terrainType, new Point2D(x, y));
                map.TerrainObjects.Add(terrainObject);
                var tile = map.GetTile(x, y);
                if (tile != null)
                    tile.TerrainObject = terrainObject;
            }
        }

        private static void FindAttachedTag(IMap map, TechnoBase techno, string attachedTagString)
        {
            if (attachedTagString != Constants.NoneValue1 && attachedTagString != Constants.NoneValue2)
            {
                Tag tag = map.Tags.Find(t => t.ID == attachedTagString);
                if (tag == null)
                {
                    Logger.Log($"Unable to find tag {attachedTagString} attached to {techno.WhatAmI()}");
                    return;
                }

                techno.AttachedTag = tag;
            }
        }

        private static BuildingType FindUpgrade(string upgradeBuildingId, IMap map)
        {
            if (upgradeBuildingId.Equals(Constants.NoneValue1, StringComparison.InvariantCultureIgnoreCase) ||
                upgradeBuildingId.Equals(Constants.NoneValue2, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return map.Rules.BuildingTypes.Find(b => b.ININame == upgradeBuildingId);
        }

        public static void ReadBuildings(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Structures");
            if (section == null)
                return;

            // [Structures]
            // INDEX=OWNER,ID,HEALTH,X,Y,FACING,TAG,AI_SELLABLE,AI_REBUILDABLE,POWERED_ON,UPGRADES,SPOTLIGHT,UPGRADE_1,UPGRADE_2,UPGRADE_3,AI_REPAIRABLE,NOMINAL

            foreach (var kvp in section.Keys)
            {
                string[] values = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < BUILDING_PROPERTY_FIELD_COUNT)
                    continue;

                string ownerName = values[0];
                string buildingTypeId = values[1];
                int health = Math.Min(Constants.ObjectHealthMax, Math.Max(0, Conversions.IntFromString(values[2], Constants.ObjectHealthMax)));
                int x = Conversions.IntFromString(values[3], 0);
                int y = Conversions.IntFromString(values[4], 0);
                int facing = Math.Min(Constants.FacingMax, Math.Max(0, Conversions.IntFromString(values[5], Constants.FacingMax)));
                string attachedTag = values[6];
                bool aiSellable = Conversions.BooleanFromString(values[7], true);
                // AI_REBUILDABLE is a leftover
                bool powered = Conversions.BooleanFromString(values[9], true);
                int upgradeCount = Conversions.IntFromString(values[10], 0);
                int spotlight = Conversions.IntFromString(values[11], 0);
                string[] upgradeIds = new string[] { values[12], values[13], values[14] };
                bool aiRepairable = Conversions.BooleanFromString(values[15], false);
                bool nominal = Conversions.BooleanFromString(values[16], false);

                var buildingType = map.Rules.BuildingTypes.Find(bt => bt.ININame == buildingTypeId);
                if (buildingType == null)
                {
                    Logger.Log($"Unable to find building type {buildingTypeId} - skipping it.");
                    continue;
                }

                House owner = map.FindOrMakeHouse(ownerName);
                var building = new Structure(buildingType)
                {
                    HP = health,
                    Position = new GameMath.Point2D(x, y),
                    Facing = (byte)facing,
                    AISellable = aiSellable,
                    Powered = powered,
                    // TODO handle upgrades
                    Spotlight = (SpotlightType)spotlight,
                    AIRepairable = aiRepairable,
                    Nominal = nominal,
                    Owner = map.FindOrMakeHouse(ownerName)
                };

                for (int i = 0; i < upgradeCount && i < buildingType.Upgrades && i < Structure.MaxUpgradeCount; i++)
                {
                    building.Upgrades[i] = FindUpgrade(upgradeIds[i], map);
                }

                FindAttachedTag(map, building, attachedTag);

                map.Structures.Add(building);
                for (int foundationY = 0; foundationY < buildingType.ArtConfig.FoundationY; foundationY++)
                {
                    for (int foundationX = 0; foundationX < buildingType.ArtConfig.FoundationX; foundationX++)
                    {
                        var tile = map.GetTile(x + foundationX, y + foundationY);
                        if (tile != null)
                            tile.Structure = building;
                    }
                }

                if (buildingType.ArtConfig.FoundationX == 0 || buildingType.ArtConfig.FoundationY == 0)
                {
                    var tile = map.GetTile(x, y);
                    if (tile != null)
                        tile.Structure = building;
                }
            }
        }

        public static void ReadAircraft(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Aircraft");
            if (section == null)
                return;

            // [Aircraft]
            // INDEX=OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

            foreach (var kvp in section.Keys)
            {
                string[] values = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < AIRCRAFT_PROPERTY_FIELD_COUNT)
                    continue;

                string ownerName = values[0];
                string aircraftTypeId = values[1];
                int health = Math.Min(Constants.ObjectHealthMax, Math.Max(0, Conversions.IntFromString(values[2], Constants.ObjectHealthMax)));
                int x = Conversions.IntFromString(values[3], 0);
                int y = Conversions.IntFromString(values[4], 0);
                int facing = Math.Min(Constants.FacingMax, Math.Max(0, Conversions.IntFromString(values[5], Constants.FacingMax)));
                string mission = values[6];
                string attachedTag = values[7];
                int veterancy = Conversions.IntFromString(values[8], 0);
                int group = Conversions.IntFromString(values[9], 0);
                bool autocreateNoRecruitable = Conversions.BooleanFromString(values[10], false);
                bool autocreateYesRecruitable = Conversions.BooleanFromString(values[11], false);

                var aircraftType = map.Rules.AircraftTypes.Find(ut => ut.ININame == aircraftTypeId);
                if (aircraftType == null)
                {
                    Logger.Log($"Unable to find aircraft type {aircraftTypeId} - skipping it.");
                    continue;
                }

                var aircraft = new Aircraft(aircraftType)
                {
                    HP = health,
                    Position = new Point2D(x, y),
                    Facing = (byte)facing,
                    Mission = mission,
                    Veterancy = veterancy,
                    Group = group,
                    AutocreateNoRecruitable = autocreateNoRecruitable,
                    AutocreateYesRecruitable = autocreateYesRecruitable,
                    Owner = map.FindOrMakeHouse(ownerName)
                };

                FindAttachedTag(map, aircraft, attachedTag);

                map.Aircraft.Add(aircraft);
                var tile = map.GetTile(x, y);
                if (tile != null)
                    tile.Aircraft = aircraft;
            }
        }

        public static void ReadUnits(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Units");
            if (section == null)
                return;

            // [Units]
            // INDEX=OWNER,ID,HEALTH,X,Y,FACING,MISSION,TAG,VETERANCY,GROUP,HIGH,FOLLOWS_INDEX,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

            foreach (var kvp in section.Keys)
            {
                string[] values = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < UNIT_PROPERTY_FIELD_COUNT)
                    continue;

                string ownerName = values[0];
                string unitTypeId = values[1];
                int health = Math.Min(Constants.ObjectHealthMax, Math.Max(0, Conversions.IntFromString(values[2], Constants.ObjectHealthMax)));
                int x = Conversions.IntFromString(values[3], 0);
                int y = Conversions.IntFromString(values[4], 0);
                int facing = Math.Min(Constants.FacingMax, Math.Max(0, Conversions.IntFromString(values[5], Constants.FacingMax)));
                string mission = values[6];
                string attachedTag = values[7];
                int veterancy = Conversions.IntFromString(values[8], 0);
                int group = Conversions.IntFromString(values[9], 0);
                bool high = Conversions.BooleanFromString(values[10], false);
                int followsIndex = Conversions.IntFromString(values[11], 0);
                bool autocreateNoRecruitable = Conversions.BooleanFromString(values[12], false);
                bool autocreateYesRecruitable = Conversions.BooleanFromString(values[13], false);

                var unitType = map.Rules.UnitTypes.Find(ut => ut.ININame == unitTypeId);
                if (unitType == null)
                {
                    Logger.Log($"Unable to find unit type {unitTypeId} - skipping it.");
                    continue;
                }

                var unit = new Unit(unitType)
                {
                    HP = health,
                    Position = new Point2D(x, y),
                    Facing = (byte)facing,
                    Mission = mission,
                    Veterancy = veterancy,
                    Group = group,
                    High = high,
                    FollowsID = followsIndex,
                    AutocreateNoRecruitable = autocreateNoRecruitable,
                    AutocreateYesRecruitable = autocreateYesRecruitable,
                    Owner = map.FindOrMakeHouse(ownerName)
                };

                FindAttachedTag(map, unit, attachedTag);

                map.Units.Add(unit);
                var tile = map.GetTile(x, y);
                if (tile != null)
                    tile.Vehicle = unit;
            }

            // Process follow IDs
            foreach (var unit in map.Units)
            {
                if (unit.FollowsID < 0)
                    continue;

                if (unit.FollowsID >= map.Units.Count)
                    continue;

                unit.FollowedUnit = map.Units[unit.FollowsID];
            }
        }

        public static void ReadInfantry(IMap map, IniFile mapIni)
        {
            IniSection section = mapIni.GetSection("Infantry");
            if (section == null)
                return;

            // [Infantry]
            // INDEX=OWNER,ID,HEALTH,X,Y,SUB_CELL,MISSION,FACING,TAG,VETERANCY,GROUP,HIGH,AUTOCREATE_NO_RECRUITABLE,AUTOCREATE_YES_RECRUITABLE

            foreach (var kvp in section.Keys)
            {
                string[] values = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < INFANTRY_PROPERTY_FIELD_COUNT)
                    continue;

                string ownerName = values[0];
                string infantryTypeId = values[1];
                int health = Math.Min(Constants.ObjectHealthMax, Math.Max(0, Conversions.IntFromString(values[2], Constants.ObjectHealthMax)));
                int x = Conversions.IntFromString(values[3], 0);
                int y = Conversions.IntFromString(values[4], 0);
                SubCell subCell = (SubCell)Conversions.IntFromString(values[5], 0);
                string mission = values[6];
                int facing = Math.Min(Constants.FacingMax, Math.Max(0, Conversions.IntFromString(values[7], Constants.FacingMax)));
                string attachedTag = values[8];
                int veterancy = Conversions.IntFromString(values[9], 0);
                int group = Conversions.IntFromString(values[10], 0);
                bool high = Conversions.BooleanFromString(values[11], false);
                bool autocreateNoRecruitable = Conversions.BooleanFromString(values[12], false);
                bool autocreateYesRecruitable = Conversions.BooleanFromString(values[13], false);

                var infantryType = map.Rules.InfantryTypes.Find(it => it.ININame == infantryTypeId);
                if (infantryType == null)
                {
                    Logger.Log($"Unable to find infantry type {infantryTypeId} - skipping it.");
                    continue;
                }

                var infantry = new Infantry(infantryType)
                {
                    HP = health,
                    Position = new Point2D(x, y), // TODO handle sub-cell in position?
                    Facing = (byte)facing,
                    Veterancy = veterancy,
                    Group = group,
                    High = high,
                    AutocreateNoRecruitable = autocreateNoRecruitable,
                    AutocreateYesRecruitable = autocreateYesRecruitable,
                    SubCell = subCell,
                    Mission = mission,
                    Owner = map.FindOrMakeHouse(ownerName)
                };

                FindAttachedTag(map, infantry, attachedTag);

                map.Infantry.Add(infantry);
                var tile = map.GetTile(x, y);
                if (tile != null)
                    tile.Infantry[(int)subCell] = infantry;
            }
        }

        public static void ReadOverlays(IMap map, IniFile mapIni)
        {
            var overlayPackSection = mapIni.GetSection("OverlayPack");
            var overlayDataPackSection = mapIni.GetSection("OverlayDataPack");
            if (overlayPackSection == null || overlayDataPackSection == null)
                return;

            var stringBuilder = new StringBuilder();
            overlayPackSection.Keys.ForEach(kvp => stringBuilder.Append(kvp.Value));
            byte[] compressedData = Convert.FromBase64String(stringBuilder.ToString());
            byte[] uncompressedOverlayPack = new byte[Constants.MAX_MAP_LENGTH_IN_DIMENSION * Constants.MAX_MAP_LENGTH_IN_DIMENSION];
            Format5.DecodeInto(compressedData, uncompressedOverlayPack, Constants.OverlayPackFormat);

            stringBuilder.Clear();
            overlayDataPackSection.Keys.ForEach(kvp => stringBuilder.Append(kvp.Value));
            compressedData = Convert.FromBase64String(stringBuilder.ToString());
            byte[] uncompressedOverlayDataPack = new byte[Constants.MAX_MAP_LENGTH_IN_DIMENSION * Constants.MAX_MAP_LENGTH_IN_DIMENSION];
            Format5.DecodeInto(compressedData, uncompressedOverlayDataPack, Constants.OverlayPackFormat);

            for (int y = 0; y < map.Tiles.Length; y++)
            {
                for (int x = 0; x < map.Tiles[y].Length; x++)
                {
                    var tile = map.Tiles[y][x];
                    if (tile == null)
                        continue;

                    int overlayDataIndex = (tile.Y * Constants.MAX_MAP_LENGTH_IN_DIMENSION) + tile.X;
                    int overlayTypeIndex = uncompressedOverlayPack[overlayDataIndex];
                    if (overlayTypeIndex == Constants.NO_OVERLAY)
                        continue;

                    if (overlayTypeIndex >= map.Rules.OverlayTypes.Count)
                    {
                        Logger.Log("Ignoring overlay on tile at " + x + ", " + y + " because it's out of bounds compared to Rules.ini overlay list");
                        continue;
                    }

                    var overlayType = map.Rules.OverlayTypes[overlayTypeIndex];
                    var overlay = new Overlay()
                    {
                        OverlayType = overlayType,
                        FrameIndex = uncompressedOverlayDataPack[overlayDataIndex],
                        Position = new Point2D(tile.X, tile.Y)
                    };
                    tile.Overlay = overlay;
                }
            }
        }

        public static void ReadWaypoints(IMap map, IniFile mapIni)
        {
            var waypointsSection = mapIni.GetSection("Waypoints");
            if (waypointsSection == null)
                return;

            foreach (var kvp in waypointsSection.Keys)
            {
                var waypoint = Waypoint.ParseWaypoint(kvp.Key, kvp.Value);
                if (waypoint == null)
                    continue;

                if (map.GetTile(waypoint.Position.X, waypoint.Position.Y) == null)
                    continue;

                map.AddWaypoint(waypoint);
            }
        }

        public static void ReadTaskForces(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("TaskForces");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                if (string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                var taskForce = TaskForce.ParseTaskForce(map.Rules, mapIni.GetSection(kvp.Value));

                map.AddTaskForce(taskForce);
            }
        }

        public static void ReadTriggers(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Triggers");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                var trigger = Trigger.ParseTrigger(kvp.Key, kvp.Value);
                if (trigger != null)
                    map.AddTrigger(trigger);

                string actionData = mapIni.GetStringValue("Actions", trigger.ID, null);
                trigger.ParseActions(actionData);

                string conditionData = mapIni.GetStringValue("Events", trigger.ID, null);
                trigger.ParseConditions(conditionData);
            }

            // Parse and apply linked triggers
            foreach (var trigger in map.Triggers)
            {
                if (Helpers.IsStringNoneValue(trigger.LinkedTriggerId))
                    continue;

                trigger.LinkedTrigger = map.Triggers.Find(otherTrigger => otherTrigger.ID == trigger.LinkedTriggerId);
            }
        }

        public static void ReadTags(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Tags");
            if (section == null)
                return;

            // [Tags]
            // ID=REPEATING,NAME,TRIGGER_ID

            foreach (var kvp in section.Keys)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                string[] parts = kvp.Value.Split(',');
                if (parts.Length != 3)
                    continue;

                int repeating = Conversions.IntFromString(parts[0], -1);
                if (repeating < 0 || repeating > Tag.REPEAT_TYPE_MAX)
                    continue;

                string triggerId = parts[2];
                Trigger trigger = map.Triggers.Find(t => t.ID == triggerId);
                if (trigger == null)
                {
                    Logger.Log("Ignoring tag " + kvp.Key + " because its related trigger " + triggerId + " does not exist!");
                    continue;
                }

                var tag = new Tag() { ID = kvp.Key, Repeating = repeating, Name = parts[1], Trigger = trigger };
                map.AddTag(tag);
            }
        }

        public static void ReadScripts(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("ScriptTypes");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                var script = Script.ParseScript(kvp.Value, mapIni.GetSection(kvp.Value));

                if (script != null)
                    map.AddScript(script);
            }
        }

        public static void ReadTeamTypes(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("TeamTypes");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key) || string.IsNullOrWhiteSpace(kvp.Value))
                    continue;

                var teamTypeSection = mapIni.GetSection(kvp.Value);
                if (teamTypeSection == null)
                    continue;

                var teamType = new TeamType(kvp.Value);
                teamType.ReadPropertiesFromIniSection(teamTypeSection);
                string houseIniName = teamTypeSection.GetStringValue("House", string.Empty);
                string scriptId = teamTypeSection.GetStringValue("Script", string.Empty);
                string taskForceId = teamTypeSection.GetStringValue("TaskForce", string.Empty);
                string tagId = teamTypeSection.GetStringValue("Tag", string.Empty);

                teamType.House = map.FindHouse(houseIniName);
                teamType.Script = map.Scripts.Find(s => s.ININame == scriptId);
                teamType.TaskForce = map.TaskForces.Find(t => t.ININame == taskForceId);
                teamType.Tag = map.Tags.Find(t => t.ID == tagId);

                if (teamType.House == null)
                {
                    Logger.Log($"TeamType {teamType.ININame} has an invalid house ({houseIniName}) specified!");
                    return;
                }

                if (teamType.Script == null)
                {
                    Logger.Log($"TeamType {teamType.ININame} has an invalid script ({scriptId}) specified!");
                }

                if (teamType.TaskForce == null)
                {
                    Logger.Log($"TeamType {teamType.ININame} has an invalid TaskForce ({taskForceId}) specified!");
                }

                map.AddTeamType(teamType);
            }
        }

        public static void ReadHouses(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("Houses");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                string houseName = kvp.Value;
                var house = new House(houseName);
                house.ID = Conversions.IntFromString(kvp.Key, -1);

                map.Houses.Add(house);

                var houseSection = mapIni.GetSection(houseName);
                if (houseSection != null)
                {
                    house.ReadPropertiesFromIniSection(houseSection);

                    var color = map.Rules.Colors.Find(c => c.Name == house.Color);
                    if (color == null)
                        house.XNAColor = Color.Black;
                    else 
                        house.XNAColor = color.XNAColor;
                }
            }
        }

        public static void ReadCellTags(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("CellTags");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                Point2D? coords = Helpers.CoordStringToPoint(kvp.Key);
                if (coords == null)
                    continue;

                var tile = map.GetTile(coords.Value.X, coords.Value.Y);
                if (tile == null)
                    continue;

                Tag tag = map.Tags.Find(t => t.ID == kvp.Value);
                if (tag == null)
                    continue;

                map.AddCellTag(new CellTag() { Position = coords.Value, Tag = tag });
            }
        }

        public static void ReadLocalVariables(IMap map, IniFile mapIni)
        {
            var section = mapIni.GetSection("VariableNames");
            if (section == null)
                return;

            foreach (var kvp in section.Keys)
            {
                string[] parts = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    Logger.Log($"Invalid local variable syntax in entry {kvp.Key}: {kvp.Value}, skipping reading locals");
                    return;
                }

                var localVariable = new LocalVariable(map.LocalVariables.Count);
                localVariable.Name = parts[0];
                localVariable.InitialState = parts[1] == "1" ? true : false;

                map.LocalVariables.Add(localVariable);
            }
        }
    }
}
