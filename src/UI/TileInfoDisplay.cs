using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using TSMapEditor.CCEngine;
using TSMapEditor.Models;
using TSMapEditor.Models.Enums;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI
{
    /// <summary>
    /// A panel that displays information about a single tile.
    /// </summary>
    public class TileInfoDisplay : EditorPanel
    {
        public TileInfoDisplay(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.map = map;
            this.theaterGraphics = theaterGraphics;
        }

        private readonly Map map;
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

            if (MapTile.Waypoint != null)
            {
                AddWaypointInfo(MapTile.Waypoint);
            }

            textRenderer.PrepareTextParts();

            Height = textRenderer.Bottom + Constants.UIEmptyBottomSpace;
        }

        private void AddWaypointInfo(Waypoint waypoint)
        {
            // Find all usages for this waypoint
            List<string> usages = new List<string>(0);

            foreach (Trigger trigger in map.Triggers)
            {
                foreach (var action in trigger.Actions)
                {
                    bool usageFound = false;

                    if (action.ActionIndex < 0 && action.ActionIndex >= map.EditorConfig.TriggerActionTypes.Count)
                        continue;

                    var triggerActionType = map.EditorConfig.TriggerActionTypes[action.ActionIndex];

                    for (int i = 0; i < triggerActionType.Parameters.Length; i++)
                    {
                        if (triggerActionType.Parameters[i] == null)
                            continue;

                        var param = triggerActionType.Parameters[i];

                        if (param.TriggerParamType == TriggerParamType.Waypoint && action.Parameters[i] == waypoint.Identifier.ToString())
                        {
                            usages.Add("trigger '" + trigger.Name + "', ");
                            usageFound = true;
                        }
                        else if (param.TriggerParamType == TriggerParamType.WaypointZZ && action.Parameters[i] == Helpers.WaypointNumberToAlphabeticalString(waypoint.Identifier))
                        {
                            usages.Add("trigger '" + trigger.Name + "', ");
                            usageFound = true;
                        }
                    }

                    // Don't list the trigger multiple times if it has multiple actions that refer to the waypoint
                    if (usageFound)
                        break;
                }
            }

            foreach (Script script in map.Scripts)
            {
                foreach (var actionEntry in script.Actions)
                {
                    if (actionEntry.Action < 0 || actionEntry.Action >= map.EditorConfig.ScriptActions.Count)
                        continue;

                    var scriptAction = map.EditorConfig.ScriptActions[actionEntry.Action];
                    if (scriptAction.ParamType == TriggerParamType.Waypoint && actionEntry.Argument == waypoint.Identifier)
                    {
                        usages.Add("script '" + script.Name + "', ");
                    }
                }
            }

            foreach (TeamType team in map.TeamTypes)
            {
                if (team.Waypoint == Helpers.WaypointNumberToAlphabeticalString(waypoint.Identifier))
                {
                    usages.Add("team '" + team.Name + "', ");
                }
            }

            if (usages.Count > 0)
            {
                string lastUsage = usages[usages.Count - 1];
                usages[usages.Count - 1] = lastUsage.Substring(0, lastUsage.Length - 2);

                textRenderer.AddTextLine(new XNATextPart("Usages of waypoint " + waypoint.Identifier + ":", Constants.UIDefaultFont, Color.Gray));

                foreach (var usage in usages)
                {
                    textRenderer.AddTextPart(new XNATextPart(usage, Constants.UIDefaultFont, Color.White));
                }
            }
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
