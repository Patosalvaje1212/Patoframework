using System.Numerics;
using Raylib_cs;

namespace PF.Visual;

public class TileSprite : Sprite
{
    protected Rectangle sRect;
    public TileSprite(Texture2D texture, Rectangle sourceRectangle) : base(texture, sourceRectangle.Size)
    {
        sRect = sourceRectangle;
    }

    public override void RenderAt(Transform transform)
    {
        var oV = transform.Get().Translation.AsVector2() + Size * transform.Get().Scale.AsVector2() / 2;
        
        var rO = sRect;
        var rF = new Rectangle(oV, oV - Size * transform.Get().Scale.AsVector2());

        Raylib.DrawTexturePro(Texture, rO, rF, oV, transform.Get().Rotation.Z, ColorMod);
    }
}
