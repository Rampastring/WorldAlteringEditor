using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using System;
using TSMapEditor.CCEngine;
using TSMapEditor.GameMath;
using TSMapEditor.Models;

namespace TSMapEditor.Rendering.ObjectRenderers
{
    /// <summary>
    /// Base class for all object renderers.
    /// </summary>
    /// <typeparam name="T">The type of game object to render.</typeparam>
    public abstract class ObjectRenderer<T> where T : GameObject
    {
        protected ObjectRenderer(RenderDependencies renderDependencies)
        {
            RenderDependencies = renderDependencies;
        }

        protected RenderDependencies RenderDependencies;

        protected Map Map => RenderDependencies.Map;
        protected TheaterGraphics TheaterGraphics => RenderDependencies.TheaterGraphics;

        protected abstract Color ReplacementColor { get; }

        /// <summary>
        /// The entry point for rendering an object.
        ///
        /// Checks whether the object is within the visible screen space. If yes,
        /// draws the graphics of the object, or the object's replacement text in
        /// case it has no loaded graphics.
        /// </summary>
        /// <param name="gameObject">The game object to render.</param>
        /// <param name="checkInCamera">Whether the object's presence within the camera should be checked.</param>
        /// <param name="drawShadow">Whether a shadow should also be drawn for this object.</param>
        public void Draw(T gameObject, bool checkInCamera, bool drawShadow)
        {
            Point2D drawPointWithoutCellHeight = CellMath.CellTopLeftPointFromCellCoords(gameObject.Position, RenderDependencies.Map);

            var mapCell = RenderDependencies.Map.GetTile(gameObject.Position);
            int heightOffset = RenderDependencies.EditorState.Is2DMode ? 0 : mapCell.Level * Constants.CellHeight;
            Point2D drawPoint = new Point2D(drawPointWithoutCellHeight.X, drawPointWithoutCellHeight.Y - heightOffset);

            CommonDrawParams drawParams = GetDrawParams(gameObject);

            PositionedTexture frame = GetFrameTexture(gameObject, drawParams, RenderDependencies.EditorState.IsLighting);

            Rectangle drawingBounds = GetTextureDrawCoords(gameObject, frame, drawPoint);

            // If the object is not in view, skip
            if (checkInCamera && !IsObjectInCamera(drawingBounds))
                return;

            if (drawShadow)
            {
                if (gameObject.HasShadow())
                    DrawShadow(gameObject, drawParams, drawPoint, heightOffset);

                return;
            }

            if (frame == null && ShouldRenderReplacementText(gameObject))
            {
                DrawObjectReplacementText(gameObject, drawParams, drawPoint);
            }
            else
            {
                Render(gameObject, heightOffset, drawPoint, drawParams);
            }
        }

        /// <summary>
        /// Returns a bool that determines whether a game object
        /// should be rendered as text in case it does not have
        /// regular graphics loaded.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        protected virtual bool ShouldRenderReplacementText(T gameObject)
        {
            return true;
        }

        /// <summary>
        /// Fetches parameters for drawing the object.
        /// Override in derived classes to get the necessary parameters
        /// for drawing the type of object.
        /// </summary>
        /// <param name="gameObject">The game object to get the drawing parameters for.</param>
        protected abstract CommonDrawParams GetDrawParams(T gameObject);

        /// <summary>
        /// Renders an object. Override in derived classes to implement and customize the rendering process.
        /// </summary>
        /// <param name="gameObject">The game object to draw.</param>
        /// <param name="heightOffset">The Y-axis draw offset from cell height.</param>
        /// <param name="drawPoint">The draw point of the object, with cell height taken into account.</param>
        /// <param name="drawParams">Draw parameters.</param>
        protected abstract void Render(T gameObject, int heightOffset, Point2D drawPoint, in CommonDrawParams drawParams);

