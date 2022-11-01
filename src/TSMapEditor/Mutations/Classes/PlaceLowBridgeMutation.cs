using System;
using System.Collections.Generic;
using TSMapEditor.GameMath;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.Rendering;

namespace TSMapEditor.Mutations.Classes
{
    /// <summary>
    /// A mutation for creating low bridges.
    /// </summary>
    public class PlaceLowBridgeMutation : Mutation
    {
        public PlaceLowBridgeMutation(IMutationTarget mutationTarget, Point2D point1, Point2D point2) : base(mutationTarget)
        {
            if (point1.X != point2.X && point1.Y != point2.Y)
            {
                throw new ArgumentException(nameof(PlaceLowBridgeMutation) + 
                    ": either the X or Y coordinate must be identical between bridge start and end points.");
            }

            if (point1 == point2)
                throw new ArgumentException(nameof(PlaceLowBridgeMutation) + ": bridge start and end points cannot point to the same cell.");

            if (point1.X == point2.X)
            {
                bridgeDirection = BridgeDirection.Y;

                if (point1.Y > point2.Y)
                {
                    startPoint = point2;
                    endPoint = point1;
                }
                else
                {
                    startPoint = point1;
                    endPoint = point2;
                }
            }
            else
            {
                bridgeDirection = BridgeDirection.X;

                if (point1.X > point2.X)
                {
                    startPoint = point2;
                    endPoint = point1;
                }
                else
                {
                    startPoint = point1;
                    endPoint = point2;
                }
            }
                
        }

        private readonly Point2D startPoint;
        private readonly Point2D endPoint;

        private readonly BridgeDirection bridgeDirection;


        private List<OriginalOverlayInfo> originalOverlayInfos = new List<OriginalOverlayInfo>();

        public override void Perform()
        {
            if (bridgeDirection == BridgeDirection.X)
            {
                const int bridgeStartOverlayIndex = 94; // LOBRDG21
                const int bridgeEndOverlayIndex = 92; // LOBRDG19
                int[] bridgePieces = new int[] 
                { 
                    74, // LOBRDG01
                    75, // LOBRDG02
                    76, // LOBRDG03
                    77  // LOBRDG04
                };

                PlaceLowBridge(bridgeStartOverlayIndex, bridgeEndOverlayIndex, bridgePieces,
                    startPoint.X, endPoint.X, PlaceXDirectionLowBridgePiece);
            }
            else
            {
                const int bridgeStartOverlayIndex = 96; // LOBRDG23
                const int bridgeEndOverlayIndex = 98; // LOBRDG25
                int[] bridgePieces = new int[]
                {
                    83, // LOBRDG10
                    84, // LOBRDG11
                    85, // LOBRDG12
                    86  // LOBRDG13
                };

                PlaceLowBridge(bridgeStartOverlayIndex, bridgeEndOverlayIndex, bridgePieces,
                    startPoint.Y, endPoint.Y, PlaceYDirectionLowBridgePiece);
            }

            MutationTarget.InvalidateMap();
        }

        private void PlaceLowBridge(int bridgeStartOverlayIndex, int bridgeEndOverlayIndex, int[] bridgePieces, int beginCoord, int endCoord, Action<int, int> piecePlacementFunction)
        {
            piecePlacementFunction(bridgeStartOverlayIndex, beginCoord);

            for (int c = beginCoord + 1; c < endCoord; c++)
            {
                int overlayIndex = bridgePieces[MutationTarget.Randomizer.GetRandomNumber(0, bridgePieces.Length - 1)];
                piecePlacementFunction(overlayIndex, c);
            }

            piecePlacementFunction(bridgeEndOverlayIndex, endCoord);
        }

        private void PlaceXDirectionLowBridgePiece(int overlayIndex, int x)
        {
            PlaceLowBridgePiece(overlayIndex, x,
                (fixedCoord, variableCoord) => new Point2D(fixedCoord, startPoint.Y + variableCoord));
        }

        private void PlaceYDirectionLowBridgePiece(int overlayIndex, int y)
        {
            PlaceLowBridgePiece(overlayIndex, y, 
                (fixedCoord, variableCoord) => new Point2D(startPoint.X + variableCoord, fixedCoord));
        }

        private void PlaceLowBridgePiece(int overlayIndex, int fixedCoordinate, Func<int, int, Point2D> coordGenerator)
        {
            for (int variableCoordinateOffset = -1; variableCoordinateOffset <= 1; variableCoordinateOffset++)
            {
                var cellCoords = coordGenerator(fixedCoordinate, variableCoordinateOffset);
                var mapCell = MutationTarget.Map.GetTile(cellCoords);

                if (mapCell == null)
                    continue;

                if (mapCell.Overlay == null || mapCell.Overlay.OverlayType == null)
                    originalOverlayInfos.Add(new OriginalOverlayInfo(-1, -1, cellCoords));
                else
                    originalOverlayInfos.Add(new OriginalOverlayInfo(mapCell.Overlay.OverlayType.Index, mapCell.Overlay.FrameIndex, cellCoords));

                mapCell.Overlay = new Overlay()
                {
                    OverlayType = MutationTarget.Map.Rules.OverlayTypes[overlayIndex],
                    FrameIndex = variableCoordinateOffset + 1,
                    Position = cellCoords
                };
            }
        }

        public override void Undo()
        {
            foreach (var originalOverlayInfo in originalOverlayInfos)
            {
                var mapCell = MutationTarget.Map.GetTile(originalOverlayInfo.CellCoords);

                if (originalOverlayInfo.OverlayTypeIndex < 0)
                {
                    mapCell.Overlay = null;
                }
                else
                {
                    mapCell.Overlay = new Overlay()
                    {
                        OverlayType = MutationTarget.Map.Rules.OverlayTypes[originalOverlayInfo.OverlayTypeIndex],
                        FrameIndex = originalOverlayInfo.FrameIndex,
                        Position = originalOverlayInfo.CellCoords
                    };
                }
            }

            MutationTarget.InvalidateMap();
        }
    }
}
