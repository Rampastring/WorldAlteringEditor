using Rampastring.XNAUI;
using System;
using TSMapEditor.Models;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    public class GenerateStandardHousesWindow : INItializableWindow
    {
        public GenerateStandardHousesWindow(WindowManager windowManager, Map map) : base(windowManager)
        {
            this.map = map;
        }

        private readonly Map map;

        public override void Initialize()
        {
            Name = nameof(GenerateStandardHousesWindow);
            base.Initialize();

            FindChild<EditorButton>("btnSingleplayer").LeftClick += BtnSingleplayer_LeftClick;
            FindChild<EditorButton>("btnMultiplayer").LeftClick += BtnMultiplayer_LeftClick;
            FindChild<EditorButton>("btnCoOp").LeftClick += BtnCoOp_LeftClick;
            FindChild<EditorButton>("btnCancel").LeftClick += (s, e) => Hide();
        }

        private void BtnSingleplayer_LeftClick(object sender, System.EventArgs e)
        {
            AddHousesFromEditorRulesSectionAndHide("SPHouses");
        }

        private void BtnMultiplayer_LeftClick(object sender, EventArgs e)
        {
            AddHousesFromEditorRulesSectionAndHide("Houses");
        }

        private void BtnCoOp_LeftClick(object sender, EventArgs e)
        {
            AddHousesFromEditorRulesSectionAndHide("CoopHouses");
        }

        private void AddHousesFromEditorRulesSectionAndHide(string sectionName)
        {
            var houses = map.Rules.GetHousesFrom(map.EditorConfig.EditorRulesIni, sectionName);

            // Set some meaningful default values
            for (int i = 0; i < houses.Count; i++)
            {
                var house = houses[i];
                house.ID = i;
                house.Edge = "North";
                house.Allies = house.ININame;
                house.TechLevel = 7;

                for (int sideIndex = 0; sideIndex < map.Rules.Sides.Count; sideIndex++)
                {
                    string side = map.Rules.Sides[sideIndex];

                    if (house.ININame.StartsWith(side))
                    {
                        house.Side = side;
                        house.ActsLike = houses.FindIndex(h => house.ININame.StartsWith(h.ININame));
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(house.Side))
                {
                    house.Side = map.Rules.Sides[0];
                    house.ActsLike = 0;
                }

                if (!Constants.UseCountries)
                {
                    var houseType = new HouseType(house.ININame);
                    houseType.ID = i;
                    houseType.Color = house.Color;
                    map.AddHouseType(houseType);
                    house.HouseType = houseType;
                }
            }

            map.AddHouses(houses);

            ReassignObjectHouses();
            Hide();
        }

        private void ReassignObjectHouses()
        {
            map.DoForAllTechnos(t =>
            {
                string ownerName = t.Owner.ININame;
                var house = map.Houses.Find(h => h.ININame == ownerName);
                if (house != null)
                    t.Owner = house;
            });
        }

        public void Open()
        {
            Show();
        }
    }
}
