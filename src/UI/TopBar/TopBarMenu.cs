using System;
using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.Rendering;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.CursorActions;
using TSMapEditor.UI.Windows;

namespace TSMapEditor.UI.TopBar
{
    class TopBarMenu : EditorPanel
    {
        public TopBarMenu(WindowManager windowManager, MutationManager mutationManager, MapView mapView, Map map, WindowController windowController) : base(windowManager)
        {
            this.mutationManager = mutationManager;
            this.mapView = mapView;
            this.map = map;
            this.windowController = windowController;
        }

        private readonly MutationManager mutationManager;
        private readonly MapView mapView;
        private readonly Map map;
        private readonly WindowController windowController;

        private MenuButton[] menuButtons;

        public override void Initialize()
        {
            Name = nameof(TopBarMenu);

            var fileContextMenu = new EditorContextMenu(WindowManager);
            fileContextMenu.Name = nameof(fileContextMenu);
            fileContextMenu.AddItem("New", () => EditorMessageBox.Show(WindowManager, "Not Implemented",
                "Creating a new map while a map is open is not " + Environment.NewLine +
                "implemented yet. Restart the editor to create a new map.", MessageBoxButtons.OK), null, null, null);
            fileContextMenu.AddItem("Open", () => EditorMessageBox.Show(WindowManager, "Not Implemented",
                "Opening another map while a map is open is not " + Environment.NewLine +
                "implemented yet. Restart the editor to open a different map.", MessageBoxButtons.OK), null, null, null);

            fileContextMenu.AddItem("Save", () =>
            {
                if (string.IsNullOrWhiteSpace(map.LoadedINI.FileName))
                {
                    SaveAs();
                    return;
                }

                map.Write();
            });
            fileContextMenu.AddItem("Save As", () => SaveAs(), null, null, null);
            fileContextMenu.AddItem("Exit", () => WindowManager.CloseGame());

            var fileButton = new MenuButton(WindowManager, fileContextMenu);
            fileButton.Name = nameof(fileButton);
            fileButton.Text = "File";
            AddChild(fileButton);

            var editContextMenu = new EditorContextMenu(WindowManager);
            editContextMenu.Name = nameof(editContextMenu);
            editContextMenu.AddItem("Undo", () => mutationManager.Undo(), () => mutationManager.CanUndo(), null, null);
            editContextMenu.AddItem("Redo", () => mutationManager.Redo(), () => mutationManager.CanRedo(), null, null);
            editContextMenu.AddItem(" ", null, () => false, null, null);
            editContextMenu.AddItem("Basic", () => windowController.BasicSectionConfigWindow.Open(), null, null, null);
            editContextMenu.AddItem("Houses", () => windowController.HousesWindow.Open(), null, null, null);
            editContextMenu.AddItem("Triggers", () => windowController.TriggersWindow.Open(), null, null, null);
            editContextMenu.AddItem("TaskForces", () => windowController.TaskForcesWindow.Open(), null, null, null);
            editContextMenu.AddItem("Scripts", () => windowController.ScriptsWindow.Open(), null, null, null);
            editContextMenu.AddItem("TeamTypes", () => windowController.TeamTypesWindow.Open(), null, null, null);
            editContextMenu.AddItem("Local Variables", () => windowController.LocalVariablesWindow.Open(), null, null, null);
            editContextMenu.Items[0].HintText = KeyboardCommands.Instance.Undo.GetKeyDisplayString();
            editContextMenu.Items[1].HintText = KeyboardCommands.Instance.Redo.GetKeyDisplayString();

            var editButton = new MenuButton(WindowManager, editContextMenu);
            editButton.Name = nameof(editButton);
            editButton.X = fileButton.Right + 1;
            editButton.Text = "Edit";
            AddChild(editButton);

            var toolsContextMenu = new EditorContextMenu(WindowManager);
            toolsContextMenu.Name = nameof(toolsContextMenu);
            // toolsContextMenu.AddItem("Options");
            toolsContextMenu.AddItem("Apply Impassable Overlay", () => windowController.AutoApplyImpassableOverlayWindow.Open(), null, null, null);
            toolsContextMenu.AddItem("Terrain Generator Options", () => windowController.TerrainGeneratorConfigWindow.Open(), null, null, null);
            toolsContextMenu.AddItem("Generate Terrain", () => EnterTerrainGenerator(), null, null, null);
            // toolsContextMenu.AddItem("Tool Scripts");

            var toolsButton = new MenuButton(WindowManager, toolsContextMenu);
            toolsButton.Name = nameof(toolsButton);
            toolsButton.X = editButton.Right + 1;
            toolsButton.Text = "Tools";
            AddChild(toolsButton);

            var aboutContextMenu = new EditorContextMenu(WindowManager);
            aboutContextMenu.Name = nameof(aboutContextMenu);
            aboutContextMenu.AddItem("About");

            var aboutButton = new MenuButton(WindowManager, aboutContextMenu);
            aboutButton.Name = nameof(aboutButton);
            aboutButton.X = toolsButton.Right + 1;
            aboutButton.Text = "About";
            AddChild(aboutButton);

            base.Initialize();

            Height = fileButton.Height;
            Width = aboutButton.Right;

            menuButtons = new MenuButton[] { fileButton, editButton, toolsButton, aboutButton };
            Array.ForEach(menuButtons, b => b.MouseEnter += MenuButton_MouseEnter);

            KeyboardCommands.Instance.GenerateTerrain.Triggered += (s, e) => EnterTerrainGenerator();
        }

        private void EnterTerrainGenerator()
        {
            if (windowController.TerrainGeneratorConfigWindow.TerrainGeneratorConfiguration == null)
            {
                windowController.TerrainGeneratorConfigWindow.Open();
                return;
            }

            var generateForestCursorAction = new GenerateForestCursorAction(mapView);
            generateForestCursorAction.TerrainGeneratorConfiguration = windowController.TerrainGeneratorConfigWindow.TerrainGeneratorConfiguration;
            mapView.CursorAction = generateForestCursorAction;
        }

        private void SaveAs()
        {
            windowController.SaveMapAsWindow.Open();
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            var menuButton = (MenuButton)sender;

            // Is a menu open?
            int openIndex = Array.FindIndex(menuButtons, b => b.ContextMenu.Enabled);
            if (openIndex > -1)
            {
                // Switch to the new button's menu
                menuButtons[openIndex].ContextMenu.Disable();
                menuButton.OpenContextMenu();
            }
        }
    }
}
