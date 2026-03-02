using System.Numerics;
using Raylib_cs;

namespace PF.Visual;
public struct AtlasData
{
    Texture2D atlas;
    


    List<List<Rectangle>> Sprites = [];

    public AtlasData(Texture2D atlas, int spriteWidth, int spriteHeight)
    {
        this.atlas = atlas;

        Vector2 size = atlas.Dimensions;

        for (int x = spriteWidth; x < size.X; x += spriteWidth)
        {
            Sprites.Add([]);
            for (int y = spriteHeight; y < size.Y; y += spriteHeight)
            {
                Sprites[^1].Add(new(x - spriteWidth, y - spriteHeight, spriteWidth, spriteHeight));
            }
        }
    }

    public readonly TileSprite CreateTileSprite(int spriteX, int spriteY)
    {
        return new(atlas, Sprites[spriteX][spriteY]);
    }

    public readonly AnimatedTileSprite CreateAnimatedTileSprite(SpriteAnimationData data)
    {
        return new AnimatedTileSprite(atlas, [.. Sprites.SelectMany(t => t)], data);
    }
}