        /// <summary>
        /// Renders the replacement text of an object, displayed when no graphics for an object have been loaded.
        /// Override in derived classes to implement and customize the rendering process.
        /// </summary>
        /// <param name="gameObject">The game object for which to draw a replacement text.</param>
        /// <param name="drawParams">Draw parameters.</param>
        /// <param name="drawPoint">The draw point of the object, with cell height taken into account.</param>
        protected virtual void DrawObjectReplacementText(T gameObject, in CommonDrawParams drawParams, Point2D drawPoint)
        {
            SetEffectParams_RGBADraw(false);

            // If the object is a techno, draw an arrow that displays its facing
            if (gameObject.IsTechno())
            {
                var techno = gameObject as TechnoBase;
                DrawObjectFacingArrow(techno.Facing, drawPoint);
            }

            Renderer.DrawString(drawParams.IniName, 1, drawPoint.ToXNAVector(), ReplacementColor, 1.0f);
        }

        protected void DrawObjectFacingArrow(byte facing, Point2D drawPoint)
        {
            var cellCenterPoint = (drawPoint + new Point2D(Constants.CellSizeX / 2, Constants.CellSizeY / 2)).ToXNAVector();

            float rad = (facing / 255.0f) * (float)Math.PI * 2.0f;

            // The in-game compass is slightly rotated compared to the usual math compass
            // and the compass used by MonoGame.
            // In the usual compass, 0 rad points directly towards the right / east, in the in-game
            // compass it points to top-right / northeast
            rad -= (float)Math.PI / 4.0f;

            var arrowEndPoint = Helpers.VectorFromLengthAndAngle(Constants.CellSizeX / 4, rad);
            arrowEndPoint += new Vector2(arrowEndPoint.X, 0f); // Isometric perspective
            RendererExtensions.DrawArrow(cellCenterPoint, cellCenterPoint + arrowEndPoint, Color.Yellow, 1f, 10f, 2);
        }

        private bool IsObjectInCamera(Rectangle drawingBounds)
        {
            if (drawingBounds.X + drawingBounds.Width < RenderDependencies.Camera.TopLeftPoint.X || drawingBounds.X > RenderDependencies.GetCameraRightXCoord())
                return false;

            if (drawingBounds.Y + drawingBounds.Height < RenderDependencies.Camera.TopLeftPoint.Y || drawingBounds.Y > RenderDependencies.GetCameraBottomYCoord())
                return false;

            return true;
        }

        private PositionedTexture GetFrameTexture(T gameObject, in CommonDrawParams drawParams, bool affectedByLighting)
        {
            if (drawParams.ShapeImage != null && drawParams.ShapeImage.GetFrameCount() > 0)
            {
                int frameIndex = gameObject.GetFrameIndex(drawParams.ShapeImage.GetFrameCount());

                if (frameIndex > -1 && frameIndex < drawParams.ShapeImage.GetFrameCount())
                    return drawParams.ShapeImage.GetFrame(frameIndex);
            }
            else if (drawParams.MainVoxel?.Frames != null && drawParams.MainVoxel.GetFrame(0, RampType.None, affectedByLighting) is var frameMain && frameMain != null)
            {
                return frameMain;
            }
            else if (drawParams.TurretVoxel?.Frames != null && drawParams.TurretVoxel.GetFrame(0, RampType.None, affectedByLighting) is var frameTur && frameTur != null)
            {
                return frameTur;
            }
            else if (drawParams.BarrelVoxel?.Frames != null && drawParams.BarrelVoxel.GetFrame(0, RampType.None, affectedByLighting) is var frameBarl && frameBarl != null)
            {
                return frameBarl;
            }

            return null;
        }

        private Rectangle GetTextureDrawCoords(T gameObject,
            PositionedTexture frame,
            Point2D initialDrawPoint)
        {
            int finalDrawPointX;
            int finalDrawPointY;

            if (frame != null)
            {
                int yDrawOffset = gameObject.GetYDrawOffset();
                int xDrawOffset = gameObject.GetXDrawOffset();

                finalDrawPointX = initialDrawPoint.X - frame.ShapeWidth / 2 + frame.OffsetX + Constants.CellSizeX / 2 + xDrawOffset;
                finalDrawPointY = initialDrawPoint.Y - frame.ShapeHeight / 2 + frame.OffsetY + Constants.CellSizeY / 2 + yDrawOffset;

            }
            else
            {
                finalDrawPointX = initialDrawPoint.X;
                finalDrawPointY = initialDrawPoint.Y;
            }

            return new Rectangle(finalDrawPointX, finalDrawPointY,
                finalDrawPointX + frame?.Texture.Width ?? 0, finalDrawPointY + frame?.Texture.Height ?? 0);
        }

