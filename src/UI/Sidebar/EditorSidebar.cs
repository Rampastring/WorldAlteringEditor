using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using TSMapEditor.Models;
using TSMapEditor.Rendering;

namespace TSMapEditor.UI.Sidebar
{
    public class EditorSidebar : EditorPanel
    {
        public EditorSidebar(WindowManager windowManager, Map map, TheaterGraphics theaterGraphics) : base(windowManager)
        {
            this.map = map;
            this.theaterGraphics = theaterGraphics;
        }

        private readonly Map map;
        private readonly TheaterGraphics theaterGraphics;

        private XNAListBox lbSelection;

        private XNAPanel[] modePanels;

        public override void Initialize()
        {
            Name = nameof(EditorSidebar);

            lbSelection = new XNAListBox(WindowManager);
            lbSelection.Name = nameof(lbSelection);
            lbSelection.X = 0;
            lbSelection.Y = 0;
            lbSelection.Width = Width;

            for (int i = 1; i < (int)SidebarMode.SidebarModeCount; i++)
            {
                SidebarMode sidebarMode = (SidebarMode)i;
                lbSelection.AddItem(new XNAListBoxItem() { Text = sidebarMode.ToString(), Tag = sidebarMode });
            }

            lbSelection.Height = lbSelection.Items.Count * lbSelection.LineHeight + 5;
            AddChild(lbSelection);

            lbSelection.EnableScrollbar = false;

            modePanels = new XNAPanel[lbSelection.Items.Count];

            var testTreeView = new TreeView(WindowManager);
            testTreeView.X = 0;
            testTreeView.Y = lbSelection.Bottom + Constants.UIEmptyTopSpace;
            testTreeView.Height = Height - Constants.UIEmptyBottomSpace - testTreeView.Y;
            testTreeView.Width = Width;
            AddChild(testTreeView);
            testTreeView.BackgroundTexture = AssetLoader.CreateTexture(new Color(0, 0, 0, 222), 2, 2);

            var sideCategories = new List<TreeViewCategory>();
            for (int i = 0; i < map.Rules.UnitTypes.Count; i++)
            {
                TreeViewCategory category = null;
                var unittype = map.Rules.UnitTypes[i];
                Color remapColor = Color.White;

                if (string.IsNullOrEmpty(unittype.Owner))
                {
                    category = FindOrMakeCategory("Unspecified", sideCategories);
                }
                else
                {
                    string[] owners = unittype.Owner.Split(',');
                    string primaryOwnerName = owners[0];
                    var house = map.StandardHouses.Find(h => h.ININame == primaryOwnerName);
                    if (house != null)
                    {
                        int actsLike = house.ActsLike;
                        if (actsLike > -1)
                            primaryOwnerName = map.StandardHouses[actsLike].ININame;
                    }

                    var ownerHouse = map.Houses.Find(h => h.ININame == primaryOwnerName);
                    if (ownerHouse != null)
                        remapColor = ownerHouse.XNAColor;

                    category = FindOrMakeCategory(primaryOwnerName, sideCategories);
                }

                Texture2D texture = null;
                Texture2D remapTexture = null;
                if (theaterGraphics.UnitTextures[i] != null)
                {
                    var frames = theaterGraphics.UnitTextures[i].Frames;
                    if (frames.Length > 0)
                    {
                        // Find the first valid frame and use that as our texture
                        int firstNotNullIndex = Array.FindIndex(frames, f => f != null);
                        if (firstNotNullIndex > -1)
                        {
                            texture = frames[firstNotNullIndex].Texture;
                            if (Constants.HQRemap && unittype.ArtConfig.Remapable)
                                remapTexture = theaterGraphics.UnitTextures[i].RemapFrames[firstNotNullIndex].Texture;
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

            sideCategories.ForEach(c => testTreeView.AddCategory(c));

            base.Initialize();
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
