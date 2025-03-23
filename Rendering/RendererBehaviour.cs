using Raylib_cs;

namespace PatoframeWork.Rendering;

public class RendererBehaviour : Behaviour
{
    public Color Color = Color.White;
    public float Size = 15;

    public enum ShapeType
    {
        Circle, Square, Image
    }

    public ShapeType RenderType;

    
    public ulong ImageID, SpriteID;

    
    public float zRot;

    public int Order;

    public override void OnAdd()
    {
        GameController.Renderers.Add(this);
    }

    public override void OnRemove()
    {
        GameController.Renderers.Remove(this);
    }

    public override void UpdateEffect()
    {
        //
    }
}