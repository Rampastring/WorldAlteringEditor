using Microsoft.Xna.Framework;
using Rampastring.XNAUI;
using TSMapEditor.UI.Controls;

namespace TSMapEditor.UI.Windows
{
    /// <summary>
    /// A window that allows the user to edit the map's TaskForces.
    /// </summary>
    public class TaskforcesWindow : INItializableWindow
    {
        public TaskforcesWindow(WindowManager windowManager) : base(windowManager)
        {
        }

        public override void Initialize()
        {
            Name = nameof(TaskforcesWindow);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
