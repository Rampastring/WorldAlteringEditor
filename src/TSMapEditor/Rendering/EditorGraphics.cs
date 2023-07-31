using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;

namespace TSMapEditor.Rendering
{
    /// <summary>
    /// Editor-only assets.
    /// </summary>
    public class EditorGraphics
    {
        public Texture2D GenericTileTexture { get; private set; }
        public Texture2D ImpassableCellHighlightTexture { get; private set; }
        public Texture2D IceGrowthHighlightTexture { get; private set; }
        public Texture2D RangeIndicatorTexture { get; private set; }

        public EditorGraphics()
        {
            GenericTileTexture = AssetLoader.LoadTextureUncached("generictile.png");
            ImpassableCellHighlightTexture = AssetLoader.LoadTextureUncached("impassablehighlight.png");
            IceGrowthHighlightTexture = AssetLoader.LoadTextureUncached("icehighlight.png");
            RangeIndicatorTexture = AssetLoader.LoadTextureUncached("rangeindicator.png");
        }

        public void DisposeAll()
        {
            GenericTileTexture.Dispose();
            ImpassableCellHighlightTexture.Dispose();
            IceGrowthHighlightTexture.Dispose();
            RangeIndicatorTexture.Dispose();
        }
    }
}
