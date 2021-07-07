using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    /// <summary>
    /// A sidebar panel for listing units.
    /// </summary>
    public class UnitListPanel : ObjectListPanel
    {
        public UnitListPanel(WindowManager windowManager, EditorState editorState, Map map, TheaterGraphics theaterGraphics) : base(windowManager, editorState, map, theaterGraphics)
        {
        }

        protected override void InitObjects()
        {
            var sideCategories = new List<TreeViewCategory>();
            for (int i = 0; i < Map.Rules.UnitTypes.Count; i++)
            {
                TreeViewCategory category = null;
                var unittype = Map.Rules.UnitTypes[i];
                Color remapColor = Color.White;

                if (string.IsNullOrEmpty(unittype.Owner))
                {
                    category = FindOrMakeCategory("Unspecified", sideCategories);
                }
                else
                {
                    string[] owners = unittype.Owner.Split(',');
                    string primaryOwnerName = owners[0];
                    var house = Map.StandardHouses.Find(h => h.ININame == primaryOwnerName);
                    if (house != null)
                    {
                        int actsLike = house.ActsLike;
                        if (actsLike > -1)
                            primaryOwnerName = Map.StandardHouses[actsLike].ININame;
                    }

                    var ownerHouse = Map.Houses.Find(h => h.ININame == primaryOwnerName);
                    if (ownerHouse != null)
                        remapColor = ownerHouse.XNAColor;

                    category = FindOrMakeCategory(primaryOwnerName, sideCategories);
                }

                Texture2D texture = null;
                Texture2D remapTexture = null;
                if (TheaterGraphics.UnitTextures[i] != null)
                {
                    var frames = TheaterGraphics.UnitTextures[i].Frames;
                    if (frames.Length > 0)
                    {
                        // Find the first valid frame and use that as our texture
                        int firstNotNullIndex = Array.FindIndex(frames, f => f != null);
                        if (firstNotNullIndex > -1)
                        {
                            texture = frames[firstNotNullIndex].Texture;
                            if (Constants.HQRemap && unittype.ArtConfig.Remapable)
                                remapTexture = TheaterGraphics.UnitTextures[i].RemapFrames[firstNotNullIndex].Texture;
                        }
                    }
                }

                category.Nodes.Add(new TreeViewNode()
                {
                    Text = (unittype.ININame.StartsWith("AI") ? "AI - " : "") + unittype.Name + " (" + unittype.ININame + ")",
                    Texture = texture,
                    RemapTexture = remapTexture,
                    RemapColor = remapColor
                });

                category.Nodes = category.Nodes.OrderBy(n => n.Text).ToList();
            }

            sideCategories.ForEach(c => ObjectTreeView.AddCategory(c));
        }

        private TreeViewCategory FindOrMakeCategory(string categoryName, List<TreeViewCategory> categoryList)
        {
            var category = categoryList.Find(c => c.Text == categoryName);
            if (category != null)
                return category;

            category = new TreeViewCategory() { Text = categoryName };
            categoryList.Add(category);
            return category;
        }
    }
}
