#pragma enable_d3d11_debug_symbols

#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float SpriteDepthBottom;
float SpriteDepthTop;
float2 WorldTextureCoordinates;
float2 SpriteSizeToWorldSizeRatio;

sampler2D SpriteTextureSampler : register(s0)
{
    Texture = (SpriteTexture); // this is set by spritebatch
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 tex = tex2D(SpriteTextureSampler, input.TextureCoordinates);

    if (tex.a <= 0)
    {
        discard;
    }

    float xRatio = SpriteSizeToWorldSizeRatio.x;
    float yRatio = SpriteSizeToWorldSizeRatio.y;

    float2 finalPosition = WorldTextureCoordinates + float2(input.TextureCoordinates.x * xRatio, input.TextureCoordinates.y * yRatio);

    // Terrain increases in depth as we go up the screen
    float spriteDepth = SpriteDepthBottom + ((SpriteDepthTop - SpriteDepthBottom) * (1.0 - input.TextureCoordinates.y));

    return float4(spriteDepth, 0, 0, 0);
}

technique SpriteDrawing
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};