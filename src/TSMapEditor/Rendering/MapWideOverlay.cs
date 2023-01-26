using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using TSMapEditor.Settings;

namespace TSMapEditor.Rendering
{
    public class MapWideOverlay
    {
        const string MapWideOverlayTextureName = "mapwideoverlay.png";

        public MapWideOverlay()
        {
            if (AssetLoader.AssetExists(MapWideOverlayTextureName))
                texture = AssetLoader.LoadTexture(MapWideOverlayTextureName);

            Opacity = UserSettings.Instance.MapWideOverlayOpacity / 255.0f;
        }

        private readonly Texture2D texture;

        public bool Enabled { get; set; }

        public float Opacity { get; private set; }

        public bool HasTexture => texture != null;

        public void Draw(Rectangle cameraRectangle)
        {
            if (Enabled && texture != null)
            {
                Renderer.DrawTexture(texture,
                    cameraRectangle,
                    Color.White * Opacity);
            }
        }
    }
}
