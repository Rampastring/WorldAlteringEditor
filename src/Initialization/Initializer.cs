using Rampastring.Tools;
using System;
using System.Collections.Generic;
using TSMapEditor.Models;
using TSMapEditor.Models.ArtConfig;
using TSMapEditor.Models.Enums;

namespace TSMapEditor.Initialization
{
    public class MapLoadException : Exception
    {
        public MapLoadException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Initializes all different object types.
    /// </summary>
    public class Initializer : IInitializer
    {
        public Initializer(IMap map)
        {
            this.map = map;
        }

        private readonly IMap map;

        private Dictionary<Type, Action<AbstractObject, IniFile, IniSection>> objectTypeInitializers
            = new Dictionary<Type, Action<AbstractObject, IniFile, IniSection>>()
            {
                { typeof(BuildingType), InitBuildingType },
                { typeof(InfantryType), InitInfantryType },
                { typeof(UnitType), InitUnitType },
                { typeof(AircraftType), InitAircraftType },
                { typeof(OverlayType), InitOverlayType },
                { typeof(TerrainType), InitTerrainType }
            };

        private Dictionary<Type, Action<IMap, AbstractObject, IniFile, IniSection>> objectTypeArtInitializers
            = new Dictionary<Type, Action<IMap, AbstractObject, IniFile, IniSection>>()
            {
                { typeof(TerrainType), InitTerrainTypeArt },
                { typeof(BuildingType), InitArtConfigGeneric },
                { typeof(OverlayType), InitArtConfigGeneric },
                { typeof(UnitType), InitArtConfigGeneric },
                { typeof(InfantryType), InitInfantryArtConfig }
            };

        public void ReadObjectTypePropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(obj.ININame);
            if (objectSection == null)
                return;

            obj.ReadPropertiesFromIniSection(objectSection);

            if (objectTypeInitializers.TryGetValue(typeof(T), out var action))
                action(obj, iniFile, objectSection);
        }

        public void ReadObjectTypeArtPropertiesFromINI<T>(T obj, IniFile iniFile) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(obj.ININame);
            if (objectSection == null)
                return;

            if (objectTypeArtInitializers.TryGetValue(typeof(T), out var action))
                action(map, obj, iniFile, objectSection);
        }

        public void ReadObjectTypeArtPropertiesFromINI<T>(T obj, IniFile iniFile, string sectionName) where T : AbstractObject, INIDefined
        {
            IniSection objectSection = iniFile.GetSection(sectionName);
            if (objectSection == null)
                return;

            if (objectTypeArtInitializers.TryGetValue(typeof(T), out var action))
                action(map, obj, iniFile, objectSection);
        }

        public void InitArt(IniFile iniFile)
        {

        }

        private static void InitBuildingType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var buildingType = (BuildingType)obj;
        }

        private static void InitArtConfigGeneric(IMap map, AbstractObject obj, IniFile artIni, IniSection artSection)
        {
            var artConfigContainer = (IArtConfigContainer)obj;
            artConfigContainer.GetArtConfig().ReadFromIniSection(artSection);
        }

        private static void InitInfantryArtConfig(IMap map, AbstractObject obj, IniFile artIni, IniSection artSection)
        {
            var infantryType = (InfantryType)obj;
            infantryType.ArtConfig.ReadFromIniSection(artSection);
            if (infantryType.ArtConfig.Sequence == null && !string.IsNullOrWhiteSpace(infantryType.ArtConfig.SequenceName))
            {
                infantryType.ArtConfig.Sequence = map.Rules.FindOrMakeInfantrySequence(artIni, infantryType.ArtConfig.SequenceName);
            }
        }

        private static void InitInfantryType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitUnitType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitAircraftType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
        }

        private static void InitOverlayType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var overlayType = (OverlayType)obj;
            overlayType.Land = (LandType)Enum.Parse(typeof(LandType), section.GetStringValue("Land", LandType.Clear.ToString()));
        }

        private static void InitTerrainType(AbstractObject obj, IniFile rulesIni, IniSection section)
        {
            var terrainType = (TerrainType)obj;
            terrainType.SnowOccupationBits = (TerrainOccupation)section.GetIntValue("SnowOccupationBits", 0);
            terrainType.TemperateOccupationBits = (TerrainOccupation)section.GetIntValue("TemperateOccupationBits", 0);
        }
        
        private static void InitTerrainTypeArt(IMap map, AbstractObject obj, IniFile artIni, IniSection artSection)
        {
            var terrainType = (TerrainType)obj;
            terrainType.Theater = artSection.GetBooleanValue("Theater", terrainType.Theater);
            terrainType.Image = artSection.GetStringValue("Image", terrainType.Image);
        }
    }
}
