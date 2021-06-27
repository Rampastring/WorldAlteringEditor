namespace TSMapEditor
{
    public class Constants
    {
        public const int CellSizeX = 48;
        public const int CellSizeY = 24;

        public const int ObjectHealthMax = 256;
        public const int FacingMax = 255;

        public const int MaxWaypoint = 100;

        // TODO parse from Rules.ini
        public const int ConditionYellowHP = 128;

        public const int UIEmptySideSpace = 10;
        public const int UIEmptyTopSpace = 10;

        public const int UIDefaultFont = 0;

        public const int MAX_MAP_LENGTH_IN_DIMENSION = 512;
        public const int NO_OVERLAY = 255; // 0xFF
        public const int OverlayPackFormat = 80;

        public const string NoneValue1 = "<none>";
        public const string NoneValue2 = "None";

        public const bool HQRemap = true;
        public const float RemapBrightenFactor = 1.25f;
    }
}
