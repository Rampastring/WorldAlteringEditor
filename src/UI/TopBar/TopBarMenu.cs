using System;
using Rampastring.XNAUI;
using TSMapEditor.Models;
using TSMapEditor.Mutations;
using TSMapEditor.UI.Controls;
using TSMapEditor.UI.Windows;

namespace TSMapEditor.UI.TopBar
{
    class TopBarMenu : EditorPanel
    {
        public TopBarMenu(WindowManager windowManager, MutationManager mutationManager, Map map, WindowController windowController) : base(windowManager)
        {
            this.mutationManager = mutationManager;
            this.map = map;
            this.windowController = windowController;
        }

        private readonly MutationManager mutationManager;
        private readonly Map map;
        private readonly WindowController windowController;

        private MenuButton[] menuButtons;

        public override void Initialize()
        {
            Name = nameof(TopBarMenu);

            var fileContextMenu = new EditorContextMenu(WindowManager);
            fileContextMenu.Name = nameof(fileContextMenu);
            fileContextMenu.AddItem("New", null, null, null, null);
            fileContextMenu.AddItem("Open", null, null, null, null);
            fileContextMenu.AddItem("Save", () =>
            {
                if (string.IsNullOrWhiteSpace(map.LoadedINI.FileName))
                    throw new NotImplementedException("Saving of new maps is not implemented yet.");

                map.Write(map.LoadedINI.FileName);
            });
            fileContextMenu.AddItem("Exit", () => WindowManager.CloseGame());

            var fileButton = new MenuButton(WindowManager, fileContextMenu);
            fileButton.Name = nameof(fileButton);
            fileButton.Text = "File";
            AddChild(fileButton);

            var editContextMenu = new EditorContextMenu(WindowManager);
            editContextMenu.Name = nameof(editContextMenu);
            editContextMenu.AddItem("Undo", () => mutationManager.Undo(), () => mutationManager.CanUndo(), null, null);
            editContextMenu.AddItem("Redo", () => mutationManager.Redo(), () => mutationManager.CanRedo(), null, null);
            editContextMenu.AddItem("TaskForces", () => windowController.TaskForcesWindow.Open(), null, null, null);
            editContextMenu.AddItem("Scripts", () => windowController.ScriptsWindow.Open(), null, null, null);
            editContextMenu.AddItem("TeamTypes", () => windowController.TeamTypesWindow.Open(), null, null, null);
            editContextMenu.Items[0].HintText = KeyboardCommands.Instance.Undo.Key.GetKeyNameString();
            editContextMenu.Items[1].HintText = KeyboardCommands.Instance.Redo.Key.GetKeyNameString();

            var editButton = new MenuButton(WindowManager, editContextMenu);
            editButton.Name = nameof(editButton);
            editButton.X = fileButton.Right + 1;
            editButton.Text = "Edit";
            AddChild(editButton);

            var toolsContextMenu = new EditorContextMenu(WindowManager);
            toolsContextMenu.Name = nameof(toolsContextMenu);
            toolsContextMenu.AddItem("Options");
            toolsContextMenu.AddItem("Tool Scripts");

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