        protected void SetEffectParams_PalettedDraw(bool isShadow, Texture2D paletteTexture)
            => SetEffectParams(RenderDependencies.PalettedColorDrawEffect, isShadow, paletteTexture, true);

        protected void SetEffectParams_RGBADraw(bool isShadow)
            => SetEffectParams(RenderDependencies.PalettedColorDrawEffect, isShadow, null, false);

        protected void SetEffectParams(Effect effect, bool isShadow, Texture2D paletteTexture, bool usePalette)
        {
            effect.Parameters["IsShadow"].SetValue(isShadow);
            RenderDependencies.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

            if (paletteTexture != null)
            {
                effect.Parameters["PaletteTexture"].SetValue(paletteTexture);
                RenderDependencies.GraphicsDevice.Textures[2] = paletteTexture;
            }

            effect.Parameters["UsePalette"].SetValue(usePalette);
            RenderDependencies.PalettedColorDrawEffect.Parameters["UseRemap"].SetValue(false); // Disable remap by default
        }

        protected virtual void DrawShadow(T gameObject, in CommonDrawParams drawParams, Point2D drawPoint, int heightOffset)
        {
            if (drawParams.ShapeImage == null)
                return;

            int shadowFrameIndex = gameObject.GetShadowFrameIndex(drawParams.ShapeImage.GetFrameCount());
            if (shadowFrameIndex > 0 && shadowFrameIndex < drawParams.ShapeImage.GetFrameCount())
            {
                DrawShapeImage(gameObject, drawParams.ShapeImage, shadowFrameIndex,
                    new Color(0, 0, 0, 128), true, false, Color.White, false, false, drawPoint, heightOffset);
            }
        }

        protected virtual float GetDepth(T gameObject, Texture2D texture)
        {
            var tile = Map.GetTile(gameObject.Position);
            // int textureHeightInCells = texture.Height / Constants.CellHeight;
            // if (textureHeightInCells == 0)
            //     textureHeightInCells++;

            return (tile.Level + 1) * Constants.DepthRenderStep;
        }

        protected void DrawShapeImage(T gameObject, ShapeImage image, int frameIndex, Color color,
            bool isShadow, bool drawRemap, Color remapColor, bool affectedByLighting, bool affectedByAmbient, Point2D drawPoint, int heightOffset)
        {
            if (image == null)
                return;

            PositionedTexture frame = image.GetFrame(frameIndex);
            if (frame == null || frame.Texture == null)
                return;

            PositionedTexture remapFrame = null;
            if (drawRemap && image.HasRemapFrames())
                remapFrame = image.GetRemapFrame(frameIndex);

            Rectangle drawingBounds = GetTextureDrawCoords(gameObject, frame, drawPoint);

            double extraLight = 0.0;
            switch (gameObject.WhatAmI())
            {
                case RTTIType.Unit:
                    extraLight = Map.Rules.ExtraUnitLight;
                    break;
                case RTTIType.Infantry:
                    extraLight = Map.Rules.ExtraInfantryLight;
                    break;
                case RTTIType.Aircraft:
                    extraLight = Map.Rules.ExtraAircraftLight;
                    break;
            }

            Vector4 lighting = Vector4.One;
            var mapCell = Map.GetTile(gameObject.Position);

            if (RenderDependencies.EditorState.IsLighting && mapCell != null)
            {
                if (affectedByLighting && image.SubjectToLighting)
                {
                    lighting = mapCell.CellLighting.ToXNAVector4(extraLight);
                    remapColor = ScaleColorToAmbient(remapColor, mapCell.CellLighting);
                }
                else if (affectedByAmbient)
                {
                    lighting = mapCell.CellLighting.ToXNAVector4Ambient(extraLight);
                    remapColor = ScaleColorToAmbient(remapColor, mapCell.CellLighting);
                }
            }

            float depth = GetDepth(gameObject, frame.Texture);

            RenderFrame(frame, remapFrame, color, drawRemap, remapColor, isShadow,
                drawingBounds.X, drawingBounds.Y, image.GetPaletteTexture(), lighting, depth);
        }

