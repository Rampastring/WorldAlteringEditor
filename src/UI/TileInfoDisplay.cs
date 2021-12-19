using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    /// <summary>
    /// A panel that displays information about a single tile.
    /// </summary>
    public class TileInfoDisplay : EditorPanel
    {
        public TileInfoDisplay(WindowManager windowManager, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.theaterGraphics = theaterGraphics;
        }

        private readonly TheaterGraphics theaterGraphics;

        private MapTile _mapTile;
        public MapTile MapTile
        {
            get => _mapTile;
            set { _mapTile = value; RefreshInfo(); }
        }

        private XNATextRenderer textRenderer;

        public override void Initialize()
        {
            Name = nameof(TileInfoDisplay);

            Width = 300;

            textRenderer = new XNATextRenderer(WindowManager);
            textRenderer.Name = nameof(textRenderer);
            textRenderer.X = Constants.UIEmptySideSpace;
            textRenderer.Y = Constants.UIEmptyTopSpace;
            textRenderer.Padding = 0;
            textRenderer.SpaceBetweenLines = 2;
            textRenderer.Width = Width - Constants.UIEmptySideSpace * 2;
            AddChild(textRenderer);

            base.Initialize();
        }

        private void RefreshInfo()
        {
            textRenderer.ClearTextParts();

            if (MapTile == null)
            {
                Visible = false;
                return;
            }

            Color subtleTextColor = Color.Gray;
            Color baseTextColor = Color.White;
            Color cellTagTextColor = Color.Red;

            Visible = true;

            textRenderer.AddTextLine(new XNATextPart(MapTile.X + ", " + MapTile.Y, Constants.UIDefaultFont, baseTextColor));

            TileImage tileGraphics = theaterGraphics.GetTileGraphics(MapTile.TileIndex);
            TileSet tileSet = theaterGraphics.Theater.TileSets[tileGraphics.TileSetId];
            textRenderer.AddTextLine(new XNATextPart("TileSet: ", Constants.UIDefaultFont, subtleTextColor));
            textRenderer.AddTextPart(new XNATextPart(tileSet.SetName + " (" + tileGraphics.TileSetId + ")", Constants.UIDefaultFont, baseTextColor));

            MGTMPImage subCellImage = tileGraphics.TMPImages[MapTile.SubTileIndex];
            string terrainType = subCellImage != null ? Helpers.LandTypeToString(subCellImage.TmpImage.TerrainType) : "Unknown";

            textRenderer.AddTextLine(new XNATextPart("Terrain Type: ", Constants.UIDefaultFont, subtleTextColor));
            textRenderer.AddTextPart(new XNATextPart(terrainType, Constants.UIDefaultFont, baseTextColor));

            CellTag cellTag = MapTile.CellTag;
            if (cellTag != null)
            {
                textRenderer.AddTextLine(new XNATextPart("CellTag: ",
                    Constants.UIDefaultFont, subtleTextColor));
                textRenderer.AddTextPart(new XNATextPart(cellTag.Tag.Name + " (" + cellTag.Tag.ID + ")",
                    Constants.UIDefaultFont, cellTagTextColor));
            }

            Overlay overlay = MapTile.Overlay;
            if (overlay != null)
            {
                textRenderer.AddTextLine(new XNATextPart(
                    "Overlay: ",
                    Constants.UIDefaultFont, subtleTextColor));

                textRenderer.AddTextPart(new XNATextPart(
                    overlay.OverlayType.Name + ", Frame: " + overlay.FrameIndex + ", Terrain Type: " + overlay.OverlayType.Land,
                    Constants.UIDefaultFont, baseTextColor));
            }

            if (MapTile.Aircraft != null)
            {
                AddObjectInformation("Aircraft: ", MapTile.Aircraft);
            }

            if (MapTile.Vehicle != null)
            {
                AddObjectInformation("Vehicle: ", MapTile.Vehicle);
            }

            if (MapTile.Structure != null)
            {
                AddObjectInformation("Structure: ", MapTile.Structure);
            }

            for (int i = 0; i < MapTile.Infantry.Length; i++)
            {
                if (MapTile.Infantry[i] != null)
                    AddObjectInformation("Infantry: ", MapTile.Infantry[i]);
            }

            textRenderer.PrepareTextParts();

            Height = textRenderer.Bottom + Constants.UIEmptyBottomSpace;
        }

        private void AddObjectInformation<T>(string objectTypeLabel, Techno<T> techno) where T : GameObjectType
        {
            textRenderer.AddTextPart(new XNATextPart(Environment.NewLine));
            textRenderer.AddTextLine(new XNATextPart(objectTypeLabel,
                Constants.UIDefaultFont, Color.Gray));
            textRenderer.AddTextPart(new XNATextPart( techno.ObjectType.Name + " (" + techno.ObjectType.ININame + "), Owner:",
                    Constants.UIDefaultFont, Color.White));
            textRenderer.AddTextPart(new XNATextPart(techno.Owner.ININame, Constants.UIBoldFont, techno.Owner.XNAColor));
            if (techno.AttachedTag != null)
            {
                textRenderer.AddTextPart(new XNATextPart(",", Constants.UIDefaultFont, Color.White));
                textRenderer.AddTextPart(new XNATextPart("Tag:", Constants.UIDefaultFont, Color.White));
                textRenderer.AddTextPart(new XNATextPart(techno.AttachedTag.Name + " (" + techno.AttachedTag.ID + ")", Constants.UIBoldFont, Color.White));
            }
        }
    }
}
