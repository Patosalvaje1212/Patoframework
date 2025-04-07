using PatoframeWork.Inspector;
using Raylib_cs;

namespace PatoframeWork.Rendering;

/// <summary>
/// Behaviour that renders a sprite onto the screen, at its Owner Entity's position.
/// </summary>
public class RendererBehaviour : Behaviour
{
    /// <summary>
    /// Applied Tint to the sprite.
    /// </summary>
    public Color Color = Color.White;

    /// <summary>
    /// Sprite's visual size.
    /// </summary>
    public float Size = 15;

    /// <summary>
    /// Visual Shape selector for a <c>RendererBehaviour</c>.
    /// </summary>
    public enum VisualShapeType
    {
        Circle, Square, Image
    }

    /// <summary>
    /// Current Shape to show.
    /// </summary>
    public VisualShapeType RenderType;

    /// <summary>
    /// If <c>RenderType</c> is set to <c>Image</c>, the Image's ID to show.
    /// </summary>
    [InspectorReceiveDrop("TextureDragData")]
    public string ImageID = "";
    /// <summary>
    /// If <c>RenderType</c> is set to <c>Image</c>, the Sprite's ID from the target image to show.
    /// </summary>
    public ulong SpriteID;

    /// <summary>
    /// Sprite's rotation along the Z axis.
    /// </summary>    
    public float zRot;

    /// <summary>
    /// The render Order of this Renderer. Higher means it renders in front.
    /// </summary>
    public int Order;

    public override void OnAdd()
    {
        base.OnAdd();

        GameController.AddRenderer(this);
    }

    public override void OnRemove()
    {
        base.OnAdd();

        GameController.RemoveRenderer(this);
    }
}