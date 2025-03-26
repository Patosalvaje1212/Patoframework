using Raylib_cs;

namespace PatoframeWork.Rendering;

public class RendererBehaviour : Behaviour
{
    public Color Color = Color.White;
    public float Size = 15;

    public enum VisualShapeType
    {
        Circle, Square, Image
    }

    public VisualShapeType RenderType;

    
    public ulong ImageID, SpriteID;

    
    public float zRot;

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