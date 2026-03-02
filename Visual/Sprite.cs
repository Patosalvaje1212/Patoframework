using System.Numerics;
using Raylib_cs;

namespace PF.Visual;

/// <summary>
/// Holds a texture and its properties for rendering.
/// </summary>
public class Sprite : IRender
{
    /// <summary>
    /// Pointer to the Texture.
    /// </summary>
    public Texture2D Texture { get; protected set; }

    /// <summary>
    /// Vector exporsing the size of the Texture.
    /// </summary>
    /// <remarks>
    /// The same as calling <c>SDL.GetTextureSize</c>.
    /// </remarks>
    public readonly Vector2 Size;

    /// <summary>
    /// Color tint to apply to the texture when rendering.
    /// </summary>
    public Color ColorMod = Color.White;

    /// <summary>
    /// Order in layer used by default sprite sorting. Higher values render the texture on top.
    /// </summary>
    public float Index = 1;

    public Sprite(Texture2D texture)
    {
        this.Texture = texture;

        Size = Texture.Dimensions;
    }

    internal Sprite(Texture2D texture, Vector2 size)
    {
        this.Texture = texture;

        Size = size;
    }

    public virtual void AdvanceTime(double delta) {}


    public virtual void RenderAt(Transform transform)
    {
        var oV = transform.Get().Translation.AsVector2() + Size * transform.Get().Scale.AsVector2() / 2;
        
        var rO = new Rectangle(0f, 0f, Size);
        var rF = new Rectangle(oV, oV - Size * transform.Get().Scale.AsVector2());

        Raylib.DrawTexturePro(Texture, rO, rF, oV, transform.Get().Rotation.Z, ColorMod);
    }
}