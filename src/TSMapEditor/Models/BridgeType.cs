using Rampastring.Tools;
using System;
using System.Collections.Generic;

namespace TSMapEditor.Models
{
    public class BridgeLoadException : Exception
    {
        public BridgeLoadException(string message) : base(message)
        {
        }
    }

    public enum BridgeKind
    {
        Low,
        High
    }

    public enum BridgeDirection
    {
        EastWest,
        NorthSouth
    }

    public class BridgeConfig
    {
        public BridgeConfig(IniSection iniSection, BridgeDirection direction, BridgeType bridgeType, Rules rules)
        {
            string suffix = direction == BridgeDirection.EastWest ? "EW" : "NS";

            if (bridgeType.Kind == BridgeKind.Low)
            {
                string bridgeStart = iniSection.GetStringValue($"BridgeStart.{suffix}", null);
                if (bridgeStart == null)
                    throw new BridgeLoadException($"Low bridge {bridgeType.Name} has no start overlay!");

                Start = rules.FindOverlayType(bridgeStart)?.Index ??
                        throw new BridgeLoadException($"Low bridge {bridgeType.Name} has an invalid start overlay {bridgeStart}!");

                string bridgeEnd = iniSection.GetStringValue($"BridgeEnd.{suffix}", null);
                if (bridgeEnd == null)
                    throw new BridgeLoadException($"Low bridge {bridgeType.Name} has no end overlay!");

                End = rules.FindOverlayType(bridgeEnd)?.Index ??
                      throw new BridgeLoadException($"Low bridge {bridgeType.Name} has an invalid end overlay {bridgeEnd}!");

                Pieces = iniSection.GetListValue($"BridgePieces.{suffix}", ',', (overlayName) => rules.FindOverlayType(overlayName)?.Index ??
                    throw new BridgeLoadException($"Low bridge {bridgeType.Name} has an invalid bridge piece {overlayName}!"));

                if (Pieces.Count == 0)
                    throw new BridgeLoadException($"Low bridge {bridgeType.Name} has no bridge pieces!");
            }
            else
            {
                string piece = iniSection.GetStringValue($"BridgePieces.{suffix}", null);
                if (piece == null)
                    throw new BridgeLoadException($"High bridge {bridgeType.Name} has no bridge piece!");

                int bridgePiece = rules.FindOverlayType(piece)?.Index ??
                      throw new BridgeLoadException($"High bridge {bridgeType.Name} has an invalid bridge piece {piece}!");

                Pieces.Add(bridgePiece);
            }
        }

        public int Start;
        public int End;
        public List<int> Pieces = new List<int>();
    }

    public class BridgeType
    {
        public BridgeType(IniSection iniSection, Rules rules)
        {
            Name = iniSection.SectionName;

            string bridgeType = iniSection.GetStringValue("Kind", null);
            if (bridgeType == "Low")
                Kind = BridgeKind.Low;
            else if (bridgeType == "High")
                Kind = BridgeKind.High;
            else throw new BridgeLoadException($"Bridge {Name} has an invalid Kind!");

            NorthSouth = new BridgeConfig(iniSection, BridgeDirection.NorthSouth, this, rules);
            EastWest = new BridgeConfig(iniSection, BridgeDirection.EastWest, this, rules);

            if (Kind == BridgeKind.High)
                TileSetIndex = iniSection.GetIntValue("TileSet", -1);
        }

        public string Name;
        public BridgeKind Kind;

        public BridgeConfig NorthSouth;
        public BridgeConfig EastWest;
        public int TileSetIndex;
    }
}