        protected void DrawVoxelModel(T gameObject, VoxelModel model, byte facing, RampType ramp,
            Color color, bool drawRemap, Color remapColor, bool affectedByLighting, Point2D drawPoint, int heightOffset)
        {
            if (model == null)
                return;

            PositionedTexture frame = model.GetFrame(facing, ramp, false);
            if (frame == null || frame.Texture == null)
                return;

            float depth = GetDepth(gameObject, frame.Texture);

            PositionedTexture remapFrame = null;
            if (drawRemap)
                remapFrame = model.GetRemapFrame(facing, ramp, false);

            double extraLight = 0.0;
            switch (gameObject.WhatAmI())
            {
                case RTTIType.Unit:
                    extraLight = Map.Rules.ExtraUnitLight;
                    break;
                case RTTIType.Infantry:
                    extraLight = Map.Rules.ExtraInfantryLight;
                    break;
                case RTTIType.Aircraft:
                    extraLight = Map.Rules.ExtraAircraftLight;
                    break;
            }

            Vector4 lighting = Vector4.One;
            var mapCell = Map.GetTile(gameObject.Position);

            if (RenderDependencies.EditorState.IsLighting && mapCell != null)
            {
                if (affectedByLighting && Constants.VoxelsAffectedByLighting)
                {
                    lighting = mapCell.CellLighting.ToXNAVector4(extraLight);
                }
                else
                {
                    lighting = mapCell.CellLighting.ToXNAVector4Ambient(extraLight);
                }
            }

            remapColor = ScaleColorToAmbient(remapColor, mapCell.CellLighting);

            Rectangle drawingBounds = GetTextureDrawCoords(gameObject, frame, drawPoint);

            RenderFrame(frame, remapFrame, color, drawRemap, remapColor, false,
                drawingBounds.X, drawingBounds.Y, null, lighting, depth);
        }

        private void RenderFrame(PositionedTexture frame, PositionedTexture remapFrame, Color color, bool drawRemap, Color remapColor,
            bool isShadow, int finalDrawPointX, int finalDrawPointY, Texture2D paletteTexture, Vector4 lightingColor, float depth)
        {
            Texture2D texture = frame.Texture;

            ApplyShaderEffectValues(isShadow, paletteTexture);

            color = new Color((color.R / 255.0f) * lightingColor.X / 2f,
                (color.B / 255.0f) * lightingColor.Y / 2f,
                (color.B / 255.0f) * lightingColor.Z / 2f, depth);

            Renderer.DrawTexture(texture, 
                new Rectangle(finalDrawPointX, finalDrawPointY, texture.Width, texture.Height),
                null, color, 0f, Vector2.Zero, SpriteEffects.None, depth);

            if (drawRemap && remapFrame != null)
            {
                remapColor = new Color(
                    (remapColor.R / 255.0f),
                    (remapColor.G / 255.0f),
                    (remapColor.B / 255.0f),
                    depth);

                RenderDependencies.PalettedColorDrawEffect.Parameters["UseRemap"].SetValue(true);

                Renderer.DrawTexture(remapFrame.Texture,
                    new Rectangle(finalDrawPointX, finalDrawPointY, texture.Width, texture.Height),
                    null,
                    remapColor,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    0f);
            }
        }

        private void ApplyShaderEffectValues(bool isShadow, Texture2D paletteTexture)
        {
            if (paletteTexture == null)
                SetEffectParams_RGBADraw(isShadow);
            else
                SetEffectParams_PalettedDraw(isShadow, paletteTexture);
        }

        protected void DrawLine(Vector2 start, Vector2 end, Color color, int thickness = 1, float depth = 0f)
            => Renderer.DrawLine(start, end, color, thickness, depth);

        protected Color ScaleColorToAmbient(Color color, MapColor mapColor)
        {
            double highestComponent = Math.Max(mapColor.R, Math.Max(mapColor.G, mapColor.B));

            return new Color((int)(color.R * highestComponent),
                (int)(color.G * highestComponent),
                (int)(color.B * highestComponent),
                color.A);
        }
    }
}
