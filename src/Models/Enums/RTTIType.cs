namespace TSMapEditor.Models
{
    public enum RTTIType
    {
        // Note that these do not currently follow 
        // the order of the game's own RTTIType enum.

        // The order of the enum values in the game
        // itself is not entirely known at this time.

        // In addition, I took the liberty of making
        // the names of the values more fitting for C# conventions.
        None,
        Unit,
        Aircraft,
        AircraftType,
        Anim,
        Building,
        BuildingType,
        Bullet,
        Cell,
        Factory,
        HouseType,
        Infantry,
        InfantryType,
        IsoTileType,
        Side,
        Smudge,
        Terrain,
        TerrainType,
        UnitType,
        Event,
        TaskForce,
        ScriptType,
        TeamType,
        OverlayType,
        Overlay,
        Waypoint
    }
}
